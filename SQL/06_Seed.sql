USE [FVN_REGISTER];
GO
MERGE auth.Role AS T USING (VALUES
(N'EMPLOYEE',N'Employee'),(N'APPROVER',N'Approver'),(N'HR',N'HR'),(N'MANAGER',N'Manager'),
(N'FINANCE',N'Finance'),(N'AUDITOR',N'Auditor'),(N'SYSTEM',N'System'),(N'ADMIN',N'Administrator')
) AS S(Code,Name) ON T.Code=S.Code
WHEN NOT MATCHED THEN INSERT(Code,Name) VALUES(S.Code,S.Name);

MERGE config.ApprovalLevel AS T USING (VALUES
(1,N'L1',N'Level 1'),(2,N'L2',N'Level 2'),(3,N'L3',N'Level 3'),(4,N'L4',N'Level 4')
) AS S(LevelNo,Code,Name) ON T.LevelNo=S.LevelNo
WHEN NOT MATCHED THEN INSERT(LevelNo,Code,Name) VALUES(S.LevelNo,S.Code,S.Name);

MERGE config.LeaveType AS T USING (VALUES
(N'ANNUAL',N'Annual Leave',1,1),(N'UNPAID',N'Unpaid Leave',0,0)
) AS S(Code,Name,IsCountedAsLeave,IsPaid) ON T.Code=S.Code
WHEN NOT MATCHED THEN INSERT(Code,Name,IsCountedAsLeave,IsPaid) VALUES(S.Code,S.Name,S.IsCountedAsLeave,S.IsPaid);
GO
