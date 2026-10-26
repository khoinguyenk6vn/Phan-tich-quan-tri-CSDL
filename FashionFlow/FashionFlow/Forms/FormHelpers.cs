using FashionFlow.BLL;
using Microsoft.Data.SqlClient;

namespace FashionFlow.Presentation.Forms;

internal static class FormHelpers
{
    public static void Configure(Form form, string title)
    {
        form.Text = title;
        form.ClientSize = new Size(1024, 576);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.AutoScaleMode = AutoScaleMode.Font;
    }

    public static void ShowError(IWin32Window owner, Exception exception)
    {
        var baseException = exception.GetBaseException();
        var message = exception is Microsoft.EntityFrameworkCore.DbUpdateException && baseException is SqlException sqlException
            ? sqlException.Number switch
            {
                51010 => "Ngày giao dự kiến phải từ ngày đặt trở đi.",
                51011 => "Số lượng cấp phát vượt quá tồn kho nguyên phụ liệu.",
                51012 => "Ngày làm không được ở tương lai.",
                _ => "Không lưu được dữ liệu. Kiểm tra mã, khóa ngoại và các dòng dữ liệu đang được sử dụng.\n" + baseException.Message
            }
            : exception.Message;
        MessageBox.Show(owner, message, "FashionFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void AddLabelledControl(TableLayoutPanel panel, int row, string text, Control control)
    {
        var label = new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left };
        panel.Controls.Add(label, 0, row);
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, 1, row);
    }

    public static void BindEnumlessCombo(ComboBox combo, IReadOnlyList<string> values)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Items.AddRange(values.Cast<object>().ToArray());
        if (combo.Items.Count > 0) combo.SelectedIndex = 0;
    }
}
