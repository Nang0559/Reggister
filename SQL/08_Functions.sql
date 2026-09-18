USE [FVN_REGISTER];
GO
CREATE OR ALTER FUNCTION dbo.fn_WorkingDays(@StartDate date,@EndDate date)
RETURNS decimal(10,2)
AS
BEGIN
 DECLARE @d date=@StartDate,@n decimal(10,2)=0;
 IF @EndDate<@StartDate RETURN 0;
 WHILE @d<=@EndDate
 BEGIN
  IF DATEPART(WEEKDAY,@d) NOT IN(1,7) AND NOT EXISTS(SELECT 1 FROM dbo.F03CompanyHolidays h WHERE h.HolidayDate=@d AND h.IsActive=1) SET @n+=1;
  SET @d=DATEADD(day,1,@d);
 END
 RETURN @n;
END;
GO
