using System.Drawing.Drawing2D;
using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Forms.Books.ReaderPortal;
using LibraryManagement.Helpers;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Forms.HomePage
{
    public partial class HomePage : Form
    {
        // ====== Màu sắc theo thiết kế Figma ======
        private static readonly Color Primary = Color.FromArgb(19, 98, 160);
        private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);
        private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
        private static readonly Color Orange = Color.FromArgb(230, 145, 20);
        private static readonly Color Green = Color.FromArgb(34, 160, 90);
        private static readonly Color Red = Color.FromArgb(200, 50, 50);
        private const string IconFont = "Segoe MDL2 Assets";
        public bool IsLogout { get; private set; }

        // Glyph (Segoe MDL2 Assets)
        private const string GHome = "\uE80F",
            GSearch = "\uE721",
            GBook = "\uE82D",
            GHeart = "\uEB51",
            GHeartFill = "\uEB52",
            GClock = "\uE121",
            GUser = "\uE77B",
            GBell = "\uEA8F",
            GCheck = "\uE73E",
            GInfo = "\uE946",
            GHistory = "\uE81C";

        // Vùng chứa các trang con (Trang 2 -> Trang 7)
        private Panel _pnlSubView = null!;
        private BookSearchControl _searchControl = null!;
        private BookDetailControl _detailControl = null!;
        private CurrentLoansControl _loansControl = null!;
        private BorrowHistoryControl _historyControl = null!;
        private FavoriteBooksControl _favoriteControl = null!;
        private ReaderProfileControl _profileControl = null!;

        private ToolStripButton _btnNavHome = null!;
        private ToolStripButton _btnNavSearch = null!;
        private ToolStripButton _btnNavLoans = null!;
        private ToolStripButton _btnNavFavorite = null!;
        private ToolStripButton _btnNavHistory = null!;
        private ToolStripButton _btnNavProfile = null!;

        private Label? _lblFavStatValue;
        private string _currentView = "Home";
        private string _previousView = "Home";

        public HomePage()
        {
            InitializeComponent();
            InitReaderPortalViews();
            BuildMenu();
            BuildToolbar();
            StyleStatic();
            BuildStatusBar();
            LoadData();

            flowBorrow.Resize += (_, _) => LayoutCards(flowBorrow, 2, flowBorrow.Height - 8);
            flowSuggest.Resize += (_, _) => LayoutCards(flowSuggest, 5, 82);
            flowNotice.Resize += (_, _) =>
            {
                foreach (Control c in flowNotice.Controls) c.Width = flowNotice.ClientSize.Width - 8;
            };
        }

        // ============================================================
        //  KHỞI TẠO & ĐIỀU HƯỚNG 6 TRANG CON (TRANG 2 -> TRANG 7)
        // ============================================================
        private void InitReaderPortalViews()
        {
            _pnlSubView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Visible = false
            };

            _searchControl = new BookSearchControl();
            _searchControl.OnOpenBookDetail += OpenBookDetail;
            _searchControl.OnDataChanged += RefreshHomeBadges;

            _detailControl = new BookDetailControl();
            _detailControl.OnBackRequested += () => SwitchView(_previousView);
            _detailControl.OnDataChanged += RefreshHomeBadges;

            _loansControl = new CurrentLoansControl();
            _loansControl.OnOpenBookDetail += OpenBookDetail;
            _loansControl.OnDataChanged += RefreshHomeBadges;

            _historyControl = new BorrowHistoryControl();
            _historyControl.OnOpenBookDetail += OpenBookDetail;

            _favoriteControl = new FavoriteBooksControl();
            _favoriteControl.OnOpenBookDetail += OpenBookDetail;
            _favoriteControl.OnDataChanged += RefreshHomeBadges;
            _favoriteControl.OnNavigateSearchRequested += () => SwitchView("Search");

            _profileControl = new ReaderProfileControl();
            _profileControl.OnProfileUpdated += LoadData;

            Controls.Add(_pnlSubView);
            Controls.SetChildIndex(_pnlSubView, 0);
        }

        private void OpenBookDetail(int bookId)
        {
            if (_currentView != "Detail")
            {
                _previousView = _currentView;
            }

            _currentView = "Detail";
            lblBreadcrumb.Text = "Cổng độc giả  ›  Tra cứu sách  ›  Chi tiết đầu sách";
            UpdateToolbarActiveState("Search");

            pnlContent.Visible = false;
            _pnlSubView.Visible = true;
            _pnlSubView.Controls.Clear();
            _pnlSubView.Controls.Add(_detailControl);
            _detailControl.LoadBook(bookId);
        }

        private void SwitchView(string viewName, string searchKeyword = "")
        {
            _currentView = viewName;
            UpdateToolbarActiveState(viewName);

            if (viewName == "Home")
            {
                lblBreadcrumb.Text = "Cổng độc giả  ›  Trang chủ";
                _pnlSubView.Visible = false;
                pnlContent.Visible = true;
                LoadData();
                return;
            }

            pnlContent.Visible = false;
            _pnlSubView.Visible = true;
            _pnlSubView.Controls.Clear();

            switch (viewName)
            {
                case "Search":
                    lblBreadcrumb.Text = "Cổng độc giả  ›  Tra cứu sách";
                    _pnlSubView.Controls.Add(_searchControl);
                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                        _searchControl.ApplyKeyword(searchKeyword);
                    else
                        _searchControl.LoadBooks();
                    break;

                case "Loans":
                    lblBreadcrumb.Text = "Cổng độc giả  ›  Sách đang mượn & Đặt trước";
                    _pnlSubView.Controls.Add(_loansControl);
                    _loansControl.ReloadData();
                    break;

                case "Favorite":
                    lblBreadcrumb.Text = "Cổng độc giả  ›  Sách yêu thích";
                    _pnlSubView.Controls.Add(_favoriteControl);
                    _favoriteControl.ReloadData();
                    break;

                case "History":
                    lblBreadcrumb.Text = "Cổng độc giả  ›  Lịch sử mượn trả";
                    _pnlSubView.Controls.Add(_historyControl);
                    _historyControl.ReloadData();
                    break;

                case "Profile":
                    lblBreadcrumb.Text = "Cổng độc giả  ›  Thông tin tài khoản";
                    _pnlSubView.Controls.Add(_profileControl);
                    _profileControl.ReloadData();
                    break;
            }
        }

        private void UpdateToolbarActiveState(string activeKey)
        {
            Color activeBg = Color.FromArgb(220, 235, 248);
            _btnNavHome.BackColor = activeKey == "Home" ? activeBg : Color.White;
            _btnNavSearch.BackColor = activeKey == "Search" ? activeBg : Color.White;
            _btnNavLoans.BackColor = activeKey == "Loans" ? activeBg : Color.White;
            _btnNavFavorite.BackColor = activeKey == "Favorite" ? activeBg : Color.White;
            _btnNavHistory.BackColor = activeKey == "History" ? activeBg : Color.White;
            _btnNavProfile.BackColor = activeKey == "Profile" ? activeBg : Color.White;
        }

        private void RefreshHomeBadges()
        {
            if (Session.CurrentReaderId <= 0) return;

            using var context = new LibraryDbContext();
            int activeBorrowCount = context.LoanDetails
                .Include(d => d.Loan)
                .Count(d => d.Loan.IdReader == Session.CurrentReaderId && d.ReturnedAt == null);

            _btnNavLoans.Image = GlyphImage(GBook, 20, Primary, activeBorrowCount);
        }

        // ============================================================
        //  DỮ LIỆU TRANG CHỦ
        // ============================================================
        private void LoadData()
        {
            using var context = new LibraryDbContext();
            int readerId = Session.CurrentReaderId;

            var reader = context.Users.FirstOrDefault(u => u.Id == readerId);
            string readerName = reader?.FullName ?? "Độc giả";
            string readerCode = reader?.UserCode ?? "DG001";

            var card = context.ReaderCards
                .Where(c => c.IdReader == readerId && c.Status == ReaderCardStatus.Active)
                .OrderByDescending(c => c.ExpiresAt)
                .FirstOrDefault();
            string expire = card != null ? card.ExpiresAt.ToString("dd/MM/yyyy") : "Chưa cấp thẻ";

            lblName.Text = readerName;
            lblCode.Text = $"Mã độc giả: {readerCode} · Thẻ còn hạn đến {expire}";
            lblUserName.Text = readerName;
            lblUserCode.Text = $"Độc giả · {readerCode}";
            if (lblStatusReader != null) lblStatusReader.Text = $"Độc giả: {readerCode}";
            picAvatar.Invalidate();

            // Truy vấn số liệu thực tế của độc giả
            var activeLoanDetails = context.LoanDetails
                .Include(d => d.Loan)
                .Include(d => d.BookCopy)
                    .ThenInclude(cp => cp.Book)
                .Where(d => d.Loan.IdReader == readerId && d.ReturnedAt == null)
                .OrderBy(d => d.DueAt)
                .ToList();

            int returnedBooksCount = context.LoanDetails
                .Include(d => d.Loan)
                .Count(d => d.Loan.IdReader == readerId && d.ReturnedAt != null);

            var favoriteBookIds = context.FavoriteBooks
                .Where(f => f.ReaderId == readerId)
                .Select(f => f.BookId)
                .ToHashSet();

            _btnNavLoans.Image = GlyphImage(GBook, 20, Primary, activeLoanDetails.Count);

            string nearestDueText = "Không có";
            if (activeLoanDetails.Count > 0)
            {
                int minDays = (activeLoanDetails[0].DueAt.Date - DateTime.Today).Days;
                nearestDueText = minDays < 0
                    ? $"Quá {Math.Abs(minDays)} ngày"
                    : minDays == 0 ? "Hôm nay" : $"{minDays:D2} ngày";
            }

            // 4 Thẻ thống kê
            tblStats.Controls.Clear();
            AddStat(0, GBook, activeLoanDetails.Count.ToString("D2"), "Sách đang mượn", () => SwitchView("Loans"));
            AddStat(1, GClock, nearestDueText, "Đến hạn gần nhất", () => SwitchView("Loans"));
            _lblFavStatValue = AddStat(2, GHeart, favoriteBookIds.Count.ToString("D2"), "Sách yêu thích", () => SwitchView("Favorite"));
            AddStat(3, GCheck, returnedBooksCount.ToString("D2"), "Sách đã đọc", () => SwitchView("History"));

            // Sách đang mượn (tối đa 2 thẻ trên Trang chủ)
            flowBorrow.Controls.Clear();
            if (activeLoanDetails.Count == 0)
            {
                var emptyBorrow = new Label
                {
                    Text = "Hiện tại bạn chưa mượn cuốn sách nào. Bấm \"Tra cứu sách\" để tìm sách trong thư viện.",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = TextGray,
                    AutoSize = true,
                    Padding = new Padding(8, 24, 8, 8)
                };
                flowBorrow.Controls.Add(emptyBorrow);
            }
            else
            {
                foreach (var d in activeLoanDetails.Take(2))
                {
                    int daysLeft = (d.DueAt.Date - DateTime.Today).Days;
                    Color pillColor = daysLeft < 0 ? Red : (daysLeft <= 5 ? Orange : Green);
                    string statusText = daysLeft < 0
                        ? $"Quá hạn {Math.Abs(daysLeft)} ngày"
                        : $"Còn {daysLeft} ngày";

                    flowBorrow.Controls.Add(CreateBookCard(
                        d.BookCopy.Book.Title,
                        $"{d.BookCopy.Book.Author} · {d.BookCopy.Book.BookCode}",
                        $"Hạn trả: {d.DueAt:dd/MM/yyyy}",
                        Primary,
                        statusText,
                        pillColor,
                        d.BookCopy.IdBook,
                        false,
                        isBorrowedCard: true));
                }
                LayoutCards(flowBorrow, 2, flowBorrow.Height - 8);
            }

            // Thông báo động
            flowNotice.Controls.Clear();
            foreach (var d in activeLoanDetails.Where(x => (x.DueAt.Date - DateTime.Today).Days <= 5).Take(2))
            {
                int daysLeft = (d.DueAt.Date - DateTime.Today).Days;
                if (daysLeft < 0)
                    AddNotice(GBell, Red, "Sách đã quá hạn trả!", $"\"{d.BookCopy.Book.Title}\" đã quá hạn {Math.Abs(daysLeft)} ngày.");
                else
                    AddNotice(GBell, Orange, "Nhắc hạn trả sách", $"\"{d.BookCopy.Book.Title}\" sẽ đến hạn sau {daysLeft} ngày.");
            }

            var readyRes = context.Reservations
                .Include(r => r.Book)
                .FirstOrDefault(r => r.IdReader == readerId && r.Status == ReservationStatus.ReadyForPickup);
            if (readyRes != null)
            {
                AddNotice(GCheck, Green, "Sách đặt trước đã sẵn sàng", $"\"{readyRes.Book.Title}\" đang chờ bạn nhận tại quầy.");
            }

            AddNotice(GInfo, Primary, "Giờ mở cửa thư viện", "Thứ Hai - Thứ Bảy: 07:30 - 20:00.");

            // Gợi ý sách từ CSDL
            flowSuggest.Controls.Clear();
            var suggestBooks = context.Books
                .OrderBy(b => b.Id)
                .Take(5)
                .Select(b => new
                {
                    b.Id,
                    b.IdCategory,
                    b.Title,
                    b.Author,
                    Ok = context.BookCopies.Any(cp => cp.IdBook == b.Id && cp.CirculationStatus == CirculationStatus.Available)
                })
                .ToList();

            foreach (var s in suggestBooks)
            {
                Color c = (s.IdCategory % 4) switch
                {
                    1 => Primary,
                    2 => Color.FromArgb(40, 140, 110),
                    3 => Color.FromArgb(110, 80, 150),
                    _ => Color.FromArgb(165, 90, 60)
                };

                bool isFav = favoriteBookIds.Contains(s.Id);
                flowSuggest.Controls.Add(CreateBookCard(
                    s.Title,
                    s.Author,
                    null,
                    c,
                    s.Ok ? "Còn sách" : "Đã hết",
                    s.Ok ? Green : Red,
                    s.Id,
                    isFav,
                    isBorrowedCard: false));
            }
            LayoutCards(flowSuggest, 5, 82);
        }

        private bool ToggleFavoriteBook(int bookId)
        {
            if (Session.CurrentReaderId <= 0) return false;

            using var context = new LibraryDbContext();
            var existing = context.FavoriteBooks
                .FirstOrDefault(f => f.ReaderId == Session.CurrentReaderId && f.BookId == bookId);

            bool isNowFavorite;
            if (existing != null)
            {
                context.FavoriteBooks.Remove(existing);
                isNowFavorite = false;
            }
            else
            {
                context.FavoriteBooks.Add(new FavoriteBook
                {
                    ReaderId = Session.CurrentReaderId,
                    BookId = bookId,
                    AddedDate = DateTime.Now
                });
                isNowFavorite = true;
            }

            context.SaveChanges();

            int newCount = context.FavoriteBooks.Count(f => f.ReaderId == Session.CurrentReaderId);
            if (_lblFavStatValue != null)
            {
                _lblFavStatValue.Text = newCount.ToString("D2");
            }

            return isNowFavorite;
        }

        // ============================================================
        //  MENU + TOOLBAR
        // ============================================================
        private void BuildMenu()
        {
            menuStrip.Items.Clear();

            var mnuSystem = new ToolStripMenuItem("Hệ thống");
            mnuSystem.DropDownItems.Add("Trang chủ", null, (_, _) => SwitchView("Home"));
            mnuSystem.DropDownItems.Add(new ToolStripSeparator());
            mnuSystem.DropDownItems.Add("Đăng xuất", null, btnLogout_Click);

            var mnuSearch = new ToolStripMenuItem("Tra cứu");
            mnuSearch.DropDownItems.Add("Tìm kiếm sách & danh mục", null, (_, _) => SwitchView("Search"));
            mnuSearch.DropDownItems.Add("Sách yêu thích", null, (_, _) => SwitchView("Favorite"));

            var mnuBorrow = new ToolStripMenuItem("Mượn sách");
            mnuBorrow.DropDownItems.Add("Sách đang mượn & Đặt trước", null, (_, _) => SwitchView("Loans"));
            mnuBorrow.DropDownItems.Add("Lịch sử mượn trả & Tiền phạt", null, (_, _) => SwitchView("History"));

            var mnuAccount = new ToolStripMenuItem("Tài khoản");
            mnuAccount.DropDownItems.Add("Thông tin cá nhân & Đổi mật khẩu", null, (_, _) => SwitchView("Profile"));

            var mnuHelp = new ToolStripMenuItem("Trợ giúp");
            mnuHelp.DropDownItems.Add("Nội quy mượn trả thư viện", null, (_, _) =>
                MessageBox.Show(
                    "• Độc giả được mượn tối đa 3 cuốn sách cùng lúc.\n" +
                    "• Mỗi đầu sách được gia hạn trước khi đến hạn theo quy định từng thể loại.\n" +
                    "• Phí phạt trả trễ hạn: 5.000 đ / ngày / cuốn.",
                    "Nội quy thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information));

            menuStrip.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuSearch, mnuBorrow, mnuAccount, mnuHelp });
        }

        private void BuildToolbar()
        {
            toolStrip.Items.Clear();
            toolStrip.Padding = new Padding(8, 2, 0, 0);
            _btnNavHome = AddTool("Trang chủ", GHome, 0, true, (_, _) => SwitchView("Home"));
            _btnNavSearch = AddTool("Tra cứu sách", GSearch, 0, false, (_, _) => SwitchView("Search"));
            _btnNavLoans = AddTool("Sách đang mượn", GBook, 0, false, (_, _) => SwitchView("Loans"));
            _btnNavFavorite = AddTool("Sách yêu thích", GHeart, 0, false, (_, _) => SwitchView("Favorite"));
            _btnNavHistory = AddTool("Lịch sử mượn", GHistory, 0, false, (_, _) => SwitchView("History"));
            _btnNavProfile = AddTool("Thông tin tài khoản", GUser, 0, false, (_, _) => SwitchView("Profile"));
        }

        private ToolStripButton AddTool(string text, string glyph, int badge, bool active, EventHandler? onClick)
        {
            var btn = new ToolStripButton(text)
            {
                Image = GlyphImage(glyph, 20, Primary, badge),
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                TextImageRelation = TextImageRelation.ImageAboveText,
                Font = new Font("Segoe UI", 7.5F),
                Margin = new Padding(2, 2, 2, 2),
                Padding = new Padding(6, 0, 6, 0),
                BackColor = active ? Color.FromArgb(220, 235, 248) : Color.White,
            };
            if (onClick != null) btn.Click += onClick;
            toolStrip.Items.Add(btn);
            return btn;
        }

        private void StyleStatic()
        {
            picAvatar.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new SolidBrush(Primary);
                e.Graphics.FillEllipse(br, 0, 0, picAvatar.Width - 1, picAvatar.Height - 1);
                TextRenderer.DrawText(e.Graphics, Initials(lblUserName.Text),
                    new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    new Rectangle(0, 0, picAvatar.Width, picAvatar.Height), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            lblBreadcrumb.Paint += (_, _) => { };
            pnlSearch.Paint += (_, e) =>
                TextRenderer.DrawText(e.Graphics, GSearch, new Font(IconFont, 12F),
                    new Point(10, 12), TextGray);
            lnkAllBorrow.LinkClicked += (_, _) => SwitchView("Loans");
        }

        // ============================================================
        //  CÁC HÀM DỰNG THẺ TRANG CHỦ
        // ============================================================
        private Label AddStat(int col, string glyph, string value, string caption, Action onClick)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, col == 3 ? 0 : 12, 0),
                Cursor = Cursors.Hand
            };
            card.Paint += (_, e) =>
                e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

            card.Controls.Add(new Label
            {
                Text = glyph, Font = new Font(IconFont, 16F), ForeColor = col == 2 ? Red : Primary, AutoSize = true,
                Location = new Point(14, 20)
            });
            var lblVal = new Label
            {
                Text = value, Font = new Font("Segoe UI", 15F, FontStyle.Bold), AutoSize = true,
                Location = new Point(48, 8)
            };
            card.Controls.Add(lblVal);
            card.Controls.Add(new Label
            {
                Text = caption, Font = new Font("Segoe UI", 8F), ForeColor = TextGray, AutoSize = true,
                Location = new Point(50, 40)
            });

            card.Click += (_, _) => onClick();
            foreach (Control child in card.Controls)
                child.Click += (_, _) => onClick();

            tblStats.Controls.Add(card, col, 0);
            return lblVal;
        }

        private Panel CreateBookCard(string title, string subtitle, string? extra, Color cover,
            string status, Color statusColor, int bookId, bool isFavorite, bool isBorrowedCard)
        {
            var card = new Panel
            {
                Height = 74,
                Width = 300,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 8, 8),
                Cursor = Cursors.Hand
            };
            card.Paint += (_, e) =>
                e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

            var coverPnl = new Label
            {
                Text = GBook, Font = new Font(IconFont, 18F), ForeColor = Color.White, BackColor = cover,
                TextAlign = ContentAlignment.MiddleCenter, Size = new Size(44, 56), Location = new Point(8, 9),
                Cursor = Cursors.Hand
            };
            var lblTitle = new Label
            {
                Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoEllipsis = true,
                Size = new Size(145, 19), Location = new Point(60, 8), Cursor = Cursors.Hand
            };
            var lblSub = new Label
            {
                Text = subtitle, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoEllipsis = true,
                Size = new Size(145, 16), Location = new Point(60, 27), Cursor = Cursors.Hand
            };
            Label? lblExtra = null;
            if (extra != null)
            {
                lblExtra = new Label
                {
                    Text = extra, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray,
                    AutoEllipsis = true, Size = new Size(145, 16), Location = new Point(60, 44)
                };
            }

            void LayoutCardText()
            {
                int textWidth = Math.Max(70, card.ClientSize.Width - (isBorrowedCard ? 176 : 110));
                lblTitle.Width = textWidth;
                lblSub.Width = textWidth;
                if (lblExtra != null)
                    lblExtra.Width = textWidth;
            }
            card.Resize += (_, _) => LayoutCardText();

            card.Controls.Add(coverPnl);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblSub);
            if (lblExtra != null)
                card.Controls.Add(lblExtra);
            LayoutCardText();

            // Bấm vào bìa hoặc tiêu đề sách để mở Trang 3 (Chi tiết sách)
            card.Click += (_, _) => OpenBookDetail(bookId);
            coverPnl.Click += (_, _) => OpenBookDetail(bookId);
            lblTitle.Click += (_, _) => OpenBookDetail(bookId);
            lblSub.Click += (_, _) => OpenBookDetail(bookId);

            var pill = new Label
            {
                Text = "● " + status, Font = new Font("Segoe UI", 7.5F), ForeColor = statusColor,
                AutoSize = true, Padding = new Padding(4, 1, 4, 1), BackColor = Color.FromArgb(25, statusColor)
            };

            if (isBorrowedCard)
            {
                pill.Location = new Point(card.Width - 108, 24);
                pill.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else
            {
                pill.Location = new Point(60, 50);
                var heart = new Button
                {
                    Text = isFavorite ? GHeartFill : GHeart,
                    ForeColor = isFavorite ? Red : Color.Black,
                    Font = new Font(IconFont, 11F),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(34, 30),
                    Location = new Point(card.Width - 42, 8),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Tag = isFavorite,
                    Cursor = Cursors.Hand
                };
                heart.FlatAppearance.BorderColor = BorderGray;
                heart.Click += (_, _) =>
                {
                    try
                    {
                        bool fav = ToggleFavoriteBook(bookId);
                        heart.Tag = fav;
                        heart.Text = fav ? GHeartFill : GHeart;
                        heart.ForeColor = fav ? Red : Color.Black;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi lưu sách yêu thích:\n{ex.Message}",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                card.Controls.Add(heart);
            }

            card.Controls.Add(pill);
            pill.BringToFront();
            return card;
        }

        private void AddNotice(string glyph, Color color, string title, string desc)
        {
            var row = new Panel
                { Height = 50, Width = flowNotice.ClientSize.Width - 8, Margin = new Padding(4, 0, 4, 0) };
            row.Paint += (_, e) =>
                e.Graphics.DrawLine(new Pen(BorderGray), 0, row.Height - 1, row.Width, row.Height - 1);
            row.Controls.Add(new Label
            {
                Text = glyph, Font = new Font(IconFont, 13F), ForeColor = color, AutoSize = true,
                Location = new Point(6, 14)
            });
            row.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true,
                Location = new Point(40, 6)
            });
            row.Controls.Add(new Label
            {
                Text = desc, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoSize = true,
                Location = new Point(40, 26)
            });
            flowNotice.Controls.Add(row);
        }

        private static void LayoutCards(FlowLayoutPanel flow, int perRow, int height)
        {
            if (flow.Controls.Count == 0 || flow.ClientSize.Width <= 0) return;
            int w = (flow.ClientSize.Width - flow.Padding.Left - 8 * perRow) / perRow;
            foreach (Control c in flow.Controls)
            {
                if (c is Panel)
                {
                    c.Width = Math.Max(w, 120);
                    c.Height = Math.Min(height, 74);
                }
            }
        }

        // ============================================================
        //  TIỆN ÍCH
        // ============================================================
        private static Bitmap GlyphImage(string glyph, int size, Color color, int badge)
        {
            var bmp = new Bitmap(size + 4, size + 4);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var f = new Font(IconFont, size, GraphicsUnit.Pixel);
            TextRenderer.DrawText(g, glyph, f, new Rectangle(0, 0, bmp.Width, bmp.Height), color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            if (badge > 0)
            {
                using var br = new SolidBrush(Red);
                g.FillEllipse(br, bmp.Width - 15, 0, 15, 15);
                TextRenderer.DrawText(g, badge.ToString(), new Font("Segoe UI", 7F, FontStyle.Bold),
                    new Rectangle(bmp.Width - 15, 0, 15, 15), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            return bmp;
        }

        private static string Initials(string name)
        {
            var p = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return p.Length == 0 ? "" : (p[0][0].ToString() + p[^1][0]).ToUpper();
        }

        // ============================================================
        //  SỰ KIỆN
        // ============================================================
        private void btnSearch_Click(object? sender, EventArgs e)
        {
            SwitchView("Search", txtSearch.Text.Trim());
        }

        private void txtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnSearch_Click(sender, e);
            }
        }

        private void btnLogout_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            IsLogout = true;
            Close();
        }

        private ToolStripStatusLabel lblStatusReader = null!;

        private void BuildStatusBar()
        {
            var status = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                SizingGrip = true,
                BackColor = Color.FromArgb(240, 242, 245),
                Font = new Font("Segoe UI", 8F)
            };

            var dot = new ToolStripStatusLabel("●") { ForeColor = Green, Margin = new Padding(4, 3, 0, 2) };
            var connected = new ToolStripStatusLabel("Đã kết nối") { Margin = new Padding(0, 3, 6, 2) };
            var hours = new ToolStripStatusLabel("Thư viện Trung tâm · Giờ mở cửa: 07:30 - 20:00");
            var spring = new ToolStripStatusLabel { Spring = true };

            lblStatusReader = new ToolStripStatusLabel("Độc giả: DG001");
            var version = new ToolStripStatusLabel("Phiên bản 2.4.0");

            status.Items.AddRange(new ToolStripItem[]
            {
                dot, connected, new ToolStripSeparator(), hours,
                spring,
                new ToolStripSeparator(), lblStatusReader,
                new ToolStripSeparator(), version
            });

            Controls.Add(status);
        }
    }
}