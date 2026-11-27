using System.Drawing.Drawing2D;
using System.Text;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Services;
using ContentAlignment = System.Drawing.ContentAlignment;
using LibraryManagement.Forms.OverduePrintPreviewForm;

namespace LibraryManagement.Forms.OverdueBooks;

public partial class OverdueBooks : UserControl
{
    // =========================================================
    // CONFIGURATION
    // =========================================================
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
    private bool _isLoading;

    private HashSet<string> _selectedIds = new(StringComparer.OrdinalIgnoreCase);
    private ManageOverdueBookInfo? _drawerBook;
    
    private Rectangle _headerCheckboxRect = Rectangle.Empty;
    private bool _headerCheckboxHover = false;
    private bool _allSelected = false;
    private bool _isBulkUpdatingCheckbox;

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
        var host = new Panel { Dock = DockStyle.Top, Height = 134, Padding = new Padding(12, 12, 12, 0) };
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
        var host = new Panel { Dock = DockStyle.Top, Height = 122, Padding = new Padding(12, 8, 12, 0) };        var grp = new GroupBox
        {
            Text = "Tìm kiếm && Bộ lọc", // Dùng && để hiển thị đúng ký tự &
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 6, 12, 8)
        };

        // =========================================================================
        // HÀNG 1: [Ô tìm kiếm] | [Quá hạn: Combo] | [Trạng thái nhắc: Combo] | [Hạn trả: Từ - Đến]
        // =========================================================================
        var row1 = new Panel { Dock = DockStyle.Top, Height = 38 };

        txtSearch = new TextBox
        {
            PlaceholderText = "Tìm mã phiếu, mã ĐG, tên ĐG, SĐT, mã vạch, tên sách...",
            Font = new Font("Segoe UI", 9F)
        };
        txtSearch.TextChanged += async (_, _) => await LoadDataAsync();

        var lblDays = MakeLabel("Quá hạn:", 0, 12);
        cboDaysFilter = MakeCombo(0, 7, 155, new[] {
            "Tất cả", "Quá hạn 1–7 ngày", "Quá hạn 8–30 ngày", "Quá hạn trên 30 ngày"
        });
        cboDaysFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();

        var lblReminderStatus = MakeLabel("Trạng thái nhắc:", 0, 12);
        cboReminderStatus = MakeCombo(0, 7, 160, new[] {
            "Tất cả", "Chưa nhắc", "Đã nhắc hôm nay", "Chưa nhắc hôm nay"
        });
        cboReminderStatus.SelectedIndexChanged += async (_, _) => await LoadDataAsync();

        chkUseDueRange = new CheckBox
        {
            Text = "Hạn trả:",
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F)
        };

        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 102,
            Enabled = false,
            Value = DateTime.Today.AddDays(-60)
        };

        var lblDateSeparator = new Label
        {
            Text = "–",
            AutoSize = true,
            ForeColor = TextGray
        };

        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 102,
            Enabled = false,
            Value = DateTime.Today
        };

        row1.Controls.AddRange(new Control[]
        {
            txtSearch, lblDays, cboDaysFilter, lblReminderStatus, cboReminderStatus,
            chkUseDueRange, dtpFrom, lblDateSeparator, dtpTo
        });

        void LayoutFilterRow()
        {
            int cy = row1.ClientSize.Height / 2;
            int right = row1.ClientSize.Width - 4;

            // Sắp xếp từ phải sang trái để các bộ lọc thẳng hàng và ô tìm kiếm tự co giãn phần còn lại
            dtpTo.Location = new Point(right - dtpTo.Width, cy - dtpTo.Height / 2);
            lblDateSeparator.Location = new Point(dtpTo.Left - lblDateSeparator.Width - 5, cy - lblDateSeparator.Height / 2);
            dtpFrom.Location = new Point(lblDateSeparator.Left - dtpFrom.Width - 5, cy - dtpFrom.Height / 2);
            chkUseDueRange.Location = new Point(dtpFrom.Left - chkUseDueRange.Width - 6, cy - chkUseDueRange.Height / 2);

            cboReminderStatus.Location = new Point(chkUseDueRange.Left - cboReminderStatus.Width - 14, cy - cboReminderStatus.Height / 2);
            lblReminderStatus.Location = new Point(cboReminderStatus.Left - lblReminderStatus.Width - 6, cy - lblReminderStatus.Height / 2);

            cboDaysFilter.Location = new Point(lblReminderStatus.Left - cboDaysFilter.Width - 14, cy - cboDaysFilter.Height / 2);
            lblDays.Location = new Point(cboDaysFilter.Left - lblDays.Width - 6, cy - lblDays.Height / 2);

            int searchWidth = Math.Max(180, lblDays.Left - 18);
            txtSearch.SetBounds(4, cy - txtSearch.Height / 2, searchWidth, txtSearch.Height);
        }
        row1.Resize += (_, _) => LayoutFilterRow();
        LayoutFilterRow();

        chkUseDueRange.CheckedChanged += async (_, _) =>
        {
            dtpFrom.Enabled = dtpTo.Enabled = chkUseDueRange.Checked;
            await LoadDataAsync();
        };
        dtpFrom.ValueChanged += async (_, _) => { if (chkUseDueRange.Checked) await LoadDataAsync(); };
        dtpTo.ValueChanged   += async (_, _) => { if (chkUseDueRange.Checked) await LoadDataAsync(); };

        // =========================================================================
        // HÀNG 2: [Kênh gửi + Nút Gửi thông báo + Ghi chú] (trái)  <--->  [Làm mới | Xuất CSV | In danh sách] (phải)
        // =========================================================================
        var row2 = new Panel { Dock = DockStyle.Top, Height = 42 };
        var lblChannel = MakeLabel("Kênh gửi:", 4, 12);
        cboChannel = MakeCombo(0, 8, 130, new[] { "Email", "SMS", "Email + SMS" });

        btnNotify = CreateToolbarButton("Gửi thông báo (0)", GSend, Orange, Color.White, bold: true);
        btnNotify.Click += async (_, _) => await SendNotificationsAsync();

        var lblHint = new Label
        {
            Text = "Tick chọn dòng trong bảng hoặc dùng menu “…” để gửi từng độc giả.",
            AutoSize = false,
            AutoEllipsis = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F)
        };

        var btnRefresh = CreateToolbarButton("Làm mới", GRefresh, Color.White, Color.Black);
        btnRefresh.Click += async (_, _) => await LoadDataAsync();

        var btnExport = CreateToolbarButton("Xuất CSV", GExport, Color.White, Color.Black);
        btnExport.Click += (_, _) => ExportCsv();

        var btnPrint = CreateToolbarButton("In danh sách", GPrint, Primary, Color.White, bold: true);
        btnPrint.Click += (_, _) => ShowPrintPreview();

        row2.Controls.AddRange(new Control[]
        {
            lblChannel, cboChannel, btnNotify, lblHint, btnRefresh, btnExport, btnPrint
        });

        void LayoutActionRow()
        {
            int cy = (row2.ClientSize.Height - 2) / 2;
            int right = row2.ClientSize.Width - 4;

            // Nhóm nút chức năng bên phải
            btnPrint.Location   = new Point(right - btnPrint.Width, cy - btnPrint.Height / 2);
            btnExport.Location  = new Point(btnPrint.Left - btnExport.Width - 8, cy - btnExport.Height / 2);
            btnRefresh.Location = new Point(btnExport.Left - btnRefresh.Width - 8, cy - btnRefresh.Height / 2);

            // Nhóm gửi thông báo bên trái
            lblChannel.Location = new Point(4, cy - lblChannel.Height / 2);
            cboChannel.Location = new Point(lblChannel.Right + 6, cy - cboChannel.Height / 2);
            btnNotify.Location  = new Point(cboChannel.Right + 10, cy - btnNotify.Height / 2);

            int hintX = btnNotify.Right + 12;
            int hintMaxW = btnRefresh.Left - hintX - 12;
            lblHint.Visible = hintMaxW > 120;
            if (lblHint.Visible)
                lblHint.SetBounds(hintX, cy - 9, hintMaxW, 18);
        }
        row2.Resize += (_, _) => LayoutActionRow();
        btnNotify.SizeChanged += (_, _) => LayoutActionRow();
        LayoutActionRow();

        var inner = new Panel { Dock = DockStyle.Fill };
        inner.Controls.Add(row2);
        inner.Controls.Add(row1);
        grp.Controls.Add(inner);

        host.Controls.Add(grp);
        return host;
    }
    
    private static Button CreateToolbarButton(string text, string glyph, Color back, Color fore, bool bold = false)
    {
        var btn = new Button
        {
            Text = " " + text,
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = fore,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 32),
            MaximumSize = new Size(0, 32),
            Font = new Font("Segoe UI", 9F, bold ? FontStyle.Bold : FontStyle.Regular),
            Image = GlyphImage(glyph, 13, fore),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(9, 0, 11, 0),
            UseCompatibleTextRendering = false,
            Cursor = Cursors.Hand
        };
        if (back == Color.White)
            btn.FlatAppearance.BorderColor = BorderGray;
        else
            btn.FlatAppearance.BorderSize = 0;

        return btn;
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
            ReadOnly = false,
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
            RowTemplate = { Height = 54 },
            ColumnHeadersHeight = 40,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Segoe UI", 9F)
        };

        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        g.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 243, 246);
        g.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;

        g.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "colSelected",
            HeaderText = "",
            Width = 44,
            MinimumWidth = 44,
            Resizable = DataGridViewTriState.False,
            ReadOnly = false,
            TrueValue = true,
            FalseValue = false
        });
        
        g.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        AddCol(g, "colTicket",   "Mã phiếu mượn",  135);
        AddCol(g, "colReader",   "Độc giả",        185);
        AddCol(g, "colContact",  "Liên hệ",        220);
        AddCol(g, "colBook",     "Thông tin sách", 250);
        AddCol(g, "colBorrowed", "Ngày mượn",      120);
        AddCol(g, "colDue",      "Hạn trả",        110);
        AddCol(g, "colOverdue",  "Quá hạn",        110);
        AddCol(g, "colPenalty",  "Phạt tạm tính",  135);
        AddCol(g, "colNotice",   "Trạng thái nhắc",175);
        AddCol(g, "colActions",  "Thao tác",       120);

        foreach (DataGridViewColumn col in g.Columns)
        {
            col.ReadOnly = col.Name != "colSelected";
        }

        // Tự giãn thêm cột "Thông tin sách" và "Liên hệ" khi màn hình lớn
        void StretchWideColumns()
        {
            int fixedWidth = g.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible && c.Name is not ("colBook" or "colContact"))
                .Sum(c => c.MinimumWidth);

            int remaining = g.ClientSize.Width - fixedWidth - 4;
            if (remaining > 460)
            {
                g.Columns["colBook"]!.Width = (int)(remaining * 0.56);
                g.Columns["colContact"]!.Width = remaining - g.Columns["colBook"]!.Width;
            }
            else
            {
                g.Columns["colBook"]!.Width = 250;
                g.Columns["colContact"]!.Width = 210;
            }
        }
        g.Resize += (_, _) => StretchWideColumns();

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
    
    g.ReadOnly = false;
    foreach (DataGridViewColumn col in g.Columns)
    {
        col.ReadOnly = col.Name != "colSelected";
    }

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
        if (dgvOverdue.Rows.Count == 0) return;

        // Kết thúc edit nếu đang focus vào 1 ô checkbox bất kỳ
        dgvOverdue.EndEdit();

        _allSelected = !_allSelected;
        _isBulkUpdatingCheckbox = true;
        try
        {
            foreach (DataGridViewRow row in dgvOverdue.Rows)
            {
                row.Cells["colSelected"].Value = _allSelected;
                if (row.Tag is ManageOverdueBookInfo b)
                {
                    if (_allSelected) _selectedIds.Add(b.LoanCode);
                    else _selectedIds.Remove(b.LoanCode);
                }
            }
        }
        finally
        {
            _isBulkUpdatingCheckbox = false;
        }

        dgvOverdue.Refresh();
        UpdateNotifyButton();
    }
    private static void AddCol(DataGridView g, string name, string header, int width)
    {
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = header,
            Width = width,
            MinimumWidth = width
        });
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
            RenderRows();
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
    private void RenderRows()
    {
        dgvOverdue.Rows.Clear();

        foreach (var book in _filteredBooks)
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

        dgvOverdue.ClearSelection();
        
        _allSelected = dgvOverdue.Rows.Count > 0
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

            var state = _allSelected ? ButtonState.Checked : ButtonState.Normal;

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
        if (_isBulkUpdatingCheckbox) return;
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (dgvOverdue.Columns[e.ColumnIndex].Name != "colSelected") return;

        var row = dgvOverdue.Rows[e.RowIndex];
        if (row.Tag is not ManageOverdueBookInfo book) return;

        bool isChecked = Convert.ToBoolean(row.Cells["colSelected"].Value ?? false);
        if (isChecked) _selectedIds.Add(book.LoanCode);
        else _selectedIds.Remove(book.LoanCode);

        _allSelected = dgvOverdue.Rows.Count > 0
                       && dgvOverdue.Rows.Cast<DataGridViewRow>()
                           .All(r => Convert.ToBoolean(r.Cells["colSelected"].Value ?? false));

        dgvOverdue.InvalidateCell(e.ColumnIndex, -1);
        UpdateNotifyButton();
    }
    private async Task Grid_CellMouseClickAsync(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (e.Button != MouseButtons.Left) return;

        var book = dgvOverdue.Rows[e.RowIndex].Tag as ManageOverdueBookInfo;
        if (book == null) return;
        
        

        string colName = dgvOverdue.Columns[e.ColumnIndex].Name;

        if (colName == "colSelected")
        {
            var cell = dgvOverdue.Rows[e.RowIndex].Cells["colSelected"];
            bool current = Convert.ToBoolean(cell.Value ?? false);
            cell.Value = !current;
            dgvOverdue.EndEdit();
            return;
        }

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
            MessageBox.Show("Không có dữ liệu quá hạn để in.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Nếu người dùng có tick chọn thì ưu tiên in các dòng đã chọn, nếu không thì in toàn bộ danh sách đang lọc
        var listToPrint = _selectedIds.Count > 0
            ? _filteredBooks.Where(b => _selectedIds.Contains(b.LoanCode)).ToList()
            : _filteredBooks;

        using var previewForm = new OverduePrintPreviewForm.OverduePrintPreviewForm(listToPrint, 5000m);
        previewForm.ShowDialog(this);
    }

    private void PrintReminder(ManageOverdueBookInfo book)
    {
        using var previewForm = new OverduePrintPreviewForm.OverduePrintPreviewForm(new[] { book }, 5000m);
        previewForm.ShowDialog(this);
    }

    // =========================================================
    // TIỆN ÍCH
    // =========================================================
    private static Bitmap GlyphImage(string glyph, int size, Color color)
    {
        var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        using var f = new Font(IconFont, size, GraphicsUnit.Pixel);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        TextRenderer.DrawText(
            g,
            glyph,
            f,
            new Rectangle(0, 0, 16, 16),
            color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
        );
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