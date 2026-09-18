USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now datetime2(0)=GETDATE();

-- =========================
-- Master data: 5 realistic test rows per important lookup table
-- =========================
INSERT dbo.F03Departments(IsActive,CreatedBy,DeptCode,DeptName,ParentDeptCode,DisplayPriority,ShowInReport)
SELECT 1,0,v.DeptCode,v.DeptName,v.ParentDeptCode,v.DisplayPriority,1
FROM (VALUES
(N'IT',N'Information Technology',NULL,1),
(N'HR',N'Human Resources',NULL,2),
(N'FIN',N'Finance',NULL,3),
(N'PROD',N'Production',NULL,4),
(N'QA',N'Quality Assurance',N'PROD',5)
) AS v(DeptCode,DeptName,ParentDeptCode,DisplayPriority)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Departments x WHERE x.DeptCode=v.DeptCode);

INSERT dbo.F03Positions(IsActive,CreatedBy,PositionCode,PositionName,IsApprove,IsAllowApprove,DefaultApproveLevel)
SELECT 1,0,v.PositionCode,v.PositionName,v.IsApprove,v.IsAllowApprove,v.DefaultApproveLevel
FROM (VALUES
(N'EMP',N'Employee',0,0,0),
(N'SL',N'Sub Leader',1,1,1),
(N'CHIEF',N'Chief',1,1,2),
(N'MGR',N'Manager',1,1,3),
(N'GM',N'General Manager',1,1,4)
) AS v(PositionCode,PositionName,IsApprove,IsAllowApprove,DefaultApproveLevel)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Positions x WHERE x.PositionCode=v.PositionCode);

INSERT dbo.F03Genders(IsActive,CreatedBy,GenderCode,GenderName)
SELECT 1,0,v.GenderCode,v.GenderName FROM (VALUES
(N'M',N'Male'),(N'F',N'Female'),(N'O',N'Other'),(N'U',N'Unknown'),(N'N',N'Not specified')
) AS v(GenderCode,GenderName)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Genders x WHERE x.GenderCode=v.GenderCode);

INSERT dbo.F03LeaveType(IsActive,CreatedBy,LeaveTypeCode,LeaveTypeName,LeaveTypeName2,IsCountedAsLeave,HRMCode)
SELECT 1,0,v.Code,v.Name,v.Name2,v.Counted,v.HRMCode FROM (VALUES
(N'AL',N'Annual Leave',N'Annual Leave',1,N'AL'),
(N'SL',N'Sick Leave',N'Sick Leave',1,N'SL'),
(N'UL',N'Unpaid Leave',N'Unpaid Leave',0,N'UL'),
(N'HD',N'Half Day Leave',N'Half Day Leave',1,N'HD'),
(N'OTR',N'Other Leave',N'Other Leave',0,N'OTR')
) AS v(Code,Name,Name2,Counted,HRMCode)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03LeaveType x WHERE x.LeaveTypeCode=v.Code);

INSERT dbo.F03OTTypes(IsActive,CreatedBy,OTTypeCode,OTTypeName,OTTypeName2,RateMultiplier,HRMCode)
SELECT 1,0,v.Code,v.Name,v.Name2,v.Rate,v.HRM FROM (VALUES
(N'WD',N'Weekday OT',N'Weekday OT',1.5,N'OT-WD'),
(N'SAT',N'Saturday OT',N'Saturday OT',2.0,N'OT-SAT'),
(N'SUN',N'Sunday OT',N'Sunday OT',2.0,N'OT-SUN'),
(N'HOL',N'Holiday OT',N'Holiday OT',3.0,N'OT-HOL'),
(N'NIGHT',N'Night OT',N'Night OT',2.0,N'OT-NIGHT')
) AS v(Code,Name,Name2,Rate,HRM)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03OTTypes x WHERE x.OTTypeCode=v.Code);

INSERT dbo.F03OTCodes(IsActive,CreatedBy,ReasonCode,DisplayName,Description,DisplayOrder)
SELECT 1,0,v.Code,v.Name,v.Description,v.SortNo FROM (VALUES
(N'PROD',N'Production support',N'Production support',1),
(N'MAINT',N'Maintenance',N'Machine maintenance',2),
(N'PROJECT',N'Project deadline',N'Project deadline',3),
(N'URGENT',N'Urgent work',N'Urgent customer/business request',4),
(N'OTHER',N'Other',N'Other approved reason',5)
) AS v(Code,Name,Description,SortNo)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03OTCodes x WHERE x.ReasonCode=v.Code);

-- Work years / holidays
IF NOT EXISTS(SELECT 1 FROM dbo.F03WorkYears WHERE WorkYear=YEAR(@Now))
INSERT dbo.F03WorkYears(WorkYear,StartDate,EndDate,Remark,CreatedBy)
VALUES(YEAR(@Now),DATEFROMPARTS(YEAR(@Now),1,1),DATEFROMPARTS(YEAR(@Now),12,31),N'Test work year',0);

INSERT dbo.F03CompanyHolidays(IsActive,CreatedBy,HolidayDate,Description,Year,IsPaidLeave)
SELECT 1,0,v.HolidayDate,v.Description,YEAR(v.HolidayDate),1
FROM (VALUES
(DATEFROMPARTS(YEAR(@Now),1,1),N'New Year'),
(DATEFROMPARTS(YEAR(@Now),4,30),N'Reunification Day'),
(DATEFROMPARTS(YEAR(@Now),5,1),N'International Labour Day'),
(DATEFROMPARTS(YEAR(@Now),9,2),N'National Day'),
(DATEADD(day,-1,DATEFROMPARTS(YEAR(@Now),9,2)),N'Test company holiday')
) AS v(HolidayDate,Description)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03CompanyHolidays h WHERE h.HolidayDate=v.HolidayDate);

-- =========================
-- Employees / users
-- Password values are TEST ONLY. Change them before production.
-- =========================
INSERT dbo.F03Employees(IsActive,CreatedBy,EmployeeCode,EmployeeName,DeptCode,PositionCode,BirthDate,GenderCode,EmailAddress,PhoneNumber,FirstWorkingDate,TotalLeaveDays,EmployeeNo,LevelApprove)
SELECT 1,0,v.Code,v.Name,v.Dept,v.Position,v.Birth,v.GenderId,v.Email,v.Phone,v.StartDate,v.LeaveDays,v.EmpNo,v.LevelApprove
FROM (VALUES
(N'E0001',N'Nguyen Van An',N'IT',N'EMP','1990-02-10',1,N'e0001@test.local',N'0900000001','2020-01-06',12,1,0),
(N'E0002',N'Tran Thi Binh',N'IT',N'SL','1988-06-15',2,N'e0002@test.local',N'0900000002','2019-03-11',14,2,1),
(N'E0003',N'Le Van Cuong',N'PROD',N'CHIEF','1985-09-20',1,N'e0003@test.local',N'0900000003','2018-07-02',14,3,2),
(N'E0004',N'Pham Thi Dung',N'HR',N'MGR','1983-12-01',2,N'e0004@test.local',N'0900000004','2017-04-03',16,4,3),
(N'E0005',N'Hoang Van Em',N'PROD',N'GM','1978-11-25',1,N'e0005@test.local',N'0900000005','2015-01-05',18,5,4)
) AS v(Code,Name,Dept,Position,Birth,GenderId,Email,Phone,StartDate,LeaveDays,EmpNo,LevelApprove)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Employees e WHERE e.EmployeeCode=v.Code);

INSERT dbo.F03Users(IsActive,CreatedBy,Password,EmployeeCode,FullName,PermissionCode,LockoutEnable,NumLoginFailed,LevelApprove,DeptCode,Cvcode)
SELECT 1,0,N'Test@123',v.Code,v.Name,v.PermissionCode,1,0,v.LevelApprove,v.Dept,v.Position
FROM (VALUES
(N'E0001',N'Nguyen Van An',1,0,N'IT',N'EMP'),
(N'E0002',N'Tran Thi Binh',1,1,N'IT',N'SL'),
(N'E0003',N'Le Van Cuong',2,2,N'PROD',N'CHIEF'),
(N'E0004',N'Pham Thi Dung',2,3,N'HR',N'MGR'),
(N'E0005',N'Hoang Van Em',3,4,N'PROD',N'GM')
) AS v(Code,Name,PermissionCode,LevelApprove,Dept,Position)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Users u WHERE u.EmployeeCode=v.Code);

INSERT dbo.F03LeaveBalances(IsActive,CreatedBy,EmployeeCode,WorkYear,TotalDays)
SELECT 1,0,v.Code,YEAR(@Now),v.Days FROM (VALUES
(N'E0001',12.0),(N'E0002',14.0),(N'E0003',14.0),(N'E0004',16.0),(N'E0005',18.0)
) AS v(Code,Days)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03LeaveBalances b WHERE b.EmployeeCode=v.Code AND b.WorkYear=YEAR(@Now));

-- Permission/function mapping
INSERT dbo.F03UserFunctions(IdUser,IdPermission,IdFunction)
SELECT u.IdUser,p.IdPermission,f.IdFunction
FROM dbo.F03Users u CROSS JOIN dbo.F03Permissions p CROSS JOIN dbo.F03Functions f
WHERE u.EmployeeCode=N'E0001' AND p.PermissionCode=1 AND f.FunctionCode=1001
AND NOT EXISTS(SELECT 1 FROM dbo.F03UserFunctions x WHERE x.IdUser=u.IdUser AND x.IdPermission=p.IdPermission AND x.IdFunction=f.IdFunction);

-- Approvers: five test approver rows
INSERT dbo.F03Approvers(IsActive,CreatedBy,UserId,RequestType,ApproverCode,PositionCode,ApproverName,ApproverEmail,ApproverDeptCode,ApproverDeptName,ApproveForDeptCode,ApproveForDeptName,Level,RoleName)
SELECT 1,0,u.IdUser,N'Leave',v.Code,v.Position,v.Name,v.Email,v.Dept,d.DeptName,v.ForDept,d2.DeptName,v.Level,v.RoleName
FROM (VALUES
(N'E0002',N'SL',N'Tran Thi Binh',N'e0002@test.local',N'IT',N'IT',N'IT',1,N'Lead/Sub Lead'),
(N'E0003',N'CHIEF',N'Le Van Cuong',N'e0003@test.local',N'PROD',N'PROD',N'PROD',2,N'Chief/A Chief'),
(N'E0004',N'MGR',N'Pham Thi Dung',N'e0004@test.local',N'HR',N'HR',N'HR',3,N'Manager/A Manager'),
(N'E0005',N'GM',N'Hoang Van Em',N'e0005@test.local',N'PROD',N'PROD',N'PROD',4,N'General Manager'),
(N'E0003',N'CHIEF',N'Le Van Cuong',N'e0003@test.local',N'PROD',N'PRODUCTION',N'PROD',1,N'Lead/Sub Lead')
) AS v(Code,Position,Name,Email,Dept,DeptName,ForDept,Level,RoleName)
JOIN dbo.F03Users u ON u.EmployeeCode=v.Code
JOIN dbo.F03Departments d ON d.DeptCode=v.Dept
JOIN dbo.F03Departments d2 ON d2.DeptCode=v.ForDept
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Approvers a WHERE a.ApproverCode=v.Code AND a.RequestType=N'Leave' AND a.ApproveForDeptCode=v.ForDept AND a.Level=v.Level);

-- Trip / equipment / email test data
INSERT dbo.F03TripRequests(IsActive,CreatedBy,EmployeeCode,DeptCode,RequestStatus,TripCode,StartDate,EndDate,Destination,Purpose,CustomerOrPartner,TransportMethod,EstimatedCost,Note)
SELECT 1,0,N'E0001',N'IT',v.Status,v.Code,v.StartDate,v.EndDate,v.Destination,v.Purpose,v.Partner,v.Transport,v.Cost,v.Note
FROM (VALUES
(1,N'TRIP-TEST-001','2026-09-20','2026-09-21',N'Hanoi',N'Internal IT meeting',N'FCC VN',N'Car',1500000,N'Test pending'),
(3,N'TRIP-TEST-002','2026-09-22','2026-09-23',N'Bac Ninh',N'Factory support',N'Customer A',N'Car',2500000,N'Test approved'),
(4,N'TRIP-TEST-003','2026-09-24','2026-09-25',N'Ha Noi',N'Partner meeting',N'Partner B',N'Car',1800000,N'Test rejected'),
(5,N'TRIP-TEST-004','2026-09-26','2026-09-26',N'Ha Noi',N'Personal cancellation test',N'',N'Car',500000,N'Test cancelled'),
(1,N'TRIP-TEST-005','2026-09-28','2026-09-29',N'Ho Chi Minh City',N'Project workshop',N'Customer C',N'Plane',5000000,N'Test pending')
) AS v(Status,Code,StartDate,EndDate,Destination,Purpose,Partner,Transport,Cost,Note)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03TripRequests t WHERE t.TripCode=v.Code);

INSERT dbo.F03EquipmentAssets(IsActive,CreatedBy,EquipmentCode,EquipmentName,Specification,SerialNumber,AssetCode,PurchasePrice,PurchaseDate,ExpectedDepreciationDate,DeptCode,Location,QrToken,IsQrActive,Note)
SELECT 1,0,v.Code,v.Name,v.Spec,v.Serial,v.AssetCode,v.Price,v.PurchaseDate,v.DepDate,v.Dept,v.Location,v.Qr,1,v.Note
FROM (VALUES
(N'IT-TEST-001',N'Laptop Dell Latitude',N'i5/16GB/512GB',N'SN-LAP-001',N'AST-0001',25000000,'2026-01-10','2029-01-10',N'IT',N'IT Room',N'QR-TEST-001',N'Test laptop'),
(N'IT-TEST-002',N'Laptop Lenovo ThinkPad',N'i5/16GB/512GB',N'SN-LAP-002',N'AST-0002',28000000,'2026-01-10','2029-01-10',N'IT',N'IT Room',N'QR-TEST-002',N'Test laptop'),
(N'PROD-TEST-001',N'Barcode Scanner',N'Industrial scanner',N'SN-SCAN-001',N'AST-0003',12000000,'2026-02-15','2029-02-15',N'PROD',N'Line 1',N'QR-TEST-003',N'Production scanner'),
(N'QA-TEST-001',N'Quality Monitor',N'24 inch monitor',N'SN-MON-001',N'AST-0004',6000000,'2026-03-01','2029-03-01',N'QA',N'QA Room',N'QR-TEST-004',N'QA monitor'),
(N'IT-TEST-003',N'Network Switch',N'24-port managed switch',N'SN-SW-001',N'AST-0005',15000000,'2026-03-05','2029-03-05',N'IT',N'Server Room',N'QR-TEST-005',N'Network switch')
) AS v(Code,Name,Spec,Serial,AssetCode,Price,PurchaseDate,DepDate,Dept,Location,Qr,Note)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EquipmentAssets a WHERE a.EquipmentCode=v.Code);

INSERT dbo.F03EmailProfiles(IsActive,CreatedBy,ParentId,IsGroup,Code,Name,NameEn,EmailServerName,EmailServerType,EmailServerPort,EmailServerEnableSsl,EmailAccountName,EmailAddress,SiteUrl)
SELECT 1,0,0,0,N'ITSYS',N'IT System',N'IT System',N'smtp.gmail.com',N'SMTP',587,1,N'test@test.local',N'test@test.local',N'https://localhost'
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EmailProfiles WHERE Code=N'ITSYS');

-- 5 test email queue rows
INSERT dbo.F03EmailQueues(IsActive,CreatedBy,ToEmail,Subject,Body,TemplateCode,Status)
SELECT 1,0,v.Email,v.Subject,v.Body,N'TEST',N'Pending'
FROM (VALUES
(N'e0001@test.local',N'Test email 1',N'Test email queue item 1'),
(N'e0002@test.local',N'Test email 2',N'Test email queue item 2'),
(N'e0003@test.local',N'Test email 3',N'Test email queue item 3'),
(N'e0004@test.local',N'Test email 4',N'Test email queue item 4'),
(N'e0005@test.local',N'Test email 5',N'Test email queue item 5')
) AS v(Email,Subject,Body)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EmailQueues q WHERE q.Subject=v.Subject AND q.ToEmail=v.Email);

-- Business rules / escalation
INSERT dbo.F03BusinessRules(IsActive,CreatedBy,Module,Code,Name,ConfigJson,HrmCode)
SELECT 1,0,v.Module,v.Code,v.Name,v.Json,NULL
FROM (VALUES
(N'Leave',N'LEAVE_WORKDAY',N'Leave working-day rule',N'{"excludeSunday":true}'),
(N'Leave',N'LEAVE_HALF_DAY',N'Leave half-day rule',N'{"morning":0.5,"afternoon":0.5}'),
(N'OT',N'OT_DAILY_LIMIT',N'Daily OT limit',N'{"hours":4}'),
(N'Trip',N'TRIP_APPROVAL',N'Trip approval',N'{"levels":3}'),
(N'Equipment',N'EQUIPMENT_APPROVAL',N'Equipment approval',N'{"levels":2}')
) AS v(Module,Code,Name,Json)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03BusinessRules b WHERE b.Module=v.Module AND b.Code=v.Code);

INSERT dbo.F03EscalationRules(IsActive,CreatedBy,RequestModule,Level,DeptCode,WarningHours,EscalateHours,DeadlineHour)
SELECT 1,0,v.Module,v.Level,v.Dept,v.Warning,v.Escalate,48
FROM (VALUES
(N'Leave',1,N'IT',24,48),(N'Leave',2,N'IT',24,48),(N'Leave',3,N'IT',24,48),
(N'OT',1,N'IT',24,48),(N'OT',2,N'IT',24,48)
) AS v(Module,Level,Dept,Warning,Escalate)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03EscalationRules r WHERE r.RequestModule=v.Module AND r.Level=v.Level AND ISNULL(r.DeptCode,N'')=v.Dept);

COMMIT;
GO
PRINT N'FVN_REGISTER test seed completed.';
PRINT N'Test users: E0001..E0005 / password: Test@123 (TEST ONLY).';
GO
