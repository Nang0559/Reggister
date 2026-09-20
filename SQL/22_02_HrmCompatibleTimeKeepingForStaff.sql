CREATE OR ALTER PROCEDURE dbo.usp_HrmCompatibleTimeKeepingForStaff
	@StaffID int,	
	@D Datetime
 AS
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
		--SET @RecordData = 'HRM.dbo.RecordDataNew'
						   
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

		-- Lay ma the.
		-- RecordDataNew la nguon cham cong goc, vi vay khong duoc bo qua
		-- toan bo ngay chi vi metadata the hoac ca chua du.
		SELECT TOP 1 @MaThe=CTMaThe
		FROM HRM.dbo.tblCapThe
		WHERE CTMaNV=@StaffID
		  AND (@D BETWEEN CTNgayApDung AND CTNgayKetThuc)
		ORDER BY CTNgayKetThuc DESC, CTMaThe;

		-- Fallback: tim the co RecordDataNew thuc te trong cua so ngay.
		IF @MaThe IS NULL
		BEGIN
			SELECT TOP 1 @MaThe=R.IDCard
			FROM HRM.dbo.RecordDataNew R
			INNER JOIN HRM.dbo.tblCapThe C ON C.CTMaNV=@StaffID AND C.CTMaThe=R.IDCard
			WHERE R.ThoiGian >= DATEADD(HOUR,-6,CONVERT(datetime,CONVERT(date,@D)))
			  AND R.ThoiGian <  DATEADD(HOUR,30,CONVERT(datetime,CONVERT(date,@D)))
			ORDER BY R.ThoiGian DESC, R.IDCard;
		END

		-- Acquisition dau tien: doc IN/OUT truc tiep tu RecordDataNew.
		-- IN  = dau doc DDChinhVao=1, lan som nhat.
		-- OUT = dau doc DDChinhVao=0, lan muon nhat.
		CREATE TABLE #tblRecordDataRaw
		(
			IDM smallint NULL,
			IDCard nvarchar(20) NOT NULL,
			ThoiGian datetime NOT NULL,
			Status bit NULL,
			HandData bit NULL,
			Pass bit NULL,
			DDChinhVao bit NULL
		);

		INSERT INTO #tblRecordDataRaw(IDM,IDCard,ThoiGian,Status,HandData,Pass,DDChinhVao)
		SELECT DISTINCT
			R.IDM,R.IDCard,R.ThoiGian,R.Status,R.HandData,R.Pass,D.DDChinhVao
		FROM HRM.dbo.RecordDataNew R
		INNER JOIN HRM.dbo.tblCapThe C
			ON C.CTMaNV=@StaffID
		   AND C.CTMaThe=R.IDCard
		LEFT JOIN HRM.dbo.tblDauDoc D
			ON D.DDMa=R.IDM
		WHERE R.ThoiGian >= DATEADD(HOUR,-6,CONVERT(datetime,CONVERT(date,@D)))
		  AND R.ThoiGian <  DATEADD(HOUR,30,CONVERT(datetime,CONVERT(date,@D)))
		  AND ISNULL(D.DDloaiChamCong,N'')=N'';

		IF EXISTS (SELECT 1 FROM #tblRecordDataRaw)
		BEGIN
			SELECT TOP 1 @TGDen=ThoiGian,@Cuaden=ISNULL(IDM,0)
			FROM #tblRecordDataRaw
			WHERE DDChinhVao=1
			ORDER BY ThoiGian ASC, IDM ASC;

			SELECT TOP 1 @TGVe=ThoiGian,@CuaVe=ISNULL(IDM,0)
			FROM #tblRecordDataRaw
			WHERE DDChinhVao=0
			ORDER BY ThoiGian DESC, IDM DESC;


			/*
			  Exact shift resolution ported from HRM.dbo.sphrmvn_FindShift_New.
			  HRM does not choose the nearest shift globally. It first uses
			  NVLichTrinhCa/CC_LichTrinhCa for the weekday, then matches the
			  punches against only those scheduled shifts.
			*/
			IF @Maca=0 AND @TGDen > '19000101'
			BEGIN
				DECLARE @HrmScheduleCode varchar(50) = '';
				DECLARE @HrmFindShiftType nvarchar(20) = N'TTDD';
				DECLARE @HrmDaySchedule nvarchar(400) = N'';
				DECLARE @ScheduleColumn sysname;
				DECLARE @ShiftID1 int = 0;
				DECLARE @ShiftID2 int = 0;

				SELECT @HrmScheduleCode=ISNULL(BCLichTrinhCa,'')
				FROM #tblBaoCao
				WHERE BCNgay=@D AND BCMaNV=@StaffID;

				IF @HrmScheduleCode=''
				BEGIN
					SELECT @HrmScheduleCode=ISNULL(NV.NVLichTrinhCa,''),
					       @HrmFindShiftType=ISNULL(VR.Loai,N'TTDD')
					FROM HRM.dbo.tblNhanVien NV
					LEFT JOIN HRM.dbo.CC_LichTrinhVaoRa VR
						ON NV.NVLichTrinhVaoRa=VR.Ma
					WHERE NV.NVMa=@StaffID;
				END;

				IF ISNULL(@HrmFindShiftType,N'')=N''
					SET @HrmFindShiftType=N'TTDD';

				SET @ScheduleColumn=N'Ngay'+RIGHT(N'00'+CONVERT(varchar(2),DATEPART(weekday,@D)),2);
				SET @SQL=N'SELECT @OutSchedule=' + QUOTENAME(@ScheduleColumn)
					+ N' FROM HRM.dbo.CC_LichTrinhCa WHERE Ma=@ScheduleCode;';

				EXEC sys.sp_executesql
					@SQL,
					N'@ScheduleCode varchar(50), @OutSchedule nvarchar(400) OUTPUT',
					@ScheduleCode=@HrmScheduleCode,
					@OutSchedule=@HrmDaySchedule OUTPUT;

				IF ISNULL(@HrmDaySchedule,N'')<>''
				BEGIN
					CREATE TABLE #HrmShiftCandidates
					(
						CMa int NOT NULL,
						CVietTat nvarchar(10) NULL,
						CTGBatDau datetime NOT NULL,
						CTGKetThuc datetime NOT NULL,
						CQuetTruocCa int NULL,
						CQuetSauCa int NULL
					);

					INSERT INTO #HrmShiftCandidates
						(CMa,CVietTat,CTGBatDau,CTGKetThuc,CQuetTruocCa,CQuetSauCa)
					SELECT CMa,CVietTat,CTGBatDau,CTGKetThuc,CQuetTruocCa,CQuetSauCa
					FROM HRM.dbo.tblCa
					WHERE ISNULL(CMa,0)<>0
					  AND CHARINDEX(
							CONCAT(N',', ISNULL(CONVERT(nvarchar(20),CVietTat), N''), N','),
							CONCAT(N',', ISNULL(@HrmDaySchedule, N''), N',')
						  ) > 0;

					/*
					  For TTDD this is the exact HRM rule:
					  ShiftID2 = IN and OUT both fit.
					  ShiftID1 = either IN or OUT fits.
					*/
					IF ISNULL(@HrmFindShiftType,N'TTDD') = N'TTDD'
					BEGIN
						SELECT TOP (1) @ShiftID2=CMa
						FROM #HrmShiftCandidates
						CROSS APPLY (SELECT DATEADD(minute,DATEDIFF(minute,CONVERT(datetime,'19000101'),CTGBatDau),CONVERT(datetime,@D)) AS ShiftStart) s
						CROSS APPLY (SELECT DATEADD(minute,DATEDIFF(minute,CONVERT(datetime,'19000101'),CTGKetThuc),s.ShiftStart) AS ShiftEnd) e
						WHERE @TGDen BETWEEN DATEADD(minute,-ISNULL(CQuetTruocCa,0),s.ShiftStart)
						                  AND DATEADD(minute,60,s.ShiftStart)
						  AND @TGVe BETWEEN DATEADD(minute,-60,e.ShiftEnd)
						                 AND DATEADD(minute,ISNULL(CQuetSauCa,0),e.ShiftEnd)
						ORDER BY CTGBatDau DESC, CMa DESC;

						SELECT TOP (1) @ShiftID1=CMa
						FROM #HrmShiftCandidates
						CROSS APPLY (SELECT DATEADD(minute,DATEDIFF(minute,CONVERT(datetime,'19000101'),CTGBatDau),CONVERT(datetime,@D)) AS ShiftStart) s
						CROSS APPLY (SELECT DATEADD(minute,DATEDIFF(minute,CONVERT(datetime,'19000101'),CTGKetThuc),s.ShiftStart) AS ShiftEnd) e
						WHERE @TGDen BETWEEN DATEADD(minute,-ISNULL(CQuetTruocCa,0),s.ShiftStart)
						                  AND DATEADD(minute,60,s.ShiftStart)
						   OR @TGVe BETWEEN DATEADD(minute,-60,e.ShiftEnd)
						                 AND DATEADD(minute,ISNULL(CQuetSauCa,0),e.ShiftEnd)
						ORDER BY CTGBatDau DESC, CMa DESC;
					END
					ELSE
					BEGIN
						IF (SELECT COUNT(*) FROM #tblRecordDataRaw)>1
						BEGIN
							SELECT TOP (1) @ShiftID2=CMa
							FROM #HrmShiftCandidates
							CROSS APPLY (SELECT DATEADD(minute,DATEDIFF(minute,CONVERT(datetime,'19000101'),CTGBatDau),CONVERT(datetime,@D)) AS ShiftStart) s
							CROSS APPLY (SELECT DATEADD(minute,DATEDIFF(minute,CONVERT(datetime,'19000101'),CTGKetThuc),s.ShiftStart) AS ShiftEnd) e
							WHERE @TGDen BETWEEN DATEADD(minute,-ISNULL(CQuetTruocCa,0),s.ShiftStart)
							                  AND DATEADD(minute,60,s.ShiftStart)
							  AND @TGVe BETWEEN DATEADD(minute,-60,e.ShiftEnd)
							                 AND DATEADD(minute,ISNULL(CQuetSauCa,0),e.ShiftEnd)
							ORDER BY CTGBatDau DESC, CMa DESC;
						END
						ELSE IF (SELECT COUNT(*) FROM #tblRecordDataRaw)=1
						BEGIN
							SELECT TOP (1) @ShiftID1=CMa
							FROM #HrmShiftCandidates
							CROSS APPLY (SELECT DATEADD(minute,DATEDIFF(minute,CONVERT(datetime,'19000101'),CTGBatDau),CONVERT(datetime,@D)) AS ShiftStart) s
							WHERE @TGDen BETWEEN DATEADD(minute,-ISNULL(CQuetTruocCa,0),s.ShiftStart)
							                  AND DATEADD(minute,60,s.ShiftStart)
							  AND CONVERT(date,@TGDen)=CONVERT(date,@D)
							ORDER BY CTGBatDau DESC, CMa DESC;
						END
					END;
					DROP TABLE #HrmShiftCandidates;

					-- HRM sphrmvn_FindShift_New resolves the selected shift after
					-- scanning all scheduled candidates: ShiftID2 (both IN/OUT)
					-- takes precedence over ShiftID1 (either IN or OUT).
					IF @ShiftID2 > 0
						SET @Maca = @ShiftID2;
					ELSE IF @ShiftID1 > 0
						SET @Maca = @ShiftID1;
				END
			END
		END

		-- Neu chua co ca, chi ket thuc phan tinh Work/OT; IN/OUT raw van duoc
		-- ghi ra #tblBaoCao de co the kiem tra truc tiep RecordDataNew.
		IF @Maca = 0
			BEGIN
				DROP TABLE #tblRecordDataRaw;
    			GOTO EndThisSub
			END

		IF @MaThe IS NULL
			BEGIN
				DROP TABLE #tblRecordDataRaw;
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
				
		set @TGLayDLDau= DateAdd(minute, -@QuetTruocCa, @TGBDCa)     -- Nguong nay de xac dinh lay gio vao hay gio ra
		set @TGLayDLCuoi=DateAdd(minute, @QuetSauCa, @TGKTCa)			-- Nguong nay de xac dinh lay gio vao hay gio ra 
		
		-- chinh lai moc thoi gian
		IF @LoaiNghi = 2 --DKNLoai = 2  DK nghi nua ca dau
			BEGIN
				SET @TGUuDaiN = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2))
							  + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)
				
				SET @TGUuDaiD = HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
							  + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)
				
				SET @TGBDCa = DateAdd(minute, @TGUD3, @TGKTNghi2)  -- lay thoi gian bat dau nua ca sau
				SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)		-- lay thoi gian bat dau nua ca sau
				SET @TGBDNghi1 = @TGBDCa
				SET @TGKTNghi1 = @TGBDCa
				SET @TGBDNghi2 = @TGBDCa
				SET @TGKTnghi2 = @TGBDCa
				SET @TGBatDauLTTC = @TGBDCa
				SET @TGBatDauLT = @TGKTCa
				SET @TGBDTinhDM = @TGBDCa
				SET @TGBDTinhVS = @TGKTCa
				SET @TGQD = @TGQDC - @TGUD3 - @TGUD4
			END
		ELSE If @LoaiNghi = 3 --DKNLoai = 3  DK nghi nua ca sau
    		BEGIN
				SET @TGUuDaiN = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa))
							  + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)
				
				SET @TGUuDaiD = HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)

				SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa) 
				SET @TGKTCa = DateAdd(minute, -@TGUD2, @TGBDNghi2)
				SET @TGBDNghi2 = @TGKTCa
				SET @TGKTnghi2 = @TGKTCa
				SET @TGBDnghi3 = @TGKTCa
				SET @TGKTNghi3 = @TGKTCa
				SET @TGBatDauLTTC = @TGBDCa
				SET @TGBatDauLT = @TGKTCa
				SET @TGBDTinhDM = @TGBDCa
				SET @TGBDTinhVS = @TGKTCa
				SET @TGQD = @TGQDD - @TGUD1 - @TGUD2
    		END
		ELSE --DKNLoai = 1  DK nghi ca ca hoac khong nghi
			BEGIN
				SET @TGUuDaiN = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa))
							  + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)
							  + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2))
							  + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa) 
							  + HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)
				

				SET @TGUuDaiD = HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa), @TGBDCa, DateAdd(minute, @TGUD1, @TGBDCa)) 
							  + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2, DateAdd(minute, -@TGUD2, @TGBDNghi2), @TGBDNghi2)
							  + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2), @TGKTNghi2, DateAdd(minute, @TGUD3, @TGKTNghi2)) 
						      + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa, DateAdd(minute, -@TGUD4, @TGKTCa), @TGKTCa)

				SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa)
				SET @TGBDNghi2 = DateAdd(minute, -@TGUD2, @TGBDNghi2)
				SET @TGKTNghi2 = DateAdd(minute, @TGUD3, @TGKTNghi2)
				SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)
				SET @TGBatDauLTTC = @TGBDCa
				SET @TGBatDauLT = @TGKTCa
				SET @TGBDTinhDM = @TGBDCa
				SET @TGBDTinhVS = @TGKTCa
				SET @TGQD = @TGQDD + @TGQDC - @TGUD1 - @TGUD2 - @TGUD3 - @TGUD4
			END
--------------------------------------------------------------------------------------------------------------------------------------------

		-- Khai bao RS DKN theo gio
		CREATE TABLE [dbo].#tblDangkynghitheogioTam ([RowID] [int] IDENTITY(1, 1),[Ngay] [datetime],[TGNghi] [datetime], [Sophut] [int],[MaLyDo] [varchar](10))  ON [PRIMARY]
		
		SET @SQL='Insert into #tblDangkynghitheogioTam(Ngay,TGNghi,Sophut,MaLyDo) SELECT Ngay,TGNghi,Sophut,MaLyDo FROM '+ @tblDangkynghitheogio+ ' WHERE (MaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + '=Ngay) '
		EXEC (@SQL)

		SELECT @NumberRecords = COUNT(*) FROM #tblDangkynghitheogioTam
		SET @RowCount = 1

------------------------------------------------------------------------------------------------------------------------------------------------

		DECLARE	@TGDenT Datetime
		DECLARE	@TGVeT Datetime
		DECLARE	@TGBD smallint
		DECLARE	@TGKT smallint
		DECLARE @TGLTToiDa int 
		DECLARE @TGLTToiDaTC int
		DECLARE	@nRan smallint
		DECLARE @LamBu DATETIME
		DECLARE @NgayNghiLe Datetime
		DECLARE @NgayNghiLeNV Datetime
		
		SET @nRan=0
		SET @TGBD=0
		SET @TGKT=0
		SET	@NgayNghiLe ='1900-01-01 00:00:00.000'
		SET @NgayNghiLeNV ='1900-01-01 00:00:00.000'
		SET	@LamBu ='1900-01-01 00:00:00.000'

		SELECT @LamBu=NBNgayLamBu FROM HRM.dbo.tblDangKyNghiBu WHERE NBMaNV=@StaffID AND @D = NBNgayLamBu
		SELECT @NgayNghiLe=NNLNgay FROM HRM.dbo.tblNgayNghiLe WHERE CONVERT(VARCHAR,@D,103) = CONVERT(VARCHAR,NNLNgay,103)
		SELECT @NgayNghiLeNV=NLNVNgay FROM HRM.dbo.tblNgayNghiLeNhanVien WHERE NLNVMaNV=@StaffID and @D = NLNVNgay

		IF(
			(
				DATEPART(DW,@D) <> 1
				OR(DATEPART(DW,@D) = 1 AND @LamBu=@D)
			)
			AND (
					CONVERT(VARCHAR,@D,103) <> CONVERT(VARCHAR,@NgayNghiLe,103)
					OR(CONVERT(VARCHAR,@D,103) = CONVERT(VARCHAR,@NgayNghiLe,103) AND @LamBu = @D)
				)
			AND (
					@D <> @NgayNghiLeNV
					OR(@D = @NgayNghiLeNV AND @LamBu=@D)
				)
			AND @LoaiNghi <> 1
		  )
			BEGIN
				SELECT @TGDenT=BCTGDen, @TGVeT=BCTGVe, @Cuaden=BCCuaDen, @CuaVe=BCCuaVe 
				FROM #tblBaoCao WHERE BCMaNV=@StaffID AND BCNgay=@D

				SELECT @TGLTToiDa=ISNULL(HRM.dbo.tblBaoCaoK.BCTGLTToiDa,0),
				       @TGLTToiDaTC=ISNULL(HRM.dbo.tblBaoCaoK.BCTGLTToiDaTC,0)
				FROM HRM.dbo.tblBaoCaoK
				WHERE BCMaNV=@StaffID AND BCNgay=@D

				WHILE @RowCount <= @NumberRecords
					BEGIN
						SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
						FROM #tblDangkynghitheogioTam 
						WHERE RowID = @RowCount

						SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
						SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
				
						IF(@TGBDCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
							BEGIN
								SET @TGBD=@TGBD+(DATEDIFF(MINUTE,@TGBDCa,@DKNTGDengio))
							END

						IF(@TGKTCa BETWEEN @DKNTGTugio AND @DKNTGDengio)
							BEGIN
								SET @TGKT=@TGKT+(DATEDIFF(MINUTE,@DKNTGTugio,@TGKTCa))
							END

						SET @RowCount = @RowCount + 1
					END

				While(1=1)
					BEGIN
						Set @nRan = ROUND(RAND() * 10,0)
						IF (@nRan<>0) BREAK;
					END 

				IF(@TGBD>0)
					BEGIN
						SET @TGLTToiDaTC=0
					END
				IF(@TGKT>0)
					BEGIN
						SET @TGLTToiDa=0
					END

				IF(@TGDenT <> '1900-01-01 00:00:00.000')
					BEGIN
						IF(DATEDIFF(MINUTE,@TGDenT,(DATEADD(MINUTE,@TGBD-@TGLTToiDaTC,@TGBDCa)))>=15)
							BEGIN
								SET @TGDen=DATEADD(MINUTE,@TGBD-@nRan-@TGLTToiDaTC,@TGBDCa)
							END
						ELSE
							BEGIN
								SET @TGDen=@TGDenT
							END
					END

				IF(@TGVeT <> '1900-01-01 00:00:00.000')
					BEGIN
						IF(DATEDIFF(MINUTE,(DATEADD(MINUTE,@TGLTToiDa-@TGKT,@TGKTCa)),@TGVeT)>=15)
							BEGIN
								SET @TGVe=DATEADD(MINUTE,@nRan+@TGLTToiDa-@TGKT,@TGKTCa)
							END
						ELSE
							BEGIN
								SET @TGVe=@TGVeT
							END
					END

-------------------------------------------------Tinh thoi gian lam, di muon, ve som, lam them-----------------------------------------------------------

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
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										IF(@TGDen<@TGBatDauLTTC)
											BEGIN
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
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
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										IF(@TGDen<@TGBatDauLTTC)
											BEGIN
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
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
								ELSE -- khong nghi
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										IF(@TGDen<@TGBatDauLTTC)
											BEGIN
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
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
							END

						-- Neu la thoi gian ve
						IF (@TGDen = '1900-01-01 00:00:00.000' AND @TGVe <> '1900-01-01 00:00:00.000')
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
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										IF(@TGVe>@TGBatDauLT)
											BEGIN
												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
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
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

										IF(@TGVe>@TGBatDauLT)
											BEGIN
												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
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
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										IF(@TGVe>@TGBatDauLT)
											BEGIN
												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
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
							END

						-- Neu la thoi gian den va ve
						IF (@TGDen <> '1900-01-01 00:00:00.000' AND @TGVe <> '1900-01-01 00:00:00.000')
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
												-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
										SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
										SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

										
										------------di muon, ve som, lam them cuoi cung-----------------
										SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN
										SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD

										SET @TGSomN=@TGSomN-@DKNTGVeSomN
										SET @TGSomD=@TGSomD-@DKNTGVeSomD

										SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
										SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD
									END
								ELSE IF @LoaiNghi = 3 --DKNLoai = 2  DK nghi nua ca sau
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
												SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
												SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

										SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

										SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
										SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
										SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

										
										------------di muon, ve som, lam them cuoi cung-----------------
										SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN
										SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD

										SET @TGSomN=@TGSomN-@DKNTGVeSomN
										SET @TGSomD=@TGSomD-@DKNTGVeSomD

										SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
										SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD
									END
								ELSE -- khong nghi
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
										SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
										SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
										
										------------di muon, ve som, lam them cuoi cung-----------------
										SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN
										SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD

										SET @TGSomN=@TGSomN-@DKNTGVeSomN
										SET @TGSomD=@TGSomD-@DKNTGVeSomD

										SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
										SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD
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
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										IF(@TGDen<@TGBatDauLTTC)
											BEGIN
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
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
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

										IF(@TGDen<@TGBatDauLTTC)
											BEGIN
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
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
								ELSE -- khong nghi
									BEGIN
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

										IF(@TGDen<@TGBatDauLTTC)
											BEGIN
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)

												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGLayDLDau, @TGBatDauLTTC, @TGDen, @TGBatDauLTTC)
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
										SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2) 
															+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2)
										SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

										SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2) 
															+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGRa, @TGBDNghi2)
										SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1) 
														+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1)
														+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
														+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2)

								SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1) 
														+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa, @TGBDNghi2, @TGBDCa, @TGBDNghi1)
														+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
														+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2)
					
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
										SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao) 
															+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao)
										SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

										SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao) 
															+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao)
										SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

										SET @RowCount = @RowCount + 1
									END

								SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
														+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
														+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa) 
														+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa)

								SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
														+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao, @TGKTnghi2, @TGBDnghi3) 
														+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa) 
														+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao, @TGKTNghi3, @TGKTCa)

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
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										IF(@TGVe>@TGBatDauLT)
											BEGIN
												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
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
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

										IF(@TGVe>@TGBatDauLT)
											BEGIN
												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
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
										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

										IF(@TGVe>@TGBatDauLT)
											BEGIN
												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)

												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
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

										----------------------------------------

										WHILE @RowCount <= @NumberRecords
											BEGIN
												SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
												FROM #tblDangkynghitheogioTam 
												WHERE RowID = @RowCount

												SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
												SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
												-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

												SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
												SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
												-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2)
												SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe_Temp, @TGBDTinhVS)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGRa_Temp, @TGBDNghi2)
												SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

												------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp)
												SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

												SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen_Temp)
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp) 
																	+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi2, @TGVao_Temp)
												SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

												SET @RowCount = @RowCount + 1
											END
										---------------------------------------------------------thoi gian den - ra-------------------------------------------
										SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa)

										SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGRa_Temp, @TGKTNghi3, @TGKTCa)

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen_Temp, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRa_Temp, @TGBDNghi2, @TGKTNghi3, @TGBDTinhVS)
										---------------------------------------------------------------------------------------------------------------------------

										---------------------------------------------------------thoi gian vao - ve-------------------------------------------
										SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa)

										SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGBDCa, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVao_Temp, @TGVe_Temp, @TGKTNghi3, @TGKTCa)

										SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS)

										SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGKTnghi2, @TGVao_Temp, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

										SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGBDTinhDM, @TGBDNghi1) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe_Temp, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
										---------------------------------------------------------------------------------------------------------------------------
					

										SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGVe_Temp, @TGBatDauLT, @TGLayDLCuoi)
																	+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen_Temp, @TGVe_Temp, @TGBatDauLT, @TGLayDLCuoi)
										SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGVe_Temp, @TGBatDauLT, @TGLayDLCuoi) 
	                
										SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC)
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC)
										SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen_Temp, @TGVe_Temp, @TGLayDLDau, @TGBatDauLTTC)

										
										------------di muon, ve som, lam them cuoi cung-----------------
										SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN
										SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD

										SET @TGSomN=@TGSomN-@DKNTGVeSomN
										SET @TGSomD=@TGSomD-@DKNTGVeSomD

										SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
										SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD
									END
								ELSE
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
														-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
														SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
														SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

														SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDnghi3)
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
														SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
														-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
														SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
														SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

														------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
														SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
														SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

														SET @RowCount = @RowCount + 1
													END

												SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

												SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDnghi3)
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

												SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3)
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

												SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

												SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

												SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDnghi3)
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

												
												------------di muon, ve som, lam them cuoi cung-----------------
												SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN
												SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD

												SET @TGSomN=@TGSomN-@DKNTGVeSomN
												SET @TGSomD=@TGSomD-@DKNTGVeSomD

												SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
												SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD
											END
										ELSE IF @LoaiNghi = 3 --DKNLoai = 2  DK nghi nua ca sau
											BEGIN
												WHILE @RowCount <= @NumberRecords
													BEGIN
														SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
														FROM #tblDangkynghitheogioTam 
														WHERE RowID = @RowCount

														SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
														SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
														-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
														SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
														SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

														SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGKTCa)

																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
														SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
														-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
														SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
														SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

														------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
														SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
														SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

														SET @RowCount = @RowCount + 1
													END

												SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

												SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGKTCa)

												SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

												SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGKTCa)

												SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)

												SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDTinhVS)
					

												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)

												
												------------di muon, ve som, lam them cuoi cung-----------------
												SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN
												SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD

												SET @TGSomN=@TGSomN-@DKNTGVeSomN
												SET @TGSomD=@TGSomD-@DKNTGVeSomD

												SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
												SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD
											END
										ELSE -- khong nghi
											BEGIN
												WHILE @RowCount <= @NumberRecords
													BEGIN
														SELECT @DKNTGNgay=Ngay, @DKNTGGio=TGNghi, @DKNTGSophut=Sophut, @MaLyDoNghi=MaLyDo
														FROM #tblDangkynghitheogioTam 
														WHERE RowID = @RowCount

														SET @DKNTGTugio=@DKNTGNgay + @DKNTGGio
														SET @DKNTGDengio=DATEADD(minute,@DKNTGSophut,@DKNTGTugio)
														-----------------------------------------------------------TG lam giao dk nghi--------------------------------------------------------------
														SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
														SET @DKNTGLamN = @DKNTGLamN + @DKNTGLamTam

														SET @DKNTGLamTam = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDCa, @TGBDNghi1) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi1, @TGBDNghi2) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTnghi2, @TGBDnghi3) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTNghi3, @TGKTCa)

																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGBDCa)
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGKTCa, @TGBDTinhVS)
														SET @DKNTGLamD = @DKNTGLamD + @DKNTGLamTam
														-----------------------------------------------------------Ve som giao dk nghi--------------------------------------------------------------
														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
														SET @DKNTGVeSomN = @DKNTGVeSomN + @DKNTGLamTam

														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGVe, @TGBDTinhVS)
														SET @DKNTGVeSomD = @DKNTGVeSomD + @DKNTGLamTam

														------------------------------------------------------------Di muon giao dk nghi-------------------------------------------------------------
														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
														SET @DKNTGDiMuonN = @DKNTGDiMuonN + @DKNTGLamTam

														SET @DKNTGLamTam  = HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen) 
																			+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @DKNTGTugio, @DKNTGDengio, @TGBDTinhDM, @TGDen)
														SET @DKNTGDiMuonD = @DKNTGDiMuonD + @DKNTGLamTam

														SET @RowCount = @RowCount + 1
													END

												SET @TGLamN = @TGLamN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

												SET @TGLamD = @TGLamD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGKTNghi3, @TGKTCa)

												SET @TGMuonN = @TGMuonN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

												SET @TGMuonD = @TGMuonD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGBDTinhDM, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDTinhDM, @TGDen, @TGKTNghi3, @TGKTCa)

												SET @TGSomN = @TGSomN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)

												SET @TGSomD = @TGSomD + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGBDCa, @TGBDNghi1) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi1, @TGBDNghi2) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTnghi2, @TGBDnghi3) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS) 
																		+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGBDTinhVS, @TGKTNghi3, @TGBDTinhVS)
					

												SET    @TGQuaN = @TGQuaN + HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
																			+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi)
												SET    @TGQuaD = @TGQuaD + HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGBatDauLT, @TGLayDLCuoi) 
	                
												SET    @TGQuaNTC = @TGQuaNTC + HRM.dbo.InterSectionTime3(@ThresholdDay1, @ThresholdNight1, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
																				+ HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
												SET    @TGQuaDTC = @TGQuaDTC + HRM.dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC) 
																				+ HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDen, @TGVe, @TGLayDLDau, @TGBatDauLTTC)
												
												------------di muon, ve som, lam them cuoi cung-----------------
												SET @TGLamN=@TGLamN-@DKNTGLamN+@DKNTGVeSomN+@DKNTGDiMuonN
												SET @TGLamD=@TGLamD-@DKNTGLamD+@DKNTGVeSomD+@DKNTGDiMuonD

												SET @TGSomN=@TGSomN-@DKNTGVeSomN
												SET @TGSomD=@TGSomD-@DKNTGVeSomD

												SET @TGMuonN=@TGMuonN-@DKNTGDiMuonN
												SET @TGMuonD=@TGMuonD-@DKNTGDiMuonD
											END
									END
							END
					END

				DROP TABLE #tblDangkynghitheogioTam
			----------------------------------------------cac dieu kien chon rieng--------------------------------------------------
				IF(ISNULL(@CongNghiGiuaCa,0)=1 AND (@TGLamN+@TGLamD)>0)
					BEGIN
						SET @TGNghiTruaN = HRM.dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGBDNghi2, @TGKTNghi2, @TGBDNghi2, @TGKTNghi2) 
											+ HRM.dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGBDNghi2, @TGKTNghi2, @TGBDNghi2, @TGKTNghi2)

						SET @TGNghiTruaD = HRM.dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGBDNghi2, @TGKTNghi2, @TGBDNghi2, @TGKTNghi2)

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
			END
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
			,BCTGQuaGioNgay=@TGQuaN
			,BCTGQuaGioToi=@TGQuaD
			,BCTGQuaGioNgayTC=@TGQuaNTC
			,BCTGQuaGioToiTC=@TGQuaDTC
			,BCTGThemNgay=@TGThemN
			,BCTGThemToi=@TGThemD
			--,BCTGThemNgayTamTinh=@TGThemN
			--,BCTGThemToiTamTinh=@TGThemD
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

	-- Return only the HRM result columns consumed by the FVN orchestration.
	SELECT
		BCNgay,BCMaNV,BCMaBP,BCMaCV,BCMaCa,
		BCCuaDen,BCTGDen,BCCuaVe,BCTGVe,BCCuaRa,BCTGRa,BCCuaVao,BCTGVao,
		BCTGLamNgay,BCTGLamToi,BCTGQuaGioNgay,BCTGQuaGioToi,BCTGQuaGioNgayTC,BCTGQuaGioToiTC,
		BCTGThemNgay,BCTGThemToi,BCTGRaNgoaiNgay,BCTGRaNgoaiToi,BCTGDiMuonNgay,BCTGDiMuonToi,
		BCTGVeSomNgay,BCTGVeSomToi,BCTGQuyDinh,BCGhiChu,BCLoai,BCLoaiLamThem,BCTinhLamThem,
		BCNghiBuChoNgay,BCNghiPhep,BCNghiH100,BCNghiH70,BCNghiKL,BCNghiBH100,BCNghiBH70,
		BCNghiCongTac,BCNghiBu,BCNghiKhac,BCLoaiNgayNghi,BCLydonghi,BCTGNghi,
		BCTGDKNTheogio,BCLoaiDKN,BCDangKyLTN,BCDangKyLTD,BCTGUuDaiN,BCTGUuDaiD,
		BCNgayLe,BCNgayLeNV,BCDaXacNhanLamThem,DLocked
	FROM #tblBaoCao
	WHERE BCMaNV=@StaffID AND BCNgay=@D;

