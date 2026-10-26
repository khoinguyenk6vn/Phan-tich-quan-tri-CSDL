/* Chay file nay sau khi dang nhap admin lan dau de EF Core ap dung migrations va tao 10 bang. */
USE FashionFlowDb;
GO

CREATE OR ALTER VIEW dbo.V_DanhSachChuaXuLy
AS
    SELECT d.MaDon, d.NgayDat, d.NgayGiaoDuKien, d.TrangThai,
           k.MaKH, k.TenKH, k.SDT,
           COUNT(ct.MaSP) AS SoMatHang,
           COALESCE(SUM(ct.SoLuong), 0) AS TongSoLuong
    FROM dbo.DonHangSanXuat AS d
    INNER JOIN dbo.KhachHangDatMay AS k ON k.MaKH = d.MaKH
    LEFT JOIN dbo.ChiTietDonHang AS ct ON ct.MaDon = d.MaDon
    WHERE d.TrangThai IN (N'Chờ xử lý', N'Đang sản xuất')
    GROUP BY d.MaDon, d.NgayDat, d.NgayGiaoDuKien, d.TrangThai, k.MaKH, k.TenKH, k.SDT;
GO

CREATE OR ALTER VIEW dbo.V_ThongKeTongHop
AS
    SELECT k.MaKH, k.TenKH,
           COUNT(DISTINCT d.MaDon) AS SoDonHang,
           COALESCE(SUM(ct.SoLuong), 0) AS TongSanPham,
           COALESCE(SUM(CONVERT(decimal(18,0), ct.SoLuong) * ct.DonGia), 0) AS TongGiaTriDonHang,
           SUM(CASE WHEN d.TrangThai = N'Hoàn thành' THEN 1 ELSE 0 END) AS SoDonHoanThanh
    FROM dbo.KhachHangDatMay AS k
    LEFT JOIN dbo.DonHangSanXuat AS d ON d.MaKH = k.MaKH
    LEFT JOIN dbo.ChiTietDonHang AS ct ON ct.MaDon = d.MaDon
    GROUP BY k.MaKH, k.TenKH;
GO

CREATE OR ALTER FUNCTION dbo.F_TinhTongGiaTri(@MaDon char(8))
RETURNS decimal(18,0)
AS
BEGIN
    DECLARE @Tong decimal(18,0);
    SELECT @Tong = COALESCE(SUM(CONVERT(decimal(18,0), SoLuong) * DonGia), 0)
    FROM dbo.ChiTietDonHang WHERE MaDon = @MaDon;
    RETURN @Tong;
END;
GO

CREATE OR ALTER PROCEDURE dbo.P_DanhSachGiaoDich
    @TrangThai nvarchar(20) = NULL,
    @MaKH char(6) = NULL,
    @NgayBatDau date = NULL,
    @NgayKetThuc date = NULL,
    @TongDon int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TongDon = COUNT(*)
    FROM dbo.DonHangSanXuat AS d
    WHERE (@TrangThai IS NULL OR d.TrangThai = @TrangThai)
      AND (@MaKH IS NULL OR d.MaKH = @MaKH)
      AND (@NgayBatDau IS NULL OR d.NgayDat >= @NgayBatDau)
      AND (@NgayKetThuc IS NULL OR d.NgayDat <= @NgayKetThuc);

    SELECT d.MaDon, k.TenKH, d.NgayDat, d.NgayGiaoDuKien, d.TrangThai,
           dbo.F_TinhTongGiaTri(d.MaDon) AS TongGiaTri
    FROM dbo.DonHangSanXuat AS d
    INNER JOIN dbo.KhachHangDatMay AS k ON k.MaKH = d.MaKH
    WHERE (@TrangThai IS NULL OR d.TrangThai = @TrangThai)
      AND (@MaKH IS NULL OR d.MaKH = @MaKH)
      AND (@NgayBatDau IS NULL OR d.NgayDat >= @NgayBatDau)
      AND (@NgayKetThuc IS NULL OR d.NgayDat <= @NgayKetThuc)
    ORDER BY d.NgayDat DESC, d.MaDon;
END;
GO

CREATE OR ALTER PROCEDURE dbo.P_ThemSuaXoaKhachHang
    @HanhDong char(1),
    @MaKH char(6),
    @TenKH nvarchar(100) = NULL,
    @SDT varchar(15) = NULL,
    @Email varchar(100) = NULL,
    @DiaChi nvarchar(200) = NULL,
    @ThongBao nvarchar(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HanhDong = 'I'
            INSERT dbo.KhachHangDatMay(MaKH, TenKH, SDT, Email, DiaChi)
            VALUES (@MaKH, @TenKH, @SDT, @Email, @DiaChi);
        ELSE IF @HanhDong = 'U'
            UPDATE dbo.KhachHangDatMay
            SET TenKH = @TenKH, SDT = @SDT, Email = @Email, DiaChi = @DiaChi
            WHERE MaKH = @MaKH;
        ELSE IF @HanhDong = 'D'
            DELETE dbo.KhachHangDatMay WHERE MaKH = @MaKH;
        ELSE
            THROW 51000, N'Hành động chỉ nhận I, U hoặc D.', 1;

        IF @@ROWCOUNT = 0 AND @HanhDong <> 'I'
            THROW 51001, N'Không tìm thấy khách hàng cần cập nhật hoặc xóa.', 1;
        SET @ThongBao = N'Thực hiện thành công.';
    END TRY
    BEGIN CATCH
        SET @ThongBao = ERROR_MESSAGE();
        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_DonHang_KiemTraNgayGiao
ON dbo.DonHangSanXuat
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted WHERE NgayGiaoDuKien < NgayDat)
        THROW 51010, N'Ngày giao dự kiến phải từ ngày đặt trở đi.', 1;
END;
GO








CREATE OR ALTER TRIGGER dbo.TR_CapPhatNPL_CapNhatTonKho
ON dbo.CapPhatNPL
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @tongThayDoi TABLE (MaNPL char(6) PRIMARY KEY, SoLuongThayDoi decimal(18,2) NOT NULL);
    INSERT @tongThayDoi(MaNPL, SoLuongThayDoi)
    SELECT MaNPL, SUM(SoLuongThayDoi)
    FROM
    (
        SELECT MaNPL, SUM(SoLuong) AS SoLuongThayDoi FROM inserted GROUP BY MaNPL
        UNION ALL
        SELECT MaNPL, -SUM(SoLuong) AS SoLuongThayDoi FROM deleted GROUP BY MaNPL
    ) AS thayDoi
    GROUP BY MaNPL;

    IF EXISTS
    (
        SELECT 1 FROM @tongThayDoi AS td
        INNER JOIN dbo.NguyenPhuLieuVai AS n ON n.MaNPL = td.MaNPL
        WHERE n.SoLuongTon - td.SoLuongThayDoi < 0
    )
        THROW 51011, N'Số lượng cấp phát vượt quá tồn kho nguyên phụ liệu.', 1;

    UPDATE n SET SoLuongTon = n.SoLuongTon - td.SoLuongThayDoi
    FROM dbo.NguyenPhuLieuVai AS n
    INNER JOIN @tongThayDoi AS td ON td.MaNPL = n.MaNPL;
END;







GO

CREATE OR ALTER TRIGGER dbo.TR_PhanCong_KiemTraNgayLam
ON dbo.PhanCong
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted WHERE NgayLam > CONVERT(date, GETDATE()))
        THROW 51012, N'Ngày phân công không được ở tương lai.', 1;
END;
GO

GRANT SELECT ON dbo.V_DanhSachChuaXuLy TO FashionFlowUser;
GRANT SELECT ON dbo.V_ThongKeTongHop TO FashionFlowUser;
GRANT EXECUTE ON dbo.F_TinhTongGiaTri TO FashionFlowUser;
GRANT EXECUTE ON dbo.P_DanhSachGiaoDich TO FashionFlowUser;
GRANT EXECUTE ON dbo.P_ThemSuaXoaKhachHang TO FashionFlowUser;
GO
