using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Helpers;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Forms.Books.ReaderPortal;

public class BookSearchControl : UserControl
{
    private static readonly Color Primary = Color.FromArgb(19, 98, 160);
    private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);

    private TreeView _treeCategories = null!;
    private TextBox _txtKeyword = null!;
    private CheckBox _chkAvailableOnly = null!;
    private DataGridView _dgvBooks = null!;
    private Label _lblCount = null!;
    private int? _selectedCategoryId;

    public event Action<int>? OnOpenBookDetail;
    public event Action? OnDataChanged;

    public BookSearchControl(string initialKeyword = "")
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(240, 242, 245);
        BuildUi();
        _txtKeyword.Text = initialKeyword;
        LoadCategoriesTree();
        LoadBooks();
    }

    private void BuildUi()
    {
        var leftHost = new Panel { Dock = DockStyle.Left, Width = 260, Padding = new Padding(0, 0, 12, 0) };
        var grpCat = new GroupBox
        {
            Text = "Danh mục thể loại",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.White,
            Padding = new Padding(8)
        };
        _treeCategories = new TreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9.5F),
            HideSelection = false,
            FullRowSelect = true
        };
        _treeCategories.AfterSelect += (_, e) =>
        {
            _selectedCategoryId = e.Node?.Tag as int?;
            LoadBooks();
        };
        grpCat.Controls.Add(_treeCategories);
        leftHost.Controls.Add(grpCat);

        var rightHost = new Panel { Dock = DockStyle.Fill };

        var pnlFilter = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        pnlFilter.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlFilter.Width - 1, pnlFilter.Height - 1);

        _txtKeyword = new TextBox
        {
            Location = new Point(14, 18),
            Size = new Size(340, 27),
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = "Nhập tên sách, tác giả, mã sách hoặc ISBN..."
        };
        _txtKeyword.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadBooks();
            }
        };

        _chkAvailableOnly = new CheckBox
        {
            Text = "Chỉ hiện sách còn bản để mượn",
            AutoSize = true,
            Font = new Font("Segoe UI", 9F),
            Location = new Point(370, 21)
        };
        _chkAvailableOnly.CheckedChanged += (_, _) => LoadBooks();

        var btnSearch = new Button
        {
            Text = "Tìm kiếm",
            BackColor = Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(92, 32),
            Location = new Point(0, 16),
            Cursor = Cursors.Hand
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += (_, _) => LoadBooks();

        var btnReset = new Button
        {
            Text = "Đặt lại",
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F),
            Size = new Size(72, 32),
            Location = new Point(0, 16),
            Cursor = Cursors.Hand
        };
        btnReset.FlatAppearance.BorderColor = BorderGray;
        btnReset.Click += (_, _) =>
        {
            _txtKeyword.Clear();
            _chkAvailableOnly.Checked = false;
            _selectedCategoryId = null;
            if (_treeCategories.Nodes.Count > 0) _treeCategories.SelectedNode = _treeCategories.Nodes[0];
            LoadBooks();
        };

        _lblCount = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Primary,
            Location = new Point(0, 22)
        };

        pnlFilter.Controls.AddRange(new Control[] { _txtKeyword, _chkAvailableOnly, btnSearch, btnReset, _lblCount });
        pnlFilter.Resize += (_, _) => LayoutFilterBar(pnlFilter, btnSearch, btnReset);
        _lblCount.TextChanged += (_, _) => LayoutFilterBar(pnlFilter, btnSearch, btnReset);
        LayoutFilterBar(pnlFilter, btnSearch, btnReset);

        var pnlGridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
        _dgvBooks = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 38 },
            ColumnHeadersHeight = 36,
            EnableHeadersVisualStyles = false,
            Font = new Font("Segoe UI", 9F)
        };
        _dgvBooks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        _dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        _dgvBooks.Columns.Add("BookId", "ID");
        _dgvBooks.Columns["BookId"]!.Visible = false;
        _dgvBooks.Columns.Add("BookCode", "Mã đầu sách");
        _dgvBooks.Columns.Add("Title", "Tên sách");
        _dgvBooks.Columns.Add("Author", "Tác giả");
        _dgvBooks.Columns.Add("CategoryName", "Thể loại");
        _dgvBooks.Columns.Add("Isbn", "ISBN");
        _dgvBooks.Columns.Add("CopiesInfo", "Bản sao khả dụng");
        _dgvBooks.Columns.Add("Status", "Trạng thái");

        var colDetail = new DataGridViewButtonColumn
        {
            Name = "Detail",
            HeaderText = "Chi tiết",
            Text = "Xem chi tiết",
            UseColumnTextForButtonValue = true,
            FillWeight = 65
        };
        var colFav = new DataGridViewButtonColumn
        {
            Name = "Favorite",
            HeaderText = "Yêu thích",
            UseColumnTextForButtonValue = false,
            FillWeight = 55
        };
        _dgvBooks.Columns.Add(colDetail);
        _dgvBooks.Columns.Add(colFav);

        _dgvBooks.Columns["BookCode"]!.FillWeight = 55;
        _dgvBooks.Columns["Title"]!.FillWeight = 130;
        _dgvBooks.Columns["Author"]!.FillWeight = 95;
        _dgvBooks.Columns["CategoryName"]!.FillWeight = 80;
        _dgvBooks.Columns["Isbn"]!.FillWeight = 75;
        _dgvBooks.Columns["CopiesInfo"]!.FillWeight = 65;
        _dgvBooks.Columns["Status"]!.FillWeight = 65;

        _dgvBooks.CellClick += DgvBooks_CellClick;
        _dgvBooks.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && int.TryParse(_dgvBooks.Rows[e.RowIndex].Cells["BookId"].Value?.ToString(), out int id))
                OnOpenBookDetail?.Invoke(id);
        };

        pnlGridHost.Controls.Add(_dgvBooks);
        rightHost.Controls.Add(pnlGridHost);
        rightHost.Controls.Add(pnlFilter);

        Controls.Add(rightHost);
        Controls.Add(leftHost);
    }

    private void LayoutFilterBar(Panel pnlFilter, Button btnSearch, Button btnReset)
    {
        const int margin = 14;
        const int gap = 8;

        int right = pnlFilter.ClientSize.Width - margin;
        _lblCount.Left = right - _lblCount.Width;
        btnReset.Left = _lblCount.Left - gap - btnReset.Width;
        btnSearch.Left = btnReset.Left - gap - btnSearch.Width;
        _chkAvailableOnly.Left = btnSearch.Left - gap - _chkAvailableOnly.Width;
        _txtKeyword.Width = Math.Max(140, _chkAvailableOnly.Left - gap - _txtKeyword.Left);
    }

    private void LoadCategoriesTree()
    {
        _treeCategories.Nodes.Clear();
        var rootNode = new TreeNode("Tất cả thể loại") { Tag = null };

        using var context = new LibraryDbContext();
        var categories = context.BookCategories
            .Where(c => c.Status == BookCategoryStatus.Active)
            .OrderBy(c => c.CategoryName)
            .ToList();

        void AddChildren(TreeNode parentNode, int? parentId)
        {
            foreach (var cat in categories.Where(c => c.IdParent == parentId))
            {
                var node = new TreeNode(cat.CategoryName) { Tag = cat.Id };
                AddChildren(node, cat.Id);
                parentNode.Nodes.Add(node);
            }
        }

        AddChildren(rootNode, null);
        _treeCategories.Nodes.Add(rootNode);
        rootNode.ExpandAll();
        _treeCategories.SelectedNode = rootNode;
    }

    public void ApplyKeyword(string keyword)
    {
        _txtKeyword.Text = keyword;
        LoadBooks();
    }

    public void LoadBooks()
    {
        using var context = new LibraryDbContext();
        int readerId = Session.CurrentReaderId;

        var favBookIds = readerId > 0
            ? context.FavoriteBooks.Where(f => f.ReaderId == readerId).Select(f => f.BookId).ToHashSet()
            : new HashSet<int>();

        var query = context.Books
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .AsQueryable();

        if (_selectedCategoryId.HasValue)
        {
            int catId = _selectedCategoryId.Value;
            var allCats = context.BookCategories.ToList();
            var targetIds = new HashSet<int> { catId };

            void CollectSubCategories(int parentId)
            {
                foreach (var child in allCats.Where(c => c.IdParent == parentId))
                {
                    if (targetIds.Add(child.Id))
                        CollectSubCategories(child.Id);
                }
            }
            CollectSubCategories(catId);
            query = query.Where(b => targetIds.Contains(b.IdCategory));
        }

        string kw = _txtKeyword.Text.Trim().ToLower();
        if (!string.IsNullOrEmpty(kw))
        {
            query = query.Where(b =>
                b.Title.ToLower().Contains(kw) ||
                b.Author.ToLower().Contains(kw) ||
                b.BookCode.ToLower().Contains(kw) ||
                (b.Isbn != null && b.Isbn.ToLower().Contains(kw)));
        }

        var books = query.OrderBy(b => b.Title).ToList();
        if (_chkAvailableOnly.Checked)
        {
            books = books.Where(b => b.Copies.Any(c => c.CirculationStatus == CirculationStatus.Available)).ToList();
        }

        _dgvBooks.Rows.Clear();
        foreach (var b in books)
        {
            int totalCopies = b.Copies.Count;
            int availCopies = b.Copies.Count(c => c.CirculationStatus == CirculationStatus.Available);
            bool isFav = favBookIds.Contains(b.Id);

            int idx = _dgvBooks.Rows.Add(
                b.Id,
                b.BookCode,
                b.Title,
                b.Author,
                b.Category?.CategoryName ?? "",
                b.Isbn ?? "—",
                $"{availCopies} / {totalCopies} cuốn",
                availCopies > 0 ? "Còn sách" : "Đã hết",
                "Xem chi tiết",
                isFav ? "♥ Đã thích" : "♡ Thích"
            );

            var row = _dgvBooks.Rows[idx];
            row.Cells["Status"].Style.ForeColor = availCopies > 0
                ? Color.FromArgb(34, 160, 90)
                : Color.FromArgb(200, 50, 50);
            row.Cells["Status"].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        _lblCount.Text = $"Tìm thấy {books.Count} đầu sách";
    }

    private void DgvBooks_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (!int.TryParse(_dgvBooks.Rows[e.RowIndex].Cells["BookId"].Value?.ToString(), out int bookId)) return;

        string colName = _dgvBooks.Columns[e.ColumnIndex].Name;
        if (colName == "Detail")
        {
            OnOpenBookDetail?.Invoke(bookId);
        }
        else if (colName == "Favorite" && Session.CurrentReaderId > 0)
        {
            using var context = new LibraryDbContext();
            var existing = context.FavoriteBooks
                .FirstOrDefault(f => f.ReaderId == Session.CurrentReaderId && f.BookId == bookId);

            if (existing != null)
                context.FavoriteBooks.Remove(existing);
            else
                context.FavoriteBooks.Add(new FavoriteBook
                {
                    ReaderId = Session.CurrentReaderId,
                    BookId = bookId,
                    AddedDate = DateTime.Now
                });

            context.SaveChanges();
            LoadBooks();
            OnDataChanged?.Invoke();
        }
    }
}