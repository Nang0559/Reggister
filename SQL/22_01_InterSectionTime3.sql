CREATE OR ALTER FUNCTION dbo.InterSectionTime3
(  
 @T11  Datetime, 
 @T12 Datetime, 
 @T21 Datetime, 
 @T22 Datetime,
 @T31 Datetime,
 @T32 Datetime
)
RETURNS int
AS  
BEGIN 

	IF @T11 IS NULL OR @T12 IS NULL OR @T21 IS NULL OR @T22 IS NULL OR @T31 IS NULL OR @T32 IS NULL
	BEGIN
		RETURN 0
	END
--SET @T11=CONVERT(VARCHAR(8),@T11,108)
--SET @T12=CONVERT(VARCHAR(8),@T12,108)
--SET @T21=CONVERT(VARCHAR(8),@T21,108)
--SET @T22=CONVERT(VARCHAR(8),@T22,108)
--SET @T31=CONVERT(VARCHAR(8),@T31,108)
--SET @T32=CONVERT(VARCHAR(8),@T32,108)

	--DECLARE @T11 Datetime
	--DECLARE @T12 Datetime

	--DECLARE @T21 Datetime
	--DECLARE @T22 Datetime

	--DECLARE @T31 Datetime
	--DECLARE @T32 Datetime

	DECLARE @T1 Datetime
	DECLARE @T2 Datetime
	Declare	@iTemp int
	
	
	--SET @T11='2009-01-01 10:00:00'
	--SET @T12='2009-01-01 13:00:00'
	--SET @T21='2009-01-01 11:00:00'
	--SET @T22='2009-01-01 13:00:00'
	--SET @T31='2009-01-01 11:00:00'
	--SET @T32='2009-01-01 12:00:00'
	
	-- Khoi tao @T1
	IF (@T11<@T21)  
		begin
			IF (@T21<@T31)  	SET @T1=@T31 ELSE SET @T1=@T21
		end
	ELSE
		BEGIN
			IF (@T11<@T31)  	SET @T1=@T31 ELSE SET @T1=@T11
		END	
		
	-- Khoi tao @T2
	IF (@T12>@T22)  
		begin
			IF (@T22>@T32)  	SET @T2=@T32 ELSE SET @T2=@T22
		end
	ELSE
		BEGIN
			IF (@T12>@T32)  	SET @T2=@T32 ELSE SET @T2=@T12
		END	
		
	SET @iTemp = CASE when @T1 > @T2 THEN 0 ELSE DateDiff(minute, @T1, @T2) end		
	

	RETURN @iTemp
END






GO
