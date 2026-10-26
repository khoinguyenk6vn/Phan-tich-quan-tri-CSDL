using FashionFlow.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionFlow.DAL;

public sealed class FashionFlowDbContext(DbContextOptions<FashionFlowDbContext> options) : DbContext(options)
{
    public DbSet<KhachHangDatMay> KhachHangDatMays => Set<KhachHangDatMay>();
    public DbSet<SanPhamMay> SanPhamMays => Set<SanPhamMay>();
    public DbSet<DonHangSanXuat> DonHangSanXuats => Set<DonHangSanXuat>();
    public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();
    public DbSet<NguyenPhuLieuVai> NguyenPhuLieuVais => Set<NguyenPhuLieuVai>();
    public DbSet<DinhMucNPL> DinhMucNPLs => Set<DinhMucNPL>();
    public DbSet<CapPhatNPL> CapPhatNPLs => Set<CapPhatNPL>();
    public DbSet<CongDoanSanXuat> CongDoanSanXuats => Set<CongDoanSanXuat>();
    public DbSet<NhanCongMay> NhanCongMays => Set<NhanCongMay>();
    public DbSet<PhanCong> PhanCongs => Set<PhanCong>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KhachHangDatMay>(e =>
        {
            e.ToTable("KhachHangDatMay"); e.HasKey(x => x.MaKH);
            e.Property(x => x.MaKH).HasColumnType("char(6)");
            e.Property(x => x.TenKH).HasMaxLength(100).IsRequired();
            e.Property(x => x.SDT).HasColumnType("varchar(15)").IsRequired();
            e.HasIndex(x => x.SDT).IsUnique();
            e.Property(x => x.Email).HasColumnType("varchar(100)").HasMaxLength(100);
            e.Property(x => x.DiaChi).HasMaxLength(200);
        });
        modelBuilder.Entity<SanPhamMay>(e =>
        {
            e.ToTable("SanPhamMay", t => t.HasCheckConstraint("CK_SanPhamMay_DonGiaBan", "[DonGiaBan] >= 0"));
            e.HasKey(x => x.MaSP); e.Property(x => x.MaSP).HasColumnType("char(6)");
            e.Property(x => x.TenSP).HasMaxLength(100).IsRequired(); e.Property(x => x.MoTa).HasMaxLength(300);
            e.Property(x => x.DonGiaBan).HasPrecision(18, 0);
        });
        modelBuilder.Entity<DonHangSanXuat>(e =>
        {
            e.ToTable("DonHangSanXuat", t =>
            {
                t.HasCheckConstraint("CK_DonHangSanXuat_TrangThai", "[TrangThai] IN (N'Chờ xử lý', N'Đang sản xuất', N'Hoàn thành', N'Đã hủy')");
                t.HasTrigger("TR_DonHang_KiemTraNgayGiao");
                t.UseSqlOutputClause(false);
            });
            e.HasKey(x => x.MaDon); e.Property(x => x.MaDon).HasColumnType("char(8)");
            e.Property(x => x.MaKH).HasColumnType("char(6)").IsRequired(); e.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("Chờ xử lý");
            e.Property(x => x.GhiChu).HasMaxLength(300); e.Property(x => x.NgayDat).HasDefaultValueSql("CONVERT(date, GETDATE())");
            e.HasOne(x => x.KhachHang).WithMany(x => x.DonHangs).HasForeignKey(x => x.MaKH).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ChiTietDonHang>(e =>
        {
            e.ToTable("ChiTietDonHang", t => { t.HasCheckConstraint("CK_ChiTietDonHang_SoLuong", "[SoLuong] > 0"); t.HasCheckConstraint("CK_ChiTietDonHang_DonGia", "[DonGia] >= 0"); });
            e.HasKey(x => new { x.MaDon, x.MaSP }); e.Property(x => x.MaDon).HasColumnType("char(8)"); e.Property(x => x.MaSP).HasColumnType("char(6)"); e.Property(x => x.DonGia).HasPrecision(18, 0);
            e.HasOne(x => x.DonHang).WithMany(x => x.ChiTietDonHangs).HasForeignKey(x => x.MaDon).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.SanPham).WithMany(x => x.ChiTietDonHangs).HasForeignKey(x => x.MaSP).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<NguyenPhuLieuVai>(e =>
        {
            e.ToTable("NguyenPhuLieuVai", t => { t.HasCheckConstraint("CK_NguyenPhuLieuVai_DonGia", "[DonGia] >= 0"); t.HasCheckConstraint("CK_NguyenPhuLieuVai_SoLuongTon", "[SoLuongTon] >= 0"); });
            e.HasKey(x => x.MaNPL); e.Property(x => x.MaNPL).HasColumnType("char(6)"); e.Property(x => x.TenNPL).HasMaxLength(100).IsRequired();
            e.Property(x => x.LoaiNPL).HasMaxLength(50).IsRequired(); e.Property(x => x.DonViTinh).HasMaxLength(20).IsRequired(); e.Property(x => x.DonGia).HasPrecision(18, 0); e.Property(x => x.SoLuongTon).HasPrecision(18, 2).HasDefaultValue(0m);
        });
        modelBuilder.Entity<DinhMucNPL>(e =>
        {
            e.ToTable("DinhMucNPL", t => t.HasCheckConstraint("CK_DinhMucNPL_SoLuong", "[SoLuongDinhMuc] > 0"));
            e.HasKey(x => new { x.MaSP, x.MaNPL }); e.Property(x => x.MaSP).HasColumnType("char(6)"); e.Property(x => x.MaNPL).HasColumnType("char(6)"); e.Property(x => x.SoLuongDinhMuc).HasPrecision(10, 3);
            e.HasOne(x => x.SanPham).WithMany(x => x.DinhMucNPLs).HasForeignKey(x => x.MaSP).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.NguyenPhuLieu).WithMany(x => x.DinhMucNPLs).HasForeignKey(x => x.MaNPL).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CapPhatNPL>(e =>
        {
            e.ToTable("CapPhatNPL", t =>
            {
                t.HasCheckConstraint("CK_CapPhatNPL_SoLuong", "[SoLuong] > 0");
                t.HasTrigger("TR_CapPhatNPL_CapNhatTonKho");
                t.UseSqlOutputClause(false);
            });
            e.HasKey(x => x.MaCapPhat); e.Property(x => x.MaCapPhat).UseIdentityColumn(); e.Property(x => x.MaDon).HasColumnType("char(8)"); e.Property(x => x.MaNPL).HasColumnType("char(6)"); e.Property(x => x.SoLuong).HasPrecision(18, 2);
            e.Property(x => x.NgayCap).HasDefaultValueSql("CONVERT(date, GETDATE())");
            e.HasOne(x => x.DonHang).WithMany().HasForeignKey(x => x.MaDon).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.NguyenPhuLieu).WithMany(x => x.CapPhatNPLs).HasForeignKey(x => x.MaNPL).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CongDoanSanXuat>(e =>
        {
            e.ToTable("CongDoanSanXuat", t => t.HasCheckConstraint("CK_CongDoanSanXuat_ThuTu", "[ThuTu] > 0"));
            e.HasKey(x => x.MaCD); e.Property(x => x.MaCD).HasColumnType("char(5)"); e.Property(x => x.TenCD).HasMaxLength(100).IsRequired(); e.Property(x => x.DonGiaCong).HasPrecision(18, 0);
        });
        modelBuilder.Entity<NhanCongMay>(e =>
        {
            e.ToTable("NhanCongMay"); e.HasKey(x => x.MaNC); e.Property(x => x.MaNC).HasColumnType("char(6)"); e.Property(x => x.HoTen).HasMaxLength(100).IsRequired(); e.Property(x => x.SDT).HasColumnType("varchar(15)"); e.Property(x => x.TayNghe).HasMaxLength(50);
            e.Property(x => x.NgayVaoLam).HasDefaultValueSql("CONVERT(date, GETDATE())");
        });
        modelBuilder.Entity<PhanCong>(e =>
        {
            e.ToTable("PhanCong", t =>
            {
                t.HasCheckConstraint("CK_PhanCong_SoLuong", "[SoLuongHoanThanh] > 0");
                t.HasTrigger("TR_PhanCong_KiemTraNgayLam");
                t.UseSqlOutputClause(false);
            });
            e.HasKey(x => x.MaPC); e.Property(x => x.MaPC).UseIdentityColumn(); e.Property(x => x.MaDon).HasColumnType("char(8)"); e.Property(x => x.MaSP).HasColumnType("char(6)"); e.Property(x => x.MaCD).HasColumnType("char(5)"); e.Property(x => x.MaNC).HasColumnType("char(6)");
            e.HasOne(x => x.ChiTietDonHang).WithMany(x => x.PhanCongs).HasForeignKey(x => new { x.MaDon, x.MaSP }).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CongDoan).WithMany(x => x.PhanCongs).HasForeignKey(x => x.MaCD).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NhanCong).WithMany(x => x.PhanCongs).HasForeignKey(x => x.MaNC).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
