using System.ComponentModel;
using FashionFlow.BLL;
using FashionFlow.DAL;
using FashionFlow.DAL.Entities;

namespace FashionFlow.Presentation.Forms;

public sealed class OrderManagementForm : Form
{
    private readonly OrderService _service;
    private readonly bool _isAdmin;
    private readonly TextBox _orderId = new();
    private readonly ComboBox _orderPicker = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _customer = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _orderDate = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _deliveryDate = new() { Format = DateTimePickerFormat.Short };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _note = new();
    private readonly ComboBox _product = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
    private readonly TextBox _quantity = new() { Text = "1", Width = 80 };
    private readonly TextBox _price = new() { Text = "0", Width = 120 };
    private readonly Button _addDetail = new() { Text = "Thêm dòng", AutoSize = true };
    private readonly Button _removeDetail = new() { Text = "Xóa dòng", AutoSize = true };
    private readonly Label _total = new() { Text = "Tổng tiền: 0", AutoSize = true, Padding = new Padding(8) };
    private readonly BindingSource _details = new();
    private readonly DataGridView _detailGrid = CreateGrid();
    private readonly List<OrderDetailDraft> _drafts = new();
    private string? _selectedOrderId;
    private bool _loading;
    private bool _editing;
    private bool _creating;

    public OrderManagementForm(LoginSession session, FashionFlowContextFactory factory)
    {
        _isAdmin = session.IsAdmin;
        _service = new OrderService(new OrderRepository(factory, session.ConnectionString));
        FormHelpers.Configure(this, "Quản lý đơn hàng sản xuất");
        _orderId.MaxLength = 8;
        _note.MaxLength = 300;
        FormHelpers.BindEnumlessCombo(_status, ["Chờ xử lý", "Đang sản xuất", "Hoàn thành", "Đã hủy"]);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = "Quản lý đơn hàng sản xuất", AutoSize = true, Padding = new Padding(0, 0, 0, 8) }, 0, 0);

        var orderBox = new GroupBox { Text = "Thông tin đơn hàng", Dock = DockStyle.Fill, Padding = new Padding(10), AutoSize = true };
        var orderLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, AutoSize = true };
        for (var i = 0; i < 3; i++) orderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        for (var i = 0; i < 3; i++) orderLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        orderLayout.Controls.Add(Labeled("Mã đơn", _orderId), 0, 0);
        orderLayout.Controls.Add(Labeled("Chọn đơn có sẵn", _orderPicker), 1, 0);
        orderLayout.Controls.Add(Labeled("Khách hàng", _customer), 2, 0);
        orderLayout.Controls.Add(Labeled("Ngày đặt", _orderDate), 0, 1);
        orderLayout.Controls.Add(Labeled("Ngày giao", _deliveryDate), 1, 1);
        orderLayout.Controls.Add(Labeled("Trạng thái", _status), 2, 1);
        orderLayout.Controls.Add(Labeled("Ghi chú", _note), 0, 2);
        orderLayout.SetColumnSpan(orderLayout.GetControlFromPosition(0, 2)!, 3);
        orderBox.Controls.Add(orderLayout);
        root.Controls.Add(orderBox, 0, 1);

        var detailBox = new GroupBox { Text = "Chi tiết sản phẩm", Dock = DockStyle.Fill, Padding = new Padding(10) };
        var detailLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        detailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var detailToolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        detailToolbar.Controls.AddRange([LabeledCompact("Sản phẩm", _product), LabeledCompact("Số lượng", _quantity), LabeledCompact("Đơn giá", _price), _addDetail, _removeDetail]);
        _detailGrid.DataSource = _details;
        detailLayout.Controls.Add(detailToolbar, 0, 0);
        detailLayout.Controls.Add(_detailGrid, 0, 1);
        detailLayout.Controls.Add(_total, 0, 2);
        detailBox.Controls.Add(detailLayout);
        root.Controls.Add(detailBox, 0, 2);

        var actions = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 6, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 5; i++) actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        actions.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var close = new Button { Text = "Đóng", AutoSize = true };
        var delete = new Button { Text = "Xóa đơn", AutoSize = true, Visible = session.IsAdmin };
        var edit = new Button { Text = "Sửa", AutoSize = true };
        var save = new Button { Text = "Lưu", AutoSize = true };
        var create = new Button { Text = "Tạo mới", AutoSize = true };
        close.Click += (_, _) => Close();
        delete.Click += Delete_Click;
        edit.Click += (_, _) => StartEdit();
        save.Click += Save_Click;
        create.Click += (_, _) => StartNew();
        actions.Controls.Add(create, 1, 0);
        actions.Controls.Add(save, 2, 0);
        actions.Controls.Add(edit, 3, 0);
        actions.Controls.Add(delete, 4, 0);
        actions.Controls.Add(close, 5, 0);
        root.Controls.Add(actions, 0, 3);
        Controls.Add(root);
        _orderPicker.SelectionChangeCommitted += OrderPicker_SelectionChangeCommitted;
        _addDetail.Click += (_, _) => AddDetail();
        _removeDetail.Click += (_, _) => RemoveDetail();
        SetEditing(false, false);
        Shown += async (_, _) => await InitializeAsync();
    }

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window
    };

    private static Control Labeled(string labelText, Control input)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, AutoSize = true, Margin = new Padding(4) };
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        input.Dock = DockStyle.Fill;
        panel.Controls.Add(input, 0, 1);
        return panel;
    }

    private static Control LabeledCompact(string labelText, Control input)
    {
        var panel = new TableLayoutPanel { ColumnCount = 1, RowCount = 2, AutoSize = true, Margin = new Padding(4) };
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true }, 0, 0);
        panel.Controls.Add(input, 0, 1);
        return panel;
    }

    private async Task InitializeAsync()
    {
        try
        {
            _customer.DataSource = await _service.GetCustomersAsync();
            _product.DataSource = await _service.GetProductsAsync();
            await LoadAsync();
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
    }

    private async Task LoadAsync(string? selectOrderId = null)
    {
        _loading = true;
        try
        {
            var orders = await _service.GetOrdersAsync();
            _orderPicker.DataSource = orders.Select(order => new LookupItem(order.MaDon.Trim(), order.TenKH)).ToList();
            var selected = orders.FirstOrDefault(item => item.MaDon.Trim() == selectOrderId)
                           ?? orders.FirstOrDefault();
            if (selected is not null)
            {
                _orderPicker.SelectedItem = _orderPicker.Items.Cast<LookupItem>().FirstOrDefault(item => item.Code == selected.MaDon.Trim());
                await DisplayOrderAsync(selected);
            }
            else StartNew();
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
        finally { _loading = false; }
    }

    private async void OrderPicker_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (_loading || _orderPicker.SelectedItem is not LookupItem selected) return;
        _loading = true;
        try
        {
            var item = (await _service.GetOrdersAsync()).FirstOrDefault(order => order.MaDon.Trim() == selected.Code);
            if (item is not null) await DisplayOrderAsync(item);
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
        finally { _loading = false; }
    }

    private async Task DisplayOrderAsync(OrderListItem item)
    {
        var order = await _service.GetOrderAsync(item.MaDon.Trim());
        if (order is null) return;
        _selectedOrderId = order.MaDon.Trim();
        _orderId.Text = order.MaDon.Trim();
        _orderPicker.SelectedItem = _orderPicker.Items.Cast<LookupItem>().FirstOrDefault(x => x.Code == _selectedOrderId);
        _customer.SelectedItem = _customer.Items.Cast<LookupItem>().FirstOrDefault(x => x.Code.Trim() == order.MaKH.Trim());
        _orderDate.Value = order.NgayDat.ToDateTime(TimeOnly.MinValue);
        _deliveryDate.Value = order.NgayGiaoDuKien.ToDateTime(TimeOnly.MinValue);
        _status.SelectedItem = order.TrangThai.Trim();
        _note.Text = order.GhiChu ?? "";
        _drafts.Clear();
        foreach (var detail in await _service.GetDetailsAsync(order.MaDon.Trim()))
            _drafts.Add(new OrderDetailDraft(detail.MaSP.Trim(), detail.TenSP, detail.SoLuong, detail.DonGia));
        RefreshDetails();
        SetEditing(false, false);
    }

    private void StartNew()
    {
        _selectedOrderId = null;
        _orderId.Clear();
        _orderDate.Value = DateTime.Today;
        _deliveryDate.Value = DateTime.Today.AddDays(7);
        _status.SelectedItem = "Chờ xử lý";
        _note.Clear();
        if (_customer.Items.Count > 0) _customer.SelectedIndex = 0;
        _drafts.Clear();
        RefreshDetails();
        _orderPicker.SelectedIndex = -1;
        SetEditing(true, true);
    }

    private void StartEdit()
    {
        if (string.IsNullOrWhiteSpace(_selectedOrderId)) return;
        SetEditing(true, false);
    }

    private void SetEditing(bool editing, bool creating)
    {
        _editing = editing;
        _creating = creating;
        _orderId.ReadOnly = !creating;
        _orderId.Enabled = editing;
        _customer.Enabled = editing;
        _orderDate.Enabled = editing;
        _deliveryDate.Enabled = editing;
        _status.Enabled = editing;
        _note.ReadOnly = !editing;
        _orderPicker.Enabled = !editing;
        _product.Enabled = editing;
        _quantity.Enabled = editing;
        _price.Enabled = editing;
        _addDetail.Enabled = editing;
        _removeDetail.Enabled = editing;
    }

    private void AddDetail()
    {
        if (!_editing) return;
        if (_product.SelectedItem is not LookupItem product)
        {
            MessageBox.Show(this, "Vui lòng tạo sản phẩm trước.", "FashionFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!int.TryParse(_quantity.Text, out var quantity) || quantity <= 0)
        {
            MessageBox.Show(this, "Số lượng phải là số nguyên lớn hơn 0.", "FashionFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!decimal.TryParse(_price.Text, out var price) || price < 0)
        {
            MessageBox.Show(this, "Đơn giá phải là số không âm.", "FashionFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_drafts.Any(x => x.MaSP == product.Code.Trim()))
        {
            MessageBox.Show(this, "Sản phẩm đã có trong đơn hàng.", "FashionFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _drafts.Add(new OrderDetailDraft(product.Code.Trim(), product.Name, quantity, price));
        RefreshDetails();
    }

    private void RemoveDetail()
    {
        if (!_editing || _detailGrid.CurrentRow?.DataBoundItem is not OrderDetailDraft row) return;
        _drafts.Remove(row);
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        _details.DataSource = new BindingList<OrderDetailDraft>(_drafts.ToList());
        _total.Text = $"Tổng tiền: {_drafts.Sum(x => x.SoLuong * x.DonGia):N0}";
    }

    private async void Save_Click(object? sender, EventArgs e)
    {
        if (!_editing) return;
        try
        {
            var adding = _creating;
            var order = new DonHangSanXuat
            {
                MaDon = _orderId.Text.Trim(),
                MaKH = (_customer.SelectedItem as LookupItem)?.Code.Trim() ?? "",
                NgayDat = DateOnly.FromDateTime(_orderDate.Value),
                NgayGiaoDuKien = DateOnly.FromDateTime(_deliveryDate.Value),
                TrangThai = _status.SelectedItem?.ToString() ?? "Chờ xử lý",
                GhiChu = string.IsNullOrWhiteSpace(_note.Text) ? null : _note.Text.Trim()
            };
            var details = _drafts.Select(x => new ChiTietDonHang { MaDon = order.MaDon, MaSP = x.MaSP, SoLuong = x.SoLuong, DonGia = x.DonGia }).ToList();
            await _service.SaveAsync(order, details);
            var createdId = order.MaDon;
            await LoadAsync(createdId);
            MessageBox.Show(this, adding ? "Đã thêm đơn hàng và các chi tiết." : "Đã cập nhật đơn hàng và các chi tiết.", "FashionFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
    }

    private async void Delete_Click(object? sender, EventArgs e)
    {
        if (!_isAdmin || string.IsNullOrWhiteSpace(_selectedOrderId)) return;
        if (MessageBox.Show(this, "Xóa đơn hàng đang chọn và các chi tiết?", "FashionFlow", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            await _service.DeleteAsync(_selectedOrderId);
            await LoadAsync();
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
    }

    private sealed record OrderDetailDraft(string MaSP, string TenSP, int SoLuong, decimal DonGia);
}
