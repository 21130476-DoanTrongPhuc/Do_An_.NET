using System.Drawing.Drawing2D;
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Models;

namespace LibraryManagement.Forms.LibraryCardManagement
{
    public partial class LibraryCardManagementControl : UserControl
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
        private const string GCard     = "\uE8C7";
        private const string GAdd      = "\uE710";
        private const string GRefresh  = "\uE72C";
        private const string GSearch   = "\uE721";
        private const string GCalendar = "\uE787";
        private const string GLock     = "\uE72E";
        private const string GPrint    = "\uE749";
        private const string GSave     = "\uE74E";

        public event Action? CardDataChanged;

        // ====== Controls danh sách trái ======
        private TextBox _txtSearch = null!;
        private ComboBox _cboStatusFilter = null!;
        private DataGridView _dgvCards = null!;
        private Label _lblSummary = null!;

        // ====== Controls nghiệp vụ thẻ bên phải ======
        private int? _selectedReaderId;
        private int? _selectedCardId;
        private Panel _pnlCardPreview = null!;
        private Label _lblPreviewCode = null!;
        private Label _lblPreviewReader = null!;
        private Label _lblPreviewDates = null!;
        private Label _lblPreviewStatus = null!;

        private TextBox _txtCardCode = null!;
        private DateTimePicker _dtpIssuedAt = null!;
        private DateTimePicker _dtpExpiresAt = null!;
        private TextBox _txtNote = null!;
        private ComboBox _cboExtendMonths = null!;

        private Button _btnIssueOrSave = null!;
        private Button _btnExtend = null!;
        private Button _btnToggleLock = null!;
        private Button _btnPrintCard = null!;

        public LibraryCardManagementControl()
        {
            BackColor = BgGray;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
            Load += (_, _) => LoadCards();
        }

        // ============================================================
        //  DỰNG GIAO DIỆN
        // ============================================================
        private void BuildUi()
        {
            SuspendLayout();

            // 1. Header & Thanh lọc (Top)
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = BgGray };
            pnlTop.Controls.Add(BuildFilterBar());
            pnlTop.Controls.Add(BuildHeader());

            // 2. Khu vực chính: Trái (Danh sách thẻ) - Phải (Chi tiết & Thao tác thẻ)
            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };

            var pnlRightHost = new Panel { Dock = DockStyle.Right, Width = 395, Padding = new Padding(10, 0, 0, 0) };
            pnlRightHost.Controls.Add(BuildCardActionGroup());

            var grpList = new GroupBox
            {
                Text = "Danh sách thẻ thư viện của độc giả",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10)
            };
            _dgvCards = BuildGrid();
            grpList.Controls.Add(_dgvCards);

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
                Text = "QUẢN LÝ THẺ THƯ VIỆN ĐỘC GIẢ",
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(14, 7)
            });

            _lblSummary = new Label
            {
                Text = "Đang tải dữ liệu thẻ thư viện...",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(16, 31)
            };
            pnl.Controls.Add(_lblSummary);

            var btnIssueNew = CreateActionButton("  Cấp thẻ độc giả mới", GAdd, Primary, Color.White, 175);
            btnIssueNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnIssueNew.Click += BtnQuickIssueDialog_Click;

            var btnRefresh = CreateActionButton("  Làm mới", GRefresh, Color.White, Color.Black, 110);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (_, _) => LoadCards();

            pnl.Controls.Add(btnIssueNew);
            pnl.Controls.Add(btnRefresh);
            pnl.Resize += (_, _) =>
            {
                btnRefresh.Location  = new Point(pnl.Width - btnRefresh.Width - 12, 11);
                btnIssueNew.Location = new Point(btnRefresh.Left - btnIssueNew.Width - 8, 11);
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
                PlaceholderText = "Mã thẻ (RC...), mã độc giả, họ tên hoặc SĐT..."
            };
            _txtSearch.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoadCards(); };

            var lblStatus = new Label { Text = "Tình trạng thẻ:", AutoSize = true, Location = new Point(0, 16), ForeColor = TextGray };

            _cboStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 12),
                Width = 195
            };
            _cboStatusFilter.Items.AddRange(new object[]
            {
                "Tất cả độc giả & thẻ",
                "Đang hoạt động (Active)",
                "Sắp hết hạn (≤ 30 ngày)",
                "Đã hết hạn (Expired)",
                "Đang bị khóa (Locked)",
                "Độc giả chưa có thẻ (Missing)"
            });
            _cboStatusFilter.SelectedIndex = 0;
            _cboStatusFilter.SelectedIndexChanged += (_, _) => LoadCards();

            var btnSearch = CreateActionButton("  Tra cứu", GSearch, SoftBlue, Primary, 120);
            btnSearch.Location = new Point(0, 9);
            btnSearch.Height = 31;
            btnSearch.Click += (_, _) => LoadCards();

            void LayoutFilterBar()
            {
                btnSearch.Left = pnl.ClientSize.Width - btnSearch.Width - 12;
                _cboStatusFilter.Left = btnSearch.Left - _cboStatusFilter.Width - 10;
                lblStatus.Left = _cboStatusFilter.Left - lblStatus.Width - 8;
                _txtSearch.Left = lblSearch.Right + 10;
                _txtSearch.Width = Math.Max(150, lblStatus.Left - _txtSearch.Left - 12);
                lblSearch.Top = (pnl.ClientSize.Height - lblSearch.Height) / 2;
                lblStatus.Top = (pnl.ClientSize.Height - lblStatus.Height) / 2;
            }

            pnl.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblStatus, _cboStatusFilter, btnSearch });
            pnl.Resize += (_, _) => LayoutFilterBar();
            LayoutFilterBar();
            return pnl;
        }

        private GroupBox BuildCardActionGroup()
        {
            var grp = new GroupBox
            {
                Text = "Thông tin & Nghiệp vụ thẻ thư viện",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12),
                BackColor = Color.White
            };

            // Mô phỏng trực quan thẻ thư viện
            _pnlCardPreview = new Panel
            {
                Location = new Point(14, 24),
                Size = new Size(345, 118),
                BackColor = Primary
            };
            _pnlCardPreview.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var borderPen = new Pen(Color.FromArgb(14, 74, 124), 1.5f);
                e.Graphics.DrawRectangle(borderPen, 0, 0, _pnlCardPreview.Width - 1, _pnlCardPreview.Height - 1);
            };

            _pnlCardPreview.Controls.Add(new Label
            {
                Text = "THẺ ĐỘC GIẢ THƯ VIỆN LIBRA",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(190, 220, 245),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(12, 8)
            });

            _lblPreviewStatus = new Label
            {
                Text = "CHƯA CHỌN",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 255, 255, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(105, 22),
                Location = new Point(228, 7)
            };

            _lblPreviewCode = new Label
            {
                Text = "MÃ THẺ: ---",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(12, 32)
            };

            _lblPreviewReader = new Label
            {
                Text = "Chủ thẻ: Chưa chọn độc giả",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(12, 62)
            };

            _lblPreviewDates = new Label
            {
                Text = "Ngày cấp: --/--/----   ·   Hạn thẻ: --/--/----",
                Font = new Font("Segoe UI", 8.2F),
                ForeColor = Color.FromArgb(220, 235, 250),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(12, 88)
            };

            _pnlCardPreview.Controls.AddRange(new Control[] { _lblPreviewStatus, _lblPreviewCode, _lblPreviewReader, _lblPreviewDates });
            grp.Controls.Add(_pnlCardPreview);

            int y = 154;

            // Mã thẻ
            grp.Controls.Add(new Label { Text = "Mã thẻ thư viện (CardCode):", AutoSize = true, ForeColor = TextGray, Location = new Point(14, y) });
            y += 20;
            _txtCardCode = new TextBox { Location = new Point(14, y), Width = 345 };
            grp.Controls.Add(_txtCardCode);
            y += 32;

            // Ngày cấp & Ngày hết hạn
            grp.Controls.Add(new Label { Text = "Ngày cấp thẻ:", AutoSize = true, ForeColor = TextGray, Location = new Point(14, y) });
            grp.Controls.Add(new Label { Text = "Ngày hết hạn:", AutoSize = true, ForeColor = TextGray, Location = new Point(194, y) });
            y += 20;

            _dtpIssuedAt = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(14, y), Width = 165 };
            _dtpExpiresAt = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(194, y), Width = 165 };
            grp.Controls.Add(_dtpIssuedAt);
            grp.Controls.Add(_dtpExpiresAt);
            y += 34;

            // Ghi chú thẻ
            grp.Controls.Add(new Label { Text = "Ghi chú thẻ (Lý do khóa / cấp lại):", AutoSize = true, ForeColor = TextGray, Location = new Point(14, y) });
            y += 20;
            _txtNote = new TextBox { Location = new Point(14, y), Width = 345, PlaceholderText = "Ghi chú tình trạng thẻ..." };
            grp.Controls.Add(_txtNote);
            y += 36;

            // Nút Cấp thẻ / Lưu thông tin thẻ
            _btnIssueOrSave = CreateActionButton("  Lưu thông tin thẻ", GSave, Primary, Color.White, 345);
            _btnIssueOrSave.Location = new Point(14, y);
            _btnIssueOrSave.Click += BtnIssueOrSave_Click;
            grp.Controls.Add(_btnIssueOrSave);
            y += 44;

            // Khu vực gia hạn nhanh
            grp.Controls.Add(new Label { Text = "Gia hạn nhanh thời hạn thẻ:", AutoSize = true, ForeColor = TextGray, Location = new Point(14, y) });
            y += 21;

            _cboExtendMonths = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(14, y + 2),
                Width = 165
            };
            _cboExtendMonths.Items.AddRange(new object[] { "Gia hạn 6 tháng", "Gia hạn 12 tháng", "Gia hạn 24 tháng" });
            _cboExtendMonths.SelectedIndex = 1;

            _btnExtend = CreateActionButton("  Gia hạn ngay", GCalendar, Green, Color.White, 170);
            _btnExtend.Location = new Point(189, y);
            _btnExtend.Height = 30;
            _btnExtend.Click += BtnExtend_Click;

            grp.Controls.Add(_cboExtendMonths);
            grp.Controls.Add(_btnExtend);
            y += 44;

            // Khóa/Mở khóa & In thẻ
            _btnToggleLock = CreateActionButton("  Khóa thẻ", GLock, Color.White, Red, 168);
            _btnToggleLock.Location = new Point(14, y);
            _btnToggleLock.Click += BtnToggleLock_Click;

            _btnPrintCard = CreateActionButton("  Xem / In thẻ", GPrint, SoftBlue, Primary, 168);
            _btnPrintCard.Location = new Point(191, y);
            _btnPrintCard.Click += BtnPrintCard_Click;

            grp.Controls.Add(_btnToggleLock);
            grp.Controls.Add(_btnPrintCard);

            SetActionPanelEnabled(false);
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

            g.Columns.Add("colReaderId", "ReaderId");
            g.Columns.Add("colCardId",   "CardId");
            g.Columns.Add("colCardCode", "Mã thẻ");
            g.Columns.Add("colReader",   "Độc giả");
            g.Columns.Add("colPhone",    "Số điện thoại");
            g.Columns.Add("colIssued",   "Ngày cấp");
            g.Columns.Add("colExpires",  "Hạn sử dụng");
            g.Columns.Add("colStatus",   "Trạng thái thẻ");

            g.Columns["colReaderId"]!.Visible = false;
            g.Columns["colCardId"]!.Visible = false;
            g.Columns["colCardCode"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colCardCode"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            g.Columns["colCardCode"]!.FillWeight = 80;
            g.Columns["colReader"]!.FillWeight   = 135;
            g.Columns["colPhone"]!.FillWeight    = 85;
            g.Columns["colIssued"]!.FillWeight   = 75;
            g.Columns["colExpires"]!.FillWeight  = 80;
            g.Columns["colStatus"]!.FillWeight   = 105;

            g.CellPainting += DgvCards_CellPainting;
            g.SelectionChanged += DgvCards_SelectionChanged;
            return g;
        }

        private void DgvCards_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 7 || e.Graphics == null) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch
            {
                "Đang hoạt động" => Green,
                "Sắp hết hạn"    => Orange,
                "Đã hết hạn"     => Red,
                "Đang bị khóa"   => Red,
                "Chưa cấp thẻ"   => TextGray,
                _                => TextGray
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
        //  TẢI DỮ LIỆU & XỬ LÝ NGHIỆP VỤ THẺ
        // ============================================================
        public void LoadCards()
        {
            try
            {
                using var db = new LibraryDbContext();
                var today = DateTime.Today;
                string kw = _txtSearch.Text.Trim().ToLower();
                int filterIdx = _cboStatusFilter.SelectedIndex;

                // Lấy toàn bộ độc giả kèm thẻ mới nhất của họ (kể cả độc giả chưa có thẻ)
                var readerCards = db.Users
                    .Where(u => u.UserType == UserType.Reader)
                    .Select(u => new
                    {
                        ReaderId = u.Id,
                        u.UserCode,
                        u.FullName,
                        u.Phone,
                        LatestCard = db.ReaderCards
                            .Where(c => c.IdReader == u.Id)
                            .OrderByDescending(c => c.Status == ReaderCardStatus.Active)
                            .ThenByDescending(c => c.ExpiresAt)
                            .Select(c => new
                            {
                                c.Id,
                                c.CardCode,
                                c.IssuedAt,
                                c.ExpiresAt,
                                c.Status
                            })
                            .FirstOrDefault()
                    })
                    .OrderByDescending(x => x.LatestCard != null ? x.LatestCard.Id : 0)
                    .ThenByDescending(x => x.ReaderId)
                    .ToList();

                int activeCount = 0, expiringCount = 0, expiredOrLockedCount = 0, missingCount = 0;

                var viewItems = readerCards.Select(x =>
                {
                    string displayStatus;
                    if (x.LatestCard == null)
                    {
                        displayStatus = "Chưa cấp thẻ";
                        missingCount++;
                    }
                    else if (x.LatestCard.Status == ReaderCardStatus.Locked)
                    {
                        displayStatus = "Đang bị khóa";
                        expiredOrLockedCount++;
                    }
                    else if (x.LatestCard.Status == ReaderCardStatus.Expired || x.LatestCard.ExpiresAt.Date < today)
                    {
                        displayStatus = "Đã hết hạn";
                        expiredOrLockedCount++;
                    }
                    else if ((x.LatestCard.ExpiresAt.Date - today).Days <= 30)
                    {
                        displayStatus = "Sắp hết hạn";
                        expiringCount++;
                        activeCount++;
                    }
                    else
                    {
                        displayStatus = "Đang hoạt động";
                        activeCount++;
                    }

                    return new
                    {
                        x.ReaderId,
                        CardId = x.LatestCard?.Id,
                        CardCode = x.LatestCard?.CardCode ?? "—",
                        x.UserCode,
                        x.FullName,
                        Phone = x.Phone ?? "—",
                        IssuedAt = x.LatestCard?.IssuedAt,
                        ExpiresAt = x.LatestCard?.ExpiresAt,
                        RawStatus = x.LatestCard?.Status ?? ReaderCardStatus.Missing,
                        DisplayStatus = displayStatus
                    };
                }).ToList();

                var filtered = viewItems.Where(item =>
                {
                    if (!string.IsNullOrEmpty(kw))
                    {
                        bool match = item.CardCode.ToLower().Contains(kw)
                                  || item.UserCode.ToLower().Contains(kw)
                                  || item.FullName.ToLower().Contains(kw)
                                  || item.Phone.ToLower().Contains(kw);
                        if (!match) return false;
                    }

                    return filterIdx switch
                    {
                        1 => item.DisplayStatus is "Đang hoạt động" or "Sắp hết hạn",
                        2 => item.DisplayStatus == "Sắp hết hạn",
                        3 => item.DisplayStatus == "Đã hết hạn",
                        4 => item.DisplayStatus == "Đang bị khóa",
                        5 => item.DisplayStatus == "Chưa cấp thẻ",
                        _ => true
                    };
                }).ToList();

                _lblSummary.Text =
                    $"Hoạt động: {activeCount}  ·  Sắp hết hạn: {expiringCount}  ·  Hết hạn/Khóa: {expiredOrLockedCount}  ·  Chưa cấp thẻ: {missingCount}";

                _dgvCards.Rows.Clear();
                foreach (var item in filtered)
                {
                    _dgvCards.Rows.Add(
                        item.ReaderId,
                        item.CardId,
                        item.CardCode,
                        $"{item.FullName} ({item.UserCode})",
                        item.Phone,
                        item.IssuedAt?.ToString("dd/MM/yyyy") ?? "—",
                        item.ExpiresAt?.ToString("dd/MM/yyyy") ?? "—",
                        item.DisplayStatus
                    );
                }

                if (_dgvCards.Rows.Count > 0)
                    _dgvCards.Rows[0].Selected = true;
                else
                    ClearActionPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách thẻ thư viện:\n{ex.Message}", "Lỗi CSDL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvCards_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dgvCards.SelectedRows.Count == 0)
            {
                ClearActionPanel();
                return;
            }

            var row = _dgvCards.SelectedRows[0];
            if (row.Cells["colReaderId"].Value is not int readerId) return;
            int? cardId = row.Cells["colCardId"].Value as int?;

            LoadCardDetail(readerId, cardId);
        }

        private void LoadCardDetail(int readerId, int? cardId)
        {
            try
            {
                using var db = new LibraryDbContext();
                var today = DateTime.Today;

                var reader = db.Users.FirstOrDefault(u => u.Id == readerId);
                if (reader == null) return;

                var card = cardId.HasValue
                    ? db.ReaderCards.FirstOrDefault(c => c.Id == cardId.Value)
                    : db.ReaderCards.Where(c => c.IdReader == readerId)
                        .OrderByDescending(c => c.ExpiresAt)
                        .FirstOrDefault();

                _selectedReaderId = reader.Id;
                _selectedCardId = card?.Id;
                SetActionPanelEnabled(true);

                if (card == null)
                {
                    // Độc giả chưa có thẻ -> Gợi ý cấp thẻ mới
                    string suggestedCode = GenerateNextCardCode(db);
                    _pnlCardPreview.BackColor = TextGray;
                    _lblPreviewStatus.Text = "CHƯA CẤP THẺ";
                    _lblPreviewCode.Text = $"MÃ GỢI Ý: {suggestedCode}";
                    _lblPreviewReader.Text = $"Độc giả: {reader.FullName} ({reader.UserCode})";
                    _lblPreviewDates.Text = "Chưa có dữ liệu phát hành thẻ";

                    _txtCardCode.Text = suggestedCode;
                    _txtCardCode.ReadOnly = false;
                    _dtpIssuedAt.Value = today;
                    _dtpExpiresAt.Value = today.AddYears(1);
                    _txtNote.Clear();

                    _btnIssueOrSave.Text = "  Cấp thẻ thư viện mới";
                    _btnIssueOrSave.BackColor = Green;
                    _btnExtend.Enabled = false;
                    _btnToggleLock.Enabled = false;
                    _btnPrintCard.Enabled = false;
                }
                else
                {
                    bool isLocked = card.Status == ReaderCardStatus.Locked;
                    bool isExpired = card.Status == ReaderCardStatus.Expired || card.ExpiresAt.Date < today;

                    _pnlCardPreview.BackColor = isLocked ? Red : (isExpired ? Orange : Primary);
                    _lblPreviewStatus.Text = isLocked ? "ĐANG KHÓA" : (isExpired ? "HẾT HẠN" : "HOẠT ĐỘNG");
                    _lblPreviewCode.Text = $"MÃ THẺ: {card.CardCode}";
                    _lblPreviewReader.Text = $"Chủ thẻ: {reader.FullName} ({reader.UserCode})";
                    _lblPreviewDates.Text = $"Ngày cấp: {card.IssuedAt:dd/MM/yyyy}   ·   Hạn thẻ: {card.ExpiresAt:dd/MM/yyyy}";

                    _txtCardCode.Text = card.CardCode;
                    _txtCardCode.ReadOnly = true;
                    _dtpIssuedAt.Value = card.IssuedAt;
                    _dtpExpiresAt.Value = card.ExpiresAt;
                    _txtNote.Text = card.Note ?? "";

                    _btnIssueOrSave.Text = "  Cập nhật thông tin thẻ";
                    _btnIssueOrSave.BackColor = Primary;
                    _btnExtend.Enabled = true;
                    _btnToggleLock.Enabled = true;
                    _btnPrintCard.Enabled = true;

                    _btnToggleLock.Text = isLocked ? "  Mở khóa thẻ" : "  Khóa thẻ";
                    _btnToggleLock.ForeColor = isLocked ? Green : Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết thẻ:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnIssueOrSave_Click(object? sender, EventArgs e)
        {
            if (_selectedReaderId == null) return;
            string code = _txtCardCode.Text.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Mã thẻ thư viện không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_dtpExpiresAt.Value.Date <= _dtpIssuedAt.Value.Date)
            {
                MessageBox.Show("Ngày hết hạn thẻ phải lớn hơn ngày cấp thẻ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var db = new LibraryDbContext();
                var today = DateTime.Today;

                if (_selectedCardId == null)
                {
                    // Cấp thẻ mới
                    if (db.ReaderCards.Any(c => c.CardCode.ToUpper() == code))
                    {
                        MessageBox.Show($"Mã thẻ '{code}' đã tồn tại trên hệ thống. Vui lòng chọn mã khác.",
                            "Trùng mã thẻ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var newCard = new ReaderCards
                    {
                        IdReader = _selectedReaderId.Value,
                        CardCode = code,
                        IssuedAt = _dtpIssuedAt.Value.Date,
                        ExpiresAt = _dtpExpiresAt.Value.Date,
                        Status = _dtpExpiresAt.Value.Date < today ? ReaderCardStatus.Expired : ReaderCardStatus.Active,
                        Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim()
                    };

                    db.ReaderCards.Add(newCard);
                    db.SaveChanges();
                    MessageBox.Show($"Đã cấp thẻ thư viện '{code}' thành công!", "Hoàn tất",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Cập nhật thẻ hiện có
                    var card = db.ReaderCards.FirstOrDefault(c => c.Id == _selectedCardId.Value);
                    if (card == null) return;

                    card.IssuedAt = _dtpIssuedAt.Value.Date;
                    card.ExpiresAt = _dtpExpiresAt.Value.Date;
                    card.Note = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim();

                    if (card.Status != ReaderCardStatus.Locked)
                    {
                        card.Status = card.ExpiresAt.Date < today ? ReaderCardStatus.Expired : ReaderCardStatus.Active;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Đã cập nhật thông tin thẻ thư viện!", "Hoàn tất",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadCards();
                CardDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                string detail = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Lỗi lưu dữ liệu thẻ:\n{detail}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExtend_Click(object? sender, EventArgs e)
        {
            if (_selectedCardId == null) return;
            int months = _cboExtendMonths.SelectedIndex switch
            {
                0 => 6,
                2 => 24,
                _ => 12
            };

            try
            {
                using var db = new LibraryDbContext();
                var card = db.ReaderCards.FirstOrDefault(c => c.Id == _selectedCardId.Value);
                if (card == null) return;

                if (card.Status == ReaderCardStatus.Locked)
                {
                    MessageBox.Show("Thẻ đang bị KHÓA. Vui lòng mở khóa thẻ trước khi gia hạn.",
                        "Thẻ đang khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime baseDate = card.ExpiresAt.Date < DateTime.Today ? DateTime.Today : card.ExpiresAt.Date;
                card.ExpiresAt = baseDate.AddMonths(months);
                card.Status = ReaderCardStatus.Active;

                db.SaveChanges();
                MessageBox.Show($"Đã gia hạn thẻ '{card.CardCode}' thêm {months} tháng.\nHạn sử dụng mới: {card.ExpiresAt:dd/MM/yyyy}",
                    "Gia hạn thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadCards();
                CardDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gia hạn thẻ:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnToggleLock_Click(object? sender, EventArgs e)
        {
            if (_selectedCardId == null) return;

            try
            {
                using var db = new LibraryDbContext();
                var card = db.ReaderCards.FirstOrDefault(c => c.Id == _selectedCardId.Value);
                if (card == null) return;

                bool currentlyLocked = card.Status == ReaderCardStatus.Locked;
                string action = currentlyLocked ? "MỞ KHÓA" : "KHÓA";

                var confirm = MessageBox.Show(
                    $"Bạn có chắc muốn {action} thẻ thư viện '{card.CardCode}'?",
                    "Xác nhận trạng thái thẻ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                if (currentlyLocked)
                {
                    card.Status = card.ExpiresAt.Date < DateTime.Today ? ReaderCardStatus.Expired : ReaderCardStatus.Active;
                }
                else
                {
                    card.Status = ReaderCardStatus.Locked;
                }

                if (!string.IsNullOrWhiteSpace(_txtNote.Text))
                    card.Note = _txtNote.Text.Trim();

                db.SaveChanges();
                LoadCards();
                CardDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thay đổi trạng thái thẻ:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnQuickIssueDialog_Click(object? sender, EventArgs e)
        {
            // Chuyển nhanh sang bộ lọc "Độc giả chưa có thẻ" để thủ thư cấp thẻ ngay
            _cboStatusFilter.SelectedIndex = 5;
            if (_dgvCards.Rows.Count == 0)
            {
                MessageBox.Show("Tất cả độc giả hiện tại đều đã được cấp thẻ thư viện.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _cboStatusFilter.SelectedIndex = 0;
            }
        }

        private void BtnPrintCard_Click(object? sender, EventArgs e)
        {
            if (_selectedCardId == null) return;

            using var dlg = new Form
            {
                Text = $"Xem trước thẻ thư viện - {_txtCardCode.Text}",
                Size = new Size(440, 290),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var cardBox = new Panel
            {
                Location = new Point(24, 20),
                Size = new Size(376, 175),
                BackColor = _pnlCardPreview.BackColor
            };

            cardBox.Controls.Add(new Label
            {
                Text = "THƯ VIỆN TRUNG TÂM LIBRA\nTHẺ ĐỘC GIẢ THƯ VIỆN",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 14)
            });
            cardBox.Controls.Add(new Label
            {
                Text = _lblPreviewCode.Text,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(254, 240, 138),
                AutoSize = true,
                Location = new Point(16, 62)
            });
            cardBox.Controls.Add(new Label
            {
                Text = _lblPreviewReader.Text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 98)
            });
            cardBox.Controls.Add(new Label
            {
                Text = _lblPreviewDates.Text,
                Font = new Font("Segoe UI", 8.8F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 132)
            });

            var btnClose = CreateActionButton("  Đóng", GCard, Primary, Color.White, 110);
            btnClose.Location = new Point(290, 208);
            btnClose.Click += (_, _) => dlg.Close();

            dlg.Controls.AddRange(new Control[] { cardBox, btnClose });
            dlg.ShowDialog(this);
        }

        // ============================================================
        //  HELPER
        // ============================================================
        private static string GenerateNextCardCode(LibraryDbContext db)
        {
            int nextId = (db.ReaderCards.Max(c => (int?)c.Id) ?? 0) + 1;
            string code = $"RC{nextId:D5}";
            while (db.ReaderCards.Any(c => c.CardCode == code))
            {
                nextId++;
                code = $"RC{nextId:D5}";
            }
            return code;
        }

        private void ClearActionPanel()
        {
            _selectedReaderId = null;
            _selectedCardId = null;
            _pnlCardPreview.BackColor = TextGray;
            _lblPreviewStatus.Text = "CHƯA CHỌN";
            _lblPreviewCode.Text = "MÃ THẺ: ---";
            _lblPreviewReader.Text = "Chủ thẻ: Chưa chọn độc giả";
            _lblPreviewDates.Text = "Ngày cấp: --/--/----   ·   Hạn thẻ: --/--/----";
            _txtCardCode.Clear();
            _txtNote.Clear();
            SetActionPanelEnabled(false);
        }

        private void SetActionPanelEnabled(bool enabled)
        {
            _txtCardCode.Enabled = enabled;
            _dtpIssuedAt.Enabled = enabled;
            _dtpExpiresAt.Enabled = enabled;
            _txtNote.Enabled = enabled;
            _btnIssueOrSave.Enabled = enabled;
            _cboExtendMonths.Enabled = enabled;
            _btnExtend.Enabled = enabled;
            _btnToggleLock.Enabled = enabled;
            _btnPrintCard.Enabled = enabled;
        }

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