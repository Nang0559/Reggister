USE [HRM]
GO
/****** Object:  Table [dbo].[tblNhanVien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhanVien](
	[NVMaNV] [nvarchar](10) NOT NULL,
	[NVHo] [nvarchar](40) NULL,
	[NVHoDem] [nchar](10) NULL,
	[NVTen] [nvarchar](40) NULL,
	[NVHoTen] [nchar](50) NOT NULL,
	[NVBiDanh] [nvarchar](40) NOT NULL,
	[NVMaBP] [int] NOT NULL,
	[NVMaCV] [varchar](50) NOT NULL,
	[NVMaCa] [int] NOT NULL,
	[NVGioiTinh] [bit] NOT NULL,
	[NVNgaySinh] [datetime] NULL,
	[NVAnh] [image] NULL,
	[NVNgayTV] [datetime] NOT NULL,
	[NVNgayChinhThuc] [datetime] NULL,
	[NVNgayVao] [datetime] NULL,
	[NVNgayRa] [datetime] NULL,
	[NVUuTien] [smallint] NOT NULL,
	[NVMaQuocTich] [nvarchar](15) NOT NULL,
	[NVMaTonGiao] [nvarchar](15) NOT NULL,
	[NVMaDanToc] [nvarchar](15) NOT NULL,
	[NVNoiOHT] [nvarchar](255) NOT NULL,
	[NVNoiSinh] [nvarchar](255) NOT NULL,
	[NVNguyenQuan] [nvarchar](255) NOT NULL,
	[NVNoiThuongTru] [nvarchar](255) NOT NULL,
	[NVSoCMTND] [nvarchar](15) NOT NULL,
	[NVNgayCapCMTND] [datetime] NULL,
	[NVNoiCapCMTND] [nvarchar](150) NOT NULL,
	[NVDienThoai] [nvarchar](35) NOT NULL,
	[NVSoFax] [nvarchar](22) NOT NULL,
	[NVEmail] [nvarchar](100) NOT NULL,
	[NVMayNhanTin] [nvarchar](12) NOT NULL,
	[NVWebsite] [nvarchar](100) NOT NULL,
	[NVNgayKNDoan] [datetime] NULL,
	[NVNoiKNDoan] [nvarchar](200) NOT NULL,
	[NVNgayKNDang] [datetime] NULL,
	[NVNoiKNDang] [nvarchar](200) NOT NULL,
	[NVNguoiNN] [bit] NOT NULL,
	[NVSoTaiKhoan] [nvarchar](20) NOT NULL,
	[NVMaNganHang] [int] NOT NULL,
	[NVHinhThucTV] [nvarchar](50) NOT NULL,
	[NVLyDoTV] [nvarchar](100) NOT NULL,
	[NVTinhLamThem] [bit] NOT NULL,
	[NVPhepTon] [numeric](18, 0) NULL,
	[NVSoNgayPhep] [int] NULL,
	[NVCongChuan] [float] NULL,
	[NVSonguoiGTGC] [int] NULL,
	[NVLamca] [smallint] NULL,
	[NVDangVien] [bit] NULL,
	[NVNgayVaoDang] [smalldatetime] NULL,
	[NVNgayCTDang] [smalldatetime] NULL,
	[NVNoiKetNapDang] [nvarchar](100) NULL,
	[NVNoiSinhHoatDang] [nvarchar](100) NULL,
	[NVDoanVien] [bit] NULL,
	[NVNgayVaoDoan] [smalldatetime] NULL,
	[NVNoiKepNapDoan] [nvarchar](100) NULL,
	[NVHocVan1] [varchar](25) NULL,
	[NVNamHV1] [varchar](10) NULL,
	[NVTruongDT1] [varchar](25) NULL,
	[NVHocVi1] [varchar](25) NULL,
	[NVHe_DT1] [varchar](25) NULL,
	[NVCNganh1] [varchar](25) NULL,
	[NVNuocDT1] [varchar](25) NULL,
	[NVHocVan2] [varchar](25) NULL,
	[NVNamHV2] [varchar](10) NULL,
	[NVTruongDT2] [varchar](25) NULL,
	[NVHocVi2] [varchar](10) NULL,
	[NVHe_DT2] [varchar](10) NULL,
	[NVCNganh2] [varchar](10) NULL,
	[NVNuocDT2] [varchar](10) NULL,
	[NVHocVan3] [varchar](25) NULL,
	[NVNamHV3] [varchar](10) NULL,
	[NVTruongDT3] [varchar](25) NULL,
	[NVHocVi3] [varchar](10) NULL,
	[NVHe_DT3] [varchar](10) NULL,
	[NVCNganh3] [varchar](10) NULL,
	[NVNuocDT3] [varchar](10) NULL,
	[NVTDNgoaiNgu] [varchar](10) NULL,
	[NVTDVitinh] [varchar](10) NULL,
	[NVVHPhothong] [varchar](10) NULL,
	[NVBangcapkhac] [nvarchar](200) NULL,
	[NVMaSoThue] [varchar](20) NULL,
	[NVNgayCapMaSoThue] [smalldatetime] NULL,
	[NVChieucao] [nvarchar](50) NULL,
	[NVSocon] [nvarchar](50) NULL,
	[NVCanNang] [nvarchar](50) NULL,
	[NVTPBT] [varchar](10) NULL,
	[NVTPGD] [varchar](10) NULL,
	[NVTTHonnhan] [varchar](10) NULL,
	[NVNVQS] [bit] NULL,
	[NVGDLS] [bit] NULL,
	[NVThuongbenhBinh] [varchar](10) NULL,
	[NVNguoiLienhe] [nvarchar](50) NULL,
	[NVNguoilienheDC] [nvarchar](400) NULL,
	[NVNguoilienheDT] [varchar](20) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[NVDoiTuong] [nvarchar](50) NULL,
	[NVKhuvucLamviec] [nvarchar](50) NULL,
	[NVPhuongTienDilai] [nvarchar](50) NULL,
	[NVLoaiTinhLuong] [varchar](50) NULL,
	[NVMa] [int] NOT NULL,
	[NVNgayTinhPhep] [datetime] NULL,
	[NVLoaiQuet] [varchar](2) NULL,
	[NVNguoiPhuThuoc] [float] NULL,
	[NVTuDungDo] [int] NULL,
	[NVLichTrinhCa] [varchar](50) NULL,
	[NVLichTrinhVaoRa] [varchar](50) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[NVMaCVBH] [varchar](50) NULL,
	[NVTDNgoaiNgu1] [varchar](50) NULL,
	[NVGhiChuTrinhDo] [nvarchar](500) NULL,
	[NVNgoaiNgu1] [varchar](50) NULL,
	[NVNgoaiNgu] [varchar](50) NULL,
	[NVVitinh] [varchar](50) NULL,
	[NVLoaiNghiViec] [nvarchar](50) NULL,
	[NVEmailCaNhan] [nvarchar](50) NULL,
	[NVTruongBoPhan] [nvarchar](50) NULL,
	[NVNgayKTTraLuong] [datetime] NULL,
	[NVTrinhDo] [nvarchar](2000) NULL,
	[NVDT_NguoiThan] [nvarchar](500) NULL,
	[NVNoiOHTE] [nvarchar](max) NULL,
	[NVNoiThuongTruE] [nvarchar](max) NULL,
	[NVNguyenQuanE] [nvarchar](max) NULL,
	[NVNoiSinhE] [nvarchar](max) NULL,
	[NVMauPhieuLuongExcel] [nvarchar](550) NULL,
	[NVMauPhieuLuongWord] [nvarchar](550) NULL,
 CONSTRAINT [PK_tblNhanVien] PRIMARY KEY CLUSTERED 
(
	[NVMaNV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_LoaiHD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_LoaiHD](
	[ldhMa] [varchar](50) NOT NULL,
	[lhdTen] [nvarchar](250) NOT NULL,
	[ldhGiatriTinh] [varchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[Nhom] [nvarchar](40) NULL,
	[FileName] [nvarchar](100) NULL,
	[lhdTenE] [nvarchar](400) NULL,
	[SoThang] [int] NULL,
 CONSTRAINT [PK_NS_LoaiHD] PRIMARY KEY CLUSTERED 
(
	[ldhMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBoPhan]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBoPhan](
	[BPMa] [int] NOT NULL,
	[BPTen] [nvarchar](70) NOT NULL,
	[BPMaCha] [int] NOT NULL,
	[BPUuTien] [float] NOT NULL,
	[BPHienThiBC] [bit] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_tblBoPhan] PRIMARY KEY CLUSTERED 
(
	[BPMa] ASC,
	[BPTen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblChucVu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblChucVu](
	[CVMa] [varchar](10) NOT NULL,
	[CVTen] [nvarchar](70) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[CVNhom] [varchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_QuyetDinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_QuyetDinh](
	[NS_QuyetDinh] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[LoaiQD] [varchar](50) NOT NULL,
	[SoQD] [varchar](30) NULL,
	[MaNV] [int] NULL,
	[MaBP] [int] NULL,
	[NgayQD] [datetime] NULL,
	[NgayHieuLuc] [datetime] NULL,
	[ChucVu] [varchar](10) NULL,
	[TuNgay] [datetime] NULL,
	[DenNgay] [datetime] NULL,
	[TuNgay1] [datetime] NULL,
	[DenNgay1] [datetime] NULL,
	[Ngayky] [datetime] NULL,
	[NgayThanhLy] [datetime] NULL,
	[NgachCC] [varchar](20) NULL,
	[BacLuong] [smallint] NULL,
	[HSCD] [decimal](5, 2) NULL,
	[ChucVuKiemNhiem] [varchar](100) NULL,
	[CNganh] [varchar](10) NULL,
	[GhiChu] [nvarchar](max) NULL,
	[TruongDT] [varchar](10) NULL,
	[HeDT] [varchar](10) NULL,
	[QuocGia] [varchar](10) NULL,
	[ChungChi] [varchar](10) NULL,
	[LoaiPhi] [varchar](10) NULL,
	[NoiDung] [text] NULL,
	[NguoiQuyetDinh] [varchar](50) NULL,
	[HinhThuc] [varchar](10) NULL,
	[Cap] [varchar](10) NULL,
	[MucLuong] [money] NULL,
	[TapSu] [bit] NULL,
	[TapSu_Thang] [tinyint] NULL,
	[LyDo] [nvarchar](200) NULL,
	[DonVi_Cu] [varchar](10) NULL,
	[PhongBan_Cu] [varchar](10) NULL,
	[ChucVu_Cu] [varchar](10) NULL,
	[ThoiHanBoNhiem] [numeric](5, 2) NULL,
	[SQLUndo] [text] NOT NULL,
	[BaoHiem] [bit] NOT NULL,
	[BH_Thang] [tinyint] NULL,
	[BH_Nam] [smallint] NULL,
	[BH_Tang] [bit] NOT NULL,
	[NgachCC_Cu] [varchar](20) NULL,
	[BacLuong_Cu] [tinyint] NULL,
	[HSL_Cu] [numeric](5, 3) NULL,
	[HSCD_Cu] [numeric](4, 2) NULL,
	[MucLuong_Cu] [money] NULL,
	[SoHD] [varchar](50) NULL,
	[LoaiHDLD] [varchar](20) NULL,
	[DanhHieu] [varchar](10) NULL,
	[SoTien] [money] NULL,
	[BangChu_SoTien] [nvarchar](100) NULL,
	[BH_GhiChu] [nvarchar](100) NULL,
	[NguoiDeNghi] [nvarchar](250) NULL,
	[TrachNhiem] [nvarchar](max) NULL,
	[QuyenHan] [nvarchar](max) NULL,
	[QuyenLoi] [nvarchar](max) NULL,
	[CanBoTheoDoi] [varchar](150) NULL,
	[NgheNghiep] [varchar](10) NULL,
	[CongViec] [nvarchar](max) NULL,
	[LuongThuViec] [money] NULL,
	[Bangchu_MucLuong] [nvarchar](200) NULL,
	[BangChu_LuongThuViec] [nvarchar](200) NULL,
	[TenBang] [nvarchar](200) NULL,
	[SoBang] [varchar](50) NULL,
	[NoiCapBang] [nvarchar](200) NULL,
	[NgayCapBang] [smalldatetime] NULL,
	[HDLD_Cu] [varchar](20) NULL,
	[HDLD_So_Cu] [varchar](20) NULL,
	[HDLD_NgayKy_Cu] [smalldatetime] NULL,
	[HDLD_DenNgay_Cu] [smalldatetime] NULL,
	[HDLD_NgayHieuLuc_Cu] [smalldatetime] NULL,
	[LuongCoBan] [money] NULL,
	[LuongBaoHiem] [money] NULL,
	[BangChu_LuongBaoHiem] [nvarchar](200) NULL,
	[LuongBaoHiem_Cu] [money] NULL,
	[TienTheChap] [money] NULL,
	[BangChu_TienTheChap] [nvarchar](150) NULL,
	[MaTuyenDung] [varchar](30) NULL,
	[ToCongTac_Cu] [varchar](15) NULL,
	[KhoaHoc] [varchar](10) NULL,
	[KetQuaDaoTao] [varchar](10) NULL,
	[HsThuong] [numeric](5, 2) NULL,
	[HsThuong_Cu] [numeric](5, 2) NULL,
	[ThayDoiLuong] [bit] NOT NULL,
	[ThayDoiLuongCongViec] [bit] NOT NULL,
	[NgachCongViec] [varchar](20) NULL,
	[BacCongViec] [tinyint] NULL,
	[NgayNangLuongCoBan] [smalldatetime] NULL,
	[NgayNangLuongCongViec] [smalldatetime] NULL,
	[DiaDiemLamViec] [varchar](20) NULL,
	[NgachCongViec_Cu] [varchar](20) NULL,
	[BacCongViec_Cu] [tinyint] NULL,
	[NgayThi] [smalldatetime] NULL,
	[DotThi] [tinyint] NULL,
	[MaLopHoc] [varchar](50) NULL,
	[DiaDiem] [nvarchar](100) NULL,
	[DonViToChuc] [nvarchar](100) NULL,
	[ThoiGian] [nvarchar](100) NULL,
	[BH_LoaiBH] [varchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[HDLDTangBH] [bit] NULL,
	[DongBHXH] [bit] NULL,
	[DongBHYT] [bit] NULL,
	[DongBHTN] [bit] NULL,
	[DongCD] [bit] NULL,
	[PCDilai] [numeric](18, 0) NULL,
	[PCDilai_Cu] [numeric](18, 0) NULL,
	[PCNhaO] [numeric](18, 0) NULL,
	[PCNhaO_Cu] [numeric](18, 0) NULL,
	[PCKhac] [numeric](18, 0) NULL,
	[PCKhac_Cu] [numeric](18, 0) NULL,
	[TCDienThoai] [numeric](18, 0) NULL,
	[TCDienThoai_Cu] [numeric](18, 0) NULL,
	[PCNN] [numeric](18, 0) NULL,
	[PCNN_Cu] [numeric](18, 0) NULL,
	[PCConnho] [numeric](18, 0) NULL,
	[PCConnho_Cu] [numeric](18, 0) NULL,
	[PCThamnien] [numeric](18, 0) NULL,
	[PCThamnien_Cu] [numeric](18, 0) NULL,
	[HTCongChuan] [numeric](18, 0) NULL,
	[HTCongChuan_Cu] [numeric](18, 0) NULL,
	[TCDocHai] [numeric](18, 0) NULL,
	[TCDocHai_Cu] [numeric](18, 0) NULL,
	[LuongCoBan_Cu] [money] NULL,
	[Pcngoaingu] [numeric](18, 0) NULL,
	[Pcngoaingu_Cu] [numeric](18, 0) NULL,
	[Pctrachnhiem] [numeric](18, 0) NULL,
	[Pctrachnhiem_Cu] [numeric](18, 0) NULL,
	[Pckynang] [numeric](18, 0) NULL,
	[Pckynang_Cu] [numeric](18, 0) NULL,
	[Pccadem] [numeric](18, 0) NULL,
	[Pccadem_Cu] [numeric](18, 0) NULL,
	[Pcnangnhoc] [numeric](18, 0) NULL,
	[Pcnangnhoc_Cu] [numeric](18, 0) NULL,
	[Pcnghenghiep] [numeric](18, 0) NULL,
	[Pcnghenghiep_Cu] [numeric](18, 0) NULL,
	[Pcchuyencan] [numeric](18, 0) NULL,
	[Pcchuyencan_Cu] [numeric](18, 0) NULL,
	[Pcdienthoai] [numeric](18, 0) NULL,
	[Pcdienthoai_Cu] [numeric](18, 0) NULL,
	[Pcbanatld] [numeric](18, 0) NULL,
	[Pcbanatld_Cu] [numeric](18, 0) NULL,
	[Pcbaoduong] [numeric](18, 0) NULL,
	[Pcbaoduong_Cu] [numeric](18, 0) NULL,
	[BHLDNVietTat] [nvarchar](5) NULL,
	[BHLuongTB] [numeric](18, 0) NULL,
	[BHSoSoBHXH] [varchar](30) NULL,
	[BHSoNgayNghiTrongKy] [int] NULL,
	[BHSoNgayNghiLuyKe] [int] NULL,
	[BHSoTien] [numeric](18, 0) NULL,
	[BHSoNamDong] [int] NULL,
	[BHSoThangDong] [int] NULL,
	[BHDieuKienHuong] [nvarchar](200) NULL,
	[BHDaXacNhan] [bit] NULL,
	[BH_DotXetDuyet] [nvarchar](100) NULL,
	[LuongBaoHiem_DC] [money] NULL,
	[BHSoNgayNghiLuyKe_DC] [int] NULL,
	[BHXacNhanDC] [bit] NULL,
	[BHSoNgayNghiTrongKy_DC] [int] NULL,
	[BH_DC] [bit] NULL,
	[BHGhiChu_DC] [nvarchar](255) NULL,
	[BHSoTien_DC] [numeric](18, 0) NULL,
	[DenNgay_DC] [smalldatetime] NULL,
	[TuNgay_DC] [smalldatetime] NULL,
	[BH_Nam_DC] [int] NULL,
	[BH_Thang_DC] [int] NULL,
	[BHThoiDiem] [smalldatetime] NULL,
	[BHNgaySinhCon] [smalldatetime] NULL,
	[BHTenCon] [nvarchar](255) NULL,
	[BH_SuaTay] [bit] NULL,
	[BHTGNghi] [float] NULL,
	[LoaiNghiViec] [varchar](50) NULL,
	[SoPLHD] [nvarchar](100) NULL,
	[KetQuaDT] [varchar](50) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[NS_QD_HDLD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create view [dbo].[NS_QD_HDLD] as SELECT  TOP 100 PERCENT NVMa,
       NVMaNV,
       tblNhanvien.NVHoTen,
       UPPER(NVHoTen) AS NVHoTen_UPPER,
       tblNhanvien.NVGioiTinh,
       tblChucVu.CVTen,
       tblNhanvien.NVNgaySinh,
       tblNhanvien.NVNgayTV,
       tblNhanvien.NVNgayChinhThuc,
       tblNhanvien.NVNgayVao,
       tblNhanvien.NVNgayRa,
       tblNhanvien.NVNoiOHT,
       tblNhanvien.NVNoiSinh,
       tblNhanvien.NVNguyenQuan,
       tblNhanvien.NVNoiThuongTru,
       tblNhanvien.NVSoCMTND,
       tblNhanvien.NVNgayCapCMTND,
       tblNhanvien.NVNoiCapCMTND,
       NS_QuyetDinh.SoQD AS HDMa,
       NS_LoaiHD.lhdTen AS HDLoaiHD,
       NS_QuyetDinh.TuNgay AS HDTuNgay,
       NS_QuyetDinh.DenNgay AS HDDenNgay,
       NS_QuyetDinh.Ngayky AS HDNgayKy,
       NS_QuyetDinh.LuongThuViec AS HDLuongTV,
       NS_QuyetDinh.LuongCoBan AS HDLuongCB,
       dbo.fuDocSoThanhChu(NS_QuyetDinh.LuongCoBan) AS HDLuongCB_bangchu,
       NS_QuyetDinh.ChucVu AS HDChucVu,
       NS_QuyetDinh.NgayThanhLy AS HDNgayThanhly,
       NS_QuyetDinh.LuongBaoHiem AS HDLuongBH,
       NS_QuyetDinh.GhiChu AS HDGhiChu,
       NS_QuyetDinh.MaBP AS HDBoPhan,
       NS_QuyetDinh.NguoiQuyetDinh AS HDNguoiKy,
       NS_QuyetDinh.CongViec AS HDCongViec,
       NS_QuyetDinh.LoaiQD AS HDLoaiQD,
       NS_QuyetDinh.DongBHYT AS HDDongBHYT,
       NS_QuyetDinh.DongBHXH AS HDDongBHXH,
       NS_QuyetDinh.DongCD AS HDDongCongDoan,
       NS_QuyetDinh.HDLDTangBH AS HDChinhthucDT,
       ISNULL(NS_QuyetDinh.PCDilai, 0) AS PCDilai,
       ISNULL(NS_QuyetDinh.PCNhaO, 0) AS PCNhaO,
       ISNULL(NS_QuyetDinh.PCKhac, 0) AS PCKhac,
       ISNULL(NS_QuyetDinh.TCDienThoai, 0) AS TCDienThoai,
       ISNULL(NS_QuyetDinh.PCNN, 0) AS PCNN,
       ISNULL(NS_QuyetDinh.PCConnho, 0) AS PCConnho,
       ISNULL(NS_QuyetDinh.PCThamnien, 0) AS PCThamnien
FROM   dbo.tblNhanVien
       INNER JOIN tblBoPhan
            ON  tblNhanvien.NVMaBP = tblBoPhan.BPMa
       LEFT JOIN tblChucVu ON tblNhanVien.NVMaCV=tblChucVu.CVMa
       INNER JOIN NS_QuyetDinh
            ON  tblNhanvien.NVMa = NS_QuyetDinh.MaNV
            LEFT JOIN NS_LoaiHD ON  NS_QuyetDinh.LoaiHDLD=NS_LoaiHD.ldhGiatriTinh
WHERE  (1 = 1) AND NS_QuyetDinh.LoaiQD='HDLD'
GO
/****** Object:  Table [dbo].[tblHopDongNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHopDongNV](
	[HDMa] [varchar](50) NOT NULL,
	[HDMaNV] [int] NOT NULL,
	[HDLoaiHD] [varchar](50) NOT NULL,
	[HDTuNgay] [datetime] NOT NULL,
	[HDDenNgay] [datetime] NOT NULL,
	[HDNgayKy] [datetime] NOT NULL,
	[HDLuongCB] [numeric](18, 0) NOT NULL,
	[HDChucVu] [varchar](50) NULL,
	[HDNgayThanhly] [datetime] NULL,
	[HDLuongBH] [numeric](18, 0) NOT NULL,
	[HDLuongTV] [numeric](18, 0) NULL,
	[HDGhiChu] [nvarchar](250) NULL,
	[HDDonVi] [varchar](50) NULL,
	[HDBoPhan] [varchar](50) NULL,
	[HDNguoiKy] [nvarchar](50) NULL,
	[HDCongViec] [nvarchar](max) NULL,
	[HDLoaiQD] [varchar](50) NULL,
	[HDDongBHYT] [bit] NULL,
	[HDDongBHXH] [bit] NULL,
	[HDDongCongDoan] [bit] NULL,
	[HDChinhthucDT] [bit] NULL,
	[HDNgayTLy] [datetime] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHopDongNV_Phuluc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHopDongNV_Phuluc](
	[HDPLMa] [varchar](50) NOT NULL,
	[HDMa] [varchar](50) NOT NULL,
	[PLTuNgay] [datetime] NOT NULL,
	[PLDenNgay] [datetime] NOT NULL,
	[PLNgayKy] [datetime] NOT NULL,
	[PLLuongCB] [numeric](18, 0) NOT NULL,
	[PLLuongBH] [numeric](18, 0) NOT NULL,
	[PLChucVu] [varchar](50) NULL,
	[PLNgayThanhly] [datetime] NULL,
	[PLPCChucVu] [numeric](18, 0) NULL,
	[PLDongBHXH] [bit] NULL,
	[PLDongCongDoan] [bit] NULL,
	[PLDongBHYT] [bit] NULL,
	[PLGhiChu] [nvarchar](50) NULL,
	[PLLuongTV] [numeric](18, 0) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_tblHopDongNV_Phuluc_1] PRIMARY KEY CLUSTERED 
(
	[HDPLMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHopdongNV_PhuLuc_PhuCap]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHopdongNV_PhuLuc_PhuCap](
	[HDPLPCMa] [nvarchar](50) NOT NULL,
	[HDPLMa] [nvarchar](50) NOT NULL,
	[PCMa] [nvarchar](50) NOT NULL,
	[HDPLPCGiaTri] [numeric](18, 0) NULL,
 CONSTRAINT [PK_tblHopdongNV_PhuLuc_PhuCap] PRIMARY KEY CLUSTERED 
(
	[HDPLPCMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[VPhuLuc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW [dbo].[VPhuLuc] as 
SELECT ll.HDPLMa,[PC0001] as PCDilai,[PC0002] as PCNhaO,[PC0003] as Pcngoaingu,[PC0004] as Pctrachnhiem,[PC0005] as Pcnghenghiep,[PC0007] as Pccadem,[PC0010] as Pcnangnhoc,[PC0011] as Pcchuyencan,[PC0012] as Pcdienthoai,[PC0013] as Pcbanatld,[PC0014] as Pcbaoduong
           FROM   tblHopDongNV_Phuluc ll
                  LEFT JOIN (
                           SELECT *
                           FROM   (
									   SELECT thnpc.PCMa,
											  thnpc.HDPLMa,
											  thnpc.HDPLPCGiaTri
									   FROM   tblHopdongNV_PhuLuc_PhuCap thnpc
                                  ) up PIVOT(
                                      SUM(HDPLPCGiaTri) FOR PCMa IN ([PC0001],[PC0002],[PC0003],[PC0004],[PC0005],[PC0007],[PC0010],[PC0011],[PC0012],[PC0013],[PC0014])
                                  ) AS PIVOT01
                       ) 
						plpivot
						ON  ll.HDPLMa = plpivot.HDPLMa
GO
/****** Object:  View [dbo].[NS_QD_PLHDLD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[NS_QD_PLHDLD] AS 
SELECT TOP 100 PERCENT 
       tblNhanvien.NVMa,
       tblNhanvien.NVMaNV,
       tblNhanvien.NVHoTen,
       UPPER(tblNhanvien.NVHoTen) AS NVHoTen_UPPER,
       tblNhanvien.NVGioiTinh,
       tblNhanvien.NVNgaySinh,
       tblNhanvien.NVNgayTV,
       tblNhanvien.NVNgayChinhThuc,
       tblNhanvien.NVNgayVao,
       tblNhanvien.NVNgayRa,
       tblNhanvien.NVNoiOHT,
       tblNhanvien.NVNoiSinh,
       tblNhanvien.NVNguyenQuan,
       tblNhanvien.NVNoiThuongTru,
       tblNhanvien.NVSoCMTND,
       tblNhanvien.NVNgayCapCMTND,
       tblNhanvien.NVNoiCapCMTND,
       tblHopDongNV.HDMa AS HDMa01,
       tblHopDongNV.HDNgayKy,
       tblHopDongNV_Phuluc.*,
       dbo.fuDocSoThanhChu(tblHopDongNV_Phuluc.PLLuongCB) AS PLLuongCB_bangchu
		 ,[PCDilai]
		  ,[PCNhaO]
		  ,[PCKhac]
		  ,[TCDienThoai]
		  ,[PCNN]
		  ,[PCConnho]
		  ,[PCThamnien]
		  ,[HTCongChuan]
		  ,[TCDocHai]
FROM   dbo.tblNhanvien
       INNER JOIN tblBoPhan
            ON  tblNhanvien.NVMaBP = tblBoPhan.BPMa
       INNER JOIN tblHopDongNV
            ON  dbo.tblNhanvien.NVMa = tblHopDongNV.HDMaNV
       INNER JOIN tblHopDongNV_Phuluc
            ON  tblHopDongNV.HDMa = tblHopDongNV_Phuluc.HDMa
                  INNER JOIN dbo.VPhuLuc 
            ON  tblHopDongNV_Phuluc.HDPLMa = VPhuLuc.HDPLMa   
              Where (1=1) --{sCondition}
GO
/****** Object:  Table [dbo].[tblBaoCao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCao](
	[BCNgay] [datetime] NOT NULL,
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
	[BCNgayNghiBu] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[DataStruct_ttx]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[DataStruct_ttx] as 
SELECT TOP 100 PERCENT tblNhanVien.NVMa,
		tblNhanVien.NVMaNV,
       tblNhanVien.NVHoTen,
       tblBoPhan.BPTen,
       tblChucVu.CVTen,
       tblBaoCao.BCNgay,
       (Case when Convert(varchar, tblBaoCao.BCTGDen,108)='00:00:00' THEN NULL ELSE tblBaoCao.BCTGDen END)  AS TGVao,
       (Case when Convert(varchar, tblBaoCao.BCTGVe,108)='00:00:00' THEN NULL ELSE tblBaoCao.BCTGVe END)  AS TGRa,
       tblBaoCao.BCTGLamNgay,
       tblBaoCao.BCTGLamToi,
       (
           ISNULL(tblBaoCao.BCTGDiMuonNgay, 0) + ISNULL(tblBaoCao.BCTGDiMuonToi, 0)
       ) AS BCTGDimuon,
       (
           ISNULL(tblBaoCao.BCTGVeSomNgay, 0) + ISNULL(tblBaoCao.BCTGDiMuonToi, 0)
       ) AS BCTGVeSom,
       tblBaoCao.BCTGQuyDinh,
       tblBaoCao.BCTGThemNgay,
       tblBaoCao.BCTGThemToi,
       tblBaoCao.BCLoaiNgayNghi,
       (Case when tblBaoCao.BCLoai=1 THEN '' ELSE '+' END) AS BCLoai,
       tblBaoCao.BCLoaiNgayNghi AS NgayCN,
       tblBaoCao.BCGhiChu,
       0 AS CongNgay_CT,
       0 AS CongNgay_TV,
       0 AS CongDem_CT,
       0 AS CongDem_TV,
       0 AS ThemNgay_CT,
       0 AS ThemNgay_TV,
       0 AS ThemDem_CT,
       0 AS ThemDem_TV,
       0 AS CNNgay,
       0 AS CNDem,
       0 AS LeNgay,
       0 AS LeDem,
       tblNhanVien.NVNgayVao AS NgayvaoCty,
       tblNhanVien.NVNgayRa AS NgayraCty
       
       FROM tblBaoCao
       INNER JOIN tblNhanVien
       ON tblBaoCao.BCMaNV=tblNhanVien.NVMa
       INNER JOIN tblBoPhan
            ON  tblNhanVien.NVMaBP = tblBoPhan.BPMa
       LEFT JOIN tblChucVu
            ON  tblNhanVien.NVMaCV = tblChucVu.CVMa   
WHERE (1=1) 
ORDER BY tblBoPhan.BPUuTien,tblNhanVien.NVMaNV, tblBaoCao.BCNgay
GO
/****** Object:  Table [dbo].[tblCapThe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCapThe](
	[CTMaNV] [int] NOT NULL,
	[CTMaThe] [nvarchar](14) NOT NULL,
	[CTNgayApDung] [datetime] NOT NULL,
	[CTNgayKetThuc] [datetime] NULL,
	[CTGhichu] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[CTMaTheNew] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblCapThe_1] PRIMARY KEY CLUSTERED 
(
	[CTMaNV] ASC,
	[CTMaThe] ASC,
	[CTNgayApDung] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataNew]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataNew](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[Remark] [nvarchar](500) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[EmployeeAttendanceView]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[EmployeeAttendanceView] AS
SELECT 
    nv.NVMaNV AS EmployeeID,
    nv.NVHoTen AS FullName,
    nv.NVMaBP AS Department,
    CONVERT(DATE, rd.ThoiGian) AS Date,
    MIN(rd.ThoiGian) AS CheckInTime,
    MAX(rd.ThoiGian) AS CheckOutTime
FROM 
    HRM.dbo.RecordDataNew rd
    INNER JOIN HRM.dbo.tblCapThe ct ON rd.IDCard = ct.CTMaThe
    INNER JOIN HRM.dbo.tblNhanVien nv ON ct.CTMaNV = nv.NVMa
WHERE 
    ct.CTNgayApDung = (
        SELECT MAX(ct2.CTNgayApDung)
        FROM HRM.dbo.tblCapThe ct2
        WHERE ct2.CTMaNV = ct.CTMaNV
    )
GROUP BY 
    nv.NVMaNV, nv.NVHoTen, nv.NVMaBP, CONVERT(DATE, rd.ThoiGian)
GO
/****** Object:  Table [dbo].[RecordData]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[view_ChangeShift]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[view_ChangeShift]
AS
SELECT     IDM, IDCard, ThoiGian, Status, HandData, Pass
FROM         dbo.RecordData
GO
/****** Object:  Table [dbo].[tblCa]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCa](
	[CMa] [nvarchar](7) NOT NULL,
	[CTen] [nvarchar](40) NOT NULL,
	[CVietTat] [nvarchar](5) NOT NULL,
	[CTGBatDau] [datetime] NOT NULL,
	[CTGBDNghi1] [datetime] NOT NULL,
	[CTGKTNghi1] [datetime] NOT NULL,
	[CTGBDNghi2] [datetime] NOT NULL,
	[CTGKTNghi2] [datetime] NOT NULL,
	[CTGBDNghi3] [datetime] NOT NULL,
	[CTGKTNghi3] [datetime] NOT NULL,
	[CTGKetThuc] [datetime] NOT NULL,
	[CTGTinhDimuon] [datetime] NULL,
	[CBDTinhLTTC] [smallint] NOT NULL,
	[CBDTinhLT] [smallint] NOT NULL,
	[CTGNghiGiuaGio] [smallint] NOT NULL,
	[CTGQDD] [smallint] NOT NULL,
	[CTGQDC] [smallint] NOT NULL,
	[CDuocRaNgoai] [bit] NOT NULL,
	[CTinhVaoSom] [bit] NOT NULL,
	[CBuTGLam] [bit] NOT NULL,
	[CCongNghiGiuaCa] [bit] NOT NULL,
	[CNguongLamThem] [int] NOT NULL,
	[CNguongLamThemTC] [tinyint] NOT NULL,
	[CDonViLamThem] [tinyint] NOT NULL,
	[CNguongDiMuon] [tinyint] NOT NULL,
	[CNguongVeSom] [tinyint] NOT NULL,
	[CQuetTruocCa] [smallint] NOT NULL,
	[CQuetSauCa] [smallint] NOT NULL,
	[CDonViChamCong] [int] NOT NULL,
	[CChuNhatLambt] [bit] NOT NULL,
	[CNgayLeLambt] [bit] NOT NULL,
	[CLoaiCa] [tinyint] NOT NULL,
	[CDSBoPhan] [nvarchar](1000) NOT NULL,
	[CNgaynghi] [tinyint] NOT NULL,
	[CKhongtinhVangMat] [bit] NOT NULL,
	[CLichtrinhVaora] [tinyint] NOT NULL,
	[CChiaLTSauca] [bit] NOT NULL,
	[CCongvaotongcong] [bit] NULL,
	[CTGTinhVeSom] [datetime] NULL,
	[CNhomCa] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[viewHTCaTam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  VIEW [dbo].[viewHTCaTam]AS SELECT nv.NVMaNV,'1'=  Max(CASE BCNgay WHEN 'Feb  1 2011 12:00AM' THEN ca.CVietTat end),'2'=  Max(CASE BCNgay WHEN 'Feb  2 2011 12:00AM' THEN ca.CVietTat end),'3'=  Max(CASE BCNgay WHEN 'Feb  3 2011 12:00AM' THEN ca.CVietTat end),'4'=  Max(CASE BCNgay WHEN 'Feb  4 2011 12:00AM' THEN ca.CVietTat end),'5'=  Max(CASE BCNgay WHEN 'Feb  5 2011 12:00AM' THEN ca.CVietTat end),'6'=  Max(CASE BCNgay WHEN 'Feb  6 2011 12:00AM' THEN ca.CVietTat end),'7'=  Max(CASE BCNgay WHEN 'Feb  7 2011 12:00AM' THEN ca.CVietTat end),'8'=  Max(CASE BCNgay WHEN 'Feb  8 2011 12:00AM' THEN ca.CVietTat end),'9'=  Max(CASE BCNgay WHEN 'Feb  9 2011 12:00AM' THEN ca.CVietTat end),'10'=  Max(CASE BCNgay WHEN 'Feb 10 2011 12:00AM' THEN ca.CVietTat end),'11'=  Max(CASE BCNgay WHEN 'Feb 11 2011 12:00AM' THEN ca.CVietTat end),'12'=  Max(CASE BCNgay WHEN 'Feb 12 2011 12:00AM' THEN ca.CVietTat end),'13'=  Max(CASE BCNgay WHEN 'Feb 13 2011 12:00AM' THEN ca.CVietTat end),'14'=  Max(CASE BCNgay WHEN 'Feb 14 2011 12:00AM' THEN ca.CVietTat end),'15'=  Max(CASE BCNgay WHEN 'Feb 15 2011 12:00AM' THEN ca.CVietTat end),'16'=  Max(CASE BCNgay WHEN 'Feb 16 2011 12:00AM' THEN ca.CVietTat end),'17'=  Max(CASE BCNgay WHEN 'Feb 17 2011 12:00AM' THEN ca.CVietTat end),'18'=  Max(CASE BCNgay WHEN 'Feb 18 2011 12:00AM' THEN ca.CVietTat end),'19'=  Max(CASE BCNgay WHEN 'Feb 19 2011 12:00AM' THEN ca.CVietTat end),'20'=  Max(CASE BCNgay WHEN 'Feb 20 2011 12:00AM' THEN ca.CVietTat end),'21'=  Max(CASE BCNgay WHEN 'Feb 21 2011 12:00AM' THEN ca.CVietTat end),'22'=  Max(CASE BCNgay WHEN 'Feb 22 2011 12:00AM' THEN ca.CVietTat end),'23'=  Max(CASE BCNgay WHEN 'Feb 23 2011 12:00AM' THEN ca.CVietTat end),'24'=  Max(CASE BCNgay WHEN 'Feb 24 2011 12:00AM' THEN ca.CVietTat end),'25'=  Max(CASE BCNgay WHEN 'Feb 25 2011 12:00AM' THEN ca.CVietTat end),'26'=  Max(CASE BCNgay WHEN 'Feb 26 2011 12:00AM' THEN ca.CVietTat end),'27'=  Max(CASE BCNgay WHEN 'Feb 27 2011 12:00AM' THEN ca.CVietTat end),'28'=  Max(CASE BCNgay WHEN 'Feb 28 2011 12:00AM' THEN ca.CVietTat end),bc.bcmanv  FROM  tblNhanVien nv Left JOIN   tblBaoCao2011_02 bc ON bc.BCMaNV=nv.NVMa  LEFT JOIN tblCa ca ON bc.BCMaCa=ca.CMa  GROUP BY bc.BCMaNV,nv.NVMaNV 
GO
/****** Object:  View [dbo].[viewHTCaTam1]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  VIEW [dbo].[viewHTCaTam1]AS   SELECT nv.NVMaNV,'21'=  Max(CASE BCNgay WHEN 'Jan 21 2011 12:00AM' THEN ca.CVietTat end),'22'=  Max(CASE BCNgay WHEN 'Jan 22 2011 12:00AM' THEN ca.CVietTat end),'23'=  Max(CASE BCNgay WHEN 'Jan 23 2011 12:00AM' THEN ca.CVietTat end),'24'=  Max(CASE BCNgay WHEN 'Jan 24 2011 12:00AM' THEN ca.CVietTat end),'25'=  Max(CASE BCNgay WHEN 'Jan 25 2011 12:00AM' THEN ca.CVietTat end),'26'=  Max(CASE BCNgay WHEN 'Jan 26 2011 12:00AM' THEN ca.CVietTat end),'27'=  Max(CASE BCNgay WHEN 'Jan 27 2011 12:00AM' THEN ca.CVietTat end),'28'=  Max(CASE BCNgay WHEN 'Jan 28 2011 12:00AM' THEN ca.CVietTat end),'29'=  Max(CASE BCNgay WHEN 'Jan 29 2011 12:00AM' THEN ca.CVietTat end),'30'=  Max(CASE BCNgay WHEN 'Jan 30 2011 12:00AM' THEN ca.CVietTat end),'31'=  Max(CASE BCNgay WHEN 'Jan 31 2011 12:00AM' THEN ca.CVietTat end),'1'=  Max(CASE BCNgay WHEN 'Feb  1 2011 12:00AM' THEN ca.CVietTat end),'2'=  Max(CASE BCNgay WHEN 'Feb  2 2011 12:00AM' THEN ca.CVietTat end),'3'=  Max(CASE BCNgay WHEN 'Feb  3 2011 12:00AM' THEN ca.CVietTat end),'4'=  Max(CASE BCNgay WHEN 'Feb  4 2011 12:00AM' THEN ca.CVietTat end),'5'=  Max(CASE BCNgay WHEN 'Feb  5 2011 12:00AM' THEN ca.CVietTat end),'6'=  Max(CASE BCNgay WHEN 'Feb  6 2011 12:00AM' THEN ca.CVietTat end),'7'=  Max(CASE BCNgay WHEN 'Feb  7 2011 12:00AM' THEN ca.CVietTat end),'8'=  Max(CASE BCNgay WHEN 'Feb  8 2011 12:00AM' THEN ca.CVietTat end),'9'=  Max(CASE BCNgay WHEN 'Feb  9 2011 12:00AM' THEN ca.CVietTat end),'10'=  Max(CASE BCNgay WHEN 'Feb 10 2011 12:00AM' THEN ca.CVietTat end),'11'=  Max(CASE BCNgay WHEN 'Feb 11 2011 12:00AM' THEN ca.CVietTat end),'12'=  Max(CASE BCNgay WHEN 'Feb 12 2011 12:00AM' THEN ca.CVietTat end),'13'=  Max(CASE BCNgay WHEN 'Feb 13 2011 12:00AM' THEN ca.CVietTat end),'14'=  Max(CASE BCNgay WHEN 'Feb 14 2011 12:00AM' THEN ca.CVietTat end),'15'=  Max(CASE BCNgay WHEN 'Feb 15 2011 12:00AM' THEN ca.CVietTat end),'16'=  Max(CASE BCNgay WHEN 'Feb 16 2011 12:00AM' THEN ca.CVietTat end),'17'=  Max(CASE BCNgay WHEN 'Feb 17 2011 12:00AM' THEN ca.CVietTat end),'18'=  Max(CASE BCNgay WHEN 'Feb 18 2011 12:00AM' THEN ca.CVietTat end),'19'=  Max(CASE BCNgay WHEN 'Feb 19 2011 12:00AM' THEN ca.CVietTat end),'20'=  Max(CASE BCNgay WHEN 'Feb 20 2011 12:00AM' THEN ca.CVietTat end),bc.bcmanv  FROM  tblNhanVien nv Left JOIN   tblBaoCao2011_02 bc ON bc.BCMaNV=nv.NVMa  LEFT JOIN tblCa ca ON bc.BCMaCa=ca.CMa 
					 GROUP BY bc.BCMaNV,nv.NVMaNV 
GO
/****** Object:  View [dbo].[viewExportShift]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  VIEW [dbo].[viewExportShift]AS SELECT nv.NVMaNV,'1'=  Max(CASE BCNgay WHEN 'Jan  1 2010 12:00AM' THEN ca.CVietTat end),'2'=  Max(CASE BCNgay WHEN 'Jan  2 2010 12:00AM' THEN ca.CVietTat end),'3'=  Max(CASE BCNgay WHEN 'Jan  3 2010 12:00AM' THEN ca.CVietTat end),'4'=  Max(CASE BCNgay WHEN 'Jan  4 2010 12:00AM' THEN ca.CVietTat end),'5'=  Max(CASE BCNgay WHEN 'Jan  5 2010 12:00AM' THEN ca.CVietTat end),'6'=  Max(CASE BCNgay WHEN 'Jan  6 2010 12:00AM' THEN ca.CVietTat end),'7'=  Max(CASE BCNgay WHEN 'Jan  7 2010 12:00AM' THEN ca.CVietTat end),'8'=  Max(CASE BCNgay WHEN 'Jan  8 2010 12:00AM' THEN ca.CVietTat end),'9'=  Max(CASE BCNgay WHEN 'Jan  9 2010 12:00AM' THEN ca.CVietTat end),'10'=  Max(CASE BCNgay WHEN 'Jan 10 2010 12:00AM' THEN ca.CVietTat end),'11'=  Max(CASE BCNgay WHEN 'Jan 11 2010 12:00AM' THEN ca.CVietTat end),'12'=  Max(CASE BCNgay WHEN 'Jan 12 2010 12:00AM' THEN ca.CVietTat end),'13'=  Max(CASE BCNgay WHEN 'Jan 13 2010 12:00AM' THEN ca.CVietTat end),'14'=  Max(CASE BCNgay WHEN 'Jan 14 2010 12:00AM' THEN ca.CVietTat end),'15'=  Max(CASE BCNgay WHEN 'Jan 15 2010 12:00AM' THEN ca.CVietTat end),'16'=  Max(CASE BCNgay WHEN 'Jan 16 2010 12:00AM' THEN ca.CVietTat end),'17'=  Max(CASE BCNgay WHEN 'Jan 17 2010 12:00AM' THEN ca.CVietTat end),'18'=  Max(CASE BCNgay WHEN 'Jan 18 2010 12:00AM' THEN ca.CVietTat end),'19'=  Max(CASE BCNgay WHEN 'Jan 19 2010 12:00AM' THEN ca.CVietTat end),'20'=  Max(CASE BCNgay WHEN 'Jan 20 2010 12:00AM' THEN ca.CVietTat end),'21'=  Max(CASE BCNgay WHEN 'Jan 21 2010 12:00AM' THEN ca.CVietTat end),'22'=  Max(CASE BCNgay WHEN 'Jan 22 2010 12:00AM' THEN ca.CVietTat end),'23'=  Max(CASE BCNgay WHEN 'Jan 23 2010 12:00AM' THEN ca.CVietTat end),'24'=  Max(CASE BCNgay WHEN 'Jan 24 2010 12:00AM' THEN ca.CVietTat end),'25'=  Max(CASE BCNgay WHEN 'Jan 25 2010 12:00AM' THEN ca.CVietTat end),'26'=  Max(CASE BCNgay WHEN 'Jan 26 2010 12:00AM' THEN ca.CVietTat end),'27'=  Max(CASE BCNgay WHEN 'Jan 27 2010 12:00AM' THEN ca.CVietTat end),'28'=  Max(CASE BCNgay WHEN 'Jan 28 2010 12:00AM' THEN ca.CVietTat end),'29'=  Max(CASE BCNgay WHEN 'Jan 29 2010 12:00AM' THEN ca.CVietTat end),'30'=  Max(CASE BCNgay WHEN 'Jan 30 2010 12:00AM' THEN ca.CVietTat end),'31'=  Max(CASE BCNgay WHEN 'Jan 31 2010 12:00AM' THEN ca.CVietTat end),bc.bcmanv  FROM  tblNhanVien nv Left JOIN   tblBaoCao2010_01 bc ON bc.BCMaNV=nv.NVMa  LEFT JOIN tblCa ca ON bc.BCMaCa=ca.CMa  GROUP BY bc.BCMaNV,nv.NVMaNV 
GO
/****** Object:  View [dbo].[viewExportShift1]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  VIEW [dbo].[viewExportShift1]AS   SELECT nv.NVMaNV,'20'=  Max(CASE BCNgay WHEN 'Dec 20 2009 12:00AM' THEN ca.CVietTat end),'21'=  Max(CASE BCNgay WHEN 'Dec 21 2009 12:00AM' THEN ca.CVietTat end),'22'=  Max(CASE BCNgay WHEN 'Dec 22 2009 12:00AM' THEN ca.CVietTat end),'23'=  Max(CASE BCNgay WHEN 'Dec 23 2009 12:00AM' THEN ca.CVietTat end),'24'=  Max(CASE BCNgay WHEN 'Dec 24 2009 12:00AM' THEN ca.CVietTat end),'25'=  Max(CASE BCNgay WHEN 'Dec 25 2009 12:00AM' THEN ca.CVietTat end),'26'=  Max(CASE BCNgay WHEN 'Dec 26 2009 12:00AM' THEN ca.CVietTat end),'27'=  Max(CASE BCNgay WHEN 'Dec 27 2009 12:00AM' THEN ca.CVietTat end),'28'=  Max(CASE BCNgay WHEN 'Dec 28 2009 12:00AM' THEN ca.CVietTat end),'29'=  Max(CASE BCNgay WHEN 'Dec 29 2009 12:00AM' THEN ca.CVietTat end),'30'=  Max(CASE BCNgay WHEN 'Dec 30 2009 12:00AM' THEN ca.CVietTat end),'31'=  Max(CASE BCNgay WHEN 'Dec 31 2009 12:00AM' THEN ca.CVietTat end),'1'=  Max(CASE BCNgay WHEN 'Jan  1 2010 12:00AM' THEN ca.CVietTat end),'2'=  Max(CASE BCNgay WHEN 'Jan  2 2010 12:00AM' THEN ca.CVietTat end),'3'=  Max(CASE BCNgay WHEN 'Jan  3 2010 12:00AM' THEN ca.CVietTat end),'4'=  Max(CASE BCNgay WHEN 'Jan  4 2010 12:00AM' THEN ca.CVietTat end),'5'=  Max(CASE BCNgay WHEN 'Jan  5 2010 12:00AM' THEN ca.CVietTat end),'6'=  Max(CASE BCNgay WHEN 'Jan  6 2010 12:00AM' THEN ca.CVietTat end),'7'=  Max(CASE BCNgay WHEN 'Jan  7 2010 12:00AM' THEN ca.CVietTat end),'8'=  Max(CASE BCNgay WHEN 'Jan  8 2010 12:00AM' THEN ca.CVietTat end),'9'=  Max(CASE BCNgay WHEN 'Jan  9 2010 12:00AM' THEN ca.CVietTat end),'10'=  Max(CASE BCNgay WHEN 'Jan 10 2010 12:00AM' THEN ca.CVietTat end),'11'=  Max(CASE BCNgay WHEN 'Jan 11 2010 12:00AM' THEN ca.CVietTat end),'12'=  Max(CASE BCNgay WHEN 'Jan 12 2010 12:00AM' THEN ca.CVietTat end),'13'=  Max(CASE BCNgay WHEN 'Jan 13 2010 12:00AM' THEN ca.CVietTat end),'14'=  Max(CASE BCNgay WHEN 'Jan 14 2010 12:00AM' THEN ca.CVietTat end),'15'=  Max(CASE BCNgay WHEN 'Jan 15 2010 12:00AM' THEN ca.CVietTat end),'16'=  Max(CASE BCNgay WHEN 'Jan 16 2010 12:00AM' THEN ca.CVietTat end),'17'=  Max(CASE BCNgay WHEN 'Jan 17 2010 12:00AM' THEN ca.CVietTat end),'18'=  Max(CASE BCNgay WHEN 'Jan 18 2010 12:00AM' THEN ca.CVietTat end),'19'=  Max(CASE BCNgay WHEN 'Jan 19 2010 12:00AM' THEN ca.CVietTat end),bc.bcmanv  FROM  tblNhanVien nv Left JOIN   tblBaoCao2010_01 bc ON bc.BCMaNV=nv.NVMa  LEFT JOIN tblCa ca ON bc.BCMaCa=ca.CMa 
					 GROUP BY bc.BCMaNV,nv.NVMaNV 
GO
/****** Object:  Table [dbo].[tblNgayNghiLe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNgayNghiLe](
	[NNLMa] [varchar](10) NOT NULL,
	[NNLNgay] [datetime] NOT NULL,
	[NNLTen] [nvarchar](40) NOT NULL,
	[NNLLoai] [varchar](5) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_tblNgayNghiLe] PRIMARY KEY CLUSTERED 
(
	[NNLMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblThamSo]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblThamSo](
	[TSTen] [nvarchar](50) NOT NULL,
	[TSGiaTri] [ntext] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoTam_CongLam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoTam_CongLam](
	[BCNgay] [datetime] NOT NULL,
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
	[BCTGDKNTheogio] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNgayNghiLeNhanVien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNgayNghiLeNhanVien](
	[NLNVMa] [varchar](10) NOT NULL,
	[NLNVMaNV] [int] NULL,
	[NLNVNgay] [datetime] NOT NULL,
	[NNLLoai] [varchar](5) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_tblNgayNghiLeNhanVien] PRIMARY KEY CLUSTERED 
(
	[NLNVMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[viewTaoBCC_DKN]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_DKN]
	AS
SELECT dknMaNV,
       dknMaBP,
       (CASE WHEN  SUM(BCNghiPhep) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiPhep) END)  AS BCNghiPhep,
       (CASE WHEN  SUM(BCNghiH100) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiH100) END)   AS BCNghiH100,
       (CASE WHEN  SUM(BCNghiH70) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiH70) END)   AS BCNghiH70,
       (CASE WHEN SUM(BCNghiKL) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiKL) END)   AS BCNghiKL,
       (CASE WHEN SUM(BCNghiBH100) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiBH100) END)   AS BCNghiBH100,
       (CASE WHEN SUM(BCNghiBH70) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiBH70) END)   AS BCNghiBH70,
       (CASE WHEN SUM(BCNghiCongTac) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiCongTac) END)   AS BCNghiCongTac,
       (CASE WHEN SUM(BCNghiBu) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiBu) END)   AS BCNghiBu,
       (CASE WHEN SUM(BCNghiKhac) >=NVCongChuan THEN NVCongChuan ELSE SUM(BCNghiKhac) END)   AS BCNghiKhac
FROM   (
           SELECT tnv.NVMa AS dknMaNV,
                  tbp.BPMa AS dknMaBP,
                  tbc1.BCNghiPhep AS BCNghiPhep,
                  tbc1.BCNghiH100 AS BCNghiH100,
                  tbc1.BCNghiH70 AS BCNghiH70,
                  tbc1.BCNghiKL AS BCNghiKL,
                  tbc1.BCNghiBH100 AS BCNghiBH100,
                  tbc1.BCNghiBH70 AS BCNghiBH70,
                  tbc1.BCNghiCongTac AS BCNghiCongTac,
                  tbc1.BCNghiBu AS BCNghiBu,
                  tbc1.BCNghiKhac AS BCNghiKhac,
                  tnv.NVCongChuan

           FROM   dbo.tblNhanVien AS tnv
                  INNER JOIN tblBoPhan tbp
                       ON  tnv.NVMaBP = tbp.BPMa
                  LEFT OUTER JOIN  (
                SELECT *
                FROM   tblBaoCaoTam_CongLam tbc
                WHERE  BCNgay <= 'Nov 30 2011 12:00AM'
                       AND BCNgay >= 'Nov  1 2011 12:00AM' AND  BCMaBP in (18)
            ) tbc1
            ON  tnv.NVMa = tbc1.BCMaNV
 WHERE  (
                                      (
                                          -- Xet dieu kien cua ca
                                          1 >= (
                                              CASE 
                                                   WHEN (
                                                            -- Che do mac dinh cua ca
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                                   -- So sanh ngay lam viec voi cac ngay trong bang dang ky nghi le
                                                        ) = 0 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        -- Het che do mac dinh ca ----------------------------------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 1 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        ------------------ Het che do ngay nghi thuong --------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong 150 %  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 2 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                       -- Khong cap ca thi lay luon            
													  WHEN (
															SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
															FROM   tblCa tc
															WHERE  tc.CMa = (
																	   SELECT tbc.BCMaCa
																	   FROM   tblBaoCaoTam_CongLam 
																			  tbc
																	   WHERE  tbc.BCNgay = tbc1.BCNgay
																			  AND tbc.BCMaNV = tnv.NVMa
																   )
													  ) IS NULL THEN 0                                                                   
                                                   ELSE 2-- Khi ca thuoc loai 200 %, 300 % thi khong tinh
                                              END
                                          )
                                      )
                                      -- Xet dieu kien cua ngay chu nhat
                                      AND (
                                              DATEPART(
                                                  dw,
                                                  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                              ) <> (
                                                  SELECT CASE -- Khi ngay do co ca , Ca la loai ngay thuong
																--Loai ca mac dinh
																WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 0 THEN (
                                                                       SELECT CASE (
                                                                                       SELECT 
                                                                                              CONVERT(INT, CONVERT(VARCHAR, TSGiatri)) --+ ISNULL(tnv.NVLamCa, 0)
                                                                                       FROM   
                                                                                              tblThamSo 
                                                                                              tts
                                                                                       WHERE  
                                                                                              tts.TSTen = 
                                                                                              'NORMALWORKINGONSUNDAY'
                                                                                   )
                                                                                   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                                   WHEN 
                                                                                        1 THEN 
                                                                                        0
                                                                                   ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                              END
                                                                   )
                                                                   -- Loai ca ngay thuong
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 1 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   )
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 2 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   ) 
                                                                   
                                                                   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
                                                              ELSE CASE (
                                                                            SELECT 
                                                                                   CONVERT(INT, CONVERT(VARCHAR, TSGiatri))
                                                                            FROM   
                                                                                   tblThamSo 
                                                                                   tts
                                                                            WHERE  
                                                                                   tts.TSTen = 
                                                                                   'NORMALWORKINGONSUNDAY'
                                                                        )
                                                                        -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                        WHEN 1 THEN 
                                                                             0
                                                                        ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                   END
                                                         END
                                              )
                                          )
                                  )            
         ) AS T1
GROUP BY
       dknMaNV,
       dknMaBP,
		NVCongChuan
       HAVING (1 = 1)
 AND  dknMaBP in (18)
GO
/****** Object:  Table [dbo].[AppliedSpace]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppliedSpace](
	[ASID] [int] IDENTITY(1,1) NOT NULL,
	[DBLMa] [int] NOT NULL,
	[TSID] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vSalaryTable]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vSalaryTable]
AS
SELECT TSID,sum(LLuongCaHC) as LLuongCaHC,sum(LLuongCa1T) as LLuongCa1T,sum(LLuongCa2T) as LLuongCa2T,sum(LLuongCa3T) as LLuongCa3T,sum(PCCa2) as PCCa2,sum(PCCa3) as PCCa3
,sum(LLuongLamThem) as LLuongLamThem,sum(LLuongCa1CN) as LLuongCa1CN,sum(LLuongCa2CN) as LLuongCa2CN,sum(LLuongCa3CN) as LLuongCa3CN,sum(LLuongCa1NL) as LLuongCa1NL
,sum(LLuongCa2NL) as LLuongCa2NL,sum(LLuongCa3NL) as LLuongCa3NL,sum(LLuongNghi70) as LLuongNghi70,sum(LLuongNghi100) as LLuongNghi100,sum(LLCongTacNT) as LLCongTacNT
,sum(LLCongTacCN) as LLCongTacCN,sum(LLCongTacNL) as LLCongTacNL,sum(LLuongNghiKL) as LLuongNghiKL,sum(LLuongDiMuonVeSom) as LLuongDiMuonVeSom,sum(LLuongMatThe) as LLuongMatThe
FROM   SalaryTable inner join AppliedSpace on SID=ASID
group by TSID

GO
/****** Object:  Table [dbo].[WorkingTimeTable]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkingTimeTable](
	[WTID] [int] NOT NULL,
	[CaHC] [real] NOT NULL,
	[Ca1T] [real] NOT NULL,
	[Ca2T] [real] NOT NULL,
	[Ca3T] [real] NOT NULL,
	[LamThem] [real] NOT NULL,
	[Ca1CN] [real] NOT NULL,
	[Ca2CN] [real] NOT NULL,
	[Ca3CN] [real] NOT NULL,
	[Ca1NL] [real] NOT NULL,
	[Ca2NL] [real] NOT NULL,
	[Ca3NL] [real] NOT NULL,
	[NghiPhep] [real] NOT NULL,
	[Nghi100] [real] NOT NULL,
	[Nghi70] [real] NOT NULL,
	[NghiKL] [real] NOT NULL,
	[CTNT] [real] NOT NULL,
	[CTCN] [real] NOT NULL,
	[CTNL] [real] NOT NULL,
	[DMVS] [real] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vWorkingTime]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vWorkingTime]
AS
SELECT TSID,sum(CaHC) as CaHC,sum(Ca1T) as Ca1T,sum(Ca2T) as Ca2T,sum(Ca3T) as Ca3T
,sum(LamThem) as LamThem,sum(Ca1CN) as Ca1CN,sum(Ca2CN) as Ca2CN,sum(Ca3CN) as Ca3CN,sum(Ca1NL) as Ca1NL
,sum(Ca2NL) as Ca2NL,sum(Ca3NL) as Ca3NL,sum(Nghi70) as Nghi70,sum(Nghi100) as Nghi100,sum(CTNT) as CTNT
,sum(CTCN) as CTCN,sum(CTNL) as CTNL,sum(NghiKL) as NghiKL,sum(DMVS) as DMVS,sum(NghiPhep) as NghiPhep
FROM   WorkingTimeTable inner join AppliedSpace on WTID=ASID
group by TSID


GO
/****** Object:  Table [dbo].[LLuong_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LLuong_ChiTiet](
	[LPCMa] [int] IDENTITY(1,1) NOT NULL,
	[LMa] [nvarchar](50) NOT NULL,
	[PCMa] [nvarchar](50) NOT NULL,
	[LuongPCGiaTri] [numeric](18, 0) NULL,
 CONSTRAINT [PK_LLuong_ChiTiet] PRIMARY KEY CLUSTERED 
(
	[LPCMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LLuong](
	[LID] [int] IDENTITY(1,1) NOT NULL,
	[LMaNV] [int] NOT NULL,
	[LMaNVN] [nvarchar](10) NOT NULL,
	[LThang] [smallint] NOT NULL,
	[LNam] [smallint] NOT NULL,
	[LLuongCBHT] [numeric](19, 4) NULL,
	[LLuongDongBH] [numeric](18, 0) NULL,
	[LLuongCa1T] [numeric](10, 0) NULL,
	[LLuongCa2T] [numeric](10, 0) NULL,
	[LLuongCa3T] [numeric](10, 0) NULL,
	[LLuongLamThemN] [numeric](10, 0) NULL,
	[LLuongLamThemD] [numeric](10, 0) NULL,
	[LLuongLamThemN_TV] [numeric](10, 0) NULL,
	[LLuongLamThemD_TV] [numeric](10, 0) NULL,
	[LLuongLamThemCongtyN] [numeric](10, 0) NULL,
	[LLuongLamThemCongtyD] [numeric](10, 0) NULL,
	[LLuongLamThemNLN] [numeric](10, 0) NULL,
	[LLuongLamThemNLD] [numeric](10, 0) NULL,
	[LLuongNghi70] [numeric](18, 0) NULL,
	[LLuongNghi100] [numeric](18, 0) NULL,
	[LLCongTacNT] [numeric](18, 0) NULL,
	[LLCongTacCN] [numeric](18, 0) NULL,
	[LLCongTacNL] [numeric](18, 0) NULL,
	[LLuongChuyenCan] [numeric](18, 0) NULL,
	[LTongKhoanCong] [numeric](18, 0) NULL,
	[LTongLuongAnCa] [numeric](18, 0) NULL,
	[LTongLuongLamThem] [numeric](18, 0) NULL,
	[LTongPhuCap] [numeric](18, 0) NULL,
	[LThueThuNhap] [numeric](18, 0) NULL,
	[LBHXHNV] [numeric](18, 0) NULL,
	[LBHYTNV] [numeric](18, 0) NULL,
	[LBHTNNV] [numeric](18, 0) NULL,
	[LCongDoan] [numeric](18, 0) NULL,
	[LLuongNghiKL] [numeric](18, 0) NULL,
	[LLuongDiMuonVeSom] [numeric](18, 0) NULL,
	[LDieuchinhLuong] [numeric](18, 0) NULL,
	[LBHXHCT] [numeric](18, 0) NULL,
	[LBHYTCT] [numeric](18, 0) NULL,
	[LTongCtyTraVND] [numeric](18, 0) NULL,
	[LLuongThucLinh] [numeric](18, 0) NULL,
	[LLuongThucLinhLT] [numeric](18, 0) NULL,
	[LThuong] [numeric](18, 0) NULL,
	[LPCKhac] [numeric](18, 0) NULL,
	[LNguoiNN] [bit] NULL,
	[LUuTien] [int] NULL,
	[LThuNhapChiuThue] [numeric](18, 0) NULL,
	[LLoai] [bit] NULL,
	[HTCongChuan] [numeric](18, 0) NULL,
	[TCDocHai] [numeric](18, 0) NULL,
	[LPCDilai] [numeric](18, 0) NULL,
	[LPCNhaO] [numeric](18, 0) NULL,
	[LTCDienThoai] [numeric](18, 0) NULL,
	[LPCNN] [numeric](18, 0) NULL,
	[LPCConnho] [numeric](18, 0) NULL,
	[LPCThamnien] [numeric](18, 0) NULL,
	[LHTCongChuan] [numeric](18, 0) NULL,
	[LTCDocHai] [numeric](18, 0) NULL,
	[PCDilai_Cu] [numeric](18, 0) NULL,
	[PCNhaO_Cu] [numeric](18, 0) NULL,
	[PCKhac_Cu] [numeric](18, 0) NULL,
	[TCDienThoai_Cu] [numeric](18, 0) NULL,
	[PCNN_Cu] [numeric](18, 0) NULL,
	[PCConnho_Cu] [numeric](18, 0) NULL,
	[PCThamnien_Cu] [numeric](18, 0) NULL,
	[HTCongChuan_Cu] [numeric](18, 0) NULL,
	[TCDocHai_Cu] [numeric](18, 0) NULL,
	[LBHTNCT] [numeric](18, 0) NULL,
	[LPcngoaingu] [numeric](18, 0) NULL,
	[LPctrachnhiem] [numeric](18, 0) NULL,
	[LPckynang] [numeric](18, 0) NULL,
	[LPccadem] [numeric](18, 0) NULL,
	[LPcnangnhoc] [numeric](18, 0) NULL,
	[LPcnghenghiep] [numeric](18, 0) NULL,
	[LPcchuyencan] [numeric](18, 0) NULL,
	[LPcdienthoai] [numeric](18, 0) NULL,
	[LPcbanatld] [numeric](18, 0) NULL,
	[LPcbaoduong] [numeric](18, 0) NULL,
	[LTienSinhNhat] [numeric](18, 0) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[LGhiChu] [nvarchar](400) NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[Vluong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW [dbo].[Vluong] as 
SELECT ll.LID,[PC0001] as PCDilai,[PC0002] as PCNhaO,[PC0003] as Pcngoaingu,[PC0004] as Pctrachnhiem,[PC0005] as Pcnghenghiep,[PC0007] as Pccadem,[PC0010] as Pcnangnhoc,[PC0011] as Pcchuyencan,[PC0012] as Pcdienthoai,[PC0013] as Pcbanatld,[PC0014] as Pcbaoduong
           FROM   LLuong ll
                  LEFT JOIN (
                           SELECT *
                           FROM   (
                                      SELECT lct.PCMa,
                                             lct.LMa ,
                                             lct.LuongPCGiaTri
                                      FROM LLuong_ChiTiet lct
                                  ) up PIVOT(
                                      SUM(LuongPCGiaTri) FOR PCMa IN ([PC0001],[PC0002],[PC0003],[PC0004],[PC0005],[PC0007],[PC0010],[PC0011],[PC0012],[PC0013],[PC0014])
                                  ) AS PIVOT01
                       ) Luongpivot
                       ON  ll.LID = Luongpivot.LMa
GO
/****** Object:  View [dbo].[vB20Employee_HrmEdit]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vB20Employee_HrmEdit]
AS
SELECT    nv.NVMaNV AS Code, nv.NVHoTen AS Name, nv.NVEmailCaNhan AS Email, cv.CVTen, bp.BPMa, bp.BPTen AS DeptName
FROM         dbo.tblNhanVien AS nv INNER JOIN
                      dbo.tblChucVu AS cv ON nv.NVMaCV = cv.CVMa INNER JOIN
                      dbo.tblBoPhan AS bp ON nv.NVMaBP = bp.BPMa
GO
/****** Object:  Table [dbo].[tblHopdongNV_PhuCap]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHopdongNV_PhuCap](
	[HDPCMa] [nvarchar](50) NOT NULL,
	[HDMa] [nvarchar](50) NOT NULL,
	[PCMa] [nvarchar](50) NOT NULL,
	[HDPCGiaTri] [numeric](18, 0) NULL,
 CONSTRAINT [PK_tblHopdongNV_PhuCap] PRIMARY KEY CLUSTERED 
(
	[HDPCMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[VHopDong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW [dbo].[VHopDong] as 
SELECT ll.HDMa,[PC0001] as PCDilai,[PC0002] as PCNhaO,[PC0003] as Pcngoaingu,[PC0004] as Pctrachnhiem,[PC0005] as Pcnghenghiep,[PC0007] as Pccadem,[PC0010] as Pcnangnhoc,[PC0011] as Pcchuyencan,[PC0012] as Pcdienthoai,[PC0013] as Pcbanatld,[PC0014] as Pcbaoduong
           FROM   tblHopDongNV ll
                  LEFT JOIN (
                           SELECT *
                           FROM   (
									   SELECT thnpc.PCMa,
											  thnpc.HDMa,
											  thnpc.HDPCGiaTri
									   FROM   tblHopdongNV_PhuCap thnpc
                                  ) up PIVOT(
                                      SUM(HDPCGiaTri) FOR PCMa IN ([PC0001],[PC0002],[PC0003],[PC0004],[PC0005],[PC0007],[PC0010],[PC0011],[PC0012],[PC0013],[PC0014])
                                  ) AS PIVOT01
                       ) 
						hdpivot
						ON  ll.HDMa = hdpivot.HDMa
GO
/****** Object:  View [dbo].[vDienBienLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

 CREATE VIEW [dbo].[vDienBienLuong] AS 
SELECT thdn.HDMaNV,
       thdn.HDLoaiHD,
       thdn.HDTuNgay,
       thdn.HDDenNgay,
       thdn.HDLuongCB,
       thdn.HDLuongBH,
       thdn.HDDongBHYT,
       thdn.HDDongBHXH,
       thdn.HDDongCongDoan,
       vd.*
FROM   tblHopDongNV thdn
       INNER JOIN VHopDong vd
            ON  thdn.HDMa = vd.HDMa
UNION ALL
SELECT HDMaNV,
       HDLoaiHD,
       [PLTuNgay],
       [PLDenNgay],
       [PLLuongCB],
       [PLLuongBH],
       [PLDongBHYT],
       [PLDongBHXH],
       [PLDongCongDoan],
       vl.*
FROM   tblHopDongNV_Phuluc
       INNER JOIN tblHopDongNV
            ON  tblHopDongNV_Phuluc.HDMa = tblHopDongNV.HDMa
       INNER JOIN VPhuLuc vl
            ON  tblHopDongNV_Phuluc.HDPLMa = vl.HDPLMa
GO
/****** Object:  Table [dbo].[NS_BaoHiemXH]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_BaoHiemXH](
	[BHXHMaNV] [int] NOT NULL,
	[BHXHSoSo] [nvarchar](10) NOT NULL,
	[BHXHNgayCap] [datetime] NULL,
	[BHXHNgayBDDong] [datetime] NULL,
	[BHXHNoiCap] [nvarchar](100) NOT NULL,
	[BHXHNamDongTruoc] [int] NOT NULL,
	[BHXHThangDongTruoc] [int] NOT NULL,
	[BHXHSTTGoc] [int] NOT NULL,
	[BHXHNoiDong] [nvarchar](100) NOT NULL,
	[BHXHGhichu] [nvarchar](100) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[BHXHNgayDongBHTN] [datetime] NULL,
	[BHXHNgayTraSo] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLyDoNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLyDoNghi](
	[LDNMa] [nvarchar](50) NOT NULL,
	[LDNTen] [nvarchar](40) NOT NULL,
	[LDNVietTat] [nvarchar](5) NOT NULL,
	[LDNLoai] [varchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[LDNBaoHiem] [varchar](10) NULL,
	[LDNTongHopNghi] [varchar](10) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[LDNQuanLyTon] [bit] NULL,
	[LDNQuanLyTonKyHieu] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLoaiNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLoaiNghi](
	[LNMa] [varchar](50) NOT NULL,
	[LNViettat] [varchar](50) NOT NULL,
	[LNTen] [nvarchar](40) NOT NULL,
	[LNHienThiBCC] [bit] NOT NULL,
	[LNHienThiDKN] [bit] NOT NULL,
	[LNCongVaoTongCong] [bit] NOT NULL,
	[LNTinhDKNgayNghi] [bit] NOT NULL,
	[LNTinhDKNgayLe] [bit] NOT NULL,
	[LNHeSoNhanCongNT] [float] NOT NULL,
	[LNHeSoNhanCongNN] [float] NOT NULL,
	[LNHeSoNhanCongNL] [float] NOT NULL,
	[LNLoai] [tinyint] NOT NULL,
	[LNUuTien] [smallint] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[LNMauSac] [varchar](50) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDangKyNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDangKyNghi](
	[DKNMa] [varchar](10) NOT NULL,
	[DKNMaNV] [int] NOT NULL,
	[DKNNgayApDung] [datetime] NOT NULL,
	[DKNNgayKetThuc] [datetime] NOT NULL,
	[DKNMaLyDo] [varchar](10) NOT NULL,
	[DKNLoai] [varchar](10) NOT NULL,
	[DKNNghiBu] [float] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [datetime2](7) NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DKNGhiChu] [nvarchar](400) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[WebId] [int] NULL,
 CONSTRAINT [PK_tblDangKyNghi] PRIMARY KEY CLUSTERED 
(
	[DKNMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[dgtBHXH]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[dgtBHXH] AS 
SELECT tblNhanVien.NVMa,
       tblNhanVien.NVHoTen,
       tblNhanVien.NVNgaySinh,
       CONVERT(date,'03/24/2011') AS Thoigian,
       NS_BaoHiemXH.BHXHSoSo,
       tblChucVu.CVTen,
       LuongMoiNhat.HDLuongCB AS BH_LuongCu,
       (
           CASE 
                WHEN ISNULL(LuongThangBH.HDLuongCB, 0) > ISNULL(LuongMoiNhat.HDLuongCB, 0) THEN 
                     LuongThangBH.HDLuongCB
                ELSE 0
           END
       ) BH_LuongMoi,
       'TM' AS BH_Ghichu,
       'Tangmoi' AS BH_Loai,
       'I' AS BH_ViTri,
       N'Lao động tăng' AS BH_Loai_Ten
       --,LuongMoiNhat.HDLuongCB,LuongHientai.HDLuongCB
FROM   (
           SELECT tblHopDongNV.HDMaNV,
                  tblHopDongNV.HDTuNgay,
                  tblHopDongNV.HDLuongCB
           FROM   tblHopDongNV
                  INNER JOIN (
                           SELECT HDMaNV,
                                  MAX(tblHopDongNV.HDTuNgay) AS HDTuNgay
                           FROM   tblHopDongNV
                           WHERE  (
                                      MONTH(HDTuNgay) = MONTH('03/24/2011')
                                      AND YEAR(HDTuNgay) = YEAR('03/24/2011')
                                  )
                           GROUP BY
                                  tblHopDongNV.HDMaNV
                       ) Top1HD
                       ON  tblHopDongNV.HDMaNV = Top1HD.HDMaNV
                       AND tblHopDongNV.HDTuNgay = Top1HD.HDTuNgay
       ) LuongThangBH
       LEFT JOIN (
                SELECT tblHopDongNV.HDMaNV,
                       tblHopDongNV.HDTuNgay,
                       tblHopDongNV.HDLuongCB
                FROM   tblHopDongNV
                       INNER JOIN (
                                SELECT HDMaNV,
                                       MAX(tblHopDongNV.HDTuNgay) AS HDTuNgay
                                FROM   tblHopDongNV
                                WHERE  (
                                           (MONTH(HDTuNgay) + 12 * YEAR(HDTuNgay)) 
                                           < (MONTH('03/24/2011') + 12 * YEAR('03/24/2011'))
                                       )
                                GROUP BY
                                       tblHopDongNV.HDMaNV
                            ) Top1HD
                            ON  tblHopDongNV.HDMaNV = Top1HD.HDMaNV
                            AND tblHopDongNV.HDTuNgay = Top1HD.HDTuNgay
            ) LuongMoiNhat
            ON  LuongMoiNhat.HDMaNV = LuongThangBH.HDMaNV
       LEFT JOIN NS_BaoHiemXH
            ON  LuongThangBH.HDMaNV = NS_BaoHiemXH.BHXHMaNV
       INNER JOIN tblNhanVien
            ON  LuongThangBH.HDMaNV = tblNhanVien.NVMa
       LEFT JOIN tblChucVu
            ON  tblNhanVien.NVMaCV = tblChucVu.CVMa
--WHERE  LuongThangBH.HDMaNV = 644

-- Noi voi DKN
UNION ALL
SELECT tblNhanVien.NVMa,
       tblNhanVien.NVHoTen,
        tblNhanVien.NVNgaySinh,
       CONVERT(date,'03/24/2011') AS Thoigian,
       NS_BaoHiemXH.BHXHSoSo,
       tblChucVu.CVTen,
       0 AS BH_LuongCu,
       HDNV.HDLuongCB AS BH_LuongMoi,
       N'ON' AS BH_Ghichu,
       'Tangmoi' AS BH_Loai,
       'I' AS BH_ViTri,
       N'Lao động tăng' AS BH_Loai_Ten
FROM   (
           SELECT tblHopDongNV.HDMaNV,
                  tblHopDongNV.HDTuNgay,
                  tblHopDongNV.HDLuongCB
           FROM   tblHopDongNV
                  INNER JOIN (
                           SELECT HDMaNV,
                                  MAX(tblHopDongNV.HDTuNgay) AS HDTuNgay
                           FROM   tblHopDongNV
                           WHERE  (
                                      MONTH(HDTuNgay) = MONTH('03/24/2011')
                                      AND YEAR(HDTuNgay) = YEAR('03/24/2011')
                                  )
                           GROUP BY
                                  tblHopDongNV.HDMaNV
                       ) Top1HD
                       ON  tblHopDongNV.HDMaNV = Top1HD.HDMaNV
                       AND tblHopDongNV.HDTuNgay = Top1HD.HDTuNgay
       ) HDNV
       INNER JOIN (
                SELECT tblDangKyNghi.DKNMaNV
                FROM   tblDangKyNghi
                       INNER JOIN tblLyDoNghi
                            ON  tblDangKyNghi.DKNMaLyDo = tblLyDoNghi.LDNMa
                       INNER JOIN tblLoaiNghi
                            ON  tblLyDoNghi.LDNLoai = tblLoaiNghi.LNMa
                WHERE  (
                           (
                               MONTH(DKNNgayKetThuc) = MONTH('03/24/2011')
                               AND YEAR(DKNNgayKetThuc) = YEAR('03/24/2011')
                               AND DAY(DKNNgayKetThuc) <= 14
                           )
                           OR (
                                  MONTH(DATEADD(m, 1, DKNNgayKetThuc)) = MONTH('03/24/2011')
                                  AND YEAR(DATEADD(m, 1, DKNNgayKetThuc)) = YEAR('03/24/2011')
                                  AND DAY(DATEADD(m, 1, DKNNgayKetThuc)) >= 15
                              )
                       )
                       AND (tblLoaiNghi.LNLoai = 5 OR tblLoaiNghi.LNLoai = 6)
            ) DKN
            ON  HDNV.HDMaNV = DKN.DKNMaNV
       LEFT JOIN NS_BaoHiemXH
            ON  DKN.DKNMaNV = NS_BaoHiemXH.BHXHMaNV
       INNER JOIN tblNhanVien
            ON  DKN.DKNMaNV = tblNhanVien.NVMa
       LEFT JOIN tblChucVu
            ON  tblNhanVien.NVMaCV = tblChucVu.CVMa
            
-- Noi voi nghi thai san
UNION ALL
SELECT tblNhanVien.NVMa,
       tblNhanVien.NVHoTen,
       tblNhanVien.NVNgaySinh,
       CONVERT(date,'03/24/2011') AS Thoigian,
       NS_BaoHiemXH.BHXHSoSo,
       tblChucVu.CVTen,
       0 AS BH_LuongCu,
       HDNV.HDLuongCB AS BH_LuongMoi,
       N'TS' AS BH_Ghichu,
       'GiamBH' AS BH_Loai,
       'II' AS BH_ViTri,
       N'Lao động giảm' AS BH_Loai_Ten
FROM   (
           SELECT tblHopDongNV.HDMaNV,
                  tblHopDongNV.HDTuNgay,
                  tblHopDongNV.HDLuongCB
           FROM   tblHopDongNV
                  INNER JOIN (
                           SELECT HDMaNV,
                                  MAX(tblHopDongNV.HDTuNgay) AS HDTuNgay
                           FROM   tblHopDongNV
                           --WHERE  (
                           --           MONTH(HDTuNgay) = MONTH('03/24/2011')
                           --           AND YEAR(HDTuNgay) = YEAR('03/24/2011')
                           --       )
                           GROUP BY
                                  tblHopDongNV.HDMaNV
                       ) Top1HD
                       ON  tblHopDongNV.HDMaNV = Top1HD.HDMaNV
                       AND tblHopDongNV.HDTuNgay = Top1HD.HDTuNgay
       ) HDNV
       INNER JOIN (
                SELECT tblDangKyNghi.DKNMaNV
                FROM   tblDangKyNghi
                       INNER JOIN tblLyDoNghi
                            ON  tblDangKyNghi.DKNMaLyDo = tblLyDoNghi.LDNMa
                       INNER JOIN tblLoaiNghi
                            ON  tblLyDoNghi.LDNLoai = tblLoaiNghi.LNMa
                WHERE  (
                           (
                               MONTH(DKNNgayApdung) = MONTH('03/24/2011')
                               AND YEAR(DKNNgayApdung) = YEAR('03/24/2011')
                               AND DAY(DKNNgayApdung) <= 14
                           )
                           OR (
                                  MONTH(DATEADD(m, 1, DKNNgayApdung)) = MONTH('03/24/2011')
                                  AND YEAR(DATEADD(m, 1, DKNNgayApdung)) = YEAR('03/24/2011')
                                  AND DAY(DATEADD(m, 1, DKNNgayApdung)) >= 15
                              )
                       )
                       AND (tblLoaiNghi.LNLoai = 5)
            ) DKN
            ON  HDNV.HDMaNV = DKN.DKNMaNV
       LEFT JOIN NS_BaoHiemXH
            ON  DKN.DKNMaNV = NS_BaoHiemXH.BHXHMaNV
       INNER JOIN tblNhanVien
            ON  DKN.DKNMaNV = tblNhanVien.NVMa
       LEFT JOIN tblChucVu
            ON  tblNhanVien.NVMaCV = tblChucVu.CVMa
   -- Noi voi nghi om 
UNION ALL    
SELECT tblNhanVien.NVMa,
       tblNhanVien.NVHoTen,
       tblNhanVien.NVNgaySinh,
       CONVERT(date,'03/24/2011') AS Thoigian,
       NS_BaoHiemXH.BHXHSoSo,
       tblChucVu.CVTen,
       0 AS BH_LuongCu,
       HDNV.HDLuongCB AS BH_LuongMoi,
       N'OF' AS BH_Ghichu,
        'GiamBH' AS BH_Loai,
        'II' AS BH_ViTri,
       N'Lao động giảm' AS BH_Loai_Ten
FROM   (
           SELECT tblHopDongNV.HDMaNV,
                  tblHopDongNV.HDTuNgay,
                  tblHopDongNV.HDLuongCB
           FROM   tblHopDongNV
                  INNER JOIN (
                           SELECT HDMaNV,
                                  MAX(tblHopDongNV.HDTuNgay) AS HDTuNgay
                           FROM   tblHopDongNV
                           --WHERE  (
                           --           MONTH(HDTuNgay) = MONTH('03/24/2011')
                           --           AND YEAR(HDTuNgay) = YEAR('03/24/2011')
                           --       )
                           GROUP BY
                                  tblHopDongNV.HDMaNV
                       ) Top1HD
                       ON  tblHopDongNV.HDMaNV = Top1HD.HDMaNV
                       AND tblHopDongNV.HDTuNgay = Top1HD.HDTuNgay
       ) HDNV
       INNER JOIN (
                SELECT tblDangKyNghi.DKNMaNV
                FROM   tblDangKyNghi
                       INNER JOIN tblLyDoNghi
                            ON  tblDangKyNghi.DKNMaLyDo = tblLyDoNghi.LDNMa
                       INNER JOIN tblLoaiNghi
                            ON  tblLyDoNghi.LDNLoai = tblLoaiNghi.LNMa
                WHERE  (
                           (
                               MONTH(DKNNgayApdung) = MONTH('03/24/2011')
                               AND YEAR(DKNNgayApdung) = YEAR('03/24/2011')
                               AND DAY(DKNNgayApdung) <= 14
                           )
                           OR (
                                  MONTH(DATEADD(m, 1, DKNNgayApdung)) = MONTH('03/24/2011')
                                  AND YEAR(DATEADD(m, 1, DKNNgayApdung)) = YEAR('03/24/2011')
                                  AND DAY(DATEADD(m, 1, DKNNgayApdung)) >= 15
                              )
                       )
                       AND (tblLoaiNghi.LNLoai = 6)
            ) DKN
            ON  HDNV.HDMaNV = DKN.DKNMaNV
       LEFT JOIN NS_BaoHiemXH
            ON  DKN.DKNMaNV = NS_BaoHiemXH.BHXHMaNV
       INNER JOIN tblNhanVien
            ON  DKN.DKNMaNV = tblNhanVien.NVMa
       LEFT JOIN tblChucVu
            ON  tblNhanVien.NVMaCV = tblChucVu.CVMa
-- Noi voi nghi nghi viec
UNION ALL     
SELECT tblNhanVien.NVMa,
       tblNhanVien.NVHoTen,
       tblNhanVien.NVNgaySinh,
       CONVERT(date,'03/24/2011') AS Thoigian,
       NS_BaoHiemXH.BHXHSoSo,
       tblChucVu.CVTen,
       0 AS BH_LuongCu,
       HDNV.HDLuongCB AS BH_LuongMoi,
       N'GH' AS BH_Ghichu,
        'GiamBH' AS BH_Loai,
        'II' AS BH_ViTri,
       N'Lao động giảm' AS BH_Loai_Ten
FROM   (
           SELECT tblHopDongNV.HDMaNV,
                  tblHopDongNV.HDTuNgay,
                  tblHopDongNV.HDLuongCB
           FROM   tblHopDongNV
                  INNER JOIN (
                           SELECT HDMaNV,
                                  MAX(tblHopDongNV.HDTuNgay) AS HDTuNgay
                           FROM   tblHopDongNV
                                  --WHERE  (
                                  --           MONTH(HDTuNgay) = MONTH('03/24/2011')
                                  --           AND YEAR(HDTuNgay) = YEAR('03/24/2011')
                                  --       )
                           GROUP BY
                                  tblHopDongNV.HDMaNV
                       ) Top1HD
                       ON  tblHopDongNV.HDMaNV = Top1HD.HDMaNV
                       AND tblHopDongNV.HDTuNgay = Top1HD.HDTuNgay
       ) HDNV
       INNER JOIN (
                SELECT tblNhanVien.NVMa AS GHNVMa
                FROM   tblNhanVien
                WHERE  MONTH(tblNhanVien.NVNgayRa) = MONTH('03/24/2011')
                       AND YEAR(tblNhanVien.NVNgayRa) = YEAR('03/24/2011')
            ) GH
            ON  HDNV.HDMaNV = GH.GHNVMa
       LEFT JOIN NS_BaoHiemXH
            ON  GH.GHNVMa = NS_BaoHiemXH.BHXHMaNV
       INNER JOIN tblNhanVien
            ON  GH.GHNVMa = tblNhanVien.NVMa
       LEFT JOIN tblChucVu
            ON  tblNhanVien.NVMaCV = tblChucVu.CVMa
GO
/****** Object:  Table [dbo].[tblBaoCaoTam_NgayThuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoTam_NgayThuong](
	[BCNgay] [datetime] NOT NULL,
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
	[BCTGDKNTheogio] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoTam_NgayNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoTam_NgayNghi](
	[BCNgay] [datetime] NOT NULL,
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
	[BCTGDKNTheogio] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[viewTaoBCC_Cong_NgayNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_Cong_NgayNghi]
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
                                           WHERE  BCNgay <= 'May 31 2011 12:00AM'
												   AND BCNgay >= 'May  1 2011 12:00AM' AND  tbc.BCMaBP in (5)
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
                                                                                                 'NORMALWORKINGONSUNDAY'
                                                                                      )
                                                                                      WHEN 
                                                                                           '1' THEN 
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
                                                  WHERE  BCNgay <= 'May 31 2011 12:00AM'
												   AND BCNgay >= 'May  1 2011 12:00AM' AND  tbc.BCMaBP in (5)
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
                                                                                                 'NORMALWORKINGONSUNDAY'
                                                                                      )
                                                                                      WHEN 
                                                                                          '1' THEN 
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
                                            WHERE  BCNgay <= 'May 31 2011 12:00AM'
											AND BCNgay >= 'May  1 2011 12:00AM' AND  tbc.BCMaBP in (5)
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
                                                                                                 'NORMALWORKINGONSUNDAY'
                                                                                      )
                                                                                      WHEN 
                                                                                           '1' THEN 
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
                                           WHERE  BCNgay <= 'May 31 2011 12:00AM'
												   AND BCNgay >= 'May  1 2011 12:00AM' AND  tbc.BCMaBP in (5)
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
                                                                                                 'NORMALWORKINGONSUNDAY'
                                                                                      )
                                                                                      WHEN 
                                                                                           '1' THEN 
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
 AND  tbp.BPMa in (5)
GO
/****** Object:  Table [dbo].[DM_Loainghile]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_Loainghile](
	[LNLMa] [varchar](5) NOT NULL,
	[LNLTen] [nvarchar](200) NOT NULL,
	[LNLLoai] [varchar](10) NOT NULL,
	[LNLKyHieu] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[LNLMauSac] [varchar](50) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_Loainghile] PRIMARY KEY CLUSTERED 
(
	[LNLMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[viewTaoBCC_TungNgay]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW [dbo].[viewTaoBCC_TungNgay] AS SELECT tnv.NVMa as tgNVMa,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  1 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  1 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  1 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay01
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  2 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  2 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  2 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay02
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  3 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  3 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  3 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay03
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  4 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  4 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  4 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay04
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  5 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  5 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  5 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay05
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  6 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  6 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  6 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay06
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  7 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  7 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  7 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay07
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  8 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  8 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  8 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay08
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan  9 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan  9 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan  9 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay09
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 10 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 10 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 10 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay10
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 11 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 11 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 11 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay11
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 12 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 12 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 12 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay12
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 13 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 13 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 13 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay13
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 14 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 14 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 14 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay14
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 15 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 15 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 15 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay15
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 16 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 16 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 16 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay16
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 17 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 17 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 17 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay17
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 18 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 18 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 18 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay18
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 19 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 19 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 19 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay19
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 20 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 20 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 20 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay20
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 21 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 21 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 21 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay21
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 22 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 22 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 22 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay22
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 23 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 23 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 23 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay23
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 24 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 24 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 24 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay24
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 25 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 25 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 25 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay25
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 26 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 26 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 26 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay26
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 27 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 27 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 27 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay27
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 28 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 28 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 28 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay28
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 29 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 29 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 29 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay29
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 30 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 30 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 30 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay30
	,
		(
	    MAX(
	        CASE 
	             WHEN tbc1.BCNgay = 'Jan 31 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') = '' THEN 
				   CASE 
						WHEN tnl.NNLNgay = 'Jan 31 2012 12:00AM'
					   AND (ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)) 
						   <= 0 THEN tnl.LNLKyHieu 
						   ELSE 	        	
	        			CONVERT(
							VARCHAR,
							CAST(
								ROUND(
									(
										(
											tbc1.BCTGLamNgay + tbc1.BCTGLamToi 
											+ tbc1.BCTGThemNgay +
											tbc1.BCTGThemToi
										) / 60.0
									),
									2
								) AS FLOAT
							)
	        			)
	        		END
	            WHEN tbc1.BCNgay = 'Jan 31 2012 12:00AM'
	        AND ISNULL(tbc1.BCGhiChu,'') <> '' THEN tbc1.BCGhiChu 
	      
	            END
	    )
	) AS Ngay31
	,'1' AS [1] FROM   tblNhanVien tnv
                       INNER JOIN (
                                SELECT *
                                FROM   tblBaoCao 
                                WHERE   tblBaoCao.BCMaBP IN (11) AND  BCNgay <= 'Jan 31 2012 12:00AM'
                                       AND BCNgay >= 'Jan  1 2012 12:00AM'
                                      
                            ) tbc1
                            ON  tnv.NVMa = tbc1.BCMaNV
					   LEFT JOIN (
								SELECT 0 as NNLMaNV,nnl.NNLMa,
									   nnl.NNLNgay,
									   nnl.NNLLoai,
									   dl.LNLKyHieu
								FROM   tblNgayNghile nnl
									   INNER JOIN DM_Loainghile dl
											ON  nnl.NNLLoai = dl.LNLLoai
								WHERE nnl.NNLNgay>= 'Jan  1 2012 12:00AM' AND nnl.NNLNgay<= 'Jan 31 2012 12:00AM'
								UNION ALL 
				                
								SELECT tnnlnv.NLNVMaNV,tnnlnv.NLNVMa,
									   tnnlnv.NLNVNgay,
									   tnnlnv.NNLLoai,
									   dl.LNLKyHieu
								FROM   tblNgayNghiLeNhanVien tnnlnv
									   INNER JOIN dbo.DM_Loainghile AS dl
											ON  tnnlnv.NNLLoai = dl.LNLLoai		
								WHERE tnnlnv.NLNVNgay>= 'Jan  1 2012 12:00AM' AND tnnlnv.NLNVNgay<= 'Jan 31 2012 12:00AM'
																) tnl
																ON tnv.NVMa=tnl.NNLMaNV OR tnl.NNLMaNV<=0
																            AND tnv.NVNgayVao <= tnl.NNLNgay
            AND tnv.NVNgayRa >= tnl.NNLNgay
                            GROUP BY tnv.NVMa
GO
/****** Object:  View [dbo].[viewTaoBCC_CL_LT_DMVS]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_CL_LT_DMVS]
	AS
	SELECT tnv.NVMa as clNVMa,
       ROUND((ISNULL(CC_LT_DM_VS_Thuong.TGLamN_CT, 0)),2) AS TGLamN,
       ROUND((ISNULL(CC_LT_DM_VS_Thuong.TGLamD_CT, 0)),2) AS TGLamD,
       ROUND(CC_LT_DM_VS_Thuong.TGLamthemN_CT,2) AS TGLamthemN,
       ROUND(CC_LT_DM_VS_Thuong.TGLamthemD_CT,2) AS TGLamthemD,
       ROUND(CC_LT_DM_VS_Thuong.TGDimuonN_CT, 2) AS TGDimuonN,
       ROUND(CC_LT_DM_VS_Thuong.TGDimuonD_CT, 2) AS TGDimuonD,
       ROUND(CC_LT_DM_VS_Thuong.TGVesomN_CT, 2) AS TGVesomN,
       ROUND(CC_LT_DM_VS_Thuong.TGVesomD_CT, 2) AS TGVesomD,
       CC_LT_DM_VS_Thuong.SoLanDiMuon_CT AS SoLanDiMuon,
       CC_LT_DM_VS_Thuong.SoLanVeSom_CT AS SoLanVeSom

FROM   tblNhanVien tnv
       INNER JOIN tblBoPhan tbp
            ON  tnv.NVMaBP = tbp.BPMa
                --LEFT JOIN viewTaoBCC_TungNgay vtbt ON tnv.NVMa=vtbt.NVMa
                --########### Bat dau tinh lam them ngay thuong, di muon, ve som ####################
                
       LEFT JOIN (
                SELECT T1.NVMa,
                       SUM(T1.TGLamN_CT) AS TGLamN_CT,
                       SUM(T1.TGLamD_CT) AS TGLamD_CT,
                       SUM(T1.TGLamthemN_CT) / 60.00 AS TGLamthemN_CT,
                       SUM(T1.TGLamthemD_CT) / 60.00 AS TGLamthemD_CT,
                       SUM(T1.TGDimuonN_CT) / 60.00 AS TGDimuonN_CT,
                       SUM(T1.TGDimuonD_CT) / 60.00 AS TGDimuonD_CT,
                       SUM(T1.TGVesomN_CT) / 60.00 AS TGVesomN_CT,
                       SUM(T1.TGVesomD_CT) / 60.00 AS TGVesomD_CT,
                       SUM(
                           CASE 
                                WHEN (ISNULL(T1.TGDimuonN_CT, 0) + ISNULL(T1.TGDimuonD_CT, 0))>= 1 THEN 1
                                ELSE 0
                           END
                       ) AS SoLanDiMuon_CT,
                       SUM(
                           CASE 
                                WHEN (ISNULL(T1.TGVesomN_CT, 0) + ISNULL(T1.TGVesomD_CT, 0))>= 1 THEN 1
                                ELSE 0
                           END
                       ) AS SoLanVeSom_CT
                FROM   (
                           SELECT tnv.NVMa,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh 
                                                    <> 0 THEN tbc1.BCTGLamNgay 
                                                    /
                                                    CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh 
                                                    = 0 THEN tbc1.BCTGLamNgay 
                                                    /
                                                    480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS TGLamN_CT,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh 
                                                    <> 0 THEN tbc1.BCTGLamToi 
                                                    / CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh 
                                                    = 0 THEN tbc1.BCTGLamToi
                                                    / 480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS TGLamD_CT,
                                  (tbc1.BCTGThemNgay) AS TGLamthemN_CT,
                                  (tbc1.BCTGThemToi) AS TGLamthemD_CT,
                                  (tbc1.BCTGDiMuonNgay) AS TGDimuonN_CT,
                                  (tbc1.BCTGDiMuonToi) AS TGDimuonD_CT,
                                  (tbc1.BCTGVeSomNgay) AS TGVesomN_CT,
                                  (tbc1.BCTGVeSomToi) AS TGVesomD_CT
                           FROM   tblNhanVien tnv
                                  LEFT JOIN (
                                           SELECT *
                                           FROM   tblBaoCaoTam_CongLam tbc
                                           WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
                                                  AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11) 
                                       ) tbc1
                                       ON  tbc1.BCMaNV = tnv.NVMa
                                       AND tbc1.BCNgay>= tnv.NVNgayChinhThuc
                                       AND tbc1.BCNgay<=tnv.NVNgayra
                           WHERE  (
                                      (
                                          -- Xet dieu kien cua ca
                                          1 >= (
                                              CASE 
                                                   WHEN (
                                                            -- Che do mac dinh cua ca
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                                   -- So sanh ngay lam viec voi cac ngay trong bang dang ky nghi le
                                                        ) = 0 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        -- Het che do mac dinh ca ----------------------------------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 1 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        ------------------ Het che do ngay nghi thuong --------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong 150 %  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 2 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                   ELSE 2-- Khi ca thuoc loai 200 %, 300 % thi khong tinh
                                              END
                                          )
                                      )
                                      -- Xet dieu kien cua ngay chu nhat
                                      AND (
                                              DATEPART(
                                                  dw,
                                                  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                              ) <> (
                                                  SELECT CASE -- Khi ngay do co ca , Ca la loai ngay thuong
																--Loai ca mac dinh
																WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 0 THEN (
                                                                       SELECT CASE (
                                                                                       SELECT 
                                                                                              CONVERT(INT, CONVERT(VARCHAR, TSGiatri)) --+ ISNULL(tnv.NVLamCa, 0)
                                                                                       FROM   
                                                                                              tblThamSo 
                                                                                              tts
                                                                                       WHERE  
                                                                                              tts.TSTen = 
                                                                                              'NORMALWORKINGONSUNDAY'
                                                                                   )
                                                                                   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                                   WHEN 
                                                                                        1 THEN 
                                                                                        0
                                                                                   ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                              END
                                                                   )
                                                                   -- Loai ca ngay thuong
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 1 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   )
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 2 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   ) 
                                                                   
                                                                   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
                                                              ELSE CASE (
                                                                            SELECT 
                                                                                   CONVERT(INT, CONVERT(VARCHAR, TSGiatri))
                                                                            FROM   
                                                                                   tblThamSo 
                                                                                   tts
                                                                            WHERE  
                                                                                   tts.TSTen = 
                                                                                   'NORMALWORKINGONSUNDAY'
                                                                        )
                                                                        -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                        WHEN 1 THEN 
                                                                             0
                                                                        ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                   END
                                                         END
                                              )
                                          )
                                  )
                       ) T1
                GROUP BY
                       T1.NVMa
            ) CC_LT_DM_VS_Thuong
            ON  tnv.NVMa = CC_LT_DM_VS_Thuong.NVMa
GROUP BY
       tbp.BPMa,
       tnv.NVMa,
       tnv.NVMaNV,
       tnv.NVHoTen,
       tnv.NVNgayVao,
       tnv.NVngayra,
       CC_LT_DM_VS_Thuong.TGLamN_CT,
       CC_LT_DM_VS_Thuong.TGLamD_CT,
       CC_LT_DM_VS_Thuong.TGLamthemN_CT,
       CC_LT_DM_VS_Thuong.TGLamthemD_CT,
       CC_LT_DM_VS_Thuong.TGDimuonN_CT,
       CC_LT_DM_VS_Thuong.TGDimuonD_CT,
       CC_LT_DM_VS_Thuong.TGVesomN_CT,
       CC_LT_DM_VS_Thuong.TGVesomD_CT,
       CC_LT_DM_VS_Thuong.SoLanDiMuon_CT,
       CC_LT_DM_VS_Thuong.SoLanVeSom_CT
HAVING (1 = 1)
 AND  tbp.BPMa in (11)
GO
/****** Object:  View [dbo].[viewTaoBCCTV_CL_LT_DMVS]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCCTV_CL_LT_DMVS]
	AS
	SELECT tnv.NVMa as clNVMa_TV,
       ROUND((ISNULL(CC_LT_DM_VS_Thuong.TGLamN_CT, 0)),2) AS TGLamN_TV,
       ROUND((ISNULL(CC_LT_DM_VS_Thuong.TGLamD_CT, 0)),2) AS TGLamD_TV,
       ROUND(CC_LT_DM_VS_Thuong.TGLamthemN_CT,2) AS TGLamthemN_TV,
       ROUND(CC_LT_DM_VS_Thuong.TGLamthemD_CT,2) AS TGLamthemD_TV,
       ROUND(CC_LT_DM_VS_Thuong.TGDimuonN_CT, 2) AS TGDimuonN_TV,
       ROUND(CC_LT_DM_VS_Thuong.TGDimuonD_CT, 2) AS TGDimuonD_TV,
       ROUND(CC_LT_DM_VS_Thuong.TGVesomN_CT, 2) AS TGVesomN_TV,
       ROUND(CC_LT_DM_VS_Thuong.TGVesomD_CT, 2) AS TGVesomD_TV,
       CC_LT_DM_VS_Thuong.SoLanDiMuon_CT AS SoLanDiMuon_TV,
       CC_LT_DM_VS_Thuong.SoLanVeSom_CT AS SoLanVeSom_TV

FROM   tblNhanVien tnv
       INNER JOIN tblBoPhan tbp
            ON  tnv.NVMaBP = tbp.BPMa
                --LEFT JOIN viewTaoBCC_TungNgay vtbt ON tnv.NVMa=vtbt.NVMa
                --########### Bat dau tinh lam them ngay thuong, di muon, ve som ####################
                
       LEFT JOIN (
                SELECT T1.NVMa,
                       SUM(T1.TGLamN_CT) AS TGLamN_CT,
                       SUM(T1.TGLamD_CT) AS TGLamD_CT,
                       SUM(T1.TGLamthemN_CT) / 60.00 AS TGLamthemN_CT,
                       SUM(T1.TGLamthemD_CT) / 60.00 AS TGLamthemD_CT,
                       SUM(T1.TGDimuonN_CT) / 60.00 AS TGDimuonN_CT,
                       SUM(T1.TGDimuonD_CT) / 60.00 AS TGDimuonD_CT,
                       SUM(T1.TGVesomN_CT) / 60.00 AS TGVesomN_CT,
                       SUM(T1.TGVesomD_CT) / 60.00 AS TGVesomD_CT,
                       SUM(
                           CASE 
                                WHEN (ISNULL(T1.TGDimuonN_CT, 0) + ISNULL(T1.TGDimuonD_CT, 0))>= 1 THEN 1
                                ELSE 0
                           END
                       ) AS SoLanDiMuon_CT,
                       SUM(
                           CASE 
                                WHEN (ISNULL(T1.TGVesomN_CT, 0) + ISNULL(T1.TGVesomD_CT, 0))>= 1 THEN 1
                                ELSE 0
                           END
                       ) AS SoLanVeSom_CT
                FROM   (
                           SELECT tnv.NVMa,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh 
                                                    <> 0 THEN tbc1.BCTGLamNgay 
                                                    /
                                                    CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh 
                                                    = 0 THEN tbc1.BCTGLamNgay 
                                                    /
                                                    480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS TGLamN_CT,
                                  ROUND(
                                      (
                                          CASE 
                                               WHEN tbc1.BCTGQuyDinh 
                                                    <> 0 THEN tbc1.BCTGLamToi 
                                                    / CONVERT(FLOAT, tbc1.BCTGQuyDinh)
                                               WHEN tbc1.BCTGQuyDinh 
                                                    = 0 THEN tbc1.BCTGLamToi
                                                    / 480
                                               ELSE 0
                                          END
                                      ),
                                      2
                                  ) AS TGLamD_CT,
                                  (tbc1.BCTGThemNgay) AS TGLamthemN_CT,
                                  (tbc1.BCTGThemToi) AS TGLamthemD_CT,
                                  (tbc1.BCTGDiMuonNgay) AS TGDimuonN_CT,
                                  (tbc1.BCTGDiMuonToi) AS TGDimuonD_CT,
                                  (tbc1.BCTGVeSomNgay) AS TGVesomN_CT,
                                  (tbc1.BCTGVeSomToi) AS TGVesomD_CT
                           FROM   tblNhanVien tnv
                                  LEFT JOIN (
                                           SELECT *
                                           FROM   tblBaoCaoTam_CongLam tbc
                                           WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
                                                  AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11) 
                                       ) tbc1
                                       ON  tbc1.BCMaNV = tnv.NVMa
									   AND tbc1.BCNgay>= tnv.NVNgayVao
                                       AND tbc1.BCNgay<=tnv.NVNgayTV
                           WHERE  (
                                      (
                                          -- Xet dieu kien cua ca
                                          1 >= (
                                              CASE 
                                                   WHEN (
                                                            -- Che do mac dinh cua ca
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                                   -- So sanh ngay lam viec voi cac ngay trong bang dang ky nghi le
                                                        ) = 0 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        -- Het che do mac dinh ca ----------------------------------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 1 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        ------------------ Het che do ngay nghi thuong --------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong 150 %  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 2 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                   ELSE 2-- Khi ca thuoc loai 200 %, 300 % thi khong tinh
                                              END
                                          )
                                      )
                                      -- Xet dieu kien cua ngay chu nhat
                                      AND (
                                              DATEPART(
                                                  dw,
                                                  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                              ) <> (
                                                  SELECT CASE -- Khi ngay do co ca , Ca la loai ngay thuong
																--Loai ca mac dinh
																WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 0 THEN (
                                                                       SELECT CASE (
                                                                                       SELECT 
                                                                                              CONVERT(INT, CONVERT(VARCHAR, TSGiatri)) --+ ISNULL(tnv.NVLamCa, 0)
                                                                                       FROM   
                                                                                              tblThamSo 
                                                                                              tts
                                                                                       WHERE  
                                                                                              tts.TSTen = 
                                                                                              'NORMALWORKINGONSUNDAY'
                                                                                   )
                                                                                   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                                   WHEN 
                                                                                        1 THEN 
                                                                                        0
                                                                                   ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                              END
                                                                   )
                                                                   -- Loai ca ngay thuong
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 1 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   )
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 2 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   ) 
                                                                   
                                                                   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
                                                              ELSE CASE (
                                                                            SELECT 
                                                                                   CONVERT(INT, CONVERT(VARCHAR, TSGiatri))
                                                                            FROM   
                                                                                   tblThamSo 
                                                                                   tts
                                                                            WHERE  
                                                                                   tts.TSTen = 
                                                                                   'NORMALWORKINGONSUNDAY'
                                                                        )
                                                                        -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                        WHEN 1 THEN 
                                                                             0
                                                                        ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                   END
                                                         END
                                              )
                                          )
                                  )
                       ) T1
                GROUP BY
                       T1.NVMa
            ) CC_LT_DM_VS_Thuong
            ON  tnv.NVMa = CC_LT_DM_VS_Thuong.NVMa
GROUP BY
       tbp.BPMa,
       tnv.NVMa,
       tnv.NVMaNV,
       tnv.NVHoTen,
       tnv.NVNgayVao,
       tnv.NVngayra,
       CC_LT_DM_VS_Thuong.TGLamN_CT,
       CC_LT_DM_VS_Thuong.TGLamD_CT,
       CC_LT_DM_VS_Thuong.TGLamthemN_CT,
       CC_LT_DM_VS_Thuong.TGLamthemD_CT,
       CC_LT_DM_VS_Thuong.TGDimuonN_CT,
       CC_LT_DM_VS_Thuong.TGDimuonD_CT,
       CC_LT_DM_VS_Thuong.TGVesomN_CT,
       CC_LT_DM_VS_Thuong.TGVesomD_CT,
       CC_LT_DM_VS_Thuong.SoLanDiMuon_CT,
       CC_LT_DM_VS_Thuong.SoLanVeSom_CT
HAVING (1 = 1)
 AND  tbp.BPMa in (11)
GO
/****** Object:  View [dbo].[viewTaoBCC_LT_NgayNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_LT_NgayNghi]
	AS
SELECT T2.NVMa as ltNVMa,
       SUM(T2.TGLamthemcongtyN_CT) / 60.00 AS TGLamthemcongtyN,
       SUM(T2.TGLamthemcongtyD_CT) / 60.00 AS TGLamthemcongtyD,
       SUM(
       		CASE 
       			WHEN (
       					(((ISNULL(T2.BCCuaDen, 0)) >= 1)   OR  (isnull(T2.BCGhiChu,'') IN ('CN')))
       					 AND 
                           (DATEPART(dw, BCNgay)<>1)
       			) THEN 1 ELSE 0 END) AS 
       BCCSoNghiCongTy
FROM   (
           SELECT tnv.NVMa,
				  tnv.NVLamca,
                  (tbc1.BCTGLamNgay + tbc1.BCTGThemNgay) AS TGLamthemcongtyN_CT,
                  (tbc1.BCTGLamToi + tbc1.BCTGThemToi) AS TGLamthemcongtyD_CT,
                  tbc1.BCCuaDen,
                  tbc1.BCGhiChu,
                  tbc1.BCNgay
           FROM   tblNhanVien tnv
                  LEFT JOIN (
                  	               SELECT *
                FROM   tblBaoCao tbc
                WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
                       AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11)
            ) tbc1
             ON  tbc1.BCMaNV = tnv.NVMa
            WHERE  ((
                      1 = (
                          CASE 
                               WHEN (
                                        SELECT tc.CNgaynghi
                                        FROM   tblCa tc
                                        WHERE  tc.CMa = (
                                                   SELECT tbc.BCMaCa
                                                   FROM   tblBaoCao tbc
                                                   WHERE  tbc.BCNgay = tbc1.BCNgay
                                                          AND tbc.BCMaNV = tnv.NVMa
                                               )
                                    ) = 0 THEN CASE 
                                                    WHEN (
                                                             DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                   NNLNgay
                                                                                                                            FROM   
                                                                                                                                   dbo.tblNgayNghiLe AS 
                                                                                                                                   tnnl
                                                                                                                            WHERE  (NNLLoai = 2)
                                                                                                                           UNION 
                                                                                                                           ALL
                                                                                                                           SELECT 
                                                                                                                                  tnnlnv.NLNVNgay
                                                                                                                           FROM   
                                                                                                                                  tblNgayNghiLeNhanVien 
                                                                                                                                  tnnlnv
                                                                                                                           WHERE  
                                                                                                                                  tnnlnv.NNLLoai = 
                                                                                                                                  2
                                                                                                                                  AND 
                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                      tnv.NVMa)
                                                         ) THEN 1
                                                    ELSE 0
                                               END
                                -- Loai 200 %               
                               WHEN (
                                        SELECT tc.CNgaynghi
                                        FROM   tblCa tc
                                        WHERE  tc.CMa = (
                                                   SELECT tbc.BCMaCa
                                                   FROM   tblBaoCao tbc
                                                   WHERE  tbc.BCNgay = tbc1.BCNgay
                                                          AND tbc.BCMaNV = tnv.NVMa
                                               )
                                    ) = 3 THEN 1
                               ELSE CASE 
                                         WHEN (
                                                  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                        NNLNgay
                                                                                                                 FROM   
                                                                                                                        dbo.tblNgayNghiLe AS 
                                                                                                                        tnnl
                                                                                                                 WHERE  (NNLLoai = 2)
                                                                                                                UNION 
                                                                                                                ALL
                                                                                                                SELECT 
                                                                                                                       tnnlnv.NLNVNgay
                                                                                                                FROM   
                                                                                                                       tblNgayNghiLeNhanVien 
                                                                                                                       tnnlnv
                                                                                                                WHERE  
                                                                                                                       tnnlnv.NNLLoai = 
                                                                                                                       2
                                                                                                                       AND 
                                                                                                                           tnnlnv.NLNVMaNV = 
                                                                                                                           tnv.NVMa)
                                              ) THEN 1
                                         ELSE 0
                                    END
                          END
                      )
                  )
                  OR  (
                          DATEPART(
                              dw,
                              DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                          ) = (
                              SELECT CASE 
										WHEN (
                                                   SELECT tc.CNgaynghi
                                                   FROM   tblCa tc
                                                   WHERE  tc.CMa = (
                                                              SELECT tbc.BCMaCa
                                                              FROM   tblBaoCao 
                                                                     tbc
                                                              WHERE  tbc.BCNgay = 
                                                                     tbc1.BCNgay
                                                                     AND tbc.BCMaNV = 
                                                                         tnv.NVMa
                                                          )
                                               ) IN (1,2,4 )  THEN 0
                                          WHEN (
                                                   SELECT tc.CNgaynghi
                                                   FROM   tblCa tc
                                                   WHERE  tc.CMa = (
                                                              SELECT tbc.BCMaCa
                                                              FROM   tblBaoCao 
                                                                     tbc
                                                              WHERE  tbc.BCNgay = 
                                                                     tbc1.BCNgay
                                                                     AND tbc.BCMaNV = 
                                                                         tnv.NVMa
                                                          )
                                               ) = 3 THEN 1
                                               ELSE (
                                                   SELECT CASE (
                                                                   SELECT 
                                                                          CONVERT(VARCHAR, TSGiatri)
                                                                   FROM   
                                                                          tblThamSo 
                                                                          tts
                                                                   WHERE  tts.TSTen = 
                                                                         'NORMALWORKINGONSUNDAY'
                                                               )
                                                               WHEN 1 THEN 0
                                                               ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                          END
                                               )
                                         
                                     END AS Expr1
                          )
                  )
            ) AND (DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) NOT  IN (SELECT 
                                                                                             NNLNgay
                                                                                                                 FROM   
                                                                                                                        dbo.tblNgayNghiLe AS 
                                                                                                                        tnnl
                                                                                                                 WHERE  (NNLLoai <> 2)))
                  
       ) T2
GROUP BY
       T2.NVMa
GO
/****** Object:  View [dbo].[viewTaoBCC_LT_NgayLe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_LT_NgayLe]
	AS
SELECT T2.NVMa as ltnlNVMa,
       SUM(T2.TGLamthemcongtyN_CT) / 60.00 AS TGLamthemNgayLeN,
       SUM(T2.TGLamthemcongtyD_CT) / 60.00 AS TGLamthemNgayLeD,
       COUNT(T2.BCCuaDen) AS 
       BCCSoNghiLe
FROM   (
           SELECT tnv.NVMa,
                  (tbc1.BCTGLamNgay + tbc1.BCTGThemNgay) AS TGLamthemcongtyN_CT,
                  (tbc1.BCTGLamToi + tbc1.BCTGThemToi) AS TGLamthemcongtyD_CT,
                  tbc1.BCCuaDen
           FROM   tblNhanVien tnv
                  LEFT JOIN (
                  	               SELECT *
                FROM   tblBaoCao tbc
                WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
                       AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11)
            ) tbc1
             ON  tbc1.BCMaNV = tnv.NVMa
             AND BCNgay>=tnv.NVNgayVao AND BCNgay<=tnv.NVNgayRa
           WHERE  (
					(
						  (
							  1 = (
								  CASE -- Neu hom do cap ca, Ca cho phep tinh ngay do la Ngay Le
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
																																	WHERE  (NNLLoai = 1))
																 ) THEN 1
															ELSE 0
													   END
											-- Neu hom do cap ca, Ca cho ko cho phep tinh ngay do la Ngay Le
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
											) = 1 THEN 0
									   ELSE CASE 
												 WHEN (
														  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
																																NNLNgay
																														 FROM   
																																dbo.tblNgayNghiLe AS 
																																tnnl
																														 WHERE  (NNLLoai = 1))
													  ) THEN 1
												 ELSE 0
											END
								  END
							  )
						  )
					  )
                  )
       ) T2
GROUP BY
       T2.NVMa
GO
/****** Object:  View [dbo].[viewTaoBCC_Cong_NgayThuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_Cong_NgayThuong]
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
														WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
															   AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11)
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
																								  'NORMALWORKINGONSUNDAY'
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
																					   'NORMALWORKINGONSUNDAY'
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
														WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
															   AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11)
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
																								  'NORMALWORKINGONSUNDAY'
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
																					   'NORMALWORKINGONSUNDAY'
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
														WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
															   AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11)
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
																								  'NORMALWORKINGONSUNDAY'
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
																					   'NORMALWORKINGONSUNDAY'
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
														WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
															   AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11)
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
																								  'NORMALWORKINGONSUNDAY'
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
																					   'NORMALWORKINGONSUNDAY'
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
 AND  tbp.BPMa in (11)
GO
/****** Object:  View [dbo].[viewTaoBCC_TCTienAn]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_TCTienAn]
	AS
	SELECT tnv.NVMa AS tctaNVMa,
       ROUND(ISNULL(CC_TCTienAn.TCTienAn, 0), 2) AS TCTienAn

FROM   tblNhanVien tnv
       INNER JOIN tblBoPhan tbp
            ON  tnv.NVMaBP = tbp.BPMa
                --LEFT JOIN viewTaoBCC_TungNgay vtbt ON tnv.NVMa=vtbt.NVMa
                --########### Bat dau tinh lam them ngay thuong, di muon, ve som ####################
                
       LEFT JOIN (
                SELECT T1.NVMa,
                       COUNT(NVMa) AS TCTienAn
                FROM   (
                           SELECT tnv.NVMa
                           FROM   tblNhanVien tnv
                                  LEFT JOIN (
                                           SELECT *
                                           FROM   tblBaoCaoTam_CongLam tbc
                                           WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
                                                  AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11) 
                                       ) tbc1
                                       ON  tbc1.BCMaNV = tnv.NVMa
                           WHERE  
                           
							   (
							   	((isnull(tbc1.BCTGThemNgay,0)+ isnull(tbc1.BCTGThemToi,0))>=3*60 ) AND
							   	((isnull(tbc1.BCTGThemNgay,0)+ isnull(tbc1.BCTGThemToi,0))<=5*60 ) AND
                           		(
						   -- Xet dieu kien cua ca
                                          1 >= (
                                              CASE 
                                                   WHEN (
                                                            -- Che do mac dinh cua ca
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                                   -- So sanh ngay lam viec voi cac ngay trong bang dang ky nghi le
                                                        ) = 0 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        -- Het che do mac dinh ca ----------------------------------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 1 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                        ------------------ Het che do ngay nghi thuong --------------------
                                                        -- ############## Che do ca ngay nghi le ngay thuong 150 %  ##############
                                                   WHEN (
                                                            SELECT CONVERT(SMALLINT, ISNULL(tc.CNgaynghi, 0))
                                                            FROM   tblCa tc
                                                            WHERE  tc.CMa = (
                                                                       SELECT 
                                                                              tbc.BCMaCa
                                                                       FROM   
                                                                              tblBaoCaoTam_CongLam 
                                                                              tbc
                                                                       WHERE  
                                                                              tbc.BCNgay = 
                                                                              tbc1.BCNgay
                                                                              AND 
                                                                                  tbc.BCMaNV = 
                                                                                  tnv.NVMa
                                                                   )
                                                        ) = 2 THEN CASE 
                                                                        WHEN (
                                                                                 DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                       NNLNgay
                                                                                                                                                FROM   
                                                                                                                                                       dbo.tblNgayNghiLe AS 
                                                                                                                                                       tnnl
                                                                                                                                                WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                       OR  (tnnl.NNLLoai = 1)
                                                                                                                                               UNION 
                                                                                                                                               ALL 
                                                                                                                                               
                                                                                                                                               SELECT 
                                                                                                                                                      tnnlnv.NLNVNgay
                                                                                                                                               FROM   
                                                                                                                                                      tblNgayNghiLeNhanVien 
                                                                                                                                                      tnnlnv
                                                                                                                                               WHERE  
                                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                                      tnv.NVMa
                                                                                                                                                      AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                             ) THEN 
                                                                             2 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                        ELSE 1 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                   END
                                                   ELSE 2-- Khi ca thuoc loai 200 %, 300 % thi khong tinh
                                              END
                                          )
                                      )
                                      -- Xet dieu kien cua ngay chu nhat
                                      AND (
                                              DATEPART(
                                                  dw,
                                                  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                              ) <> (
                                                  SELECT CASE -- Khi ngay do co ca , Ca la loai ngay thuong
																--Loai ca mac dinh
																WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 0 THEN (
                                                                       SELECT CASE (
                                                                                       SELECT 
                                                                                              CONVERT(INT, CONVERT(VARCHAR, TSGiatri)) --+ ISNULL(tnv.NVLamCa, 0)
                                                                                       FROM   
                                                                                              tblThamSo 
                                                                                              tts
                                                                                       WHERE  
                                                                                              tts.TSTen = 
                                                                                              'NORMALWORKINGONSUNDAY'
                                                                                   )
                                                                                   -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                                   WHEN 
                                                                                        1 THEN 
                                                                                        0
                                                                                   ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                              END
                                                                   )
                                                                   -- Loai ca ngay thuong
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 1 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   )
                                                              WHEN (
                                                                       SELECT tc.CNgaynghi
                                                                       FROM   
                                                                              tblCa 
                                                                              tc
                                                                       WHERE  tc.CMa = (
                                                                                  SELECT 
                                                                                         tbc.BCMaCa
                                                                                  FROM   
                                                                                         tblBaoCaoTam_CongLam 
                                                                                         tbc
                                                                                  WHERE  
                                                                                         tbc.BCNgay = 
                                                                                         tbc1.BCNgay
                                                                                         AND 
                                                                                             tbc.BCMaNV = 
                                                                                             tnv.NVMa
                                                                              )
                                                                   ) = 2 THEN (
                                                                       CASE 
                                                                            WHEN (
                                                                                     DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                                           NNLNgay
                                                                                                                                                    FROM   
                                                                                                                                                           dbo.tblNgayNghiLe AS 
                                                                                                                                                           tnnl
                                                                                                                                                    WHERE  (tnnl.NNLLoai = 2)
                                                                                                                                                           OR  (tnnl.NNLLoai = 1)
                                                                                                                                                   UNION 
                                                                                                                                                   ALL 
                                                                                                                                                   
                                                                                                                                                   SELECT 
                                                                                                                                                          tnnlnv.NLNVNgay
                                                                                                                                                   FROM   
                                                                                                                                                          tblNgayNghiLeNhanVien 
                                                                                                                                                          tnnlnv
                                                                                                                                                   WHERE  
                                                                                                                                                          tnnlnv.NLNVMaNV = 
                                                                                                                                                          tnv.NVMa
                                                                                                                                                          AND ((NNLLoai = 2) OR (NNLLoai = 1)))
                                                                                 ) THEN 
                                                                                 1 -- Neu ton tai trong bang dang ky nghi le=> Loai
                                                                            ELSE 
                                                                                 0 -- Neu Ko ton tai trong bang dang ky nghi le=> OK
                                                                       END
                                                                   ) 
                                                                   
                                                                   -- Khi khong co ca thi kiem tra tham so Chu nhat lam bt
                                                              ELSE CASE (
                                                                            SELECT 
                                                                                   CONVERT(INT, CONVERT(VARCHAR, TSGiatri))
                                                                            FROM   
                                                                                   tblThamSo 
                                                                                   tts
                                                                            WHERE  
                                                                                   tts.TSTen = 
                                                                                   'NORMALWORKINGONSUNDAY'
                                                                        )
                                                                        -- Khi chu nhat lam binh thuong thi khong duoc tinh ngay nghi
                                                                        WHEN 1 THEN 
                                                                             0
                                                                        ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                   END
                                                         END
                                              )
                                          )
	                           
							   )
                       ) T1
                GROUP BY
                       T1.NVMa
            ) CC_TCTienAn
            ON  tnv.NVMa = CC_TCTienAn.NVMa
	GROUP BY
	tbp.BPMa,
       tbp.BPMa,
       tnv.NVMa,
       tnv.NVMaNV,
       tnv.NVHoTen,
       tnv.NVNgayVao,
       tnv.NVNgayRa,
       CC_TCTienAn.TCTienAn
HAVING (1=1)
 AND  tbp.BPMa in (11)
GO
/****** Object:  View [dbo].[viewTaoBCC_TCTienAnNgayNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_TCTienAnNgayNghi]
	AS
SELECT T2.NVMa as ltNVMa,
		COUNT(NVMa) AS ltTCTienAn
FROM   (
           SELECT tnv.NVMa

           FROM   tblNhanVien tnv
                  LEFT JOIN (
                  	               SELECT *
                FROM   tblBaoCao tbc
                WHERE  BCNgay <= 'Jan 31 2012 12:00AM'
                       AND BCNgay >= 'Jan  1 2012 12:00AM' AND  tbc.BCMaBP in (11)
            ) tbc1
             ON  tbc1.BCMaNV = tnv.NVMa
            WHERE  
					(
						((ISNULL(tbc1.BCTGLamNgay, 0) +  ISNULL(tbc1.BCTGLamToi, 0) + (isnull(tbc1.BCTGThemNgay,0)+ isnull(tbc1.BCTGThemToi,0))>=3*60 ) AND
						(ISNULL(tbc1.BCTGLamNgay, 0) +  ISNULL(tbc1.BCTGLamToi, 0) + (isnull(tbc1.BCTGThemNgay,0)+ isnull(tbc1.BCTGThemToi,0))<=5*60 ))
						Or
						
						((ISNULL(tbc1.BCTGLamNgay, 0) +  ISNULL(tbc1.BCTGLamToi, 0) + (isnull(tbc1.BCTGThemNgay,0)+ isnull(tbc1.BCTGThemToi,0))>=11*60 ) AND
						(ISNULL(tbc1.BCTGLamNgay, 0) +  ISNULL(tbc1.BCTGLamToi, 0) + (isnull(tbc1.BCTGThemNgay,0)+ isnull(tbc1.BCTGThemToi,0))<=13*60 ))
						
					) 
					AND
					(
					(
                      1 = (
                          CASE 
                               WHEN (
                                        SELECT tc.CNgaynghi
                                        FROM   tblCa tc
                                        WHERE  tc.CMa = (
                                                   SELECT tbc.BCMaCa
                                                   FROM   tblBaoCao tbc
                                                   WHERE  tbc.BCNgay = tbc1.BCNgay
                                                          AND tbc.BCMaNV = tnv.NVMa
                                               )
                                    ) = 0 THEN CASE 
                                                    WHEN (
                                                             DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                   NNLNgay
                                                                                                                            FROM   
                                                                                                                                   dbo.tblNgayNghiLe AS 
                                                                                                                                   tnnl
                                                                                                                            WHERE  (NNLLoai = 2)
                                                                                                                           UNION 
                                                                                                                           ALL
                                                                                                                           SELECT 
                                                                                                                                  tnnlnv.NLNVNgay
                                                                                                                           FROM   
                                                                                                                                  tblNgayNghiLeNhanVien 
                                                                                                                                  tnnlnv
                                                                                                                           WHERE  
                                                                                                                                  tnnlnv.NNLLoai = 
                                                                                                                                  2
                                                                                                                                  AND 
                                                                                                                                      tnnlnv.NLNVMaNV = 
                                                                                                                                      tnv.NVMa)
                                                         ) THEN 1
                                                    ELSE 0
                                               END
                                -- Loai 200 %               
                               WHEN (
                                        SELECT tc.CNgaynghi
                                        FROM   tblCa tc
                                        WHERE  tc.CMa = (
                                                   SELECT tbc.BCMaCa
                                                   FROM   tblBaoCao tbc
                                                   WHERE  tbc.BCNgay = tbc1.BCNgay
                                                          AND tbc.BCMaNV = tnv.NVMa
                                               )
                                    ) = 3 THEN 1
                               ELSE CASE 
                                         WHEN (
                                                  DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                        NNLNgay
                                                                                                                 FROM   
                                                                                                                        dbo.tblNgayNghiLe AS 
                                                                                                                        tnnl
                                                                                                                 WHERE  (NNLLoai = 2)
                                                                                                                UNION 
                                                                                                                ALL
                                                                                                                SELECT 
                                                                                                                       tnnlnv.NLNVNgay
                                                                                                                FROM   
                                                                                                                       tblNgayNghiLeNhanVien 
                                                                                                                       tnnlnv
                                                                                                                WHERE  
                                                                                                                       tnnlnv.NNLLoai = 
                                                                                                                       2
                                                                                                                       AND 
                                                                                                                           tnnlnv.NLNVMaNV = 
                                                                                                                           tnv.NVMa)
                                              ) THEN 1
                                         ELSE 0
                                    END
                          END
                      )
                  )
                  OR  (
                          DATEPART(
                              dw,
                              DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                          ) = (
                              SELECT CASE 
										WHEN (
                                                   SELECT tc.CNgaynghi
                                                   FROM   tblCa tc
                                                   WHERE  tc.CMa = (
                                                              SELECT tbc.BCMaCa
                                                              FROM   tblBaoCao 
                                                                     tbc
                                                              WHERE  tbc.BCNgay = 
                                                                     tbc1.BCNgay
                                                                     AND tbc.BCMaNV = 
                                                                         tnv.NVMa
                                                          )
                                               ) IN (1,2,4 )  THEN 0
                                          WHEN (
                                                   SELECT tc.CNgaynghi
                                                   FROM   tblCa tc
                                                   WHERE  tc.CMa = (
                                                              SELECT tbc.BCMaCa
                                                              FROM   tblBaoCao 
                                                                     tbc
                                                              WHERE  tbc.BCNgay = 
                                                                     tbc1.BCNgay
                                                                     AND tbc.BCMaNV = 
                                                                         tnv.NVMa
                                                          )
                                               ) = 3 THEN 1
                                               ELSE (
                                                   SELECT CASE (
                                                                   SELECT 
                                                                          CONVERT(VARCHAR, TSGiatri)
                                                                   FROM   
                                                                          tblThamSo 
                                                                          tts
                                                                   WHERE  tts.TSTen = 
                                                                         'NORMALWORKINGONSUNDAY'
                                                               )
                                                               WHEN 1 THEN 0
                                                               ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                          END
                                               )
                                         
                                     END AS Expr1
                          )
                  )
                  )
       ) T2
GROUP BY
       T2.NVMa
GO
/****** Object:  View [dbo].[viewTaoBCC_NgayKhongLam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW  [dbo].[viewTaoBCC_NgayKhongLam]
	AS
	
	
	SELECT nklMaNV,NgaykhongDL.NgayKL
FROM   tblNhanVien tnv
       INNER JOIN tblBoPhan tbp
            ON  tnv.NVMaBP = tbp.BPMa
       LEFT JOIN (
                SELECT BCMaNV AS nklMaNV,
                       COUNT(tbc1.BCNgay) AS NgayKL
                FROM   tblBaoCaoTam_NgayNghi tbc1
                       INNER JOIN tblNhanVien tnv
                              INNER JOIN tblBoPhan tbp
						ON  tnv.NVMaBP = tbp.BPMa
                            ON  tbc1.BCMaNV = tnv.NVMa
                WHERE   (
                               (
                               	ISNULL(tbc1.BCTGLamNgay, 0) + ISNULL(tbc1.BCTGLamToi, 0)
                               	+ISNULL(tbc1.BCTGThemNgay, 0) + ISNULL(tbc1.BCTGThemToi, 0)
                               ) 
                               <= 0
                           )
                       AND ISNULL(tbc1.BCGhiChu, '') = ''
                       AND tbc1.BCNgay >= 'Jan  1 2012 12:00AM'
                       AND tbc1.BCNgay <= 'Jan 31 2012 12:00AM' AND  tbp.BPMa in (11)
                       
                       AND (
                               (
                                   1 <> (
                                       CASE 
                                            WHEN (
                                                     SELECT tc.CNgaynghi
                                                     FROM   tblCa tc
                                                     WHERE  tc.CMa = (
                                                                SELECT tbc.BCMaCa
                                                                FROM   tblBaoCaoTam_NgayNghi 
                                                                       tbc
                                                                WHERE  tbc.BCNgay = 
                                                                       tbc1.BCNgay
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
                                                                                                                                        --WHERE  (NNLLoai = 2)
                                                                                                                                        UNION 
                                                                                                                                        ALL
                                                                                                                                        SELECT 
                                                                                                                                               tnnlnv.NLNVNgay
                                                                                                                                        FROM   
                                                                                                                                               tblNgayNghiLeNhanVien 
                                                                                                                                               tnnlnv
                                                                                                                                        WHERE  --tnnlnv.NNLLoai =
                                                                                                                                               --2
                                                                                                                                               --AND 
                                                                                                                                               tnnlnv.NLNVMaNV = 
                                                                                                                                               tnv.NVMa)
                                                                      ) THEN 1
                                                                 ELSE 0
                                                            END
                                            WHEN (
                                                     SELECT tc.CNgaynghi
                                                     FROM   tblCa tc
                                                     WHERE  tc.CMa = (
                                                                SELECT tbc.BCMaCa
                                                                FROM   tblBaoCaoTam_NgayNghi 
                                                                       tbc
                                                                WHERE  tbc.BCNgay = 
                                                                       tbc1.BCNgay
                                                                       AND tbc.BCMaNV = 
                                                                           tnv.NVMa
                                                            )
                                                 ) <> 0 THEN 1
                                            ELSE CASE 
                                                      WHEN (
                                                               DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay) IN (SELECT 
                                                                                                                                     NNLNgay
                                                                                                                              FROM   
                                                                                                                                     dbo.tblNgayNghiLe AS 
                                                                                                                                     tnnl
                                                                                                                             --WHERE  (NNLLoai = 2)
                                                                                                                             UNION 
                                                                                                                             ALL
                                                                                                                             SELECT 
                                                                                                                                    tnnlnv.NLNVNgay
                                                                                                                             FROM   
                                                                                                                                    tblNgayNghiLeNhanVien 
                                                                                                                                    tnnlnv
                                                                                                                             WHERE  --tnnlnv.NNLLoai =
                                                                                                                                    --2
                                                                                                                                    --AND 
                                                                                                                                    tnnlnv.NLNVMaNV = 
                                                                                                                                    tnv.NVMa)
                                                           ) THEN 1
                                                      ELSE 0
                                                 END
                                       END
                                   )
                               )
                               AND (
                                       DATEPART(
                                           dw,
                                           DATEADD(DAY, ISNULL(tbc1.BCNghiBuChoNgay, 0), tbc1.BCNgay)
                                       ) <> (
                                           SELECT CASE 
                                                       WHEN (
                                                                SELECT tc.CNgaynghi
                                                                FROM   tblCa tc
                                                                WHERE  tc.CMa = (
                                                                           SELECT 
                                                                                  tbc.BCMaCa
                                                                           FROM   
                                                                                  tblBaoCaoTam_NgayNghi 
                                                                                  tbc
                                                                           WHERE  
                                                                                  tbc.BCNgay = 
                                                                                  tbc1.BCNgay
                                                                                  AND 
                                                                                      tbc.BCMaNV = 
                                                                                      tnv.NVMa
                                                                       )
                                                            ) IN (1, 2,3, 4) THEN 
                                                            0
                                                  ELSE (
                                                                SELECT CASE (
                                                                                SELECT 
                                                                                       CONVERT(VARCHAR, TSGiatri)
                                                                                FROM   
                                                                                       tblThamSo 
                                                                                       tts
                                                                                WHERE  
                                                                                       tts.TSTen = 
                                                                                       'NORMALWORKINGONSUNDAY'
                                                                            )
                                                                            WHEN 
                                                                                 1 THEN 
                                                                                 0
                                                                            ELSE (1 - ISNULL(tnv.NVLamCa, 0))
                                                                       END
                                                            )
                                                  END AS Expr1
                                       )
                                   )
                           )
                GROUP BY
                       tbc1.BCMaNV
            ) NgaykhongDL
            ON  tnv.NVMa = NgaykhongDL.nklMaNV
	
WHERE (1=1)            
 AND  tbp.BPMa in (11)
GO
/****** Object:  Table [dbo].[HT_HeThong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_HeThong](
	[Logo] [image] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[MyView0_1]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/************************************************************
 * Code formatted by SoftTree SQL Assistant © v4.7.11
 * Time: 14/09/2013 14:45:12
 ************************************************************/

CREATE VIEW [dbo].[MyView0_1]
 AS 

SELECT tblNhanvien.NVMaNV,
       tblNhanvien.NVHoTen,
       tblNhanvien.NVNgaySinh,
       CASE 
            WHEN tblNhanvien.NVGioiTinh = 1 THEN 'Nam'
            ELSE N'Nữ'
       END AS NVGioiTinh,
       tblNhanvien.NVAnh,
       tblBoPhan.BPUuTien AS STTBP,
       tblBoPhan.BPTen AS TenBP,
       tblNhanvien.NVUuTien AS STTNV,
       tblChucVu.CVTen AS ChucVu,
       tblBoPhanCu.BPTen AS BPChuyenDi,
       tblBoPhanMoi.BPTen AS BPChuyenDen,
       NS_QuyetDinh.NgayHieuLuc AS NgayChuyen,
       NS_QuyetDinh.GhiChu AS GhiChu,
       (SELECT HT_HeThong.Logo FROM HT_HeThong ) AS Logo
FROM   NS_QuyetDinh
       INNER JOIN tblNhanvien
            ON  tblNhanvien.NVMa = NS_QuyetDinh.MaNV
       INNER JOIN tblBoPhan
            ON  tblNhanvien.NVMaBP = tblBoPhan.BPMa
       LEFT JOIN tblChucVu
            ON  tblNhanvien.NVMaCV = tblChucVu.CVMa
       LEFT JOIN tblBoPhan tblBoPhanCu
            ON  NS_QuyetDinh.PhongBan_Cu = tblBoPhanCu.BPMa
       LEFT JOIN tblBoPhan tblBoPhanMoi
            ON  NS_QuyetDinh.MaBP = tblBoPhanMoi.BPMa
WHERE  LoaiQD = 'QD_DieuChuyen' --{sCondition1}
GO
/****** Object:  Table [dbo].[tblQuocTich]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblQuocTich](
	[QTMa] [nvarchar](50) NOT NULL,
	[QTTen] [nvarchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDanToc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDanToc](
	[DTMa] [varchar](50) NOT NULL,
	[DTTen] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_tblDanToc] PRIMARY KEY CLUSTERED 
(
	[DTMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTonGiao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTonGiao](
	[TGMa] [nvarchar](50) NOT NULL,
	[TGTen] [nvarchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[ToKhaiBHXH]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[ToKhaiBHXH] AS 
SELECT TOP 100 PERCENT Upper(tblNhanVien.NVHoTen)  AS HoTen,
       CASE 
            WHEN tblnhanvien.NVGioiTinh = 1 THEN 'V'
            ELSE ''
       END AS GioiTinhNam,
       CASE 
            WHEN tblnhanvien.NVGioiTinh = 0 THEN 'V'
            ELSE ''
       END AS GioiTinhNu,
       
       tblnhanvien.NVNgaySinh AS Ngaysinh,
       tblChucVu.CVTen AS CVTen,
       tblDanToc.DTTen AS Dantoc,
       tbltongiao.TGTen AS TonGiao,
       tblQuocTich.QTTen AS Quoctich,
       tblnhanvien.NVNguyenQuan AS Nguyenquan,
       tblnhanvien.NVNoiThuongTru AS Thuongtru,
       tblnhanvien.NVSoCMTND,
       tblnhanvien.NVNgayCapCMTND,
       tblnhanvien.NVNoiCapCMTND,
       tblChucVu.CVTen AS ChucVu,
       HDLD.SoHD,
       HDLD.LoaiHD,
       HDLD.Ngayky,
       HDLD.TuNgay,
       HDLD.LuongBaoHiem
FROM   tblNhanVien
       INNER JOIN tblBoPhan
            ON  tblNhanVien.NVMaBP = tblBoPhan.BPMa
       LEFT JOIN tblChucVu
            ON  tblNhanVien.NVMaCV = tblChucVu.CVMa
       LEFT JOIN tblDanToc
            ON  tblNhanVien.NVMaDanToc = tblDanToc.DTMa
       LEFT JOIN tblTonGiao
            ON  tblnhanvien.NVMaTonGiao = tbltongiao.TGMa
       LEFT JOIN tblQuocTich
            ON  tblnhanvien.NVMaQuocTich = tblQuocTich.QTMa
       LEFT JOIN (
                SELECT MaNV,
                       NS_QuyetDinh.SoQD AS SoHD,
                       NS_LoaiHD.lhdTen AS LoaiHD,
                       NS_QuyetDinh.Ngayky,
                       NS_QuyetDinh.TuNgay,
                       NS_QuyetDinh.LuongBaoHiem
                FROM   NS_QuyetDinh
                       INNER JOIN NS_LoaiHD
                            ON  NS_LoaiHD.ldhGiatriTinh = NS_QuyetDinh.LoaiHDLD
                WHERE  NS_QuyetDinh.LoaiQD = 'HDLD'
            ) HDLD
            ON  tblNhanVien.NVMa = HDLD.MaNV 
WHERE (1=1)   AND tblNhanvien.NVMa = 2021
GO
/****** Object:  View [dbo].[ViewUpdateWT]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW  [dbo].[ViewUpdateWT] AS Select bcc.BCCID , 'P_CT' = Sum( CASE LNMa WHEN 1 THEN Songay_CT END), 'H_CT' = Sum( CASE LNMa WHEN 2 THEN Songay_CT END), 'KL_CT' = Sum( CASE LNMa WHEN 4 THEN Songay_CT END), 'BH100_CT' = Sum( CASE LNMa WHEN 5 THEN Songay_CT END), 'BH70_CT' = Sum( CASE LNMa WHEN 6 THEN Songay_CT END), 'NB_CT' = Sum( CASE LNMa WHEN 8 THEN Songay_CT END), 'H70_CT' = Sum( CASE LNMa WHEN 9 THEN Songay_CT END), 'CT_CT' = Sum( CASE LNMa WHEN 10 THEN Songay_CT END), 'P_TV' = Sum( CASE LNMa WHEN 1 THEN Songay_TV END), 'H_TV' = Sum( CASE LNMa WHEN 2 THEN Songay_TV END), 'KL_TV' = Sum( CASE LNMa WHEN 4 THEN Songay_TV END), 'BH100_TV' = Sum( CASE LNMa WHEN 5 THEN Songay_TV END), 'BH70_TV' = Sum( CASE LNMa WHEN 6 THEN Songay_TV END), 'NB_TV' = Sum( CASE LNMa WHEN 8 THEN Songay_TV END), 'H70_TV' = Sum( CASE LNMa WHEN 9 THEN Songay_TV END), 'CT_TV' = Sum( CASE LNMa WHEN 10 THEN Songay_TV END) From tblBangChamCong2009  bcc inner join TBangchamcong_LoaiNghi2009 ccln    on bcc.BCCID=ccln.BCCID  group by bcc.BCCID,bcc.BCCMaNV,bcc.BCCThang  having BCCThang=1
GO
/****** Object:  View [dbo].[Vluong2009_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW  [dbo].[Vluong2009_01] AS Select LMaNV , 'PCDilai' = Sum( CASE PCID WHEN 5 THEN PCgiatri END), 'PCDochai' = Sum( CASE PCID WHEN 6 THEN PCgiatri END) from LLuong2009 inner join Lluong2009_Phucap    on Lluong2009.LID=Lluong2009_Phucap.LID  group by Lmanv,Lthang having Lthang=1
GO
/****** Object:  View [dbo].[Vluong2009_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW  [dbo].[Vluong2009_02] AS Select LMaNV , 'PCDilai' = Sum( CASE PCID WHEN 5 THEN PCgiatri END), 'PCDochai' = Sum( CASE PCID WHEN 6 THEN PCgiatri END) from LLuong2009 inner join Lluong2009_Phucap    on Lluong2009.LID=Lluong2009_Phucap.LID  group by Lmanv,Lthang having Lthang=2
GO
/****** Object:  View [dbo].[Vluong2009_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW  [dbo].[Vluong2009_03] AS Select LMaNV , 'PCDilai' = Sum( CASE PCID WHEN 5 THEN PCgiatri END), 'PCDochai' = Sum( CASE PCID WHEN 6 THEN PCgiatri END) from LLuong2009 inner join Lluong2009_Phucap    on Lluong2009.LID=Lluong2009_Phucap.LID  group by Lmanv,Lthang having Lthang=3
GO
/****** Object:  View [dbo].[Vluong2009_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW  [dbo].[Vluong2009_04] AS Select LMaNV , 'PCDilai' = Sum( CASE PCID WHEN 5 THEN PCgiatri END), 'PCDochai' = Sum( CASE PCID WHEN 6 THEN PCgiatri END) from LLuong2009 inner join Lluong2009_Phucap    on Lluong2009.LID=Lluong2009_Phucap.LID  group by Lmanv,Lthang having Lthang=4
GO
/****** Object:  View [dbo].[Vluong2009_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE VIEW  [dbo].[Vluong2009_05] AS Select LMaNV , 'PCDilai' = Sum( CASE PCID WHEN 5 THEN PCgiatri END), 'PCDochai' = Sum( CASE PCID WHEN 6 THEN PCgiatri END) from LLuong2009 inner join Lluong2009_Phucap    on Lluong2009.LID=Lluong2009_Phucap.LID  group by Lmanv,Lthang having Lthang=5
GO
/****** Object:  View [dbo].[vThamsoluong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vThamsoluong] AS 				
SELECT     b1.TSLNgayApdung,b1.TSLNgayKetThuc,SUM(CASE b2.TSGTTen WHEN 'TSLCa1' THEN b2.TSGTGiatri END) AS [TSLCa1]
FROM         dbo.LThamSoLuong_Bang1 AS b1 INNER JOIN
                      dbo.LThamsoluong_Bang2 AS b2 ON  b1.TSLID=b2.TSGTID
GROUP BY b1.TSLID,b1.TSLNgayApdung,b1.TSLNgayKetThuc
Having b1.TSLID=1
GO
/****** Object:  Table [dbo].[Audit]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Audit](
	[Type] [char](1) NULL,
	[TableName] [varchar](128) NULL,
	[PK] [varchar](1000) NULL,
	[FieldName] [varchar](128) NULL,
	[OldValue] [varchar](1000) NULL,
	[NewValue] [varchar](1000) NULL,
	[UpdateDate] [datetime] NULL,
	[UserName] [varchar](128) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[bc_LamThem]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[bc_LamThem](
	[BCMaNV] [int] NOT NULL,
	[BCMaBP] [int] NOT NULL,
	[NVMaNV] [nvarchar](10) NOT NULL,
	[NVHoTen] [nchar](50) NOT NULL,
	[BPTen] [nvarchar](70) NOT NULL,
	[CVTen] [nvarchar](70) NULL,
	[21_N] [int] NULL,
	[21_D] [int] NULL,
	[22_N] [int] NULL,
	[22_D] [int] NULL,
	[23_N] [int] NULL,
	[23_D] [int] NULL,
	[24_N] [int] NULL,
	[24_D] [int] NULL,
	[25_N] [int] NULL,
	[25_D] [int] NULL,
	[26_N] [int] NULL,
	[26_D] [int] NULL,
	[27_N] [int] NULL,
	[27_D] [int] NULL,
	[28_N] [int] NULL,
	[28_D] [int] NULL,
	[29_N] [int] NULL,
	[29_D] [int] NULL,
	[30_N] [int] NULL,
	[30_D] [int] NULL,
	[31_N] [int] NULL,
	[31_D] [int] NULL,
	[Tong_TGLamThemN] [int] NULL,
	[Tong_TGLamThemD] [int] NULL,
	[Tong_TGLamThem] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_BangChamCong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_BangChamCong](
	[BCCID] [int] IDENTITY(1,1) NOT NULL,
	[BCCThang] [tinyint] NULL,
	[BCCNam] [int] NULL,
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
	[BCCCongN_CT] [float] NULL,
	[BCCCongD_CT] [float] NULL,
	[BCCLamThemN_CT] [float] NULL,
	[BCCLamThemD_CT] [float] NULL,
	[BCCLamThemCongTyN_CT] [float] NULL,
	[BCCLamThemCongTyD_CT] [float] NULL,
	[BCCLamThemLeN_CT] [float] NULL,
	[BCCLamThemLeD_CT] [float] NULL,
	[BCCDiMuonN_CT] [float] NULL,
	[BCCDiMuonD_CT] [float] NULL,
	[BCCVeSomN_CT] [float] NULL,
	[BCCVeSomD_CT] [float] NULL,
	[BCCSoLanDiMuon_CT] [tinyint] NULL,
	[BCCSoLanVeSom_CT] [tinyint] NULL,
	[BCCSoNghiLe_CT] [float] NULL,
	[BCCSoNghiCongTy_CT] [float] NULL,
	[BCCSoNghiThuong_CT] [float] NULL,
	[BCCTongCongTruLui_CT] [float] NULL,
	[BCCTongCong_CT] [float] NULL,
	[BCCSoCongKhongLam_CT] [float] NULL,
	[BCCSoCaHC_CT] [float] NULL,
	[BCCSoCa1_CT] [float] NULL,
	[BCCSoCa2_CT] [float] NULL,
	[BCCSoCa3_CT] [float] NULL,
	[BCCSoCaHCCN_CT] [float] NULL,
	[BCCSoCa1CN_CT] [float] NULL,
	[BCCSoCa2CN_CT] [float] NULL,
	[BCCSoCa3CN_CT] [float] NULL,
	[BCCSoCaHCNL_CT] [float] NULL,
	[BCCSoCa1NL_CT] [float] NULL,
	[BCCSoCa2NL_CT] [float] NULL,
	[BCCSoCa3NL_CT] [float] NULL,
	[BCCNghiPhep_CT] [float] NULL,
	[BCCNghiH100_CT] [float] NULL,
	[BCCNghiH70_CT] [float] NULL,
	[BCCNghiTNLD_CT] [float] NULL,
	[BCCNghiKL_CT] [float] NULL,
	[BCCNghiBH100_CT] [float] NULL,
	[BCCNghiBH70_CT] [float] NULL,
	[BCCNghiCongTac_CT] [float] NULL,
	[BCCNghiKhac_CT] [float] NULL,
	[BCCNghiBu_CT] [float] NULL,
	[BCCTCTienAn] [float] NULL,
	[BCCCongN_TV] [float] NULL,
	[BCCCongD_TV] [float] NULL,
	[BCCLamThemN_TV] [float] NULL,
	[BCCLamThemD_TV] [float] NULL,
	[BCCLamThemCongTyN_TV] [float] NULL,
	[BCCLamThemCongTyD_TV] [float] NULL,
	[BCCLamThemLeN_TV] [float] NULL,
	[BCCLamThemLeD_TV] [float] NULL,
	[BCCDiMuonN_TV] [float] NULL,
	[BCCDiMuonD_TV] [float] NULL,
	[BCCVeSomN_TV] [float] NULL,
	[BCCVeSomD_TV] [float] NULL,
	[BCCSoLanDiMuon_TV] [tinyint] NULL,
	[BCCSoLanVeSom_TV] [tinyint] NULL,
	[BCCSoNghiLe_TV] [float] NULL,
	[BCCSoNghiCongTy_TV] [float] NULL,
	[BCCSoNghiThuong_TV] [float] NULL,
	[BCCTongCong_TV] [float] NULL,
	[BCCSoCongKhongLam_TV] [float] NULL,
	[BCCSoCaHC_TV] [float] NULL,
	[BCCSoCa1_TV] [float] NULL,
	[BCCSoCa2_TV] [float] NULL,
	[BCCSoCa3_TV] [float] NULL,
	[BCCSoCaHCCN_TV] [float] NULL,
	[BCCSoCa1CN_TV] [float] NULL,
	[BCCSoCa2CN_TV] [float] NULL,
	[BCCSoCa3CN_TV] [float] NULL,
	[BCCSoCaHCNL_TV] [float] NULL,
	[BCCSoCa1NL_TV] [float] NULL,
	[BCCSoCa2NL_TV] [float] NULL,
	[BCCSoCa3NL_TV] [float] NULL,
	[BCCNghiPhep_TV] [float] NULL,
	[BCCNghiH100_TV] [float] NULL,
	[BCCNghiH70_TV] [float] NULL,
	[BCCNghiTNLD_TV] [float] NULL,
	[BCCNghiKL_TV] [float] NULL,
	[BCCNghiBH100_TV] [float] NULL,
	[BCCNghiBH70_TV] [float] NULL,
	[BCCNghiCongTac_TV] [float] NULL,
	[BCCNghiKhac_TV] [float] NULL,
	[BCCLoai] [bit] NOT NULL,
	[BCCRaNgoaiN_CT] [float] NULL,
	[BCCRaNgoaiD_CT] [float] NULL,
	[BCCCongThieu_CT] [float] NULL,
	[BCCSoNgayDiLam_CT] [float] NULL,
	[BCCSoNgayDiLam_TV] [float] NULL,
	[BCCCongChuanThang] [float] NULL,
	[BCCKhoaDuLieu] [bit] NULL,
	[BCCNEW_BH100] [float] NULL,
	[BCCNEW_BH70] [float] NULL,
	[BCCNEW_CO] [float] NULL,
	[BCCNEW_CT] [float] NULL,
	[BCCNEW_H70] [float] NULL,
	[BCCNEW_NB] [float] NULL,
	[BCCNEW_P] [float] NULL,
	[BCCNEW_R] [float] NULL,
	[BCCNEW_R0] [float] NULL,
	[BCCNEW_Ro] [float] NULL,
	[BCCNEW_B] [float] NULL,
	[BCCNEW_H100] [float] NULL,
	[BCCNEW_NK] [float] NULL,
	[BCCNEW_DS] [float] NULL,
	[BCCNEW_HH] [float] NULL,
	[BCCNEW_K] [float] NULL,
	[BCCNEW_N7] [float] NULL,
	[BCCNEW_NS] [float] NULL,
	[BCCNEW_O] [float] NULL,
	[BCCNEW_P/2] [float] NULL,
	[BCCNEW_V] [float] NULL,
	[BCCNEW_0.001] [float] NULL,
	[BCCNEW_QS] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_BangChamCongK]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_BangChamCongK](
	[BCCID] [int] IDENTITY(1,1) NOT NULL,
	[BCCThang] [tinyint] NULL,
	[BCCNam] [int] NULL,
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
	[BCCCongN_CT] [float] NULL,
	[BCCCongD_CT] [float] NULL,
	[BCCLamThemN_CT] [float] NULL,
	[BCCLamThemD_CT] [float] NULL,
	[BCCLamThemCongTyN_CT] [float] NULL,
	[BCCLamThemCongTyD_CT] [float] NULL,
	[BCCLamThemLeN_CT] [float] NULL,
	[BCCLamThemLeD_CT] [float] NULL,
	[BCCDiMuonN_CT] [float] NULL,
	[BCCDiMuonD_CT] [float] NULL,
	[BCCVeSomN_CT] [float] NULL,
	[BCCVeSomD_CT] [float] NULL,
	[BCCSoLanDiMuon_CT] [tinyint] NULL,
	[BCCSoLanVeSom_CT] [tinyint] NULL,
	[BCCSoNghiLe_CT] [float] NULL,
	[BCCSoNghiCongTy_CT] [float] NULL,
	[BCCSoNghiThuong_CT] [float] NULL,
	[BCCTongCongTruLui_CT] [float] NULL,
	[BCCTongCong_CT] [float] NULL,
	[BCCSoCongKhongLam_CT] [float] NULL,
	[BCCSoCaHC_CT] [float] NULL,
	[BCCSoCa1_CT] [float] NULL,
	[BCCSoCa2_CT] [float] NULL,
	[BCCSoCa3_CT] [float] NULL,
	[BCCSoCaHCCN_CT] [float] NULL,
	[BCCSoCa1CN_CT] [float] NULL,
	[BCCSoCa2CN_CT] [float] NULL,
	[BCCSoCa3CN_CT] [float] NULL,
	[BCCSoCaHCNL_CT] [float] NULL,
	[BCCSoCa1NL_CT] [float] NULL,
	[BCCSoCa2NL_CT] [float] NULL,
	[BCCSoCa3NL_CT] [float] NULL,
	[BCCNghiPhep_CT] [float] NULL,
	[BCCNghiH100_CT] [float] NULL,
	[BCCNghiH70_CT] [float] NULL,
	[BCCNghiTNLD_CT] [float] NULL,
	[BCCNghiKL_CT] [float] NULL,
	[BCCNghiBH100_CT] [float] NULL,
	[BCCNghiBH70_CT] [float] NULL,
	[BCCNghiCongTac_CT] [float] NULL,
	[BCCNghiKhac_CT] [float] NULL,
	[BCCNghiBu_CT] [float] NULL,
	[BCCTCTienAn] [float] NULL,
	[BCCCongN_TV] [float] NULL,
	[BCCCongD_TV] [float] NULL,
	[BCCLamThemN_TV] [float] NULL,
	[BCCLamThemD_TV] [float] NULL,
	[BCCLamThemCongTyN_TV] [float] NULL,
	[BCCLamThemCongTyD_TV] [float] NULL,
	[BCCLamThemLeN_TV] [float] NULL,
	[BCCLamThemLeD_TV] [float] NULL,
	[BCCDiMuonN_TV] [float] NULL,
	[BCCDiMuonD_TV] [float] NULL,
	[BCCVeSomN_TV] [float] NULL,
	[BCCVeSomD_TV] [float] NULL,
	[BCCSoLanDiMuon_TV] [tinyint] NULL,
	[BCCSoLanVeSom_TV] [tinyint] NULL,
	[BCCSoNghiLe_TV] [float] NULL,
	[BCCSoNghiCongTy_TV] [float] NULL,
	[BCCSoNghiThuong_TV] [float] NULL,
	[BCCTongCong_TV] [float] NULL,
	[BCCSoCongKhongLam_TV] [float] NULL,
	[BCCSoCaHC_TV] [float] NULL,
	[BCCSoCa1_TV] [float] NULL,
	[BCCSoCa2_TV] [float] NULL,
	[BCCSoCa3_TV] [float] NULL,
	[BCCSoCaHCCN_TV] [float] NULL,
	[BCCSoCa1CN_TV] [float] NULL,
	[BCCSoCa2CN_TV] [float] NULL,
	[BCCSoCa3CN_TV] [float] NULL,
	[BCCSoCaHCNL_TV] [float] NULL,
	[BCCSoCa1NL_TV] [float] NULL,
	[BCCSoCa2NL_TV] [float] NULL,
	[BCCSoCa3NL_TV] [float] NULL,
	[BCCNghiPhep_TV] [float] NULL,
	[BCCNghiH100_TV] [float] NULL,
	[BCCNghiH70_TV] [float] NULL,
	[BCCNghiTNLD_TV] [float] NULL,
	[BCCNghiKL_TV] [float] NULL,
	[BCCNghiBH100_TV] [float] NULL,
	[BCCNghiBH70_TV] [float] NULL,
	[BCCNghiCongTac_TV] [float] NULL,
	[BCCNghiKhac_TV] [float] NULL,
	[BCCLoai] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_CauhinhBCC]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_CauhinhBCC](
	[chMa] [nvarchar](50) NOT NULL,
	[chHienthiV] [nvarchar](50) NULL,
	[chHienthiE] [nvarchar](50) NULL,
	[Formula] [nvarchar](200) NULL,
	[Formula_EN] [nvarchar](200) NULL,
	[Formula_TQ] [nvarchar](200) NULL,
	[Formula_JP] [nvarchar](200) NULL,
	[Formula_OT] [nvarchar](200) NULL,
	[FormulaCT] [nvarchar](200) NULL,
	[FormulaCT_EN] [nvarchar](200) NULL,
	[FormulaCT_TQ] [nvarchar](200) NULL,
	[FormulaCT_JP] [nvarchar](200) NULL,
	[FormulaCT_OT] [nvarchar](200) NULL,
	[chSQL] [nvarchar](max) NULL,
	[chTruonghienthi] [nvarchar](50) NULL,
	[chDorong] [nvarchar](50) NULL,
	[chHienthi] [bit] NULL,
	[chUutien] [int] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_CC_CauhinhBCC] PRIMARY KEY CLUSTERED 
(
	[chMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_DanhMuc_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_DanhMuc_ChiTiet](
	[BiDanh] [varchar](50) NOT NULL,
	[STT] [float] NULL,
	[Truong] [nvarchar](50) NOT NULL,
	[KieuDuLieu] [nvarchar](20) NULL,
	[TenHienThi] [nvarchar](50) NULL,
	[DoRong] [real] NULL,
	[DieuKien] [nvarchar](50) NULL,
	[TuBang] [nvarchar](50) NULL,
	[TruongLuu] [nvarchar](50) NULL,
	[TruongHienThi] [nvarchar](50) NULL,
	[NgamDinh] [ntext] NULL,
	[DinhDangKieuSo] [nvarchar](50) NULL,
	[CanLe] [tinyint] NULL,
	[HienThi] [bit] NULL,
	[ChiDoc] [bit] NULL,
	[GhiChu] [nvarchar](250) NULL,
	[BatBuocNhap] [bit] NULL,
	[GiuLai] [bit] NULL,
	[TruongKhoa] [bit] NULL,
	[TuTang] [bit] NULL,
	[ToiDa] [int] NULL,
	[CamQuyenXem] [nvarchar](300) NULL,
	[SoThapPhan] [tinyint] NULL,
	[XauChuan] [bit] NULL,
	[GiaTriToiDa] [numeric](18, 0) NULL,
	[GiaTriToiThieu] [smallint] NULL,
	[CoDinh] [bit] NULL,
	[TenHienThiE] [nvarchar](400) NULL,
 CONSTRAINT [PK_CC_DanhMuc_ChiTiet] PRIMARY KEY CLUSTERED 
(
	[BiDanh] ASC,
	[Truong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_DSDangkythetudong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_DSDangkythetudong](
	[IDM] [int] NOT NULL,
	[UserID] [int] NULL,
	[Card] [nvarchar](10) NOT NULL,
	[IsDelCard] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_LichTrinhCa]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_LichTrinhCa](
	[Ma] [varchar](50) NOT NULL,
	[Ten] [nvarchar](200) NOT NULL,
	[PhanTheoThang] [bit] NOT NULL,
	[Ngay01] [varchar](200) NULL,
	[Ngay02] [varchar](200) NULL,
	[Ngay03] [varchar](200) NULL,
	[Ngay04] [varchar](200) NULL,
	[Ngay05] [varchar](200) NULL,
	[Ngay06] [varchar](200) NULL,
	[Ngay07] [varchar](200) NULL,
	[Ngay08] [varchar](200) NULL,
	[Ngay09] [varchar](200) NULL,
	[Ngay10] [varchar](200) NULL,
	[Ngay11] [varchar](200) NULL,
	[Ngay12] [varchar](200) NULL,
	[Ngay13] [varchar](200) NULL,
	[Ngay14] [varchar](200) NULL,
	[Ngay15] [varchar](200) NULL,
	[Ngay16] [varchar](200) NULL,
	[Ngay17] [varchar](200) NULL,
	[Ngay18] [varchar](200) NULL,
	[Ngay19] [varchar](200) NULL,
	[Ngay20] [varchar](200) NULL,
	[Ngay21] [varchar](200) NULL,
	[Ngay22] [varchar](200) NULL,
	[Ngay23] [varchar](200) NULL,
	[Ngay24] [varchar](200) NULL,
	[Ngay25] [varchar](200) NULL,
	[Ngay26] [varchar](200) NULL,
	[Ngay27] [varchar](200) NULL,
	[Ngay28] [varchar](200) NULL,
	[Ngay29] [varchar](200) NULL,
	[Ngay30] [varchar](200) NULL,
	[Ngay31] [varchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_CC_LichTrinhCa] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_LichtrinhChamcong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_LichtrinhChamcong](
	[ltccMa] [varchar](50) NOT NULL,
	[ltccTen] [nvarchar](200) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_LichTrinhVaoRa]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_LichTrinhVaoRa](
	[Ma] [varchar](50) NOT NULL,
	[Ten] [nvarchar](200) NOT NULL,
	[Loai] [varchar](50) NOT NULL,
	[ChonCapVaoRa] [varchar](50) NULL,
	[TGLonNhat] [int] NOT NULL,
	[TGNhoNhat] [int] NOT NULL,
	[KhoangGio] [int] NOT NULL,
	[TGKhoiTaoLaiTrangThai] [float] NOT NULL,
	[Lay1DLThangSau] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_CC_LichTrinhVaoRa] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_LoaiCa]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_LoaiCa](
	[LCMa] [nvarchar](50) NOT NULL,
	[LCTen] [nvarchar](200) NOT NULL,
	[LCGiaTri] [tinyint] NOT NULL,
 CONSTRAINT [PK_CC_LoaiCa] PRIMARY KEY CLUSTERED 
(
	[LCMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_LoaiDauDoc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_LoaiDauDoc](
	[Ma] [int] NOT NULL,
	[Ten] [nvarchar](50) NULL,
	[Loai] [nchar](10) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_CC_LoaiDauDoc] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_PhepTon]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_PhepTon](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NPNam] [int] NOT NULL,
	[NPMaNV] [int] NOT NULL,
	[NPTonNamTruoc] [float] NOT NULL,
	[NPCongVaoTongPhep] [bit] NOT NULL,
	[NPGhiChu] [nvarchar](50) NULL,
	[NPCongKhac] [float] NULL,
 CONSTRAINT [PK_CC_PhepNam] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_TrangthaiQuetthe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_TrangthaiQuetthe](
	[TTMa] [varchar](50) NOT NULL,
	[TTHienthi] [nvarchar](50) NOT NULL,
	[TTGiatri] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CC_Vitrithe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CC_Vitrithe](
	[UserID] [int] NOT NULL,
	[Card] [nvarchar](10) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CongthucLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CongthucLuong](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Tenbang] [nvarchar](50) NULL,
	[TenTruong] [nvarchar](50) NULL,
	[Dieukienloc] [nvarchar](200) NULL,
	[DienGiai] [nvarchar](200) NULL,
	[CongThuc] [text] NULL,
	[CauLenhSQL] [text] NULL,
	[PCID] [int] NULL,
	[LoaiTinhLuong] [nvarchar](50) NULL,
	[KieuDulieu] [nvarchar](50) NULL,
	[Dodai] [int] NULL,
	[ThutuTinh] [int] NULL,
	[HienThi] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CTL_CapnhatBangLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CTL_CapnhatBangLuong](
	[ColID] [varchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Field] [nvarchar](50) NOT NULL,
	[Display] [bit] NULL,
	[ColWidth] [int] NULL,
	[OrderBy] [float] NULL,
	[strSQL] [nvarchar](max) NULL,
	[Format] [nvarchar](10) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[isGroup] [bit] NULL,
	[strSQLGroup] [nvarchar](400) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CTL_CapnhatChamcong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CTL_CapnhatChamcong](
	[CCID] [varchar](10) NOT NULL,
	[CCTruongNguon] [nvarchar](200) NULL,
	[CCTruongDich] [nvarchar](200) NULL,
	[CCTen] [nvarchar](100) NULL,
	[CCDinhdang] [nvarchar](50) NULL,
	[CCHienthi] [bit] NULL,
	[CCCaulenhSQL] [nvarchar](max) NULL,
	[CCUutien] [int] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CTL_CapnhatChamcongT13]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CTL_CapnhatChamcongT13](
	[CCID] [varchar](10) NOT NULL,
	[CCTruongNguon] [nvarchar](200) NULL,
	[CCTruongDich] [nvarchar](200) NULL,
	[CCTen] [nvarchar](100) NULL,
	[CCDinhdang] [nvarchar](50) NULL,
	[CCHienthi] [bit] NULL,
	[CCCaulenhSQL] [nvarchar](max) NULL,
	[CCUutien] [int] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Delete_SoCai]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Delete_SoCai](
	[Xoa] [bit] NULL,
	[Lan] [tinyint] NULL,
	[LyDoXoaSua] [nvarchar](50) NULL,
	[LoaiCT] [nvarchar](15) NULL,
	[Chung_tu] [nvarchar](20) NULL,
	[Ngay] [smalldatetime] NULL,
	[Nhanvien] [nvarchar](15) NULL,
	[KhachHang] [nvarchar](15) NULL,
	[MST] [nvarchar](30) NULL,
	[NguoiMua] [nvarchar](50) NULL,
	[Diachi] [nvarchar](150) NULL,
	[Ghichu] [nvarchar](100) NULL,
	[Kho] [nvarchar](15) NULL,
	[PsNo] [numeric](18, 0) NULL,
	[PsCo] [numeric](18, 0) NULL,
	[NgayHenTra] [smalldatetime] NULL,
	[Gio] [nvarchar](5) NULL,
	[SoHoaDon] [nvarchar](20) NULL,
	[KhoanMuc] [nvarchar](10) NULL,
	[HangHoa] [nvarchar](15) NULL,
	[So_Luong] [numeric](18, 0) NULL,
	[DVT] [nvarchar](10) NULL,
	[Dongia] [money] NULL,
	[ChietKhau] [money] NULL,
	[KhuyenMai] [money] NULL,
	[ThanhTien] [money] NULL,
	[User1] [smallint] NULL,
	[date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_BaoCao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_BaoCao](
	[Ma] [varchar](50) NULL,
	[Macha] [varchar](50) NULL,
	[Caption] [nvarchar](250) NULL,
	[Caption_EN] [nvarchar](250) NULL,
	[Caption_TQ] [nvarchar](250) NULL,
	[Caption_JP] [nvarchar](250) NULL,
	[Caption_OT] [nvarchar](250) NULL,
	[ClassName] [varchar](50) NULL,
	[SelectedImageKey] [varchar](50) NULL,
	[ImageKey] [varchar](50) NULL,
	[ImageIndex] [varchar](50) NULL,
	[SelectedImageIndex] [varchar](50) NULL,
	[QuyenXem] [varchar](200) NULL,
	[LaBCDong] [bit] NULL,
	[InBC] [bit] NULL,
	[NhomTheoBoPhan] [bit] NULL,
	[NhomTheoNhanVien] [bit] NULL,
	[NhomTheoNgay] [bit] NULL,
	[ChophepExcel] [bit] NULL,
	[ChophepXemIn] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[CamHienThi] [nvarchar](300) NULL,
	[UuTien] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_BenhVien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_BenhVien](
	[Ma] [varchar](50) NOT NULL,
	[Ten] [nvarchar](200) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_CNganh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_CNganh](
	[CNganh] [varchar](10) NOT NULL,
	[Ten] [nvarchar](100) NOT NULL,
	[CachViet] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_CNganh] PRIMARY KEY CLUSTERED 
(
	[CNganh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_DoituongNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_DoituongNV](
	[Ma] [nvarchar](50) NOT NULL,
	[Ten] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DieuKien] [nvarchar](1000) NULL,
	[NgayChamCong] [smallint] NULL,
 CONSTRAINT [PK_DM_DoituongNV] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_DotDanhGia]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_DotDanhGia](
	[Ma] [varchar](50) NOT NULL,
	[Ten] [nvarchar](200) NULL,
	[Ngay] [date] NOT NULL,
	[GhiChu] [nvarchar](200) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_HocHam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_HocHam](
	[HocHam] [varchar](10) NOT NULL,
	[Ten] [varchar](30) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_HocHam] PRIMARY KEY CLUSTERED 
(
	[HocHam] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_HocVan]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_HocVan](
	[HocVan] [varchar](10) NOT NULL,
	[Ten] [nvarchar](50) NOT NULL,
	[STT] [tinyint] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[KyHieu] [varchar](10) NULL,
 CONSTRAINT [PK_DM_HocVan] PRIMARY KEY CLUSTERED 
(
	[HocVan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_HocVi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_HocVi](
	[HocVi] [varchar](10) NOT NULL,
	[Ten] [nvarchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_HocVi] PRIMARY KEY CLUSTERED 
(
	[HocVi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_KetQuaDanhGia]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_KetQuaDanhGia](
	[Ma] [nvarchar](50) NULL,
	[Ten] [nvarchar](50) NULL,
	[GhiChu] [nvarchar](50) NULL,
	[GiaTriTang] [int] NULL,
	[LoaiDanhGia] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_KetQuaDaoTao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_KetQuaDaoTao](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_KhoaHoc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_KhoaHoc](
	[Ma] [varchar](10) NOT NULL,
	[Ten] [nvarchar](100) NULL,
	[TuNgay] [smalldatetime] NULL,
	[DenNgay] [smalldatetime] NULL,
	[CNganh] [varchar](10) NULL,
	[HeDT] [varchar](10) NULL,
	[TruongDT] [varchar](10) NULL,
	[QuocGia] [varchar](10) NULL,
	[ChungChi] [varchar](10) NULL,
	[LoaiPhi] [varchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_KhuvucLamviec]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_KhuvucLamviec](
	[Ma] [nvarchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiBang]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiBang](
	[LoaiBang] [nvarchar](10) NOT NULL,
	[Ten] [nvarchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[Nhom] [varchar](20) NULL,
 CONSTRAINT [PK_DM_LoaiBang] PRIMARY KEY CLUSTERED 
(
	[LoaiBang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiBH]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiBH](
	[Ma] [varchar](50) NOT NULL,
	[Ten] [nvarchar](100) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[MaCha] [varchar](50) NULL,
	[KyHieu] [varchar](10) NULL,
	[UuTien] [int] NULL,
	[LoaiQD] [varchar](30) NULL,
 CONSTRAINT [PK_DM_LoaiBH] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiChucVu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiChucVu](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiDanhGia]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiDanhGia](
	[Ma] [nvarchar](50) NULL,
	[Ten] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiKhenThuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiKhenThuong](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[UuTien] [float] NULL,
	[HienThiBC] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiKyLuat]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiKyLuat](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[UuTien] [float] NULL,
	[HienThiBC] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiNghiViec]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiNghiViec](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[FileName] [nvarchar](400) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiPhi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiPhi](
	[Ma] [varchar](10) NOT NULL,
	[Ten] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiQD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiQD](
	[LoaiQD] [varchar](30) NOT NULL,
	[Ten] [nvarchar](100) NULL,
	[Loai] [tinyint] NULL,
	[TangNS] [bit] NOT NULL,
	[GiamNS] [bit] NOT NULL,
	[ThayLuong] [bit] NOT NULL,
	[DatQuyen] [varchar](200) NOT NULL,
	[FileName] [varchar](50) NULL,
	[NhomQD] [varchar](30) NULL,
	[STT] [tinyint] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[Deleted] [bit] NOT NULL,
	[KH] [varchar](50) NULL,
 CONSTRAINT [PK_DM_LoaiQD] PRIMARY KEY CLUSTERED 
(
	[LoaiQD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiThuMoiThuViec]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiThuMoiThuViec](
	[Ma] [nvarchar](50) NOT NULL,
	[Ten] [nvarchar](50) NULL,
	[FileName] [nvarchar](500) NULL,
 CONSTRAINT [PK_DM_LoaiThuMoiThuViec] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_LoaiTinhLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_LoaiTinhLuong](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](250) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_MauInThe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_MauInThe](
	[Ma] [varchar](10) NOT NULL,
	[Ten] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_NgoaiNgu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_NgoaiNgu](
	[NgoaiNgu] [varchar](10) NOT NULL,
	[Ten] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_NgoaiNgu] PRIMARY KEY CLUSTERED 
(
	[NgoaiNgu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_NhomBaoCao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_NhomBaoCao](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[Ten_EN] [nvarchar](250) NULL,
	[Ten_TQ] [nvarchar](250) NULL,
	[Ten_JP] [nvarchar](250) NULL,
	[Ten_OT] [nvarchar](250) NULL,
	[MaCha] [varchar](50) NULL,
	[QuyenXem] [varchar](200) NULL,
	[UuTien] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_NhomChucVu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_NhomChucVu](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_NhomTaiLieu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_NhomTaiLieu](
	[Ma] [nvarchar](50) NOT NULL,
	[Ten] [nvarchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_PhuongTien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_PhuongTien](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_PhuongtienDilai]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_PhuongtienDilai](
	[Ma] [nvarchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_QuanHeGD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_QuanHeGD](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_SuatAn]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_SuatAn](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[VietTat] [nvarchar](50) NULL,
	[Ten] [nvarchar](500) NULL,
	[TGBD] [datetime] NOT NULL,
	[TGKT] [datetime] NOT NULL,
	[SoTien] [money] NULL,
	[CaMacDinh] [varchar](200) NULL,
	[LoaiSuatAn] [int] NULL,
	[NgayDangKyChenhlech] [int] NULL,
	[NguongDangKy] [datetime] NULL,
	[SoNgayDangKyChenhLech] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_TDNgoaiNgu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_TDNgoaiNgu](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_ThamSoLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_ThamSoLuong](
	[TSLID] [varchar](50) NOT NULL,
	[TSLCa1] [float] NOT NULL,
	[TSLCa2] [float] NOT NULL,
	[TSLCa3] [float] NOT NULL,
	[TSLLamThem] [float] NOT NULL,
	[TSLChuNhat] [float] NOT NULL,
	[TSLNgayLe] [float] NOT NULL,
	[TSLBHXHNV] [float] NOT NULL,
	[TSLBHYTNV] [float] NOT NULL,
	[TSLBHTNNV] [float] NOT NULL,
	[TSLCongDoan] [float] NOT NULL,
	[TSLBHXHCT] [float] NOT NULL,
	[TSLBHYTCT] [float] NOT NULL,
	[TSLBHTNCT] [float] NOT NULL,
	[TSLNgayCongQD] [smallint] NOT NULL,
	[TSLHeSoNghi70] [float] NOT NULL,
	[TSLHeSoNghi100] [float] NOT NULL,
	[TSLHeSoThuViec] [real] NOT NULL,
	[TSLTCTienAn] [numeric](18, 0) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_ThamSoLuong] PRIMARY KEY CLUSTERED 
(
	[TSLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_Thongbao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_Thongbao](
	[Noidung] [nvarchar](250) NULL,
	[Ngaydang] [datetime] NULL,
	[Phongbanapdung] [varchar](250) NULL,
	[Nguoidang] [varchar](250) NULL,
	[Tungay] [datetime] NULL,
	[Denngay] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_Thuongbenhbinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_Thuongbenhbinh](
	[TBBinh] [varchar](50) NOT NULL,
	[Ten] [nvarchar](200) NULL,
 CONSTRAINT [PK_DM_Thuongbenhbinh] PRIMARY KEY CLUSTERED 
(
	[TBBinh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_TieuChiDanhGia]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_TieuChiDanhGia](
	[Ma] [nvarchar](50) NULL,
	[Ten] [nvarchar](50) NULL,
	[LoaiDanhGia] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_Tinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_Tinh](
	[Ma] [varchar](50) NOT NULL,
	[Ten] [nvarchar](50) NULL,
 CONSTRAINT [PK_DM_Tinh] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_TPBT]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_TPBT](
	[TPBT] [varchar](10) NOT NULL,
	[Ten] [nvarchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_TPBT] PRIMARY KEY CLUSTERED 
(
	[TPBT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_TPGD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_TPGD](
	[TPGD] [varchar](10) NOT NULL,
	[Ten] [nvarchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_TPGD] PRIMARY KEY CLUSTERED 
(
	[TPGD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_TruongDT]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_TruongDT](
	[TruongDT] [nvarchar](15) NOT NULL,
	[Ten] [nvarchar](200) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_TruongDT] PRIMARY KEY CLUSTERED 
(
	[TruongDT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_TuyenDung]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_TuyenDung](
	[Ma] [int] IDENTITY(1,1) NOT NULL,
	[Ten] [nvarchar](255) NULL,
	[Loai] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_VHPhothong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_VHPhothong](
	[VHPHT] [nvarchar](50) NOT NULL,
	[Ten] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DM_VHPhothong] PRIMARY KEY CLUSTERED 
(
	[VHPHT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_Vitinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_Vitinh](
	[Vitinh] [nvarchar](50) NOT NULL,
	[Ten] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_DM_Vitinh] PRIMARY KEY CLUSTERED 
(
	[Vitinh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DM_VungDiaLy]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DM_VungDiaLy](
	[Ma] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[F03_SyncQueue_Employee]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[F03_SyncQueue_Employee](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NVMaNV] [nvarchar](50) NOT NULL,
	[ChangeType] [char](1) NOT NULL,
	[QueuedAt] [datetime] NOT NULL,
	[Processed] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_BackUp]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_BackUp](
	[MayTram] [nvarchar](30) NULL,
	[DuongDan] [nvarchar](200) NULL,
	[NguoiDung] [smallint] NULL,
	[Ngay] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_CauhinhSQL]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_CauhinhSQL](
	[Parameter] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](250) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Condition]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Condition](
	[STT] [numeric](18, 0) NULL,
	[STT1] [numeric](18, 0) NULL,
	[FieldName] [nvarchar](50) NULL,
	[DataType] [nvarchar](50) NULL,
	[TableName] [nvarchar](50) NULL,
	[FieldDisplay] [nvarchar](50) NULL,
	[Caption] [nvarchar](50) NULL,
	[FormName] [nvarchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_DanhMuc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_DanhMuc](
	[STT] [smallint] NOT NULL,
	[BiDanh] [nvarchar](50) NOT NULL,
	[TenBang] [nvarchar](50) NOT NULL,
	[TenHienThi] [nvarchar](100) NOT NULL,
	[TruongKhoa] [nvarchar](50) NULL,
	[TruongSapXep] [nvarchar](255) NULL,
	[DieuKien] [nvarchar](max) NULL,
	[CamQuyen] [nvarchar](200) NULL,
	[CamHienThi] [nvarchar](200) NULL,
	[TruongTimKiem] [varchar](50) NULL,
	[QuyenThemMoi] [ntext] NULL,
	[QuyenSua] [ntext] NULL,
	[HeThong] [bit] NOT NULL,
	[TruongQuanHe] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[EnterToFind] [bit] NOT NULL,
	[QuyenXoa] [nvarchar](250) NULL,
	[TenHienThiE] [nvarchar](500) NULL,
 CONSTRAINT [PK_HT_DanhMuc] PRIMARY KEY CLUSTERED 
(
	[BiDanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_DanhMuc_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_DanhMuc_ChiTiet](
	[BiDanh] [varchar](50) NOT NULL,
	[STT] [float] NULL,
	[Truong] [nvarchar](50) NOT NULL,
	[KieuDuLieu] [nvarchar](20) NULL,
	[TenHienThi] [nvarchar](50) NULL,
	[DoRong] [real] NULL,
	[DieuKien] [nvarchar](50) NULL,
	[TuBang] [nvarchar](50) NULL,
	[TruongLuu] [nvarchar](50) NULL,
	[TruongHienThi] [nvarchar](50) NULL,
	[NgamDinh] [ntext] NULL,
	[DinhDangKieuSo] [nvarchar](50) NULL,
	[CanLe] [tinyint] NULL,
	[HienThi] [bit] NULL,
	[ChiDoc] [bit] NULL,
	[GhiChu] [nvarchar](250) NULL,
	[BatBuocNhap] [bit] NULL,
	[GiuLai] [bit] NULL,
	[TruongKhoa] [bit] NULL,
	[TuTang] [bit] NULL,
	[ToiDa] [int] NULL,
	[CamQuyenXem] [nvarchar](300) NULL,
	[SoThapPhan] [tinyint] NULL,
	[XauChuan] [bit] NULL,
	[GiaTriToiDa] [numeric](18, 0) NULL,
	[GiaTriToiThieu] [smallint] NULL,
	[CoDinh] [bit] NULL,
	[TenHienThiE] [nvarchar](500) NULL,
 CONSTRAINT [PK_HT_DanhMuc_ChiTiet] PRIMARY KEY CLUSTERED 
(
	[BiDanh] ASC,
	[Truong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_DSBaocao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_DSBaocao](
	[BCBidanh] [nvarchar](50) NOT NULL,
	[BCTen] [nvarchar](50) NULL,
	[BCDuongdan] [nvarchar](50) NULL,
	[BCTenbang] [nvarchar](50) NULL,
	[BCTenThutuc] [nvarchar](50) NULL,
	[BCDSThamso] [nvarchar](50) NULL,
	[BCDSGiatri] [nvarchar](max) NULL,
	[BCCaulenhSQL] [nvarchar](max) NULL,
	[BCThutu] [int] NULL,
	[BCLoaiBC] [nvarchar](50) NULL,
 CONSTRAINT [PK_HT_DSBaocao] PRIMARY KEY CLUSTERED 
(
	[BCBidanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Export]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Export](
	[TableNameGroup] [varchar](50) NULL,
	[TableName] [varchar](50) NOT NULL,
	[KeyField] [varchar](50) NULL,
	[RemoveField] [nvarchar](50) NULL,
	[TypeExportData] [tinyint] NULL,
	[Condition] [nvarchar](max) NULL,
	[Caption] [nvarchar](200) NULL,
	[Caption_EN] [nvarchar](200) NULL,
	[Caption_TQ] [nvarchar](200) NULL,
	[Caption_JP] [nvarchar](200) NULL,
	[Caption_OT] [nvarchar](200) NULL,
	[GroupID] [smallint] NULL,
	[DongBoDL] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_ExportExcel]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_ExportExcel](
	[ExcelCode] [nvarchar](50) NOT NULL,
	[TableList] [nvarchar](200) NULL,
	[SQL] [nvarchar](max) NULL,
	[Name] [nvarchar](200) NULL,
	[Sapxep] [nvarchar](max) NULL,
	[DieuKienThoiGian] [nvarchar](100) NULL,
	[DieuKienThem] [nvarchar](200) NULL,
	[CamQuyen] [nvarchar](200) NULL,
	[CamHienThi] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DatQuyen] [nvarchar](200) NULL,
 CONSTRAINT [PK_HT_ExportExcel1] PRIMARY KEY CLUSTERED 
(
	[ExcelCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_ExportExcel_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_ExportExcel_ChiTiet](
	[ExcelCode] [nvarchar](50) NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[ColName] [nvarchar](50) NOT NULL,
	[Uutien] [int] NULL,
	[CaulenhSQL] [nvarchar](200) NULL,
	[OK] [bit] NULL,
	[Original] [nvarchar](250) NOT NULL,
	[Caption] [nvarchar](250) NOT NULL,
	[Dorong] [int] NULL,
	[DoRongtheoExcel] [bit] NULL,
	[Caption_EN] [nvarchar](250) NULL,
	[Caption_TQ] [nvarchar](250) NULL,
	[Caption_JP] [nvarchar](250) NULL,
	[Caption_OT] [nvarchar](250) NULL,
 CONSTRAINT [PK_HT_ExcelExport] PRIMARY KEY CLUSTERED 
(
	[ExcelCode] ASC,
	[ColName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_ExportExcel_Condition]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_ExportExcel_Condition](
	[TT] [tinyint] NULL,
	[Ma] [nvarchar](100) NULL,
	[Ten] [nvarchar](100) NULL,
	[KieuDuLieu] [nvarchar](100) NULL,
	[TuBang] [nvarchar](100) NULL,
	[TruongLuu] [nvarchar](100) NULL,
	[TruongHienThi] [nvarchar](100) NULL,
	[BatBuoc] [bit] NOT NULL,
	[NgamDinh] [nvarchar](200) NULL,
	[TruongDieuKien] [nvarchar](100) NULL,
	[BieuThucSoSanh] [nvarchar](20) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Form]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Form](
	[Form] [nvarchar](400) NOT NULL,
	[TieuDe] [nvarchar](1000) NULL,
	[Gioithieu] [nvarchar](1000) NOT NULL,
	[Gioithieu_EN] [nvarchar](250) NULL,
	[Gioithieu_TQ] [nvarchar](250) NULL,
	[Gioithieu_JP] [nvarchar](250) NULL,
	[Gioithieu_OT] [nvarchar](250) NULL,
	[ChuThich] [nvarchar](max) NULL,
	[ChuThich_EN] [nvarchar](250) NULL,
	[ChuThich_TQ] [nvarchar](250) NULL,
	[ChuThich_JP] [nvarchar](250) NULL,
	[ChuThich_OT] [nvarchar](250) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[TieuDe_EN] [nvarchar](250) NULL,
	[TieuDe_TQ] [nvarchar](250) NULL,
	[TieuDe_JP] [nvarchar](250) NULL,
	[TieuDe_OT] [nvarchar](250) NULL,
 CONSTRAINT [PK_HT_Form] PRIMARY KEY CLUSTERED 
(
	[Form] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_GiaoDien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_GiaoDien](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[Form] [nvarchar](200) NOT NULL,
	[ControlName] [nvarchar](250) NOT NULL,
	[Original] [nvarchar](250) NOT NULL,
	[Caption] [nvarchar](250) NOT NULL,
	[Caption_EN] [nvarchar](250) NULL,
	[Caption_TQ] [nvarchar](250) NULL,
	[Caption_JP] [nvarchar](250) NULL,
	[Caption_OT] [nvarchar](250) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_GiaoDien_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_GiaoDien_ChiTiet](
	[Form] [nvarchar](200) NOT NULL,
	[ControlName] [nvarchar](250) NOT NULL,
	[ControlNameDetail] [nvarchar](250) NOT NULL,
	[Caption] [nvarchar](250) NOT NULL,
	[Caption_EN] [nvarchar](250) NULL,
	[Caption_TQ] [nvarchar](250) NULL,
	[Caption_JP] [nvarchar](250) NULL,
	[Caption_OT] [nvarchar](250) NULL,
	[Title] [nvarchar](250) NULL,
	[Title_EN] [nvarchar](250) NULL,
	[Title_TQ] [nvarchar](250) NULL,
	[Title_JP] [nvarchar](250) NULL,
	[Title_OT] [nvarchar](250) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Grid]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Grid](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Form] [varchar](200) NULL,
	[Grid] [varchar](200) NULL,
	[TT] [int] NOT NULL,
	[TenCot] [varchar](200) NULL,
	[CanLe] [tinyint] NOT NULL,
	[Caption] [nvarchar](200) NULL,
	[Caption_EN] [nvarchar](200) NULL,
	[Caption_TQ] [nvarchar](200) NULL,
	[Caption_JP] [nvarchar](200) NULL,
	[Caption_OT] [nvarchar](200) NULL,
	[DinhDangSo] [varchar](10) NULL,
	[DoRong] [numeric](6, 2) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DinhDang] [nvarchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Grid_Config]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Grid_Config](
	[Form] [varchar](30) NULL,
	[Grid] [varchar](30) NULL,
	[AllowAddNew] [bit] NULL,
	[AllowDelete] [bit] NULL,
	[SelectionMode] [smallint] NULL,
	[AllowFreezing] [smallint] NULL,
	[AllowResizing] [smallint] NULL,
	[Cols_Fixed] [smallint] NULL,
	[Cols_Frozen] [smallint] NULL,
	[Rows_Fixed] [smallint] NULL,
	[Rows_Frozen] [smallint] NULL,
	[ExtendLastCol] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_GridSize]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_GridSize](
	[FormName] [nvarchar](60) NOT NULL,
	[GridName] [nvarchar](60) NOT NULL,
	[ColumnName] [nvarchar](60) NOT NULL,
	[Width] [smallint] NULL,
 CONSTRAINT [PK_HT_GridSize] PRIMARY KEY CLUSTERED 
(
	[FormName] ASC,
	[GridName] ASC,
	[ColumnName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Import]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Import](
	[TableName] [nvarchar](50) NOT NULL,
	[LastUpdate] [datetime] NULL,
 CONSTRAINT [PK_HT_Import] PRIMARY KEY CLUSTERED 
(
	[TableName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_ImportExcel]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_ImportExcel](
	[BiDanh] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[TenBang] [nvarchar](200) NULL,
	[FontNguon] [nvarchar](200) NULL,
	[Tuhang] [varchar](50) NULL,
	[Denhang] [varchar](50) NULL,
	[TuCot] [varchar](50) NULL,
	[DenCot] [varchar](50) NULL,
	[Loai] [varchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DatQuyen] [nvarchar](200) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_ImportExcel_Chitiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_ImportExcel_Chitiet](
	[BiDanh] [varchar](50) NULL,
	[CotExcel] [int] NULL,
	[TieuDeCotExcel] [nvarchar](200) NULL,
	[TruongDuLieu] [varchar](50) NULL,
	[TuBang] [varchar](50) NULL,
	[TruongLuu] [varchar](50) NULL,
	[TruongHienThi] [varchar](50) NULL,
	[TuTang] [bit] NULL,
	[CamTrung] [bit] NULL,
	[LatruongSoSanh] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_LoaiBaoCao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_LoaiBaoCao](
	[LBCMa] [nvarchar](50) NOT NULL,
	[Caption] [nvarchar](250) NOT NULL,
	[Caption_EN] [nvarchar](250) NULL,
	[Caption_TQ] [nvarchar](250) NULL,
	[Caption_JP] [nvarchar](250) NULL,
	[Caption_OT] [nvarchar](250) NULL,
	[Uutien] [int] NULL,
	[Hienthi] [bit] NULL,
	[LBCMaCha] [nvarchar](50) NULL,
 CONSTRAINT [PK_HT_LoaiBaoCao] PRIMARY KEY CLUSTERED 
(
	[LBCMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_MayTram]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_MayTram](
	[TenMay] [nvarchar](30) NOT NULL,
	[NgayCapNhatCuoi] [datetime] NULL,
	[Font] [nvarchar](30) NULL,
	[NgayLuu] [nvarchar](30) NULL,
	[DuongDanLogo] [nvarchar](100) NULL,
	[NgayKhoaSo] [smalldatetime] NULL,
	[TuDongNangCap] [bit] NULL,
	[NhatKyNguoiDung] [bit] NULL,
	[NhapDauKy] [bit] NULL,
	[GiaoDien] [nvarchar](20) NULL,
	[BackupDir] [nvarchar](250) NULL,
	[Font_Interface] [nvarchar](30) NULL,
	[DangKy] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Menu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Menu](
	[Ma] [varchar](50) NOT NULL,
	[STT] [int] NULL,
	[Hethong] [bit] NULL,
	[Sapxep] [varchar](50) NULL,
	[Caption] [nvarchar](250) NULL,
	[Caption_EN] [nvarchar](250) NULL,
	[Caption_TQ] [nvarchar](250) NULL,
	[Caption_JP] [nvarchar](250) NULL,
	[Caption_OT] [nvarchar](250) NULL,
	[CamHienThi] [nvarchar](200) NULL,
	[CamQuyen] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_HT_Menu] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_NgonguKhac]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_NgonguKhac](
	[Original] [nvarchar](250) NOT NULL,
	[Caption] [nvarchar](250) NOT NULL,
	[Caption_EN] [nvarchar](250) NULL,
	[Caption_TQ] [nvarchar](250) NULL,
	[Caption_JP] [nvarchar](250) NULL,
	[Caption_OT] [nvarchar](250) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_NhapLieu_GiaTri]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_NhapLieu_GiaTri](
	[ValuesNo] [varchar](100) NOT NULL,
	[ValuesName] [nvarchar](4000) NOT NULL,
 CONSTRAINT [PK_HT_NhapLieu_GiaTri] PRIMARY KEY CLUSTERED 
(
	[ValuesNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_NhapLieu_Truong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_NhapLieu_Truong](
	[FieldsNo] [varchar](100) NOT NULL,
	[FieldsName] [nchar](50) NOT NULL,
 CONSTRAINT [PK_HT_NhapLieu_Truong] PRIMARY KEY CLUSTERED 
(
	[FieldsNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_NhatKy]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_NhatKy](
	[Ma_User] [smallint] NULL,
	[MayTram] [nvarchar](30) NULL,
	[NgayGio] [nvarchar](30) NULL,
	[Ngay] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Report]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Report](
	[ID] [nchar](10) NOT NULL,
	[ReportKey] [nvarchar](50) NOT NULL,
	[Formula] [nvarchar](50) NOT NULL,
	[Original] [nvarchar](1000) NOT NULL,
	[Caption] [nvarchar](1000) NOT NULL,
	[Caption_EN] [nvarchar](1000) NULL,
	[Caption_TQ] [nvarchar](1000) NULL,
	[Caption_JP] [nvarchar](1000) NULL,
	[Caption_OT] [nvarchar](1000) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_Shortcut]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_Shortcut](
	[HTSC_BiDanh] [nvarchar](100) NOT NULL,
	[Caption] [nvarchar](200) NULL,
	[Caption_EN] [nvarchar](200) NULL,
	[Caption_TQ] [nvarchar](200) NULL,
	[Caption_JP] [nvarchar](200) NULL,
	[Caption_OT] [nvarchar](200) NULL,
	[HTSC_ClassName] [nvarchar](200) NULL,
	[HTSC_Uutien] [int] NULL,
	[CamQuyen] [nvarchar](200) NULL,
	[CamHienThi] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[CamThemMoi] [ntext] NULL,
	[CamSua] [ntext] NULL,
	[CamXoa] [ntext] NULL,
 CONSTRAINT [PK_HT_Shortcut] PRIMARY KEY CLUSTERED 
(
	[HTSC_BiDanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_SQL]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_SQL](
	[Ma] [varchar](50) NULL,
	[Ten] [varchar](100) NULL,
	[GiaTri] [text] NULL,
	[DienGiai] [varchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_ThongBao]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_ThongBao](
	[Ma] [varchar](50) NULL,
	[NoiDung] [nvarchar](2000) NULL,
	[ThoiGian] [datetime] NULL,
	[UserUpdated] [varchar](1000) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HT_ThongbaoCapnhat]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HT_ThongbaoCapnhat](
	[TBValues] [nvarchar](4000) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HTN_DanhMuc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HTN_DanhMuc](
	[STT] [smallint] NOT NULL,
	[BiDanh] [nvarchar](50) NOT NULL,
	[TenBang] [nvarchar](50) NOT NULL,
	[TenHienThi] [nvarchar](100) NOT NULL,
	[TruongKhoa] [nvarchar](50) NOT NULL,
	[TruongSapXep] [nvarchar](255) NOT NULL,
	[TruongDKBP] [nvarchar](50) NOT NULL,
	[TruongDKNV] [nvarchar](50) NOT NULL,
	[DieuKien] [nvarchar](max) NOT NULL,
	[SELECT_Them] [varchar](256) NOT NULL,
	[FROM_Them] [varchar](1000) NOT NULL,
	[HienThiV_Them] [nvarchar](256) NOT NULL,
	[HienThiE_Them] [nvarchar](256) NOT NULL,
	[CamQuyen] [nvarchar](200) NOT NULL,
	[CamHienThi] [nvarchar](200) NOT NULL,
	[TruongTimKiem] [varchar](50) NOT NULL,
	[QuyenThemMoi] [ntext] NULL,
	[QuyenSua] [ntext] NULL,
	[QuyenXoa] [ntext] NULL,
	[HeThong] [bit] NOT NULL,
	[TruongQuanHe] [nvarchar](50) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[EnterToFind] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HTN_DanhMuc_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HTN_DanhMuc_ChiTiet](
	[BiDanh] [varchar](50) NOT NULL,
	[STT] [decimal](2, 0) NULL,
	[Truong] [nvarchar](50) NOT NULL,
	[TruongSQL] [nvarchar](200) NULL,
	[KieuDuLieu] [nvarchar](20) NULL,
	[TenHienThi] [nvarchar](50) NULL,
	[TenHienThi_E] [nvarchar](50) NULL,
	[DoRong] [real] NULL,
	[DieuKien] [nvarchar](50) NULL,
	[TruongThem] [nvarchar](50) NULL,
	[TruongThemHienThi] [nvarchar](200) NULL,
	[TruongThemHienThi_E] [nvarchar](200) NULL,
	[TuBang] [nvarchar](50) NULL,
	[TruongLuu] [nvarchar](50) NULL,
	[TruongHienThi] [nvarchar](256) NULL,
	[TruongLienKet_Tubang1] [nvarchar](50) NULL,
	[TuBang1] [nvarchar](50) NULL,
	[TruongLuu1] [nvarchar](50) NULL,
	[TruongHienThi1] [nvarchar](50) NULL,
	[ChoPhepHienthi_TuBang1] [bit] NULL,
	[DinhDang] [nvarchar](50) NULL,
	[NgamDinh] [ntext] NULL,
	[DinhDangKieuSo] [nvarchar](50) NULL,
	[CanLe] [tinyint] NULL,
	[HienThi] [bit] NULL,
	[HienThiTrenluoi] [bit] NULL,
	[ChiDoc] [bit] NULL,
	[GhiChu] [nvarchar](250) NULL,
	[BatBuocNhap] [bit] NULL,
	[GiuLai] [bit] NULL,
	[TruongKhoa] [bit] NULL,
	[TuTang] [bit] NULL,
	[ToiDa] [int] NULL,
	[CamQuyenXem] [nvarchar](300) NULL,
	[SoThapPhan] [tinyint] NULL,
	[XauChuan] [bit] NULL,
	[GiaTriToiDa] [smallint] NULL,
	[GiaTriToiThieu] [smallint] NULL,
	[CoDinh] [bit] NULL,
 CONSTRAINT [PK_HTN_DanhMuc_ChiTiet] PRIMARY KEY CLUSTERED 
(
	[BiDanh] ASC,
	[Truong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HTN_DSBang]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HTN_DSBang](
	[STT] [smallint] NULL,
	[Tablename] [varchar](50) NULL,
	[LeftTableName] [varchar](50) NULL,
	[JoinType] [varchar](50) NULL,
	[RightTableName] [varchar](50) NULL,
	[LeftColumnName] [varchar](50) NULL,
	[RightColumnName] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HTN_TruongHienThi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HTN_TruongHienThi](
	[STT] [smallint] NULL,
	[Banggoc] [varchar](50) NULL,
	[Tenbang] [varchar](50) NULL,
	[Tentruong] [varchar](50) NULL,
	[Bidanh] [varchar](50) NULL,
	[OK] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HTNS_ChucNang]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HTNS_ChucNang](
	[Ma] [nvarchar](50) NOT NULL,
	[Ten] [nvarchar](50) NULL,
	[Icon] [nvarchar](250) NULL,
	[Form] [nvarchar](50) NULL,
	[TypeControl] [nvarchar](50) NULL,
	[STT] [int] NULL,
	[MaCha] [nvarchar](50) NULL,
	[ShowHot] [bit] NULL,
	[OK] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[QuyenXem] [nvarchar](250) NULL,
	[QuyenThemMoi] [nvarchar](250) NULL,
	[QuyenSua] [nvarchar](250) NULL,
	[QuyenXoa] [nvarchar](250) NULL,
 CONSTRAINT [PK_HTNS_ChucNang] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LBangChamCong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LBangChamCong](
	[bcclma] [int] IDENTITY(1,1) NOT NULL,
	[bcclmanv] [int] NULL,
	[bcclthang] [smallint] NULL,
	[bcclNam] [smallint] NULL,
	[bcclLoai] [bit] NULL,
	[bcclLydosua] [nvarchar](1000) NULL,
	[bcclNguoiSua] [int] NULL,
	[BCCLLamThemN_CT] [float] NULL,
	[bcclLamThemD_CT] [float] NULL,
	[bcclSoNghiLe_CT] [float] NULL,
	[bcclLamThemCongTyD_CT] [float] NULL,
	[BCCLLamThemLeD_CT] [float] NULL,
	[bcclLamThemCongTyN_CT] [float] NULL,
	[BCCLLamThemLeN_CT] [float] NULL,
	[BCCLSoNghiCongTy_CT] [float] NULL,
	[bcclCongChuan] [float] NULL,
	[bcclSongayNKL] [float] NULL,
	[bcclDuCong] [float] NULL,
	[BCCLCongN_CT] [float] NULL,
	[BCCLCongD_CT] [float] NULL,
	[BCCLSonguoiPhuThuoc] [float] NULL,
	[BCCLNghiLayOFF] [float] NULL,
	[BCCLCongN_TV] [float] NULL,
	[BCCLCongD_TV] [float] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LBangChamCongTam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LBangChamCongTam](
	[bcclmanv] [int] NULL,
	[bcclmanvn] [nvarchar](10) NOT NULL,
	[bcclthang] [tinyint] NULL,
	[bcclNam] [int] NULL,
	[bcclSoNghiLe_CT] [float] NOT NULL,
	[BCCLLamThemN_CT] [float] NOT NULL,
	[BCCLSoCaHC] [float] NOT NULL,
	[bcclSoCa2_CT] [float] NOT NULL,
	[bcclSoCa3_CT] [float] NOT NULL,
	[bcclLamThemD_CT] [float] NOT NULL,
	[BCCLSoNghiCongTy_CT] [float] NOT NULL,
	[bcclLamThemCongTyD_CT] [float] NOT NULL,
	[bcclSongayNKL] [float] NOT NULL,
	[BCCLLamThemLeD_CT] [float] NOT NULL,
	[bcclLamThemCongTyN_CT] [float] NOT NULL,
	[BCCLLamThemLeN_CT] [float] NOT NULL,
	[bcclCongChuan] [float] NOT NULL,
	[BCCLTCTienAn] [float] NOT NULL,
	[bcclDuCong] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LDienBienLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LDienBienLuong](
	[DBLThang] [int] NOT NULL,
	[DBLNam] [int] NOT NULL,
	[HDMaNV] [int] NOT NULL,
	[HDLoaiHD] [varchar](50) NULL,
	[HDTuNgay] [datetime] NULL,
	[HDDenNgay] [datetime] NULL,
	[HDLuongCB] [numeric](18, 0) NULL,
	[HDLuongBH] [numeric](18, 0) NULL,
	[HDLuongTV] [numeric](18, 0) NULL,
	[HDDongBHYT] [bit] NULL,
	[HDDongBHXH] [bit] NULL,
	[HDDongCongDoan] [bit] NULL,
	[PCDilai] [numeric](38, 0) NULL,
	[PCNhaO] [numeric](38, 0) NULL,
	[PCKhac] [numeric](38, 0) NULL,
	[TCDienThoai] [numeric](38, 0) NULL,
	[PCNN] [numeric](38, 0) NULL,
	[PCConnho] [numeric](38, 0) NULL,
	[PCThamnien] [numeric](38, 0) NULL,
	[HTCongChuan] [numeric](38, 0) NULL,
	[TCDocHai] [numeric](38, 0) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[GhiChu] [nvarchar](50) NULL,
	[Pcngoaingu] [numeric](18, 0) NULL,
	[Pctrachnhiem] [numeric](18, 0) NULL,
	[Pckynang] [numeric](18, 0) NULL,
	[Pccadem] [numeric](18, 0) NULL,
	[Pcnangnhoc] [numeric](18, 0) NULL,
	[Pcnghenghiep] [numeric](18, 0) NULL,
	[Pcchuyencan] [numeric](18, 0) NULL,
	[Pcdienthoai] [numeric](18, 0) NULL,
	[Pcbanatld] [numeric](18, 0) NULL,
	[Pcbaoduong] [numeric](18, 0) NULL,
	[DBLNgayBDGianDoan] [datetime] NULL,
	[DBLNgayKTGianDoan] [datetime] NULL,
	[DBLNgayBDTinhGianDoan] [datetime] NULL,
	[DBLNgayKTTinhGianDoan] [datetime] NULL,
	[DBLNgayNghiViec] [datetime] NULL,
	[DBLLoai] [bit] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[HDDongBHTN] [bit] NULL,
	[DBLLoai1] [bit] NULL,
	[HDDongBHXHCTEN] [bit] NULL,
	[HDDongBHYTCTEN] [bit] NULL,
	[HDDongBHTNCTEN] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lluong_CauhinhDieuChinhLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lluong_CauhinhDieuChinhLuong](
	[DCLID] [nvarchar](50) NOT NULL,
	[DCLTenSQL] [nvarchar](50) NOT NULL,
	[DCLTen] [nvarchar](250) NOT NULL,
	[DCLTen_EN] [nvarchar](250) NULL,
	[DCLTen_TQ] [nvarchar](250) NULL,
	[DCLTen_JP] [nvarchar](250) NULL,
	[DCLTen_OT] [nvarchar](250) NULL,
	[DCLHienThi] [bit] NULL,
	[DCLUuTien] [int] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_lluong_CauhinhDieuChinhLuong] PRIMARY KEY CLUSTERED 
(
	[DCLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lLuong_CongChuan]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lLuong_CongChuan](
	[CCMa] [varchar](50) NULL,
	[CCNgay] [datetime] NULL,
	[CCSoCong5] [float] NULL,
	[CCSoCong] [float] NULL,
	[CCSoCongHC] [float] NULL,
	[CCSoCongA] [float] NULL,
	[CCSoCongB] [float] NULL,
	[CCSoCongC] [float] NULL,
	[CCSoCongD] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lLuong_CongThucLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lLuong_CongThucLuong](
	[ID] [int] NULL,
	[TenCot] [varchar](30) NULL,
	[DienGiai] [nvarchar](100) NULL,
	[CongThuc] [nvarchar](max) NULL,
	[CauLenhSQL] [nvarchar](max) NULL,
	[Loai] [varchar](30) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[KieuTinhLuong] [varchar](30) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lLuong_DieuChinhLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lLuong_DieuChinhLuong](
	[dcclma] [int] IDENTITY(1,1) NOT NULL,
	[dcclmanv] [int] NULL,
	[dcclthang] [smallint] NULL,
	[dcclNam] [smallint] NULL,
	[dcclLoai] [bit] NULL,
	[dcclLydosua] [nvarchar](1000) NULL,
	[dcclNguoiSua] [int] NULL,
	[dclDieuChinhLuong] [float] NULL,
	[dclSinhNhat] [float] NULL,
	[dclTroCapAnCa] [float] NULL,
	[dclTroCapOmdau] [float] NULL,
	[dclThuongKhac] [float] NULL,
	[dclBHTra] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LLuong_ThamSoLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LLuong_ThamSoLuong](
	[TSLID] [varchar](50) NOT NULL,
	[TSLThang] [int] NOT NULL,
	[TSLNam] [int] NOT NULL,
	[TSLCa1] [float] NOT NULL,
	[TSLCa2] [float] NOT NULL,
	[TSLCa3] [float] NOT NULL,
	[TSLLamThem] [float] NOT NULL,
	[TSLChuNhat] [float] NOT NULL,
	[TSLNgayLe] [float] NOT NULL,
	[TSLBHXHNV] [float] NOT NULL,
	[TSLBHYTNV] [float] NOT NULL,
	[TSLBHTNNV] [float] NOT NULL,
	[TSLCongDoan] [float] NOT NULL,
	[TSLBHXHCT] [float] NOT NULL,
	[TSLBHYTCT] [float] NOT NULL,
	[TSLBHTNCT] [float] NOT NULL,
	[TSLNgayCongQD] [smallint] NOT NULL,
	[TSLHeSoNghi70] [float] NOT NULL,
	[TSLHeSoNghi100] [float] NOT NULL,
	[TSLHeSoThuViec] [real] NOT NULL,
	[TSLTCTienAn] [numeric](18, 0) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_LLuong_ThamSoLuong] PRIMARY KEY CLUSTERED 
(
	[TSLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LLuong_ThamSoLuongT13]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LLuong_ThamSoLuongT13](
	[TSLID] [varchar](50) NOT NULL,
	[TSLThang] [int] NOT NULL,
	[TSLNam] [int] NOT NULL,
	[TSLCa1] [float] NOT NULL,
	[TSLCa2] [float] NOT NULL,
	[TSLCa3] [float] NOT NULL,
	[TSLLamThem] [float] NOT NULL,
	[TSLChuNhat] [float] NOT NULL,
	[TSLNgayLe] [float] NOT NULL,
	[TSLBHXHNV] [float] NOT NULL,
	[TSLBHYTNV] [float] NOT NULL,
	[TSLBHTNNV] [float] NOT NULL,
	[TSLCongDoan] [float] NOT NULL,
	[TSLBHXHCT] [float] NOT NULL,
	[TSLBHYTCT] [float] NOT NULL,
	[TSLBHTNCT] [float] NOT NULL,
	[TSLNgayCongQD] [smallint] NOT NULL,
	[TSLHeSoNghi70] [float] NOT NULL,
	[TSLHeSoNghi100] [float] NOT NULL,
	[TSLHeSoThuViec] [real] NOT NULL,
	[TSLTCTienAn] [numeric](18, 0) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LPhiDichVu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LPhiDichVu](
	[pdvma] [int] IDENTITY(1,1) NOT NULL,
	[pdvmanv] [int] NULL,
	[pdvthang] [smallint] NULL,
	[pdvNam] [smallint] NULL,
	[pdvLoai] [bit] NULL,
	[pdvDaKhoaBangCong] [bit] NULL,
	[pdvCongNgay] [float] NULL,
	[pdvCongDem] [float] NULL,
	[pdvNghiH100] [float] NULL,
	[pdvTienDichVu] [numeric](18, 0) NULL,
	[pdvGhiChu] [nvarchar](500) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[pdvPhanTramSVC] [float] NULL,
	[pdvPhanTramTru] [float] NULL,
	[pdvCongNgay_TV] [float] NULL,
	[pdvCongDem_TV] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LTongPhiDichVuThang]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LTongPhiDichVuThang](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Thang] [smallint] NULL,
	[Nam] [smallint] NULL,
	[TongTien] [numeric](18, 0) NULL,
	[GhiChu] [nvarchar](500) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Luong_PhuCap]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Luong_PhuCap](
	[PCID] [nvarchar](50) NOT NULL,
	[PCTenSQL] [nvarchar](50) NOT NULL,
	[PCTen] [nvarchar](250) NOT NULL,
	[PCTen_EN] [nvarchar](250) NULL,
	[PCTen_TQ] [nvarchar](250) NULL,
	[PCTen_JP] [nvarchar](250) NULL,
	[PCTen_OT] [nvarchar](250) NULL,
	[PCHienThi] [bit] NULL,
	[PCUuTien] [int] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_Luong_PhuCap] PRIMARY KEY CLUSTERED 
(
	[PCID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MCC_User]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MCC_User](
	[UserID] [varchar](50) NULL,
	[UserName] [nvarchar](500) NULL,
	[Card] [nvarchar](200) NULL,
	[PIN] [nvarchar](50) NULL,
	[Privilege] [varchar](50) NULL,
	[Enable] [bit] NULL,
	[User1] [int] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [int] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [int] NULL,
	[Date3] [smalldatetime] NULL,
	[FaceData] [nvarchar](max) NULL,
	[FaceDataLength] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TZ] [nvarchar](50) NULL,
	[Pri] [nvarchar](500) NULL,
	[BioDataNo] [nvarchar](200) NULL,
	[BioDataIndex] [nvarchar](200) NULL,
	[BioDataDuress] [nvarchar](200) NULL,
	[BioDataMajorVer] [nvarchar](200) NULL,
	[BioDataMinorVer] [nvarchar](200) NULL,
	[BioDataFormat] [nvarchar](200) NULL,
	[BioDataType] [int] NULL,
	[BioDataTmp] [nvarchar](max) NULL,
	[BioPhotoFileName] [nvarchar](200) NULL,
	[BioPhotoContent] [nvarchar](max) NULL,
	[BioPhotoType] [int] NULL,
	[BioPhotoSize] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MCC_User_09052024]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MCC_User_09052024](
	[UserID] [varchar](50) NULL,
	[UserName] [nvarchar](500) NULL,
	[Card] [nvarchar](200) NULL,
	[PIN] [nvarchar](50) NULL,
	[Privilege] [varchar](50) NULL,
	[Enable] [bit] NULL,
	[User1] [int] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [int] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [int] NULL,
	[Date3] [smalldatetime] NULL,
	[FaceData] [nvarchar](max) NULL,
	[FaceDataLength] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TZ] [nvarchar](50) NULL,
	[Pri] [nvarchar](500) NULL,
	[BioDataNo] [nvarchar](200) NULL,
	[BioDataIndex] [nvarchar](200) NULL,
	[BioDataDuress] [nvarchar](200) NULL,
	[BioDataMajorVer] [nvarchar](200) NULL,
	[BioDataMinorVer] [nvarchar](200) NULL,
	[BioDataFormat] [nvarchar](200) NULL,
	[BioDataType] [int] NULL,
	[BioDataTmp] [nvarchar](max) NULL,
	[BioPhotoFileName] [nvarchar](200) NULL,
	[BioPhotoContent] [nvarchar](max) NULL,
	[BioPhotoType] [int] NULL,
	[BioPhotoSize] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MCC_User_b]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MCC_User_b](
	[UserID] [varchar](50) NULL,
	[UserName] [nvarchar](200) NULL,
	[Card] [nvarchar](200) NULL,
	[PIN] [nvarchar](50) NULL,
	[Privilege] [varchar](50) NULL,
	[Enable] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MCC_UserDetail]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MCC_UserDetail](
	[UserID] [varchar](50) NULL,
	[FPIndex] [int] NULL,
	[FPData] [nvarchar](max) NULL,
	[FPData1] [nvarchar](max) NULL,
	[FPData2] [nvarchar](max) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FIDType] [nvarchar](200) NULL,
 CONSTRAINT [PK_MCC_UserDetail] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MCC_UserNew]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MCC_UserNew](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PID] [int] NOT NULL,
	[UserID] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](500) NULL,
	[Passwd] [nvarchar](50) NULL,
	[Card] [nvarchar](50) NULL,
	[Grp] [nvarchar](50) NULL,
	[TZ] [nvarchar](50) NULL,
	[Pri] [nvarchar](50) NULL,
	[BioDataNo] [nvarchar](200) NULL,
	[BioDataIndex] [nvarchar](200) NULL,
	[BioDataDuress] [nvarchar](200) NULL,
	[BioDataMajorVer] [nvarchar](200) NULL,
	[BioDataMinorVer] [nvarchar](200) NULL,
	[BioDataFormat] [nvarchar](200) NULL,
	[BioDataType] [int] NULL,
	[BioDataTmp] [nvarchar](max) NULL,
	[BioPhotoFileName] [nvarchar](200) NULL,
	[BioPhotoType] [int] NULL,
	[BioPhotoSize] [int] NULL,
	[BioPhotoContent] [nvarchar](max) NULL,
	[User1] [int] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [int] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_MCC_UserNew] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MCCTMP]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MCCTMP](
	[UserId] [nchar](10) NULL,
	[Card] [nchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_BaoHiemYTe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_BaoHiemYTe](
	[BHYTMaNV] [int] NOT NULL,
	[BHYTSoBHYT] [nvarchar](25) NULL,
	[BHYTNoiCap] [nvarchar](100) NULL,
	[BHYTNgayCap] [datetime] NULL,
	[BHYTTuNgay] [datetime] NULL,
	[BHYTDenNgay] [datetime] NULL,
	[BHYTMaTinh] [varchar](50) NULL,
	[BHYTMaBV] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_BCThongTinNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_BCThongTinNV](
	[Ma] [varchar](50) NOT NULL,
	[LoaiTTNV] [varchar](50) NULL,
	[Ten] [nvarchar](200) NULL,
	[QuyenXem] [varchar](400) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_NS_BCThongTinNV] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_CauhinhExportExcel]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_CauhinhExportExcel](
	[TenBang] [nvarchar](200) NOT NULL,
	[TenCot] [nvarchar](200) NOT NULL,
	[Diengiai] [nvarchar](200) NULL,
	[CauLenhSQL] [nvarchar](500) NULL,
	[Sothutu] [int] NULL,
	[MacDinhChon] [bit] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_NS_CauhinhExcel] PRIMARY KEY CLUSTERED 
(
	[TenBang] ASC,
	[TenCot] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_DanhGiaNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_DanhGiaNV](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [nvarchar](50) NULL,
	[DotDanhGia] [nvarchar](50) NULL,
	[LoaiDanhGia] [nvarchar](50) NULL,
	[DuocHienThi] [bit] NULL,
	[GhiChu] [nvarchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_DanhGiaNV_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_DanhGiaNV_ChiTiet](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [nvarchar](50) NULL,
	[DotDanhGia] [nvarchar](50) NULL,
	[LoaiDanhGia] [nvarchar](50) NULL,
	[TieuChiDanhGia] [nvarchar](50) NULL,
	[KetQuaDanhGia] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_Danhmuc_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_Danhmuc_ChiTiet](
	[BiDanh] [varchar](50) NOT NULL,
	[STT] [float] NULL,
	[Truong] [nvarchar](50) NOT NULL,
	[KieuDuLieu] [nvarchar](20) NULL,
	[TenHienThi] [nvarchar](50) NULL,
	[DoRong] [real] NULL,
	[DieuKien] [nvarchar](50) NULL,
	[TuBang] [nvarchar](50) NULL,
	[TruongLuu] [nvarchar](50) NULL,
	[TruongHienThi] [nvarchar](50) NULL,
	[NgamDinh] [ntext] NULL,
	[DinhDangKieuSo] [nvarchar](50) NULL,
	[CanLe] [tinyint] NULL,
	[HienThi] [bit] NULL,
	[ChiDoc] [bit] NULL,
	[GhiChu] [nvarchar](250) NULL,
	[BatBuocNhap] [bit] NULL,
	[GiuLai] [bit] NULL,
	[TruongKhoa] [bit] NULL,
	[TuTang] [bit] NULL,
	[ToiDa] [int] NULL,
	[CamQuyenXem] [nvarchar](300) NULL,
	[SoThapPhan] [tinyint] NULL,
	[XauChuan] [bit] NULL,
	[GiaTriToiDa] [numeric](18, 0) NULL,
	[GiaTriToiThieu] [smallint] NULL,
	[CoDinh] [bit] NULL,
	[TenHienThiE] [nvarchar](400) NULL,
 CONSTRAINT [PK_NS_Danhmuc_ChiTiet] PRIMARY KEY CLUSTERED 
(
	[BiDanh] ASC,
	[Truong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_HopDong_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_HopDong_ChiTiet](
	[BiDanh] [varchar](50) NOT NULL,
	[STT] [float] NULL,
	[Truong] [nvarchar](50) NOT NULL,
	[KieuDuLieu] [nvarchar](20) NULL,
	[TenHienThi] [nvarchar](50) NULL,
	[DoRong] [real] NULL,
	[DieuKien] [nvarchar](50) NULL,
	[TuBang] [nvarchar](50) NULL,
	[TruongLuu] [nvarchar](50) NULL,
	[TruongHienThi] [nvarchar](50) NULL,
	[NgamDinh] [ntext] NULL,
	[DinhDangKieuSo] [nvarchar](50) NULL,
	[CanLe] [tinyint] NULL,
	[HienThi] [bit] NULL,
	[ChiDoc] [bit] NULL,
	[GhiChu] [nvarchar](250) NULL,
	[BatBuocNhap] [bit] NULL,
	[GiuLai] [bit] NULL,
	[TruongKhoa] [bit] NULL,
	[TuTang] [bit] NULL,
	[ToiDa] [int] NULL,
	[CamQuyenXem] [nvarchar](300) NULL,
	[SoThapPhan] [tinyint] NULL,
	[XauChuan] [bit] NULL,
	[GiaTriToiDa] [numeric](18, 0) NULL,
	[GiaTriToiThieu] [smallint] NULL,
	[CoDinh] [bit] NULL,
	[TenHienThiE] [nvarchar](400) NULL,
 CONSTRAINT [PK_NS_HopDong_ChiTiet] PRIMARY KEY CLUSTERED 
(
	[BiDanh] ASC,
	[Truong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_Khenthuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_Khenthuong](
	[MaNV] [int] NOT NULL,
	[SoQD] [varchar](20) NULL,
	[NgayQD] [smalldatetime] NULL,
	[NgayHieuLuc] [smalldatetime] NULL,
	[HinhThuc] [varchar](10) NULL,
	[NoiDung] [varchar](100) NULL,
	[NguoiKy] [nvarchar](50) NULL,
	[CapKhenThuong] [varchar](10) NULL,
	[GhiChu] [nvarchar](max) NULL,
	[DanhHieu] [varchar](10) NULL,
	[NS_KhenThuong] [int] NOT NULL,
	[SoTien] [money] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_Kyluat]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_Kyluat](
	[MaNV] [int] NULL,
	[SoQD] [varchar](20) NULL,
	[NgayQD] [smalldatetime] NULL,
	[NgayHieuLuc] [smalldatetime] NULL,
	[HinhThuc] [varchar](15) NULL,
	[NoiDung] [nvarchar](200) NULL,
	[NguoiKy] [varchar](50) NULL,
	[GhiChu] [text] NULL,
	[NS_KyLuat] [int] NOT NULL,
	[ThoiGian] [tinyint] NULL,
	[LoaiKyLuat] [varchar](50) NULL,
	[SoTien] [money] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_QTCT]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_QTCT](
	[NS_QTCT] [int] IDENTITY(1,1) NOT NULL,
	[SoQD] [varchar](20) NULL,
	[NgayQD] [smalldatetime] NULL,
	[MaNV] [int] NOT NULL,
	[NgayHieuLuc] [smalldatetime] NULL,
	[TuNgay] [smalldatetime] NULL,
	[DenNgay] [smalldatetime] NULL,
	[MaBP] [varchar](10) NULL,
	[ChucVu] [varchar](10) NULL,
	[KiemNhiem] [varchar](100) NULL,
	[CongTrinh] [varchar](10) NULL,
	[CNganh] [varchar](10) NULL,
	[LoaiQD] [varchar](30) NULL,
	[GhiChu] [nvarchar](max) NULL,
	[ThoiGian] [varchar](50) NULL,
	[MucLuong] [money] NULL,
	[ToCongTac] [varchar](15) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_QTCTNgoaiCTy]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_QTCTNgoaiCTy](
	[CTNCTMaNV] [int] NOT NULL,
	[CTNCTTuNgay] [smalldatetime] NOT NULL,
	[CTNCTBophan] [nvarchar](100) NULL,
	[CTNCTDenNgay] [smalldatetime] NULL,
	[CTNCTChucVu] [nvarchar](100) NULL,
	[CTNCTKiemNhiem] [nvarchar](100) NULL,
	[CTNCTCongTrinh] [nvarchar](100) NULL,
	[CTNCTGhiChu] [nvarchar](100) NULL,
	[CTNCTMucLuong] [money] NULL,
	[CTNCTThoiGian] [nvarchar](50) NULL,
	[CTNCTNoiDung] [nvarchar](500) NULL,
	[CTNCTDiaDiem] [nvarchar](500) NULL,
	[CTNCTQuocGia] [varchar](50) NULL,
 CONSTRAINT [PK_NS_QTCTNgoaiCTy] PRIMARY KEY CLUSTERED 
(
	[CTNCTMaNV] ASC,
	[CTNCTTuNgay] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_QuanheGD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_QuanheGD](
	[QHGDMa] [int] IDENTITY(1,1) NOT NULL,
	[QHGDMaNV] [int] NOT NULL,
	[QHGDTen] [nvarchar](40) NOT NULL,
	[QHGDQuanHe] [nvarchar](30) NOT NULL,
	[QHGDNgaySinh] [datetime] NOT NULL,
	[QHGDNoiO] [nvarchar](100) NOT NULL,
	[QHGDCongViec] [nvarchar](200) NOT NULL,
	[QHGDSTT] [tinyint] NOT NULL,
	[QHGDTrugiacanh] [bit] NULL,
	[QHGDTuNgay] [datetime] NULL,
	[QHGDDenNgay] [datetime] NULL,
	[QHGDSoCMTND] [varchar](10) NULL,
	[QHGDMasothue] [varchar](50) NULL,
	[QHGDQuanLyTSConNho] [bit] NULL,
 CONSTRAINT [PK_NS_QuanheGD] PRIMARY KEY CLUSTERED 
(
	[QHGDMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_QuaTrinhBH]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_QuaTrinhBH](
	[QTXHMaNV] [int] NOT NULL,
	[QTXHTuNgay] [datetime] NOT NULL,
	[QTXHToiNgay] [datetime] NOT NULL,
	[QTXHMucLuong] [int] NOT NULL,
	[QTXHNhiemVu] [nvarchar](100) NOT NULL,
	[QTXHNamBH] [int] NOT NULL,
	[QTXHThangBH] [int] NOT NULL,
	[QTXHPhuCap] [int] NULL,
 CONSTRAINT [PK_NS_QuaTrinhBH] PRIMARY KEY CLUSTERED 
(
	[QTXHMaNV] ASC,
	[QTXHTuNgay] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_QuaTrinhBHNew]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_QuaTrinhBHNew](
	[STT] [int] NULL,
	[MaNV] [int] NULL,
	[TuNgay] [datetime] NULL,
	[DenNgay] [datetime] NULL,
	[Congty] [nvarchar](500) NULL,
	[ChucVu] [nvarchar](500) NULL,
	[LuongBH] [numeric](18, 0) NULL,
	[TyLeBHXH] [float] NULL,
	[TyLeBHYT] [float] NULL,
	[TyLeBHTN] [float] NULL,
	[DaDongTaiCongTy] [bit] NULL,
	[GhiChu] [nvarchar](550) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_TangGiamBH]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_TangGiamBH](
	[TGBHMaNV] [int] NULL,
	[TGBHNgayKhaibao] [datetime] NULL,
	[TGBHNoidung] [varchar](50) NULL,
	[TGBHTratheBHYT] [bit] NULL,
	[TGBHChucvu] [int] NULL,
	[TGBHSoQD] [nvarchar](50) NULL,
	[TGBHDakhai] [bit] NULL,
	[TGBHLoai] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_TongNgayPhep]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_TongNgayPhep](
	[nsnpMa] [nvarchar](50) NOT NULL,
	[nsnpMaNV] [int] NULL,
	[nsnpNgayApdung] [datetime] NULL,
	[nsnpNgayKetThuc] [datetime] NULL,
	[nsnpSoNgay] [smallint] NULL,
	[TonNamTruoc] [float] NULL,
	[DaSuaTay] [bit] NULL,
	[GhiChu] [nvarchar](250) NULL,
 CONSTRAINT [PK_NS_TongNgayPhep] PRIMARY KEY CLUSTERED 
(
	[nsnpMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NS_XacNhanNhanSu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NS_XacNhanNhanSu](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [int] NULL,
	[SoQD] [nvarchar](100) NULL,
	[NgayVao] [datetime] NULL,
	[NgayNghiViec] [datetime] NULL,
	[BoPhan] [nvarchar](50) NULL,
	[ChucVu] [nvarchar](50) NULL,
	[GhiChu] [nvarchar](200) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Q11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Q11](
	[Ngay] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RB_Condition]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RB_Condition](
	[TT] [tinyint] NULL,
	[Ma] [nvarchar](100) NULL,
	[Ten] [nvarchar](100) NULL,
	[KieuDuLieu] [nvarchar](100) NULL,
	[TuBang] [nvarchar](100) NULL,
	[TruongLuu] [nvarchar](100) NULL,
	[TruongHienThi] [nvarchar](100) NULL,
	[BatBuoc] [bit] NOT NULL,
	[NgamDinh] [nvarchar](200) NULL,
	[TruongDieuKien] [nvarchar](100) NULL,
	[BieuThucSoSanh] [nvarchar](20) NULL,
	[ThamsoStore] [nvarchar](50) NULL,
	[Ten_EN] [nvarchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RB_CustomizeColumn]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RB_CustomizeColumn](
	[Form] [nvarchar](100) NULL,
	[Grid] [nvarchar](100) NULL,
	[TT] [tinyint] NULL,
	[TenCot] [nvarchar](100) NULL,
	[HienThi] [nvarchar](200) NULL,
	[CanLe] [tinyint] NULL,
	[SoThapPhan] [tinyint] NULL,
	[DoRong] [decimal](18, 0) NULL,
	[Frozen] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RB_Main]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RB_Main](
	[OK] [bit] NOT NULL,
	[Ma] [nvarchar](100) NOT NULL,
	[DuongDan] [nvarchar](400) NULL,
	[DuongDan14] [varchar](500) NULL,
	[DuongDan13] [varchar](500) NULL,
	[DuongDan12] [varchar](500) NULL,
	[DuongDan11] [varchar](500) NULL,
	[DuongDan10] [varchar](500) NULL,
	[DuongDan9] [varchar](500) NULL,
	[DuongDan8] [varchar](500) NULL,
	[DuongDan7] [varchar](500) NULL,
	[DuongDan6] [varchar](500) NULL,
	[DuongDan5] [varchar](500) NULL,
	[DuongDan4] [varchar](500) NULL,
	[DuongDan3] [varchar](500) NULL,
	[DuongDan2] [varchar](500) NULL,
	[DuongDan1] [varchar](500) NULL,
	[NhomDuLieu] [bit] NULL,
	[Ten] [nvarchar](400) NOT NULL,
	[TT] [smallint] NULL,
	[SQL1] [ntext] NULL,
	[SQL2] [ntext] NULL,
	[SQL3] [ntext] NULL,
	[Note] [ntext] NULL,
	[NhomBC] [nvarchar](100) NULL,
	[Icon] [nvarchar](100) NULL,
	[NgaySua] [smalldatetime] NULL,
	[DieuKienThoiGian] [nvarchar](100) NULL,
	[DieuKienThem] [nvarchar](200) NULL,
	[XemChiTiet] [nvarchar](30) NULL,
	[CotDieuKien] [nvarchar](30) NULL,
	[ChayChucNang] [nvarchar](30) NULL,
	[ChayThuTuc] [nvarchar](250) NULL,
	[QuyenXem] [nvarchar](300) NULL,
	[tmp_BangTrungGian] [nvarchar](100) NULL,
	[SQLSetValue] [nvarchar](max) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[ExcelTheoTemplate] [bit] NULL,
	[NhomBCNew] [nvarchar](50) NULL,
	[Ten_EN] [nvarchar](500) NULL,
 CONSTRAINT [PK_RB_Main] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RB_Procedure]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RB_Procedure](
	[TT] [smallint] NULL,
	[Ma] [nvarchar](250) NOT NULL,
	[Ten] [nvarchar](1000) NULL,
	[DieuKien] [nvarchar](50) NULL,
	[Loai] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_RB_Procedure] PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2006_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2006_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2012_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2012_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2016_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2016_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2018_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2018_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2018_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2018_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2018_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2018_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2018_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2018_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2018_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2018_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2018_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2018_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2018_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2018_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2019_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2019_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2020_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2020_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2021_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2021_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2022_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2022_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2023_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2023_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2024_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2024_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_10]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_10](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2025_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2025_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_01]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_01](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_02]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_02](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_03]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_03](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_04]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_04](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_05]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_05](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_06]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_06](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_07]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_07](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_08]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_08](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_09]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_09](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_11]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_11](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordData2026_12]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordData2026_12](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataBackUp]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataBackUp](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataFull]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataFull](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [nvarchar](50) NULL,
	[Ngay] [datetime] NULL,
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataNew_2020]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataNew_2020](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataNew_2022]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataNew_2022](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDatanew0101_2025]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDatanew0101_2025](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataNewBAK160522]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataNewBAK160522](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataNewIDM0]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataNewIDM0](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataQuanSo]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataQuanSo](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecordDataTam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordDataTam](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL,
	[Privilege] [int] NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [float] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reg_System]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reg_System](
	[SysCode] [nvarchar](20) NULL,
	[SysValue] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportColumn]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportColumn](
	[ColID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Field] [nvarchar](50) NOT NULL,
	[NameE] [nvarchar](50) NULL,
	[Display] [bit] NULL,
	[ColWidth] [int] NULL,
	[OrderBy] [int] NULL,
	[strSQL] [text] NULL,
	[Format] [nvarchar](10) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Result_NB]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Result_NB](
	[GiaTri] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sl_TGDMVS]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sl_TGDMVS](
	[BCMaNV] [int] NOT NULL,
	[TongDMVS] [int] NULL,
	[TongTGDMVS] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBacTho]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBacTho](
	[MaTuSinh] [int] IDENTITY(1,1) NOT NULL,
	[TenBacTho] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBangCapVT]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBangCapVT](
	[MaTuSinh] [int] IDENTITY(1,1) NOT NULL,
	[Ten] [nvarchar](30) NOT NULL,
	[MoTa] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBangChamCong2010]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBangChamCong2010](
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
GO
/****** Object:  Table [dbo].[tblBangChamCong2011]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBangChamCong2011](
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
GO
/****** Object:  Table [dbo].[tblbaocao0101_2025]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblbaocao0101_2025](
	[BCNgay] [datetime] NOT NULL,
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
	[BCNhietDoVe] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblbaocao0106_2024]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblbaocao0106_2024](
	[BCNgay] [datetime] NOT NULL,
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
	[BCNhietDoVe] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCao1003_2021]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCao1003_2021](
	[BCNgay] [datetime] NOT NULL,
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
	[BCTGUuDaiD] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaocao22_01_21]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaocao22_01_21](
	[BCNgay] [datetime] NOT NULL,
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
	[BCNhietDoVe] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoBangChamCong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoBangChamCong](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[MaNV] [nvarchar](10) NULL,
	[TenNV] [nvarchar](40) NULL,
	[F1] [nvarchar](10) NULL,
	[F2] [nvarchar](10) NULL,
	[F3] [nvarchar](10) NULL,
	[F4] [nvarchar](10) NULL,
	[F5] [nvarchar](10) NULL,
	[F6] [nvarchar](10) NULL,
	[F7] [nvarchar](10) NULL,
	[F8] [nvarchar](10) NULL,
	[F9] [nvarchar](10) NULL,
	[F10] [nvarchar](10) NULL,
	[F11] [nvarchar](10) NULL,
	[F12] [nvarchar](10) NULL,
	[F13] [nvarchar](10) NULL,
	[F14] [nvarchar](10) NULL,
	[F15] [nvarchar](10) NULL,
	[F16] [nvarchar](10) NULL,
	[F17] [nvarchar](10) NULL,
	[F18] [nvarchar](10) NULL,
	[F19] [nvarchar](10) NULL,
	[F20] [nvarchar](10) NULL,
	[F21] [nvarchar](10) NULL,
	[F22] [nvarchar](10) NULL,
	[F23] [nvarchar](10) NULL,
	[F24] [nvarchar](10) NULL,
	[F25] [nvarchar](10) NULL,
	[F26] [nvarchar](10) NULL,
	[F27] [nvarchar](10) NULL,
	[F28] [nvarchar](10) NULL,
	[F29] [nvarchar](10) NULL,
	[F30] [nvarchar](10) NULL,
	[F31] [nvarchar](10) NULL,
	[F32] [nvarchar](10) NULL,
	[F33] [nvarchar](10) NULL,
	[F34] [nvarchar](10) NULL,
	[F35] [nvarchar](10) NULL,
	[F36] [nvarchar](10) NULL,
	[F37] [nvarchar](10) NULL,
	[F38] [nvarchar](10) NULL,
	[F39] [nvarchar](10) NULL,
	[F40] [nvarchar](10) NULL,
	[F41] [nvarchar](10) NULL,
	[F42] [nvarchar](10) NULL,
	[F43] [nvarchar](10) NULL,
	[F44] [nvarchar](10) NULL,
	[F45] [nvarchar](10) NULL,
	[F46] [nvarchar](10) NULL,
	[F47] [nvarchar](10) NULL,
	[F48] [nvarchar](10) NULL,
	[F49] [nvarchar](10) NULL,
	[F50] [nvarchar](10) NULL,
	[F51] [nvarchar](10) NULL,
	[F52] [nvarchar](10) NULL,
	[F53] [nvarchar](10) NULL,
	[F54] [nvarchar](10) NULL,
	[F55] [nvarchar](10) NULL,
	[F56] [nvarchar](10) NULL,
	[F57] [nvarchar](10) NULL,
	[F58] [nvarchar](10) NULL,
	[F59] [nvarchar](10) NULL,
	[F60] [nvarchar](10) NULL,
	[F61] [nvarchar](10) NULL,
	[F62] [nvarchar](10) NULL,
	[F63] [nvarchar](10) NULL,
	[F64] [nvarchar](10) NULL,
	[UuTien] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoCaDangKy]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoCaDangKy](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[NVMaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[CaCoDinh] [nvarchar](40) NULL,
	[Ngay1] [nvarchar](5) NULL,
	[Ngay2] [nvarchar](5) NULL,
	[Ngay3] [nvarchar](5) NULL,
	[Ngay4] [nvarchar](5) NULL,
	[Ngay5] [nvarchar](5) NULL,
	[Ngay6] [nvarchar](5) NULL,
	[Ngay7] [nvarchar](5) NULL,
	[Ngay8] [nvarchar](5) NULL,
	[Ngay9] [nvarchar](5) NULL,
	[Ngay10] [nvarchar](5) NULL,
	[Ngay11] [nvarchar](5) NULL,
	[Ngay12] [nvarchar](5) NULL,
	[Ngay13] [nvarchar](5) NULL,
	[Ngay14] [nvarchar](5) NULL,
	[Ngay15] [nvarchar](5) NULL,
	[Ngay16] [nvarchar](5) NULL,
	[Ngay17] [nvarchar](5) NULL,
	[Ngay18] [nvarchar](5) NULL,
	[Ngay19] [nvarchar](5) NULL,
	[Ngay20] [nvarchar](5) NULL,
	[Ngay21] [nvarchar](5) NULL,
	[Ngay22] [nvarchar](5) NULL,
	[Ngay23] [nvarchar](5) NULL,
	[Ngay24] [nvarchar](5) NULL,
	[Ngay25] [nvarchar](5) NULL,
	[Ngay26] [nvarchar](5) NULL,
	[Ngay27] [nvarchar](5) NULL,
	[Ngay28] [nvarchar](5) NULL,
	[Ngay29] [nvarchar](5) NULL,
	[Ngay30] [nvarchar](5) NULL,
	[Ngay31] [nvarchar](5) NULL,
	[UuTien] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoCapThe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoCapThe](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[NVMaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[MaThe] [nvarchar](14) NULL,
	[NgayCap] [datetime] NULL,
	[NgayHetHan] [datetime] NULL,
	[UuTien] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoChuyenBoPhan]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoChuyenBoPhan](
	[STTBP] [smallint] NULL,
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](30) NULL,
	[STTNV] [int] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](14) NULL,
	[TenNV] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[BPChuyenDi] [nvarchar](40) NULL,
	[BPChuyenDen] [nvarchar](40) NULL,
	[NgayChuyen] [datetime] NULL,
	[GhiChu] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoChuyenBoPhanBP]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoChuyenBoPhanBP](
	[STTBP] [smallint] NULL,
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](30) NULL,
	[STTNV] [int] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](14) NULL,
	[TenNV] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[BPChuyenDi] [nvarchar](40) NULL,
	[BPChuyenDen] [nvarchar](40) NULL,
	[NgayChuyen] [datetime] NULL,
	[GhiChu] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoChuyenNhom]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoChuyenNhom](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[NVMaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[Nhom] [nvarchar](40) NULL,
	[NgayApDung] [datetime] NULL,
	[NgayKetThuc] [datetime] NULL,
	[UuTien] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblbaocaoCT_TV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblbaocaoCT_TV](
	[BCCMaNV] [int] NULL,
	[CongNgay_CT] [float] NULL,
	[CongNgay_TV] [float] NULL,
	[CongDem_CT] [float] NULL,
	[CongDem_TV] [float] NULL,
	[ThemNgay_CT] [float] NULL,
	[ThemNgay_TV] [float] NULL,
	[ThemDem_CT] [float] NULL,
	[ThemDem_TV] [float] NULL,
	[CNNgay] [float] NULL,
	[CNDem] [float] NULL,
	[LeNgay] [float] NULL,
	[LeDem] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoCua]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoCua](
	[Ma] [smallint] NULL,
	[Ten] [nvarchar](40) NULL,
	[Loai] [nvarchar](40) NULL,
	[CongGiaoTiep] [nvarchar](40) NULL,
	[SoBanGhi] [nvarchar](10) NULL,
	[UuTien] [smallint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoDailly]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoDailly](
	[BCNgay] [datetime] NULL,
	[BCMaBP] [int] NULL,
	[BCTenBP] [nvarchar](40) NULL,
	[BCMaNV] [nvarchar](10) NULL,
	[BCTenNV] [nvarchar](40) NULL,
	[BCChucVu] [nvarchar](40) NULL,
	[BCTenCa] [nvarchar](40) NULL,
	[BCCuaDen] [nvarchar](40) NULL,
	[BCTGDen] [datetime] NULL,
	[BCCuaVe] [nvarchar](40) NULL,
	[BCTGVe] [datetime] NULL,
	[BCCuaRa] [nvarchar](40) NULL,
	[BCTGRa] [datetime] NULL,
	[BCCuaVao] [nvarchar](40) NULL,
	[BCTGVao] [datetime] NULL,
	[TGLamNgay] [smallint] NULL,
	[TGLamToi] [smallint] NULL,
	[TGQuaGioNgay] [smallint] NULL,
	[TGQuaGioToi] [smallint] NULL,
	[TGLamThemNgay] [smallint] NULL,
	[TGLamThemToi] [smallint] NULL,
	[TGRaNgoaiNgay] [smallint] NULL,
	[TGRaNgoaiToi] [smallint] NULL,
	[TGDiMuonNgay] [smallint] NULL,
	[TGDiMuonToi] [smallint] NULL,
	[TGVeSomNgay] [smallint] NULL,
	[TGVeSomToi] [smallint] NULL,
	[TGQuyDinh] [smallint] NULL,
	[BCLoaiLamThem] [nvarchar](5) NULL,
	[BCGhiChu] [nvarchar](50) NULL,
	[UuTien] [smallint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoDangKyNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoDangKyNghi](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[NVMaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[LyDoNghi] [nvarchar](40) NULL,
	[LoaiNghi] [nvarchar](40) NULL,
	[NgayApDung] [datetime] NULL,
	[NgayKetThuc] [datetime] NULL,
	[UuTien] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoDangKyUuDai]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoDangKyUuDai](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[NVMaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[LoaiUuDai] [nvarchar](40) NULL,
	[NgayApDung] [datetime] NULL,
	[NgayKetThuc] [datetime] NULL,
	[UuTien] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoDMVS]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoDMVS](
	[BCMaNV] [int] NULL,
	[BCMaBP] [int] NULL,
	[MaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[BoPhan] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[Ngay] [datetime] NULL,
	[CaLV] [nvarchar](10) NULL,
	[TGDen] [datetime] NULL,
	[TGVe] [datetime] NULL,
	[TGDiMuon] [nvarchar](8) NULL,
	[TGVeSom] [nvarchar](8) NULL,
	[GhiChu] [nvarchar](50) NULL,
	[UuTien] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoDSCa]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoDSCa](
	[CMa] [int] NOT NULL,
	[CTen1] [nvarchar](10) NULL,
	[CTen] [nvarchar](10) NOT NULL,
	[CVietTat] [nvarchar](5) NULL,
	[CTGBatDau] [datetime] NULL,
	[CTGBDNghi1] [datetime] NULL,
	[CTGKTNghi1] [datetime] NULL,
	[CTGBDNghi2] [datetime] NULL,
	[CTGKTNghi2] [datetime] NULL,
	[CTGBDNghi3] [datetime] NULL,
	[CTGKTNghi3] [datetime] NULL,
	[CTGKetThuc] [datetime] NULL,
	[CBDTinhLT] [datetime] NULL,
	[CTGNghiGiuaGio] [smallint] NULL,
	[CTGQDD] [smallint] NULL,
	[CTGQDC] [smallint] NULL,
	[CDuocRaNgoai] [nvarchar](10) NULL,
	[CTinhVaoSom] [nvarchar](10) NULL,
	[CBuTGLam] [nvarchar](10) NULL,
	[CCongNghiGiuaCa] [nvarchar](10) NULL,
	[CNguongLamThem] [tinyint] NULL,
	[CDonViLamThem] [tinyint] NULL,
	[CNguongDiMuon] [tinyint] NULL,
	[CNguongVeSom] [tinyint] NULL,
	[CQuetTruocCa] [smallint] NULL,
	[CQuetSauCa] [smallint] NULL,
	[CDonViChamCong] [int] NULL,
	[CLoaiCa] [tinyint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoDSNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoDSNV](
	[MaBP] [smallint] NULL,
	[BoPhan] [nvarchar](40) NULL,
	[MaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[NgayBatDau] [datetime] NULL,
	[NgayKetThuc] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoERR]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoERR](
	[MaNV] [int] NULL,
	[Ngay] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoHopDong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoHopDong](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[STTBP] [smallint] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](14) NULL,
	[TenNV] [nvarchar](40) NULL,
	[HeSoLuong] [real] NULL,
	[NgaySinh] [datetime] NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[BatDauHD] [datetime] NULL,
	[KetThucHD] [datetime] NULL,
	[DiaChi] [nvarchar](100) NULL,
	[STTNV] [smallint] NULL,
	[GhiChu] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoHopDongBP]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoHopDongBP](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[STTBP] [smallint] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](14) NULL,
	[TenNV] [nvarchar](40) NULL,
	[HSLuong] [real] NULL,
	[NgaySinh] [datetime] NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[BatDauHD] [datetime] NULL,
	[KetThucHD] [datetime] NULL,
	[DiaChi] [nvarchar](100) NULL,
	[STTNV] [smallint] NULL,
	[GhiChu] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoK]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoK](
	[BCNgay] [datetime] NOT NULL,
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
	[BCTGLTToiDa] [smallint] NOT NULL,
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
	[BCCNLamBT] [bit] NULL,
	[BCHienThiNL] [bit] NULL,
	[BCDangKyUuDai] [bit] NULL,
	[BCLoaiTGDen] [bit] NULL,
	[BCLoaiTGRa] [bit] NULL,
	[BCLoaiTGVao] [bit] NULL,
	[BCLoaiTGVe] [bit] NULL,
	[DLocked] [bit] NULL,
	[BCTGLTToiDaTC] [int] NULL,
	[BCTGQuaGioNgayTC] [int] NULL,
	[BCTGQuaGioToiTC] [int] NULL,
	[BCNgayLe] [int] NULL,
	[BCNgayLeNV] [int] NULL,
	[BCTGUuDaiN] [int] NULL,
	[BCTGUuDaiD] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoLoainghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoLoainghi](
	[BCMa] [int] NOT NULL,
	[BCTen] [nvarchar](40) NULL,
	[BCHTCC] [nvarchar](5) NULL,
	[BCHTDKN] [nvarchar](5) NULL,
	[BCCong] [nvarchar](5) NULL,
	[BCTinhNgayNghi] [nvarchar](5) NULL,
	[BCTinhNgayLe] [nvarchar](5) NULL,
	[BCHeSo1] [float] NULL,
	[BCHeSo2] [float] NULL,
	[BCHeSo3] [float] NULL,
	[BCUuTien] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoNhaAn]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoNhaAn](
	[MaNV] [nvarchar](14) NOT NULL,
	[DauDoc] [tinyint] NOT NULL,
	[Ngay] [datetime] NOT NULL,
	[Ca] [int] NULL,
	[ThoiGian] [datetime] NULL,
	[STT] [int] NOT NULL,
	[GhiChu] [nvarchar](200) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoNhanSu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoNhanSu](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[TongSoNV] [smallint] NULL,
	[TSNam] [smallint] NULL,
	[TSNu] [smallint] NULL,
	[TSNoiTinh] [smallint] NULL,
	[TSNgoaiTinh] [smallint] NULL,
	[TSVaoCTy] [smallint] NULL,
	[TSTuThoiViec] [smallint] NULL,
	[TSBuocThoiViec] [smallint] NULL,
	[TSChuyenDen] [smallint] NULL,
	[TSChuyenDi] [smallint] NULL,
	[STT] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoNVTheoGT]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoNVTheoGT](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[STTBP] [int] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](10) NULL,
	[TenNV] [nvarchar](40) NULL,
	[STTNV] [int] NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[DiaChi] [nvarchar](200) NULL,
	[NgaySinh] [datetime] NULL,
	[NgayVao] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoNVVaoCTy]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoNVVaoCTy](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[STTBP] [int] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](10) NULL,
	[TenNV] [nvarchar](40) NULL,
	[STTNV] [int] NULL,
	[DiaChi] [nvarchar](200) NULL,
	[NgaySinh] [datetime] NULL,
	[NgayVao] [datetime] NULL,
	[luong] [numeric](18, 0) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoQuetThe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoQuetThe](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[NVMaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[MaThe] [nvarchar](14) NULL,
	[ThoiGian] [datetime] NULL,
	[Cua] [nvarchar](40) NULL,
	[TrangThai] [nvarchar](40) NULL,
	[UuTien] [smallint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoSYLLChinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoSYLLChinh](
	[Anh] [image] NULL,
	[MaNhanVien] [nvarchar](20) NULL,
	[HoTen] [nvarchar](40) NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[BoPhan] [nvarchar](100) NULL,
	[ChucVu] [nvarchar](100) NULL,
	[BiDanh] [nvarchar](40) NULL,
	[DanToc] [nvarchar](50) NULL,
	[QuocTich] [nvarchar](50) NULL,
	[NgaySinh] [date] SPARSE  NULL,
	[NoiSinh] [nvarchar](300) NULL,
	[NguyenQuan] [nvarchar](300) NULL,
	[HoKhauThuongTru] [nvarchar](300) NULL,
	[ChoOHienNay] [nvarchar](300) NULL,
	[DTCoDinh] [nvarchar](50) NULL,
	[DTDiDong] [nvarchar](50) NULL,
	[SoCMTND] [nvarchar](15) NULL,
	[NgayCapCMTND] [date] NULL,
	[NoiCapCMTND] [nvarchar](300) NULL,
	[TrinhDoVanHoa] [nvarchar](50) NULL,
	[NgheNghiepChuyenMon] [nvarchar](50) NULL,
	[CapBac] [nvarchar](50) NULL,
	[NgayVaoDang] [date] NULL,
	[NoiKetNapDang] [nvarchar](300) NULL,
	[NguoiLienHe] [nvarchar](50) NULL,
	[DiaChiNLH] [nvarchar](300) NULL,
	[DienThoaiNLH] [nvarchar](50) NULL,
	[NgayVaoCTy] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoSYLLTongHop]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoSYLLTongHop](
	[Anh] [image] NULL,
	[MaNhanVien] [nvarchar](20) NULL,
	[HoTen] [nvarchar](40) NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[BiDanh] [nvarchar](40) NULL,
	[DanToc] [nvarchar](50) NULL,
	[QuocTich] [nvarchar](50) NULL,
	[NgaySinh] [datetime] NULL,
	[NoiSinh] [nvarchar](250) NULL,
	[NguyenQuan] [nvarchar](250) NULL,
	[HoKhauThuongTru] [nvarchar](250) NULL,
	[ChoOHienNay] [nvarchar](250) NULL,
	[DTCoDinh] [nvarchar](50) NULL,
	[DTDiDong] [nvarchar](50) NULL,
	[SoCMTND] [nvarchar](15) NULL,
	[NgayCapCMTND] [datetime] NULL,
	[NoiCapCMTND] [nvarchar](200) NULL,
	[TrinhDoVanHoa] [nvarchar](50) NULL,
	[NgheNghiepChuyenMon] [nvarchar](50) NULL,
	[CapBac] [nvarchar](50) NULL,
	[NgayVaoDang] [datetime] NULL,
	[NoiKetNapDang] [nvarchar](200) NULL,
	[NguoiLienHe] [nvarchar](50) NULL,
	[DiaChiNLH] [nvarchar](200) NULL,
	[DienThoaiNLH] [nvarchar](50) NULL,
	[QHGDQuanHe1] [nvarchar](30) NULL,
	[QHGDTen1] [nvarchar](40) NULL,
	[QHGDNgaySinh1] [datetime] NULL,
	[QHGDNoiO1] [nvarchar](100) NULL,
	[QHGDCongViec1] [nvarchar](200) NULL,
	[QHGDQuanHe2] [nvarchar](30) NULL,
	[QHGDTen2] [nvarchar](40) NULL,
	[QHGDNgaySinh2] [datetime] NULL,
	[QHGDNoiO2] [nvarchar](100) NULL,
	[QHGDCongViec2] [nvarchar](200) NULL,
	[QHGDQuanHe3] [nvarchar](30) NULL,
	[QHGDTen3] [nvarchar](40) NULL,
	[QHGDNgaySinh3] [datetime] NULL,
	[QHGDNoiO3] [nvarchar](100) NULL,
	[QHGDCongViec3] [nvarchar](200) NULL,
	[QHGDQuanHe4] [nvarchar](30) NULL,
	[QHGDTen4] [nvarchar](40) NULL,
	[QHGDNgaySinh4] [datetime] NULL,
	[QHGDNoiO4] [nvarchar](100) NULL,
	[QHGDCongViec4] [nvarchar](200) NULL,
	[QHGDQuanHe5] [nvarchar](30) NULL,
	[QHGDTen5] [nvarchar](40) NULL,
	[QHGDNgaySinh5] [datetime] NULL,
	[QHGDNoiO5] [nvarchar](100) NULL,
	[QHGDCongViec5] [nvarchar](200) NULL,
	[QHGDQuanHe6] [nvarchar](30) NULL,
	[QHGDTen6] [nvarchar](40) NULL,
	[QHGDNgaySinh6] [datetime] NULL,
	[QHGDNoiO6] [nvarchar](100) NULL,
	[QHGDCongViec6] [nvarchar](200) NULL,
	[QHGDQuanHe7] [nvarchar](30) NULL,
	[QHGDTen7] [nvarchar](40) NULL,
	[QHGDNgaySinh7] [datetime] NULL,
	[QHGDNoiO7] [nvarchar](100) NULL,
	[QHGDCongViec7] [nvarchar](200) NULL,
	[QHGDQuanHe8] [nvarchar](30) NULL,
	[QHGDTen8] [nvarchar](40) NULL,
	[QHGDNgaySinh8] [datetime] NULL,
	[QHGDNoiO8] [nvarchar](100) NULL,
	[QHGDCongViec8] [nvarchar](200) NULL,
	[QHGDQuanHe9] [nvarchar](30) NULL,
	[QHGDTen9] [nvarchar](40) NULL,
	[QHGDNgaySinh9] [datetime] NULL,
	[QHGDNoiO9] [nvarchar](100) NULL,
	[QHGDCongViec9] [nvarchar](200) NULL,
	[QHGDQuanHe10] [nvarchar](30) NULL,
	[QHGDTen10] [nvarchar](40) NULL,
	[QHGDNgaySinh10] [datetime] NULL,
	[QHGDNoiO10] [nvarchar](100) NULL,
	[QHGDCongViec10] [nvarchar](200) NULL,
	[QTCTThoiGian1] [nvarchar](25) NULL,
	[QTCTChucVu1] [nvarchar](100) NULL,
	[QTCTMucLuong1] [numeric](18, 0) NULL,
	[QTCTNoiCT1] [nvarchar](200) NULL,
	[QTCTThoiGian2] [nvarchar](25) NULL,
	[QTCTChucVu2] [nvarchar](100) NULL,
	[QTCTMucLuong2] [numeric](18, 0) NULL,
	[QTCTNoiCT2] [nvarchar](200) NULL,
	[QTCTThoiGian3] [nvarchar](25) NULL,
	[QTCTChucVu3] [nvarchar](100) NULL,
	[QTCTMucLuong3] [numeric](18, 0) NULL,
	[QTCTNoiCT3] [nvarchar](200) NULL,
	[QTCTThoiGian4] [nvarchar](25) NULL,
	[QTCTChucVu4] [nvarchar](100) NULL,
	[QTCTMucLuong4] [numeric](18, 0) NULL,
	[QTCTNoiCT4] [nvarchar](200) NULL,
	[QTCTThoiGian5] [nvarchar](25) NULL,
	[QTCTChucVu5] [nvarchar](100) NULL,
	[QTCTMucLuong5] [numeric](18, 0) NULL,
	[QTCTNoiCT5] [nvarchar](200) NULL,
	[QTCTThoiGian6] [nvarchar](25) NULL,
	[QTCTChucVu6] [nvarchar](100) NULL,
	[QTCTMucLuong6] [numeric](18, 0) NULL,
	[QTCTNoiCT6] [nvarchar](200) NULL,
	[QTCTThoiGian7] [nvarchar](25) NULL,
	[QTCTChucVu7] [nvarchar](100) NULL,
	[QTCTMucLuong7] [numeric](18, 0) NULL,
	[QTCTNoiCT7] [nvarchar](200) NULL,
	[QTCTThoiGian8] [nvarchar](25) NULL,
	[QTCTChucVu8] [nvarchar](100) NULL,
	[QTCTMucLuong8] [numeric](18, 0) NULL,
	[QTCTNoiCT8] [nvarchar](200) NULL,
	[QTCTThoiGian9] [nvarchar](25) NULL,
	[QTCTChucVu9] [nvarchar](100) NULL,
	[QTCTMucLuong9] [numeric](18, 0) NULL,
	[QTCTNoiCT9] [nvarchar](200) NULL,
	[QTCTThoiGian10] [nvarchar](25) NULL,
	[QTCTChucVu10] [nvarchar](100) NULL,
	[QTCTMucLuong10] [numeric](18, 0) NULL,
	[QTCTNoiCT10] [nvarchar](200) NULL,
	[KTSoVB1] [nvarchar](10) NULL,
	[KTNgay1] [datetime] NULL,
	[KTNoiDung1] [nvarchar](200) NULL,
	[KTHinhThuc1] [nvarchar](200) NULL,
	[KTSoVB2] [nvarchar](10) NULL,
	[KTNgay2] [datetime] NULL,
	[KTNoiDung2] [nvarchar](200) NULL,
	[KTHinhThuc2] [nvarchar](200) NULL,
	[KTSoVB3] [nvarchar](10) NULL,
	[KTNgay3] [datetime] NULL,
	[KTNoiDung3] [nvarchar](200) NULL,
	[KTHinhThuc3] [nvarchar](200) NULL,
	[KTSoVB4] [nvarchar](10) NULL,
	[KTNgay4] [datetime] NULL,
	[KTNoiDung4] [nvarchar](200) NULL,
	[KTHinhThuc4] [nvarchar](200) NULL,
	[KTSoVB5] [nvarchar](10) NULL,
	[KTNgay5] [datetime] NULL,
	[KTNoiDung5] [nvarchar](200) NULL,
	[KTHinhThuc5] [nvarchar](200) NULL,
	[KTSoVB6] [nvarchar](10) NULL,
	[KTNgay6] [datetime] NULL,
	[KTNoiDung6] [nvarchar](200) NULL,
	[KTHinhThuc6] [nvarchar](200) NULL,
	[KTSoVB7] [nvarchar](10) NULL,
	[KTNgay7] [datetime] NULL,
	[KTNoiDung7] [nvarchar](200) NULL,
	[KTHinhThuc7] [nvarchar](200) NULL,
	[KTSoVB8] [nvarchar](10) NULL,
	[KTNgay8] [datetime] NULL,
	[KTNoiDung8] [nvarchar](200) NULL,
	[KTHinhThuc8] [nvarchar](200) NULL,
	[KTSoVB9] [nvarchar](10) NULL,
	[KTNgay9] [datetime] NULL,
	[KTNoiDung9] [nvarchar](200) NULL,
	[KTHinhThuc9] [nvarchar](200) NULL,
	[KTSoVB10] [nvarchar](10) NULL,
	[KTNgay10] [datetime] NULL,
	[KTNoiDung10] [nvarchar](200) NULL,
	[KTHinhThuc10] [nvarchar](200) NULL,
	[KLSoVB1] [nvarchar](10) NULL,
	[KLNgay1] [datetime] NULL,
	[KLNoiDung1] [nvarchar](200) NULL,
	[KLHinhThuc1] [nvarchar](200) NULL,
	[KLSoVB2] [nvarchar](10) NULL,
	[KLNgay2] [datetime] NULL,
	[KLNoiDung2] [nvarchar](200) NULL,
	[KLHinhThuc2] [nvarchar](200) NULL,
	[KLSoVB3] [nvarchar](10) NULL,
	[KLNgay3] [datetime] NULL,
	[KLNoiDung3] [nvarchar](200) NULL,
	[KLHinhThuc3] [nvarchar](200) NULL,
	[KLSoVB4] [nvarchar](10) NULL,
	[KLNgay4] [datetime] NULL,
	[KLNoiDung4] [nvarchar](200) NULL,
	[KLHinhThuc4] [nvarchar](200) NULL,
	[KLSoVB5] [nvarchar](10) NULL,
	[KLNgay5] [datetime] NULL,
	[KLNoiDung5] [nvarchar](200) NULL,
	[KLHinhThuc5] [nvarchar](200) NULL,
	[KLSoVB6] [nvarchar](10) NULL,
	[KLNgay6] [datetime] NULL,
	[KLNoiDung6] [nvarchar](200) NULL,
	[KLHinhThuc6] [nvarchar](200) NULL,
	[KLSoVB7] [nvarchar](10) NULL,
	[KLNgay7] [datetime] NULL,
	[KLNoiDung7] [nvarchar](200) NULL,
	[KLHinhThuc7] [nvarchar](200) NULL,
	[KLSoVB8] [nvarchar](10) NULL,
	[KLNgay8] [datetime] NULL,
	[KLNoiDung8] [nvarchar](200) NULL,
	[KLHinhThuc8] [nvarchar](200) NULL,
	[KLSoVB9] [nvarchar](10) NULL,
	[KLNgay9] [datetime] NULL,
	[KLNoiDung9] [nvarchar](200) NULL,
	[KLHinhThuc9] [nvarchar](200) NULL,
	[KLSoVB10] [nvarchar](10) NULL,
	[KLNgay10] [datetime] NULL,
	[KLNoiDung10] [nvarchar](200) NULL,
	[KLHinhThuc10] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoThoiGian]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoThoiGian](
	[BCMaBP] [int] NOT NULL,
	[BCTenBP] [nvarchar](40) NULL,
	[BCMaNV] [nvarchar](10) NULL,
	[BCTenNV] [nvarchar](50) NULL,
	[BCChucVu] [nvarchar](40) NULL,
	[BCNgay] [datetime] NULL,
	[BCTGVao] [datetime] NULL,
	[BCTGRa] [datetime] NULL,
	[BCTGLamViec] [smallint] NULL,
	[BCTGQuaGio] [smallint] NULL,
	[BCTGThieuGio] [smallint] NULL,
	[BCTGMuon] [smallint] NULL,
	[BCTGSom] [smallint] NULL,
	[BCGhiChu] [nvarchar](50) NULL,
	[BCSTT] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoThoiViec]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoThoiViec](
	[BCSTTBP] [int] NULL,
	[BCMaBP] [int] NULL,
	[BCTenBP] [nvarchar](40) NULL,
	[BCMaNV1] [int] NULL,
	[BCMaNV2] [nvarchar](10) NULL,
	[BCTenNV] [nvarchar](40) NULL,
	[BCMaThe] [nvarchar](15) NULL,
	[BCChucVu] [nvarchar](40) NULL,
	[BCNgayVao] [datetime] NULL,
	[BCNgayThoi] [datetime] NULL,
	[BCHinhThucTV] [nvarchar](50) NULL,
	[BCLyDo] [nvarchar](200) NULL,
	[BCDiaChi] [nvarchar](250) NULL,
	[BCQueQuan] [nvarchar](250) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoThongKe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoThongKe](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](30) NULL,
	[STTBP] [tinyint] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](14) NULL,
	[TenNV] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](50) NULL,
	[NgayVaoCT] [datetime] NULL,
	[NgaySinh] [datetime] NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[NoiOHienTai] [nvarchar](200) NULL,
	[DienThoai] [nvarchar](50) NULL,
	[STTNV] [smallint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoThongKeNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoThongKeNV](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](30) NULL,
	[STTBP] [tinyint] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](14) NULL,
	[TenNV] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](50) NULL,
	[NgayVaoCT] [datetime] NULL,
	[NgaySinh] [datetime] NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[NoiOHienTai] [nvarchar](200) NULL,
	[DienThoai] [nvarchar](50) NULL,
	[STTNV] [smallint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoTKDKNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoTKDKNghi](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](30) NULL,
	[STTBP] [tinyint] NULL,
	[MaNV1] [int] NULL,
	[MaNV2] [nvarchar](14) NULL,
	[TenNV] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](50) NULL,
	[LDT1] [nvarchar](5) NULL,
	[Tong1] [tinyint] NULL,
	[LDT2] [nvarchar](5) NULL,
	[Tong2] [tinyint] NULL,
	[LDT3] [nvarchar](5) NULL,
	[Tong3] [tinyint] NULL,
	[LDT4] [nvarchar](5) NULL,
	[Tong4] [tinyint] NULL,
	[LDT5] [nvarchar](5) NULL,
	[Tong5] [tinyint] NULL,
	[LDT6] [nvarchar](5) NULL,
	[Tong6] [tinyint] NULL,
	[LDT7] [nvarchar](5) NULL,
	[Tong7] [tinyint] NULL,
	[LDT8] [nvarchar](5) NULL,
	[Tong8] [tinyint] NULL,
	[LDT9] [nvarchar](5) NULL,
	[Tong9] [tinyint] NULL,
	[LDT10] [nvarchar](5) NULL,
	[Tong10] [tinyint] NULL,
	[LDT11] [nvarchar](5) NULL,
	[Tong11] [tinyint] NULL,
	[LDT12] [nvarchar](5) NULL,
	[Tong12] [tinyint] NULL,
	[LDCaNam] [nvarchar](5) NULL,
	[TongCaNam] [smallint] NULL,
	[STTNV] [smallint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblbaocaotonghopThang]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblbaocaotonghopThang](
	[NVma] [int] NOT NULL,
	[NVmaNV] [nvarchar](10) NOT NULL,
	[NVHoTen] [nvarchar](40) NOT NULL,
	[BPTen] [nvarchar](40) NOT NULL,
	[CVTen] [nvarchar](40) NULL,
	[Thang1] [float] NULL,
	[TongNghi] [float] NULL,
	[NghØ èm] [float] NULL,
	[TongDMVS] [int] NULL,
	[TongTGDMVS] [numeric](18, 6) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoVangMat]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoVangMat](
	[BCMaBP] [int] NULL,
	[BCTenBP] [nvarchar](40) NULL,
	[BCSTTNV] [int] NULL,
	[BCMaNV2] [nvarchar](10) NULL,
	[BCTenNV] [nvarchar](40) NULL,
	[BCChucVu] [nvarchar](40) NULL,
	[BCNgay] [datetime] NULL,
	[BCGhiChu] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoVangMat_KLD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoVangMat_KLD](
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](40) NULL,
	[STTNV] [int] NULL,
	[MaNV2] [nvarchar](10) NULL,
	[TenNV] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[Ngay] [datetime] NULL,
	[GhiChu] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaoCaoVaoRa]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaoCaoVaoRa](
	[MaBP] [int] NULL,
	[BoPhan] [nvarchar](40) NULL,
	[UuTien] [int] NULL,
	[MaNV] [nvarchar](10) NULL,
	[HoTen] [nvarchar](40) NULL,
	[ChucVu] [nvarchar](40) NULL,
	[Ngay] [datetime] NULL,
	[TGVao] [nvarchar](30) NULL,
	[TGRa] [nvarchar](30) NULL,
	[GhiChu] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaocaoVaoraTam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaocaoVaoraTam](
	[NVMa] [nvarchar](20) NULL,
	[HoTen] [nvarchar](100) NULL,
	[MaBP] [int] NULL,
	[TenBP] [nvarchar](100) NULL,
	[MaChucVu] [int] NULL,
	[TenChucVu] [nvarchar](100) NULL,
	[Ngay] [smalldatetime] NULL,
	[GioVao] [varchar](10) NULL,
	[GioRa] [varchar](10) NULL,
	[IDCard] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBaocaoVaoraTam_Giora]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBaocaoVaoraTam_Giora](
	[NVMa] [nvarchar](20) NULL,
	[MaBP] [int] NULL,
	[Ngay] [smalldatetime] NULL,
	[GioRa] [varchar](10) NULL,
	[IDCard] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBCDSBoPhan]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBCDSBoPhan](
	[BCSTTBP] [int] NULL,
	[BCTenBP] [nvarchar](255) NULL,
	[BCHTBC] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBCDSNgayNghiLe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBCDSNgayNghiLe](
	[BCNgay] [datetime] NULL,
	[BCTenV] [nvarchar](40) NULL,
	[BCTenE] [nvarchar](40) NULL,
	[BCLoai] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBCDSNhanVien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBCDSNhanVien](
	[BCSTTBP] [int] NULL,
	[BCTenBP] [nvarchar](40) NOT NULL,
	[BCSTTNV] [int] NOT NULL,
	[BCMaNV] [nvarchar](10) NULL,
	[BCTenNV] [nvarchar](40) NULL,
	[BCChucVu] [nvarchar](40) NULL,
	[BCNgaySinh] [datetime] NOT NULL,
	[BCNgayVao] [datetime] NOT NULL,
	[BCNgayRa] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBCDSTheNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBCDSTheNV](
	[MaNV] [int] NOT NULL,
	[MaNV1] [nvarchar](10) NOT NULL,
	[TenNV1] [nvarchar](40) NOT NULL,
	[AnhNV1] [image] NULL,
	[MaNV2] [nvarchar](10) NOT NULL,
	[TenNV2] [nvarchar](40) NOT NULL,
	[AnhNV2] [image] NULL,
	[BPTen1] [nvarchar](40) NULL,
	[BPTen2] [nvarchar](40) NULL,
	[ChucVu1] [nvarchar](40) NULL,
	[ChucVu2] [nvarchar](40) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBCTuDien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBCTuDien](
	[BCTenV] [nvarchar](40) NULL,
	[BCTenE] [nvarchar](40) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBenhVien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBenhVien](
	[MaBV] [nvarchar](50) NOT NULL,
	[TenBV] [nvarchar](50) NOT NULL,
	[GhiChu] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBoPhanCon]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBoPhanCon](
	[BPConLoginID] [int] NULL,
	[BPConMaBP] [int] NULL,
	[BPConMaCha] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBoPhanTam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBoPhanTam](
	[BPMa] [int] NULL,
	[BPMaCha] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblChucVuBaoHiem]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblChucVuBaoHiem](
	[Ma] [varchar](10) NOT NULL,
	[Ten] [nvarchar](250) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblChuyenBoPhan]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblChuyenBoPhan](
	[MaTuSinh] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [int] NOT NULL,
	[MaBP] [int] NOT NULL,
	[MaBPDen] [int] NOT NULL,
	[NgayChuyen] [datetime] NOT NULL,
	[GhiChu] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblChuyenNganh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblChuyenNganh](
	[CNMa] [int] IDENTITY(1,1) NOT NULL,
	[CNTen] [nvarchar](30) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCongTac]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCongTac](
	[MaTuSinh] [int] IDENTITY(1,1) NOT NULL,
	[CTMa] [int] NOT NULL,
	[Thoigian] [nvarchar](25) NOT NULL,
	[NoiCT] [nvarchar](200) NOT NULL,
	[Mucluong] [numeric](18, 0) NOT NULL,
	[Chucvu_NN] [nvarchar](100) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCongViec]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCongViec](
	[CVMa] [int] IDENTITY(1,1) NOT NULL,
	[CVTen] [nvarchar](50) NOT NULL,
	[CVNoiDung] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCTNuocNgoai]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCTNuocNgoai](
	[MaTuSinh] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [int] NOT NULL,
	[MaQG] [smallint] NOT NULL,
	[Tungay] [datetime] NOT NULL,
	[CViec] [nvarchar](100) NOT NULL,
	[Denngay] [datetime] NOT NULL,
	[KinhPhi] [nvarchar](30) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDangKyNghiBu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDangKyNghiBu](
	[NBMa] [varchar](10) NOT NULL,
	[NBMaNV] [int] NOT NULL,
	[NBNgayNghi] [datetime] NOT NULL,
	[NBNgayLamBu] [datetime] NOT NULL,
	[NBGhiChu] [nvarchar](200) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_tblDangKyNghiBu] PRIMARY KEY CLUSTERED 
(
	[NBMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDangkynghiTam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDangkynghiTam](
	[DKNMaNV] [int] NOT NULL,
	[DKNNgayApDung] [datetime] NOT NULL,
	[DKNNgayKetThuc] [datetime] NOT NULL,
	[DKNMaLyDo] [int] NOT NULL,
	[DKNLoai] [tinyint] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDangKyNghiTheoGio]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDangKyNghiTheoGio](
	[DKNMa] [varchar](10) NOT NULL,
	[MaNV] [int] NOT NULL,
	[Ngay] [datetime] NOT NULL,
	[TGNghi] [datetime] NOT NULL,
	[Sophut] [int] NOT NULL,
	[MaLyDo] [varchar](10) NOT NULL,
	[GhiChu] [nvarchar](200) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_tblDangKyNghiTheoGio1] PRIMARY KEY CLUSTERED 
(
	[DKNMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDangKyUuDai]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDangKyUuDai](
	[DKUDMa] [varchar](50) NOT NULL,
	[DKUDMaNV] [int] NOT NULL,
	[DKUDLoaiUuDai] [varchar](50) NOT NULL,
	[DKUDNgayApDung] [datetime] NOT NULL,
	[DKUDNgayKetThuc] [datetime] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_tblDangKyUuDai2010] PRIMARY KEY CLUSTERED 
(
	[DKUDMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDangkyUudaiTam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDangkyUudaiTam](
	[DKUDMaNV] [int] NOT NULL,
	[DKUDLoaiUuDai] [int] NOT NULL,
	[DKUDNgayApDung] [datetime] NOT NULL,
	[DKUDNgayKetThuc] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDauDoc]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDauDoc](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DDMa] [varchar](2) NOT NULL,
	[DDTen] [nvarchar](40) NOT NULL,
	[DDSoBanGhi] [int] NOT NULL,
	[DDChinhVao] [bit] NOT NULL,
	[DDLoai] [tinyint] NOT NULL,
	[DDCongGiaoTiep] [tinyint] NOT NULL,
	[DDloaithe] [tinyint] NOT NULL,
	[DDIP] [nvarchar](50) NULL,
	[DDPort] [int] NULL,
	[DDKieuKN] [bit] NULL,
	[DDBaudRate] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DDLanTaiCuoi] [datetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[DDLoaiChamCong] [nvarchar](50) NULL,
	[DDTuDongBoTG] [bit] NULL,
	[DDKhongDangKyUser] [bit] NULL,
	[DDMaBaoMat] [nvarchar](50) NULL,
	[DDKhongTuDongXoaDuLieu] [bit] NULL,
	[DDKey] [nvarchar](200) NULL,
	[DDSerial] [nvarchar](200) NULL,
	[IsPushSDK] [bit] NULL,
	[TransactionCount] [int] NULL,
	[MAC] [nvarchar](50) NULL,
	[FPCount] [int] NULL,
	[FaceCount] [int] NULL,
	[UserCount] [int] NULL,
 CONSTRAINT [PK_tblDauDoc] PRIMARY KEY CLUSTERED 
(
	[DDMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDauDocDangKyThe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDauDocDangKyThe](
	[DDMa] [varchar](2) NOT NULL,
	[DDTen] [nvarchar](40) NOT NULL,
	[DDSoBanGhi] [int] NOT NULL,
	[DDChinhVao] [bit] NOT NULL,
	[DDLoai] [tinyint] NOT NULL,
	[DDCongGiaoTiep] [tinyint] NOT NULL,
	[DDloaithe] [tinyint] NOT NULL,
	[DDIP] [nvarchar](50) NULL,
	[DDPort] [int] NULL,
	[DDKieuKN] [bit] NULL,
	[DDBaudRate] [nvarchar](50) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DDLanTaiCuoi] [datetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[DDLoaiChamCong] [nvarchar](50) NULL,
	[DDTuDongBoTG] [bit] NULL,
 CONSTRAINT [PK_tblDauDocDangKyThe] PRIMARY KEY CLUSTERED 
(
	[DDMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDSDangKyThe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDSDangKyThe](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[dktMaDauDoc] [nvarchar](5) NOT NULL,
	[dktMaThe] [nvarchar](14) NOT NULL,
	[dktLoai] [smallint] NOT NULL,
	[dktName] [nvarchar](50) NULL,
	[dktNgayapDung] [datetime] NULL,
	[dktNgayKetThuc] [datetime] NULL,
	[dktTimeZone] [nvarchar](200) NULL,
	[dktDoorGroup] [nvarchar](200) NULL,
	[dktCardLevel] [nvarchar](5) NULL,
	[dktUserID] [nvarchar](10) NULL,
	[dktPIN] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblExportlchamCong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblExportlchamCong](
	[NVMaNV] [nvarchar](10) NOT NULL,
	[NVHoTen] [nvarchar](40) NOT NULL,
	[BPTen] [nvarchar](40) NOT NULL,
	[BCCLCT_CT] [float] NULL,
	[BCCLBH100_CT] [float] NULL,
	[bcclloai] [varchar](2) NULL,
	[bccluutien] [int] NULL,
	[BCCLLamThemN_CT] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblGDChinhSach]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblGDChinhSach](
	[GDCSMa] [smallint] IDENTITY(1,1) NOT NULL,
	[GDCSTen] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblGioiTinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblGioiTinh](
	[GTMa] [bit] NOT NULL,
	[GTTen] [nvarchar](20) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHealth]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHealth](
	[NVMa] [int] NOT NULL,
	[SoThe] [nvarchar](10) NOT NULL,
	[TuNgay] [datetime] NULL,
	[DenNgay] [datetime] NULL,
	[Noicapthe] [nvarchar](30) NULL,
	[TGEnd] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHealthQuaTrinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHealthQuaTrinh](
	[NVMa] [int] NOT NULL,
	[ThoiGian] [nvarchar](20) NULL,
	[QuaTrinh] [nvarchar](30) NULL,
	[MaBV] [numeric](18, 0) NULL,
	[GhiChu] [nvarchar](30) NULL,
	[IdHealth] [numeric](18, 0) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHienthiBCC_Cha]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHienthiBCC_Cha](
	[HTMa] [int] IDENTITY(1,1) NOT NULL,
	[HTmaht] [varchar](50) NOT NULL,
	[HTTenV] [varchar](100) NOT NULL,
	[HTTenE] [varchar](100) NOT NULL,
	[HTInV] [varchar](50) NOT NULL,
	[HTInE] [varchar](50) NOT NULL,
	[HTInchuthichV] [varchar](200) NOT NULL,
	[HTInchuthichE] [varchar](200) NOT NULL,
	[HTDodai] [int] NOT NULL,
	[HTSQL] [varchar](256) NOT NULL,
	[HTChiaND] [bit] NOT NULL,
	[HTHienthi] [bit] NOT NULL,
	[HTUutien] [tinyint] NOT NULL,
 CONSTRAINT [PK_tblHienthiBCC] PRIMARY KEY CLUSTERED 
(
	[HTMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHienthiBCC_Con]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHienthiBCC_Con](
	[HTCMa] [int] IDENTITY(1,1) NOT NULL,
	[HTMa] [int] NOT NULL,
	[HTCTenV] [varchar](100) NOT NULL,
	[HTCTenE] [varchar](100) NOT NULL,
	[HTCInV] [varchar](50) NOT NULL,
	[HTCInE] [varchar](50) NOT NULL,
	[HTCInchuthichV] [varchar](200) NOT NULL,
	[HTCInchuthichE] [varchar](200) NOT NULL,
	[HTCDodai] [int] NOT NULL,
	[HTCSQL] [varchar](50) NOT NULL,
	[HTCUutien] [tinyint] NOT NULL,
 CONSTRAINT [PK_tblHienthiBCC_Con] PRIMARY KEY CLUSTERED 
(
	[HTCMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHoanCanhGD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHoanCanhGD](
	[DKGDMaNV] [int] NOT NULL,
	[DKGDLietSy] [bit] NOT NULL,
	[DKGDMaGDCS] [smallint] NOT NULL,
	[DKGDNguoiLH] [nvarchar](50) NOT NULL,
	[DKGDDienThoai] [nvarchar](20) NOT NULL,
	[DKGDDiaChi] [nvarchar](100) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHoChieu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHoChieu](
	[MaNV] [int] NOT NULL,
	[SoHC] [nvarchar](10) NULL,
	[TGHieuLuc] [datetime] NULL,
	[CapTai] [nvarchar](100) NULL,
	[NgayCap] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHocVi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHocVi](
	[HVMa] [int] IDENTITY(1,1) NOT NULL,
	[HVTen] [nvarchar](30) NOT NULL,
	[HVMoTa] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImportDienBienLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImportDienBienLuong](
	[F1] [float] NULL,
	[F2] [nvarchar](255) NULL,
	[F3] [nvarchar](255) NULL,
	[F4] [nvarchar](255) NULL,
	[F5] [datetime] NULL,
	[F6] [float] NULL,
	[F7] [float] NULL,
	[F8] [float] NULL,
	[F9] [float] NULL,
	[F10] [bit] NULL,
	[F11] [bit] NULL,
	[F12] [bit] NULL,
	[F13] [bit] NULL,
	[F14] [float] NULL,
	[F15] [float] NULL,
	[F16] [nvarchar](255) NULL,
	[F17] [nvarchar](255) NULL,
	[F18] [nvarchar](255) NULL,
	[F19] [nvarchar](255) NULL,
	[F20] [nvarchar](255) NULL,
	[F21] [nvarchar](255) NULL,
	[F22] [nvarchar](255) NULL,
	[F23] [nvarchar](255) NULL,
	[F24] [nvarchar](255) NULL,
	[F25] [nvarchar](255) NULL,
	[F26] [nvarchar](255) NULL,
	[F27] [nvarchar](255) NULL,
	[F28] [nvarchar](255) NULL,
	[F29] [nvarchar](255) NULL,
	[F30] [nvarchar](255) NULL,
	[F31] [nvarchar](255) NULL,
	[F32] [nvarchar](255) NULL,
	[F33] [nvarchar](255) NULL,
	[F34] [nvarchar](255) NULL,
	[F35] [nvarchar](255) NULL,
	[F36] [nvarchar](255) NULL,
	[F37] [nvarchar](255) NULL,
	[F38] [nvarchar](255) NULL,
	[F39] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImportLBangchamcong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImportLBangchamcong](
	[Cônng ty Rhythm Việt Nam] [nvarchar](255) NULL,
	[F2] [nvarchar](255) NULL,
	[F3] [nvarchar](255) NULL,
	[F4] [float] NULL,
	[F5] [float] NULL,
	[F6] [float] NULL,
	[F7] [float] NULL,
	[F8] [float] NULL,
	[F9] [float] NULL,
	[F10] [float] NULL,
	[F11] [float] NULL,
	[F12] [float] NULL,
	[F13] [float] NULL,
	[F14] [float] NULL,
	[F15] [float] NULL,
	[F16] [float] NULL,
	[F17] [float] NULL,
	[F18] [float] NULL,
	[F19] [float] NULL,
	[F20] [bit] NULL,
	[F21] [float] NULL,
	[F22] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImportLDienbienluong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImportLDienbienluong](
	[Cônng ty Rhythm Việt Nam] [float] NULL,
	[F2] [nvarchar](255) NULL,
	[F3] [nvarchar](255) NULL,
	[F4] [nvarchar](255) NULL,
	[F5] [nvarchar](255) NULL,
	[F6] [nvarchar](255) NULL,
	[F7] [datetime] NULL,
	[F8] [datetime] NULL,
	[F9] [datetime] NULL,
	[F10] [float] NULL,
	[F11] [float] NULL,
	[F12] [nvarchar](255) NULL,
	[F13] [nvarchar](255) NULL,
	[F14] [nvarchar](255) NULL,
	[F15] [nvarchar](255) NULL,
	[F16] [float] NULL,
	[F17] [float] NULL,
	[F18] [float] NULL,
	[F19] [float] NULL,
	[F20] [float] NULL,
	[F21] [float] NULL,
	[F22] [float] NULL,
	[F23] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImportLDieuChinhLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImportLDieuChinhLuong](
	[Cônng ty Rhythm Việt Nam] [float] NULL,
	[F2] [nvarchar](255) NULL,
	[F3] [nvarchar](255) NULL,
	[F4] [nvarchar](255) NULL,
	[F5] [float] NULL,
	[F6] [float] NULL,
	[F7] [bit] NULL,
	[F8] [float] NULL,
	[F9] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImportLuong]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImportLuong](
	[F2] [nvarchar](255) NULL,
	[F5] [nvarchar](255) NULL,
	[F6] [nvarchar](255) NULL,
	[F7] [float] NULL,
	[F8] [nvarchar](255) NULL,
	[F9] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImportRegisterLeaveDay]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImportRegisterLeaveDay](
	[Công ty cổ phần Digitech] [nvarchar](255) NULL,
	[F2] [nvarchar](255) NULL,
	[F3] [nvarchar](255) NULL,
	[F4] [float] NULL,
	[F5] [nvarchar](255) NULL,
	[F6] [nvarchar](255) NULL,
	[F7] [nvarchar](255) NULL,
	[F8] [nvarchar](255) NULL,
	[F9] [nvarchar](255) NULL,
	[F10] [nvarchar](255) NULL,
	[F11] [nvarchar](255) NULL,
	[F12] [nvarchar](255) NULL,
	[F13] [nvarchar](255) NULL,
	[F14] [nvarchar](255) NULL,
	[F15] [nvarchar](255) NULL,
	[F16] [nvarchar](255) NULL,
	[F17] [nvarchar](255) NULL,
	[F18] [nvarchar](255) NULL,
	[F19] [nvarchar](255) NULL,
	[F20] [nvarchar](255) NULL,
	[F21] [nvarchar](255) NULL,
	[F22] [nvarchar](255) NULL,
	[F23] [nvarchar](255) NULL,
	[F24] [nvarchar](255) NULL,
	[F25] [nvarchar](255) NULL,
	[F26] [nvarchar](255) NULL,
	[F27] [nvarchar](255) NULL,
	[F28] [nvarchar](255) NULL,
	[F29] [nvarchar](255) NULL,
	[F30] [nvarchar](255) NULL,
	[F31] [nvarchar](255) NULL,
	[F32] [nvarchar](255) NULL,
	[F33] [nvarchar](255) NULL,
	[F34] [nvarchar](255) NULL,
	[F35] [nvarchar](255) NULL,
	[F36] [nvarchar](255) NULL,
	[F37] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImportShift]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImportShift](
	[Công ty cổ phần Xi Mang Ha Tien 1 - Nha May Xi măng Kiên Lương] [float] NULL,
	[F2] [nvarchar](255) NULL,
	[F3] [nvarchar](255) NULL,
	[F4] [nvarchar](255) NULL,
	[F5] [nvarchar](255) NULL,
	[F6] [nvarchar](255) NULL,
	[F7] [nvarchar](255) NULL,
	[F8] [nvarchar](255) NULL,
	[F9] [nvarchar](255) NULL,
	[F10] [nvarchar](255) NULL,
	[F11] [nvarchar](255) NULL,
	[F12] [nvarchar](255) NULL,
	[F13] [nvarchar](255) NULL,
	[F14] [nvarchar](255) NULL,
	[F15] [nvarchar](255) NULL,
	[F16] [nvarchar](255) NULL,
	[F17] [nvarchar](255) NULL,
	[F18] [nvarchar](255) NULL,
	[F19] [nvarchar](255) NULL,
	[F20] [nvarchar](255) NULL,
	[F21] [nvarchar](255) NULL,
	[F22] [nvarchar](255) NULL,
	[F23] [nvarchar](255) NULL,
	[F24] [nvarchar](255) NULL,
	[F25] [nvarchar](255) NULL,
	[F26] [nvarchar](255) NULL,
	[F27] [nvarchar](255) NULL,
	[F28] [nvarchar](255) NULL,
	[F29] [nvarchar](255) NULL,
	[F30] [nvarchar](255) NULL,
	[F31] [nvarchar](255) NULL,
	[F32] [nvarchar](255) NULL,
	[F33] [nvarchar](255) NULL,
	[F34] [nvarchar](255) NULL,
	[F35] [nvarchar](255) NULL,
	[F36] [nvarchar](255) NULL,
	[F37] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblInOutThangtx]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblInOutThangtx](
	[NVMaNV] [nvarchar](10) NOT NULL,
	[NVHoTen] [nvarchar](40) NOT NULL,
	[BPMa] [int] NOT NULL,
	[BPTen] [nvarchar](40) NOT NULL,
	[Ngay] [datetime] NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[InOut] [varchar](3) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblInsurance]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblInsurance](
	[Idinsua] [int] NOT NULL,
	[NVMa] [int] NOT NULL,
	[ThoiGian] [nvarchar](20) NULL,
	[NoiDung] [nvarchar](30) NULL,
	[TienBaoHiem] [numeric](18, 0) NULL,
	[GhiChu] [nvarchar](30) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblKetQuaTD]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblKetQuaTD](
	[Ma] [int] IDENTITY(1,1) NOT NULL,
	[MaUV] [int] NULL,
	[NgayPV1] [datetime] NULL,
	[NguoiPV1] [nvarchar](255) NULL,
	[KetQua1] [nvarchar](255) NULL,
	[NoiDung1] [nvarchar](255) NULL,
	[NgayPV2] [nvarchar](255) NULL,
	[NguoiPV2] [nvarchar](255) NULL,
	[KetQua2] [nvarchar](255) NULL,
	[NoiDung2] [nvarchar](255) NULL,
	[NgayPV3] [nvarchar](255) NULL,
	[NguoiPV3] [nvarchar](255) NULL,
	[KetQua3] [nvarchar](255) NULL,
	[NoiDung3] [nvarchar](255) NULL,
	[KetLuan] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblKhoangNgay]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblKhoangNgay](
	[KNMaNV] [int] NULL,
	[KNNgay] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblKhoangNgay1]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblKhoangNgay1](
	[KNMaNV] [int] NULL,
	[KNNgay] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblKTKL]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblKTKL](
	[MaTuSinh] [int] IDENTITY(1,1) NOT NULL,
	[KTKLMa] [int] NOT NULL,
	[vbso] [nvarchar](10) NOT NULL,
	[ktkl] [bit] NOT NULL,
	[ngay] [datetime] NOT NULL,
	[noidung] [nvarchar](200) NOT NULL,
	[hinhthuc] [nvarchar](200) NOT NULL,
	[ghichu] [nvarchar](200) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLoaiDKNghi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLoaiDKNghi](
	[ldknMa] [varchar](50) NOT NULL,
	[ldknTen] [nvarchar](200) NULL,
 CONSTRAINT [PK_tblLoaiDKNghi] PRIMARY KEY CLUSTERED 
(
	[ldknMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLoaiUuDai]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLoaiUuDai](
	[LUDMa] [nvarchar](15) NOT NULL,
	[LUDTen] [nvarchar](40) NOT NULL,
	[LUDDauCa] [smallint] NOT NULL,
	[LUDTruocNghi] [smallint] NOT NULL,
	[LUDSauNghi] [smallint] NOT NULL,
	[LUDCuoiCa] [smallint] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLocDuLieu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLocDuLieu](
	[IDM] [int] NULL,
	[IDCard] [nvarchar](14) NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLocdulieutam]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLocdulieutam](
	[IDM] [tinyint] NOT NULL,
	[IDCard] [nvarchar](14) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
	[HandData] [bit] NOT NULL,
	[Pass] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLogin]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLogin](
	[logID] [int] IDENTITY(1,1) NOT NULL,
	[logUserName] [nvarchar](50) NULL,
	[logPassWord] [nvarchar](50) NULL,
	[logDID] [int] NULL,
	[logAdmin] [bit] NULL,
	[logFullName] [nvarchar](50) NULL,
	[logGender] [bit] NOT NULL,
	[logEmail] [nvarchar](50) NOT NULL,
	[logIP] [nvarchar](50) NOT NULL,
	[logYM] [nvarchar](50) NOT NULL,
	[logSkype] [nvarchar](50) NULL,
	[logMobile] [nvarchar](50) NOT NULL,
	[logCompanyID] [int] NOT NULL,
 CONSTRAINT [PK_wLogin] PRIMARY KEY CLUSTERED 
(
	[logID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNganHang]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNganHang](
	[NHMa] [int] IDENTITY(1,1) NOT NULL,
	[NHTen] [nvarchar](100) NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNgoaiNgu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNgoaiNgu](
	[NNMa] [smallint] IDENTITY(1,1) NOT NULL,
	[NNTen] [nvarchar](30) NOT NULL,
	[NNMoTa] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhanVien_BoPhan]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhanVien_BoPhan](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Thang] [int] NULL,
	[Nam] [int] NULL,
	[MaNV] [int] NULL,
	[MaBP] [int] NULL,
	[NgayChuyen] [datetime] NULL,
	[GhiChu] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhanVienCon]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhanVienCon](
	[NVConMaBP] [int] NOT NULL,
	[NVMaNV] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhanVienTemp]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhanVienTemp](
	[NVMaCu] [int] NULL,
	[NVMamoi] [int] NULL,
	[NVMaNVN] [varchar](50) NULL,
	[NVMaOK] [int] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhom]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhom](
	[NMa] [nvarchar](50) NOT NULL,
	[NTen] [nvarchar](40) NOT NULL,
	[NVietTat] [nvarchar](5) NOT NULL,
	[NMaCa1] [int] NOT NULL,
	[NSoNgay1] [tinyint] NOT NULL,
	[NMaCa2] [int] NOT NULL,
	[NSoNgay2] [tinyint] NOT NULL,
	[NMaCa3] [int] NOT NULL,
	[NSoNgay3] [tinyint] NOT NULL,
	[NMaCa4] [int] NOT NULL,
	[NSoNgay4] [tinyint] NOT NULL,
	[NNgayApDung] [datetime] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhom_QuyenTruycap]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhom_QuyenTruycap](
	[NhomTCID] [int] NULL,
	[QuyenTCID] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhomNhanVien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhomNhanVien](
	[NNVMa] [nvarchar](50) NOT NULL,
	[NNVMaNV] [int] NOT NULL,
	[NNVMaNhom] [nvarchar](50) NOT NULL,
	[NNVNgayApDung] [datetime] NOT NULL,
	[NNVNgayKetThuc] [datetime] NOT NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
 CONSTRAINT [PK_tblNhomNhanVien] PRIMARY KEY CLUSTERED 
(
	[NNVMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhomtruycap]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhomtruycap](
	[NhomTCID] [int] IDENTITY(1,1) NOT NULL,
	[NhomTCTenV] [nvarchar](100) NOT NULL,
	[NhomTCTenA] [nvarchar](100) NOT NULL,
	[NhomAdmin] [bit] NOT NULL,
	[OrderBy] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblParameter]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblParameter](
	[Parameter] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[description] [nvarchar](100) NOT NULL,
	[User1] [int] NULL,
	[Date1] [datetime] NULL,
	[user2] [int] NULL,
	[date2] [datetime] NULL,
 CONSTRAINT [PK_tblParameter] PRIMARY KEY CLUSTERED 
(
	[Parameter] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblQTLamViec]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblQTLamViec](
	[Ma] [int] IDENTITY(1,1) NOT NULL,
	[MaUV] [int] NULL,
	[LoaiQuaTrinh] [nvarchar](20) NULL,
	[TuNgay] [datetime] NULL,
	[DenNgay] [datetime] NULL,
	[ChuyenNganh] [nvarchar](255) NULL,
	[HinhThucDaoTao] [nvarchar](255) NULL,
	[TruongDaoTao] [nvarchar](255) NULL,
	[QuocGia] [nvarchar](255) NULL,
	[BangCap] [nvarchar](255) NULL,
	[DonVi] [nvarchar](255) NULL,
	[PhongBan] [nvarchar](255) NULL,
	[ChucVu] [nvarchar](255) NULL,
	[KiemNhiem] [nvarchar](255) NULL,
	[LuongNghiViec] [nvarchar](255) NULL,
	[ThoiGian] [nvarchar](255) NULL,
	[GhiChu] [nvarchar](255) NULL,
	[Hoten] [nvarchar](255) NULL,
	[NgheNghip] [nvarchar](255) NULL,
	[DiaChi] [nvarchar](255) NULL,
	[DienThoai] [nvarchar](255) NULL,
	[QuanHe] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Ma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblquettheloi]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblquettheloi](
	[NVMaNV] [nvarchar](10) NOT NULL,
	[NVHoTen] [nvarchar](40) NOT NULL,
	[BPMa] [int] NOT NULL,
	[BPTen] [nvarchar](40) NOT NULL,
	[ThoiGian] [datetime] NOT NULL,
	[InOut] [varchar](3) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTaiLieu]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTaiLieu](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StaffCode] [int] NULL,
	[FromDate] [date] NULL,
	[ToDate] [date] NULL,
	[FileName] [nvarchar](200) NULL,
	[FileGroup] [nvarchar](50) NULL,
	[Filetype] [nvarchar](200) NULL,
	[Filedata] [varbinary](max) NULL,
	[FileSite] [float] NULL,
	[FileKeyWord] [nvarchar](500) NULL,
	[FileReMark] [nvarchar](500) NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL,
 CONSTRAINT [PK_wordFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTaoBCCTemp]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTaoBCCTemp](
	[NVMa] [int] NOT NULL,
	[NVMaNV] [nvarchar](10) NOT NULL,
	[NVHoTen] [nchar](50) NOT NULL,
	[tgNVMa] [int] NULL,
	[Ngay01] [nvarchar](50) NULL,
	[Ngay02] [nvarchar](50) NULL,
	[Ngay03] [nvarchar](50) NULL,
	[Ngay04] [nvarchar](50) NULL,
	[Ngay05] [nvarchar](50) NULL,
	[Ngay06] [nvarchar](50) NULL,
	[Ngay07] [nvarchar](50) NULL,
	[Ngay08] [nvarchar](50) NULL,
	[Ngay09] [nvarchar](50) NULL,
	[Ngay10] [nvarchar](50) NULL,
	[Ngay11] [nvarchar](50) NULL,
	[Ngay12] [nvarchar](50) NULL,
	[Ngay13] [nvarchar](50) NULL,
	[Ngay14] [nvarchar](50) NULL,
	[Ngay15] [nvarchar](50) NULL,
	[Ngay16] [nvarchar](50) NULL,
	[Ngay17] [nvarchar](50) NULL,
	[Ngay18] [nvarchar](50) NULL,
	[Ngay19] [nvarchar](50) NULL,
	[Ngay20] [nvarchar](50) NULL,
	[Ngay21] [nvarchar](50) NULL,
	[Ngay22] [nvarchar](50) NULL,
	[Ngay23] [nvarchar](50) NULL,
	[Ngay24] [nvarchar](50) NULL,
	[Ngay25] [nvarchar](50) NULL,
	[Ngay26] [nvarchar](50) NULL,
	[Ngay27] [nvarchar](50) NULL,
	[Ngay28] [nvarchar](50) NULL,
	[Ngay29] [nvarchar](50) NULL,
	[Ngay30] [nvarchar](50) NULL,
	[Ngay31] [nvarchar](50) NULL,
	[1] [varchar](1) NULL,
	[clNVMa] [int] NULL,
	[TGLamN] [float] NULL,
	[TGLamD] [float] NULL,
	[TGLamthemN] [numeric](18, 6) NULL,
	[TGLamthemD] [numeric](18, 6) NULL,
	[TGDimuonN] [numeric](18, 6) NULL,
	[TGDimuonD] [numeric](18, 6) NULL,
	[TGVesomN] [numeric](18, 6) NULL,
	[TGVesomD] [numeric](18, 6) NULL,
	[SoLanDiMuon] [int] NULL,
	[SoLanVeSom] [int] NULL,
	[clNVMa_TV] [int] NULL,
	[TGLamN_TV] [float] NULL,
	[TGLamD_TV] [float] NULL,
	[TGLamthemN_TV] [numeric](18, 6) NULL,
	[TGLamthemD_TV] [numeric](18, 6) NULL,
	[TGDimuonN_TV] [numeric](18, 6) NULL,
	[TGDimuonD_TV] [numeric](18, 6) NULL,
	[TGVesomN_TV] [numeric](18, 6) NULL,
	[TGVesomD_TV] [numeric](18, 6) NULL,
	[SoLanDiMuon_TV] [int] NULL,
	[SoLanVeSom_TV] [int] NULL,
	[ltNVMa] [int] NULL,
	[TGLamthemcongtyN] [numeric](18, 6) NULL,
	[TGLamthemcongtyD] [numeric](18, 6) NULL,
	[BCCSoNghiCongTy] [int] NULL,
	[ltnlNVMa] [int] NULL,
	[TGLamthemNgayLeN] [numeric](18, 6) NULL,
	[TGLamthemNgayLeD] [numeric](18, 6) NULL,
	[BCCSoNghiLe] [int] NULL,
	[cntNVMa] [int] NULL,
	[SoCaHC] [float] NULL,
	[SoCa1] [float] NULL,
	[SoCa2] [float] NULL,
	[SoCa3] [float] NULL,
	[dknMaNV] [int] NULL,
	[dknMaBP] [int] NULL,
	[BCNghiPhep] [float] NULL,
	[BCNghiH100] [float] NULL,
	[BCNghiH70] [float] NULL,
	[BCNghiKL] [float] NULL,
	[BCNghiBH100] [float] NULL,
	[BCNghiBH70] [float] NULL,
	[BCNghiCongTac] [float] NULL,
	[BCNghiBu] [float] NULL,
	[BCNghiKhac] [float] NULL,
	[tctaNVMa] [int] NULL,
	[TCTienAn] [int] NULL,
	[nklMaNV] [int] NULL,
	[NgayKL] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTaoLenhMCC]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTaoLenhMCC](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PID] [int] NOT NULL,
	[IDGroup] [nvarchar](50) NOT NULL,
	[CommandType] [nvarchar](50) NOT NULL,
	[Remark] [nvarchar](500) NULL,
	[User1] [int] NULL,
	[Date1] [datetime] NULL,
	[User2] [int] NULL,
	[Date2] [datetime] NULL,
 CONSTRAINT [PK_tblTaoLenhMCC] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTestImportData]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTestImportData](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Adress] [nvarchar](max) NULL,
	[User1] [smallint] NULL,
	[Date1] [smalldatetime] NULL,
	[User2] [smallint] NULL,
	[Date2] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTinh]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTinh](
	[TMa] [smallint] IDENTITY(1,1) NOT NULL,
	[TTen] [nvarchar](30) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTrinhDo]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTrinhDo](
	[TDMa] [smallint] IDENTITY(1,1) NOT NULL,
	[TDTen] [nvarchar](30) NULL,
	[TDMoTa] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTrinhDoNV]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTrinhDoNV](
	[MaNhanVien] [int] NOT NULL,
	[HocVan] [smallint] NULL,
	[ChuyenMon] [smallint] NULL,
	[TrinhDo] [smallint] NULL,
	[LoaiBang] [smallint] NULL,
	[NgoaiNgu1] [smallint] NULL,
	[TrinhDoNN1] [smallint] NULL,
	[NgoaiNgu2] [smallint] NULL,
	[TrinhDoNN2] [smallint] NULL,
	[ViTinh] [smallint] NULL,
	[TrinhDoVT] [smallint] NULL,
	[HocVi] [smallint] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTruyCap]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTruyCap](
	[TCMa] [int] NOT NULL,
	[TCTen] [nvarchar](30) NULL,
	[TCMatKhau] [nvarchar](50) NULL,
	[TCQuyen] [tinyint] NULL,
	[TCNhom] [int] NULL,
	[TCDoimatkhau] [bit] NULL,
	[TCTaoBC] [bit] NULL,
	[TCDatquyen] [bit] NULL,
	[PID] [varchar](10) NULL,
	[TCKhoaDL] [bit] NULL,
	[TCMoKhoaDL] [bit] NULL,
	[TCTBDKNVanQuetThe] [bit] NULL,
	[DLocked] [bit] NULL,
	[User3] [smallint] NULL,
	[Date3] [smalldatetime] NULL,
	[TCSHOWHDLD] [bit] NULL,
	[TCKhongDuocDoiCa] [bit] NULL,
	[TCCAMEXPORTEXCEL] [bit] NULL,
 CONSTRAINT [PK_tblTruyCap] PRIMARY KEY CLUSTERED 
(
	[TCMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblUngVien]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUngVien](
	[UVMa] [int] IDENTITY(1,1) NOT NULL,
	[UVMaTD] [int] NULL,
	[UVMaUV] [nvarchar](255) NULL,
	[UVHoTen] [nvarchar](255) NOT NULL,
	[UVGioiTinh] [bit] NOT NULL,
	[UVNgaySinh] [datetime] NULL,
	[UVNoiSinh] [nvarchar](255) NULL,
	[UVDienThoai] [nvarchar](255) NULL,
	[UVDanToc] [nvarchar](255) NULL,
	[UVTonGiao] [nvarchar](255) NULL,
	[UVEmail] [nvarchar](255) NULL,
	[UVNguyenQuan] [nvarchar](255) NULL,
	[UVThuongTru] [nvarchar](255) NULL,
	[UVDiaChi] [nvarchar](255) NULL,
	[UVCMND] [nvarchar](20) NULL,
	[UVNgayCap] [datetime] NULL,
	[UVNoiCap] [nvarchar](255) NULL,
	[UVHonNhan] [nvarchar](255) NULL,
	[UVCoCon] [bit] NULL,
	[UVSucKhoe] [nvarchar](20) NULL,
	[UVChieuCao] [nvarchar](20) NULL,
	[UVCanNang] [nvarchar](20) NULL,
	[UVMaBP] [int] NULL,
	[UVMaCV] [int] NULL,
	[UVNguoiGT] [nvarchar](255) NULL,
	[UVLuong] [numeric](18, 0) NULL,
	[UVHocVan] [nvarchar](20) NULL,
	[UVNgoaiNgu] [nvarchar](20) NULL,
	[UVTinHoc] [nvarchar](20) NULL,
	[UVNguoiBT] [nvarchar](255) NULL,
	[UVDiaChiBT] [nvarchar](255) NULL,
	[UVDienThoaiBT] [nvarchar](255) NULL,
	[UVQuanHeBT] [nvarchar](255) NULL,
	[UVNgayNop] [datetime] NULL,
	[UVNgayPV] [datetime] NULL,
	[UVPhanLoai] [bit] NULL,
	[UVNgayHenPhongVan] [datetime] NULL,
	[UVAnh] [nvarchar](255) NULL,
	[UVDaChuyen] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[UVMa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblViTriThe]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblViTriThe](
	[VTTID] [int] IDENTITY(1,1) NOT NULL,
	[vttSysID] [int] NOT NULL,
	[vttCard] [nvarchar](10) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TD_Luong_ChiTiet]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TD_Luong_ChiTiet](
	[BiDanh] [varchar](50) NOT NULL,
	[STT] [float] NULL,
	[Truong] [nvarchar](50) NOT NULL,
	[KieuDuLieu] [nvarchar](20) NULL,
	[TenHienThi] [nvarchar](50) NULL,
	[DoRong] [real] NULL,
	[DieuKien] [nvarchar](50) NULL,
	[TuBang] [nvarchar](50) NULL,
	[TruongLuu] [nvarchar](50) NULL,
	[TruongHienThi] [nvarchar](50) NULL,
	[NgamDinh] [ntext] NULL,
	[DinhDangKieuSo] [nvarchar](50) NULL,
	[CanLe] [tinyint] NULL,
	[HienThi] [bit] NULL,
	[ChiDoc] [bit] NULL,
	[GhiChu] [nvarchar](250) NULL,
	[BatBuocNhap] [bit] NULL,
	[GiuLai] [bit] NULL,
	[TruongKhoa] [bit] NULL,
	[TuTang] [bit] NULL,
	[ToiDa] [int] NULL,
	[CamQuyenXem] [nvarchar](300) NULL,
	[SoThapPhan] [tinyint] NULL,
	[XauChuan] [bit] NULL,
	[GiaTriToiDa] [numeric](18, 0) NULL,
	[GiaTriToiThieu] [smallint] NULL,
	[CoDinh] [bit] NULL,
	[TenHienThiE] [nvarchar](400) NULL,
 CONSTRAINT [PK_TD_Luong_ChiTiet] PRIMARY KEY CLUSTERED 
(
	[BiDanh] ASC,
	[Truong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TD_ThuMoiThuViec]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TD_ThuMoiThuViec](
	[MaUV] [nvarchar](50) NOT NULL,
	[SoThuMoi] [nvarchar](50) NULL,
	[DonVi] [nvarchar](50) NULL,
	[BoPhan] [nvarchar](50) NULL,
	[ChucVu] [nvarchar](50) NULL,
	[NgayHieuLuc] [smalldatetime] NULL,
	[DenNgay] [smalldatetime] NULL,
	[MucLuong] [money] NULL,
	[GhiChu] [nvarchar](500) NULL,
	[MucLuongChinhThuc] [money] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[temp_TimCa]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temp_TimCa](
	[Ngay] [datetime] NULL,
	[MaNV] [int] NULL,
	[MaCa] [int] NULL,
	[GanCa] [int] NULL,
	[TGDen] [datetime] NULL,
	[TGVe] [datetime] NULL,
	[TGDen2] [datetime] NULL,
	[TGVe2] [datetime] NULL,
	[trongso] [int] NULL,
	[lan] [float] NULL,
	[chon] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[temp_TimCa2]    Script Date: 9/19/2026 9:36:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temp_TimCa2](
	[Ngay] [datetime] NULL,
	[MaNV] [int] NULL,
	[MaCa] [int] NULL,
	[ThoiGian] [datetime] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AppliedSpace] ADD  CONSTRAINT [DF_AppliedSpace_DBLMa]  DEFAULT (0) FOR [DBLMa]
GO
ALTER TABLE [dbo].[AppliedSpace] ADD  CONSTRAINT [DF_AppliedSpace_TSID]  DEFAULT (0) FOR [TSID]
GO
ALTER TABLE [dbo].[CC_CauhinhBCC] ADD  CONSTRAINT [DF_CC_CauhinhBCC_chHienthi]  DEFAULT ((1)) FOR [chHienthi]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_Tenbang]  DEFAULT ('') FOR [Tenbang]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_TenTruong]  DEFAULT ('') FOR [TenTruong]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_Dieukienloc]  DEFAULT ('') FOR [Dieukienloc]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_DienGiai]  DEFAULT ('') FOR [DienGiai]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_CongThuc]  DEFAULT ('') FOR [CongThuc]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_CauLenhSQL]  DEFAULT ('') FOR [CauLenhSQL]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_PCID]  DEFAULT ((0)) FOR [PCID]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_LoaiTinhLuong]  DEFAULT ('') FOR [LoaiTinhLuong]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_KieuDulieu]  DEFAULT ('') FOR [KieuDulieu]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_Dodai]  DEFAULT ((0)) FOR [Dodai]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_ThutuTinh]  DEFAULT ((0)) FOR [ThutuTinh]
GO
ALTER TABLE [dbo].[CongthucLuong] ADD  CONSTRAINT [DF_CongthucLuong_HienThi]  DEFAULT ((0)) FOR [HienThi]
GO
ALTER TABLE [dbo].[DM_CNganh] ADD  CONSTRAINT [DF_DM_CNganh_CNganh]  DEFAULT ('') FOR [CNganh]
GO
ALTER TABLE [dbo].[DM_CNganh] ADD  CONSTRAINT [DF_DM_CNganh_Ten]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[DM_HocHam] ADD  CONSTRAINT [DF_DM_HocHam_HocHam]  DEFAULT ('') FOR [HocHam]
GO
ALTER TABLE [dbo].[DM_HocHam] ADD  CONSTRAINT [DF_DM_HocHam_Ten]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[DM_HocVan] ADD  CONSTRAINT [DF__DM_HocVan__HocVa__62841DFC]  DEFAULT ('') FOR [HocVan]
GO
ALTER TABLE [dbo].[DM_HocVan] ADD  CONSTRAINT [DF__DM_HocVan__Ten__63784235]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[DM_HocVi] ADD  CONSTRAINT [DF_DM_HocVi_HocVi]  DEFAULT ('') FOR [HocVi]
GO
ALTER TABLE [dbo].[DM_HocVi] ADD  CONSTRAINT [DF_DM_HocVi_Ten]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[DM_LoaiBang] ADD  DEFAULT ('') FOR [LoaiBang]
GO
ALTER TABLE [dbo].[DM_LoaiBang] ADD  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[DM_LoaiQD] ADD  CONSTRAINT [DF_DM_LoaiQD_TangNS]  DEFAULT ((0)) FOR [TangNS]
GO
ALTER TABLE [dbo].[DM_LoaiQD] ADD  CONSTRAINT [DF_DM_LoaiQD_GiamNS]  DEFAULT ((0)) FOR [GiamNS]
GO
ALTER TABLE [dbo].[DM_LoaiQD] ADD  CONSTRAINT [DF_DM_LoaiQD_DoiLuong]  DEFAULT ((0)) FOR [ThayLuong]
GO
ALTER TABLE [dbo].[DM_LoaiQD] ADD  CONSTRAINT [DF__DM_LoaiQD__DatQu__6CD78F3D]  DEFAULT ('') FOR [DatQuyen]
GO
ALTER TABLE [dbo].[DM_LoaiQD] ADD  CONSTRAINT [DF__DM_LoaiQD__Delet__6DCBB376]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[DM_ThamSoLuong] ADD  CONSTRAINT [DF_DM_ThamSoLuong_TSLTCTienAn]  DEFAULT ((0)) FOR [TSLTCTienAn]
GO
ALTER TABLE [dbo].[DM_TPBT] ADD  CONSTRAINT [DF_DM_TPBT_TPBT]  DEFAULT ('') FOR [TPBT]
GO
ALTER TABLE [dbo].[DM_TPBT] ADD  CONSTRAINT [DF_DM_TPBT_Ten]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[DM_TPGD] ADD  CONSTRAINT [DF_DM_TPGD_TPGD]  DEFAULT ('') FOR [TPGD]
GO
ALTER TABLE [dbo].[DM_TPGD] ADD  CONSTRAINT [DF_DM_TPGD_Ten]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[DM_TruongDT] ADD  CONSTRAINT [DF__DM_Truong__Truon__59BE6011]  DEFAULT ('') FOR [TruongDT]
GO
ALTER TABLE [dbo].[DM_TruongDT] ADD  CONSTRAINT [DF__DM_DMTruong__Ten__6A73EAC7]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[F03_SyncQueue_Employee] ADD  DEFAULT (getdate()) FOR [QueuedAt]
GO
ALTER TABLE [dbo].[F03_SyncQueue_Employee] ADD  DEFAULT ((0)) FOR [Processed]
GO
ALTER TABLE [dbo].[HT_CauhinhSQL] ADD  CONSTRAINT [DF_HT_CauhinhSQL_Name]  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[HT_CauhinhSQL] ADD  CONSTRAINT [DF_HT_CauhinhSQL_Value]  DEFAULT ('') FOR [Value]
GO
ALTER TABLE [dbo].[HT_CauhinhSQL] ADD  CONSTRAINT [DF_HT_CauhinhSQL_Description]  DEFAULT ('') FOR [Description]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_STT]  DEFAULT ((0)) FOR [STT]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_BiDanh]  DEFAULT ('') FOR [BiDanh]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_TenBang]  DEFAULT ('') FOR [TenBang]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_TenHienThi]  DEFAULT ('') FOR [TenHienThi]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_TruongKhoa]  DEFAULT ('') FOR [TruongKhoa]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_TruongSapXep]  DEFAULT ('') FOR [TruongSapXep]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_DieuKien]  DEFAULT ('') FOR [DieuKien]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_CamQuyen]  DEFAULT ('') FOR [CamQuyen]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_CamHienThi]  DEFAULT ('') FOR [CamHienThi]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_TruongTimKiem]  DEFAULT ('') FOR [TruongTimKiem]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_HeThong]  DEFAULT ((0)) FOR [HeThong]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_TruongQuanHe]  DEFAULT ('') FOR [TruongQuanHe]
GO
ALTER TABLE [dbo].[HT_DanhMuc] ADD  CONSTRAINT [DF_HT_DanhMuc_EnterToFind]  DEFAULT ((0)) FOR [EnterToFind]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_DoRong_1]  DEFAULT ((100)) FOR [DoRong]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_CanLe_1]  DEFAULT ((0)) FOR [CanLe]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_HienThi_1]  DEFAULT ((0)) FOR [HienThi]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_ChiDoc_1]  DEFAULT ((0)) FOR [ChiDoc]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_BatBuocNhap_1]  DEFAULT ((0)) FOR [BatBuocNhap]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_GiuLai_1]  DEFAULT ((0)) FOR [GiuLai]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_TruongKhoa_1]  DEFAULT ((0)) FOR [TruongKhoa]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_TuTang_1]  DEFAULT ((0)) FOR [TuTang]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_ToiDa_1]  DEFAULT ((0)) FOR [ToiDa]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_SoThapPhan_1]  DEFAULT ((0)) FOR [SoThapPhan]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_XauChuan_1]  DEFAULT ((0)) FOR [XauChuan]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_GiaTriToiDa_1]  DEFAULT ((32767)) FOR [GiaTriToiDa]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_GiaTriToiThieu_1]  DEFAULT ((0)) FOR [GiaTriToiThieu]
GO
ALTER TABLE [dbo].[HT_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HT_DanhMuc_ChiTiet_CoDinh_1]  DEFAULT ((0)) FOR [CoDinh]
GO
ALTER TABLE [dbo].[HT_ExportExcel] ADD  CONSTRAINT [DF_HT_ExportExcel1_CamQuyen]  DEFAULT ('') FOR [CamQuyen]
GO
ALTER TABLE [dbo].[HT_ExportExcel] ADD  CONSTRAINT [DF_HT_ExportExcel1_CamHienThi]  DEFAULT ('') FOR [CamHienThi]
GO
ALTER TABLE [dbo].[HT_Shortcut] ADD  CONSTRAINT [DF_HT_Shortcut_CamQuyen]  DEFAULT ('') FOR [CamQuyen]
GO
ALTER TABLE [dbo].[HT_Shortcut] ADD  CONSTRAINT [DF_HT_Shortcut_CamHienThi]  DEFAULT ('') FOR [CamHienThi]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_STT]  DEFAULT ((0)) FOR [STT]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_BiDanh]  DEFAULT ('') FOR [BiDanh]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TenBang]  DEFAULT ('') FOR [TenBang]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TenHienThi]  DEFAULT ('') FOR [TenHienThi]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongKhoa]  DEFAULT ('') FOR [TruongKhoa]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongSapXep]  DEFAULT ('') FOR [TruongSapXep]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongDKBP]  DEFAULT ('') FOR [TruongDKBP]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongDKNV]  DEFAULT ('') FOR [TruongDKNV]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_DieuKien]  DEFAULT ('') FOR [DieuKien]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongSelect]  DEFAULT ('') FOR [SELECT_Them]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongLienket]  DEFAULT ('') FOR [FROM_Them]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongFROM2]  DEFAULT ('') FOR [HienThiV_Them]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongHienThiV1]  DEFAULT ('') FOR [HienThiE_Them]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_CamQuyen]  DEFAULT ('') FOR [CamQuyen]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_CamHienThi]  DEFAULT ('') FOR [CamHienThi]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongTimKiem]  DEFAULT ('') FOR [TruongTimKiem]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_HeThong]  DEFAULT ((0)) FOR [HeThong]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_TruongQuanHe]  DEFAULT ('') FOR [TruongQuanHe]
GO
ALTER TABLE [dbo].[HTN_DanhMuc] ADD  CONSTRAINT [DF_HTN_DanhMuc_EnterToFind]  DEFAULT ((0)) FOR [EnterToFind]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_DoRong_1]  DEFAULT ((100)) FOR [DoRong]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_DinhDang]  DEFAULT ('') FOR [DinhDang]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_CanLe_1]  DEFAULT ((0)) FOR [CanLe]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_HienThi_1]  DEFAULT ((0)) FOR [HienThi]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_HienThiTrenluoi]  DEFAULT ((1)) FOR [HienThiTrenluoi]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_ChiDoc_1]  DEFAULT ((0)) FOR [ChiDoc]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_BatBuocNhap_1]  DEFAULT ((0)) FOR [BatBuocNhap]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_GiuLai_1]  DEFAULT ((0)) FOR [GiuLai]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_TruongKhoa_1]  DEFAULT ((0)) FOR [TruongKhoa]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_TuTang_1]  DEFAULT ((0)) FOR [TuTang]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_ToiDa_1]  DEFAULT ((0)) FOR [ToiDa]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_SoThapPhan_1]  DEFAULT ((0)) FOR [SoThapPhan]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_XauChuan_1]  DEFAULT ((0)) FOR [XauChuan]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_GiaTriToiDa_1]  DEFAULT ((32767)) FOR [GiaTriToiDa]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_GiaTriToiThieu_1]  DEFAULT ((0)) FOR [GiaTriToiThieu]
GO
ALTER TABLE [dbo].[HTN_DanhMuc_ChiTiet] ADD  CONSTRAINT [DF_HTN_DanhMuc_ChiTiet_CoDinh_1]  DEFAULT ((0)) FOR [CoDinh]
GO
ALTER TABLE [dbo].[LBangChamCong] ADD  CONSTRAINT [DF_LBangChamCong_bcclLoai]  DEFAULT ((0)) FOR [bcclLoai]
GO
ALTER TABLE [dbo].[lLuong_DieuChinhLuong] ADD  CONSTRAINT [DF_lDieuChinhLuong_bcclLoai]  DEFAULT ((0)) FOR [dcclLoai]
GO
ALTER TABLE [dbo].[LLuong_ThamSoLuong] ADD  CONSTRAINT [DF_LLuong_ThamSoLuong_TSLTCTienAn]  DEFAULT ((0)) FOR [TSLTCTienAn]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_DoRong]  DEFAULT ((100)) FOR [DoRong]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_CanLe]  DEFAULT ((0)) FOR [CanLe]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_HienThi]  DEFAULT ((0)) FOR [HienThi]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_ChiDoc]  DEFAULT ((0)) FOR [ChiDoc]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_BatBuocNhap]  DEFAULT ((0)) FOR [BatBuocNhap]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_GiuLai]  DEFAULT ((0)) FOR [GiuLai]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_TruongKhoa]  DEFAULT ((0)) FOR [TruongKhoa]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_TuTang]  DEFAULT ((0)) FOR [TuTang]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_ToiDa]  DEFAULT ((0)) FOR [ToiDa]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_SoThapPhan]  DEFAULT ((0)) FOR [SoThapPhan]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_XauChuan]  DEFAULT ((0)) FOR [XauChuan]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_GiaTriToiDa]  DEFAULT ((32767)) FOR [GiaTriToiDa]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_GiaTriToiThieu]  DEFAULT ((0)) FOR [GiaTriToiThieu]
GO
ALTER TABLE [dbo].[NS_Danhmuc_ChiTiet] ADD  CONSTRAINT [DF_NS_Danhmuc_ChiTiet_CoDinh]  DEFAULT ((0)) FOR [CoDinh]
GO
ALTER TABLE [dbo].[NS_Khenthuong] ADD  CONSTRAINT [DF_Table_1_NhanSu_1]  DEFAULT ('') FOR [MaNV]
GO
ALTER TABLE [dbo].[NS_QTCT] ADD  CONSTRAINT [DF_Table_1_NhanSu]  DEFAULT ('') FOR [MaNV]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_NS_QuanheGD_QHGDMaNV]  DEFAULT ((0)) FOR [QHGDMaNV]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_NS_QuanheGD_QHGDTen]  DEFAULT ('') FOR [QHGDTen]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_NS_QuanheGD_QHGDQuanHe]  DEFAULT ('') FOR [QHGDQuanHe]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_NS_QuanheGD_QHGDNgaySinh]  DEFAULT (((1)/(1))/(2000)) FOR [QHGDNgaySinh]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_NS_QuanheGD_QHGDNoiO]  DEFAULT ('') FOR [QHGDNoiO]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_NS_QuanheGD_QHGDCongViec]  DEFAULT ('') FOR [QHGDCongViec]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_NS_QuanheGD_QHGDSTT]  DEFAULT ((0)) FOR [QHGDSTT]
GO
ALTER TABLE [dbo].[NS_QuanheGD] ADD  CONSTRAINT [DF_Table_1_LaRuotThit]  DEFAULT ((0)) FOR [QHGDTrugiacanh]
GO
ALTER TABLE [dbo].[NS_QuyetDinh] ADD  CONSTRAINT [DF__NS_QuyetD__SQLUn__12A69BBC]  DEFAULT ('') FOR [SQLUndo]
GO
ALTER TABLE [dbo].[NS_QuyetDinh] ADD  CONSTRAINT [DF__NS_QuyetD__BaoHi__139ABFF5]  DEFAULT ((0)) FOR [BaoHiem]
GO
ALTER TABLE [dbo].[NS_QuyetDinh] ADD  CONSTRAINT [DF__NS_QuyetD__BH_Ta__148EE42E]  DEFAULT ((1)) FOR [BH_Tang]
GO
ALTER TABLE [dbo].[NS_QuyetDinh] ADD  CONSTRAINT [DF__NS_QuyetD__ThayD__6193271F]  DEFAULT ((0)) FOR [ThayDoiLuong]
GO
ALTER TABLE [dbo].[NS_QuyetDinh] ADD  CONSTRAINT [DF__NS_QuyetD__ThayD__5FFFE468]  DEFAULT ((0)) FOR [ThayDoiLuongCongViec]
GO
ALTER TABLE [dbo].[RB_Condition] ADD  CONSTRAINT [DF_RB_Condition_BatBuoc]  DEFAULT ((0)) FOR [BatBuoc]
GO
ALTER TABLE [dbo].[RB_CustomizeColumn] ADD  DEFAULT ((0)) FOR [Frozen]
GO
ALTER TABLE [dbo].[RB_Main] ADD  CONSTRAINT [DF_RB_Main_OK]  DEFAULT ((1)) FOR [OK]
GO
ALTER TABLE [dbo].[RecordData] ADD  CONSTRAINT [DF_RecordData_Status_1]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[RecordData] ADD  CONSTRAINT [DF_RecordData_HandData_1]  DEFAULT ((0)) FOR [HandData]
GO
ALTER TABLE [dbo].[RecordData] ADD  CONSTRAINT [DF_RecordData_Pass_1]  DEFAULT ((0)) FOR [Pass]
GO
ALTER TABLE [dbo].[RecordDataBackUp] ADD  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[RecordDataBackUp] ADD  DEFAULT ((0)) FOR [HandData]
GO
ALTER TABLE [dbo].[RecordDataBackUp] ADD  DEFAULT ((1)) FOR [Pass]
GO
ALTER TABLE [dbo].[tblBacTho] ADD  CONSTRAINT [DF_tblBacTho_TenBacTho]  DEFAULT ('') FOR [TenBacTho]
GO
ALTER TABLE [dbo].[tblBangCapVT] ADD  CONSTRAINT [DF_tblBangCapVT_Ten]  DEFAULT ('') FOR [Ten]
GO
ALTER TABLE [dbo].[tblBangChamCong2010] ADD  CONSTRAINT [DF_tblBangChamCong2010_BCCLoai]  DEFAULT ((0)) FOR [BCCLoai]
GO
ALTER TABLE [dbo].[tblBangChamCong2011] ADD  CONSTRAINT [DF_tblBangChamCong2011_BCCLoai]  DEFAULT ((0)) FOR [BCCLoai]
GO
ALTER TABLE [dbo].[tblBaoCaoK] ADD  CONSTRAINT [DF_tblBaoCaoK_BCLoai]  DEFAULT ((0)) FOR [BCLoai]
GO
ALTER TABLE [dbo].[tblBaoCaoK] ADD  CONSTRAINT [DF_tblBaoCaoK_BCTGLTToiDa]  DEFAULT ((0)) FOR [BCTGLTToiDa]
GO
ALTER TABLE [dbo].[tblBaoCaoNhaAn] ADD  CONSTRAINT [DF_tblBaoCaoNhaAn_Ca]  DEFAULT ((0)) FOR [Ca]
GO
ALTER TABLE [dbo].[tblBaoCaoNhaAn] ADD  CONSTRAINT [DF_Table_1_Status]  DEFAULT ((1)) FOR [STT]
GO
ALTER TABLE [dbo].[tblBenhVien] ADD  CONSTRAINT [DF_tblBenhVien_TenBV]  DEFAULT ('') FOR [TenBV]
GO
ALTER TABLE [dbo].[tblBoPhan] ADD  CONSTRAINT [DF_tblBoPhan_BPMaCha]  DEFAULT ((0)) FOR [BPMaCha]
GO
ALTER TABLE [dbo].[tblBoPhan] ADD  CONSTRAINT [DF_tblBoPhan_BPUuTien]  DEFAULT ((0)) FOR [BPUuTien]
GO
ALTER TABLE [dbo].[tblBoPhan] ADD  CONSTRAINT [DF_tblBoPhan_BPHienThiBC]  DEFAULT ((1)) FOR [BPHienThiBC]
GO
ALTER TABLE [dbo].[tblBoPhanCon] ADD  CONSTRAINT [DF_tblBoPhanCon_BPConLoginID]  DEFAULT ((0)) FOR [BPConLoginID]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CBDTinhLT1]  DEFAULT ((0)) FOR [CBDTinhLTTC]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CBDTinhLT]  DEFAULT ((0)) FOR [CBDTinhLT]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CTinhVaoSom]  DEFAULT ((0)) FOR [CTinhVaoSom]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CBuTGLam]  DEFAULT ((0)) FOR [CBuTGLam]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CCongNghiGiuaCa]  DEFAULT ((0)) FOR [CCongNghiGiuaCa]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CNguongLamThem]  DEFAULT ((0)) FOR [CNguongLamThem]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CNguongLamThem1]  DEFAULT ((0)) FOR [CNguongLamThemTC]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CDonViLamThem]  DEFAULT ((1)) FOR [CDonViLamThem]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CNguongDiMuon]  DEFAULT ((0)) FOR [CNguongDiMuon]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CNguongVeSom]  DEFAULT ((0)) FOR [CNguongVeSom]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CQuetTruocCa]  DEFAULT ((240)) FOR [CQuetTruocCa]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CQuetSauCa]  DEFAULT ((240)) FOR [CQuetSauCa]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CDonViChamCong]  DEFAULT ((1)) FOR [CDonViChamCong]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CChunhatlambt]  DEFAULT ((0)) FOR [CChuNhatLambt]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CChuNhatLambt2]  DEFAULT ((0)) FOR [CNgayLeLambt]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CLoaiCa]  DEFAULT ((0)) FOR [CLoaiCa]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CDSBoPhan]  DEFAULT ('') FOR [CDSBoPhan]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CNgaynghi]  DEFAULT ((0)) FOR [CNgaynghi]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CKhongtinhVangMat]  DEFAULT ((1)) FOR [CKhongtinhVangMat]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CLichtrinhVaora]  DEFAULT ((0)) FOR [CLichtrinhVaora]
GO
ALTER TABLE [dbo].[tblCa] ADD  CONSTRAINT [DF_tblCa_CChiaLTSauca]  DEFAULT ((0)) FOR [CChiaLTSauca]
GO
ALTER TABLE [dbo].[tblChucVu] ADD  CONSTRAINT [DF_tblChucVu_CVTen]  DEFAULT ('') FOR [CVTen]
GO
ALTER TABLE [dbo].[tblChuyenBoPhan] ADD  CONSTRAINT [DF_tblChuyenBoPhan_MaNV]  DEFAULT (0) FOR [MaNV]
GO
ALTER TABLE [dbo].[tblChuyenBoPhan] ADD  CONSTRAINT [DF_tblChuyenBoPhan_MaBP]  DEFAULT (0) FOR [MaBP]
GO
ALTER TABLE [dbo].[tblChuyenBoPhan] ADD  CONSTRAINT [DF_tblChuyenBoPhan_MaBPDen]  DEFAULT (0) FOR [MaBPDen]
GO
ALTER TABLE [dbo].[tblChuyenBoPhan] ADD  CONSTRAINT [DF_tblChuyenBoPhan_NgayChuyen]  DEFAULT (1 / 1 / 2005) FOR [NgayChuyen]
GO
ALTER TABLE [dbo].[tblChuyenBoPhan] ADD  CONSTRAINT [DF_tblChuyenBoPhan_GhiChu]  DEFAULT ('') FOR [GhiChu]
GO
ALTER TABLE [dbo].[tblChuyenNganh] ADD  CONSTRAINT [DF_tblChuyenNganh_CNTen]  DEFAULT ('') FOR [CNTen]
GO
ALTER TABLE [dbo].[tblCongTac] ADD  CONSTRAINT [DF_tblCongTac_CTMa]  DEFAULT (0) FOR [CTMa]
GO
ALTER TABLE [dbo].[tblCongTac] ADD  CONSTRAINT [DF_tblCongTac_Thoigian]  DEFAULT ('') FOR [Thoigian]
GO
ALTER TABLE [dbo].[tblCongTac] ADD  CONSTRAINT [DF_tblCongTac_NoiCT]  DEFAULT ('') FOR [NoiCT]
GO
ALTER TABLE [dbo].[tblCongTac] ADD  CONSTRAINT [DF_tblCongTac_Mucluong]  DEFAULT (0) FOR [Mucluong]
GO
ALTER TABLE [dbo].[tblCongTac] ADD  CONSTRAINT [DF_tblCongTac_Chucvu_NN]  DEFAULT ('') FOR [Chucvu_NN]
GO
ALTER TABLE [dbo].[tblCongViec] ADD  CONSTRAINT [DF_tblCongViec_CVTen]  DEFAULT ('') FOR [CVTen]
GO
ALTER TABLE [dbo].[tblCTNuocNgoai] ADD  CONSTRAINT [DF_tblCTNuocNgoai_MaNV]  DEFAULT (0) FOR [MaNV]
GO
ALTER TABLE [dbo].[tblCTNuocNgoai] ADD  CONSTRAINT [DF_tblCTNuocNgoai_MaQG]  DEFAULT (0) FOR [MaQG]
GO
ALTER TABLE [dbo].[tblCTNuocNgoai] ADD  CONSTRAINT [DF_tblCTNuocNgoai_Tungay]  DEFAULT (1 / 1 / 2005) FOR [Tungay]
GO
ALTER TABLE [dbo].[tblCTNuocNgoai] ADD  CONSTRAINT [DF_tblCTNuocNgoai_CViec]  DEFAULT ('') FOR [CViec]
GO
ALTER TABLE [dbo].[tblCTNuocNgoai] ADD  CONSTRAINT [DF_tblCTNuocNgoai_Denngay]  DEFAULT (1 / 1 / 2005) FOR [Denngay]
GO
ALTER TABLE [dbo].[tblCTNuocNgoai] ADD  CONSTRAINT [DF_tblCTNuocNgoai_KinhPhi]  DEFAULT ('') FOR [KinhPhi]
GO
ALTER TABLE [dbo].[tblDangKyNghi] ADD  CONSTRAINT [DF_tblDangKyNghi2009_DKNNghiBu]  DEFAULT ((0)) FOR [DKNNghiBu]
GO
ALTER TABLE [dbo].[tblDangKyNghi] ADD  CONSTRAINT [DF_tblDangKyNghi_WebId]  DEFAULT ((0)) FOR [WebId]
GO
ALTER TABLE [dbo].[tblDauDoc] ADD  CONSTRAINT [DF_tblDauDoc_DDSoBanGhi]  DEFAULT ((0)) FOR [DDSoBanGhi]
GO
ALTER TABLE [dbo].[tblDauDoc] ADD  CONSTRAINT [DF_tblDauDoc_DDChinhVao]  DEFAULT ((1)) FOR [DDChinhVao]
GO
ALTER TABLE [dbo].[tblDauDoc] ADD  CONSTRAINT [DF_tblDauDoc_DDLoai]  DEFAULT ((0)) FOR [DDLoai]
GO
ALTER TABLE [dbo].[tblDauDoc] ADD  CONSTRAINT [DF_tblDauDoc_DDCongGiaoTiep]  DEFAULT ((1)) FOR [DDCongGiaoTiep]
GO
ALTER TABLE [dbo].[tblDauDoc] ADD  CONSTRAINT [DF_tblDauDoc_DDloaithe]  DEFAULT ((6)) FOR [DDloaithe]
GO
ALTER TABLE [dbo].[tblDauDoc] ADD  CONSTRAINT [DF_tblDauDoc_DDPort]  DEFAULT ((0)) FOR [DDPort]
GO
ALTER TABLE [dbo].[tblDauDoc] ADD  CONSTRAINT [DF_tblDauDoc_DDKieuKN]  DEFAULT ((0)) FOR [DDKieuKN]
GO
ALTER TABLE [dbo].[tblDauDocDangKyThe] ADD  CONSTRAINT [DF_tblDauDocDangKyThe_DDSoBanGhi]  DEFAULT ((0)) FOR [DDSoBanGhi]
GO
ALTER TABLE [dbo].[tblDauDocDangKyThe] ADD  CONSTRAINT [DF_tblDauDocDangKyThe_DDChinhVao]  DEFAULT ((1)) FOR [DDChinhVao]
GO
ALTER TABLE [dbo].[tblDauDocDangKyThe] ADD  CONSTRAINT [DF_tblDauDocDangKyThe_DDLoai]  DEFAULT ((0)) FOR [DDLoai]
GO
ALTER TABLE [dbo].[tblDauDocDangKyThe] ADD  CONSTRAINT [DF_tblDauDocDangKyThe_DDCongGiaoTiep]  DEFAULT ((1)) FOR [DDCongGiaoTiep]
GO
ALTER TABLE [dbo].[tblDauDocDangKyThe] ADD  CONSTRAINT [DF_tblDauDocDangKyThe_DDloaithe]  DEFAULT ((6)) FOR [DDloaithe]
GO
ALTER TABLE [dbo].[tblDauDocDangKyThe] ADD  CONSTRAINT [DF_tblDauDocDangKyThe_DDPort]  DEFAULT ((0)) FOR [DDPort]
GO
ALTER TABLE [dbo].[tblDauDocDangKyThe] ADD  CONSTRAINT [DF_tblDauDocDangKyThe_DDKieuKN]  DEFAULT ((0)) FOR [DDKieuKN]
GO
ALTER TABLE [dbo].[tblGDChinhSach] ADD  CONSTRAINT [DF_tblGDChinhSach_GDCSTen]  DEFAULT ('') FOR [GDCSTen]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_Cha_HTmaht]  DEFAULT ('') FOR [HTmaht]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_htTenV]  DEFAULT ('') FOR [HTTenV]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_htTenE]  DEFAULT ('') FOR [HTTenE]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_Cha_HTInvt]  DEFAULT ('') FOR [HTInV]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_Cha_HTInV1]  DEFAULT ('') FOR [HTInE]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_Cha_HTInchuthich]  DEFAULT ('') FOR [HTInchuthichV]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_Cha_HTInchuthich1]  DEFAULT ('') FOR [HTInchuthichE]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_htDodai]  DEFAULT ((1200)) FOR [HTDodai]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_htSQL]  DEFAULT ('') FOR [HTSQL]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_Cha_HTChiaND]  DEFAULT ((0)) FOR [HTChiaND]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_Cha_HTHienthi]  DEFAULT ((1)) FOR [HTHienthi]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Cha] ADD  CONSTRAINT [DF_tblHienthiBCC_htUutien]  DEFAULT ((0)) FOR [HTUutien]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTTenV]  DEFAULT ('') FOR [HTCTenV]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTTenE]  DEFAULT ('') FOR [HTCTenE]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTInvt]  DEFAULT ('') FOR [HTCInV]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTCInV1]  DEFAULT ('') FOR [HTCInE]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTInchuthich]  DEFAULT ('') FOR [HTCInchuthichV]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTCInchuthichV1]  DEFAULT ('') FOR [HTCInchuthichE]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTDodai]  DEFAULT ((1200)) FOR [HTCDodai]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTSQL]  DEFAULT ('') FOR [HTCSQL]
GO
ALTER TABLE [dbo].[tblHienthiBCC_Con] ADD  CONSTRAINT [DF_tblHienthiBCC_Con_HTUutien]  DEFAULT ((0)) FOR [HTCUutien]
GO
ALTER TABLE [dbo].[tblHocVi] ADD  CONSTRAINT [DF_tblHocVi_HVTen]  DEFAULT ('') FOR [HVTen]
GO
ALTER TABLE [dbo].[tblHopDongNV] ADD  CONSTRAINT [DF_tblHopDongNV_HDMaNV]  DEFAULT ((0)) FOR [HDMaNV]
GO
ALTER TABLE [dbo].[tblHopDongNV] ADD  CONSTRAINT [DF_tblHopDongNV_HDLoaiHD]  DEFAULT ((1)) FOR [HDLoaiHD]
GO
ALTER TABLE [dbo].[tblHopDongNV] ADD  CONSTRAINT [DF_tblHopDongNV_HDTuNgay]  DEFAULT ('2000-01-01') FOR [HDTuNgay]
GO
ALTER TABLE [dbo].[tblHopDongNV] ADD  CONSTRAINT [DF_tblHopDongNV_HDDenNgay]  DEFAULT ('9990-01-01') FOR [HDDenNgay]
GO
ALTER TABLE [dbo].[tblHopDongNV] ADD  CONSTRAINT [DF_tblHopDongNV_HDNgayKy]  DEFAULT ('2000-01-01') FOR [HDNgayKy]
GO
ALTER TABLE [dbo].[tblHopDongNV_Phuluc] ADD  CONSTRAINT [DF_Table_1_HDTuNgay]  DEFAULT ('2000-01-01') FOR [PLTuNgay]
GO
ALTER TABLE [dbo].[tblHopDongNV_Phuluc] ADD  CONSTRAINT [DF_Table_1_HDDenNgay]  DEFAULT ('9990-01-01') FOR [PLDenNgay]
GO
ALTER TABLE [dbo].[tblHopDongNV_Phuluc] ADD  CONSTRAINT [DF_Table_1_HDNgayKy]  DEFAULT ('2000-01-01') FOR [PLNgayKy]
GO
ALTER TABLE [dbo].[tblKTKL] ADD  CONSTRAINT [DF_tblKTKL_KTKLMa]  DEFAULT (0) FOR [KTKLMa]
GO
ALTER TABLE [dbo].[tblKTKL] ADD  CONSTRAINT [DF_tblKTKL_vbso]  DEFAULT ('') FOR [vbso]
GO
ALTER TABLE [dbo].[tblKTKL] ADD  CONSTRAINT [DF_tblKTKL_ktkl]  DEFAULT (0) FOR [ktkl]
GO
ALTER TABLE [dbo].[tblKTKL] ADD  CONSTRAINT [DF_tblKTKL_ngay]  DEFAULT (1 / 1 / 2000) FOR [ngay]
GO
ALTER TABLE [dbo].[tblKTKL] ADD  CONSTRAINT [DF_tblKTKL_noidung]  DEFAULT ('') FOR [noidung]
GO
ALTER TABLE [dbo].[tblKTKL] ADD  CONSTRAINT [DF_tblKTKL_hinhthuc]  DEFAULT ('') FOR [hinhthuc]
GO
ALTER TABLE [dbo].[tblKTKL] ADD  CONSTRAINT [DF_tblKTKL_ghichu]  DEFAULT ('') FOR [ghichu]
GO
ALTER TABLE [dbo].[tblLoaiNghi] ADD  CONSTRAINT [DF_tblLoaiNghi_LNViettat]  DEFAULT ('') FOR [LNViettat]
GO
ALTER TABLE [dbo].[tblLoaiNghi] ADD  CONSTRAINT [DF_tblLoaiNghi_LNLoai]  DEFAULT ((0)) FOR [LNLoai]
GO
ALTER TABLE [dbo].[tblLoaiUuDai] ADD  CONSTRAINT [DF_tblLoaiUuDai_LUDDauCa]  DEFAULT ((0)) FOR [LUDDauCa]
GO
ALTER TABLE [dbo].[tblLoaiUuDai] ADD  CONSTRAINT [DF_tblLoaiUuDai_LUDTruocNghi]  DEFAULT ((0)) FOR [LUDTruocNghi]
GO
ALTER TABLE [dbo].[tblLoaiUuDai] ADD  CONSTRAINT [DF_tblLoaiUuDai_LUDSauNghi]  DEFAULT ((0)) FOR [LUDSauNghi]
GO
ALTER TABLE [dbo].[tblLoaiUuDai] ADD  CONSTRAINT [DF_tblLoaiUuDai_LUDCuoiCa]  DEFAULT ((0)) FOR [LUDCuoiCa]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logUserName]  DEFAULT ('') FOR [logUserName]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logPassWord]  DEFAULT ('') FOR [logPassWord]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logDID]  DEFAULT ((0)) FOR [logDID]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logAdmin]  DEFAULT ((0)) FOR [logAdmin]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logFullName]  DEFAULT ('') FOR [logFullName]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_Table_1_SGender]  DEFAULT ((1)) FOR [logGender]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logEmail]  DEFAULT ('') FOR [logEmail]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logIP]  DEFAULT ('') FOR [logIP]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logYM]  DEFAULT ('') FOR [logYM]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logSkype]  DEFAULT ('') FOR [logSkype]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logMobile]  DEFAULT ('') FOR [logMobile]
GO
ALTER TABLE [dbo].[tblLogin] ADD  CONSTRAINT [DF_wLogin_logCompanyID]  DEFAULT ((0)) FOR [logCompanyID]
GO
ALTER TABLE [dbo].[tblNgayNghiLe] ADD  CONSTRAINT [DF_tblNgayNghiLe_NNLLoai]  DEFAULT ((0)) FOR [NNLLoai]
GO
ALTER TABLE [dbo].[tblNgoaiNgu] ADD  CONSTRAINT [DF_tblNgoaiNgu_NNTen]  DEFAULT ('') FOR [NNTen]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVHo]  DEFAULT ('') FOR [NVHo]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVHoDem]  DEFAULT ('') FOR [NVHoDem]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVHoTen]  DEFAULT ('') FOR [NVHoTen]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVBiDanh]  DEFAULT ('') FOR [NVBiDanh]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMaBP]  DEFAULT ((0)) FOR [NVMaBP]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMaCV]  DEFAULT ((0)) FOR [NVMaCV]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMaCa]  DEFAULT ((0)) FOR [NVMaCa]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVGioiTinh]  DEFAULT ((0)) FOR [NVGioiTinh]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNgaySinh]  DEFAULT ('1/1/1980') FOR [NVNgaySinh]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNgayVao1]  DEFAULT ('1/1/2000') FOR [NVNgayTV]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNgayVao1_1]  DEFAULT ('1/1/2000') FOR [NVNgayChinhThuc]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNgayVao]  DEFAULT ('1/1/2000') FOR [NVNgayVao]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNgayRa]  DEFAULT ('12/31/9990') FOR [NVNgayRa]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVUuTien]  DEFAULT ((0)) FOR [NVUuTien]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMaQuocTich]  DEFAULT ((0)) FOR [NVMaQuocTich]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMaTonGiao]  DEFAULT ((0)) FOR [NVMaTonGiao]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMaDanToc]  DEFAULT ((0)) FOR [NVMaDanToc]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNoiOHT]  DEFAULT ('') FOR [NVNoiOHT]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNoiSinh]  DEFAULT ('') FOR [NVNoiSinh]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNguyenQuan]  DEFAULT ('') FOR [NVNguyenQuan]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNoiThuongTru]  DEFAULT ('') FOR [NVNoiThuongTru]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVSoCMTND]  DEFAULT ('') FOR [NVSoCMTND]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNoiCapCMTND]  DEFAULT ('') FOR [NVNoiCapCMTND]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVDienThoai]  DEFAULT ('') FOR [NVDienThoai]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVSoFax]  DEFAULT ('') FOR [NVSoFax]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVEmail]  DEFAULT ('') FOR [NVEmail]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMayNhanTin]  DEFAULT ('') FOR [NVMayNhanTin]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVWebsite]  DEFAULT ('') FOR [NVWebsite]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNoiKNDoan]  DEFAULT ('') FOR [NVNoiKNDoan]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNoiKNDang]  DEFAULT ('') FOR [NVNoiKNDang]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVNguoiNN]  DEFAULT ((0)) FOR [NVNguoiNN]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVSoTaiKhoan]  DEFAULT ('') FOR [NVSoTaiKhoan]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVMaNganHang]  DEFAULT ((0)) FOR [NVMaNganHang]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_TTHinhThucTV]  DEFAULT ('') FOR [NVHinhThucTV]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVLyDoTV]  DEFAULT ('') FOR [NVLyDoTV]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVTinhLamThem]  DEFAULT ((1)) FOR [NVTinhLamThem]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_DangVien]  DEFAULT ((0)) FOR [NVDangVien]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_DoanVien]  DEFAULT ((0)) FOR [NVDoanVien]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  CONSTRAINT [DF_tblNhanVien_NVQS]  DEFAULT ((0)) FOR [NVNVQS]
GO
ALTER TABLE [dbo].[tblNhanVien] ADD  DEFAULT ((0)) FOR [NVMa]
GO
ALTER TABLE [dbo].[tblNhanVienCon] ADD  CONSTRAINT [DF_tblNhanVienCon_NVConMaBP]  DEFAULT ((0)) FOR [NVConMaBP]
GO
ALTER TABLE [dbo].[tblNhanVienCon] ADD  CONSTRAINT [DF_tblNhanVienCon_NVMaNV]  DEFAULT ((0)) FOR [NVMaNV]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NMaCa1]  DEFAULT ((0)) FOR [NMaCa1]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NSoNgay1]  DEFAULT ((0)) FOR [NSoNgay1]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NMaCa2]  DEFAULT ((0)) FOR [NMaCa2]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NSoNgay2]  DEFAULT ((0)) FOR [NSoNgay2]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NMaCa3]  DEFAULT ((0)) FOR [NMaCa3]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NSoNgay3]  DEFAULT ((0)) FOR [NSoNgay3]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NMaCa4]  DEFAULT ((0)) FOR [NMaCa4]
GO
ALTER TABLE [dbo].[tblNhom] ADD  CONSTRAINT [DF_tblNhom_NSoNgay4]  DEFAULT ((0)) FOR [NSoNgay4]
GO
ALTER TABLE [dbo].[tblNhomNhanVien] ADD  CONSTRAINT [DF_tblNhomNhanVien_NNVMaNV]  DEFAULT ((0)) FOR [NNVMaNV]
GO
ALTER TABLE [dbo].[tblNhomNhanVien] ADD  CONSTRAINT [DF_tblNhomNhanVien_NNVMaNhom]  DEFAULT ((0)) FOR [NNVMaNhom]
GO
ALTER TABLE [dbo].[tblNhomtruycap] ADD  CONSTRAINT [DF_tblNhomtruycap_NhomAdmin]  DEFAULT ((1)) FOR [NhomAdmin]
GO
ALTER TABLE [dbo].[tblNhomtruycap] ADD  CONSTRAINT [DF_tblNhomtruycap_OrderBy]  DEFAULT ((0)) FOR [OrderBy]
GO
ALTER TABLE [dbo].[tblTinh] ADD  CONSTRAINT [DF_tblTinh_TTen]  DEFAULT ('') FOR [TTen]
GO
ALTER TABLE [dbo].[tblTrinhDo] ADD  CONSTRAINT [DF_tblTrinhDo_TDTen]  DEFAULT ('') FOR [TDTen]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_CaHC]  DEFAULT (0) FOR [CaHC]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCa1T]  DEFAULT (0) FOR [Ca1T]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCa2T]  DEFAULT (0) FOR [Ca2T]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCa3T]  DEFAULT (0) FOR [Ca3T]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLLamThem]  DEFAULT (0) FOR [LamThem]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCa1CN]  DEFAULT (0) FOR [Ca1CN]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_Ca2CN]  DEFAULT (0) FOR [Ca2CN]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_Ca3CN]  DEFAULT (0) FOR [Ca3CN]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCa1NL]  DEFAULT (0) FOR [Ca1NL]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCa2NL]  DEFAULT (0) FOR [Ca2NL]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCa3NL]  DEFAULT (0) FOR [Ca3NL]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLNghiPhep]  DEFAULT (0) FOR [NghiPhep]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLNghi100]  DEFAULT (0) FOR [Nghi100]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLNghi70]  DEFAULT (0) FOR [Nghi70]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLNghiKL]  DEFAULT (0) FOR [NghiKL]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCTNT]  DEFAULT (0) FOR [CTNT]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCTCN]  DEFAULT (0) FOR [CTCN]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLCTNL]  DEFAULT (0) FOR [CTNL]
GO
ALTER TABLE [dbo].[WorkingTimeTable] ADD  CONSTRAINT [DF_WorkingTimeTable_BCCLDMVS]  DEFAULT (0) FOR [DMVS]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cố định cột' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HT_DanhMuc_ChiTiet', @level2type=N'COLUMN',@level2name=N'CoDinh'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thứ tự trường' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HT_NhapLieu_GiaTri', @level2type=N'COLUMN',@level2name=N'ValuesNo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên Trường' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HT_NhapLieu_GiaTri', @level2type=N'COLUMN',@level2name=N'ValuesName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thứ tự trường' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HT_NhapLieu_Truong', @level2type=N'COLUMN',@level2name=N'FieldsNo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên Trường' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HT_NhapLieu_Truong', @level2type=N'COLUMN',@level2name=N'FieldsName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thêm các cột không có trong bảng gốc' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HTN_DanhMuc', @level2type=N'COLUMN',@level2name=N'SELECT_Them'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thêm câu lệnh liên kết từ các bảng' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HTN_DanhMuc', @level2type=N'COLUMN',@level2name=N'FROM_Them'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Danh sách hiển thị V tiêu đề trên lưới của các cột thêm' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HTN_DanhMuc', @level2type=N'COLUMN',@level2name=N'HienThiV_Them'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Danh sách hiển thị V tiêu đề trên lưới của các cột thêm' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HTN_DanhMuc', @level2type=N'COLUMN',@level2name=N'HienThiE_Them'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cố định cột' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'HTN_DanhMuc_ChiTiet', @level2type=N'COLUMN',@level2name=N'CoDinh'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cố định cột' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'NS_Danhmuc_ChiTiet', @level2type=N'COLUMN',@level2name=N'CoDinh'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID User login' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblBoPhanCon', @level2type=N'COLUMN',@level2name=N'BPConLoginID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã BP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblBoPhanCon', @level2type=N'COLUMN',@level2name=N'BPConMaBP'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã BP Cha' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblBoPhanCon', @level2type=N'COLUMN',@level2name=N'BPConMaCha'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã NV' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblHopDongNV', @level2type=N'COLUMN',@level2name=N'HDMaNV'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loại HD 1: Đào tạo 2: Thử việc 3:Có thời hạn 4: Không thời hạn' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblHopDongNV', @level2type=N'COLUMN',@level2name=N'HDLoaiHD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã tự sinh' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên đăng nhập' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logUserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mật khẩu' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logPassWord'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã bộ phận được phép truy cập' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logDID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Quyền Admin' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logAdmin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Họ tên' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logFullName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giới  tính (Mặc định nam)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logGender'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Địa chỉ Email' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logEmail'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Địa chỉ IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logIP'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Địa chỉ Yahoo' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logYM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Điện thoại' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logMobile'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã công ty lấy từ bảng tblBophan' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblLogin', @level2type=N'COLUMN',@level2name=N'logCompanyID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 : Admin 1 : User' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblNhomtruycap', @level2type=N'COLUMN',@level2name=N'NhomAdmin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "nv"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 303
               Right = 294
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cv"
            Begin Extent = 
               Top = 7
               Left = 342
               Bottom = 170
               Right = 536
            End
            DisplayFlags = 280
            TopColumn = 1
         End
         Begin Table = "bp"
            Begin Extent = 
               Top = 7
               Left = 584
               Bottom = 170
               Right = 778
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 5256
         Alias = 900
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vB20Employee_HrmEdit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vB20Employee_HrmEdit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "RecordData"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 190
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'view_ChangeShift'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'view_ChangeShift'
GO
