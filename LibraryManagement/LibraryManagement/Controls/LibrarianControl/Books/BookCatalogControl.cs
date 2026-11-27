using System.Drawing.Drawing2D;
using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.Books;

public class BookCatalogControl : UserControl
{
    // ====== Bảng màu đồng bộ hệ thống ======
    private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
    private static readonly Color BgGray     = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
    private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
    private static readonly Color Green      = Color.FromArgb(34, 160, 90);
    private static readonly Color Red        = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    // Glyph icons
    private const string GAdd      = "\uE710";
    private const string GEdit     = "\uE70F";
    private const string GToggle   = "\uE8AB";
    private const string GCopies   = "\uE8F1";
    private const string GRefresh  = "\uE72C";

    private readonly CatalogManagementService _service = new();
    private List<Book> _books = new();

    private Label _lblSummary = null!;
    private TextBox _txtSearch = null!;
    private ComboBox _cboCategoryFilter = null!;
    private CheckBox _chkShowDiscontinued = null!;
    private DataGridView _dgvBooks = null!;

    public event Action<Book>? ViewCopiesRequested;
    public event Action? CatalogDataChanged;

    public BookCatalogControl()
    {
        BackColor = BgGray;
        Font = new Font("Segoe UI", 9F);
        BuildUi();
        LoadFilters();
        LoadData();
    }

    // ============================================================
    //  DỰNG GIAO DIỆN ĐỒNG BỘ
    // ============================================================
    private void BuildUi()
    {
        SuspendLayout();

        // 1. Header + FilterBar (Top)
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = BgGray };
        pnlTop.Controls.Add(BuildFilterBar());
        pnlTop.Controls.Add(BuildHeader());

        // 2. Main GroupBox chứa DataGridView
        var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
        var grpList = new GroupBox
        {
            Text = "Danh mục đầu sách trong thư viện (Nhấp đúp vào dòng để chỉnh sửa)",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10)
        };

        _dgvBooks = BuildGrid();
        grpList.Controls.Add(_dgvBooks);
        pnlMain.Controls.Add(grpList);

        Controls.Add(pnlMain);
        Controls.Add(pnlTop);

        ResumeLayout(true);
    }

    private Panel BuildHeader()
    {
        var pnl = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = SoftBlue };
        pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ DANH MỤC ĐẦU SÁCH",
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(14, 8)
        };
        pnl.Controls.Add(lblTitle);

        _lblSummary = new Label
        {
            Text = "Đang tải danh mục đầu sách...",
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(16, 32)
        };
        pnl.Controls.Add(_lblSummary);

        // Dùng FlowLayoutPanel neo bên phải để các nút tự căn hàng thẳng tắp, không bao giờ chồng chéo hay mất chữ
        var flowActions = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var btnRefresh = CreateActionButton("Làm mới", GRefresh, Color.White, Color.Black);
        btnRefresh.Click += (_, _) => LoadData();

        var btnViewCopies = CreateActionButton("Xem bản sách", GCopies, Color.White, Primary);
        btnViewCopies.Click += (_, _) => { if (GetSelectedBook() is { } b) ViewCopiesRequested?.Invoke(b); };

        var btnToggleStatus = CreateActionButton("Ngừng / Mở lưu hành", GToggle, Color.FromArgb(255, 244, 229), Orange);
        btnToggleStatus.Click += BtnToggleStatus_Click;

        var btnEdit = CreateActionButton("Chỉnh sửa", GEdit, Color.White, Color.Black);
        btnEdit.Click += (_, _) => { if (GetSelectedBook() is { } b) OpenBookDialog(b); };

        var btnAdd = CreateActionButton("Thêm đầu sách", GAdd, Primary, Color.White);
        btnAdd.Click += (_, _) => OpenBookDialog(null);

        // Add theo thứ tự từ phải sang trái vì FlowDirection = RightToLeft
        flowActions.Controls.AddRange(new Control[] { btnRefresh, btnViewCopies, btnToggleStatus, btnEdit, btnAdd });
        pnl.Controls.Add(flowActions);

        void LayoutHeader()
        {
            flowActions.Location = new Point(
                Math.Max(lblTitle.Right + 16, pnl.ClientSize.Width - flowActions.Width - 12),
                (pnl.ClientSize.Height - flowActions.Height) / 2
            );
        }

        pnl.Resize += (_, _) => LayoutHeader();
        LayoutHeader();

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

        var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, ForeColor = TextGray, Location = new Point(14, 16) };

        _txtSearch = new TextBox
        {
            Location = new Point(0, 12),
            Width = 300,
            PlaceholderText = "Tên đầu sách, tác giả, mã ISBN hoặc mã sách..."
        };
        _txtSearch.TextChanged += (_, _) => LoadData();

        var lblCat = new Label { Text = "Thể loại:", AutoSize = true, ForeColor = TextGray, Location = new Point(0, 16) };

        _cboCategoryFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(0, 12),
            Width = 215
        };
        _cboCategoryFilter.SelectedIndexChanged += (_, _) => LoadData();

        _chkShowDiscontinued = new CheckBox
        {
            Text = "Hiển thị cả sách ngừng lưu hành",
            Checked = true,
            AutoSize = true,
            ForeColor = Color.FromArgb(60, 65, 75)
        };
        _chkShowDiscontinued.CheckedChanged += (_, _) => LoadData();

        void LayoutFilterBar()
        {
            _chkShowDiscontinued.Left = pnl.ClientSize.Width - _chkShowDiscontinued.Width - 14;
            _chkShowDiscontinued.Top = (pnl.ClientSize.Height - _chkShowDiscontinued.Height) / 2;

            _cboCategoryFilter.Left = _chkShowDiscontinued.Left - _cboCategoryFilter.Width - 14;
            lblCat.Left = _cboCategoryFilter.Left - lblCat.Width - 8;

            _txtSearch.Left = lblSearch.Right + 10;
            _txtSearch.Width = Math.Max(160, lblCat.Left - _txtSearch.Left - 14);

            lblSearch.Top = (pnl.ClientSize.Height - lblSearch.Height) / 2;
            lblCat.Top = (pnl.ClientSize.Height - lblCat.Height) / 2;
        }

        pnl.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblCat, _cboCategoryFilter, _chkShowDiscontinued });
        pnl.Resize += (_, _) => LayoutFilterBar();
        LayoutFilterBar();
        return pnl;
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
            ColumnHeadersHeight = 46,
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

        g.Columns.Add("colCode",     "Mã sách");
        g.Columns.Add("colIsbn",     "ISBN");
        g.Columns.Add("colTitle",    "Tên đầu sách");
        g.Columns.Add("colAuthor",   "Tác giả");
        g.Columns.Add("colCategory", "Thể loại");
        g.Columns.Add("colPub",      "NXB / Năm");
        g.Columns.Add("colCopies",   "Bản sách (Sẵn/Tổng)");
        g.Columns.Add("colStatus",   "Trạng thái");

        g.Columns["colCode"]!.DefaultCellStyle.ForeColor = Primary;
        g.Columns["colCode"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        g.Columns["colCode"]!.FillWeight     = 75;
        g.Columns["colIsbn"]!.FillWeight     = 85;
        g.Columns["colTitle"]!.FillWeight    = 155;
        g.Columns["colAuthor"]!.FillWeight   = 110;
        g.Columns["colCategory"]!.FillWeight = 95;
        g.Columns["colPub"]!.FillWeight      = 95;
        g.Columns["colCopies"]!.FillWeight   = 95;
        g.Columns["colStatus"]!.FillWeight   = 105;

        g.CellPainting += DgvBooks_CellPainting;
        g.CellDoubleClick += (_, _) => { if (GetSelectedBook() is { } b) OpenBookDialog(b); };
        return g;
    }

    private void DgvBooks_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 7 || e.Graphics == null) return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
        string text = e.Value?.ToString() ?? "";
        Color c = text == "Đang lưu hành" ? Green : Red;

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
    //  LOAD DATA & NGHIỆP VỤ
    // ============================================================
    public void LoadFilters()
    {
        var cats = _service.GetAllCategories();
        var list = new List<object> { new { Id = 0, Name = "-- Tất cả thể loại --" } };
        list.AddRange(cats.Select(c => new { Id = c.Id, Name = $"{c.CategoryName} ({c.CategoryCode})" }));
        _cboCategoryFilter.DataSource = list;
        _cboCategoryFilter.DisplayMember = "Name";
        _cboCategoryFilter.ValueMember = "Id";
    }

    public void LoadData()
    {
        int catId = _cboCategoryFilter.SelectedValue is int id ? id : 0;
        _books = _service.SearchBooks(_txtSearch.Text, catId, _chkShowDiscontinued.Checked);

        int activeCount = _books.Count(b => b.Status == "Active");
        int totalCopies = _books.Sum(b => b.Copies.Count);
        int availCopies = _books.Sum(b => b.Copies.Count(c => c.CirculationStatus == CirculationStatus.Available));

        _lblSummary.Text =
            $"Tổng đầu sách: {_books.Count:N0}  ·  Đang lưu hành: {activeCount:N0}  ·  Bản sách sẵn sàng: {availCopies:N0}/{totalCopies:N0} cuốn";

        _dgvBooks.Rows.Clear();
        foreach (var b in _books)
        {
            int total = b.Copies.Count;
            int avail = b.Copies.Count(c => c.CirculationStatus == CirculationStatus.Available);
            string statusText = b.Status == "Active" ? "Đang lưu hành" : "Ngừng lưu hành";

            int idx = _dgvBooks.Rows.Add(
                b.BookCode,
                b.Isbn ?? "-",
                b.Title,
                b.Author,
                b.Category?.CategoryName ?? "-",
                $"{b.Publisher ?? "-"} ({(b.PublishedYear > 0 ? b.PublishedYear.ToString() : "-")})",
                $"{avail} / {total}",
                statusText
            );
            _dgvBooks.Rows[idx].Tag = b;
            if (b.Status != "Active")
                _dgvBooks.Rows[idx].DefaultCellStyle.ForeColor = TextGray;
        }
    }

    private Book? GetSelectedBook()
    {
        if (_dgvBooks.CurrentRow?.Tag is Book b) return b;
        MessageBox.Show("Vui lòng chọn một đầu sách trong danh sách.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void OpenBookDialog(Book? book)
    {
        using var dlg = new BookEditDialog(book);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            LoadData();
            CatalogDataChanged?.Invoke();
        }
    }

    private void BtnToggleStatus_Click(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        try
        {
            _service.ToggleBookDiscontinued(book.Id);
            LoadData();
            CatalogDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể đổi trạng thái", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static Button CreateActionButton(string text, string glyph, Color back, Color fore)
    {
        // Vẽ glyph vào khung 16x16 với NoGlyphOverhang để căn chính giữa tâm dọc, không bị tụt xuống đáy
        var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        using (var f = new Font(IconFont, 13F, GraphicsUnit.Pixel))
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            TextRenderer.DrawText(
                g,
                glyph,
                f,
                new Rectangle(0, 0, 16, 16),
                fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );
        }

        var btn = new Button
        {
            Text = " " + text,
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = fore,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 34),
            MaximumSize = new Size(0, 34),
            Image = bmp,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(8, 0, 10, 0),
            Margin = new Padding(6, 0, 0, 0),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            UseCompatibleTextRendering = false,
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

public class BookEditDialog : Form
{
    private readonly CatalogManagementService _service = new();
    private readonly Book _book;

    private TextBox _txtCode = null!;
    private TextBox _txtIsbn = null!;
    private TextBox _txtTitle = null!;
    private TextBox _txtAuthor = null!;
    private ComboBox _cboCategory = null!;
    private TextBox _txtPublisher = null!;
    private TextBox _txtYear = null!;
    private TextBox _txtLanguage = null!;
    private NumericUpDown _numPages = null!;
    private TextBox _txtCoverPath = null!;
    private PictureBox _picCover = null!;
    private TextBox _txtDesc = null!;

    public BookEditDialog(Book? book)
    {
        _book = book ?? new Book { BookCode = _service.GenerateNextBookCode(), Language = "Tiếng Việt" };
        Text = book == null ? "Thêm mới đầu sách" : $"Cập nhật đầu sách - {book.BookCode}";
        Size = new Size(760, 585);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        BuildUi();
        BindData();
    }

    private void BuildUi()
    {
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(232, 240, 250) };
        pnlHeader.Controls.Add(new Label
        {
            Text = Text.ToUpper(),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(19, 98, 160),
            AutoSize = true,
            Location = new Point(16, 13)
        });

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(14) };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _txtCode = new TextBox { Dock = DockStyle.Fill };
        _txtIsbn = new TextBox { Dock = DockStyle.Fill };
        _txtTitle = new TextBox { Dock = DockStyle.Fill };
        _txtAuthor = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ngăn cách nhiều tác giả bằng dấu phẩy (,)" };
        _cboCategory = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _txtPublisher = new TextBox { Dock = DockStyle.Fill };
        _txtYear = new TextBox { Dock = DockStyle.Fill };
        _txtLanguage = new TextBox { Dock = DockStyle.Fill };
        _numPages = new NumericUpDown { Minimum = 0, Maximum = 10000, Width = 120 };
        _txtDesc = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 85, ScrollBars = ScrollBars.Vertical };

        AddRow(fields, 0, "Mã đầu sách (*):", _txtCode);
        AddRow(fields, 1, "Mã ISBN:", _txtIsbn);
        AddRow(fields, 2, "Tên đầu sách (*):", _txtTitle);
        AddRow(fields, 3, "Tác giả (*):", _txtAuthor);
        AddRow(fields, 4, "Thể loại (*):", _cboCategory);
        AddRow(fields, 5, "Nhà xuất bản:", _txtPublisher);
        AddRow(fields, 6, "Năm xuất bản:", _txtYear);
        AddRow(fields, 7, "Ngôn ngữ:", _txtLanguage);
        AddRow(fields, 8, "Số trang:", _numPages);
        AddRow(fields, 9, "Mô tả tóm tắt:", _txtDesc, 92);

        var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 0, 0) };
        _picCover = new PictureBox
        {
            Dock = DockStyle.Top,
            Height = 250,
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        _txtCoverPath = new TextBox { Dock = DockStyle.Top, ReadOnly = true };
        var btnBrowse = new Button
        {
            Text = "Chọn ảnh bìa...",
            Dock = DockStyle.Top,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(232, 240, 250),
            ForeColor = Color.FromArgb(19, 98, 160),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnBrowse.Click += (_, _) =>
        {
            using var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _txtCoverPath.Text = ofd.FileName;
                _picCover.ImageLocation = ofd.FileName;
            }
        };

        pnlRight.Controls.Add(btnBrowse);
        pnlRight.Controls.Add(_txtCoverPath);
        pnlRight.Controls.Add(_picCover);

        main.Controls.Add(fields, 0, 0);
        main.Controls.Add(pnlRight, 1, 0);

        var pnlFooter = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            BackColor = Color.FromArgb(245, 247, 250),
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(14, 7, 14, 7)
        };
        var btnCancel = new Button { Text = "Hủy", Width = 90, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
        var btnSave = new Button
        {
            Text = "Lưu thông tin",
            Width = 120,
            Height = 32,
            BackColor = Color.FromArgb(19, 98, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        btnSave.Click += BtnSave_Click;

        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnSave);

        Controls.Add(main);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
    }

    private static void AddRow(TableLayoutPanel tbl, int row, string label, Control c, int height = 34)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        tbl.Controls.Add(new Label
        {
            Text = label,
            ForeColor = Color.FromArgb(80, 90, 105),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        }, 0, row);
        tbl.Controls.Add(c, 1, row);
    }

    private void BindData()
    {
        var cats = _service.GetAllCategories();
        _cboCategory.DataSource = cats;
        _cboCategory.DisplayMember = "CategoryName";
        _cboCategory.ValueMember = "Id";

        _txtCode.Text = _book.BookCode;
        _txtIsbn.Text = _book.Isbn;
        _txtTitle.Text = _book.Title;
        _txtAuthor.Text = _book.Author;
        if (_book.IdCategory > 0) _cboCategory.SelectedValue = _book.IdCategory;
        _txtPublisher.Text = _book.Publisher;
        _txtYear.Text = _book.PublishedYear > 0 ? _book.PublishedYear.ToString() : "";
        _txtLanguage.Text = _book.Language ?? "Tiếng Việt";
        _txtDesc.Text = _book.Description;
        _txtCoverPath.Text = _book.CoverImagePath;
        if (!string.IsNullOrWhiteSpace(_book.CoverImagePath) && File.Exists(_book.CoverImagePath))
            _picCover.ImageLocation = _book.CoverImagePath;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _book.BookCode = _txtCode.Text;
            _book.Isbn = _txtIsbn.Text;
            _book.Title = _txtTitle.Text;
            _book.Author = _txtAuthor.Text;
            _book.IdCategory = _cboCategory.SelectedValue is int id ? id : 0;
            _book.Publisher = _txtPublisher.Text;
            if (!int.TryParse(_txtYear.Text, out int year) || year < 1000 || year > 9999)
                throw new InvalidOperationException("Năm xuất bản phải là số 4 chữ số (1000–9999).");
            _book.PublishedYear = year;
            _book.Language = _txtLanguage.Text;
            _book.CoverImagePath = string.IsNullOrWhiteSpace(_txtCoverPath.Text) ? null : _txtCoverPath.Text;
            _book.Description = _txtDesc.Text;

            _service.SaveBook(_book);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}