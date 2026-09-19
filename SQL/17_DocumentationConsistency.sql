-- FVN REGISTER documentation consistency checks
-- This script is intentionally read-only.

IF OBJECT_ID('dbo.F03Functions','U') IS NOT NULL
BEGIN
    SELECT FunctionCode, ModuleCode, ActionCode, ScopeCode, DisplayOrder
    FROM dbo.F03Functions
    WHERE FunctionCode IN (2001,2005,2101,2105,2201,2205,2301,2305,2306,2701,2801)
    ORDER BY FunctionCode;
END

IF OBJECT_ID('dbo.F03EquipmentFieldDefinitions','U') IS NOT NULL
BEGIN
    SELECT DeptCode, FieldKey, FieldLabel, DataType, IsRequired, IsImportable, IsSearchable, IsActiveField, DisplayOrder
    FROM dbo.F03EquipmentFieldDefinitions
    ORDER BY DeptCode, DisplayOrder, FieldKey;
END

IF OBJECT_ID('dbo.F03EquipmentImportBatches','U') IS NOT NULL
BEGIN
    SELECT Id, DeptCode, FileName, Status, TotalRows, ValidRows, InvalidRows, ImportedRows
    FROM dbo.F03EquipmentImportBatches
    ORDER BY Id DESC;
END

IF OBJECT_ID('dbo.F03PublicInformation','U') IS NOT NULL
BEGIN
    SELECT Id, Type, Title, Status, EffectiveFrom, EffectiveTo, IsImportant, PublishedAt
    FROM dbo.F03PublicInformation
    ORDER BY Id DESC;
END

IF OBJECT_ID('dbo.F03AuditLogs','U') IS NOT NULL
BEGIN
    SELECT TOP (50) Action, UserId, CreatedAt, Description
    FROM dbo.F03AuditLogs
    WHERE Action IN ('SECURITY_USER_ROLES_CHANGED','SECURITY_ROLE_FUNCTIONS_CHANGED',
                     'APPROVAL_APPROVED','APPROVAL_REJECTED',
                     'PUBLIC_INFO_CREATED','PUBLIC_INFO_UPDATED','PUBLIC_INFO_PUBLISHED','PUBLIC_INFO_ARCHIVED',
                     'EQUIPMENT_IMPORT_STAGED','EQUIPMENT_IMPORT_COMMITTED')
    ORDER BY CreatedAt DESC;
END