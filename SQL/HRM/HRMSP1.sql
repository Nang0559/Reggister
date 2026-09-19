USE [HRM]
GO
/****** Object:  StoredProcedure [dbo].[cp_Taobangchotungnguoi]    Script Date: 9/19/2026 10:35:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[cp_Taobangchotungnguoi]
@MaNV INT ,
@Nam INT,
@Thang INT
AS
DECLARE @Sql1 VARCHAR(4000)
DECLARE @Sql2 VARCHAR(4000)
SET @Sql1 = '
UPDATE tblBangChamCong'+ CONVERT(VARCHAR(5),@Nam) +'
SET BCCNgay01 = ISNULL(T1.TBCCNgay01,0),
BCCNgay02 = ISNULL(T2.TBCCNgay01,0),
BCCNgay03 = ISNULL(T3.TBCCNgay01,0),
BCCNgay04 = ISNULL(T4.TBCCNgay01,0),
BCCNgay05 = ISNULL(T5.TBCCNgay01,0),
BCCNgay06 = ISNULL(T6.TBCCNgay01,0),
BCCNgay07 = ISNULL(T7.TBCCNgay01,0),
BCCNgay08 = ISNULL(T8.TBCCNgay01,0),
BCCNgay09 = ISNULL(T9.TBCCNgay01,0),
BCCNgay10 = ISNULL(T10.TBCCNgay01,0),
BCCNgay11 = ISNULL(T10.TBCCNgay01,0),
BCCNgay12 = ISNULL(T12.TBCCNgay01,0),
BCCNgay13 = ISNULL(T13.TBCCNgay01,0),
BCCNgay14 = ISNULL(T14.TBCCNgay01,0),
BCCNgay15 = ISNULL(T15.TBCCNgay01,0),
BCCNgay16 = ISNULL(T16.TBCCNgay01,0),
BCCNgay17= ISNULL(T17.TBCCNgay01,0),
BCCNgay18 = ISNULL(T18.TBCCNgay01,0),
BCCNgay19 = ISNULL(T19.TBCCNgay01,0),
BCCNgay20 = ISNULL(T20.TBCCNgay01,0),
BCCNgay21 = ISNULL(T21.TBCCNgay01,0),
BCCNgay22 = ISNULL(T22.TBCCNgay01,0),
BCCNgay23 = ISNULL(T23.TBCCNgay01,0),
BCCNgay24 = ISNULL(T24.TBCCNgay01,0),
BCCNgay25 = ISNULL(T25.TBCCNgay01,0),
BCCNgay26 = ISNULL(T26.TBCCNgay01,0),
BCCNgay27 = ISNULL(T27.TBCCNgay01,0),
BCCNgay28 = ISNULL(T28.TBCCNgay01,0),
BCCNgay29 = ISNULL(T29.TBCCNgay01,0),
BCCNgay30 = ISNULL(T30.TBCCNgay01,0),
BCCNgay31 = ISNULL(T31.TBCCNgay01,0)
FROM tblBangChamCong'+ CONVERT(VARCHAR(5),@Nam) +' tb
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +' and DAY(BCNgay)=1
)T1 ON T1.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=3
)T3 ON T3.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=4
)T4 ON T4.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=5
)T5 ON T5.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=6
)T6 ON T6.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=7
)T7 ON T7.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=8
)T8 ON T8.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=9
)T9 ON T9.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=10
)T10 ON T10.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=11
)T11 ON T11.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=12
)T12 ON T12.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=13
)T13 ON T13.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=14
)T14 ON T14.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=15
)T15 ON T15.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=16
)T16 ON T16.BCMaNV = tb.BCCMaNV '

Set @Sql2 = 'LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=17
)T17 ON T17.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=18
)T18 ON T18.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=19
)T19 ON T19.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=20
)T20 ON T20.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=21
)T21 ON T21.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=22
)T22 ON T22.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=23
)T23 ON T23.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=24
)T24 ON T24.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=25
)T25 ON T25.BCMaNV = tb.BCCMaNV

LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=26
)T26 ON T26.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=27
)T27 ON T27.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=28
)T28 ON T28.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=29
)T29 ON T29.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=30
)T30 ON T30.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=31
)T31 ON T31.BCMaNV = tb.BCCMaNV
LEFT JOIN (
	SELECT BCMaNV,Round( (BCTGLamNgay + BCTGThemNgay)/60,2) AS TBCCNgay01 FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam) +''+ Right('_'+ RIGHT( '00'+ CONVERT(VARCHAR(5),@Thang),2),3) +' WHERE BCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'  and DAY(BCNgay)=2
)T2 ON T2.BCMaNV = tb.BCCMaNV
WHERE BCCThang = '+ CONVERT(VARCHAR(3),@Thang) +' AND BCCMaNV = '+ CONVERT(VARCHAR(10),@MaNV) +'
'
EXEC(@Sql1+@Sql2)
GO
/****** Object:  StoredProcedure [dbo].[cp_TongCongChoMotNhanVien_Children]    Script Date: 9/19/2026 10:35:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[cp_TongCongChoMotNhanVien_Children]
@MaNV INT,
@Thang INT,
@Nam INT

AS
declare @e varchar(5)
declare @c varchar(5)
SET @e = '00'
SET @c= '01'
DECLARE @Sql1 VARCHAR(4000)
DECLARE @Sql2 VARCHAR(4000)

SET @Sql1 =  ' declare @Ngay smalldatetime 
set @Ngay = convert(varchar(5), '+ Convert(varchar(5),@Nam) +') + Right(convert(varchar(2),'+ @e +') + Convert(varchar(5),'+ Convert(varchar(5),@Thang) +'),2) + Right(Convert(varchar(2),'+ @e +') + Convert(varchar(5),'+ @c +') ,2)
DECLARE @CCTuNgay SMALLDATETIME
DECLARE @CCDenNgay SMALLDATETIME
IF DAY(@Ngay) = 1
BEGIN
SET @CCTuNgay = @Ngay
SET @CCDenNgay = DATEADD(DAY,-1,DATEADD(MONTH,1,@Ngay))
END 
ELSE
BEGIN
SET @CCTuNgay = DATEADD(DAY,-DAY(@Ngay)+1,@Ngay)
SET @CCDenNgay = DATEADD(DAY,-1,DATEADD(MONTH,1,DATEADD(DAY,-DAY(@Ngay)+1,@Ngay)))
END	
	
	DECLARE @DKNNgayKetThuc SMALLDATETIME
	DECLARE @DKNNgayApDung SMALLDATETIME
	DECLARE @DKNMaLyDo INT
	DECLARE @DKNLoai INT
	
	CREATE TABLE [dbo].[#TempDKN] ( [Ngay] [datetime] NOT NULL ,[DKNMaLyDo] [INT] NOT NULL, [DKNLoai] [INT] NOT NULL)
	
	DECLARE DKNghi CURSOR FOR
		SELECT DISTINCT DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai
		FROM tblDangKyNghi'+ Convert(varchar(10),@Nam) +'
		WHERE DKNMaNV = '+ CONVERT(VARCHAR(5),@MaNV) +'
	OPEN DKNghi
	FETCH NEXT FROM DKNghi INTO @DKNNgayApDung,@DKNNgayKetThuc,@DKNMaLyDo,@DKNLoai
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF @CCTuNgay <= @DKNNgayApDung AND @CCDenNgay > @DKNNgayKetThuc
		WHILE @DKNNgayApDung < = @DKNNgayKetThuc
		BEGIN
			INSERT INTO #TempDKN (Ngay,DKNLoai,DKNMaLyDo) VALUES (@DKNNgayApDung,@DKNLoai,@DKNMaLyDo) 
			SET @DKNNgayApDung = DATEADD(DAY,1,@DKNNgayApDung)
		END
		IF @CCTuNgay <= @DKNNgayApDung AND @CCDenNgay <= @DKNNgayKetThuc
		WHILE @DKNNgayApDung < = @CCDenNgay
		BEGIN
			INSERT INTO #TempDKN (Ngay,DKNLoai,DKNMaLyDo) VALUES (@DKNNgayApDung,@DKNLoai,@DKNMaLyDo)
			SET @DKNNgayApDung = DATEADD(DAY,1,@DKNNgayApDung)
		END
		
		IF @CCTuNgay > @DKNNgayApDung AND @CCTuNgay <= @DKNNgayKetThuc AND @DKNNgayKetThuc < = @CCDenNgay
		WHILE @CCTuNgay < = @DKNNgayKetThuc
		BEGIN
			INSERT INTO #TempDKN (Ngay,DKNLoai,DKNMaLyDo) VALUES (@CCTuNgay,@DKNLoai,@DKNMaLyDo)
			SET @CCTuNgay = DATEADD(DAY,1,@CCTuNgay)
		END
		IF @CCTuNgay > @DKNNgayApDung AND @CCTuNgay <= @DKNNgayKetThuc AND @DKNNgayKetThuc > @CCDenNgay
		WHILE @CCTuNgay < = @CCDenNgay
		BEGIN
			INSERT INTO #TempDKN (Ngay,DKNLoai,DKNMaLyDo) VALUES (@CCTuNgay,@DKNLoai,@DKNMaLyDo)
			SET @CCTuNgay = DATEADD(DAY,1,@CCTuNgay)
		END
		FETCH NEXT FROM DKNghi INTO @DKNNgayApDung,@DKNNgayKetThuc,@DKNMaLyDo,@DKNLoai
	END
	CLOSE DKNghi
	DEALLOCATE DKNghi
'
SET @Sql2 = ' 
UPDATE tblBangChamCong'+ CONVERT(VARCHAR(5),@Nam) +'
SET BCCCongN = isnull(T1.TCong,0), 
	BCCCongD = isnull(T2.TCong,0),
	BCCLamThemN = isnull(T3.TCong,0),
	BCCLamThemD = Isnull(T4.TCong,0)
FROM tblBangChamCong'+ Convert(varchar(5),@Nam) +' tb 
LEFT JOIN (
SELECT '+ convert(varchar(5),@MaNV) +' AS MaNV,SUM((CONVERT(float,BCTGLamNgay)/CONVERT(float,BCTGQuyDinh))) AS TCong
FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam)  + Right( '_' + RIGHT( '00' + CONVERT(VARCHAR(5),@Thang),2),3)  +'
WHERE BCMaNV= '+ convert(varchar(5),@MaNV) +' 
AND BCNgay IN (SELECT NNLNgay FROM tblNgayNghiLe)   
AND BCNgay NOT IN (SELECT Ngay FROM #TempDKN WHERE DKNLoai=0)) T1 ON tb.BCCMaNV = T1.MaNV
LEFT JOIN (
SELECT '+ convert(varchar(5),@MaNV) +' AS MaNV,SUM((CONVERT(float,BCTGLamToi)/CONVERT(float,BCTGQuyDinh))) AS TCong
FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam)  + Right( '_' + RIGHT( '00' + CONVERT(VARCHAR(5),@Thang),2),3)  +' 
WHERE BCMaNV= '+ convert(varchar(5),@MaNV) +'  
AND BCNgay IN (SELECT NNLNgay FROM tblNgayNghiLe)   
AND BCNgay NOT IN (SELECT Ngay FROM #TempDKN WHERE DKNLoai=0)) T2 ON tb.BCCMaNV = T2.MaNV
LEFT JOIN (
SELECT '+ convert(varchar(5),@MaNV) +' AS MaNV,SUM((CONVERT(float,BCTGThemNgay)/CONVERT(float,BCTGQuyDinh))) AS TCong
FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam)  + Right( '_' + RIGHT( '00' + CONVERT(VARCHAR(5),@Thang),2),3)  +' 
WHERE BCMaNV= '+ convert(varchar(5),@MaNV) +'  
AND BCNgay IN (SELECT NNLNgay FROM tblNgayNghiLe)   
AND BCNgay NOT IN (SELECT Ngay FROM #TempDKN WHERE DKNLoai=0)) T3 ON tb.BCCMaNV = T3.MaNV
LEFT JOIN (
SELECT '+ convert(varchar(5),@MaNV) +' AS MaNV,SUM((CONVERT(float,BCTGThemToi)/CONVERT(float,BCTGQuyDinh))) AS TCong
FROM tblBaoCao'+ CONVERT(VARCHAR(5),@Nam)  + Right( '_' + RIGHT( '00' + CONVERT(VARCHAR(5),@Thang),2),3)  +' 
WHERE BCMaNV= '+ convert(varchar(5),@MaNV) +'  
AND BCNgay IN (SELECT NNLNgay FROM tblNgayNghiLe)   
AND BCNgay NOT IN (SELECT Ngay FROM #TempDKN WHERE DKNLoai=0)) T4 ON tb.BCCMaNV = T4.MaNV
WHERE tb.BCCMaNV = '+ convert(varchar(5),@MaNV) +' 
'
EXEC(@Sql1+@Sql2)
GO
/****** Object:  StoredProcedure [dbo].[sp_TimeKeeping]    Script Date: 9/19/2026 10:35:40 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO



CREATE PROCEDURE [dbo].[sp_TimeKeeping]
	@StaffID int,	
	@D Datetime
 as
	Begin
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblBaocaotam]') AND type in (N'U'))
	DROP TABLE [dbo].[tblBaocaotam]
	
	
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblLocdulieutam]') AND type in (N'U'))
	DROP TABLE [dbo].[tblLocdulieutam]
	
	
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblDangkynghiTam]') AND type in (N'U'))
	DROP TABLE [dbo].[tblDangkynghiTam]
	
	
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblDangkyUudaiTam]') AND type in (N'U'))
	DROP TABLE [dbo].[tblDangkyUudaiTam]
	
	
	
	--DECLARE @StaffID int
	--DECLARE @D Datetime 
		
	--SET @StaffID=7
	--SET @D='2009-02-03'

	DECLARE @TGBDCa Datetime 	
	DECLARE @TGBDNghi1  Datetime 	
	DECLARE @TGKTNghi1 Datetime 	
	DECLARE @TGBDNghi2  Datetime 	
	DECLARE @TGKTNghi2 Datetime 	
	DECLARE @TGBDNghi3  Datetime 	
	DECLARE @TGKTNghi3 Datetime 	
	DECLARE @TGKTCa Datetime 
	DECLARE @TGQDD int
	DECLARE @TGQDC int
	DECLARE @TGQD int
	DECLARE @TGQD1 int
	DECLARE @TGNghiGiuaGio int
	DECLARE @TGBatDauLT Datetime 
	DECLARE @PhutBatDauLT int
	DECLARE @DuocRaNgoai bit
	DECLARE @CongNghiGiuaCa bit
	DECLARE @DonViLamThem smallint
	DECLARE @NguongLamThem	smallint
	DECLARE @NguongDiMuon smallint
	DECLARE @NguongVeSom smallint
	DECLARE @QuetTruocCa int
	DECLARE @QuetSauCa int
	DECLARE @DonViChamCong int
	DECLARE @LoaiCa smallint
	DECLARE @TinhVaoSom bit	
	DECLARE @TGLayDLDau  Datetime 	
	DECLARE @TGLayDLCuoi Datetime 

	DECLARE @Trangthai smallint
	
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
	
	-- Khai bao cac nguong lay DL
	DECLARE @ThresholdDay Datetime 
	DECLARE @ThresholdNight Datetime 

	DECLARE @ThresholdDay1 Datetime 
	DECLARE @ThresholdDay2 Datetime 
	DECLARE @ThresholdDay3 Datetime
	DECLARE @ThresholdNight1 Datetime 
	DECLARE @ThresholdNight2 Datetime 
	DECLARE @ThresholdNight3 Datetime
    -- Thoi gian di som
    DECLARE @TGSomN int
    DECLARE @TGSomD int
    -- Thoi gian den, cua den
	DECLARE	@TGDen Datetime
	DECLARE	@Cuaden smallint
    -- Thoi gian den, cua den
	DECLARE	@TGVe Datetime
	DECLARE	@CuaVe smallint
	-- Thoi gian vao, cua vao, thoi gian ra, cua ra
	DECLARE	@TGVao Datetime
	DECLARE	@CuaVao smallint
	DECLARE	@TGRa Datetime
	DECLARE	@CuaRa smallint
	
	-- Thoi gian lam ngay, lam dem, them ngay, them dem
    DECLARE @TGLamN int
    DECLARE @TGLamD int
    DECLARE @TGQuaN int
    DECLARE @TGQuaD int
    DECLARE @TGThemN int
    DECLARE @TGThemD int
	
	-- Thoi gian ra ngoai
    DECLARE @TGRaNgoaiN int
    DECLARE @TGRaNgoaiD  int
    -- Thoi gian di muon
    DECLARE @TGMuonN int
    DECLARE @TGMuonD int
    -- Khai bao loai nghi, Ma ly do nghi, Ghi chu, 
    DECLARE @LoaiNghi smallint
    DECLARE @MaLyDoNghi smallint
    DECLARE @LydonghiViettat nvarchar(10)
    
    DECLARE @Ghichu nvarchar(50)
    
     -- Thoi gian ca tam, thoi gian den tam
    DECLARE @TGVeTemp datetime
    DECLARE @TGBDCaTemp  datetime
    DECLARE @TGKTCaTemp datetime
    DECLARE @TGDenTemp datetime
    DECLARE @TGRaTemp  datetime    
    DECLARE @TGVaoTemp  datetime    
                    
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
	-- Khai bao uu dai
    DECLARE    @TGUD1 int
    DECLARE    @TGUD2 int
    DECLARE    @TGUD3 int
    DECLARE    @TGUD4 int
	
	DECLARE	@i int
	
	DECLARE @SQL nvarchar(4000) 	
	DECLARE @SQL1 nvarchar(4000) 	
	DECLARE @SQL2 nvarchar(4000) 	
	DECLARE @SQL3 nvarchar(4000) 	
	DECLARE @SQL4 nvarchar(4000) 	
	
	
	
	SET @Trangthai=0
	
	-- Lay cac tham so cham cong
	
	SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='THRESHOLDDAY'
	SET @ThresholdDay=CONVERT(Datetime,@SQL)
	
	SELECT @SQL= TSGiaTri FROM tblThamSo  WHERE TSTen='THRESHOLDNIGHT'
	SET @ThresholdNight=CONVERT(Datetime,@SQL)
	
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
	Create TABLE tblBaocaotam ([BCmaca][int],[BCLoai] [bit] )
	EXEC ('Insert INTO tblBaocaotam(bcmaca,BCLoai) Select bcmaca,BCloai  From  ' + @tblBaocao + ' WHERE (BCMaNV=' + @StaffID +') AND (BCNgay=' + '''' + @D + '''' + ')')
	select @Maca = BCmaca from tblBaocaotam
	select @BCloai = BCloai from tblBaocaotam
	
	if (@Maca is null)
		BEGIN
			-- Neu khong ton tai nhan vien trong bang bao cao thi khoi tao lai
			EXEC SP_CreateRecordLNV @StaffID,@D
			EXEC ('Insert INTO tblBaocaotam(bcmaca) Select bcmaca  From  ' + @tblBaocao + ' WHERE (BCMaNV=' + @StaffID +') AND (BCNgay=' + '''' + @D + '''' + ')')
			select @Maca = BCmaca from tblBaocaotam
		end
	
	-- Kiem tra xem nhan vien co dang ky nghi khong
	
		-- Tao bang dang ky nghi tam
		SET @SQL='CREATE TABLE [dbo].tblDangkynghiTam ([DKNMaNV] [int] NOT NULL ,[DKNNgayApDung] [datetime] NOT NULL ,[DKNNgayKetThuc] [datetime] NOT NULL ,[DKNMaLyDo] [int] NOT NULL ,[DKNLoai] [tinyint] NOT NULL) ON [PRIMARY]'
		EXEC (@SQL)
		
		SET @SQL='Insert into tblDangkynghiTam(DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai) SELECT DKNMaNV,DKNNgayApDung,DKNNgayKetThuc,DKNMaLyDo,DKNLoai FROM '+ @tblDangkynghi+ ' WHERE (DKNMaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKNNgayApDung AND DKNNgayKetThuc) '
		EXEC (@SQL)
		
		DECLARE curRS 
		CURSOR FOR 
			SELECT DKNLoai,DKNMaLyDo  FROM tblDangkynghiTam 
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
				SET @SQL='UPDATE ' + @tblBaocao + ' SET BCCuaDen=0,BCTGDen=NULL,BCCuaVe=0,BCTGVe=NULL,BCCuaRa=0,BCTGRa=NULL,BCCuaVao=0,BCTGVao=NULL,BCTGLamNgay=0,BCTGLamToi=0,BCTGQuaGioNgay=0,'
				SET @SQL= @SQL + 'BCTGQuaGioToi=0,BCTGThemNgay=0,BCTGThemToi=0,BCTGRaNgoaiNgay=0,BCTGRaNgoaiToi=0,BCTGDiMuonNgay=0,BCTGDiMuonToi=0,BCTGVeSomNgay=0,BCTGVeSomToi=0,BCTGQuyDinh=480,BCLoai=0,BCLoaiLamThem=0,BCGhiChu=' + '''' +  CONVERT(nvarchar(30),@GhiChu) + '''' + ' WHERE (BCMaNV=' + CONVERT(nvarchar(30),@StaffID) +') AND (BCNgay=' + '''' + CONVERT(nvarchar(30),@D) + '''' +')'
				EXEC (@SQL)
				
				GOTO EndThisSub
			END
		CLOSE curRS
		DEALLOCATE curRS
			
		--Kiem tra neu cham cong bang tay thi thoat
		IF @BCloai=1 GOTO Endthissub
	
	
	-- Kiem tra dang ky uu dai
		SET @SQL='CREATE TABLE [dbo].tblDangkyUudaiTam ([DKUDMaNV] [int] NOT NULL ,[DKUDLoaiUuDai] [int] NOT NULL , [DKUDNgayApDung] [datetime] NOT NULL ,[DKUDNgayKetThuc] [datetime] NULL) ON [PRIMARY]'
		EXEC (@SQL)
		SET @SQL='Insert into tblDangkyUudaiTam(DKUDMaNV,DKUDNgayApDung,DKUDNgayKetThuc,DKUDLoaiUuDai) SELECT DKUDMaNV,DKUDNgayApDung,DKUDNgayKetThuc,DKUDLoaiUuDai FROM '+ @tblDangkyuudai+ ' WHERE (DKudMaNV=' + CONVERT(nvarchar(30),@StaffID) + ') AND (' + ''''  + CONVERT(nvarchar(30),@D) + '''' + ' BETWEEN DKUDNgayApDung AND DKUDNgayKetThuc) '
		EXEC (@SQL)
		
		DECLARE curRS 
		CURSOR FOR 
			SELECT LUDDauCa,LUDTruocNghi,LUDSauNghi,LUDCuoiCa  FROM tblDangkyUudaiTam INNER JOIN tblLoaiUuDai ON  tblDangkyUudaiTam.DKUDLoaiUuDai=tblLoaiUuDai.LUDMa WHERE (DKudMaNV= @StaffID ) AND ( @D BETWEEN DKUDNgayApDung AND DKUDNgayKetThuc)
		OPEN curRS

		FETCH NEXT FROM curRS INTO @TGUD1,@TGUD2,@TGUD3,@TGUD4
		if @@FETCH_STATUS = 0
			BEGIN
				
				FETCH NEXT FROM curRS INTO @TGUD1,@TGUD2,@TGUD3,@TGUD4
			END
		ELSE
		BEGIN
			 SET @TGUD1 =0
			 SET @TGUD2=0
			 SET @TGUD3 =0
			 SET @TGUD4=0
		END
		CLOSE curRS
		DEALLOCATE curRS
		
	
	DECLARE curRS 
	CURSOR FOR 
		SELECT CTGBatDau,CTGBDNghi1,CTGKTNghi1,CTGBDNghi2,CTGKTNghi2,CTGBDNghi3,CTGKTNghi3,CTGKetThuc,CTGNghiGiuaGio,CTGQDD,CTGQDC,CDuocRaNgoai,CTinhVaoSom,CCongNghiGiuaCa,CBDTinhLT,CNguongLamThem,CDonViLamThem,CNguongDiMuon,@NguongVeSom,CQuetTruocCa,CQuetSauCa,CDonViChamCong,CLoaiCa FROM tblCa WHERE CMa=@Maca
	OPEN curRS

	FETCH NEXT FROM curRS INTO @TGBDCa, @TGBDNghi1,@TGKTNghi1, @TGBDNghi2,@TGKTNghi2, @TGBDNghi3,@TGKTNghi3,@TGKTCa,@TGNghiGiuaGio,@TGQDD,@TGQDC,@DuocRaNgoai,@TinhVaoSom,@CongNghiGiuaCa,@PhutBatDauLT,@NguongLamThem,@DonViLamThem,@NguongDiMuon,@NguongVeSom,@QuetTruocCa,@QuetSauCa,@DonViChamCong,@LoaiCa
	if @@FETCH_STATUS = 0
		BEGIN
			
			--Lay thong tin tu bang ca
			set @TGBDCa=@D + @TGBDCa
			set @TGBDNghi1=@TGBDCa + @TGBDNghi1
			set @TGKTNghi1=@TGBDCa + @TGKTNghi1
			set @TGBDNghi2=@TGBDCa + @TGBDNghi2
			set @TGKTNghi2=@TGBDCa + @TGKTNghi2
			set @TGBDNghi3=@TGBDCa + @TGBDNghi3
			set @TGKTNghi3=@TGBDCa + @TGKTNghi3
			set @TGKTCa=@TGBDCa + @TGKTCa
			
			set @TGBatDauLT=DateAdd(minute, @PhutBatDauLT, @TGKTCa)
			
			
			set @TGLayDLDau= DateAdd(minute, -@QuetTruocCa, @TGBDCa)
			set @TGLayDLCuoi=DateAdd(minute, @QuetSauCa, @TGKTCa)
			
	ENd
		CLOSE curRS
	DEALLOCATE curRS
	
	---- Lay thoi gian quy dinh cua ca
	
    If @LoaiNghi = 2 
		BEGIN
			SET @TGBDCa = DateAdd(minute, @TGUD3, @TGKTNghi2)
			SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)
			SET @TGQD = @TGQDC - @TGUD3 - @TGUD4
		END
    ELSE
    	If @LoaiNghi = 3 
    	BEGIN
			SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa)
			SET @TGKTCa = DateAdd(minute, -@TGUD2, @TGBDNghi2)
			SET @TGQD = @TGQDD - @TGUD1 - @TGUD2
    	END
		ELSE
			BEGIN
				SET @TGBDCa = DateAdd(minute, @TGUD1, @TGBDCa)
				SET @TGBDNghi2 = DateAdd(minute, -@TGUD2, @TGBDNghi2)
				SET @TGKTNghi2 = DateAdd(minute, @TGUD3, @TGKTNghi2)
				SET @TGKTCa = DateAdd(minute, -@TGUD4, @TGKTCa)
				SET @TGQD = @TGQDD + @TGQDC - @TGUD1 - @TGUD2 - @TGUD3 - @TGUD4
			END
    SET @TGQD1 = @TGQDD + @TGQDC - @TGUD1 - @TGUD2 - @TGUD3 - @TGUD4
    	
	-- Chinh lai moc ca
    If (@TGKTCa < @TGBDCa ) SET @TGKTCa = @TGBDCa
    If (@TGBDNghi1 < @TGBDCa) SET @TGBDNghi1 = @TGBDCa
    If (@TGKTNghi1 < @TGBDCa) SET @TGKTNghi1 = @TGBDCa
    If (@TGBDNghi2 < @TGBDCa) SET @TGBDNghi2 = @TGBDCa
    If (@TGKTnghi2 < @TGBDCa) SET @TGKTnghi2 = @TGBDCa
    If (@TGBDnghi3 < @TGBDCa) SET @TGBDnghi3 = @TGBDCa
    If (@TGKTNghi3 < @TGBDCa) SET @TGKTNghi3 = @TGBDCa
    
    If (@TGBDNghi1 > @TGKTCa) SET @TGBDNghi1 = @TGKTCa
    If (@TGKTNghi1 > @TGKTCa) SET @TGKTNghi1 = @TGKTCa
    If (@TGBDNghi2 > @TGKTCa) SET @TGBDNghi2 = @TGKTCa
    If (@TGKTnghi2 > @TGKTCa) SET @TGKTnghi2 = @TGKTCa
    If (@TGBDnghi3 > @TGKTCa) SET @TGBDnghi3 = @TGKTCa
    If (@TGKTNghi3 > @TGKTCa) SET @TGKTNghi3 = @TGKTCa	
    
    -- Lay ma the
    
    SELECT @MaThe= CTMaThe FROM tblCapThe WHERE (CTMaNV=@StaffID) AND (@D BETWEEN CTNgayApDung AND CTNgayKetThuc) ORDER BY CTNgayKetThuc DESC
    IF @MaThe IS NULL SET @MaThe='' 
    
    
	-- Loc du lieu
	--drop table [tblLocdulieutam]
	--DELETE tblLocdulieutam
	CREATE TABLE [dbo].[tblLocdulieutam]([IDM] [tinyint] NOT NULL,[IDCard] [nvarchar](14) NOT NULL,[ThoiGian] [datetime] NOT NULL,[Status] [bit] NOT NULL,[HandData] [bit] NOT NULL,[Pass] [bit] NOT NULL)
	EXEC('INSERT INTO tblLocdulieutam(IDM,IDCard,ThoiGian,Status,HandData,Pass) SELECT DISTINCT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM ' + @RecordData + ' WHERE (ThoiGian BETWEEN '+ '''' + @TGLayDLDau + '''' + ' AND '+ '''' + @TGLayDLCuoi + ''''+ ')  and  IDCard=' + '''' + @MaThe + ''''+ '  ORDER BY ThoiGian')
	
	DECLARE curRS 
	CURSOR Scroll FOR 
		SELECT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM tblLocdulieutam ORDER BY Thoigian
	OPEN curRS
		FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
		
	 -- Thay doi trang thai vao ra
	 --- Neu la trang thai dau doc
    If @OriginalStatus <> 0 
		BEGIN
	    	-- Neu khong phai loai tinh gio la quyet dau, quyet cuoi thi loc du lieu
	    	IF @TypeCalculator <> 2
	    	BEGIN
	    		-- Viet code loc du lieu o day
	    		SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
	    		
	    		-- Neu co du lieu moi loc
	    		IF @recordCount>1 
	    		BEGIN
	    			-- Xoa cac ban ghi ra dau tien
	    			WHILE @@FETCH_STATUS = 0 AND  @Status=0 AND @recordCount>1
	    			BEGIN
	    				DELETE tblLocdulieutam WHERE ThoiGian=@ThoiGian
	    				SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
						FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    			END
	    			
	    			-- Xoa cac ban ghi vao cuoi cung
		    		SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
					FETCH LAST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    			WHILE @@FETCH_STATUS = 0 AND  @Status=1 AND @recordCount>1
	    			BEGIN
	    				DELETE tblLocdulieutam WHERE ThoiGian=@ThoiGian
	    				SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
						FETCH PRIOR FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    			END
	    			
	    			-- Xoa cac ban ghi lap nhau - Kiem tra ban ghi sau trung trang thai ban ghi truoc
	    			FETCH FIRST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    			SET @OldStatus = @Status
	    			FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    			WHILE @@FETCH_STATUS = 0 
	    			BEGIN
	    				IF (@Status=@OldStatus)
	    					begin 
	    						IF @Status=1 DELETE tblLocdulieutam WHERE ThoiGian=@ThoiGian
	    					end
	    				ELSE
	    					BEGIN
	    						SET @OldStatus=@Status
	    					END
						FETCH NEXT FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    			END
	    				
	    			-- Xoa cac ban ghi lap nhau - Kiem tra ban ghi truoc trung trang thai ban ghi sau
	    			FETCH LAST FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    			IF @@FETCH_STATUS = 0  
	    			BEGIN
	    				SET @OldStatus = @Status
	    				FETCH PRIOR FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	    				WHILE @@FETCH_STATUS = 0 
	    				BEGIN
	    					
	    					--SELECT * FROM tblLocdulieutam
	    					
	    					IF  @Status=@OldStatus
	    						BEGIN
	    							IF @Status=0 DELETE tblLocdulieutam WHERE ThoiGian=@ThoiGian
	    						END
	    					ELSE
	    						BEGIN
	    							SET @OldStatus=@Status
	    						END
	    					
	    					--CLOSE curRS
	    					
							--CURSOR Scroll FOR 
							--	SELECT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM tblLocdulieutam ORDER BY Thoigian
		    				--OPEN curRS 
		    				
							FETCH PRIOR FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
							
	    				END
	    			END
	    			
	    		END
	    		
	    	END
	    	-- Neu kieu tinh gio là cac cap vao ra hoac vao dau ra cuoi
	    	ELSE
	    	BEGIN
	    		SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 

	    	END
		END
	-- Neu la xen ke trang thai
    ELSE
    	BEGIN
			WHILE @@FETCH_STATUS = 0
				BEGIN
					set @Trangthai=1-@Trangthai
					exec('Update ' + @RecordData + ' set Status=' + @Trangthai + ' where (IDCard=' + '''' + @IDCard + '''' + ') and (ThoiGian=' + '''' + @ThoiGian + '''' + ')' )
					exec('Update tblLocdulieutam set Status=' + @Trangthai + ' where (IDCard=' + '''' + @IDCard + '''' + ') and (ThoiGian=' + '''' + @ThoiGian + '''' + ')' )
					FETCH NEXT FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
				END
    	END

    -- Sau khi loc ma khong co ban ghi nao thi cap nhat gia tri ve mac dinh
		SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
		IF @recordCount =0
			BEGIN
				SET @SQL='UPDATE ' + @tblBaocao +' SET BCCuaDen=0,BCTGDen=NULL,BCCuaVe=0,BCTGVe=NULL,BCCuaRa=0,BCTGRa=NULL,BCCuaVao=0,BCTGVao=NULL,BCTGLamNgay=0,BCTGLamToi=0,BCTGQuaGioNgay=0,BCTGQuaGioToi=0,BCTGThemNgay=0,BCTGThemToi=0,BCTGRaNgoaiNgay=0,BCTGRaNgoaiToi=0,BCTGDiMuonNgay=0,BCTGDiMuonToi=0,BCTGVeSomNgay=0,BCTGVeSomToi=0,BCTGQuyDinh=480,BCLoai=0,BCLoaiLamThem=0,BCGhiChu='''' WHERE (BCMaNV=' + CONVERT(nvarchar(20),@StaffID) + ') AND (BCNgay='+ '''' + CONVERT(nvarchar(20),@D) + '''' +')'
				EXEC (@SQL)
				
				CLOSE curRS
				DEALLOCATE curRS
				GOTO Endthissub
			END
    
	 -- Loc du lieu theo dieu kien tinh toan
    If @TypeCalculator <> 2 
    BEGIN
		SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
        If @Status =0  And (@recordCount > 1) DELETE FROM tblLocdulieutam WHERE ThoiGian=@ThoiGian
		SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
        If (@recordCount> 1) 
        BEGIN
        	FETCH Last FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
            If @Status=1  DELETE FROM tblLocdulieutam WHERE  ThoiGian=@ThoiGian
        END
    END
	
		CLOSE curRS
	DEALLOCATE curRS
	
	--select * from tblLocdulieutam
	
	-- Lay cac nguong tinh tinh gio
	SET @ThresholdDay1 = DateAdd(day, -1, @D) + @ThresholdDay
    SET @ThresholdDay2 = @D + @ThresholdDay
    SET @ThresholdDay3 = DateAdd(day, 1, @D) + @ThresholdDay
    SET @ThresholdNight1 =DateAdd(day, -1, @D) + @ThresholdNight
    SET @ThresholdNight2 = @D + @ThresholdNight
    SET @ThresholdNight3 = DateAdd(day, 1, @D) + @ThresholdNight
	
	-- Lay tong so ban ghi
	SELECT @recordCount = COUNT(idm)  FROM tblLocdulieutam 
	
	-- Lay thoi gian den va ve
	DECLARE curRS 
	CURSOR Scroll FOR 
		SELECT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM tblLocdulieutam ORDER BY Thoigian
	OPEN curRS
	
	--	-- Lay thoi gian den, cua den
	FETCH First FROM curRS INTO @Cuaden,@IDCard,@TGDen,@Status,@HandData,@Pass
	IF (@@FETCH_STATUS = 0 )
	BEGIN
		-- Neu co 2 ban ghi tro len thi lam 
		IF @recordCount>1 
			BEGIN
				
				FETCH Last FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
					IF (@@FETCH_STATUS = 0 )
					BEGIN
						 If  {fn mod(@recordCount,2)} = 1 
								FETCH   PRIOR FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
								SET @TGVe = @ThoiGian
								SET @CuaVe =@IDM
								SET @TGRa = @TGVe
								SET @TGVao = @TGVe
								SET @CuaRa = 0
								SET @CuaVao = 0
					END
					
					FETCH First FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass

					SET @i=0
					WHILE @i < = @recordCount/2
						BEGIN
							IF  @Status<>1
								BEGIN
									If (Abs(DateDiff(minute, @ThoiGian, @TGBDNghi2)) < Abs(DateDiff(minute, @TGRa, @TGBDNghi2)))
										BEGIN
											FETCH Next FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
											IF (@@FETCH_STATUS = 0 )
											BEGIN
												SET @TGVao = @ThoiGian
												SET @CuaVao =@IDM
												
												FETCH   PRIOR FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
													IF (@@FETCH_STATUS = 0 )
													begin
														SET @TGRa = @ThoiGian
														SET @CuaRa = @IDM
													END
											END		
										END
								END
							
							SET @I=	@I+1
							FETCH next FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
						END
					
					-- Lay thoi gian ve, cua ve
				FETCH last FROM curRS INTO @Cuave,@IDCard,@TGVe,@Status,@HandData,@Pass
				
			END
		
	END 
		CLOSE curRS
	DEALLOCATE curRS
	
	-- Ket thuc phan lay thoi gian den, thoi gian ve

	-- Tinh thoi gian den som
    SET @TGSomN = DBO.InterSectionTime2(@ThresholdDay2, @ThresholdNight2, @TGVe, @TGKTCa) + DBO.InterSectionTime2(@ThresholdDay3, @ThresholdNight3, @TGVe, @TGKTCa)
    SET @TGSomD = DBO.InterSectionTime2(@ThresholdNight2, @ThresholdDay3, @TGVe, @TGKTCa)


	-- Tinh thoi gian som ngay, som dem
	SET @TGSomN=dbo.InterSectionTime2(@ThresholdDay2,@ThresholdNight2,@TGVe, @TGKTCa) +
				dbo.InterSectionTime2(@ThresholdDay3,@ThresholdNight3,@TGVe, @TGKTCa)
	SET @TGSomD=dbo.InterSectionTime2(@ThresholdNight2,@ThresholdDay3,@TGVe, @TGKTCa)
	
	-- Tinh thoi gian lam
	DECLARE curRS 
	CURSOR Scroll FOR 
		SELECT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM tblLocdulieutam ORDER BY Thoigian
	OPEN curRS

		-- Khoi tao lai cac gia tri
        SET @TGLamN = 0
        SET @TGLamD = 0
        SET @TGQuaN = 0
        SET @TGQuaD = 0
        SET @TGThemN = 0
        SET @TGThemD = 0
		
	FETCH first  FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	
	WHILE @@FETCH_STATUS = 0
		BEGIN
		 IF @Status=1 
		  BEGIN
		  	SET @TGDenTemp = @ThoiGian
		  	
			FETCH NEXT FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
			if @@FETCH_STATUS = 0 
			begin
				SET @TGVeTemp =@ThoiGian
				SET @TGBDCaTemp = CASE when @TinhVaoSom=1 then @TGDenTemp else @TGBDCa end
				SET @TGKTCaTemp = DateAdd(minute, @QuetSauCa, @TGKTCa)     
                SET @TGLamN = @TGLamN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDenTemp, @TGVeTemp, @TGBDCaTemp, @TGBDNghi1) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDenTemp, @TGVeTemp, @TGBDCa, @TGBDNghi1) 
                                           + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDenTemp, @TGVeTemp, @TGKTNghi1, @TGBDNghi2) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDenTemp, @TGVeTemp, @TGKTNghi1, @TGBDNghi2) 
                                           + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDenTemp, @TGVeTemp, @TGKTnghi2, @TGBDnghi3) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDenTemp, @TGVeTemp, @TGKTnghi2, @TGBDnghi3) 
                                           + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDenTemp, @TGVeTemp, @TGKTNghi3, @TGKTCa) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDenTemp, @TGVeTemp, @TGKTNghi3, @TGKTCa)
                SET @TGLamD = @TGLamD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDenTemp, @TGVeTemp, @TGBDCaTemp, @TGBDNghi1) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDenTemp, @TGVeTemp, @TGBDCa, @TGBDNghi1) 
                                           + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDenTemp, @TGVeTemp, @TGKTNghi1, @TGBDNghi2) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDenTemp, @TGVeTemp, @TGKTNghi1, @TGBDNghi2) 
                                           + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDenTemp, @TGVeTemp, @TGKTnghi2, @TGBDnghi3) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDenTemp, @TGVeTemp, @TGKTnghi2, @TGBDnghi3) 
                                           + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGDenTemp, @TGVeTemp, @TGKTNghi3, @TGKTCa) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDenTemp, @TGVeTemp, @TGKTNghi3, @TGKTCa)
                SET    @TGQuaN = @TGQuaN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGDenTemp, @TGVeTemp, @TGBatDauLT, @TGKTCaTemp)
                SET    @TGQuaD = @TGQuaD + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGDenTemp, @TGVeTemp, @TGBatDauLT, @TGKTCaTemp) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGDenTemp, @TGVeTemp, @TGBatDauLT, @TGKTCaTemp)
			end
		  END
		  FETCH NEXT FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
		ENd
		CLOSE curRS
	DEALLOCATE curRS


	-- Tinh thoi gian ra ngoai
	DECLARE curRS 
	CURSOR Scroll FOR 
		SELECT IDM,IDCard,ThoiGian,Status,HandData,Pass FROM tblLocdulieutam ORDER BY Thoigian
	OPEN curRS

		-- Khoi tao lai cac gia tri
        SET @TGRaNgoaiN = 0
        SET @TGRaNgoaiD = 0
	FETCH first  FROM curRS INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
	
	WHILE @@FETCH_STATUS = 0
		BEGIN
		 IF @Status<>1 
		  BEGIN
		  	SET @TGRaTemp = @ThoiGian
		  	
			FETCH NEXT FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
			if @@FETCH_STATUS = 0 
			begin
			SET @TGVaoTemp=@ThoiGian
                    set @TGRaNgoaiN = @TGRaNgoaiN + dbo.InterSectionTime3(@ThresholdDay2, @ThresholdNight2, @TGRaTemp, @TGVaoTemp, @TGBDCa, @TGBDNghi1) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRaTemp, @TGVaoTemp, @TGBDCa, @TGBDNghi1) 
                                           + dbo.InterSectionTime3(@ThresholdDay2,@ThresholdNight2, @TGRaTemp, @TGVaoTemp, @TGKTNghi1, @TGBDNghi2) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRaTemp, @TGVaoTemp, @TGKTNghi1, @TGBDNghi2) 
                                           + dbo.InterSectionTime3(@ThresholdDay2,@ThresholdNight2, @TGRaTemp, @TGVaoTemp, @TGKTnghi2, @TGBDnghi3) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRaTemp, @TGVaoTemp, @TGKTnghi2, @TGBDnghi3) 
                                           + dbo.InterSectionTime3(@ThresholdDay2,@ThresholdNight2, @TGRaTemp, @TGVaoTemp, @TGKTNghi3, @TGKTCa) + dbo.InterSectionTime3(@ThresholdDay3, @ThresholdNight3, @TGRaTemp, @TGVaoTemp, @TGKTNghi3, @TGKTCa)
                    set @TGRaNgoaiD = @TGRaNgoaiD + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRaTemp, @TGVaoTemp, @TGBDCa, @TGBDNghi1) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRaTemp, @TGVaoTemp, @TGBDCa, @TGBDNghi1) 
                                           + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRaTemp, @TGVaoTemp, @TGKTNghi1, @TGBDNghi2) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRaTemp, @TGVaoTemp, @TGKTNghi1, @TGBDNghi2) 
                                           + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRaTemp, @TGVaoTemp, @TGKTnghi2, @TGBDnghi3) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRaTemp, @TGVaoTemp, @TGKTnghi2, @TGBDnghi3) 
                                           + dbo.InterSectionTime3(@ThresholdNight1, @ThresholdDay2, @TGRaTemp, @TGVaoTemp, @TGKTNghi3, @TGKTCa) + dbo.InterSectionTime3(@ThresholdNight2, @ThresholdDay3, @TGRaTemp, @TGVaoTemp, @TGKTNghi3, @TGKTCa)
			end
		  END
		  FETCH NEXT FROM curRS  INTO @IDM,@IDCard,@ThoiGian,@Status,@HandData,@Pass
		ENd
		CLOSE curRS
	DEALLOCATE curRS
	
	-- Neu duoc ra ngoai thi cong thoi gian ra ngoai vao thoi gian lam
    If (@DuocRaNgoai=1)
		BEGIN
			SET @TGLamN = @TGLamN + @TGRaNgoaiN
			SET @TGLamD = @TGLamD + @TGRaNgoaiD
		END
	-- Neu khong du thoi gian lam
		-- Neu cong nghi giua ca vao thoi gian lam
        If (@TGLamN + @TGLamD < @TGQD) 
			begin
				If @CongNghiGiuaCa =1 
					BEGIN
						IF (@LoaiCa=0 OR  @LoaiCa=1 OR @LoaiCa=2 OR @LoaiCa=4)
						BEGIN
							IF (@TGLamN + @TGLamD + @TGNghiGiuaGio > @TGQD) SET @TGLamN=@TGQD - @TGLamD ELSE SET @TGLamN=@TGLamN + @TGNghiGiuaGio
						END
						ELSE
						BEGIN
							IF (@TGLamN + @TGLamD + @TGNghiGiuaGio > @TGQD) SET @TGLamD=@TGQD - @TGLamN ELSE SET @TGLamD=@TGLamD + @TGNghiGiuaGio
						END	
                end
			END
		-- Neu thoi gian lam ma lon hon thoi gian quy dinh
		ELSE
			BEGIN
				IF (@LoaiCa=0 OR @LoaiCa=1 OR @LoaiCa=2 OR @LoaiCa=4)
					BEGIN
						IF (@TGLamN + @TGLamD > @TGQD) 
							IF @TGQD - @TGLamN > 0 SET @TGLamD= @TGQD - @TGLamN ELSE SET @TGLamD=0
						IF (@TGLamN + @TGLamD > @TGQD) 
							IF @TGQD - @TGLamD > 0 SET @TGLamN= @TGQD - @TGLamD ELSE SET @TGLamN=0
					END
				ELSE
					BEGIN
						IF (@TGLamN + @TGLamD > @TGQD) 
							IF @TGQD - @TGLamN > 0 SET @TGLamD= @TGQD - @TGLamN ELSE SET @TGLamD=0
						IF (@TGLamN + @TGLamD > @TGQD) 
							IF @TGQD - @TGLamD > 0 SET @TGLamN= @TGQD - @TGLamD ELSE SET @TGLamN=0
					END
			END
            	
	-- Lam tron theo don vi cham cong
        SET @TGLamN = Round(@TGLamN / @DonViChamCong,0) * @DonViChamCong
        SET @TGLamD = Round(@TGLamD / @DonViChamCong,0) * @DonViChamCong
        
    -- Tinh thoi gian lam them
        SET @TGThemN = 0
        SET @TGThemD = 0
        SET @TGThemN = @TGQuaN
        SET @TGThemD = @TGQuaD
        SET @TGThemN = Round(@TGThemN / @DonViLamThem,0) * @DonViLamThem
        SET @TGThemD = Round(@TGThemD / @DonViLamThem,0) * @DonViLamThem
        If (@TGThemN + @TGThemD < @NguongLamThem)
			BEGIN 
				SET @TGThemN = 0
				SET @TGThemD = 0
			END
                
	-- Tinh thoi gian di muon, Ve som
    SET @TGMuonN = DBO.InterSectionTime2(@ThresholdDay2, @ThresholdNight2, @TGBDCa, @TGDen)
    SET @TGMuonD = DBO.InterSectionTime2(@ThresholdNight1, @ThresholdDay2, @TGBDCa, @TGDen) + DBO.InterSectionTime2(@ThresholdNight2, @ThresholdDay3, @TGBDCa, @TGDen)
    If (@TGMuonN + @TGMuonD < @NguongDiMuon )
		BEGIN
			SET @TGMuonN = 0
			SET @TGMuonD = 0
        END
    If (@TGSomN + @TGSomD < @NguongVeSom) 
		BEGIN
			SET @TGSomN = 0
			SET @TGSomD = 0
		END

-- Thiet lap lai
	SET @TGVe=CASE WHEN (@TGVe <> @TGDen) THEN   @TGVe   else null END
	SET @TGRa=CASE WHEN (@TGRa <> @TGVao) THEN   @TGRa   else null END
	SET @TGVao=CASE WHEN (@TGRa <> @TGVao) THEN   @TGVao   else null END
	
-- Update vao CSDL

	SET @SQL1='UPDATE ' + @tblBaocao +' SET BCTGDen=' + '''' + CONVERT(nvarchar(30),@TGDen) + '''' + ',BCCuaDen=' + CONVERT(nvarchar(30),@Cuaden) +',BCTGVe=' +  '''' + CONVERT(nvarchar(30),@TGVe) + '''' + ',BCCuaVe=' + CONVERT(nvarchar(30),@CuaVe)   
	SET @SQL2=    CASE WHEN @TGRa IS NULL THEN ',BCTGRa=Null' ELSE ',BCTGRa=' +  '''' + CONVERT(nvarchar(30),@TGRa) + '''' end   + ',BCCuaRa=' + CONVERT(nvarchar(30),@CuaRa) +  CASE WHEN @TGVao IS NULL THEN ',BCTGVao=null' ELSE  ',BCTGVao=' + '''' + CONVERT(nvarchar(30),@TGVao) + '''' end + ',BCCuaVao=' + CONVERT(nvarchar(30),@CuaVao)  
	SET @SQL3=',BCTGLamNgay=' + '''' + CONVERT(nvarchar(30),@TGLamN) + '''' + ',BCTGLamToi=' + '''' +  CONVERT(nvarchar(30),@TGLamD) + '''' + ',BCTGQuaGioNgay=' + '''' + CONVERT(nvarchar(30),@TGQuaN) + '''' + ',BCTGQuaGioToi=' + '''' +  CONVERT(nvarchar(30),@TGQuaD) + '''' + ',BCTGThemNgay=' + '''' +  CONVERT(nvarchar(30),@TGThemN) + '''' + ',BCTGThemToi=' + '''' + CONVERT(nvarchar(30),@TGThemD) + '''' + ',BCTGRaNgoaiNgay=' + CONVERT(nvarchar(30),@TGRaNgoaiN) + ',BCTGRaNgoaiToi=' +  CONVERT(nvarchar(30),@TGRaNgoaiD) 
	SET @SQL4=',BCTGDiMuonNgay=' + CONVERT(nvarchar(30),@TGMuonN) + ',BCTGDiMuonToi=' + CONVERT(nvarchar(30),@TGMuonD) + ',BCTGVeSomNgay=' + CONVERT(nvarchar(30),@TGSomN) + ',BCTGVeSomToi=' +  CONVERT(nvarchar(30),@TGSomD) + ',BCTGQuyDinh=' + CONVERT(nvarchar(30),@TGQD1) + ' WHERE (BCMaNV=' +  CONVERT(nvarchar(30),@StaffID) + ') AND (BCNgay='+ '''' + CONVERT(nvarchar(30),@D) + '''' + ')'

    EXEC(@SQL1+@SQL2+@SQL3+@SQL4)

--DROP TABLE tblBaocaotam
EndThisSub:
--DROP TABLE tblLocdulieutam

end

GO
