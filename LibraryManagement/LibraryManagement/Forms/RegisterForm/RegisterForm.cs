using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Forms.RegisterForm;

public partial class RegisterForm : Form
{
    public string RegisteredEmail { get; private set; } = string.Empty;

    public RegisterForm()
    {
        InitializeComponent();
    }

    private void btnRegister_Click(object? sender, EventArgs e)
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
            MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            using var context = new LibraryDbContext();

            // Kiểm tra trùng Email hoặc Số điện thoại trong SQLite
            bool isExisted = context.Readers.Any(r => r.Email == email || r.Phone == phone);
            if (isExisted)
            {
                MessageBox.Show("Email hoặc số điện thoại này đã được đăng ký!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tự động sinh mã độc giả (VD: DG001, DG002...)
            int nextNumber = context.Readers.Count() + 1;
            string readerCode = $"DG{nextNumber:D3}";

            var newReader = new Readers
            {
                ReaderCode = readerCode,
                FullName = fullName,
                Email = email,
                Phone = phone,
                Password = password,
                IsLocked = false
            };

            context.Readers.Add(newReader);
            context.SaveChanges();

            RegisteredEmail = email;

            MessageBox.Show($"Đăng ký thành công!\nMã độc giả của bạn là: {readerCode}", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnBack_Click(object? sender, EventArgs e)
    {
        this.Close();
    }
}