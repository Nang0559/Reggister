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
  APPROVER provisioning

  IMPORTANT:
  F03ApprovalPolicies.PositionCode belongs to the REQUESTER, not the approver.
  F03Approvers is the candidate pool for a RequestType + Level.

  HRM position capability:
      F03Positions.IsApprove = 1
      F03Positions.IsAllowApprove = 1
      F03Positions.DefaultApproveLevel = canonical approval rank

  Canonical level mapping:
      Leave / Trip / Equipment:
          position rank 1 -> level 1
          position rank 2 -> level 2
          position rank 3 -> level 3
          position rank 4 (GM) -> level 3

      OT:
          position rank 1 -> level 3
          position rank 2 -> level 5
          position rank 3 -> level 6
          position rank 4 (GM) -> level 7

  This keeps business OT levels (3,5,6,7) distinct from Sequence.

  HRM may CREATE missing candidate rows and synchronize rows that it owns
  (LastModifiedSource = HRM). Existing manually configured approvers are
  never overwritten by HRM.
*/
CREATE OR ALTER PROCEDURE dbo.usp_ReconcileEmployeeApprovers
    @EmployeeCode nvarchar(50)=NULL,
    @CreatedBy int=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /*
      Materialize the candidate pool because it is used by both MERGE and
      stale-row cleanup. F03ApprovalPolicies is intentionally NOT joined here:
      its PositionCode belongs to the requester.
    */
    SELECT DISTINCT
        c.EmployeeCode,
        c.PositionCode,
        c.EmployeeName,
        c.EmailAddress,
        c.DeptCode,
        c.DeptName,
        CASE v.RequestType
            WHEN 0 THEN N'Leave'
            WHEN 1 THEN N'Overtime'
            WHEN 2 THEN N'Trip'
            WHEN 3 THEN N'Equipment'
        END AS RequestType,
        v.Level,
        CASE
            WHEN c.PositionApproveRank=4 THEN N'ALL'
            ELSE c.DeptCode
        END AS ApproveForDeptCode,
        CASE c.PositionApproveRank
            WHEN 1 THEN
                CASE v.RequestType
                    WHEN 3 THEN N'EquipmentApprover'
                    ELSE N'SubLeader'
                END
            WHEN 2 THEN
                CASE v.RequestType
                    WHEN 1 THEN N'Chief'
                    WHEN 3 THEN N'EquipmentManager'
                    ELSE N'Manager'
                END
            WHEN 3 THEN N'Manager'
            WHEN 4 THEN N'GM'
        END AS RoleName
    INTO #HrmApproverSource
    FROM
    (
        SELECT
            e.EmployeeCode,
            e.PositionCode,
            e.EmployeeName,
            e.EmailAddress,
            e.DeptCode,
            d.DeptName,
            pos.DefaultApproveLevel AS PositionApproveRank
        FROM dbo.F03Employees e
        INNER JOIN dbo.F03Positions pos
            ON pos.PositionCode=e.PositionCode
           AND pos.IsActive=1
           AND pos.IsApprove=1
           AND pos.IsAllowApprove=1
        LEFT JOIN dbo.F03Departments d
            ON d.DeptCode=e.DeptCode
        WHERE e.IsActive=1
          AND (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
    ) c
    CROSS APPLY
    (
        VALUES
        (0, CASE c.PositionApproveRank
                WHEN 1 THEN 1
                WHEN 2 THEN 2
                WHEN 3 THEN 3
                WHEN 4 THEN 3
                ELSE NULL
            END),
        (1, CASE c.PositionApproveRank
                WHEN 1 THEN 3
                WHEN 2 THEN 5
                WHEN 3 THEN 6
                WHEN 4 THEN 7
                ELSE NULL
            END),
        (2, CASE c.PositionApproveRank
                WHEN 1 THEN 1
                WHEN 2 THEN 2
                WHEN 3 THEN 3
                WHEN 4 THEN 3
                ELSE NULL
            END),
        (3, CASE c.PositionApproveRank
                WHEN 1 THEN 1
                WHEN 2 THEN 2
                WHEN 3 THEN 3
                WHEN 4 THEN 3
                ELSE NULL
            END)
    ) v(RequestType,Level)
    WHERE v.Level IS NOT NULL;

    MERGE dbo.F03Approvers AS target
    USING #HrmApproverSource AS src
      ON target.ApproverCode=src.EmployeeCode
     AND target.RequestType=src.RequestType
     AND target.Level=src.Level
     AND target.ApproveForDeptCode=src.ApproveForDeptCode
    WHEN MATCHED
         AND ISNULL(target.LastModifiedSource,N'')=N'HRM'
    THEN
        UPDATE SET
            target.IsActive=1,
            target.PositionCode=src.PositionCode,
            target.ApproverName=src.EmployeeName,
            target.ApproverEmail=ISNULL(src.EmailAddress,N''),
            target.ApproverDeptCode=ISNULL(src.DeptCode,N''),
            target.ApproverDeptName=ISNULL(src.DeptName,src.DeptCode),
            target.ApproveForDeptName=
                CASE WHEN src.ApproveForDeptCode=N'ALL'
                     THEN N'Toàn công ty'
                     ELSE ISNULL(src.DeptName,src.DeptCode) END,
            target.RoleName=src.RoleName,
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
            src.RequestType,src.EmployeeCode,src.PositionCode,src.EmployeeName,
            ISNULL(src.EmailAddress,N''),ISNULL(src.DeptCode,N''),
            ISNULL(src.DeptName,src.DeptCode),src.ApproveForDeptCode,
            CASE WHEN src.ApproveForDeptCode=N'ALL'
                 THEN N'Toàn công ty'
                 ELSE ISNULL(src.DeptName,src.DeptCode) END,
            src.Level,src.RoleName
        );

    /*
      Deactivate only HRM-owned stale rows for the affected employee.
      Manual approver configuration is intentionally preserved.
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
              FROM #HrmApproverSource src
              WHERE src.EmployeeCode=e.EmployeeCode
                AND src.RequestType=a.RequestType
                AND src.Level=a.Level
                AND src.ApproveForDeptCode=a.ApproveForDeptCode
          )
      );

    SELECT
        AffectedEmployee=@EmployeeCode,
        ActiveApprovers=(SELECT COUNT(*) FROM dbo.F03Approvers WHERE IsActive=1);
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
