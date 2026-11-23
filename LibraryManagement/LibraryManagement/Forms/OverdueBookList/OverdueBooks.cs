using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Services;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace LibraryManagement.Forms.OverdueBooks;

public partial class OverdueBooks : UserControl
{
    // =========================================================
    // CONFIGURATION
    // =========================================================
    private const int PageSize = 20;
    private const string IconFont = "Segoe MDL2 Assets";

    private const string GSearch  = "\uE721";
    private const string GExport  = "\uEDE1";
    private const string GPrint   = "\uE749";
    private const string GRefresh = "\uE72C";
    private const string GWarn    = "\uE7BA";
    private const string GMore    = "\uE712";
    private const string GClose   = "\uE711";
    private const string GMail    = "\uE715";
    private const string GSend    = "\uE724";
    private const string GReturn  = "\uE7B8";
    private const string GHistory = "\uE81C";
    private const string GEdit    = "\uE70F";

    // ====== Màu sắc (đồng bộ với FineManagement) ======
    private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
    private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
    private static readonly Color Green      = Color.FromArgb(34, 160, 90);
    private static readonly Color Red        = Color.FromArgb(200, 40, 40);
    private static readonly Color Gray       = Color.FromArgb(140, 146, 156);
    private static readonly Color BgPage     = Color.FromArgb(240, 242, 245);

    // ====== Service ======
    private readonly ManageOverdueServiceImpl _service;

    // ====== State ======
    private List<ManageOverdueBookInfo> _allBooks = new();
    private List<ManageOverdueBookInfo> _filteredBooks = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private bool _isLoading;

    private HashSet<string> _selectedIds = new(StringComparer.OrdinalIgnoreCase);
    private ManageOverdueBookInfo? _drawerBook;
    
    private Rectangle _headerCheckboxRect = Rectangle.Empty;
    private bool _headerCheckboxHover = false;
    private bool _allSelectedOnPage = false;

    // ====== Sự kiện host ======
    public event EventHandler? CloseRequested;
    public event EventHandler? ReturnBookRequested;

    // ====== Controls ======
    private Label lblKpiBooks = null!;
    private Label lblKpiReaders = null!;
    private Label lblKpiFine = null!;
    private Label lblDate = null!;

    private TextBox txtSearch = null!;
    private ComboBox cboDaysFilter = null!;
    private ComboBox cboReminderStatus = null!;
    private ComboBox cboChannel = null!;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private CheckBox chkUseDueRange = null!;

    private DataGridView dgvOverdue = null!;
    private Button btnNotify = null!;
    private Button btnPrevPage = null!;
    private Button btnNextPage = null!;
    private Label lblPageInfo = null!;

    // Drawer
    private Panel pnlDrawer = null!;
    private Label lblDrawerTitle = null!;
    private Label lblDrawerSub = null!;
    private TabControl tabDrawer = null!;
    private FlowLayoutPanel flowHistory = null!;

    // =========================================================
    // CONSTRUCTOR
    // =========================================================
    public OverdueBooks(ManageOverdueServiceImpl service)
    {
        _service = service;
        BuildInterface();
        _ = LoadDataAsync();
    }

    // =========================================================
    // BUILD INTERFACE
    // =========================================================
    private void BuildInterface()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        BackColor = BgPage;
        Font = new Font("Segoe UI", 9F);

        // Thứ tự add: Fill -> Top dưới -> Top trên -> Drawer (Right)
        Controls.Add(BuildTableArea());
        Controls.Add(BuildPager());
        Controls.Add(BuildToolbar());
        Controls.Add(BuildKpiArea());
        Controls.Add(BuildHeader());
        Controls.Add(BuildDrawer());

        ResumeLayout(true);
    }

    // ---------------------------------------------------------
    // HEADER
    // ---------------------------------------------------------
    private Panel BuildHeader()
    {
        var pnl = new Panel
        {
            Dock = DockStyle.Top,
            Height = 62,
            BackColor = SoftBlue,
            Padding = new Padding(14, 8, 14, 8)
        };
        pnl.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderGray);
            e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
        };

        pnl.Controls.Add(new Label
        {
            Text = "DANH SÁCH SÁCH QUÁ HẠN",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(14, 8)
        });

        lblDate = new Label
        {
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(16, 36)
        };
        UpdateDateLabel();
        pnl.Controls.Add(lblDate);

        var btnClose = new Button
        {
            Text = "",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Size = new Size(32, 32),
            Image = GlyphImage(GClose, 12, Color.Black),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnClose.FlatAppearance.BorderColor = BorderGray;
        btnClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        pnl.Controls.Add(btnClose);
        pnl.Resize += (_, _) => btnClose.Location = new Point(pnl.Width - btnClose.Width - 12, 14);
        btnClose.Location = new Point(pnl.Width - btnClose.Width - 12, 14);

        return pnl;
    }

    private void UpdateDateLabel()
    {
        string[] days = { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
        var d = DateTime.Now;
        if (lblDate != null)
            lblDate.Text = $"{days[(int)d.DayOfWeek]}, ngày {d:dd} tháng {d:MM} năm {d:yyyy}";
    }

    // ---------------------------------------------------------
    // KPI
    // ---------------------------------------------------------
    private Panel BuildKpiArea()
    {
        var host = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(12, 12, 12, 0) };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
        for (int i = 0; i < 3; i++) tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        tbl.Controls.Add(KpiCard(GWarn, Red,    "Tổng số sách quá hạn",   "0", 0, out lblKpiBooks),   0, 0);
        tbl.Controls.Add(KpiCard(GWarn, Orange, "Độc giả đang trễ hạn",   "0", 1, out lblKpiReaders), 1, 0);
        tbl.Controls.Add(KpiCard(GWarn, Primary,"Tổng tiền phạt tạm tính","0 đ", 2, out lblKpiFine),   2, 0);

        host.Controls.Add(tbl);
        return host;
    }

    private Panel KpiCard(string glyph, Color accent, string caption, string value, int index, out Label valueLabel)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(index == 0 ? 0 : 6, 0, index == 2 ? 0 : 6, 0)
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(BorderGray);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        card.Controls.Add(new Label
        {
            Text = glyph, Font = new Font(IconFont, 20F), ForeColor = accent,
            BackColor = Color.FromArgb(22, accent),
            TextAlign = ContentAlignment.MiddleCenter, Size = new Size(56, 56),
            Location = new Point(16, 22)
        });
        card.Controls.Add(new Label
        {
            Text = caption, Font = new Font("Segoe UI", 9F), ForeColor = TextGray,
            AutoSize = true, Location = new Point(86, 20)
        });
        valueLabel = new Label
        {
            Text = value, Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = accent, AutoSize = true, Location = new Point(84, 44)
        };
        card.Controls.Add(valueLabel);
        return card;
    }

    // ---------------------------------------------------------
    // TOOLBAR (GroupBox "Tìm kiếm & Bộ lọc")
    // ---------------------------------------------------------
    private Panel BuildToolbar()
    {
        var host = new Panel { Dock = DockStyle.Top, Height = 132, Padding = new Padding(12, 8, 12, 0) };
        var grp = new GroupBox
        {
            Text = "Tìm kiếm & Bộ lọc",
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 6, 12, 8)
        };

        // --- Row 1: search + loại + trạng thái + ngày + action ---
        var row1 = new Panel { Dock = DockStyle.Top, Height = 38 };

        txtSearch = new TextBox
        {
            PlaceholderText = "Tìm mã phiếu, mã ĐG, tên ĐG, SĐT, mã vạch, tên sách...",
            Location = new Point(6, 6), Width = 320, Font = new Font("Segoe UI", 9F)
        };
        txtSearch.TextChanged += async (_, _) => await LoadDataAsync();
        row1.Controls.Add(txtSearch);

        row1.Controls.Add(MakeLabel("Quá hạn:", 336, 12));
        cboDaysFilter = MakeCombo(402, 8, 165, new[] {
            "Tất cả", "Quá hạn 1–7 ngày", "Quá hạn 8–30 ngày", "Quá hạn trên 30 ngày"
        });
        cboDaysFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();
        row1.Controls.Add(cboDaysFilter);

        row1.Controls.Add(MakeLabel("Trạng thái nhắc:", 580, 12));
        cboReminderStatus = MakeCombo(690, 8, 175, new[] {
            "Tất cả", "Chưa nhắc", "Đã nhắc hôm nay", "Chưa nhắc hôm nay"
        });
        cboReminderStatus.SelectedIndexChanged += async (_, _) => await LoadDataAsync();
        row1.Controls.Add(cboReminderStatus);

        chkUseDueRange = new CheckBox
        {
            Text = "Hạn trả:",
            AutoSize = true, ForeColor = TextGray, Font = new Font("Segoe UI", 8.5F),
            Location = new Point(880, 12)
        };
        row1.Controls.Add(chkUseDueRange);

        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short, Location = new Point(950, 8),
            Width = 110, Enabled = false, Value = DateTime.Today.AddDays(-60)
        };
        row1.Controls.Add(dtpFrom);

        row1.Controls.Add(new Label { Text = "–", AutoSize = true, ForeColor = TextGray, Location = new Point(1064, 12) });

        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short, Location = new Point(1080, 8),
            Width = 110, Enabled = false, Value = DateTime.Today
        };
        row1.Controls.Add(dtpTo);

        chkUseDueRange.CheckedChanged += async (_, _) =>
        {
            dtpFrom.Enabled = dtpTo.Enabled = chkUseDueRange.Checked;
            await LoadDataAsync();
        };
        dtpFrom.ValueChanged += async (_, _) => { if (chkUseDueRange.Checked) await LoadDataAsync(); };
        dtpTo.ValueChanged   += async (_, _) => { if (chkUseDueRange.Checked) await LoadDataAsync(); };

        // Action buttons bên phải row1
        var right = new Panel { Dock = DockStyle.Right, Width = 330 };
        row1.Controls.Add(right);

        var btnRefresh = new Button
        {
            Text = "  Làm mới", Size = new Size(110, 32), Location = new Point(0, 4),
            FlatStyle = FlatStyle.Flat, BackColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            Image = GlyphImage(GRefresh, 14, Color.Black),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnRefresh.FlatAppearance.BorderColor = BorderGray;
        btnRefresh.Click += async (_, _) => await LoadDataAsync();
        right.Controls.Add(btnRefresh);

        var btnExport = new Button
        {
            Text = "  Xuất CSV", Size = new Size(120, 32), Location = new Point(120, 4),
            FlatStyle = FlatStyle.Flat, BackColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            Image = GlyphImage(GExport, 14, Color.Black),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnExport.FlatAppearance.BorderColor = BorderGray;
        btnExport.Click += (_, _) => ExportCsv();
        right.Controls.Add(btnExport);

        var btnPrint = new Button
        {
            Text = "  In danh sách", Size = new Size(150, 32), Location = new Point(250, 4),
            FlatStyle = FlatStyle.Flat, BackColor = Primary, ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GPrint, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += (_, _) => ShowPrintPreview();
        right.Controls.Add(btnPrint);

        // --- Row 2: kênh gửi + nút Gửi thông báo (to, ngang) ---
        var row2 = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 6, 0, 0) };

        row2.Controls.Add(MakeLabel("Kênh gửi:", 6, 12));
        cboChannel = MakeCombo(80, 8, 160, new[] { "Email", "SMS", "Email + SMS" });
        row2.Controls.Add(cboChannel);

        btnNotify = new Button
        {
            Text = "  Gửi thông báo (0)",
            Size = new Size(240, 34),
            Location = new Point(256, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Orange,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GSend, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        btnNotify.FlatAppearance.BorderSize = 0;
        btnNotify.Click += async (_, _) => await SendNotificationsAsync();
        row2.Controls.Add(btnNotify);

        var lblHint = new Label
        {
            Text = "Tick chọn dòng trong bảng (cột đầu) hoặc dùng menu “…” để gửi từng độc giả.",
            AutoSize = true, ForeColor = TextGray, Font = new Font("Segoe UI", 8.5F),
            Location = new Point(510, 14)
        };
        row2.Controls.Add(lblHint);

        // group chứa cả 2 row
        var inner = new Panel { Dock = DockStyle.Fill };
        inner.Controls.Add(row2);
        inner.Controls.Add(row1);
        grp.Controls.Add(inner);

        host.Controls.Add(grp);
        return host;
    }

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text = text, AutoSize = true, ForeColor = Color.FromArgb(110, 117, 128),
        Font = new Font("Segoe UI", 8.5F), Location = new Point(x, y)
    };

    private static ComboBox MakeCombo(int x, int y, int width, string[] items)
    {
        var c = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(x, y), Width = width, Font = new Font("Segoe UI", 9F)
        };
        c.Items.AddRange(items);
        c.SelectedIndex = 0;
        return c;
    }

    // ---------------------------------------------------------
    // TABLE AREA
    // ---------------------------------------------------------
    private GroupBox BuildTableArea()
    {
        var grp = new GroupBox
        {
            Text = "Danh sách sách quá hạn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10),
            Margin = new Padding(12)
        };
        dgvOverdue = BuildGrid();
        grp.Controls.Add(dgvOverdue);
        return grp;
    }

    private DataGridView BuildGrid()
{
    var g = new DataGridView
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,               // grid vẫn ReadOnly...
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        RowHeadersVisible = false, BackgroundColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
        GridColor = Color.FromArgb(225, 228, 233),
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, EnableHeadersVisualStyles = false,
        RowTemplate = { Height = 52 },
        ColumnHeadersHeight = 38,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        Font = new Font("Segoe UI", 9F)
    };

    g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
    g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
    g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 243, 246);
    g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
    g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
    g.DefaultCellStyle.SelectionForeColor = Color.Black;

    // --- Cột checkbox: ReadOnly = FALSE để tick được ---
    g.Columns.Add(new DataGridViewCheckBoxColumn
    {
        Name = "colSelected",
        HeaderText = "",              // sẽ vẽ checkbox header bằng tay
        FillWeight = 30,
        ReadOnly = false,             // ⭐ quan trọng
        TrueValue = true,
        FalseValue = false
    });

    AddCol(g, "colTicket",   "Mã phiếu mượn",    70);
    AddCol(g, "colReader",   "Độc giả",           130);
    AddCol(g, "colContact",  "Liên hệ",           140);
    AddCol(g, "colBook",     "Thông tin sách",    150);
    AddCol(g, "colBorrowed", "Ngày mượn",         80);
    AddCol(g, "colDue",      "Hạn trả",           80);
    AddCol(g, "colOverdue",  "Quá hạn",           80);
    AddCol(g, "colPenalty",  "Phạt tạm tính",     90);
    AddCol(g, "colNotice",   "Trạng thái nhắc",   140);
    AddCol(g, "colActions",  "Thao tác",          110);

    g.Columns["colTicket"]!.DefaultCellStyle.ForeColor = Primary;
    g.Columns["colTicket"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    g.Columns["colPenalty"]!.DefaultCellStyle.ForeColor = Red;
    g.Columns["colPenalty"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

    // ⭐ Bắt buộc: commit ngay khi click checkbox
    g.CurrentCellDirtyStateChanged += (_, _) =>
    {
        if (g.IsCurrentCellDirty)
            g.CommitEdit(DataGridViewDataErrorContexts.Commit);
    };

    g.CellValueChanged       += Grid_CellValueChanged;
    g.CellPainting           += Grid_CellPainting;
    g.CellMouseClick         += async (s, e) => await Grid_CellMouseClickAsync(s, e);
    g.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;   // ⭐ bắt click header
    
    g.CellMouseEnter += (s, e) =>
    {
        if (e.RowIndex == -1 && e.ColumnIndex >= 0 &&
            g.Columns[e.ColumnIndex].Name == "colSelected")
        {
            _headerCheckboxHover = true;
            g.InvalidateCell(e.ColumnIndex, -1);
        }
    };
    g.CellMouseLeave += (s, e) =>
    {
        if (e.RowIndex == -1)
        {
            _headerCheckboxHover = false;
            dgvOverdue.InvalidateCell(dgvOverdue.Columns["colSelected"].HeaderCell);
        }
    };
    
    return g;
}
    
    private void Grid_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0) return;
        if (dgvOverdue.Columns[e.ColumnIndex].Name != "colSelected") return;

        // Tính lại rect theo toạ độ CELL (e.Location cùng hệ toạ độ)
        int box = 16;
        int cellW = dgvOverdue.Columns[e.ColumnIndex].Width;
        int cellH = dgvOverdue.ColumnHeadersHeight;
        var localRect = new Rectangle((cellW - box) / 2, (cellH - box) / 2, box, box);

        if (!localRect.Contains(e.Location)) return;

        _allSelectedOnPage = !_allSelectedOnPage;

        foreach (DataGridViewRow row in dgvOverdue.Rows)
        {
            row.Cells["colSelected"].Value = _allSelectedOnPage;

            if (row.Tag is ManageOverdueBookInfo b)
            {
                if (_allSelectedOnPage) _selectedIds.Add(b.LoanCode);
                else                    _selectedIds.Remove(b.LoanCode);
            }
        }

        dgvOverdue.InvalidateColumn(e.ColumnIndex);
        UpdateNotifyButton();
    }
    private static void AddCol(DataGridView g, string name, string header, float weight)
    {
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name, HeaderText = header, FillWeight = weight
        });
    }

    // ---------------------------------------------------------
    // PAGER
    // ---------------------------------------------------------
    private Panel BuildPager()
    {
        var host = new Panel
        {
            Dock = DockStyle.Bottom, Height = 46,
            Padding = new Padding(12, 4, 12, 8), BackColor = BgPage
        };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        btnPrevPage = MakeFlatButton("‹  Trước", 95);
        btnPrevPage.Click += (_, _) => { if (_currentPage > 1) { _currentPage--; RenderPage(); } };

        lblPageInfo = new Label
        {
            Text = "Trang 1/1 - 0 dòng", AutoSize = false, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = TextGray,
            Font = new Font("Segoe UI", 9F)
        };

        btnNextPage = MakeFlatButton("Sau  ›", 95);
        btnNextPage.Click += (_, _) => { if (_currentPage < _totalPages) { _currentPage++; RenderPage(); } };

        var btnRefresh = MakeFlatButton("Làm mới", 90);
        btnRefresh.Image = GlyphImage(GRefresh, 14, Color.Black);
        btnRefresh.ImageAlign = ContentAlignment.MiddleLeft;
        btnRefresh.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnRefresh.Padding = new Padding(6, 0, 0, 0);
        btnRefresh.Click += async (_, _) => await LoadDataAsync();

        tbl.Controls.Add(btnPrevPage, 0, 0);
        tbl.Controls.Add(lblPageInfo, 1, 0);
        tbl.Controls.Add(btnNextPage, 2, 0);
        tbl.Controls.Add(btnRefresh, 3, 0);
        host.Controls.Add(tbl);
        return host;
    }

    private static Button MakeFlatButton(string text, int width) => new()
    {
        Text = text, Width = width, Height = 32,
        BackColor = Color.White, ForeColor = Color.Black,
        FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F),
        Margin = new Padding(4, 8, 0, 4), Cursor = Cursors.Hand
    };

    // =========================================================
    // LOAD DATA
    // =========================================================
    private async Task LoadDataAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            Cursor = Cursors.WaitCursor;

            var filter = BuildFilter();
            _allBooks = await _service.GetManageOverdueBooks(filter);

            _filteredBooks = _allBooks;
            _currentPage = 1;
            _totalPages = Math.Max(1, (int)Math.Ceiling(_filteredBooks.Count / (double)PageSize));
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            RenderPage();
            RecalculateKpi();
            UpdateNotifyButton();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không tải được dữ liệu quá hạn: " + ex.Message,
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _isLoading = false;
        }
    }

    private ManageOverdueBookFilter BuildFilter()
    {
        var filter = new ManageOverdueBookFilter
        {
            KeyWord = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim(),
            OverduePeriod = cboDaysFilter.SelectedIndex switch
            {
                1 => OverduePeriod.From1To7Days,
                2 => OverduePeriod.From8To30Days,
                3 => OverduePeriod.Over30Days,
                _ => OverduePeriod.All
            },
            Status = cboReminderStatus.SelectedIndex switch
            {
                1 => ReminderFilterStatus.NeverReminded,
                2 => ReminderFilterStatus.RemindedToday,
                3 => ReminderFilterStatus.NotRemindedToday,
                _ => ReminderFilterStatus.All
            }
        };

        if (chkUseDueRange.Checked)
        {
            filter = filter with
            {
                DueFrom = dtpFrom.Value.Date,
                DueTo = dtpTo.Value.Date
            };
        }
        return filter;
    }

    // =========================================================
    // RENDER
    // =========================================================
    private void RenderPage()
    {
        dgvOverdue.Rows.Clear();

        var pageRows = _filteredBooks
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var book in pageRows)
        {
            int idx = dgvOverdue.Rows.Add();
            var row = dgvOverdue.Rows[idx];
            row.Tag = book;

            row.Cells["colSelected"].Value = _selectedIds.Contains(book.LoanCode);
            row.Cells["colTicket"].Value   = book.LoanCode;
            row.Cells["colReader"].Value   = $"{book.FullName}\n{book.UserCode}";
            row.Cells["colContact"].Value  = $"{book.PhoneNumber}\n{book.Email}".Trim();
            row.Cells["colBook"].Value     = $"{book.Title}\n{book.CopyCode}";
            row.Cells["colBorrowed"].Value = book.BorrowedDay.ToString("dd/MM/yyyy");
            row.Cells["colDue"].Value      = book.DueDay.ToString("dd/MM/yyyy");
            row.Cells["colOverdue"].Value  = $"{book.OverdueDays} ngày";
            row.Cells["colPenalty"].Value  = book.FinePay.ToString("N0") + " đ";
            row.Cells["colNotice"].Value   = BuildNoticeText(book);
        }

        lblPageInfo.Text = $"Trang {_currentPage}/{_totalPages} - {_filteredBooks.Count} dòng";
        btnPrevPage.Enabled = _currentPage > 1;
        btnNextPage.Enabled = _currentPage < _totalPages;
        dgvOverdue.ClearSelection();
        
        // ⭐ đồng bộ trạng thái checkbox header với dữ liệu trang hiện tại
        _allSelectedOnPage = dgvOverdue.Rows.Count > 0
                             && dgvOverdue.Rows.Cast<DataGridViewRow>()
                                 .All(r => r.Cells["colSelected"].Value is bool b && b);

        dgvOverdue.InvalidateCell(dgvOverdue.Columns["colSelected"].HeaderCell);
    }

    private static string BuildNoticeText(ManageOverdueBookInfo book)
    {
        if (book.ReminderStatus == ReminderEmailPhone.NeverReminded || !book.LatestReminderAt.HasValue)
            return "Chưa nhắc";

        string tag = book.ReminderStatus switch
        {
            ReminderEmailPhone.RemindedToday  => "Đã nhắc hôm nay",
            ReminderEmailPhone.RemindedBefore => "Đã nhắc trước đó",
            _ => "Đã nhắc"
        };
        return $"{tag} · {book.LatestReminderAt.Value:dd/MM HH:mm}";
    }

    private void RecalculateKpi()
    {
        int totalBooks = _filteredBooks.Count;
        int totalReaders = _filteredBooks.Select(b => b.UserCode).Distinct().Count();
        double totalFine = _filteredBooks.Sum(b => b.FinePay);

        lblKpiBooks.Text   = totalBooks.ToString("N0");
        lblKpiReaders.Text = totalReaders.ToString("N0");
        lblKpiFine.Text    = totalFine.ToString("N0") + " đ";
    }

    private void UpdateNotifyButton()
    {
        int count = _selectedIds.Count;
        btnNotify.Text = $"Gửi thông báo ({count})";
        btnNotify.Enabled = count > 0;
    }

    // =========================================================
    // GRID PAINTING (pill + icon buttons) — theo FineManagement
    // =========================================================
    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.Graphics == null) return;

// ========== HEADER CHECKBOX ==========
        if (e.RowIndex == -1 && e.ColumnIndex >= 0 &&
            dgvOverdue.Columns[e.ColumnIndex].Name == "colSelected")
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);

            int box = 16;
            _headerCheckboxRect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - box) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - box) / 2,
                box, box);

            var state = _allSelectedOnPage ? ButtonState.Checked : ButtonState.Normal;

            if (_headerCheckboxHover)
                state |= ButtonState.Flat;   // hoặc dùng Pushed để có hiệu ứng hover

            ControlPaint.DrawCheckBox(e.Graphics, _headerCheckboxRect, state);
            e.Handled = true;
            return;
        }

        // ========== BODY CELLS ==========
        if (e.RowIndex < 0) return;
        string colName = dgvOverdue.Columns[e.ColumnIndex].Name;
        var book = dgvOverdue.Rows[e.RowIndex].Tag as ManageOverdueBookInfo;
        if (book == null) return;

        if (colName == "colOverdue")
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background
                | DataGridViewPaintParts.Border
                | DataGridViewPaintParts.SelectionBackground);

            Color color = book.OverdueDays > 30 ? Red
                        : book.OverdueDays > 7  ? Orange
                        : Color.FromArgb(190, 90, 30);

            DrawPill(e.Graphics, e.CellBounds, $"{book.OverdueDays} ngày", color);
            e.Handled = true;
        }
        else if (colName == "colNotice")
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background
                | DataGridViewPaintParts.Border
                | DataGridViewPaintParts.SelectionBackground);

            bool reminded = book.ReminderStatus != ReminderEmailPhone.NeverReminded
                            && book.LatestReminderAt.HasValue;
            Color color = reminded ? Green : Red;
            string text = reminded
                ? $"Đã nhắc · {book.LatestReminderAt!.Value:dd/MM HH:mm}"
                : "Chưa nhắc";

            DrawPill(e.Graphics, e.CellBounds, text, color, fontSize: 8F);
            e.Handled = true;
        }
        else if (colName == "colActions")
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background
                | DataGridViewPaintParts.Border
                | DataGridViewPaintParts.SelectionBackground);
            DrawActionButtons(e.Graphics, e.CellBounds, book);
            e.Handled = true;
        }
    }
    private void DrawPill(Graphics g, Rectangle bounds, string text, Color color, float fontSize = 8.5F)
    {
        var font = new Font("Segoe UI", fontSize, FontStyle.Bold);
        var size = TextRenderer.MeasureText(text, font);
        int width = Math.Min(size.Width + 14, bounds.Width - 8);
        var rect = new Rectangle(bounds.X + 6, bounds.Y + (bounds.Height - 24) / 2, width, 24);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var path = RoundedRect(rect, 12))
        using (var fill = new SolidBrush(Color.FromArgb(22, color)))
        using (var pen  = new Pen(Color.FromArgb(150, color)))
        {
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }
        TextRenderer.DrawText(g, text, font, rect, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    private void DrawActionButtons(Graphics g, Rectangle bounds, ManageOverdueBookInfo book)
    {
        int cx = bounds.X + bounds.Width / 2;

        // Menu "…" luôn ở phải
        var moreRect = new Rectangle(cx - 4, bounds.Y + (bounds.Height - 28) / 2, 28, 28);
        DrawIconButton(g, moreRect, GMore, TextGray);

        // Icon "Gửi nhắc" bên trái (thay cho "Thu tiền" của FineManagement)
        var remindRect = new Rectangle(cx - 46, bounds.Y + (bounds.Height - 28) / 2, 40, 28);
        DrawIconButton(g, remindRect, GSend, Orange);
    }

    private void DrawIconButton(Graphics g, Rectangle rect, string glyph, Color color)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var path = RoundedRect(rect, 6))
        using (var fill = new SolidBrush(Color.FromArgb(18, color)))
        using (var pen  = new Pen(Color.FromArgb(120, color)))
        {
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }
        using var f = new Font(IconFont, 12F);
        TextRenderer.DrawText(g, glyph, f, rect, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    // =========================================================
    // GRID EVENTS
    // =========================================================
    private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (dgvOverdue.Columns[e.ColumnIndex].Name != "colSelected") return;

        var row = dgvOverdue.Rows[e.RowIndex];
        if (row.Tag is not ManageOverdueBookInfo book) return;

        bool isChecked = row.Cells["colSelected"].Value is bool b && b;
        if (isChecked) _selectedIds.Add(book.LoanCode);
        else _selectedIds.Remove(book.LoanCode);

        // ⭐ đồng bộ header checkbox
        _allSelectedOnPage = dgvOverdue.Rows.Count > 0
                             && dgvOverdue.Rows.Cast<DataGridViewRow>()
                                 .All(r => r.Cells["colSelected"].Value is bool b2 && b2);
        dgvOverdue.InvalidateCell(dgvOverdue.Columns["colSelected"].HeaderCell);

        UpdateNotifyButton();
    }
    private async Task Grid_CellMouseClickAsync(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (e.Button != MouseButtons.Left) return;

        var book = dgvOverdue.Rows[e.RowIndex].Tag as ManageOverdueBookInfo;
        if (book == null) return;
        
        

        string colName = dgvOverdue.Columns[e.ColumnIndex].Name;
        
        if (colName == "colSelected") return;   // để DataGridView tự xử lý toggle

        // Click vào Mã phiếu → mở drawer chi tiết
        if (colName == "colTicket")
        {
            OpenDrawer(book);
            return;
        }

        if (colName != "colActions") return;

        int relX = e.Location.X;
        int cellW = dgvOverdue.Columns[e.ColumnIndex].Width;
        int cx = cellW / 2;

        bool onRemindIcon = relX >= cx - 50 && relX <= cx - 2;
        bool onMoreIcon   = relX >= cx - 6  && relX <= cx + 30;

        if (onRemindIcon)
        {
            await SendSingleNotificationAsync(book);
            return;
        }

        // onMoreIcon hoặc fallback → mở menu
        ShowMoreMenu(book, e.RowIndex, e.ColumnIndex);
    }

    // =========================================================
    // MENU "…"
    // =========================================================
    private void ShowMoreMenu(ManageOverdueBookInfo book, int rowIndex, int colIndex)
    {
        try
        {
            var menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };

            var mnuDetail = new ToolStripMenuItem("  Xem chi tiết",
                GlyphImage(GHistory, 14, Color.Black), (_, _) => OpenDrawer(book));
            var mnuRemind = new ToolStripMenuItem("  Gửi nhắc trả sách",
                GlyphImage(GSend, 14, Orange), async (_, _) => await SendSingleNotificationAsync(book));
            var mnuReturn = new ToolStripMenuItem("  Trả sách",
                GlyphImage(GReturn, 14, Green), (_, _) => ReturnBookRequested?.Invoke(this, EventArgs.Empty));
            var mnuPrint = new ToolStripMenuItem("  In phiếu nhắc",
                GlyphImage(GPrint, 14, Color.Black), (_, _) => PrintReminder(book));

            menu.Items.Add(mnuDetail);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(mnuRemind);
            menu.Items.Add(mnuReturn);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(mnuPrint);

            var cellRect = dgvOverdue.GetCellDisplayRectangle(colIndex, rowIndex, false);
            var pt = dgvOverdue.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
            menu.Show(pt);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không mở được menu thao tác:\n" + ex,
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintReminder(ManageOverdueBookInfo book)
    {
        MessageBox.Show(
            $"Đang mở xem trước phiếu nhắc trả sách {book.LoanCode}...\n(Tính năng in sẽ nối với PrintService)",
            "In phiếu nhắc", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // =========================================================
    // SEND NOTIFICATIONS
    // =========================================================
    private async Task SendNotificationsAsync()
    {
        var selectedBooks = _allBooks
            .Where(b => _selectedIds.Contains(b.LoanCode))
            .ToList();

        if (selectedBooks.Count == 0) return;

        var channel = GetSelectedChannel();
        if (!ValidateContacts(selectedBooks, channel)) return;

        if (MessageBox.Show(
            $"Bạn đang gửi {selectedBooks.Count} thông báo nhắc trả sách.\n" +
            $"Kênh: {ChannelText(channel)}\n\nTiếp tục?",
            "Xác nhận gửi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        int ok = 0, fail = 0;
        string? lastError = null;

        btnNotify.Enabled = false;
        try
        {
            foreach (var book in selectedBooks)
            {
                try
                {
                    await _service.AddReminderToUser(
                        book.IdLoanDetail, channel, BuildReminderContent(book));
                    ok++;
                }
                catch (Exception ex)
                {
                    fail++;
                    lastError = ex.Message;
                }
            }
        }
        finally
        {
            btnNotify.Enabled = true;
        }

        MessageBox.Show(
            $"Đã gửi thành công {ok} thông báo." +
            (fail > 0 ? $"\nThất bại: {fail}" + (lastError != null ? $"\nLỗi: {lastError}" : "") : ""),
            "LIBRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

        _selectedIds.Clear();
        await LoadDataAsync();
    }

    private async Task SendSingleNotificationAsync(ManageOverdueBookInfo book)
    {
        var channel = GetSelectedChannel();
        if (!ValidateContacts(new[] { book }, channel)) return;

        if (MessageBox.Show(
            $"Độc giả: {book.FullName} ({book.UserCode})\n" +
            $"Kênh gửi: {ChannelText(channel)}\n\nTiếp tục?",
            "Gửi thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            await _service.AddReminderToUser(book.IdLoanDetail, channel, BuildReminderContent(book));
            MessageBox.Show("Đã gửi thông báo.", "LIBRA",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Gửi thông báo thất bại: " + ex.Message,
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private ReminderChannel GetSelectedChannel() => cboChannel.SelectedIndex switch
    {
        1 => ReminderChannel.SMS,
        2 => ReminderChannel.Both,
        _ => ReminderChannel.Email
    };

    private static string ChannelText(ReminderChannel channel) => channel switch
    {
        ReminderChannel.Email => "Email",
        ReminderChannel.SMS   => "SMS",
        ReminderChannel.Both  => "Email + SMS",
        _                     => "Email"
    };

    private bool ValidateContacts(IEnumerable<ManageOverdueBookInfo> books, ReminderChannel channel)
    {
        foreach (var b in books)
        {
            bool hasEmail = !string.IsNullOrWhiteSpace(b.Email);
            bool hasPhone = !string.IsNullOrWhiteSpace(b.PhoneNumber);
            bool valid = channel switch
            {
                ReminderChannel.Email => hasEmail,
                ReminderChannel.SMS   => hasPhone,
                ReminderChannel.Both  => hasEmail && hasPhone,
                _ => false
            };
            if (!valid)
            {
                MessageBox.Show(
                    $"Độc giả {b.FullName} thiếu thông tin liên hệ phù hợp cho kênh {ChannelText(channel)}.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
        return true;
    }

    private static string BuildReminderContent(ManageOverdueBookInfo book)
    {
        return
            $"Kính gửi {book.FullName} ({book.UserCode}),\n\n" +
            $"Cuốn sách: {book.Title}\n" +
            $"Mã bản sách: {book.CopyCode}\n" +
            $"Ngày mượn: {book.BorrowedDay:dd/MM/yyyy}\n" +
            $"Hạn trả: {book.DueDay:dd/MM/yyyy}\n" +
            $"Số ngày quá hạn: {book.OverdueDays} ngày\n" +
            $"Tiền phạt tạm tính: {book.FinePay:N0} đ\n\n" +
            "Vui lòng mang sách đến thư viện để trả.\n\nTrân trọng.\nLIBRA Library";
    }

    // =========================================================
    // DRAWER
    // =========================================================
    private Panel BuildDrawer()
    {
        pnlDrawer = new Panel
        {
            Dock = DockStyle.Right, Width = 0,
            BackColor = Color.White, Padding = new Padding(0)
        };
        pnlDrawer.Paint += (s, e) =>
        {
            using var pen = new Pen(BorderGray);
            e.Graphics.DrawLine(pen, 0, 0, 0, pnlDrawer.Height);
        };

        // Header
        var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = SoftBlue, Padding = new Padding(14, 10, 14, 10) };
        lblDrawerTitle = new Label
        {
            Text = "Chi tiết quá hạn",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            AutoSize = true, Location = new Point(14, 12)
        };
        lblDrawerSub = new Label
        {
            Text = "", Font = new Font("Segoe UI", 8.5F), ForeColor = TextGray,
            AutoSize = true, Location = new Point(14, 40)
        };
        var btnClose = new Button
        {
            Text = "", Size = new Size(30, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            Image = GlyphImage(GClose, 14, Color.Black)
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (_, _) => CloseDrawer();
        header.Controls.Add(lblDrawerTitle);
        header.Controls.Add(lblDrawerSub);
        header.Controls.Add(btnClose);
        header.Resize += (_, _) => btnClose.Location = new Point(header.Width - 40, 10);

        // Tabs
        tabDrawer = new TabControl
        {
            Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Padding = new Point(14, 4)
        };
        var tabInfo    = new TabPage("Thông tin chi tiết") { BackColor = Color.White, Padding = new Padding(8) };
        var tabHistory = new TabPage("Lịch sử nhắc")       { BackColor = Color.White, Padding = new Padding(8) };

        // Tab info: dùng TableLayoutPanel đơn giản
        var infoGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true,
            Padding = new Padding(4)
        };
        infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tabInfo.Controls.Add(infoGrid);
        tabInfo.Tag = infoGrid;

        // Tab history: FlowLayoutPanel các item lịch sử nhắc
        flowHistory = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.White
        };
        flowHistory.Resize += (_, _) =>
        {
            foreach (Control c in flowHistory.Controls) c.Width = flowHistory.ClientSize.Width - 8;
        };
        tabHistory.Controls.Add(flowHistory);

        tabDrawer.TabPages.Add(tabInfo);
        tabDrawer.TabPages.Add(tabHistory);

        pnlDrawer.Controls.Add(tabDrawer);
        pnlDrawer.Controls.Add(header);
        return pnlDrawer;
    }

    private void OpenDrawer(ManageOverdueBookInfo book)
    {
        _drawerBook = book;
        lblDrawerTitle.Text = $"Chi tiết phiếu {book.LoanCode}";
        lblDrawerSub.Text   = $"{book.FullName} · {book.UserCode}  —  Quá hạn {book.OverdueDays} ngày / {book.FinePay:N0} đ";

        var infoGrid = (TableLayoutPanel)tabDrawer.TabPages[0].Tag!;
        infoGrid.SuspendLayout();
        infoGrid.Controls.Clear();
        infoGrid.RowStyles.Clear();
        infoGrid.RowCount = 0;

        void AddRow(string label, string value)
        {
            infoGrid.RowCount++;
            infoGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            int r = infoGrid.RowCount - 1;

            var lbl = new Label
            {
                Text = label, ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill
            };
            var val = new Label
            {
                Text = value, Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill
            };
            infoGrid.Controls.Add(lbl, 0, r);
            infoGrid.Controls.Add(val, 1, r);
        }

        AddRow("Mã phiếu mượn", book.LoanCode);
        AddRow("Mã độc giả",    book.UserCode);
        AddRow("Họ và tên",     book.FullName);
        AddRow("Số điện thoại", book.PhoneNumber ?? "—");
        AddRow("Email",         book.Email ?? "—");
        AddRow("Tên sách",      book.Title);
        AddRow("Mã vạch",       book.CopyCode);
        AddRow("Ngày mượn",     book.BorrowedDay.ToString("dd/MM/yyyy"));
        AddRow("Hạn trả",       book.DueDay.ToString("dd/MM/yyyy"));
        AddRow("Số ngày quá hạn", $"{book.OverdueDays} ngày");
        AddRow("Phạt tạm tính", book.FinePay.ToString("N0") + " đ");
        AddRow("Trạng thái nhắc", BuildNoticeText(book));

        infoGrid.ResumeLayout(true);

        // Tab history
        flowHistory.Controls.Clear();
        if (book.ReminderStatus == ReminderEmailPhone.NeverReminded || !book.LatestReminderAt.HasValue)
        {
            flowHistory.Controls.Add(BuildHistoryItem(
                "Chưa nhắc", Red, "Chưa gửi thông báo nào cho độc giả này.", ""));
        }
        else
        {
            flowHistory.Controls.Add(BuildHistoryItem(
                book.ReminderStatus switch
                {
                    ReminderEmailPhone.RemindedToday  => "Đã nhắc hôm nay",
                    ReminderEmailPhone.RemindedBefore => "Đã nhắc trước đó",
                    _ => "Đã nhắc"
                },
                Green,
                "Lần nhắc gần nhất được ghi nhận từ backend.",
                book.LatestReminderAt.Value.ToString("dd/MM/yyyy HH:mm")));
        }

        pnlDrawer.Width = 480;
    }

    private void CloseDrawer()
    {
        pnlDrawer.Width = 0;
        _drawerBook = null;
    }

    private Panel BuildHistoryItem(string action, Color color, string reason, string time)
    {
        var panel = new Panel
        {
            Height = 84,
            BackColor = Color.FromArgb(250, 251, 252),
            Margin = new Padding(4, 0, 4, 8),
            Padding = new Padding(10)
        };
        panel.Paint += (s, e) =>
        {
            using var pen = new Pen(BorderGray);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            using var bar = new SolidBrush(color);
            e.Graphics.FillRectangle(bar, 0, 0, 4, panel.Height);
        };

        panel.Controls.Add(new Label
        {
            Text = action, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = color, AutoSize = true, Location = new Point(14, 8)
        });
        panel.Controls.Add(new Label
        {
            Text = time, Font = new Font("Segoe UI", 8F), ForeColor = TextGray,
            AutoSize = true, Location = new Point(150, 11)
        });
        panel.Controls.Add(new Label
        {
            Text = reason, Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(60, 60, 60),
            AutoSize = false, Size = new Size(panel.Width - 28, 42),
            Location = new Point(14, 34), AutoEllipsis = true
        });
        return panel;
    }

    // =========================================================
    // EXPORT / PRINT
    // =========================================================
    private void ExportCsv()
    {
        if (_filteredBooks.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV file (*.csv)|*.csv",
            FileName = $"SachQuaHan_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Mã phiếu,Mã độc giả,Họ tên,SĐT,Email,Mã vạch,Tên sách,Ngày mượn,Hạn trả,Quá hạn,Phạt tạm tính,Trạng thái nhắc");

            foreach (var b in _filteredBooks)
            {
                string[] values =
                {
                    b.LoanCode, b.UserCode, b.FullName,
                    b.PhoneNumber ?? "", b.Email ?? "",
                    b.CopyCode, b.Title,
                    b.BorrowedDay.ToString("dd/MM/yyyy"),
                    b.DueDay.ToString("dd/MM/yyyy"),
                    b.OverdueDays.ToString(),
                    b.FinePay.ToString("N0"),
                    BuildNoticeText(b)
                };
                sb.AppendLine(string.Join(",", values.Select(CsvValue)));
            }
            File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show("Xuất CSV thành công!", "LIBRA",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi xuất CSV: " + ex.Message, "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string CsvValue(string v) => $"\"{v.Replace("\"", "\"\"")}\"";

    private void ShowPrintPreview()
    {
        if (_filteredBooks.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xem trước.");
            return;
        }

        using var form = new Form
        {
            Text = "Xem trước danh sách sách quá hạn",
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(850, 600),
            Font = Font
        };
        var txt = new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
            ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 10F)
        };
        var sb = new StringBuilder();
        sb.AppendLine("DANH SÁCH SÁCH QUÁ HẠN");
        sb.AppendLine("LIBRA LIBRARY");
        sb.AppendLine(new string('-', 90));
        foreach (var b in _filteredBooks)
        {
            sb.AppendLine($"Mã phiếu: {b.LoanCode}");
            sb.AppendLine($"Độc giả: {b.FullName} ({b.UserCode})");
            sb.AppendLine($"Sách: {b.Title}");
            sb.AppendLine($"Mã bản sách: {b.CopyCode}");
            sb.AppendLine($"Hạn trả: {b.DueDay:dd/MM/yyyy}");
            sb.AppendLine($"Quá hạn: {b.OverdueDays} ngày");
            sb.AppendLine($"Tiền phạt: {b.FinePay:N0} đ");
            sb.AppendLine($"Trạng thái nhắc: {BuildNoticeText(b)}");
            sb.AppendLine(new string('-', 90));
        }
        txt.Text = sb.ToString();
        form.Controls.Add(txt);
        form.ShowDialog(this);
    }

    // =========================================================
    // TIỆN ÍCH
    // =========================================================
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
}