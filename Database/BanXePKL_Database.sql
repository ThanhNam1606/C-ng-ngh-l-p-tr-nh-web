/* ============================================================
   PROJECT: WEB BÁN XE PHÂN KHỐI LỚN (PKL) - MÔ HÌNH MVC
   PHẦN: DATABASE (Người 1 phụ trách)
   DBMS : Microsoft SQL Server
   Ghi chú:
   - Nếu nhóm dùng ASP.NET Core Identity cho đăng nhập/phân quyền
     (Người 4 phụ trách), có thể bỏ 2 bảng VaiTro/NguoiDung bên dưới
     và thay UserId bằng khóa ngoại tới AspNetUsers.Id (nvarchar(450)).
   - Script này để nhóm chạy độc lập, không phụ thuộc EF Identity.
   ============================================================ */

IF DB_ID('BanXePKL') IS NOT NULL
BEGIN
    ALTER DATABASE BanXePKL SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BanXePKL;
END
GO

CREATE DATABASE BanXePKL;
GO

USE BanXePKL;
GO

/* ============================================================
   1. VAI TRÒ & NGƯỜI DÙNG
   ============================================================ */

CREATE TABLE VaiTro (
    VaiTroId    INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro   NVARCHAR(50) NOT NULL UNIQUE      -- Admin, KhachHang
);
GO

CREATE TABLE NguoiDung (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    HoTen           NVARCHAR(100)  NOT NULL,
    Email           NVARCHAR(150)  NOT NULL UNIQUE,
    MatKhauHash     NVARCHAR(255)  NOT NULL,
    SoDienThoai     NVARCHAR(15)   NULL,
    DiaChi          NVARCHAR(250)  NULL,
    VaiTroId        INT NOT NULL DEFAULT 2,        -- mặc định = KhachHang
    NgayTao         DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai       BIT NOT NULL DEFAULT 1,         -- 1: hoạt động, 0: khóa
    CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY (VaiTroId) REFERENCES VaiTro(VaiTroId)
);
GO

/* ============================================================
   2. HÃNG XE & DANH MỤC
   ============================================================ */

CREATE TABLE HangXe (
    HangXeId    INT IDENTITY(1,1) PRIMARY KEY,
    TenHang     NVARCHAR(100) NOT NULL UNIQUE,     -- Honda, Yamaha, Ducati...
    QuocGia     NVARCHAR(100) NULL,
    LogoUrl     NVARCHAR(250) NULL,
    MoTa        NVARCHAR(500) NULL
);
GO

CREATE TABLE DanhMuc (
    DanhMucId   INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc  NVARCHAR(100) NOT NULL UNIQUE      -- Sport, Naked, Touring, Adventure...
);
GO

/* ============================================================
   3. XE & HÌNH ẢNH
   ============================================================ */

CREATE TABLE Xe (
    XeId            INT IDENTITY(1,1) PRIMARY KEY,
    TenXe           NVARCHAR(150) NOT NULL,
    HangXeId        INT NOT NULL,
    DanhMucId       INT NOT NULL,
    PhanKhoi        INT NOT NULL,                  -- dung tích xy-lanh (cc)
    GiaBan          DECIMAL(18,2) NOT NULL,
    SoLuongTon      INT NOT NULL DEFAULT 0,
    NamSanXuat      INT NULL,
    MauSac          NVARCHAR(100) NULL,
    ThongSoKyThuat  NVARCHAR(MAX) NULL,             -- JSON hoặc text mô tả thông số
    MoTa            NVARCHAR(MAX) NULL,
    AnhDaiDien      NVARCHAR(250) NULL,
    NgayThem        DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai       BIT NOT NULL DEFAULT 1,          -- 1: đang bán, 0: ngừng bán
    CONSTRAINT FK_Xe_HangXe FOREIGN KEY (HangXeId) REFERENCES HangXe(HangXeId),
    CONSTRAINT FK_Xe_DanhMuc FOREIGN KEY (DanhMucId) REFERENCES DanhMuc(DanhMucId),
    CONSTRAINT CK_Xe_GiaBan CHECK (GiaBan >= 0),
    CONSTRAINT CK_Xe_SoLuongTon CHECK (SoLuongTon >= 0)
);
GO

CREATE TABLE AnhXe (
    AnhId       INT IDENTITY(1,1) PRIMARY KEY,
    XeId        INT NOT NULL,
    DuongDanAnh NVARCHAR(250) NOT NULL,
    LaAnhChinh  BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_AnhXe_Xe FOREIGN KEY (XeId) REFERENCES Xe(XeId) ON DELETE CASCADE
);
GO

/* ============================================================
   4. GIỎ HÀNG
   ============================================================ */

CREATE TABLE GioHang (
    GioHangId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL,
    NgayTao     DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_GioHang_NguoiDung FOREIGN KEY (UserId) REFERENCES NguoiDung(UserId)
);
GO

CREATE TABLE GioHangChiTiet (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    GioHangId   INT NOT NULL,
    XeId        INT NOT NULL,
    SoLuong     INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_GHCT_GioHang FOREIGN KEY (GioHangId) REFERENCES GioHang(GioHangId) ON DELETE CASCADE,
    CONSTRAINT FK_GHCT_Xe FOREIGN KEY (XeId) REFERENCES Xe(XeId),
    CONSTRAINT CK_GHCT_SoLuong CHECK (SoLuong > 0)
);
GO

/* ============================================================
   5. ĐƠN HÀNG
   ============================================================ */

CREATE TABLE DonHang (
    DonHangId           INT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT NOT NULL,
    NgayDat             DATETIME NOT NULL DEFAULT GETDATE(),
    TongTien            DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiaChiGiao          NVARCHAR(250) NOT NULL,
    SoDienThoaiNhan     NVARCHAR(15)  NOT NULL,
    PhuongThucThanhToan NVARCHAR(50)  NOT NULL DEFAULT N'COD',   -- COD, VNPay, Momo
    TrangThaiDonHang    NVARCHAR(50)  NOT NULL DEFAULT N'ChoXacNhan',
        -- ChoXacNhan -> DaXacNhan -> DangGiao -> DaGiao -> DaHuy
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_DonHang_NguoiDung FOREIGN KEY (UserId) REFERENCES NguoiDung(UserId)
);
GO

CREATE TABLE ChiTietDonHang (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    DonHangId   INT NOT NULL,
    XeId        INT NOT NULL,
    SoLuong     INT NOT NULL,
    DonGia      DECIMAL(18,2) NOT NULL,             -- giá tại thời điểm mua
    ThanhTien   AS (SoLuong * DonGia) PERSISTED,     -- cột tính toán
    CONSTRAINT FK_CTDH_DonHang FOREIGN KEY (DonHangId) REFERENCES DonHang(DonHangId) ON DELETE CASCADE,
    CONSTRAINT FK_CTDH_Xe FOREIGN KEY (XeId) REFERENCES Xe(XeId),
    CONSTRAINT CK_CTDH_SoLuong CHECK (SoLuong > 0)
);
GO

/* ============================================================
   6. ĐÁNH GIÁ XE
   ============================================================ */

CREATE TABLE DanhGia (
    DanhGiaId   INT IDENTITY(1,1) PRIMARY KEY,
    XeId        INT NOT NULL,
    UserId      INT NOT NULL,
    SoSao       INT NOT NULL,
    NoiDung     NVARCHAR(1000) NULL,
    NgayDanhGia DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DanhGia_Xe FOREIGN KEY (XeId) REFERENCES Xe(XeId) ON DELETE CASCADE,
    CONSTRAINT FK_DanhGia_NguoiDung FOREIGN KEY (UserId) REFERENCES NguoiDung(UserId),
    CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5)
);
GO

/* ============================================================
   7. KHUYẾN MÃI (optional)
   ============================================================ */

CREATE TABLE KhuyenMai (
    KhuyenMaiId INT IDENTITY(1,1) PRIMARY KEY,
    MaCode      NVARCHAR(50) NOT NULL UNIQUE,
    PhanTramGiam INT NOT NULL,
    NgayBatDau  DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL,
    TrangThai   BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_KhuyenMai_PhanTram CHECK (PhanTramGiam BETWEEN 1 AND 100)
);
GO

/* ============================================================
   8. INDEX GỢI Ý (tăng tốc truy vấn thường dùng)
   ============================================================ */

CREATE INDEX IX_Xe_HangXeId       ON Xe(HangXeId);
CREATE INDEX IX_Xe_DanhMucId      ON Xe(DanhMucId);
CREATE INDEX IX_Xe_GiaBan         ON Xe(GiaBan);
CREATE INDEX IX_DonHang_UserId    ON DonHang(UserId);
CREATE INDEX IX_ChiTietDH_DonHangId ON ChiTietDonHang(DonHangId);
GO

/* ============================================================
   9. DỮ LIỆU MẪU (SEED DATA)
   ============================================================ */

INSERT INTO VaiTro (TenVaiTro) VALUES (N'Admin'), (N'KhachHang');
GO

INSERT INTO NguoiDung (HoTen, Email, MatKhauHash, SoDienThoai, VaiTroId)
VALUES
(N'Quản Trị Viên', 'admin@BanXePKL_Project.com', 'HASHED_PASSWORD_HERE', '0900000000', 1),
(N'Nguyễn Văn A',   'nguyenvana@gmail.com', 'HASHED_PASSWORD_HERE', '0911111111', 2);
GO

INSERT INTO HangXe (TenHang, QuocGia) VALUES
(N'Honda', N'Nhật Bản'),
(N'Yamaha', N'Nhật Bản'),
(N'Kawasaki', N'Nhật Bản'),
(N'Ducati', N'Ý'),
(N'Harley-Davidson', N'Mỹ');
GO

INSERT INTO DanhMuc (TenDanhMuc) VALUES
(N'Sport'), (N'Naked'), (N'Touring'), (N'Adventure'), (N'Cruiser');
GO

INSERT INTO Xe (TenXe, HangXeId, DanhMucId, PhanKhoi, GiaBan, SoLuongTon, NamSanXuat, MauSac, MoTa)
VALUES
(N'Honda CBR1000RR-R', 1, 1, 1000, 599000000, 5, 2025, N'Đỏ/Đen', N'Sportbike cao cấp của Honda'),
(N'Yamaha MT-09', 2, 2, 890, 350000000, 8, 2025, N'Xanh Đen', N'Naked bike mạnh mẽ'),
(N'Kawasaki Ninja ZX-10R', 3, 1, 998, 620000000, 3, 2025, N'Xanh Lá', N'Superbike đường đua'),
(N'Ducati Monster', 4, 2, 937, 450000000, 4, 2024, N'Đỏ', N'Naked bike phong cách Ý'),
(N'Harley-Davidson Fat Boy', 5, 5, 1868, 780000000, 2, 2024, N'Đen Nhám', N'Cruiser đậm chất Mỹ');
GO

PRINT N'Tạo database BanXePKL thành công!';
