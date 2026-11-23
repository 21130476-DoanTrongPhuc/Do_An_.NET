using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Forms.Readers;

public class ReaderManagementControl : UserControl
{
    private TextBox _txtSearch = null!;
    private ComboBox _cboStatusFilter = null!;
    private DataGridView _dgvReaders = null!;
    private Label _lblCount = null!;

    public event Action? ReaderDataChanged;

    public ReaderManagementControl()
    {
        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(10);

        // Thanh công cụ nghiệp vụ
        var pnlTop = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Color.White,
            Padding = new Padding(8, 6, 8, 6)
        };

        var btnAdd = MakeBtn("Thêm độc giả mới", Color.FromArgb(19, 98, 160), Color.White);
        var btnEdit = MakeBtn("Chỉnh sửa thông tin", Color.FromArgb(245, 247, 250), Color.Black);
        var btnToggleStatus = MakeBtn("Khóa / Mở khóa tài khoản", Color.FromArgb(255, 244, 229), Color.FromArgb(180, 100, 0));

        btnAdd.Click += (_, _) => OpenReaderDialog(null);
        btnEdit.Click += (_, _) =>
        {
            if (GetSelectedReaderId() is { } id) OpenReaderDialog(id);
        };
        btnToggleStatus.Click += BtnToggleStatus_Click;

        pnlTop.Controls.AddRange([btnAdd, btnEdit, btnToggleStatus]);

        // Thanh tìm kiếm & lọc
        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            Padding = new Padding(0, 8, 0, 4)
        };

        _txtSearch = new TextBox { Width = 320, PlaceholderText = "Tìm theo mã độc giả, họ tên, SĐT, email, mã thẻ..." };
        _txtSearch.TextChanged += (_, _) => LoadData();

        _cboStatusFilter = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboStatusFilter.Items.AddRange(["Tất cả trạng thái", "Đang hoạt động", "Đã khóa"]);
        _cboStatusFilter.SelectedIndex = 0;
        _cboStatusFilter.SelectedIndexChanged += (_, _) => LoadData();

        pnlFilter.Controls.Add(new Label { Text = "Từ khóa:", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        pnlFilter.Controls.Add(_txtSearch);
        pnlFilter.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Padding = new Padding(12, 6, 4, 0) });
        pnlFilter.Controls.Add(_cboStatusFilter);

        _dgvReaders = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            RowTemplate = { Height = 32 }
        };

        _dgvReaders.Columns.Add("colCode", "Mã độc giả");
        _dgvReaders.Columns.Add("colName", "Họ và tên");
        _dgvReaders.Columns.Add("colPhone", "Số điện thoại");
        _dgvReaders.Columns.Add("colEmail", "Email");
        _dgvReaders.Columns.Add("colCard", "Mã thẻ thư viện");
        _dgvReaders.Columns.Add("colExpire", "Hạn thẻ");
        _dgvReaders.Columns.Add("colMaxCopies", "Số sách tối đa");
        _dgvReaders.Columns.Add("colStatus", "Trạng thái");

        _dgvReaders.CellDoubleClick += (_, _) =>
        {
            if (GetSelectedReaderId() is { } id) OpenReaderDialog(id);
        };

        _lblCount = new Label { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleRight };

        Controls.Add(_dgvReaders);
        Controls.Add(_lblCount);
        Controls.Add(pnlFilter);
        Controls.Add(pnlTop);
    }

    private static Button MakeBtn(string text, Color back, Color fore) => new()
    {
        Text = text,
        BackColor = back,
        ForeColor = fore,
        FlatStyle = FlatStyle.Flat,
        Height = 32,
        AutoSize = true,
        Padding = new Padding(8, 0, 8, 0),
        Margin = new Padding(0, 0, 8, 0),
        Cursor = Cursors.Hand
    };

    public void LoadData()
    {
        try
        {
            using var context = new LibraryDbContext();
            string kw = _txtSearch.Text.Trim().ToLower();
            int statusFilter = _cboStatusFilter.SelectedIndex;

            var readers = context.Users
                .Where(u => u.UserType == "Reader")
                .Select(u => new
                {
                    u.Id,
                    u.UserCode,
                    u.FullName,
                    u.Phone,
                    u.Email,
                    u.MaxActiveCopies,
                    u.Status,
                    ActiveCard = context.ReaderCards
                        .Where(rc => rc.IdReader == u.Id)
                        .OrderByDescending(rc => rc.Status == "Active")
                        .ThenByDescending(rc => rc.Id)
                        .Select(rc => new { rc.CardCode, rc.ExpiresAt, rc.Status })
                        .FirstOrDefault()
                })
                .ToList();

            var filtered = readers.Where(r =>
            {
                if (statusFilter == 1 && r.Status != "Active") return false;
                if (statusFilter == 2 && r.Status != "Inactive") return false;

                if (!string.IsNullOrEmpty(kw))
                {
                    bool match = r.UserCode.ToLower().Contains(kw)
                                 || r.FullName.ToLower().Contains(kw)
                                 || (r.Phone != null && r.Phone.ToLower().Contains(kw))
                                 || (r.Email != null && r.Email.ToLower().Contains(kw))
                                 || (r.ActiveCard != null && r.ActiveCard.CardCode.ToLower().Contains(kw));
                    if (!match) return false;
                }
                return true;
            }).OrderByDescending(r => r.Id).ToList();

            _dgvReaders.Rows.Clear();
            foreach (var r in filtered)
            {
                string statusText = r.Status == "Active" ? "Hoạt động" : "Đã khóa";
                string cardCode = r.ActiveCard?.CardCode ?? "Chưa cấp thẻ";
                string expireText = r.ActiveCard != null ? r.ActiveCard.ExpiresAt.ToString("dd/MM/yyyy") : "-";

                int idx = _dgvReaders.Rows.Add(
                    r.UserCode,
                    r.FullName,
                    r.Phone ?? "-",
                    r.Email ?? "-",
                    cardCode,
                    expireText,
                    r.MaxActiveCopies ?? 5,
                    statusText
                );
                _dgvReaders.Rows[idx].Tag = r.Id;
                if (r.Status != "Active")
                    _dgvReaders.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
            }

            _lblCount.Text = $"Tổng số độc giả: {filtered.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh sách độc giả:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private int? GetSelectedReaderId()
    {
        if (_dgvReaders.CurrentRow?.Tag is int id) return id;
        MessageBox.Show("Vui lòng chọn một độc giả trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void OpenReaderDialog(int? readerId)
    {
        using var dlg = new ReaderEditDialog(readerId);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            LoadData();
            ReaderDataChanged?.Invoke();
        }
    }

    private void BtnToggleStatus_Click(object? sender, EventArgs e)
    {
        if (GetSelectedReaderId() is not { } id) return;

        try
        {
            using var context = new LibraryDbContext();
            var user = context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return;

            user.Status = user.Status == "Active" ? "Inactive" : "Active";
            user.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            context.SaveChanges();

            LoadData();
            ReaderDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể đổi trạng thái độc giả:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

public sealed class ReaderEditDialog : Form
{
    private readonly int? _readerId;

    private TextBox _txtUserCode = null!;
    private TextBox _txtFullName = null!;
    private DateTimePicker _dtpDob = null!;
    private CheckBox _chkHasDob = null!;
    private TextBox _txtPhone = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtAddress = null!;
    private NumericUpDown _numMaxCopies = null!;

    private TextBox _txtUsername = null!;
    private TextBox _txtPassword = null!;

    private TextBox _txtCardCode = null!;
    private DateTimePicker _dtpIssuedAt = null!;
    private DateTimePicker _dtpExpiresAt = null!;

    public ReaderEditDialog(int? readerId)
    {
        _readerId = readerId;
        Text = readerId == null ? "Thêm mới độc giả & Cấp thẻ thư viện" : "Cập nhật thông tin độc giả";
        Size = new Size(560, 590);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi();
        LoadFormData();
    }

    private void BuildUi()
    {
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(16)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _txtUserCode = new TextBox { Dock = DockStyle.Fill };
        _txtFullName = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Nhập họ và tên đầy đủ" };

        var pnlDob = new FlowLayoutPanel { Dock = DockStyle.Fill, Margin = new Padding(0) };
        _chkHasDob = new CheckBox { Text = "Có ngày sinh", Checked = true, AutoSize = true, Padding = new Padding(0, 4, 8, 0) };
        _dtpDob = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 140, Value = new DateTime(2002, 1, 1) };
        _chkHasDob.CheckedChanged += (_, _) => _dtpDob.Enabled = _chkHasDob.Checked;
        pnlDob.Controls.AddRange([_chkHasDob, _dtpDob]);

        _txtPhone = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ví dụ: 0901234567" };
        _txtEmail = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ví dụ: docgia@gmail.com" };
        _txtAddress = new TextBox { Dock = DockStyle.Fill };
        _numMaxCopies = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 5, Width = 100 };

        _txtUsername = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Để trống nếu không tạo tài khoản cổng độc giả" };
        _txtPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, PlaceholderText = "Mật khẩu đăng nhập" };

        _txtCardCode = new TextBox { Dock = DockStyle.Fill };
        _dtpIssuedAt = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 140, Value = DateTime.Today };
        _dtpExpiresAt = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 140, Value = DateTime.Today.AddYears(1) };

        int r = 0;
        AddRow(tbl, r++, "Mã độc giả (*):", _txtUserCode);
        AddRow(tbl, r++, "Họ và tên (*):", _txtFullName);
        AddRow(tbl, r++, "Ngày sinh:", pnlDob);
        AddRow(tbl, r++, "Số điện thoại:", _txtPhone);
        AddRow(tbl, r++, "Email:", _txtEmail);
        AddRow(tbl, r++, "Địa chỉ:", _txtAddress);
        AddRow(tbl, r++, "Số sách mượn tối đa (*):", _numMaxCopies);
        AddRow(tbl, r++, "Tên đăng nhập:", _txtUsername);
        AddRow(tbl, r++, "Mật khẩu:", _txtPassword);
        AddRow(tbl, r++, "Mã thẻ thư viện (*):", _txtCardCode);
        AddRow(tbl, r++, "Ngày cấp thẻ:", _dtpIssuedAt);
        AddRow(tbl, r++, "Ngày hết hạn thẻ:", _dtpExpiresAt);

        var pnlFooter = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 6, 16, 6)
        };
        var btnCancel = new Button { Text = "Hủy", Width = 90, Height = 32 };
        var btnSave = new Button
        {
            Text = "Lưu độc giả",
            Width = 120,
            Height = 32,
            BackColor = Color.FromArgb(19, 98, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        btnSave.Click += BtnSave_Click;

        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnSave);

        Controls.Add(tbl);
        Controls.Add(pnlFooter);
    }

    private static void AddRow(TableLayoutPanel tbl, int row, string label, Control c)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
        tbl.Controls.Add(c, 1, row);
    }

    private void LoadFormData()
    {
        using var context = new LibraryDbContext();

        if (_readerId == null)
        {
            int nextUserNum = context.Users.Count() + 1;
            string candidateUserCode;
            do
            {
                candidateUserCode = $"RD{nextUserNum:D4}";
                nextUserNum++;
            } while (context.Users.Any(u => u.UserCode == candidateUserCode));
            _txtUserCode.Text = candidateUserCode;

            int nextCardNum = context.ReaderCards.Count() + 1;
            string candidateCardCode;
            do
            {
                candidateCardCode = $"CARD{nextCardNum:D4}";
                nextCardNum++;
            } while (context.ReaderCards.Any(rc => rc.CardCode == candidateCardCode));
            _txtCardCode.Text = candidateCardCode;
            return;
        }

        var user = context.Users.FirstOrDefault(u => u.Id == _readerId);
        if (user == null) return;

        _txtUserCode.Text = user.UserCode;
        _txtFullName.Text = user.FullName;
        if (DateTime.TryParse(user.DateOfBirth, out var dob))
        {
            _chkHasDob.Checked = true;
            _dtpDob.Value = dob;
        }
        else
        {
            _chkHasDob.Checked = false;
        }

        _txtPhone.Text = user.Phone;
        _txtEmail.Text = user.Email;
        _txtAddress.Text = user.Address;
        _numMaxCopies.Value = user.MaxActiveCopies ?? 5;
        _txtUsername.Text = user.Username;

        var card = context.ReaderCards
            .Where(rc => rc.IdReader == user.Id)
            .OrderByDescending(rc => rc.Status == "Active")
            .ThenByDescending(rc => rc.Id)
            .FirstOrDefault();

        if (card != null)
        {
            _txtCardCode.Text = card.CardCode;
            _dtpIssuedAt.Value = card.IssuedAt;
            _dtpExpiresAt.Value = card.ExpiresAt;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        string userCode = _txtUserCode.Text.Trim();
        string fullName = _txtFullName.Text.Trim();
        string cardCode = _txtCardCode.Text.Trim();
        string username = _txtUsername.Text.Trim();
        string password = _txtPassword.Text.Trim();

        if (string.IsNullOrEmpty(userCode) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(cardCode))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ Mã độc giả, Họ tên và Mã thẻ thư viện.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_dtpExpiresAt.Value.Date < _dtpIssuedAt.Value.Date)
        {
            MessageBox.Show("Ngày hết hạn thẻ phải lớn hơn hoặc bằng ngày cấp thẻ.", "Lỗi ngày tháng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!string.IsNullOrEmpty(username) && _readerId == null && string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Vui lòng nhập mật khẩu nếu bạn cấp tên đăng nhập cho độc giả.", "Thiếu mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var context = new LibraryDbContext();
            using var tx = context.Database.BeginTransaction();

            int currentReaderId = _readerId ?? 0;

            if (context.Users.Any(u => u.UserCode == userCode && u.Id != currentReaderId))
            {
                MessageBox.Show($"Mã độc giả \"{userCode}\" đã tồn tại.", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(username) && context.Users.Any(u => u.Username == username && u.Id != currentReaderId))
            {
                MessageBox.Show($"Tên đăng nhập \"{username}\" đã được sử dụng.", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Users user;
            if (_readerId == null)
            {
                user = new Users
                {
                    UserCode = userCode,
                    FullName = fullName,
                    DateOfBirth = _chkHasDob.Checked ? _dtpDob.Value.ToString("yyyy-MM-dd") : null,
                    Phone = string.IsNullOrWhiteSpace(_txtPhone.Text) ? null : _txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(_txtAddress.Text) ? null : _txtAddress.Text.Trim(),
                    UserType = "Reader",
                    MaxActiveCopies = (int)_numMaxCopies.Value,
                    Status = "Active",
                    Username = string.IsNullOrEmpty(username) ? null : username,
                    PasswordHash = string.IsNullOrEmpty(username) ? null : password,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
            else
            {
                user = context.Users.First(u => u.Id == _readerId);
                user.UserCode = userCode;
                user.FullName = fullName;
                user.DateOfBirth = _chkHasDob.Checked ? _dtpDob.Value.ToString("yyyy-MM-dd") : null;
                user.Phone = string.IsNullOrWhiteSpace(_txtPhone.Text) ? null : _txtPhone.Text.Trim();
                user.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
                user.Address = string.IsNullOrWhiteSpace(_txtAddress.Text) ? null : _txtAddress.Text.Trim();
                user.MaxActiveCopies = (int)_numMaxCopies.Value;

                if (string.IsNullOrEmpty(username))
                {
                    user.Username = null;
                    user.PasswordHash = null;
                }
                else
                {
                    user.Username = username;
                    if (!string.IsNullOrEmpty(password))
                        user.PasswordHash = password;
                    else if (string.IsNullOrEmpty(user.PasswordHash))
                        user.PasswordHash = "123456";
                }
                user.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                context.SaveChanges();
            }

            var existingCard = context.ReaderCards
                .Where(rc => rc.IdReader == user.Id)
                .OrderByDescending(rc => rc.Status == "Active")
                .ThenByDescending(rc => rc.Id)
                .FirstOrDefault();

            int currentCardId = existingCard != null ? existingCard.Id : 0;

            if (context.ReaderCards.Any(rc => rc.CardCode == cardCode && rc.Id != currentCardId))
            {
                MessageBox.Show($"Mã thẻ thư viện \"{cardCode}\" đã tồn tại.", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (existingCard == null)
            {
                context.ReaderCards.Add(new ReaderCards
                {
                    IdReader = user.Id,
                    CardCode = cardCode,
                    IssuedAt = _dtpIssuedAt.Value.Date,
                    ExpiresAt = _dtpExpiresAt.Value.Date,
                    Status = "Active"
                });
            }
            else
            {
                existingCard.CardCode = cardCode;
                existingCard.IssuedAt = _dtpIssuedAt.Value.Date;
                existingCard.ExpiresAt = _dtpExpiresAt.Value.Date;
            }

            context.SaveChanges();
            tx.Commit();

            MessageBox.Show("Lưu thông tin độc giả thành công!", "LIBRA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu độc giả:\n{ex.InnerException?.Message ?? ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}