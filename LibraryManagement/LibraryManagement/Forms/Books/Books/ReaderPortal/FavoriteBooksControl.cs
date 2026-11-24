using LibraryManagement.Data;
using LibraryManagement.Helpers;

namespace LibraryManagement.Forms.Books.ReaderPortal;

public class FavoriteBooksControl : UserControl
{
    private static readonly Color Primary = Color.FromArgb(19, 98, 160);
    private static readonly Color SoftBlue = Color.FromArgb(232, 240, 250);
    private static readonly Color BgGray = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);
    private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
    private static readonly Color Green = Color.FromArgb(34, 160, 90);
    private static readonly Color Red = Color.FromArgb(200, 50, 50);

    private const string IconFont = "Segoe MDL2 Assets";
    private const string GBook = "\uE82D";
    private const string GHeart = "\uEB51";
    private const string GHeartFill = "\uEB52";
    private const string GSearch = "\uE721";
    private const string GRefresh = "\uE72C";

    public event Action<int>? OnOpenBookDetail;
    public event Action? OnDataChanged;
    public event Action? OnNavigateSearchRequested;

    private Label _lblSubtitle = null!;
    private TextBox _txtSearch = null!;
    private ComboBox _cboCategory = null!;
    private ComboBox _cboStatus = null!;
    private ComboBox _cboSort = null!;
    private Label _lblResultCount = null!;
    private FlowLayoutPanel _flowBooks = null!;
    private Panel _pnlEmpty = null!;
    private Label _lblEmptyTitle = null!;
    private Label _lblEmptyDesc = null!;
    private readonly ToolTip _toolTip = new();

    private List<FavoriteBookItem> _allFavorites = new();

    public FavoriteBooksControl()
    {
        Dock = DockStyle.Fill;
        BackColor = BgGray;
        Padding = new Padding(12, 0, 12, 12);

        BuildUi();
        _flowBooks.Resize += (_, _) => LayoutCards();
        ReloadData();
    }

    public void ReloadData()
    {
        if (Session.CurrentReaderId <= 0) return;

        try
        {
            int readerId = Session.CurrentReaderId;
            using var context = new LibraryDbContext();

            int selectedCategoryId = (_cboCategory.SelectedItem as CategoryOption)?.Id ?? 0;
            var categories = context.BookCategories
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryOption { Id = c.Id, Name = c.CategoryName })
                .ToList();

            categories.Insert(0, new CategoryOption { Id = 0, Name = "Tất cả thể loại" });
            _cboCategory.DataSource = categories;
            _cboCategory.DisplayMember = nameof(CategoryOption.Name);
            _cboCategory.ValueMember = nameof(CategoryOption.Id);
            _cboCategory.SelectedValue = categories.Any(c => c.Id == selectedCategoryId) ? selectedCategoryId : 0;

            _allFavorites = (from fav in context.FavoriteBooks
                             join book in context.Books on fav.BookId equals book.Id
                             join cat in context.BookCategories on book.IdCategory equals cat.Id
                             where fav.ReaderId == readerId
                             orderby fav.AddedDate descending
                             select new FavoriteBookItem
                             {
                                 BookId = book.Id,
                                 BookCode = book.BookCode,
                                 Isbn = book.ISBN ?? "Chưa cập nhật",
                                 Title = book.Title,
                                 Author = book.Author,
                                 Publisher = book.Publisher ?? "Chưa rõ",
                                 PublishedYear = book.PublishedYear ?? "",
                                 CategoryId = cat.Id,
                                 CategoryName = cat.CategoryName,
                                 AddedDate = fav.AddedDate,
                                 TotalCopies = context.BookCopies.Count(cp => cp.IdBook == book.Id),
                                 AvailableCopies = context.BookCopies.Count(cp =>
                                     cp.IdBook == book.Id && cp.CirculationStatus == "Available"),
                                 ShelfLocation = context.BookCopies
                                     .Where(cp => cp.IdBook == book.Id && cp.ShelfLocation != null)
                                     .Select(cp => cp.ShelfLocation!)
                                     .FirstOrDefault() ?? "Chưa xếp kệ"
                             }).ToList();

            _lblSubtitle.Text = $"Bạn đang lưu {_allFavorites.Count} cuốn sách yêu thích · Bấm \"Chi tiết\" để xem bản sao & đặt trước";
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải danh sách sách yêu thích:\n{ex.Message}",
                "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BuildUi()
    {
        SuspendLayout();

        var pnlBody = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 12, 0, 0)
        };

        _flowBooks = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = BgGray,
            Padding = new Padding(0, 2, 0, 8)
        };

        _pnlEmpty = BuildEmptyPanel();
        _pnlEmpty.Visible = false;

        pnlBody.Controls.Add(_flowBooks);
        pnlBody.Controls.Add(_pnlEmpty);

        Controls.Add(pnlBody);
        Controls.Add(BuildFilterBar());
        Controls.Add(BuildHeaderBanner());

        ResumeLayout(true);
    }

    private Panel BuildHeaderBanner()
    {
        var banner = new Panel
        {
            Dock = DockStyle.Top,
            Height = 82,
            BackColor = Primary
        };

        var lblTitle = new Label
        {
            Text = "SÁCH YÊU THÍCH CỦA TÔI",
            Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(18, 15)
        };

        _lblSubtitle = new Label
        {
            Text = "Đang tải danh sách sách yêu thích...",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(220, 235, 248),
            AutoSize = true,
            Location = new Point(19, 46)
        };

        var pnlSearch = new Panel
        {
            Size = new Size(430, 40),
            Location = new Point(banner.Width - 448, 21),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.White
        };
        pnlSearch.Paint += (_, e) =>
            TextRenderer.DrawText(e.Graphics, GSearch, new Font(IconFont, 11F),
                new Point(10, 11), TextGray);

        _txtSearch = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9.5F),
            PlaceholderText = "Tìm tên sách, tác giả, mã sách...",
            Location = new Point(36, 11),
            Size = new Size(295, 20)
        };
        _txtSearch.TextChanged += (_, _) => ApplyFilter();

        var btnClearSearch = new Button
        {
            Text = "Xóa tìm",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(21, 134, 200),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8.5F),
            Size = new Size(84, 28),
            Location = new Point(338, 6),
            Cursor = Cursors.Hand
        };
        btnClearSearch.FlatAppearance.BorderSize = 0;
        btnClearSearch.Click += (_, _) =>
        {
            _txtSearch.Clear();
            _txtSearch.Focus();
        };

        pnlSearch.Controls.Add(_txtSearch);
        pnlSearch.Controls.Add(btnClearSearch);

        banner.Controls.Add(lblTitle);
        banner.Controls.Add(_lblSubtitle);
        banner.Controls.Add(pnlSearch);

        return banner;
    }

    private Panel BuildFilterBar()
    {
        var host = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            Padding = new Padding(0, 10, 0, 0)
        };

        var bar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        bar.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, bar.Width - 1, bar.Height - 1);

        var lblCat = new Label
        {
            Text = "Thể loại:",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(14, 14)
        };

        _cboCategory = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(70, 10),
            Size = new Size(175, 25)
        };
        _cboCategory.SelectedIndexChanged += (_, _) => ApplyFilter();

        var lblStatus = new Label
        {
            Text = "Tình trạng:",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(260, 14)
        };

        _cboStatus = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(328, 10),
            Size = new Size(150, 25)
        };
        _cboStatus.Items.AddRange(new object[] { "Tất cả trạng thái", "Còn sách sẵn sàng", "Đã hết sách" });
        _cboStatus.SelectedIndex = 0;
        _cboStatus.SelectedIndexChanged += (_, _) => ApplyFilter();

        var lblSort = new Label
        {
            Text = "Sắp xếp:",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(494, 14)
        };

        _cboSort = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(550, 10),
            Size = new Size(155, 25)
        };
        _cboSort.Items.AddRange(new object[] { "Mới lưu gần đây", "Lưu cũ nhất", "Tên sách (A → Z)" });
        _cboSort.SelectedIndex = 0;
        _cboSort.SelectedIndexChanged += (_, _) => ApplyFilter();

        var btnRefresh = new Button
        {
            Text = "  Làm mới",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 8.5F),
            Size = new Size(96, 28),
            Location = new Point(bar.Width - 110, 9),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Image = GlyphImage(GRefresh, 13, Primary),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderColor = BorderGray;
        btnRefresh.Click += (_, _) => ReloadData();

        _lblResultCount = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Primary,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(bar.Width - 260, 14)
        };

        bar.Resize += (_, _) =>
        {
            btnRefresh.Left = bar.Width - btnRefresh.Width - 12;
            _lblResultCount.Left = btnRefresh.Left - _lblResultCount.Width - 16;
        };

        bar.Controls.AddRange(new Control[]
        {
            lblCat, _cboCategory, lblStatus, _cboStatus, lblSort, _cboSort, _lblResultCount, btnRefresh
        });

        host.Controls.Add(bar);
        return host;
    }

    private Panel BuildEmptyPanel()
    {
        var pnl = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        pnl.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

        var centerBox = new Panel
        {
            Size = new Size(460, 220),
            BackColor = Color.White
        };

        var lblIcon = new Label
        {
            Text = GHeart,
            Font = new Font(IconFont, 38F),
            ForeColor = Color.FromArgb(180, 190, 204),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(460, 64),
            Location = new Point(0, 10)
        };

        _lblEmptyTitle = new Label
        {
            Text = "Bạn chưa có cuốn sách yêu thích nào",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 52, 64),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(460, 30),
            Location = new Point(0, 82)
        };

        _lblEmptyDesc = new Label
        {
            Text = "Hãy tìm kiếm sách trong thư viện và bấm vào biểu tượng trái tim để lưu vào danh sách của bạn.",
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(420, 44),
            Location = new Point(20, 114)
        };

        var btnExplore = new Button
        {
            Text = "Khám phá kho sách ngay",
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(200, 36),
            Location = new Point(130, 170),
            Cursor = Cursors.Hand
        };
        btnExplore.FlatAppearance.BorderSize = 0;
        btnExplore.Click += (_, _) => OnNavigateSearchRequested?.Invoke();

        centerBox.Controls.AddRange(new Control[] { lblIcon, _lblEmptyTitle, _lblEmptyDesc, btnExplore });
        pnl.Controls.Add(centerBox);

        pnl.Resize += (_, _) =>
        {
            centerBox.Location = new Point(
                Math.Max(0, (pnl.Width - centerBox.Width) / 2),
                Math.Max(20, (pnl.Height - centerBox.Height) / 2 - 10));
        };

        return pnl;
    }

    private void ApplyFilter()
    {
        if (_cboStatus.SelectedIndex < 0 || _cboSort.SelectedIndex < 0) return;

        string keyword = _txtSearch.Text.Trim().ToLowerInvariant();
        int categoryId = (_cboCategory.SelectedItem as CategoryOption)?.Id ?? 0;
        int statusMode = _cboStatus.SelectedIndex;
        int sortMode = _cboSort.SelectedIndex;

        IEnumerable<FavoriteBookItem> query = _allFavorites;

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(b =>
                b.Title.ToLowerInvariant().Contains(keyword) ||
                b.Author.ToLowerInvariant().Contains(keyword) ||
                b.BookCode.ToLowerInvariant().Contains(keyword));
        }

        if (categoryId > 0)
            query = query.Where(b => b.CategoryId == categoryId);

        if (statusMode == 1)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (statusMode == 2)
            query = query.Where(b => b.AvailableCopies == 0);

        query = sortMode switch
        {
            1 => query.OrderBy(b => b.AddedDate),
            2 => query.OrderBy(b => b.Title),
            _ => query.OrderByDescending(b => b.AddedDate)
        };

        var filtered = query.ToList();
        _lblResultCount.Text = $"Hiển thị {filtered.Count} / {_allFavorites.Count} cuốn";
        if (_lblResultCount.Parent != null)
        {
            _lblResultCount.Left = _lblResultCount.Parent.Width - 110 - _lblResultCount.Width - 16;
        }

        _flowBooks.SuspendLayout();
        _flowBooks.Controls.Clear();

        if (filtered.Count == 0)
        {
            _flowBooks.Visible = false;
            _pnlEmpty.Visible = true;

            if (_allFavorites.Count == 0)
            {
                _lblEmptyTitle.Text = "Bạn chưa có cuốn sách yêu thích nào";
                _lblEmptyDesc.Text = "Hãy tìm kiếm sách trong thư viện và bấm vào biểu tượng trái tim để lưu vào danh sách của bạn.";
            }
            else
            {
                _lblEmptyTitle.Text = "Không tìm thấy sách phù hợp với bộ lọc";
                _lblEmptyDesc.Text = "Thử xóa từ khóa tìm kiếm hoặc chọn \"Tất cả thể loại\" để xem lại toàn bộ sách đã lưu.";
            }
        }
        else
        {
            _pnlEmpty.Visible = false;
            _flowBooks.Visible = true;

            foreach (var item in filtered)
            {
                _flowBooks.Controls.Add(CreateFavoriteCard(item));
            }
            LayoutCards();
        }

        _flowBooks.ResumeLayout(true);
    }

    private Panel CreateFavoriteCard(FavoriteBookItem book)
    {
        Color coverColor = GetCoverColor(book.CategoryId);
        bool isAvailable = book.AvailableCopies > 0;
        Color statusColor = isAvailable ? Green : Red;

        var card = new Panel
        {
            Width = 500,
            Height = 132,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 12, 12)
        };

        bool hovered = false;
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(hovered ? Primary : BorderGray, hovered ? 1.5f : 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        card.MouseEnter += (_, _) => { hovered = true; card.Invalidate(); };
        card.MouseLeave += (_, _) => { hovered = false; card.Invalidate(); };

        var cover = new Label
        {
            Text = GBook,
            Font = new Font(IconFont, 22F),
            ForeColor = Color.White,
            BackColor = coverColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(62, 104),
            Location = new Point(14, 14)
        };

        var lblCode = new Label
        {
            Text = book.BookCode,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = Primary,
            BackColor = SoftBlue,
            AutoSize = true,
            Padding = new Padding(5, 2, 5, 2),
            Location = new Point(88, 14)
        };

        var lblCategory = new Label
        {
            Text = book.CategoryName,
            Font = new Font("Segoe UI", 8F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(156, 16)
        };

        var lblTitle = new Label
        {
            Text = book.Title,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 32, 42),
            AutoEllipsis = true,
            Size = new Size(260, 24),
            Location = new Point(88, 39),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _toolTip.SetToolTip(lblTitle, book.Title);

        string pubInfo = string.IsNullOrEmpty(book.PublishedYear)
            ? book.Publisher
            : $"{book.Publisher} ({book.PublishedYear})";
        var lblAuthor = new Label
        {
            Text = $"Tác giả: {book.Author}  ·  {pubInfo}",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(75, 82, 92),
            AutoEllipsis = true,
            Size = new Size(260, 20),
            Location = new Point(88, 66),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var lblMeta = new Label
        {
            Text = $"Đã lưu: {book.AddedDate:dd/MM/yyyy}  ·  Vị trí: {book.ShelfLocation}",
            Font = new Font("Segoe UI", 8F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(88, 94)
        };

        string statusText = isAvailable
            ? $"● Còn {book.AvailableCopies}/{book.TotalCopies} cuốn"
            : $"● Đã hết (0/{book.TotalCopies})";

        var lblStatus = new Label
        {
            Text = statusText,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = statusColor,
            BackColor = Color.FromArgb(24, statusColor),
            AutoSize = true,
            Padding = new Padding(6, 2, 6, 2),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(card.Width - 134, 14)
        };

        var btnDetail = new Button
        {
            Text = "Chi tiết",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Primary,
            Font = new Font("Segoe UI", 8.5F),
            Size = new Size(106, 30),
            Location = new Point(card.Width - 120, 48),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };
        btnDetail.FlatAppearance.BorderColor = Primary;
        btnDetail.Click += (_, _) => OnOpenBookDetail?.Invoke(book.BookId);

        var btnRemove = new Button
        {
            Text = "  Bỏ thích",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(254, 242, 242),
            ForeColor = Red,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Size = new Size(106, 30),
            Location = new Point(card.Width - 120, 86),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Image = GlyphImage(GHeartFill, 13, Red),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand
        };
        btnRemove.FlatAppearance.BorderColor = Color.FromArgb(245, 195, 195);
        btnRemove.Click += (_, _) => RemoveFavorite(book);

        card.Resize += (_, _) =>
        {
            lblStatus.Left = card.Width - lblStatus.Width - 14;
            btnDetail.Left = card.Width - btnDetail.Width - 14;
            btnRemove.Left = card.Width - btnRemove.Width - 14;
            int textMaxWidth = Math.Max(140, btnDetail.Left - 102);
            lblTitle.Width = textMaxWidth;
            lblAuthor.Width = textMaxWidth;
        };

        card.Controls.AddRange(new Control[]
        {
            cover, lblCode, lblCategory, lblTitle, lblAuthor, lblMeta, lblStatus, btnDetail, btnRemove
        });

        return card;
    }

    private void LayoutCards()
    {
        if (_flowBooks.Controls.Count == 0 || _flowBooks.ClientSize.Width <= 0) return;

        int cols = _flowBooks.ClientSize.Width >= 920 ? 2 : 1;
        int gap = 12;
        int cardWidth = (_flowBooks.ClientSize.Width - gap * cols - 4) / cols;

        foreach (Control c in _flowBooks.Controls)
        {
            c.Width = Math.Max(cardWidth, 380);
        }
    }

    private void RemoveFavorite(FavoriteBookItem book)
    {
        if (Session.CurrentReaderId <= 0) return;

        try
        {
            using var context = new LibraryDbContext();
            var entity = context.FavoriteBooks
                .FirstOrDefault(f => f.ReaderId == Session.CurrentReaderId && f.BookId == book.BookId);

            if (entity != null)
            {
                context.FavoriteBooks.Remove(entity);
                context.SaveChanges();
            }

            _allFavorites.RemoveAll(b => b.BookId == book.BookId);
            _lblSubtitle.Text = $"Bạn đang lưu {_allFavorites.Count} cuốn sách yêu thích · Bấm \"Chi tiết\" để xem bản sao & đặt trước";
            ApplyFilter();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể bỏ yêu thích cuốn sách này:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Color GetCoverColor(int categoryId) => (categoryId % 4) switch
    {
        1 => Primary,
        2 => Color.FromArgb(40, 140, 110),
        3 => Color.FromArgb(110, 80, 150),
        _ => Color.FromArgb(165, 90, 60)
    };

    private static Bitmap GlyphImage(string glyph, int size, Color color)
    {
        var bmp = new Bitmap(size + 6, size + 6);
        using var g = Graphics.FromImage(bmp);
        using var f = new Font(IconFont, size, GraphicsUnit.Pixel);
        TextRenderer.DrawText(g, glyph, f, new Rectangle(0, 0, bmp.Width, bmp.Height), color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        return bmp;
    }

    private sealed class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class FavoriteBookItem
    {
        public int BookId { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string PublishedYear { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string ShelfLocation { get; set; } = string.Empty;
    }
}