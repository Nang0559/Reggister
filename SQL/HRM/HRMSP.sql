USE [HRM]
GO
/****** Object:  UserDefinedFunction [dbo].[AddValToStr]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[DonVi_IDs]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[F_DemSoNgayCN]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[FindShiftOfStaff]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_CheckDKN]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_FindShiftNew]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_Max]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_Min]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_SumDangKyNghi]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_TGThemN]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_TinhTGLamThem]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_TongNgayNP]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fnReplaceField]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fuDocBaSo]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fuDocBaSo_Ben]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fuDocSoThanhChu]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[GetDateOnly]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[GetTimeOnly]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime2]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime2_Day]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime3]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[InterSectionTime3_Day]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[ReturnNumeric]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[SplitString]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[udfTaxableIncome]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[ufn_GetDaysInMonth]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[fn_OTActualCheckInOut]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  UserDefinedFunction [dbo].[udfRayrise]    Script Date: 9/19/2026 10:24:46 AM ******/
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
/****** Object:  StoredProcedure [dbo].[CalculatePartTimeKeeping]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[CalculatePartTimeKeeping]
	@DID int,
	@DIDList nvarchar(1000),	
	@D Datetime
 as
	
BEGIN
	
	DECLARE @DID_Tmp	Int
	DECLARE @DIDParent	Int
	DECLARE @SID_Tmp	Int
	Declare @SQL nvarchar(1000)
	
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblBoPhanTam]') AND type in (N'U'))
	DROP TABLE [dbo].[tblBoPhanTam]
	
	Create TABLE tblBoPhanTam([BPMa][int],[BPMaCha] [int] )
	set @SQL= 'Insert INTO tblBoPhanTam(BPMa,BPMaCha) SELECT BPMa,BPMaCha FROM tblBoPhan WHERE BPMa in (' + @DIDList + ')'
	exec (@SQL)
	
	DECLARE curRS1 CURSOR FOR
	SELECT BPMa,BPMaCha FROM tblBoPhanTam 

	OPEN curRS1
	FETCH NEXT FROM curRS1 INTO @DID_Tmp,@DIDParent
	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		DECLARE curRSNV CURSOR FOR
		SELECT NVMa FROM tblNhanvien WHERE NVMaBP = @DID_Tmp
		OPEN curRSNV
		FETCH NEXT FROM curRSNV INTO @SID_Tmp
			WHILE @@FETCH_STATUS = 0
			BEGIN
				
				Exec dbo.sp_TimeKeeping @SID_Tmp,@D
				FETCH NEXT FROM curRSNV INTO @SID_Tmp
			END
		CLOSE curRSNV
		DEALLOCATE curRSNV
		
	    FETCH NEXT FROM curRS1 INTO @DID_Tmp,@DIDParent
	END
	CLOSE curRS1
	DEALLOCATE curRS1
END
GO
/****** Object:  StoredProcedure [dbo].[cp_BangChamCongOnl_New]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[cp_BangChamCongOnl_New]
@CCMaNV  INT,
@CCMaBP INT,
@NgayLoc SMALLDATETIME
AS

DECLARE @SQL1 VARCHAR(4000)
declare @a varchar(5)
declare @b varchar(5)
declare @c varchar(5)
declare @e varchar(5)
set @a= convert(varchar(5),year(@NgayLoc))
set @b = convert(varchar(5),month(@NgayLoc))
set @c = Right('00'+convert(varchar(5),day(@NgayLoc)),2)
set @e = '00'

SET @SQL1 =  'declare @Ngay smalldatetime 
set @Ngay = convert(varchar(5), '+ @a +') + Right(convert(varchar(2),'+ @e +') + Convert(varchar(5),'+ @b +'),2) + Right(Convert(varchar(2),'+ @e +') + Convert(varchar(5),'+ @c +') ,2)
DECLARE @CurMaNV INT
DECLARE @CurMaBP INT
DECLARE @Nam INT
DECLARE @Thang INT

SET @Thang = MONTH(@Ngay)
SET @Nam = YEAR(@Ngay)

IF LEN('+ CONVERT(VARCHAR(10),@CCMaNV) +') = 0 OR '+ CONVERT(VARCHAR(10),@CCMaNV) +' = 0
	BEGIN
	DECLARE CusortblBangchamcong2009 cursor
	FOR
		SELECT
		BCCMaNV AS CurMaNV ,
		BCCMaBP AS CurMaBP
		FROM tblBangchamcong2009
		WHERE BCCThang = MONTH(@Ngay) 
		AND  BCCMaBP  IN ( '+dbo.DonVi_IDs(@CCMaBP) +')
	END
ELSE	
	BEGIN
	DECLARE CusortblBangchamcong2009 cursor
	FOR
		SELECT
		BCCMaNV AS CurMaNV ,
		BCCMaBP AS CurMaBP
		FROM tblBangchamcong2009
	WHERE BCCThang = MONTH(@Ngay) AND BCCMaNV = '+ CONVERT(VARCHAR(10),@CCMaNV) +' 
	END	
OPEN CusortblBangchamcong2009
FETCH NEXT FROM CusortblBangchamcong2009 INTO @CurMaNV, @CurMaBP 
	WHILE @@Fetch_Status = 0
	BEGIN
		exec cp_Taobangchotungnguoi @CurMaNV , @Nam , @Thang
		exec cp_TongCongChoMotNhanVien_Children @CurMaNV ,@Thang,@Nam  
	FETCH NEXT FROM CusortblBangchamcong2009 INTO @CurMaNV, @CurMaBP
	END
CLOSE  CusortblBangchamcong2009	
DEALLOCATE CusortblBangchamcong2009
'
exec(@SQL1)
GO
/****** Object:  StoredProcedure [dbo].[cp_CapNhatTuChamCong_LChamCong_Staff]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[cp_CapNhatTuChamCong_LChamCong_Staff]
@NVMa INT,
@Date DATETIME
as

DECLARE @Nam VARCHAR(5)
DECLARE @Thang VARCHAR(3)

SET @Nam = CONVERT(VARCHAR(5),YEAR(@Date))
SET @Thang =  CONVERT(VARCHAR(5),Month(@Date))

DECLARE  @SQL VARCHAR(8000)
DECLARE  @SQLUpdate VARCHAR(8000)
DECLARE  @SQLInsert VARCHAR(8000)
DECLARE  @SQLInsertT VARCHAR(8000)


DECLARE @CCTruongNguon VARCHAR(1000)
DECLARE @CCTruongDich VARCHAR(1000)
DECLARE @CCCauLenhSQL VARCHAR(8000)

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[LBangChamCongThangtx]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[LBangChamCongThangtx]


SET @SQL = 'Select BCCMaNV AS bcclmanv,NVMaNV AS bcclmanvn,BCCMaBP AS bcclmabp, BCCMaCV AS bcclmacv, BCCThang AS bcclthang '

SET @SQLUpdate = 'Update LBangChamCong'+ @Nam +' Set bcclmanvn = t.bcclmanvn '
set @SQLInsert = 'insert into LBangChamCong'+ @Nam +' (bcclmanv,bcclmanvn,bcclmabp,bcclmacv,bcclthang '
set @SQLInsertT = ' bcclmanv,bcclmanvn,bcclmabp,bcclmacv,bcclthang '
DECLARE CurrCTL CURSOR FOR
 SELECT CCTruongNguon,CCTruongDich, CCCaulenhSQL FROM CTL_CapnhatChamcong
OPEN 	CurrCTL
FETCH NEXT FROM CurrCTL INTO @CCTruongNguon,@CCTruongDich,@CCCauLenhSQL
WHILE @@FETCH_STATUS = 0 
BEGIN
	SET @SQL = @SQL + ','+ @CCCauLenhSQL + ' AS ' + @CCTruongDich	
	SET @SQLUpdate =  @SQLUpdate + ','+ @CCTruongDich + '= t.'+ @CCTruongDich	
	SET @SQLInsert =  @SQLInsert + ','+ @CCTruongDich	
	SET @SQLInsertT =  @SQLInsertT + ','+ @CCTruongDich	
	FETCH NEXT FROM CurrCTL INTO @CCTruongNguon,@CCTruongDich,@CCCauLenhSQL 
END
CLOSE CurrCTL
DEALLOCATE CurrCTL
--PRINT @SQL

DECLARE @SQLTable VARCHAR(8000)

SET @SQLTable = ' Into LBangChamCongThangtx  From tblBangChamCong'+ @Nam +' tcc 
Inner join tblNhanVien tnv On tcc.BCCMaNV = tnv.NVMa
--Inner join tblLDangkynghi'+ @Nam +'  dkn On dkn.LDKNMaNV = tcc.BCCMaNV AND dkn.LDKNThang = tcc.BCCThang
where tcc.BCCThang = '+ @Thang +'
AND  tcc.BCCMaNV = '+ Convert(varchar(10),@NVMa) +' '
SET @SQL = @SQL + @SQLTable
EXEC(@SQL)

SET @SQLUpdate = @SQLUpdate + ' From LBangChamCong'+ @Nam +'  bc 
Inner Join LBangChamCongThangtx t On t.bcclmanv = bc.bcclmanv AND t.bcclthang = bc.bcclthang   '

SET @SQLInsert = @SQLInsert + ') '

DECLARE @SQLCheck VARCHAR(8000)

CREATE TABLE #Value(GiaTri int)
DELETE FROM #Value

SET @SQLCheck = '
if exists (select * from LBangChamCong'+ @Nam +'  where  bcclmanv = '+  Convert(varchar(10),@NVMa)  +' and bcclthang = '+ @Thang +' )
BEGIN
	insert into #Values(GiaTri) values(1)
END
'
EXEC(@SQLCheck)

DECLARE @intcheck INT

SET @intcheck = ISNULL((SELECT * FROM #Value),0)

IF @intcheck = 1
	EXEC(@SQLUpdate)
ELSE
 BEGIN
 	SET @SQLInsert = @SQLInsert + ' Select '+@SQLInsertT+' from LBangChamCongThangtx '
 	EXEC(@SQLInsert)
 END
GO
/****** Object:  StoredProcedure [dbo].[cp_CheckAndCreateTable_Recorddata]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		thangtx
-- Create date: 2009-10-16
-- Description:	create table RecordData
-- =============================================
CREATE PROCEDURE [dbo].[cp_CheckAndCreateTable_Recorddata]
	@D DATETIME
AS
DECLARE @Check1 VARCHAR(8000)
DECLARE @Year VARCHAR(4)
DECLARE @Month VARCHAR(2)

SET @Year= YEAR(@D)
SET @Month= MONTH(@D)

SET @Check1 = ' 
IF NOT EXISTS(SELECT * FROM information_schema.tables WHERE Table_Name = '+ '''' +'RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'' + '''' +' )
	BEGIN
		CREATE TABLE [dbo].[RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +']
		 (		[IDM] [tinyint] NOT NULL ,
				[IDCard] [nvarchar] (14) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
				[ThoiGian] [datetime] NOT NULL ,
				[Status] [bit] NOT NULL ,
				[HandData] [bit] NOT NULL , 
				[Pass] [bit] NOT NULL,
				[Privilege] [int],
				[Option1] [nvarchar](max),
				[Option2] [float]
				
		 ) ON [PRIMARY]

	END
'

EXEC( @Check1)

SET @Check1 = ' 
IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE Table_Name = '+ '''' +'RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'' + ''''  + ' AND COLUMN_Name=' + '''' + 'Privilege' + '''' +' )
	BEGIN
		ALTER TABLE [RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'] ADD Privilege int 
		ALTER TABLE [RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'] ADD Option1 nvarchar(MAX)
		ALTER TABLE [RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'] ADD Option2 float
	END
'

EXEC( @Check1)


SET @Check1 = ' 
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name=' + '''' + 'IX_RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'_IDCard_Thoigian' + '''' + ' AND object_id = OBJECT_ID( '+ '''' +'RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'' + ''''  + ' ))
	BEGIN
		CREATE NONCLUSTERED INDEX [IX_RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'_IDCard_Thoigian] ON [dbo].[RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'] ([IDCard] ASC, [ThoiGian] ASC)ON [PRIMARY]

	END
'

EXEC( @Check1)

SET @Check1 = ' 
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name=' + '''' + 'IX_RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'_IDM_IDCard_Thoigian' + '''' + ' AND object_id = OBJECT_ID( '+ '''' +'RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'' + ''''  + ' ))
	BEGIN
		CREATE NONCLUSTERED INDEX [IX_RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'_IDM_IDCard_Thoigian] ON [dbo].[RecordData' + CONVERT(VARCHAR(10),@Year) +'_'+ RIGHT('00' + CONVERT(VARCHAR(5),@Month ),2) +'] ([IDM] ASC,[IDCard] ASC, [ThoiGian] ASC)ON [PRIMARY]

	END
'

EXEC( @Check1)
GO
/****** Object:  StoredProcedure [dbo].[cp_CheckAndCreateTable_tblBangChamCong]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		thangtx
-- Create date: 2009-10-16
-- Description:	create table TBangChamCong
-- =============================================
CREATE  PROCEDURE [dbo].[cp_CheckAndCreateTable_tblBangChamCong]
@Year INT
AS
DECLARE @Check1 VARCHAR(8000)
SET @Check1 = ' 
IF NOT EXISTS(SELECT * FROM information_schema.tables WHERE Table_Name = '+ '''' +'tblBangChamCong'+ CONVERT(VARCHAR(10),@Year)+'' + '''' +' )
BEGIN
	CREATE TABLE [dbo].[tblBangChamCong'+ CONVERT(VARCHAR(10),@Year)+'](
	[BCCID] [int] IDENTITY(1,1) NOT NULL,
	[BCCThang] [tinyint] NULL,
	[BCCMaNV] [int] NULL,
	[BCCMaBP] [int] NULL,
	[BCCMaCV] [int] NULL,
	[BCCNgay01] [nvarchar](8) NULL,
	[BCCNgay02] [nvarchar](8) NULL,
	[BCCNgay03] [nvarchar](8) NULL,
	[BCCNgay04] [nvarchar](8) NULL,
	[BCCNgay05] [nvarchar](8) NULL,
	[BCCNgay06] [nvarchar](8) NULL,
	[BCCNgay07] [nvarchar](8) NULL,
	[BCCNgay08] [nvarchar](8) NULL,
	[BCCNgay09] [nvarchar](8) NULL,
	[BCCNgay10] [nvarchar](8) NULL,
	[BCCNgay11] [nvarchar](8) NULL,
	[BCCNgay12] [nvarchar](8) NULL,
	[BCCNgay13] [nvarchar](8) NULL,
	[BCCNgay14] [nvarchar](8) NULL,
	[BCCNgay15] [nvarchar](8) NULL,
	[BCCNgay16] [nvarchar](8) NULL,
	[BCCNgay17] [nvarchar](8) NULL,
	[BCCNgay18] [nvarchar](8) NULL,
	[BCCNgay19] [nvarchar](8) NULL,
	[BCCNgay20] [nvarchar](8) NULL,
	[BCCNgay21] [nvarchar](8) NULL,
	[BCCNgay22] [nvarchar](8) NULL,
	[BCCNgay23] [nvarchar](8) NULL,
	[BCCNgay24] [nvarchar](8) NULL,
	[BCCNgay25] [nvarchar](8) NULL,
	[BCCNgay26] [nvarchar](8) NULL,
	[BCCNgay27] [nvarchar](8) NULL,
	[BCCNgay28] [nvarchar](8) NULL,
	[BCCNgay29] [nvarchar](8) NULL,
	[BCCNgay30] [nvarchar](8) NULL,
	[BCCNgay31] [nvarchar](8) NULL,
	[BCCCongN] [float] NULL,
	[BCCCongD] [float] NULL,
	[BCCLamThemN] [float] NULL,
	[BCCLamThemD] [float] NULL,
	[BCCLamThemCongTyN] [float] NULL,
	[BCCLamThemCongTyD] [float] NULL,
	[BCCLamThemLeN] [float] NULL,
	[BCCLamThemLeD] [float] NULL,
	[BCCDiMuonN] [float] NULL,
	[BCCDiMuonD] [float] NULL,
	[BCCVeSomN] [float] NULL,
	[BCCVeSomD] [float] NULL,
	[BCCSoLanDiMuon] [tinyint] NULL,
	[BCCSoLanVeSom] [tinyint] NULL,
	[BCCSoNghiLe] [float] NULL,
	[BCCSoNghiCongTy] [float] NULL,
	[BCCSoNghiThuong] [float] NULL,
	[BCCTongDKN] [nvarchar](200) NULL,
	[BCCTongCong] [float] NULL,
	[BCCSoCongKhongLam] [float] NULL,
	[BCCSoCaHC] [float] NULL,
	[BCCSoCa1] [float] NULL,
	[BCCSoCa2] [float] NULL,
	[BCCSoCa3] [float] NULL,
	[BCCSoCaDB1] [float] NULL,
	[BCCSoCaDB2] [float] NULL,
	[BCCSoCaHCCN] [float] NULL,
	[BCCSoCa1CN] [float] NULL,
	[BCCSoCa2CN] [float] NULL,
	[BCCSoCa3CN] [float] NULL,
	[BCCSoCaDB1CN] [float] NULL,
	[BCCSoCaDB2CN] [float] NULL,
	[BCCSoCaHCNL] [float] NULL,
	[BCCSoCa1NL] [float] NULL,
	[BCCSoCa2NL] [float] NULL,
	[BCCSoCa3NL] [float] NULL,
	[BCCSoCaDB1NL] [float] NULL,
	[BCCSoCaDB2NL] [float] NULL,
	[BCCLoai] [bit] NOT NULL
) ON [PRIMARY]

ALTER TABLE [dbo].[tblBangChamCong'+ CONVERT(VARCHAR(10),@Year)+'] ADD  CONSTRAINT [DF_tblBangChamCong'+ CONVERT(VARCHAR(10),@Year)+'_BCCLoai]  DEFAULT ((0)) FOR [BCCLoai]
END	
'
EXEC(@Check1)

GO
/****** Object:  StoredProcedure [dbo].[cp_CheckDiLam]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[cp_CheckDiLam]
@NVMa INT,
@Ngay DATETIME,
@Int INT OUTPUT
AS
SET @Int = 0

DECLARE @Nam INT
DECLARE @Thang VARCHAR(5)
SET @Nam = YEAR(@Ngay)
SET @Thang = Right('00'+ Convert(varchar(3),MONTH(@ngay)),2)

CREATE TABLE #Value (GiaiTri int)
DELETE FROM #Value
EXEC('Insert into #Value(GiaiTri) Select (Isnull(BCTGLamNgay,0) + Isnull(BCTGLamToi,0) + Isnull(BCTGThemNgay,0) + Isnull(BCTGThemToi,0)) AS GiaiTri from tblBaoCao'+ @Nam +'_'+ @Thang +' where BCMaNV = '+ @NVMa +' AND BCNgay =  ' + '''' +  @ngay + '''' + '  ')

SET @Int = ISNULL((SELECT * FROM #Value ),0)
IF @Int <> 0 SET @Int = 1

GO
/****** Object:  StoredProcedure [dbo].[cp_CheckRecord_tblBangChamCong_OutputBCCID]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[cp_CheckRecord_tblBangChamCong_OutputBCCID]
@Year INT,
@Month INT,
@MaNV INT,
@BCCID INT OUTPUT
AS
declare @Thang varchar(5)
set @Thang = RIGHT('00'+convert(varchar(5),@Month),2)
SET @BCCID = 0
DECLARE @strSql VARCHAR(500)
DECLARE @strCmd1 VARCHAR(500)

	SET @strSql='Select BCCID From tblBangChamCong'+ convert(varchar,@Year) +' Where BCCThang = '+ @Thang +' AND BCCMaNV = '+ convert(varchar,@MaNV) +' '
	select @strCmd1=' DECLARE sp_cursor CURSOR FOR ' + @strSql
	exec(@strcmd1)
	OPEN sp_cursor
	FETCH NEXT FROM sp_cursor into @BCCID
	IF(@@FETCH_STATUS <> 0)
	BEGIN
			EXEC ('
				insert into tblBangChamCong'+@Year +' ( BCCThang, BCCMaNV,BCCMaBP,BCCMaCV ) Select '+ @Thang +' ,'+ @MaNV +',NVMaBP,NVMaCV   From tblNhanVien where NVMa = '+ @MaNV +'
			')
	END
	
	CLOSE sp_cursor
	DEALLOCATE sp_cursor

	SET @strSql='Select BCCID From tblBangChamCong'+ convert(varchar,@Year) +' Where BCCThang = '+ @Thang +' AND BCCMaNV = '+ convert(varchar,@MaNV) +' '
	select @strCmd1=' DECLARE sp_cursor CURSOR FOR ' + @strSql
	exec(@strcmd1)
	OPEN sp_cursor
	FETCH NEXT FROM sp_cursor into @BCCID
	CLOSE sp_cursor
	DEALLOCATE sp_cursor

--SET @BCCID = ISNULL((SELECT * FROM #Value),0)



--IF @BCCID = 0 
--BEGIN
--	EXEC ('
--		insert into tblBangChamCong'+ convert(varchar,@Year) +' ( BCCThang, BCCMaNV,BCCMaBP,BCCMaCV ) Select '+ @Thang +' ,'+ @MaNV +',NVMaBP,NVMaCV   From tblNhanVien where NVMa = '+ @MaNV +'
--	')
--	DELETE FROM #Value
--	EXEC('Insert into #Value (GiaTri) Select BCCID From tblBangChamCong'+ convert(varchar,@Year) +' Where BCCThang = '+ @Thang +' AND BCCMaNV = '+ @MaNV +' ')
--	SET @BCCID = ISNULL((SELECT * FROM #Value),0)
--END



GO
/****** Object:  StoredProcedure [dbo].[cp_DeleteDKNghi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[cp_DeleteDKNghi]
@MaNV INT,
@NgayApDung DATETIME,
@NgayKetThuc DATETIME,
@DKNghiMaLoaiNghi INT
AS
DECLARE @Nam VARCHAR(5)

SET @Nam = CONVERT(VARCHAR(5),YEAR(@NgayApDung))
DECLARE @LoaiNghi INT
if EXISTS (SELECT * FROM tblLoaiNghi tln INNER JOIN tblLyDoNghi ld ON ld.LDNLoai = tln.LNMa WHERE tln.LNMa = 8 AND ld.LDNMa = @DKNghiMaLoaiNghi ) 
BEGIN
	EXEC ('DELETE FROM tblDangKyNghi'+ @Nam +' 
	       where DKNMaNV =  '+ @MaNV +'
	       AND DKNNgayApDung = ' + '''' +  @NgayApDung + '''' + '
	       AND DKNNgayKetThuc = ' + '''' +  @NgayKetThuc + '''' + '
	       DKNMaLyDo = '+ @DKNghiMaLoaiNghi +'
	')
	exec cp_InsertDangKyNghi @MaNV,@NgayApDung
END
ELSE
	BEGIN
				EXEC ('DELETE FROM tblDangKyNghi'+ @Nam +' 
					   where DKNMaNV =  '+ @MaNV +'
					   AND DKNNgayApDung = ' + '''' +  @NgayApDung + '''' + '
					   AND DKNNgayKetThuc = ' + '''' +  @NgayKetThuc + '''' + '
					   DKNMaLyDo = '+ @DKNghiMaLoaiNghi +'
						')
	END
GO
/****** Object:  StoredProcedure [dbo].[cp_InsertDangKyNghi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[cp_InsertDangKyNghi]
@MaNV INT,
@NgayApDung DATETIME
AS
DECLARE @NamApDung VARCHAR(5)
SET @NamApDung =  CONVERT(VARCHAR(5),YEAR(@NgayApDung))
--CREATE TABLE #Value (NgayNghiBu SMALLDATETIME) 
DECLARE @NgayNghiBu DATETIME

DECLARE @MaLoaiNB TINYINT

SELECT @MaLoaiNB=tln.LNMa FROM tblLoaiNghi tln WHERE tln.LNUuTien=8

	DECLARE @strSql VARCHAR(500)
	DECLARE @strCmd1 VARCHAR(500)
	SET @strSql='Select DATEADD(DAY,DKNNghiBu,DKNNgayApDung) From tblDangKyNghi'+ @NamApDung +' where DKNMaNV = '+ convert(varchar,@MaNV) +' AND DKNNghiBu <> 0 AND DKNNgayApDung = ' + '''' +  convert(varchar,@NgayApDung) + '''' + ' '
	select @strCmd1=' DECLARE sp_cursor CURSOR FOR ' + @strSql
	exec(@strcmd1)
	OPEN sp_cursor
	FETCH NEXT FROM sp_cursor into @NgayNghiBu
	IF(@@FETCH_STATUS =0)
	BEGIN
		IF (@NgayNghiBu IS NULL)
		BEGIN
			set @NgayNghiBu = @NgayApDung
		END
	END
	CLOSE sp_cursor
	DEALLOCATE sp_cursor


--SET @NgayNghiBu =  ISNULL((SELECT * FROM #Value),'1900-01-01')  
--if (@NgayNghiBu = '1900-01-01' )
--BEGIN
--	set @NgayNghiBu = @NgayApDung
--END

CREATE TABLE #NghiBu (MaNV INT,NgayNghiBu DATETIME,HeSo FLOAT,LNMa INT,ThuViec INT)

IF @NgayNghiBu <> '1900-01-01'
BEGIN
	DECLARE @StartDate DATETIME
	DECLARE @FinishDate DATETIME
	exec cp_LayStartDate_FinishDate_OfBCC @NgayNghiBu, @StartDate OUTPUT, @FinishDate OUTPUT 
	DECLARE @NAME VARCHAR(20)
	DECLARE @Nam VARCHAR(20)
	DECLARE CurNam CURSOR FOR
		select NAME,RIGHT(NAME,4) AS Nam
		from dbo.sysobjects 
		where LEFT(NAME,13) =  'tblDangKyNghi' 
		and OBJECTPROPERTY(id, N'IsUserTable') = 1
		AND LEN([name]) = 17
		ORDER BY RIGHT(NAME,4) ASC
	OPEN CurNam
	FETCH NEXT FROM CurNam INTO @NAME,@Nam
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC ( ' 
			Insert into #NghiBu (MaNV,NgayNghiBu,HeSo,LNMa)
			Select dkn.DKNMaNV,Dateadd(day,DKNNghiBu,DKNNgayApDung),Case DKNLoai when 0 then 1 else 0.5 end AS HeSo,loai.LNMa
			From '+ @NAME +' dkn
			inner join tblLyDoNghi lydo On dkn.DKNMaLyDo = lydo.LDNMa
			Inner join tblLoaiNghi loai ON loai.LNMa = lydo.LDNLoai
			where loai.LNLoai = 8
			AND dkn.DKNMaNV = '+ @MaNV +'
			AND dkn.DKNNghiBu <> 0
			AND Dateadd(day,DKNNghiBu,DKNNgayApDung) BETWEEN ' + '''' +  @StartDate + '''' + '  AND  ' + '''' +  @FinishDate + '''' + ' 
		')
		FETCH NEXT FROM CurNam INTO @NAME,@Nam
	END
	CLOSE CurNam
	DEALLOCATE CurNam
	-- Update trang thai thu viec
	--lay ngay thu viec
	DECLARE @NgayThuViec DATETIME
	DECLARE @NgayKetThucThuViec DATETIME
	SET @NgayThuViec  = '1900-01-01'
	SET @NgayKetThucThuViec  = '1900-01-01'
	SELECT @NgayThuViec = thdn.HDTuNgay,@NgayKetThucThuViec = thdn.HDDenNgay 
	FROM tblHopDongNV thdn WHERE thdn.HDLoaiHD = 2 AND thdn.HDMaNV = @MaNV
	
	IF datediff(day,@NgayKetThucThuViec,'1900-01-01') = 0
	begin	
		UPDATE #NghiBu
		SET
			ThuViec = 1
	end
	ELSE
		BEGIN
			UPDATE #NghiBu
			SET
				ThuViec = 0
			WHERE NgayNghiBu BETWEEN @NgayThuViec AND @NgayKetThucThuViec
		
		
			UPDATE #NghiBu
			SET
				ThuViec = 1
			WHERE NgayNghiBu Not BETWEEN @NgayThuViec AND @NgayKetThucThuViec
		END

--Check tao bang hay chua
	DECLARE @Thang INT
	SET @Thang = MONTH(@FinishDate)
	SET @Nam = YEAR(@FinishDate)
	exec cp_CheckAndCreateTable_tblBangChamCong @Nam
	exec cp_CheckAndCreateTable_TBangChamCong_LoaiNghi @Nam

	DECLARE @BCC INT
	--SET @BCC=26
	exec cp_CheckRecord_tblBangChamCong_OutputBCCID @Nam,@Thang,@MaNV,@BCC OUTPUT
	--PRINT @BCC
	DECLARE @SQL VARCHAR(8000)
	SET @SQL = 'IF EXISTS(Select LNMa From TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +' where BCCID = '+ Convert(varchar(5),@BCC) +' AND  LNMa = ' + Convert(varchar(5),@MaLoaiNB) +')
				BEGIN
					 IF not EXISTS(Select LNMa  From #NghiBu where  LNMa = ' + Convert(varchar(5),@MaLoaiNB) +')
					 BEGIN
						Update TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +'
						SET SoNgay_TV = 0,SoNgay_CT=0
						From TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +' ln
						Where   ln.LNMa = ' + Convert(varchar(5),@MaLoaiNB) +' AND  ln.BCCID = '+ Convert(varchar(5),@BCC) +'
					 END
					
					Update TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +'
					SET SoNgay_CT = Isnull(t.HeSo,0)
					From TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +' ln
					Inner Join (Select LNMa,Sum(HeSo) AS HeSo From #NghiBu where ThuViec = 1 Group By LNMa  ) t On ln.LNMa = t.LNMa
					Where   ln.LNMa = ' + Convert(varchar(5),@MaLoaiNB) +' AND  ln.BCCID = '+ Convert(varchar(5),@BCC) +'
					
					Update TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +'
					SET SoNgay_TV = Isnull(t.HeSo,0)
					From TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +' ln
					Inner Join (Select LNMa,Sum(HeSo) AS HeSo From #NghiBu where ThuViec = 0 Group By LNMa  ) t On ln.LNMa = t.LNMa
					Where    ln.LNMa = ' + Convert(varchar(5),@MaLoaiNB) +' AND   ln.BCCID = '+ Convert(varchar(5),@BCC) +'
					
				END
				Else
					BEGIN
						Insert into TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +' ( BCCID,LNMa) values( '+ Convert(varchar(5),@BCC) +',' + Convert(varchar(5),@MaLoaiNB) +')
						
						Update TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +'
						SET SoNgay_CT = Isnull(t.HeSo,0)
						From TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +' ln
						Inner Join (Select LNMa,Sum(HeSo) AS HeSo From #NghiBu where ThuViec = 1 Group By LNMa  ) t On ln.LNMa = t.LNMa
						Where   ln.LNMa = ' + Convert(varchar(5),@MaLoaiNB) +' AND ln.BCCID = '+ Convert(varchar(5),@BCC) +'
						
						Update TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +'
						SET SoNgay_TV = Isnull(t.HeSo,0)
						From TBangChamCong_LoaiNghi'+ Convert(varchar(5),@Nam) +' ln
						Inner Join (Select LNMa,Sum(HeSo) AS HeSo From #NghiBu where ThuViec = 0 Group By LNMa  ) t On ln.LNMa = t.LNMa
						Where    ln.LNMa = ' + Convert(varchar(5),@MaLoaiNB) +' AND ln.BCCID = '+ Convert(varchar(5),@BCC) +'  
					END
					Select * From #NghiBu
	 '
	 
	EXEC(@SQL)
	
END		


GO
/****** Object:  StoredProcedure [dbo].[cp_InsertDKNghi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[cp_InsertDKNghi]
@MaNV INT,
@NgayApDung DATETIME,
@NgayKetThuc DATETIME,
@DKNghiMaLoaiNghi INT,
@DKNghiLoai FLOAT,
@SoNgayNghiBu int
AS
DECLARE @Nam VARCHAR(5)

SET @Nam = CONVERT(VARCHAR(5),YEAR(@NgayApDung))
DECLARE @LoaiNghi INT
if EXISTS (SELECT * FROM tblLoaiNghi tln INNER JOIN tblLyDoNghi ld ON ld.LDNLoai = tln.LNMa WHERE tln.LNMa = 8 AND ld.LDNMa = @DKNghiMaLoaiNghi ) 
BEGIN
	--EXEC ('INSERT INTO tblDangKyNghi'+ @Nam +' (DKNMaNV, DKNNgayApDung, DKNNgayKetThuc, DKNMaLyDo,
	--       DKNLoai, DKNNghiBu) Values ( '+ @MaNV +',' + '''' +  @NgayApDung + '''' + ',' + '''' +  @NgayKetThuc + '''' + ','+ @DKNghiMaLoaiNghi +',
	--       '+ @DKNghiLoai +' ,'+ @SoNgayNghiBu +' )
	--')
	exec cp_InsertDangKyNghi @MaNV,@NgayApDung
END
--ELSE
--	BEGIN
--			EXEC ('INSERT INTO tblDangKyNghi'+ @Nam +' (DKNMaNV, DKNNgayApDung, DKNNgayKetThuc, DKNMaLyDo,
--				   DKNLoai, DKNNghiBu) Values ( '+ @MaNV +',' + '''' +  @NgayApDung + '''' + ',' + '''' +  @NgayKetThuc + '''' + ','+ @DKNghiMaLoaiNghi +',
--				   '+ @DKNghiLoai +' ,'+ @SoNgayNghiBu +' )
--					')
--	END

GO
/****** Object:  StoredProcedure [dbo].[cp_QuetTheLoi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[cp_QuetTheLoi]
@BPMa INT,
@NVMa INT,
@StartDate DATETIME,
@FinishDate DATETIME
AS
Delete from RecordDataQuetTheLoi
DECLARE @SQL VARCHAR(8000)

IF @NVMa <> 0
BEGIN
	SET @SQL  = '	declare @StartDate smalldatetime
	declare @FinishDate smalldatetime
	Declare @SID int
	
	Set @StartDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@StartDate)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@StartDate)),2)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@StartDate)),2)) +') ) 
					
	Set @FinishDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@FinishDate)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@FinishDate)),2)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@FinishDate)),2)) +')	)
	
	while @StartDate < = @FinishDate
	BEGIN
		exec cp_QuetTheLoi_Children '+ CONVERT(VARCHAR(10),@NVMa) +',@StartDate
		set @StartDate = dateadd(day,1,@StartDate)
	END
	'
	EXEC(@SQL)				
END
ELSE 
	BEGIN
			SET @SQL  = '	declare @StartDate smalldatetime
							declare @FinishDate smalldatetime
				Declare @SID int
				
				Set @StartDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@StartDate)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@StartDate)),2)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@StartDate)),2)) +') ) 
								
				Set @FinishDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@FinishDate)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@FinishDate)),2)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@FinishDate)),2)) +')	)
				declare @NVMa int
				declare @S smalldatetime
				declare @F smalldatetime
				
				DECLARE curr CURSOR FOR
					SELECT distinct NVMa
					FROM tblNhanVien nv
					INNER JOIN tblBoPhan tbp ON nv.NVMaBP = tbp.BPMa
					WHERE tbp.BPMa IN ( '+dbo.DonVi_IDs(@BPMa) +')
				OPEN curr
				FETCH NEXT FROM curr INTO @NVMa
						
				WHILE @@FETCH_STATUS = 0
				begin
						Set @S = @StartDate
						set @F = @FinishDate
						while @S < = @F
						BEGIN
							exec cp_QuetTheLoi_Children @NVMa,@S
							set @S = dateadd(day,1,@S)
						END
					FETCH NEXT FROM curr INTO @NVMa
				END
				CLOSE curr
				DEALLOCATE curr
	'
	EXEC(@SQL)
	END

	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblquettheloi]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[tblquettheloi]
	SELECT	nv.NVMaNV,
			nv.NVHoTen,
			bp.BPMa,
			bp.BPTen,
			r.ThoiGian,
			
			CASE r.Status WHEN 1 THEN 'In' ELSE 'Out' END AS InOut
	into tblquettheloi
	FROM tblNhanVien nv
	INNER JOIN tblCapThe ct ON ct.CTMaNV = nv.NVMa
	INNER JOIN tblBoPhan bp ON bp.BPMa = nv.NVMaBP
	INNER JOIN RecordDataQuetTheLoi r ON r.IDCard = ct.CTMaThe

GO
/****** Object:  StoredProcedure [dbo].[cp_QuetTheLoi_Children]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[cp_QuetTheLoi_Children]
@StaffID int,
@D DATETIME
 as
	Begin
	DECLARE @TGBDCa Datetime 		
	DECLARE @TGKTCa Datetime 
	DECLARE @LoaiCa smallint	
	DECLARE @TGLayDLDau  Datetime 	
	DECLARE @TGLayDLCuoi Datetime 
	
	DECLARE @QuetTruocCa int
	DECLARE @QuetSauCa int
	
	-- Khai bao ten cac bang
	DECLARE @RecordData nvarchar(20) 	
	DECLARE @tblBaocao nvarchar(20) 	
	DECLARE @tblDangkynghi nvarchar(20) 	
	DECLARE @tblDangkyuudai nvarchar(20) 	
	
	-- Khai bao bien loc trang thai
	DECLARE @IDM smallint
	DECLARE @ThoiGian  Datetime 
	DECLARE @IDCard nvarchar(15)
	DECLARE @Status bit
	DECLARE @HandData bit
	DECLARE @Pass bit

    -- Thoi gian den, cua den
	--DECLARE	@TGVe Datetime
	--DECLARE	@CuaVe smallint
	-- Thoi gian vao, cua vao, thoi gian ra, cua ra
	--DECLARE	@TGVao Datetime
	--DECLARE	@CuaVao smallint
	--DECLARE	@TGRa Datetime
	--DECLARE	@CuaRa smallint
    -- Khai bao loai nghi, Ma ly do nghi, Ghi chu, 
    DECLARE @LoaiNghi smallint
    DECLARE @MaLyDoNghi smallint
    DECLARE @LydonghiViettat nvarchar(10)
    
    DECLARE @Ghichu nvarchar(50)
    
	-- Bien tam dem tong so ban ghi tra ve
	DECLARE	@recordCount smallint
	-- Luu kieu xac dinh trang thai vao ra
	DECLARE @OriginalStatus smallint
	-- Kieu tinh gio
	DECLARE @TypeCalculator  smallint
	-- Bien luu trang thai tam khi loc du lieu
	DECLARE @OldStatus bit
	-- Khai bao cac bien dang bao cao
	DECLARE	@Maca int
	-- Khai bao bien ma the cua nhan vien
	DECLARE @MaThe nvarchar(14) 
	-- Khai bao loai tblbaocao
	DECLARE @BCLoai bit
	
	DECLARE	@i int
	
	DECLARE @SQL nvarchar(4000) 	
	DECLARE @SQL1 nvarchar(4000) 	
	DECLARE @SQL2 nvarchar(4000) 	
	DECLARE @SQL3 nvarchar(4000) 	
	DECLARE @SQL4 nvarchar(4000) 	

	
	-- Lay cac tham so cham cong
	-- Cach lay du lieu quet: Xem ke trang thai hoac lay tu dau doc  (1: Xen ke, 0: dau doc)

	SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='ORIGINALSTATUS'
	SET @OriginalStatus=CONVERT(int,@SQL)
	
	SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='TYPECALCULATOR'
	SET @TypeCalculator=CONVERT(int,@SQL)
	
	-- Thiet lap ten bao cao
	SET @RecordData = case when LEN(Month(@D))=2 then 
	 'RecordData' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	 else
	 'RecordData' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	 end
	 
	SET @tblBaocao = case when LEN(Month(@D))=2 then 
	 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	 else
	 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	END
	
	SET @tblDangkynghi = 'tblDangkynghi' + CONVERT(nvarchar,YEAR(@D)) 
	SET @tblDangkyuudai = 'tblDangkyUuDai' + CONVERT(nvarchar,YEAR(@D)) 
	
	 	 
	 	-- Mo bang tblBaocao lay ca thong tin lien quan
	Create TABLE #tblBaocaotam ([BCmaca][int],[BCLoai] [bit] )
	EXEC ('Insert INTO #tblBaocaotam(bcmaca,BCLoai) Select bcmaca,BCloai  From  ' + @tblBaocao + ' WHERE (BCMaNV=' + @StaffID +') AND (BCNgay=' + '''' + @D + '''' + ')')
	select @Maca = BCmaca from #tblBaocaotam
	select @BCloai = BCloai from #tblBaocaotam
	
	if (@Maca is null)
		BEGIN
			-- Neu khong ton tai nhan vien trong bang bao cao thi khoi tao lai
			GOTO EndThisSub
		end
	
	-- Kiem tra xem nhan vien co dang ky nghi khong
	
		-- Tao bang dang ky nghi tam
		CREATE TABLE [dbo].#tblDangkynghiTam ([DKNMaNV] [int] NOT NULL ,[DKNNgayApDung] [datetime] NOT NULL ,[DKNNgayKetThuc] [datetime] NOT NULL ,[DKNMaLyDo] [int] NOT NULL ,[DKNLoai] [tinyint] NOT NULL) ON [PRIMARY]
		
		SET @SQL='Insert into #tblDangkynghiTam(DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai) SELECT DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai FROM '+ @tblDangkynghi+ ' WHERE (DKNMaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKNNgayApDung AND DKNNgayKetThuc) '
		EXEC (@SQL)
		
		DECLARE curRS 
		CURSOR FOR 
			SELECT DKNLoai,DKNMaLyDo  FROM #tblDangkynghiTam 
		OPEN curRS

		FETCH NEXT FROM curRS INTO @LoaiNghi,@MaLyDoNghi
		if @@FETCH_STATUS = 0
			BEGIN
				SET @LoaiNghi=@LoaiNghi+1
				SELECT @LydonghiViettat= LDNVietTat FROM tblLyDoNghi WHERE LDNMa=@MaLyDoNghi			
				IF @LydonghiViettat IS NOT NULL 
						SET @Ghichu=CASE WHEN @LoaiNghi<>1 THEN '1/2' + CONVERT(nvarchar(30),@LydonghiViettat) ELSE CONVERT(nvarchar(30),@LydonghiViettat) end
				FETCH NEXT FROM curRS INTO @LoaiNghi,@MaLyDoNghi
			end
		-- Neu loai nghi =1 thi cap nhat cac gia tri ve mac dinh
		IF @LoaiNghi=1
			BEGIN
				GOTO EndThisSub
			END
		CLOSE curRS
		DEALLOCATE curRS
			
		--Kiem tra neu cham cong bang tay thi thoat
		IF @BCloai=1 GOTO Endthissub

	DECLARE curRS 
	CURSOR FOR 
		SELECT CTGBatDau,CTGKetThuc,CQuetTruocCa,CQuetSauCa,CLoaiCa FROM tblCa WHERE CMa=@Maca
	OPEN curRS

	FETCH NEXT FROM curRS INTO @TGBDCa,@TGKTCa,@QuetTruocCa,@QuetSauCa,@LoaiCa
	if @@FETCH_STATUS = 0
		BEGIN
			--Lay thong tin tu bang ca
			set @TGBDCa=@D + @TGBDCa
			set @TGKTCa=@TGBDCa + @TGKTCa			
			set @TGLayDLDau= DateAdd(minute, -@QuetTruocCa, @TGBDCa)
			set @TGLayDLCuoi=DateAdd(minute, @QuetSauCa, @TGKTCa)
		ENd
		CLOSE curRS
	DEALLOCATE curRS
	
    -- Lay ma the
   
    SELECT @MaThe= CTMaThe FROM tblCapThe WHERE (CTMaNV=@StaffID) AND (@D BETWEEN CTNgayApDung AND CTNgayKetThuc) ORDER BY CTNgayKetThuc DESC
    
    IF (@MaThe IS NULL) 
    BEGIN
    	GoTo EndThisSub
    END 
    
    
	-- Loc du lieu	
	DECLARE @in INT
	DECLARE @Out int
	
	CREATE TABLE #value (GiaTri int)
	DELETE FROM #Value
	EXEC('INSERT INTO #Value SELECT Count(*) FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  AND Status = 1 ')
	SET @in =  isnull((SELECT * FROM #value),0)
	DELETE FROM #Value
	EXEC('INSERT INTO #Value SELECT Count(*) FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  AND Status = 0 ')
	SET @Out =  isnull((SELECT * FROM #value),0)
	
	Set @recordCount =  @in + @Out

	IF @recordCount > 0
	BEGIN
		IF (@recordCount - (@recordCount/2)*2) = 1
		BEGIN
			EXEC('INSERT INTO RecordDataQuetTheLoi(IDM,IDCard,ThoiGian,Status,HandData,Pass) SELECT DISTINCT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  ORDER BY ThoiGian')
		END
		ELSE
			BEGIN
				IF @in <> @Out
					EXEC('INSERT INTO RecordDataQuetTheLoi(IDM,IDCard,ThoiGian,Status,HandData,Pass) SELECT DISTINCT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  ORDER BY ThoiGian')
			END
	END
	 
EndThisSub:

end


GO
/****** Object:  StoredProcedure [dbo].[cp_Report_InOut]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[cp_Report_InOut]
@BPMa INT,
@NVMa INT,
@StartDate DATETIME,
@FinishDate DATETIME
AS
Delete from RecordDataInOutThangtx
DECLARE @SQL VARCHAR(8000)

IF @NVMa <> 0
BEGIN
	SET @SQL  = '	declare @StartDate smalldatetime
	declare @FinishDate smalldatetime
	Declare @SID int
	
	Set @StartDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@StartDate)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@StartDate)),2)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@StartDate)),2)) +') ) 
					
	Set @FinishDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@FinishDate)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@FinishDate)),2)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@FinishDate)),2)) +')	)
	
	while @StartDate < = @FinishDate
	BEGIN
		exec cp_QuetTheLoi_Children '+ CONVERT(VARCHAR(10),@NVMa) +',@StartDate
		set @StartDate = dateadd(day,1,@StartDate)
	END
	'
	EXEC(@SQL)				
END
ELSE 
	BEGIN
			SET @SQL  = '	declare @StartDate smalldatetime
							declare @FinishDate smalldatetime
				Declare @SID int
				
				Set @StartDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@StartDate)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@StartDate)),2)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@StartDate)),2)) +') ) 
								
				Set @FinishDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@FinishDate)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@FinishDate)),2)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@FinishDate)),2)) +')	)
				declare @NVMa int
				declare @S smalldatetime
				declare @F smalldatetime
				
				DECLARE curr CURSOR FOR
					SELECT distinct NVMa
					FROM tblNhanVien nv
					INNER JOIN tblBoPhan tbp ON nv.NVMaBP = tbp.BPMa
					WHERE tbp.BPMa IN ( '+dbo.DonVi_IDs(@BPMa) +')
				OPEN curr
				FETCH NEXT FROM curr INTO @NVMa
						
				WHILE @@FETCH_STATUS = 0
				begin
						Set @S = @StartDate
						set @F = @FinishDate
						while @S < = @F
						BEGIN
							exec cp_Report_InOut_Children @NVMa,@S
							set @S = dateadd(day,1,@S)
						END
					FETCH NEXT FROM curr INTO @NVMa
				END
				CLOSE curr
				DEALLOCATE curr
	'
	EXEC(@SQL)
	END

	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblInOutThangtx]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[tblInOutThangtx]
	SELECT	
			nv.NVMaNV,
			nv.NVHoTen,
			bp.BPMa,
			bp.BPTen,
			r.Ngay,
			r.ThoiGian,
			r.Status,
			CASE r.Status WHEN 1 THEN 'In' ELSE 'Out' END AS InOut
	into tblInOutThangtx
	FROM tblNhanVien nv
	INNER JOIN tblCapThe ct ON ct.CTMaNV = nv.NVMa
	INNER JOIN tblBoPhan bp ON bp.BPMa = nv.NVMaBP
	INNER JOIN RecordDataInOutThangtx r ON r.IDCard = ct.CTMaThe
	Inner join tblCa c On r.MaCa = c.CMa

GO
/****** Object:  StoredProcedure [dbo].[cp_Report_InOut_Children]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create  PROCEDURE [dbo].[cp_Report_InOut_Children]
@StaffID int,
@D DATETIME
 as
	Begin
	DECLARE @TGBDCa Datetime 		
	DECLARE @TGKTCa Datetime 
	DECLARE @LoaiCa smallint	
	DECLARE @TGLayDLDau  Datetime 	
	DECLARE @TGLayDLCuoi Datetime 
	
	DECLARE @QuetTruocCa int
	DECLARE @QuetSauCa int
	
	-- Khai bao ten cac bang
	DECLARE @RecordData nvarchar(20) 	
	DECLARE @tblBaocao nvarchar(20) 	
	DECLARE @tblDangkynghi nvarchar(20) 	
	DECLARE @tblDangkyuudai nvarchar(20) 	
	
	-- Khai bao bien loc trang thai
	DECLARE @IDM smallint
	DECLARE @ThoiGian  Datetime 
	DECLARE @IDCard nvarchar(15)
	DECLARE @Status bit
	DECLARE @HandData bit
	DECLARE @Pass bit

    -- Thoi gian den, cua den
	--DECLARE	@TGVe Datetime
	--DECLARE	@CuaVe smallint
	-- Thoi gian vao, cua vao, thoi gian ra, cua ra
	--DECLARE	@TGVao Datetime
	--DECLARE	@CuaVao smallint
	--DECLARE	@TGRa Datetime
	--DECLARE	@CuaRa smallint
    -- Khai bao loai nghi, Ma ly do nghi, Ghi chu, 
    DECLARE @LoaiNghi smallint
    DECLARE @MaLyDoNghi smallint
    DECLARE @LydonghiViettat nvarchar(10)
    
    DECLARE @Ghichu nvarchar(50)
    
	-- Bien tam dem tong so ban ghi tra ve
	DECLARE	@recordCount smallint
	-- Luu kieu xac dinh trang thai vao ra
	DECLARE @OriginalStatus smallint
	-- Kieu tinh gio
	DECLARE @TypeCalculator  smallint
	-- Bien luu trang thai tam khi loc du lieu
	DECLARE @OldStatus bit
	-- Khai bao cac bien dang bao cao
	DECLARE	@Maca int
	-- Khai bao bien ma the cua nhan vien
	DECLARE @MaThe nvarchar(14) 
	-- Khai bao loai tblbaocao
	DECLARE @BCLoai bit
	
	DECLARE	@i int
	
	DECLARE @SQL nvarchar(4000) 	
	DECLARE @SQL1 nvarchar(4000) 	
	DECLARE @SQL2 nvarchar(4000) 	
	DECLARE @SQL3 nvarchar(4000) 	
	DECLARE @SQL4 nvarchar(4000) 	

	
	-- Lay cac tham so cham cong
	-- Cach lay du lieu quet: Xem ke trang thai hoac lay tu dau doc  (1: Xen ke, 0: dau doc)

	SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='ORIGINALSTATUS'
	SET @OriginalStatus=CONVERT(int,@SQL)
	
	SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='TYPECALCULATOR'
	SET @TypeCalculator=CONVERT(int,@SQL)
	
	-- Thiet lap ten bao cao
	SET @RecordData = case when LEN(Month(@D))=2 then 
	 'RecordData' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	 else
	 'RecordData' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	 end
	 
	SET @tblBaocao = case when LEN(Month(@D))=2 then 
	 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	 else
	 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	END
	
	SET @tblDangkynghi = 'tblDangkynghi' + CONVERT(nvarchar,YEAR(@D)) 
	SET @tblDangkyuudai = 'tblDangkyUuDai' + CONVERT(nvarchar,YEAR(@D)) 
	
	 	 
	 	-- Mo bang tblBaocao lay ca thong tin lien quan
	Create TABLE #tblBaocaotam ([BCmaca][int],[BCLoai] [bit] )
	EXEC ('Insert INTO #tblBaocaotam(bcmaca,BCLoai) Select bcmaca,BCloai  From  ' + @tblBaocao + ' WHERE (BCMaNV=' + @StaffID +') AND (BCNgay=' + '''' + @D + '''' + ')')
	select @Maca = BCmaca from #tblBaocaotam
	select @BCloai = BCloai from #tblBaocaotam
	
	if (@Maca is null)
		BEGIN
			-- Neu khong ton tai nhan vien trong bang bao cao thi khoi tao lai
			GOTO EndThisSub
		end
	
	-- Kiem tra xem nhan vien co dang ky nghi khong
	
		-- Tao bang dang ky nghi tam
		CREATE TABLE [dbo].#tblDangkynghiTam ([DKNMaNV] [int] NOT NULL ,[DKNNgayApDung] [datetime] NOT NULL ,[DKNNgayKetThuc] [datetime] NOT NULL ,[DKNMaLyDo] [int] NOT NULL ,[DKNLoai] [tinyint] NOT NULL) ON [PRIMARY]
		
		SET @SQL='Insert into #tblDangkynghiTam(DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai) SELECT DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai FROM '+ @tblDangkynghi+ ' WHERE (DKNMaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKNNgayApDung AND DKNNgayKetThuc) '
		EXEC (@SQL)
		
		DECLARE curRS 
		CURSOR FOR 
			SELECT DKNLoai,DKNMaLyDo  FROM #tblDangkynghiTam 
		OPEN curRS

		FETCH NEXT FROM curRS INTO @LoaiNghi,@MaLyDoNghi
		if @@FETCH_STATUS = 0
			BEGIN
				SET @LoaiNghi=@LoaiNghi+1
				SELECT @LydonghiViettat= LDNVietTat FROM tblLyDoNghi WHERE LDNMa=@MaLyDoNghi			
				IF @LydonghiViettat IS NOT NULL 
						SET @Ghichu=CASE WHEN @LoaiNghi<>1 THEN '1/2' + CONVERT(nvarchar(30),@LydonghiViettat) ELSE CONVERT(nvarchar(30),@LydonghiViettat) end
				FETCH NEXT FROM curRS INTO @LoaiNghi,@MaLyDoNghi
			end
		-- Neu loai nghi =1 thi cap nhat cac gia tri ve mac dinh
		IF @LoaiNghi=1
			BEGIN
				GOTO EndThisSub
			END
		CLOSE curRS
		DEALLOCATE curRS
			
		--Kiem tra neu cham cong bang tay thi thoat
		IF @BCloai=1 GOTO Endthissub

	DECLARE curRS 
	CURSOR FOR 
		SELECT CTGBatDau,CTGKetThuc,CQuetTruocCa,CQuetSauCa,CLoaiCa FROM tblCa WHERE CMa=@Maca
	OPEN curRS

	FETCH NEXT FROM curRS INTO @TGBDCa,@TGKTCa,@QuetTruocCa,@QuetSauCa,@LoaiCa
	if @@FETCH_STATUS = 0
		BEGIN
			--Lay thong tin tu bang ca
			set @TGBDCa=@D + @TGBDCa
			set @TGKTCa=@TGBDCa + @TGKTCa			
			set @TGLayDLDau= DateAdd(minute, -@QuetTruocCa, @TGBDCa)
			set @TGLayDLCuoi=DateAdd(minute, @QuetSauCa, @TGKTCa)
		ENd
		CLOSE curRS
	DEALLOCATE curRS
	
    -- Lay ma the
   
    SELECT @MaThe= CTMaThe FROM tblCapThe WHERE (CTMaNV=@StaffID) AND (@D BETWEEN CTNgayApDung AND CTNgayKetThuc) ORDER BY CTNgayKetThuc DESC
    
    IF (@MaThe IS NULL) 
    BEGIN
    	GoTo EndThisSub
    END 
	-- Loc du lieu	
	EXEC('INSERT INTO RecordDataInOutThangtx(MaCa,Ngay,IDM,IDCard,ThoiGian,Status,HandData,Pass) SELECT DISTINCT '+ @Maca +','+ '''' + @D + '''' + ',IDM,IDCard,ThoiGian,Status,HandData,Pass FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  ORDER BY ThoiGian')
	
	 
EndThisSub:

end





GO
/****** Object:  StoredProcedure [dbo].[cp_rpt_Baocaovaora]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[cp_rpt_Baocaovaora]
@MaNhanVien INT,
@MaPhongBan INT,
@TuNgay SMALLDATETIME,
@DenNgay SMALLDATETIME

AS

DECLARE @XauChuanI VARCHAR(8000)
DECLARE @XauChuanII VARCHAR(8000)
DECLARE @XauChuanIIII VARCHAR(8000)

DECLARE @bitMaNhanVien int

IF @MaNhanVien = -1
BEGIN
	SET @bitMaNhanVien = 1
END
ELSE
	BEGIN
		set @bitMaNhanVien = 0
	END 


DECLARE @ADen INT
DECLARE @BDen INT
DECLARE @CDen INT

DECLARE @ADi INT
DECLARE @BDi INT
DECLARE @CDi INT 
DECLARE @GachNgang VARCHAR(5)
DECLARE @OO VARCHAR(5)

set @ADen= convert(varchar(5),year(@TuNgay))
set @BDen = convert(varchar(5),month(@TuNgay))
set @CDen = convert(varchar(5),day(@TuNgay))


set @ADi= convert(varchar(5),year(@DenNgay))
set @BDi = convert(varchar(5),month(@DenNgay))
set @CDi = convert(varchar(5),day(@DenNgay))
SET @GachNgang = '-'
SET @OO = '00'
	
set @XauChuanI = 'declare @TempDenNgay smalldatetime
					declare @TempTuNgay smalldatetime	
					declare @bitMaNhanVien1 int	
					SET @bitMaNhanVien1 = '+ Convert(VARCHAR(10), @bitMaNhanVien) +'
					
		set  @TempTuNgay = convert(varchar(5), '+ convert(varchar(5),@ADen) +') + Right(convert(varchar(2),'+ Convert(varchar(5),@OO) +') + Convert(varchar(5),'+ convert(varchar(5),@BDen) +'),2) + Right(Convert(varchar(2),'+ Convert(varchar(5),@OO) +') + Convert(varchar(5),'+ Convert(varchar(5),@CDen) +') ,2)  '
		
set @XauChuanI = '		
		set @TempDenNgay = convert(varchar(5), '+ convert(varchar(5),@Adi) +') + Right(convert(varchar(2),'+ Convert(varchar(5),@OO) +') + Convert(varchar(5),'+ convert(varchar(5),@BDi) +'),2) + Right(Convert(varchar(2),'+ Convert(varchar(5),@OO) +') + Convert(varchar(5),'+ Convert(varchar(5),@CDi) +') ,2)    
			Declare @BangTam table 
			 (
				[IDM] [tinyint] NOT NULL ,
				[IDCard] [nvarchar] (14) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
				[ThoiGian] [datetime] NOT NULL ,
				[Status] [bit] NOT NULL ,
				[HandData] [bit] NOT NULL ,
				[Pass] [bit] NOT NULL 
			) ON [PRIMARY]
'

DECLARE @TThang INT
DECLARE @TNam INT
DECLARE @X INT
DECLARE @i INT
SET @TThang = MONTH(@TuNgay)
SET @TNam = YEAR(@TuNgay)
SET @X = DATEDIFF(MONTH,@TuNgay,@DenNgay)
SET @i = 0
SET @XauChuanIIII = ' '
WHILE @i <= @X 
BEGIN
	SET @XauChuanIIII = @XauChuanIIII +'
		WHILE DATEDIFF(DAY,@TempTuNgay,@TempDenNgay) >=0
		BEGIN
		IF @bitMaNhanVien1 = 1
		BEGIN
		INSERT  @BangTam
		(
			NVMa, 
			HoTen, 
			MaBP, 
			TenBP, 
			Ngay, 
			GioVao,
			IDCard
		)
		SELECT distinct 
			tnv.NVMa,
			tnv.NVHoTen,
			tbp.BPMa,
			tbp.BPTenV,
			@TempTuNgay,
			CONVERT(CHAR(8),rd.ThoiGian,108),
			rd.IDCard
		FROM tblNhanVien tnv
			INNER JOIN tblBoPhan tbp ON tnv.NVMaBP = tbp.BPMa
			INNER JOIN tblCapThe tct ON tct.CTMaNV = tnv.NVMa
			INNER JOIN RecordData'+ CONVERT(VARCHAR(5),@TNam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@TThang),2),3) +' rd ON rd.IDCard = tct.CTMaThe
		WHERE  DATEDIFF(DAY,rd.ThoiGian,@TempTuNgay) = 0
			AND rd.[Status] = 1
			AND tbp.BPMa IN ( '+dbo.DonVi_IDs(@MaPhongBan) +')
		END
		Else
			BEGIN
				INSERT  @BangTam
				(
					NVMa, 
					HoTen, 
					MaBP, 
					TenBP, 
					Ngay, 
					GioVao,
					IDCard
				)
				SELECT distinct
					tnv.NVMa,
					tnv.NVHoTen,
					tbp.BPMa,
					tbp.BPTenV,
					@TempTuNgay,
					CONVERT(CHAR(8),rd.ThoiGian,108),
					rd.IDCard
				FROM tblNhanVien tnv
					INNER JOIN tblBoPhan tbp ON tnv.NVMaBP = tbp.BPMa
					INNER JOIN tblCapThe tct ON tct.CTMaNV = tnv.NVMa
					INNER JOIN RecordData'+ CONVERT(VARCHAR(5),@TNam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@TThang),2),3) +' rd ON rd.IDCard = tct.CTMaThe
				WHERE  DATEDIFF(DAY,rd.ThoiGian,@TempTuNgay) = 0
					AND rd.[Status] = 1
					AND tnv.NVMa = '+ CONVERT(VARCHAR(10),@MaNhanVien) +'
			END
		
		DECLARE @NVMa INT
		DECLARE @Ngay SMALLDATETIME
		DECLARE @GioVao VARCHAR(10)
		DECLARE @IDcard VARCHAR(20)
		DECLARE BangTemp1 cursor
		FOR
			SELECT     distinct
				NVMa, Ngay, GioVao, IDcard
			FROM @BangTam where datediff(day,Ngay,TempTuNgay) =0
			ORDER BY NVMa

		OPEN BangTemp1 
			FETCH NEXT FROM BangTemp1 INTO @NVMa,@Ngay,@GioVao,@IDcard
			WHILE @@Fetch_Status = 0
			BEGIN			
				UPDATE @BangTam
				SET
					GioRa = (SELECT TOP 1 CONVERT(CHAR(8),ThoiGian,108)  FROM RecordData'+ CONVERT(VARCHAR(5),@TNam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@TThang),2),3) +'
							WHERE [Status] = 0 AND IDCard = @IDcard
							AND DATEDIFF(DAY,ThoiGian,@TempTuNgay)= 0
							AND CONVERT(CHAR(8),ThoiGian,108) > @GioVao
						 ORDER BY ThoiGian ASC)
				WHERE NVMa = @NVMa AND DATEDIFF(DAY,Ngay,@Ngay) = 0
				AND GioVao = @GioVao AND IDcard = @IDcard				
				
				FETCH NEXT FROM BangTemp1 INTO @NVMa,@Ngay,@GioVao,@IDcard
			END
		CLOSE  BangTemp1	
		DEALLOCATE BangTemp1
		SET @TempTuNgay = DATEADD(DAY,1,@TempTuNgay) 
		END
	'
	SET @i = @i+1
	SET @TThang  = @TThang + 1
	IF @TThang >12 
	BEGIN
		SET @TNam = @TNam + 1
		SET @TThang = 1
	END
END
SET @XauChuanIIII = ' Select * from @BangTam '
EXEC(@XauChuanI+@XauChuanII+@XauChuanIIII)

GO
/****** Object:  StoredProcedure [dbo].[cp_UpdateDKNghi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Batch submitted through debugger: SQLQuery12.sql|0|0|C:\Documents and Settings\THANG\Local Settings\Temp\~vs23BF.sql

CREATE  PROCEDURE [dbo].[cp_UpdateDKNghi]
@MaNV INT,
@NgayApDungOLD DATETIME,
@NgayKetThucOLD DATETIME,
@NgayApDungNew DATETIME,
@NgayKetThucNew DATETIME,
@DKNghiMaLoaiNghiOLD INT,
@DKNghiMaLoaiNghiNew INT,
@DKNghiLoaiOLD INT,
@DKNghiLoaiNEW INT,
@SoNgayNghiBuOLD INT,
@SoNgayNghiBuNew INT
AS
DECLARE @Nam VARCHAR(5)

SET @Nam = CONVERT(VARCHAR(5),YEAR(@NgayApDungOLD))
DECLARE @LoaiNghi INT
 DECLARE @NgayApDungOLD1 DATETIME
DECLARE @NgayApDungNew1 DATETIME

SET @NgayApDungNew1=Dateadd(day,@SoNgayNghiBuNew,@NgayApDungNew)
SET @NgayApDungOLD1=Dateadd(day,@SoNgayNghiBuOLD,@NgayApDungNew)
	exec cp_InsertDangKyNghi @MaNV,@NgayApDungOLD
	exec cp_Update_DeleteDangKyNghi @MaNV,@NgayApDungOLD1
	exec cp_InsertDangKyNghi @MaNV,@NgayApDungNew
	exec cp_Update_DeleteDangKyNghi @MaNV,@NgayApDungNew1
--if EXISTS (SELECT * FROM tblLoaiNghi tln INNER JOIN tblLyDoNghi ld ON ld.LDNLoai = tln.LNMa WHERE tln.LNMa = 8 AND ld.LDNMa = @DKNghiMaLoaiNghiOLD ) 
--BEGIN
	
--	--EXEC ('Update tblDangKyNghi'+ @Nam +' 
--	--		SET DKNNgayApDung = ' + '''' +  @NgayApDungNew + '''' + ',
--	--				DKNNgayKetThuc = ' + '''' +  @NgayKetThucNew + '''' + ',
--	--				DKNMaLyDo = '+ @DKNghiMaLoaiNghiNew +',
--	--				DKNLoai = '+ @DKNghiLoaiNew +',
--	--				DKNNghiBu = '+ @SoNgayNghiBuNew +'
--	--       where DKNMaNV = '+ @MaNV +' and DKNNgayApDung = ' + '''' +  @NgayApDungOLD + '''' + '
--	--       and DKNNgayKetThuc = ' + '''' +  @NgayKetThucOLD + '''' + '
--	--       and DKNMaLyDo = '+ @DKNghiMaLoaiNghiOLD +'
--	--       and DKNLoai = '+ @DKNghiLoaiOLD +'
--	--       and DKNNghiBu = '+ @SoNgayNghiBuOLD +' 
--	--	')
--	exec cp_InsertDangKyNghi @MaNV,@NgayApDungOLD
--		if EXISTS (SELECT * FROM tblLoaiNghi tln INNER JOIN tblLyDoNghi ld ON ld.LDNLoai = tln.LNMa WHERE tln.LNMa = 8 AND ld.LDNMa = @DKNghiMaLoaiNghiNew ) 
--			BEGIN
--				exec cp_InsertDangKyNghi @MaNV,@NgayApDungNew
--			END
--END
--ELSE
--	BEGIN
--		IF EXISTS (SELECT * FROM tblLoaiNghi tln INNER JOIN tblLyDoNghi ld ON ld.LDNLoai = tln.LNMa WHERE tln.LNMa = 8 AND ld.LDNMa = @DKNghiMaLoaiNghiNew )
--		BEGIN
--				--EXEC ('Update tblDangKyNghi'+ @Nam +' 
--				--		SET DKNNgayApDung = ' + '''' +  @NgayApDungNew + '''' + ',
--				--				DKNNgayKetThuc = ' + '''' +  @NgayKetThucNew + '''' + ',
--				--				DKNMaLyDo = '+ @DKNghiMaLoaiNghiNew +',
--				--				DKNLoai = '+ @DKNghiLoaiNew +',
--				--				DKNNghiBu = '+ @SoNgayNghiBuNew +'
--				--	   where DKNMaNV = '+ @MaNV +' and DKNNgayApDung = ' + '''' +  @NgayApDungOLD + '''' + '
--				--	   and DKNNgayKetThuc = ' + '''' +  @NgayKetThucOLD + '''' + '
--				--	   and DKNMaLyDo = '+ @DKNghiMaLoaiNghiOLD +'
--				--	   and DKNLoai = '+ @DKNghiLoaiOLD +'
--				--	   and DKNNghiBu = '+ @SoNgayNghiBuOLD +' 
--				--	')
--			exec cp_InsertDangKyNghi @MaNV,@NgayApDungNew
--		END
--		--ELSE
--		--	BEGIN
--		--			EXEC ('	Update tblDangKyNghi'+ @Nam +' 
--		--						SET		DKNNgayApDung = ' + '''' +  @NgayApDungNew + '''' + ',
--		--								DKNNgayKetThuc = ' + '''' +  @NgayKetThucNew + '''' + ',
--		--								DKNMaLyDo = '+ @DKNghiMaLoaiNghiNew +',
--		--								DKNLoai = '+ @DKNghiLoaiNew +',
--		--								DKNNghiBu = '+ @SoNgayNghiBuNew +'
--		--					   where DKNMaNV = '+ @MaNV +' and DKNNgayApDung = ' + '''' +  @NgayApDungOLD + '''' + '
--		--					   and DKNNgayKetThuc = ' + '''' +  @NgayKetThucOLD + '''' + '
--		--					   and DKNMaLyDo = '+ @DKNghiMaLoaiNghiOLD +'
--		--					   and DKNLoai = '+ @DKNghiLoaiOLD +'
--		--					   and DKNNghiBu = '+ @SoNgayNghiBuOLD +' 
--		--			')
--		--	END
--	END

GO
/****** Object:  StoredProcedure [dbo].[SP_CalculateAll]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[SP_CalculateAll]
  @MaBP Int,
 @Ngaybatdau datetime,
 @Ngayketthuc datetime
AS
--
	Declare @TempDate datetime
	DECLARE @DIDList nvarchar(1000)
		
	SET @DIDList=dbo.DonVi_IDs(@MaBP)
	
	set @TempDate=@Ngaybatdau
	WHILE @TempDate <= @Ngayketthuc
		BEGIN	
			
			exec dbo.CalculatePartTimeKeeping @MaBP,@DIDList,@TempDate
			SET @TempDate = DATEADD(d,1,@TempDate)
		END
						
GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_ALTERviewTaoBCC_Cong_NgayNghi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/************************************************************
*  Routine       :	sphrmvn_ALTERviewTaoBCC_Cong_NgayNghi
*  Created by    :  Duong Van Quyet , at 15/02/2011 - 16:44:37  
*  Machine       :  DIGISOFT
*  Description   :  Tinh cong ngay nghi
*  Parameters    :  @D1 datetime, @D2 datetime, @MaNV int, @DSMaBP varchar(1000)
************************************************************/
CREATE PROCEDURE [dbo].[sphrmvn_ALTERviewTaoBCC_Cong_NgayNghi]
	@D1	DATETIME,
	@D2 AS DATETIME,
	@MaNV AS INT,
	@DSMaBP AS VARCHAR(1000)
AS
BEGIN
	DECLARE @sSQL  VARCHAR(MAX)
	DECLARE @sCondition VARCHAR(MAX)
	DECLARE @sCondition01 VARCHAR(MAX)

	IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[viewTaoBCC_Cong_NgayNghi]'))
	DROP VIEW [dbo].[viewTaoBCC_Cong_NgayNghi]
	
	IF (LEN(@DSMaBP)=0)
	BEGIN
		SET @sCondition=' AND tbc.BCMaNV=' + convert(varchar,@MaNV)
		SET @sCondition01=' AND tnv.NVMa=' + convert(varchar,@MaNV)

	END
	ELSE
		BEGIN
		SET @sCondition=' AND  tbc.BCMaBP in (' + @DSMaBP + ')'
		SET @sCondition01=' AND  tbp.BPMa in (' + @DSMaBP + ')'
		END
	
	exec ('TRUNCATE TABLE tblBaoCaoTam_NgayNghi')
	Declare @SQL1 VARCHAR(1000)
	SET @SQL1= 'INSERT INTO [tblBaoCaoTam_NgayNghi] SELECT *
														FROM   tblBaoCao tbc
														WHERE  tbc.BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + ' AND tbc.BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
															   + @sCondition + ''
	EXEC (@SQL1)
	SET @sSQL=''
	set @sSQL='CREATE VIEW  [viewTaoBCC_Cong_NgayNghi]
	AS
SELECT tnv.NVMa AS nghictyMaNV,
       HC.SoCaHC as SoCaHCCN,
       C1.SoCa1 as SoCa1CN ,
       C2.SoCa2  as SoCa2CN,
       C3.SoCa3  as SoCa3CN
FROM   tblNhanVien tnv
       INNER JOIN tblBoPhan tbp
            ON  tnv.NVMaBP = tbp.BPMa

       LEFT JOIN (
                SELECT NVMa AS NVMa,
                       SUM(SoCaHCCN_CT) AS SoCaHC
                FROM   (
                           SELECT tnv.NVMa,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh <> 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh = 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / 480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS SoCaHCCN_CT
                           FROM   dbo.tblNhanVien AS tnv
                                  LEFT OUTER JOIN (
                                           SELECT *
                                           FROM   tblBaoCaoTam_NgayNghi AS tbc
                                           WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
												   AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
												   + @sCondition + '
                                       ) AS tbc1
                                       ON  tbc1.BCMaNV = tnv.NVMa
                           WHERE  (
                                      -- Doan dieu kien de tinh ngay nay la ngay nghi
                                      (
                                          (
											(
																  (
																	  1 = (
																		  CASE -- Neu hom do cap ca, Ca cho phep tinh ngay do la Ngay nghi
																			   WHEN (
																						SELECT tc.CNgayLeLambt
																						FROM   tblCa tc
																						WHERE  tc.CMa = (
																								   SELECT tbc.BCMaCa
																								   FROM   tblBaoCao 
																										  tbc
																								   WHERE  tbc.BCNgay = tbc1.BCNgay
																										  AND tbc.BCMaNV = 
																											  tnv.NVMa
																							   )
																					) = 0 THEN CASE 
																									WHEN (
																											 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																												   NNLNgay
																																											FROM   
																																												   dbo.tblNgayNghiLe AS 
																																												   tnnl
																																											WHERE  (NNLLoai = 2))
																										 ) THEN 1
																									ELSE 0
																							   END
																					-- Neu hom do cap ca, Ca cho ko cho phep tinh ngay do la Ngay nghi
																			   WHEN (
																						SELECT tc.CNgayLeLambt
																						FROM   tblCa tc
																						WHERE  tc.CMa = (
																								   SELECT tbc.BCMaCa
																								   FROM   tblBaoCaoTam_NgayThuong 
																										  tbc
																								   WHERE  tbc.BCNgay = tbc1.BCNgay
																										  AND tbc.BCMaNV = 
																											  tnv.NVMa
																							   )
																					) = 1 THEN 0
																			   ELSE CASE 
																						 WHEN (
																								  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																										NNLNgay
																																								 FROM   
																																										dbo.tblNgayNghiLe AS 
																																										tnnl
																																								 WHERE  (NNLLoai = 2))
																							  ) THEN 1
																						 ELSE 0
																					END
																		  END
																	  )
																  )
															  )                                          
                                          )
                                          OR (
                                                 DATEPART(
                                                     dw,
                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                                 ) = (
                                                     SELECT CASE 
                                                                 WHEN (
                                                                          SELECT 
                                                                                 tc.CChuNhatLambt
                                                                          FROM   
                                                                                 tblCa 
                                                                                 tc
                                                                          WHERE  
                                                                                 tc.CMa = (
                                                                                     SELECT 
                                                                                            tbc.BCMaCa
                                                                                     FROM   
                                                                                            tblBaoCaoTam_NgayThuong 
                                                                                            tbc
                                                                                     WHERE  
                                                                                            tbc.BCNgay =tbc1.BCNgay
                                                                                            AND 
                                                                                                tbc.BCMaNV = 
                                                                                                tnv.NVMa
                                                                                 )
                                                                      ) <> 1 THEN (
                                                                          SELECT CASE (
                                                                                          SELECT 
                                                                                                 CONVERT(VARCHAR, TSGiatri)
                                                                                          FROM   
                                                                                                 tblThamSo 
                                                                                                 tts
                                                                                          WHERE  
                                                                                                 tts.TSTen = 
                                                                                                 ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
                                                                                      )
                                                                                      WHEN 
                                                                                           ' + '''' + '1' + '''' + ' THEN 
                                                                                           0
                                                                                      ELSE 
                                                                                           1
                                                                                 END
                                                                      )
                                                                 ELSE 1
                                                            END
                                                 )
                                             )
                                      )
                                      --Ket thuc dieu kien de tinh ngay nay la ngay nghi
                                      AND -- Doan lenh dieu kien de tinh ca la ca HC =0
                                          (
                                              (
                                                  SELECT CONVERT(TINYINT, tc.CLoaiCa)
                                                  FROM   tblCa tc
                                                  WHERE  tc.CMa = (
                                                             SELECT tbc.BCMaCa
                                                             FROM   tblBaoCaoTam_NgayThuong 
                                                                    tbc
                                                             WHERE  tbc.BCNgay =tbc1.BCNgay
                                                                    AND tbc.BCMaNV = 
                                                                        tnv.NVMa
                                                         )
                                              ) = 0
                                          )
                                          -- Ket thuc dieu kien tinh so cong lam ca HC chu nhat
                                  )
                       ) AS T2
                GROUP BY
                       NVMa
            ) HC
            ON  tnv.NVMa = HC.NVMa
       LEFT JOIN (
                SELECT NVMa AS NVMa,
                       SUM(SoCa1_CT) AS SoCa1
                FROM   (
                           SELECT tnv.NVMa,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh <> 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh = 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / 480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS SoCa1_CT
                           FROM   dbo.tblNhanVien AS tnv
                                  LEFT OUTER JOIN (
                                           SELECT *
                                           FROM   dbo.tblBaoCaoTam_NgayThuong AS tbc
                                                  WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
												   AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
												   + @sCondition + '
                                       ) AS tbc1
                                       ON  tbc1.BCMaNV = tnv.NVMa
                           WHERE  (
                                      -- Doan dieu kien de tinh ngay nay la ngay nghi
                                      (
                                          (
											(
												  (
													  1 = (
														  CASE -- Neu hom do cap ca, Ca cho phep tinh ngay do la Ngay nghi
															   WHEN (
																		SELECT tc.CNgayLeLambt
																		FROM   tblCa tc
																		WHERE  tc.CMa = (
																				   SELECT tbc.BCMaCa
																				   FROM   tblBaoCaoTam_NgayThuong 
																						  tbc
																				   WHERE  tbc.BCNgay = tbc1.BCNgay
																						  AND tbc.BCMaNV = 
																							  tnv.NVMa
																			   )
																	) = 0 THEN CASE 
																					WHEN (
																							 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																								   NNLNgay
																																							FROM   
																																								   dbo.tblNgayNghiLe AS 
																																								   tnnl
																																							WHERE  (NNLLoai = 2))
																						 ) THEN 1
																					ELSE 0
																			   END
																	-- Neu hom do cap ca, Ca cho ko cho phep tinh ngay do la Ngay nghi
															   WHEN (
																		SELECT tc.CNgayLeLambt
																		FROM   tblCa tc
																		WHERE  tc.CMa = (
																				   SELECT tbc.BCMaCa
																				   FROM   tblBaoCaoTam_NgayThuong 
																						  tbc
																				   WHERE  tbc.BCNgay = tbc1.BCNgay
																						  AND tbc.BCMaNV = 
																							  tnv.NVMa
																			   )
																	) = 1 THEN 0
															   ELSE CASE 
																		 WHEN (
																				  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																						NNLNgay
																																				 FROM   
																																						dbo.tblNgayNghiLe AS 
																																						tnnl
																																				 WHERE  (NNLLoai = 2))
																			  ) THEN 1
																		 ELSE 0
																	END
														  END
													  )
												  )
											  )                                          )
                                          OR (
                                                 DATEPART(
                                                     dw,
                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                                 ) = (
                                                     SELECT CASE 
                                                                 WHEN (
                                                                          SELECT 
                                                                                 tc.CChuNhatLambt
                                                                          FROM   
                                                                                 tblCa 
                                                                                 tc
                                                                          WHERE  
                                                                                 tc.CMa = (
                                                                                     SELECT 
                                                                                            tbc.BCMaCa
                                                                                     FROM   
                                                                                            tblBaoCaoTam_NgayThuong 
                                                                                            tbc
                                                                                     WHERE  
                                                                                            tbc.BCNgay =tbc1.BCNgay
                                                                                            AND 
                                                                                                tbc.BCMaNV = 
                                                                                                tnv.NVMa
                                                                                 )
                                                                      ) <> 1 THEN (
                                                                          SELECT CASE (
                                                                                          SELECT 
                                                                                                 CONVERT(VARCHAR, TSGiatri)
                                                                                          FROM   
                                                                                                 tblThamSo 
                                                                                                 tts
                                                                                          WHERE  
                                                                                                 tts.TSTen = 
                                                                                                 ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
                                                                                      )
                                                                                      WHEN 
                                                                                          ' + '''' + '1' + '''' + ' THEN 
                                                                                           0
                                                                                      ELSE 
                                                                                           1
                                                                                 END
                                                                      )
                                                                 ELSE 1
                                                            END
                                                 )
                                             )
                                      )
                                      --Ket thuc dieu kien de tinh ngay nay la ngay nghi
                                      AND -- Doan lenh dieu kien de tinh ca la ca HC =1
                                          (
                                              (
                                                  SELECT CONVERT(TINYINT, tc.CLoaiCa)
                                                  FROM   tblCa tc
                                                  WHERE  tc.CMa = (
                                                             SELECT tbc.BCMaCa
                                                             FROM   tblBaoCaoTam_NgayThuong 
                                                                    tbc
                                                             WHERE  tbc.BCNgay =tbc1.BCNgay
                                                                    AND tbc.BCMaNV = 
                                                                        tnv.NVMa
                                                         )
                                              ) = 1
                                          )
                                          -- Ket thuc dieu kien tinh so cong lam ca 1 chu nhat
                                  )
                       ) AS T2
                GROUP BY
                       NVMa
            ) C1
            ON  tnv.NVMa = C1.NVMa
       LEFT JOIN (
                SELECT NVMa AS NVMa,
                       SUM(SoCa2_CT) AS SoCa2
                FROM   (
                           SELECT tnv.NVMa,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh <> 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh = 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / 480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS SoCa2_CT
                           FROM   dbo.tblNhanVien AS tnv
                                  LEFT OUTER JOIN (
                                           SELECT *
                                           FROM   dbo.tblBaoCao AS tbc
                                            WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
											AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
											+ @sCondition + '
                                       ) AS tbc1
                                       ON  tbc1.BCMaNV = tnv.NVMa
                           WHERE  (
                                      -- Doan dieu kien de tinh ngay nay la ngay nghi
                                      (
                                          (
							(
												  (
													  1 = (
														  CASE -- Neu hom do cap ca, Ca cho phep tinh ngay do la Ngay nghi
															   WHEN (
																		SELECT tc.CNgayLeLambt
																		FROM   tblCa tc
																		WHERE  tc.CMa = (
																				   SELECT tbc.BCMaCa
																				   FROM   tblBaoCaoTam_NgayThuong 
																						  tbc
																				   WHERE  tbc.BCNgay = tbc1.BCNgay
																						  AND tbc.BCMaNV = 
																							  tnv.NVMa
																			   )
																	) = 0 THEN CASE 
																					WHEN (
																							 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																								   NNLNgay
																																							FROM   
																																								   dbo.tblNgayNghiLe AS 
																																								   tnnl
																																							WHERE  (NNLLoai = 2))
																						 ) THEN 1
																					ELSE 0
																			   END
																	-- Neu hom do cap ca, Ca cho ko cho phep tinh ngay do la Ngay nghi
															   WHEN (
																		SELECT tc.CNgayLeLambt
																		FROM   tblCa tc
																		WHERE  tc.CMa = (
																				   SELECT tbc.BCMaCa
																				   FROM   tblBaoCaoTam_NgayThuong 
																						  tbc
																				   WHERE  tbc.BCNgay = tbc1.BCNgay
																						  AND tbc.BCMaNV = 
																							  tnv.NVMa
																			   )
																	) = 1 THEN 0
															   ELSE CASE 
																		 WHEN (
																				  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																						NNLNgay
																																				 FROM   
																																						dbo.tblNgayNghiLe AS 
																																						tnnl
																																				 WHERE  (NNLLoai = 2))
																			  ) THEN 1
																		 ELSE 0
																	END
														  END
													  )
												  )
											  )                                          )
                                          OR (
                                                 DATEPART(
                                                     dw,
                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                                 ) = (
                                                     SELECT CASE 
                                                                 WHEN (
                                                                          SELECT 
                                                                                 tc.CChuNhatLambt
                                                                          FROM   
                                                                                 tblCa 
                                                                                 tc
                                                                          WHERE  
                                                                                 tc.CMa = (
                                                                                     SELECT 
                                                                                            tbc.BCMaCa
                                                                                     FROM   
                                                                                            tblBaoCaoTam_NgayThuong 
                                                                                            tbc
                                                                                     WHERE  
                                                                                            tbc.BCNgay =tbc1.BCNgay
                                                                                            AND 
                                                                                                tbc.BCMaNV = 
                                                                                                tnv.NVMa
                                                                                 )
                                                                      ) <> 1 THEN (
                                                                          SELECT CASE (
                                                                                          SELECT 
                                                                                                 CONVERT(VARCHAR, TSGiatri)
                                                                                          FROM   
                                                                                                 tblThamSo 
                                                                                                 tts
                                                                                          WHERE  
                                                                                                 tts.TSTen = 
                                                                                                 ' + '''' + 'NORMALWORKINGONSUNDAY' +'''' + '
                                                                                      )
                                                                                      WHEN 
                                                                                           ' + '''' + '1' + '''' + ' THEN 
                                                                                           0
                                                                                      ELSE 
                                                                                           1
                                                                                 END
                                                                      )
                                                                 ELSE 1
                                                            END
                                                 )
                                             )
                                      )
                                      --Ket thuc dieu kien de tinh ngay nay la ngay nghi
                                      AND -- Doan lenh dieu kien de tinh ca la ca HC =2
                                          (
                                              (
                                                  SELECT CONVERT(TINYINT, tc.CLoaiCa)
                                                  FROM   tblCa tc
                                                  WHERE  tc.CMa = (
                                                             SELECT tbc.BCMaCa
                                                             FROM   tblBaoCaoTam_NgayThuong 
                                                                    tbc
                                                             WHERE  tbc.BCNgay = 
                                                                    DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                                                    AND tbc.BCMaNV = 
                                                                        tnv.NVMa
                                                         )
                                              ) = 2
                                          )
                                          -- Ket thuc dieu kien tinh so cong lam ca 2 chu nhat
                                  )
                       ) AS T2
                GROUP BY
                       NVMa
            ) C2
            ON  tnv.NVMa = C2.NVMa
       LEFT JOIN (
                SELECT NVMa AS NVMa,
                       SUM(SoCa3_CT) AS SoCa3
                FROM   (
                           SELECT tnv.NVMa,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh <> 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh = 0 THEN (
                                                        ISNULL(tbc1.BCTGLamNgay, 0)
                                                        + ISNULL(tbc1.BCTGLamToi, 0) 
                                                    ) / 480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS SoCa3_CT
                           FROM   dbo.tblNhanVien AS tnv
                                  LEFT OUTER JOIN (
                                           SELECT *
                                           FROM   dbo.tblBaoCaoTam_NgayThuong AS tbc
                                           WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
												   AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
												   + @sCondition + '
                                       ) AS tbc1
                                       ON  tbc1.BCMaNV = tnv.NVMa
                           WHERE  (
                                      -- Doan dieu kien de tinh ngay nay la ngay nghi
                                      (
                                          (
							(
												  (
													  1 = (
														  CASE -- Neu hom do cap ca, Ca cho phep tinh ngay do la Ngay nghi
															   WHEN (
																		SELECT tc.CNgayLeLambt
																		FROM   tblCa tc
																		WHERE  tc.CMa = (
																				   SELECT tbc.BCMaCa
																				   FROM   tblBaoCaoTam_NgayThuong 
																						  tbc
																				   WHERE  tbc.BCNgay = tbc1.BCNgay
																						  AND tbc.BCMaNV = 
																							  tnv.NVMa
																			   )
																	) = 0 THEN CASE 
																					WHEN (
																							 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																								   NNLNgay
																																							FROM   
																																								   dbo.tblNgayNghiLe AS 
																																								   tnnl
																																							WHERE  (NNLLoai = 2))
																						 ) THEN 1
																					ELSE 0
																			   END
																	-- Neu hom do cap ca, Ca cho ko cho phep tinh ngay do la Ngay nghi
															   WHEN (
																		SELECT tc.CNgayLeLambt
																		FROM   tblCa tc
																		WHERE  tc.CMa = (
																				   SELECT tbc.BCMaCa
																				   FROM   tblBaoCaoTam_NgayThuong 
																						  tbc
																				   WHERE  tbc.BCNgay = tbc1.BCNgay
																						  AND tbc.BCMaNV = 
																							  tnv.NVMa
																			   )
																	) = 1 THEN 0
															   ELSE CASE 
																		 WHEN (
																				  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																						NNLNgay
																																				 FROM   
																																						dbo.tblNgayNghiLe AS 
																																						tnnl
																																				 WHERE  (NNLLoai = 2))
																			  ) THEN 1
																		 ELSE 0
																	END
														  END
													  )
												  )
											  )                                          )
                                          OR (
                                                 DATEPART(
                                                     dw,
                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                                 ) = (
                                                     SELECT CASE 
                                                                 WHEN (
                                                                          SELECT 
                                                                                 tc.CChuNhatLambt
                                                                          FROM   
                                                                                 tblCa 
                                                                                 tc
                                                                          WHERE  
                                                                                 tc.CMa = (
                                                                                     SELECT 
                                                                                            tbc.BCMaCa
                                                                                     FROM   
                                                                                            tblBaoCaoTam_NgayThuong 
                                                                                            tbc
                                                                                     WHERE  
                                                                                            tbc.BCNgay =tbc1.BCNgay
                                                                                            AND 
                                                                                                tbc.BCMaNV = 
                                                                                                tnv.NVMa
                                                                                 )
                                                                      ) <> 1 THEN (
                                                                          SELECT CASE (
                                                                                          SELECT 
                                                                                                 CONVERT(VARCHAR, TSGiatri)
                                                                                          FROM   
                                                                                                 tblThamSo 
                                                                                                 tts
                                                                                          WHERE  
                                                                                                 tts.TSTen = 
                                                                                                 ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
                                                                                      )
                                                                                      WHEN 
                                                                                           ' + '''' +'1' + '''' + ' THEN 
                                                                                           0
                                                                                      ELSE 
                                                                                           1
                                                                                 END
                                                                      )
                                                                 ELSE 1
                                                            END
                                                 )
                                             )
                                      )
                                      --Ket thuc dieu kien de tinh ngay nay la ngay nghi
                                      AND -- Doan lenh dieu kien de tinh ca la ca HC =3
                                          (
                                              (
                                                  SELECT CONVERT(TINYINT, tc.CLoaiCa)
                                                  FROM   tblCa tc
                                                  WHERE  tc.CMa = (
                                                             SELECT tbc.BCMaCa
                                                             FROM   tblBaoCaoTam_NgayThuong 
                                                                    tbc
                                                             WHERE  tbc.BCNgay = 
                                                                    DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                                                    AND tbc.BCMaNV = 
                                                                        tnv.NVMa
                                                         )
                                              ) = 3
                                          )
                                          -- Ket thuc dieu kien tinh so cong lam ca 3 chu nhat
                                  )
                       ) AS T2
                GROUP BY
                       NVMa
            ) C3
            ON  tnv.NVMa = C3.NVMa
WHERE (1=1)            
' + @sCondition01
PRINT @sSQL
EXEC (@sSQL)
END

       
GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_ALTERviewTaoBCC_Cong_NgayThuong]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/************************************************************
*  Routine       :	sphrmvn_ALTERviewTaoBCC_Cong_NgayThuong
*  Created by    :  Duong Van Quyet , at 17/02/2011 - 10:08:18  
*  Machine       :  DIGISOFT
*  Description   :  Tinh so cong ngay thuong
*  Parameters    :  @D1 datetime, @D2 datetime, @MaNV int, @DSMaBP varchar(1000)
************************************************************/
CREATE PROCEDURE [dbo].[sphrmvn_ALTERviewTaoBCC_Cong_NgayThuong]
	@D1	DATETIME,
	@D2 AS DATETIME,
	@MaNV AS INT,
	@DSMaBP AS VARCHAR(1000)
AS
BEGIN
	DECLARE @sSQL  NVARCHAR(MAX)
	DECLARE @sCondition NVARCHAR(MAX)
	DECLARE @sCondition01 VARCHAR(MAX)
	IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[viewTaoBCC_Cong_NgayThuong]'))
	DROP VIEW [dbo].[viewTaoBCC_Cong_NgayThuong]
	
	IF (LEN(@DSMaBP)=0)
	BEGIN
		SET @sCondition=' AND tbc.BCMaNV=' + convert(varchar,@MaNV)
				SET @sCondition01=' AND tnv.NVMa=' + convert(varchar,@MaNV)

	END
	ELSE
		BEGIN
		SET @sCondition=' AND  tbc.BCMaBP in (' + @DSMaBP + ')'
				SET @sCondition01=' AND  tbp.BPMa in (' + @DSMaBP + ')'

		END
	
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblBaoCaoTam_CongLam]') AND type in (N'U'))
	DROP TABLE [dbo].[tblBaoCaoTam_NgayThuong]
	Declare @SQL1 VARCHAR(1000)
	SET @SQL1= ' SELECT * Into [tblBaoCaoTam_NgayThuong]
														FROM   tblBaoCao tbc
														WHERE  tbc.BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + ' AND tbc.BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
															   + @sCondition + ''
	EXEC (@SQL1)
	
	SET @sSQL=''
	set @sSQL='CREATE VIEW  [viewTaoBCC_Cong_NgayThuong]
	AS
	SELECT tnv.NVMa as cntNVMa,
		   HC.SoCa_CT AS SoCaHC,
		   C1.SoCa_CT AS SoCa1,
		   C2.SoCa_CT AS SoCa2,
		   C3.SoCa_CT AS SoCa3
	FROM   dbo.tblNhanVien tnv
       INNER JOIN tblBoPhan tbp
            ON  tnv.NVMaBP = tbp.BPMa
		   LEFT JOIN (
					SELECT NVMa,
						   SUM(SoCa_CT) AS SoCa_CT
					FROM   (
							   SELECT tnv.NVMa,
									  ROUND(
										  (
											  CASE 
												   WHEN tbc1.BCTGQuyDinh <> 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ CONVERT(FLOAT, tbc1.BCTGQuyDinh)
												   WHEN tbc1.BCTGQuyDinh = 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ 480
												   ELSE 0
											  END
										  ),
										  2
									  ) AS SoCa_CT
							   FROM   dbo.tblNhanVien AS tnv
									  LEFT OUTER JOIN (
                  									   SELECT *
														FROM   tblBaoCaoTam_NgayThuong tbc
														WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
															   AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
															   + @sCondition + '
										   ) AS tbc1
										   ON  tbc1.BCMaNV = tnv.NVMa
							   WHERE  (
										  (
											  -- Xet dieu kien cua ca
											  1 >= (
												  CASE 
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Chu nhat va ngay le lam binh thuong
															) = 0 THEN CASE 
																			WHEN (
																					 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																						   NNLNgay
																																					FROM   
																																						   dbo.tblNgayNghiLe AS 
																																						   tnnl)
																				 ) THEN 
																				 0
																			ELSE 1
																	   END
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Khi la chu nhat hay ngay le thi khong tinh la ngay thuong
															) > 1 THEN 0
													   ELSE CASE -- Neu chua cap ca thi nhung ngay nao nam ngoai  tblNgayNghiLe la ngay thuong
																 WHEN (
																		  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																				NNLNgay
																																		 FROM   
																																				dbo.tblNgayNghiLe AS 
																																				tnnl)
																	  ) THEN 0
																 ELSE 1
															END
												  END
											  )
										  )
										  -- Xet dieu kien cua ngay chu nhat
										  AND (
												  DATEPART(
													  dw,
													  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
												  ) <> (
													  SELECT CASE -- Khi ngay do co ca , chu nhat tinh la ngay nghi
																  WHEN (
																		   SELECT tc.CChuNhatLambt
																		   FROM   
																				  tblCa 
																				  tc
																		   WHERE  tc.CMa = (
																					  SELECT 
																							 tbc.BCMaCa
																					  FROM   
																							 tblBaoCaoTam_NgayThuong 
																							 tbc
																					  WHERE  
																							 tbc.BCNgay = 
																							 tbc1.BCNgay
																							 AND 
																								 tbc.BCMaNV = 
																								 tnv.NVMa
																				  )
																	   ) <> 1 THEN (
																		   SELECT CASE (
																						   SELECT 
																								  CONVERT(VARCHAR, TSGiatri)
																						   FROM   
																								  tblThamSo 
																								  tts
																						   WHERE  
																								  tts.TSTen = 
																								  ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																					   )
																					   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																					   WHEN 
																							1 THEN 
																							0
																					   ELSE 
																							1
																				  END
																	   )
																	   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
																  ELSE CASE (
																				SELECT 
																					   CONVERT(VARCHAR, TSGiatri)
																				FROM   
																					   tblThamSo 
																					   tts
																				WHERE  
																					   tts.TSTen = 
																					   ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																			)
																			-- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																			WHEN 1 THEN 
																				 0
																			ELSE 1
																	   END
															 END
												  )
											  )
											  -- Dieu kien xet ca HC
										  AND (
												  (
													  SELECT CONVERT(TINYINT, CLoaiCa)
													  FROM   dbo.tblCa AS tc
													  WHERE  (
																 CMa = (
																	 SELECT BCMaCa
																	 FROM   dbo.tblBaoCaoTam_NgayThuong AS 
																			tbc
																	 WHERE  (BCNgay = tbc1.BCNgay)
																			AND (BCMaNV = tnv.NVMa)
																 )
															 )
												  ) = 0
											  )
									  )
						   ) AS T2
					GROUP BY
						   NVMa
				) HC
				ON  tnv.NVMa = HC.NVMa
		   LEFT JOIN (
					SELECT NVMa,
						   SUM(SoCa_CT) AS SoCa_CT
					FROM   (
							   SELECT tnv.NVMa,
									  ROUND(
										  (
											  CASE 
												   WHEN tbc1.BCTGQuyDinh <> 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ CONVERT(FLOAT, tbc1.BCTGQuyDinh)
												   WHEN tbc1.BCTGQuyDinh = 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ 480
												   ELSE 0
											  END
										  ),
										  2
									  ) AS SoCa_CT
							   FROM   dbo.tblNhanVien AS tnv
									  LEFT OUTER JOIN (
                  									   SELECT *
														FROM   tblBaoCaoTam_NgayThuong tbc
														WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
															   AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
															   + @sCondition + '
										   ) AS tbc1
										   ON  tbc1.BCMaNV = tnv.NVMa
							   WHERE  (
										  (
											  -- Xet dieu kien cua ca
											  1 >= (
												  CASE 
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Chu nhat va ngay le lam binh thuong
															) = 0 THEN CASE 
																			WHEN (
																					 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																						   NNLNgay
																																					FROM   
																																						   dbo.tblNgayNghiLe AS 
																																						   tnnl)
																				 ) THEN 
																				 0
																			ELSE 1
																	   END
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Khi la chu nhat hay ngay le thi khong tinh la ngay thuong
															) > 1 THEN 0
													   ELSE CASE -- Neu chua cap ca thi nhung ngay nao nam ngoai  tblNgayNghiLe la ngay thuong
																 WHEN (
																		  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																				NNLNgay
																																		 FROM   
																																				dbo.tblNgayNghiLe AS 
																																				tnnl)
																	  ) THEN 0
																 ELSE 1
															END
												  END
											  )
										  )
										  -- Xet dieu kien cua ngay chu nhat
										  AND (
												  DATEPART(
													  dw,
													  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
												  ) <> (
													  SELECT CASE -- Khi ngay do co ca , chu nhat tinh la ngay nghi
																  WHEN (
																		   SELECT tc.CChuNhatLambt
																		   FROM   
																				  tblCa 
																				  tc
																		   WHERE  tc.CMa = (
																					  SELECT 
																							 tbc.BCMaCa
																					  FROM   
																							 tblBaoCaoTam_NgayThuong 
																							 tbc
																					  WHERE  
																							 tbc.BCNgay = 
																							 tbc1.BCNgay
																							 AND 
																								 tbc.BCMaNV = 
																								 tnv.NVMa
																				  )
																	   ) <> 1 THEN (
																		   SELECT CASE (
																						   SELECT 
																								  CONVERT(VARCHAR, TSGiatri)
																						   FROM   
																								  tblThamSo 
																								  tts
																						   WHERE  
																								  tts.TSTen = 
																								  ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																					   )
																					   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																					   WHEN 
																							1 THEN 
																							0
																					   ELSE 
																							1
																				  END
																	   )
																	   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
																  ELSE CASE (
																				SELECT 
																					   CONVERT(VARCHAR, TSGiatri)
																				FROM   
																					   tblThamSo 
																					   tts
																				WHERE  
																					   tts.TSTen = 
																					   ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																			)
																			-- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																			WHEN 1 THEN 
																				 0
																			ELSE 1
																	   END
															 END
												  )
											  )
											  -- Dieu kien xet ca C1
										  AND (
												  (
													  SELECT CONVERT(TINYINT, CLoaiCa)
													  FROM   dbo.tblCa AS tc
													  WHERE  (
																 CMa = (
																	 SELECT BCMaCa
																	 FROM   dbo.tblBaoCaoTam_NgayThuong AS 
																			tbc
																	 WHERE  (BCNgay = tbc1.BCNgay)
																			AND (BCMaNV = tnv.NVMa)
																 )
															 )
												  ) = 1
											  )
									  )
						   ) AS T2
					GROUP BY
						   NVMa
				) C1
				ON  tnv.NVMa = C1.NVMa
		   LEFT JOIN (
					SELECT NVMa,
						   SUM(SoCa_CT) AS SoCa_CT
					FROM   (
							   SELECT tnv.NVMa,
									  ROUND(
										  (
											  CASE 
												   WHEN tbc1.BCTGQuyDinh <> 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ CONVERT(FLOAT, tbc1.BCTGQuyDinh)
												   WHEN tbc1.BCTGQuyDinh = 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ 480
												   ELSE 0
											  END
										  ),
										  2
									  ) AS SoCa_CT
							   FROM   dbo.tblNhanVien AS tnv
									  LEFT OUTER JOIN (
                  									   SELECT *
														FROM   tblBaoCaoTam_NgayThuong tbc
														WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
															   AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
															   + @sCondition + '
										   ) AS tbc1
										   ON  tbc1.BCMaNV = tnv.NVMa
							   WHERE  (
										  (
											  -- Xet dieu kien cua ca
											  1 >= (
												  CASE 
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Chu nhat va ngay le lam binh thuong
															) = 0 THEN CASE 
																			WHEN (
																					 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																						   NNLNgay
																																					FROM   
																																						   dbo.tblNgayNghiLe AS 
																																						   tnnl)
																				 ) THEN 
																				 0
																			ELSE 1
																	   END
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Khi la chu nhat hay ngay le thi khong tinh la ngay thuong
															) > 1 THEN 0
													   ELSE CASE -- Neu chua cap ca thi nhung ngay nao nam ngoai  tblNgayNghiLe la ngay thuong
																 WHEN (
																		  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																				NNLNgay
																																		 FROM   
																																				dbo.tblNgayNghiLe AS 
																																				tnnl)
																	  ) THEN 0
																 ELSE 1
															END
												  END
											  )
										  )
										  -- Xet dieu kien cua ngay chu nhat
										  AND (
												  DATEPART(
													  dw,
													  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
												  ) <> (
													  SELECT CASE -- Khi ngay do co ca , chu nhat tinh la ngay nghi
																  WHEN (
																		   SELECT tc.CChuNhatLambt
																		   FROM   
																				  tblCa 
																				  tc
																		   WHERE  tc.CMa = (
																					  SELECT 
																							 tbc.BCMaCa
																					  FROM   
																							 tblBaoCaoTam_NgayThuong 
																							 tbc
																					  WHERE  
																							 tbc.BCNgay = 
																							 tbc1.BCNgay
																							 AND 
																								 tbc.BCMaNV = 
																								 tnv.NVMa
																				  )
																	   ) <> 1 THEN (
																		   SELECT CASE (
																						   SELECT 
																								  CONVERT(VARCHAR, TSGiatri)
																						   FROM   
																								  tblThamSo 
																								  tts
																						   WHERE  
																								  tts.TSTen = 
																								  ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																					   )
																					   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																					   WHEN 
																							1 THEN 
																							0
																					   ELSE 
																							1
																				  END
																	   )
																	   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
																  ELSE CASE (
																				SELECT 
																					   CONVERT(VARCHAR, TSGiatri)
																				FROM   
																					   tblThamSo 
																					   tts
																				WHERE  
																					   tts.TSTen = 
																					   ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																			)
																			-- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																			WHEN 1 THEN 
																				 0
																			ELSE 1
																	   END
															 END
												  )
											  )
											  -- Dieu kien xet ca HC
										  AND (
												  (
													  SELECT CONVERT(TINYINT, CLoaiCa)
													  FROM   dbo.tblCa AS tc
													  WHERE  (
																 CMa = (
																	 SELECT BCMaCa
																	 FROM   dbo.tblBaoCaoTam_NgayThuong AS 
																			tbc
																	 WHERE  (BCNgay = tbc1.BCNgay)
																			AND (BCMaNV = tnv.NVMa)
																 )
															 )
												  ) = 2
											  )
									  )
						   ) AS T2
					GROUP BY
						   NVMa
				) C2
				ON  tnv.NVMa = C2.NVMa
		   LEFT JOIN (
					SELECT NVMa,
						   SUM(SoCa_CT) AS SoCa_CT
					FROM   (
							   SELECT tnv.NVMa,
									  ROUND(
										  (
											  CASE 
												   WHEN tbc1.BCTGQuyDinh <> 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ CONVERT(FLOAT, tbc1.BCTGQuyDinh)
												   WHEN tbc1.BCTGQuyDinh = 0 THEN (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
														/ 480
												   ELSE 0
											  END
										  ),
										  2
									  ) AS SoCa_CT
							   FROM   dbo.tblNhanVien AS tnv
									  LEFT OUTER JOIN (
                  									   SELECT *
														FROM   tblBaoCaoTam_NgayThuong tbc
														WHERE  BCNgay <= ' + '''' + convert(varchar,@D2) + '''' + '
															   AND BCNgay >= ' + '''' + convert(varchar,@D1) + '''' + ''
															   + @sCondition + '
										   ) AS tbc1
										   ON  tbc1.BCMaNV = tnv.NVMa
							   WHERE  (
										  (
											  -- Xet dieu kien cua ca
											  1 >= (
												  CASE 
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Chu nhat va ngay le lam binh thuong
															) = 0 THEN CASE 
																			WHEN (
																					 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																						   NNLNgay
																																					FROM   
																																						   dbo.tblNgayNghiLe AS 
																																						   tnnl)
																				 ) THEN 
																				 0
																			ELSE 1
																	   END
													   WHEN (
																SELECT CONVERT(SMALLINT, ISNULL(tc.CChuNhatLambt, 0)) 
																	   + CONVERT(SMALLINT, ISNULL(tc.CNgayLeLambt, 0))
																FROM   tblCa tc
																WHERE  tc.CMa = (
																		   SELECT 
																				  tbc.BCMaCa
																		   FROM   
																				  tblBaoCaoTam_NgayThuong 
																				  tbc
																		   WHERE  
																				  tbc.BCNgay = 
																				  tbc1.BCNgay
																				  AND 
																					  tbc.BCMaNV = 
																					  tnv.NVMa
																	   )
																	   -- Khi la chu nhat hay ngay le thi khong tinh la ngay thuong
															) > 1 THEN 0
													   ELSE CASE -- Neu chua cap ca thi nhung ngay nao nam ngoai  tblNgayNghiLe la ngay thuong
																 WHEN (
																		  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																				NNLNgay
																																		 FROM   
																																				dbo.tblNgayNghiLe AS 
																																				tnnl)
																	  ) THEN 0
																 ELSE 1
															END
												  END
											  )
										  )
										  -- Xet dieu kien cua ngay chu nhat
										  AND (
												  DATEPART(
													  dw,
													  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
												  ) <> (
													  SELECT CASE -- Khi ngay do co ca , chu nhat tinh la ngay nghi
																  WHEN (
																		   SELECT tc.CChuNhatLambt
																		   FROM   
																				  tblCa 
																				  tc
																		   WHERE  tc.CMa = (
																					  SELECT 
																							 tbc.BCMaCa
																					  FROM   
																							 tblBaoCaoTam_NgayThuong 
																							 tbc
																					  WHERE  
																							 tbc.BCNgay = 
																							 tbc1.BCNgay
																							 AND 
																								 tbc.BCMaNV = 
																								 tnv.NVMa
																				  )
																	   ) <> 1 THEN (
																		   SELECT CASE (
																						   SELECT 
																								  CONVERT(VARCHAR, TSGiatri)
																						   FROM   
																								  tblThamSo 
																								  tts
																						   WHERE  
																								  tts.TSTen = 
																								  ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																					   )
																					   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																					   WHEN 
																							1 THEN 
																							0
																					   ELSE 
																							1
																				  END
																	   )
																	   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
																  ELSE CASE (
																				SELECT 
																					   CONVERT(VARCHAR, TSGiatri)
																				FROM   
																					   tblThamSo 
																					   tts
																				WHERE  
																					   tts.TSTen = 
																					   ' + '''' + 'NORMALWORKINGONSUNDAY' + '''' + '
																			)
																			-- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
																			WHEN 1 THEN 
																				 0
																			ELSE 1
																	   END
															 END
												  )
											  )
											  -- Dieu kien xet ca C3
										  AND (
												  (
													  SELECT CONVERT(TINYINT, CLoaiCa)
													  FROM   dbo.tblCa AS tc
													  WHERE  (
																 CMa = (
																	 SELECT BCMaCa
																	 FROM   dbo.tblBaoCaoTam_NgayThuong AS 
																			tbc
																	 WHERE  (BCNgay = tbc1.BCNgay)
																			AND (BCMaNV = tnv.NVMa)
																 )
															 )
												  ) = 3
											  )
									  )
						   ) AS T2
					GROUP BY
						   NVMa
				) C3
				ON  tnv.NVMa = C3.NVMa
	WHERE (1=1) 				
' + @sCondition01
PRINT @sSQL
EXEC (@sSQL)
END

       
GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_CapnhatLoaingaynghi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




/************************************************************
*  Routine       :	sphrmvn_CapnhatLoaingaynghi
*  Created by    :  Duong Van Quyet , at 07/03/2011 - 10:59:35  
*  Machine       :  DIGISOFT
*  Description   :  Cap nhat loai ngay nghi
*  Parameters    :  @D datetime, @StaffID int, @MaCa int
************************************************************/

CREATE PROCEDURE [dbo].[sphrmvn_CapnhatLoaingaynghi]
	@D DATETIME,
	@StaffID INT,
	@MaCa INT
AS
BEGIN
	DECLARE @CNLamBT       AS SMALLINT
	DECLARE @NLLamBT       AS SMALLINT
	DECLARE @TSCNLamBT     AS SMALLINT
	DECLARE @TSNLLamBT     AS SMALLINT
	DECLARE @KieuNgayNghi  AS VARCHAR(4)
	
	SELECT @CNLamBT = tc.CChuNhatLambt
	FROM   tblCa tc
	WHERE  tc.CMa = @MaCa
	
	SELECT @NLLamBT = tc.CNgayLeLambt
	FROM   tblCa tc
	WHERE  tc.CMa = @MaCa
	
	SELECT @TSCNLamBT = CONVERT(VARCHAR, tts.TSGiaTri)
	FROM   tblThamSo tts
	WHERE  tts.TSTen = 'NORMALWORKINGONSUNDAY'
	
	
	SELECT @KieuNgayNghi = tnnl.NNLLoai
	FROM   tblNgayNghiLe tnnl
	WHERE  tnnl.NNLNgay = @D
	
	-- Neu la ngay nghi
	IF (@KieuNgayNghi = 2 OR DATENAME(dw, @D) = 'Sunday')
	BEGIN
	    --Neu co cap ca va chu nhat lam binh thuong thi Update luong ngay do la ngay bt
	    IF ((@CNLamBT) = 1)
	    BEGIN
	        UPDATE tblbaocao
	        SET    BCLoaiNgayNghi = 3
	        WHERE  BCNgay = @D
	               AND BCMaNV = @StaffID
	        
	        GOTO EndThisSub
	    END
	    
	    IF (@TSCNLamBT = 1)
	    BEGIN
	        UPDATE tblbaocao
	        SET    BCLoaiNgayNghi =3
	        WHERE  BCNgay = @D
	               AND BCMaNV = @StaffID
	        
	        GOTO EndThisSub
	    END
	END
	
	-- Neu la ngay nle
	IF (@KieuNgayNghi = 1)
	BEGIN
	    IF ((@NLLamBT) = 1)
	    BEGIN
	        UPDATE tblbaocao
	        SET    BCLoaiNgayNghi = 3
	        WHERE  BCNgay = @D
	               AND BCMaNV = @StaffID
	        
	        GOTO EndThisSub
	    END
	END
	
	SET @KieuNgayNghi =  ISNULL(@KieuNgayNghi, 3)
	DECLARE @SQL AS NVARCHAR(1000)
	
	UPDATE tblbaocao
	SET    BCLoaiNgayNghi = @KieuNgayNghi
	WHERE  BCNgay = @D
	       AND BCMaNV = @StaffID
	           
	           EndThisSub:
END
GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_FindShift_New]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sphrmvn_FindShift_New]
	 @MaBP      AS INT,
	 @MaNV      AS INT,
	 @D      AS DATETIME,
	 @isDel bit
AS 
	-- Neu khoa du lieu thi thoat luon
	IF EXISTS (SELECT tblBaoCao.DLocked FROM tblBaoCao WHERE BCNgay=@D AND BCMaNV=@MaNV AND DLocked=1)
	RETURN

	DECLARE @ShiftID AS  VARCHAR(5)
	DECLARE @ShiftID1 AS  VARCHAR(5)
	DECLARE @ShiftID2 AS  VARCHAR(5)
	DECLARE @OldShift  AS VARCHAR(5)

	DECLARE @TGDen AS DATETIME
	DECLARE @TGVe Datetime

	DECLARE @KieuTimCa NVARCHAR(20)
	DECLARE @sMaLichTrinhCa VARCHAR(400)
	DECLARE @sDSCaTrongLichTrinh VARCHAR(400)
	DECLARE @CMa AS  VARCHAR(5)
	DECLARE @CVietTat AS VARCHAR(10)
	DECLARE @TGBDCa AS DATETIME
	DECLARE @TGKTCa Datetime
	DECLARE @TGKTNghi2 Datetime
	DECLARE @TGBDNghi2 Datetime
	DECLARE @QuetTruocCa int
	DECLARE @QuetSauCa int
	DECLARE @TGLayDLDau  Datetime 	
	DECLARE @TGLayDLCuoi Datetime 

	-- Khai bao ten cac bang	
	DECLARE @tblDangkynghi nvarchar(20) 	
	DECLARE @tblDangkyuudai nvarchar(20) 
	DECLARE @tblDangkynghitheogio nvarchar(20) 	

	-- khai bao dang ky uu dai
	DECLARE @TGUD1 int
	DECLARE @TGUD2 int
	DECLARE @TGUD3 int
	DECLARE @TGUD4 int
	DECLARE @UD1 int
	DECLARE @UD2 int
	DECLARE @UD3 int
	DECLARE @UD4 int

	-- khai bao dang ky nghi theo ca
	DECLARE @NgayApDung SMALLDATETIME
	DECLARE @NgayKetThuc SMALLDATETIME
	DECLARE @LoaiNghi smallint
	DECLARE @MaLyDoNghi nvarchar(10)

	-- khai bao dang ky nghi theo gio
	DECLARE @DKNTGNgay DATETIME
	DECLARE @DKNTGGio DATETIME
	DECLARE	@DKNTGSophut INT
	DECLARE	@DKNTGLyDo INT
	DECLARE @DKNTGTugio DATETIME
	DECLARE @DKNTGDengio DATETIME

	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @RowCount int
	DECLARE @NumberRecords int
	DECLARE @RowCount_TG int
	DECLARE @NumberRecords_TG int
	DECLARE	@recordCount SMALLINT
	DECLARE @TGVe_HomTruoc AS DATETIME

	DECLARE @NguongDM INT
	DECLARE @NguongVS INT
	DECLARE @NV int
	DECLARE @DepartmentID int
	DECLARE @PositionID int
	DECLARE @AllowOT int

	SET @TGUD1 = 0
	SET @TGUD2 = 0
	SET @TGUD3 = 0
	SET @TGUD4 = 0
	SET @UD1 = 0
	SET @UD2 = 0
	SET @UD3 = 0
	SET @UD4 = 0

	SET @LoaiNghi =0
	SET	@DKNTGSophut=0
	SET	@DKNTGLyDo=0

	SET @sMaLichTrinhCa=''
	SET @KieuTimCa=''

	SET @OldShift=0
	SET @ShiftID=0
	SET @ShiftID1=0
	SET @ShiftID2=0
	SET @QuetTruocCa=0
	SET @QuetSauCa=0
	SET	@recordCount=0
	SET @TGVe_HomTruoc='1900-01-01 00:00:00.000'

	SET @NguongDM=60
	SET @NguongVS=60

	-- Bat dau cursor uu dai----------------------------------------------------------------------
	CREATE TABLE [dbo].#tblDangkyUudaiTam ([RowID] [int] IDENTITY(1, 1),[DKUDMaNV] [int] ,[DKUDLoaiUuDai] [int] , [DKUDNgayApDung] [datetime] ,[DKUDNgayKetThuc] [datetime]) ON [PRIMARY]
		
	SET @SQL='Insert into #tblDangkyUudaiTam(DKUDMaNV,DKUDNgayApDung,DKUDNgayKetThuc,DKUDLoaiUuDai) SELECT DKUDMaNV,DKUDNgayApDung,DKUDNgayKetThuc,DKUDLoaiUuDai FROM '+ @tblDangkyuudai+ ' WHERE (DKudMaNV=' + CONVERT(nvarchar(30),@MaNV) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKUDNgayApDung AND DKUDNgayKetThuc) '
	EXEC (@SQL)

	SELECT @NumberRecords = COUNT(*)  FROM #tblDangkyUudaiTam
	SET @RowCount = 1
		
	WHILE @RowCount <= @NumberRecords
		BEGIN
			SELECT @UD1=LUDDauCa,@UD2=LUDTruocNghi,@UD3=LUDSauNghi,@UD4=LUDCuoiCa
			FROM #tblDangkyUudaiTam
			INNER JOIN tblLoaiUuDai ON  #tblDangkyUudaiTam.DKUDLoaiUuDai=tblLoaiUuDai.LUDMa
			WHERE RowID = @RowCount

			SET @TGUD1 =@TGUD1+@UD1
			SET @TGUD2=@TGUD2+@UD2
			SET @TGUD3 =@TGUD3+@UD3
			SET @TGUD4=@TGUD4+@UD4

			SET @RowCount = @RowCount + 1
		END
	DROP TABLE #tblDangkyUudaiTam
	-- Ket Thuc cursor uu dai--------------------------------------------------------------------

	-- Bat dau cursor nghi theo ca----------------------------------------------------------------
	CREATE TABLE [dbo].#tblDangkynghiTam ([RowID] [int] IDENTITY(1, 1),[DKNMaNV] [int],[DKNNgayApDung] [datetime],[DKNNgayKetThuc] [datetime],[DKNMaLyDo]  [varchar](10),[DKNLoai] [tinyint]) ON [PRIMARY]
		
	SET @SQL='Insert into #tblDangkynghiTam(DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai) SELECT DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai FROM '+ @tblDangkynghi+ ' WHERE (DKNMaNV=' + CONVERT(nvarchar(30),@MaNV) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKNNgayApDung AND DKNNgayKetThuc)'
	EXEC (@SQL)

	SELECT @NumberRecords = COUNT(*)  FROM #tblDangkynghiTam
	SET @RowCount = 1

	WHILE @RowCount <= @NumberRecords
		BEGIN
			SELECT @LoaiNghi=DKNLoai, @MaLyDoNghi=DKNMaLyDo, @NgayApDung=DKNNgayApDung,@NgayKetThuc=DKNNgayKetThuc
			FROM #tblDangkynghiTam
			WHERE RowID = @RowCount

			SET @RowCount = @RowCount + 1
		END
	DROP TABLE #tblDangkynghiTam
	-- Ket thuc cursor nghi theo ca----------------------------------------------------------------

	-- Lay thoi gian lam viec cua ngay hom truoc
	SELECT  @TGVe_HomTruoc=Isnull(tblBaoCao.BCTGVe,'1900-01-01 00:00:00.000') FROM tblBaoCao WHERE BCMaNV=@MaNV AND BCNgay=DATEADD(DAY,-1,@D)
	
	IF(@TGVe_HomTruoc='1900-01-01 00:00:00.000')
		BEGIN
			SET @TGVe_HomTruoc=@D
		END

	-- Lay lich trinh ca
	SELECT @sMaLichTrinhCa=ISNULL(BCLichTrinhCa,''), @OldShift=BCMaCa, @NV=BCMaNV FROM tblBaoCao WHERE BCNgay=@D AND BCMaNV=@MaNV

	IF(@sMaLichTrinhCa='')
		BEGIN
			SELECT @sMaLichTrinhCa=ISNULL(NVLichTrinhCa,''), @KieuTimCa=ISNULL(Loai,'')
			FROM tblNhanVien 
			LEFT JOIN CC_LichTrinhVaoRa ON tblNhanVien.NVLichTrinhVaoRa = CC_LichTrinhVaoRa.Ma
			WHERE NVMa=@MaNV
		END

	IF(ISNULL(@KieuTimCa,'')='')
	BEGIN
	 SET @KieuTimCa='TTDD'
	END

	IF(@sMaLichTrinhCa<>'' AND @KieuTimCa<>'')
		BEGIN
			CREATE TABLE [dbo].[#tblDSLichTrinh] ([RowID] [int] IDENTITY(1, 1),[LichTrinh] [nvarchar](50)) ON [PRIMARY]
			SET @SQL=
 				'INSERT INTO #tblDSLichTrinh(LichTrinh) SELECT Ngay' + RIGHT('00' + Convert(Varchar,DATEPART(weekday,@D)),2) +' FROM CC_LichTrinhCa 
 				 Where (1=1) AND (Ma=''' + CONVERT(VARCHAR,@sMaLichTrinhCa) +''')'
 				EXEC(@SQL)

			SELECT @NumberRecords = COUNT(*)  FROM #tblDSLichTrinh
			SET @RowCount = 1

			WHILE @RowCount <= @NumberRecords
				BEGIN
					SELECT @sDSCaTrongLichTrinh=LichTrinh 
					FROM #tblDSLichTrinh
					WHERE RowID = @RowCount

					SET @RowCount = @RowCount + 1
				END
			DROP TABLE #tblDSLichTrinh
		END
	ELSE
		BEGIN
			GOTO EndThisSub
		END

	CREATE TABLE [dbo].[#tblData] ([ThoiGian] [datetime], [DDChinhVao] [bit])

	CREATE TABLE [dbo].[#tblDSCa] ([RowID] [int] IDENTITY(1, 1),[CMa] [nvarchar](10),[CVietTat] [nvarchar](10),[CTGBatDau] [datetime],[CTGKetThuc] [datetime],[CQuetTruocCa] [smallint],[CQuetSauCa] [smallint],[CTGKTNghi2] [datetime],[CTGBDNghi2] [datetime]) ON [PRIMARY]
		
	SET @SQL='Insert into #tblDSCa(CMa,CVietTat,CTGBatDau,CTGKetThuc,CQuetTruocCa,CQuetSauCa,CTGKTNghi2,CTGBDNghi2) SELECT CMa,CVietTat,CTGBatDau,CTGKetThuc,CQuetTruocCa,CQuetSauCa,CTGKTNghi2,CTGBDNghi2 FROM tblca ORDER BY CTGBatDau'
	EXEC (@SQL)

	SELECT @NumberRecords = COUNT(*)  FROM #tblDSCa
	SET @RowCount = 1
		
	WHILE @RowCount <= @NumberRecords
		BEGIN
			SELECT @CMa=CMa,@CVietTat=CVietTat,@TGBDCa=CTGBatDau,@TGKTCa=CTGKetThuc,@QuetTruocCa=CQuetTruocCa,@QuetSauCa=CQuetSauCa,@TGKTNghi2=CTGKTNghi2,@TGBDNghi2=CTGBDNghi2
			FROM #tblDSCa
			WHERE RowID = @RowCount

			IF(CHARINDEX(',' + convert(varchar,@CVietTat) + ',',@sDSCaTrongLichTrinh)>0)
				BEGIN
					SET @TGBDCa=@D + @TGBDCa
					SET @TGBDNghi2=@TGBDCa + @TGBDNghi2
					SET @TGKTNghi2=@TGBDCa + @TGKTNghi2
					SET @TGKTCa=@TGBDCa + @TGKTCa
					SET @TGLayDLDau= DateAdd(minute, -@QuetTruocCa, @TGBDCa)
					SET @TGLayDLCuoi=DateAdd(minute, @QuetSauCa, @TGKTCa)

					IF @LoaiNghi = 2 --Nua ca dau
						BEGIN
							SET @TGBDCa = DateAdd(minute, @TGUD3, @TGKTNghi2)
							SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)
						END
    				IF @LoaiNghi = 3 --Nua ca sau
    					BEGIN
							SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa)
							SET @TGKTCa = DateAdd(minute, -@TGUD2, @TGBDNghi2)
    					END
					IF @LoaiNghi = 0 --Khong nghi
						BEGIN
							SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa)
							SET @TGBDNghi2 = DateAdd(minute, -@TGUD2, @TGBDNghi2)
							SET @TGKTNghi2 = DateAdd(minute, @TGUD3, @TGKTNghi2)
							SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)
						END

					---------------------------------------------Nghi theo gio--------------------------------------------------

					-- Khai bao RS DKN theo gio
					CREATE TABLE [dbo].#tblDangkynghitheogioTam ([RowID] [int] IDENTITY(1, 1),[Ngay] [datetime] NOT NULL ,[TGNghi] [datetime] NOT NULL , [Sophut] [int] NOT NULL ,[MaLyDo] [varchar](10) NULL)  ON [PRIMARY]
		
					SET @SQL='Insert into #tblDangkynghitheogioTam(Ngay,TGNghi,Sophut,MaLyDo) SELECT Ngay,TGNghi,Sophut,MaLyDo FROM '+ @tblDangkynghitheogio+ ' WHERE (MaNV=' + CONVERT(nvarchar(30),@MaNV) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + '=Ngay) '
					EXEC (@SQL)

					SELECT @NumberRecords_TG = COUNT(*)  FROM #tblDangkynghitheogioTam
					SET @RowCount_TG = 1

					WHILE @RowCount_TG <= @NumberRecords_TG
						BEGIN
							SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
							FROM #tblDangkynghitheogioTam 
							WHERE RowID = @RowCount

							SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
							SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												
							IF(@TGBDCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
								BEGIN
									SET @TGBDCa=@DKNTGDengio
								END

							IF(@TGKTCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
								BEGIN
									SET @TGKTCa=@DKNTGTugio
								END

							SET @RowCount_TG = @RowCount_TG + 1
						END
					DROP TABLE #tblDangkynghitheogioTam
					-------------------------------------------Ket thuc nghi theo gio----------------------------------------------------
					DELETE #tblData
					
					SET @SQL='INSERT INTO #tblData(ThoiGian, DDChinhVao) SELECT Thoigian, DDChinhVao FROM RecordDataNew 
							 LEFT JOIN tblDauDoc ON  IDM = tblDauDoc.DDMa
								WHERE (IsNull(DDLoaiChamCong,'''')<>''NA'') 
									AND (
											IDCard IN (SELECT CTMaThe FROM tblCapThe 
														WHERE  (CTMaNV =  ' + CONVERT(VARCHAR,@MaNV) +' )
																AND ('+ '''' +  CONVERT(VARCHAR,@D) + '''' + ' BETWEEN CTNgayApDung AND CTNgayKetThuc))
										)
									AND (ThoiGian BETWEEN '+ '''' +  CONVERT(VARCHAR,@TGLayDLDau) + '''' +' AND '+ '''' +  CONVERT(VARCHAR,@TGLayDLCuoi) + '''' +')
									AND (ThoiGian > '+ '''' +  CONVERT(VARCHAR,@TGVe_HomTruoc,21) + '''' + ')
								ORDER BY ThoiGian'
					EXEC(@SQL)

					SELECT @recordCount = COUNT(*)  FROM #tblData

					IF(@recordCount>0)
						BEGIN
							IF(@KieuTimCa='TTXK')
								BEGIN
									IF(@recordCount>1)
										BEGIN
											SELECT TOP 1 @TGDen=ThoiGian  FROM #tblData ORDER BY ThoiGian
											SELECT TOP 1 @TGVe=ThoiGian  FROM #tblData ORDER BY ThoiGian DESC

											IF(
												(@TGDen BETWEEN @TGLayDLDau AND DATEADD (MINUTE, @NguongDM, @TGBDCa))
												AND (@TGVe BETWEEN DATEADD (MINUTE, -@NguongVS, @TGKTCa) AND @TGLayDLCuoi)
												)
						    					BEGIN
						    						SET @ShiftID2=@CMa
						    					END
										END
									IF(@recordCount=1)
										BEGIN
											SELECT TOP 1 @TGDen=ThoiGian  FROM #tblData ORDER BY ThoiGian

											IF(
												(@TGDen BETWEEN @TGLayDLDau AND DATEADD (MINUTE, @NguongDM, @TGBDCa))
												AND CONVERT(VARCHAR,@TGDen,23)=CONVERT(VARCHAR,@D,23)
												)
						    					BEGIN
						    						SET @ShiftID1=@CMa
						    					END
										END
								END
							IF(@KieuTimCa='TTDD')
								BEGIN
									SELECT TOP 1 @TGDen=ThoiGian  FROM #tblData WHERE DDChinhVao=1 ORDER BY ThoiGian
									SELECT TOP 1 @TGVe=ThoiGian  FROM #tblData WHERE DDChinhVao=0 ORDER BY ThoiGian DESC

									IF(
										(@TGDen BETWEEN @TGLayDLDau AND DATEADD (MINUTE, @NguongDM, @TGBDCa))
										AND (@TGVe BETWEEN DATEADD (MINUTE, -@NguongVS, @TGKTCa) AND @TGLayDLCuoi)
										)
										BEGIN
											SET @ShiftID2=@CMa
										END

									IF(
										(@TGDen BETWEEN @TGLayDLDau AND DATEADD (MINUTE, @NguongDM, @TGBDCa))
										OR (@TGVe BETWEEN DATEADD (MINUTE, -@NguongVS, @TGKTCa) AND @TGLayDLCuoi)
										)
										BEGIN
											SET @ShiftID1=@CMa
										END
								END
						END
				END

			SET @RowCount = @RowCount + 1
		END

	DROP TABLE #tblDSCa
	DROP TABLE #tblData
	
	IF(@ShiftID1>0 AND @ShiftID2>0)
		BEGIN
			SET @ShiftID=@ShiftID2
		END
	IF(@ShiftID1>0 AND @ShiftID2=0)
		BEGIN
			SET @ShiftID=@ShiftID1
		END
	IF(@ShiftID1=0 AND @ShiftID2>0)
		BEGIN
			SET @ShiftID=@ShiftID2
		END

	IF (@NV IS NULL)
		BEGIN
			SELECT @DepartmentID=NVMaBP, @PositionID= NVMaCV,@AllowOT=NVTinhLamThem FROM dbo.tblNhanVien WHERE NVMa=@MaNV
			IF @DepartmentID IS NOT NULL 
				BEGIN 
					SET @SQL='INSERT INTO tblBaoCao (BCNgay,BCDay,BCMonth,BCYear,BCMaNV,BCMaBP,BCMaCV,BCMaCa,BCCuaDen,BCTGDen,BCCuaVe,BCTGVe,BCCuaRa,BCTGRa,BCCuaVao,BCTGVao,BCTGLamNgay,BCTGLamToi,BCTGQuaGioNgay,BCTGQuaGioToi,BCTGThemNgay,BCTGThemToi,BCTGRaNgoaiNgay,BCTGRaNgoaiToi,BCTGDiMuonNgay,BCTGDiMuonToi,BCTGVeSomNgay,BCTGVeSomToi,BCTGQuyDinh,BCGhiChu,BCLoai,BCLoaiLamThem,BCTinhLamThem) 
										Values (' + '''' + CONVERT(nvarchar(20),@D) + '''' + ',' + CONVERT(nvarchar(2),DAY(@D)) + ','+ CONVERT(nvarchar(2),MONTH(@D)) + ','+ CONVERT(nvarchar(4),YEAR(@D)) + ',' + CONVERT(nvarchar(20),@MaNV) + ',' + CONVERT(nvarchar(20),@DepartmentID) + ',' + CONVERT(nvarchar(20),@PositionID) + ',' + CONVERT(nvarchar(20),@ShiftID) + ',0,NULL,0,NULL,0,NULL,0,NULL,0,0,0,0,0,0,0,0,0,0,0,0,480,NULL,0,0,' + CONVERT(nvarchar(20),@AllowOT) + ')'
					EXEC(@SQL)
				END
		END
		
	IF (@OldShift<>@ShiftID)
		BEGIN
			UPDATE tblBaoCao SET BCMaCa = @ShiftID WHERE BCNgay=@D AND BCMaNV=@MaNV
		END
		
	--EXEC sphrmvn_TimeKeepingForStaff @StaffID = @MaNV, @D = @D

	EndThisSub:

GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_FindShiftForPart_InOut]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



create PROCEDURE [dbo].[sphrmvn_FindShiftForPart_InOut]
	@MaBP INT,
	@MaNV INT ,
	@D1 DATETIME,
	@D2 DATETIME
AS
BEGIN
	PRINT @MaNV
END

GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_FindShiftForPart_New]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[sphrmvn_FindShiftForPart_New]
	@MaBP INT,
	@MaNV INT ,
	@D1 DATETIME,
	@D2 DATETIME,
	@isDel bit
AS
BEGIN
	DECLARE @MaNVTemp   INT  
	DECLARE @MaBPTemp   INT  
	DECLARE @TempD      DATETIME
	
	DECLARE @SQL AS VARCHAR(500)
	
	DECLARE @DSBoPhan AS VARCHAR(MAX)
	
	
		
		IF (CAST(@MaBP AS INT)>=0) 
		BEGIN
			SET @DSBoPhan=dbo.DonVi_IDs(@MaBP)
			
				DECLARE curRSNV      CURSOR  
				FOR		
				SELECT tblNhanVien.NVMa,tblNhanVien.NVMaBP
				FROM   tblNhanVien 
				WHERE CHARINDEX(',' + CAST(+ tblNhanVien.NVMaBP AS VARCHAR),',' + @DSBoPhan) >0
		END
	ELSE
		BEGIN
			
			
			DECLARE curRSNV      CURSOR  
			FOR	
				SELECT tblNhanVien.NVMa,tblNhanVien.NVMaBP
					FROM   tblNhanVien 
					WHERE tblNhanVien.NVMa = @MaNV
				END

	
	OPEN curRSNV
	
	FETCH NEXT FROM curRSNV INTO @MaNVTemp ,@MaBPTemp                     
	WHILE @@FETCH_STATUS = 0
	BEGIN
	    SET @TempD = @D1
	    WHILE (@TempD <= @D2)
	    BEGIN
	    	SET @SQL=' sphrmvn_FindShift_New ' +  Convert(varchar,@MaBPTemp) + ',' +  Convert(varchar,@MaNVTemp) + ',' + '''' + CONVERT(VARCHAR,@TempD) + '''' + ',' + CONVERT(VARCHAR,@isDel)
	       EXEC( @SQL)
	        SET @TempD = DATEADD(DAY, 1, @TempD)
	    END
	    FETCH NEXT FROM curRSNV INTO @MaNVTemp,@MaBPTemp
	END
	CLOSE curRSNV
	DEALLOCATE curRSNV
END



GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_ImportFromExcelRegisterLeaveDay]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sphrmvn_ImportFromExcelRegisterLeaveDay]
AS
	DECLARE @SQL AS VARCHAR(max)
	
	DECLARE @i        INT
	
	DECLARE @Start    DATETIME
	DECLARE @Finish   DATETIME
	
	DECLARE @StartT   DATETIME
	DECLARE @FinishT  DATETIME
	
	
	DECLARE @Thang    DATETIME
	DECLARE @GiaTri   VARCHAR(50)
	
	DECLARE @LyDoNghiVT   VARCHAR(50)
	DECLARE @BCTGNghi   FLOAT
	DECLARE @BCLoaiDKN   VARCHAR(50)

	
	SELECT @Thang = CONVERT(DATETIME, RIGHT(F2, 4) + LEFT(F2, 2) + '01')
	FROM   tblImportRegisterLeaveDay
	WHERE  F2 LIKE '%/%'
	
	EXEC cp_LayStartDate_FinishDate_OfBCC @Thang,
	     @StartT OUTPUT,
	     @FinishT OUTPUT
	
	DECLARE @NamI     VARCHAR(5),
	        @ThangI   VARCHAR(5),
	        @NamII    VARCHAR(5),
	        @ThangII  VARCHAR(5)
	
	SET @NamI = CONVERT(VARCHAR(5), YEAR(@StartT))
	SET @ThangI = RIGHT('00' + CONVERT(VARCHAR(5), MONTH(@StartT)), 2)
	SET @NamII = CONVERT(VARCHAR(5), YEAR(@FinishT))
	SET @ThangII = RIGHT('00' + CONVERT(VARCHAR(5), MONTH(@FinishT)), 2)
	
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_Value]') AND type in (N'U'))
	DROP TABLE [dbo].[tmp_Value]


	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_Values]') AND type in (N'U'))
	DROP TABLE [dbo].[tmp_Values]
	
	CREATE TABLE tmp_Value
	(
		Giatri VARCHAR(5)
	)
	CREATE TABLE tmp_Values
	(
		NVMa    INT,
		MaNV    VARCHAR(10),
		Ngay    DATETIME,
		Giatri  VARCHAR(50),
		LyDoNghiVT    VARCHAR(50),
		BCTGNghi    FLOAT,
		BCLoaiDKN    VARCHAR(50)
	)
	
	DECLARE @MaNV  VARCHAR(10)
	DECLARE @NVMa  INT 
	
	
	
	DECLARE Curr   CURSOR  
	FOR
	    SELECT nv.NVMa,
	           nv  .NVMaNV
	    FROM   tblImportRegisterLeaveDay s
				   INNER JOIN tblNhanVien nv
	                ON  nv.NVMaNV = s.F2
	    WHERE  NOT F2 IS NULL
	           AND NOT F2 LIKE '%/%'
	           AND nv.NVNgayRa >  = @StartT
	           AND nv.NVNgayVao <  = @FinishT
	
	OPEN Curr
	FETCH NEXT FROM Curr INTO @NVMa,@MaNV                   
	WHILE @@FETCH_STATUS = 0
	BEGIN
	    DELETE 
	    FROM   tmp_Value
	    
	    DELETE 
	    FROM   tmp_Values
	    
	    SET @i = 6
	    SET @Start = @StartT
	    SET @Finish = @FinishT
	    
	    WHILE @Start <= @Finish
	    BEGIN
	        DELETE 
	        FROM   tmp_Value
	        
	        
	        SET @SQL= 'insert into tmp_Value(GiaTri) Select distinct [F' + Convert(varchar,@i) + 
	                 '] FROM tblImportRegisterLeaveDay WHERE F2 = ' + '''' + Convert(varchar,@MaNV) + '''' + 
	                 ''
	        
	        EXEC (@SQL )
	        
	        SET @GiaTri = ISNULL(
	                (
	                    SELECT *
	                    FROM   tmp_Value
	                ),
	                ''
	        )
	        
	        -- Nghi nua ca Sau
	        IF(CHARINDEX('1/',@GiaTri)>0)
	        BEGIN
    			SET @LyDoNghiVT   =LTrim(Rtrim(@GiaTri))
	        	SET @BCTGNghi  =0.5
	        	SET @BCLoaiDKN  ='0003'
	        END
			ELSE
					-- Nghi nua ca dau
			IF(CHARINDEX('/1',@GiaTri)>0)
			BEGIN
    			SET @LyDoNghiVT   =LTrim(Rtrim(@GiaTri))
    			SET @BCTGNghi  =0.5
    			SET @BCLoaiDKN  ='0002'
			END
			ELSE
				--Ca ca
				BEGIN
    				SET @LyDoNghiVT   =LTrim(Rtrim(@GiaTri))
    				SET @BCTGNghi  =1
    				SET @BCLoaiDKN  =CASE WHEN @LyDoNghiVT='X' THEN '' ELSE '0001' END 				
				END
			SET @LyDoNghiVT   =REPLACE(@LyDoNghiVT,'1/','')
			SET @LyDoNghiVT   =REPLACE(@LyDoNghiVT,'/1','')
			
	        
	        INSERT INTO tmp_Values
	          (
	            NVMa,
	            MaNV,
	            Ngay,
	            Giatri,
	            LyDoNghiVT,
				BCTGNghi,
				BCLoaiDKN 
	          )
	        VALUES
	          (
	            @NVMa,
	            @MaNV,
	            @Start,
	            @GiaTri,
	            @LyDoNghiVT,
	            @BCTGNghi,
	            @BCLoaiDKN
	          )
	        SET @Start = DATEADD(DAY, 1, @Start)
	        SET @i = @i + 1
	    END
	    DELETE 
	    FROM   tmp_Values
	    WHERE  Giatri = ''
	    
	    ---- Cap nhat lai gia tri
	    --UPDATE tmp_Values
	    --SET    LyDoNghiVT = tc.CMa
	    --FROM   tmp_Values v
	    --       INNER JOIN tblCa tc
	    --            ON  v.Giatri COLLATE DATABASE_DEFAULT  = tc.CVietTat COLLATE DATABASE_DEFAULT
	    
	    DELETE 
	    FROM   tmp_Values
	    WHERE  LyDoNghiVT IS NULL
	    
	    UPDATE tblBaoCao
	    SET
	    	
	    	BCLoaiNgayNghi ='',
	    	BCLydonghi = '',
	    	BCTGNghi = 0,
	    	BCLoaiDKN = ''
	    FROM tblBaoCao INNER JOIN tmp_Values v
	                ON  V.NVMa = tblBaoCao.BCMaNV
	                AND v.Ngay = tblBaoCao.BCNgay
	    WHERE  tblBaoCao.BCMaNV = @NVMa  AND v.LyDoNghiVT='X'
	    
	    
	    DELETE 
	    FROM   tmp_Values
	    WHERE  LyDoNghiVT ='X'
	    
	    --Cap nhat vao CSDL
	    UPDATE tblBaoCao
	    SET    BCLydonghi = v.LyDoNghiVT ,BCTGNghi=v.BCTGNghi,BCLoaiDKN=v.BCLoaiDKN,tblBaoCao.BCGhiChu = Case when v.BCTGNghi=0.5 then '1/2' Else '' END +v.LyDoNghiVT
	    FROM   tblBaoCao r
	           INNER JOIN tmp_Values v
	                ON  V.NVMa = r.BCMaNV
	                AND v.Ngay = r.BCNgay
	    WHERE  r.BCMaNV = @NVMa 
	    
	    
	    
	    
	    DECLARE @NgayInsertRD  DATETIME
	    DECLARE @MaCa          INT 
	    
		DECLARE @LyDoNghiVT_New   VARCHAR(50)
		DECLARE @BCTGNghi_New   FLOAT
		DECLARE @BCLoaiDKN_New   VARCHAR(50)	    
	     
	    DECLARE CurInsert           CURSOR  
	    FOR
	        SELECT Ngay,
	               tmp_Values.LyDoNghiVT,
	               tmp_Values.BCTGNghi,
	               tmp_Values.BCLoaiDKN           
	        FROM   tmp_Values
	        WHERE  Ngay NOT IN (SELECT DISTINCT BCNgay
	                            FROM   tblBaoCao
	                            WHERE  BCMaNV = @NVMa)
	               AND MONTH(Ngay) = @ThangI
	    
	    OPEN CurInsert 
	    FETCH NEXT FROM CurInsert INTO @NgayInsertRD,@LyDoNghiVT_New,@BCTGNghi_New,@BCLoaiDKN_New                                 
	    WHILE @@FETCH_STATUS = 0
	    BEGIN
	        INSERT INTO tblBaoCao
	          (
	            BCNgay,
	            BCMaNV,
	            BCMaBP,
	            BCMaCV,
	            BCMaCa,
	            BCCuaDen,
	            BCTGDen,
	            BCCuaVe,
	            BCTGVe,
	            BCCuaRa,
	            BCTGRa,
	            BCCuaVao,
	            BCTGVao,
	            BCTGLamNgay,
	            BCTGLamToi,
	            BCTGQuaGioNgay,
	            BCTGQuaGioToi,
	            BCTGThemNgay,
	            BCTGThemToi,
	            BCTGRaNgoaiNgay,
	            BCTGRaNgoaiToi,
	            BCTGDiMuonNgay,
	            BCTGDiMuonToi,
	            BCTGVeSomNgay,
	            BCTGVeSomToi,
	            BCTGQuyDinh,
	            BCGhiChu,
	            BCLoai,
	            BCLoaiLamThem,
	            BCTinhLamThem,
	            BCLydonghi,
	            BCTGNghi,
	            BCLoaiDKN
	          )
	        SELECT @NgayInsertRD,
	               NVMa,
	               NVMaBP,
	               NVMaCV,
	               nv.NVMaCa,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               480,
	                Case when @BCTGNghi_New=0.5 then '1/2' Else '' END + @LyDoNghiVT_New,
	               0,
	               0,
	               nv.NVTinhLamThem,
					@LyDoNghiVT_New,
					@BCTGNghi_New,
					@BCLoaiDKN_New
	        FROM   tblNhanVien nv
	        WHERE  NVMa = @NVMa
	        
	        FETCH NEXT FROM CurInsert INTO @NgayInsertRD,@LyDoNghiVT_New,@BCTGNghi_New,@BCLoaiDKN_New
	    END
	    CLOSE CurInsert
	    DEALLOCATE CurInsert
	    --Thang sau
	    --DECLARE @NgayInsertRD DATETIME
	    --		DECLARE @MaCa INT
	    DECLARE CurInsertII  CURSOR  
	    FOR
	        SELECT DISTINCT Ngay,
	               tmp_Values.LyDoNghiVT,
	               tmp_Values.BCTGNghi,
	               tmp_Values.BCLoaiDKN        
	        FROM   tmp_Values
	        WHERE  Ngay NOT IN (SELECT DISTINCT BCNgay
	                            FROM   tblBaoCao
	                            WHERE  BCMaNV = @NVMa)
	               AND MONTH(Ngay) = @ThangII
	    
	    OPEN CurInsertII 
	    FETCH NEXT FROM CurInsertII INTO @NgayInsertRD,@LyDoNghiVT_New,@BCTGNghi_New,@BCLoaiDKN_New                     
	    WHILE @@FETCH_STATUS = 0
	    BEGIN
	        INSERT INTO tblBaoCao
	          (
	            BCNgay,
	            BCMaNV,
	            BCMaBP,
	            BCMaCV,
	            BCMaCa,
	            BCCuaDen,
	            BCTGDen,
	            BCCuaVe,
	            BCTGVe,
	            BCCuaRa,
	            BCTGRa,
	            BCCuaVao,
	            BCTGVao,
	            BCTGLamNgay,
	            BCTGLamToi,
	            BCTGQuaGioNgay,
	            BCTGQuaGioToi,
	            BCTGThemNgay,
	            BCTGThemToi,
	            BCTGRaNgoaiNgay,
	            BCTGRaNgoaiToi,
	            BCTGDiMuonNgay,
	            BCTGDiMuonToi,
	            BCTGVeSomNgay,
	            BCTGVeSomToi,
	            BCTGQuyDinh,
	            BCGhiChu,
	            BCLoai,
	            BCLoaiLamThem,
	            BCTinhLamThem,
	            BCLydonghi,
	            BCTGNghi,
	            BCLoaiDKN
	          )
	        SELECT @NgayInsertRD,
	               NVMa,
	               NVMaBP,
	               NVMaCV,
	               nv.NVMaCa,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               480,
	               Case when @BCTGNghi_New=0.5 then '1/2' Else '' END + @LyDoNghiVT_New,
	               0,
	               0,
	               nv.NVTinhLamThem,
					@LyDoNghiVT_New,
					@BCTGNghi_New,
					@BCLoaiDKN_New
	        FROM   tblNhanVien nv
	        WHERE  NVMa = @NVMa
	        
	        FETCH NEXT FROM CurInsertII INTO @NgayInsertRD,@LyDoNghiVT_New,@BCTGNghi_New,@BCLoaiDKN_New
	    END
	    CLOSE CurInsertII
	    DEALLOCATE CurInsertII
	    
	    
	    FETCH NEXT FROM Curr INTO @NVMa,@MaNV
	END
	CLOSE Curr
	DEALLOCATE Curr

GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_ImportFromExcelShift]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sphrmvn_ImportFromExcelShift]
AS
	DECLARE @i        INT
	
	DECLARE @Start    DATETIME
	DECLARE @Finish   DATETIME
	
	DECLARE @StartT   DATETIME
	DECLARE @FinishT  DATETIME
	
	
	DECLARE @Thang    DATETIME
	DECLARE @GiaTri   VARCHAR(5)
	SELECT @Thang = CONVERT(DATETIME, RIGHT(F2, 4) + LEFT(F2, 2) + '01')
	FROM   tblImportShift
	WHERE  F2 LIKE '%/%'
	
	EXEC cp_LayStartDate_FinishDate_OfBCC @Thang,
	     @StartT OUTPUT,
	     @FinishT OUTPUT
	
	DECLARE @NamI     VARCHAR(5),
	        @ThangI   VARCHAR(5),
	        @NamII    VARCHAR(5),
	        @ThangII  VARCHAR(5)
	
	SET @NamI = CONVERT(VARCHAR(5), YEAR(@StartT))
	SET @ThangI = RIGHT('00' + CONVERT(VARCHAR(5), MONTH(@StartT)), 2)
	SET @NamII = CONVERT(VARCHAR(5), YEAR(@FinishT))
	SET @ThangII = RIGHT('00' + CONVERT(VARCHAR(5), MONTH(@FinishT)), 2)
	
	CREATE TABLE #Value
	(
		Giatri VARCHAR(5)
	)
	CREATE TABLE #Values
	(
		NVMa    INT,
		MaNV    VARCHAR(10),
		Ngay    DATETIME,
		Giatri  VARCHAR(5),
		MaCa    INT
	)
	
	DECLARE @MaNV  VARCHAR(10)
	DECLARE @NVMa  INT 
	
	
	
	DECLARE Curr   CURSOR  
	FOR
	    SELECT nv.NVMa,
	           nv  .NVMaNV
	    FROM   tblImportShift s
	           INNER JOIN tblNhanVien nv
	                ON  nv.NVMaNV = s.F2
	    WHERE  NOT F2 IS NULL
	           AND NOT F2 LIKE '%/%'
	           AND nv.NVNgayRa >  = @StartT
	           AND nv.NVNgayVao <  = @FinishT
	
	OPEN Curr
	FETCH NEXT FROM Curr INTO @NVMa,@MaNV                   
	WHILE @@FETCH_STATUS = 0
	BEGIN
	    DELETE 
	    FROM   #Value
	    
	    DELETE 
	    FROM   #Values
	    
	    SET @i = 6
	    SET @Start = @StartT
	    SET @Finish = @FinishT
	    
	    WHILE @Start <= @Finish
	    BEGIN
	        DELETE 
	        FROM   #Value
	        
	        EXEC (
	                 'insert into #value(GiaTri) Select distinct [F' + @i + 
	                 '] FROM tblImportShift WHERE F2 = ' + '''' + @MaNV + '''' + 
	                 ''
	             )
	        
	        SET @GiaTri = ISNULL(
	                (
	                    SELECT *
	                    FROM   #Value
	                ),
	                ''
	            )
	        
	        INSERT INTO #Values
	          (
	            NVMa,
	            MaNV,
	            Ngay,
	            Giatri
	          )
	        VALUES
	          (
	            @NVMa,
	            @MaNV,
	            @Start,
	            @GiaTri
	          )
	        SET @Start = DATEADD(DAY, 1, @Start)
	        SET @i = @i + 1
	    END
	    DELETE 
	    FROM   #Values
	    WHERE  Giatri = ''
	    
	    UPDATE #Values
	    SET    MaCa = tc.CMa
	    FROM   #Values v
	           INNER JOIN tblCa tc
	                ON  v.Giatri COLLATE DATABASE_DEFAULT  = tc.CVietTat COLLATE DATABASE_DEFAULT
	    
	    DELETE 
	    FROM   #Values
	    WHERE  MaCa IS NULL
	    
	    UPDATE tblBaoCao
	    SET    BCMaCa = v.MaCa
	    FROM   tblBaoCao r
	           INNER JOIN #Values v
	                ON  V.NVMa = r.BCMaNV
	                AND v.Ngay = r.BCNgay
	    WHERE  r.BCMaNV = @NVMa 
	    
	    
	    
	    
	    DECLARE @NgayInsertRD  DATETIME
	    DECLARE @MaCa          INT  
	    DECLARE CurInsert           CURSOR  
	    FOR
	        SELECT Ngay,
	               MaCa             
	        FROM   #Values
	        WHERE  Ngay NOT IN (SELECT DISTINCT BCNgay
	                            FROM   tblBaoCao
	                            WHERE  BCMaNV = @NVMa)
	               AND MONTH(Ngay) = @ThangI
	    
	    OPEN CurInsert 
	    FETCH NEXT FROM CurInsert INTO @NgayInsertRD,@MaCa                                    
	    WHILE @@FETCH_STATUS = 0
	    BEGIN
	        INSERT INTO tblBaoCao
	          (
	            BCNgay,
	            BCMaNV,
	            BCMaBP,
	            BCMaCV,
	            BCMaCa,
	            BCCuaDen,
	            BCTGDen,
	            BCCuaVe,
	            BCTGVe,
	            BCCuaRa,
	            BCTGRa,
	            BCCuaVao,
	            BCTGVao,
	            BCTGLamNgay,
	            BCTGLamToi,
	            BCTGQuaGioNgay,
	            BCTGQuaGioToi,
	            BCTGThemNgay,
	            BCTGThemToi,
	            BCTGRaNgoaiNgay,
	            BCTGRaNgoaiToi,
	            BCTGDiMuonNgay,
	            BCTGDiMuonToi,
	            BCTGVeSomNgay,
	            BCTGVeSomToi,
	            BCTGQuyDinh,
	            BCGhiChu,
	            BCLoai,
	            BCLoaiLamThem,
	            BCTinhLamThem
	          )
	        SELECT @NgayInsertRD,
	               NVMa,
	               NVMaBP,
	               NVMaCV,
	               @MaCa,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               480,
	               NULL,
	               0,
	               0,
	               nv.NVTinhLamThem
	        FROM   tblNhanVien nv
	        WHERE  NVMa = @NVMa
	        
	        FETCH NEXT FROM CurInsert INTO @NgayInsertRD,@MaCa
	    END
	    CLOSE CurInsert
	    DEALLOCATE CurInsert
	    --Thang sau
	    --DECLARE @NgayInsertRD DATETIME
	    --		DECLARE @MaCa INT
	    DECLARE CurInsertII  CURSOR  
	    FOR
	        SELECT DISTINCT Ngay,
	               MaCa      
	        FROM   #Values
	        WHERE  Ngay NOT IN (SELECT DISTINCT BCNgay
	                            FROM   tblBaoCao
	                            WHERE  BCMaNV = @NVMa)
	               AND MONTH(Ngay) = @ThangII
	    
	    OPEN CurInsertII 
	    FETCH NEXT FROM CurInsertII INTO @NgayInsertRD,@MaCa                             
	    WHILE @@FETCH_STATUS = 0
	    BEGIN
	        INSERT INTO tblBaoCao
	          (
	            BCNgay,
	            BCMaNV,
	            BCMaBP,
	            BCMaCV,
	            BCMaCa,
	            BCCuaDen,
	            BCTGDen,
	            BCCuaVe,
	            BCTGVe,
	            BCCuaRa,
	            BCTGRa,
	            BCCuaVao,
	            BCTGVao,
	            BCTGLamNgay,
	            BCTGLamToi,
	            BCTGQuaGioNgay,
	            BCTGQuaGioToi,
	            BCTGThemNgay,
	            BCTGThemToi,
	            BCTGRaNgoaiNgay,
	            BCTGRaNgoaiToi,
	            BCTGDiMuonNgay,
	            BCTGDiMuonToi,
	            BCTGVeSomNgay,
	            BCTGVeSomToi,
	            BCTGQuyDinh,
	            BCGhiChu,
	            BCLoai,
	            BCLoaiLamThem,
	            BCTinhLamThem
	          )
	        SELECT @NgayInsertRD,
	               NVMa,
	               NVMaBP,
	               NVMaCV,
	               @MaCa,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               NULL,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               0,
	               480,
	               NULL,
	               0,
	               0,
	               nv.NVTinhLamThem
	        FROM   tblNhanVien nv
	        WHERE  NVMa = @NVMa
	        
	        FETCH NEXT FROM CurInsertII INTO @NgayInsertRD,@MaCa
	    END
	    CLOSE CurInsertII
	    DEALLOCATE CurInsertII
	    
	    
	    FETCH NEXT FROM Curr INTO @NVMa,@MaNV
	END
	CLOSE Curr
	DEALLOCATE Curr

GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_LocDuLieu]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/************************************************************
*  Routine       :	sphrmvn_LocDuLieu
*  Created by    :  Duong Van Quyet , at 24/01/2011 - 10:53:23  
*  Machine       :  DIGISOFT
*  Description   :  Loc du lieu theo dieu kien
*  Parameters    :  @StaffID int, @D datetime
************************************************************/
CREATE PROCEDURE [dbo].[sphrmvn_LocDuLieu]
	@StaffID INT ,
	@D DATETIME
AS
BEGIN
	DECLARE @Trangthai       SMALLINT
	-- Bien tam dem tong so ban ghi tra ve
	DECLARE @recordCount     SMALLINT
	
	-- Luu kieu xac dinh trang thai vao ra
	DECLARE @OriginalStatus  SMALLINT
	
	-- Kieu tinh gio
	DECLARE @TypeCalculator  SMALLINT
	
	-- Bien luu trang thai tam khi loc du lieu
	DECLARE @OldStatus       BIT 
	
	-- Trong bang Recorddata
	DECLARE @IDM             SMALLINT -- dau doc nao
	DECLARE @ThoiGian        DATETIME 
	DECLARE @IDCard          NVARCHAR(15)
	DECLARE @Status          BIT                                                       
	DECLARE @HandData        BIT
	DECLARE @Pass            BIT
	
	
	DECLARE @i               INT
	DECLARE @RecordData      NVARCHAR(30) 	
	DECLARE @tblBaocao       NVARCHAR(20)
	
	DECLARE @SQL             VARCHAR(1000)
	
	SET @Trangthai = 0
	
	-- Lay tham so nguong lam ngay 06:00:00
	--SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='THRESHOLDDAY'
	--SET @ThresholdDay=CONVERT(Datetime,@SQL)
	---- Lay tham so nguong lam ngay 22:00:00
	--SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='THRESHOLDNIGHT'
	--SET @ThresholdNight=CONVERT(Datetime,@SQL)
	
	-- Cach lay du lieu quet: Xem ke trang thai hoac lay tu dau doc  (1: Xen ke, 0: dau doc)
	
	SELECT @SQL = TSGiaTri
	FROM   tblThamSo
	WHERE  TSTen = 'ORIGINALSTATUS'
	
	SET @OriginalStatus = CONVERT(INT, @SQL)
	
	-- Cach tinh cho trang thai vao ra: 0: Tinh tat cac cac cap vao ra; 1: Vao dau tien ra cuoi cung;
	-- 2:Quet dau tien Quet cuoi cung
	SELECT @SQL = TSGiaTri
	FROM   tblThamSo
	WHERE  TSTen = 'TYPECALCULATOR'
	
	SET @TypeCalculator = CONVERT(INT, @SQL)
	
	Select @TypeCalculator =tblCa.CLichtrinhVaora
	  From tblBaoCao INNER JOIN tblCa ON tblBaoCao.BCMaCa=tblCa.CMa  where BCMaNV=@StaffID and BCNgay=@D 
	
	SET @RecordData = CASE 
	                       WHEN LEN(MONTH(@D)) = 2 THEN 'RecordData' + CONVERT(NVARCHAR, YEAR(@D)) 
	                            + '_' + CONVERT(NVARCHAR, MONTH(@D))
	                       ELSE 'RecordData' + CONVERT(NVARCHAR, YEAR(@D)) +
	                            '_0' + CONVERT(NVARCHAR, MONTH(@D))
	                  END
	
	--SET @tblBaocao = case when LEN(Month(@D))=2 then
	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	-- else
	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	--END
	SET @tblBaocao = 'tblBaoCao'
	
	SET @RecordData = CASE 
	                       WHEN LEN(MONTH(@D)) = 2 THEN 'RecordData' + CONVERT(NVARCHAR, YEAR(@D)) 
	                            + '_' + CONVERT(NVARCHAR, MONTH(@D))
	                       ELSE 'RecordData' + CONVERT(NVARCHAR, YEAR(@D)) +
	                            '_0' + CONVERT(NVARCHAR, MONTH(@D))
	                  END
	
	--SET @tblBaocao = case when LEN(Month(@D))=2 then
	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	-- else
	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	--END
	SET @tblBaocao = 'tblBaoCao'
	
	
	
	DECLARE curRS                                                              CURSOR SCROLL 
	FOR
	    SELECT IDM,
	           IDCard,
	           ThoiGian,
	           STATUS,
	           HandData,
	           Pass                                                            
	    FROM   #tblLocdulieutam
	    ORDER BY
	           Thoigian
	
	OPEN curRS
	FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass  
	
	-- Thay doi trang thai vao ra
	--- Neu la trang thai dau doc
	IF @OriginalStatus <> 0
	BEGIN
	    -- Neu khong phai loai tinh gio la quyet dau, quyet cuoi thi loc du lieu
	    IF @TypeCalculator <> 2
	    BEGIN
	        -- Viet code loc du lieu o day
	        SELECT @recordCount = COUNT(idm)
	        FROM   #tblLocdulieutam 
	        
	        -- Neu co du lieu moi loc
	        IF @recordCount > 1
	        BEGIN
	            -- Xoa cac ban ghi ra dau tien
	            WHILE @@FETCH_STATUS = 0
	                  AND @Status = 0
	                  AND @recordCount > 1
	            BEGIN
	                DELETE #tblLocdulieutam
	                WHERE  ThoiGian = @ThoiGian
	                
	                SELECT @recordCount = COUNT(idm)
	                FROM   #tblLocdulieutam
	                
	                FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	                @Pass
	            END
	            
	            -- Xoa cac ban ghi vao cuoi cung
	            SELECT @recordCount = COUNT(idm)
	            FROM   #tblLocdulieutam
	            
	            FETCH LAST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	            @Pass
	            WHILE @@FETCH_STATUS = 0
	                  AND @Status = 1
	                  AND @recordCount > 1
	            BEGIN
	                DELETE #tblLocdulieutam
	                WHERE  ThoiGian = @ThoiGian
	                
	                SELECT @recordCount = COUNT(idm)
	                FROM   #tblLocdulieutam
	                
	                FETCH PRIOR FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	                @Pass
	            END
	            
	            -- Xoa cac ban ghi lap nhau - Kiem tra ban ghi sau trung trang thai ban ghi truoc
	            FETCH FIRST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	            @Pass
	            SET @OldStatus = @Status
	            FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	            @Pass
	            WHILE @@FETCH_STATUS = 0
	            BEGIN
	                IF (@Status = @OldStatus)
	                BEGIN
	                    IF @Status = 1
	                        DELETE #tblLocdulieutam
	                        WHERE  ThoiGian = @ThoiGian
	                END
	                ELSE
	                BEGIN
	                    SET @OldStatus = @Status
	                END
	                FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	                @Pass
	            END
	            
	            -- Xoa cac ban ghi lap nhau - Kiem tra ban ghi truoc trung trang thai ban ghi sau
	            FETCH LAST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	            @Pass
	            IF @@FETCH_STATUS = 0
	            BEGIN
	                SET @OldStatus = @Status
	                FETCH PRIOR FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	                @Pass
	                WHILE @@FETCH_STATUS = 0
	                BEGIN
	                    --SELECT * FROM #tblLocdulieutam
	                    
	                    IF @Status = @OldStatus
	                    BEGIN
	                        IF @Status = 0
	                            DELETE #tblLocdulieutam
	                            WHERE  ThoiGian = @ThoiGian
	                    END
	                    ELSE
	                    BEGIN
	                        SET @OldStatus = @Status
	                    END
	                    
	                    --CLOSE curRS
	                    
	                    --CURSOR Scroll FOR
	                    --	SELECT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM #tblLocdulieutam ORDER BY Thoigian
	                    --OPEN curRS 
	                    
	                    FETCH PRIOR FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,
	                    @HandData,
	                    @Pass
	                END
	            END
	        END
	    END-- Neu kieu tinh gio là cac cap vao ra hoac vao dau ra cuoi
	    ELSE
	    BEGIN
	        SELECT @recordCount = COUNT(idm)
	        FROM   #tblLocdulieutam
	    END
	END-- Neu la xen ke trang thai
	ELSE
	BEGIN
	    WHILE @@FETCH_STATUS = 0
	    BEGIN
	        SET @Trangthai = 1 -@Trangthai
	        SET @SQL = 'Update ' + @RecordData + ' set Status=' + CONVERT(VARCHAR, @Trangthai) 
	            +
	            ' where (IDCard=' + '''' + @IDCard + '''' +
	            ') and (ThoiGian=' 
	            + '''' + CONVERT(VARCHAR, @ThoiGian, 121) + '''' + ')'
	        
	        EXEC (@SQL)
	        
	        SET @SQL = 'Update #tblLocdulieutam set Status=' + CONVERT(VARCHAR, @Trangthai) 
	            +
	            ' where (IDCard=' + '''' + @IDCard + '''' +
	            ') and (ThoiGian=' 
	            + '''' + CONVERT(VARCHAR, @ThoiGian, 121) + '''' + ')'
	        
	        EXEC (@SQL)
	        
	        FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	        @Pass
	    END
	END
	
	CLOSE curRS
	OPEN curRS
	FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass 
	
	-- Sau khi loc ma khong co ban ghi nao thi cap nhat gia tri ve mac dinh
	SELECT @recordCount = COUNT(idm)
	FROM   #tblLocdulieutam
	
	IF @recordCount = 0
	BEGIN
	    --SET @SQL='UPDATE ' + @tblBaocao +' SET BCCuaDen=0,BCTGDen=NULL,BCCuaVe=0,BCTGVe=NULL,BCCuaRa=0,BCTGRa=NULL,BCCuaVao=0,BCTGVao=NULL,BCTGLamNgay=0,BCTGLamToi=0,BCTGQuaGioNgay=0,BCTGQuaGioToi=0,BCTGThemNgay=0,BCTGThemToi=0,BCTGRaNgoaiNgay=0,BCTGRaNgoaiToi=0,BCTGDiMuonNgay=0,BCTGDiMuonToi=0,BCTGVeSomNgay=0,BCTGVeSomToi=0,BCTGQuyDinh=480,BCLoai=0,BCLoaiLamThem=0,BCGhiChu='''' WHERE (BCMaNV=' + CONVERT(nvarchar(20),@StaffID) + ') AND (BCNgay='+ '''' + CONVERT(nvarchar(20),@D) + '''' +')'
	    --EXEC (@SQL)
	    EXEC sphrm_UpdateDefaultValuetblBaocao @tblBaocao,
	         @StaffID,
	         @D
	    
	    
	    --CLOSE curRS
	    --DEALLOCATE curRS
	    GOTO Endthissub
	END
	
	-- Loc du lieu theo dieu kien tinh toan
	IF @TypeCalculator <> 2
	BEGIN
	    SELECT @recordCount = COUNT(idm)
	    FROM   #tblLocdulieutam
	    
	    FETCH FIRST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    IF (@@FETCH_STATUS = 0)
	    BEGIN
	        IF @Status = 0
	           AND (@recordCount > 1)
	            DELETE 
	            FROM   #tblLocdulieutam
	            WHERE  ThoiGian = @ThoiGian
	    END
	    
	    SELECT @recordCount = COUNT(idm)
	    FROM   #tblLocdulieutam
	    
	    IF (@recordCount > 1)
	    BEGIN
	        FETCH LAST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	        @Pass
	        IF (@@FETCH_STATUS = 0)
	        BEGIN
	            IF @Status = 1
	                DELETE 
	                FROM   #tblLocdulieutam
	                WHERE  ThoiGian = @ThoiGian
	        END
	    END
	END
	
	IF (@TypeCalculator <> 0)
	BEGIN
	    SET @i = 1
	    SELECT @recordCount = COUNT(idm)
	    FROM   #tblLocdulieutam
	    
	    FETCH FIRST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    WHILE @i <= @recordCount -2
	    BEGIN
	        DELETE 
	        FROM   #tblLocdulieutam
	        WHERE  ThoiGian = @ThoiGian
	        
	        FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,
	        @Pass
	        SET @i = @i + 1
	    END
	END
	   EndThisSub:
	
	CLOSE curRS
	DEALLOCATE curRS
END
GO
/****** Object:  StoredProcedure [dbo].[sphrmvnbc_BaoCaoLamThem]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/************************************************************
*  Routine       :	sphrmvnbc_BaoCaoLamThem
*  Created by    :  Duong Van Quyet , at 15/03/2011 - 17:08:21  
*  Machine       :  DIGISOFT
*  Description   :  Bao cao lam them
*  Parameters    :  @TuNgay datetime, @DenNgay datetime,
                    @MaBP varchar(10), @MaNV varchar(10)
************************************************************/

CREATE    PROC [dbo].[sphrmvnbc_BaoCaoLamThem]
@MaNV VARCHAR(10),
@MaBP VARCHAR(10),
@TuNgay DATETIME,
@DenNgay DATETIME

AS
CREATE TABLE #tbllamthemTam
(
	[BCNgay]        [datetime] NOT NULL,
	[NvMa]          [int] NOT NULL,
	[BCTGThemNgay]  [varchar](10) NOT NULL,
	[BCTGThemToi]   [varchar](10) NOT NULL
)
DECLARE @i          INT
DECLARE @SQL        VARCHAR(4000)
DECLARE @tblbaocao  VARCHAR(100)
SET @i = MONTH(@TuNgay)

IF ( @MaNV > 0)
BEGIN
    SET @SQL = 
        'INSERT INTO #tbllamthemTam
	SELECT BCNgay,BCMaNV,Case when BCDaXacNhanLamThem=1 then   isnull((BCTGThemNgay),'''') else 0 end  as BCTGThemNgay,Case when BCDaXacNhanLamThem=1 then   isnull((BCTGThemToi),'''') else 0 end  as BCTGThemToi From tblBaocao Where BCNgay BETWEEN '
        + '''' + CONVERT(VARCHAR, @TuNgay, 23) + '''' + ' AND ' + '''' +
        CONVERT(VARCHAR, @DenNgay, 23) + ''''
        + ' AND BCMaNV = ' + @MaNV
END
ELSE
BEGIN
    SET @SQL = 
        'INSERT INTO #tbllamthemTam
	SELECT bc.BCNgay,bc.BCMaNV,isnull((BCTGThemNgay),'''') as BCTGThemNgay ,isnull((BCTGThemToi),'''') as BCTGThemToi From tblBaocao bc INNER JOIN tblNhanVien tnv ON bc.BCMaNV = tnv.NVMa
	Where tnv.NVMaBP IN (' + dbo.DonVi_IDs(@MaBP) + ') And bc.BCNgay BETWEEN '
        + '''' + CONVERT(VARCHAR, @TuNgay, 23) + '''' + ' AND ' + '''' +
        CONVERT(VARCHAR, @DenNgay, 23) + ''''
END
EXEC (@SQL)

SELECT tnv.NVMaNV,
       tnv.NVHoTen,
       tbp.BPTen,
       tcv.CVTen,
       (CONVERT(VARCHAR, DAY(Lt.BCNgay))) AS Ngay,
       Lt.BCTGThemNgay,
       Lt.BCTGThemToi
       INTO #PPP
FROM   tblNhanVien tnv
       INNER JOIN #tbllamthemTam Lt
            ON  tnv.NVMa = Lt.NvMa
       INNER JOIN tblBoPhan tbp
            ON  tbp.BPMa = tnv.NVMaBP
       LEFT JOIN tblChucVu tcv
            ON  tcv.CVMa = tnv.NVMaCV
  
IF EXISTS (
       SELECT *
       FROM   sys.objects
       WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[bc_LamThem]')
              AND TYPE IN (N'U')
   )
    DROP TABLE [dbo].[bc_LamThem]
  
DECLARE @sSelect  AS VARCHAR(MAX)
DECLARE @TempD    AS DATETIME
  
SET @TempD = @TuNgay
SET @sSelect = 'SELECT  LT.NVMa as BCMaNV,LT.BPMa as BCMaBP,LT.NVMaNV,LT.NVHoTen, LT.BPTen,LT.CVTen,'
WHILE (@TempD <= @DenNgay)
BEGIN
        SET @sSelect = @sSelect + '''' + CONVERT(VARCHAR, DAY(@TempD)) +
            '_N' + '''' + ' = SUM(CASE Ngay WHEN ' + CONVERT(VARCHAR, DAY(@TempD)) 
            + ' THEN CONVERT(INT, TGLamThemN) END),'
        
        SET @sSelect = @sSelect + '''' + CONVERT(VARCHAR, DAY(@TempD)) +
            '_D' + '''' + ' = SUM(CASE Ngay WHEN ' + CONVERT(VARCHAR, DAY(@TempD)) 
            + ' THEN CONVERT(INT, TGLamThemD) END),'
    SET @TempD = DATEADD(DAY, 1, @TempD)
END
	
  SET @sSelect = @sSelect + '
       SUM(CONVERT(INT, TGLamThemN)) AS Tong_TGLamThemN,
       SUM(CONVERT(INT, TGLamThemD)) AS Tong_TGLamThemD,
       SUM(CONVERT(INT, TGLamThemN) + CONVERT(INT, TGLamThemD)) 
       AS 
       Tong_TGLamThem
       
       INTO 
       bc_LamThem
FROM   (
            SELECT tnv.NVMa, tnv.NVMaNV,
                  tnv.NVHoTen,
                  tbp.BPMa,
                  tbp.BPTen,
                  tcv.CVTen,
                  (CONVERT(VARCHAR, DAY(Lt.BCNgay))) AS Ngay,
                  Lt.BCTGThemNgay AS TGLamThemN,
                  Lt.BCTGThemToi AS TGLamThemD
           FROM   tblNhanVien tnv
                  INNER JOIN #tbllamthemTam Lt
                       ON  tnv.NVMa = Lt.NvMa
                  INNER JOIN tblBoPhan tbp
                       ON  tbp.BPMa = tnv.NVMaBP
                  LEFT JOIN tblChucVu tcv
                       ON  tcv.CVMa = tnv.NVMaCV
       ) LT
GROUP BY
       NVMa,BPMa,
       NVMaNV,
       NVHoTen,
       BPTen,
       CVTen 
       '
  
  EXEC (@sSelect)

       
DECLARE @SQL7 VARCHAR(100)
WHILE (@TuNgay <= @DenNgay)
BEGIN
    SET @SQL7 = (
            'UPDATE bc_LamThem SET [' + CONVERT(VARCHAR, DAY(@TuNgay)) +
            '_N] = NULL WHERE [' + CONVERT(VARCHAR, DAY(@TuNgay)) +
            '_N] = 0'
        )
    
    EXEC (@SQL7)
    
    SET @SQL7 = (
            'UPDATE bc_LamThem SET [' + CONVERT(VARCHAR, DAY(@TuNgay)) +
            '_D] = NULL WHERE [' + CONVERT(VARCHAR, DAY(@TuNgay)) +
            '_D] = 0'
        )
    
    EXEC (@SQL7)
    SET @TuNgay = @TuNgay + 1
END
	UPDATE bc_LamThem SET  Tong_TGLamThemN= NULL WHERE Tong_TGLamThemN = 0
	UPDATE bc_LamThem SET  Tong_TGLamThemD= NULL WHERE Tong_TGLamThemD = 0
	UPDATE bc_LamThem SET  Tong_TGLamThem= NULL WHERE Tong_TGLamThem = 0
GO
/****** Object:  StoredProcedure [dbo].[sphrmvnbc_Baocaovangmat]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sphrmvnbc_Baocaovangmat]
@MaBP VARCHAR(10),
@MaNV VARCHAR(10),
@TuNgay DATETIME,
@DenNgay DATETIME
AS
DECLARE @NgayNghiDau     DATETIME
DECLARE @NgayNghiCuoi    DATETIME
DECLARE @tblrecorddata1  NVARCHAR(100)
DECLARE @TempD           DATETIME
DECLARE @i               INT
DECLARE @SQL             NVARCHAR(MAX)


SET @MaBP = ISNULL(@MaBP, -1)
SET @MaNV = ISNULL(@MaNV, -1)

IF(@MaNV=-1 AND @MaBP=-1 )
BEGIN
	SET @MaBP=0
END

SET @DenNgay = DATEADD(Minute, -1, DATEADD(DAY, 1, @DenNgay))

TRUNCATE TABLE tblBaoCaoVangMat

CREATE TABLE #RecordDataTemp
(
	[IDM]       [tinyint] NOT NULL,
	[IDCard]    [nvarchar] (14) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ThoiGian]  [datetime] NOT NULL,
	[Status]    [bit] NOT NULL,
	[HandData]  [bit] NOT NULL,
	[Pass]      [bit] NOT NULL
)



-- Chuyen du lieu va cac bang Temp
SET @TempD = @TuNgay
WHILE (YEAR(@TempD) <= YEAR(@DenNgay))
BEGIN
    SET @i = MONTH(@TempD)
    WHILE (@i <= MONTH(@DenNgay))
    BEGIN
        SET @tblrecorddata1 = 'RecordData' 
            + CONVERT(VARCHAR, YEAR(@TempD)) + '_' + RIGHT('0' + CONVERT(VARCHAR(2), @i), 2)
        
        
        -- Neu ma bo phan <>0 => Chen du lieu vao bang tblBaoCaotemp tu bang tblBaocao
        -- Va  chen du lieu quet the vao bang  #RecordDataTemp
        IF (@MaBP >= 0)
        BEGIN
            IF EXISTS (
                   SELECT *
                   FROM   dbo.sysobjects
                   WHERE  id = OBJECT_ID(N'[dbo].[' + @tblrecorddata1 + ']')
                          AND OBJECTPROPERTY(id, N'IsUserTable') = 1
               )
            BEGIN
                SET @SQL = 
                    'INSERT INTO #RecordDataTemp SELECT [IDM],[IDCard] ,[ThoiGian] ,[Status] ,[HandData] ,[Pass] 
									FROM tblNhanVien AS NV INNER JOIN tblCapThe tct 
												ON nv.NVMa=tct.CTMaNV INNER JOIN '
                    + @tblrecorddata1 +
                    ' rd ON tct.CTMaThe = rd.IDCard
									WHERE IDCard  IN (SELECT CTMathe FROM tblcapthe ) 
													AND nv.NVMaBP IN (' + dbo.DonVi_IDs(@MaBP) 
                    + ')'
                
                EXEC (@SQL)
            END
        END
        ELSE
            -- Neu la nhan vien <>0 => Chen du lieu vao bang tblBaoCaotemp tu bang tblBaocao
            -- Va  chen du lieu quet the vao bang  #RecordDataTemp
        BEGIN
            --IF (@MaNV > 0)
            -- BEGIN
            IF EXISTS (
                   SELECT *
                   FROM   dbo.sysobjects
                   WHERE  id = OBJECT_ID(N'[dbo].[' + @tblrecorddata1 + ']')
                          AND OBJECTPROPERTY(id, N'IsUserTable') = 1
               )
            BEGIN
                --SET @SQL='INSERT INTO #RecordDataTemp([IDM],[IDCard] ,[ThoiGian] ,[Status] ,[HandData] ,[Pass]) Select * From ' + @tblrecorddata1
                SET @SQL = 
                    'INSERT INTO #RecordDataTemp SELECT [IDM],[IDCard] ,[ThoiGian] ,[Status] ,[HandData] ,[Pass] 
		FROM tblNhanVien AS NV INNER JOIN tblCapThe tct ON nv.NVMa=tct.CTMaNV INNER JOIN '
                    + @tblrecorddata1 +
                    ' rd ON tct.CTMaThe = rd.IDCard
		WHERE IDCard  IN (SELECT CTMathe FROM tblcapthe ) AND nv.NVMa = ' + @MaNV
                
                EXEC (@SQL)
            END--END
        END	
        SET @i = @i + 1
    END
    
    
    SET @TempD = DATEADD(YEAR, 1, @TempD)
END

INSERT INTO tblBaoCaoVangMat
  (
    BCMaBP,
    BCTenBP,
    BCSTTNV,
    BCMaNV2,
    BCTenNV,
    BCChucVu,
    BCNgay,
    BCGhiChu
  )
SELECT tbp.BPMa,
       tbp.BPTen,
       tnv.NVUuTien,
       tnv.NVMaNV,
       tnv.NVHoTen,
       tcv.CVTen,
       tblBaocao.BCNgay,
       '' AS ghichu
FROM   tblNhanVien tnv
       INNER JOIN tblBoPhan tbp
            ON  tnv.NVMaBP = tbp.BPMa
       LEFT JOIN tblChucVu tcv
            ON  tnv.NVMaCV = tcv.CVMa
       INNER JOIN (
                SELECT tbc.BCMaNV,
                       tbc.BCNgay
                FROM   tblBaoCao
                       tbc
                       INNER JOIN tblNhanVien tnv
                            ON  tbc.BCMaNV = tnv.NVMa
                       INNER JOIN tblBoPhan tbp
                            ON  tnv.NVMaBP = tbp.BPMa
                                -- Ket noi Leftjoin voi BCTonghopNgayQT de chon
                                -- ra nhung nhan vien khong di lam
                                
                       LEFT JOIN (
                                SELECT --*
                                       MocCa.BCMaNV,
                                       MocCa.BCNgay,
                                       COUNT(MocCa.BCMaNV) AS TSBGQuetThe
                                       --,MocCa.CKhongtinhVangMat
                                FROM   (
                                           SELECT tbc.BCMaNV,
                                                  tbc.BCNgay,
                                                  tc.CTen,
                                                  DATEADD(minute, -tc.CQuetTruocCa, (tbc.BCNgay + tc.CTGBatDau)) AS 
                                                  TGBDCa,
                                                  DATEADD(
                                                      minute,
                                                      tc.CQuetSauCa,
                                                      (tbc.BCNgay + tc.CTGBatDau + tc.CTGKetThuc)
                                                  ) AS TGKTCa,
                                                  tc.CQuetTruocCa,
                                                  tc.CQuetSauCa,
                                                  tc.CKhongtinhVangMat
                                           FROM   tblBaoCao tbc
                                                  LEFT JOIN tblCa tc
                                                       ON  tbc.BCMaCa = tc.CMa
                                                           --WHERE tc.CKhongtinhVangMat=1
                                       ) MocCa
                                       -- TIm nhung nhan vien di lam trong khoang ca
                                       
                                       INNER JOIN (
                                                SELECT *
                                                FROM   #RecordDataTemp rd2
                                                       INNER JOIN tblCapThe tct
                                                            ON  rd2.IDCard = tct.CTMaThe
                                                            AND (
                                                                    rd2.ThoiGian
                                                                    >= tct.CTNgayApDung
                                                                    AND rd2.ThoiGian
                                                                        <= tct.CTNgayKetThuc
                                                                )
                                                WHERE  --tct.CTMaNV = 50
                                                       --AND 
                                                       rd2.ThoiGian >= @TuNgay
                                                       AND rd2.ThoiGian <= @DenNgay
                                            ) DLQuetthe
                                            ON  MocCa.BCMaNV = DLQuetthe.CTMaNV
                                            AND DLQuetthe.ThoiGian <= DLQuetthe.CTNgayKetThuc
                                            AND DLQuetthe.ThoiGian >= DLQuetthe.CTNgayApDung
                                            AND DLQuetthe.ThoiGian BETWEEN MocCa.TGBDCa 
                                                AND 
                                                MocCa.TGKTCa
                                GROUP BY
                                       MocCa.BCMaNV,
                                       MocCa.BCNgay
                            ) BCTonghopNgayQT
                            ON  tbc.BCNgay = BCTonghopNgayQT.BCNgay
                            AND tbc.BCMaNV = BCTonghopNgayQT.BCMaNV
                                --AND BCTonghopNgayQT.CKhongtinhVangMat >0
                WHERE  BCTonghopNgayQT.TSBGQuetThe IS NULL
                       --AND BCTonghopNgayQT.CKhongtinhVangMat>0
                       AND tbc.BCNgay >= @TuNgay
                       AND tbc.BCNgay <= @DenNgay
                       AND 0 < CASE 
                                    WHEN CAST(@MaBP AS INT) >= 0 THEN (
                                             CHARINDEX(
                                                 ',' + CAST(+ tbp.BPMa AS VARCHAR) 
                                                 + ',',
                                                 ',' + dbo.DonVi_IDs(@MaBP) + 
                                                 ','
                                             )
                                         )
                                    ELSE 1
                               END
                       AND tnv.NVMa = CASE 
                                           WHEN CAST(@MaNV AS INT) > 0 THEN (@MaNV)
                                           ELSE tnv.NVMa
                                      END
            ) tblBaocao
            ON  tnv.NVMa = tblBaocao.BCMaNV

WHERE tnv.NVNgayvao<=@DenNgay AND tnv.NVNgayra>=@TuNgay
		-- Xoa du lieu nhung ngay co ca khong tinh vao bao cao vang mat
DELETE 
FROM   tblBaoCaoVangMat
WHERE  (
           CONVERT(VARCHAR, BCMaNV2) + ';' + CONVERT(VARCHAR, BCNgay, 103)
       )
       IN (SELECT --*
                  CONVERT(VARCHAR, MocCa.NVMaNV) + ';' + CONVERT(VARCHAR, MocCa.BCNgay, 103)
           FROM   (
                      SELECT tbc.BCMaNV,
                             tnv.NVMaNV,
                             tbc.BCNgay,
                             tc.CKhongtinhVangMat
                      FROM   tblBaoCao tbc
                             INNER JOIN tblNhanVien tnv
                                  ON  tbc.BCMaNV = tnv.NVMa
                             LEFT JOIN tblCa tc
                                  ON  tbc.BCMaCa = tc.CMa
                      WHERE  ISNULL(tc.CKhongtinhVangMat, 0) = 1
                  ) MocCa
           GROUP BY
                  MocCa.NVMaNV,
                  MocCa.BCNgay)


-- Xoa nhung ngay la chu nhat
DELETE 
FROM   tblBaoCaoVangMat
WHERE  DATEPART(dw, BCNgay) = (
           SELECT CASE (
                           SELECT CONVERT(VARCHAR, TSGiatri)
                           FROM   tblThamSo 
                                  tts
                           WHERE  tts.TSTen = 'NORMALWORKINGONSUNDAY'
                       )
                       WHEN '1' THEN 0
                       ELSE 1
                  END
       )
                                                               

-- Cap nhat ly do nghi
UPDATE tblBaoCaoVangMat
SET    tblBaoCaoVangMat.BCGhiChu = dkn.LDNTen
FROM   tblBaoCaoVangMat tbvm
       INNER JOIN (
                SELECT tbcvm.BCMaNV2,
                       tbcvm.BCNgay,
                       tldn.LDNTen
                FROM   (
                           SELECT tnv.NVMa,
                                  tbcvm.*
                           FROM   tblNhanVien tnv
                                  INNER JOIN tblBaoCaoVangMat tbcvm
                                       ON  tnv.NVMaNV = tbcvm.BCMaNV2
                       ) tbcvm
                       INNER JOIN tblDangkynghi tdkn
                            ON  tbcvm.NVMa = tdkn.DKNMaNV
                            AND tbcvm.BCNgay >= tdkn.DKNNgayApDung
                            AND tbcvm.BCNgay <= tdkn.DKNNgayKetThuc
                       INNER JOIN tblLyDoNghi tldn
                            ON  tdkn.DKNMaLyDo = tldn.LDNMa
            ) dkn
            ON  tbvm.BCNgay = dkn.BCNgay
            AND tbvm.BCMaNV2 = dkn.BCMaNV2
      
SELECT *
FROM   tblBaoCaoVangMat
WHERE  (1 = 1)
ORDER BY
       BCTenNV,
       BCNgay ASC

GO
/****** Object:  StoredProcedure [dbo].[sphrmvnbc_InoutReport]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sphrmvnbc_InoutReport]
	@MaNVN VARCHAR(50),
	@MaBP INT,
	@TuNgay SMALLDATETIME,
	@DenNgay SMALLDATETIME
	--,	@MaCa INT
AS
BEGIN
	DECLARE @MaNVTemp   INT  
	DECLARE @TempD      DATETIME
	DECLARE @TempDate        AS DATETIME
	DECLARE @i               AS INT 
	DECLARE @SQL AS VARCHAR(500)
	DECLARE @MaNV INT
	
	DECLARE @MaCa INT
	SET @MaCa=0

	SELECT @MaNV=NVMa FROM tblNhanVien WHERE NVMaNV=@MaNVN

	SET @MaNV=ISNULL(@MaNV,-1)
	
	--Xoa bao cao cu
	TRUNCATE TABLE tblBaoCaoVaoRa
	--Xoa khoang ngay
    Delete  From tblKhoangNgay
    SET @i=0
    WHILE (@i<=DateDiff(DAY, @TuNgay, @DenNgay))
    BEGIN
        set @TempDate = DateAdd(day, @i, @TuNgay)
        Insert Into tblKhoangNgay(KNMaNV,KNNgay)  Values(0,@TempDate )
        SET @i=@i+1
    END	
	
	DECLARE curRSNV      CURSOR  
	FOR
	    SELECT tnv.NVMa
	    FROM   tblNhanVien tnv
	           INNER JOIN tblBoPhan tbp
	                ON  tnv.NVMaBP = tbp.BPMa
	    WHERE  tnv.NVNgayVao <= @DenNgay
	           AND tnv.NVNgayRa >= @TuNgay
	           AND 0 < CASE 
	                        WHEN CAST(@MaBP AS INT) >= 0 THEN (
	                                 CHARINDEX(
	                                     ',' + CAST( tbp.BPMa AS VARCHAR) + ',',
	                                     ',' + dbo.DonVi_IDs(@MaBP) + ','
	                                 )
	                             )
	                        ELSE 1
	                   END
	           AND tnv.NVMa = CASE 
	                               WHEN CAST(@MaNV AS INT) > 0 THEN (@MaNV)
	                               ELSE tnv.NVMa
	                          END
	
	OPEN curRSNV
	
	FETCH NEXT FROM curRSNV INTO @MaNVTemp                       
	WHILE @@FETCH_STATUS = 0
	BEGIN
	    Exec sphrmvnbc_InoutReportStaff @MaNV =@MaNVTemp,
										@MaBP =@MaBP,
										@TuNgay =@TuNgay,
										@DenNgay =@DenNgay,
										@MaCa =@MaCa
	    FETCH NEXT FROM curRSNV INTO @MaNVTemp
	END
	CLOSE curRSNV
	DEALLOCATE curRSNV
END
GO
/****** Object:  StoredProcedure [dbo].[sphrmvnbc_InoutReportStaff]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/************************************************************
*  Routine       :	sphrmvnbc_InoutReport
*  Created by    :  Duong Van Quyet , at 28/02/2011 - 14:07:31  
*  Machine       :  DIGISOFT
*  Description   :  Bao cao vao ra, @MaNV=-1=> goi  Bo phan
*  Parameters    :  @MaNVN VARCHAR(50), @MaBP int, @TuNgay smalldatetime,
                    @DenNgay smalldatetime
************************************************************/
CREATE PROCEDURE [dbo].[sphrmvnbc_InoutReportStaff]
	@MaNV INT,
	@MaBP INT,
	@TuNgay SMALLDATETIME,
	@DenNgay SMALLDATETIME
	,	@MaCa INT
	
AS


DECLARE @RecordData      AS VARCHAR(50)


DECLARE @TempDate        AS DATETIME

DECLARE @i               AS INT 
DECLARE @NumberOfMonths  AS INT 
DECLARE @MaBPTemp  AS INT 


DECLARE @Tenbophan       AS NVARCHAR(200)
DECLARE @HoTen           AS NVARCHAR(200)
DECLARE @Chucvu          AS NVARCHAR(200)
DECLARE @KNNgay          AS DATETIME
DECLARE @NVUutien        AS INT 
DECLARE @SQL             AS NVARCHAR(MAX)

DECLARE @vDate           AS DATETIME
DECLARE @strTimeIn       AS NVARCHAR(50)
DECLARE @strTimeOut      AS NVARCHAR(50)

DECLARE @ThoiGian        AS DATETIME
DECLARE @IDCard          AS NVARCHAR(20)
DECLARE @Status          AS BIT
DECLARE @CTNgayApDung    AS DATETIME
DECLARE @CTNgayKetThuc   AS DATETIME

DECLARE @MaNVN VARCHAR(50)


--SET @TuNgay='2011-06-20'
--SET @DenNgay='2011-06-29'
--SET @MaNV=1
--SET @MaCa=0

SET @vDate = '1/1/1900'
SET @strTimeIn = ''
SET @strTimeOut = ''



SET @NumberOfMonths = DATEDIFF(MONTH, @TuNgay, @DenNgay)

UPDATE tblKhoangNgay
SET    KNMaNV = @MaNV

SET @i = 0
WHILE (@i <= @NumberOfMonths)
BEGIN
    SET @TempDate = DATEADD(MONTH, @i, @TuNgay)
    SET @RecordData = 'RecordData' + CONVERT(VARCHAR(4), YEAR(@TempDate)) + '_' 
        + RIGHT( '00' + CONVERT(VARCHAR(4), MONTH(@TempDate)),2)
    
    DECLARE rsGetInfor  CURSOR  
    FOR
        SELECT B.BPMa, B.BPTen AS TenBP,
               A.NVUuTien,
               A.NVMaNV,
               A.NVHoTen,
               D.CVTen AS TenCV,
               C.KNNgay
        FROM   (
                   (
                       tblNhanVien AS A INNER JOIN tblBoPhan AS B ON A.NVMaBP =
                       B.BPMa
                   ) INNER JOIN tblKhoangNgay AS C ON C.KNMaNV = A.NVMa
               )
               LEFT OUTER JOIN tblChucVu AS D
                    ON  A.NVMaCV = D.CVMa
        WHERE  (A.NVMa = @MaNV)
               AND (C.KNNgay BETWEEN A.NVNgayVao AND A.NVNgayRa)
        ORDER BY
               C.KNNgay
    
    OPEN rsGetInfor
    
    FETCH NEXT FROM rsGetInfor INTO @MaBPTemp,@Tenbophan,@NVUutien,@MaNVN,@HoTen,@Chucvu,@KNNgay                        
    IF @@FETCH_STATUS = 0
    BEGIN
        IF (@MaCa = 0)
        BEGIN
            SET @SQL = 
                'SELECT C.IDCard,
							   C.ThoiGian,
							   C.Status,
							   A.CTNgayApDung,
							   A.CTNgayKetThuc
						FROM   (
								   tblCapThe AS A INNER JOIN tblKhoangNgay AS B ON A.CTMaNV = B.KNMaNV
							   )
							   INNER JOIN ' + @RecordData + 
                ' AS C
									ON  (A.CTMaThe = C.IDCard)
						WHERE  (DATEDIFF(DAY,B.KNNgay,c.ThoiGian)=0) 
							   AND MONTH(C.ThoiGian) = ' + CONVERT(VARCHAR(2), MONTH(@TempDate))
                + 
                '
							   AND MONTH(C.ThoiGian) = MONTH(B.KNNgay)
							   AND YEAR(C.ThoiGian) = YEAR(B.KNNgay)
							   AND  C.IDM  in  (Select tblDauDoc.DDMa From tblDauDoc where IsNull(DDLoaiChamCong,'''')='''')
						ORDER BY
							   C.ThoiGian
    		'
        END
        ELSE
        BEGIN
            SET @SQL = 
                'SELECT C.IDCard,
							   C.ThoiGian,
							   C.Status,
							   A.CTNgayApDung,
							   A.CTNgayKetThuc
						FROM   (
								   (
									   tblCapThe AS A INNER JOIN tblKhoangNgay AS B ON A.CTMaNV = B.KNMaNV
								   ) INNER JOIN ' + @RecordData + 
                ' AS C ON A.CTMaThe = C.IDCard
							   )
							   INNER JOIN tblBaoCao AS D
									ON  D.BCMaNV = B.KNMaNV
						WHERE  (DATEDIFF(DAY,B.KNNgay,c.ThoiGian)=0) 
							   AND MONTH(C.ThoiGian) = ' + CONVERT(VARCHAR(2), MONTH(@TempDate))
                + 
                '
							   AND MONTH(C.ThoiGian) = MONTH(B.KNNgay)
							   AND YEAR(C.ThoiGian) = YEAR(B.KNNgay)
							   AND D.BCNgay = B.KNNgay
							   AND  C.IDM  in  (Select tblDauDoc.DDMa From tblDauDoc where IsNull(DDLoaiChamCong,'''')='''')
							   AND D.BCMaCa = ' + CONVERT(VARCHAR(3), @MaCa) + 
                '
						ORDER BY
							   C.ThoiGian
    		'
        END
        
        SET @vDate = '1/1/1900'
        SET @strTimeIn = ''
        SET @strTimeOut = ''
        
        SELECT @SQL = ' DECLARE Rs CURSOR FOR ' + @SQL
        EXEC (@SQL)
        OPEN Rs
        FETCH NEXT FROM Rs INTO @IDCard,@ThoiGian,@Status,@CTNgayApDung,@CTNgayKetThuc
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF (
                   CONVERT(VARCHAR, @vDate, 101) <> CONVERT(VARCHAR, @ThoiGian, 101)
               )
            BEGIN
                IF (@strTimeIn <> @strTimeOut)
                BEGIN
                    INSERT INTO tblBaoCaoVaoRa
                      (
                        MaBP,
                        BoPhan,
                        UuTien,
                        MaNV,
                        HoTen,
                        ChucVu,
                        Ngay,
                        TGVao,
                        TGRa,
                        GhiChu
                      )
                    VALUES
                      (
                        @MaBPTemp,
                        @Tenbophan,
                        @NVUutien,
                        @MaNVN,
                        @HoTen,
                        @Chucvu,
                        @vDate,
                        @strTimeIn,
                        @strTimeOut,
                        ''
                      )
                      
                    SET @strTimeIn = ''
                    SET @strTimeOut = ''
                END
                
                SET @vDate = @ThoiGian
            END
            

           
            IF (
            	DATEDIFF(DAY,@vDate,@CTNgayApDung)<=0
            	AND
            	DATEDIFF(DAY,@CTNgayKetThuc,@vDate)<=0
                   --CONVERT(VARCHAR, @CTNgayApDung, 101) <= CONVERT(VARCHAR, @vDate, 101)
                   --AND CONVERT(VARCHAR, @vDate, 101) <= CONVERT(VARCHAR, @CTNgayKetThuc, 101)
               )
            BEGIN
                IF (@Status = 1)
                BEGIN
                    IF (@strTimeIn <> '')
                    BEGIN
                        INSERT INTO tblBaoCaoVaoRa
                          (
                            MaBP,
                            BoPhan,
                            UuTien,
                            MaNV,
                            HoTen,
                            ChucVu,
                            Ngay,
                            TGVao,
                            TGRa,
                            GhiChu
                          )
                        VALUES
                          (
                            @MaBPTemp,
                            @Tenbophan,
                            @NVUutien,
                            @MaNVN,
                            @HoTen,
                            @Chucvu,
                            @vDate,
                            @strTimeIn,
                            @strTimeOut,
                            ''
                          )
                    END
                    
                    SET @strTimeIn = CONVERT(VARCHAR, @ThoiGian, 108)
                END
                ELSE
                BEGIN
                    SET @strTimeOut = CONVERT(VARCHAR, @ThoiGian, 108)
                    INSERT INTO tblBaoCaoVaoRa
                      (
                        MaBP,
                        BoPhan,
                        UuTien,
                        MaNV,
                        HoTen,
                        ChucVu,
                        Ngay,
                        TGVao,
                        TGRa,
                        GhiChu
                      )
                    VALUES
                      (
                        @MaBPTemp,
                        @Tenbophan,
                        @NVUutien,
                        @MaNVN,
                        @HoTen,
                        @Chucvu,
                        @vDate,
                        @strTimeIn,
                        @strTimeOut,
                        ''
                      )
                    
                    SET @strTimeIn = ''
                    SET @strTimeOut = ''
                END
            END
            
            FETCH NEXT FROM Rs INTO @IDCard,@ThoiGian,@Status,@CTNgayApDung,@CTNgayKetThuc
        END	
        CLOSE Rs
		DEALLOCATE Rs 	    	
        IF (@strTimeIn <> '')
        BEGIN
            INSERT INTO tblBaoCaoVaoRa
              (
                MaBP,
                BoPhan,
                UuTien,
                MaNV,
                HoTen,
                ChucVu,
                Ngay,
                TGVao,
                TGRa,
                GhiChu
              )
            VALUES
              (
                @MaBPTemp,
                @Tenbophan,
                @NVUutien,
                @MaNVN,
                @HoTen,
                @Chucvu,
                @vDate,
                @strTimeIn,
                @strTimeOut,
                ''
              )
        END
    END
    
    CLOSE rsGetInfor
    DEALLOCATE rsGetInfor 
    
    
    SET @i = @i + 1
END

--SELECT * FROM tblBaoCaoVaoRa

--SELECT CONVERT(NVARCHAR, MaBP) AS MaBP,
--       TenBP,
--       NVMa AS MaNV,
--       HoTen,
--       TenChucVu AS Chucvu,
--       Ngay,
--       GioVao AS TGVao,
--       GioRa AS TGRa,
--       IDCard AS GhiChu
--FROM   #BangTam
--ORDER BY
--       NVMa,
--       Ngay,
--       ISNULL(giovao, giora)
       


GO
/****** Object:  StoredProcedure [dbo].[sphrmvnbc_QuetTheLoi]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sphrmvnbc_QuetTheLoi]
@BPMa INT,
@NVMa INT,
@StartDate DATETIME,
@FinishDate DATETIME
AS
Delete from RecordDataQuetTheLoi
DECLARE @SQL VARCHAR(8000)

IF @NVMa <> 0
BEGIN
	SET @SQL  = '	declare @StartDate smalldatetime
	declare @FinishDate smalldatetime
	Declare @SID int
	
	Set @StartDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@StartDate)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@StartDate)),2)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@StartDate)),2)) +') ) 
					
	Set @FinishDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@FinishDate)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@FinishDate)),2)) +')
					+  '+ '''' +''+ '-' +''+ '''' +' 
					+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@FinishDate)),2)) +')	)
	
	while @StartDate < = @FinishDate
	BEGIN
		exec sphrmvnbc_QuetTheLoi_Nhanvien '+ CONVERT(VARCHAR(10),@NVMa) +',@StartDate
		set @StartDate = dateadd(day,1,@StartDate)
	END
	'
	EXEC(@SQL)				
END
ELSE 
	BEGIN
			SET @SQL  = '	declare @StartDate smalldatetime
							declare @FinishDate smalldatetime
				Declare @SID int
				
				Set @StartDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@StartDate)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@StartDate)),2)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@StartDate)),2)) +') ) 
								
				Set @FinishDate = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@FinishDate)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@FinishDate)),2)) +')
								+  '+ '''' +''+ '-' +''+ '''' +' 
								+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@FinishDate)),2)) +')	)
				declare @NVMa int
				declare @S smalldatetime
				declare @F smalldatetime
				
				DECLARE curr CURSOR FOR
					SELECT distinct NVMa
					FROM tblNhanVien nv
					INNER JOIN tblBoPhan tbp ON nv.NVMaBP = tbp.BPMa
					WHERE tbp.BPMa IN ( '+dbo.DonVi_IDs(@BPMa) +')
				OPEN curr
				FETCH NEXT FROM curr INTO @NVMa
						
				WHILE @@FETCH_STATUS = 0
				begin
						Set @S = @StartDate
						set @F = @FinishDate
						while @S < = @F
						BEGIN
							exec sphrmvnbc_QuetTheLoi_Nhanvien @NVMa,@S
							set @S = dateadd(day,1,@S)
						END
					FETCH NEXT FROM curr INTO @NVMa
				END
				CLOSE curr
				DEALLOCATE curr
	'
	EXEC(@SQL)
	END
IF EXISTS (
       SELECT *
       FROM   dbo.sysobjects
       WHERE  id = OBJECT_ID(N'[dbo].[tmp_quettheloi]')
              AND OBJECTPROPERTY(id, N'IsUserTable') = 1
   )
    DROP TABLE [dbo].[tmp_quettheloi]

SELECT nv.NVMaNV,
       nv.NVHoTen,
       bp.BPMa,
       bp.BPTen,
       r.ThoiGian,
       CASE r.Status
            WHEN 1 THEN 'In'
            ELSE 'Out'
       END AS InOut
       INTO tmp_quettheloi
FROM   tblNhanVien nv
       INNER JOIN tblCapThe ct
            ON  ct.CTMaNV = nv.NVMa
            AND ct.CTNgayApDung <= @FinishDate
            AND ct.CTNgayKetThuc >= @StartDate
       INNER JOIN tblBoPhan bp
            ON  bp.BPMa = nv.NVMaBP
       INNER JOIN RecordDataQuetTheLoi r
            ON  r.IDCard = ct.CTMaThe
GO
/****** Object:  StoredProcedure [dbo].[sphrmvnbc_QuetTheLoi_Nhanvien]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[sphrmvnbc_QuetTheLoi_Nhanvien]
@StaffID int,
@D DATETIME
 as
	Begin
	DECLARE @TGBDCa Datetime 		
	DECLARE @TGKTCa Datetime 
	DECLARE @LoaiCa smallint	
	DECLARE @TGLayDLDau  Datetime 	
	DECLARE @TGLayDLCuoi Datetime 
	
	DECLARE @QuetTruocCa int
	DECLARE @QuetSauCa int
	
	-- Khai bao ten cac bang
	DECLARE @RecordData nvarchar(20) 	
	DECLARE @tblBaocao nvarchar(20) 	
	DECLARE @tblDangkynghi nvarchar(20) 	
	DECLARE @tblDangkyuudai nvarchar(20) 	
	
	-- Khai bao bien loc trang thai
	DECLARE @IDM smallint
	DECLARE @ThoiGian  Datetime 
	DECLARE @IDCard nvarchar(15)
	DECLARE @Status bit
	DECLARE @HandData bit
	DECLARE @Pass bit

    -- Thoi gian den, cua den
	--DECLARE	@TGVe Datetime
	--DECLARE	@CuaVe smallint
	-- Thoi gian vao, cua vao, thoi gian ra, cua ra
	--DECLARE	@TGVao Datetime
	--DECLARE	@CuaVao smallint
	--DECLARE	@TGRa Datetime
	--DECLARE	@CuaRa smallint
    -- Khai bao loai nghi, Ma ly do nghi, Ghi chu, 
    DECLARE @LoaiNghi smallint
    DECLARE @MaLyDoNghi smallint
    DECLARE @LydonghiViettat nvarchar(10)
    
    DECLARE @Ghichu nvarchar(50)
    
	-- Bien tam dem tong so ban ghi tra ve
	DECLARE	@recordCount smallint
	-- Luu kieu xac dinh trang thai vao ra
	DECLARE @OriginalStatus smallint
	-- Kieu tinh gio
	DECLARE @TypeCalculator  smallint
	-- Bien luu trang thai tam khi loc du lieu
	DECLARE @OldStatus bit
	-- Khai bao cac bien dang bao cao
	DECLARE	@Maca int
	-- Khai bao bien ma the cua nhan vien
	DECLARE @MaThe nvarchar(14) 
	-- Khai bao loai tblbaocao
	DECLARE @BCLoai bit
	
	DECLARE	@i int
	
	DECLARE @SQL nvarchar(4000) 	
	DECLARE @SQL1 nvarchar(4000) 	
	DECLARE @SQL2 nvarchar(4000) 	
	DECLARE @SQL3 nvarchar(4000) 	
	DECLARE @SQL4 nvarchar(4000) 	

	
	-- Lay cac tham so cham cong
	-- Cach lay du lieu quet: Xem ke trang thai hoac lay tu dau doc  (1: Xen ke, 0: dau doc)

	--SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='ORIGINALSTATUS'
	--SET @OriginalStatus=CONVERT(int,@SQL)
	
	--SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='TYPECALCULATOR'
	--SET @TypeCalculator=CONVERT(int,@SQL)
	
	-- Thiet lap ten bao cao
	SET @RecordData = case when LEN(Month(@D))=2 then 
	 'RecordData' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	 else
	 'RecordData' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	 end
	 
	SET @tblBaocao = case when LEN(Month(@D))=2 then 
	 'tblBaoCao' --+ CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	 else
	 'tblBaoCao' --+ CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	END
	
	SET @tblDangkynghi = 'tblDangkynghi' --+ CONVERT(nvarchar,YEAR(@D)) 
	SET @tblDangkyuudai = 'tblDangkyUuDai' --+ CONVERT(nvarchar,YEAR(@D)) 
	
	 	 
	 	-- Mo bang tblBaocao lay ca thong tin lien quan
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_Baocaotam]') AND type in (N'U'))
	DROP TABLE [dbo].[tmp_Baocaotam]
	
	Create TABLE tmp_Baocaotam ([BCmaca][int],[BCLoai] [bit] )
	EXEC ('Insert INTO tmp_Baocaotam(bcmaca,BCLoai) Select bcmaca,BCloai  From  ' + @tblBaocao + ' WHERE (BCMaNV=' + @StaffID +') AND (BCNgay=' + '''' + @D + '''' + ')')
	select @Maca = BCmaca from tmp_Baocaotam
	select @BCloai = BCloai from tmp_Baocaotam
	
	if (@Maca is null)
		BEGIN
			-- Neu khong ton tai nhan vien trong bang bao cao thi khoi tao lai
			GOTO EndThisSub
		end
	
	-- Kiem tra xem nhan vien co dang ky nghi khong
	
		-- Tao bang dang ky nghi tam
		IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_DangkynghiTam]') AND type in (N'U'))
		DROP TABLE [dbo].[tmp_DangkynghiTam]
		CREATE TABLE [dbo].tmp_DangkynghiTam ([DKNMaNV] [int] NOT NULL ,[DKNNgayApDung] [datetime] NOT NULL ,[DKNNgayKetThuc] [datetime] NOT NULL ,[DKNMaLyDo] [int] NOT NULL ,[DKNLoai] [tinyint] NOT NULL) ON [PRIMARY]
		
		SET @SQL='Insert into tmp_DangkynghiTam(DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai) SELECT DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai FROM '+ @tblDangkynghi+ ' WHERE (DKNMaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKNNgayApDung AND DKNNgayKetThuc) '
		EXEC (@SQL)
		
		DECLARE curRS 
		CURSOR FOR 
			SELECT DKNLoai,DKNMaLyDo  FROM tmp_DangkynghiTam 
		OPEN curRS

		FETCH NEXT FROM curRS INTO @LoaiNghi,@MaLyDoNghi
		if @@FETCH_STATUS = 0
			BEGIN
				SET @LoaiNghi=@LoaiNghi+1
				SELECT @LydonghiViettat= LDNVietTat FROM tblLyDoNghi WHERE LDNMa=@MaLyDoNghi			
				IF @LydonghiViettat IS NOT NULL 
						SET @Ghichu=CASE WHEN @LoaiNghi<>1 THEN '1/2' + CONVERT(nvarchar(30),@LydonghiViettat) ELSE CONVERT(nvarchar(30),@LydonghiViettat) end
				FETCH NEXT FROM curRS INTO @LoaiNghi,@MaLyDoNghi
			end
		-- Neu loai nghi =1 thi cap nhat cac gia tri ve mac dinh
		IF @LoaiNghi=1
			BEGIN
				GOTO EndThisSub
			END
		CLOSE curRS
		DEALLOCATE curRS
			
		--Kiem tra neu cham cong bang tay thi thoat
		IF @BCloai=1 GOTO Endthissub

	DECLARE curRS 
	CURSOR FOR 
		SELECT CTGBatDau,CTGKetThuc,CQuetTruocCa,CQuetSauCa,CLoaiCa FROM tblCa WHERE CMa=@Maca
	OPEN curRS

	FETCH NEXT FROM curRS INTO @TGBDCa,@TGKTCa,@QuetTruocCa,@QuetSauCa,@LoaiCa
	if @@FETCH_STATUS = 0
		BEGIN
			--Lay thong tin tu bang ca
			set @TGBDCa=@D + @TGBDCa
			set @TGKTCa=@TGBDCa + @TGKTCa			
			set @TGLayDLDau= DateAdd(minute, -@QuetTruocCa, @TGBDCa)
			set @TGLayDLCuoi=DateAdd(minute, @QuetSauCa, @TGKTCa)
		ENd
		CLOSE curRS
	DEALLOCATE curRS
	
    -- Lay ma the
   
    SELECT @MaThe= CTMaThe FROM tblCapThe WHERE (CTMaNV=@StaffID) AND (@D BETWEEN CTNgayApDung AND CTNgayKetThuc) ORDER BY CTNgayKetThuc DESC
    
    IF (@MaThe IS NULL) 
    BEGIN
    	GoTo EndThisSub
    END 
    
    
	-- Loc du lieu	
	DECLARE @in INT
	DECLARE @Out int
	
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_value]') AND type in (N'U'))
	DROP TABLE [dbo].[tmp_value]
	CREATE TABLE tmp_value (GiaTri int)
	DELETE FROM tmp_value
	EXEC('INSERT INTO tmp_value SELECT Count(*) FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  AND Status = 1 ')
	SET @in =  isnull((SELECT * FROM tmp_value),0)
	DELETE FROM tmp_value
	EXEC('INSERT INTO tmp_value SELECT Count(*) FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  AND Status = 0 ')
	SET @Out =  isnull((SELECT * FROM tmp_value),0)
	
	Set @recordCount =  @in + @Out

	IF @recordCount > 0
	BEGIN
		IF (@recordCount - (@recordCount/2)*2) = 1
		BEGIN
			EXEC('INSERT INTO RecordDataQuetTheLoi(IDM,IDCard,ThoiGian,Status,HandData,Pass) SELECT DISTINCT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  ORDER BY ThoiGian')
		END
		ELSE
			BEGIN
				IF @in <> @Out
					EXEC('INSERT INTO RecordDataQuetTheLoi(IDM,IDCard,ThoiGian,Status,HandData,Pass) SELECT DISTINCT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  ORDER BY ThoiGian')
			END
	END
	 
EndThisSub:

end


GO
/****** Object:  StoredProcedure [dbo].[sphrmvnDKN_TinhTongNgayPhep]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/************************************************************
*  Routine       :	sphrmvnDKN_TinhTongNgayPhep
*  Created by    :  Duong Van Quyet , at 14/04/2011 - 18:40:35  
*  Machine       :  DIGISOFT
*  Description   :  Tinh tong phep toi da cua nhan vien
*  Parameters    :  @MaNV varchar(10), @MaBP varchar(10)
************************************************************/
CREATE PROC [dbo].[sphrmvnDKN_TinhTongNgayPhep]
 @MaBP VARCHAR(10),
 @MaNV VARCHAR(10)
AS
                       
INSERT INTO NS_TongNgayPhep
  (
    nsnpMa,
    nsnpMaNV,
    nsnpNgayApdung,
    nsnpNgayKetThuc,
    nsnpSoNgay
  )
SELECT RIGHT(
           '0000' + dbo.AddValToStr(
               (
                   SELECT ISNULL(MAX(NS_TongNgayPhep.nsnpMa), '0000')
                   FROM   NS_TongNgayPhep
               ) + CONVERT(INT, (ROW_NUMBER() OVER(ORDER BY NVMa))),
               1
           ),
           4
       ) AS Ma,
       tnv.NVMa,
       CASE 
            WHEN DAY(tnv.NVNgayVao) >= '15' THEN DATEADD(DAY, -DAY(tnv.NVNgayVao) + 1, tnv.NVNgayVao)
            ELSE DATEADD(
                     MONTH,
                     -1,
                     DATEADD(DAY, -DAY(tnv.NVNgayVao) + 1, tnv.NVNgayVao)
                 )
       END AS NgayApDung,
       DATEADD(
           DAY,
           -1,
           DATEADD(
               MONTH,
               1,
               DATEADD(
                   YEAR,
                   5,
                   CASE 
                        WHEN DAY(tnv.NVNgayVao) >= '15' THEN DATEADD(DAY, -DAY(tnv.NVNgayVao) + 1, tnv.NVNgayVao)
                        ELSE DATEADD(
                                 MONTH,
                                 -1,
                                 DATEADD(DAY, -DAY(tnv.NVNgayVao) + 1, tnv.NVNgayVao)
                             )
                   END
               )
           )
       ) AS NgayKetThuc,
       12 AS SoNgay
FROM   tblNhanVien tnv
WHERE  tnv.NVMa IN (SELECT NVMa
                    FROM   tblNhanVien tnv
                    WHERE  tnv.NVMa NOT IN (SELECT DISTINCT nsnpMaNV
                                            FROM   NS_TongNgayPhep
                                            WHERE  NS_TongNgayPhep.nsnpSoNgay = 
                                                   12))
           
              
              
--Sau 5 nam duoc 13 ngay                        
INSERT INTO NS_TongNgayPhep
  (
    nsnpMa,
    nsnpMaNV,
    nsnpNgayApdung,
    nsnpNgayKetThuc,
    nsnpSoNgay
  )
SELECT RIGHT(
           '0000' + dbo.AddValToStr(
               (
                   SELECT ISNULL(MAX(NS_TongNgayPhep.nsnpMa), '0000')
                   FROM   NS_TongNgayPhep
               ) + CONVERT(INT, (ROW_NUMBER() OVER(ORDER BY NVMa))),
               1
           ),
           4
       ) AS Ma,
       tnv.NVMa,
       DATEADD(
           MONTH,
           1,
           DATEADD(
               YEAR,
               5,
               CASE 
                    WHEN DAY(tnv.NVNgayVao) >= '15' THEN DATEADD(DAY, -DAY(tnv.NVNgayVao) + 1, tnv.NVNgayVao)
                    ELSE DATEADD(
                             MONTH,
                             -1,
                             DATEADD(DAY, -DAY(tnv.NVNgayVao) + 1, tnv.NVNgayVao)
                         )
               END
           )
       ) AS NgayApDung,
       '9990-12-31' AS NgayKetthuc,
       13 AS SoNgay
FROM   tblNhanVien tnv
WHERE  tnv.NVMa IN (SELECT NVMa
                    FROM   tblNhanVien tnv
                    WHERE  tnv.NVMa NOT IN (SELECT DISTINCT nsnpMaNV
                                            FROM   NS_TongNgayPhep
                                            WHERE  NS_TongNgayPhep.nsnpSoNgay = 
                                                   13))

GO
/****** Object:  StoredProcedure [dbo].[TG_Sl_DiMuonVeSom]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROC [dbo].[TG_Sl_DiMuonVeSom]
@NgayBatdau SMALLDATETIME,
@Ngayketthuc SMALLDATETIME,
@Bophan int,
@NhanVien INT
AS
DECLARE @i INT
DECLARE @tblbaocao NVARCHAR(100)
DECLARE @SQL NVARCHAR(1000)
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Sl_TGDMVS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Sl_TGDMVS]
CREATE TABLE [dbo].[Sl_TGDMVS] (
	[BCMaNV] [int] NOT NULL ,
	[TongDMVS] [int] NULL ,
	[TongTGDMVS] [int] NULL 
) ON [PRIMARY]
SET @i=Month(@NgayBatdau)
	WHILE(@i<=Month(@Ngayketthuc))
BEGIN
	IF (len(@i))=1
		BEGIN
			SET @tblbaocao= 'tblbaocao' + convert(VARCHAR,YEAR(@NgayBatdau)) + '_0'+ convert(varchar(2),@i)
		END
	ELSE
		BEGIN
			SET @tblbaocao= 'tblbaocao' + convert(VARCHAR,YEAR(@NgayBatdau))+ '_'+ convert(varchar(2),@i)
		END
	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].['+@tblbaocao+']') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	BEGIN
		IF(@NhanVien<>0)
		BEGIN
		SET @SQL='INSERT INTO Sl_TGDMVS Select BCMaNV,COUNT(BCNgay),(SUM(BCTGDiMuonngay+BCTGDiMuonToi)+Sum(BCTGVeSomNgay+BCTGVeSomToi)) From '+ @tblbaocao + ' 
		WHERE  (BCMaNV='+CONVERT(varchar,@NhanVien)+') And (BCTGDiMuonngay+BCTGDiMuonToi <>0) or (BCTGVeSomNgay+BCTGVeSomToi)<>0 GROUP BY BCMaNV '
		EXEC (@SQL)
		END
		ELSE
		BEGIN
		SET @SQL='INSERT INTO Sl_TGDMVS Select BCMaNV,COUNT(BCNgay),(SUM(BCTGDiMuonngay+BCTGDiMuonToi)+Sum(BCTGVeSomNgay+BCTGVeSomToi)) From '+ @tblbaocao + ' 
		WHERE(BCMaNV IN ( '+dbo.DonVi_IDs(@Bophan) +')) And (BCTGDiMuonngay+BCTGDiMuonToi <>0) or (BCTGVeSomNgay+BCTGVeSomToi)<>0 GROUP BY BCMaNV '
		EXEC (@SQL)
		END
	END
--WHERE (BCTGDiMuonngay+BCTGDiMuonToi <>0) or (BCTGVeSomNgay+BCTGVeSomToi)<>0 GROUP BY BCMaNV 
	SET @i=@i+1
END	



GO
/****** Object:  StoredProcedure [dbo].[TongHopDangKyNghi_Thang]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE      PROCEDURE [dbo].[TongHopDangKyNghi_Thang]
@MaNV INT,
@MaBP INT,
@StartDate SMALLDATETIME,
@FinishDate SMALLDATETIME
AS
DECLARE @NamS INT
DECLARE @SoLN VARCHAR(20)
SET @NamS = YEAR(@StartDate)
DECLARE @SQL VARCHAR(8000)
CREATE TABLE [#DangKyNghiTemp] (
[Ma] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
[MaNV] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
[HoTen] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,[MaBP] [int] NULL ,
[TenBoPhan] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
[TuNgay] [smalldatetime] NULL ,[DenNgay] [smalldatetime] NULL ,
[VietTat] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
[CongNghi] [float] NULL ,[TenVietTat] [varchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
[DKNLoai] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,[LDNMa] [int] )
CREATE TABLE [#Tong] ([MaNV] [varchar] (10),[TongNghi] [REAL] )    
		SET @SQL = '
		declare @NgayApDung smalldatetime
		declare @NgayketThuc smalldatetime
		Set @NgayApDung = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@StartDate)) +')
			+  '+ '''' +''+ '-' +''+ '''' +' 
			+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@StartDate)),2)) +')
			+  '+ '''' +''+ '-' +''+ '''' +' 
			+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@StartDate)),2)) +') ) 
			
		Set @NgayketThuc = Convert(smalldatetime,Convert(varchar(5),'+ CONVERT(VARCHAR(5),YEAR(@FinishDate)) +')
			+  '+ '''' +''+ '-' +''+ '''' +' 
			+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),MONTH(@FinishDate)),2)) +')
			+  '+ '''' +''+ '-' +''+ '''' +' 
			+ Convert(Varchar(3),'+ CONVERT(VARCHAR(3),RIGHT('00'+CONVERT(VARCHAR(2),Day(@FinishDate)),2)) +')	)
		INSERT INTO #DangKyNghiTemp(Ma,MaNV,HoTen,MaBP,TenBoPhan,TuNgay,DenNgay,VietTat,TenVietTat,DKNLoai,LDNMa)
		 SELECT nv.NVMa,nv.NVMaNV,nv.NVHoTen,bp.BPMa,bp.BPTen,A.DKNNgayApDung,A.DKNNgayKetThuc,B.LDNVietTat,B.LDNTen,A.DKNLoai,B.LDNMa
		 FROM tblDangKyNghi'+ Convert(varchar,@NamS) +' AS A 
		 INNER JOIN tblNhanVien nv ON nv.NVMa = A.DKNMaNV
		 INNER JOIN tblBoPhan bp ON bp.BPMa = nv.NVMaBP
		 INNER JOIN tblLyDoNghi AS B ON A.DKNMaLyDo = B.LDNMa 
		 INNER JOIN tblLoaiNghi AS C ON B.LDNLoai = C.LNMa   
		 WHERE  DATEDIFF(day,dbo.fn_Max(@NgayApDung,DKNNgayApDung),dbo.fn_Min(@NgayketThuc,DKNNgayKetThuc)) >= 0 '
				If @MaNV = 0
					set @SQL = @SQL + 'AND  bp.BPMa  IN ( '+dbo.DonVi_IDs(@MaBP) +')'
				else
					Set @SQL =  @SQL + ' AND A.DKNMaNV = ' + Convert(varchar,@MaNV) + ' '
		 EXEC (@SQL)
		 UPDATE #DangKyNghiTemp SET CongNghi = (DATEDIFF(day,dbo.fn_Max(@StartDate,TuNgay),dbo.fn_Min(@FinishDate,DenNgay))+1)*0.5
		 WHERE DKNLoai<>0
		 UPDATE #DangKyNghiTemp SET CongNghi = (DATEDIFF(day,dbo.fn_Max(@StartDate,TuNgay),dbo.fn_Min(@FinishDate,DenNgay))+1)
		 WHERE DKNLoai=0
if (select COUNT(*) from #DangKyNghiTemp)>0
BEGIN
EXEC Crosstab 'SELECT Ma FROM #DangKyNghiTemp GROUP BY Ma', 'sum(congnghi)', 'TenVietTat', '#DangKyNghiTemp'
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[bangtemp]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[bangtemp]
select D.Ma,D.MaNV,Sum(D.Congnghi) as TongNghi into bangtemp from abc1 as L, #DangKyNghiTemp as D
Where L.ma=D.ma Group by D.Ma,D.MaNV
--SELECT * FROM #DangKyNghiTemp
END
else
BEGIN
EXEC Crosstab 'SELECT LNma as Ma FROM tblloainghi GROUP BY LNma', 'sum(LNMa)', 'LNten', 'tblloainghi'
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[bangtemp]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[bangtemp]
CREATE TABLE [dbo].[bangtemp] (
	[Ma] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[MaNV] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[TongNghi] [float] NULL 
) ON [PRIMARY]
END
--SELECT * FROM bangtemp


GO
/****** Object:  StoredProcedure [dbo].[TongNghiCacThang]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE     PROC [dbo].[TongNghiCacThang]
@NgayBD SMALLDATETIME,
@NgayKT SMALLDATETIME,
@Bophan int,
@NhanVien int
AS
DECLARE @i INT
DECLARE @nam VARCHAR(500)
DECLARE @nam2 VARCHAR(500)
DECLARE @SQL NVARCHAR(4000)
DECLARE @SQLF NVARCHAR(4000)
DECLARE @SQL11 NVARCHAR(1000)
DECLARE @SQL111 NVARCHAR(1000)
DECLARE @SQL12 NVARCHAR(1000)
DECLARE @SQL122 NVARCHAR(1000)
DECLARE @SQL10 NVARCHAR(1000)
DECLARE @SQL100 NVARCHAR(1000)
DECLARE @SQL9 NVARCHAR(1000)
DECLARE @SQL99 NVARCHAR(1000)
DECLARE @SQL8 NVARCHAR(1000)
DECLARE @SQL88 NVARCHAR(1000)
DECLARE @SQL7 NVARCHAR(1000)
DECLARE @SQL77 NVARCHAR(1000)
DECLARE @SQL6 NVARCHAR(1000)
DECLARE @SQL66 NVARCHAR(1000)
DECLARE @SQL5 NVARCHAR(1000)
DECLARE @SQL55 NVARCHAR(1000)
DECLARE @SQL4 NVARCHAR(1000)
DECLARE @SQL44 NVARCHAR(1000)
DECLARE @SQL3 NVARCHAR(1000)
DECLARE @SQL33 NVARCHAR(1000)
DECLARE @SQL2 NVARCHAR(1000)
DECLARE @SQL22 NVARCHAR(1000)
DECLARE @SQL0 NVARCHAR(1000)
DECLARE @SQL00 NVARCHAR(1000)
DECLARE @SQlTT NVARCHAR(1000)
SET @i=MONTH(@NgayBD)
SET @SQL10=''
set @SQl100=''
Set @SQL11=''
set @SQL111=''
Set @SQL12=''
set @SQL122=''
SET @SQL9=''
set @SQl99=''
Set @SQL8=''
set @SQL88=''
Set @SQL7=''
set @SQL77=''
Set @SQL6=''
set @SQL66=''
Set @SQL5=''
set @SQL55=''
Set @SQL4=''
set @SQL44=''
Set @SQL3=''
set @SQL33=''
Set @SQL2=''
set @SQL22=''
Set @SQL0=''
set @SQL00=''
WHILE(@i<=MONTH(@NgayKT))
BEGIN
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[BangNghiCacThang]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[BangNghiCacThang]
	SET @SQLF=' into #BangNghiCacThang FROM tblnhanvien AS nv '
	IF(@i=1) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-31'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * INTO #thang1 FROM bangtemp
		SET @SQL0=@SQL0+' ,T1.Tongnghi AS  Thang1 '
		set @SQl00=@SQl00+' LEFT JOIN #Thang1 AS T1 ON nv.NVMaNV=T1.MaNV '

	End
	IF(@i=2) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-28'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * INTO #thang2 FROM bangtemp
		SET @SQL2=@SQL2+' ,T2.Tongnghi AS  Thang2 '
		set @SQl22=@SQl22+' LEFT JOIN #Thang2 AS T2 ON nv.NVMaNV=T2.MaNV '

	End

	IF(@i=3) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-31'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * INTO #thang3 FROM bangtemp
		SET @SQL3=@SQL3+' ,T3.Tongnghi AS  Thang3 '
		set @SQl33=@SQl33+' LEFT JOIN #Thang3 AS T3 ON nv.NVMaNV=T3.MaNV '

	End
	IF(@i=4) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-30'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * INTO #thang4 FROM bangtemp
		SET @SQL4=@SQL4+' ,T4.Tongnghi AS  Thang4 '
		set @SQl44=@SQl44+' LEFT JOIN #Thang4 AS T4 ON nv.NVMaNV=T4.MaNV '

	End
	IF(@i=5) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-31'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * INTO #thang5 FROM bangtemp
		SET @SQL5=@SQL5+' ,T5.Tongnghi AS  Thang5 '
		set @SQl55=@SQl55+' LEFT JOIN #Thang5 AS T5 ON nv.NVMaNV=T5.MaNV '

	End

	IF(@i=6) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-30'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * INTO #thang6 FROM bangtemp
		SET @SQL6=@SQL6+' ,T6.Tongnghi AS  Thang6 '
		set @SQl66=@SQl66+' LEFT JOIN #Thang6 AS T6 ON nv.NVMaNV=T6.MaNV '
	End

	IF(@i=7) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-31'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * INTO #thang7 FROM bangtemp
		SET @SQL7=@SQL7+' ,T7.Tongnghi AS  Thang7 '
		set @SQl77=@SQl77+' LEFT JOIN #Thang7 AS T7 ON nv.NVMaNV=T7.MaNV '

	End
	IF(@i=8) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-31'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		SELECT * into #thang8 FROM bangtemp
		SET @SQL8=@SQL8+' ,T8.Tongnghi AS  Thang8 '
		set @SQl88=@SQl88+' LEFT JOIN #Thang8 AS T8 ON nv.NVMaNV=T8.MaNV '
	End
	IF(@i=9) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-30'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		--insert into #thang9
		SELECT * into #thang9 FROM bangtemp
		SET @SQL9=@SQL9+' ,T9.Tongnghi AS  Thang9 '
		set @SQl99=@SQl99+' LEFT JOIN #Thang9 AS T9 ON nv.NVMaNV=T9.MaNV '
	END
	IF(@i=10) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-31'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		--SELECT a.*,tmp.TongNghi INTO thang10 FROM bangtemp tmp INNER JOIN abc1 a ON tmp.MaNV=a.maNV
		SELECT * into #thang10 FROM bangtemp
		SET @SQL10=@SQL10+' ,T10.Tongnghi AS  Thang10 '
		set @SQl100=@SQl100+' LEFT JOIN #Thang10 AS T10 ON nv.NVMaNV=T10.MaNV '
	END
	IF(@i=11) 
	BEGIN

		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-30'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		--SELECT tmp.TongNghi INTO thang11 FROM bangtemp tmp INNER JOIN abc1 a ON tmp.MaNV=a.maNV
		SELECT * into #thang11 FROM bangtemp
		SET @SQL11=@SQL11+' ,T11.Tongnghi AS  Thang11 '
		set @SQl111=@SQl111+' LEFT JOIN #Thang11 AS T11 ON nv.NVMaNV=T11.MaNV '
	End
	IF(@i=12) 
	BEGIN
		SET @nam=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-01'
		SET @nam2=convert(varchar,YEAR(@NgayBD))+'-'+convert(varchar,@i)+'-31'
		EXEC TongHopDangKyNghi_Thang @NhanVien,@Bophan,@nam,@nam2
		--SELECT * FROM bangtemp
		SELECT * into #thang12 FROM bangtemp
		SET @SQL12=@SQL12+' ,T12.Tongnghi AS  Thang12 '
		set @SQl122=@SQL122+' LEFT JOIN #Thang12 AS T12 ON nv.NVMaNV=T12.MaNV '		
	END
SET @i=@i+1
END
--Goi thu tuc TG_Sl_DiMuonVeSom
EXEC TG_Sl_DiMuonVeSom @NgayBD,@NgayKT,@Bophan,@NhanVien
SELECT BCMaNV,SUM(TongDMVS) AS TongDMVS, round(Sum(TongTGDMVS) * (1.0/60),2) AS TongTGDMVS INTO #TGDMVS FROM Sl_TGDMVS Group BY BCMaNV
--Goi thu tuc BaoCaoVangMat
--exec BaoCaoVangMat1 @NgayBD,@NgayKT,@Bophan,@NhanVien
--SELECT maNV2,count(maNV2) AS TongVM INTO #TongVangmat FROM tblBaoCaoVangMat GROUP BY MaBP,maNV2
--Thuc thi cau lenh SQL
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[tblbaocaotonghopThang]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[tblbaocaotonghopThang]
SET @SQL=@SQL0+@SQL2+@SQL3+@SQL4+@SQL5+@SQL6+@SQL7+@SQL8+@SQL9+@SQL10+@SQL11+@SQL12+@SQLF
+@SQL00+@SQL22+@SQL33+@SQL44+@SQL55+@SQL66+@SQL77+@SQL88+@SQL99+@SQl100+@SQl111+@SQl122
SET @SQlTT='SELECT thang.*,tmp.TongNghi,a.*,tg.TongDMVS,tg.TongTGDMVS  into tblbaocaotonghopThang FROM #BangNghiCacThang as thang LEFT JOIN bangtemp tmp ON thang.NVMa=tmp.Ma 
LEFT JOIN abc1 a ON tmp.Ma=a.Ma LEFT JOIN #TGDMVS as tg ON tg.BCMaNV=thang.NVma  '
EXEC TongHopDangKyNghi_Thang 0,0,@NgayBD,@NgayKT
DECLARE @Kiemtra VARCHAR(1000)
IF(@NhanVien<>0)
BEGIN
	SET @Kiemtra=' inner JOIN tblbophan as BP on BP.BPMa=nv.NVMaBP Left Join tblchucvu as CV on CV.CVMa=nv.NVMaCV where nv.NVma='+convert(varchar,@NhanVien)
	exec('Select nv.NVma,nv.NVmaNV,Nv.NVHoTen,BP.BPTen,CV.CVTen '+@SQL+@Kiemtra+@SQlTT)
END
ELSE
BEGIN
	SET @Kiemtra=' inner JOIN tblbophan as BP on BP.BPMa=nv.NVMaBP Left Join tblchucvu as CV on CV.CVMa=nv.NVMaCV where nv.NVMaBP in('+dbo.DonVi_IDs(@Bophan)+')'
	exec('Select nv.NVma,nv.NVmaNV,Nv.NVHoTen,BP.BPTen,CV.CVTen '+@SQL+@Kiemtra+@SQlTT)
END
ALTER TABLE tblbaocaotonghopThang
DROP COLUMN Ma

GO
/****** Object:  StoredProcedure [dbo].[usp_CountStaffs]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Batch submitted through debugger: SQLQuery3.sql|7|0|C:\Users\Administrator\AppData\Local\Temp\~vsCEC2.sql


/************************************************************
*  Routine       :	dbo.ufn_CountStaffs
*  Created by    :  Duong Van Quyet , at 09/02/2011 - 15:53:20  
*  Machine       :  DIGISOFT
*  Description   :  Dem tong so nhan vien cua bo phan
*  Parameters    :  @MaBP int
************************************************************/
CREATE PROCEDURE [dbo].[usp_CountStaffs]
(@MaBP INT, @SoNV NVARCHAR(15) OUTPUT)
AS
BEGIN
	DECLARE @SQL     NVARCHAR(max)
	DECLARE @Return  NVARCHAR(9)
	if(@MaBP=0)
		set @sql ='select COUNT(*) from tblNhanVien'
	else
	begin
	SET
	 @SQL = 
	    'SELECT COUNT(*) AS TongNV FROM tblNhanVien INNER JOIN tblBoPhan ON tblNhanVien.NVMaBP= tblbophan.BPMa WHERE (NVMaBP IN ( ' 
	    + dbo.DonVi_IDs(@MaBP) + ')) AND'+
	
		   ' GETDATE() >= (
			CASE 
				 WHEN 1 = (
						  SELECT CONVERT(VARCHAR(1), TSGiaTri)
						  FROM   tblThamSo
						  WHERE  TSTen = ''SHOWALLSTAFF''
					  ) THEN GETDATE()
				 ELSE tblNhanVien.NVNgayVao
			END
		)
	  AND 
		  GETDATE() <= (
			CASE 
				 WHEN 1 = (
						  SELECT CONVERT(VARCHAR(1), TSGiaTri)
						  FROM   tblThamSo
						  WHERE  TSTen = ''SHOWALLSTAFF''
					  ) THEN GETDATE()
				 ELSE tblNhanVien.NVNgayRa
			END
		)
		   '
		   end
	EXEC usp_GetValueFromSQL @SQL,
	     @Return OUTPUT
	
	SET @SoNV = @Return
END

GO
/****** Object:  StoredProcedure [dbo].[usp_GetValueFromSQL]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/************************************************************
*  Routine       :	usp_GetValueFromSQL
*  Created by    :  Duong Van Quyet , at 09/02/2011 - 15:47:01  
*  Machine       :  DIGISOFT
*  Description   :  Lay gia tri tu chuoi SQL
*  Parameters    :  @strSql varchar(4000), @strReturn varchar(255)
************************************************************/

CREATE PROCEDURE [dbo].[usp_GetValueFromSQL]
	@strSql VARCHAR(max),
	@strReturn NVARCHAR(max) OUTPUT
AS
	DECLARE @strCmd1 VARCHAR(max)
	SELECT @strCmd1 = ' DECLARE sp_cursor CURSOR FOR ' + @strSql
	EXEC (@strcmd1)
	OPEN sp_cursor
	FETCH NEXT FROM sp_cursor INTO @strReturn
	IF (@@FETCH_STATUS <> 0)
	BEGIN
	    SET @strReturn = ''
	END

	    CLOSE sp_cursor 
	    DEALLOCATE sp_cursor

  
GO
/****** Object:  StoredProcedure [dbo].[usp_tblDangKyNghi_InsUpd]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_tblDangKyNghi_InsUpd]
(
	@_LeaveTypeId int, 
	@_EmployeeCode nvarchar(50),
	@_StartDate datetime, 
	@_EndDate datetime, 
	@_HRMCode nvarchar(50)
)
AS
BEGIN
DECLARE @_DKNMa nvarchar(50),@_DKNMaNV nvarchar(50)

SELECT TOP(1) @_DKNMa = DKNMa FROM tblDangKyNghi ORDER BY Date1 desc
SET @_DKNMa = CONVERT(int,@_DKNMa) + 1
SELECT @_DKNMa

SELECT @_DKNMaNV = NVMa FROM tblNhanVien WHERE NVMaNV = @_EmployeeCode

IF EXISTS (SELECT 1 FROM tblDangKyNghi WHERE WebId = @_LeaveTypeId)
BEGIN
--UPDATE
UPDATE dbo.tblDangKyNghi
SET			DKNMaNV = @_DKNMaNV
           ,DKNNgayApDung = @_StartDate
           ,DKNNgayKetThuc = @_EndDate
           ,DKNMaLyDo = @_HRMCode
           ,DKNLoai = '0001'
           ,DKNNghiBu = 0
           ,User1 = 0
           ,Date1 = GETDATE()
WHERE WebId = @_LeaveTypeId
    
END
ELSE
BEGIN
INSERT INTO dbo.tblDangKyNghi
           (DKNMa
           ,DKNMaNV
           ,DKNNgayApDung
           ,DKNNgayKetThuc
           ,DKNMaLyDo
           ,DKNLoai
           ,DKNNghiBu
           ,User1
           ,Date1
           ,WebId)
     VALUES
           (@_DKNMa
           ,@_DKNMaNV
           ,@_StartDate
           ,@_EndDate
           ,@_HRMCode
           ,'0001'
           ,0
           ,0
           ,GETDATE()
           ,@_LeaveTypeId)
END


END
GO
/****** Object:  StoredProcedure [dbo].[XoaBangRecordData]    Script Date: 9/19/2026 10:24:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[XoaBangRecordData]

 @Tungay AS DATETIME,
 @DenNgay AS DATETIME
 
 AS 
DECLARE @D AS DATETIME
DECLARE @Tenbaocao AS NVARCHAR(50)


--SET @Tungay='2010-01-01'


--SET @DenNgay='2011-01-01'

DECLARE @SQL NVARCHAR(4000)
SET @D=@Tungay
WHILE(@D<=@DenNgay)
BEGIN

	
	SET @Tenbaocao='RecordData' + convert(varchar(4),YEAR(@D)) + '_' +  RIGHT( '00' + convert(varchar(2),month(@D)),2)
	
	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].['+ @Tenbaocao +']') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	BEGIN
	set @SQl='DROP TABLE '+ @Tenbaocao
	
	PRINT  @SQl
	--exec(@SQl)
	END
	SET @D= DATEADD(MONTH,1,@D)
END
GO
