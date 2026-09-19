USE [HRM]
GO
/****** Object:  UserDefinedFunction [dbo].[AddValToStr]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/************************************************************
*  Routine       :	dbo.AddValToStr
*  Created by    :  Duong Van Quyet , at 20/01/2011 - 14:53:17  
*  Machine       :  DIGISOFT
*  Description   :  Them gia tri tu tang
*  Parameters    :  @str nvarchar(20), @iStep int
************************************************************/
CREATE FUNCTION [dbo].[AddValToStr]
(
	@str    NVARCHAR(20),
	@iStep  INT
)
RETURNS NVARCHAR(20)
AS
BEGIN
	DECLARE @sThanhPhanSo   NVARCHAR(20) 
	DECLARE @sThanhPhanChu  NVARCHAR(20) 
	DECLARE @sReturn        NVARCHAR(20) 
	DECLARE @tg             INT
	DECLARE @ii             INT
	DECLARE @nSoKhong       INT
	
	SET @sThanhPhanSo = dbo.ReturnNumeric(@str, 0);
	
	IF (@sThanhPhanSo = '0')
	BEGIN
	    IF (SUBSTRING(@sThanhPhanSo, LEN(@sThanhPhanSo) -1, 1) = '0')
	    BEGIN
	        SET @sThanhPhanChu = SUBSTRING(@str, 0, LEN(@str) -LEN(@sThanhPhanSo) +1)
	    END
	    ELSE
	    BEGIN
	        SET @sThanhPhanChu = @str
	    END
	END
	ELSE
	    SET @sThanhPhanChu = SUBSTRING(@str, 0, LEN(@str) -LEN(@sThanhPhanSo) +1)
	
	
	SET @tg = CAST(@sThanhPhanSo AS INT) + @iStep;
	SET @nSoKhong = LEN(@sThanhPhanSo) - LEN(@tg)
	SET @sReturn = @sThanhPhanChu
	
	-- Them so khong
	SET @ii = 1
	WHILE (@ii <= @nSoKhong)
	BEGIN
	    SET @sReturn = @sReturn + '0'
	    SET @ii = @ii + 1
	END
	
	SET @sReturn = @sReturn + CAST(@tg AS NVARCHAR) 
	RETURN @sReturn;
END
GO
/****** Object:  UserDefinedFunction [dbo].[DonVi_IDs]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/************************************************************
*  Routine       :	dbo.DonVi_IDs
*  Created by    :  Duong Van Quyet , at 22/01/2011 - 09:35:36  
*  Machine       :  DIGISOFT
*  Description   :  Lay DS nhan vien
*  Parameters    :  @mDonVi_ID int
************************************************************/


CREATE FUNCTION [dbo].[DonVi_IDs]
(
	@mDonVi_ID INT
)
RETURNS NVARCHAR(4000)
AS
  
BEGIN
	DECLARE @mDonVi_IDs     NVARCHAR(4000),
	        @mDonVi_ID_Tmp  INT
	
	IF (@mDonVi_ID < 0)
	    RETURN ''
	
	SET @mDonVi_IDs = CAST(@mDonVi_ID AS VARCHAR) 
	
	DECLARE DonVi_Cursor CURSOR  
	FOR
	    SELECT BPMa
	    FROM   tblBoPhan
	    WHERE  BPMaCha = @mDonVi_ID
	
	OPEN DonVi_Cursor
	FETCH NEXT FROM DonVi_Cursor INTO @mDonVi_ID_Tmp
	WHILE @@FETCH_STATUS = 0
	BEGIN
	    SET @mDonVi_IDs = @mDonVi_IDs + ',' + dbo.DonVi_IDs(@mDonVi_ID_Tmp) 
	    FETCH NEXT FROM DonVi_Cursor INTO @mDonVi_ID_Tmp
	END
	CLOSE DonVi_Cursor
	DEALLOCATE DonVi_Cursor
	RETURN @mDonVi_IDs
END



GO
/****** Object:  UserDefinedFunction [dbo].[F_DemSoNgayCN]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[F_DemSoNgayCN]
(
	@FromDate  AS DATETIME,
	@ToDate    AS DATETIME
)
RETURNS TINYINT
AS
BEGIN

	DECLARE @TempDate  AS DATETIME
	DECLARE @SoNgay    AS TINYINT
	
	SET @TempDate = @FromDate
	SET @SoNgay = 0
	
	WHILE @TempDate <= @ToDate
	BEGIN
	    IF DATEPART(dw, @TempDate) = 1
	    BEGIN
	        -- Kiem tra ngay nay khong trung trong bang tblNgaynghile thi moi dem ngay chu nhat
	        IF NOT EXISTS (
	               SELECT *
	               FROM   tblNgayNghiLe tnnl
	               WHERE  tnnl.NNLNgay = @TempDate
	           )
	        BEGIN
	            SET @SoNgay = @SoNgay + 1
	        END
	    END
	    
	    SET @TempDate = DATEADD(DAY, 1, @TempDate)
	END
	--PRINT @SoNgay
	RETURN @SoNgay
END 
GO
/****** Object:  UserDefinedFunction [dbo].[FindShiftOfStaff]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[FindShiftOfStaff]
(@staffID	Int,@D datetime)  
RETURNS int
AS  
BEGIN 
	DECLARE @Maca int
	
	DECLARE @ChuKy INT
	
	DECLARE @SoNgay1 INT
	DECLARE @SoNgay2 INT  
	DECLARE @SoNgay3 INT
	DECLARE @SoNgay4 INT  
		
	
	DECLARE @MaCa1 VARCHAR(5)
	DECLARE @MaCa2 VARCHAR(5)  
	DECLARE @MaCa3 VARCHAR(5)
	DECLARE @MaCa4 VARCHAR(5)  
	
	DECLARE @NgayApDung DATETIME

	DECLARE @TongNgay INT
					
	-- Kiem tra nhom
	--IF  EXISTS (SELECT NMaCa1,NSoNgay1,NMaCa2,NSoNgay2,NMaCa3,NSoNgay3,NMaCa4,NSoNgay4,NNgayApDung FROM tblNhomNhanVien INNER JOIN tblNhom ON tblNhomNhanVien.NNVMaNhom=tblNhom.NMa WHERE NNVMaNV=@staffID AND @D BETWEEN  NNVNgayApDung AND NNVNgayKetThuc)
	--BEGIN
	DECLARE curRS 
		CURSOR FOR 
			SELECT NMaCa1,NSoNgay1,NMaCa2,NSoNgay2,NMaCa3,NSoNgay3,NMaCa4,NSoNgay4,NNgayApDung  FROM tblNhomNhanVien INNER JOIN tblNhom ON tblNhomNhanVien.NNVMaNhom=tblNhom.NMa WHERE NNVMaNV=@staffID AND @D BETWEEN  NNVNgayApDung AND NNVNgayKetThuc
		OPEN curRS

		FETCH NEXT FROM curRS INTO @MaCa1,@SoNgay1,@MaCa2,@SoNgay2,@MaCa3,@SoNgay3,@MaCa4,@SoNgay4,@NgayApDung
		if @@FETCH_STATUS = 0
			BEGIN
				
				SET @ChuKy=ISNULL(@SoNgay1,0) + ISNULL(@SoNgay2,0)+ ISNULL(@SoNgay3,0) + ISNULL(@SoNgay4,0)
				
				IF(@ChuKy>0)
				BEGIN
					--TongNgay = DateDiff("d", Rs!NNgayApDung, D)
					
					SET @TongNgay=DATEDIFF(DAY,@NgayApDung,@D)
					SET @TongNgay = {fn mod(@TongNgay,@ChuKy)}
					
					 --If TongNgay < 0 Then TongNgay = TongNgay + ChuKy
					IF (@TongNgay<0) 
					BEGIN
						SET @TongNgay=@TongNgay +@ChuKy
					END
					
					--            MaCa = IIf(TongNgay < Rs!NSoNgay1, Rs!NMaCa1, IIf(TongNgay < Rs!NSoNgay1 + Rs!NSoNgay2, Rs!NMaCa2, IIf(TongNgay < Rs!NSoNgay1 + Rs!NSoNgay2 + Rs!NSoNgay3, Rs!NMaCa3, Rs!NMaCa4)))
					IF(@TongNgay<@SoNgay1)
					BEGIN
						SET @Maca=@MaCa1
						
					END
					ELSE IF(@TongNgay<(@SoNgay1 +@SoNgay2 ))
					BEGIN
						SET @Maca=@MaCa2
					END
					ELSE IF(@TongNgay<(@SoNgay1 +@SoNgay2 +@SoNgay3))
					BEGIN
						SET @Maca=@MaCa3
					END						
					ELSE 
					BEGIN
						SET @Maca=@MaCa4
					END						
						
				END
					Set @Maca=ISNULL(@Maca,0)
					RETURN @Maca
			END
	--END
	
	--SELECT @Maca=NVMaCa FROM tblNhanVien WHERE NVMa=@staffID
	SET @Maca= -1 -- CASE WHEN @Maca IS NULL THEN 0 ELSE @Maca end
	
	RETURN @Maca
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_CheckDKN]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[fn_CheckDKN]
(
	@MaNV INT,
	@D1 SMALLDATETIME,
	@D2 SMALLDATETIME,
	@LDNMa TINYINT,
	@NPConlai FLOAT,
	@Loainghi BIT  --0: Ca ca  1: Nua ca
)
RETURNS BIT
AS
BEGIN
	DECLARE @HDTuNgay SMALLDATETIME
	DECLARE @HDDenNgay SMALLDATETIME
	DECLARE @Ret BIT
	DECLARE @SongayDuocDKN FLOAT
	
	SET @Ret=0
	
	-- Neu khong la nghi phep
	IF (NOT EXISTS (SELECT tldn.LDNMa FROM tblLyDoNghi tldn INNER JOIN tblLoaiNghi tln ON tldn.LDNLoai=tln.LNMa WHERE (tldn.LDNMa=@LDNMa) AND (tln.LNLoai=1)))
	BEGIN
		SET @Ret=0
		GOTO EndTHisSUb
	END
	
	IF (NOT EXISTS (SELECT HDMa  FROM   tblHopDongNV WHERE  (HDMaNV = @MaNv) AND (HDLoaiHD = 3)))
	BEGIN
			SET @Ret=1
		GOTO EndThisSub
	END
	
		SELECT TOP 1 @HDTuNgay = thdn.HDTuNgay,@HDDenNgay = thdn.HDDenNgay FROM tblHopDongNV thdn WHERE (thdn.HDMaNV = @MaNv) AND (thdn.HDLoaiHD = 3) ORDER BY thdn.HDDenNgay DESC
		
		-- Kiem tra ngay dang ky nghi voi ngay ky hop dong
		IF (NOT ((@D2>@HDTuNgay) AND (@D1<@HDDenNgay)))
		BEGIN
			SET @Ret=1
			GOTO EndThisSub
		END
		
		-- Lay so ngay dang ky nghi
		IF (@Loainghi=0)
			set @SongayDuocDKN = (DATEDIFF(DAY,@D1,@D2)+1)
		ELSE
			set @SongayDuocDKN = (DATEDIFF(DAY,@D1,@D2)+1) * 0.5
		
		IF (@NPConlai-@SongayDuocDKN<0)
		BEGIN
			set @Ret=1
		END
	EndThisSub:
	
	RETURN @Ret
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_FindShiftNew]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_FindShiftNew](
@Manv int,
@vD AS DATETIME,
@vD1 AS DATETIME,
@vD2 AS DATETIME,
@vD3 AS DATETIME,
@vD4 AS DATETIME,
@DSCa AS NVARCHAR(500),
@DKNLoai AS NVARCHAR(10)
)
RETURNS INT
BEGIN


DECLARE @SQL NVARCHAR(MAX)
DECLARE @NumberRecords INT
DECLARE @RowCount INT


DECLARE @CMa AS int





-- Chi co 1 du lieu

IF((@vD1 <> '1900-01-01 00:00:00.000' AND @vD2 = '1900-01-01 00:00:00.000' AND @vD3 = '1900-01-01 00:00:00.000' AND @vD4 = '1900-01-01 00:00:00.000')
 OR(@vD1 = '1900-01-01 00:00:00.000' AND @vD2 <> '1900-01-01 00:00:00.000' AND @vD3 = '1900-01-01 00:00:00.000' AND @vD4 = '1900-01-01 00:00:00.000')
 OR(@vD1 = '1900-01-01 00:00:00.000' AND @vD2 = '1900-01-01 00:00:00.000' AND @vD3 <> '1900-01-01 00:00:00.000' AND @vD4 = '1900-01-01 00:00:00.000')
 OR(@vD1 = '1900-01-01 00:00:00.000' AND @vD2 = '1900-01-01 00:00:00.000' AND @vD3 = '1900-01-01 00:00:00.000' AND @vD4 <> '1900-01-01 00:00:00.000')
)
BEGIN
	SELECT TOP 1 
@CMa = tblca.CMa
FROM tblca
LEFT JOIN 
(
    SELECT tblCa.CMa,
           tblCa.CTen,
           tblCa.CVietTat,
		   CASE WHEN ISNULL(@dknloai,'') = '0002'
		   then DATEADD(second,59,(@vD + tblCa.CTGBatDau+ tblca.cTGBDNghi2))
		   else
           DATEADD(second,59,DATEADD(minute,tblca.cnguongdimuon,(@vD + tblCa.CTGBatDau))) END  TGBDCaNew,
		   CASE WHEN ISNULL(@dknloai,'') = '0003' 
		   THEN (@vD + tblCa.CTGBatDau + tblca.cTGBDNghi2)
		   ELSE 
           DATEADD(minute,-tblca.cnguongvesom,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc)) end TGKTCaNew,
           DATEADD(minute,-tblca.CQuetTruocCa,(@vD + tblCa.CTGBatDau)) AS NguongBDCaNew,
           DATEADD(second,59,DATEADD(minute,tblca.cquetsauca,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc))) AS NguongKTCa,
           ABS(DATEDIFF(minute,@vD1,(@vD + tblCa.CTGBatDau))) AS SoPhut
    FROM   tblCa
    WHERE  (1 = 1)
           AND CHARINDEX(',' + CVietTat + ',',@DSCa) > 0
) Moc
ON tblca.CMa = Moc.CMa
WHERE (@vD1 BETWEEN Moc.NguongBDCaNew AND Moc.TGBDCaNew
		OR CASE WHEN @vD4 = '1900-01-01 00:00:00.000' THEN @vD2 ELSE @vD4 END
			BETWEEN Moc.TGKTCaNew AND Moc.NguongKTCa)


ORDER BY Moc.SoPhut

	
END
--- Co 2 du lieu tro len
ELSE 
BEGIN
	SELECT TOP 1 
@CMa = tblca.CMa
FROM tblca
LEFT JOIN 
(
    SELECT tblCa.CMa,
           tblCa.CTen,
           tblCa.CVietTat,
		   CASE WHEN ISNULL(@dknloai,'') = '0002'
		   then DATEADD(second,59,(@vD + tblCa.CTGBatDau+ tblca.cTGBDNghi2))
		   else
           DATEADD(second,59,DATEADD(minute,tblca.cnguongdimuon,(@vD + tblCa.CTGBatDau))) END  TGBDCaNew,
		   CASE WHEN ISNULL(@dknloai,'') = '0003' 
		   THEN (@vD + tblCa.CTGBatDau + tblca.cTGBDNghi2)
		   ELSE 
           DATEADD(minute,-tblca.cnguongvesom,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc)) end TGKTCaNew,
           DATEADD(minute,-tblca.CQuetTruocCa,(@vD + tblCa.CTGBatDau)) AS NguongBDCaNew,
           DATEADD(second,59,DATEADD(minute,tblca.cquetsauca,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc))) AS NguongKTCa,
           ABS(DATEDIFF(minute,@vD1,(@vD + tblCa.CTGBatDau))) AS SoPhut
    FROM   tblCa
    WHERE  (1 = 1)
           AND CHARINDEX(',' + CVietTat + ',',@DSCa) > 0
) Moc
ON tblca.CMa = Moc.CMa
WHERE (@vD1 BETWEEN Moc.NguongBDCaNew AND Moc.TGBDCaNew
		AND CASE WHEN @vD4 = '1900-01-01 00:00:00.000' THEN @vD2 ELSE @vD4 END
			BETWEEN Moc.TGKTCaNew AND Moc.NguongKTCa)


ORDER BY Moc.SoPhut


-- Neu khong tim thay ca do di muon,ve som thi lay theo TGDen

IF (ISNULL(@CMa,0) = 0)
begin
 SELECT TOP 1 
@CMa = tblca.CMa
FROM tblca
LEFT JOIN 
(
    SELECT tblCa.CMa,
           tblCa.CTen,
           tblCa.CVietTat,
		   CASE WHEN ISNULL(@dknloai,'') = '0002'
		   then DATEADD(second,59,(@vD + tblCa.CTGBatDau+ tblca.cTGBDNghi2))
		   else
           DATEADD(second,59,DATEADD(minute,tblca.cnguongdimuon,(@vD + tblCa.CTGBatDau))) END  TGBDCaNew,
		   CASE WHEN ISNULL(@dknloai,'') = '0003' 
		   THEN (@vD + tblCa.CTGBatDau + tblca.cTGBDNghi2)
		   ELSE 
           DATEADD(minute,-tblca.cnguongvesom,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc)) end TGKTCaNew,
           DATEADD(minute,-tblca.CQuetTruocCa,(@vD + tblCa.CTGBatDau)) AS NguongBDCaNew,
           DATEADD(second,59,DATEADD(minute,tblca.cquetsauca,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc))) AS NguongKTCa,
           ABS(DATEDIFF(minute,@vD1,(@vD + tblCa.CTGBatDau))) AS SoPhut
    FROM   tblCa
    WHERE  (1 = 1)
           AND CHARINDEX(',' + CVietTat + ',',@DSCa) > 0
) Moc
ON tblca.CMa = Moc.CMa
WHERE (@vD1 BETWEEN Moc.NguongBDCaNew AND Moc.TGBDCaNew)


ORDER BY Moc.SoPhut
END

-- neu di muon thi lay gio ve so sanh

IF (ISNULL(@CMa,0) = 0)
begin
 SELECT TOP 1 
@CMa = tblca.CMa
FROM tblca
LEFT JOIN 
(
    SELECT tblCa.CMa,
           tblCa.CTen,
           tblCa.CVietTat,
		   CASE WHEN ISNULL(@dknloai,'') = '0002'
		   then DATEADD(second,59,(@vD + tblCa.CTGBatDau+ tblca.cTGBDNghi2))
		   else
           DATEADD(second,59,DATEADD(minute,tblca.cnguongdimuon,(@vD + tblCa.CTGBatDau))) END  TGBDCaNew,
		   CASE WHEN ISNULL(@dknloai,'') = '0003' 
		   THEN (@vD + tblCa.CTGBatDau + tblca.cTGBDNghi2)
		   ELSE 
           DATEADD(minute,-tblca.cnguongvesom,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc)) end TGKTCaNew,
           DATEADD(minute,-tblca.CQuetTruocCa,(@vD + tblCa.CTGBatDau)) AS NguongBDCaNew,
           DATEADD(second,59,DATEADD(minute,tblca.cquetsauca,(@vD + tblCa.CTGBatDau + tblca.cTGketthuc))) AS NguongKTCa,
           ABS(DATEDIFF(minute,@vD1,(@vD + tblCa.CTGBatDau))) AS SoPhut
    FROM   tblCa
    WHERE  (1 = 1)
           AND CHARINDEX(',' + CVietTat + ',',@DSCa) > 0
) Moc
ON tblca.CMa = Moc.CMa
WHERE (CASE WHEN @vD4 = '1900-01-01 00:00:00.000' THEN @vD2 ELSE @vD4 END
			BETWEEN Moc.TGKTCaNew AND Moc.NguongKTCa)


ORDER BY Moc.SoPhut
end
	
END

RETURN @CMa 

END



GO
/****** Object:  UserDefinedFunction [dbo].[fn_Max]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_Max](
@D1 DATETIME,
@D2 DATETIME
)
RETURNS DATETIME
BEGIN
	DECLARE @Ra DATETIME
	IF @D1 >= @D2 
		Set @Ra = @D1
	ELSE
		SET @Ra = @D2
	RETURN @Ra
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_Min]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fn_Min](
@D1 DATETIME,
@D2 DATETIME
)
RETURNS DATETIME
BEGIN
	DECLARE @Ra DATETIME
	IF @D1 >= @D2 
		Set @Ra = @D2
	ELSE
		SET @Ra = @D1
	RETURN @Ra
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_SumDangKyNghi]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[fn_SumDangKyNghi]
(
	@MaNV INT,
	@TuNgay SMALLDATETIME,
	@DenNgay SMALLDATETIME,
	@Kieu int
)
RETURNS FLOAT
AS
BEGIN
	
	DECLARE @bien FLOAT
	IF @Kieu = 0 
	BEGIN
		SET @bien = DATEDIFF(DAY, @TuNgay, @DenNgay) 
	END
	IF @Kieu = 1 
	BEGIN
		SET @bien = DATEDIFF(DAY, @TuNgay, @DenNgay) * 0.5
	END

	IF @bien < 0 SET @bien = 0
	
	RETURN @bien

END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_TGThemN]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Batch submitted through debugger: SQLQuery113.sql|0|0|C:\Users\Admin\AppData\Local\Temp\~vs81ED.sql

CREATE FUNCTION [dbo].[fn_TGThemN]
(
	@TinhLamThem         INT,
	@CChiaLTSauca        INT,
	@LoaiCa              INT,
	@NguongLamThem       INT,
	@NguongLamThemTC     INT,
	@DonViLamThem       INT,
	@TGQuaN              INT,
	@TGQuaD              INT,
	@TGQuaNTC            INT,
	@TGQuaDTC            INT,
	@LaOTNgay int
)
RETURNS INT
AS


BEGIN
	DECLARE @iReturn INT 
	DECLARE @TGThemN INT
	DECLARE @TGThemD INT
	DECLARE @TGThemNTC INT
	DECLARE @TGThemDTC INT
	
	
	SET @iReturn = 0
	IF (@TinhLamThem = 1)
	BEGIN
		
		-- Gan thoi gian qua ngay, qua dem vao thoi gian lam
	    SET @TGThemN = @TGQuaN
	    SET @TGThemD = @TGQuaD
	    
	    SET @TGThemNTC = @TGQuaNTC
	    SET @TGThemDTC = @TGQuaDTC
	    
	    -- Neu @CChiaLTSauca=1 => Khong chia 
	    SET @CChiaLTSauca = ISNULL(@CChiaLTSauca, 0)
	    
	    -- Neu chia thoi gian lam them thi chia luon
	    IF (@CChiaLTSauca = 1)
	    BEGIN
	        IF (@LoaiCa = 3)
	        BEGIN
	            SET @TGThemD = @TGThemD + @TGThemN
	            SET @TGThemN = 0
	        END
	        ELSE
	        BEGIN
	            SET @TGThemN = @TGThemD + @TGThemN
	            SET @TGThemD = 0
	        END
	    END
	    
	    -- Kiem tra voi nguong lam them, neu thoi gian lam them nho hon nguong lam them thi khong tinh
	    IF (@TGThemN + @TGThemD < @NguongLamThem)
	    BEGIN
	        SET @TGThemN = 0
	        SET @TGThemD = 0
	    END
	    
	    IF (
	           (@TGThemNTC + @TGThemDTC) < @NguongLamThemTC
	           OR @NguongLamThemTC = 0
	       )
	    BEGIN
	        SET @TGThemNTC = 0
	        SET @TGThemDTC = 0
	    END
	    
	    
	    SET @TGThemN = @TGThemN + @TGThemNTC
	    SET @TGThemD = @TGThemD + @TGThemDTC
	    
	    -- Lam trong theo don vi lam them
	    SET @TGThemN = ROUND(@TGThemN / @DonViLamThem, 0) * @DonViLamThem
	    SET @TGThemD = ROUND(@TGThemD / @DonViLamThem, 0) * @DonViLamThem
	    
	    IF (@TGThemN + @TGThemD < @NguongLamThem)
	    BEGIN
	        SET @TGThemN = 0
	        SET @TGThemD = 0
	    END
	    
	    IF(@LaOTNgay=1)
	    BEGIN
	    	SET @iReturn=@TGThemN
	    END
	    ELSE
	    	BEGIN
	    		SET @iReturn=@TGThemD
	    	END
	    
	END
	ELSE
	BEGIN
	    SET @iReturn = 0
	END
	
	RETURN @iReturn
END





GO
/****** Object:  UserDefinedFunction [dbo].[fn_TinhTGLamThem]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create FUNCTION [dbo].[fn_TinhTGLamThem]
(
	@TinhLamThem         INT,
	@CChiaLTSauca        INT,
	@LoaiCa              INT,
	@NguongLamThem       INT,
	@NguongLamThemTC     INT,
	@DonViLamThem       INT,
	@TGQuaN              INT,
	@TGQuaD              INT,
	@TGQuaNTC            INT,
	@TGQuaDTC            INT,
	@LaOTNgay int
)
RETURNS INT
AS


BEGIN
	DECLARE @iReturn INT 
	DECLARE @TGThemN INT
	DECLARE @TGThemD INT
	DECLARE @TGThemNTC INT
	DECLARE @TGThemDTC INT
	
	
	SET @iReturn = 0
	IF (@TinhLamThem = 1)
	BEGIN
		
		-- Gan thoi gian qua ngay, qua dem vao thoi gian lam
	    SET @TGThemN = @TGQuaN
	    SET @TGThemD = @TGQuaD
	    
	    SET @TGThemNTC = @TGQuaNTC
	    SET @TGThemDTC = @TGQuaDTC
	    
	    -- Neu @CChiaLTSauca=1 => Khong chia 
	    SET @CChiaLTSauca = ISNULL(@CChiaLTSauca, 0)
	    
	    -- Neu chia thoi gian lam them thi chia luon
	    IF (@CChiaLTSauca = 1)
	    BEGIN
	        IF (@LoaiCa = 3)
	        BEGIN
	            SET @TGThemD = @TGThemD + @TGThemN
	            SET @TGThemN = 0
	        END
	        ELSE
	        BEGIN
	            SET @TGThemN = @TGThemD + @TGThemN
	            SET @TGThemD = 0
	        END
	    END
	    
	    -- Kiem tra voi nguong lam them, neu thoi gian lam them nho hon nguong lam them thi khong tinh
	    IF (@TGThemN + @TGThemD < @NguongLamThem)
	    BEGIN
	        SET @TGThemN = 0
	        SET @TGThemD = 0
	    END
	    
	    IF (
	           (@TGThemNTC + @TGThemDTC) < @NguongLamThemTC
	           OR @NguongLamThemTC = 0
	       )
	    BEGIN
	        SET @TGThemNTC = 0
	        SET @TGThemDTC = 0
	    END
	    
	    
	    SET @TGThemN = @TGThemN + @TGThemNTC
	    SET @TGThemD = @TGThemD + @TGThemDTC
	    
	    -- Lam trong theo don vi lam them
	    SET @TGThemN = ROUND(@TGThemN / @DonViLamThem, 0) * @DonViLamThem
	    SET @TGThemD = ROUND(@TGThemD / @DonViLamThem, 0) * @DonViLamThem
	    
	    IF (@TGThemN + @TGThemD < @NguongLamThem)
	    BEGIN
	        SET @TGThemN = 0
	        SET @TGThemD = 0
	    END
	    
	    IF(@LaOTNgay=1)
	    BEGIN
	    	SET @iReturn=@TGThemN
	    END
	    ELSE
	    	BEGIN
	    		SET @iReturn=@TGThemD
	    	END
	    
	END
	ELSE
	BEGIN
	    SET @iReturn = 0
	END
	
	RETURN @iReturn
END





GO
/****** Object:  UserDefinedFunction [dbo].[fn_TongNgayNP]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fn_TongNgayNP]
(
	@Manv INT
)
RETURNS TINYINT
AS
BEGIN
	DECLARE @NgayVao SMALLDATETIME
	DECLARE @TongNP TINYINT
	
	-- Lay ngay vao cua nhan vien
	SELECT @NgayVao=NVNgayVao FROM tblNhanVien tnv WHERE tnv.NVMa=@Manv
	IF (@NgayVao IS NULL) RETURN 0
	-- Neu vao truoc sau ngay 20 thi bi tri di mot thang
	IF DAY(@NgayVao)>=20 
	BEGIN
		SET @NgayVao=DATEADD(MONTH,-1,@NgayVao)
	END

	-- Neu so thang lam viec <60 thi duoc 12 ngay phep
	IF (DATEDIFF(MONTH,@NgayVao,GETDATE())<60) 
		set @TongNP=12
	ELSE
		set @TongNP=13
	RETURN @TongNP
END
GO
/****** Object:  UserDefinedFunction [dbo].[fnReplaceField]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[fnReplaceField]
(
	@strSelectLluong VARCHAR(4000)
)
RETURNS VARCHAR(8000)
AS

  
BEGIN
    DECLARE @strTentruong   VARCHAR(50)
    DECLARE @strCaulenhSQL  VARCHAR(4000)
    DECLARE @strCheck       VARCHAR(8000)
    
    SET @strCheck = ''
    
    DECLARE curRS          CURSOR  
    FOR
        SELECT Tentruong
              ,CaulenhSQL  
        FROM   CongthucLuong cl
        WHERE  cl.LoaiTinhLuong = 'Capnhatluong'
    
    OPEN curRS
    
    FETCH NEXT FROM curRS INTO @strTentruong,@strCaulenhSQL                                                                            
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @strSelectLluong = REPLACE(@strSelectLluong ,@strTentruong ,@strCaulenhSQL) 
        SET @strCheck = @strCheck+','+@strTentruong
        FETCH NEXT FROM curRS INTO @strTentruong,@strCaulenhSQL
    END
    CLOSE curRS
    DEALLOCATE curRS
    
    DECLARE curRS          CURSOR  
    FOR
        SELECT Tentruong
              ,CaulenhSQL  
        FROM   CongthucLuong cl
        WHERE  cl.LoaiTinhLuong = 'Capnhatluong'
    
    OPEN curRS
    
    FETCH NEXT FROM curRS INTO @strCheck,@strCaulenhSQL                                                                            
    WHILE @@FETCH_STATUS=0
    BEGIN
        IF (CHARINDEX(@strCheck ,@strSelectLluong)>0)
        BEGIN
            SET @strSelectLluong = dbo.fnReplaceField(@strSelectLluong)
        END
        
        FETCH NEXT FROM curRS INTO @strCheck,@strCaulenhSQL
    END
    CLOSE curRS
    DEALLOCATE curRS 
    
    RETURN @strSelectLluong
    --PRINT @strSelectLluong
END

GO
/****** Object:  UserDefinedFunction [dbo].[fuDocBaSo]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fuDocBaSo]
(
@BaSo int
)
RETURNS NVARCHAR(500)
AS
BEGIN

DECLARE @KetQua nvarchar(500)
SET @KetQua=''
DECLARE @TBSo_Chu TABLE
(So int, ChuSo nvarchar(20))
INSERT INTO @TBSo_Chu
SELECT 0, N' không' UNION
SELECT 1, N' một'UNION
SELECT 2, N' hai' UNION
SELECT 3, N' ba' UNION
SELECT 4, N' bốn' UNION
SELECT 5, N' năm' UNION
SELECT 6, N' sáu' UNION
SELECT 7, N' bảy' UNION
SELECT 8, N' tám' UNION
SELECT 9, N' chín'
DECLARE @Tram int
DECLARE @Chuc int
DECLARE @DonVi int
DECLARE @nStr nvarchar(20)
SET @Tram =cast(@BaSo/100 AS int)
SET @Chuc=cast((@BaSo%100)/10 AS int);
SET @DonVi=@BaSo%10
IF (@Tram=0 AND @Chuc=0 AND @DonVi=0) SET @KetQua=''
IF @Tram<>0
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So = @Tram
SET @KetQua = @nStr + N' trăm '
IF @Chuc=0 AND @DonVi<>0 SET @KetQua=@KetQua+ ' linh '
END
IF @Chuc <>0 AND @Chuc<>1
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@Chuc
SET @KetQua = @KetQua + @nStr + N' mươi '
IF @Chuc=0 AND @DonVi<>0
SET @KetQua=@KetQua+ ' linh '
END
IF @Chuc=1 SET @KetQua=@KetQua+ N' mười '
IF @DonVi=1
BEGIN
IF @Chuc<>0 AND @Chuc<>1
SET @KetQua =@KetQua +N' mốt'
ELSE
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@DonVi
SET @KetQua=@KetQua+@nStr
END
END
ELSE
BEGIN
IF @DonVi=5
BEGIN
IF @Chuc=0
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@DonVi
SET @KetQua=@KetQua+@nStr
END
ELSE
SET @KetQua=@KetQua+N' lăm'
END
ELSE
BEGIN
IF @DonVi<>0
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@DonVi
SET @KetQua=@KetQua+@nStr
END
END
END
RETURN @KetQua
END
GO
/****** Object:  UserDefinedFunction [dbo].[fuDocBaSo_Ben]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fuDocBaSo_Ben]
(
@BaSo int
)
RETURNS NVARCHAR(500)

AS
BEGIN
DECLARE @KetQua nvarchar(500)
SET @KetQua=''
DECLARE @TBSo_Chu TABLE
(So int, ChuSo nvarchar(20))
INSERT INTO @TBSo_Chu
SELECT 0, N' không' UNION
SELECT 1, N' một'UNION
SELECT 2, N' hai' UNION
SELECT 3, N' ba' UNION
SELECT 4, N' bốn' UNION
SELECT 5, N' năm' UNION
SELECT 6, N' sáu' UNION
SELECT 7, N' bảy' UNION
SELECT 8, N' tám' UNION
SELECT 9, N' chín'
DECLARE @Tram int
DECLARE @Chuc int
DECLARE @DonVi int
DECLARE @nStr nvarchar(20)
SET @Tram =cast(@BaSo/100 AS int)
SET @Chuc=cast((@BaSo%100)/10 AS int);
SET @DonVi=@BaSo%10
IF (@Tram=0 AND @Chuc=0 AND @DonVi=0) SET @KetQua=''
IF @Tram<>0
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So = @Tram
SET @KetQua = @nStr + N' trăm '
IF @Chuc=0 AND @DonVi<>0 SET @KetQua=@KetQua+ ' linh '
END
if @Tram=0
BEGIN
if @Chuc=0
BEGIN
if @DonVi=0
SET @KetQua = @KetQua + N' '
else
SET @KetQua = @KetQua + N' không trăm linh'
END

if @Chuc<>0
SET @KetQua = @KetQua + N' không trăm'
END

IF @Chuc <>0 AND @Chuc<>1
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@Chuc
SET @KetQua = @KetQua + @nStr + N' mươi '
IF @Chuc=0 AND @DonVi<>0
SET @KetQua=@KetQua+ ' linh '
END
IF @Chuc=1 SET @KetQua=@KetQua+ N' mười '

IF @DonVi=1
BEGIN
IF @Chuc<>0 AND @Chuc<>1
SET @KetQua =@KetQua +N' mốt'
ELSE
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@DonVi
SET @KetQua=@KetQua+@nStr
END
END
ELSE
BEGIN
IF @DonVi=5
BEGIN
IF @Chuc=0
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@DonVi
SET @KetQua=@KetQua+@nStr
END
ELSE
SET @KetQua=@KetQua+N' lăm'
END
ELSE
BEGIN
IF @DonVi<>0
BEGIN
SELECT @nStr = ChuSo FROM @TBSo_Chu WHERE So=@DonVi
SET @KetQua=@KetQua+@nStr
END
END
END
RETURN @KetQua
END
GO
/****** Object:  UserDefinedFunction [dbo].[fuDocSoThanhChu]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fuDocSoThanhChu](@SoCanDoc bigint)
RETURNS nvarchar(200)
AS
BEGIN
DECLARE @DocThanhChu nvarchar(200)

DECLARE @String nvarchar(50)
IF len(@SoCanDoc)>15
BEGIN
SET @DocThanhChu=N'Số quá lớn, Tôi không biết đọc'
END
ELSE
SET @String =Replace(Convert(VARCHAR,CAST(@SoCanDoc AS MONEY),1 ),'.00','')
BEGIN
DECLARE @Count int
SELECT @Count = COUNT(*) FROM dbo.SplitString(@String,',')
DECLARE @tram nvarchar(10)
DECLARE @Nghin nvarchar(10)
DECLARE @Trieu nvarchar(10)
DECLARE @ty nvarchar(10)
DECLARE @nghinty nvarchar(10)
DECLARE @trieuty nvarchar(10)
IF @Count=1
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@SoCanDoc)
END
IF @Count=2
BEGIN
SELECT @Nghin=part FROM dbo.SplitString(@String,',') WHERE id=1
SELECT @tram=part FROM dbo.SplitString(@String,',') WHERE id=2
SET @DocThanhChu=dbo.fuDocBaSo(@Nghin)+N' nghìn '+ dbo.fuDocBaSo_Ben(@tram)
END
IF @Count=3
BEGIN

SELECT @Trieu=part FROM dbo.SplitString(@String,',') WHERE id=1
SELECT @Nghin=part FROM dbo.SplitString(@String,',') WHERE id=2
SELECT @tram = part FROM dbo.SplitString(@String,',') WHERE id=3

IF Cast(@Nghin as int)>0
BEGIN
IF Cast(@tram as int)>0
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@Trieu) +N' triệu' +
dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'+ dbo.fuDocBaSo_Ben(@tram)
END
ELSE
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@Trieu) +N' triệu' +
dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
END
End
ELSE
BEGIN
if Cast(@tram as int) =0
SET @DocThanhChu=dbo.fuDocBaSo(@Trieu) +N' triệu'
else
SET @DocThanhChu=dbo.fuDocBaSo(@Trieu) +N' triệu' +
dbo.fuDocBaSo_Ben(@tram)
END
END
IF @Count=4
BEGIN
SELECT @ty=part FROM dbo.SplitString(@String,',') WHERE id=1
SELECT @Trieu=part FROM dbo.SplitString(@String,',') WHERE id=2
SELECT @Nghin=part FROM dbo.SplitString(@String,',') WHERE id=3
SELECT @tram = part FROM dbo.SplitString(@String,',') WHERE id=4

if cast(@Trieu as int)>0
BEGIN
IF cast(@Nghin as int)>0
BEGIN
if cast(@tram as int)>0
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Trieu) + N' triệu '
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn '
+ dbo.fuDocBaSo_Ben(@tram)
END
else
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
END
END
ELSE
BEGIN
IF cast(@tram as int)>0
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Trieu) + N' triệu '
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn '
+ dbo.fuDocBaSo_Ben(@tram)
END
ELSE
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Trieu) + N' triệu '
END
END
END
ELSE
BEGIN
if cast(@Nghin as int)>0
BEGIN
if Cast(@tram as int)>0
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn '
+ dbo.fuDocBaSo_Ben(@tram)
END
else
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn '
END
END
else
if cast(@tram as int)>0
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@tram)
END
else
BEGIN
SET @DocThanhChu=dbo.fuDocBaSo(@ty) +N' tỷ'
END
END
END
IF @Count=5
BEGIN

SELECT @nghinty =part FROM dbo.SplitString(@String,',') WHERE id=1
SELECT @ty=part FROM dbo.SplitString(@String,',') WHERE id=2
SELECT @Trieu=part FROM dbo.SplitString(@String,',') WHERE id=3
SELECT @Nghin=part FROM dbo.SplitString(@String,',') WHERE id=4
SELECT @tram = part FROM dbo.SplitString(@String,',') WHERE id=5

if cast(@ty as int)>0
BEGIN
if cast(@Trieu as int)>0
BEGIN
if cast(@Nghin as int)>0
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
END
else
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
END
END
else
BEGIN
if cast(@Nghin as int)>0
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
END
else
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn'
+dbo.fuDocBaSo_Ben(@ty) +N' tỷ'
END
END
END
else
BEGIN


if cast(@Trieu as int)>0
BEGIN
if cast(@Nghin as int)>0
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
END
else
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
+dbo.fuDocBaSo_Ben(@Trieu) + N' triệu'
END
END
else
BEGIN
if cast(@Nghin as int)>0
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
+ dbo.fuDocBaSo_Ben(@Nghin) + N' nghìn'
END
else
BEGIN
if cast(@tram as int)>0
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
+ dbo.fuDocBaSo_Ben(@tram)
else
SET @DocThanhChu= dbo.fuDocBaSo(@nghinty) +N' nghìn tỷ'
END
END
END
END
END
RETURN ltrim(UPPER(left(@DocThanhChu,2)) + SUBSTRING( @DocThanhChu,3,LEN(@DocThanhChu)) + N' đồng')
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetDateOnly]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/************************************************************
*  Routine       :	dbo.GetDateOnly
*  Created by    :  Duong Van Quyet , at 20/01/2011 - 14:54:03  
*  Machine       :  DIGISOFT
*  Description   :  Chi lay ngay tu @D
*  Parameters    :  @D datetime
************************************************************/

CREATE FUNCTION [dbo].[GetDateOnly]
(
	@D   DATETIME

)
RETURNS DATETIME
AS




BEGIN
	DECLARE @Temp DATETIME
	SET @Temp= DATEADD(dd, 0, DATEDIFF(dd, 0, @D))
	RETURN @Temp;
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetTimeOnly]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/************************************************************
*  Routine       :	dbo.GetTimeOnly
*  Created by    :  Duong Van Quyet , at 20/01/2011 - 14:54:56  
*  Machine       :  DIGISOFT
*  Description   :  Lay Time tu @D
*  Parameters    :  @D datetime
************************************************************/
CREATE FUNCTION [dbo].[GetTimeOnly]
(
	@D   DATETIME

)
RETURNS DATETIME
AS




BEGIN
	DECLARE @Temp DATETIME
	SET @Temp= CONVERT(VARCHAR(8),@D,108)
	RETURN @Temp;
END

GO
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime2]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO



CREATE FUNCTION [dbo].[InterSectionTime2]
(  
 @T11  Datetime, 
 @T12 Datetime, 
 @T21 Datetime, 
 @T22 Datetime)
RETURNS int
AS  
BEGIN 
	DECLARE @T1 Datetime
	DECLARE @T2 Datetime
	Declare	@iTemp int
	
	IF ((@T11 + @T12 + @T21+ @T22) IS NULL)
	BEGIN
		RETURN 0
	END
	
	--SET @T11=CONVERT(VARCHAR(8),@T11,108)
	--SET @T12=CONVERT(VARCHAR(8),@T12,108)
	--SET @T21=CONVERT(VARCHAR(8),@T21,108)
	--SET @T22=CONVERT(VARCHAR(8),@T22,108)
	
	SET @T1= CASE when @T11<@T21 THEN @T21 ELSE @T11 end
	SET @T2= CASE when @T12>@T22 THEN @T22 ELSE @T12 end
	
	SET @iTemp = CASE when @T1 > @T2 THEN 0 ELSE DateDiff(minute, @T1, @T2) end
	--T1 = IIf(T11 < T21, T21, T11)
    --T2 = IIf(T12 > T22, T22, T12)
    --InterSectionTime2 = IIf(T1 >= T2, 0, DateDiff("n", T1, T2))
    
	RETURN @iTemp
END





GO
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime2_Day]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE FUNCTION [dbo].[InterSectionTime2_Day]
(  
 @T11  Datetime, 
 @T12 Datetime, 
 @T21 Datetime, 
 @T22 Datetime)
RETURNS int
AS  
BEGIN 
	DECLARE @T1 Datetime
	DECLARE @T2 Datetime
	Declare	@iTemp int
	
	SET @T1= CASE when @T11<@T21 THEN @T21 ELSE @T11 end
	SET @T2= CASE when @T12>@T22 THEN @T22 ELSE @T12 end
	
	SET @iTemp = CASE when @T1 > @T2 THEN 0 ELSE DateDiff(day, @T1, @T2) end
	--T1 = IIf(T11 < T21, T21, T11)
    --T2 = IIf(T12 > T22, T22, T12)
    --InterSectionTime2 = IIf(T1 >= T2, 0, DateDiff("n", T1, T2))
    
	RETURN @iTemp
END

GO
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime3]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO




CREATE FUNCTION [dbo].[InterSectionTime3]
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

	IF ((@T11 + @T12 + @T21+ @T22 + @T31 + @T32) IS NULL)
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
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime3_Day]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO





CREATE FUNCTION [dbo].[InterSectionTime3_Day]
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
		
	SET @iTemp = CASE when @T1 > @T2 THEN 0 ELSE DateDiff(day, @T1, @T2) END
		
	IF(@iTemp>0) SET 	@iTemp=@iTemp+1
	IF (@T1=@T2)	SET @iTemp=1

	RETURN @iTemp
END







GO
/****** Object:  UserDefinedFunction [dbo].[ReturnNumeric]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/************************************************************
*  Routine       :	dbo.ReturnNumeric
*  Created by    :  Duong Van Quyet , at 20/01/2011 - 14:55:46  
*  Machine       :  DIGISOFT
*  Description   :  Lay so tu chuoi truyen vao
*  Parameters    :  @St nvarchar(20), @FromLeft bit
************************************************************/
CREATE FUNCTION [dbo].[ReturnNumeric]
(
	@St        NVARCHAR(20),
	@FromLeft  BIT
)
RETURNS NVARCHAR(20)
AS
BEGIN
	DECLARE @returnValue  NVARCHAR(20) 
	DECLARE @ii           INT 
	DECLARE @BookMark     INT 
	
	DECLARE @sTemp        NVARCHAR(1)
	
	-- Neu chuoi ky tu dua vao rong thi tra ve gia tri 0
	IF ((@St IS NULL) OR (LEN(@St) = 0))
	BEGIN
	    SET @returnValue = '0'
	    GOTO EndThisSub
	END
	
	-- Neu la lay so tu ben trai
	IF (@FromLeft = 1)
	BEGIN
	    SET @ii = 1
	    WHILE (@ii <= LEN(@St))
	    BEGIN
	        SET @sTemp = SUBSTRING(@St, @ii, 1)
	        IF (ISNUMERIC(Replace(@sTemp,'-','')) = 0)
	        BEGIN

	            BREAK
	        END
	        
	        SET @BookMark = @ii
	        
	        SET @ii = @ii + 1
	    END
	    
	    -- Lay gia tri sau khi tim duoc vi tri @BookMark
	    
	    IF (@BookMark >= 1)
	    BEGIN
	        SET @returnValue = SUBSTRING(@St, 0, @BookMark + 1)
	    END
	    ELSE
	        SET @returnValue = '0'
	END-- Neu lay gia tri tu phai sang
	ELSE
	BEGIN
	    SET @ii = LEN(@St)
	    
	    WHILE (@ii >= 0)
	    BEGIN
	        SET @sTemp = SUBSTRING(@St, @ii, 1)
	        IF (ISNUMERIC(Replace(@sTemp,'-','')) = 0)
	        BEGIN
	            BREAK
	        END
	        ELSE
	            SET @BookMark = @ii
	        
	        SET @ii = @ii - 1
	    END
	    
	    -- Lay gia tri sau khi tim duoc vi tri @BookMark
	    
	    IF (@BookMark <= LEN(@St) AND @BookMark > 0)
	    BEGIN
	        SET @returnValue = SUBSTRING(@St, @BookMark, LEN(@St) -@BookMark+1)
	    END
	    ELSE
	        SET @returnValue = '0'
	END
	EndThisSub:
	
	RETURN @returnValue
END
GO
/****** Object:  UserDefinedFunction [dbo].[SplitString]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--CREATE BY webmaster@hmweb.com.vn

-- Split function

CREATE FUNCTION [dbo].[SplitString]

(

    @myString varchar(500),

    @deliminator varchar(10)

)

RETURNS

@ReturnTable TABLE

(

    [id] [int] IDENTITY(1,1) NOT NULL,

    [part] [varchar](50) NULL

)

AS

BEGIN

    Declare @iSpaces int

    Declare @part varchar(50)

    --initialize spaces

    Select @iSpaces = charindex(@deliminator,@myString,0)

    While @iSpaces > 0

    BEGIN

        Select @part =

            substring(@myString,0,charindex(@deliminator,@myString,0))

        Insert Into @ReturnTable(part)

        Select @part

          Select @myString =

            substring(@mystring,charindex(@deliminator,@myString,0)+

            len(@deliminator),len(@myString)- charindex(' ',@myString,0))

        Select @iSpaces = charindex(@deliminator,@myString,0)

    END

    If len(@myString) > 0

        Insert Into @ReturnTable

        Select @myString

    RETURN

END
GO
/****** Object:  UserDefinedFunction [dbo].[udfTaxableIncome]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE FUNCTION [dbo].[udfTaxableIncome]
(@ToTalMoney as numeric,
 @Foreigner as bit)
RETURNS numeric AS  	
begin
   declare @TSTHSDanhThue as numeric
   declare @TSTGHDuoi as numeric
   declare @TSTGHTren as numeric
   declare @Result as numeric
   set @Result=0
   declare CurTax cursor dynamic for select TSTHSDanhThue,TSTGHDuoi,TSTGHTren from 
	 LThamSoThue where (TSTNguoiNN=@Foreigner)
   open CurTax
   while (0=0)
     begin
	fetch next from CurTax into @TSTHSDanhThue,@TSTGHDuoi,@TSTGHTren
	if (@@fetch_status<> 0)
	    break
	if (@ToTalMoney between @TSTGHDuoi and @TSTGHTren)
	   begin
		set @Result=@Result+(@ToTalMoney-@TSTGHDuoi)*@TSTHSDanhThue/100
		break
	   end
	else
	   begin
		if (@TSTGHTren=0) 
		    set @Result=@Result+(@ToTalMoney-@TSTGHDuoi)*@TSTHSDanhThue/100
		else
		    set @Result=@Result+(@TSTGHTren-@TSTGHDuoi)*@TSTHSDanhThue/100
	   end
     end
     close CurTax
     deallocate CurTax
     return @Result
end
GO
/****** Object:  UserDefinedFunction [dbo].[ufn_GetDaysInMonth]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/************************************************************
*  Routine       :	dbo.ufn_GetDaysInMonth
*  Created by    :  Duong Van Quyet , at 25/01/2011 - 10:08:56  
*  Machine       :  DIGISOFT
*  Description   :  Lay so ngay cua thang
*  Parameters    :  @pDate datetime
************************************************************/
CREATE FUNCTION [dbo].[ufn_GetDaysInMonth]
(
	@pDate DATETIME
)
RETURNS INT
AS
BEGIN
	SET @pDate = CONVERT(VARCHAR(10), @pDate, 101)
	SET @pDate = @pDate - DAY(@pDate) + 1
	
	RETURN DATEDIFF(DD, @pDate, DATEADD(MM, 1, @pDate))
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_OTActualCheckInOut]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_OTActualCheckInOut]
(
    @WorkDate DATE
)
RETURNS TABLE
AS
RETURN
(
    WITH
    LatestCard AS (
        SELECT CTMaNV, CTMaThe
        FROM (
            SELECT CTMaNV, CTMaThe,
                   ROW_NUMBER() OVER (PARTITION BY CTMaNV ORDER BY CTNgayApDung DESC) AS rn
            FROM [HRM].[dbo].[tblCapThe]
        ) x
        WHERE rn = 1
    ),

    AllSwipes AS (
        SELECT
            RTRIM(nv.NVMaNV)            AS NVMaNV,
            RTRIM(nv.NVHoTen)           AS HoTen,
            fe.DeptCode,
            r.ThoiGian,
            dd.DDChinhVao               AS IsCheckIn
        FROM [HRM].[dbo].[RecordDataNew]      r
        INNER JOIN [HRM].[dbo].[tblDauDoc]   dd ON dd.DDMa    = r.IDM
        INNER JOIN LatestCard                lc ON lc.CTMaThe = r.IDCard
        INNER JOIN [HRM].[dbo].[tblNhanVien] nv ON nv.NVMa    = lc.CTMaNV
        INNER JOIN [FVNWEBAPP].[dbo].[F03Employee] fe
            ON  fe.EmployeeCode = RTRIM(nv.NVMaNV)
            AND fe.IsActive     = 1
        WHERE r.ThoiGian >= DATEADD(HOUR, 17, CAST(DATEADD(DAY,-1,@WorkDate) AS DATETIME))
          AND r.ThoiGian <  DATEADD(HOUR,  7, CAST(DATEADD(DAY, 1,@WorkDate) AS DATETIME))
    ),

    -- ✅ [FIX] Gọi fn_GetShiftFromCheckIn qua CROSS APPLY — không viết lại window-match
    CheckInWithShift AS (
        SELECT
            s.NVMaNV, s.HoTen, s.DeptCode,
            s.ThoiGian                                   AS CheckInDT,
            CAST(s.ThoiGian AS TIME(0))                  AS CheckInTime,
            g.ShiftId, g.ShiftCode, g.ShiftName, g.IsOvernight,
            g.OT_ThresholdMinutes, g.OT_BlockMinutes,

            -- Ghép EndTime (TIME) với WorkDate cụ thể — chỉ là date arithmetic, không phải business logic
            CASE WHEN g.IsOvernight = 1
                 THEN CAST(DATEADD(DAY,1,@WorkDate) AS DATETIME) + CAST(g.EndTime AS DATETIME)
                 ELSE CAST(@WorkDate AS DATETIME) + CAST(g.EndTime AS DATETIME)
            END                                           AS ShiftEndDT,

            ROW_NUMBER() OVER (PARTITION BY s.NVMaNV ORDER BY s.ThoiGian ASC) AS RankIn

        FROM AllSwipes s
        CROSS APPLY [FVNWEBAPP].[dbo].fn_GetShiftFromCheckIn(CAST(s.ThoiGian AS TIME)) g
        WHERE s.IsCheckIn = 1
    ),

    FirstIn AS (
        SELECT * FROM CheckInWithShift WHERE RankIn = 1
    ),

    LastOut AS (
        SELECT i.NVMaNV, MAX(o.ThoiGian) AS CheckOutDT
        FROM FirstIn i
        INNER JOIN AllSwipes o
            ON  o.NVMaNV    = i.NVMaNV
            AND o.IsCheckIn = 0
            AND o.ThoiGian  > i.CheckInDT
            AND o.ThoiGian <= DATEADD(HOUR, 16, i.CheckInDT)
        GROUP BY i.NVMaNV
    ),

    Holidays AS (
        SELECT
            CAST(HolidayDate AS DATE) AS HDate,
            Description,
            CASE
                WHEN DATEPART(WEEKDAY, HolidayDate) = 1 THEN 'SUNDAY'
                WHEN DATEPART(WEEKDAY, HolidayDate) = 7 THEN 'SATURDAY'
                ELSE 'PUBLIC_HOLIDAY'
            END AS HolidayType
        FROM [FVNWEBAPP].[dbo].[CompanyHoliday]
        WHERE HolidayDate IS NOT NULL
          AND CAST(HolidayDate AS DATE) = @WorkDate
    )

    SELECT
        i.NVMaNV       AS EmployeeCode,
        i.DeptCode,
        i.HoTen        AS FullName,
        @WorkDate      AS WorkDate,
        CONVERT(VARCHAR(5), i.CheckInTime, 108)                  AS CheckInText,
        CONVERT(VARCHAR(5), CAST(o.CheckOutDT AS TIME(0)), 108)  AS CheckOutText,
        i.CheckInDT    AS CheckInDateTime,
        o.CheckOutDT   AS CheckOutDateTime,
        i.ShiftCode, i.ShiftName, i.ShiftEndDT, i.OT_ThresholdMinutes,

        CASE WHEN h.HDate IS NOT NULL THEN 1 ELSE 0 END          AS IsHoliday,
        h.HolidayType,
        h.Description  AS HolidayDesc,

        CASE WHEN o.CheckOutDT IS NOT NULL
             THEN CAST(DATEDIFF(MINUTE, i.CheckInDT, o.CheckOutDT) / 60.0 AS DECIMAL(5,2))
             ELSE NULL
        END AS TotalHours,

        -- ✅ [FIX] Gọi fn_CalcOTMinutes — 1 nguồn công thức duy nhất, có block rounding
        CAST(
            [FVNWEBAPP].[dbo].fn_CalcOTMinutes(
                i.CheckInDT, o.CheckOutDT, i.ShiftEndDT,
                i.OT_ThresholdMinutes, i.OT_BlockMinutes,
                CASE WHEN h.HDate IS NOT NULL THEN 1 ELSE 0 END
            ) / 60.0
        AS DECIMAL(5,2)) AS OTHours,

        CASE
            WHEN o.CheckOutDT IS NULL                              THEN 'NO_CHECKOUT'
            WHEN h.HolidayType = 'PUBLIC_HOLIDAY'                  THEN 'OT_HOLIDAY'
            WHEN h.HolidayType = 'SUNDAY'                          THEN 'OT_SUNDAY'
            WHEN h.HolidayType = 'SATURDAY'                        THEN 'OT_SATURDAY'
            WHEN o.CheckOutDT > DATEADD(MINUTE, i.OT_ThresholdMinutes, i.ShiftEndDT)
                                                                     THEN 'HC_OT'
            ELSE 'HC_NORMAL'
        END AS ShiftType

    FROM FirstIn   i
    LEFT JOIN LastOut  o ON o.NVMaNV = i.NVMaNV
    LEFT JOIN Holidays h ON h.HDate  = @WorkDate
);
GO
/****** Object:  UserDefinedFunction [dbo].[udfRayrise]    Script Date: 9/19/2026 10:40:47 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE FUNCTION [dbo].[udfRayrise]
(@FromDate as datetime,
 @ToDate as datetime,
 @PartCode as int,
 @StaffCode as int)
RETURNS TABLE AS  
 	return select * from tblNhanVien inner join tblBoPhan on NVMaBP=BPMa
				  inner join tblChucVu on NVMaCV=CVMa
				  inner join LDienBienLuong on NVMa=DBLMaNV
	where ((NVNgayVao<=@ToDate) and (NVNgayRa>=@FromDate)) and
		(DBLNgayApDung between @FromDate and @ToDate) and
		(BPMa=@PartCode) and (NVMa=@StaffCode)
GO
