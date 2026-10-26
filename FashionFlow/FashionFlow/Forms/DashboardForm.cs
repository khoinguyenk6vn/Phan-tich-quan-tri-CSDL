using FashionFlow.BLL;
using FashionFlow.DAL;
using FashionFlow.DAL.Entities;

namespace FashionFlow.Presentation.Forms;

public sealed class DashboardForm : Form
{
    private readonly LoginSession _session;
    private readonly FashionFlowContextFactory _factory;

    public DashboardForm(LoginSession session, FashionFlowContextFactory factory)
    {
        _session = session;
        _factory = factory;
        FormHelpers.Configure(this, "FashionFlow - Quản lý xưởng may");

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(16) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var account = new Label { Text = $"Tài khoản: {session.UserName}    Quyền: {(session.IsAdmin ? "Admin" : "User")}", AutoSize = true, Padding = new Padding(0, 0, 0, 12) };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, Padding = new Padding(4) };
        AddButton(buttons, "Khách hàng", () => OpenCatalog<KhachHangDatMay>("Khách hàng đặt may", "MaKH"));
        AddButton(buttons, "Sản phẩm", () => OpenCatalog<SanPhamMay>("Sản phẩm may", "MaSP"));
        AddButton(buttons, "Nguyên phụ liệu", () => OpenCatalog<NguyenPhuLieuVai>("Nguyên phụ liệu vải", "MaNPL"));
        AddButton(buttons, "Công đoạn", () => OpenCatalog<CongDoanSanXuat>("Công đoạn sản xuất", "MaCD"));
        AddButton(buttons, "Nhân công", () => OpenCatalog<NhanCongMay>("Nhân công may", "MaNC"));
        AddButton(buttons, "Định mức nguyên liệu", () => OpenCatalog<DinhMucNPL>("Định mức nguyên phụ liệu", "MaSP,MaNPL"));
        AddButton(buttons, "Cấp phát nguyên liệu", () => OpenCatalog<CapPhatNPL>("Cấp phát nguyên phụ liệu", "MaCapPhat"));
        AddButton(buttons, "Phân công sản xuất", () => OpenCatalog<PhanCong>("Phân công sản xuất", "MaPC"));
        AddButton(buttons, "Đơn hàng sản xuất", OpenOrders);
        var note = new Label { Text = "Chọn chức năng để quản lý dữ liệu. Các thao tác lưu và liên kết dữ liệu được xử lý qua EF Core.", AutoSize = true, Padding = new Padding(0, 10, 0, 0) };
        root.Controls.Add(account, 0, 0);
        root.Controls.Add(buttons, 0, 1);
        root.Controls.Add(note, 0, 2);
        Controls.Add(root);
    }

    private static void AddButton(FlowLayoutPanel panel, string text, Action click)
    {
        var button = new Button { Text = text, Width = 210, Height = 42, Margin = new Padding(8), AutoSize = false };
        button.Click += (_, _) => click();
        panel.Controls.Add(button);
    }

    private void OpenCatalog<TEntity>(string title, string keyName) where TEntity : class, new()
    {
        using var form = new EntityCatalogForm<TEntity>(_session, _factory, title, keyName);
        form.ShowDialog(this);
    }

    private void OpenOrders()
    {
        using var form = new OrderManagementForm(_session, _factory);
        form.ShowDialog(this);
    }
}
