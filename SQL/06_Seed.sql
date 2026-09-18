USE [FVN_REGISTER];
GO
IF NOT EXISTS(SELECT 1 FROM dbo.F03Permissions WHERE PermissionCode=1)
INSERT dbo.F03Permissions(PermissionCode,PermissionName,Detail,CreatedBy) VALUES(1,N'User',N'Quyền người dùng cơ bản',0);
IF NOT EXISTS(SELECT 1 FROM dbo.F03Permissions WHERE PermissionCode=2)
INSERT dbo.F03Permissions(PermissionCode,PermissionName,Detail,CreatedBy) VALUES(2,N'Admin',N'Quản trị hệ thống',0);
IF NOT EXISTS(SELECT 1 FROM dbo.F03Permissions WHERE PermissionCode=3)
INSERT dbo.F03Permissions(PermissionCode,PermissionName,Detail,CreatedBy) VALUES(3,N'SuperAdmin',N'Quản trị toàn hệ thống',0);

INSERT dbo.F03Functions(FunctionCode,FunctionName,Detail,CreatedBy)
SELECT 1001,N'LeaveModule',N'Quản lý nghỉ phép',0 WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=1001);
INSERT dbo.F03Functions(FunctionCode,FunctionName,Detail,CreatedBy)
SELECT 1002,N'OTModule',N'Quản lý tăng ca',0 WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=1002);
INSERT dbo.F03Functions(FunctionCode,FunctionName,Detail,CreatedBy)
SELECT 1003,N'TripModule',N'Quản lý công tác',0 WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=1003);
INSERT dbo.F03Functions(FunctionCode,FunctionName,Detail,CreatedBy)
SELECT 1004,N'EquipmentModule',N'Sổ quản lý thiết bị và QR',0 WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=1004);
INSERT dbo.F03Functions(FunctionCode,FunctionName,Detail,CreatedBy)
SELECT 1005,N'ApprovalModule',N'Quản lý phê duyệt',0 WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=1005);

INSERT dbo.F03EmailTemplates(Code,Subject,Body,Description,CreatedBy)
SELECT N'TRIP_APPROVED',N'Đăng ký công tác đã được duyệt',N'Đăng ký công tác {{TripCode}} đã được duyệt.',N'Trip approved',0
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'TRIP_APPROVED');
INSERT dbo.F03EmailTemplates(Code,Subject,Body,Description,CreatedBy)
SELECT N'TRIP_REJECTED',N'Đăng ký công tác bị từ chối',N'Đăng ký công tác {{TripCode}} đã bị từ chối.',N'Trip rejected',0
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'TRIP_REJECTED');
INSERT dbo.F03EmailTemplates(Code,Subject,Body,Description,CreatedBy)
SELECT N'LEAVE_APPROVAL',N'Đơn nghỉ phép cần duyệt',N'Đơn nghỉ phép {{RequestId}} đang chờ bạn xử lý.',N'Leave approval notification',0
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'LEAVE_APPROVAL');
INSERT dbo.F03EmailTemplates(Code,Subject,Body,Description,CreatedBy)
SELECT N'OT_APPROVAL',N'Đơn OT cần duyệt',N'Đơn OT {{RequestId}} đang chờ bạn xử lý.',N'OT approval notification',0
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'OT_APPROVAL');
INSERT dbo.F03EmailTemplates(Code,Subject,Body,Description,CreatedBy)
SELECT N'EQUIPMENT_APPROVAL',N'Yêu cầu thiết bị cần duyệt',N'Yêu cầu thiết bị {{RequestId}} đang chờ bạn xử lý.',N'Equipment approval notification',0
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'EQUIPMENT_APPROVAL');
IF NOT EXISTS(SELECT 1 FROM dbo.F03WorkYears WHERE WorkYear=YEAR(GETDATE()))
INSERT dbo.F03WorkYears(WorkYear,StartDate,EndDate,Remark,CreatedBy)
VALUES(YEAR(GETDATE()),DATEFROMPARTS(YEAR(GETDATE()),1,1),DATEFROMPARTS(YEAR(GETDATE()),12,31),N'Auto seed',0);
GO
