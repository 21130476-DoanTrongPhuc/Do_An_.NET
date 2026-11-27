using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.Books;

public class CategoryManagementControl : UserControl
{
    // ====== Bảng màu đồng bộ hệ thống ======
    private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
    private static readonly Color BgGray     = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
    private static readonly Color Red        = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    // Glyph icons
    private const string GAdd     = "\uE710";
    private const string GSave    = "\uE74E";
    private const string GDelete  = "\uE74D";
    private const string GRefresh = "\uE72C";

    private readonly CatalogManagementService _service = new();
    private List<Categories> _categories = new();
    private Categories? _selectedCategory;

    private Label _lblSummary = null!;
    private TreeView _treeCategories = null!;
    private TextBox _txtSearch = null!;
    private TextBox _txtCode = null!;
    private TextBox _txtName = null!;
    private ComboBox _cboParent = null!;
    private NumericUpDown _numMaxDays = null!;
    private NumericUpDown _numMaxRenewals = null!;
    private ComboBox _cboStatus = null!;
    private Label _lblBookStats = null!;

    public CategoryManagementControl()
    {
        BackColor = BgGray;
        Font = new Font("Segoe UI", 9F);
        BuildUi();
        LoadCategories();
    }

    // ============================================================
    //  DỰNG GIAO DIỆN ĐỒNG BỘ
    // ============================================================
    private void BuildUi()
    {
        SuspendLayout();

        // 1. Header đồng bộ (Top)
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = BgGray };
        pnlTop.Controls.Add(BuildHeader());

        // 2. Khu vực chính: Trái (Cây danh mục) - Phải (Form thông tin chi tiết)
        var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };

        var pnlLeftHost = new Panel { Dock = DockStyle.Left, Width = 380, Padding = new Padding(0, 0, 10, 0) };
        pnlLeftHost.Controls.Add(BuildTreeGroup());

        var grpForm = BuildDetailGroup();

        pnlMain.Controls.Add(grpForm);
        pnlMain.Controls.Add(pnlLeftHost);

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
            Text = "QUẢN LÝ THỂ LOẠI SÁCH",
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(14, 8)
        };
        pnl.Controls.Add(lblTitle);

        _lblSummary = new Label
        {
            Text = "Đang tải danh mục thể loại...",
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(16, 32)
        };
        pnl.Controls.Add(_lblSummary);

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
        btnRefresh.Click += (_, _) => LoadCategories();

        var btnAdd = CreateActionButton("Thêm thể loại", GAdd, Primary, Color.White);
        btnAdd.Click += BtnAdd_Click;

        flowActions.Controls.AddRange(new Control[] { btnRefresh, btnAdd });
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
    
    private void BtnAdd_Click(object? sender, EventArgs e) => ClearForm();

    private GroupBox BuildTreeGroup()
    {
        var grpTree = new GroupBox
        {
            Text = "Cây danh mục && thể loại sách",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 10)
        };

        var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(0, 0, 0, 6) };
        _txtSearch = new TextBox
        {
            PlaceholderText = "Tìm kiếm thể loại theo tên hoặc mã...",
            Dock = DockStyle.Fill
        };
        _txtSearch.TextChanged += (_, _) => RenderTree(_txtSearch.Text);
        pnlSearch.Controls.Add(_txtSearch);

        _treeCategories = new TreeView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F),
            BorderStyle = BorderStyle.FixedSingle,
            HideSelection = false,
            FullRowSelect = true,
            ItemHeight = 28
        };
        _treeCategories.AfterSelect += TreeCategories_AfterSelect;

        grpTree.Controls.Add(_treeCategories);
        grpTree.Controls.Add(pnlSearch);
        return grpTree;
    }

    private GroupBox BuildDetailGroup()
    {
        var grpForm = new GroupBox
        {
            Text = "Thông tin chi tiết && Quy định mượn của thể loại",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14, 10, 14, 12)
        };

        // Thẻ thống kê đầu sách nổi bật phía trên (giống card preview bên phải của các màn hình khác)
        var pnlStatCard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = SoftBlue,
            Padding = new Padding(12, 8, 12, 8)
        };
        pnlStatCard.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlStatCard.Width - 1, pnlStatCard.Height - 1);

        _lblBookStats = new Label
        {
            Text = "Chưa chọn thể loại — Nhấn 'Thêm thể loại mới' hoặc chọn trên cây bên trái",
            ForeColor = Primary,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        pnlStatCard.Controls.Add(_lblBookStats);

       var tbl = new TableLayoutPanel
       {
           Dock = DockStyle.Top,
           Height = 256, // 6 hàng * 40px + 14px padding trên + 2px viền (xóa khoảng trống dư ở hàng cuối)
           ColumnCount = 2,
           RowCount = 6,
           Padding = new Padding(0, 14, 0, 0)
       };
       tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165));
       tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

       _txtCode = new TextBox { Width = 280 };
       _txtName = new TextBox { Width = 380 };
       _cboParent = new ComboBox { Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
       _numMaxDays = new NumericUpDown { Minimum = 1, Maximum = 90, Value = 14, Width = 140 };
       _numMaxRenewals = new NumericUpDown { Minimum = 0, Maximum = 10, Value = 1, Width = 140 };
       _cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
       _cboStatus.Items.AddRange(new object[] { BookCategoryStatus.Active, BookCategoryStatus.Inactive });
       _cboStatus.SelectedIndex = 0;

        AddFormRow(tbl, 0, "Mã thể loại (*):", _txtCode);
        AddFormRow(tbl, 1, "Tên thể loại (*):", _txtName);
        AddFormRow(tbl, 2, "Thuộc danh mục cha:", _cboParent);
        AddFormRow(tbl, 3, "Số ngày mượn tối đa:", _numMaxDays);
        AddFormRow(tbl, 4, "Số lần gia hạn tối đa:", _numMaxRenewals);
        AddFormRow(tbl, 5, "Trạng thái lưu hành:", _cboStatus);

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            WrapContents = false,
            Padding = new Padding(165, 10, 0, 0)
        };

        var btnSave = CreateActionButton("Lưu thể loại", GSave, Primary, Color.White);
        btnSave.Margin = new Padding(0, 0, 10, 0);
        btnSave.Click += BtnSave_Click;

        var btnDelete = CreateActionButton("Xóa thể loại", GDelete, Color.White, Red);
        btnDelete.Margin = new Padding(0);
        btnDelete.Click += BtnDelete_Click;

        pnlButtons.Controls.AddRange(new Control[] { btnSave, btnDelete });

        grpForm.Controls.Add(pnlButtons);
        grpForm.Controls.Add(tbl);
        grpForm.Controls.Add(pnlStatCard);

        return grpForm;
    }

    private static void AddFormRow(TableLayoutPanel tbl, int row, string label, Control ctrl)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tbl.Controls.Add(new Label
        {
            Text = label,
            ForeColor = TextGray,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        }, 0, row);

        ctrl.Anchor = AnchorStyles.Left;
        ctrl.Margin = new Padding(0, 6, 0, 6);
        tbl.Controls.Add(ctrl, 1, row);
    }

    private static Button CreateActionButton(string text, string glyph, Color back, Color fore, int? fixedWidth = null)
    {
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
            Text = " " + text.Trim(),
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = fore,
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

        if (fixedWidth.HasValue)
        {
            btn.AutoSize = false;
            btn.Size = new Size(fixedWidth.Value, 34);
        }
        else
        {
            btn.AutoSize = true;
            btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btn.MinimumSize = new Size(0, 34);
            btn.MaximumSize = new Size(0, 34);
        }

        btn.FlatAppearance.BorderColor = BorderGray;
        if (back != Color.White && back != SoftBlue)
            btn.FlatAppearance.BorderSize = 0;

        return btn;
    }

    // ============================================================
    //  LOAD DATA & NGHIỆP VỤ
    // ============================================================
    public void LoadCategories()
    {
        _categories = _service.GetAllCategories();
        int rootCount = _categories.Count(c => c.IdParent == null);
        int activeCount = _categories.Count(c => c.Status == BookCategoryStatus.Active);

        _lblSummary.Text =
            $"Tổng số thể loại: {_categories.Count:N0}  ·  Danh mục gốc: {rootCount:N0}  ·  Đang hoạt động: {activeCount:N0}";

        BindParentComboBox();
        RenderTree(_txtSearch.Text);
    }

    private void BindParentComboBox()
    {
        var items = new List<object> { new { Id = (int?)null, Display = "-- Danh mục gốc (Không có cha) --" } };
        foreach (var c in _categories)
        {
            if (_selectedCategory == null || c.Id != _selectedCategory.Id)
                items.Add(new { Id = (int?)c.Id, Display = $"{c.CategoryCode} - {c.CategoryName}" });
        }
        _cboParent.DataSource = items;
        _cboParent.DisplayMember = "Display";
        _cboParent.ValueMember = "Id";
    }

    private void RenderTree(string? filter = null)
    {
        _treeCategories.Nodes.Clear();
        string kw = (filter ?? string.Empty).Trim().ToLower();

        void AddNodes(int? parentId, TreeNodeCollection nodes)
        {
            var children = _categories.Where(c => c.IdParent == parentId).ToList();
            foreach (var cat in children)
            {
                int totalBooks = _service.CountBooksInCategoryTree(cat.Id, _categories);
                var node = new TreeNode($"{cat.CategoryName} [{cat.CategoryCode}] ({totalBooks} đầu sách)")
                {
                    Tag = cat
                };
                AddNodes(cat.Id, node.Nodes);

                bool match = string.IsNullOrEmpty(kw) ||
                             cat.CategoryName.ToLower().Contains(kw) ||
                             cat.CategoryCode.ToLower().Contains(kw) ||
                             node.Nodes.Count > 0;

                if (match)
                    nodes.Add(node);
            }
        }

        AddNodes(null, _treeCategories.Nodes);
        _treeCategories.ExpandAll();
    }

    private void TreeCategories_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is not Categories cat) return;
        _selectedCategory = cat;
        BindParentComboBox();

        _txtCode.Text = cat.CategoryCode;
        _txtName.Text = cat.CategoryName;
        _cboParent.SelectedValue = (object?)cat.IdParent ?? DBNull.Value;
        if (cat.IdParent == null) _cboParent.SelectedIndex = 0;
        _numMaxDays.Value = Math.Clamp(cat.MaxLoanDays, 1, 90);
        _numMaxRenewals.Value = Math.Clamp(cat.MaxRenewals, 0, 10);
        _cboStatus.SelectedItem = cat.Status;

        int directBooks = cat.Books.Count;
        int totalTreeBooks = _service.CountBooksInCategoryTree(cat.Id, _categories);
        _lblBookStats.Text = $"Thể loại [{cat.CategoryCode}]: Trực tiếp {directBooks} đầu sách  ·  Tổng cả nhánh con: {totalTreeBooks} đầu sách";
    }

    private void ClearForm()
    {
        _selectedCategory = null;
        BindParentComboBox();
        _txtCode.Clear();
        _txtName.Clear();
        _cboParent.SelectedIndex = 0;
        _numMaxDays.Value = 14;
        _numMaxRenewals.Value = 1;
        _cboStatus.SelectedIndex = 0;
        _lblBookStats.Text = "Đang tạo mới thể loại sách";
        _txtCode.Focus();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            int? parentId = _cboParent.SelectedValue is int id ? id : null;
            var model = new Categories
            {
                Id = _selectedCategory?.Id ?? 0,
                CategoryCode = _txtCode.Text,
                CategoryName = _txtName.Text,
                IdParent = parentId,
                MaxLoanDays = (int)_numMaxDays.Value,
                MaxRenewals = (int)_numMaxRenewals.Value,
                Status = _cboStatus.SelectedItem is BookCategoryStatus status
                    ? status
                    : BookCategoryStatus.Active,
            };

            _service.SaveCategory(model);
            MessageBox.Show("Lưu thể loại sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadCategories();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_selectedCategory == null)
        {
            MessageBox.Show("Vui lòng chọn thể loại cần xóa trên cây danh mục.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show($"Bạn có chắc muốn xóa thể loại '{_selectedCategory.CategoryName}'?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            _service.DeleteCategory(_selectedCategory.Id);
            MessageBox.Show("Đã xóa thể loại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
            LoadCategories();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}