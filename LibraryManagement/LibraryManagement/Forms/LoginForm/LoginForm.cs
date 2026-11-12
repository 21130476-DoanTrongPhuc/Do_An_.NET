using LibraryManagement.Data;
using LibraryManagement.Helpers;

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

        try
        {
            using var context = new LibraryDbContext();
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khởi tạo cơ sở dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void rdoReader_CheckedChanged(object sender, EventArgs e)
    {
        if (rdoReader.Checked)
        {
            lblUsernameTitle.Text = "Mã độc giả hoặc email";
            txtUsername.Clear();
            txtPassword.Clear();
        }
    }

    private void rdoLibrarian_CheckedChanged(object sender, EventArgs e)
    {
        if (rdoLibrarian.Checked)
        {
            lblUsernameTitle.Text = "Tên đăng nhập thủ thư";
            txtUsername.Clear();
            txtPassword.Clear();
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

        try
        {
            using var context = new LibraryDbContext();

            if (rdoLibrarian.Checked)
            {
                var librarian = context.Librarians
                    .FirstOrDefault(l => l.Username == username && l.Password == password);

                if (librarian != null)
                {
                    Session.CurrentLibrarianId = librarian.Id;

                    var f = new LibrarianFormMain();
                    f.FormClosed += (_, _) => AfterClose(f.IsLogout);
                    this.Hide();
                    f.Show();
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu thủ thư.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                var reader = context.Readers
                    .FirstOrDefault(r => (r.ReaderCode == username || r.Email == username) && r.Password == password);

                if (reader != null)
                {
                    if (reader.IsLocked)
                    {
                        MessageBox.Show("Tài khoản của bạn đang bị khóa, vui lòng liên hệ thủ thư!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    Session.CurrentReaderId = reader.Id;

                    var f = new HomePage();
                    f.FormClosed += (_, _) => AfterClose(f.IsLogout);
                    this.Hide();
                    f.Show();
                }
                else
                {
                    MessageBox.Show("Sai mã độc giả/email hoặc mật khẩu.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnRegister_Click(object sender, EventArgs e)
    {
        if (rdoLibrarian.Checked)
        {
            MessageBox.Show("Thủ thư không được tự đăng ký tài khoản.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var registerForm = new RegisterForm();
        if (registerForm.ShowDialog() == DialogResult.OK)
        {
            txtUsername.Text = registerForm.RegisteredEmail;
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }

    private void AfterClose(bool logout)
    {
        if (logout)
        {
            txtPassword.Clear();
            Session.Clear();
            this.Show();
        }
        else
        {
            this.Close();
        }
    }
}