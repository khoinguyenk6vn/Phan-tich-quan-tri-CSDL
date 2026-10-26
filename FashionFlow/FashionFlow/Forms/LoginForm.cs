using FashionFlow.BLL;
using FashionFlow.DAL;

namespace FashionFlow.Presentation.Forms;

public sealed class LoginForm : Form
{
    private readonly LoginService _loginService;
    private readonly TextBox _userName = new() { Text = "admin" };
    private readonly TextBox _password = new() { UseSystemPasswordChar = true };
    private readonly Button _loginButton = new() { Text = "Đăng nhập", AutoSize = true };
    private readonly Label _status = new() { AutoSize = true };

    public LoginForm(FashionFlowContextFactory contextFactory)
    {
        _loginService = new LoginService(contextFactory);
        FormHelpers.Configure(this, "FashionFlow - Đăng nhập");

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, Padding = new Padding(24) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, AutoSize = true, Padding = new Padding(12) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        FormHelpers.AddLabelledControl(fields, 0, "Tài khoản SQL", _userName);
        FormHelpers.AddLabelledControl(fields, 1, "Mật khẩu", _password);
        _loginButton.Click += LoginButton_Click;
        fields.Controls.Add(_loginButton, 1, 2);
        fields.Controls.Add(_status, 1, 3);
        root.Controls.Add(fields, 1, 1);
        Controls.Add(root);
        AcceptButton = _loginButton;
    }

    private async void LoginButton_Click(object? sender, EventArgs e)
    {
        _loginButton.Enabled = false;
        _status.Text = "Đang kết nối...";
        try
        {
            var session = await _loginService.SignInAsync(_userName.Text, _password.Text);
            Hide();
            using var dashboard = new DashboardForm(session, new FashionFlowContextFactory());
            dashboard.ShowDialog(this);
            Close();
        }
        catch (Exception exception)
        {
            FormHelpers.ShowError(this, exception);
            _status.Text = "";
        }
        finally
        {
            if (!IsDisposed) _loginButton.Enabled = true;
        }
    }
}
