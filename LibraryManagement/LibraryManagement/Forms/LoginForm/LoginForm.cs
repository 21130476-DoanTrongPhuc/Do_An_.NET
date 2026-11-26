using System.Drawing.Drawing2D;
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Helpers;
using LibraryManagement.Models;

namespace LibraryManagement.Forms.LoginForm;

public partial class LoginForm : Form
{
    // ====== Bảng màu đồng bộ với toàn hệ thống LIBRA ======
    private static readonly Color Primary      = Color.FromArgb(19, 98, 160);
    private static readonly Color PrimaryDark  = Color.FromArgb(13, 71, 120);
    private static readonly Color SoftBlue     = Color.FromArgb(232, 240, 250);
    private static readonly Color BorderGray   = Color.FromArgb(210, 216, 224);
    private static readonly Color TextDark     = Color.FromArgb(30, 41, 59);
    private static readonly Color TextGray     = Color.FromArgb(100, 116, 139);
    private static readonly Color BgLight      = Color.FromArgb(248, 250, 252);
    private static readonly Color Red          = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    // ====== Controls ======
    private TextBox _txtAccount = null!;
    private TextBox _txtPassword = null!;
    private Panel _pnlAccountBox = null!;
    private Panel _pnlPasswordBox = null!;
    private Button _btnTogglePass = null!;
    private Button _btnLogin = null!;
    private Label _lblError = null!;
    private bool _isPasswordVisible = false;

    public LoginForm()
    {
        InitializeComponent();
        BuildModernUi();
        Load += LoginForm_Load;
    }

    // ============================================================
    //  DỰNG GIAO DIỆN ĐĂNG NHẬP HIỆN ĐẠI (SPLIT-SCREEN)
    // ============================================================
    private void BuildModernUi()
{
    SuspendLayout();
    Controls.Clear();

    Text = "LIBRA - Đăng nhập hệ thống thư viện";
    Size = new Size(960, 575);          // Nới từ 920 -> 960
    MinimumSize = new Size(920, 540);
    StartPosition = FormStartPosition.CenterScreen;
    FormBorderStyle = FormBorderStyle.FixedSingle;
    MaximizeBox = false;
    BackColor = Color.White;
    Font = new Font("Segoe UI", 9.5F);

    // 1. CỘT TRÁI: BANNER THƯƠNG HIỆU THƯ VIỆN (Nới rộng lên 410px)
    var pnlBrand = new Panel
    {
        Dock = DockStyle.Left,
        Width = 410,
        BackColor = Primary
    };
    pnlBrand.Paint += PnlBrand_Paint;

    int brandInnerWidth = pnlBrand.Width - 72; // Chừa lề trái 36px + lề phải 36px

    var lblBookGlyph = new Label
    {
        Text = "\uE82D",
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
        Text = "HỆ THỐNG QUẢN LÝ THƯ VIỆN THÔNG MINH",
        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
        ForeColor = Color.FromArgb(186, 220, 248),
        BackColor = Color.Transparent,
        AutoSize = true,
        MaximumSize = new Size(brandInnerWidth, 0)
    };

    var lblDesc = new Label
    {
        Text = "Cổng truy cập thống nhất dành cho Độc giả và Cán bộ Thư viện.\n\n" +
            "✓  Tra cứu kho sách & đặt trước trực tuyến\n" +
            "✓  Quản lý lưu thông mượn / trả tự động\n" +
            "✓  Theo dõi thẻ thư viện & lịch sử giao dịch",
        Font = new Font("Segoe UI", 9.5F),
        ForeColor = Color.FromArgb(230, 242, 255),
        BackColor = Color.Transparent,
        AutoSize = true,
        MaximumSize = new Size(brandInnerWidth, 0)
    };

    var lblFooterBrand = new Label
    {
        Text = "Thư viện Trung tâm · Phiên bản 2.4.0",
        Font = new Font("Segoe UI", 8.5F),
        ForeColor = Color.FromArgb(165, 205, 240),
        BackColor = Color.Transparent,
        AutoSize = true,
        Location = new Point(36, 490)
    };

    pnlBrand.Controls.AddRange(new Control[]
    {
        lblBookGlyph, lblBrandTitle, lblBrandSub, lblDesc, lblFooterBrand
    });

    pnlBrand.PerformLayout();
    lblBrandTitle.Location = new Point(36, lblBookGlyph.Bottom + 14);
    lblBrandSub.Location   = new Point(36, lblBrandTitle.Bottom + 10);
    lblDesc.Location       = new Point(36, lblBrandSub.Bottom + 22);

    // 2. CỘT PHẢI: FORM ĐĂNG NHẬP
    var pnlRight = new Panel
    {
        Dock = DockStyle.Fill,
        BackColor = Color.White
    };

    int x = 60;
    int w = 410;

    var lblWelcome = new Label
    {
        Text = "Chào mừng trở lại!",
        Font = new Font("Segoe UI", 20F, FontStyle.Bold),
        ForeColor = TextDark,
        AutoSize = true,
        Location = new Point(x - 2, 34)
    };

    var lblHint = new Label
    {
        Text = "Đăng nhập bằng tài khoản Độc giả hoặc Thủ thư để tiếp tục",
        Font = new Font("Segoe UI", 9.5F),
        ForeColor = TextGray,
        AutoSize = true,
        Location = new Point(x, 76)
    };

    // Nhãn & Ô nhập tài khoản
    var lblAccount = new Label
    {
        Text = "Tài khoản đăng nhập",
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        ForeColor = TextDark,
        AutoSize = true,
        Location = new Point(x, 116)
    };

    _pnlAccountBox = CreateInputContainer(x, 140, w, "\uE77B", out _txtAccount,
        "Mã độc giả, Email hoặc Tên đăng nhập...");
    _txtAccount.KeyDown += (_, e) =>
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            _txtPassword.Focus();
        }
    };

    // Nhãn & Ô nhập mật khẩu
    var lblPassword = new Label
    {
        Text = "Mật khẩu",
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        ForeColor = TextDark,
        AutoSize = true,
        Location = new Point(x, 198)
    };

    _pnlPasswordBox = CreateInputContainer(x, 222, w, "\uE72E", out _txtPassword,
        "Nhập mật khẩu của bạn...");
    _txtPassword.UseSystemPasswordChar = true;
    _txtPassword.Width = w - 82;
    _txtPassword.KeyDown += (_, e) =>
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            PerformLogin();
        }
    };

    _btnTogglePass = new Button
    {
        Text = "\uE7B3",
        Font = new Font(IconFont, 11F),
        ForeColor = TextGray,
        BackColor = BgLight,
        FlatStyle = FlatStyle.Flat,
        Size = new Size(36, 36),
        Location = new Point(w - 40, 3),
        Cursor = Cursors.Hand,
        TabStop = false
    };
    _btnTogglePass.FlatAppearance.BorderSize = 0;
    _btnTogglePass.Click += (_, _) => TogglePasswordVisibility();
    _pnlPasswordBox.Controls.Add(_btnTogglePass);

    // Link Quên mật khẩu
    var lnkForgot = new LinkLabel
    {
        Text = "Quên mật khẩu?",
        Font = new Font("Segoe UI", 9F),
        LinkColor = Primary,
        ActiveLinkColor = PrimaryDark,
        AutoSize = true,
        Location = new Point(x + w - 102, 272)
    };
    lnkForgot.LinkClicked += LnkForgot_LinkClicked;

    // Thông báo lỗi inline
    _lblError = new Label
    {
        Text = "",
        Font = new Font("Segoe UI", 8.8F),
        ForeColor = Red,
        AutoSize = false,
        Size = new Size(w, 24),
        Location = new Point(x, 298),
        Visible = false
    };

    // Nút Đăng nhập chính
    _btnLogin = new Button
    {
        Text = "ĐĂNG NHẬP",
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.White,
        BackColor = Primary,
        FlatStyle = FlatStyle.Flat,
        Size = new Size(w, 42),
        Location = new Point(x, 328),
        Cursor = Cursors.Hand
    };
    _btnLogin.FlatAppearance.BorderSize = 0;
    _btnLogin.MouseEnter += (_, _) => _btnLogin.BackColor = PrimaryDark;
    _btnLogin.MouseLeave += (_, _) => _btnLogin.BackColor = Primary;
    _btnLogin.Click += (_, _) => PerformLogin();

    // Đường phân cách "Hoặc"
    var pnlLineLeft = new Panel { BackColor = BorderGray, Size = new Size((w - 50) / 2, 1), Location = new Point(x, 392) };
    var lblOr = new Label
    {
        Text = "HOẶC",
        Font = new Font("Segoe UI", 8F, FontStyle.Bold),
        ForeColor = TextGray,
        TextAlign = ContentAlignment.MiddleCenter,
        Size = new Size(50, 18),
        Location = new Point(x + (w - 50) / 2, 383)
    };
    var pnlLineRight = new Panel { BackColor = BorderGray, Size = new Size((w - 50) / 2, 1), Location = new Point(x + (w + 50) / 2, 392) };

    // Nút Đăng ký tài khoản độc giả (Hiển thị rõ ràng, đầy đủ chiều rộng)
    var btnRegister = new Button
    {
        Text = "ĐĂNG KÝ TÀI KHOẢN ĐỘC GIẢ MỚI",
        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
        ForeColor = Primary,
        BackColor = SoftBlue,
        FlatStyle = FlatStyle.Flat,
        Size = new Size(w, 42),
        Location = new Point(x, 414),
        Cursor = Cursors.Hand
    };
    btnRegister.FlatAppearance.BorderColor = Primary;
    btnRegister.FlatAppearance.BorderSize = 1;
    btnRegister.MouseEnter += (_, _) => btnRegister.BackColor = Color.FromArgb(215, 230, 248);
    btnRegister.MouseLeave += (_, _) => btnRegister.BackColor = SoftBlue;
    btnRegister.Click += (_, _) => OpenRegisterForm();

    var lblNote = new Label
    {
        Text = "Lưu ý: Chỉ áp dụng tự đăng ký cho tài khoản Độc giả.",
        Font = new Font("Segoe UI", 8.2F, FontStyle.Italic),
        ForeColor = TextGray,
        TextAlign = ContentAlignment.MiddleCenter,
        Size = new Size(w, 20),
        Location = new Point(x, 464)
    };

    pnlRight.Controls.AddRange(new Control[]
    {
        lblWelcome, lblHint,
        lblAccount, _pnlAccountBox,
        lblPassword, _pnlPasswordBox,
        lnkForgot, _lblError, _btnLogin,
        pnlLineLeft, lblOr, pnlLineRight,
        btnRegister, lblNote
    });

    Controls.Add(pnlRight);
    Controls.Add(pnlBrand);

    AcceptButton = _btnLogin;
    ResumeLayout(true);
}

    private void OpenRegisterForm()
    {
        using var registerForm = new RegisterForm.RegisterForm();
        if (registerForm.ShowDialog(this) == DialogResult.OK)
        {
            _txtAccount.Text = registerForm.RegisteredEmail;
            _txtPassword.Clear();
            _lblError.Visible = false;
            _txtPassword.Focus();
        }
    }

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

    private void PnlBrand_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Vẽ họa tiết vòng tròn trang trí mờ hiện đại ở góc dưới
        using var circleBrush1 = new SolidBrush(Color.FromArgb(18, 255, 255, 255));
        using var circleBrush2 = new SolidBrush(Color.FromArgb(12, 255, 255, 255));
        g.FillEllipse(circleBrush1, -60, 320, 260, 260);
        g.FillEllipse(circleBrush2, 180, -50, 240, 240);
    }

    private void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
        _txtPassword.UseSystemPasswordChar = !_isPasswordVisible;
        _btnTogglePass.Text = _isPasswordVisible ? "\uED1A" : "\uE7B3";
        _btnTogglePass.ForeColor = _isPasswordVisible ? Primary : TextGray;
    }

    // ============================================================
    //  KHỞI TẠO DỮ LIỆU & ĐĂNG NHẬP PHÂN QUYỀN TỰ ĐỘNG
    // ============================================================
    private void LoginForm_Load(object? sender, EventArgs e)
    {
        try
        {
            using var context = new LibraryDbContext();

            if (!context.Users.Any(u => u.Username == "admin"))
            {
                context.Users.Add(new Users
                {
                    UserCode = "NV000",
                    Username = "admin",
                    PasswordHash = "123456",
                    FullName = "Thủ thư",
                    UserType = UserType.Librarian,
                    MaxActiveCopies = null,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khởi tạo cơ sở dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        _txtAccount.Focus();
    }

    private void PerformLogin()
    {
        string input = _txtAccount.Text.Trim();
        string password = _txtPassword.Text;

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(password))
        {
            ShowError("Vui lòng nhập đầy đủ tài khoản và mật khẩu.");
            if (string.IsNullOrEmpty(input)) _txtAccount.Focus();
            else _txtPassword.Focus();
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            string inputLower = input.ToLower();

            // Tìm người dùng khớp bất kỳ định danh nào (Username, UserCode hoặc Email)
            var user = context.Users.FirstOrDefault(u =>
                u.PasswordHash == password &&
                ((u.Username != null && u.Username.ToLower() == inputLower) ||
                 u.UserCode.ToLower() == inputLower ||
                 (u.Email != null && u.Email.ToLower() == inputLower)));

            if (user == null)
            {
                ShowError("Thông tin đăng nhập hoặc mật khẩu không chính xác.");
                _txtPassword.SelectAll();
                _txtPassword.Focus();
                return;
            }

            if (user.Status != UserStatus.Active)
            {
                ShowError("Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên thư viện.");
                return;
            }

            // Phân quyền điều hướng tự động theo UserType
            if (user.UserType == UserType.Librarian)
            {
                Session.Clear();
                Session.CurrentLibrarianId = user.Id;

                var librarianForm = new LibrarianFormMain.LibrarianFormMain();
                librarianForm.FormClosed += (_, _) => AfterClose(librarianForm.IsLogout);
                Hide();
                librarianForm.Show();
            }
            else if (user.UserType == UserType.Reader)
            {
                Session.Clear();
                Session.CurrentReaderId = user.Id;

                var homePage = new HomePage.HomePage();
                homePage.FormClosed += (_, _) => AfterClose(homePage.IsLogout);
                Hide();
                homePage.Show();
            }
            else
            {
                ShowError("Tài khoản chưa được cấp quyền truy cập hệ thống.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu:\n{ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowError(string message)
    {
        _lblError.Text = "⚠  " + message;
        _lblError.Visible = true;
    }

    private void LnkRegister_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using var registerForm = new RegisterForm.RegisterForm();
        if (registerForm.ShowDialog(this) == DialogResult.OK)
        {
            _txtAccount.Text = registerForm.RegisteredEmail;
            _txtPassword.Clear();
            _lblError.Visible = false;
            _txtPassword.Focus();
        }
    }

    private void LnkForgot_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using var forgotForm = new ForgotPasswordForm.ForgotPasswordForm();
        forgotForm.ShowDialog(this);
    }

    private void AfterClose(bool logout)
    {
        if (logout)
        {
            _txtPassword.Clear();
            _lblError.Visible = false;
            Session.Clear();
            Show();
            _txtAccount.Focus();
        }
        else
        {
            Close();
        }
    }
}