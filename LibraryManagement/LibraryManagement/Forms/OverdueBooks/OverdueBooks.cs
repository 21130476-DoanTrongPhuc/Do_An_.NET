namespace LibraryManagement.Forms.OverdueBooks;

using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;


public partial class OverdueBooks : UserControl
{
    // ====== Màu sắc ======
    private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
    private static readonly Color SoftRed    = Color.FromArgb(253, 234, 234);
    private static readonly Color SoftOrange = Color.FromArgb(254, 244, 226);
    private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
    private static readonly Color Green      = Color.FromArgb(34, 160, 90);
    private static readonly Color Red        = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    // Glyph (Segoe MDL2 Assets)
    private const string GSearch   = "\uE721";
    private const string GRefresh  = "\uE72C";
    private const string GMail     = "\uE715";
    private const string GExport   = "\uE896";
    private const string GPrint    = "\uE749";
    private const string GWarn     = "\uE7BA";
    private const string GPeople   = "\uE716";
    private const string GMoney    = "\uE8C7";
    private const string GReturn   = "\uE7A7";
    private const string GChevLeft = "\uE76B";
    private const string GChevRight= "\uE76C";
    private const string GCalendar = "\uE787";

    // ====== Tham số ======
    private const decimal PenaltyPerDay = 2000m;
    private const int PageSize = 20;

    // ====== Dữ liệu mẫu ======
    private sealed class OverdueRow
    {
        public string TicketId { get; set; } = "";
        public string ReaderId { get; set; } = "";
        public string ReaderName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string CopyId { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue => Math.Max(0, (DateTime.Today - DueDate).Days);
        public decimal Penalty => DaysOverdue * PenaltyPerDay;
        public string LastNotice { get; set; } = "Chưa nhắc";
        public bool Selected { get; set; }
    }

    private List<OverdueRow> _allRows = new();
    private List<OverdueRow> _filtered = new();
    private int _page = 1;
    private int _totalPages = 1;

    // ====== Control ======
    private Label lblKpiBooks = null!;
    private Label lblKpiReaders = null!;
    private Label lblKpiFine = null!;

    private TextBox txtSearch = null!;
    private ComboBox cboDaysFilter = null!;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private CheckBox chkUseDueRange = null!;

    private Button btnNotify = null!;
    private Button btnExport = null!;
    private Button btnPrint = null!;

    private DataGridView dgvOverdue = null!;
    private Label lblPageInfo = null!;
    private Button btnPrevPage = null!;
    private Button btnNextPage = null!;

    // ====== Sự kiện ra ngoài ======
    public event EventHandler? CloseRequested;
    public event EventHandler<OverdueRowEventArgs>? ReturnBookRequested;

    public OverdueBooks()
    {
        InitializeComponent();
        BuildUi();
        LoadSampleData();
        ApplyFilter();
    }

    // ============================================================
    //  DỮ LIỆU MẪU
    // ============================================================
    private void LoadSampleData()
    {
        var today = DateTime.Today;
        _allRows = new List<OverdueRow>
        {
            new() { TicketId="PM012", ReaderId="DG005", ReaderName="Nguyễn Minh Anh", Phone="0901 234 567", Email="minhanh@example.com", CopyId="BC-0012", Title="Nhà giả kim",        BorrowDate=today.AddDays(-40), DueDate=today.AddDays(-32) },
            new() { TicketId="PM018", ReaderId="DG021", ReaderName="Trần Hoàng Nam", Phone="0912 345 678", Email="hoangnam@example.com", CopyId="BC-0045", Title="Đắc nhân tâm",       BorrowDate=today.AddDays(-30), DueDate=today.AddDays(-25), LastNotice="Đã gửi mail 16/11" },
            new() { TicketId="PM021", ReaderId="DG033", ReaderName="Lê Thu Hà",      Phone="0987 654 321", Email="thuha@example.com",    CopyId="BC-0067", Title="Lược sử thời gian",  BorrowDate=today.AddDays(-25), DueDate=today.AddDays(-18) },
            new() { TicketId="PM025", ReaderId="DG041", ReaderName="Đỗ Hải Yến",     Phone="0903 111 222", Email="haiyen@example.com",   CopyId="BC-0081", Title="Mắt biếc",            BorrowDate=today.AddDays(-20), DueDate=today.AddDays(-12), LastNotice="Đã gửi SMS 14/11" },
            new() { TicketId="PM031", ReaderId="DG052", ReaderName="Phạm Quốc Bảo",  Phone="0918 777 888", Email="quocbao@example.com",  CopyId="BC-0093", Title="Sapiens",              BorrowDate=today.AddDays(-18), DueDate=today.AddDays(-9) },
            new() { TicketId="PM033", ReaderId="DG058", ReaderName="Vũ Thanh Tùng",  Phone="0909 222 333", Email="thanhtung@example.com",CopyId="BC-0105", Title="Tuổi trẻ đáng giá",   BorrowDate=today.AddDays(-15), DueDate=today.AddDays(-6) },
            new() { TicketId="PM037", ReaderId="DG063", ReaderName="Ngô Lan Phương", Phone="0932 444 555", Email="lanphuong@example.com",CopyId="BC-0117", Title="Cà phê cùng Tony",     BorrowDate=today.AddDays(-12), DueDate=today.AddDays(-4) },
            new() { TicketId="PM040", ReaderId="DG071", ReaderName="Bùi Anh Khoa",   Phone="0977 666 777", Email="anhkhoa@example.com",  CopyId="BC-0124", Title="Tư duy nhanh và chậm", BorrowDate=today.AddDays(-10), DueDate=today.AddDays(-2) },
            new() { TicketId="PM042", ReaderId="DG079", ReaderName="Hoàng Mai Linh", Phone="0968 999 000", Email="mailinh@example.com",  CopyId="BC-0136", Title="Dám bị ghét",          BorrowDate=today.AddDays(-8),  DueDate=today.AddDays(-1) },
            new() { TicketId="PM044", ReaderId="DG082", ReaderName="Đặng Quốc Huy",  Phone="0911 555 666", Email="quochuy@example.com",  CopyId="BC-0142", Title="Khéo ăn nói",          BorrowDate=today.AddDays(-6),  DueDate=today.AddDays(-1) },
        };
    }

    // ============================================================
    //  DỰNG GIAO DIỆN
    // ============================================================
    private void BuildUi()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(240, 242, 245);
        Font = new Font("Segoe UI", 9F);

        Controls.Add(BuildTableArea());   // Fill (add đầu)
        Controls.Add(BuildToolbar());      // Top
        Controls.Add(BuildKpiArea());      // Top (trên cùng)

        ResumeLayout(true);
    }

    // ---------- 1. KPI SUMMARY CARDS ----------
    private Panel BuildKpiArea()
    {
        var host = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(12, 12, 12, 0) };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };
        for (int i = 0; i < 3; i++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        tbl.Controls.Add(KpiCard(GWarn,   Red,      "Tổng số cuốn sách quá hạn", "0", "cuốn",       0), 0, 0);
        tbl.Controls.Add(KpiCard(GPeople, Orange,   "Số độc giả đang trễ hạn",   "0", "độc giả",    1), 1, 0);
        tbl.Controls.Add(KpiCard(GMoney,  Primary,  "Tổng tiền phạt tạm tính",   "0 đ","ước tính",   2), 2, 0);

        host.Controls.Add(tbl);
        return host;
    }

    private Panel KpiCard(string glyph, Color accent, string caption, string value, string sub, int index)
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

        var icon = new Label
        {
            Text = glyph,
            Font = new Font(IconFont, 20F),
            ForeColor = accent,
            BackColor = Color.FromArgb(22, accent),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(56, 56),
            Location = new Point(16, 22)
        };
        card.Controls.Add(icon);

        card.Controls.Add(new Label
        {
            Text = caption,
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(86, 20)
        });

        var lblValue = new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = accent,
            AutoSize = true,
            Location = new Point(84, 42)
        };
        card.Controls.Add(lblValue);

        card.Controls.Add(new Label
        {
            Text = sub,
            Font = new Font("Segoe UI", 8F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(86 + 4, 84)
        });

        // Lưu label giá trị để cập nhật
        if (index == 0) lblKpiBooks = lblValue;
        else if (index == 1) lblKpiReaders = lblValue;
        else lblKpiFine = lblValue;

        return card;
    }

    // ---------- 2. FILTER & ACTION TOOLBAR ----------
    private Panel BuildToolbar()
    {
        var host = new Panel { Dock = DockStyle.Top, Height = 96, Padding = new Padding(12, 8, 12, 0) };

        var grpFilter = new GroupBox
        {
            Text = "Bộ lọc",
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 6, 12, 8)
        };

        // Hàng 1: Search + Days + Actions
        var row1 = new Panel { Dock = DockStyle.Top, Height = 38 };

        // Search
        txtSearch = new TextBox
        {
            PlaceholderText = "Tìm theo mã độc giả, tên độc giả, SĐT hoặc mã sách...",
            Location = new Point(6, 6),
            Width = 340,
            Font = new Font("Segoe UI", 9F)
        };
        txtSearch.TextChanged += (_, _) => { _page = 1; ApplyFilter(); };
        txtSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _page = 1; ApplyFilter(); }
        };
        row1.Controls.Add(txtSearch);

        // Days filter
        row1.Controls.Add(new Label
        {
            Text = "Quá hạn:",
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(360, 12)
        });
        cboDaysFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(424, 8),
            Width = 170,
            Font = new Font("Segoe UI", 9F)
        };
        cboDaysFilter.Items.AddRange(new object[]
        {
            "Tất cả", "Quá hạn 1–7 ngày", "Quá hạn 7–30 ngày", "Quá hạn trên 30 ngày"
        });
        cboDaysFilter.SelectedIndex = 0;
        cboDaysFilter.SelectedIndexChanged += (_, _) => { _page = 1; ApplyFilter(); };
        row1.Controls.Add(cboDaysFilter);

        // Due range filter (tùy chọn)
        chkUseDueRange = new CheckBox
        {
            Text = "Hạn trả:",
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(608, 12)
        };
        row1.Controls.Add(chkUseDueRange);

        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(684, 8),
            Width = 110,
            Enabled = false,
            Value = DateTime.Today.AddDays(-60)
        };
        row1.Controls.Add(dtpFrom);

        row1.Controls.Add(new Label
        {
            Text = "–",
            AutoSize = true,
            ForeColor = TextGray,
            Location = new Point(798, 12)
        });

        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(814, 8),
            Width = 110,
            Enabled = false,
            Value = DateTime.Today
        };
        row1.Controls.Add(dtpTo);

        chkUseDueRange.CheckedChanged += (_, _) =>
        {
            dtpFrom.Enabled = dtpTo.Enabled = chkUseDueRange.Checked;
            _page = 1;
            ApplyFilter();
        };
        dtpFrom.ValueChanged += (_, _) => { _page = 1; ApplyFilter(); };
        dtpTo.ValueChanged += (_, _) => { _page = 1; ApplyFilter(); };

        // Action buttons bên phải
        var right = new Panel { Dock = DockStyle.Right, Width = 470 };
        row1.Controls.Add(right);

        btnNotify = new Button
        {
            Text = "  Gửi thông báo (0)",
            Size = new Size(190, 32),
            Location = new Point(0, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Orange,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GMail, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnNotify.FlatAppearance.BorderSize = 0;
        btnNotify.Click += (_, _) => SendNotifications();

        btnExport = new Button
        {
            Text = "  Xuất .CSV",
            Size = new Size(130, 32),
            Location = new Point(198, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            Image = GlyphImage(GExport, 14, Color.Black),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnExport.FlatAppearance.BorderColor = BorderGray;
        btnExport.Click += (_, _) => ExportCsv();

        btnPrint = new Button
        {
            Text = "  In danh sách đòi sách",
            Size = new Size(210, 32),
            Location = new Point(336, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GPrint, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += (_, _) => PrintReminderList();

        right.Controls.Add(btnNotify);
        right.Controls.Add(btnExport);
        right.Controls.Add(btnPrint);

        grpFilter.Controls.Add(row1);
        host.Controls.Add(grpFilter);
        return host;
    }

    // ---------- 3. DATA TABLE + PAGINATION ----------
    private GroupBox BuildTableArea()
    {
        var grp = new GroupBox
        {
            Text = "Danh sách sách quá hạn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10),
            Margin = new Padding(12)
        };

        // Pagination footer (Bottom - add đầu)
        var pager = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(0, 6, 0, 0) };
        btnPrevPage = new Button
        {
            Text = "  Trước",
            Size = new Size(90, 28),
            Location = new Point(0, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 8.5F),
            Image = GlyphImage(GChevLeft, 12, Color.Black),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        btnPrevPage.FlatAppearance.BorderColor = BorderGray;
        btnPrevPage.Click += (_, _) =>
        {
            if (_page > 1) { _page--; RenderPage(); }
        };
        pager.Controls.Add(btnPrevPage);

        lblPageInfo = new Label
        {
            Text = "Trang 1 / 1",
            AutoSize = true,
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            Location = new Point(100, 10)
        };
        pager.Controls.Add(lblPageInfo);

        btnNextPage = new Button
        {
            Text = "  Sau",
            Size = new Size(90, 28),
            Location = new Point(200, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 8.5F),
            Image = GlyphImage(GChevRight, 12, Color.Black),
            ImageAlign = ContentAlignment.MiddleRight,
            TextImageRelation = TextImageRelation.TextBeforeImage
        };
        btnNextPage.FlatAppearance.BorderColor = BorderGray;
        btnNextPage.Click += (_, _) =>
        {
            if (_page < _totalPages) { _page++; RenderPage(); }
        };
        pager.Controls.Add(btnNextPage);

        // Grid
        dgvOverdue = BuildGrid();

        grp.Controls.Add(dgvOverdue);
        grp.Controls.Add(pager);
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
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            MultiSelect = false,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 40 },
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

        // Cột
        g.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "colCheck", HeaderText = "", FillWeight = 24, FlatStyle = FlatStyle.Standard
        });
        AddTextCol(g, "colTicket",  "Mã phiếu",   55);
        AddTextCol(g, "colReader",  "Mã ĐG",      48);
        AddTextCol(g, "colName",    "Họ tên",     100);
        AddTextCol(g, "colContact", "Liên hệ",    130);
        AddTextCol(g, "colCopy",    "Mã vạch",    60);
        AddTextCol(g, "colTitle",   "Tên sách",   140);
        AddTextCol(g, "colBorrow",  "Ngày mượn",  65);
        AddTextCol(g, "colDue",     "Hạn trả",    65);
        AddTextCol(g, "colOverdue", "Quá hạn",    60);
        AddTextCol(g, "colFine",    "Phạt tạm tính", 75);
        AddTextCol(g, "colNotice",  "Trạng thái nhắc", 100);
        AddTextCol(g, "colActions", "Thao tác",   90);

        // Style cột
        g.Columns["colTicket"]!.DefaultCellStyle.ForeColor = Primary;
        g.Columns["colTicket"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        g.Columns["colFine"]!.DefaultCellStyle.ForeColor = Red;
        g.Columns["colFine"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        // Sự kiện
        g.CellPainting += DgvOverdue_CellPainting;
        g.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (g.IsCurrentCellDirty && g.CurrentCell is DataGridViewCheckBoxCell)
                g.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        g.CellValueChanged += DgvOverdue_CellValueChanged;
        g.DataError += (_, e) => e.ThrowException = false;
        g.CellContentClick += DgvOverdue_CellContentClick;

        // Header checkbox chọn tất cả
        g.Columns["colCheck"]!.HeaderCell = new DataGridViewCheckBoxHeaderCell();
        ((DataGridViewCheckBoxHeaderCell)g.Columns["colCheck"]!.HeaderCell).OnCheckBoxClicked +=
            (_, _) => ToggleAllRows();

        return g;
    }

    private static void AddTextCol(DataGridView g, string name, string header, float weight)
    {
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name, HeaderText = header, FillWeight = weight
        });
    }

    // ============================================================
    //  RENDER TRANG
    // ============================================================
    private void ApplyFilter()
    {
        string q = txtSearch.Text.Trim().ToLowerInvariant();
        int daysIdx = cboDaysFilter.SelectedIndex;
        bool useRange = chkUseDueRange.Checked;
        var from = dtpFrom.Value.Date;
        var to = dtpTo.Value.Date;

        _filtered = _allRows.Where(r =>
        {
            // Search
            if (!string.IsNullOrEmpty(q))
            {
                string hay = $"{r.ReaderId} {r.ReaderName} {r.Phone} {r.CopyId}".ToLowerInvariant();
                if (!hay.Contains(q)) return false;
            }
            // Days filter
            if (daysIdx == 1 && (r.DaysOverdue < 1 || r.DaysOverdue > 7)) return false;
            if (daysIdx == 2 && (r.DaysOverdue < 7 || r.DaysOverdue > 30)) return false;
            if (daysIdx == 3 && r.DaysOverdue <= 30) return false;
            // Due range
            if (useRange && (r.DueDate.Date < from || r.DueDate.Date > to)) return false;
            return true;
        })
        .OrderByDescending(r => r.DaysOverdue)
        .ThenBy(r => r.ReaderName)
        .ToList();

        _totalPages = Math.Max(1, (int)Math.Ceiling(_filtered.Count / (double)PageSize));
        if (_page > _totalPages) _page = _totalPages;

        RenderPage();
        RecalcKpi();
        UpdateNotifyButtonText();
    }

    private void RenderPage()
    {
        dgvOverdue.Rows.Clear();
        var slice = _filtered.Skip((_page - 1) * PageSize).Take(PageSize).ToList();

        foreach (var r in slice)
        {
            int idx = dgvOverdue.Rows.Add();
            var row = dgvOverdue.Rows[idx];
            row.Tag = r;
            row.Cells["colCheck"].Value = r.Selected;
            row.Cells["colTicket"].Value = r.TicketId;
            row.Cells["colReader"].Value = r.ReaderId;
            row.Cells["colName"].Value = r.ReaderName;
            row.Cells["colContact"].Value = $"{r.Phone}  ·  {r.Email}";
            row.Cells["colCopy"].Value = r.CopyId;
            row.Cells["colTitle"].Value = r.Title;
            row.Cells["colBorrow"].Value = r.BorrowDate.ToString("dd/MM/yyyy");
            row.Cells["colDue"].Value = r.DueDate.ToString("dd/MM/yyyy");
            row.Cells["colOverdue"].Value = r.DaysOverdue;
            row.Cells["colFine"].Value = r.Penalty.ToString("N0") + " đ";
            row.Cells["colNotice"].Value = r.LastNotice;

            // Cột thao tác: text để hiển thị icon (paint custom)
            row.Cells["colActions"].Value = "✉  ↺";
        }

        lblPageInfo.Text = $"Trang {_page} / {_totalPages}  ·  {_filtered.Count} dòng";
        btnPrevPage.Enabled = _page > 1;
        btnNextPage.Enabled = _page < _totalPages;
        dgvOverdue.ClearSelection();
    }

    // ============================================================
    //  KPI + ĐẾM CHỌN
    // ============================================================
    private void RecalcKpi()
    {
        int totalBooks = _filtered.Count;
        int readers = _filtered.Select(r => r.ReaderId).Distinct().Count();
        decimal totalFine = _filtered.Sum(r => r.Penalty);

        lblKpiBooks.Text = totalBooks.ToString("N0");
        lblKpiReaders.Text = readers.ToString("N0");
        lblKpiFine.Text = totalFine.ToString("N0") + " đ";
    }

    private void UpdateNotifyButtonText()
    {
        int selected = _filtered.Count(r => r.Selected);
        btnNotify.Text = $"  Gửi thông báo ({selected})";
        btnNotify.Enabled = selected > 0;
    }

    // ============================================================
    //  PAINT BADGE / ICON
    // ============================================================
    private void DgvOverdue_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics == null) return;
        string colName = dgvOverdue.Columns[e.ColumnIndex].Name;

        if (colName == "colOverdue")
        {
            e.Paint(e.CellBounds,
                DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            int days = e.Value is int n ? n : 0;
            Color c = days > 30 ? Red : (days > 7 ? Orange : Color.FromArgb(190, 90, 30));
            DrawPill(e.Graphics, e.CellBounds, days + " ngày", c);
            e.Handled = true;
        }
        else if (colName == "colActions")
        {
            e.Paint(e.CellBounds,
                DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            DrawActionIcons(e.Graphics, e.CellBounds);
            e.Handled = true;
        }
        else if (colName == "colNotice")
        {
            string text = e.Value?.ToString() ?? "";
            if (text == "Chưa nhắc") return; // để grid tự vẽ
            e.Paint(e.CellBounds,
                DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            DrawPill(e.Graphics, e.CellBounds, text, Green, fontSize: 8F);
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
        using (var pen = new Pen(Color.FromArgb(150, color)))
        {
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }
        TextRenderer.DrawText(g, text, font, rect, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    private void DrawActionIcons(Graphics g, Rectangle bounds)
    {
        var font = new Font(IconFont, 14F);
        int cx = bounds.X + bounds.Width / 2;

        // Icon 1: Gửi mail (bên trái)
        var icon1Rect = new Rectangle(cx - 34, bounds.Y + (bounds.Height - 26) / 2, 28, 26);
        DrawIconButton(g, icon1Rect, GMail, Primary);

        // Icon 2: Thu hồi / Trả sách (bên phải)
        var icon2Rect = new Rectangle(cx + 6, bounds.Y + (bounds.Height - 26) / 2, 28, 26);
        DrawIconButton(g, icon2Rect, GReturn, Green);
    }

    private void DrawIconButton(Graphics g, Rectangle rect, string glyph, Color color)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var path = RoundedRect(rect, 6))
        using (var fill = new SolidBrush(Color.FromArgb(18, color)))
        using (var pen = new Pen(Color.FromArgb(120, color)))
        {
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }
        using var f = new Font(IconFont, 12F);
        TextRenderer.DrawText(g, glyph, f, rect, color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    // ============================================================
    //  TƯƠNG TÁC BẢNG
    // ============================================================
    private void DgvOverdue_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (dgvOverdue.Columns[e.ColumnIndex].Name != "colCheck") return;

        if (dgvOverdue.Rows[e.RowIndex].Tag is OverdueRow row)
        {
            row.Selected = Convert.ToBoolean(dgvOverdue.Rows[e.RowIndex].Cells["colCheck"].Value ?? false);
        }
        UpdateNotifyButtonText();
    }

    private void DgvOverdue_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (dgvOverdue.Columns[e.ColumnIndex].Name != "colActions") return;

        var cellRect = dgvOverdue.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
        var mouse = dgvOverdue.PointToClient(Cursor.Position);
        int relX = mouse.X - cellRect.X;
        int cx = cellRect.Width / 2;

        var row = dgvOverdue.Rows[e.RowIndex].Tag as OverdueRow;
        if (row == null) return;

        if (relX < cx) // Gửi mail lẻ
        {
            SendNotification(row);
        }
        else // Thu hồi / Trả sách
        {
            ReturnBookRequested?.Invoke(this, new OverdueRowEventArgs(row.TicketId, row.ReaderId));
        }
    }

    private void ToggleAllRows()
    {
        bool anyUnchecked = dgvOverdue.Rows.Cast<DataGridViewRow>()
            .Any(r => !Convert.ToBoolean(r.Cells["colCheck"].Value ?? false));

        foreach (DataGridViewRow vr in dgvOverdue.Rows)
        {
            vr.Cells["colCheck"].Value = anyUnchecked;
            if (vr.Tag is OverdueRow or) or.Selected = anyUnchecked;
        }
        UpdateNotifyButtonText();
    }

    // ============================================================
    //  HÀNH ĐỘNG
    // ============================================================
    private void SendNotifications()
    {
        var selected = _filtered.Where(r => r.Selected).ToList();
        if (selected.Count == 0) return;

        if (MessageBox.Show($"Gửi thông báo nhắc nhở tới {selected.Count} độc giả?\n" +
                            "Kênh: Email + SMS (nếu có).",
                "Gửi thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            != DialogResult.Yes) return;

        // TODO: NotificationService.Send(...)
        string stamp = DateTime.Now.ToString("dd/MM");
        foreach (var r in selected)
            r.LastNotice = $"Đã gửi mail {stamp}";

        ApplyFilter();
        MessageBox.Show($"Đã gửi {selected.Count} thông báo.", "LIBRA",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SendNotification(OverdueRow row)
    {
        if (MessageBox.Show($"Gửi thông báo nhắc nhở tới:\n{row.ReaderName} ({row.Phone})\n" +
                            $"Sách: {row.Title}\nQuá hạn: {row.DaysOverdue} ngày",
                "Gửi thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            != DialogResult.Yes) return;

        row.LastNotice = $"Đã gửi mail {DateTime.Now:dd/MM}";
        ApplyFilter();
    }

    private void ExportCsv()
    {
        if (_filtered.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất.", "Xuất CSV",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "CSV file (*.csv)|*.csv",
            FileName = $"SachQuaHan_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Mã phiếu,Mã ĐG,Họ tên,SĐT,Email,Mã vạch,Tên sách,Ngày mượn,Hạn trả,Quá hạn (ngày),Phạt tạm tính,Trạng thái nhắc");
            foreach (var r in _filtered)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Csv(r.TicketId), Csv(r.ReaderId), Csv(r.ReaderName), Csv(r.Phone), Csv(r.Email),
                    Csv(r.CopyId), Csv(r.Title), r.BorrowDate.ToString("dd/MM/yyyy"),
                    r.DueDate.ToString("dd/MM/yyyy"), r.DaysOverdue.ToString(),
                    r.Penalty.ToString("N0"), Csv(r.LastNotice)
                }));
            }
            File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show("Xuất CSV thành công!", "LIBRA",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi xuất file:\n" + ex.Message, "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string Csv(string s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";

    private void PrintReminderList()
    {
        // Mở form xem trước in (khổ A4)
        using var preview = new OverduePrintPreviewForm.OverduePrintPreviewForm(_filtered, PenaltyPerDay);
        preview.ShowDialog(this);
    }

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

    // ============================================================
    //  HEADER CHECKBOX
    // ============================================================
    private sealed class DataGridViewCheckBoxHeaderCell : DataGridViewColumnHeaderCell
    {
        public event EventHandler? OnCheckBoxClicked;
        private bool _checked;

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates dataGridViewElementState,
            object? value, object? formattedValue, string? errorText,
            DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState,
                value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            const int size = 14;
            var rect = new Rectangle(
                cellBounds.X + (cellBounds.Width - size) / 2,
                cellBounds.Y + (cellBounds.Height - size) / 2,
                size, size);

            ControlPaint.DrawCheckBox(graphics, rect, _checked ? ButtonState.Checked : ButtonState.Normal);
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            _checked = !_checked;
            OnCheckBoxClicked?.Invoke(this, EventArgs.Empty);
            DataGridView?.InvalidateCell(this);
            base.OnMouseClick(e);
        }
    }
}

public sealed class OverdueRowEventArgs : EventArgs
{
    public string TicketId { get; }
    public string ReaderId { get; }
    public OverdueRowEventArgs(string ticketId, string readerId)
    {
        TicketId = ticketId;
        ReaderId = readerId;
    }
}
