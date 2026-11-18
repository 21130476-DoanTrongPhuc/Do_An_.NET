using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.Books;

public class BookCatalogControl : UserControl
{
    private readonly CatalogManagementService _service = new();
    private List<Book> _books = new();

    private TextBox _txtSearch = null!;
    private ComboBox _cboCategoryFilter = null!;
    private CheckBox _chkShowDiscontinued = null!;
    private DataGridView _dgvBooks = null!;
    private Label _lblCount = null!;

    public event Action<Book>? ViewCopiesRequested;

    public BookCatalogControl()
    {
        BuildUi();
        LoadFilters();
        LoadData();
    }

    private void BuildUi()
    {
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(10);

        // Thanh công cụ nghiệp vụ 1.1
        var pnlTop = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Color.White,
            Padding = new Padding(8, 6, 8, 6)
        };

        var btnAdd = MakeBtn("Thêm đầu sách", Color.FromArgb(19, 98, 160), Color.White);
        var btnEdit = MakeBtn("Chỉnh sửa", Color.FromArgb(245, 247, 250), Color.Black);
        var btnToggleStatus = MakeBtn("Ngừng / Mở lưu hành", Color.FromArgb(255, 244, 229), Color.FromArgb(180, 100, 0));
        var btnViewCopies = MakeBtn("Xem danh sách bản sách", Color.FromArgb(232, 240, 250), Color.FromArgb(19, 98, 160));

        btnAdd.Click += (_, _) => OpenBookDialog(null);
        btnEdit.Click += (_, _) =>
        {
            if (GetSelectedBook() is { } b) OpenBookDialog(b);
        };
        btnToggleStatus.Click += BtnToggleStatus_Click;
        btnViewCopies.Click += (_, _) =>
        {
            if (GetSelectedBook() is { } b) ViewCopiesRequested?.Invoke(b);
        };

        pnlTop.Controls.AddRange([btnAdd, btnEdit, btnToggleStatus, btnViewCopies]);

        // Thanh tìm kiếm theo tên, tác giả, ISBN, thể loại
        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            Padding = new Padding(0, 8, 0, 4)
        };

        _txtSearch = new TextBox { Width = 320, PlaceholderText = "Tìm theo tên sách, tác giả, ISBN, mã sách..." };
        _txtSearch.TextChanged += (_, _) => LoadData();

        _cboCategoryFilter = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboCategoryFilter.SelectedIndexChanged += (_, _) => LoadData();

        _chkShowDiscontinued = new CheckBox { Text = "Hiển thị cả sách ngừng lưu hành", Checked = true, AutoSize = true, Padding = new Padding(10, 4, 0, 0) };
        _chkShowDiscontinued.CheckedChanged += (_, _) => LoadData();

        pnlFilter.Controls.Add(new Label { Text = "Từ khóa:", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        pnlFilter.Controls.Add(_txtSearch);
        pnlFilter.Controls.Add(new Label { Text = "Thể loại:", AutoSize = true, Padding = new Padding(12, 6, 4, 0) });
        pnlFilter.Controls.Add(_cboCategoryFilter);
        pnlFilter.Controls.Add(_chkShowDiscontinued);

        _dgvBooks = new DataGridView
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
        _dgvBooks.Columns.Add("colCode", "Mã sách");
        _dgvBooks.Columns.Add("colIsbn", "ISBN");
        _dgvBooks.Columns.Add("colTitle", "Tên đầu sách");
        _dgvBooks.Columns.Add("colAuthor", "Tác giả");
        _dgvBooks.Columns.Add("colCategory", "Thể loại");
        _dgvBooks.Columns.Add("colPub", "NXB / Năm");
        _dgvBooks.Columns.Add("colCopies", "Bản sách (Sẵn/Tổng)");
        _dgvBooks.Columns.Add("colStatus", "Trạng thái");
        _dgvBooks.CellDoubleClick += (_, _) =>
        {
            if (GetSelectedBook() is { } b) OpenBookDialog(b);
        };

        _lblCount = new Label { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleRight };

        Controls.Add(_dgvBooks);
        Controls.Add(_lblCount);
        Controls.Add(pnlFilter);
        Controls.Add(pnlTop);
    }

    private static Button MakeBtn(string text, Color back, Color fore)
    {
        var btn = new Button
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
        return btn;
    }

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

        _dgvBooks.Rows.Clear();
        foreach (var b in _books)
        {
            int total = b.Copies.Count;
            int avail = b.Copies.Count(c => c.CirculationStatus == "Available");
            string statusText = b.Status == "Active" ? "Đang lưu hành" : "Ngừng lưu hành";

            int idx = _dgvBooks.Rows.Add(
                b.BookCode,
                b.ISBN ?? "-",
                b.Title,
                b.Author,
                b.Category?.CategoryName ?? "-",
                $"{b.Publisher ?? "-"} ({b.PublishedYear ?? "-"})",
                $"{avail} / {total}",
                statusText
            );
            _dgvBooks.Rows[idx].Tag = b;
            if (b.Status != "Active")
                _dgvBooks.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
        }
        _lblCount.Text = $"Tổng số đầu sách: {_books.Count}";
    }

    private Book? GetSelectedBook()
    {
        if (_dgvBooks.CurrentRow?.Tag is Book b) return b;
        MessageBox.Show("Vui lòng chọn một đầu sách trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void OpenBookDialog(Book? book)
    {
        using var dlg = new BookEditDialog(book);
        if (dlg.ShowDialog() == DialogResult.OK)
            LoadData();
    }

    private void BtnToggleStatus_Click(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        try
        {
            _service.ToggleBookDiscontinued(book.Id);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể đổi trạng thái", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
        Size = new Size(760, 560);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi();
        BindData();
    }

    private void BuildUi()
    {
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

        // Cột phải: Ảnh bìa
        var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 0, 0) };
        _picCover = new PictureBox
        {
            Dock = DockStyle.Top,
            Height = 250,
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.WhiteSmoke
        };
        _txtCoverPath = new TextBox { Dock = DockStyle.Top, ReadOnly = true };
        var btnBrowse = new Button { Text = "Chọn ảnh bìa...", Dock = DockStyle.Top, Height = 32 };
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

        var pnlFooter = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(14, 6, 14, 6) };
        var btnCancel = new Button { Text = "Hủy", Width = 90, Height = 32 };
        var btnSave = new Button { Text = "Lưu thông tin", Width = 120, Height = 32, BackColor = Color.FromArgb(19, 98, 160), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        btnSave.Click += BtnSave_Click;

        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnSave);

        Controls.Add(main);
        Controls.Add(pnlFooter);
    }

    private static void AddRow(TableLayoutPanel tbl, int row, string label, Control c, int height = 34)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
        tbl.Controls.Add(c, 1, row);
    }

    private void BindData()
    {
        var cats = _service.GetAllCategories();
        _cboCategory.DataSource = cats;
        _cboCategory.DisplayMember = "CategoryName";
        _cboCategory.ValueMember = "Id";

        _txtCode.Text = _book.BookCode;
        _txtIsbn.Text = _book.ISBN;
        _txtTitle.Text = _book.Title;
        _txtAuthor.Text = _book.Author;
        if (_book.IdCategory > 0) _cboCategory.SelectedValue = _book.IdCategory;
        _txtPublisher.Text = _book.Publisher;
        _txtYear.Text = _book.PublishedYear;
        _txtLanguage.Text = _book.Language ?? "Tiếng Việt";
        _numPages.Value = _book.PageCount ?? 0;
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
            _book.ISBN = _txtIsbn.Text;
            _book.Title = _txtTitle.Text;
            _book.Author = _txtAuthor.Text;
            _book.IdCategory = _cboCategory.SelectedValue is int id ? id : 0;
            _book.Publisher = _txtPublisher.Text;
            _book.PublishedYear = _txtYear.Text;
            _book.Language = _txtLanguage.Text;
            _book.PageCount = _numPages.Value > 0 ? (int)_numPages.Value : null;
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