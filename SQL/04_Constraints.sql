USE [FVN_REGISTER];
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Permissions_Code' AND object_id=OBJECT_ID(N'dbo.F03Permissions')) CREATE UNIQUE INDEX IX_F03Permissions_Code ON dbo.F03Permissions(PermissionCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Functions_Code' AND object_id=OBJECT_ID(N'dbo.F03Functions')) CREATE UNIQUE INDEX IX_F03Functions_Code ON dbo.F03Functions(FunctionCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Users_EmployeeCode' AND object_id=OBJECT_ID(N'dbo.F03Users')) CREATE UNIQUE INDEX IX_F03Users_EmployeeCode ON dbo.F03Users(EmployeeCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Departments_Code' AND object_id=OBJECT_ID(N'dbo.F03Departments')) CREATE UNIQUE INDEX IX_F03Departments_Code ON dbo.F03Departments(DeptCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Positions_Code' AND object_id=OBJECT_ID(N'dbo.F03Positions')) CREATE UNIQUE INDEX IX_F03Positions_Code ON dbo.F03Positions(PositionCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Genders_Code' AND object_id=OBJECT_ID(N'dbo.F03Genders')) CREATE UNIQUE INDEX IX_F03Genders_Code ON dbo.F03Genders(GenderCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03LeaveType_Code' AND object_id=OBJECT_ID(N'dbo.F03LeaveType')) CREATE UNIQUE INDEX IX_F03LeaveType_Code ON dbo.F03LeaveType(LeaveTypeCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03OTTypes_Code' AND object_id=OBJECT_ID(N'dbo.F03OTTypes')) CREATE UNIQUE INDEX IX_F03OTTypes_Code ON dbo.F03OTTypes(OTTypeCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03OTCodes_Code' AND object_id=OBJECT_ID(N'dbo.F03OTCodes')) CREATE UNIQUE INDEX IX_F03OTCodes_Code ON dbo.F03OTCodes(ReasonCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03OTRequests_Code' AND object_id=OBJECT_ID(N'dbo.F03OTRequests')) CREATE UNIQUE INDEX IX_F03OTRequests_Code ON dbo.F03OTRequests(OTCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03TripRequests_Code' AND object_id=OBJECT_ID(N'dbo.F03TripRequests')) CREATE UNIQUE INDEX IX_F03TripRequests_Code ON dbo.F03TripRequests(TripCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAssets_Code' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssets')) CREATE UNIQUE INDEX IX_F03EquipmentAssets_Code ON dbo.F03EquipmentAssets(EquipmentCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAssets_QrToken' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssets')) CREATE UNIQUE INDEX IX_F03EquipmentAssets_QrToken ON dbo.F03EquipmentAssets(QrToken);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentRepair_Request' AND object_id=OBJECT_ID(N'dbo.F03EquipmentRepairHistory')) CREATE UNIQUE INDEX IX_F03EquipmentRepair_Request ON dbo.F03EquipmentRepairHistory(RequestId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EmailProfiles_Code' AND object_id=OBJECT_ID(N'dbo.F03EmailProfiles')) CREATE UNIQUE INDEX IX_F03EmailProfiles_Code ON dbo.F03EmailProfiles(Code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EmailTemplates_Code' AND object_id=OBJECT_ID(N'dbo.F03EmailTemplates')) CREATE UNIQUE INDEX IX_F03EmailTemplates_Code ON dbo.F03EmailTemplates(Code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03BusinessRules_Module_Code' AND object_id=OBJECT_ID(N'dbo.F03BusinessRules')) CREATE UNIQUE INDEX IX_F03BusinessRules_Module_Code ON dbo.F03BusinessRules(Module,Code);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03LeaveBalance_Employee_Year' AND object_id=OBJECT_ID(N'dbo.F03LeaveBalances')) CREATE UNIQUE INDEX IX_F03LeaveBalance_Employee_Year ON dbo.F03LeaveBalances(EmployeeCode,WorkYear);
GO
