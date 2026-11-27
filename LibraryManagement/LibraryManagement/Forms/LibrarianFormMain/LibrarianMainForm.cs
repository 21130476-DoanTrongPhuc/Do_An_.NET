using System.Drawing.Drawing2D;
using LibraryManagement.Controls;
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Forms.Books;
using LibraryManagement.Forms.FineManagement;
using LibraryManagement.Forms.LibraryCardManagement;
using LibraryManagement.Forms.ReaderManagement;
using LibraryManagement.Forms.ReservationManagement;
using LibraryManagement.Helpers;
using LibraryManagement.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Forms.LibrarianFormMain
{
    public partial class LibrarianFormMain : Form
    {
        // ====== Màu sắc ======
        private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
        private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
        private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
        private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
        private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
        private static readonly Color Green      = Color.FromArgb(34, 160, 90);
        private static readonly Color Red        = Color.FromArgb(200, 40, 40);
        private const string IconFont = "Segoe MDL2 Assets";

        // Glyph (Segoe MDL2 Assets)
        private const string GHome     = "\uE80F";
        private const string GBook     = "\uE82D";
        private const string GPeople   = "\uE716";
        private const string GForward  = "\uE72A";
        private const string GBack     = "\uE72B";
        private const string GRefresh  = "\uE72C";
        private const string GHelp     = "\uE897";
        private const string GWarn     = "\uE7BA";
        private const string GCalendar = "\uE787";
        private const string GReport   = "\uE9F9";
        private const string GMoney    = "\uE8C7";
        private const string GReturn   = "\uE7A7";

        public bool IsLogout { get; private set; }

        // ====== Vùng nội dung động ======
        private Panel _pnlContent = null!;
        private TabControl _tabMain = null!;
        private TabPage? _tabLoan;
        private TabPage? _tabReport;
        private TabPage? _tabReturn;
        private TabPage? _tabBookCatalog;
        private TabPage? _tabBookCopies;
        private TabPage? _tabCategories;
        private TabPage? _tabOverdue;
        private TabPage? _tabFines;
        private TabPage? _tabReaders;
        private TabPage? _tabReservations;
        private TabPage? _tabReaderCards;

        // ====== Control trong trang Tổng quan ======
        private TableLayoutPanel _tblStats = null!;
        private DataGridView _dgvTrans = null!;
        private FlowLayoutPanel _flowNotice = null!;
        private Label _lblDate = null!;
        private Label _lblUserName = null!;
        private ToolStripStatusLabel _lblStatusRole = null!;

        public LibrarianFormMain()
        {
            InitializeComponent();
            BuildUi();
            Load += (_, _) =>
            {
                Text = "LIBRA - Quản lý thư viện";
                ShowOverviewPage();
            };
        }

        // ============================================================
        //  ĐIỀU HƯỚNG
        // ============================================================
        private void ShowOverviewPage()
        {
            _pnlContent.SuspendLayout();
            _pnlContent.Controls.Clear();

            // Reset toàn bộ tab cache vì TabControl bị tạo lại
            ResetAllTabCaches();

            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Padding = new Point(20, 5), // Chừa thêm khoảng trống bên phải cho nút ×
                DrawMode = TabDrawMode.OwnerDrawFixed
            };

            // Đăng ký các sự kiện vẽ nút × và xử lý click đóng tab
            _tabMain.DrawItem += TabMain_DrawItem;
            _tabMain.MouseDown += TabMain_MouseDown;
            _tabMain.MouseMove += TabMain_MouseMove;
            _tabMain.MouseLeave += (_, _) => { _hoveredCloseTabIndex = -1; _tabMain.Invalidate(); };

            var tabOverview = new TabPage("Tổng quan")
            {
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(8)
            };
            _tabMain.TabPages.Add(tabOverview);
            BuildOverview(tabOverview);

            _pnlContent.Controls.Add(_tabMain);
            _pnlContent.ResumeLayout(true);
            LoadData();
        }

        private void ShowUserControl(UserControl uc)
        {
            _pnlContent.SuspendLayout();
            _pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            _pnlContent.Controls.Add(uc);
            _pnlContent.ResumeLayout(true);
        }

        private void ShowBookReturn()
        {
            var uc = AppServices.Provider.GetRequiredService<BookFormReturn.BookFormReturn>();
            uc.CloseRequested += (_, _) => ShowOverviewPage();
            ShowUserControl(uc);
        }

        private void ShowOverdueBooks()
        {
            var uc = AppServices.Provider.GetRequiredService<OverdueBooks.OverdueBooks>();
            uc.CloseRequested += (_, _) => ShowOverviewPage();
            uc.ReturnBookRequested += (_, _) => ShowBookReturn();
            ShowUserControl(uc);
        }

        private void ShowFineManagement()
        {
            var uc = new FineManagement.FineManagement();
            ShowUserControl(uc);
        }

        // ============================================================
        //  DỮ LIỆU
        // ============================================================
        private void LoadData()
        {
            var d = DateTime.Now;
            string[] days = { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
            _lblDate.Text = $"{days[(int)d.DayOfWeek]}, ngày {d:dd} tháng {d:MM} năm {d:yyyy}";

            try
            {
                using var context = new LibraryDbContext();
                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);

                // 1. Thông tin thủ thư đang đăng nhập
                var currentUser = context.Users
                    .Where(u => u.Id == Session.CurrentLibrarianId)
                    .Select(u => new { u.FullName, u.UserType, u.UserCode })
                    .FirstOrDefault();

                if (_lblUserName != null)
                    _lblUserName.Text = currentUser?.FullName ?? "Thủ thư";
                if (_lblStatusRole != null)
                {
                    _lblStatusRole.Text = currentUser != null
                        ? $"Thủ thư: {currentUser.UserCode}"
                        : "Thủ thư";
                }

                // 2. Thống kê tổng quan
                int totalBooks  = context.Books.Count();
                int totalCopies = context.BookCopies.Count();

                int activeReaders = context.Users.Count(u => u.UserType == UserType.Reader && u.Status == UserStatus.Active);
                int newReadersThisMonth = context.Users
                    .Where(u => u.UserType == UserType.Reader)
                    .Select(u => u.CreatedAt)
                    .AsEnumerable()
                    .Count(created => created >= monthStart);

                var unreturned = context.LoanDetails
                    .Where(ld => ld.ReturnedAt == null)
                    .Select(ld => new { ld.DueAt, BorrowedAt = ld.Loan.BorrowedAt })
                    .ToList();

                int borrowingCount    = unreturned.Count;
                int borrowedToday     = unreturned.Count(ld => ld.BorrowedAt.Date == today);
                int overdueCount      = unreturned.Count(ld => ld.DueAt.Date < today);

                _tblStats.Controls.Clear();
                AddStat(0, GBook,   "Tổng đầu sách",       totalBooks.ToString("N0"),    $"{totalCopies:N0} bản sách vật lý", false);
                AddStat(1, GPeople, "Độc giả hoạt động",   activeReaders.ToString("N0"), $"+{newReadersThisMonth} độc giả mới tháng này", false);
                AddStat(2, GForward,"Sách đang mượn",      borrowingCount.ToString("N0"),$"{borrowedToday} lượt mượn hôm nay", false);
                AddStat(3, GWarn,   "Sách quá hạn",       overdueCount.ToString("N0"),  overdueCount > 0 ? "Cần xử lý ngay" : "Không có quá hạn", overdueCount > 0);

                // 3. Giao dịch gần đây
                var recentDetails = context.LoanDetails
                    .OrderByDescending(ld => ld.Loan.BorrowedAt)
                    .ThenByDescending(ld => ld.Id)
                    .Take(15)
                    .Select(ld => new
                    {
                        ld.Loan.LoanCode,
                        ReaderName = ld.Loan.Reader.FullName,
                        BookTitle  = ld.BookCopy.Book.Title,
                        ld.Loan.BorrowedAt,
                        ld.DueAt,
                        ld.ReturnedAt
                    })
                    .ToList();

                _dgvTrans.Rows.Clear();
                foreach (var item in recentDetails)
                {
                    string statusText =
                        item.ReturnedAt != null                                ? "Đã trả" :
                        item.DueAt.Date < today                                ? "Quá hạn" :
                        (item.DueAt.Date - today).Days <= 3                    ? "Sắp đến hạn" :
                                                                                 "Đang mượn";

                    _dgvTrans.Rows.Add(
                        item.LoanCode,
                        item.ReaderName,
                        item.BookTitle,
                        item.BorrowedAt.ToString("dd/MM/yyyy"),
                        item.DueAt.ToString("dd/MM/yyyy"),
                        statusText);
                }
                _dgvTrans.ClearSelection();

                // 4. Thông báo động
                _flowNotice.Controls.Clear();

                if (overdueCount > 0)
                    AddNotice(GWarn, Red, $"{overdueCount} cuốn sách đã quá hạn trả",
                        "Cần liên hệ độc giả và xử lý thu hồi sách", "Kiểm tra", OpenOverdueTab);

                int expiringCards = context.ReaderCards
                    .Count(rc => rc.Status == ReaderCardStatus.Active && rc.ExpiresAt >= today && rc.ExpiresAt <= today.AddDays(30));
                if (expiringCards > 0)
                    AddNotice(GPeople, Primary, $"{expiringCards} thẻ độc giả sắp hết hạn",
                        "Sẽ hết hạn trong vòng 30 ngày tới", "Xem báo cáo", OpenReportTab);

                int lowStock = context.Books.Count(b => b.Copies.Count(c => c.CirculationStatus == CirculationStatus.Available) < 2);
                if (lowStock > 0)
                    AddNotice(GBook ,Orange, $"{lowStock} đầu sách sắp hết bản sẵn sàng",
                        "Số lượng bản có thể mượn dưới 2 cuốn", "Xem bản sách", OpenBookCopiesTab);

                int unpaidFines = context.Fines.Count(f => f.Status == FineStatus.Unpaid || f.Status == FineStatus.Partial);                if (unpaidFines > 0)
                    AddNotice(GMoney, Orange, $"{unpaidFines} khoản phạt chưa thanh toán",
                        "Độc giả còn nợ phí phạt quá hạn / hư hỏng", "Thu phạt", OpenFinesTab);

                if (_flowNotice.Controls.Count == 0)
                    AddNotice(GBook, Green, "Hệ thống hoạt động ổn định",
                        "Hiện không có phiếu quá hạn hay cảnh báo tồn đọng", "Làm mới", LoadData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu trang chủ thủ thư:\n{ex.Message}",
                    "Lỗi kết nối CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  DỰNG GIAO DIỆN
        // ============================================================
        private void BuildUi()
        {
            SuspendLayout();

            _pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 242, 245) };
            Controls.Add(_pnlContent);

            Controls.Add(BuildStatusBar()); // Bottom
            Controls.Add(BuildToolbar());   // Top
            Controls.Add(BuildMenu());      // Top (trên cùng)

            ResumeLayout(true);
        }

        private MenuStrip BuildMenu()
        {
            var menu = new MenuStrip { BackColor = Color.White, Dock = DockStyle.Top };

            var mnuSystem = new ToolStripMenuItem("Hệ thống");
            mnuSystem.DropDownItems.Add("Trang chủ tổng quan", null, (_, _) => ShowOverviewPage());
            mnuSystem.DropDownItems.Add("Làm mới dữ liệu", null, (_, _) => LoadData());
            mnuSystem.DropDownItems.Add(new ToolStripSeparator());
            mnuSystem.DropDownItems.Add("Đăng xuất", null, btnLogout_Click);

            var mnuCatalog = new ToolStripMenuItem("Danh mục");
            mnuCatalog.DropDownItems.Add("Quản lý đầu sách", null, (_, _) => OpenBookCatalogTab());
            mnuCatalog.DropDownItems.Add("Quản lý thể loại", null, (_, _) => OpenCategoryTab());
            mnuCatalog.DropDownItems.Add("Quản lý bản sách", null, (_, _) => OpenBookCopiesTab());
            mnuCatalog.DropDownItems.Add("Thêm độc giả", null, (_, _) => OpenReaderRegistration());
            mnuCatalog.DropDownItems.Add("Quản lý độc giả", null, (_, _) => OpenReadersTab());
            mnuCatalog.DropDownItems.Add("Quản lý thẻ thư viện", null, (_, _) => OpenReaderCardsTab());

            var mnuCirculation = new ToolStripMenuItem("Lưu thông");
            mnuCirculation.DropDownItems.Add("Quản lý mượn sách", null, (_, _) => OpenLoanTab());
            mnuCirculation.DropDownItems.Add("Nhận trả sách", null, (_, _) => OpenReturnTab());
            mnuCirculation.DropDownItems.Add("Sách quá hạn", null, (_, _) => OpenOverdueTab());
            mnuCirculation.DropDownItems.Add("Quản lý tiền phạt", null, (_, _) => OpenFinesTab());
            mnuCirculation.DropDownItems.Add("Duyệt đặt trước sách", null, (_, _) => OpenReservationsTab());

            var mnuReport = new ToolStripMenuItem("Báo cáo");
            mnuReport.DropDownItems.Add("Báo cáo & Thống kê", null, (_, _) => OpenReportTab());

            var mnuHelp = new ToolStripMenuItem("Trợ giúp");
            mnuHelp.DropDownItems.Add("Hướng dẫn sử dụng", null, (_, _) =>
                MessageBox.Show("Hệ thống Quản lý Thư viện LIBRA - Phân hệ Thủ thư.", "Trợ giúp",
                    MessageBoxButtons.OK, MessageBoxIcon.Information));

            menu.Items.AddRange([mnuSystem, mnuCatalog, mnuCirculation, mnuReport, mnuHelp]);
            MainMenuStrip = menu;
            return menu;
        }

        private Panel BuildToolbar()
        {
            var host = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.FromArgb(246, 247, 249) };
            host.Paint += (_, e) => e.Graphics.DrawLine(new Pen(BorderGray), 0, host.Height - 1, host.Width, host.Height - 1);

            var ts = new ToolStrip
            {
                Dock = DockStyle.Fill, GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.FromArgb(246, 247, 249),
                ImageScalingSize = new Size(24, 24), Padding = new Padding(6, 4, 0, 0)
            };
            AddTool(ts, "Trang chủ",       GHome,    ShowOverviewPage);
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Đầu sách",        GBook,    OpenBookCatalogTab);
            AddTool(ts, "Bản sách",        GBook,    OpenBookCopiesTab);
            AddTool(ts, "Thêm độc giả",    GPeople,  OpenReaderRegistration);
            AddTool(ts, "Độc giả", GPeople, OpenReadersTab);
            AddTool(ts, "Thẻ thư viện", GMoney, OpenReaderCardsTab);
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Lập phiếu mượn",  GForward, OpenLoanTab);
            AddTool(ts, "Nhận trả sách",   GBack,    OpenReturnTab);
            AddTool(ts, "Quá hạn",         GWarn,    OpenOverdueTab);
            AddTool(ts, "Đặt trước", GCalendar, OpenReservationsTab);
            AddTool(ts, "Tiền phạt",       GMoney,   OpenFinesTab);
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Báo cáo",         GReport,  OpenReportTab);
            AddTool(ts, "Làm mới",         GRefresh, LoadData);

            // Khu người dùng bên phải
            var user = new Panel { Dock = DockStyle.Right, Width = 340 };
            // user.Controls.Add(new Label
            // {
            //     Text = "Người dùng:", AutoSize = true, ForeColor = TextGray,
            //     Font = new Font("Segoe UI", 9F), Location = new Point(8, 22)
            // });
            // _lblUserName = new Label
            // {
            //     AutoSize = false, AutoEllipsis = true, Size = new Size(140, 20),
            //     Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            //     Location = new Point(84, 22)
            // };
            // user.Controls.Add(_lblUserName);

            var btnLogout = new Button
            {
                Text = "Đăng xuất", FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                Size = new Size(92, 32), Location = new Point(user.Width - 104, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.FlatAppearance.BorderColor = BorderGray;
            btnLogout.Click += btnLogout_Click;
            user.Controls.Add(btnLogout);

            host.Controls.Add(ts);
            host.Controls.Add(user);
            return host;
        }

        private StatusStrip BuildStatusBar()
        {
            var st = new StatusStrip
            {
                Dock = DockStyle.Bottom, SizingGrip = true,
                BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Segoe UI", 8F)
            };

            _lblStatusRole = new ToolStripStatusLabel(string.Empty);

            st.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripStatusLabel("●") { ForeColor = Green },
                new ToolStripStatusLabel("Đã kết nối CSDL"),
                new ToolStripSeparator(),
                new ToolStripStatusLabel("Thư viện Trung tâm · Giờ mở cửa: 07:30 - 20:00"),
                new ToolStripStatusLabel { Spring = true },
                new ToolStripSeparator(),
                _lblStatusRole,
                new ToolStripSeparator(),
                new ToolStripStatusLabel("Phiên bản 2.4.0")
            });
            return st;
        }

        private void BuildOverview(TabPage page)
        {
            // Khối dưới: giao dịch + thông báo
            var pnlBottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };

            var grpTrans = new GroupBox { Text = "Giao dịch gần đây", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 10) };
            _dgvTrans = BuildGrid();
            grpTrans.Controls.Add(_dgvTrans);

            var noticeHost = new Panel { Dock = DockStyle.Right, Width = 560, Padding = new Padding(12, 0, 0, 0) };
            var grpNotice = new GroupBox { Text = "Thông báo và nhắc việc", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 10) };
            _flowNotice = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true
            };
            _flowNotice.Resize += (_, _) =>
            {
                foreach (Control c in _flowNotice.Controls) c.Width = _flowNotice.ClientSize.Width - 8;
            };
            grpNotice.Controls.Add(_flowNotice);
            noticeHost.Controls.Add(grpNotice);

            pnlBottom.Controls.Add(grpTrans);   // Fill
            pnlBottom.Controls.Add(noticeHost); // Right

            // Khối giữa: thống kê + tác vụ nhanh
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 168, Padding = new Padding(0, 10, 0, 0) };

            var grpStats = new GroupBox { Text = "Thống kê hôm nay", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 10) };
            _tblStats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) _tblStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _tblStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grpStats.Controls.Add(_tblStats);

            var quickHost = new Panel { Dock = DockStyle.Right, Width = 392, Padding = new Padding(12, 0, 0, 0) };
            var grpQuick = new GroupBox { Text = "Tác vụ nhanh", Dock = DockStyle.Fill, Padding = new Padding(6, 8, 6, 6) };
            var tblQuick = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            tblQuick.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblQuick.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblQuick.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblQuick.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblQuick.Controls.Add(QuickButton("Lập phiếu mượn",   GForward, OpenLoanTab),        0, 0);
            tblQuick.Controls.Add(QuickButton("Nhận trả sách",    GBack,    OpenReturnTab),      1, 0);
            tblQuick.Controls.Add(QuickButton("Sách quá hạn",     GWarn,    OpenOverdueTab),     0, 1);
            tblQuick.Controls.Add(QuickButton("Quản lý tiền phạt",GMoney,   OpenFinesTab),       1, 1);
            grpQuick.Controls.Add(tblQuick);
            quickHost.Controls.Add(grpQuick);

            pnlTop.Controls.Add(grpStats);  // Fill
            pnlTop.Controls.Add(quickHost); // Right

            // Khối đầu: tiêu đề
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = SoftBlue };
            pnlHeader.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlHeader.Width - 1, pnlHeader.Height - 1);
            pnlHeader.Controls.Add(new Label
            {
                Text = "TỔNG QUAN HOẠT ĐỘNG THƯ VIỆN", AutoSize = true, BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(14, 8)
            });
            _lblDate = new Label
            {
                AutoSize = true, BackColor = Color.Transparent, ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F), Location = new Point(16, 36)
            };
            pnlHeader.Controls.Add(_lblDate);

            var btnRefresh = new Button
            {
                Text = "Làm mới dữ liệu", FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                Size = new Size(190, 36),
                Image = GlyphImage(GRefresh, 14, Color.Black),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefresh.Location = new Point(1000, 14);
            btnRefresh.FlatAppearance.BorderColor = BorderGray;
            btnRefresh.Click += (_, _) => LoadData();
            pnlHeader.Controls.Add(btnRefresh);
            pnlHeader.Resize += (_, _) => btnRefresh.Left = pnlHeader.Width - btnRefresh.Width - 12;

            page.Controls.Add(pnlBottom);
            page.Controls.Add(pnlTop);
            page.Controls.Add(pnlHeader);
        }

        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 228, 233),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, EnableHeadersVisualStyles = false,
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
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 243, 246);
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;

            g.Columns.Add("colId",     "Mã phiếu");
            g.Columns.Add("colReader", "Độc giả");
            g.Columns.Add("colBook",   "Tên sách");
            g.Columns.Add("colBorrow", "Ngày mượn");
            g.Columns.Add("colDue",    "Hạn trả");
            g.Columns.Add("colStatus", "Trạng thái");

            g.Columns["colId"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colId"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colId"]!.FillWeight     = 70;
            g.Columns["colReader"]!.FillWeight = 90;
            g.Columns["colBook"]!.FillWeight   = 100;
            g.Columns["colBorrow"]!.FillWeight = 75;
            g.Columns["colDue"]!.FillWeight    = 75;
            g.Columns["colStatus"]!.FillWeight = 85;

            g.CellPainting += DgvTrans_CellPainting;
            return g;
        }

        private void DgvTrans_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 5 || e.Graphics == null) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch
            {
                "Đang mượn"    => Primary,
                "Đã trả"       => Green,
                "Sắp đến hạn"  => Orange,
                "Quá hạn"      => Red,
                _              => TextGray
            };

            var font = new Font("Segoe UI", 8.5F);
            string label = "●  " + text;
            var size = TextRenderer.MeasureText(label, font);
            var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 26) / 2, size.Width + 8, 26);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(rect, 13))
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
        //  CÁC HÀM DỰNG THẺ
        // ============================================================
        private void AddStat(int col, string glyph, string caption, string value, string sub, bool danger)
        {
            var accent = danger ? Red : Primary;
            var card = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.White,
                Margin = new Padding(0, 0, col == 3 ? 0 : 10, 0)
            };
            card.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

            card.Controls.Add(new Label
            {
                Text = glyph, Font = new Font(IconFont, 18F), ForeColor = accent,
                BackColor = danger ? Color.FromArgb(253, 234, 234) : SoftBlue,
                TextAlign = ContentAlignment.MiddleCenter, Size = new Size(50, 50),
                Location = new Point(10, 22)
            });
            card.Controls.Add(new Label { Text = caption, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(60, 60, 60), AutoSize = true, Location = new Point(74, 14) });
            card.Controls.Add(new Label { Text = value,   Font = new Font("Segoe UI", 18F, FontStyle.Bold), AutoSize = true, Location = new Point(70, 30) });
            card.Controls.Add(new Label { Text = sub,     Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoSize = true, Location = new Point(74, 70) });
            _tblStats.Controls.Add(card, col, 0);
        }

        private Button QuickButton(string text, string glyph, Action? onClick = null)
        {
            var b = new Button
            {
                Text = "  " + text, Dock = DockStyle.Fill, Margin = new Padding(4),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(246, 247, 249),
                Font = new Font("Segoe UI", 9F),
                Image = GlyphImage(glyph, 16, Color.Black),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = BorderGray;
            b.Click += (_, _) => (onClick ?? (() => Todo(text)))();
            return b;
        }

        private void AddNotice(string glyph, Color color, string title, string desc, string linkText, Action onLinkClick)
        {
            var row = new Panel
            {
                Height = 66,
                Width = Math.Max(_flowNotice.ClientSize.Width - 8, 200),
                Margin = new Padding(4, 0, 4, 10),
                BackColor = Color.White
            };
            row.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, row.Width - 1, row.Height - 1);
            row.Controls.Add(new Label { Text = glyph, Font = new Font(IconFont, 14F), ForeColor = color, AutoSize = true, Location = new Point(12, 20) });
            row.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), AutoSize = true, Location = new Point(52, 12) });
            row.Controls.Add(new Label { Text = desc,  Font = new Font("Segoe UI", 8F), ForeColor = TextGray, AutoSize = true, Location = new Point(54, 36) });

            var link = new LinkLabel
            {
                Text = linkText, AutoSize = true, Font = new Font("Segoe UI", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            link.Location = new Point(row.Width - 85, 24);
            link.LinkClicked += (_, _) => onLinkClick();
            row.Controls.Add(link);
            _flowNotice.Controls.Add(row);
        }

        private void AddTool(ToolStrip ts, string text, string glyph, Action onClick)
        {
            var b = new ToolStripButton(text)
            {
                Image = GlyphImage(glyph, 20, Primary),
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                TextImageRelation = TextImageRelation.ImageAboveText,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(2, 0, 2, 2)
            };
            b.Click += (_, _) => onClick();
            ts.Items.Add(b);
        }

        // ============================================================
        //  ĐIỀU HƯỚNG TAB CHỨC NĂNG
        // ============================================================
        private void OpenOrSelectTab(ref TabPage? tabField, string title, Func<UserControl> createControl)
        {
            // Đảm bảo đang ở Overview (có _tabMain hợp lệ)
            if (_pnlContent.Controls.Count == 0 || !_pnlContent.Controls.Contains(_tabMain))
                ShowOverviewPage();

            if (tabField != null && _tabMain.TabPages.Contains(tabField))
            {
                _tabMain.SelectedTab = tabField;
                return;
            }

            tabField = new TabPage(title)
            {
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(8)
            };

            var ctrl = createControl();
            ctrl.Dock = DockStyle.Fill;
            tabField.Controls.Add(ctrl);

            _tabMain.TabPages.Add(tabField);
            _tabMain.SelectedTab = tabField;
        }

        // ============================================================
        //  VẼ NÚT ĐÓNG (×) & XỬ LÝ TẮT TAB TRÊN _tabMain
        // ============================================================
        private int _hoveredCloseTabIndex = -1;

        private void ResetAllTabCaches()
        {
            _tabLoan = _tabReport = _tabReturn = _tabBookCatalog =
                _tabBookCopies = _tabCategories = _tabOverdue = _tabFines = null;
            // Nếu bạn đã thêm các biến tab mới (Độc giả, Đặt trước, Thẻ thư viện) thì gán null luôn ở đây:
            // _tabReaders = _tabReservations = _tabReaderCards = null;
        }

        private void ClearTabCacheReference(TabPage tab)
        {
            if (tab == _tabLoan) _tabLoan = null;
            else if (tab == _tabReport) _tabReport = null;
            else if (tab == _tabReturn) _tabReturn = null;
            else if (tab == _tabBookCatalog) _tabBookCatalog = null;
            else if (tab == _tabBookCopies) _tabBookCopies = null;
            else if (tab == _tabCategories) _tabCategories = null;
            else if (tab == _tabOverdue) _tabOverdue = null;
            else if (tab == _tabFines) _tabFines = null;
            // Nếu có thêm tab mới thì bổ sung else if tương tự:
            // else if (tab == _tabReaders) _tabReaders = null;
            // else if (tab == _tabReservations) _tabReservations = null;
            // else if (tab == _tabReaderCards) _tabReaderCards = null;
        }

        private static Rectangle GetCloseButtonRect(Rectangle tabRect)
        {
            int size = 16;
            return new Rectangle(
                tabRect.Right - size - 6,
                tabRect.Top + (tabRect.Height - size) / 2,
                size,
                size);
        }

        private void TabMain_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (_tabMain == null || e.Index < 0 || e.Index >= _tabMain.TabPages.Count) return;

            var tab = _tabMain.TabPages[e.Index];
            var rect = e.Bounds;
            bool isSelected = (_tabMain.SelectedIndex == e.Index);
            bool canClose = e.Index > 0; // Tab 0 ("Tổng quan") không cho phép đóng

            // Nền tab
            Color bgColor = isSelected ? Color.White : Color.FromArgb(232, 236, 242);
            using (var bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, rect);
            }

            // Vạch nhấn màu xanh Primary ở mép trên nếu đang chọn
            if (isSelected)
            {
                using var accentBrush = new SolidBrush(Primary);
                e.Graphics.FillRectangle(accentBrush, rect.X, rect.Y, rect.Width, 3);
            }

            // Chữ tiêu đề tab
            var textRect = canClose
                ? new Rectangle(rect.X + 8, rect.Y + 2, rect.Width - 28, rect.Height - 2)
                : rect;

            using var font = new Font("Segoe UI", 9F, isSelected ? FontStyle.Bold : FontStyle.Regular);
            TextRenderer.DrawText(
                e.Graphics,
                tab.Text,
                font,
                textRect,
                isSelected ? Primary : Color.FromArgb(55, 65, 81),
                (canClose ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter)
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis);

            // Vẽ nút × cho các tab chức năng (index > 0)
            if (canClose)
            {
                var closeRect = GetCloseButtonRect(rect);
                bool isHovered = (_hoveredCloseTabIndex == e.Index);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (isHovered)
                {
                    using var hoverBrush = new SolidBrush(Red);
                    e.Graphics.FillEllipse(hoverBrush, closeRect);
                }

                using var xPen = new Pen(isHovered ? Color.White : TextGray, 1.6f);
                int pad = 4;
                e.Graphics.DrawLine(xPen, closeRect.Left + pad, closeRect.Top + pad, closeRect.Right - pad, closeRect.Bottom - pad);
                e.Graphics.DrawLine(xPen, closeRect.Right - pad, closeRect.Top + pad, closeRect.Left + pad, closeRect.Bottom - pad);
            }
        }

        private void TabMain_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_tabMain == null) return;

            int newHover = -1;
            for (int i = 1; i < _tabMain.TabPages.Count; i++) // Bỏ qua tab 0 ("Tổng quan")
            {
                var closeRect = GetCloseButtonRect(_tabMain.GetTabRect(i));
                if (closeRect.Contains(e.Location))
                {
                    newHover = i;
                    break;
                }
            }

            if (newHover != _hoveredCloseTabIndex)
            {
                _hoveredCloseTabIndex = newHover;
                _tabMain.Invalidate();
            }
        }

        private void TabMain_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_tabMain == null) return;

            for (int i = 1; i < _tabMain.TabPages.Count; i++) // Tab 0 không đóng
            {
                var tabRect = _tabMain.GetTabRect(i);
                if (!tabRect.Contains(e.Location)) continue;

                // 1. Click chuột trái vào đúng nút ×
                if (e.Button == MouseButtons.Left && GetCloseButtonRect(tabRect).Contains(e.Location))
                {
                    CloseTabAt(i);
                    return;
                }

                // 2. Click chuột giữa (Middle click) vào bất kỳ đâu trên tab
                if (e.Button == MouseButtons.Middle)
                {
                    CloseTabAt(i);
                    return;
                }

                // 3. Click chuột phải mở menu đóng nhanh
                if (e.Button == MouseButtons.Right)
                {
                    int targetIndex = i;
                    var ctxMenu = new ContextMenuStrip();
                    ctxMenu.Items.Add("Đóng tab này", null, (_, _) => CloseTabAt(targetIndex));
                    if (_tabMain.TabPages.Count > 2)
                        ctxMenu.Items.Add("Đóng các tab khác", null, (_, _) => CloseOtherTabs(targetIndex));
                    ctxMenu.Items.Add("Đóng tất cả tab chức năng", null, (_, _) => CloseAllClosableTabs());
                    ctxMenu.Show(_tabMain, e.Location);
                    return;
                }
            }
        }

        private void CloseTabAt(int index)
        {
            if (_tabMain == null || index <= 0 || index >= _tabMain.TabPages.Count) return;

            var tabToClose = _tabMain.TabPages[index];
            int nextSelectedIndex = (_tabMain.SelectedIndex == index)
                ? Math.Max(0, index - 1)
                : _tabMain.SelectedIndex;

            ClearTabCacheReference(tabToClose);
            _tabMain.TabPages.RemoveAt(index);
            tabToClose.Dispose();

            _hoveredCloseTabIndex = -1;
            if (nextSelectedIndex < _tabMain.TabPages.Count)
                _tabMain.SelectedIndex = nextSelectedIndex;
        }

    private void CloseOtherTabs(int keepIndex)
    {
        if (_tabMain == null) return;
        for (int i = _tabMain.TabPages.Count - 1; i >= 1; i--)
        {
            if (i != keepIndex) CloseTabAt(i);
        }
    }

    private void CloseAllClosableTabs()
    {
        if (_tabMain == null) return;
        for (int i = _tabMain.TabPages.Count - 1; i >= 1; i--)
        {
            CloseTabAt(i);
        }
    }

        private void OpenReadersTab() =>
            OpenOrSelectTab(ref _tabReaders, "Quản lý độc giả", () =>
            {
                var ctrl = AppServices.Provider.GetRequiredService<ReaderManagementControl>();
                ctrl.ReaderDataChanged += LoadData;
                return ctrl;
            });

        private void OpenLoanTab() =>
            OpenOrSelectTab(ref _tabLoan, "Quản lý mượn sách", () =>
            {
                var ctrl = new LoanManagement.LoanManagementControl();
                ctrl.LoanDataChanged += LoadData;
                return ctrl;
            });

        private void OpenReportTab() =>
            OpenOrSelectTab(ref _tabReport, "Báo cáo & Thống kê", () => AppServices.Provider.GetRequiredService<StatisticalReportControl>());

        private void OpenBookCatalogTab() =>
            OpenOrSelectTab(ref _tabBookCatalog, "Quản lý đầu sách", () =>
            {
                var control = new BookCatalogControl();
                control.CatalogDataChanged += LoadData; 
                control.ViewCopiesRequested += book =>
                {
                    OpenBookCopiesTab();
                    var copiesControl = _tabBookCopies?.Controls
                        .OfType<BookCopyControl>()
                        .FirstOrDefault();
                    copiesControl?.FilterByBook(book.Id, book.Title);
                };
                return control;
            });

        private void OpenCategoryTab() =>
            OpenOrSelectTab(ref _tabCategories, "Quản lý thể loại", () => new CategoryManagementControl());

        private void OpenReaderRegistration()
        {
            using var form = new LibraryManagement.Forms.RegisterForm.RegisterForm(librarianMode: true);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void OpenReturnTab() =>
            OpenOrSelectTab(ref _tabReturn, "Nhận trả sách", () => AppServices.Provider.GetRequiredService<BookFormReturn.BookFormReturn>());

        private void OpenBookCopiesTab() =>
            OpenOrSelectTab(ref _tabBookCopies, "Quản lý bản sách", () => new BookCopyControl());

        private void OpenReservationsTab() =>
            OpenOrSelectTab(ref _tabReservations, "Đặt trước sách", () =>
            {
                var ctrl = new ReservationManagementControl();
                ctrl.ReservationDataChanged += LoadData;
                return ctrl;
            });

        private void OpenOverdueTab() =>
            OpenOrSelectTab(ref _tabOverdue, "Sách quá hạn", () => AppServices.Provider.GetRequiredService<OverdueBooks.OverdueBooks>());

        private void OpenReaderCardsTab() =>
            OpenOrSelectTab(ref _tabReaderCards, "Thẻ thư viện", () =>
            {
                var ctrl = new LibraryCardManagementControl();
                ctrl.CardDataChanged += LoadData;
                return ctrl;
            });

        private void OpenFinesTab() =>
            OpenOrSelectTab(ref _tabFines, "Quản lý tiền phạt", () => new FineManagement.FineManagement());

        // ============================================================
        //  TIỆN ÍCH
        // ============================================================
        private static Bitmap GlyphImage(string glyph, int size, Color color)
        {
            var bmp = new Bitmap(size + 6, size + 6);
            using var g = Graphics.FromImage(bmp);
            using var f = new Font(IconFont, size, GraphicsUnit.Pixel);
            TextRenderer.DrawText(g, glyph, f, new Rectangle(0, -1, bmp.Width, bmp.Height), color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            return bmp;
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

        private void Todo(string feature) =>
            MessageBox.Show($"Chức năng \"{feature}\" đang chờ hoàn thiện giao diện.", "LIBRA",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnLogout_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            IsLogout = true;
            Close();
        }
    }
}