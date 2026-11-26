using System.Drawing.Drawing2D;
using LibraryManagement.Data;
using LibraryManagement.Helpers;

namespace LibraryManagement.Forms.ForgotPasswordForm;

public partial class ForgotPasswordForm : Form
{
    // ====== Bảng màu đồng bộ với LoginForm & toàn hệ thống LIBRA ======
    private static readonly Color Primary     = Color.FromArgb(19, 98, 160);
    private static readonly Color PrimaryDark = Color.FromArgb(13, 71, 120);
    private static readonly Color SoftBlue    = Color.FromArgb(232, 240, 250);
    private static readonly Color BorderGray  = Color.FromArgb(210, 216, 224);
    private static readonly Color TextDark    = Color.FromArgb(30, 41, 59);
    private static readonly Color TextGray    = Color.FromArgb(100, 116, 139);
    private static readonly Color BgLight     = Color.FromArgb(248, 250, 252);
    private static readonly Color Green       = Color.FromArgb(34, 160, 90);
    private static readonly Color Red         = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    // ====== Trạng thái nghiệp vụ OTP ======
    private string _generatedOtp = string.Empty;
    private DateTime _otpExpiredAt;
    private int _targetReaderId;
    private string _targetPhone = string.Empty;
    private bool _isOtpVerified = false;

    // ====== Controls ======
    private TextBox _txtIdentifier = null!;
    private Button _btnSendOtp = null!;
    private Panel _pnlVerifiedBadge = null!;
    private Label _lblVerifiedText = null!;

    private Panel _pnlResetSection = null!;
    private TextBox _txtNewPassword = null!;
    private TextBox _txtConfirmPassword = null!;
    private Button _btnToggleNew = null!;
    private Button _btnToggleConfirm = null!;
    private Button _btnResetPassword = null!;
    private Label _lblError = null!;
    private bool _showNew;
    private bool _showConfirm;

    public ForgotPasswordForm()
    {
        InitializeComponent();
        BuildModernUi();
        Load += (_, _) => _txtIdentifier.Focus();
    }

    // ============================================================
    //  DỰNG GIAO DIỆN QUÊN MẬT KHẨU HIỆN ĐẠI (SPLIT-SCREEN)
    // ============================================================
    private void BuildModernUi()
    {
        SuspendLayout();
        Controls.Clear();

        Text = "LIBRA - Khôi phục mật khẩu";
        Size = new Size(960, 585);
        MinimumSize = new Size(920, 550);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);

        // 1. CỘT TRÁI: BANNER HƯỚNG DẪN KHÔI PHỤC TÀI KHOẢN
        var pnlBrand = new Panel
        {
            Dock = DockStyle.Left,
            Width = 400,
            BackColor = Primary
        };
        pnlBrand.Paint += PnlBrand_Paint;

        int brandInnerWidth = pnlBrand.Width - 72;

        var lblGlyph = new Label
        {
            Text = "\uE8D7", // Icon chìa khóa bảo mật
            Font = new Font(IconFont, 38F),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(36, 42)
        };

        var lblBrandTitle = new Label
        {
            Text = "LIBRA",
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true
        };

        var lblBrandSub = new Label
        {
            Text = "KHÔI PHỤC QUYỀN TRUY CẬP TÀI KHOẢN",
            Font = new Font("Segoe UI", 9.2F, FontStyle.Bold),
            ForeColor = Color.FromArgb(186, 220, 248),
            BackColor = Color.Transparent,
            AutoSize = true,
            MaximumSize = new Size(brandInnerWidth, 0)
        };

        var lblDesc = new Label
        {
            Text = "Quy trình lấy lại mật khẩu an toàn gồm 3 bước:\n\n" +
                   "①  Nhập Email hoặc Số điện thoại đã đăng ký\n" +
                   "②  Xác thực mã OTP 6 số gửi qua tin nhắn SMS\n" +
                   "③  Thiết lập mật khẩu mới để đăng nhập lại\n\n" +
                   "Mã OTP có hiệu lực trong vòng 03 phút.",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(230, 242, 255),
            BackColor = Color.Transparent,
            AutoSize = true,
            MaximumSize = new Size(brandInnerWidth, 0)
        };

        var lblFooter = new Label
        {
            Text = "Thư viện Trung tâm LIBRA · Bảo mật tài khoản",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(165, 205, 240),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(36, 500)
        };

        pnlBrand.Controls.AddRange(new Control[]
        {
            lblGlyph, lblBrandTitle, lblBrandSub, lblDesc, lblFooter
        });

        pnlBrand.PerformLayout();
        lblBrandTitle.Location = new Point(36, lblGlyph.Bottom + 14);
        lblBrandSub.Location   = new Point(36, lblBrandTitle.Bottom + 10);
        lblDesc.Location       = new Point(36, lblBrandSub.Bottom + 22);

        // 2. CỘT PHẢI: FORM XÁC THỰC & ĐỔI MẬT KHẨU
        var pnlRight = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        int x = 56;
        int w = 440;

        var lblHeading = new Label
        {
            Text = "Quên Mật Khẩu?",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = TextDark,
            AutoSize = true,
            Location = new Point(x - 2, 28)
        };

        var lblSubHeading = new Label
        {
            Text = "Nhập thông tin tài khoản để nhận mã xác thực OTP qua điện thoại",
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(x, 68)
        };

        // BƯỚC 1: Nhập Email / SĐT + Nút Gửi OTP
        pnlRight.Controls.Add(MakeFieldLabel("Bước 1: Email hoặc Số điện thoại đăng ký *", x, 106));

        int btnOtpWidth = 128;
        int inputIdWidth = w - btnOtpWidth - 10;

        var pnlIdentifier = CreateInputContainer(x, 130, inputIdWidth, "\uE715", out _txtIdentifier,
            "Nhập email hoặc SĐT...");
        _txtIdentifier.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnSendOtp_Click(this, EventArgs.Empty);
            }
        };

        _btnSendOtp = new Button
        {
            Text = "Gửi mã OTP",
            Font = new Font("Segoe UI", 9.2F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(btnOtpWidth, 42),
            Location = new Point(x + inputIdWidth + 10, 130),
            Cursor = Cursors.Hand
        };
        _btnSendOtp.FlatAppearance.BorderSize = 0;
        _btnSendOtp.MouseEnter += (_, _) => { if (_btnSendOtp.Enabled) _btnSendOtp.BackColor = PrimaryDark; };
        _btnSendOtp.MouseLeave += (_, _) => { if (_btnSendOtp.Enabled) _btnSendOtp.BackColor = Primary; };
        _btnSendOtp.Click += btnSendOtp_Click;

        // Badge hiển thị trạng thái đã xác thực OTP thành công (mặc định ẩn)
        _pnlVerifiedBadge = new Panel
        {
            Location = new Point(x, 182),
            Size = new Size(w, 34),
            BackColor = Color.FromArgb(232, 248, 239),
            Visible = false
        };
        _pnlVerifiedBadge.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(Color.FromArgb(140, 215, 170)), 0, 0, _pnlVerifiedBadge.Width - 1, _pnlVerifiedBadge.Height - 1);

        _lblVerifiedText = new Label
        {
            Text = "✔  Đã xác thực OTP thành công. Vui lòng nhập mật khẩu mới bên dưới.",
            Font = new Font("Segoe UI", 8.6F, FontStyle.Bold),
            ForeColor = Green,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(10, 8)
        };
        _pnlVerifiedBadge.Controls.Add(_lblVerifiedText);

        // BƯỚC 2 (SAU POP-UP): Vùng nhập mật khẩu mới (Khóa cho tới khi xác thực xong OTP)
        _pnlResetSection = new Panel
        {
            Location = new Point(x, 224),
            Size = new Size(w, 165),
            BackColor = Color.White,
            Enabled = false
        };

        _pnlResetSection.Controls.Add(MakeFieldLabel("Bước 2: Thiết lập mật khẩu mới *", 0, 0));

        var pnlNewPass = CreateInputContainer(0, 24, w, "\uE72E", out _txtNewPassword,
            "Nhập mật khẩu mới (tối thiểu 6 ký tự)...");
        _txtNewPassword.UseSystemPasswordChar = true;
        _txtNewPassword.Width = w - 82;
        _btnToggleNew = CreateEyeButton(w - 40, () =>
        {
            _showNew = !_showNew;
            _txtNewPassword.UseSystemPasswordChar = !_showNew;
            _btnToggleNew.Text = _showNew ? "\uED1A" : "\uE7B3";
            _btnToggleNew.ForeColor = _showNew ? Primary : TextGray;
        });
        pnlNewPass.Controls.Add(_btnToggleNew);

        _pnlResetSection.Controls.Add(MakeFieldLabel("Xác nhận mật khẩu mới *", 0, 78));

        var pnlConfirmPass = CreateInputContainer(0, 102, w, "\uE72E", out _txtConfirmPassword,
            "Nhập lại mật khẩu mới...");
        _txtConfirmPassword.UseSystemPasswordChar = true;
        _txtConfirmPassword.Width = w - 82;
        _txtConfirmPassword.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && _isOtpVerified)
            {
                e.SuppressKeyPress = true;
                btnResetPassword_Click(this, EventArgs.Empty);
            }
        };
        _btnToggleConfirm = CreateEyeButton(w - 40, () =>
        {
            _showConfirm = !_showConfirm;
            _txtConfirmPassword.UseSystemPasswordChar = !_showConfirm;
            _btnToggleConfirm.Text = _showConfirm ? "\uED1A" : "\uE7B3";
            _btnToggleConfirm.ForeColor = _showConfirm ? Primary : TextGray;
        });
        pnlConfirmPass.Controls.Add(_btnToggleConfirm);

        _pnlResetSection.Controls.AddRange(new Control[] { pnlNewPass, pnlConfirmPass });

        // Thông báo lỗi inline
        _lblError = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = Red,
            AutoSize = false,
            Size = new Size(w, 24),
            Location = new Point(x, 394),
            Visible = false
        };

        // Nút Đổi mật khẩu (Chỉ kích hoạt sau khi xác thực OTP trên Pop-up)
        _btnResetPassword = new Button
        {
            Text = "XÁC NHẬN ĐỔI MẬT KHẨU",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(160, 174, 192),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(w, 44),
            Location = new Point(x, 422),
            Enabled = false,
            Cursor = Cursors.Hand
        };
        _btnResetPassword.FlatAppearance.BorderSize = 0;
        _btnResetPassword.MouseEnter += (_, _) => { if (_btnResetPassword.Enabled) _btnResetPassword.BackColor = PrimaryDark; };
        _btnResetPassword.MouseLeave += (_, _) => { if (_btnResetPassword.Enabled) _btnResetPassword.BackColor = Primary; };
        _btnResetPassword.Click += btnResetPassword_Click;

        // Nút Quay lại
        var btnBack = new Button
        {
            Text = "← Quay lại trang đăng nhập",
            Font = new Font("Segoe UI", 9.2F, FontStyle.Bold),
            ForeColor = TextGray,
            BackColor = BgLight,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(w, 38),
            Location = new Point(x, 476),
            Cursor = Cursors.Hand
        };
        btnBack.FlatAppearance.BorderColor = BorderGray;
        btnBack.FlatAppearance.BorderSize = 1;
        btnBack.MouseEnter += (_, _) => { btnBack.BackColor = Color.FromArgb(238, 242, 246); btnBack.ForeColor = TextDark; };
        btnBack.MouseLeave += (_, _) => { btnBack.BackColor = BgLight; btnBack.ForeColor = TextGray; };
        btnBack.Click += btnBack_Click;

        pnlRight.Controls.AddRange(new Control[]
        {
            lblHeading, lblSubHeading,
            pnlIdentifier, _btnSendOtp,
            _pnlVerifiedBadge, _pnlResetSection,
            _lblError, _btnResetPassword, btnBack
        });

        Controls.Add(pnlRight);
        Controls.Add(pnlBrand);

        CancelButton = btnBack;
        ResumeLayout(true);
    }

    // ============================================================
    //  BƯỚC 1: GỬI MÃ OTP & MỞ POP-UP XÁC THỰC
    // ============================================================
    private async void btnSendOtp_Click(object? sender, EventArgs e)
    {
        string input = _txtIdentifier.Text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            ShowError("Vui lòng nhập số điện thoại hoặc email đã đăng ký!");
            _txtIdentifier.Focus();
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            string inputLower = input.ToLower();

            var reader = context.Users.FirstOrDefault(r =>
                r.Phone == input ||
                (r.Email != null && r.Email.ToLower() == inputLower) ||
                r.UserCode.ToLower() == inputLower);

            if (reader == null)
            {
                ShowError("Không tìm thấy tài khoản khớp với thông tin đã nhập!");
                return;
            }

            if (string.IsNullOrWhiteSpace(reader.Phone))
            {
                ShowError("Tài khoản này chưa cập nhật số điện thoại để nhận mã OTP!");
                return;
            }

            _targetReaderId = reader.Id;
            _targetPhone = reader.Phone;

            _btnSendOtp.Enabled = false;
            _btnSendOtp.Text = "Đang gửi...";
            _lblError.Visible = false;

            bool sent = await SendNewOtpAsync();

            _btnSendOtp.Enabled = true;
            _btnSendOtp.Text = "Gửi lại OTP";

            if (!sent)
            {
                ShowError("Gửi tin nhắn OTP thất bại, vui lòng thử lại!");
                return;
            }

            // Bật màn hình Pop-up nhập mã OTP
            if (ShowOtpVerificationPopup())
            {
                _isOtpVerified = true;
                _txtIdentifier.ReadOnly = true;
                _btnSendOtp.Enabled = false;
                _btnSendOtp.Text = "Đã xác thực";

                _pnlVerifiedBadge.Visible = true;
                _pnlResetSection.Enabled = true;
                _btnResetPassword.Enabled = true;
                _btnResetPassword.BackColor = Primary;
                AcceptButton = _btnResetPassword;

                _txtNewPassword.Focus();
            }
        }
        catch (Exception ex)
        {
            _btnSendOtp.Enabled = true;
            _btnSendOtp.Text = "Gửi mã OTP";
            MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task<bool> SendNewOtpAsync()
    {
        _generatedOtp = Random.Shared.Next(100000, 999999).ToString();
        _otpExpiredAt = DateTime.Now.AddMinutes(3);
        return await SmsService.SendOtpAsync(_targetPhone, _generatedOtp);
    }

    // ============================================================
    //  MÀN HÌNH POP-UP NHẬP & XÁC THỰC MÃ OTP
    // ============================================================
    private bool ShowOtpVerificationPopup()
    {
        bool verified = false;
        string maskedPhone = MaskPhoneNumber(_targetPhone);

        using var popup = new Form
        {
            Text = "Xác thực mã bảo mật OTP",
            Size = new Size(430, 315),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9.5F)
        };

        // Header xanh đồng bộ
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Primary
        };
        pnlHeader.Controls.Add(new Label
        {
            Text = "\uE72E",
            Font = new Font(IconFont, 18F),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(18, 14)
        });
        pnlHeader.Controls.Add(new Label
        {
            Text = "XÁC THỰC MÃ OTP",
            Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(52, 16)
        });

        var lblDesc = new Label
        {
            Text = $"Mã xác thực gồm 6 chữ số đã được gửi tới số điện thoại {maskedPhone}.",
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = TextDark,
            AutoSize = false,
            Size = new Size(370, 40),
            Location = new Point(24, 70)
        };

        // Ô nhập mã OTP 6 chữ số căn giữa nổi bật
        var txtOtpInput = new TextBox
        {
            Location = new Point(24, 116),
            Size = new Size(366, 42),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            TextAlign = HorizontalAlignment.Center,
            MaxLength = 6,
            BackColor = BgLight,
            ForeColor = Primary,
            PlaceholderText = "● ● ● ● ● ●"
        };
        txtOtpInput.KeyPress += (_, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        };

        var lblCountdown = new Label
        {
            Text = "Mã OTP có hiệu lực trong: 03:00",
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(24, 168)
        };

        var lnkResend = new LinkLabel
        {
            Text = "Gửi lại mã mới",
            Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
            LinkColor = Primary,
            AutoSize = true,
            Location = new Point(296, 168)
        };

        var lblPopupErr = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.6F),
            ForeColor = Red,
            AutoSize = false,
            Size = new Size(366, 22),
            Location = new Point(24, 192),
            Visible = false
        };

        txtOtpInput.TextChanged += (_, _) => lblPopupErr.Visible = false;

        var btnVerify = new Button
        {
            Text = "XÁC NHẬN MÃ OTP",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(236, 40),
            Location = new Point(24, 222),
            Cursor = Cursors.Hand
        };
        btnVerify.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Hủy bỏ",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = TextGray,
            BackColor = BgLight,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 40),
            Location = new Point(270, 222),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = BorderGray;
        btnCancel.Click += (_, _) => popup.Close();

        // Timer đếm ngược 3 phút theo thời gian thực
        using var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (_, _) =>
        {
            var remain = _otpExpiredAt - DateTime.Now;
            if (remain.TotalSeconds <= 0)
            {
                timer.Stop();
                lblCountdown.ForeColor = Red;
                lblCountdown.Text = "Mã OTP đã hết hạn. Vui lòng nhấn 'Gửi lại mã mới'.";
            }
            else
            {
                lblCountdown.ForeColor = TextGray;
                lblCountdown.Text = $"Mã OTP có hiệu lực trong: {remain.Minutes:D2}:{remain.Seconds:D2}";
            }
        };
        timer.Start();

        lnkResend.LinkClicked += async (_, _) =>
        {
            lnkResend.Enabled = false;
            lblPopupErr.Visible = false;
            bool ok = await SendNewOtpAsync();
            lnkResend.Enabled = true;

            if (ok)
            {
                txtOtpInput.Clear();
                txtOtpInput.Focus();
                timer.Start();
                MessageBox.Show($"Đã gửi lại mã OTP mới tới số {maskedPhone}.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblPopupErr.Text = "⚠  Gửi lại OTP thất bại, vui lòng thử lại.";
                lblPopupErr.Visible = true;
            }
        };

        void VerifyAction()
        {
            if (DateTime.Now > _otpExpiredAt)
            {
                lblPopupErr.Text = "⚠  Mã OTP đã hết hạn! Vui lòng nhấn 'Gửi lại mã mới'.";
                lblPopupErr.Visible = true;
                return;
            }

            if (txtOtpInput.Text.Trim() != _generatedOtp)
            {
                lblPopupErr.Text = "⚠  Mã OTP không chính xác. Vui lòng kiểm tra lại tin nhắn.";
                lblPopupErr.Visible = true;
                txtOtpInput.SelectAll();
                txtOtpInput.Focus();
                return;
            }

            verified = true;
            timer.Stop();
            popup.DialogResult = DialogResult.OK;
            popup.Close();
        }

        btnVerify.Click += (_, _) => VerifyAction();
        txtOtpInput.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                VerifyAction();
            }
        };

        popup.Controls.AddRange(new Control[]
        {
            pnlHeader, lblDesc, txtOtpInput, lblCountdown, lnkResend, lblPopupErr, btnVerify, btnCancel
        });
        popup.AcceptButton = btnVerify;
        popup.CancelButton = btnCancel;
        popup.Shown += (_, _) => txtOtpInput.Focus();

        popup.ShowDialog(this);
        return verified;
    }

    // ============================================================
    //  BƯỚC 3: LƯU MẬT KHẨU MỚI VÀO CSDL
    // ============================================================
    private void btnResetPassword_Click(object? sender, EventArgs e)
    {
        if (!_isOtpVerified)
        {
            ShowError("Vui lòng hoàn tất bước xác thực mã OTP trước!");
            return;
        }

        string newPass = _txtNewPassword.Text;
        string confirmPass = _txtConfirmPassword.Text;

        if (string.IsNullOrEmpty(newPass) || newPass.Length < 6)
        {
            ShowError("Mật khẩu mới phải có ít nhất 6 ký tự.");
            _txtNewPassword.Focus();
            return;
        }

        if (newPass != confirmPass)
        {
            ShowError("Mật khẩu xác nhận không khớp với mật khẩu mới!");
            _txtConfirmPassword.SelectAll();
            _txtConfirmPassword.Focus();
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            var reader = context.Users.Find(_targetReaderId);
            if (reader != null)
            {
                reader.PasswordHash = newPass;
                reader.UpdatedAt = DateTime.Now;
                context.SaveChanges();

                MessageBox.Show("Đặt lại mật khẩu thành công!\nBạn có thể đăng nhập ngay bằng mật khẩu mới.",
                    "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
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
        DialogResult = DialogResult.Cancel;
        Close();
    }

    // ============================================================
    //  HELPER UI
    // ============================================================
    private static Label MakeFieldLabel(string text, int x, int y) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        ForeColor = TextDark,
        AutoSize = true,
        Location = new Point(x, y)
    };

    private Panel CreateInputContainer(int x, int y, int width, string glyph, out TextBox textBox, string placeholder)
    {
        bool isFocused = false;
        var container = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, 42),
            BackColor = BgLight
        };

        container.Paint += (_, e) =>
        {
            using var pen = new Pen(isFocused ? Primary : BorderGray, isFocused ? 1.8f : 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, container.Width - 1, container.Height - 1);
        };

        var lblIcon = new Label
        {
            Text = glyph,
            Font = new Font(IconFont, 12F),
            ForeColor = TextGray,
            BackColor = BgLight,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(38, 38),
            Location = new Point(2, 2)
        };

        textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = BgLight,
            ForeColor = TextDark,
            Font = new Font("Segoe UI", 10F),
            Location = new Point(42, 11),
            Width = width - 52,
            PlaceholderText = placeholder
        };

        textBox.Enter += (_, _) =>
        {
            isFocused = true;
            lblIcon.ForeColor = Primary;
            container.Invalidate();
        };
        textBox.Leave += (_, _) =>
        {
            isFocused = false;
            lblIcon.ForeColor = TextGray;
            container.Invalidate();
        };
        textBox.TextChanged += (_, _) => _lblError.Visible = false;

        container.Controls.Add(lblIcon);
        container.Controls.Add(textBox);
        return container;
    }

    private static Button CreateEyeButton(int x, Action onClick)
    {
        var btn = new Button
        {
            Text = "\uE7B3",
            Font = new Font(IconFont, 11F),
            ForeColor = TextGray,
            BackColor = BgLight,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(36, 36),
            Location = new Point(x, 3),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (_, _) => onClick();
        return btn;
    }

    private static void PnlBrand_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var b1 = new SolidBrush(Color.FromArgb(18, 255, 255, 255));
        using var b2 = new SolidBrush(Color.FromArgb(12, 255, 255, 255));
        g.FillEllipse(b1, -60, 340, 260, 260);
        g.FillEllipse(b2, 180, -50, 240, 240);
    }

    private void ShowError(string msg)
    {
        _lblError.Text = "⚠  " + msg;
        _lblError.Visible = true;
    }

    private static string MaskPhoneNumber(string phone)
    {
        if (phone.Length <= 4) return phone;
        return new string('*', phone.Length - 3) + phone[^3..];
    }
}