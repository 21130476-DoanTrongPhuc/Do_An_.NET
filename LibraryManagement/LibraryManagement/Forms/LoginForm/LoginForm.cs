using System;
using System.Windows.Forms;
using static LibraryManagement.Forms.LibrarianFormMain;

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

        // Demo: luôn đăng nhập thành công. Sau này thay bằng kiểm tra CSDL.
        bool ok = true;
        if (!ok)
        {
            MessageBox.Show("Sai tài khoản hoặc mật khẩu.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (rdoLibrarian.Checked)
        {
            // Thủ thư -> giao diện quản lý
            var f = new LibraryManagement.Forms.LibrarianFormMain();
            f.FormClosed += (_, _) => AfterClose(f.IsLogout);
            this.Hide();
            f.Show();
        }
        else
        {
            // Người mượn sách -> giao diện độc giả
            var f = new LibraryManagement.Forms.HomePage();
            f.FormClosed += (_, _) => AfterClose(f.IsLogout);
            this.Hide();
            f.Show();
        }
    }

    private void AfterClose(bool logout)
    {
        if (logout)
        {
            txtPassword.Clear();
            this.Show(); // quay lại trang đăng nhập
        }
        else
        {
            this.Close(); // bấm X thì thoát hẳn app
        }
    }
}