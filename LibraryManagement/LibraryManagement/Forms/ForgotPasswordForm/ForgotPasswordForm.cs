using LibraryManagement.Data;
using LibraryManagement.Helpers;

namespace LibraryManagement.Forms.ForgotPasswordForm;

public partial class ForgotPasswordForm : Form
{
    private string _generatedOtp = string.Empty;
    private DateTime _otpExpiredAt;
    private int _targetReaderId;

    public ForgotPasswordForm()
    {
        InitializeComponent();
    }

    private async void btnSendOtp_Click(object? sender, EventArgs e)
    {
        string input = txtIdentifier.Text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Vui lòng nhập số điện thoại hoặc email đã đăng ký!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            var reader = context.Readers.FirstOrDefault(r => r.Phone == input || r.Email == input);

            if (reader == null)
            {
                MessageBox.Show("Không tìm thấy tài khoản độc giả khớp với thông tin này!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(reader.Phone))
            {
                MessageBox.Show("Tài khoản này chưa cập nhật số điện thoại để nhận OTP!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tạo mã OTP ngẫu nhiên 6 chữ số, có hiệu lực 3 phút
            _targetReaderId = reader.Id;
            _generatedOtp = Random.Shared.Next(100000, 999999).ToString();
            _otpExpiredAt = DateTime.Now.AddMinutes(3);

            btnSendOtp.Enabled = false;
            bool sent = await SmsService.SendOtpAsync(reader.Phone, _generatedOtp);
            btnSendOtp.Enabled = true;

            if (sent)
            {
                txtIdentifier.ReadOnly = true;
                txtOtp.Focus();
            }
            else
            {
                MessageBox.Show("Gửi tin nhắn OTP thất bại, vui lòng thử lại!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            btnSendOtp.Enabled = true;
            MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnResetPassword_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_generatedOtp))
        {
            MessageBox.Show("Vui lòng bấm 'Gửi mã OTP' trước!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (DateTime.Now > _otpExpiredAt)
        {
            MessageBox.Show("Mã OTP đã hết hạn! Vui lòng gửi lại mã mới.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (txtOtp.Text.Trim() != _generatedOtp)
        {
            MessageBox.Show("Mã OTP không chính xác!", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string newPass = txtNewPassword.Text;
        string confirmPass = txtConfirmPassword.Text;

        if (string.IsNullOrEmpty(newPass) || newPass != confirmPass)
        {
            MessageBox.Show("Mật khẩu mới trống hoặc xác nhận không khớp!", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            var reader = context.Readers.Find(_targetReaderId);
            if (reader != null)
            {
                reader.Password = newPass;
                context.SaveChanges();

                MessageBox.Show("Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi cập nhật mật khẩu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnBack_Click(object? sender, EventArgs e)
    {
        this.Close();
    }
}