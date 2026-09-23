/*
===============================================================================
FVN_REGISTER - BaseAuditEntity schema compatibility
===============================================================================
BaseAuditEntity currently exposes:
  Id
  IsActive
  CreatedBy
  LastModifiedSource
  CreatedAt
  ModifiedBy
  ModifiedAt

This compatibility migration is idempotent. It adds the missing audit columns
to tables mapped by entities inheriting BaseAuditEntity (including
BaseRequestEntity descendants).

Existing columns are never altered.
===============================================================================
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @AuditTables TABLE (TableName sysname NOT NULL PRIMARY KEY);

INSERT @AuditTables(TableName)
VALUES
    (N'F03ApprovalHistories'),
    (N'F03ApprovalPolicies'),
    (N'F03ApprovalReminderLogs'),
    (N'F03ApprovalSelections'),
    (N'F03ApprovalSnapshots'),
    (N'F03ApprovalStepSnapshots'),
    (N'F03Approvers'),
    (N'F03AppNotifications'),
    (N'F03Attachments'),
    (N'F03AuditLogs'),
    (N'F03BusinessRules'),
    (N'F03CompanyHolidays'),
    (N'F03EmailLogs'),
    (N'F03EmailProfiles'),
    (N'F03EmailQueues'),
    (N'F03EmailTemplates'),
    (N'F03EscalationLogs'),
    (N'F03EscalationRules'),
    (N'F03PublicInformation'),
    (N'F03UserLogs'),
    (N'F03WorkYears'),
    (N'F03EquipmentAssets'),
    (N'F03EquipmentFieldDefinitions'),
    (N'F03EquipmentImportBatches'),
    (N'F03EquipmentImportRows'),
    (N'F03EquipmentRepairHistory'),
    (N'F03EquipmentRequests'),
    (N'F03Departments'),
    (N'F03Employees'),
    (N'F03Genders'),
    (N'F03Positions'),
    (N'F03LeaveDays'),
    (N'F03LeaveDayDetails'),
    (N'F03LeaveBalances'),
    (N'F03LeaveTypes'),
    (N'F03OTEmployees'),
    (N'F03OTLimitRules'),
    (N'F03OTReasonCodes'),
    (N'F03OTTypes'),
    (N'F03OTRequests'),
    (N'F03PayrollCalculationPeriods'),
    (N'F03PayrollInputs'),
    (N'F03PublicForms'),
    (N'F03PublicFormAnswers'),
    (N'F03PublicFormAudiences'),
    (N'F03PublicFormQuestions'),
    (N'F03PublicFormQuestionOptions'),
    (N'F03PublicFormSubmissions'),
    (N'F03Functions'),
    (N'F03HrmUserRoleRules'),
    (N'F03ManagedScopes'),
    (N'F03PasswordResetRequests'),
    (N'F03Permissions'),
    (N'F03Roles'),
    (N'F03RoleFunctions'),
    (N'F03Users'),
    (N'F03UserFunctions'),
    (N'F03UserRoles'),
    (N'F03UserSessions'),
    (N'F03StagingTrips'),
    (N'F03TripRequests'),
    (N'F03ActionItems'),
    (N'F03ActionPolicies'),
    (N'F03CalendarModuleDefinitions'),
    (N'F03CalendarModulePolicies'),
    (N'F03CalendarProjections'),
    (N'F03ExecutionConfirmations'),
    (N'F03ExecutionConfirmationEvidence'),
    (N'F03ExecutionCorrections'),
    (N'F03ExecutionPolicies'),
    (N'F03ExecutionReconciliations'),
    (N'F03ExecutionReconciliationHistory'),
    (N'F03ExecutionResolutions');

DECLARE @TableName sysname;
DECLARE @Sql nvarchar(max);

DECLARE AuditCursor CURSOR LOCAL FAST_FORWARD FOR
SELECT TableName
FROM @AuditTables
WHERE OBJECT_ID(N'dbo.' + TableName, N'U') IS NOT NULL;

OPEN AuditCursor;
FETCH NEXT FROM AuditCursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF COL_LENGTH(N'dbo.' + @TableName, N'IsActive') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE dbo.' + QUOTENAME(@TableName)
                 + N' ADD IsActive bit NULL CONSTRAINT '
                 + QUOTENAME(N'DF_' + @TableName + N'_IsActive')
                 + N' DEFAULT(1) WITH VALUES;';
        EXEC sys.sp_executesql @Sql;
    END;

    IF COL_LENGTH(N'dbo.' + @TableName, N'CreatedBy') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE dbo.' + QUOTENAME(@TableName)
                 + N' ADD CreatedBy int NOT NULL CONSTRAINT '
                 + QUOTENAME(N'DF_' + @TableName + N'_CreatedBy')
                 + N' DEFAULT(0) WITH VALUES;';
        EXEC sys.sp_executesql @Sql;
    END;

    IF COL_LENGTH(N'dbo.' + @TableName, N'CreatedAt') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE dbo.' + QUOTENAME(@TableName)
                 + N' ADD CreatedAt datetime2(7) NOT NULL CONSTRAINT '
                 + QUOTENAME(N'DF_' + @TableName + N'_CreatedAt')
                 + N' DEFAULT(GETDATE()) WITH VALUES;';
        EXEC sys.sp_executesql @Sql;
    END;

    IF COL_LENGTH(N'dbo.' + @TableName, N'ModifiedBy') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE dbo.' + QUOTENAME(@TableName)
                 + N' ADD ModifiedBy int NULL;';
        EXEC sys.sp_executesql @Sql;
    END;

    IF COL_LENGTH(N'dbo.' + @TableName, N'ModifiedAt') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE dbo.' + QUOTENAME(@TableName)
                 + N' ADD ModifiedAt datetime2(7) NULL;';
        EXEC sys.sp_executesql @Sql;
    END;

    IF COL_LENGTH(N'dbo.' + @TableName, N'LastModifiedSource') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE dbo.' + QUOTENAME(@TableName)
                 + N' ADD LastModifiedSource nvarchar(max) NULL;';
        EXEC sys.sp_executesql @Sql;
    END;

    FETCH NEXT FROM AuditCursor INTO @TableName;
END;

CLOSE AuditCursor;
DEALLOCATE AuditCursor;

/* Verification: only tables that exist are reported. */
SELECT
    t.name AS TableName,
    CASE WHEN COL_LENGTH(N'dbo.' + t.name, N'LastModifiedSource') IS NULL THEN 0 ELSE 1 END AS HasLastModifiedSource
FROM sys.tables AS t
WHERE t.name IN (SELECT TableName FROM @AuditTables)
ORDER BY t.name;
