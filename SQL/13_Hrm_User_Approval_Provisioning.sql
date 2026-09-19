/*
FVN_REGISTER - HRM User / Approval Provisioning
Canonical model:

F03Employee -> F03User
F03Employee.PositionCode -> F03ApprovalPolicies.PositionCode (requester policy)
F03Employee.PositionCode -> F03Positions approval capability -> F03Approvers (candidate pool)

There is NO ApprovalGroup / PositionGroup layer.
PositionCode is the HRM source-of-truth for employee identity and approval capability.

F03ApprovalPolicies defines required levels for the REQUESTER position.
F03Approvers is the candidate pool. Therefore approver provisioning must NOT
join an approver employee directly to the requester's policy PositionCode.
Candidate Level is derived from the approver employee's HRM approval position.
*/
USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
  USER provisioning
  - creates missing users from F03Employees
  - synchronizes HRM identity/scope fields
  - preserves existing PermissionCode
  - deactivates users whose employee is inactive
  - never writes back to HRM
*/
CREATE OR ALTER PROCEDURE dbo.usp_ReconcileEmployeeUsers
    @EmployeeCode nvarchar(50)=NULL,
    @CreatedBy int=0,
    @DefaultPasswordHash nvarchar(255)=N'f925916e2754e5e03f75dd58a5733251'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    INSERT dbo.F03Users
    (
        IsActive,CreatedBy,LastModifiedSource,Password,EmployeeCode,FullName,
        PermissionCode,LockoutEnable,NumLoginFailed,LevelApprove,DeptCode,Cvcode
    )
    SELECT
        CASE WHEN e.IsActive=1 THEN 1 ELSE 0 END,
        @CreatedBy,N'HRM',
        @DefaultPasswordHash,
        e.EmployeeCode,e.EmployeeName,
        ISNULL(
            (
                SELECT TOP (1) r.PermissionCode
                FROM dbo.F03HrmUserRoleRules r
                WHERE r.IsActive=1
                  AND (r.DeptCode=e.DeptCode OR r.DeptCode IS NULL)
                  AND (r.PositionCode=e.PositionCode OR r.PositionCode IS NULL)
                ORDER BY
                    CASE
                        WHEN r.DeptCode=e.DeptCode AND r.PositionCode=e.PositionCode THEN 0
                        WHEN r.DeptCode=e.DeptCode THEN 1
                        WHEN r.PositionCode=e.PositionCode THEN 2
                        ELSE 3
                    END,
                    r.Priority,r.Id
            ),5),
        1,0,ISNULL(e.LevelApprove,0),e.DeptCode,e.PositionCode
    FROM dbo.F03Employees e
    WHERE (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.F03Users u
          WHERE u.EmployeeCode=e.EmployeeCode
      );

    UPDATE u
       SET u.IsActive=CASE WHEN e.IsActive=1 THEN 1 ELSE 0 END,
           u.FullName=e.EmployeeName,
           u.LevelApprove=ISNULL(e.LevelApprove,0),
           u.DeptCode=e.DeptCode,
           u.Cvcode=e.PositionCode,
           u.ModifiedBy=@CreatedBy,
           u.ModifiedAt=GETDATE(),
           u.LastModifiedSource=N'HRM'
    FROM dbo.F03Users u
    INNER JOIN dbo.F03Employees e
        ON e.EmployeeCode=u.EmployeeCode
    WHERE (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode);

    SELECT
        AffectedEmployee=@EmployeeCode,
        ActiveUsers=(SELECT COUNT(*) FROM dbo.F03Users WHERE IsActive=1);
END;
GO

/*
  APPROVER provisioning - POLICY DRIVEN

  F03ApprovalPolicies is the source of truth:
      Policy.PositionCode -> F03Employee.PositionCode
      Policy.RequestType   -> F03Approvers.RequestType
      Policy.Level         -> F03Approvers.Level

  Therefore Admin configures approval policy once. HRM security reconcile then:
      1. marks every PositionCode referenced by an active policy as approval-capable;
      2. derives DefaultApproveLevel from the lowest configured policy level;
      3. creates/synchronizes F03Approvers for active employees whose PositionCode
         exists in an active policy;
      4. deactivates stale HRM-owned approver rows when policy/employee no longer qualifies.

  No ApprovalGroup / PositionGroup layer is used.
*/
CREATE OR ALTER PROCEDURE dbo.usp_ReconcileEmployeeApprovers
    @EmployeeCode nvarchar(50)=NULL,
    @CreatedBy int=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /*
      Policy -> Position metadata.
      IsApprove/IsAllowApprove are derived metadata, not a prerequisite
      maintained manually by Admin.
    */
    UPDATE p
       SET p.IsApprove=1,
           p.IsAllowApprove=1,
           p.DefaultApproveLevel=(
               SELECT MIN(ap.Level)
               FROM dbo.F03ApprovalPolicies ap
               WHERE ap.IsActive=1
                 AND ap.PositionCode=p.PositionCode
           ),
           p.ModifiedBy=@CreatedBy,
           p.ModifiedAt=GETDATE(),
           p.LastModifiedSource=N'HRM'
    FROM dbo.F03Positions p
    WHERE p.IsActive=1
      AND EXISTS
      (
          SELECT 1
          FROM dbo.F03ApprovalPolicies ap
          WHERE ap.IsActive=1
            AND ap.PositionCode=p.PositionCode
      );

    /*
      Positions that are no longer referenced by any active policy are no
      longer approval-capable. Do not touch positions owned by another source.
    */
    UPDATE p
       SET p.IsApprove=0,
           p.IsAllowApprove=0,
           p.DefaultApproveLevel=NULL,
           p.ModifiedBy=@CreatedBy,
           p.ModifiedAt=GETDATE(),
           p.LastModifiedSource=N'HRM'
    FROM dbo.F03Positions p
    WHERE p.LastModifiedSource=N'HRM'
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.F03ApprovalPolicies ap
          WHERE ap.IsActive=1
            AND ap.PositionCode=p.PositionCode
      );

    /*
      Candidate pool is now driven directly by active ApprovalPolicies.
      A position can have different levels for different RequestTypes.
    */
    SELECT DISTINCT
        e.EmployeeCode,
        e.PositionCode,
        e.EmployeeName,
        e.EmailAddress,
        e.DeptCode,
        d.DeptName,
        ap.RequestType,
        ap.Level,
        ap.LevelName,
        ap.RoleName,
        e.DeptCode AS ApproveForDeptCode,
        ISNULL(d.DeptName,e.DeptCode) AS ApproveForDeptName
    INTO #HrmApproverSource
    FROM dbo.F03Employees e
    INNER JOIN dbo.F03ApprovalPolicies ap
        ON ap.PositionCode=e.PositionCode
       AND ap.IsActive=1
    LEFT JOIN dbo.F03Departments d
        ON d.DeptCode=e.DeptCode
    WHERE e.IsActive=1
      AND (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode);

    MERGE dbo.F03Approvers AS target
    USING #HrmApproverSource AS src
      ON target.ApproverCode=src.EmployeeCode
     AND target.RequestType=
         CASE src.RequestType
             WHEN 0 THEN N'Leave'
             WHEN 1 THEN N'Overtime'
             WHEN 2 THEN N'Trip'
             WHEN 3 THEN N'Equipment'
         END
     AND target.Level=src.Level
     AND target.ApproveForDeptCode=src.ApproveForDeptCode
    WHEN MATCHED
         AND ISNULL(target.LastModifiedSource,N'')=N'HRM'
    THEN
        UPDATE SET
            target.IsActive=1,
            target.UserId=(SELECT TOP(1) u.Id FROM dbo.F03Users u WHERE u.EmployeeCode=src.EmployeeCode),
            target.PositionCode=src.PositionCode,
            target.ApproverName=src.EmployeeName,
            target.ApproverEmail=ISNULL(src.EmailAddress,N''),
            target.ApproverDeptCode=ISNULL(src.DeptCode,N''),
            target.ApproverDeptName=ISNULL(src.DeptName,src.DeptCode),
            target.ApproveForDeptName=src.ApproveForDeptName,
            target.RoleName=ISNULL(src.RoleName,src.LevelName),
            target.ModifiedBy=@CreatedBy,
            target.ModifiedAt=GETDATE(),
            target.LastModifiedSource=N'HRM'
    WHEN NOT MATCHED THEN
        INSERT
        (
            IsActive,CreatedBy,LastModifiedSource,UserId,
            RequestType,ApproverCode,PositionCode,ApproverName,ApproverEmail,
            ApproverDeptCode,ApproverDeptName,ApproveForDeptCode,ApproveForDeptName,
            Level,RoleName
        )
        VALUES
        (
            1,@CreatedBy,N'HRM',
            (SELECT TOP(1) u.Id
             FROM dbo.F03Users u
             WHERE u.EmployeeCode=src.EmployeeCode),
            CASE src.RequestType
                WHEN 0 THEN N'Leave'
                WHEN 1 THEN N'Overtime'
                WHEN 2 THEN N'Trip'
                WHEN 3 THEN N'Equipment'
            END,
            src.EmployeeCode,src.PositionCode,src.EmployeeName,
            ISNULL(src.EmailAddress,N''),ISNULL(src.DeptCode,N''),
            ISNULL(src.DeptName,src.DeptCode),src.ApproveForDeptCode,
            src.ApproveForDeptName,src.Level,ISNULL(src.RoleName,src.LevelName)
        );

    /*
      Deactivate only HRM-owned rows that are no longer represented by
      active employee + active ApprovalPolicy configuration.
    */
    UPDATE a
       SET a.IsActive=0,
           a.ModifiedBy=@CreatedBy,
           a.ModifiedAt=GETDATE(),
           a.LastModifiedSource=N'HRM'
    FROM dbo.F03Approvers a
    INNER JOIN dbo.F03Employees e
        ON e.EmployeeCode=a.ApproverCode
    WHERE a.LastModifiedSource=N'HRM'
      AND (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
      AND
      (
          e.IsActive=0
          OR NOT EXISTS
          (
              SELECT 1
              FROM dbo.F03ApprovalPolicies ap
              WHERE ap.IsActive=1
                AND ap.PositionCode=e.PositionCode
                AND a.RequestType=
                    CASE ap.RequestType
                        WHEN 0 THEN N'Leave'
                        WHEN 1 THEN N'Overtime'
                        WHEN 2 THEN N'Trip'
                        WHEN 3 THEN N'Equipment'
                    END
                AND a.Level=ap.Level
                AND a.ApproveForDeptCode=e.DeptCode
          )
      );

    SELECT
        AffectedEmployee=@EmployeeCode,
        ActiveApprovers=(SELECT COUNT(*) FROM dbo.F03Approvers WHERE IsActive=1),
        PolicyPositions=(
            SELECT COUNT(DISTINCT ap.PositionCode)
            FROM dbo.F03ApprovalPolicies ap
            WHERE ap.IsActive=1
        );
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReconcileHrmSecurity
    @EmployeeCode nvarchar(50)=NULL,
    @CreatedBy int=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    EXEC dbo.usp_ReconcileEmployeeUsers
        @EmployeeCode=@EmployeeCode,
        @CreatedBy=@CreatedBy;

    EXEC dbo.usp_ReconcileEmployeeApprovers
        @EmployeeCode=@EmployeeCode,
        @CreatedBy=@CreatedBy;
END;
GO

/*
  Verification: these are read-only and intentionally return zero when
  current HRM-derived security is fully provisioned.
*/
SELECT MissingUsers=COUNT(*)
FROM dbo.F03Employees e
LEFT JOIN dbo.F03Users u ON u.EmployeeCode=e.EmployeeCode
WHERE e.IsActive=1 AND u.Id IS NULL;

SELECT MissingApprovers=COUNT(*)
FROM dbo.F03Employees e
INNER JOIN dbo.F03Positions pos
    ON pos.PositionCode=e.PositionCode
   AND pos.IsActive=1
   AND pos.IsApprove=1
   AND pos.IsAllowApprove=1
LEFT JOIN dbo.F03Approvers a
    ON a.ApproverCode=e.EmployeeCode
   AND a.IsActive=1
WHERE e.IsActive=1
  AND a.Id IS NULL;
GO
