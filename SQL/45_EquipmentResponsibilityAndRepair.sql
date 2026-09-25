IF COL_LENGTH(N'dbo.F03EquipmentAssets', N'OperatingResponsibleDeptCode') IS NULL ALTER TABLE dbo.F03EquipmentAssets ADD OperatingResponsibleDeptCode nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentAssets', N'OperatingResponsibleEmployeeCode') IS NULL ALTER TABLE dbo.F03EquipmentAssets ADD OperatingResponsibleEmployeeCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentAssets', N'OperatingResponsibleAssignedAt') IS NULL ALTER TABLE dbo.F03EquipmentAssets ADD OperatingResponsibleAssignedAt datetime2 NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRequests', N'RepairResponsibleDeptCode') IS NULL ALTER TABLE dbo.F03EquipmentRequests ADD RepairResponsibleDeptCode nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRequests', N'RepairAssigneeEmployeeCode') IS NULL ALTER TABLE dbo.F03EquipmentRequests ADD RepairAssigneeEmployeeCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRequests', N'RepairAssigneeUserId') IS NULL ALTER TABLE dbo.F03EquipmentRequests ADD RepairAssigneeUserId int NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRequests', N'RepairFeedback') IS NULL ALTER TABLE dbo.F03EquipmentRequests ADD RepairFeedback nvarchar(1000) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRequests', N'RepairCompletedAt') IS NULL ALTER TABLE dbo.F03EquipmentRequests ADD RepairCompletedAt datetime2 NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRepairHistory', N'ResponsibleDeptCode') IS NULL ALTER TABLE dbo.F03EquipmentRepairHistory ADD ResponsibleDeptCode nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRepairHistory', N'RepairerEmployeeCode') IS NULL ALTER TABLE dbo.F03EquipmentRepairHistory ADD RepairerEmployeeCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRepairHistory', N'RepairFeedback') IS NULL ALTER TABLE dbo.F03EquipmentRepairHistory ADD RepairFeedback nvarchar(1000) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentRepairHistory', N'CompletedAt') IS NULL ALTER TABLE dbo.F03EquipmentRepairHistory ADD CompletedAt datetime2 NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAssets_OperatingResponsible' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssets')) CREATE INDEX IX_F03EquipmentAssets_OperatingResponsible ON dbo.F03EquipmentAssets(OperatingResponsibleEmployeeCode, OperatingResponsibleDeptCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentRequests_RepairAssignee' AND object_id=OBJECT_ID(N'dbo.F03EquipmentRequests')) CREATE INDEX IX_F03EquipmentRequests_RepairAssignee ON dbo.F03EquipmentRequests(RepairAssigneeEmployeeCode, RequestStatus);
IF COL_LENGTH(N'dbo.F03EquipmentImportBatches', N'AssignToEmployee') IS NULL
BEGIN
    ALTER TABLE dbo.F03EquipmentImportBatches ADD AssignToEmployee bit NOT NULL CONSTRAINT DF_F03EquipmentImportBatches_AssignToEmployee DEFAULT (0);
END;
GO

/* Repair execution mail templates */
IF OBJECT_ID(N'dbo.F03EmailTemplates', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'EQUIPMENT_REPAIR_ASSIGNED')
        INSERT dbo.F03EmailTemplates(IsActive,CreatedBy,CreatedAt,Code,Subject,Body,Description)
        VALUES (1,0,GETDATE(),'EQUIPMENT_REPAIR_ASSIGNED',N'[FCC Smart Portal] Nhiệm vụ sửa chữa thiết bị {{EquipmentName}}',
                N'<p>Kính gửi {{EmployeeName}},</p><p>Bạn được giao xử lý yêu cầu sửa chữa thiết bị <b>{{EquipmentName}}</b>.</p><p>Yêu cầu #{{Id}}: {{RepairContent}}</p><p>Vui lòng đăng nhập FCC Smart Portal để tiếp nhận và phản hồi kết quả.</p>',
                N'Giao nhiệm vụ sửa chữa thiết bị');

    IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'EQUIPMENT_REPAIR_COMPLETED')
        INSERT dbo.F03EmailTemplates(IsActive,CreatedBy,CreatedAt,Code,Subject,Body,Description)
        VALUES (1,0,GETDATE(),'EQUIPMENT_REPAIR_COMPLETED',N'[FCC Smart Portal] Hoàn thành sửa chữa {{EquipmentName}}',
                N'<p>Yêu cầu sửa chữa #{{Id}} cho thiết bị <b>{{EquipmentName}}</b> đã hoàn thành.</p><p>Kết quả: {{RepairFeedback}}</p><p>Thời gian hoàn thành: {{CompletedAt}}</p>',
                N'Thông báo kết quả sửa chữa');

    IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'EQUIPMENT_REPAIR_ASSIGNED' AND Priority = 100)
        INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
        SELECT 1,0,GETDATE(),'EQUIPMENT_REPAIR_ASSIGNED',COALESCE((SELECT TOP 1 Code FROM dbo.F03EmailProfiles WHERE IsActive=1 AND IsDefault=1),'SYSTEMSMTP'),'AutoSend',100,N'Giao nhiệm vụ sửa chữa';

    IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'EQUIPMENT_REPAIR_COMPLETED' AND Priority = 100)
        INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
        SELECT 1,0,GETDATE(),'EQUIPMENT_REPAIR_COMPLETED',COALESCE((SELECT TOP 1 Code FROM dbo.F03EmailProfiles WHERE IsActive=1 AND IsDefault=1),'SYSTEMSMTP'),'AutoSend',100,N'Thông báo hoàn thành sửa chữa';
END;
GO

IF COL_LENGTH(N'dbo.F03EquipmentRequests', N'ResponsibleEmployeeCode') IS NULL
    ALTER TABLE dbo.F03EquipmentRequests ADD ResponsibleEmployeeCode nvarchar(50) NULL;
GO
