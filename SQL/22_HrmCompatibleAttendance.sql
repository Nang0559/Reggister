USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
/*
 HRM-COMPATIBLE ATTENDANCE CALCULATION
 HRM is READ ONLY. This is a port of HRM.dbo.sphrmvn_TimeKeepingForStaff.
*/
IF OBJECT_ID(N'dbo.F03HrmAttendanceCalculated',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03HrmAttendanceCalculated(
  Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03HrmAttendanceCalculated PRIMARY KEY,
  CalculationBatchId uniqueidentifier NOT NULL, CalculationVersion nvarchar(50) NOT NULL, WorkDate date NOT NULL,
  HrmEmployeeId int NOT NULL, EmployeeCode nvarchar(50) NULL, FullName nvarchar(200) NULL,
  HrmDeptId int NULL, DeptCode nvarchar(20) NULL, HrmPositionId int NULL, ShiftId int NULL, ShiftAbbr nvarchar(10) NULL,
  CheckInGate int NULL, CheckInTime datetime2(0) NULL, CheckOutGate int NULL, CheckOutTime datetime2(0) NULL,
  ExitGate int NULL, ExitTime datetime2(0) NULL, EntryGate int NULL, EntryTime datetime2(0) NULL,
  WorkMinutesDay int NOT NULL DEFAULT 0, WorkMinutesNight int NOT NULL DEFAULT 0,
  OTMinutesDay int NOT NULL DEFAULT 0, OTMinutesNight int NOT NULL DEFAULT 0, OTMinutesDayTC int NOT NULL DEFAULT 0, OTMinutesNightTC int NOT NULL DEFAULT 0,
  OTRecognizedMinutesDay int NOT NULL DEFAULT 0, OTRecognizedMinutesNight int NOT NULL DEFAULT 0,
  LateMinutesDay int NOT NULL DEFAULT 0, LateMinutesNight int NOT NULL DEFAULT 0, EarlyLeaveMinutesDay int NOT NULL DEFAULT 0, EarlyLeaveMinutesNight int NOT NULL DEFAULT 0,
  RequiredMinutes int NOT NULL DEFAULT 0,
  LeaveTotal decimal(9,2) NULL, LeaveAnnual decimal(9,2) NULL, Leave100 decimal(9,2) NULL, Leave70 decimal(9,2) NULL, LeaveUnpaid decimal(9,2) NULL,
  LeaveBH100 decimal(9,2) NULL, LeaveBH70 decimal(9,2) NULL, LeaveBusinessTrip decimal(9,2) NULL, LeaveCompensatory decimal(9,2) NULL, LeaveOther decimal(9,2) NULL,
  LeaveTypeCode nvarchar(20) NULL, LeaveReason nvarchar(50) NULL, Note nvarchar(500) NULL,
  HrmType bit NULL, HrmHoliday bit NULL, HrmEmployeeHoliday bit NULL, IsLocked bit NULL,
  HrmBCGhiChu nvarchar(50) NULL, HrmBCLyDoNghi nvarchar(20) NULL, HrmBCNghiTotal decimal(9,2) NULL,
  HrmBCNghiPhep decimal(9,2) NULL, HrmBCNghiH100 decimal(9,2) NULL, HrmBCNghiH70 decimal(9,2) NULL, HrmBCNghiKL decimal(9,2) NULL,
  HrmBCNghiBH100 decimal(9,2) NULL, HrmBCNghiBH70 decimal(9,2) NULL, HrmBCNghiCongTac decimal(9,2) NULL, HrmBCNghiBu decimal(9,2) NULL, HrmBCNghiKhac decimal(9,2) NULL,
  HrmBCDaXacNhanLamThem bit NULL, HrmBCLoaiLamThem bit NULL, HrmBCTinhLamThem bit NULL, HrmBCNgayLe int NULL, HrmBCNgayLeNV int NULL,
  AttendanceDisplayValue nvarchar(50) NULL, OtDisplayValue nvarchar(50) NULL,
  CalculatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03HrmAttendanceCalculated_CalculatedAt DEFAULT GETDATE(),
  CalculatedBy nvarchar(100) NULL, SourceSystem nvarchar(20) NOT NULL CONSTRAINT DF_F03HrmAttendanceCalculated_SourceSystem DEFAULT N'HRM',
  CONSTRAINT UQ_F03HrmAttendanceCalculated_Batch UNIQUE(CalculationBatchId,HrmEmployeeId,WorkDate)
 );
END;
GO
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCGhiChu') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCGhiChu nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCLyDoNghi') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCLyDoNghi nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiTotal') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiTotal decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiPhep') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiPhep decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiH100') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiH100 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiH70') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiH70 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiKL') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiKL decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiBH100') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiBH100 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiBH70') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiBH70 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiCongTac') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiCongTac decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiBu') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiBu decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiKhac') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiKhac decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCDaXacNhanLamThem') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCDaXacNhanLamThem bit NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCLoaiLamThem') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCLoaiLamThem bit NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCTinhLamThem') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCTinhLamThem bit NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNgayLe') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNgayLe int NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNgayLeNV') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNgayLeNV int NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'AttendanceDisplayValue') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD AttendanceDisplayValue nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'OtDisplayValue') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD OtDisplayValue nvarchar(50) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmAttendanceCalculated') AND name=N'IX_F03HrmAttendanceCalculated_DateDept')
 CREATE INDEX IX_F03HrmAttendanceCalculated_DateDept ON dbo.F03HrmAttendanceCalculated(WorkDate,DeptCode,HrmEmployeeId,Id);
GO
IF OBJECT_ID(N'dbo.F03HrmOTActual',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03HrmOTActual(
  Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03HrmOTActual PRIMARY KEY,
  CalculationBatchId uniqueidentifier NOT NULL, WorkDate date NOT NULL, HrmEmployeeId int NOT NULL,
  EmployeeCode nvarchar(50) NULL, DeptCode nvarchar(20) NULL, ActualStartTime datetime2(0) NULL, ActualEndTime datetime2(0) NULL,
  ActualMinutes int NOT NULL DEFAULT 0, ActualOTDayMinutes int NOT NULL DEFAULT 0, ActualOTNightMinutes int NOT NULL DEFAULT 0,
  RecognizedOTMinutes int NOT NULL DEFAULT 0, SourceAttendanceId bigint NULL, CalculatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
  CONSTRAINT UQ_F03HrmOTActual_Batch UNIQUE(CalculationBatchId,HrmEmployeeId,WorkDate)
 );
END;
GO
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
GO
CREATE OR ALTER PROCEDURE dbo.usp_HrmCompatibleTimeKeepingForStaff
	@StaffID int,	
	@D Datetime
 AS
	BEGIN
    CREATE TABLE #tblBaoCao ([BCNgay] [datetime] NOT NULL,
	[BCDay] [int] NULL,
	[BCMonth] [int] NULL,
	[BCYear] [int] NULL,
	[BCMaNV] [int] NOT NULL,
	[BCMaBP] [int] NOT NULL,
	[BCMaCV] [int] NOT NULL,
	[BCMaCa] [int] NOT NULL,
	[BCCuaDen] [int] NOT NULL,
	[BCTGDen] [datetime] NULL,
	[BCCuaVe] [int] NOT NULL,
	[BCTGVe] [datetime] NULL,
	[BCCuaRa] [int] NOT NULL,
	[BCTGRa] [datetime] NULL,
	[BCCuaVao] [int] NOT NULL,
	[BCTGVao] [datetime] NULL,
	[BCTGLamNgay] [smallint] NOT NULL,
	[BCTGLamToi] [smallint] NOT NULL,
	[BCTGQuaGioNgay] [smallint] NOT NULL,
	[BCTGQuaGioToi] [smallint] NOT NULL,
	[BCTGThemNgay] [smallint] NOT NULL,
	[BCTGThemToi] [smallint] NOT NULL,
	[BCTGRaNgoaiNgay] [smallint] NOT NULL,
	[BCTGRaNgoaiToi] [smallint] NOT NULL,
	[BCTGDiMuonNgay] [smallint] NOT NULL,
	[BCTGDiMuonToi] [smallint] NOT NULL,
	[BCTGVeSomNgay] [smallint] NOT NULL,
	[BCTGVeSomToi] [smallint] NOT NULL,
	[BCTGQuyDinh] [smallint] NOT NULL,
	[BCGhiChu] [nvarchar](50) NULL,
	[BCLoai] [bit] NOT NULL,
	[BCLoaiLamThem] [bit] NOT NULL,
	[BCTinhLamThem] [bit] NOT NULL,
	[BCNghiBuChoNgay] [float] NULL,
	[BCNghiPhep] [float] NULL,
	[BCNghiH100] [float] NULL,
	[BCNghiH70] [float] NULL,
	[BCNghiKL] [float] NULL,
	[BCNghiBH100] [float] NULL,
	[BCNghiBH70] [float] NULL,
	[BCNghiCongTac] [float] NULL,
	[BCNghiBu] [float] NULL,
	[BCNghiKhac] [float] NULL,
	[BCLoaiNgayNghi] [smallint] NULL,
	[BCLydonghi] [nvarchar](20) NULL,
	[BCTGNghi] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[BCTGDKNTheogio] [float] NULL,
	[BCLoaiDKN] [varchar](10) NULL,
	[BCDangKyLTN] [float] NULL,
	[BCDangKyLTD] [float] NULL,
	[BCQuanLyXacNhanLT] [nvarchar](200) NULL,
	[BCGhiChuLT] [nvarchar](500) NULL,
	[BCTGThemNgayTamTinh] [smallint] NULL,
	[BCTGThemToiTamTinh] [smallint] NULL,
	[BCGhiChuXNLT] [nvarchar](200) NULL,
	[BCDaXacNhanLamThem] [bit] NULL,
	[BCLichTrinhCa] [varchar](50) NULL,
	[BCLoaiDoiCa] [bit] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[BCTGQuaGioNgayTC] [smallint] NULL,
	[BCTGQuaGioToiTC] [smallint] NULL,
	[BCDangKyUuDai] [bit] NULL,
	[BCCNLamBT] [bit] NULL,
	[BCDangKyGiamCa] [int] NULL,
	[BCTGDoiCa] [smalldatetime] NULL,
	[BCMaCaOld] [nvarchar](50) NULL,
	[BCLoaiDKN1] [varchar](10) NULL,
	[BCTGNghi1] [float] NULL,
	[BCLydonghi1] [nvarchar](20) NULL,
	[BCLaDKNTrucTiep] [bit] NULL,
	[BCLoaiUuDai] [nvarchar](50) NULL,
	[BCUudai1] [int] NULL,
	[BCUudai2] [int] NULL,
	[BCUudai3] [int] NULL,
	[BCUudai4] [int] NULL,
	[OTTrongNTC] [int] NULL,
	[OTTrongDTC] [int] NULL,
	[TG1] [datetime] NULL,
	[TG2] [datetime] NULL,
	[TG3] [datetime] NULL,
	[TG4] [datetime] NULL,
	[TG5] [datetime] NULL,
	[TG6] [datetime] NULL,
	[TG7] [datetime] NULL,
	[TG8] [datetime] NULL,
	[TG9] [datetime] NULL,
	[TG10] [datetime] NULL,
	[BCNgayLe] [int] NULL,
	[BCNgayLeNV] [int] NULL,
	[OTTrongNSC] [int] NULL,
	[OTTrongDSC] [int] NULL,
	[BCTGUuDaiN] [int] NULL,
	[BCTGUuDaiD] [int] NULL,
	[BCNhietDoDen] [float] NULL,
	[BCNhietDoVe] [float] NULL,
	[BCNgayNghiBu] [datetime] NULL);
		IF EXISTS (SELECT DLocked FROM #tblBaoCao WHERE BCNgay=@D AND BCMaNV=@StaffID AND DLocked=1)
		RETURN

		DECLARE @TGBDCa Datetime 	-- thoi gian bat dau vao ca
		DECLARE @TGBDNghi1  Datetime 	-- thoi gian nghi giua ca sang
		DECLARE @TGKTNghi1 Datetime 	
		DECLARE @TGBDNghi2  Datetime 	-- thoi gian nghi giua ca 
		DECLARE @TGKTNghi2 Datetime 	
		DECLARE @TGBDNghi3  Datetime 	-- thoi gian nghi giua ca Chieu
		DECLARE @TGKTNghi3 Datetime 	
		DECLARE @TGKTCa Datetime  -- thoi gian ket thuc ca
		DECLARE @TGQDD INT		-- Thoi gian quy dinh nua ca dau VD :240
		DECLARE @TGQDC INT      -- Thoi gian quy dinh nua ca sau VD :240
		DECLARE @TGQD INT

		DECLARE @CVietTat VARCHAR(10)
		DECLARE @TGBDTinhDM Datetime
		DECLARE @TGBDTinhVS Datetime 
		DECLARE @TGNghiGiuaGio INT  -- luu thoi gian nghi giua ca
		DECLARE @TGBatDauLT Datetime -- Moc thoi gian bat dau tinh lam them
		DECLARE @TGBatDauLTTC Datetime -- Moc thoi gian bat dau tinh lam them truoc ca
		DECLARE @PhutBatDauLT FLOAT
		DECLARE @PhutBatDauLTTC FLOAT
		DECLARE @DuocRaNgoai bit
		DECLARE @CongNghiGiuaCa BIT
		DECLARE @CChiaLTSauca BIT
		DECLARE @TinhVaoSom bit
		DECLARE @CNgaynghi SMALLINT		-- Ngay Nghi  |  0: mac dinh; 1: ngay thuong; 2: ngay nghi 150; 3: Ngay nghi 200; 4 :Ngay le
		DECLARE @LoaiCa SMALLINT
	
		DECLARE @DonViChamCong INT		-- Don vi cham cong
		DECLARE @DonViLamThem SMALLINT	-- Don vi lam them
		DECLARE @NguongLamThem	SMALLINT -- Nguong lam them
		DECLARE @NguongLamThemTC	SMALLINT -- Nguong lam themTC
		DECLARE @NguongDiMuon SMALLINT		-- Nguong di muon 
		DECLARE @NguongVeSom SMALLINT		-- Nguong ve som
	
		DECLARE @QuetTruocCa int
		DECLARE @QuetSauCa INT
		DECLARE @TGLayDLDau  Datetime 	
		DECLARE @TGLayDLCuoi Datetime 

		-- Khai bao ten cac bang
		DECLARE @RecordData nvarchar(30) 	
		DECLARE @tblBaocao nvarchar(128) 	
		DECLARE @tblDangkynghi nvarchar(128) 	
		DECLARE @tblDangkyuudai nvarchar(128) 
		DECLARE @tblDangkynghitheogio nvarchar(128) 	
	
		-- Khai bao bien trong bang Recorddata
		DECLARE @IDM SMALLINT		
		DECLARE @ThoiGian  Datetime 
		DECLARE @IDCard nvarchar(20)
		DECLARE @Status bit
		DECLARE @HandData bit
		DECLARE @Pass bit
	
		-- Khai bao cac nguong lay DL
		DECLARE @ThresholdDay Datetime	--Nguong tinh lam ngay  06:00:00
		DECLARE @ThresholdNight Datetime -- Nguong tinh lam dem 22:00:00
		DECLARE @ThresholdDay1 Datetime 
		DECLARE @ThresholdDay2 Datetime 
		DECLARE @ThresholdDay3 DATETIME
		DECLARE @ThresholdNight1 Datetime 
		DECLARE @ThresholdNight2 Datetime 
		DECLARE @ThresholdNight3 DATETIME
	
		-- Khoi ta cac bien Update
		DECLARE	@TGDen Datetime
		DECLARE	@Cuaden SMALLINT
		DECLARE	@TGVe Datetime
		DECLARE	@CuaVe smallint
		DECLARE	@TGVao Datetime
		DECLARE	@CuaVao smallint
		DECLARE	@TGRa Datetime
		DECLARE	@CuaRa smallint
		DECLARE @TGLamN int
		DECLARE @TGLamD int
		DECLARE @TGQuaN int
		DECLARE @TGQuaD int
		DECLARE @TGQuaNTC int
		DECLARE @TGQuaDTC int
		DECLARE @TGOTTrongCaN int
		DECLARE @TGOTTrongCaD int
		DECLARE @TGOTTrongCaNTC int
		DECLARE @TGOTTrongCaDTC int
		DECLARE @TGThemN int
		DECLARE @TGThemD int
		DECLARE @TGMuonN int
		DECLARE @TGMuonD INT
		DECLARE @TGSomN int
		DECLARE @TGSomD INT

		-- Khai bao dang ky nghi theo gio
		DECLARE @DKNTGNgay DATETIME -- Ngay dang ky nghi theo gio
		DECLARE @DKNTGGio DATETIME -- Gio dang ky nghi theo gio
		DECLARE	@DKNTGSophut INT  -- So phut DKN theo gio
		DECLARE	@DKNTGLydo INT  -- So phut DKN theo gio
		DECLARE @DKNTGDengio datetime
		DECLARE @DKNTGTugio datetime

		-- Khai bao dang ky nghi theo ca
		DECLARE @NgayApDung SMALLDATETIME
		DECLARE @NgayKetThuc SMALLDATETIME
		DECLARE @LoaiNghi smallint
		DECLARE @TGNghi FLOAT
		DECLARE @MaLyDoNghi nvarchar(10)
		DECLARE @LydonghiViettat nvarchar(10)
		DECLARE @Ghichu nvarchar(50)

		-- khai bao dang ky uu dai
		DECLARE @TGUD1 int
		DECLARE @TGUD2 int
		DECLARE @TGUD3 int
		DECLARE @TGUD4 int
		DECLARE @UD1 int
		DECLARE @UD2 int
		DECLARE @UD3 int
		DECLARE @UD4 int
    
		-- Thoi gian lam ngay, lam dem, them ngay, them dem giao voi khoang DKN
		DECLARE @DKNTGLamN int
		DECLARE @DKNTGLamD int
		DECLARE @DKNTGDiMuonN int
		DECLARE @DKNTGDiMuonD int
		DECLARE @DKNTGVeSomN int
		DECLARE @DKNTGVeSomD int
		DECLARE @DKNTGTrongCaNTC int
		DECLARE @DKNTGTrongCaDTC INT
		DECLARE @DKNTGTrongCaNSC int
		DECLARE @DKNTGTrongCaDSC INT
		DECLARE @DKNTGLamTam FLOAT    

		-- Bien tam dem tong so ban ghi tra ve
		DECLARE	@recordCount SMALLINT
		-- Luu kieu xac dinh trang thai vao ra
		DECLARE @OriginalStatus SMALLINT
		-- Kieu tinh gio
		DECLARE @TypeCalculator  SMALLINT
		-- Khai bao bien ma the cua nhan vien
		DECLARE @MaThe nvarchar(20) 

		DECLARE @sLocDuLieu nvarchar(4000)
		DECLARE @SQL nvarchar(4000)
		DECLARE @NumberRecords int
		DECLARE @RowCount int
		DECLARE @Count int

		DECLARE @TGBDCa_Loc datetime
		DECLARE @TGBDNCa_Loc datetime
		DECLARE @TGKTNCa_Loc datetime
		DECLARE @TGKTCa_Loc datetime

		DECLARE @TGDen_Temp datetime
		DECLARE @TGRa_Temp datetime
		DECLARE @TGVao_Temp datetime
		DECLARE @TGVe_Temp datetime

		DECLARE	@TGOTTrongTC Datetime
		DECLARE	@TGOTTrongSC Datetime

		DECLARE @TGNghiTruaN int
		DECLARE @TGNghiTruaD int

		DECLARE @TGUuDaiN int
		DECLARE @TGUuDaiD int

		DECLARE @TGDM int
		DECLARE @TGVS int

		DECLARE @BCNgayLe int
		DECLARE @BCNgayLeNV int
	-----------------------------------------------------------------------------

		SET	@TGDen ='1900-01-01 00:00:00.000'
		Set	@TGVe ='1900-01-01 00:00:00.000'
		Set	@TGVao ='1900-01-01 00:00:00.000'
		Set	@TGRa ='1900-01-01 00:00:00.000'

		SET @TGBDTinhDM='1900-01-01 00:00:00.000'
		SET @TGBDTinhVS='1900-01-01 00:00:00.000'

		SET @TGBDCa_Loc ='1900-01-01 00:00:00.000'
		SET @TGBDNCa_Loc ='1900-01-01 00:00:00.000'
		SET @TGKTNCa_Loc ='1900-01-01 00:00:00.000'
		SET @TGKTCa_Loc ='1900-01-01 00:00:00.000'

		SET @TGDen_Temp ='1900-01-01 00:00:00.000'
		SET @TGRa_Temp ='1900-01-01 00:00:00.000'
		SET @TGVao_Temp ='1900-01-01 00:00:00.000'
		SET @TGVe_Temp ='1900-01-01 00:00:00.000'

		SET @Ghichu=''
		SET @LydonghiViettat=''

		Set	@Cuaden =0
		Set	@CuaVe =0
		Set	@CuaVao =0
		Set	@CuaRa =0
		
		SET @TGUD1 = 0
		SET @TGUD2 = 0
		SET @TGUD3 = 0
		SET @TGUD4 = 0
		SET @UD1 = 0
		SET @UD2 = 0
		SET @UD3 = 0
		SET @UD4 = 0
	
		SET @TGOTTrongCaN =0
		SET @TGOTTrongCaD =0
		SET @TGOTTrongCaNTC =0
		SET @TGOTTrongCaDTC =0
		SET @TGLamN =0
		SET @TGLamD =0
		SET @TGQuaN =0
		SET @TGQuaD =0
		SET @TGQuaNTC =0
		SET @TGQuaDTC =0
		SET @TGQD=480

		SET @DKNTGLamN =0
		SET @DKNTGLamD =0
		SET @DKNTGDiMuonN =0
		SET @DKNTGDiMuonD =0
		SET @DKNTGVeSomN =0
		SET @DKNTGVeSomD =0
		SET @DKNTGTrongCaNTC =0
		SET @DKNTGTrongCaDTC =0
		SET @DKNTGTrongCaNSC =0
		SET @DKNTGTrongCaDSC =0

		SET @LoaiNghi =0
		SET @TGThemN =0
		SET @TGThemD =0
		SET @TGMuonN =0
		SET @TGMuonD =0
		SET @TGSomN =0
		SET @TGSomD =0
		SET @TGNghiTruaN=0
		SET @TGNghiTruaD=0
		SET @DKNTGLamTam=0
		SET @TGUuDaiN=0
		SET @TGUuDaiD=0
		SET @BCNgayLe=0
		SET @BCNgayLeNV=0
		SET @Count=0
	

		-- Lay tham so nguong lam ngay 06:00:00
		SELECT @SQL= TSGiaTri FROM HRM.dbo.tblThamSo  WHERE TSTen='THRESHOLDDAY' 
		SET @ThresholdDay=CONVERT(Datetime,@SQL)

		-- Lay tham so nguong lam ngay 22:00:00
		SELECT @SQL= TSGiaTri FROM HRM.dbo.tblThamSo  WHERE TSTen='THRESHOLDNIGHT'
		SET @ThresholdNight=CONVERT(Datetime,@SQL)
	
		-- Cach lay du lieu quet: Xem ke trang thai hoac lay tu dau doc
		SELECT @SQL= TSGiaTri FROM HRM.dbo.tblThamSo  WHERE TSTen='ORIGINALSTATUS'
		SET @OriginalStatus=CONVERT(int,@SQL)
	
		-- Cach tinh cho trang thai vao ra: 0: Tinh tat cac cac cap vao ra - 1: Vao dau tien ra cuoi cung - 2:Quet dau tien Quet cuoi cung
		--SELECT @SQL= TSGiaTri FROM HRM.dbo.tblThamSo  WHERE TSTen='TYPECALCULATOR'
		--SET @TypeCalculator=CONVERT(int,@SQL)
	
		-- Lay bang RecordData thang may
		--SET @RecordData = case when LEN(Month(@D))=2 
		--						then 'RecordData' + CONVERT(nvarchar,YEAR(@D))  +'_'+  CONVERT(nvarchar,Month(@D))
		--						else 'RecordData'  + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D)) 
		--				   end
						   
		SET @RecordData = 'HRM.dbo.RecordDataNew'
	
		-- Thiet lap ten bao cao
		SET @tblBaocao = '#tblBaoCao'
		SET @tblDangkynghi = 'HRM.dbo.tblDangKyNghi' 
		SET @tblDangkyuudai = 'HRM.dbo.tblDangKyUuDai'
		SET @tblDangkynghitheogio = 'HRM.dbo.tblDangKyNghiTheoGio'

		-- Lay cac nguong tinh tinh gio
		SET @ThresholdDay = @D + CONVERT(VARCHAR(8),@ThresholdDay,108)	-- '2009-10-30 06:00:00'
		SET @ThresholdNight = @D + CONVERT(VARCHAR(8),@ThresholdNight,108)	-- '2009-10-30 22:00:00'
		SET @ThresholdDay1 = DateAdd(day, -1, @D) + CONVERT(VARCHAR(8),@ThresholdDay,108)	-- '2009-10-30 06:00:00'
		SET @ThresholdDay2 = @D + CONVERT(VARCHAR(8),@ThresholdDay,108)						-- '2009-10-31 06:00:00'
		SET @ThresholdDay3 = DateAdd(day, 1, @D) + CONVERT(VARCHAR(8),@ThresholdDay,108)	-- '2009-11-01 06:00:00'
		SET @ThresholdNight1 =DateAdd(day, -1, @D) + CONVERT(VARCHAR(8),@ThresholdNight,108)	-- '2009-10-30 22:00:00'
		SET @ThresholdNight2 = @D +  CONVERT(VARCHAR(8),@ThresholdNight,108)						-- '2009-10-31 22:00:00'
		SET @ThresholdNight3 = DateAdd(day, 1, @D) +  CONVERT(VARCHAR(8),@ThresholdNight,108)	-- '2009-11-01 22:00:00'
	
		DECLARE @MaNV int
		DECLARE @DepartmentID int
		DECLARE @PositionID int
		DECLARE @AllowOT int
		DECLARE @ShiftCode int
		DECLARE	@Maca INT
		SET @Maca=0

		Create TABLE #tblBaocaotam ([BCManv] [int],[BCmaca][int],[BCLoai] [bit] )
		EXEC ('Insert INTO #tblBaocaotam(BCManv,bcmaca,BCLoai) Select BCManv,bcmaca,BCloai  From  ' + @tblBaocao + ' WHERE (BCMaNV=' + @StaffID +') AND (BCNgay=' + '''' + @D + '''' + ')')
	
		select @MaNV = BCManv, @Maca = ISNULL(BCmaca,0) from #tblBaocaotam

		SELECT @BCNgayLe=COUNT(*) FROM HRM.dbo.tblNgayNghiLe WHERE CONVERT(VARCHAR,HRM.dbo.tblNgayNghiLe.NNLNgay,23)=CONVERT(VARCHAR,@D,23)
		SELECT @BCNgayLeNV=COUNT(*) FROM HRM.dbo.tblNgayNghiLeNhanVien WHERE HRM.dbo.tblNgayNghiLeNhanVien.NLNVNgay=@D AND HRM.dbo.tblNgayNghiLeNhanVien.NLNVMaNV=@StaffID
	
		-- Neu khong ton tai nhan vien trong bang bao cao thi khoi tao lai
		IF (@MaNV IS NULL)
			BEGIN
				SELECT @DepartmentID=NVMaBP, @PositionID= NVMaCV,@AllowOT=NVTinhLamThem,@Maca=ISNULL(NVMaCa,0) FROM HRM.dbo.tblNhanVien WHERE NVMa=@StaffID
				IF @DepartmentID IS NOT NULL 
					BEGIN 
						SET @SQL='INSERT INTO ' + @tblBaocao + ' (BCNgay,BCDay,BCMonth,BCYear,BCMaNV,BCMaBP,BCMaCV,BCMaCa,BCCuaDen,BCTGDen,BCCuaVe,BCTGVe,BCCuaRa,BCTGRa,BCCuaVao,BCTGVao,BCTGLamNgay,BCTGLamToi,BCTGQuaGioNgay,BCTGQuaGioToi,BCTGThemNgay,BCTGThemToi,BCTGRaNgoaiNgay,BCTGRaNgoaiToi,BCTGDiMuonNgay,BCTGDiMuonToi,BCTGVeSomNgay,BCTGVeSomToi,BCTGQuyDinh,BCGhiChu,BCLoai,BCLoaiLamThem,BCTinhLamThem,BCNgayLe,BCNgayLeNV) 
											Values (' + '''' + CONVERT(nvarchar(20),@D) + '''' + ',' + CONVERT(nvarchar(2),DAY(@D)) + ','+ CONVERT(nvarchar(2),MONTH(@D)) + ','+ CONVERT(nvarchar(4),YEAR(@D)) + ',' + CONVERT(nvarchar(20),@StaffID) + ',' + CONVERT(nvarchar(20),@DepartmentID) + ',' + CONVERT(nvarchar(20),@PositionID) + ',' + CONVERT(nvarchar(20),@Maca) + ',0,NULL,0,NULL,0,NULL,0,NULL,0,0,0,0,0,0,0,0,0,0,0,0,480,NULL,0,0,' + CONVERT(nvarchar(20),@AllowOT) + ',' + CONVERT(nvarchar(20),@BCNgayLe) + ',' + CONVERT(nvarchar(20),@BCNgayLeNV) + ')'
						EXEC(@SQL)
					END
					
			END
		ELSE
			BEGIN
				IF(@Maca=0)
					BEGIN
						SELECT @ShiftCode=ISNULL(NVMaCa,0) FROM HRM.dbo.tblNhanVien WHERE NVMa=@StaffID
						IF(@ShiftCode<>0)
							BEGIN
								SET @SQL='UPDATE ' + @tblBaocao + ' SET BCMaca=' + CONVERT(nvarchar(20),@ShiftCode) + ' WHERE (BCMaNV=' +  CONVERT(nvarchar(20),@StaffID) +') AND (BCNgay=' + '''' + CONVERT(nvarchar(20),@D) + '''' + ')'
								EXEC(@SQL)
							END
					END
			END
		DROP TABLE #tblBaocaotam
--------------------------------------------------------------------------------------------------------------------------------------------

		-- Kiem tra xem nhan vien co dang ky uu dai khong

		CREATE TABLE [dbo].#tblDangkyUudaiTam ([RowID] [int] IDENTITY(1, 1),[DKUDMaNV] [int],[DKUDLoaiUuDai] [int], [DKUDNgayApDung] [datetime],[DKUDNgayKetThuc] [datetime]) ON [PRIMARY]
		
		SET @SQL='Insert into #tblDangkyUudaiTam(DKUDMaNV,DKUDNgayApDung,DKUDNgayKetThuc,DKUDLoaiUuDai) SELECT DKUDMaNV,DKUDNgayApDung,DKUDNgayKetThuc,DKUDLoaiUuDai FROM '+ @tblDangkyuudai+ ' WHERE (DKudMaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKUDNgayApDung AND DKUDNgayKetThuc) '
		EXEC (@SQL)

		SELECT @NumberRecords = COUNT(*)  FROM #tblDangkyUudaiTam
		SET @RowCount = 1

		--create clustered index idx_tmp on #tblDangkyUudaiTam(RowID) WITH FILLFACTOR = 100
		
		WHILE @RowCount <= @NumberRecords
			BEGIN
				SELECT @UD1=LUDDauCa,@UD2=LUDTruocNghi,@UD3=LUDSauNghi,@UD4=LUDCuoiCa
				FROM #tblDangkyUudaiTam
				INNER JOIN HRM.dbo.tblLoaiUuDai ON  #tblDangkyUudaiTam.DKUDLoaiUuDai=HRM.dbo.tblLoaiUuDai.LUDMa
				WHERE RowID = @RowCount

				SET @TGUD1 =@TGUD1+@UD1
				SET @TGUD2=@TGUD2+@UD2
				SET @TGUD3 =@TGUD3+@UD3
				SET @TGUD4=@TGUD4+@UD4

				SET @RowCount = @RowCount + 1
			END
		DROP TABLE #tblDangkyUudaiTam
--------------------------------------------------------------------------------------------------------------------------------------------

		-- Kiem tra xem nhan vien co dang ky nghi theo ca khong

		CREATE TABLE [dbo].[#tblDangkynghiTam] ([RowID] [int] IDENTITY(1, 1),[DKNMaNV] [int],[DKNNgayApDung] [datetime],[DKNNgayKetThuc] [datetime],[DKNMaLyDo]  [varchar](10),[DKNLoai] [tinyint]) ON [PRIMARY]
		
		SET @SQL='Insert into #tblDangkynghiTam(DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai) SELECT DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai FROM '+ @tblDangkynghi+ ' WHERE (DKNMaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKNNgayApDung AND DKNNgayKetThuc)'
		EXEC (@SQL)

		SELECT @NumberRecords = COUNT(*)  FROM #tblDangkynghiTam
		SET @RowCount = 1

		--create clustered index idx_tmp on #tblDangkynghiTam(RowID) WITH FILLFACTOR = 100

		SET @SQL = 'UPDATE ' + @tblBaocao + ' SET BCNghiPhep=0, BCNghiH100=0, BCNghiH70=0, BCNghiKL=0, BCNghiBH100=0, BCNghiBH70=0, BCNghiCongTac=0, BCNghiBu=0, BCNghiKhac=0, BCLydonghi='', BCGhiChu='', BCTGNghi=0 ' 
					+ ' WHERE (BCMaNV=' + CONVERT(VARCHAR, @StaffID) + ') AND  (BCNgay=' + '''' + CONVERT(VARCHAR, @D, 121) + '''' + ')'
		EXEC (@SQL)
		
		WHILE @RowCount <= @NumberRecords
			BEGIN
				SELECT @TGNghi=(CASE WHEN DKNLoai=1 THEN 1 ELSE 0.5 END), @LoaiNghi=DKNLoai, @MaLyDoNghi=DKNMaLyDo, 
					   @NgayApDung=DKNNgayApDung,@NgayKetThuc=DKNNgayKetThuc
				FROM #tblDangkynghiTam
				WHERE RowID = @RowCount

				 --Update ngay nghi vao cac cot nghi
				 SELECT @SQL = (CASE WHEN HRM.dbo.tblLoaiNghi.LNLoai=1 THEN 'BCNghiPhep'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=2 THEN 'BCNghiH100'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=3 THEN 'BCNghiH70'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=4 THEN 'BCNghiKL'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=5 THEN 'BCNghiBH100'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=6 THEN 'BCNghiBH70'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=7 THEN 'BCNghiCongTac'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=8 THEN 'BCNghiBu'
									   WHEN HRM.dbo.tblLoaiNghi.LNLoai=9 THEN 'BCNghiKhac'
									   ELSE N'BCNghiKhac'
								END),
						@LydonghiViettat=HRM.dbo.tblLyDoNghi.LDNVietTat
				FROM HRM.dbo.tblLyDoNghi 
				INNER JOIN HRM.dbo.tblLoaiNghi ON  HRM.dbo.tblLyDoNghi.LDNLoai = HRM.dbo.tblLoaiNghi.LNMa 
				WHERE  HRM.dbo.tblLyDoNghi.LDNMa = @MaLyDoNghi

				IF @LydonghiViettat IS NOT NULL 
					BEGIN
						SET @Ghichu=CASE WHEN @LoaiNghi<>1 THEN CONVERT(nvarchar(30),@LydonghiViettat)+'/2' ELSE CONVERT(nvarchar(30),@LydonghiViettat) end
					END

				SET @SQL = 'UPDATE ' + @tblBaocao + ' SET ' + @SQL + '=' + CONVERT(VARCHAR, @TGNghi)
						 + ',BCLydonghi=' + '''' + @LydonghiViettat + '''' + ',BCGhiChu=' + '''' + @Ghichu + '''' 
						 + ',BCTGNghi=' + CONVERT(VARCHAR, @TGNghi)
						 + ' WHERE (BCMaNV=' + CONVERT(VARCHAR, @StaffID) + ') AND  (BCNgay=' + '''' + CONVERT(VARCHAR, @D, 121) + '''' + ')'
				EXEC (@SQL)

				SET @RowCount = @RowCount + 1
			END
		DROP TABLE #tblDangkynghiTam
--------------------------------------------------------------------------------------------------------------------------------------------

		-- Lay ma the
		SELECT @MaThe= CTMaThe FROM HRM.dbo.tblCapThe WHERE (CTMaNV=@StaffID) AND (@D  BETWEEN CTNgayApDung AND CTNgayKetThuc) ORDER BY CTNgayKetThuc DESC
		IF @MaThe IS NULL
			BEGIN
    			GOTO EndThisSub
			END

		IF @Maca = 0
			BEGIN
    			GOTO EndThisSub
			END
			
		IF @LoaiNghi = 1
			BEGIN
    			GOTO EndThisSub
			END 
--------------------------------------------------------------------------------------------------------------------------------------------

		-- Lay thong tin ca lam viec
		SELECT 
			@CVietTat=CVietTat,
			@TGBDCa=CTGBatDau,
			@TGBDNghi1=CTGBDNghi1,@TGKTNghi1=CTGKTNghi1,
			@TGBDNghi2=CTGBDNghi2,@TGKTNghi2=CTGKTNghi2,
			@TGBDNghi3=CTGBDNghi3,@TGKTNghi3=CTGKTNghi3,
			@TGKTCa=CTGKetThuc,
			@TGNghiGiuaGio=CTGNghiGiuaGio,
			@TGQDD=CTGQDD,@TGQDC=CTGQDC,
			@DuocRaNgoai=CDuocRaNgoai,
			@TinhVaoSom=CTinhVaoSom,
			@CongNghiGiuaCa=CCongNghiGiuaCa,
			@PhutBatDauLT=CBDTinhLT,
			@PhutBatDauLTTC=CBDTinhLTTC,
			@NguongLamThem=CNguongLamThem,
			@NguongLamThemTC=CNguongLamThemTC,
			@DonViLamThem=CDonViLamThem,
			@NguongDiMuon=CNguongDiMuon,@NguongVeSom=CNguongVeSom,
			@QuetTruocCa=CQuetTruocCa,@QuetSauCa=CQuetSauCa,
			@DonViChamCong=CDonViChamCong,@LoaiCa=CLoaiCa,@CNgaynghi=CNgaynghi,
			@CChiaLTSauca=CChiaLTSauca,@TGBDTinhDM=CTGTinhDimuon,@TGBDTinhVS=CTGTinhVeSom,
			@TypeCalculator=CLichtrinhVaora
		FROM HRM.dbo.tblCa 
		WHERE CMa=@Maca
		
		--Lay thong tin tu bang ca @D la datetime chuyen vao thu tuc
		set @TGBDCa=@D + @TGBDCa  -- VD: '2009-10-31 00:00:00' + '1990-01-01 06:00:00' = 2009-10-31 06:00:00
		set @TGBDNghi1=@TGBDCa + @TGBDNghi1
		set @TGKTNghi1=@TGBDCa + @TGKTNghi1
		set @TGBDNghi2=@TGBDCa + @TGBDNghi2
		set @TGKTNghi2=@TGBDCa + @TGKTNghi2
		set @TGBDNghi3=@TGBDCa + @TGBDNghi3
		set @TGKTNghi3=@TGBDCa + @TGKTNghi3
		set @TGKTCa=@TGBDCa + @TGKTCa

		SET @TGBDTinhDM=convert(varchar,@TGBDCa,23) + @TGBDTinhDM
		SET @TGBDTinhVS=convert(varchar,@TGKTCa,23) + @TGBDTinhVS

		SET @TGDM = DATEDIFF(MINUTE, @TGBDCa, @TGBDTinhDM)
		SET @TGVS = DATEDIFF(MINUTE, @TGKTCa, @TGBDTinhVS)
				
		set @TGLayDLDau= DateAdd(minute, -@QuetTruocCa, @TGBDCa)     -- Nguong nay de xac dinh lay gio vao hay gio ra
		set @TGLayDLCuoi=DateAdd(minute, @QuetSauCa, @TGKTCa)			-- Nguong nay de xac dinh lay gio vao hay gio ra 
		
		-- chinh lai moc thoi gian
		IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
			BEGIN
				SET @TGUuDaiN = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2))
							  + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)
				
				SET @TGUuDaiD = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
							  + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)
				
				SET @TGBDCa = DateAdd(minute, @TGUD3, @TGKTNghi2)  -- lay thoi gian bat dau nua ca sau
				SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)		-- lay thoi gian bat dau nua ca sau
				SET @TGBDNghi1 = @TGBDCa
				SET @TGKTNghi1 = @TGBDCa
				SET @TGBDNghi2 = @TGBDCa
				SET @TGKTnghi2 = @TGBDCa
				--SET @TGBatDauLTTC = DateAdd(minute, -@PhutBatDauLTTC, @TGBDCa)
				SET @TGBatDauLTTC = @TGBDCa
				SET @TGBatDauLT = DateAdd(minute, @PhutBatDauLT, @TGKTCa)
				--SET @TGBDTinhDM = DateAdd(minute, @TGDM, @TGBDCa)
				SET @TGBDTinhDM = @TGBDCa
				SET @TGBDTinhVS = DateAdd(minute, -@TGUD4, @TGBDTinhVS)
				SET @TGQD = @TGQDC - @TGUD3 - @TGUD4
			END
		ELSE If @LoaiNghi = 3 --DKNLoai = 3  DK nghi nua ca sau
    		BEGIN
				SET @TGUuDaiN = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa))
							  + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)
				
				SET @TGUuDaiD = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)

				SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa) 
				SET @TGKTCa = DateAdd(minute, -@TGUD2, @TGBDNghi2)
				SET @TGBDNghi2 = @TGKTCa
				SET @TGKTnghi2 = @TGKTCa
				SET @TGBDnghi3 = @TGKTCa
				SET @TGKTNghi3 = @TGKTCa
				SET @TGBatDauLTTC = DateAdd(minute, -@PhutBatDauLTTC, @TGBDCa)
				--SET @TGBatDauLT = DateAdd(minute, @PhutBatDauLT, @TGKTCa)
				SET @TGBatDauLT = @TGKTCa
				SET @TGBDTinhDM = DateAdd(minute, @TGUD1, @TGBDTinhDM)
				--SET @TGBDTinhVS = DateAdd(minute, @TGVS, @TGKTCa)
				SET @TGBDTinhVS = @TGKTCa
				SET @TGQD = @TGQDD - @TGUD1 - @TGUD2
    		END
		ELSE --DKNLoai = 1  DK nghi ca ca hoac khong nghi
			BEGIN
				SET @TGUuDaiN = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa))
							  + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)
							  + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2))
							  + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa) 
							  + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)
				

				SET @TGUuDaiD = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)
							  + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
						      + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)

				SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa)
				SET @TGBDNghi2 = DateAdd(minute, -@TGUD2, @TGBDNghi2)
				SET @TGKTNghi2 = DateAdd(minute, @TGUD3, @TGKTNghi2)
				SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)
				SET @TGBatDauLTTC = DateAdd(minute, -@PhutBatDauLTTC, @TGBDCa)
				SET @TGBatDauLT = DateAdd(minute, @PhutBatDauLT, @TGKTCa)
				SET @TGBDTinhDM=DateAdd(minute, @TGUD1, @TGBDTinhDM)
				SET @TGBDTinhVS=DateAdd(minute, -@TGUD4, @TGBDTinhVS)
				SET @TGQD = @TGQDD + @TGQDC - @TGUD1 - @TGUD2 - @TGUD3 - @TGUD4
			END
--------------------------------------------------------------------------------------------------------------------------------------------

		CREATE TABLE [dbo].[#tblLocdulieutam]
						([RowID] [int] IDENTITY(1, 1),[IDM] [tinyint],[IDCard] [nvarchar](14),
						[ThoiGian] [datetime],[Status] [bit],[HandData] [bit],[Pass] [bit])
		SET @sLocDuLieu=
						'INSERT INTO #tblLocdulieutam(IDM,IDCard,ThoiGian,Status,HandData,Pass) 
						SELECT DISTINCT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM ' + @RecordData +
						' RecordData LEFT JOIN HRM.dbo.tblDauDoc ON  RecordData.IDM = HRM.dbo.tblDauDoc.DDMa 
						WHERE (ISNULL(HRM.dbo.tblDauDoc.DDloaiChamCong,'''')='''') AND (ThoiGian BETWEEN '+ '''' + convert(varchar,@TGLayDLDau,121) 
						+ '''' + ' AND '+ '''' + convert(varchar,@TGLayDLCuoi,121) + ''''+ ')  
						and  IDCard=' + '''' + convert(varchar,@MaThe) + ''''+ '  ORDER BY ThoiGian'
		EXEC(@sLocDuLieu)

		SELECT @NumberRecords = COUNT(*)  FROM #tblLocdulieutam
		SET @RowCount = 1

		SELECT @recordCount = COUNT(*)  FROM #tblLocdulieutam

		IF @recordCount = 0
			BEGIN
    			GOTO EndThisSub
			END

	------------------------------------------them 10/04/2018 tao bao cao quet the nhieu lan-----------------------------------------------------

		WHILE @RowCount <= @NumberRecords
			BEGIN
				SELECT @IDM=IDM,@IDCard=IDCard,@ThoiGian=ThoiGian,@Status=Status,@HandData=HandData,@Pass=Pass
				FROM #tblLocdulieutam
				WHERE RowID = @RowCount

				SET @SQL='UPDATE '+@tblBaocao+' SET TG'+CONVERT(varchar(10),@RowCount)+'='+ '''' + CONVERT(nvarchar(30),@ThoiGian) + ''''+' WHERE (BCMaNV=' +  CONVERT(nvarchar(30),@StaffID) + ') AND (BCNgay='+ '''' + CONVERT(nvarchar(30),@D) + '''' + ')'
				EXEC(@SQL)
				SET @RowCount = @RowCount + 1
			END
	------------------------------------------------------------------------------------------------------------------------------------------------
		
		-- Khai bao RS DKN theo gio
		CREATE TABLE [dbo].#tblDangkynghitheogioTam ([RowID] [int] IDENTITY(1, 1),[Ngay] [datetime],[TGNghi] [datetime], [Sophut] [int],[MaLyDo] [varchar](10))  ON [PRIMARY]
		
		SET @SQL='Insert into #tblDangkynghitheogioTam(Ngay,TGNghi,Sophut,MaLyDo) SELECT Ngay,TGNghi,Sophut,MaLyDo FROM '+ @tblDangkynghitheogio+ ' WHERE (MaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + '=Ngay) '
		EXEC (@SQL)

		SELECT @NumberRecords = COUNT(*)  FROM #tblDangkynghitheogioTam
		SET @RowCount = 1

------------------------------------------------------------------------------------------------------------------------------------------------

		-- Loc du lieu
		IF (@OriginalStatus <> 0) -- Neu la trang thai dau doc
			BEGIN
				IF (@TypeCalculator = 2) -- Neu la quet dau tien va quet cuoi cung
					BEGIN
						SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM FROM #tblLocdulieutam INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa WHERE HRM.dbo.tblDauDoc.DDChinhVao=1 ORDER BY ThoiGian
						SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM FROM #tblLocdulieutam INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa WHERE HRM.dbo.tblDauDoc.DDChinhVao=0 ORDER BY ThoiGian DESC
					END
				ELSE -- Neu la tat ca cap vao ra
					BEGIN
						IF(@LoaiNghi=0)
							BEGIN
								IF(@NumberRecords>=1)
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												
												IF(@TGBDCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGBDCa_Loc = @DKNTGDengio
													END

												IF(@TGBDNghi2 BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGBDNCa_Loc = @DKNTGTugio
													END

												IF(@TGKTNghi2 BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGKTNCa_Loc = @DKNTGDengio
													END

												IF(@TGKTCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGKTCa_Loc = @DKNTGTugio
													END

												SET @RowCount = @RowCount + 1
											END
									END
								ELSE
									BEGIN
										SET @TGBDCa_Loc = @TGBDCa
										SET @TGBDNCa_Loc = @TGBDNghi2
										SET @TGKTNCa_Loc = @TGKTNghi2
										SET @TGKTCa_Loc = @TGKTCa
									END

								IF(@TGBDCa_Loc>@TGKTNghi2 OR @TGKTCa_Loc<@TGBDNghi2)
									BEGIN
										SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM 
										FROM #tblLocdulieutam 
										INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa 
										WHERE HRM.dbo.tblDauDoc.DDChinhVao=1 
												AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian

										SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM 
										FROM #tblLocdulieutam 
										INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa 
										WHERE HRM.dbo.tblDauDoc.DDChinhVao=0 
												AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian DESC
									END
								ELSE
									BEGIN
										SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM 
										FROM #tblLocdulieutam 
										INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa 
										WHERE HRM.dbo.tblDauDoc.DDChinhVao=1 
												AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian

										SELECT TOP 1 @TGRa=ThoiGian, @CuaRa=IDM 
										FROM #tblLocdulieutam 
										INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa 
										WHERE HRM.dbo.tblDauDoc.DDChinhVao=0 
												AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
												AND ThoiGian<=@TGKTNghi2
										ORDER BY ThoiGian DESC

										SELECT TOP 1 @TGVao=ThoiGian, @CuaVao=IDM 
										FROM #tblLocdulieutam 
										INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa 
										WHERE HRM.dbo.tblDauDoc.DDChinhVao=1 
												AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
												AND ThoiGian>=@TGBDNghi2
										ORDER BY ThoiGian

										SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM 
										FROM #tblLocdulieutam 
										INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa 
										WHERE HRM.dbo.tblDauDoc.DDChinhVao=0 
												AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
										ORDER BY ThoiGian DESC
									END
							END
						ELSE
							BEGIN
								SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM FROM #tblLocdulieutam INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa WHERE HRM.dbo.tblDauDoc.DDChinhVao=1 ORDER BY ThoiGian
								SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM FROM #tblLocdulieutam INNER JOIN HRM.dbo.tblDauDoc ON #tblLocdulieutam.IDM=HRM.dbo.tblDauDoc.DDMa WHERE HRM.dbo.tblDauDoc.DDChinhVao=0 ORDER BY ThoiGian DESC
							END
					END
			END
		ELSE -- Neu la trang thai xen ke
			BEGIN
				IF (@TypeCalculator = 2) -- Neu la quet dau tien va quet cuoi cung
					BEGIN
						IF(@recordCount=1)
							BEGIN
								IF(@NumberRecords>=1)
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												
												IF(@TGBDCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGBDCa_Loc = @DKNTGDengio
													END

												IF(@TGKTCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGKTCa_Loc = @DKNTGTugio
													END

												SET @RowCount = @RowCount + 1
											END
									END
								ELSE
									BEGIN
										SET @TGBDCa_Loc = @TGBDCa
										SET @TGKTCa_Loc = @TGKTCa
									END

								SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM 
								FROM #tblLocdulieutam 
								WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
								ORDER BY ThoiGian

								SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM 
								FROM #tblLocdulieutam 
								WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
								ORDER BY ThoiGian DESC
							END

						IF(@recordCount>1)
							BEGIN
								SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM FROM #tblLocdulieutam WHERE RowID IN (SELECT MIN(RowID) FROM #tblLocdulieutam)
								SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM FROM #tblLocdulieutam WHERE RowID IN (SELECT MAX(RowID) FROM #tblLocdulieutam)
							END
					END
				ELSE -- Neu la tat ca cap vao ra
					BEGIN
						IF(@LoaiNghi=0)
							BEGIN
								IF(@NumberRecords>=1)
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												
												IF(@TGBDCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGBDCa_Loc = @DKNTGDengio
													END

												IF(@TGBDNghi2 BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGBDNCa_Loc = @DKNTGTugio
													END

												IF(@TGKTNghi2 BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGKTNCa_Loc = @DKNTGDengio
													END

												IF(@TGKTCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGKTCa_Loc = @DKNTGTugio
													END

												SET @RowCount = @RowCount + 1
											END
									END
								ELSE
									BEGIN
										SET @TGBDCa_Loc = @TGBDCa
										SET @TGBDNCa_Loc = @TGBDNghi2
										SET @TGKTNCa_Loc = @TGKTNghi2
										SET @TGKTCa_Loc = @TGKTCa
									END

								IF(@TGBDCa_Loc>@TGKTNghi2 OR @TGKTCa_Loc<@TGBDNghi2)
									BEGIN
										SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM 
										FROM #tblLocdulieutam 
										WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian

										SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM 
										FROM #tblLocdulieutam 
										WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian DESC
									END
								ELSE
									BEGIN
										SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM 
										FROM #tblLocdulieutam 
										WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian

										SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM 
										FROM #tblLocdulieutam 
										WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
										ORDER BY ThoiGian DESC

										SELECT @Count = COUNT(*) FROM #tblLocdulieutam 
										WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc)
												AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc)
												AND ThoiGian<>@TGDen
												AND ThoiGian<>@TGVe

										IF(@Count=1)
											BEGIN
												SELECT TOP 1 @TGRa=ThoiGian, @CuaRa=IDM 
												FROM #tblLocdulieutam 
												WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
														AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
														AND ThoiGian<>@TGDen

												SELECT TOP 1 @TGVao=ThoiGian, @CuaVao=IDM 
												FROM #tblLocdulieutam 
												WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
														AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
														AND ThoiGian<>@TGVe
											END
										ELSE IF(@Count=2)
											BEGIN
												SELECT TOP 1 @TGRa=ThoiGian, @CuaRa=IDM 
												FROM #tblLocdulieutam 
												WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc)
														AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc)
														AND ThoiGian<>@TGDen
												ORDER BY ThoiGian

												SELECT TOP 1 @TGVao=ThoiGian, @CuaVao=IDM 
												FROM #tblLocdulieutam 
												WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc)
														AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc)
														AND ThoiGian<>@TGVe
												ORDER BY ThoiGian DESC
											END
										ELSE  IF(@Count>2)
											BEGIN
												SELECT @Count = COUNT(*) FROM #tblLocdulieutam 
												WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
														AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
														AND ThoiGian<>@TGDen

												IF(@Count>0)
													BEGIN
														SELECT @Count = COUNT(*) FROM #tblLocdulieutam 
														WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
																AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																AND ThoiGian<>@TGVe

														IF(@Count>0)
															BEGIN
																SELECT TOP 1 @TGRa=ThoiGian, @CuaRa=IDM 
																FROM #tblLocdulieutam 
																WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
																		AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																		AND ThoiGian<>@TGDen
																ORDER BY ThoiGian DESC

																SELECT TOP 1 @TGVao=ThoiGian, @CuaVao=IDM 
																FROM #tblLocdulieutam 
																WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
																		AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																		AND ThoiGian<>@TGVe
																ORDER BY ThoiGian
															END
														ELSE
															BEGIN
																SELECT @Count = COUNT(*) FROM #tblLocdulieutam 
																WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
																		AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																		AND ThoiGian<>@TGDen

																IF(@Count=1)
																	BEGIN
																		SELECT TOP 1 @TGRa=ThoiGian, @CuaRa=IDM 
																		FROM #tblLocdulieutam 
																		WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
																				AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																				AND ThoiGian<>@TGDen
																	END
																ELSE  IF(@Count>1)
																	BEGIN
																		SELECT TOP 1 @TGVao=ThoiGian, @CuaVao=IDM 
																		FROM #tblLocdulieutam 
																		WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
																				AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																				AND ThoiGian<>@TGDen
																		ORDER BY ThoiGian DESC

																		SELECT TOP 1 @TGRa=ThoiGian, @CuaRa=IDM 
																		FROM #tblLocdulieutam 
																		WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGBDNCa_Loc)/2.0),@TGBDCa_Loc) 
																				AND ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																				AND ThoiGian<>@TGDen
																				AND ThoiGian<>@TGVao
																		ORDER BY ThoiGian DESC
																	END
															END
													END
												ELSE
													BEGIN
														SELECT @Count = COUNT(*) FROM #tblLocdulieutam 
														WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
																AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																AND ThoiGian<>@TGVe

														IF(@Count=1)
															BEGIN
																SELECT TOP 1 @TGVao=ThoiGian, @CuaVao=IDM 
																FROM #tblLocdulieutam 
																WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
																		AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																		AND ThoiGian<>@TGVe
															END
														ELSE  IF(@Count>1)
															BEGIN
																SELECT TOP 1 @TGRa=ThoiGian, @CuaRa=IDM 
																FROM #tblLocdulieutam 
																WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
																		AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																		AND ThoiGian<>@TGVe
																ORDER BY ThoiGian

																SELECT TOP 1 @TGVao=ThoiGian, @CuaVao=IDM 
																FROM #tblLocdulieutam 
																WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGKTNCa_Loc,@TGKTCa_Loc)/2.0),@TGKTNCa_Loc) 
																		AND ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDNCa_Loc,@TGKTNCa_Loc)/2.0),@TGBDNCa_Loc)
																		AND ThoiGian<>@TGRa
																		AND ThoiGian<>@TGVe
																ORDER BY ThoiGian
															END
													END
											END
									END
							END
						ELSE
							BEGIN
								IF(@recordCount=1)
									BEGIN
										IF(@NumberRecords>=1)
											BEGIN
												WHILE @RowCount <= @NumberRecords
													BEGIN
														SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
														FROM #tblDangkynghitheogioTam 
														WHERE RowID = @RowCount

														SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
														SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												
														IF(@TGBDCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
															BEGIN
																SET @TGBDCa_Loc = @DKNTGDengio
															END

														IF(@TGKTCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
															BEGIN
																SET @TGKTCa_Loc = @DKNTGTugio
															END

														SET @RowCount = @RowCount + 1
													END
											END
										ELSE
											BEGIN
												SET @TGBDCa_Loc = @TGBDCa
												SET @TGKTCa_Loc = @TGKTCa
											END

										SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM 
										FROM #tblLocdulieutam 
										WHERE ThoiGian<=DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian

										SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM 
										FROM #tblLocdulieutam 
										WHERE ThoiGian>DATEADD(MINUTE,(DATEDIFF(MINUTE,@TGBDCa_Loc,@TGKTCa_Loc)/2.0),@TGBDCa_Loc) 
										ORDER BY ThoiGian DESC
									END

								IF(@recordCount>1)
									BEGIN
										SELECT TOP 1 @TGDen=ThoiGian, @Cuaden=IDM FROM #tblLocdulieutam WHERE RowID IN (SELECT MIN(RowID) FROM #tblLocdulieutam)
										SELECT TOP 1 @TGVe=ThoiGian, @CuaVe=IDM FROM #tblLocdulieutam WHERE RowID IN (SELECT MAX(RowID) FROM #tblLocdulieutam)
									END
							END
					END
			END

-------------------------------------------------Tinh thoi gian lam, di muon, ve som, lam them-----------------------------------------------------------

		SET @RowCount = 1

		IF (@TypeCalculator = 2)
			BEGIN
				-- Neu la thoi gian den
				IF (@TGDen <> '1900-01-01 00:00:00.000' AND @TGVe = '1900-01-01 00:00:00.000')
					BEGIN
						IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
							BEGIN
								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)

										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								IF(@TGDen<@TGBatDauLTTC)
									BEGIN
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
									END

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								IF(@TGMuonN+@TGMuonD<=@NguongDiMuon)
									BEGIN
										SET @TGMuonN=0
										SET @TGMuonD=0
									END
							END
						ELSE IF @LoaiNghi = 3 --DKNLoai = 3  DK nghi nua ca sau
    						BEGIN
								IF(@TGDen>@TGBatDauLTTC)
									BEGIN
										SET @TGOTTrongTC=@TGDen
									END
								ELSE
									BEGIN
										SET @TGOTTrongTC=@TGBatDauLTTC
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

								IF(@TGDen<@TGBatDauLTTC)
									BEGIN
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
																			+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa) 
																			+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
								SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC

								IF(@TGMuonN+@TGMuonD<=@NguongDiMuon)
									BEGIN
										SET @TGMuonN=0
										SET @TGMuonD=0
									END
							END
						ELSE -- khong nghi
							BEGIN
								IF(@TGDen>@TGBatDauLTTC)
									BEGIN
										SET @TGOTTrongTC=@TGDen
									END
								ELSE
									BEGIN
										SET @TGOTTrongTC=@TGBatDauLTTC
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								IF(@TGDen<@TGBatDauLTTC)
									BEGIN
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
																			+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa) 
																			+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
								SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC

								IF(@TGMuonN+@TGMuonD<=@NguongDiMuon)
									BEGIN
										SET @TGMuonN=0
										SET @TGMuonD=0
									END
							END
					END

				-- Neu la thoi gian ve
				IF (@TGDen = '1900-01-01 00:00:00.000' AND @TGVe <> '1900-01-01 00:00:00.000')
					BEGIN
						IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
							BEGIN
								IF(@TGVe<@TGBatDauLT)
									BEGIN
										SET @TGOTTrongSC=@TGVe
									END
								ELSE
									BEGIN
										SET @TGOTTrongSC=@TGBatDauLT
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

								IF(@TGVe>@TGBatDauLT)
									BEGIN
										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
								SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC

								IF(@TGSomN+@TGSomD<=@NguongVeSom)
									BEGIN
										SET @TGSomN=0
										SET @TGSomD=0
									END
							END
						ELSE IF @LoaiNghi = 3 --DKNLoai = 3  DK nghi nua ca sau
    						BEGIN
								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

								IF(@TGVe>@TGBatDauLT)
									BEGIN
										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
									END

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								IF(@TGSomN+@TGSomD<=@NguongVeSom)
									BEGIN
										SET @TGSomN=0
										SET @TGSomD=0
									END
							END
						ELSE -- khong nghi
							BEGIN
								IF(@TGVe<@TGBatDauLT)
									BEGIN
										SET @TGOTTrongSC=@TGVe
									END
								ELSE
									BEGIN
										SET @TGOTTrongSC=@TGBatDauLT
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

								IF(@TGVe>@TGBatDauLT)
									BEGIN
										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
								SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC

								IF(@TGSomN+@TGSomD<=@NguongVeSom)
									BEGIN
										SET @TGSomN=0
										SET @TGSomD=0
									END
							END
					END

				-- Neu la thoi gian den va ve
				IF (@TGDen <> '1900-01-01 00:00:00.000' AND @TGVe <> '1900-01-01 00:00:00.000')
					BEGIN
						IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
							BEGIN
								IF(@TGVe<@TGBatDauLT)
									BEGIN
										SET @TGOTTrongSC=@TGVe
									END
								ELSE
									BEGIN
										SET @TGOTTrongSC=@TGBatDauLT
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
										SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
										SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

								SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

								SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
								SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
								SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
								SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
								SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN+@DKNTGTrongCaNSC
								SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD+@DKNTGTrongCaDSC

								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
								SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC
							END
						ELSE IF @LoaiNghi = 3 --DKNLoai = 2  DK nghi nua ca sau
							BEGIN
								IF(@TGDen>@TGBatDauLTTC)
									BEGIN
										SET @TGOTTrongTC=@TGDen
									END
								ELSE
									BEGIN
										SET @TGOTTrongTC=@TGBatDauLTTC
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
										SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
										SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

								SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

								SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
								SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
								SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
								SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
																			+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
								SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa) 
																			+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN+@DKNTGTrongCaNTC
								SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD+@DKNTGTrongCaDTC

								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
								SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC
							END
						ELSE -- khong nghi
							BEGIN
								IF(@TGDen>@TGBatDauLTTC)
									BEGIN
										SET @TGOTTrongTC=@TGDen
									END
								ELSE
									BEGIN
										SET @TGOTTrongTC=@TGBatDauLTTC
									END

								IF(@TGVe<@TGBatDauLT)
									BEGIN
										SET @TGOTTrongSC=@TGVe
									END
								ELSE
									BEGIN
										SET @TGOTTrongSC=@TGBatDauLT
									END
										
								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
										SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
										SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

								SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

								SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
								SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
								SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
								SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
								SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)

								SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
																			+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
								SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa) 
																			+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN+@DKNTGTrongCaNTC+@DKNTGTrongCaNSC
								SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD+@DKNTGTrongCaDTC+@DKNTGTrongCaDSC

								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
								SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC

								SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
								SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC
							END
					END
			END
		ELSE
			BEGIN
				-- Neu la thoi gian den
				IF (
						@TGDen <> '1900-01-01 00:00:00.000' AND @TGRa = '1900-01-01 00:00:00.000'
						AND @TGVao = '1900-01-01 00:00:00.000' AND @TGVe = '1900-01-01 00:00:00.000'
					)
					BEGIN
						IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
							BEGIN
								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)

										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								IF(@TGDen<@TGBatDauLTTC)
									BEGIN
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
									END

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								IF(@TGMuonN+@TGMuonD<=@NguongDiMuon)
									BEGIN
										SET @TGMuonN=0
										SET @TGMuonD=0
									END
							END
						ELSE IF @LoaiNghi = 3 --DKNLoai = 3  DK nghi nua ca sau
    						BEGIN
								IF(@TGDen>@TGBatDauLTTC)
									BEGIN
										SET @TGOTTrongTC=@TGDen
									END
								ELSE
									BEGIN
										SET @TGOTTrongTC=@TGBatDauLTTC
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

								IF(@TGDen<@TGBatDauLTTC)
									BEGIN
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
																			+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa) 
																			+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
								SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC

								IF(@TGMuonN+@TGMuonD<=@NguongDiMuon)
									BEGIN
										SET @TGMuonN=0
										SET @TGMuonD=0
									END
							END
						ELSE -- khong nghi
							BEGIN
								IF(@TGDen>@TGBatDauLTTC)
									BEGIN
										SET @TGOTTrongTC=@TGDen
									END
								ELSE
									BEGIN
										SET @TGOTTrongTC=@TGBatDauLTTC
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

								IF(@TGDen<@TGBatDauLTTC)
									BEGIN
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
																			+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa) 
																			+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGBDCa, @TGBatDauLTTC, @TGBDCa)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
								SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC

								IF(@TGMuonN+@TGMuonD<=@NguongDiMuon)
									BEGIN
										SET @TGMuonN=0
										SET @TGMuonD=0
									END
							END
					END

				-- Neu la thoi gian ra
				ELSE IF (
							@TGDen = '1900-01-01 00:00:00.000' AND @TGRa <> '1900-01-01 00:00:00.000'
							AND @TGVao = '1900-01-01 00:00:00.000' AND @TGVe = '1900-01-01 00:00:00.000'
						)
					BEGIN
						WHILE @RowCount <= @NumberRecords
							BEGIN
								SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
								FROM #tblDangkynghitheogioTam 
								WHERE RowID = @RowCount

								SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
								SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
								-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
								SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2) 
													+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2)
								SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

								SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2) 
													+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2)
								SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

								SET @RowCount = @RowCount + 1
							END

						SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1) 
												+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1)
												+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
												+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2)

						SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1) 
												+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1)
												+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
												+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2)
					
						------------di muon, ve som, lam them cuoi cung-----------------
						SET @TGSomN=@TGSomN-@DKNTGVeSomN
						SET @TGSomD=@TGSomD-@DKNTGVeSomD

						IF(@TGSomN+@TGSomD<=@NguongVeSom)
							BEGIN
								SET @TGSomN=0
								SET @TGSomD=0
							END
					END

				-- Neu la thoi gian vao
				ELSE IF (
							@TGDen = '1900-01-01 00:00:00.000' AND @TGRa = '1900-01-01 00:00:00.000'
							AND @TGVao <> '1900-01-01 00:00:00.000' AND @TGVe = '1900-01-01 00:00:00.000'
						)
					BEGIN
						WHILE @RowCount <= @NumberRecords
							BEGIN
								SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
								FROM #tblDangkynghitheogioTam 
								WHERE RowID = @RowCount

								SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
								SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
								------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
								SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao) 
													+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao)
								SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

								SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao) 
													+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao)
								SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

								SET @RowCount = @RowCount + 1
							END

						SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
												+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
												+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa) 
												+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa)

						SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
												+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
												+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa) 
												+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa)

						------------di muon, ve som, lam them cuoi cung-----------------
						SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
						SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

						IF(@TGMuonN+@TGMuonD<=@NguongDiMuon)
							BEGIN
								SET @TGMuonN=0
								SET @TGMuonD=0
							END
					END

				-- Neu la thoi gian ve
				ELSE IF (
							@TGDen = '1900-01-01 00:00:00.000' AND @TGRa = '1900-01-01 00:00:00.000'
							AND @TGVao = '1900-01-01 00:00:00.000' AND @TGVe <> '1900-01-01 00:00:00.000'
						)
					BEGIN
						IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
							BEGIN
								IF(@TGVe<@TGBatDauLT)
									BEGIN
										SET @TGOTTrongSC=@TGVe
									END
								ELSE
									BEGIN
										SET @TGOTTrongSC=@TGBatDauLT
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

								IF(@TGVe>@TGBatDauLT)
									BEGIN
										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
								SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC

								IF(@TGSomN+@TGSomD<=@NguongVeSom)
									BEGIN
										SET @TGSomN=0
										SET @TGSomD=0
									END
							END
						ELSE IF @LoaiNghi = 3 --DKNLoai = 3  DK nghi nua ca sau
    						BEGIN
								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

								IF(@TGVe>@TGBatDauLT)
									BEGIN
										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
									END

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								IF(@TGSomN+@TGSomD<=@NguongVeSom)
									BEGIN
										SET @TGSomN=0
										SET @TGSomD=0
									END
							END
						ELSE -- khong nghi
							BEGIN
								IF(@TGVe<@TGBatDauLT)
									BEGIN
										SET @TGOTTrongSC=@TGVe
									END
								ELSE
									BEGIN
										SET @TGOTTrongSC=@TGBatDauLT
									END

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

								IF(@TGVe>@TGBatDauLT)
									BEGIN
										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
									END

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTCa, @TGVe, @TGKTCa, @TGBatDauLT)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
								SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC

								IF(@TGSomN+@TGSomD<=@NguongVeSom)
									BEGIN
										SET @TGSomN=0
										SET @TGSomD=0
									END
							END
					END

				-- Neu lon hon 2 du lieu (tat ca cap vao ra)
				ELSE
					BEGIN
						IF(@LoaiNghi=0)
							BEGIN
								IF(@NumberRecords>=1)
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												
												IF(@TGBDCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGBDCa_Loc = @DKNTGDengio
													END

												IF(@TGBDNghi2 BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGBDNCa_Loc = @DKNTGTugio
													END

												IF(@TGKTNghi2 BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGKTNCa_Loc = @DKNTGDengio
													END

												IF(@TGKTCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
													BEGIN
														SET @TGKTCa_Loc = @DKNTGTugio
													END

												SET @RowCount = @RowCount + 1
											END
									END
								ELSE
									BEGIN
										SET @TGBDCa_Loc = @TGBDTinhDM
										SET @TGBDNCa_Loc = @TGBDNghi2
										SET @TGKTNCa_Loc = @TGKTNghi2
										SET @TGKTCa_Loc = @TGBDTinhVS
									END

								-------------------------------------
								IF(@TGDen='1900-01-01 00:00:00.000')
									BEGIN
										IF(@TGRa='1900-01-01 00:00:00.000')
											BEGIN
												SET @TGDen_Temp=@TGBDNCa_Loc
											END
										ELSE IF(@TGRa<@TGBDNCa_Loc AND @TGRa<>'1900-01-01 00:00:00.000')
											BEGIN
												SET @TGDen_Temp=@TGRa
											END
										ELSE
											BEGIN
												SET @TGDen_Temp=@TGBDNCa_Loc
											END
									END
								ELSE
									BEGIN
										SET @TGDen_Temp=@TGDen
									END

								IF(@TGRa='1900-01-01 00:00:00.000')
									BEGIN
										IF(@TGDen='1900-01-01 00:00:00.000')
											BEGIN
												SET @TGRa_Temp=@TGBDNCa_Loc
											END
										ELSE IF(@TGDen>@TGBDCa_Loc)
											BEGIN
												SET @TGRa_Temp=@TGDen
											END
										ELSE
											BEGIN
												SET @TGRa_Temp=@TGBDCa_Loc
											END
									END
								ELSE
									BEGIN
										SET @TGRa_Temp=@TGRa
									END

								IF(@TGVao='1900-01-01 00:00:00.000')
									BEGIN
										IF(@TGVe='1900-01-01 00:00:00.000')
											BEGIN
												SET @TGVao_Temp=@TGKTNCa_Loc
											END
										ELSE IF(@TGVe<@TGKTCa_Loc AND @TGVe<>'1900-01-01 00:00:00.000')
											BEGIN
												SET @TGVao_Temp=@TGVe
											END
										ELSE
											BEGIN
												SET @TGVao_Temp=@TGKTCa_Loc
											END
									END
								ELSE
									BEGIN
										SET @TGVao_Temp=@TGVao
									END

								IF(@TGVe='1900-01-01 00:00:00.000')
									BEGIN
										IF(@TGVao='1900-01-01 00:00:00.000')
											BEGIN
												SET @TGVe_Temp=@TGKTNCa_Loc
											END
										ELSE IF(@TGVao>@TGKTNCa_Loc)
											BEGIN
												SET @TGVe_Temp=@TGVao
											END
										ELSE
											BEGIN
												SET @TGVe_Temp=@TGKTNCa_Loc
											END
									END
								ELSE
									BEGIN
										SET @TGVe_Temp=@TGVe
									END

								--------------------------------------

								IF(@TGDen>@TGBatDauLTTC)
									BEGIN
										SET @TGOTTrongTC=@TGDen
									END
								ELSE
									BEGIN
										SET @TGOTTrongTC=@TGBatDauLTTC
									END

								IF(@TGVe<@TGBatDauLT)
									BEGIN
										SET @TGOTTrongSC=@TGVe
									END
								ELSE
									BEGIN
										SET @TGOTTrongSC=@TGBatDauLT
									END

								----------------------------------------

								WHILE @RowCount <= @NumberRecords
									BEGIN
										SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
										FROM #tblDangkynghitheogioTam 
										WHERE RowID = @RowCount

										SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
										SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
										-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
										SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
										SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
										-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS)
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp)
															+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp)
															+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp) 
															+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
										SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

										-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

										SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
										SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END
								---------------------------------------------------------thoi gian den - ra-------------------------------------------
								SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa)

								SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa)

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS)
								---------------------------------------------------------------------------------------------------------------------------

								---------------------------------------------------------thoi gian vao - ve-------------------------------------------
								SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa)

								SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa)

								SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS)

								SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

								SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
														+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
														+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
								---------------------------------------------------------------------------------------------------------------------------
					

								SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGVe_Temp, @TGBatDauLT, @TGLayDLCuoi)
															+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGVe_Temp, @TGBatDauLT, @TGLayDLCuoi)
								SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGVe_Temp, @TGBatDauLT, @TGLayDLCuoi) 
	                
								SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC)
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC)
								SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC)

								---------------------------------------------------------ot trong ca------------------------------------------------------------------
								SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGVe_Temp, @TGKTCa, @TGBatDauLT)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGVe_Temp, @TGKTCa, @TGBatDauLT)
								SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGVe_Temp, @TGKTCa, @TGBatDauLT)

								SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen_Temp, @TGVe_Temp, @TGBatDauLTTC, @TGBDCa)
																			+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGVe_Temp, @TGBatDauLTTC, @TGBDCa)
								SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGVe_Temp, @TGBatDauLTTC, @TGBDCa) 
																			+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGVe_Temp, @TGBatDauLTTC, @TGBDCa)
								---------------------------------------------------------------------------------------------------------------------------------------

								------------di muon, ve som, lam them cuoi cung-----------------
								SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN+@DKNTGTrongCaNTC+@DKNTGTrongCaNSC
								SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD+@DKNTGTrongCaDTC+@DKNTGTrongCaDSC

								SET @TGSomN=@TGSomN-@DKNTGVeSomN
								SET @TGSomD=@TGSomD-@DKNTGVeSomD

								SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
								SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

								SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
								SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC

								SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
								SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC
							END
						ELSE
							BEGIN
								IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
									BEGIN
										IF(@TGVe<@TGBatDauLT)
											BEGIN
												SET @TGOTTrongSC=@TGVe
											END
										ELSE
											BEGIN
												SET @TGOTTrongSC=@TGBatDauLT
											END

										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
												SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
												SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3)
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

										---------------------------------------------------------ot trong ca------------------------------------------------------------------
										SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
																			+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
										SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
										---------------------------------------------------------------------------------------------------------------------------------------

										------------di muon, ve som, lam them cuoi cung-----------------
										SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN+@DKNTGTrongCaNSC
										SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD+@DKNTGTrongCaDSC

										SET @TGSomN=@TGSomN-@DKNTGVeSomN
										SET @TGSomD=@TGSomD-@DKNTGVeSomD

										SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
										SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

										SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
										SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC
									END
								ELSE IF @LoaiNghi = 3 --DKNLoai = 2  DK nghi nua ca sau
									BEGIN
										IF(@TGDen>@TGBatDauLTTC)
											BEGIN
												SET @TGOTTrongTC=@TGDen
											END
										ELSE
											BEGIN
												SET @TGOTTrongTC=@TGBatDauLTTC
											END

										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
												SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
												SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
												SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
												SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

										SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

										SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

										---------------------------------------------------------ot trong ca------------------------------------------------------------------
										SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
																					+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
										SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa) 
																					+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
										---------------------------------------------------------------------------------------------------------------------------------------

										------------di muon, ve som, lam them cuoi cung-----------------
										SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN+@DKNTGTrongCaNTC
										SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD+@DKNTGTrongCaDTC

										SET @TGSomN=@TGSomN-@DKNTGVeSomN
										SET @TGSomD=@TGSomD-@DKNTGVeSomD

										SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
										SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

										SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
										SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC
									END
								ELSE -- khong nghi
									BEGIN
										IF(@TGDen>@TGBatDauLTTC)
											BEGIN
												SET @TGOTTrongTC=@TGDen
											END
										ELSE
											BEGIN
												SET @TGOTTrongTC=@TGBatDauLTTC
											END

										IF(@TGVe<@TGBatDauLT)
											BEGIN
												SET @TGOTTrongSC=@TGVe
											END
										ELSE
											BEGIN
												SET @TGOTTrongSC=@TGBatDauLT
											END
										
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
												SET @DKNTGTrongCaNTC = @DKNTGTrongCaNTC + @DKNTGLamTam

												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGOTTrongTC, @TGBDCa)
												SET @DKNTGTrongCaDTC = @DKNTGTrongCaDTC + @DKNTGLamTam

												-----------------------------------------------------------Lam them trong ca giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC) 
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
												SET @DKNTGTrongCaNSC = @DKNTGTrongCaNSC + @DKNTGLamTam

												SET @DKNTGLamTam = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGOTTrongSC)
												SET @DKNTGTrongCaDSC = @DKNTGTrongCaDSC + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGMuonN = @TGMuonN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGSomN = @TGSomN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
										SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
										SET    @TGQuaNTC = @TGQuaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																		+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										SET    @TGQuaDTC = @TGQuaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																		+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										---------------------------------------------------------ot trong ca------------------------------------------------------------------
										SET @TGOTTrongCaN = @TGOTTrongCaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
																			+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)
										SET @TGOTTrongCaD = @TGOTTrongCaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTCa, @TGBatDauLT)

										SET    @TGOTTrongCaNTC = @TGOTTrongCaNTC + dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
																					+ dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
										SET    @TGOTTrongCaDTC = @TGOTTrongCaDTC + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa) 
																					+ dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLTTC, @TGBDCa)
										---------------------------------------------------------------------------------------------------------------------------------------

										------------di muon, ve som, lam them cuoi cung-----------------
										SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN+@DKNTGTrongCaNTC+@DKNTGTrongCaNSC
										SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD+@DKNTGTrongCaDTC+@DKNTGTrongCaDSC

										SET @TGSomN=@TGSomN-@DKNTGVeSomN
										SET @TGSomD=@TGSomD-@DKNTGVeSomD

										SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
										SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD

										SET @TGOTTrongCaNTC=@TGOTTrongCaNTC-@DKNTGTrongCaNTC
										SET @TGOTTrongCaDTC=@TGOTTrongCaDTC-@DKNTGTrongCaDTC

										SET @TGOTTrongCaN=@TGOTTrongCaN-@DKNTGTrongCaNSC
										SET @TGOTTrongCaD=@TGOTTrongCaD-@DKNTGTrongCaDSC
									END
							END
					END
			END

		DROP TABLE #tblDangkynghitheogioTam
		DROP TABLE #tblLocdulieutam
	----------------------------------------------cac dieu kien chon rieng--------------------------------------------------
		IF(ISNULL(@CongNghiGiuaCa,0)=1 AND (@TGLamN+@TGLamD)>0)
			BEGIN
				SET @TGNghiTruaN = dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDNghi2, @TGKTNghi2, @TGBDNghi2, @TGKTNghi2) 
									+ dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDNghi2, @TGKTNghi2, @TGBDNghi2, @TGKTNghi2)

				SET @TGNghiTruaD = dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDNghi2, @TGKTNghi2, @TGBDNghi2, @TGKTNghi2)

				SET @TGLamN = @TGLamN + @TGNghiTruaN
				SET @TGLamD = @TGLamD + @TGNghiTruaD
			END

		IF(@TGMuonN+@TGMuonD<=@NguongDiMuon AND (@TGLamN+@TGLamD)>0)
			BEGIN
				SET @TGLamN = @TGLamN + @TGMuonN
				SET @TGLamD = @TGLamD + @TGMuonD
				SET @TGMuonN=0
				SET @TGMuonD=0
			END

		IF(@TGSomN+@TGSomD<=@NguongVeSom AND (@TGLamN+@TGLamD)>0)
			BEGIN
				SET @TGLamN = @TGLamN + @TGSomN
				SET @TGLamD = @TGLamD + @TGSomD
				SET @TGSomN=0
				SET @TGSomD=0
			END

		IF(@TGQuaNTC+@TGQuaDTC<=@NguongLamThemTC)
			BEGIN
				SET @TGQuaNTC=0
				SET @TGQuaDTC=0
			END

		IF(@TGQuaN+@TGQuaD<=@NguongLamThem)
			BEGIN
				SET @TGQuaN=0
				SET @TGQuaD=0
			END
	--------------------------------------------------------------------------------------------------------------------------------------------
		
		SET @TGThemN = @TGQuaNTC + @TGQuaN
		SET @TGThemD = @TGQuaDTC + @TGQuaD

		--SET @TGLamN = Round(@TGLamN / @DonViChamCong,0) * @DonViChamCong
		--SET @TGLamD = Round(@TGLamD / @DonViChamCong,0) * @DonViChamCong
		SET @TGLamN = ceiling(Round(@TGLamN / CONVERT(FLOAT,@DonViChamCong),1)) * @DonViChamCong
		SET @TGLamD = ceiling(Round(@TGLamD / CONVERT(FLOAT,@DonViChamCong),1)) * @DonViChamCong

		--SET @TGThemN = Round(@TGThemN / @DonViLamThem,0) * @DonViLamThem
		--SET @TGThemD = Round(@TGThemD / @DonViLamThem,0) * @DonViLamThem
		SET @TGThemN = ceiling(Round(@TGThemN / CONVERT(FLOAT,@DonViLamThem),1)) * @DonViLamThem
		SET @TGThemD = ceiling(Round(@TGThemD / CONVERT(FLOAT,@DonViLamThem),1)) * @DonViLamThem

		--SET @OTTrongNTC = Round(@OTTrongNTC / @DonViLamThem,0) * @DonViLamThem
		--SET @OTTrongDTC = Round(@OTTrongDTC / @DonViLamThem,0) * @DonViLamThem
		SET @TGOTTrongCaNTC = ceiling(Round(@TGOTTrongCaNTC / CONVERT(FLOAT,@DonViLamThem),1)) * @DonViLamThem
		SET @TGOTTrongCaDTC = ceiling(Round(@TGOTTrongCaDTC / CONVERT(FLOAT,@DonViLamThem),1)) * @DonViLamThem

		--SET @OTTrongNSC = Round(@OTTrongNSC / @DonViLamThem,0) * @DonViLamThem
		--SET @OTTrongDSC = Round(@OTTrongDSC / @DonViLamThem,0) * @DonViLamThem
		SET @TGOTTrongCaN = ceiling(Round(@TGOTTrongCaN / CONVERT(FLOAT,@DonViLamThem),1)) * @DonViLamThem
		SET @TGOTTrongCaD = ceiling(Round(@TGOTTrongCaD / CONVERT(FLOAT,@DonViLamThem),1)) * @DonViLamThem
--------------------------------------------------------------------------------------------------------------------------------------------

		EndThisSub:

		-- Update vao CSDL
		UPDATE #tblBaoCao 
		SET BCTGDen=@TGDen
			,BCCuaDen=@Cuaden
			,BCTGVe=@TGVe
			,BCCuaVe=@CuaVe
			,BCTGRa=@TGRa
			,BCCuaRa=@CuaRa
			,BCTGVao=@TGVao
			,BCCuaVao=@CuaVao 
			,BCTGLamNgay=@TGLamN
			,BCTGLamToi=@TGLamD
			,OTTrongNTC=@TGOTTrongCaNTC
			,OTTrongDTC=@TGOTTrongCaDTC
			,OTTrongNSC=@TGOTTrongCaN
			,OTTrongDSC=@TGOTTrongCaD
			,BCTGQuaGioNgay=@TGQuaN
			,BCTGQuaGioToi=@TGQuaD
			,BCTGQuaGioNgayTC=@TGQuaNTC
			,BCTGQuaGioToiTC=@TGQuaDTC
			,BCTGThemNgay=@TGThemN
			,BCTGThemToi=@TGThemD
			,BCTGThemNgayTamTinh=@TGThemN
			,BCTGThemToiTamTinh=@TGThemD
			,BCTGDiMuonNgay=@TGMuonN
			,BCTGDiMuonToi=@TGMuonD
			,BCTGVeSomNgay=@TGSomN
			,BCTGVeSomToi=@TGSomD
			,BCTGQuyDinh=@TGQD
			,BCGhiChu= @Ghichu
			,BCTGNghi=@TGNghi
			,BCNgayLe=@BCNgayLe
			,BCNgayLeNV=@BCNgayLeNV
			,BCLydonghi=@LydonghiViettat
			,BCMaCa=@Maca
			,BCTGUuDaiN=@TGUuDaiN
			,BCTGUuDaiD=@TGUuDaiD
			--,BCDaXacNhanLamThem=1
		WHERE (BCMaNV=@StaffID  AND (BCNgay= @D))
        SELECT [BCNgay],[BCMaNV],[BCMaBP],[BCMaCV],[BCMaCa],[BCCuaDen],[BCTGDen],[BCCuaVe],[BCTGVe],[BCCuaRa],[BCTGRa],[BCCuaVao],[BCTGVao],[BCTGLamNgay],[BCTGLamToi],[BCTGQuaGioNgay],[BCTGQuaGioToi],[BCTGQuaGioNgayTC],[BCTGQuaGioToiTC],[BCTGThemNgay],[BCTGThemToi],[BCTGRaNgoaiNgay],[BCTGRaNgoaiToi],[BCTGDiMuonNgay],[BCTGDiMuonToi],[BCTGVeSomNgay],[BCTGVeSomToi],[BCTGQuyDinh],[BCGhiChu],[BCLoai],[BCLoaiLamThem],[BCTinhLamThem],[BCNghiBuChoNgay],[BCNghiPhep],[BCNghiH100],[BCNghiH70],[BCNghiKL],[BCNghiBH100],[BCNghiBH70],[BCNghiCongTac],[BCNghiBu],[BCNghiKhac],[BCLoaiNgayNghi],[BCLydonghi],[BCTGNghi],[BCTGDKNTheogio],[BCLoaiDKN],[BCDangKyLTN],[BCDangKyLTD],[BCTGUuDaiN],[BCTGUuDaiD],[BCNgayLe],[BCNgayLeNV],[BCDaXacNhanLamThem],[DLocked]
        FROM #tblBaoCao WHERE BCMaNV=@StaffID AND BCNgay=@D;

END

GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_TimeKeepingForStaff_K]    Script Date: 9/19/2026 11:05:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_TimeKeepingForStaff_K]    Script Date: 9/19/2026 11:05:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

GO
CREATE OR ALTER PROCEDURE dbo.usp_CalculateHrmAttendance
 @DeptCode nvarchar(20)=NULL,@FromDate date,@ToDate date,@TriggeredBy nvarchar(100)=NULL,@CalculationVersion nvarchar(50)=N'HRM-PORT-1.0'
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate>@ToDate THROW 51320,N'Khoảng ngày không hợp lệ.',1;
 DECLARE @BatchId uniqueidentifier=NEWID(),@HrmDeptId int=TRY_CONVERT(int,NULLIF(@DeptCode,N''));
 CREATE TABLE #Result(
  BCNgay datetime NOT NULL,BCMaNV int NOT NULL,BCMaBP int NOT NULL,BCMaCV int NOT NULL,BCMaCa int NOT NULL,
  BCCuaDen int NULL,BCTGDen datetime NULL,BCCuaVe int NULL,BCTGVe datetime NULL,BCCuaRa int NULL,BCTGRa datetime NULL,BCCuaVao int NULL,BCTGVao datetime NULL,
  BCTGLamNgay int NULL,BCTGLamToi int NULL,BCTGQuaGioNgay int NULL,BCTGQuaGioToi int NULL,BCTGQuaGioNgayTC int NULL,BCTGQuaGioToiTC int NULL,
  BCTGThemNgay int NULL,BCTGThemToi int NULL,BCTGRaNgoaiNgay int NULL,BCTGRaNgoaiToi int NULL,BCTGDiMuonNgay int NULL,BCTGDiMuonToi int NULL,
  BCTGVeSomNgay int NULL,BCTGVeSomToi int NULL,BCTGQuyDinh int NULL,BCGhiChu nvarchar(50) NULL,BCLoai bit NULL,BCLoaiLamThem bit NULL,BCTinhLamThem bit NULL,
  BCNghiBuChoNgay float NULL,BCNghiPhep float NULL,BCNghiH100 float NULL,BCNghiH70 float NULL,BCNghiKL float NULL,BCNghiBH100 float NULL,BCNghiBH70 float NULL,
  BCNghiCongTac float NULL,BCNghiBu float NULL,BCNghiKhac float NULL,BCLoaiNgayNghi smallint NULL,BCLydonghi nvarchar(20) NULL,BCTGNghi float NULL,
  BCTGDKNTheogio float NULL,BCLoaiDKN varchar(10) NULL,BCDangKyLTN float NULL,BCDangKyLTD float NULL,BCTGUuDaiN int NULL,BCTGUuDaiD int NULL,
  BCNgayLe int NULL,BCNgayLeNV int NULL,BCDaXacNhanLamThem bit NULL,DLocked bit NULL
 );
 DECLARE @D date=@FromDate;
 WHILE @D<=@ToDate
 BEGIN
  DECLARE @StaffID int;
  DECLARE staff_cur CURSOR LOCAL FAST_FORWARD FOR
   SELECT NVMa FROM HRM.dbo.tblNhanVien WHERE ISNULL(DLocked,0)=0 AND (@HrmDeptId IS NULL OR NVMaBP=@HrmDeptId);
  OPEN staff_cur; FETCH NEXT FROM staff_cur INTO @StaffID;
  WHILE @@FETCH_STATUS=0
  BEGIN
   DELETE FROM #Result;
   INSERT #Result EXEC dbo.usp_HrmCompatibleTimeKeepingForStaff @StaffID=@StaffID,@D=@D;
   INSERT dbo.F03HrmAttendanceCalculated(
    CalculationBatchId,CalculationVersion,WorkDate,HrmEmployeeId,EmployeeCode,FullName,HrmDeptId,DeptCode,HrmPositionId,ShiftId,ShiftAbbr,
    CheckInGate,CheckInTime,CheckOutGate,CheckOutTime,ExitGate,ExitTime,EntryGate,EntryTime,WorkMinutesDay,WorkMinutesNight,
    OTMinutesDay,OTMinutesNight,OTMinutesDayTC,OTMinutesNightTC,OTRecognizedMinutesDay,OTRecognizedMinutesNight,LateMinutesDay,LateMinutesNight,
    EarlyLeaveMinutesDay,EarlyLeaveMinutesNight,RequiredMinutes,LeaveTotal,LeaveAnnual,Leave100,Leave70,LeaveUnpaid,LeaveBH100,LeaveBH70,LeaveBusinessTrip,
    LeaveCompensatory,LeaveOther,LeaveTypeCode,LeaveReason,Note,HrmType,HrmHoliday,HrmEmployeeHoliday,IsLocked,
    HrmBCGhiChu,HrmBCLyDoNghi,HrmBCNghiTotal,HrmBCNghiPhep,HrmBCNghiH100,HrmBCNghiH70,HrmBCNghiKL,HrmBCNghiBH100,HrmBCNghiBH70,HrmBCNghiCongTac,HrmBCNghiBu,HrmBCNghiKhac,
    HrmBCDaXacNhanLamThem,HrmBCLoaiLamThem,HrmBCTinhLamThem,HrmBCNgayLe,HrmBCNgayLeNV,AttendanceDisplayValue,OtDisplayValue,CalculatedAt,CalculatedBy)
   SELECT @BatchId,@CalculationVersion,CAST(r.BCNgay AS date),r.BCMaNV,RTRIM(nv.NVMaNV),RTRIM(nv.NVHoTen),r.BCMaBP,CONVERT(nvarchar(20),r.BCMaBP),r.BCMaCV,r.BCMaCa,ca.CVietTat,
    r.BCCuaDen,r.BCTGDen,r.BCCuaVe,r.BCTGVe,r.BCCuaRa,r.BCTGRa,r.BCCuaVao,r.BCTGVao,ISNULL(r.BCTGLamNgay,0),ISNULL(r.BCTGLamToi,0),
    ISNULL(r.BCTGQuaGioNgay,0),ISNULL(r.BCTGQuaGioToi,0),ISNULL(r.BCTGQuaGioNgayTC,0),ISNULL(r.BCTGQuaGioToiTC,0),ISNULL(r.BCTGThemNgay,0),ISNULL(r.BCTGThemToi,0),
    ISNULL(r.BCTGDiMuonNgay,0),ISNULL(r.BCTGDiMuonToi,0),ISNULL(r.BCTGVeSomNgay,0),ISNULL(r.BCTGVeSomToi,0),ISNULL(r.BCTGQuyDinh,0),
    r.BCNghiPhep+r.BCNghiH100+r.BCNghiH70+r.BCNghiKL+r.BCNghiBH100+r.BCNghiBH70+r.BCNghiCongTac+r.BCNghiBu+r.BCNghiKhac,
    r.BCNghiPhep,r.BCNghiH100,r.BCNghiH70,r.BCNghiKL,r.BCNghiBH100,r.BCNghiBH70,r.BCNghiCongTac,r.BCNghiBu,r.BCNghiKhac,
    r.BCLydonghi,r.BCLydonghi,r.BCGhiChu,r.BCLoai,CASE WHEN ISNULL(r.BCNgayLe,0)<>0 THEN 1 ELSE 0 END,CASE WHEN ISNULL(r.BCNgayLeNV,0)<>0 THEN 1 ELSE 0 END,r.DLocked,
    r.BCGhiChu,r.BCLydonghi,ISNULL(r.BCTGNghi,0),r.BCNghiPhep,r.BCNghiH100,r.BCNghiH70,r.BCNghiKL,r.BCNghiBH100,r.BCNghiBH70,r.BCNghiCongTac,r.BCNghiBu,r.BCNghiKhac,
    r.BCDaXacNhanLamThem,r.BCLoaiLamThem,r.BCTinhLamThem,r.BCNgayLe,r.BCNgayLeNV,NULL,NULL,GETDATE(),@TriggeredBy
   FROM #Result r INNER JOIN HRM.dbo.tblNhanVien nv ON nv.NVMa=r.BCMaNV
   LEFT JOIN HRM.dbo.tblCa ca ON CONVERT(nvarchar(20),ca.CMa)=CONVERT(nvarchar(20),r.BCMaCa)
   WHERE r.BCMaNV=@StaffID AND CAST(r.BCNgay AS date)=@D;
   UPDATE a SET AttendanceDisplayValue=CASE
       WHEN ISNULL(a.LeaveTotal,0)>0 THEN COALESCE(NULLIF(LTRIM(RTRIM(a.HrmBCLyDoNghi)),N''),NULLIF(LTRIM(RTRIM(a.HrmBCGhiChu)),N''),N'K')
       WHEN a.CheckInTime IS NULL AND a.CheckOutTime IS NULL THEN N''
       WHEN a.CheckInTime IS NULL OR a.CheckOutTime IS NULL THEN N'?'
       WHEN ISNULL(a.WorkMinutesDay,0)+ISNULL(a.WorkMinutesNight,0)<=0 THEN N''
       WHEN ISNULL(a.RequiredMinutes,0)>0 AND ISNULL(a.WorkMinutesDay,0)+ISNULL(a.WorkMinutesNight,0)=a.RequiredMinutes THEN NULLIF(LTRIM(RTRIM(a.ShiftAbbr)),N'')
       ELSE CONVERT(nvarchar(50),CONVERT(float,(ISNULL(a.WorkMinutesDay,0)+ISNULL(a.WorkMinutesNight,0))/60.0)) END,
       OtDisplayValue=CASE
       WHEN ISNULL(a.HrmBCDaXacNhanLamThem,0)=0 THEN N''
       WHEN ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0)<=0 THEN N''
       WHEN ISNULL(a.HrmBCNgayLe,0)<>0 OR ISNULL(a.HrmBCNgayLeNV,0)<>0 THEN
            CASE WHEN ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0)>=480
                 THEN N'NL'+COALESCE(NULLIF(LTRIM(RTRIM(a.ShiftAbbr)),N''),N'')+CONVERT(nvarchar(20),CONVERT(float,(ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0))/60.0))
                 ELSE N'NL'+LEFT(COALESCE(NULLIF(LTRIM(RTRIM(a.ShiftAbbr)),N''),N'C'),1)+CONVERT(nvarchar(20),ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0)) END
       WHEN ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0)>=480 THEN N'CN'+CONVERT(nvarchar(20),CONVERT(float,(ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0))/60.0))
       ELSE CONVERT(nvarchar(20),CONVERT(float,(ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0))/60.0)) END
   FROM dbo.F03HrmAttendanceCalculated a WHERE a.CalculationBatchId=@BatchId AND a.WorkDate=@D AND a.HrmEmployeeId=@StaffID;
   FETCH NEXT FROM staff_cur INTO @StaffID;
  END
  CLOSE staff_cur; DEALLOCATE staff_cur; SET @D=DATEADD(day,1,@D);
 END
 INSERT dbo.F03HrmOTActual(CalculationBatchId,WorkDate,HrmEmployeeId,EmployeeCode,DeptCode,ActualStartTime,ActualEndTime,ActualMinutes,ActualOTDayMinutes,ActualOTNightMinutes,RecognizedOTMinutes,SourceAttendanceId)
 SELECT CalculationBatchId,WorkDate,HrmEmployeeId,EmployeeCode,DeptCode,CheckInTime,CheckOutTime,CASE WHEN CheckInTime IS NOT NULL AND CheckOutTime IS NOT NULL THEN DATEDIFF(minute,CheckInTime,CheckOutTime) ELSE 0 END,OTMinutesDay+OTMinutesDayTC,OTMinutesNight+OTMinutesNightTC,OTRecognizedMinutesDay+OTRecognizedMinutesNight,Id
 FROM dbo.F03HrmAttendanceCalculated WHERE CalculationBatchId=@BatchId AND (CheckInTime IS NOT NULL OR CheckOutTime IS NOT NULL);

 /* Centralized OT actual synchronization: HRM calculation owns this write. */
 ;WITH LatestActual AS
 (
  SELECT h.*, ROW_NUMBER() OVER(PARTITION BY h.HrmEmployeeId,h.WorkDate ORDER BY h.CalculatedAt DESC,h.Id DESC) rn
  FROM dbo.F03HrmOTActual h
  WHERE h.CalculationBatchId=@BatchId
 )
 UPDATE emp
 SET emp.ActualStartTime = a.ActualStartTime,
     emp.ActualEndTime = a.ActualEndTime,
     emp.ActualHours = CAST(ISNULL(a.RecognizedOTMinutes,0) / 60.0 AS decimal(5,2)),
     emp.ValidationStatus = CASE WHEN a.ActualStartTime IS NOT NULL AND a.ActualEndTime IS NOT NULL THEN 1 ELSE 0 END,
     emp.ValidationMessage = CASE WHEN a.ActualStartTime IS NOT NULL AND a.ActualEndTime IS NOT NULL THEN NULL ELSE N'Chưa đủ dữ liệu CheckIn/CheckOut từ HRM-compatible calculation.' END,
     emp.ModifiedAt=GETDATE(),
     emp.ModifiedBy=0
 FROM dbo.F03OTEmployees emp
 INNER JOIN dbo.F03OTRequests ot ON ot.Id=emp.OTRequestId AND ot.IsActive=1 AND ot.RequestStatus=3
 INNER JOIN HRM.dbo.tblNhanVien nv ON RTRIM(nv.NVMaNV)=emp.EmployeeCode
 INNER JOIN LatestActual a ON a.HrmEmployeeId=nv.NVMa AND a.WorkDate=CAST(ot.OTDate AS date) AND a.rn=1
 WHERE emp.IsActive=1 AND a.CalculationBatchId=@BatchId;
 SELECT @BatchId AS CalculationBatchId,@DeptCode AS DeptCode,@FromDate AS FromDate,@ToDate AS ToDate,COUNT(DISTINCT HrmEmployeeId) AS EmployeeCount,COUNT(*) AS CalculatedRows,MIN(CalculatedAt) AS StartedAt,MAX(CalculatedAt) AS FinishedAt,@CalculationVersion AS CalculationVersion
 FROM dbo.F03HrmAttendanceCalculated WHERE CalculationBatchId=@BatchId;
END;
GO

CREATE OR ALTER VIEW dbo.VF03HrmAttendanceDaily AS
SELECT a.* FROM dbo.F03HrmAttendanceCalculated a
INNER JOIN (
 SELECT WorkDate,EmployeeCode,MAX(Id) AS MaxId
 FROM dbo.F03HrmAttendanceCalculated
 GROUP BY WorkDate,EmployeeCode
) x ON x.MaxId=a.Id;

GO
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