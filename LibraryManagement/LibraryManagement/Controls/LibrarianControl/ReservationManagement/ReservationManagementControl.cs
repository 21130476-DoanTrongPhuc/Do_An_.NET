using System.Drawing.Drawing2D;
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Forms.ReservationManagement
{
    public partial class ReservationManagementControl : UserControl
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
        private const string GCalendar = "\uE787";
        private const string GRefresh  = "\uE72C";
        private const string GSearch   = "\uE721";
        private const string GCheck    = "\uE73E";
        private const string GMail     = "\uE715";
        private const string GClose    = "\uE711";
        private const string GBook     = "\uE82D";

        public event Action? ReservationDataChanged;

        // ====== Controls danh sách trái ======
        private TextBox _txtSearch = null!;
        private ComboBox _cboStatusFilter = null!;
        private DataGridView _dgvReservations = null!;
        private Label _lblSummary = null!;

        // ====== Controls xử lý duyệt bên phải ======
        private int? _selectedReservationId;
        private Label _lblResCode = null!;
        private Label _lblReaderInfo = null!;
        private Label _lblBookInfo = null!;
        private Label _lblTimeInfo = null!;
        private ComboBox _cboAvailableCopies = null!;
        private NumericUpDown _numHoldDays = null!;
        private TextBox _txtNotifyMessage = null!;
        private Button _btnApproveAndNotify = null!;
        private Button _btnResendNotify = null!;
        private Button _btnCancelRes = null!;

        // Lấy động giá trị enum theo project của bạn để tránh lệch tên hằng số
        private static readonly ReservationStatus StatusReadyToPick = ResolveReadyReservationStatus();
        private static readonly CirculationStatus CopyReservedStatus = ResolveReservedCopyStatus();

        public ReservationManagementControl()
        {
            BackColor = BgGray;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
            Load += (_, _) => LoadReservations();
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

            // 2. Nội dung chính: Trái (Danh sách đặt trước) - Phải (Duyệt & Thông báo)
            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };

            var pnlRightHost = new Panel { Dock = DockStyle.Right, Width = 400, Padding = new Padding(10, 0, 0, 0) };
            pnlRightHost.Controls.Add(BuildApprovalGroup());

            var grpList = new GroupBox
            {
                Text = "Danh sách yêu cầu đặt trước sách (Ưu tiên theo thời gian đăng ký)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10)
            };
            _dgvReservations = BuildGrid();
            grpList.Controls.Add(_dgvReservations);

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
                Text = "QUẢN LÝ ĐẶT TRƯỚC SÁCH & THÔNG BÁO NHẬN SÁCH",
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(14, 7)
            });

            _lblSummary = new Label
            {
                Text = "Đang tải danh sách đặt trước...",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(16, 31)
            };
            pnl.Controls.Add(_lblSummary);

            var btnRefresh = CreateActionButton("Làm mới dữ liệu", GRefresh, Primary, Color.White, 185);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (_, _) => LoadReservations();

            pnl.Controls.Add(btnRefresh);
            pnl.Resize += (_, _) => btnRefresh.Location = new Point(pnl.Width - btnRefresh.Width - 12, 11);

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
                PlaceholderText = "Mã đặt trước, tên độc giả hoặc tên sách..."
            };
            _txtSearch.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoadReservations(); };

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, Location = new Point(0, 16), ForeColor = TextGray };

            _cboStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 12),
                Width = 185
            };
            _cboStatusFilter.Items.AddRange(new object[]
            {
                "Tất cả yêu cầu",
                "Chờ duyệt (Pending)",
                "Đã duyệt - Sẵn sàng lấy",
                "Đã hoàn tất (Đã mượn)",
                "Đã hủy / Hết hạn giữ"
            });
            _cboStatusFilter.SelectedIndex = 1; // Mặc định lọc các phiếu đang chờ duyệt
            _cboStatusFilter.SelectedIndexChanged += (_, _) => LoadReservations();

            var btnSearch = CreateActionButton("  Lọc danh sách", GSearch, SoftBlue, Primary, 145);
            btnSearch.Location = new Point(0, 9);
            btnSearch.Height = 31;
            btnSearch.Click += (_, _) => LoadReservations();

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

        private GroupBox BuildApprovalGroup()
        {
            var grp = new GroupBox
            {
                Text = "Duyệt phiếu & Gửi thông báo cho độc giả",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12),
                BackColor = Color.White
            };

            // Khung thông tin phiếu đặt trước
            var pnlCard = new Panel
            {
                Location = new Point(14, 26),
                Size = new Size(350, 132),
                BackColor = SoftBlue,
                Padding = new Padding(10)
            };
            pnlCard.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);

            _lblResCode = new Label
            {
                Text = "Mã phiếu: --",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Primary,
                AutoSize = true,
                Location = new Point(10, 8)
            };
            _lblReaderInfo = new Label
            {
                Text = "Độc giả: Chưa chọn yêu cầu",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.Black,
                AutoSize = false,
                Size = new Size(330, 34),
                Location = new Point(10, 30)
            };
            _lblBookInfo = new Label
            {
                Text = "Sách đặt: --",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize = false,
                Size = new Size(330, 34),
                Location = new Point(10, 66)
            };
            _lblTimeInfo = new Label
            {
                Text = "Thời gian đặt: --",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(10, 104)
            };

            pnlCard.Controls.AddRange(new Control[] { _lblResCode, _lblReaderInfo, _lblBookInfo, _lblTimeInfo });
            grp.Controls.Add(pnlCard);

            int y = 170;

            // Chọn bản sách vật lý trên kệ
            grp.Controls.Add(new Label
            {
                Text = "1. Chọn bản sách vật lý sẵn sàng gán cho độc giả:",
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                Location = new Point(14, y)
            });
            y += 22;

            _cboAvailableCopies = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(14, y),
                Width = 350
            };
            _cboAvailableCopies.SelectedIndexChanged += (_, _) => UpdateNotificationTemplate();
            grp.Controls.Add(_cboAvailableCopies);
            y += 36;

            // Số ngày giữ sách
            var lblHoldDays = new Label
            {
                Text = "2. Thời hạn giữ sách tại quầy (ngày):",
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(14, y + 3)
            };
            grp.Controls.Add(lblHoldDays);

            _numHoldDays = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 14,
                Value = 3,
                Location = new Point(lblHoldDays.Right + 12, y),
                Width = 109
            };
            _numHoldDays.ValueChanged += (_, _) => UpdateNotificationTemplate();
            grp.Controls.Add(_numHoldDays);
            y += 36;

            // Nội dung thông báo gửi độc giả
            grp.Controls.Add(new Label
            {
                Text = "3. Nội dung thông báo gửi đến độc giả (Email / SMS):",
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                Location = new Point(14, y)
            });
            y += 22;

            _txtNotifyMessage = new TextBox
            {
                Location = new Point(14, y),
                Size = new Size(350, 96),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 8.8F)
            };
            grp.Controls.Add(_txtNotifyMessage);
            y += 108;

            // Các nút hành động
            _btnApproveAndNotify = CreateActionButton("  Duyệt & Thông báo sách sẵn sàng", GCheck, Green, Color.White, 350);
            _btnApproveAndNotify.Height = 38;
            _btnApproveAndNotify.Location = new Point(14, y);
            _btnApproveAndNotify.Click += BtnApproveAndNotify_Click;
            grp.Controls.Add(_btnApproveAndNotify);
            y += 46;

            _btnResendNotify = CreateActionButton("  Gửi lại thông báo", GMail, SoftBlue, Primary, 170);
            _btnResendNotify.Location = new Point(14, y);
            _btnResendNotify.Click += BtnResendNotify_Click;

            _btnCancelRes = CreateActionButton("  Hủy yêu cầu", GClose, Color.White, Red, 170);
            _btnCancelRes.Location = new Point(194, y);
            _btnCancelRes.Click += BtnCancelRes_Click;

            grp.Controls.Add(_btnResendNotify);
            grp.Controls.Add(_btnCancelRes);

            SetApprovalPanelEnabled(false);
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
                RowTemplate = { Height = 42 },
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

            g.Columns.Add("colId",       "ID");
            g.Columns.Add("colCode",     "Mã đặt trước");
            g.Columns.Add("colReader",   "Độc giả");
            g.Columns.Add("colBook",     "Đầu sách đặt");
            g.Columns.Add("colReserved", "Ngày đặt");
            g.Columns.Add("colAvail",    "Kho sẵn sàng");
            g.Columns.Add("colCopy",     "Bản sách gán");
            g.Columns.Add("colStatus",   "Trạng thái");

            g.Columns["colId"]!.Visible = false;
            g.Columns["colCode"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colCode"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            g.Columns["colCode"]!.FillWeight     = 80;
            g.Columns["colReader"]!.FillWeight   = 110;
            g.Columns["colBook"]!.FillWeight     = 130;
            g.Columns["colReserved"]!.FillWeight = 80;
            g.Columns["colAvail"]!.FillWeight    = 75;
            g.Columns["colCopy"]!.FillWeight     = 80;
            g.Columns["colStatus"]!.FillWeight   = 105;

            g.CellPainting += DgvReservations_CellPainting;
            g.SelectionChanged += DgvReservations_SelectionChanged;
            return g;
        }

        private void DgvReservations_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 7 || e.Graphics == null) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch
            {
                "Chờ duyệt"      => Orange,
                "Sẵn sàng lấy"   => Green,
                "Đã nhận sách"   => Primary,
                "Đã hủy"         => Red,
                "Hết hạn giữ"    => Red,
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
        //  TẢI DỮ LIỆU & NGHIỆP VỤ DUYỆT ĐẶT TRƯỚC
        // ============================================================
        public void LoadReservations()
        {
            try
            {
                using var context = new LibraryDbContext();
                var today = DateTime.Today;
                string keyword = _txtSearch.Text.Trim().ToLower();
                int filterIndex = _cboStatusFilter.SelectedIndex;

                var list = context.Reservations
                    .Include(r => r.Reader)
                    .Include(r => r.Book)
                    .Include(r => r.BookCopy)
                    .Select(r => new
                    {
                        r.Id,
                        r.ReservationCode,
                        ReaderCode = r.Reader.UserCode,
                        ReaderName = r.Reader.FullName,
                        BookCode = r.Book.BookCode,
                        BookTitle = r.Book.Title,
                        r.ReservedAt,
                        r.ExpiresAt,
                        r.FulfilledAt,
                        r.CancelledAt,
                        r.Status,
                        AssignedCopyCode = r.BookCopy != null ? r.BookCopy.CopyCode : null,
                        AvailableCount = context.BookCopies.Count(c =>
                            c.IdBook == r.IdBook && c.CirculationStatus == CirculationStatus.Available)
                    })
                    .OrderBy(r => r.Status == ReservationStatus.Pending ? 0 : 1)
                    .ThenBy(r => r.ReservedAt) // Ưu tiên người đặt trước
                    .ToList();

                int pendingCount = list.Count(x => x.Status == ReservationStatus.Pending);
                int readyCount   = list.Count(x => x.Status == ReservationStatus.ReadyForPickup && x.FulfilledAt == null && x.CancelledAt == null);
               
                var filtered = list.Where(r =>
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        bool match = r.ReservationCode.ToLower().Contains(keyword)
                                  || r.ReaderName.ToLower().Contains(keyword)
                                  || r.ReaderCode.ToLower().Contains(keyword)
                                  || r.BookTitle.ToLower().Contains(keyword)
                                  || r.BookCode.ToLower().Contains(keyword);
                        if (!match) return false;
                    }

                    // Trong LoadReservations():
                    int pendingCount = list.Count(x => x.Status == ReservationStatus.Pending);
                    int readyCount   = list.Count(x => x.Status == ReservationStatus.ReadyForPickup && x.FulfilledAt == null && x.CancelledAt == null);
                    
                    return filterIndex switch
                    {
                        1 => r.Status == ReservationStatus.Pending,
                        2 => r.Status == ReservationStatus.ReadyForPickup,
                        3 => r.Status == ReservationStatus.Completed || r.FulfilledAt != null,
                        4 => r.Status == ReservationStatus.Cancelled || r.Status == ReservationStatus.Expired || r.CancelledAt != null,
                        _ => true
                    };
                }).ToList();

                _lblSummary.Text = $"Chờ duyệt: {pendingCount:N0} yêu cầu  ·  Đang giữ chờ độc giả lấy: {readyCount:N0} cuốn  ·  Hiển thị: {filtered.Count:N0} phiếu";

                _dgvReservations.Rows.Clear();
                foreach (var r in filtered)
                {
                    string statusText = MapReservationStatusText(r.Status, r.ExpiresAt, r.FulfilledAt, r.CancelledAt, today);
                    string availText = r.AvailableCount > 0 ? $"{r.AvailableCount} cuốn" : "Hết trên kệ";

                    _dgvReservations.Rows.Add(
                        r.Id,
                        r.ReservationCode,
                        $"{r.ReaderName} ({r.ReaderCode})",
                        r.BookTitle,
                        r.ReservedAt.ToString("dd/MM/yyyy HH:mm"),
                        availText,
                        r.AssignedCopyCode ?? "Chưa gán",
                        statusText
                    );
                }

                if (_dgvReservations.Rows.Count > 0)
                    _dgvReservations.Rows[0].Selected = true;
                else
                    ClearApprovalPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách đặt trước sách:\n{ex.Message}", "Lỗi CSDL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvReservations_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dgvReservations.SelectedRows.Count == 0)
            {
                ClearApprovalPanel();
                return;
            }

            if (_dgvReservations.SelectedRows[0].Cells["colId"].Value is not int resId) return;
            LoadReservationDetail(resId);
        }

        private void LoadReservationDetail(int reservationId)
        {
            try
            {
                using var context = new LibraryDbContext();
                var res = context.Reservations
                    .Include(r => r.Reader)
                    .Include(r => r.Book)
                    .Include(r => r.BookCopy)
                    .FirstOrDefault(r => r.Id == reservationId);

                if (res == null) return;

                _selectedReservationId = res.Id;
                _lblResCode.Text = $"Mã phiếu đặt: {res.ReservationCode}";
                string contact = !string.IsNullOrWhiteSpace(res.Reader.Email) ? res.Reader.Email : (res.Reader.Phone ?? "Chưa có SĐT/Email");
                _lblReaderInfo.Text = $"Độc giả: {res.Reader.FullName} ({res.Reader.UserCode}) · {contact}";
                _lblBookInfo.Text = $"Sách đặt: [{res.Book.BookCode}] {res.Book.Title}";
                _lblTimeInfo.Text = res.ExpiresAt != null
                    ? $"Đặt lúc: {res.ReservedAt:dd/MM/yyyy HH:mm}  ·  Hạn giữ đến: {res.ExpiresAt:dd/MM/yyyy}"
                    : $"Đặt lúc: {res.ReservedAt:dd/MM/yyyy HH:mm}";

                // Tải danh sách bản sách vật lý đang Available của đầu sách này
                var availableCopies = context.BookCopies
                    .Where(c => c.IdBook == res.IdBook && c.CirculationStatus == CirculationStatus.Available)
                    .OrderBy(c => c.CopyCode)
                    .Select(c => new CopyOption
                    {
                        Id = c.Id,
                        Display = $"{c.CopyCode} — Kệ: {c.ShelfLocation ?? "Chưa xếp kệ"} ({c.PhysicalCondition})"
                    })
                    .ToList();

                _cboAvailableCopies.DataSource = null;
                if (res.BookCopy != null)
                {
                    // Trường hợp phiếu đã được duyệt gán bản sách trước đó
                    _cboAvailableCopies.DataSource = new List<CopyOption>
                    {
                        new CopyOption
                        {
                            Id = res.BookCopy.Id,
                            Display = $"{res.BookCopy.CopyCode} — Đã giữ cho độc giả (Kệ: {res.BookCopy.ShelfLocation ?? "N/A"})"
                        }
                    };
                    _cboAvailableCopies.DisplayMember = nameof(CopyOption.Display);
                    _cboAvailableCopies.ValueMember = nameof(CopyOption.Id);
                }
                else if (availableCopies.Count > 0)
                {
                    _cboAvailableCopies.DataSource = availableCopies;
                    _cboAvailableCopies.DisplayMember = nameof(CopyOption.Display);
                    _cboAvailableCopies.ValueMember = nameof(CopyOption.Id);
                }
                else
                {
                    _cboAvailableCopies.Items.Clear();
                    _cboAvailableCopies.Items.Add("-- Hiện không còn bản sách nào sẵn sàng trên kệ --");
                    _cboAvailableCopies.SelectedIndex = 0;
                }

                bool isPending = res.Status == ReservationStatus.Pending;
                bool isReady   = res.Status == ReservationStatus.ReadyForPickup;

                _cboAvailableCopies.Enabled = isPending && availableCopies.Count > 0;
                _numHoldDays.Enabled         = isPending && availableCopies.Count > 0;
                _txtNotifyMessage.Enabled    = isPending || isReady;
                _btnApproveAndNotify.Enabled = isPending && availableCopies.Count > 0;
                _btnResendNotify.Enabled     = isReady;
                _btnCancelRes.Enabled        = isPending || isReady;

                UpdateNotificationTemplate(res);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết phiếu đặt trước:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateNotificationTemplate(Reservations? loadedRes = null)
        {
            if (_selectedReservationId == null) return;

            string copyCode = (_cboAvailableCopies.SelectedItem as CopyOption)?.Display.Split('—')[0].Trim() ?? "bản sách đã giữ";
            DateTime holdUntil = DateTime.Today.AddDays((int)_numHoldDays.Value);

            if (loadedRes != null && !string.IsNullOrWhiteSpace(loadedRes.Note) && loadedRes.Status != ReservationStatus.Pending)
            {
                _txtNotifyMessage.Text = loadedRes.Note;
                return;
            }

            string readerName = _lblReaderInfo.Text.Replace("Độc giả:", "").Split('(')[0].Trim();
            string bookTitle  = _lblBookInfo.Text.Replace("Sách đặt:", "").Trim();

            _txtNotifyMessage.Text =
                $"Xin chào {readerName},\r\n" +
                $"Cuốn sách \"{bookTitle}\" (Mã bản sách: {copyCode}) bạn đặt trước tại Thư viện LIBRA đã sẵn sàng.\r\n" +
                $"Vui lòng đến quầy thủ thư nhận sách trước ngày {holdUntil:dd/MM/yyyy}. Sau thời hạn này, hệ thống sẽ tự động hủy giữ chỗ.";
        }

        private void BtnApproveAndNotify_Click(object? sender, EventArgs e)
        {
            if (_selectedReservationId == null) return;
            if (_cboAvailableCopies.SelectedItem is not CopyOption selectedCopy)
            {
                MessageBox.Show("Không có bản sách sẵn sàng trên kệ để duyệt phiếu này.", "Không thể duyệt",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var context = new LibraryDbContext();
                var res = context.Reservations
                    .Include(r => r.Reader)
                    .Include(r => r.Book)
                    .FirstOrDefault(r => r.Id == _selectedReservationId.Value);

                var copy = context.BookCopies.FirstOrDefault(c => c.Id == selectedCopy.Id);

                if (res == null || copy == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu đặt trước hoặc bản sách.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (copy.CirculationStatus != CirculationStatus.Available)
                {
                    MessageBox.Show("Bản sách này vừa được người khác mượn hoặc giữ chỗ. Vui lòng làm mới danh sách.",
                        "Xung đột dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadReservations();
                    return;
                }

                var now = DateTime.Now;
                var oldCopyStatus = copy.CirculationStatus;

                // 1. Cập nhật phiếu đặt trước
                res.IdBookCopy = copy.Id;
                res.AssignedAt = now;
                res.ExpiresAt  = DateTime.Today.AddDays((int)_numHoldDays.Value).AddHours(20);
                res.Status     = ReservationStatus.ReadyForPickup;
                res.Note       = _txtNotifyMessage.Text.Trim();

                // 2. Cập nhật trạng thái bản sách vật lý sang giữ chỗ
                copy.CirculationStatus = CirculationStatus.Reserved;
                copy.UpdatedAt = now;

                // 3. Ghi lịch sử biến động bản sách (BookCopyHistory)
                context.BookCopyHistories.Add(new BookCopyHistory
                {
                    IdBookCopy   = copy.Id,
                    ChangedAt    = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ActionType   = $"Giữ đặt trước ({res.ReservationCode})",
                    OldStatus    = oldCopyStatus,
                    NewStatus    = CirculationStatus.Reserved,
                    OldCondition = copy.PhysicalCondition,
                    NewCondition = copy.PhysicalCondition,
                    Note         = $"Độc giả: {res.Reader.FullName} ({res.Reader.UserCode})"
                });

                context.SaveChanges();

                string targetContact = !string.IsNullOrWhiteSpace(res.Reader.Email)
                    ? $"Email: {res.Reader.Email}"
                    : (!string.IsNullOrWhiteSpace(res.Reader.Phone) ? $"SĐT: {res.Reader.Phone}" : "Hệ thống thông báo nội bộ");

                MessageBox.Show(
                    $"Đã duyệt phiếu đặt trước {res.ReservationCode} thành công!\n\n" +
                    $"• Bản sách gán: {copy.CopyCode}\n" +
                    $"• Giữ đến hết ngày: {res.ExpiresAt:dd/MM/yyyy}\n" +
                    $"• Đã gửi thông báo sách sẵn sàng tới độc giả ({targetContact}).",
                    "Duyệt & Thông báo thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadReservations();
                ReservationDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                string detail = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Lỗi khi duyệt phiếu đặt trước:\n{detail}", "Lỗi CSDL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnResendNotify_Click(object? sender, EventArgs e)
        {
            if (_selectedReservationId == null) return;

            try
            {
                using var context = new LibraryDbContext();
                var res = context.Reservations
                    .Include(r => r.Reader)
                    .FirstOrDefault(r => r.Id == _selectedReservationId.Value);

                if (res == null) return;

                res.Note = _txtNotifyMessage.Text.Trim();
                context.SaveChanges();

                string contact = res.Reader.Email ?? res.Reader.Phone ?? res.Reader.FullName;
                MessageBox.Show($"Đã gửi lại thông báo nhắc nhận sách đến độc giả: {contact}.",
                    "Đã gửi thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi lại thông báo:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelRes_Click(object? sender, EventArgs e)
        {
            if (_selectedReservationId == null) return;

            var confirm = MessageBox.Show(
                "Bạn có chắc muốn hủy phiếu đặt trước này?\nNếu đã gán bản sách vật lý, sách sẽ được trả về trạng thái sẵn sàng trên kệ.",
                "Xác nhận hủy đặt trước",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var context = new LibraryDbContext();
                var res = context.Reservations
                    .Include(r => r.BookCopy)
                    .FirstOrDefault(r => r.Id == _selectedReservationId.Value);

                if (res == null) return;

                var now = DateTime.Now;
                res.CancelledAt = now;
                res.Status = ReservationStatus.Cancelled;

                // Giải phóng bản sách vật lý nếu đã gán trước đó
                if (res.BookCopy != null && res.BookCopy.CirculationStatus == CopyReservedStatus)
                {
                    var oldStatus = res.BookCopy.CirculationStatus;
                    res.BookCopy.CirculationStatus = CirculationStatus.Available;
                    res.BookCopy.UpdatedAt = now;

                    context.BookCopyHistories.Add(new BookCopyHistory
                    {
                        IdBookCopy = res.BookCopy.Id,
                        ChangedAt  = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        ActionType = "ReserveCancelled",
                        OldStatus  = oldStatus,
                        NewStatus  = CirculationStatus.Available,
                        Note       = $"Hủy phiếu đặt trước {res.ReservationCode}, trả sách về kệ"
                    });
                }

                context.SaveChanges();
                MessageBox.Show("Đã hủy phiếu đặt trước thành công.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadReservations();
                ReservationDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy phiếu đặt trước:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  HELPER & XỬ LÝ ENUM AN TOÀN
        // ============================================================
        private static ReservationStatus ResolveReadyReservationStatus()
        {
            // Hỗ trợ linh hoạt dù enum của bạn đặt tên là Ready, Assigned, Approved hay Confirmed
            string[] candidates = { "Ready", "ReadyForPickup", "Assigned", "Approved", "Confirmed", "Notified" };
            foreach (var name in candidates)
            {
                if (Enum.TryParse<ReservationStatus>(name, true, out var val))
                    return val;
            }
            var values = Enum.GetValues<ReservationStatus>();
            return values.Length > 1 ? values[1] : ReservationStatus.Pending;
        }

        private static CirculationStatus ResolveReservedCopyStatus()
        {
            string[] candidates = { "Reserved", "OnHold", "Held" };
            foreach (var name in candidates)
            {
                if (Enum.TryParse<CirculationStatus>(name, true, out var val))
                    return val;
            }
            return CirculationStatus.Available;
        }

        private static string MapReservationStatusText(
            ReservationStatus status, DateTime? expiresAt, DateTime? fulfilledAt, DateTime? cancelledAt, DateTime today)
        {
            if (status == ReservationStatus.Completed || fulfilledAt != null) return "Đã nhận sách";
            if (status == ReservationStatus.Cancelled || cancelledAt != null) return "Đã hủy";
            if (status == ReservationStatus.Expired || (expiresAt != null && expiresAt.Value.Date < today)) return "Hết hạn giữ";
            if (status == ReservationStatus.ReadyForPickup) return "Sẵn sàng lấy";
            return "Chờ duyệt";
        }

        private void ClearApprovalPanel()
        {
            _selectedReservationId = null;
            _lblResCode.Text = "Mã phiếu: --";
            _lblReaderInfo.Text = "Độc giả: Chưa chọn yêu cầu";
            _lblBookInfo.Text = "Sách đặt: --";
            _lblTimeInfo.Text = "Thời gian đặt: --";
            _cboAvailableCopies.DataSource = null;
            _cboAvailableCopies.Items.Clear();
            _txtNotifyMessage.Clear();
            SetApprovalPanelEnabled(false);
        }

        private void SetApprovalPanelEnabled(bool enabled)
        {
            _cboAvailableCopies.Enabled = enabled;
            _numHoldDays.Enabled         = enabled;
            _txtNotifyMessage.Enabled    = enabled;
            _btnApproveAndNotify.Enabled = enabled;
            _btnResendNotify.Enabled     = enabled;
            _btnCancelRes.Enabled        = enabled;
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

        private sealed class CopyOption
        {
            public int Id { get; set; }
            public string Display { get; set; } = string.Empty;
        }
    }
}