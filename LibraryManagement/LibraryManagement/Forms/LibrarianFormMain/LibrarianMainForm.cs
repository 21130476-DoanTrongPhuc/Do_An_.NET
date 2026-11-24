using System.Drawing.Drawing2D;
using LibraryManagement.Forms.FineManagement;
using LibraryManagement.Controls;
using LibraryManagement.Data;
using LibraryManagement.Helpers;
using LibraryManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LibraryManagement.Data;
using LibraryManagement.Helpers;
using Microsoft.EntityFrameworkCore;

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
        private const string GHome    = "\uE80F";
        private const string GBook    = "\uE82D";
        private const string GPeople  = "\uE716";
        private const string GForward = "\uE72A";
        private const string GBack    = "\uE72B";
        private const string GRefresh = "\uE72C";
        private const string GHelp    = "\uE897";
        private const string GWarn    = "\uE7BA";
        private const string GCalendar= "\uE787";
        private const string GReport  = "\uE9F9";
        private const string GMoney   = "\uE8C7";
        private const string GReturn  = "\uE7A7";
        private const string GHome = "\uE80F", GBook = "\uE82D", GPeople = "\uE716",
            GForward = "\uE72A", GBack = "\uE72B", GRefresh = "\uE72C", GHelp = "\uE897",
            GWarn = "\uE7BA", GCalendar = "\uE787", GReport = "\uE9F9", GMoney = "\uE8C7";

        public bool IsLogout { get; private set; }

        // Control cần truy cập sau
        private TabControl _tabMain = null!;
        private TabPage? _tabLoan;
        // ====== Vùng nội dung động ======
        private Panel _pnlContent = null!;
        private TabControl? _tabMain;
        private TabPage? _tabOverview;
        private TabPage? _tabReport;
        private TabPage? _tabReturn;
        private TabPage? _tabBookCopies;
        private TabPage? _tabOverdue;
        private TabPage? _tabFines;

        private Control? _currentPage;

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
                LoadData();
            };
        }

        // ============================================================
        //  ĐIỀU HƯỚNG
        // ============================================================
        private void ShowOverviewPage()
        {
            _pnlContent.SuspendLayout();
            _pnlContent.Controls.Clear();

            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Padding = new Point(14, 4)
            };
            _tabOverview = new TabPage("Tổng quan")
            {
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(8)
            };
            _tabMain.TabPages.Add(_tabOverview);
            BuildOverview(_tabOverview);

            _pnlContent.Controls.Add(_tabMain);
            _pnlContent.ResumeLayout(true);

            _currentPage = _tabMain;
            _tabReport = null;
        }

        private void ShowUserControl(UserControl uc)
        {
            _pnlContent.SuspendLayout();
            _pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            _pnlContent.Controls.Add(uc);
            _pnlContent.ResumeLayout(true);
            _currentPage = uc;
        }

        // --------- Trang Nhận trả sách ---------
        private void ShowBookReturn()
        {
            var uc = AppServices.Provider.GetRequiredService<BookFormReturn.BookFormReturn>();
            uc.CloseRequested += (_, _) => ShowOverviewPage();
            ShowUserControl(uc);
        }

        // --------- Trang Sách quá hạn ---------
        private void ShowOverdueBooks()
        {
            var uc = AppServices.Provider.GetRequiredService<OverdueBooks.OverdueBooks>();
            uc.CloseRequested += (_, _) => ShowOverviewPage();

            // Điều hướng sang trang Nhận trả sách (có thể prefill mã phiếu sau)
            uc.ReturnBookRequested += (_, _) =>
            {
                ShowBookReturn();
            };

            ShowUserControl(uc);
        }

        // --------- Trang Quản lý phiếu phạt ---------
        private void ShowFineManagement()
        {
            var uc = new FineManagement.FineManagement();
            ShowUserControl(uc);
        }

        // --------- Trang Báo cáo & Thống kê ---------
        private void OpenReportTab()
        {
            // Nếu đang ở Overview, chỉ cần mở tab
            if (_tabMain != null && _pnlContent.Controls.Contains(_tabMain))
            {
                if (_tabReport != null && _tabMain.TabPages.Contains(_tabReport))
                {
                    _tabMain.SelectedTab = _tabReport;
                    return;
                }
                _tabReport = new TabPage("Báo cáo & Thống kê")
                {
                    BackColor = Color.FromArgb(240, 242, 245),
                    Padding = new Padding(8)
                };
                _tabReport.Controls.Add(new Controls.StatisticalReportControl { Dock = DockStyle.Fill });
                _tabMain.TabPages.Add(_tabReport);
                _tabMain.SelectedTab = _tabReport;
                return;
            }

            // Nếu đang ở UserControl khác → quay về Overview rồi mở tab
            ShowOverviewPage();
            OpenReportTab();
        }

        // ============================================================
        //  DỮ LIỆU
        //  DỮ LIỆU - KẾT NỐI TRỰC TIẾP VỚI DATABASE (LibraryDbContext)
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

                // 1. Lấy thông tin thủ thư / admin đang đăng nhập (chỉ Select các cột cần dùng)
                var currentUser = context.Users
                    .Where(u => u.Id == Session.CurrentLibrarianId)
                    .Select(u => new { u.FullName, u.UserType, u.UserCode })
                    .FirstOrDefault();

                _lblUserName.Text = currentUser?.FullName ?? "Quản trị viên";
                if (_lblStatusRole != null)
                {
                    _lblStatusRole.Text = currentUser != null
                        ? $"{(currentUser.UserType == "Admin" ? "Quản trị viên" : "Thủ thư")}: {currentUser.UserCode}"
                        : "Thủ thư";
                }

            _tblStats.Controls.Clear();
            AddStat(0, GBook, "Tổng đầu sách", "2.486", "+32 trong tháng", false);
            AddStat(1, GPeople, "Độc giả hoạt động", "1.248", "86 độc giả mới", false);
            AddStat(2, GForward, "Sách đang mượn", "386", "62 lượt hôm nay", false);
            AddStat(3, GWarn, "Phiếu quá hạn", "24", "Cần xử lý", true);
                // 2. Truy vấn thống kê tổng quan từ Database
                int totalBooks = context.Books.Count();
                int totalCopies = context.BookCopies.Count();

                int activeReaders = context.Users.Count(u => u.UserType == "Reader" && u.Status == "Active");
                int newReadersThisMonth = context.Users
                    .Where(u => u.UserType == "Reader")
                    .Select(u => u.CreatedAt)
                    .AsEnumerable()
                    .Count(createdStr => DateTime.TryParse(createdStr, out var created) && created >= monthStart);

                var unreturnedDetails = context.LoanDetails
                    .Where(ld => ld.ReturnedAt == null)
                    .Select(ld => new
                    {
                        ld.DueAt,
                        BorrowedAt = ld.Loan.BorrowedAt
                    })
                    .ToList();

                int borrowingCount = unreturnedDetails.Count;
                int borrowedTodayCount = unreturnedDetails.Count(ld => ld.BorrowedAt.Date == today);
                int overdueCount = unreturnedDetails.Count(ld => ld.DueAt.Date < today);

                _tblStats.Controls.Clear();
                AddStat(0, GBook, "Tổng đầu sách", totalBooks.ToString("N0"), $"{totalCopies:N0} bản sách vật lý", false);
                AddStat(1, GPeople, "Độc giả hoạt động", activeReaders.ToString("N0"), $"+{newReadersThisMonth} độc giả mới tháng này", false);
                AddStat(2, GForward, "Sách đang mượn", borrowingCount.ToString("N0"), $"{borrowedTodayCount} lượt mượn hôm nay", false);
                AddStat(3, GWarn, "Phiếu quá hạn", overdueCount.ToString("N0"), overdueCount > 0 ? "Cần xử lý ngay" : "Không có quá hạn", overdueCount > 0);

            _dgvTrans.Rows.Clear();
            _dgvTrans.Rows.Add("PM-240315", "Nguyễn Minh Anh", "Nhà giả kim", "15/03/2024", "29/03/2024", "Đang mượn");
            _dgvTrans.Rows.Add("PM-240314", "Trần Hoàng Nam", "Đắc nhân tâm", "14/03/2024", "28/03/2024", "Sắp đến hạn");
            _dgvTrans.Rows.Add("PM-240311", "Lê Thu Hà", "Lược sử thời gian", "11/03/2024", "25/03/2024", "Quá hạn");
            _dgvTrans.Rows.Add("PM-240309", "Đỗ Hải Yến", "Mắt biếc", "09/03/2024", "23/03/2024", "Quá hạn");
            _dgvTrans.ClearSelection();
                // 3. Giao dịch mượn/trả gần đây (Dùng Select để tránh lỗi NULL ở các cột Note / Condition)
                var recentDetails = context.LoanDetails
                    .OrderByDescending(ld => ld.Loan.BorrowedAt)
                    .ThenByDescending(ld => ld.Id)
                    .Take(15)
                    .Select(ld => new
                    {
                        ld.Loan.LoanCode,
                        ReaderName = ld.Loan.Reader.FullName,
                        BookTitle = ld.BookCopy.Book.Title,
                        ld.Loan.BorrowedAt,
                        ld.DueAt,
                        ld.ReturnedAt
                    })
                    .ToList();

                _dgvTrans.Rows.Clear();
                foreach (var item in recentDetails)
                {
                    string statusText;
                    if (item.ReturnedAt != null)
                    {
                        statusText = "Đã trả";
                    }
                    else if (item.DueAt.Date < today)
                    {
                        statusText = "Quá hạn";
                    }
                    else if ((item.DueAt.Date - today).Days <= 3)
                    {
                        statusText = "Sắp đến hạn";
                    }
                    else
                    {
                        statusText = "Đang mượn";
                    }

                    _dgvTrans.Rows.Add(
                        item.LoanCode,
                        item.ReaderName,
                        item.BookTitle,
                        item.BorrowedAt.ToString("dd/MM/yyyy"),
                        item.DueAt.ToString("dd/MM/yyyy"),
                        statusText
                    );
                }
                _dgvTrans.ClearSelection();

            _flowNotice.Controls.Clear();
            AddNotice(GWarn, Red, "24 phiếu đã quá hạn", "Cần liên hệ độc giả và xử lý", "Kiểm tra");
            AddNotice(GPeople, Primary, "38 thẻ sắp hết hạn", "Trong vòng 30 ngày tới", "Xem");
            AddNotice(GBook, Orange, "12 đầu sách sắp hết", "Số lượng khả dụng dưới 2 cuốn", "Xem");
                // 4. Thông báo và nhắc việc động từ Database
                _flowNotice.Controls.Clear();

                if (overdueCount > 0)
                {
                    AddNotice(GWarn, Red,
                        $"{overdueCount} cuốn sách đã quá hạn trả",
                        "Cần liên hệ độc giả và xử lý thu hồi sách",
                        "Kiểm tra",
                        OpenOverdueTab);
                }

                int expiringCardsCount = context.ReaderCards
                    .Count(rc => rc.Status == "Active"
                                 && rc.ExpiresAt >= today
                                 && rc.ExpiresAt <= today.AddDays(30));
                if (expiringCardsCount > 0)
                {
                    AddNotice(GPeople, Primary,
                        $"{expiringCardsCount} thẻ độc giả sắp hết hạn",
                        "Sẽ hết hạn trong vòng 30 ngày tới",
                        "Xem báo cáo",
                        OpenReportTab);
                }

                int lowStockBooksCount = context.Books
                    .Count(b => b.Copies.Count(c => c.CirculationStatus == "Available") < 2);
                if (lowStockBooksCount > 0)
                {
                    AddNotice(GBook, Orange,
                        $"{lowStockBooksCount} đầu sách sắp hết bản sẵn sàng",
                        "Số lượng bản có thể mượn dưới 2 cuốn",
                        "Xem bản sách",
                        OpenBookCopiesTab);
                }

                int unpaidFinesCount = context.Fines.Count();
                if (unpaidFinesCount > 0)
                {
                    AddNotice(GMoney, Orange,
                        $"{unpaidFinesCount} khoản phạt chưa thanh toán",
                        "Độc giả còn nợ phí phạt quá hạn / hư hỏng",
                        "Thu phạt",
                        OpenFinesTab);
                }

                if (_flowNotice.Controls.Count == 0)
                {
                    AddNotice(GBook, Green,
                        "Hệ thống hoạt động ổn định",
                        "Hiện không có phiếu quá hạn hay cảnh báo tồn đọng",
                        "Làm mới",
                        LoadData);
                }
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

            // ---- Vùng nội dung động (Fill - add đầu tiên) ----
            _pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 242, 245) };
            Controls.Add(_pnlContent);

            Controls.Add(BuildStatusBar());   // Bottom
            Controls.Add(BuildToolbar());     // Top
            Controls.Add(BuildMenu());        // Top (trên cùng)

            ResumeLayout(true);
        }

        private MenuStrip BuildMenu()
        {
            var menu = new MenuStrip { BackColor = Color.White, Dock = DockStyle.Top };

            var mnuSystem = new ToolStripMenuItem("Hệ thống");
            mnuSystem.DropDownItems.Add("Trang chủ tổng quan", null, (_, _) => _tabMain.SelectedIndex = 0);
            mnuSystem.DropDownItems.Add("Làm mới dữ liệu", null, (_, _) => LoadData());
            mnuSystem.DropDownItems.Add(new ToolStripSeparator());
            mnuSystem.DropDownItems.Add("Đăng xuất", null, btnLogout_Click);

            var mnuCatalog = new ToolStripMenuItem("Danh mục");
            mnuCatalog.DropDownItems.Add("Quản lý bản sách", null, (_, _) => OpenBookCopiesTab());
            mnuCatalog.DropDownItems.Add("Thêm đầu sách", null, (_, _) => Todo("Thêm đầu sách"));
            mnuCatalog.DropDownItems.Add("Thêm độc giả", null, (_, _) => Todo("Thêm độc giả"));

            var mnuCirculation = new ToolStripMenuItem("Lưu thông");
            mnuCirculation.DropDownItems.Add("Quản lý mượn sách", null, (_, _) => OpenLoanTab());
            mnuCirculation.DropDownItems.Add("Nhận trả sách", null, (_, _) => OpenReturnTab());
            mnuCirculation.DropDownItems.Add("Sách quá hạn", null, (_, _) => OpenOverdueTab());
            mnuCirculation.DropDownItems.Add("Quản lý tiền phạt", null, (_, _) => OpenFinesTab());

            var mnuReport = new ToolStripMenuItem("Báo cáo");
            mnuReport.DropDownItems.Add("Báo cáo & Thống kê", null, (_, _) => OpenReportTab());

            var mnuHelp = new ToolStripMenuItem("Trợ giúp");
            mnuHelp.DropDownItems.Add("Hướng dẫn sử dụng", null, (_, _) =>
                MessageBox.Show("Hệ thống Quản lý Thư viện LIBRA - Phân hệ Thủ thư.", "Trợ giúp",
                    MessageBoxButtons.OK, MessageBoxIcon.Information));

            menu.Items.AddRange([mnuSystem, mnuCatalog, mnuCirculation, mnuReport, mnuHelp]);

            // Hệ thống
            var mnuSystem = new ToolStripMenuItem("Hệ thống");
            mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Đăng xuất", null,
                (_, _) => btnLogout_Click(this, EventArgs.Empty)));
            mnuSystem.DropDownItems.Add(new ToolStripSeparator());
            mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Thoát", null,
                (_, _) => Close()));
            menu.Items.Add(mnuSystem);

            // Danh mục
            var mnuCatalog = new ToolStripMenuItem("Danh mục");
            mnuCatalog.DropDownItems.Add(new ToolStripMenuItem("Sách", null,
                (_, _) => Todo("Danh mục sách")));
            mnuCatalog.DropDownItems.Add(new ToolStripMenuItem("Độc giả", null,
                (_, _) => Todo("Danh mục độc giả")));
            menu.Items.Add(mnuCatalog);

            // Lưu thông
            var mnuCirculation = new ToolStripMenuItem("Lưu thông");
            mnuCirculation.DropDownItems.Add(new ToolStripMenuItem("Lập phiếu mượn", null,
                (_, _) => Todo("Lập phiếu mượn")));
            mnuCirculation.DropDownItems.Add(new ToolStripMenuItem("Nhận trả sách", null,
                (_, _) => ShowBookReturn()));
            mnuCirculation.DropDownItems.Add(new ToolStripSeparator());
            mnuCirculation.DropDownItems.Add(new ToolStripMenuItem("Sách quá hạn", null,
                (_, _) => ShowOverdueBooks()));
            mnuCirculation.DropDownItems.Add(new ToolStripMenuItem("Quản lý phiếu phạt", null,
                (_, _) => ShowFineManagement()));
            menu.Items.Add(mnuCirculation);

            // Báo cáo
            var mnuReport = new ToolStripMenuItem("Báo cáo");
            mnuReport.DropDownItems.Add(new ToolStripMenuItem("Thống kê tổng hợp", null,
                (_, _) => OpenReportTab()));
            mnuReport.DropDownItems.Add(new ToolStripMenuItem("Sách quá hạn", null,
                (_, _) => ShowOverdueBooks()));
            menu.Items.Add(mnuReport);

            // Trợ giúp
            menu.Items.Add(new ToolStripMenuItem("Trợ giúp", null,
                (_, _) => Todo("Trợ giúp")));

            MainMenuStrip = menu;
            return menu;
        }

        private Panel BuildToolbar()
        {
            var host = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.FromArgb(246, 247, 249) };
            host.Paint += (_, e) => e.Graphics.DrawLine(new Pen(BorderGray), 0, host.Height - 1, host.Width, host.Height - 1);

            var ts = new ToolStrip
            {
                Dock = DockStyle.Fill, GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.FromArgb(246, 247, 249),
                ImageScalingSize = new Size(24, 24), Padding = new Padding(6, 4, 0, 0)
            };
            AddTool(ts, "Trang chủ", GHome, ShowOverviewPage);
            AddTool(ts, "Trang chủ", GHome, () => { _tabMain.SelectedIndex = 0; LoadData(); });
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Thêm đầu sách", GBook, () => Todo("Thêm đầu sách"));
            AddTool(ts, "Bản sách", GBook, OpenBookCopiesTab);
            AddTool(ts, "Thêm độc giả", GPeople, () => Todo("Thêm độc giả"));
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Lập phiếu mượn", GForward, OpenLoanTab);
            AddTool(ts, "Nhận trả sách", GBack, OpenReturnTab);
            AddTool(ts, "Quá hạn", GWarn, OpenOverdueTab);
            AddTool(ts, "Tiền phạt", GMoney, OpenFinesTab);
            AddTool(ts, "Lập phiếu mượn", GForward, () => Todo("Lập phiếu mượn"));
            AddTool(ts, "Nhận trả sách", GBack, ShowBookReturn);
            AddTool(ts, "Sách quá hạn", GWarn, ShowOverdueBooks);
            AddTool(ts, "Phiếu phạt", GMoney, ShowFineManagement);
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Báo cáo", GReport, OpenReportTab);
            AddTool(ts, "Làm mới", GRefresh, LoadData);

            // Khu người dùng bên phải
            var user = new Panel { Dock = DockStyle.Right, Width = 340 };
            user.Controls.Add(new Label
            {
                Text = "Người dùng:", AutoSize = true, ForeColor = TextGray,
                Font = new Font("Segoe UI", 9F), Location = new Point(8, 22)
            });
            _lblUserName = new Label
            {
                AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(84, 22)
            };
            user.Controls.Add(_lblUserName);
            var btnLogout = new Button
            {
                Text = "Đăng xuất", FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                Size = new Size(92, 32),
                Location = new Point(user.Width - 104, 14),
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
            var st = new StatusStrip { Dock = DockStyle.Bottom, SizingGrip = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Segoe UI", 8F) };
            _lblStatusRole = new ToolStripStatusLabel("Thủ thư");
            st.Items.AddRange([
            var st = new StatusStrip
            {
                Dock = DockStyle.Bottom, SizingGrip = true,
                BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Segoe UI", 8F)
            };
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
            // Khối dưới: giao dịch + thông báo (Fill - add đầu)
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
            tblQuick.Controls.Add(QuickButton("Nhận trả sách", GBack, OpenReturnTab), 0, 0);
            tblQuick.Controls.Add(QuickButton("Sách quá hạn", GWarn, OpenOverdueTab), 1, 0);
            tblQuick.Controls.Add(QuickButton("Quản lý bản sách", GBook, OpenBookCopiesTab), 0, 1);
            tblQuick.Controls.Add(QuickButton("Quản lý tiền phạt", GMoney, OpenFinesTab), 1, 1);
            tblQuick.Controls.Add(QuickButton("Lập phiếu mượn", GForward, () => Todo("Lập phiếu mượn")), 0, 0);
            tblQuick.Controls.Add(QuickButton("Nhận trả sách", GBack, ShowBookReturn), 1, 0);
            tblQuick.Controls.Add(QuickButton("Sách quá hạn", GWarn, ShowOverdueBooks), 0, 1);
            tblQuick.Controls.Add(QuickButton("Phiếu phạt", GMoney, ShowFineManagement), 1, 1);
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
                Text = "  Làm mới dữ liệu", FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                Size = new Size(150, 34),
                Image = GlyphImage(GRefresh, 14, Color.Black),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
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

            g.Columns.Add("colId", "Mã phiếu");
            g.Columns.Add("colReader", "Độc giả");
            g.Columns.Add("colBook", "Tên sách");
            g.Columns.Add("colBorrow", "Ngày mượn");
            g.Columns.Add("colDue", "Hạn trả");
            g.Columns.Add("colStatus", "Trạng thái");
            g.Columns["colId"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colId"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colId"]!.FillWeight = 70;
            g.Columns["colReader"]!.FillWeight = 90;
            g.Columns["colBook"]!.FillWeight = 100;
            g.Columns["colBorrow"]!.FillWeight = 75;
            g.Columns["colDue"]!.FillWeight = 75;
            g.Columns["colStatus"]!.FillWeight = 85;

            g.CellPainting += DgvTrans_CellPainting;
            return g;
        }

        // Vẽ nhãn trạng thái dạng "pill"
        private void DgvTrans_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 5 || e.Graphics == null) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch
            {
                "Đang mượn" => Primary,
                "Đã trả" => Green,
                "Sắp đến hạn" => Orange,
                "Quá hạn" => Red,
                _ => TextGray
            };

            var font = new Font("Segoe UI", 8.5F);
            string label = "●  " + text;
            var size = TextRenderer.MeasureText(label, font);
            var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 26) / 2, size.Width + 8, 26);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(rect, 13))
            using (var fill = new SolidBrush(Color.FromArgb(22, c)))
            using (var pen = new Pen(Color.FromArgb(150, c)))
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
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 18F, FontStyle.Bold), AutoSize = true, Location = new Point(70, 30) });
            card.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoSize = true, Location = new Point(74, 70) });
            _tblStats.Controls.Add(card, col, 0);
        }

        private Button QuickButton(string text, string glyph, Action onClick)
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
            b.Click += (_, _) => onClick();
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
            row.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 8F), ForeColor = TextGray, AutoSize = true, Location = new Point(54, 36) });

            var link = new LinkLabel { Text = linkText, AutoSize = true, Font = new Font("Segoe UI", 8.5F), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            link.Location = new Point(row.Width - 85, 24);
            link.LinkClicked += (_, _) => onLinkClick();
            var link = new LinkLabel
            {
                Text = linkText, AutoSize = true, Font = new Font("Segoe UI", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            link.Location = new Point(row.Width - 70, 24);
            link.LinkClicked += (_, _) =>
            {
                // Điều hướng thông minh theo nội dung thông báo
                if (title.Contains("quá hạn")) ShowOverdueBooks();
                else Todo(title);
            };
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
        //  ĐIỀU HƯỚNG CÁC TAB CHỨC NĂNG CỦA THỦ THƯ
        // ============================================================
        private void OpenOrSelectTab(ref TabPage? tabField, string title, Func<UserControl> createControl)
        {
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

        private void OpenLoanTab() =>
            OpenOrSelectTab(ref _tabLoan, "Quản lý mượn sách", () =>
            {
                var ctrl = new LoanManagement.LoanManagementControl();
                ctrl.LoanDataChanged += LoadData;
                return ctrl;
            });

        private void OpenReportTab() =>
            OpenOrSelectTab(ref _tabReport, "Báo cáo & Thống kê", () => new Controls.StatisticalReportControl());

        private void OpenReturnTab() =>
            OpenOrSelectTab(ref _tabReturn, "Nhận trả sách", () => new BookFormReturn.BookFormReturn());

        private void OpenBookCopiesTab() =>
            OpenOrSelectTab(ref _tabBookCopies, "Quản lý bản sách", () => new BookCopyControl());

        private void OpenOverdueTab() =>
            OpenOrSelectTab(ref _tabOverdue, "Sách quá hạn", () => new OverdueBooks.OverdueBooks());

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
            TextRenderer.DrawText(g, glyph, f, new Rectangle(0, 0, bmp.Width, bmp.Height), color,
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
            MessageBox.Show($"Chức năng \"{feature}\" đang chờ hoàn thiện giao diện.", "LIBRA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MessageBox.Show($"Chức năng \"{feature}\" chưa được nối.", "LIBRA",
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