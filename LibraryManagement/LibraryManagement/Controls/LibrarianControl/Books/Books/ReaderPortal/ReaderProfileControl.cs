using System.Drawing.Drawing2D;
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Helpers;

namespace LibraryManagement.Forms.Books.ReaderPortal;

public class ReaderProfileControl : UserControl
{
    private static readonly Color Primary = Color.FromArgb(19, 98, 160);
    private static readonly Color PrimaryDark = Color.FromArgb(14, 76, 126);
    private static readonly Color SoftBlue = Color.FromArgb(232, 240, 250);
    private static readonly Color BgGray = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);
    private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
    private static readonly Color Green = Color.FromArgb(34, 160, 90);
    private static readonly Color Red = Color.FromArgb(200, 50, 50);

    public event Action? OnProfileUpdated;

    private Panel _picAvatar = null!;
    private Label _lblDisplayName = null!;
    private Label _lblDisplayMeta = null!;
    private Label _lblCardCode = null!;
    private Label _lblCardDates = null!;
    private Label _lblCardStatus = null!;
    private Label _lblQuotaInfo = null!;

    private TextBox _txtUserCode = null!;
    private TextBox _txtUsername = null!;
    private TextBox _txtFullName = null!;
    private TextBox _txtDateOfBirth = null!;
    private TextBox _txtPhone = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtAddress = null!;

    private TextBox _txtCurrentPass = null!;
    private TextBox _txtNewPass = null!;
    private TextBox _txtConfirmPass = null!;

    public ReaderProfileControl()
    {
        Dock = DockStyle.Fill;
        BackColor = BgGray;
        Padding = new Padding(12, 0, 12, 12);

        BuildUi();
        ReloadData();
    }

    private void BuildUi()
    {
        SuspendLayout();

        // 1. Thẻ thông tin tổng quan & Thẻ thư viện ở trên
        var headerCard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 116,
            BackColor = Color.White
        };
        headerCard.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, headerCard.Width - 1, headerCard.Height - 1);

        _picAvatar = new Panel
        {
            Size = new Size(68, 68),
            Location = new Point(20, 24)
        };
        _picAvatar.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var br = new SolidBrush(Primary);
            e.Graphics.FillEllipse(br, 0, 0, _picAvatar.Width - 1, _picAvatar.Height - 1);
            TextRenderer.DrawText(e.Graphics, GetInitials(_lblDisplayName.Text),
                new Font("Segoe UI", 16F, FontStyle.Bold),
                new Rectangle(0, 0, _picAvatar.Width, _picAvatar.Height), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        _lblDisplayName = new Label
        {
            Text = "Độc giả",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 32, 42),
            AutoSize = false,
            AutoEllipsis = true,
            Size = new Size(460, 28),
            Location = new Point(104, 22)
        };

        _lblDisplayMeta = new Label
        {
            Text = "Mã độc giả: DG000 · Tài khoản: reader",
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            AutoSize = false,
            AutoEllipsis = true,
            Size = new Size(460, 20),
            Location = new Point(105, 52)
        };

        _lblQuotaInfo = new Label
        {
            Text = "Hạn mức mượn tối đa: 3 cuốn",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Primary,
            BackColor = SoftBlue,
            AutoSize = true,
            Padding = new Padding(8, 3, 8, 3),
            Location = new Point(105, 76)
        };

        // Khung thẻ thư viện bên phải banner
        var cardBox = new Panel
        {
            Size = new Size(340, 88),
            Location = new Point(headerCard.Width - 358, 14),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = PrimaryDark
        };

        _lblCardCode = new Label
        {
            Text = "THẺ ĐỘC GIẢ: CHƯA CẤP THẺ",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(14, 12)
        };

        _lblCardDates = new Label
        {
            Text = "Ngày cấp: —   |   Hết hạn: —",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(215, 232, 248),
            AutoSize = true,
            Location = new Point(14, 38)
        };

        _lblCardStatus = new Label
        {
            Text = "● Chưa kích hoạt",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(145, 245, 185),
            AutoSize = true,
            Location = new Point(14, 61)
        };

        cardBox.Controls.AddRange(new Control[] { _lblCardCode, _lblCardDates, _lblCardStatus });
        headerCard.Controls.AddRange(new Control[] { _picAvatar, _lblDisplayName, _lblDisplayMeta, _lblQuotaInfo, cardBox });

        void LayoutHeaderLabels()
        {
            int infoWidth = Math.Max(120, cardBox.Left - _lblDisplayName.Left - 12);
            _lblDisplayName.Width = infoWidth;
            _lblDisplayMeta.Width = infoWidth;
        }
        headerCard.Resize += (_, _) => LayoutHeaderLabels();
        LayoutHeaderLabels();

        // 2. Khu vực chính chia 2 cột: Trái (Hồ sơ cá nhân) - Phải (Đổi mật khẩu)
        var bodyHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 12, 0, 0)
        };

        var tblCols = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        tblCols.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        tblCols.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

        // --- Cột trái: Thông tin cá nhân ---
        var pnlLeft = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 10, 0),
            Padding = new Padding(18)
        };
        pnlLeft.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlLeft.Width - 1, pnlLeft.Height - 1);

        var lblLeftTitle = new Label
        {
            Text = "Thông tin cá nhân độc giả",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 38, 48),
            AutoSize = true,
            Location = new Point(18, 14)
        };

        int y = 52;
        _txtUserCode = AddFormRow(pnlLeft, "Mã độc giả:", ref y, readOnly: true);
        _txtUsername = AddFormRow(pnlLeft, "Tên đăng nhập:", ref y, readOnly: true);
        _txtFullName = AddFormRow(pnlLeft, "Họ và tên (*):", ref y, readOnly: false);
        _txtDateOfBirth = AddFormRow(pnlLeft, "Ngày sinh (yyyy-MM-dd):", ref y, readOnly: false);
        _txtPhone = AddFormRow(pnlLeft, "Số điện thoại:", ref y, readOnly: false);
        _txtEmail = AddFormRow(pnlLeft, "Email:", ref y, readOnly: false);
        _txtAddress = AddFormRow(pnlLeft, "Địa chỉ liên hệ:", ref y, readOnly: false);
        pnlLeft.Resize += (_, _) => ResizeInputs(pnlLeft,
            _txtUserCode, _txtUsername, _txtFullName, _txtDateOfBirth, _txtPhone, _txtEmail, _txtAddress);

        var btnSaveProfile = new Button
        {
            Text = "Lưu thông tin cá nhân",
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Size = new Size(195, 36),
            Location = new Point(185, y + 8),
            Cursor = Cursors.Hand
        };
        btnSaveProfile.FlatAppearance.BorderSize = 0;
        btnSaveProfile.Click += (_, _) => SaveProfile();

        pnlLeft.Controls.Add(lblLeftTitle);
        pnlLeft.Controls.Add(btnSaveProfile);

        // --- Cột phải: Đổi mật khẩu ---
        var pnlRight = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(2, 0, 0, 0),
            Padding = new Padding(18)
        };
        pnlRight.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlRight.Width - 1, pnlRight.Height - 1);

        var lblRightTitle = new Label
        {
            Text = "Đổi mật khẩu đăng nhập",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 38, 48),
            AutoSize = true,
            Location = new Point(18, 14)
        };

        var lblPassHint = new Label
        {
            Text = "Mật khẩu mới cần tối thiểu 6 ký tự và không trùng mật khẩu cũ.",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextGray,
            AutoSize = false,
            Size = new Size(340, 34),
            Location = new Point(18, 42)
        };

        int py = 86;
        _txtCurrentPass = AddPasswordRow(pnlRight, "Mật khẩu hiện tại:", ref py);
        _txtNewPass = AddPasswordRow(pnlRight, "Mật khẩu mới:", ref py);
        _txtConfirmPass = AddPasswordRow(pnlRight, "Xác nhận mật khẩu mới:", ref py);
        void LayoutPasswordPanel()
        {
            lblPassHint.Width = Math.Max(120, pnlRight.ClientSize.Width - 36);
            ResizeInputs(pnlRight, _txtCurrentPass, _txtNewPass, _txtConfirmPass);
        }
        pnlRight.Resize += (_, _) => LayoutPasswordPanel();
        LayoutPasswordPanel();

        var chkShowPass = new CheckBox
        {
            Text = "Hiển thị mật khẩu",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(18, py)
        };
        chkShowPass.CheckedChanged += (_, _) =>
        {
            char mask = chkShowPass.Checked ? '\0' : '●';
            _txtCurrentPass.PasswordChar = mask;
            _txtNewPass.PasswordChar = mask;
            _txtConfirmPass.PasswordChar = mask;
        };

        var btnChangePass = new Button
        {
            Text = "Cập nhật mật khẩu",
            FlatStyle = FlatStyle.Flat,
            BackColor = PrimaryDark,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Size = new Size(185, 36),
            Location = new Point(18, py + 34),
            Cursor = Cursors.Hand
        };
        btnChangePass.FlatAppearance.BorderSize = 0;
        btnChangePass.Click += (_, _) => ChangePassword();

        pnlRight.Controls.AddRange(new Control[] { lblRightTitle, lblPassHint, chkShowPass, btnChangePass });

        tblCols.Controls.Add(pnlLeft, 0, 0);
        tblCols.Controls.Add(pnlRight, 1, 0);
        bodyHost.Controls.Add(tblCols);

        Controls.Add(bodyHost);
        Controls.Add(headerCard);

        ResumeLayout(true);
    }

    public void ReloadData()
    {
        if (Session.CurrentReaderId <= 0) return;

        try
        {
            using var context = new LibraryDbContext();
            var user = context.Users.FirstOrDefault(u => u.Id == Session.CurrentReaderId);
            if (user == null) return;

            _lblDisplayName.Text = user.FullName;
            _lblDisplayMeta.Text = $"Mã độc giả: {user.UserCode}  ·  Tên đăng nhập: {user.Username ?? "Chưa có"}";
            _lblQuotaInfo.Text = $"Hạn mức mượn tối đa: {user.MaxActiveCopies ?? 3} cuốn";
            _picAvatar.Invalidate();

            _txtUserCode.Text = user.UserCode;
            _txtUsername.Text = user.Username ?? "";
            _txtFullName.Text = user.FullName;
            _txtDateOfBirth.Text = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? "";
            _txtPhone.Text = user.Phone ?? "";
            _txtEmail.Text = user.Email ?? "";
            _txtAddress.Text = user.Address ?? "";

            var card = context.ReaderCards
                .Where(c => c.IdReader == user.Id)
                .OrderByDescending(c => c.ExpiresAt)
                .FirstOrDefault();

            if (card != null)
            {
                _lblCardCode.Text = $"THẺ ĐỘC GIẢ: {card.CardCode}";
                _lblCardDates.Text = $"Ngày cấp: {card.IssuedAt:dd/MM/yyyy}   |   Hết hạn: {card.ExpiresAt:dd/MM/yyyy}";
                bool isValid = card.Status == ReaderCardStatus.Active && card.ExpiresAt.Date >= DateTime.Today;
                _lblCardStatus.Text = isValid
                    ? "● Đang hoạt động"
                    : $"● {FormatCardStatus(card.Status)}";
                _lblCardStatus.ForeColor = isValid
                    ? Color.FromArgb(145, 245, 185)
                    : Color.FromArgb(255, 185, 185);
            }
            else
            {
                _lblCardCode.Text = "THẺ ĐỘC GIẢ: CHƯA CẤP THẺ";
                _lblCardDates.Text = "Liên hệ thủ thư tại quầy để kích hoạt thẻ mượn sách";
                _lblCardStatus.Text = "● Chưa có thẻ";
                _lblCardStatus.ForeColor = Color.FromArgb(255, 215, 140);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải thông tin tài khoản:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveProfile()
    {
        if (Session.CurrentReaderId <= 0) return;

        string fullName = _txtFullName.Text.Trim();
        if (string.IsNullOrEmpty(fullName))
        {
            MessageBox.Show("Họ và tên không được để trống!", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtFullName.Focus();
            return;
        }

        string dob = _txtDateOfBirth.Text.Trim();
        if (!string.IsNullOrEmpty(dob) && !DateTime.TryParse(dob, out _))
        {
            MessageBox.Show("Ngày sinh không đúng định dạng (ví dụ: 2004-08-15)!", "Sai định dạng",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtDateOfBirth.Focus();
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            var user = context.Users.Find(Session.CurrentReaderId);
            if (user == null) return;

            user.FullName = fullName;
            user.DateOfBirth = string.IsNullOrEmpty(dob) ? null : Convert.ToDateTime(dob);
            user.Phone = string.IsNullOrWhiteSpace(_txtPhone.Text) ? null : _txtPhone.Text.Trim();
            user.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
            user.Address = string.IsNullOrWhiteSpace(_txtAddress.Text) ? null : _txtAddress.Text.Trim();
            user.UpdatedAt = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            context.SaveChanges();

            MessageBox.Show("Cập nhật thông tin cá nhân thành công!", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            ReloadData();
            OnProfileUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể lưu thông tin:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ChangePassword()
    {
        if (Session.CurrentReaderId <= 0) return;

        string currentPass = _txtCurrentPass.Text;
        string newPass = _txtNewPass.Text;
        string confirmPass = _txtConfirmPass.Text;

        if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới!", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (newPass.Length < 6)
        {
            MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự!", "Mật khẩu yếu",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNewPass.Focus();
            return;
        }

        if (newPass != confirmPass)
        {
            MessageBox.Show("Xác nhận mật khẩu mới không khớp!", "Lỗi xác nhận",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _txtConfirmPass.Focus();
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            var user = context.Users.Find(Session.CurrentReaderId);
            if (user == null) return;

            if (user.PasswordHash != currentPass)
            {
                MessageBox.Show("Mật khẩu hiện tại không chính xác!", "Sai mật khẩu",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _txtCurrentPass.Focus();
                return;
            }

            user.PasswordHash = newPass;
            user.UpdatedAt = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            context.SaveChanges();

            _txtCurrentPass.Clear();
            _txtNewPass.Clear();
            _txtConfirmPass.Clear();

            MessageBox.Show("Đổi mật khẩu thành công!", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi đổi mật khẩu:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static TextBox AddFormRow(Panel host, string labelText, ref int y, bool readOnly)
    {
        var lbl = new Label
        {
            Text = labelText,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(65, 72, 82),
            AutoSize = true,
            Location = new Point(18, y + 4)
        };

        var txt = new TextBox
        {
            Font = new Font("Segoe UI", 9.5F),
            ReadOnly = readOnly,
            BackColor = readOnly ? Color.FromArgb(245, 247, 250) : Color.White,
            Size = new Size(340, 26),
            Location = new Point(185, y),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        host.Controls.Add(lbl);
        host.Controls.Add(txt);
        y += 38;
        return txt;
    }

    private static TextBox AddPasswordRow(Panel host, string labelText, ref int y)
    {
        var lbl = new Label
        {
            Text = labelText,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(65, 72, 82),
            AutoSize = true,
            Location = new Point(18, y)
        };

        var txt = new TextBox
        {
            Font = new Font("Segoe UI", 9.5F),
            PasswordChar = '●',
            Size = new Size(320, 26),
            Location = new Point(18, y + 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        host.Controls.Add(lbl);
        host.Controls.Add(txt);
        y += 58;
        return txt;
    }

    private static void ResizeInputs(Panel host, params TextBox[] inputs)
    {
        int width = Math.Max(120, host.ClientSize.Width - inputs[0].Left - 18);
        foreach (var input in inputs)
            input.Width = width;
    }

    private static string FormatCardStatus(ReaderCardStatus status) => status switch
    {
        ReaderCardStatus.Active => "Đang hoạt động",
        ReaderCardStatus.Expired => "Thẻ đã hết hạn",
        ReaderCardStatus.Locked => "Thẻ đang bị khóa",
        ReaderCardStatus.Missing => "Thẻ bị mất",
        _ => status.ToString()
    };

    private static string GetInitials(string name)
    {
        var p = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return p.Length == 0 ? "DG" : (p[0][0].ToString() + p[^1][0]).ToUpper();
    }
}