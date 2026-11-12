using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Forms;

public partial class RegisterForm : Form
{
    public string RegisteredEmail { get; private set; } = string.Empty;

    public RegisterForm()
    {
        InitializeComponent();
    }

    private void btnRegister_Click(object sender, EventArgs e)
    {
        string fullName = txtFullName.Text.Trim();
        string email = txtEmail.Text.Trim();
        string phone = txtPhone.Text.Trim();
        string password = txtPassword.Text;
        string confirmPassword = txtConfirmPassword.Text;

        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (password != confirmPassword)
        {
            MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            context.Database.EnsureCreated();

            if (context.Readers.Any(r => r.Email == email))
            {
                MessageBox.Show("Email này đã được sử dụng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string readerCode = $"DG{DateTime.Now:yyMMddHHmmss}";

            var newReader = new Readers
            {
                ReaderId = readerCode,
                ReaderCode = readerCode,
                FullName = fullName,
                Email = email,
                Phone = phone,
                Password = password,
                CardExpiryDate = DateTime.Now.AddYears(1),
                IsCardExpiryDate = DateTime.Now.AddYears(1),
                IsLocked = false
            };

            context.Readers.Add(newReader);
            context.SaveChanges();

            RegisteredEmail = email;

            MessageBox.Show($"Đăng ký thành công!\nMã độc giả của bạn là: {readerCode}", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnBack_Click(object sender, EventArgs e)
    {
        Close();
    }
}