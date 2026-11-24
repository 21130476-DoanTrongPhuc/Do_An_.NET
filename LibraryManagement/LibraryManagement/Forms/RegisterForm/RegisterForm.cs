using LibraryManagement.Data;
using LibraryManagement.Enums;
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

            bool isExisted = context.Users.Any(r =>
                r.Email == email || r.Username == email || r.Phone == phone);
            if (isExisted)
            {
                MessageBox.Show("Email hoặc số điện thoại này đã được đăng ký!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int nextNumber = (context.Users.Max(u => (int?)u.Id) ?? 0) + 1;
            string readerCode = $"DG{nextNumber:D3}";
            while (context.Users.Any(u => u.UserCode == readerCode) ||
                   context.ReaderCards.Any(c => c.CardCode == $"CARD-{readerCode}"))
            {
                nextNumber++;
                readerCode = $"DG{nextNumber:D3}";
            }

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var newReader = new Users
            {
                UserCode = readerCode,
                Username = email,
                FullName = fullName,
                Email = email,
                Phone = phone,
                PasswordHash = password,
                UserType = UserType.Reader,
                MaxActiveCopies = 3,
                Status = "Active",
                CreatedAt = now,
                UpdatedAt = now
            };

            context.Users.Add(newReader);
            context.SaveChanges();

            context.ReaderCards.Add(new ReaderCards
            {
                IdReader = newReader.Id,
                CardCode = $"CARD-{readerCode}",
                IssuedAt = DateTime.Today,
                ExpiresAt = DateTime.Today.AddYears(1),
                Status = "Active",
                Note = "Thẻ đăng ký mới"
            });
            context.SaveChanges();

            RegisteredEmail = email;

            MessageBox.Show($"Đăng ký thành công!\nMã độc giả của bạn là: {readerCode}", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            string detail = ex.InnerException?.Message ?? ex.Message;
            MessageBox.Show($"Lỗi lưu dữ liệu: {detail}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnBack_Click(object? sender, EventArgs e)
    {
        this.Close();
    }
}