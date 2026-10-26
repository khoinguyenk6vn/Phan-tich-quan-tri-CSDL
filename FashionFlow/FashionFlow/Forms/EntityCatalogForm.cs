using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;
using FashionFlow.BLL;
using FashionFlow.DAL;
using FashionFlow.DAL.Entities;

namespace FashionFlow.Presentation.Forms;

public sealed class EntityCatalogForm<TEntity> : Form where TEntity : class, new()
{
    private readonly CatalogService<TEntity> _service;
    private readonly string[] _keyNames;
    private readonly bool _isAdmin;
    private readonly BindingSource _source = new();
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    private readonly Panel _editorHost = new() { Dock = DockStyle.Fill, AutoScroll = true };
    private readonly TableLayoutPanel _editorFields = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, GrowStyle = TableLayoutPanelGrowStyle.AddRows };
    private readonly Dictionary<PropertyInfo, Control> _scalarEditors = new();
    private readonly Dictionary<string, ComboBox> _foreignKeyEditors = new(StringComparer.Ordinal);
    private readonly List<ForeignKeyEditorField> _foreignKeyFields = new();
    private TEntity? _current;
    private bool _loading;
    private bool _isNew;

    public EntityCatalogForm(LoginSession session, FashionFlowContextFactory factory, string title, string keyName)
    {
        _keyNames = keyName.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        _isAdmin = session.IsAdmin;
        _service = new CatalogService<TEntity>(new EfCatalogRepository<TEntity>(factory, session.ConnectionString));
        FormHelpers.Configure(this, $"FashionFlow - {title}");

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = title, AutoSize = true, Padding = new Padding(0, 0, 0, 8) }, 0, 0);

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
        var editor = new GroupBox { Text = "Thông tin", Dock = DockStyle.Fill, Padding = new Padding(10) };
        _editorHost.Controls.Add(_editorFields);
        editor.Controls.Add(_editorHost);
        split.Panel1.Controls.Add(editor);
        _grid.DataSource = _source;
        _grid.SelectionChanged += Grid_SelectionChanged;
        split.Panel2.Controls.Add(_grid);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 8, 0, 0) };
        var add = new Button { Text = "Thêm", AutoSize = true };
        var edit = new Button { Text = "Sửa", AutoSize = true };
        var save = new Button { Text = "Lưu", AutoSize = true };
        var delete = new Button { Text = "Xóa", AutoSize = true, Visible = session.IsAdmin };
        var refresh = new Button { Text = "Làm mới", AutoSize = true };
        var close = new Button { Text = "Đóng", AutoSize = true };
        add.Click += (_, _) => StartNew();
        edit.Click += (_, _) => StartEdit();
        save.Click += Save_Click;
        delete.Click += Delete_Click;
        refresh.Click += async (_, _) => await LoadDataAsync();
        close.Click += (_, _) => Close();
        actions.Controls.AddRange([add, edit, save, delete, refresh, close]);
        root.Controls.Add(split, 0, 1);
        root.Controls.Add(actions, 0, 2);
        Controls.Add(root);
        SetEditorEnabled(false);
        Shown += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            _foreignKeyFields.AddRange(await _service.GetForeignKeyFieldsAsync());
            BuildEditor();
            await LoadDataAsync();
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
    }

    private void BuildEditor()
    {
        _editorFields.Controls.Clear();
        _scalarEditors.Clear();
        _foreignKeyEditors.Clear();
        var foreignKeyProperties = _foreignKeyFields.SelectMany(field => field.PropertyNames).ToHashSet(StringComparer.Ordinal);
        foreach (var field in _foreignKeyFields)
        {
            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = nameof(ForeignKeyEditorOption.Text), DataSource = field.Options };
            _foreignKeyEditors.Add(string.Join("|", field.PropertyNames), combo);
            AddField(field.Label, combo);
        }

        foreach (var property in typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
                     .Where(property => property.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false)
                     .Where(property => !foreignKeyProperties.Contains(property.Name))
                     .Where(property => property.GetCustomAttribute<DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption != DatabaseGeneratedOption.Identity))
        {
            Control input;
            if (property.PropertyType == typeof(DateOnly))
            {
                var datePicker = new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill };
                if (typeof(TEntity) == typeof(PhanCong) && property.Name == nameof(PhanCong.NgayLam))
                    datePicker.MaxDate = DateTime.Today;
                input = datePicker;
            }
            else
            {
                input = new TextBox { Dock = DockStyle.Fill };
            }
            _scalarEditors.Add(property, input);
            AddField(property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name, input);
        }
    }

    private void AddField(string labelText, Control input)
    {
        var row = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, RowCount = 2, Dock = DockStyle.Top, Margin = new Padding(3, 3, 3, 8) };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        input.Width = 250;
        input.Margin = new Padding(0, 3, 0, 0);
        row.Controls.Add(input, 0, 1);
        _editorFields.Controls.Add(row);
    }

    private async Task LoadDataAsync(object?[]? selectedKey = null)
    {
        _loading = true;
        try
        {
            _source.DataSource = await _service.GetAllAsync();
            _grid.ClearSelection();
            DataGridViewRow? selected = null;
            if (selectedKey is not null)
                selected = FindRow(selectedKey);
            selected ??= _grid.Rows.Cast<DataGridViewRow>().FirstOrDefault();
            if (selected?.DataBoundItem is TEntity entity)
            {
                selected.Selected = true;
                _grid.CurrentCell = selected.Cells[0];
                ShowEntity(entity);
            }
            else
            {
                _current = null;
                ClearEditor();
                SetEditorEnabled(false);
            }
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
        finally { _loading = false; }
    }

    private DataGridViewRow? FindRow(object?[] values) => _grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row =>
        _keyNames.Select((name, index) => Equals(typeof(TEntity).GetProperty(name)?.GetValue(row.DataBoundItem), values[index])).All(match => match));

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_loading || _grid.CurrentRow?.DataBoundItem is not TEntity entity) return;
        ShowEntity(entity);
    }

    private void ShowEntity(TEntity entity)
    {
        _current = entity;
        _isNew = false;
        foreach (var (property, control) in _scalarEditors)
        {
            var value = property.GetValue(entity);
            if (control is TextBox textBox)
                textBox.Text = Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";
            else if (control is DateTimePicker datePicker && value is DateOnly date)
                datePicker.Value = date.ToDateTime(TimeOnly.MinValue);
        }
        foreach (var field in _foreignKeyFields)
        {
            var values = field.PropertyNames.Select(name => typeof(TEntity).GetProperty(name)!.GetValue(entity)).ToArray();
            var key = string.Join("|", field.PropertyNames);
            var choice = _foreignKeyEditors[key].Items.Cast<ForeignKeyEditorOption>().FirstOrDefault(option =>
                option.Values.SequenceEqual(values));
            _foreignKeyEditors[key].SelectedItem = choice;
        }
        SetEditorEnabled(false);
    }

    private void StartNew()
    {
        _grid.ClearSelection();
        _current = new TEntity();
        _isNew = true;
        FillEditorFromEntity(_current);
        SetEditorEnabled(true);
    }

    private void StartEdit()
    {
        if (_current is null) return;
        _isNew = false;
        SetEditorEnabled(true);
    }

    private void FillEditorFromEntity(TEntity entity)
    {
        foreach (var (property, control) in _scalarEditors)
        {
            var value = property.GetValue(entity);
            if (control is TextBox textBox)
                textBox.Text = Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";
            else if (control is DateTimePicker datePicker && value is DateOnly date)
                datePicker.Value = date.ToDateTime(TimeOnly.MinValue);
        }
        foreach (var field in _foreignKeyFields)
        {
            var values = field.PropertyNames.Select(name => typeof(TEntity).GetProperty(name)!.GetValue(entity)).ToArray();
            var key = string.Join("|", field.PropertyNames);
            _foreignKeyEditors[key].SelectedItem = _foreignKeyEditors[key].Items.Cast<ForeignKeyEditorOption>()
                .FirstOrDefault(option => option.Values.SequenceEqual(values));
        }
    }

    private void ClearEditor()
    {
        foreach (var control in _scalarEditors.Values)
        {
            if (control is TextBox textBox) textBox.Clear();
            else if (control is DateTimePicker datePicker) datePicker.Value = DateTime.Today;
        }
        foreach (var combo in _foreignKeyEditors.Values) combo.SelectedItem = null;
    }

    private void SetEditorEnabled(bool enabled)
    {
        foreach (var (property, control) in _scalarEditors)
            control.Enabled = enabled && (_isNew || !_keyNames.Contains(property.Name, StringComparer.Ordinal));
        foreach (var field in _foreignKeyFields)
            _foreignKeyEditors[string.Join("|", field.PropertyNames)].Enabled = enabled &&
                (_isNew || !field.PropertyNames.Any(name => _keyNames.Contains(name, StringComparer.Ordinal)));
    }

    private async void Save_Click(object? sender, EventArgs e)
    {
        if (_current is null) return;
        try
        {
            var adding = _isNew;
            ReadEditorIntoEntity(_current);
            await _service.SaveAsync(_current);
            var keyValues = _keyNames.Select(name => typeof(TEntity).GetProperty(name)!.GetValue(_current)).ToArray();
            await LoadDataAsync(keyValues);
            MessageBox.Show(this, adding ? "Đã thêm dữ liệu." : "Đã cập nhật dữ liệu.", "FashionFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
    }

    private void ReadEditorIntoEntity(TEntity entity)
    {
        foreach (var (property, control) in _scalarEditors)
        {
            if (control is DateTimePicker datePicker)
            {
                property.SetValue(entity, DateOnly.FromDateTime(datePicker.Value));
                continue;
            }
            if (control is not TextBox textBox) continue;
            var text = textBox.Text.Trim();
            if (property.PropertyType == typeof(string))
            {
                var isNullable = new NullabilityInfoContext().Create(property).WriteState == NullabilityState.Nullable;
                property.SetValue(entity, isNullable && text.Length == 0 ? null : text);
            }
            else if (property.PropertyType == typeof(int) && int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var integer))
                property.SetValue(entity, integer);
            else if (property.PropertyType == typeof(decimal) && TryParseDecimal(text, out var number))
                property.SetValue(entity, number);
            else
                throw new InvalidOperationException($"Giá trị '{text}' không hợp lệ cho {property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name}.");
        }
        foreach (var field in _foreignKeyFields)
        {
            var combo = _foreignKeyEditors[string.Join("|", field.PropertyNames)];
            if (combo.SelectedItem is not ForeignKeyEditorOption option)
                throw new InvalidOperationException($"Vui lòng chọn {field.Label}.");
            for (var index = 0; index < field.PropertyNames.Length; index++)
                typeof(TEntity).GetProperty(field.PropertyNames[index])!.SetValue(entity, option.Values[index]);
        }
    }

    private static bool TryParseDecimal(string text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private async void Delete_Click(object? sender, EventArgs e)
    {
        if (!_isAdmin || _current is null) return;
        if (MessageBox.Show(this, "Bạn có muốn xóa dòng đang chọn?", "FashionFlow", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            await _service.DeleteAsync(_current);
            _current = null;
            await LoadDataAsync();
        }
        catch (Exception exception) { FormHelpers.ShowError(this, exception); }
    }
}
