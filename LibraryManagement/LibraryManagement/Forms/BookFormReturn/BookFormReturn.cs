using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Services;
using System.Drawing.Drawing2D;

namespace LibraryManagement.Forms.BookFormReturn;

public partial class BookFormReturn : UserControl
{
    private readonly ReturnedBookServiceImpl _returnBookService;
    private ReturnedBookReaderInfor? _currentReader;

    // ====== Màu sắc (đồng bộ LibrarianFormMain / OverdueBooks) ======
    private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
    private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
    private static readonly Color Green      = Color.FromArgb(34, 160, 90);
    private static readonly Color Red        = Color.FromArgb(200, 40, 40);
    private static readonly Color BgPage     = Color.FromArgb(240, 242, 245);
    private const string IconFont = "Segoe MDL2 Assets";

    // Glyph
    private const string GSearch  = "\uE721";
    private const string GBook    = "\uE82D";
    private const string GPeople  = "\uE716";
    private const string GMoney   = "\uE8C7";
    private const string GRefresh = "\uE72C";
    private const string GCheck   = "\uE73E";
    private const string GPrint   = "\uE749";
    private const string GWarn    = "\uE7BA";

    private const decimal PenaltyPerDay = 2000m;

    // ====== Controls ======
    private Label lblKpiBorrowing = null!;
    private Label lblKpiSelected  = null!;
    private Label lblKpiFine      = null!;
    private Label lblDate         = null!;

    private TextBox  txtSearch       = null!;
    private ComboBox cboStatusFilter = null!;

    // Panel chứa thông tin độc giả dạng flow các badge (trong Toolbar)
    private FlowLayoutPanel flowReaderInfo = null!;

    private DataGridView dgvBooks = null!;

    private Button btnPrevPage = null!;
    private Button btnNextPage = null!;
    private Label  lblPageInfo = null!;

    private Label lblTotalBooks     = null!;
    private Label lblTotalLateFee   = null!;
    private Label lblTotalDamageFee = null!;
    private Label lblGrandTotal     = null!;
    private RadioButton rdoPaid = null!;
    private RadioButton rdoDebt = null!;

    // Phân trang client-side (giữ nguyên nghiệp vụ, chỉ để hiển thị đẹp)
    private const int PageSize = 20;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private List<DataGridViewRow> _allRowsBuffer = new();
    private List<ReturnedBookInfor> _allBooks = new();

    public event EventHandler? CloseRequested;

    public BookFormReturn(ReturnedBookServiceImpl returnBookService)
    {
        InitializeComponent();
        _returnBookService = returnBookService;
        BuildUi();
    }

    // =========================================================
    // BUILD UI — cấu trúc giống OverdueBooks
    // =========================================================
    private void BuildUi()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        BackColor = BgPage;
        Font = new Font("Segoe UI", 9F);

        // add ngược từ dưới lên
        Controls.Add(BuildTableArea());    // Fill
        Controls.Add(BuildFooter());       // Bottom (footer tổng kết)
        Controls.Add(BuildPager());        // Bottom (pager)
        Controls.Add(BuildToolbar());      // Top
        Controls.Add(BuildFilterBar());    // Top
        Controls.Add(BuildKpiArea());      // Top
        Controls.Add(BuildHeader());       // Top (trên cùng)

        ResumeLayout(true);
    }

    // ---------------------------------------------------------
    // HEADER (giống OverdueBooks)
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
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

        pnl.Controls.Add(new Label
        {
            Text = "NHẬN TRẢ SÁCH",
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
        string[] days = { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
        var d = DateTime.Now;
        lblDate.Text = $"{days[(int)d.DayOfWeek]}, ngày {d:dd} tháng {d:MM} năm {d:yyyy}";
        pnl.Controls.Add(lblDate);

        var btnClose = new Button
        {
            Text = "✕  Đóng",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Size = new Size(96, 32),
            Font = new Font("Segoe UI", 8.5F),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnClose.FlatAppearance.BorderColor = BorderGray;
        btnClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        pnl.Controls.Add(btnClose);
        pnl.Resize += (_, _) => btnClose.Location = new Point(pnl.Width - btnClose.Width - 12, 14);
        btnClose.Location = new Point(pnl.Width - btnClose.Width - 12, 14);

        return pnl;
    }

    // ---------------------------------------------------------
    // KPI (3 thẻ, dùng TableLayoutPanel chia đều)
    // ---------------------------------------------------------
    private Panel BuildKpiArea()
    {
        var host = new Panel
        {
            Dock = DockStyle.Top,
            Height = 118,
            Padding = new Padding(12, 12, 12, 0)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };
        for (int i = 0; i < 3; i++)
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        table.Controls.Add(CreateKpiCard(GPeople, "Sách đang mượn", Primary, out lblKpiBorrowing), 0, 0);
        table.Controls.Add(CreateKpiCard(GCheck, "Sách chọn trả", Green, out lblKpiSelected), 1, 0);
        table.Controls.Add(CreateKpiCard(GMoney, "Tổng phạt tạm tính", Red, out lblKpiFine), 2, 0);

        host.Controls.Add(table);
        return host;
    }

    private Panel CreateKpiCard(string glyph, string caption, Color accent, out Label valueLabel)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(6, 0, 6, 0),
            Padding = new Padding(14, 12, 14, 12)
        };
        card.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

        card.Controls.Add(new Label
        {
            Text = glyph,
            Font = new Font(IconFont, 18F),
            ForeColor = accent,
            BackColor = Color.FromArgb(22, accent),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(50, 50),
            Location = new Point(12, 22)
        });

        card.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            ForeColor = Color.FromArgb(60, 60, 60),
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(74, 14)
        });

        valueLabel = new Label
        {
            Text = "0",
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = accent,
            Location = new Point(72, 32)
        };
        card.Controls.Add(valueLabel);

        return card;
    }

    // ---------------------------------------------------------
    // FILTER BAR (giống OverdueBooks: search + lọc trạng thái)
    // ---------------------------------------------------------
    private Panel BuildFilterBar()
    {
        var host = new Panel
        {
            Dock = DockStyle.Top,
            Height = 54,
            Padding = new Padding(12, 8, 12, 4),
            BackColor = BgPage
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F)); // search
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // label
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F)); // status
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // filler
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // filler
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = Font,
            PlaceholderText = "Tìm mã độc giả, mã phiếu mượn hoặc quét mã vạch...",
            Margin = new Padding(0, 8, 6, 8)
        };
        txtSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { DoSearch(); e.SuppressKeyPress = true; }
        };

        cboStatusFilter = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = Font,
            Margin = new Padding(3, 8, 3, 8)
        };
        cboStatusFilter.Items.AddRange(new object[] { "Tất cả", "Có phạt trễ", "Không phạt", "Sách hư hỏng" });
        cboStatusFilter.SelectedIndex = 0;
        cboStatusFilter.SelectedIndexChanged += (_, _) => ApplyStatusFilter();

        var btnSearch = new Button
        {
            Text = "  Tra cứu",
            Width = 110,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GSearch, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Margin = new Padding(4, 8, 0, 8),
            Cursor = Cursors.Hand
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += (_, _) => DoSearch();

        tbl.Controls.Add(txtSearch, 0, 0);
        tbl.Controls.Add(btnSearch, 1, 0);
        tbl.Controls.Add(new Label
        {
            Text = "Lọc:",
            AutoSize = true,
            ForeColor = TextGray,
            Margin = new Padding(8, 10, 2, 0)
        }, 2, 0);
        // cboStatusFilter đè lên label "Lọc:" — dùng flow riêng
        tbl.Controls.RemoveAt(tbl.Controls.Count - 1);

        var filterFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };
        filterFlow.Controls.Add(new Label
        {
            Text = "Lọc:",
            AutoSize = true,
            ForeColor = TextGray,
            Margin = new Padding(8, 10, 2, 0)
        });
        filterFlow.Controls.Add(cboStatusFilter);
        tbl.Controls.Add(filterFlow, 2, 0);
        tbl.SetColumnSpan(filterFlow, 3);

        host.Controls.Add(tbl);
        return host;
    }

    // ---------------------------------------------------------
    // TOOLBAR (giống OverdueBooks: hàng thao tác + info reader)
    // ---------------------------------------------------------
    private Panel BuildToolbar()
    {
        var host = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            Padding = new Padding(12, 4, 12, 8),
            BackColor = BgPage
        };
        host.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(BorderGray), 0, host.Height - 1, host.Width, host.Height - 1);

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // icon + label "Độc giả:"
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // info flow
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // action buttons
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Cột 0: label cố định
        var lblReader = new Label
        {
            Text = "Độc giả:",
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Margin = new Padding(0, 22, 8, 0)
        };
        tbl.Controls.Add(lblReader, 0, 0);

        // Cột 1: flow badge thông tin độc giả
        flowReaderInfo = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Margin = new Padding(0, 8, 8, 8)
        };
        tbl.Controls.Add(flowReaderInfo, 1, 0);

        // Cột 2: nút Làm mới
        var btnReset = CreateButton("  Làm mới", Color.White, Color.Black, 100, GRefresh);
        btnReset.Margin = new Padding(0, 8, 0, 8);
        btnReset.Click += (_, _) => ResetForm();
        tbl.Controls.Add(btnReset, 2, 0);

        host.Controls.Add(tbl);
        return host;
    }

    // ---------------------------------------------------------
    // TABLE AREA (giữ nguyên cột nghiệp vụ)
    // ---------------------------------------------------------
    private Panel BuildTableArea()
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 4, 12, 0)
        };

        var group = new GroupBox
        {
            Text = "Danh sách sách đang mượn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10)
        };

        dgvBooks = new DataGridView
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
        dgvBooks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvBooks.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        dgvBooks.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 243, 246);
        dgvBooks.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        dgvBooks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
        dgvBooks.DefaultCellStyle.SelectionForeColor = Color.Black;

        // === GIỮ NGUYÊN CÁC CỘT NGHIỆP VỤ ===
        dgvBooks.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "colCheck", HeaderText = "", FillWeight = 28, FlatStyle = FlatStyle.Standard
        });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLoanDetailId", Visible = false });

        AddTextCol(dgvBooks, "colTicket", "Mã phiếu", 55);
        AddTextCol(dgvBooks, "colCopyCode", "Mã vạch", 65);
        AddTextCol(dgvBooks, "colTitle", "Tên sách", 150);
        AddTextCol(dgvBooks, "colCondBorrow", "Tình trạng lúc mượn", 100);
        AddTextCol(dgvBooks, "colDue", "Hạn trả", 65);
        AddTextCol(dgvBooks, "colReturn", "Ngày trả", 65);
        AddTextCol(dgvBooks, "colLate", "Số ngày trễ", 55);
        dgvBooks.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "colCondReturn",
            HeaderText = "Tình trạng khi trả",
            FillWeight = 100,
            FlatStyle = FlatStyle.Flat,
            DataSource = new[] { "Bình thường", "Hư hỏng", "Hư hỏng nặng", "Mất sách" }
        });
        AddTextCol(dgvBooks, "colLateFee", "Phạt trễ hạn", 75);
        AddTextCol(dgvBooks, "colDamageFee", "Phạt hư hỏng/mất", 85);
        AddTextCol(dgvBooks, "colNote", "Ghi chú", 100);

        dgvBooks.Columns["colTicket"]!.DefaultCellStyle.ForeColor = Primary;
        dgvBooks.Columns["colTicket"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvBooks.Columns["colLate"]!.DefaultCellStyle.ForeColor = Red;

        dgvBooks.CellPainting += DgvBooks_CellPainting;
        dgvBooks.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (dgvBooks.IsCurrentCellDirty && dgvBooks.CurrentCell is DataGridViewCheckBoxCell)
                dgvBooks.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        dgvBooks.CellValueChanged += DgvBooks_CellValueChanged;
        dgvBooks.DataError += (_, e) => e.ThrowException = false;
        dgvBooks.CellEndEdit += (_, e) =>
        {
            if (e.ColumnIndex == dgvBooks.Columns["colDamageFee"]!.Index)
                CommitDamageFee(e.RowIndex);
        };

        dgvBooks.Columns["colCheck"]!.HeaderCell = new DataGridViewCheckBoxHeaderCell();
        ((DataGridViewCheckBoxHeaderCell)dgvBooks.Columns["colCheck"]!.HeaderCell).OnCheckBoxClicked +=
            (_, _) => ToggleAllRows();

        group.Controls.Add(dgvBooks);
        host.Controls.Add(group);
        return host;
    }

    private static void AddTextCol(DataGridView g, string name, string header, float weight)
    {
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name, HeaderText = header, FillWeight = weight
        });
    }

    // ---------------------------------------------------------
    // PAGER (giống OverdueBooks)
    // ---------------------------------------------------------
    private Panel BuildPager()
    {
        var host = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            Padding = new Padding(12, 4, 12, 8),
            BackColor = BgPage
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // prev
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // info
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // next
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // refresh
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        btnPrevPage = CreateButton("‹  Trước", Color.White, Color.Black, 95);
        btnPrevPage.Click += (_, _) =>
        {
            if (_currentPage > 1) { _currentPage--; RenderPage(); }
        };

        lblPageInfo = new Label
        {
            Text = "Trang 1/1 - 0 dòng",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 9F)
        };

        btnNextPage = CreateButton("Sau  ›", Color.White, Color.Black, 95);
        btnNextPage.Click += (_, _) =>
        {
            if (_currentPage < _totalPages) { _currentPage++; RenderPage(); }
        };

        var btnRefresh = CreateButton("Làm mới", Color.White, Color.Black, 90);
        btnRefresh.Click += (_, _) =>
        {
            if (_currentReader != null) { txtSearch.Text = _currentReader.UserCode; DoSearch(); }
            else ResetForm();
        };

        tbl.Controls.Add(btnPrevPage, 0, 0);
        tbl.Controls.Add(lblPageInfo, 1, 0);
        tbl.Controls.Add(btnNextPage, 2, 0);
        tbl.Controls.Add(btnRefresh, 3, 0);

        host.Controls.Add(tbl);
        return host;
    }

    // ---------------------------------------------------------
    // FOOTER (tổng kết + 3 nút hành động — giữ nguyên nghiệp vụ)
    // ---------------------------------------------------------
    private Panel BuildFooter()
    {
        var host = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 140,
            Padding = new Padding(12, 4, 12, 12)
        };

        var grpSummary = new GroupBox
        {
            Text = "Tổng kết",
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 8, 14, 10)
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // summary
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));  // actions
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // LEFT: Tổng kết dạng TableLayout 2 cột
        var leftTbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Margin = new Padding(0)
        };
        leftTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        leftTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (int i = 0; i < 4; i++)
            leftTbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        leftTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        lblTotalBooks     = AddSummaryRow(leftTbl, "Tổng số sách trả:", "0", 0);
        lblTotalLateFee   = AddSummaryRow(leftTbl, "Tổng phạt trễ hạn:", "0 đ", 1);
        lblTotalDamageFee = AddSummaryRow(leftTbl, "Tổng phạt hư hỏng/mất:", "0 đ", 2);

        var pnlGrand = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 4, 0, 0)
        };
        lblGrandTotal = new Label
        {
            Text = "Tổng thanh toán:  0 đ",
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Red,
            Margin = new Padding(0, 4, 24, 0)
        };
        pnlGrand.Controls.Add(lblGrandTotal);

        rdoPaid = new RadioButton
        {
            Text = "Đã thu tiền", AutoSize = true, Checked = true,
            Margin = new Padding(0, 8, 8, 0)
        };
        rdoDebt = new RadioButton
        {
            Text = "Ghi nợ phạt", AutoSize = true, Margin = new Padding(0, 8, 0, 0)
        };
        pnlGrand.Controls.Add(rdoPaid);
        pnlGrand.Controls.Add(rdoDebt);

        leftTbl.Controls.Add(pnlGrand, 0, 3);
        leftTbl.SetColumnSpan(pnlGrand, 2);

        // RIGHT: 3 nút xếp dọc
        var rightTbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(8, 0, 0, 0)
        };
        rightTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        rightTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        rightTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

        var btnReset = CreateButton("  Làm mới", Color.White, Color.Black, 0, GRefresh);
        btnReset.Dock = DockStyle.Fill;
        btnReset.Margin = new Padding(0, 4, 0, 4);
        btnReset.Click += (_, _) => ResetForm();

        var btnConfirm = CreateButton("  Xác nhận trả sách", Green, Color.White, 0, GCheck);
        btnConfirm.Dock = DockStyle.Fill;
        btnConfirm.Margin = new Padding(0, 4, 0, 4);
        btnConfirm.Click += (_, _) => _ = ConfirmReturnAsync(false);

        var btnPrint = CreateButton("  Xác nhận & In phiếu", Primary, Color.White, 0, GPrint);
        btnPrint.Dock = DockStyle.Fill;
        btnPrint.Margin = new Padding(0, 4, 0, 4);
        btnPrint.Click += (_, _) => _ = ConfirmReturnAsync(true);

        rightTbl.Controls.Add(btnReset, 0, 0);
        rightTbl.Controls.Add(btnConfirm, 0, 1);
        rightTbl.Controls.Add(btnPrint, 0, 2);

        tbl.Controls.Add(leftTbl, 0, 0);
        tbl.Controls.Add(rightTbl, 1, 0);

        grpSummary.Controls.Add(tbl);
        host.Controls.Add(grpSummary);
        return host;
    }

    private Label AddSummaryRow(TableLayoutPanel parent, string caption, string value, int row)
    {
        parent.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = false, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = TextGray, Font = new Font("Segoe UI", 9F),
            Margin = new Padding(0)
        }, 0, row);

        var lbl = new Label
        {
            Text = value,
            AutoSize = false, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F), ForeColor = Color.Black,
            Margin = new Padding(0)
        };
        parent.Controls.Add(lbl, 1, row);
        return lbl;
    }

    // =========================================================
    // READER INFO (flow badges trong toolbar)
    // =========================================================
    private void LoadReaderInfo(ReturnedBookReaderInfor reader)
    {
        _currentReader = reader;
        flowReaderInfo.SuspendLayout();
        flowReaderInfo.Controls.Clear();

        AddReaderBadge("👤 " + reader.FullName, Primary);
        AddReaderBadge("🆔 " + reader.UserCode, TextGray);
        AddReaderBadge("✉ " + reader.Email, TextGray);
        AddReaderBadge("📞 " + reader.Phone, TextGray);

        flowReaderInfo.ResumeLayout(true);
    }

    private void AddReaderBadge(string text, Color color)
    {
        var lbl = new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = color,
            BackColor = Color.FromArgb(22, color),
            Padding = new Padding(10, 6, 10, 6),
            Margin = new Padding(4, 4, 4, 4),
            TextAlign = ContentAlignment.MiddleCenter
        };
        flowReaderInfo.Controls.Add(lbl);
    }

    // =========================================================
    // LOAD BOOKS
    // =========================================================
    private void LoadBooks(List<ReturnedBookInfor>? books)
    {
        _allBooks = books ?? new List<ReturnedBookInfor>();
        _currentPage = 1;
        _totalPages = Math.Max(1, (int)Math.Ceiling(_allBooks.Count / (double)PageSize));
        RenderPage();
        RecalcSummary();
    }

    private void RenderPage()
    {
        dgvBooks.Rows.Clear();

        var pageBooks = _allBooks
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var book in pageBooks)
        {
            int index = dgvBooks.Rows.Add();
            var row = dgvBooks.Rows[index];

            row.Cells["colLoanDetailId"].Value = book.LoanDetailId;
            row.Cells["colCheck"].Value = false;
            row.Cells["colTicket"].Value = book.LoanCode;
            row.Cells["colTitle"].Value = book.Title;
            row.Cells["colCondBorrow"].Value = book.PhysicalConditionBefore.ToString();
            row.Cells["colDue"].Value = book.DueAt == DateTime.MinValue ? "" : book.DueAt.ToString("dd/MM/yyyy");
            row.Cells["colReturn"].Value = book.ReturnedAt?.ToString("dd/MM/yyyy") ?? "";
            row.Cells["colCopyCode"].Value = book.CopyCode.ToString();
            row.Cells["colLate"].Value = book.NumberDaysLate;
            row.Cells["colCondReturn"].Value = "Bình thường";
            row.Cells["colLateFee"].Value = book.AmountPayDaysLate.ToString("N0") + " đ";
            row.Cells["colDamageFee"].Value = "0 đ";
            row.Cells["colNote"].Value = "";

            UpdateRowFees(index);
        }

        lblPageInfo.Text = $"Trang {_currentPage}/{_totalPages} - {_allBooks.Count} dòng";
        btnPrevPage.Enabled = _currentPage > 1;
        btnNextPage.Enabled = _currentPage < _totalPages;
        dgvBooks.ClearSelection();

        UpdateKpi();
        RecalcSummary();
    }

    // =========================================================
    // KPI / SUMMARY
    // =========================================================
    private void UpdateKpi()
    {
        int borrowing = _allBooks.Count;
        int selected = 0;
        decimal fine = 0;

        foreach (DataGridViewRow row in dgvBooks.Rows)
        {
            if (row.IsNewRow) continue;
            if (row.Cells["colCheck"].Value is bool b && b) selected++;
            fine += ParseMoney(row.Cells["colLateFee"].Value) + ParseMoney(row.Cells["colDamageFee"].Value);
        }

        lblKpiBorrowing.Text = borrowing.ToString("N0");
        lblKpiSelected.Text = selected.ToString("N0");
        lblKpiFine.Text = fine.ToString("N0") + " đ";
    }

    private void RecalcSummary()
    {
        int count = 0;
        decimal lateFee = 0, damageFee = 0;

        foreach (DataGridViewRow row in dgvBooks.Rows)
        {
            if (row.IsNewRow) continue;
            if (!(row.Cells["colCheck"].Value is bool v && v)) continue;

            count++;
            lateFee += ParseMoney(row.Cells["colLateFee"].Value);
            damageFee += ParseMoney(row.Cells["colDamageFee"].Value);
        }

        decimal total = lateFee + damageFee;
        lblTotalBooks.Text = count.ToString();
        lblTotalLateFee.Text = lateFee.ToString("N0") + " đ";
        lblTotalDamageFee.Text = damageFee.ToString("N0") + " đ";
        lblGrandTotal.Text = "Tổng thanh toán: " + total.ToString("N0") + " đ";

        bool hasFee = total > 0;
        rdoPaid.Visible = hasFee;
        rdoDebt.Visible = hasFee;

        UpdateKpi();
    }

    private static decimal ParseMoney(object? value)
    {
        string s = (value?.ToString() ?? "0").Replace(" đ", "").Replace(",", "").Trim();
        return decimal.TryParse(s, out var v) ? v : 0;
    }

    // =========================================================
    // NGHIỆP VỤ
    // =========================================================
    private static BookCondition ConvertCondition(string condition) => condition switch
    {
        "Bình thường" => BookCondition.Good,
        "Hư hỏng nhẹ" => BookCondition.MinorDamage,
        "Hư hỏng nặng" => BookCondition.MajorDamage,
        "Mất sách" => BookCondition.Lost,
        _ => BookCondition.Good
    };

    private async Task ConfirmReturnAsync(bool print)
    {
        var checkedRows = dgvBooks.Rows.Cast<DataGridViewRow>()
            .Where(r => Convert.ToBoolean(r.Cells["colCheck"].Value ?? false))
            .ToList();

        if (checkedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một cuốn sách.");
            return;
        }

        decimal total = checkedRows.Sum(row =>
            ParseMoney(row.Cells["colLateFee"].Value) +
            ParseMoney(row.Cells["colDamageFee"].Value));

        string payMode = rdoPaid.Checked ? "Đã thu tiền" : "Ghi nợ phạt";
        string msg = $"Xác nhận trả {checkedRows.Count} cuốn sách.\n" +
                     $"Tổng phạt: {total:N0} đ ({payMode}).";

        if (MessageBox.Show(msg, "Xác nhận trả sách",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            foreach (var row in checkedRows)
            {
                int loanDetailId = Convert.ToInt32(row.Cells["colLoanDetailId"].Value);
                string condition = row.Cells["colCondReturn"].Value?.ToString() ?? "Bình thường";
                var request = new ReturnedBookAddRequest
                {
                    PhysicalConditionAfter = ConvertCondition(condition)
                };
                await _returnBookService.ReturnAsync(loanDetailId, request);
            }

            MessageBox.Show("Đã xác nhận trả sách thành công.",
                "LIBRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (print) { /* TODO: in phiếu trả sách */ }

            await SearchCurrentReaderAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể trả sách: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SearchCurrentReaderAsync()
    {
        if (_currentReader == null) return;
        var filter = new ReturnedBookFilter { keyWord = _currentReader.UserCode };
        var books = await _returnBookService.GetReturneedBookInfor(filter);
        LoadBooks(books);
    }

    private async void DoSearch()
    {
        string userCode = txtSearch.Text.Trim();
        if (string.IsNullOrWhiteSpace(userCode))
        {
            MessageBox.Show("Vui lòng nhập mã độc giả / mã phiếu mượn / mã vạch.",
                "Tra cứu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var reader = await _returnBookService.GetReaderInforAsync(userCode);
            if (reader == null)
            {
                MessageBox.Show("Không tìm thấy độc giả.");
                ResetForm();
                return;
            }

            LoadReaderInfo(reader);
            var filter = new ReturnedBookFilter { keyWord = userCode };
            var books = await _returnBookService.GetReturneedBookInfor(filter);
            LoadBooks(books);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // =========================================================
    // FILTER / GRID EVENTS
    // =========================================================
    private void ApplyStatusFilter()
    {
        // Lọc client-side đơn giản — không ảnh hưởng dữ liệu gốc
        if (_allBooks.Count == 0) return;
        RenderPage();
    }

    private void DgvBooks_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics == null) return;
        string colName = dgvBooks.Columns[e.ColumnIndex].Name;

        if (colName == "colTicket")
        {
            e.Paint(e.CellBounds,
                DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch { "PM001" => Primary, "PM002" => Orange, _ => Green };
            DrawPill(e.Graphics, e.CellBounds, text, c);
            e.Handled = true;
        }
        else if (colName == "colLate")
        {
            int late = e.Value is int n ? n : 0;
            if (late > 0)
            {
                e.Paint(e.CellBounds,
                    DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
                DrawPill(e.Graphics, e.CellBounds, late + " ngày", Red);
                e.Handled = true;
            }
        }
    }

    private void DrawPill(Graphics g, Rectangle bounds, string text, Color color)
    {
        var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        var size = TextRenderer.MeasureText(text, font);
        var rect = new Rectangle(bounds.X + 6, bounds.Y + (bounds.Height - 24) / 2, size.Width + 14, 24);

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

    private void DgvBooks_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        string col = dgvBooks.Columns[e.ColumnIndex].Name;
        if (col == "colCheck" || col == "colCondReturn")
        {
            UpdateRowFees(e.RowIndex);
            RecalcSummary();
        }
    }

    private void UpdateRowFees(int rowIdx)
    {
        var row = dgvBooks.Rows[rowIdx];
        int late = row.Cells["colLate"].Value is int n ? n : 0;
        row.Cells["colLateFee"].Value = (late * PenaltyPerDay).ToString("N0") + " đ";

        var cond = row.Cells["colCondReturn"].Value?.ToString() ?? "Bình thường";
        bool needDamageFee = cond == "Hư hỏng" || cond == "Hư hỏng nặng" || cond == "Mất sách";
        row.Cells["colDamageFee"].ReadOnly = !needDamageFee;
        if (!needDamageFee) row.Cells["colDamageFee"].Value = "0 đ";
    }

    private void CommitDamageFee(int rowIdx)
    {
        var cell = dgvBooks.Rows[rowIdx].Cells["colDamageFee"];
        string raw = (cell.Value?.ToString() ?? "0").Replace(" đ", "").Replace(",", "").Trim();
        if (decimal.TryParse(raw, out var v))
            cell.Value = v.ToString("N0") + " đ";
        RecalcSummary();
    }

    private void ToggleAllRows()
    {
        bool anyUnchecked = dgvBooks.Rows.Cast<DataGridViewRow>()
            .Any(r => !Convert.ToBoolean(r.Cells["colCheck"].Value ?? false));
        foreach (DataGridViewRow r in dgvBooks.Rows)
            r.Cells["colCheck"].Value = anyUnchecked;
        RecalcSummary();
    }

    private void ResetForm()
    {
        txtSearch.Clear();
        flowReaderInfo.Controls.Clear();
        dgvBooks.Rows.Clear();
        _allBooks.Clear();
        _currentReader = null;
        _currentPage = 1;
        _totalPages = 1;
        lblPageInfo.Text = "Trang 1/1 - 0 dòng";
        RecalcSummary();
    }

    // =========================================================
    // HELPERS
    // =========================================================
    private Button CreateButton(string text, Color backColor, Color foreColor, int width, string? glyph = null)
    {
        var b = new Button
        {
            Text = text,
            Width = width,
            Height = 32,
            BackColor = backColor,
            ForeColor = foreColor,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, backColor != Color.White ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(4, 8, 0, 4),
            Cursor = Cursors.Hand
        };
        if (glyph != null)
        {
            b.Image = GlyphImage(glyph, 14, foreColor);
            b.ImageAlign = ContentAlignment.MiddleLeft;
            b.TextImageRelation = TextImageRelation.ImageBeforeText;
        }
        b.FlatAppearance.BorderColor = backColor == Color.White ? BorderGray : backColor;
        if (backColor != Color.White) b.FlatAppearance.BorderSize = 0;
        return b;
    }

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

    // =========================================================
    // HEADER CHECKBOX
    // =========================================================
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

            var state = _checked ? ButtonState.Checked : ButtonState.Normal;
            ControlPaint.DrawCheckBox(graphics, rect, state);
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