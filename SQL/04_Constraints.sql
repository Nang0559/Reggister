USE [FVN_REGISTER];
GO
/* EF business-key indexes */
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03Permissions_Code' AND object_id=OBJECT_ID('dbo.F03Permissions')) CREATE UNIQUE INDEX IX_F03Permissions_Code ON dbo.F03Permissions(PermissionCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03Functions_Code' AND object_id=OBJECT_ID('dbo.F03Functions')) CREATE UNIQUE INDEX IX_F03Functions_Code ON dbo.F03Functions(FunctionCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03Users_EmployeeCode' AND object_id=OBJECT_ID('dbo.F03Users')) CREATE UNIQUE INDEX IX_F03Users_EmployeeCode ON dbo.F03Users(EmployeeCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03Departments_Code' AND object_id=OBJECT_ID('dbo.F03Departments')) CREATE UNIQUE INDEX IX_F03Departments_Code ON dbo.F03Departments(DeptCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03Positions_Code' AND object_id=OBJECT_ID('dbo.F03Positions')) CREATE UNIQUE INDEX IX_F03Positions_Code ON dbo.F03Positions(PositionCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03Genders_Code' AND object_id=OBJECT_ID('dbo.F03Genders')) CREATE UNIQUE INDEX IX_F03Genders_Code ON dbo.F03Genders(GenderCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03LeaveType_Code' AND object_id=OBJECT_ID('dbo.F03LeaveType')) CREATE UNIQUE INDEX IX_F03LeaveType_Code ON dbo.F03LeaveType(LeaveTypeCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03OTTypes_Code' AND object_id=OBJECT_ID('dbo.F03OTTypes')) CREATE UNIQUE INDEX IX_F03OTTypes_Code ON dbo.F03OTTypes(OTTypeCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03OTCodes_Code' AND object_id=OBJECT_ID('dbo.F03OTCodes')) CREATE UNIQUE INDEX IX_F03OTCodes_Code ON dbo.F03OTCodes(ReasonCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03OTRequests_Code' AND object_id=OBJECT_ID('dbo.F03OTRequests')) CREATE UNIQUE INDEX IX_F03OTRequests_Code ON dbo.F03OTRequests(OTCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03TripRequests_Code' AND object_id=OBJECT_ID('dbo.F03TripRequests')) CREATE UNIQUE INDEX IX_F03TripRequests_Code ON dbo.F03TripRequests(TripCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03EquipmentAssets_Code' AND object_id=OBJECT_ID('dbo.F03EquipmentAssets')) CREATE UNIQUE INDEX IX_F03EquipmentAssets_Code ON dbo.F03EquipmentAssets(EquipmentCode);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03EquipmentAssets_QrToken' AND object_id=OBJECT_ID('dbo.F03EquipmentAssets')) CREATE UNIQUE INDEX IX_F03EquipmentAssets_QrToken ON dbo.F03EquipmentAssets(QrToken);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03EquipmentRepair_Request' AND object_id=OBJECT_ID('dbo.F03EquipmentRepairHistory')) CREATE UNIQUE INDEX IX_F03EquipmentRepair_Request ON dbo.F03EquipmentRepairHistory(RequestId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03EmailProfiles_Code' AND object_id=OBJECT_ID('dbo.F03EmailProfiles')) CREATE UNIQUE INDEX IX_F03EmailProfiles_Code ON dbo.F03EmailProfiles(Code);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03EmailTemplates_Code' AND object_id=OBJECT_ID('dbo.F03EmailTemplates')) CREATE UNIQUE INDEX IX_F03EmailTemplates_Code ON dbo.F03EmailTemplates(Code);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03BusinessRules_Module_Code' AND object_id=OBJECT_ID('dbo.F03BusinessRules')) CREATE UNIQUE INDEX IX_F03BusinessRules_Module_Code ON dbo.F03BusinessRules(Module,Code);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03LeaveBalance_Employee_Year' AND object_id=OBJECT_ID('dbo.F03LeaveBalances')) CREATE UNIQUE INDEX IX_F03LeaveBalance_Employee_Year ON dbo.F03LeaveBalances(EmployeeCode,WorkYear);
GO
/* EF relationships */
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03Employees_Department') ALTER TABLE dbo.F03Employees ADD CONSTRAINT FK_F03Employees_Department FOREIGN KEY(DeptCode) REFERENCES dbo.F03Departments(DeptCode);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03Employees_Position') ALTER TABLE dbo.F03Employees ADD CONSTRAINT FK_F03Employees_Position FOREIGN KEY(PositionCode) REFERENCES dbo.F03Positions(PositionCode);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03Users_Permission') ALTER TABLE dbo.F03Users ADD CONSTRAINT FK_F03Users_Permission FOREIGN KEY(PermissionCode) REFERENCES dbo.F03Permissions(PermissionCode);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03UserFunctions_User') ALTER TABLE dbo.F03UserFunctions ADD CONSTRAINT FK_F03UserFunctions_User FOREIGN KEY(IdUser) REFERENCES dbo.F03Users(IdUser) ON DELETE CASCADE;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03UserFunctions_Permission') ALTER TABLE dbo.F03UserFunctions ADD CONSTRAINT FK_F03UserFunctions_Permission FOREIGN KEY(IdPermission) REFERENCES dbo.F03Permissions(IdPermission);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03UserFunctions_Function') ALTER TABLE dbo.F03UserFunctions ADD CONSTRAINT FK_F03UserFunctions_Function FOREIGN KEY(IdFunction) REFERENCES dbo.F03Functions(IdFunction) ON DELETE CASCADE;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03UserSessions_User') ALTER TABLE dbo.F03UserSessions ADD CONSTRAINT FK_F03UserSessions_User FOREIGN KEY(UserId) REFERENCES dbo.F03Users(IdUser) ON DELETE CASCADE;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03LeaveDayDetail_F03LeaveDay') ALTER TABLE dbo.F03LeaveDayDetails ADD CONSTRAINT FK_F03LeaveDayDetail_F03LeaveDay FOREIGN KEY(LeaveDaysId) REFERENCES dbo.F03LeaveDays(Id) ON DELETE CASCADE;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03OTEmployee_Request') ALTER TABLE dbo.F03OTEmployees ADD CONSTRAINT FK_F03OTEmployee_Request FOREIGN KEY(OTRequestId) REFERENCES dbo.F03OTRequests(Id) ON DELETE CASCADE;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03EquipmentRequest_Asset') ALTER TABLE dbo.F03EquipmentRequests ADD CONSTRAINT FK_F03EquipmentRequest_Asset FOREIGN KEY(AssetId) REFERENCES dbo.F03EquipmentAssets(Id);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03EquipmentAsset_RepairHistory') ALTER TABLE dbo.F03EquipmentRepairHistory ADD CONSTRAINT FK_F03EquipmentAsset_RepairHistory FOREIGN KEY(AssetId) REFERENCES dbo.F03EquipmentAssets(Id);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03EquipmentAsset_Requests') ALTER TABLE dbo.F03EquipmentRequests ADD CONSTRAINT FK_F03EquipmentAsset_Requests FOREIGN KEY(AssetId) REFERENCES dbo.F03EquipmentAssets(Id);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03ApprovalStepSnapshot_Snapshot') ALTER TABLE dbo.F03ApprovalStepSnapshots ADD CONSTRAINT FK_F03ApprovalStepSnapshot_Snapshot FOREIGN KEY(SnapshotId) REFERENCES dbo.F03ApprovalSnapshots(Id) ON DELETE CASCADE;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03Approver_User') ALTER TABLE dbo.F03Approvers ADD CONSTRAINT FK_F03Approver_User FOREIGN KEY(UserId) REFERENCES dbo.F03Users(IdUser);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_F03AuditLog_User') ALTER TABLE dbo.F03AuditLogs ADD CONSTRAINT FK_F03AuditLog_User FOREIGN KEY(UserId) REFERENCES dbo.F03Users(IdUser);
GO
