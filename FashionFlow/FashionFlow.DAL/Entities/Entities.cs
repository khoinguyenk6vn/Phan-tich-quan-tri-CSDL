using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionFlow.DAL.Entities;

public class KhachHangDatMay
{
    [Key, StringLength(6), Required, DisplayName("Mã khách hàng")]
    public string MaKH { get; set; } = "";
    [Required, StringLength(100), DisplayName("Tên khách hàng")]
    public string TenKH { get; set; } = "";
    [Required, StringLength(15), DisplayName("Số điện thoại")]
    public string SDT { get; set; } = "";
    [StringLength(100), DisplayName("Email")]
    public string? Email { get; set; }
    [StringLength(200), DisplayName("Địa chỉ")]
    public string? DiaChi { get; set; }
    [Browsable(false)] public ICollection<DonHangSanXuat> DonHangs { get; set; } = new List<DonHangSanXuat>();
}

public class SanPhamMay
{
    [Key, StringLength(6), Required, DisplayName("Mã sản phẩm")]
    public string MaSP { get; set; } = "";
    [Required, StringLength(100), DisplayName("Tên sản phẩm")]
    public string TenSP { get; set; } = "";
    [StringLength(300), DisplayName("Mô tả")]
    public string? MoTa { get; set; }
    [Range(0, double.MaxValue), DisplayName("Đơn giá bán")]
    public decimal DonGiaBan { get; set; }
    [Browsable(false)] public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    [Browsable(false)] public ICollection<DinhMucNPL> DinhMucNPLs { get; set; } = new List<DinhMucNPL>();
}

public class DonHangSanXuat
{
    [Key, StringLength(8), Required, DisplayName("Mã đơn")]
    public string MaDon { get; set; } = "";
    [Required, StringLength(6), DisplayName("Mã khách hàng")]
    public string MaKH { get; set; } = "";
    [DisplayName("Ngày đặt")]
    public DateOnly NgayDat { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [DisplayName("Ngày giao dự kiến")]
    public DateOnly NgayGiaoDuKien { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
    [Required, StringLength(20), DisplayName("Trạng thái")]
    public string TrangThai { get; set; } = "Chờ xử lý";
    [StringLength(300), DisplayName("Ghi chú")]
    public string? GhiChu { get; set; }
    [Browsable(false)] public KhachHangDatMay? KhachHang { get; set; }
    [Browsable(false)] public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
}

public class ChiTietDonHang
{
    [StringLength(8)] public string MaDon { get; set; } = "";
    [StringLength(6)] public string MaSP { get; set; } = "";
    [Range(1, int.MaxValue), DisplayName("Số lượng")]
    public int SoLuong { get; set; } = 1;
    [Range(0, double.MaxValue), DisplayName("Đơn giá")]
    public decimal DonGia { get; set; }
    [Browsable(false)] public DonHangSanXuat? DonHang { get; set; }
    [Browsable(false)] public SanPhamMay? SanPham { get; set; }
    [Browsable(false)] public ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
}

public class NguyenPhuLieuVai
{
    [Key, StringLength(6), Required, DisplayName("Mã nguyên phụ liệu")]
    public string MaNPL { get; set; } = "";
    [Required, StringLength(100), DisplayName("Tên nguyên phụ liệu")]
    public string TenNPL { get; set; } = "";
    [Required, StringLength(50), DisplayName("Loại")]
    public string LoaiNPL { get; set; } = "";
    [Required, StringLength(20), DisplayName("Đơn vị tính")]
    public string DonViTinh { get; set; } = "";
    [Range(0, double.MaxValue), DisplayName("Đơn giá")]
    public decimal DonGia { get; set; }
    [Range(0, double.MaxValue), DisplayName("Số lượng tồn")]
    public decimal SoLuongTon { get; set; }
    [Browsable(false)] public ICollection<DinhMucNPL> DinhMucNPLs { get; set; } = new List<DinhMucNPL>();
    [Browsable(false)] public ICollection<CapPhatNPL> CapPhatNPLs { get; set; } = new List<CapPhatNPL>();
}

public class DinhMucNPL
{
    public string MaSP { get; set; } = "";
    public string MaNPL { get; set; } = "";
    [Range(0.001, double.MaxValue)] public decimal SoLuongDinhMuc { get; set; }
    [Browsable(false)] public SanPhamMay? SanPham { get; set; }
    [Browsable(false)] public NguyenPhuLieuVai? NguyenPhuLieu { get; set; }
}

public class CapPhatNPL
{
    [Key, ReadOnly(true), DatabaseGenerated(DatabaseGeneratedOption.Identity), DisplayName("Mã cấp phát")]
    public int MaCapPhat { get; set; }
    [Required, StringLength(8), DisplayName("Mã đơn")]
    public string MaDon { get; set; } = "";
    [Required, StringLength(6), DisplayName("Mã nguyên phụ liệu")]
    public string MaNPL { get; set; } = "";
    [Range(0.01, double.MaxValue), DisplayName("Số lượng")]
    public decimal SoLuong { get; set; }
    [DisplayName("Ngày cấp")]
    public DateOnly NgayCap { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Browsable(false)] public DonHangSanXuat? DonHang { get; set; }
    [Browsable(false)] public NguyenPhuLieuVai? NguyenPhuLieu { get; set; }
}

public class CongDoanSanXuat
{
    [Key, StringLength(5), Required, DisplayName("Mã công đoạn")]
    public string MaCD { get; set; } = "";
    [Required, StringLength(100), DisplayName("Tên công đoạn")]
    public string TenCD { get; set; } = "";
    [Range(1, int.MaxValue), DisplayName("Thứ tự")]
    public int ThuTu { get; set; } = 1;
    [Range(0, double.MaxValue), DisplayName("Đơn giá công")]
    public decimal DonGiaCong { get; set; }
    [Browsable(false)] public ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
}

public class NhanCongMay
{
    [Key, StringLength(6), Required, DisplayName("Mã nhân công")]
    public string MaNC { get; set; } = "";
    [Required, StringLength(100), DisplayName("Họ tên")]
    public string HoTen { get; set; } = "";
    [StringLength(15), DisplayName("Số điện thoại")]
    public string? SDT { get; set; }
    [StringLength(50), DisplayName("Tay nghề")]
    public string? TayNghe { get; set; }
    [DisplayName("Ngày vào làm")]
    public DateOnly NgayVaoLam { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Browsable(false)] public ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
}

public class PhanCong : IValidatableObject
{
    [Key, ReadOnly(true), DatabaseGenerated(DatabaseGeneratedOption.Identity), DisplayName("Mã phân công")]
    public int MaPC { get; set; }
    [Required, StringLength(8), DisplayName("Mã đơn")]
    public string MaDon { get; set; } = "";
    [Required, StringLength(6), DisplayName("Mã sản phẩm")]
    public string MaSP { get; set; } = "";
    [Required, StringLength(5), DisplayName("Mã công đoạn")]
    public string MaCD { get; set; } = "";
    [Required, StringLength(6), DisplayName("Mã nhân công")]
    public string MaNC { get; set; } = "";
    [DisplayName("Ngày làm")]
    public DateOnly NgayLam { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Range(1, int.MaxValue), DisplayName("Số lượng hoàn thành")]
    public int SoLuongHoanThanh { get; set; } = 1;
    [Browsable(false)] public ChiTietDonHang? ChiTietDonHang { get; set; }
    [Browsable(false)] public CongDoanSanXuat? CongDoan { get; set; }
    [Browsable(false)] public NhanCongMay? NhanCong { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgayLam > DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("Ngày làm không được ở tương lai.", [nameof(NgayLam)]);
    }
}
