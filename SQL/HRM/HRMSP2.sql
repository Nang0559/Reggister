USE [HRM]
GO
/****** Object:  StoredProcedure [dbo].[cp_QuetTheLoi_Children]    Script Date: 9/19/2026 10:48:21 AM ******/
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
/****** Object:  StoredProcedure [dbo].[SP_CreateRecordLNV]    Script Date: 9/19/2026 10:48:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/************************************************************
*  Routine       :	SP_CreateRecordLNV
*  Created by    :  Duong Van Quyet , at 22/01/2011 - 00:29:21  
*  Machine       :  DIGISOFT
*  Description   :  Tao nhan vien trong bang tblBaocao
*  Parameters    :  @staffID int, @D datetime
************************************************************/
CREATE PROCEDURE [dbo].[SP_CreateRecordLNV]
(@staffID Int,
@D datetime)
AS
	--DECLARE @staffID Int
	--DECLARE @D datetime

	DECLARE @sql1 varchar(8000)
	DECLARE @sql2 varchar(8000)
	DECLARE @DepartmentID INT
	DECLARE @PositionID INT
	DECLARE @AllowOT INT
	DECLARE @ShiftCode int
	
	SET @ShiftCode=0
	
	IF NOT  EXISTS (SELECT tblBaoCao.BCMaNV
	                  FROM tblBaoCao WHERE (BCMaNV=@StaffID AND (BCNgay= CONVERT(nvarchar,@D,101))))
	BEGIN
		SELECT @DepartmentID=NVMaBP, @PositionID= NVMaCV,@AllowOT=NVTinhLamThem FROM dbo.tblNhanVien WHERE NVMa=@StaffID
		INSERT INTO tblBaoCao(BCNgay,BCMaNV,BCMaBP,BCMaCV,BCMaCa,BCCuaDen,BCTGDen,BCCuaVe,BCTGVe,BCCuaRa,BCTGRa,BCCuaVao,BCTGVao,BCTGLamNgay,BCTGLamToi,BCTGQuaGioNgay,BCTGQuaGioToi,BCTGThemNgay,BCTGThemToi,BCTGRaNgoaiNgay,BCTGRaNgoaiToi,BCTGDiMuonNgay,BCTGDiMuonToi,BCTGVeSomNgay,BCTGVeSomToi,BCTGQuyDinh,BCGhiChu,BCLoai,BCLoaiLamThem,BCTinhLamThem) Values ( CONVERT(nvarchar(20),@D) ,  CONVERT(nvarchar(9),@StaffID) , @DepartmentID , @PositionID ,@ShiftCode ,0,NULL,0,NULL,0,NULL,0,NULL,0,0,0,0,0,0,0,0,0,0,0,0,480,NULL,0,0, @AllowOT ) 
	END
	
--SET @sql1='
--	DECLARE @MaNV int
--	DECLARE @ShiftCode int
--	DECLARE @DepartmentID int
--	DECLARE @PositionID int
--	DECLARE @AllowOT int
--	'
	
--	--SET @vtblBaocao = case when LEN(Month(@D))=2 then 
--	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
--	-- else
--	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
--	-- end 
--SET @vtblBaocao ='tblBaoCao'
--SET @sql1= @sql1 + 'DECLARE @tblBaocaotam TABLE(BCManv [int])
--			 Insert  @tblBaocaotam(BCManv)  Select BCManv From  ' + @vtblBaocao + ' WHERE (BCMaNV=' + CONVERT(nvarchar(9),@StaffID) + ') AND (BCNgay=' + '''' + CONVERT(nvarchar,@D,101) + '''' + ')'
--SET @sql1=@sql1 + '	select @MaNV = BCManv from @tblBaocaotam
	
--	IF @MaNV IS NULL
--		BEGIN
--			SET @ShiftCode=DBO.FindShiftOfStaff(' +  CONVERT(nvarchar(9),@StaffID) + ',' + '''' + CONVERT(nvarchar(20),@D) + '''' + ')
--			SELECT @DepartmentID=NVMaBP, @PositionID= NVMaCV,@AllowOT=NVTinhLamThem FROM dbo.tblNhanVien WHERE NVMa=' + CONVERT(nvarchar(9),@StaffID)+ '
--			IF @DepartmentID IS not NULL 
--			BEGIN 
--				INSERT INTO ' + @vtblBaocao +'(BCNgay,BCDay,BCMonth,BCYear,BCMaNV,BCMaBP,BCMaCV,BCMaCa,BCCuaDen,BCTGDen,BCCuaVe,BCTGVe,BCCuaRa,BCTGRa,BCCuaVao,BCTGVao,BCTGLamNgay,BCTGLamToi,BCTGQuaGioNgay,BCTGQuaGioToi,BCTGThemNgay,BCTGThemToi,BCTGRaNgoaiNgay,BCTGRaNgoaiToi,BCTGDiMuonNgay,BCTGDiMuonToi,BCTGVeSomNgay,BCTGVeSomToi,BCTGQuyDinh,BCGhiChu,BCLoai,BCLoaiLamThem,BCTinhLamThem) Values (' + '''' + CONVERT(nvarchar(20),@D) + '''' + ',' + CONVERT(nvarchar(2),DAY(@D)) + ','+ CONVERT(nvarchar(2),MONTH(@D)) + ','+ CONVERT(nvarchar(4),YEAR(@D)) + ',' + CONVERT(nvarchar(9),@StaffID) + ', @DepartmentID , @PositionID ,@ShiftCode ,0,NULL,0,NULL,0,NULL,0,NULL,0,0,0,0,0,0,0,0,0,0,0,0,480,NULL,0,0, @AllowOT ) 
--			end
			
--		END
--	ELSE
--		BEGIN
--				SET @ShiftCode=DBO.FindShiftOfStaff('+ CONVERT(nvarchar(9),@StaffID) +',' + '''' +  CONVERT(nvarchar,@D,101) + '''' +')
--				Update ' + @vtblBaocao + ' set BCMaca=@ShiftCode WHERE (BCMaNV=' +  CONVERT(nvarchar(9),@StaffID) +') AND (BCNgay=' + '''' + CONVERT(nvarchar(20),@D) + '''' + ') AND (BCMaCa<=0)
--		END'
	
--EXEC( @sql1)
GO
/****** Object:  StoredProcedure [dbo].[SP_CreateRecordLNV_K]    Script Date: 9/19/2026 10:48:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/************************************************************
*  Routine       :	SP_CreateRecordLNV
*  Created by    :  Duong Van Quyet , at 22/01/2011 - 00:29:21  
*  Machine       :  DIGISOFT
*  Description   :  Tao nhan vien trong bang tblBaocao
*  Parameters    :  @staffID int, @D datetime
************************************************************/
CREATE PROCEDURE [dbo].[SP_CreateRecordLNV_K]
(@staffID Int,
@D datetime)
AS
	--DECLARE @staffID Int
	--DECLARE @D datetime

	DECLARE @sql1 varchar(8000)
	DECLARE @sql2 varchar(8000)
	DECLARE @vtblBaocao nvarchar(20)
	
	--SET @staffID=1
	--SET @D='2000-04-02'
	
SET @sql1='
	DECLARE @MaNV int
	DECLARE @ShiftCode int
	DECLARE @DepartmentID int
	DECLARE @PositionID int
	DECLARE @AllowOT int
	'
	
	--SET @vtblBaocao = case when LEN(Month(@D))=2 then 
	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_' +  CONVERT(nvarchar,Month(@D))
	-- else
	-- 'tblBaoCao' + CONVERT(nvarchar,YEAR(@D)) + '_0' +  CONVERT(nvarchar,Month(@D))
	-- end 
SET @vtblBaocao ='tblBaoCaoK'
SET @sql1= @sql1 + 'DECLARE @tblBaocaotam TABLE(BCManv [int])
			 Insert  @tblBaocaotam(BCManv)  Select BCManv From  ' + @vtblBaocao + ' WHERE (BCMaNV=' + CONVERT(nvarchar(9),@StaffID) + ') AND (BCNgay=' + '''' + CONVERT(nvarchar,@D,101) + '''' + ')'
SET @sql1=@sql1 + '	select @MaNV = BCManv from @tblBaocaotam
	
	IF @MaNV IS NULL
		BEGIN
			SET @ShiftCode=DBO.FindShiftOfStaff(' +  CONVERT(nvarchar(9),@StaffID) + ',' + '''' + CONVERT(nvarchar(20),@D) + '''' + ')
			SELECT @DepartmentID=NVMaBP, @PositionID= NVMaCV,@AllowOT=NVTinhLamThem FROM dbo.tblNhanVien WHERE NVMa=' + CONVERT(nvarchar(9),@StaffID)+ '
			IF @DepartmentID IS not NULL 
			BEGIN 
				INSERT INTO ' + @vtblBaocao +'(BCNgay,BCDay,BCMonth,BCYear,BCMaNV,BCMaBP,BCMaCV,BCMaCa,BCCuaDen,BCTGDen,BCCuaVe,BCTGVe,BCCuaRa,BCTGRa,BCCuaVao,BCTGVao,BCTGLamNgay,BCTGLamToi,BCTGQuaGioNgay,BCTGQuaGioToi,BCTGThemNgay,BCTGThemToi,BCTGRaNgoaiNgay,BCTGRaNgoaiToi,BCTGDiMuonNgay,BCTGDiMuonToi,BCTGVeSomNgay,BCTGVeSomToi,BCTGQuyDinh,BCGhiChu,BCLoai,BCLoaiLamThem,BCTinhLamThem) Values (' + '''' + CONVERT(nvarchar(20),@D) + '''' + ',' + CONVERT(nvarchar(2),DAY(@D)) + ','+ CONVERT(nvarchar(2),MONTH(@D)) + ','+ CONVERT(nvarchar(4),YEAR(@D)) + ',' + CONVERT(nvarchar(9),@StaffID) + ', @DepartmentID , @PositionID ,@ShiftCode ,0,NULL,0,NULL,0,NULL,0,NULL,0,0,0,0,0,0,0,0,0,0,0,0,480,NULL,0,0, @AllowOT ) 
			end
			
		END
	ELSE
		BEGIN
				SET @ShiftCode=DBO.FindShiftOfStaff('+ CONVERT(nvarchar(9),@StaffID) +',' + '''' +  CONVERT(nvarchar,@D,101) + '''' +')
				Update ' + @vtblBaocao + ' set BCMaca=@ShiftCode WHERE (BCMaNV=' +  CONVERT(nvarchar(9),@StaffID) +') AND (BCNgay=' + '''' + CONVERT(nvarchar(20),@D) + '''' + ') AND (BCMaCa<=0)
		END'
	
EXEC( @sql1)
GO
/****** Object:  StoredProcedure [dbo].[sphrmvn_FindShift_New]    Script Date: 9/19/2026 10:48:21 AM ******/
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
/****** Object:  StoredProcedure [dbo].[sphrmvn_FindShiftForPart_InOut]    Script Date: 9/19/2026 10:48:21 AM ******/
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
/****** Object:  StoredProcedure [dbo].[sphrmvn_FindShiftForPart_New]    Script Date: 9/19/2026 10:48:21 AM ******/
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
