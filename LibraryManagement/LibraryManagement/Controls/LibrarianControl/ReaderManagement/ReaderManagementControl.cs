using System.Drawing.Drawing2D;
using LibraryManagement.Data;
using LibraryManagement.Enums;

namespace LibraryManagement.Forms.ReaderManagement
{
    public partial class ReaderManagementControl : UserControl
    {
        // ====== Bảng màu đồng bộ với LibrarianFormMain ======
        private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
        private static readonly Color BgGray     = Color.FromArgb(240, 242, 245);
        private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
        private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
        private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
        private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
        private static readonly Color Green      = Color.FromArgb(34, 160, 90);
        private static readonly Color Red        = Color.FromArgb(200, 40, 40);
        private const string IconFont = "Segoe MDL2 Assets";

        // Glyph
        private const string GPeople   = "\uE716";
        private const string GAddUser  = "\uE8FA";
        private const string GRefresh  = "\uE72C";
        private const string GSearch   = "\uE721";
        private const string GSave     = "\uE74E";
        private const string GLock     = "\uE72E";
        private const string GUnlock   = "\uE785";
        private const string GCalendar = "\uE787";

        public event Action? ReaderDataChanged;

        // ====== Controls ======
        private TextBox _txtSearch = null!;
        private ComboBox _cboFilterStatus = null!;
        private DataGridView _dgvReaders = null!;
        private Label _lblSummary = null!;

        // Form chi tiết bên phải
        private int? _selectedReaderId;
        private TextBox _txtCode = null!;
        private TextBox _txtFullName = null!;
        private TextBox _txtEmail = null!;
        private TextBox _txtPhone = null!;
        private Label _lblCardInfo = null!;
        private Label _lblBorrowStats = null!;
        private ComboBox _cboExtendMonths = null!;
        private Button _btnSaveInfo = null!;
        private Button _btnExtendCard = null!;
        private Button _btnToggleLock = null!;

        public ReaderManagementControl()
        {
            BackColor = BgGray;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
            Load += (_, _) => LoadReaders();
        }

        // ============================================================
        //  DỰNG GIAO DIỆN
        // ============================================================
        private void BuildUi()
        {
            SuspendLayout();

            // 1. Header & Thanh tìm kiếm (Top)
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = BgGray };
            pnlTop.Controls.Add(BuildFilterBar());
            pnlTop.Controls.Add(BuildHeader());

            // 2. Khu vực chính: Trái (Danh sách) - Phải (Chi tiết & Nghiệp vụ thẻ)
            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };

            // Panel phải: Thông tin chi tiết & Gia hạn thẻ
            var pnlRightHost = new Panel { Dock = DockStyle.Right, Width = 370, Padding = new Padding(10, 0, 0, 0) };
            pnlRightHost.Controls.Add(BuildDetailGroup());

            // Panel trái: DataGridView danh sách độc giả
            var grpList = new GroupBox
            {
                Text = "Danh sách độc giả thư viện",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10)
            };
            _dgvReaders = BuildGrid();
            grpList.Controls.Add(_dgvReaders);

            pnlMain.Controls.Add(grpList);
            pnlMain.Controls.Add(pnlRightHost);

            Controls.Add(pnlMain);
            Controls.Add(pnlTop);

            ResumeLayout(true);
        }

        private Panel BuildHeader()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = SoftBlue };
            pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

            pnl.Controls.Add(new Label
            {
                Text = "QUẢN LÝ ĐỘC GIẢ & THẺ THƯ VIỆN",
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(14, 7)
            });

            _lblSummary = new Label
            {
                Text = "Đang tải dữ liệu độc giả...",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(16, 31)
            };
            pnl.Controls.Add(_lblSummary);

            var btnAdd = CreateActionButton("  Thêm độc giả mới", GAddUser, Primary, Color.White, 180);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += BtnAddReader_Click;

            var btnRefresh = CreateActionButton("  Làm mới", GRefresh, Color.White, Color.Black, 110);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (_, _) => LoadReaders();

            pnl.Controls.Add(btnAdd);
            pnl.Controls.Add(btnRefresh);
            pnl.Resize += (_, _) =>
            {
                btnRefresh.Location = new Point(pnl.Width - btnRefresh.Width - 12, 11);
                btnAdd.Location     = new Point(btnRefresh.Left - btnAdd.Width - 8, 11);
            };

            return pnl;
        }

        private Panel BuildFilterBar()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(12, 8, 12, 8)
            };
            pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

            var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, Location = new Point(14, 16), ForeColor = TextGray };

            _txtSearch = new TextBox
            {
                Location = new Point(0, 12),
                Width = 260,
                PlaceholderText = "Mã độc giả, họ tên, email hoặc SĐT..."
            };
            _txtSearch.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoadReaders(); };

            var lblStatus = new Label { Text = "Lọc trạng thái:", AutoSize = true, Location = new Point(0, 16), ForeColor = TextGray };

            _cboFilterStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 12),
                Width = 185
            };
            _cboFilterStatus.Items.AddRange(new object[]
            {
                "Tất cả độc giả",
                "Đang hoạt động",
                "Thẻ sắp hết hạn (≤ 30 ngày)",
                "Thẻ đã hết hạn",
                "Tài khoản bị khóa"
            });
            _cboFilterStatus.SelectedIndex = 0;
            _cboFilterStatus.SelectedIndexChanged += (_, _) => LoadReaders();

            var btnSearch = CreateActionButton("  Tìm kiếm", GSearch, SoftBlue, Primary, 120);
            btnSearch.Location = new Point(0, 9);
            btnSearch.Height = 31;
            btnSearch.Click += (_, _) => LoadReaders();

            void LayoutFilterBar()
            {
                btnSearch.Left = pnl.ClientSize.Width - btnSearch.Width - 12;
                _cboFilterStatus.Left = btnSearch.Left - _cboFilterStatus.Width - 10;
                lblStatus.Left = _cboFilterStatus.Left - lblStatus.Width - 8;
                _txtSearch.Left = lblSearch.Right + 10;
                _txtSearch.Width = Math.Max(150, lblStatus.Left - _txtSearch.Left - 12);
                lblSearch.Top = (pnl.ClientSize.Height - lblSearch.Height) / 2;
                lblStatus.Top = (pnl.ClientSize.Height - lblStatus.Height) / 2;
            }

            pnl.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblStatus, _cboFilterStatus, btnSearch });
            pnl.Resize += (_, _) => LayoutFilterBar();
            LayoutFilterBar();
            return pnl;
        }

        private GroupBox BuildDetailGroup()
        {
            var grp = new GroupBox
            {
                Text = "Hồ sơ & Nghiệp vụ thẻ độc giả",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12),
                BackColor = Color.White
            };

            int y = 26;
            void AddField(string label, out TextBox txt, bool readOnly = false)
            {
                grp.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = TextGray, Location = new Point(14, y) });
                y += 20;
                txt = new TextBox
                {
                    Location = new Point(14, y),
                    Width = 320,
                    ReadOnly = readOnly,
                    BackColor = readOnly ? Color.FromArgb(246, 247, 249) : Color.White
                };
                grp.Controls.Add(txt);
                y += 32;
            }

            AddField("Mã độc giả:", out _txtCode, readOnly: true);
            AddField("Họ và tên:", out _txtFullName);
            AddField("Email liên hệ:", out _txtEmail);
            AddField("Số điện thoại:", out _txtPhone);

            _btnSaveInfo = CreateActionButton("  Cập nhật thông tin", GSave, Primary, Color.White, 320);
            _btnSaveInfo.Location = new Point(14, y + 4);
            _btnSaveInfo.Click += BtnSaveInfo_Click;
            grp.Controls.Add(_btnSaveInfo);
            y += 48;

            // Khung trạng thái thẻ & lưu thông
            var pnlCardBox = new Panel
            {
                Location = new Point(14, y),
                Size = new Size(320, 92),
                BackColor = SoftBlue,
                Padding = new Padding(10)
            };
            pnlCardBox.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlCardBox.Width - 1, pnlCardBox.Height - 1);

            _lblCardInfo = new Label
            {
                Text = "Chọn một độc giả ở bảng bên trái để xem thẻ",
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                ForeColor = Primary,
                AutoSize = false,
                Size = new Size(300, 42),
                Location = new Point(10, 8)
            };
            _lblBorrowStats = new Label
            {
                Text = "Tình hình mượn sách: --",
                Font = new Font("Segoe UI", 8.3F),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = false,
                Size = new Size(300, 32),
                Location = new Point(10, 52)
            };
            pnlCardBox.Controls.Add(_lblCardInfo);
            pnlCardBox.Controls.Add(_lblBorrowStats);
            grp.Controls.Add(pnlCardBox);
            y += 104;

            // Gia hạn thẻ thư viện
            grp.Controls.Add(new Label { Text = "Gia hạn / Cấp thẻ thư viện:", AutoSize = true, ForeColor = TextGray, Location = new Point(14, y) });
            y += 22;

            _cboExtendMonths = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(14, y + 2),
                Width = 145
            };
            _cboExtendMonths.Items.AddRange(new object[] { "Thêm 6 tháng", "Thêm 12 tháng", "Thêm 24 tháng" });
            _cboExtendMonths.SelectedIndex = 1;

            _btnExtendCard = CreateActionButton("  Gia hạn thẻ", GCalendar, Green, Color.White, 165);
            _btnExtendCard.Location = new Point(169, y);
            _btnExtendCard.Height = 30;
            _btnExtendCard.Click += BtnExtendCard_Click;

            grp.Controls.Add(_cboExtendMonths);
            grp.Controls.Add(_btnExtendCard);
            y += 46;

            // Khóa / Mở khóa tài khoản
            _btnToggleLock = CreateActionButton("  Khóa tài khoản độc giả", GLock, Color.White, Red, 320);
            _btnToggleLock.Location = new Point(14, y);
            _btnToggleLock.Click += BtnToggleLock_Click;
            grp.Controls.Add(_btnToggleLock);

            SetDetailEnabled(false);
            return grp;
        }

        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 228, 233),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 40 },
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F)
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;

            g.Columns.Add("colId",     "ID");
            g.Columns.Add("colCode",   "Mã độc giả");
            g.Columns.Add("colName",   "Họ và tên");
            g.Columns.Add("colContact","Email / SĐT");
            g.Columns.Add("colExpire", "Hạn thẻ");
            g.Columns.Add("colBorrow", "Đang mượn");
            g.Columns.Add("colStatus", "Trạng thái");

            g.Columns["colId"]!.Visible = false;
            g.Columns["colCode"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colCode"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            g.Columns["colCode"]!.FillWeight    = 75;
            g.Columns["colName"]!.FillWeight    = 115;
            g.Columns["colContact"]!.FillWeight = 120;
            g.Columns["colExpire"]!.FillWeight  = 75;
            g.Columns["colBorrow"]!.FillWeight  = 65;
            g.Columns["colStatus"]!.FillWeight  = 95;

            g.CellPainting += DgvReaders_CellPainting;
            g.SelectionChanged += DgvReaders_SelectionChanged;
            return g;
        }

        private void DgvReaders_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 6 || e.Graphics == null) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch
            {
                "Hoạt động"    => Green,
                "Sắp hết hạn"  => Orange,
                "Hết hạn thẻ"  => Red,
                "Bị khóa"      => Red,
                _              => TextGray
            };

            var font = new Font("Segoe UI", 8.5F);
            string label = "●  " + text;
            var size = TextRenderer.MeasureText(label, font);
            var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 25) / 2, size.Width + 8, 25);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(rect, 12))
            using (var fill = new SolidBrush(Color.FromArgb(22, c)))
            using (var pen  = new Pen(Color.FromArgb(150, c)))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            }
            TextRenderer.DrawText(e.Graphics, label, font, rect, c,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            e.Handled = true;
        }

        // ============================================================
        //  TẢI DỮ LIỆU & XỬ LÝ NGHIỆP VỤ
        // ============================================================
        public void LoadReaders()
        {
            try
            {
                using var context = new LibraryDbContext();
                var today = DateTime.Today;
                string keyword = _txtSearch.Text.Trim().ToLower();
                int filterMode = _cboFilterStatus.SelectedIndex;

                var rawReaders = context.Users
                    .Where(u => u.UserType == UserType.Reader)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserCode,
                        u.FullName,
                        u.Email,
                        u.Phone,
                        u.Status,
                        LatestCard = context.ReaderCards
                            .Where(rc => rc.IdReader == u.Id)
                            .OrderByDescending(rc => rc.ExpiresAt)
                            .Select(rc => new { rc.CardCode, rc.ExpiresAt, rc.Status })
                            .FirstOrDefault(),
                        ActiveLoans = context.LoanDetails
                            .Count(ld => ld.Loan.IdReader == u.Id && ld.ReturnedAt == null),
                        OverdueLoans = context.LoanDetails
                            .Count(ld => ld.Loan.IdReader == u.Id && ld.ReturnedAt == null && ld.DueAt.Date < today)
                    })
                    .OrderByDescending(x => x.Id)
                    .ToList();

                int totalReaders = rawReaders.Count;
                int activeCount = rawReaders.Count(r => r.Status == UserStatus.Active);

                var filtered = rawReaders.Where(r =>
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        bool match = (r.UserCode?.ToLower().Contains(keyword) ?? false)
                                  || (r.FullName?.ToLower().Contains(keyword) ?? false)
                                  || (r.Email?.ToLower().Contains(keyword) ?? false)
                                  || (r.Phone?.ToLower().Contains(keyword) ?? false);
                        if (!match) return false;
                    }

                    DateTime? exp = r.LatestCard?.ExpiresAt.Date;
                    return filterMode switch
                    {
                        1 => r.Status == UserStatus.Active,
                        2 => r.Status == UserStatus.Active && exp != null && exp >= today && exp <= today.AddDays(30),
                        3 => exp == null || exp < today,
                        4 => r.Status != UserStatus.Active,
                        _ => true
                    };
                }).ToList();

                _lblSummary.Text = $"Tổng số: {totalReaders:N0} độc giả  ·  Đang hoạt động: {activeCount:N0}  ·  Hiển thị: {filtered.Count:N0} kết quả";

                _dgvReaders.Rows.Clear();
                foreach (var r in filtered)
                {
                    DateTime? exp = r.LatestCard?.ExpiresAt.Date;
                    string statusText =
                        r.Status != UserStatus.Active          ? "Bị khóa" :
                        exp == null || exp < today             ? "Hết hạn thẻ" :
                        (exp.Value - today).Days <= 30         ? "Sắp hết hạn" :
                                                                 "Hoạt động";

                    string contact = !string.IsNullOrWhiteSpace(r.Phone) ? r.Phone : (r.Email ?? "--");
                    string borrowText = r.OverdueLoans > 0
                        ? $"{r.ActiveLoans} ({r.OverdueLoans} trễ)"
                        : $"{r.ActiveLoans} cuốn";

                    _dgvReaders.Rows.Add(
                        r.Id,
                        r.UserCode,
                        r.FullName,
                        contact,
                        exp?.ToString("dd/MM/yyyy") ?? "Chưa cấp",
                        borrowText,
                        statusText
                    );
                }

                if (_dgvReaders.Rows.Count > 0)
                    _dgvReaders.Rows[0].Selected = true;
                else
                    ClearDetail();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách độc giả:\n{ex.Message}", "Lỗi CSDL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvReaders_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dgvReaders.SelectedRows.Count == 0)
            {
                ClearDetail();
                return;
            }

            if (_dgvReaders.SelectedRows[0].Cells["colId"].Value is not int readerId) return;
            LoadReaderDetail(readerId);
        }

        private void LoadReaderDetail(int readerId)
        {
            try
            {
                using var context = new LibraryDbContext();
                var today = DateTime.Today;

                var user = context.Users.FirstOrDefault(u => u.Id == readerId);
                if (user == null) return;

                _selectedReaderId = user.Id;
                SetDetailEnabled(true);

                _txtCode.Text = user.UserCode;
                _txtFullName.Text = user.FullName;
                _txtEmail.Text = user.Email ?? "";
                _txtPhone.Text = user.Phone ?? "";

                var card = context.ReaderCards
                    .Where(rc => rc.IdReader == readerId)
                    .OrderByDescending(rc => rc.ExpiresAt)
                    .FirstOrDefault();

                if (card == null)
                {
                    _lblCardInfo.ForeColor = Red;
                    _lblCardInfo.Text = "Thẻ thư viện: Chưa được cấp thẻ";
                    _btnExtendCard.Text = "  Cấp thẻ mới";
                }
                else
                {
                    bool expired = card.ExpiresAt.Date < today;
                    _lblCardInfo.ForeColor = expired ? Red : Primary;
                    _lblCardInfo.Text = $"Mã thẻ: {card.CardCode}\nHạn sử dụng: {card.ExpiresAt:dd/MM/yyyy} {(expired ? "(Đã hết hạn)" : "")}";
                    _btnExtendCard.Text = "  Gia hạn thẻ";
                }

                int borrowing = context.LoanDetails.Count(ld => ld.Loan.IdReader == readerId && ld.ReturnedAt == null);
                int overdue = context.LoanDetails.Count(ld => ld.Loan.IdReader == readerId && ld.ReturnedAt == null && ld.DueAt.Date < today);
                int totalHistory = context.LoanDetails.Count(ld => ld.Loan.IdReader == readerId);

                _lblBorrowStats.ForeColor = overdue > 0 ? Red : Color.FromArgb(60, 60, 60);
                _lblBorrowStats.Text = $"Đang mượn: {borrowing} cuốn (Quá hạn: {overdue}) · Tổng lịch sử: {totalHistory} lượt";

                bool isActive = user.Status == UserStatus.Active;
                _btnToggleLock.Text = isActive ? "  Khóa tài khoản độc giả" : "  Mở khóa tài khoản";
                _btnToggleLock.ForeColor = isActive ? Red : Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải hồ sơ độc giả:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddReader_Click(object? sender, EventArgs e)
        {
            using var form = new LibraryManagement.Forms.RegisterForm.RegisterForm(librarianMode: true);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadReaders();
                ReaderDataChanged?.Invoke();
            }
        }

        private void BtnSaveInfo_Click(object? sender, EventArgs e)
        {
            if (_selectedReaderId == null) return;
            if (string.IsNullOrWhiteSpace(_txtFullName.Text))
            {
                MessageBox.Show("Họ tên độc giả không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var context = new LibraryDbContext();
                var user = context.Users.FirstOrDefault(u => u.Id == _selectedReaderId.Value);
                if (user == null) return;

                user.FullName = _txtFullName.Text.Trim();
                user.Email = _txtEmail.Text.Trim();
                user.Phone = _txtPhone.Text.Trim();

                context.SaveChanges();
                MessageBox.Show("Đã cập nhật thông tin độc giả thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadReaders();
                ReaderDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật thông tin:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExtendCard_Click(object? sender, EventArgs e)
        {
            if (_selectedReaderId == null) return;
            int months = _cboExtendMonths.SelectedIndex switch
            {
                0 => 6,
                2 => 24,
                _ => 12
            };

            try
            {
                using var context = new LibraryDbContext();
                var card = context.ReaderCards
                    .Where(rc => rc.IdReader == _selectedReaderId.Value)
                    .OrderByDescending(rc => rc.ExpiresAt)
                    .FirstOrDefault();

                if (card == null)
                {
                    card = new LibraryManagement.Models.ReaderCards
                    {
                        IdReader = _selectedReaderId.Value,
                        CardCode = $"CARD-{DateTime.Now:yyMMdd}-{_selectedReaderId.Value:D4}",
                        IssuedAt = DateTime.Now,
                        ExpiresAt = DateTime.Today.AddMonths(months),
                        Status = ReaderCardStatus.Active
                    };
                    context.ReaderCards.Add(card);
                }
                else
                {
                    var baseDate = card.ExpiresAt.Date < DateTime.Today ? DateTime.Today : card.ExpiresAt.Date;
                    card.ExpiresAt = baseDate.AddMonths(months);
                    card.Status = ReaderCardStatus.Active;
                }

                context.SaveChanges();
                MessageBox.Show($"Đã gia hạn thẻ thêm {months} tháng (Hạn mới: {card.ExpiresAt:dd/MM/yyyy}).",
                    "Gia hạn thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadReaders();
                ReaderDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gia hạn thẻ thư viện:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnToggleLock_Click(object? sender, EventArgs e)
        {
            if (_selectedReaderId == null) return;

            try
            {
                using var context = new LibraryDbContext();
                var user = context.Users.FirstOrDefault(u => u.Id == _selectedReaderId.Value);
                if (user == null) return;

                bool currentlyActive = user.Status == UserStatus.Active;
                string actionName = currentlyActive ? "khóa" : "mở khóa";

                var confirm = MessageBox.Show($"Bạn có chắc muốn {actionName} tài khoản của độc giả \"{user.FullName}\"?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                user.Status = currentlyActive ? UserStatus.Inactive : UserStatus.Active;
                context.SaveChanges();

                LoadReaders();
                ReaderDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thay đổi trạng thái tài khoản:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDetail()
        {
            _selectedReaderId = null;
            _txtCode.Clear();
            _txtFullName.Clear();
            _txtEmail.Clear();
            _txtPhone.Clear();
            _lblCardInfo.Text = "Chưa chọn độc giả";
            _lblBorrowStats.Text = "Tình hình mượn sách: --";
            SetDetailEnabled(false);
        }

        private void SetDetailEnabled(bool enabled)
        {
            _txtFullName.Enabled = enabled;
            _txtEmail.Enabled = enabled;
            _txtPhone.Enabled = enabled;
            _btnSaveInfo.Enabled = enabled;
            _cboExtendMonths.Enabled = enabled;
            _btnExtendCard.Enabled = enabled;
            _btnToggleLock.Enabled = enabled;
        }

        // ============================================================
        //  HELPER UI
        // ============================================================
        private static Button CreateActionButton(string text, string glyph, Color back, Color fore, int width)
        {
            var bmp = new Bitmap(18, 18);
            using (var g = Graphics.FromImage(bmp))
            using (var f = new Font(IconFont, 13, GraphicsUnit.Pixel))
            {
                TextRenderer.DrawText(g, glyph, f, new Rectangle(0, -1, 18, 18), fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Size = new Size(width, 34),
                Image = bmp,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = BorderGray;
            return btn;
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}