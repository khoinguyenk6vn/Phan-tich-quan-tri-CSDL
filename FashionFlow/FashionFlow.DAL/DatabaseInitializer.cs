using FashionFlow.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionFlow.DAL;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(FashionFlowDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
        if (await db.SanPhamMays.AnyAsync(cancellationToken)) return;

        db.KhachHangDatMays.Add(new KhachHangDatMay
        {
            MaKH = "KH0001", TenKH = "Khách hàng mẫu", SDT = "0900000001", Email = "khachhang@example.com", DiaChi = "TP. Hồ Chí Minh"
        });
        db.SanPhamMays.AddRange(
            new SanPhamMay { MaSP = "SP0001", TenSP = "Áo sơ mi", MoTa = "Áo sơ mi may theo đơn", DonGiaBan = 250000 },
            new SanPhamMay { MaSP = "SP0002", TenSP = "Quần tây", MoTa = "Quần tây may theo đơn", DonGiaBan = 350000 });
        db.NguyenPhuLieuVais.Add(new NguyenPhuLieuVai
        {
            MaNPL = "NPL001", TenNPL = "Vải kaki", LoaiNPL = "Vải", DonViTinh = "Mét", DonGia = 80000, SoLuongTon = 100
        });
        db.CongDoanSanXuats.AddRange(
            new CongDoanSanXuat { MaCD = "CD001", TenCD = "Cắt", ThuTu = 1, DonGiaCong = 10000 },
            new CongDoanSanXuat { MaCD = "CD002", TenCD = "May thân", ThuTu = 2, DonGiaCong = 30000 });
        db.NhanCongMays.Add(new NhanCongMay { MaNC = "NC0001", HoTen = "Nhân công mẫu", TayNghe = "Thợ chính" });
        await db.SaveChangesAsync(cancellationToken);
    }
}
