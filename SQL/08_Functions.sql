USE [FVN_REGISTER];
GO
CREATE OR ALTER FUNCTION config.fn_WorkingDays(@FromDate date,@ToDate date)
RETURNS int
AS
BEGIN
    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate>@ToDate RETURN 0;
    DECLARE @n int=0,@d date=@FromDate;
    WHILE @d<=@ToDate
    BEGIN
        IF DATEPART(WEEKDAY,@d) NOT IN (1) AND NOT EXISTS(SELECT 1 FROM config.CompanyHoliday h WHERE h.HolidayDate=@d AND h.IsActive=1) SET @n+=1;
        SET @d=DATEADD(DAY,1,@d);
    END
    RETURN @n;
END
GO
