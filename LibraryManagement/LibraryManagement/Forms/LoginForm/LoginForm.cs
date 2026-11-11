using System;
using System.Windows.Forms;
using LibraryManagement.Forms.LibrarianFormMain;

namespace LibraryManagement.Forms;

public partial class LoginForm : Form
{
    public LoginForm()
    {
        InitializeComponent();
    }

    private void LoginForm_Load(object sender, EventArgs e)
    {
        rdoReader.Checked = true;
    }

    private void rdoReader_CheckedChanged(object sender, EventArgs e)
    {
        if (rdoReader.Checked)
        {
            lblUsernameTitle.Text = "Mã độc giả hoặc email";
            txtUsername.Text = "DG001";
        }
    }

    private void rdoLibrarian_CheckedChanged(object sender, EventArgs e)
    {
        if (rdoLibrarian.Checked)
        {
            lblUsernameTitle.Text = "Tên đăng nhập thủ thư";
            txtUsername.Text = "admin";
        }
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (rdoLibrarian.Checked)
        {
            // Demo: mở giao diện chính của thủ thư
            var mainForm = new LibrarianMainForm();
            mainForm.FormClosed += (s, args) => Close(); // đóng form chính thì thoát app
            mainForm.Show();
            Hide();
        }
        else
        {
            MessageBox.Show($"Đăng nhập Độc giả thành công: {username}", "Thông báo");
        }
    }
}