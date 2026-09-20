CREATE OR ALTER PROCEDURE dbo.usp_GetHrmAttendanceCalculation
 @FromDate date,@ToDate date,@DeptCode nvarchar(20)=NULL,@EmployeeCode nvarchar(50)=NULL
AS
BEGIN
 SET NOCOUNT ON;
 SELECT * FROM dbo.VF03HrmAttendanceDaily WHERE WorkDate BETWEEN @FromDate AND @ToDate
  AND (@DeptCode IS NULL OR DeptCode=@DeptCode) AND (@EmployeeCode IS NULL OR EmployeeCode=@EmployeeCode)
 ORDER BY DeptCode,EmployeeCode,WorkDate;
END;
GO