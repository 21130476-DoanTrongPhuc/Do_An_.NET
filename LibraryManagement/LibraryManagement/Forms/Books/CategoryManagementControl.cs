using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.Books;

public class CategoryManagementControl : UserControl
{
    private readonly CatalogManagementService _service = new();
    private List<Categories> _categories = new();
    private Categories? _selectedCategory;

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
        BuildUi();
        LoadCategories();
    }

    private void BuildUi()
    {
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(10);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 420,
            BackColor = BackColor
        };

        // --- Panel trái: ViewTree thể loại ---
        var grpTree = new GroupBox
        {
            Text = "Cây danh mục & thể loại sách",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Padding = new Padding(10)
        };

        var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 38 };
        _txtSearch = new TextBox { PlaceholderText = "Tìm kiếm thể loại theo tên hoặc mã...", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F) };
        _txtSearch.TextChanged += (_, _) => RenderTree(_txtSearch.Text);
        pnlSearch.Controls.Add(_txtSearch);

        _treeCategories = new TreeView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5F),
            HideSelection = false,
            FullRowSelect = true,
            ItemHeight = 26
        };
        _treeCategories.AfterSelect += TreeCategories_AfterSelect;

        grpTree.Controls.Add(_treeCategories);
        grpTree.Controls.Add(pnlSearch);

        // --- Panel phải: Form chi tiết & thống kê ---
        var grpForm = new GroupBox
        {
            Text = "Thông tin chi tiết thể loại",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Padding = new Padding(16)
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 330,
            ColumnCount = 2,
            Font = new Font("Segoe UI", 9.5F)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _txtCode = new TextBox { Dock = DockStyle.Fill };
        _txtName = new TextBox { Dock = DockStyle.Fill };
        _cboParent = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _numMaxDays = new NumericUpDown { Minimum = 1, Maximum = 90, Value = 14, Width = 120 };
        _numMaxRenewals = new NumericUpDown { Minimum = 0, Maximum = 10, Value = 1, Width = 120 };
        _cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
        _cboStatus.Items.AddRange(["Active", "Inactive"]);
        _cboStatus.SelectedIndex = 0;

        _lblBookStats = new Label
        {
            Text = "Chưa chọn thể loại",
            ForeColor = Color.FromArgb(19, 98, 160),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            AutoSize = true
        };

        AddFormRow(tbl, 0, "Mã thể loại (*):", _txtCode);
        AddFormRow(tbl, 1, "Tên thể loại (*):", _txtName);
        AddFormRow(tbl, 2, "Thuộc danh mục cha:", _cboParent);
        AddFormRow(tbl, 3, "Số ngày mượn tối đa:", _numMaxDays);
        AddFormRow(tbl, 4, "Số lần gia hạn tối đa:", _numMaxRenewals);
        AddFormRow(tbl, 5, "Trạng thái:", _cboStatus);
        AddFormRow(tbl, 6, "Thống kê đầu sách:", _lblBookStats);

        var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 8, 0, 0) };
        var btnNew = CreateBtn("Thêm mới", Color.FromArgb(235, 245, 255), Color.FromArgb(19, 98, 160));
        var btnSave = CreateBtn("Lưu thể loại", Color.FromArgb(19, 98, 160), Color.White);
        var btnDelete = CreateBtn("Xóa thể loại", Color.FromArgb(253, 234, 234), Color.FromArgb(200, 40, 40));

        btnNew.Click += (_, _) => ClearForm();
        btnSave.Click += BtnSave_Click;
        btnDelete.Click += BtnDelete_Click;

        pnlButtons.Controls.AddRange([btnNew, btnSave, btnDelete]);

        grpForm.Controls.Add(pnlButtons);
        grpForm.Controls.Add(tbl);

        split.Panel1.Controls.Add(grpTree);
        split.Panel2.Controls.Add(grpForm);
        Controls.Add(split);
    }

    private static void AddFormRow(TableLayoutPanel tbl, int row, string label, Control ctrl)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
        tbl.Controls.Add(ctrl, 1, row);
    }

    private static Button CreateBtn(string text, Color back, Color fore)
    {
        var b = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(125, 34),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = fore;
        return b;
    }

    public void LoadCategories()
    {
        _categories = _service.GetAllCategories();
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
        _lblBookStats.Text = $"Trực tiếp: {directBooks} đầu sách | Tổng cả nhánh con: {totalTreeBooks} đầu sách";
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
        _lblBookStats.Text = "Đang tạo mới thể loại";
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