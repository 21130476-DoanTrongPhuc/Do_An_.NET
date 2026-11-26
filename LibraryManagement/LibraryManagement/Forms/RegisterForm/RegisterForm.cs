using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Models;

namespace LibraryManagement.Forms.RegisterForm;

public partial class RegisterForm : Form
{
    // ====== Bảng màu đồng bộ với LoginForm & toàn hệ thống LIBRA ======
    private static readonly Color Primary     = Color.FromArgb(19, 98, 160);
    private static readonly Color PrimaryDark = Color.FromArgb(13, 71, 120);
    private static readonly Color SoftBlue    = Color.FromArgb(232, 240, 250);
    private static readonly Color BorderGray  = Color.FromArgb(210, 216, 224);
    private static readonly Color TextDark    = Color.FromArgb(30, 41, 59);
    private static readonly Color TextGray    = Color.FromArgb(100, 116, 139);
    private static readonly Color BgLight     = Color.FromArgb(248, 250, 252);
    private static readonly Color Red         = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    public string RegisteredEmail { get; private set; } = string.Empty;
    private readonly bool _librarianMode;

    // ====== Controls ======
    private TextBox _txtFullName = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtPhone = null!;
    private TextBox _txtPassword = null!;
    private TextBox _txtConfirmPassword = null!;
    private Button _btnTogglePass = null!;
    private Button _btnToggleConfirm = null!;
    private Button _btnRegister = null!;
    private Label _lblError = null!;
    private bool _showPass;
    private bool _showConfirm;

    public RegisterForm(bool librarianMode = false)
    {
        InitializeComponent();
        _librarianMode = librarianMode;
        BuildModernUi();
        Load += (_, _) => _txtFullName.Focus();
    }

    // ============================================================
    //  DỰNG GIAO DIỆN ĐĂNG KÝ HIỆN ĐẠI (SPLIT-SCREEN)
    // ============================================================
    private void BuildModernUi()
    {
        SuspendLayout();
        Controls.Clear();

        Text = _librarianMode
            ? "LIBRA - Thêm hồ sơ độc giả mới"
            : "LIBRA - Đăng ký tài khoản độc giả";
        Size = new Size(990, 620);          // Nới từ 940 -> 990 để cân đối với cột trái 400px
        MinimumSize = new Size(950, 590);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);

        // 1. CỘT TRÁI: BANNER THÔNG TIN QUYỀN LỢI / HƯỚNG DẪN (Nới rộng lên 400px)
        var pnlBrand = new Panel
        {
            Dock = DockStyle.Left,
            Width = 400,
            BackColor = Primary
        };
        pnlBrand.Paint += PnlBrand_Paint;

        int brandInnerWidth = pnlBrand.Width - 68; // Chừa lề trái 34px + lề phải 34px

        var lblGlyph = new Label
        {
            Text = _librarianMode ? "\uE8FA" : "\uE716",
            Font = new Font(IconFont, 36F),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(34, 40)
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
            Text = _librarianMode
                ? "PHÂN HỆ THỦ THƯ · CẤP THẺ MỚI"
                : "THÀNH VIÊN THƯ VIỆN TRUNG TÂM",
            Font = new Font("Segoe UI", 9.2F, FontStyle.Bold),
            ForeColor = Color.FromArgb(186, 220, 248),
            BackColor = Color.Transparent,
            AutoSize = true,
            MaximumSize = new Size(brandInnerWidth, 0)
        };

        string benefitsText = _librarianMode
            ? "Hệ thống sẽ tự động khởi tạo hồ sơ và thẻ thư viện cho độc giả:\n\n" +
            "✓  Tự động cấp mã độc giả (DGxxx)\n" +
            "✓  Phát hành thẻ CARD-DGxxx (hạn 1 năm)\n" +
            "✓  Hạn mức mượn mặc định: 3 cuốn sách\n" +
            "✓  Kích hoạt quyền mượn & đặt sách ngay"
            : "Tạo tài khoản miễn phí để trải nghiệm đầy đủ tiện ích thư viện:\n\n" +
            "✓  Cấp thẻ thư viện điện tử hiệu lực 1 năm\n" +
            "✓  Mượn tối đa 3 đầu sách cùng lúc\n" +
            "✓  Đặt giữ trước sách trực tuyến 24/7\n" +
            "✓  Nhận nhắc nhở hạn trả sách tự động";

        var lblDesc = new Label
        {
            Text = benefitsText,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(232, 243, 255),
            BackColor = Color.Transparent,
            AutoSize = true,
            MaximumSize = new Size(brandInnerWidth, 0)
        };

        var lblFooter = new Label
        {
            Text = "Thư viện Trung tâm LIBRA",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(165, 205, 240),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(34, 535)
        };

        pnlBrand.Controls.AddRange(new Control[]
        {
            lblGlyph, lblBrandTitle, lblBrandSub, lblDesc, lblFooter
        });

        pnlBrand.PerformLayout();
        lblBrandTitle.Location = new Point(34, lblGlyph.Bottom + 14);
        lblBrandSub.Location   = new Point(34, lblBrandTitle.Bottom + 10);
        lblDesc.Location       = new Point(34, lblBrandSub.Bottom + 20);

        // 2. CỘT PHẢI: BIỂU MẪU NHẬP THÔNG TIN
        var pnlRight = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        int x = 48;
        int w = 485;

        var lblHeading = new Label
        {
            Text = _librarianMode ? "Thêm Độc Giả Mới" : "Đăng Ký Tài Khoản Độc Giả",
            Font = new Font("Segoe UI", 18.5F, FontStyle.Bold),
            ForeColor = TextDark,
            AutoSize = true,
            Location = new Point(x - 2, 26)
        };

        var lblSubHeading = new Label
        {
            Text = _librarianMode
                ? "Nhập thông tin cá nhân để khởi tạo tài khoản và thẻ thư viện cho độc giả"
                : "Điền đầy đủ thông tin bên dưới để nhận mã độc giả và thẻ thư viện",
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(x, 64)
        };

        // Hàng 1: Họ và tên (Full width)
        int y = 102;
        pnlRight.Controls.Add(MakeFieldLabel("Họ và tên độc giả *", x, y));
        var pnlFullName = CreateInputContainer(x, y + 22, w, "\uE77B", out _txtFullName,
            "Nhập họ và tên đầy đủ (VD: Nguyễn Văn An)...");

        // Hàng 2: Email & Số điện thoại (Chia 2 cột cân đối)
        y = 176;
        int halfW = (w - 14) / 2;
        pnlRight.Controls.Add(MakeFieldLabel("Email (Tên đăng nhập) *", x, y));
        var pnlEmail = CreateInputContainer(x, y + 22, halfW, "\uE715", out _txtEmail,
            "example@gmail.com");

        pnlRight.Controls.Add(MakeFieldLabel("Số điện thoại *", x + halfW + 14, y));
        var pnlPhone = CreateInputContainer(x + halfW + 14, y + 22, halfW, "\uE717", out _txtPhone,
            "09xxxxxxxx");

        // Hàng 3: Mật khẩu & Xác nhận mật khẩu (Chia 2 cột + nút mắt hiện/ẩn)
        y = 250;
        pnlRight.Controls.Add(MakeFieldLabel("Mật khẩu *", x, y));
        var pnlPass = CreateInputContainer(x, y + 22, halfW, "\uE72E", out _txtPassword,
            "Tối thiểu 6 ký tự...");
        _txtPassword.UseSystemPasswordChar = true;
        _txtPassword.Width = halfW - 78;
        _btnTogglePass = CreateEyeButton(halfW - 38, () =>
        {
            _showPass = !_showPass;
            _txtPassword.UseSystemPasswordChar = !_showPass;
            _btnTogglePass.Text = _showPass ? "\uED1A" : "\uE7B3";
            _btnTogglePass.ForeColor = _showPass ? Primary : TextGray;
        });
        pnlPass.Controls.Add(_btnTogglePass);

        pnlRight.Controls.Add(MakeFieldLabel("Xác nhận mật khẩu *", x + halfW + 14, y));
        var pnlConfirm = CreateInputContainer(x + halfW + 14, y + 22, halfW, "\uE72E", out _txtConfirmPassword,
            "Nhập lại mật khẩu...");
        _txtConfirmPassword.UseSystemPasswordChar = true;
        _txtConfirmPassword.Width = halfW - 78;
        _btnToggleConfirm = CreateEyeButton(halfW - 38, () =>
        {
            _showConfirm = !_showConfirm;
            _txtConfirmPassword.UseSystemPasswordChar = !_showConfirm;
            _btnToggleConfirm.Text = _showConfirm ? "\uED1A" : "\uE7B3";
            _btnToggleConfirm.ForeColor = _showConfirm ? Primary : TextGray;
        });
        pnlConfirm.Controls.Add(_btnToggleConfirm);

        // Thẻ thông tin cấp thẻ tự động
        var pnlInfoBanner = new Panel
        {
            Location = new Point(x, 326),
            Size = new Size(w, 46),
            BackColor = SoftBlue
        };
        pnlInfoBanner.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(Color.FromArgb(185, 210, 238)), 0, 0, pnlInfoBanner.Width - 1, pnlInfoBanner.Height - 1);

        pnlInfoBanner.Controls.Add(new Label
        {
            Text = "\uE946",
            Font = new Font(IconFont, 13F),
            ForeColor = Primary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(12, 13)
        });
        pnlInfoBanner.Controls.Add(new Label
        {
            Text = "Hệ thống tự động cấp thẻ thư viện có hiệu lực 01 năm (hạn mức mượn tối đa 3 cuốn) ngay sau khi hoàn tất.",
            Font = new Font("Segoe UI", 8.4F),
            ForeColor = PrimaryDark,
            BackColor = Color.Transparent,
            Size = new Size(w - 48, 34),
            Location = new Point(38, 6)
        });

        // Thông báo lỗi inline
        _lblError = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = Red,
            AutoSize = false,
            Size = new Size(w, 24),
            Location = new Point(x, 382),
            Visible = false
        };

        // Nút Đăng ký / Thêm độc giả (Chính)
        _btnRegister = new Button
        {
            Text = _librarianMode ? "XÁC NHẬN THÊM ĐỘC GIẢ" : "HOÀN TẤT ĐĂNG KÝ",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(w, 44),
            Location = new Point(x, 412),
            Cursor = Cursors.Hand
        };
        _btnRegister.FlatAppearance.BorderSize = 0;
        _btnRegister.MouseEnter += (_, _) => _btnRegister.BackColor = PrimaryDark;
        _btnRegister.MouseLeave += (_, _) => _btnRegister.BackColor = Primary;
        _btnRegister.Click += btnRegister_Click;

        // Nút Quay lại
        var btnBack = new Button
        {
            Text = _librarianMode ? "Hủy & Quay lại" : "← Quay lại trang đăng nhập",
            Font = new Font("Segoe UI", 9.2F, FontStyle.Bold),
            ForeColor = TextGray,
            BackColor = BgLight,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(w, 38),
            Location = new Point(x, 466),
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
            pnlFullName, pnlEmail, pnlPhone, pnlPass, pnlConfirm,
            pnlInfoBanner, _lblError, _btnRegister, btnBack
        });

        Controls.Add(pnlRight);
        Controls.Add(pnlBrand);

        AcceptButton = _btnRegister;
        CancelButton = btnBack;
        ResumeLayout(true);
    }

    private static Label MakeFieldLabel(string text, int x, int y) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
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
            Size = new Size(width, 40),
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
            Font = new Font(IconFont, 11.5F),
            ForeColor = TextGray,
            BackColor = BgLight,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(36, 36),
            Location = new Point(2, 2)
        };

        textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = BgLight,
            ForeColor = TextDark,
            Font = new Font("Segoe UI", 9.8F),
            Location = new Point(40, 10),
            Width = width - 48,
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
            Font = new Font(IconFont, 10.5F),
            ForeColor = TextGray,
            BackColor = BgLight,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(34, 34),
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
        g.FillEllipse(b1, -60, 380, 260, 260);
        g.FillEllipse(b2, 160, -50, 230, 230);
    }

    private void ShowError(string msg)
    {
        _lblError.Text = "⚠  " + msg;
        _lblError.Visible = true;
    }

    // ============================================================
    //  XỬ LÝ NGHIỆP VỤ ĐĂNG KÝ & CẤP THẺ THƯ VIỆN
    // ============================================================
    private void btnRegister_Click(object? sender, EventArgs e)
    {
        string fullName = _txtFullName.Text.Trim();
        string email = _txtEmail.Text.Trim();
        string phone = _txtPhone.Text.Trim();
        string password = _txtPassword.Text;
        string confirmPassword = _txtConfirmPassword.Text;

        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
        {
            ShowError("Vui lòng nhập đầy đủ tất cả các trường bắt buộc (*).");
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowError("Định dạng địa chỉ Email không hợp lệ.");
            _txtEmail.Focus();
            return;
        }

        if (!Regex.IsMatch(phone, @"^\d{9,11}$"))
        {
            ShowError("Số điện thoại phải gồm 9 đến 11 chữ số.");
            _txtPhone.Focus();
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Mật khẩu phải có ít nhất 6 ký tự.");
            _txtPassword.Focus();
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Mật khẩu xác nhận không khớp với mật khẩu đã nhập.");
            _txtConfirmPassword.SelectAll();
            _txtConfirmPassword.Focus();
            return;
        }

        try
        {
            using var context = new LibraryDbContext();

            bool isExisted = context.Users.Any(r =>
                r.Email == email || r.Username == email || r.Phone == phone);
            if (isExisted)
            {
                ShowError("Email hoặc số điện thoại này đã được đăng ký trên hệ thống.");
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

            DateTime now = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
                Status = UserStatus.Active,
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
                Status = ReaderCardStatus.Active,
                Note = "Thẻ đăng ký mới"
            });
            context.SaveChanges();

            RegisteredEmail = email;

            string successMessage = _librarianMode
                ? $"Thêm độc giả thành công!\n• Mã độc giả: {readerCode}\n• Mã thẻ thư viện: CARD-{readerCode}"
                : $"Đăng ký thành công!\n• Mã độc giả của bạn: {readerCode}\n• Mã thẻ thư viện: CARD-{readerCode}";
            MessageBox.Show(successMessage, "Hoàn tất",
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
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}