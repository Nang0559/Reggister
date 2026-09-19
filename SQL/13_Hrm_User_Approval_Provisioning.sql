/*
FVN_REGISTER - HRM User / Approval Provisioning
Canonical model:

F03Employee -> F03User
F03Employee.PositionCode -> F03ApprovalPolicies.PositionCode -> F03Approvers

There is NO ApprovalGroup / PositionGroup layer.
PositionCode is the HRM source-of-truth for approval routing.

This script contains reconciliation procedures only. The approval policy
schema and seed are owned by SQL/17_ApprovalRouteSelection.sql.
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
                WHERE (r.DeptCode=e.DeptCode OR r.DeptCode IS NULL)
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
          SELECT 1 FROM dbo.F03Users u WHERE u.EmployeeCode=e.EmployeeCode
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
    INNER JOIN dbo.F03Employees e ON e.EmployeeCode=u.EmployeeCode
    WHERE (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode);

    SELECT
        AffectedEmployee=@EmployeeCode,
        ActiveUsers=(SELECT COUNT(*) FROM dbo.F03Users WHERE IsActive=1);
END;
GO

/*
  APPROVER provisioning
  Canonical lookup:
      Employee.PositionCode
          -> ApprovalPolicies.PositionCode
          -> candidate Employee

  F03ApprovalPolicies.RequestType is INT in the canonical route model.
  F03Approvers.RequestType remains NVARCHAR for compatibility with the
  existing approval runtime, so RequestType is converted to text here.
*/
CREATE OR ALTER PROCEDURE dbo.usp_ReconcileEmployeeApprovers
    @EmployeeCode nvarchar(50)=NULL,
    @CreatedBy int=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    ;WITH SourceRows AS
    (
        SELECT
            e.EmployeeCode,
            e.PositionCode,
            e.EmployeeName,
            e.EmailAddress,
            e.DeptCode,
            d.DeptName,
            p.RequestType,
            p.Level,
            p.LevelName,
            p.RoleName
        FROM dbo.F03Employees e
        INNER JOIN dbo.F03ApprovalPolicies p
            ON p.PositionCode=e.PositionCode
           AND p.IsActive=1
           AND p.Required=1
        INNER JOIN dbo.F03Positions pos
            ON pos.PositionCode=e.PositionCode
           AND pos.IsApprove=1
           AND pos.IsAllowApprove=1
        LEFT JOIN dbo.F03Departments d
            ON d.DeptCode=e.DeptCode
        WHERE e.IsActive=1
          AND (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
    )
    MERGE dbo.F03Approvers AS target
    USING
    (
        SELECT
            EmployeeCode,PositionCode,EmployeeName,EmailAddress,DeptCode,DeptName,
            CONVERT(nvarchar(20),RequestType) AS RequestType,
            Level,LevelName,RoleName,
            CASE
                WHEN PositionCode=N'0001' THEN N'ALL'
                ELSE DeptCode
            END AS ApproveForDeptCode
        FROM SourceRows
    ) AS src
      ON target.ApproverCode=src.EmployeeCode
     AND target.RequestType=src.RequestType
     AND target.Level=src.Level
     AND target.ApproveForDeptCode=src.ApproveForDeptCode
    WHEN MATCHED THEN
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
            (SELECT TOP(1) u.Id FROM dbo.F03Users u
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
      Deactivate stale rows for the affected employee.
      We deliberately do not delete approval history.
    */
    UPDATE a
       SET a.IsActive=0,
           a.ModifiedBy=@CreatedBy,
           a.ModifiedAt=GETDATE(),
           a.LastModifiedSource=N'HRM'
    FROM dbo.F03Approvers a
    INNER JOIN dbo.F03Employees e
        ON e.EmployeeCode=a.ApproverCode
    WHERE (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
      AND
      (
          e.IsActive=0
          OR NOT EXISTS
          (
              SELECT 1
              FROM dbo.F03ApprovalPolicies p
              INNER JOIN dbo.F03Positions pos
                  ON pos.PositionCode=e.PositionCode
                 AND pos.IsApprove=1
                 AND pos.IsAllowApprove=1
              WHERE p.PositionCode=e.PositionCode
                AND p.IsActive=1
                AND p.Required=1
                AND CONVERT(nvarchar(20),p.RequestType)=a.RequestType
                AND p.Level=a.Level
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
INNER JOIN dbo.F03ApprovalPolicies p
    ON p.PositionCode=e.PositionCode
   AND p.IsActive=1
   AND p.Required=1
INNER JOIN dbo.F03Positions pos
    ON pos.PositionCode=e.PositionCode
   AND pos.IsApprove=1
   AND pos.IsAllowApprove=1
LEFT JOIN dbo.F03Approvers a
    ON a.ApproverCode=e.EmployeeCode
   AND a.RequestType=CONVERT(nvarchar(20),p.RequestType)
   AND a.Level=p.Level
WHERE e.IsActive=1
  AND a.Id IS NULL;
GO
