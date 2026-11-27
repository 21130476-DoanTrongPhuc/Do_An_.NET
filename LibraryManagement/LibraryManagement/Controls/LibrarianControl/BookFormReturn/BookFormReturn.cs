using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Services;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

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

    private Label lblTotalBooks     = null!;
    private Label lblTotalLateFee   = null!;
    private Label lblTotalDamageFee = null!;
    private Label lblGrandTotal     = null!;
    private RadioButton rdoPaid = null!;
    private RadioButton rdoDebt = null!;

    private List<ReturnedBookInfor> _allBooks = new();
    private sealed record ReturnReceiptLine(
        string CopyCode,
        string Title,
        string DueDate,
        string ConditionAfter,
        decimal LateFee,
        decimal DamageFee);

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
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Location = new Point(14, 8)
        });

        lblDate = new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(16, 36)
        };
        string[] days = { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
        var d = DateTime.Now;
        lblDate.Text = $"{days[(int)d.DayOfWeek]}, ngày {d:dd} tháng {d:MM} năm {d:yyyy}";
        pnl.Controls.Add(lblDate);

        var btnClose = CreateButton("Đóng", Color.White, Color.Black, null, "\uE711");
        btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        pnl.Controls.Add(btnClose);
        pnl.Resize += (_, _) => btnClose.Location = new Point(pnl.Width - btnClose.Width - 12, (pnl.Height - btnClose.Height) / 2);
        btnClose.Location = new Point(pnl.Width - btnClose.Width - 12, (pnl.Height - btnClose.Height) / 2);

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
            Height = 52,
            Padding = new Padding(12, 8, 12, 4),
            BackColor = BgPage
        };

        txtSearch = new TextBox
        {
            Font = Font,
            PlaceholderText = "Tìm mã độc giả, mã phiếu mượn hoặc quét mã vạch..."
        };
        txtSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { DoSearch(); e.SuppressKeyPress = true; }
        };

        var btnSearch = CreateButton("Tra cứu", Primary, Color.White, null, GSearch);
        btnSearch.MinimumSize = new Size(0, 32);
        btnSearch.MaximumSize = new Size(0, 32);
        btnSearch.Click += (_, _) => DoSearch();

        var lblFilter = new Label
        {
            Text = "Lọc:",
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 9F)
        };

        cboStatusFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = Font,
            Width = 175
        };
        cboStatusFilter.Items.AddRange(new object[] { "Tất cả", "Có phạt trễ", "Không phạt", "Sách hư hỏng" });
        cboStatusFilter.SelectedIndex = 0;
        cboStatusFilter.SelectedIndexChanged += (_, _) => ApplyStatusFilter();

        void LayoutFilterBar()
        {
            int right = host.ClientSize.Width - 12;
            int centerY = host.ClientSize.Height / 2;

            cboStatusFilter.Location = new Point(right - cboStatusFilter.Width, centerY - cboStatusFilter.Height / 2);
            lblFilter.Location = new Point(cboStatusFilter.Left - lblFilter.Width - 8, centerY - lblFilter.Height / 2);
            btnSearch.Location = new Point(lblFilter.Left - btnSearch.Width - 16, centerY - btnSearch.Height / 2);

            txtSearch.SetBounds(
                12,
                centerY - txtSearch.Height / 2,
                Math.Max(180, btnSearch.Left - 20),
                txtSearch.Height
            );
        }

        host.Controls.AddRange(new Control[] { txtSearch, btnSearch, lblFilter, cboStatusFilter });
        host.Resize += (_, _) => LayoutFilterBar();
        LayoutFilterBar();

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
            Height = 58,
            Padding = new Padding(12, 4, 12, 6),
            BackColor = BgPage
        };
        host.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(BorderGray), 0, host.Height - 1, host.Width, host.Height - 1);

        var lblReader = new Label
        {
            Text = "Độc giả:",
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        flowReaderInfo = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var btnReset = CreateButton("Làm mới", Color.White, Color.Black, null, GRefresh);
        btnReset.Click += (_, _) => ResetForm();

        var btnReload = CreateButton("Tải lại", Color.White, Color.Black, null, GRefresh);
        btnReload.Click += (_, _) =>
        {
            if (_currentReader != null) { txtSearch.Text = _currentReader.UserCode; DoSearch(); }
            else ResetForm();
        };

        void LayoutToolbar()
        {
            int centerY = (host.ClientSize.Height - 4) / 2;

            btnReload.Location = new Point(host.ClientSize.Width - btnReload.Width - 12, centerY - btnReload.Height / 2);
            btnReset.Location = new Point(btnReload.Left - btnReset.Width - 8, centerY - btnReset.Height / 2);
            lblReader.Location = new Point(12, centerY - lblReader.Height / 2);

            int flowX = lblReader.Right + 10;
            int flowW = Math.Max(120, btnReset.Left - flowX - 12);
            flowReaderInfo.SetBounds(flowX, centerY - 18, flowW, 36);
        }

        host.Controls.AddRange(new Control[] { lblReader, flowReaderInfo, btnReset, btnReload });
        host.Resize += (_, _) => LayoutToolbar();
        LayoutToolbar();

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
            RowTemplate = { Height = 42 },
            ColumnHeadersHeight = 40,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Segoe UI", 9F)
        };
        dgvBooks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvBooks.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        dgvBooks.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        dgvBooks.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 243, 246);
        dgvBooks.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        dgvBooks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
        dgvBooks.DefaultCellStyle.SelectionForeColor = Color.Black;

        // Khai báo chiều rộng cố định đủ lớn cho từng tiêu đề cột
        dgvBooks.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "colCheck",
            HeaderText = "",
            Width = 44,
            MinimumWidth = 44,
            Resizable = DataGridViewTriState.False,
            FlatStyle = FlatStyle.Standard
        });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLoanDetailId", Visible = false });

        AddTextCol(dgvBooks, "colTicket",     "Mã phiếu",            105);
        AddTextCol(dgvBooks, "colCopyCode",   "Mã vạch",             115);
        AddTextCol(dgvBooks, "colTitle",      "Tên sách",            150);
        AddTextCol(dgvBooks, "colCondBorrow", "Tình trạng lúc mượn", 175);
        AddTextCol(dgvBooks, "colDue",        "Hạn trả",             115);
        AddTextCol(dgvBooks, "colReturn",     "Ngày trả",            115);
        AddTextCol(dgvBooks, "colLate",       "Số ngày trễ",         115);

        dgvBooks.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "colCondReturn",
            HeaderText = "Tình trạng khi trả",
            Width = 175,
            MinimumWidth = 175,
            FlatStyle = FlatStyle.Flat,
            DataSource = new[] { "Bình thường", "Hư hỏng", "Hư hỏng nặng", "Mất sách" }
        });

        AddTextCol(dgvBooks, "colLateFee",   "Phạt trễ hạn",     130);
        AddTextCol(dgvBooks, "colDamageFee", "Phạt hư hỏng/mất", 160);
        AddTextCol(dgvBooks, "colNote",      "Ghi chú",          180);

        // Khóa chỉ đọc cho các cột chỉ hiển thị, giữ mở cho cột checkbox, tình trạng trả, tiền phạt hư hỏng và ghi chú
        foreach (DataGridViewColumn col in dgvBooks.Columns)
        {
            col.ReadOnly = col.Name is not ("colCheck" or "colCondReturn" or "colDamageFee" or "colNote");
        }

        // Tự động giãn thêm cột "Tên sách" và "Ghi chú" nếu màn hình rộng hơn tổng chiều rộng mặc định
        void StretchColumnsIfWide()
        {
            int fixedTotal = dgvBooks.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible && c.Name is not ("colTitle" or "colNote"))
                .Sum(c => c.MinimumWidth);

            int remaining = dgvBooks.ClientSize.Width - fixedTotal - 4;
            if (remaining > 430)
            {
                dgvBooks.Columns["colTitle"]!.Width = (int)(remaining * 0.6);
                dgvBooks.Columns["colNote"]!.Width = remaining - dgvBooks.Columns["colTitle"]!.Width;
            }
            else
            {
                dgvBooks.Columns["colTitle"]!.Width = 250;
                dgvBooks.Columns["colNote"]!.Width = 180;
            }
        }
        dgvBooks.Resize += (_, _) => StretchColumnsIfWide();

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

    private static void AddTextCol(DataGridView g, string name, string header, int width)
    {
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = header,
            Width = width,
            MinimumWidth = width
        });
    }

    // ---------------------------------------------------------
    // FOOTER (tổng kết + 3 nút hành động — giữ nguyên nghiệp vụ)
    // ---------------------------------------------------------
    private Panel BuildFooter()
    {
        var host = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 205,
            Padding = new Padding(12, 4, 12, 12)
        };

        var grpSummary = new GroupBox
        {
            Text = "Tổng kết",
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 10, 14, 10)
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // summary
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));  // actions
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // LEFT: Tổng kết dạng TableLayout 2 cột, 5 hàng
        var leftTbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Margin = new Padding(0)
        };
        leftTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
        leftTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        leftTbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F)); // Row 0: Tổng số sách trả
        leftTbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F)); // Row 1: Tổng phạt trễ hạn
        leftTbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F)); // Row 2: Tổng phạt hư hỏng/mất
        leftTbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F)); // Row 3: Tổng thanh toán (cao 38px để không cắt chữ 13pt)
        leftTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row 4: RadioButton hình thức thu phạt

        lblTotalBooks     = AddSummaryRow(leftTbl, "Tổng số sách trả:", "0", 0);
        lblTotalLateFee   = AddSummaryRow(leftTbl, "Tổng phạt trễ hạn:", "0 đ", 1);
        lblTotalDamageFee = AddSummaryRow(leftTbl, "Tổng phạt hư hỏng/mất:", "0 đ", 2);

        // Row 3: Hiển thị trọn vẹn dòng "Tổng thanh toán"
        lblGrandTotal = new Label
        {
            Text = "Tổng thanh toán:  0 đ",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Red,
            Margin = new Padding(0, 2, 0, 0)
        };
        leftTbl.Controls.Add(lblGrandTotal, 0, 3);
        leftTbl.SetColumnSpan(lblGrandTotal, 2);

        // Row 4: Đưa 2 lựa chọn thu tiền xuống dưới để không chèn ép chiều ngang của "Tổng thanh toán"
        var pnlPayMode = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 2, 0, 0)
        };

        rdoPaid = new RadioButton
        {
            Text = "Đã thu tiền",
            AutoSize = true,
            Checked = true,
            Margin = new Padding(0, 4, 20, 0)
        };
        rdoDebt = new RadioButton
        {
            Text = "Ghi nợ phạt",
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0)
        };
        pnlPayMode.Controls.Add(rdoPaid);
        pnlPayMode.Controls.Add(rdoDebt);

        leftTbl.Controls.Add(pnlPayMode, 0, 4);
        leftTbl.SetColumnSpan(pnlPayMode, 2);

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

        var btnReset = CreateButton("Làm mới", Color.White, Color.Black, 0, GRefresh);
        btnReset.Dock = DockStyle.Fill;
        btnReset.Margin = new Padding(0, 4, 0, 4);
        btnReset.Click += (_, _) => ResetForm();

        var btnConfirm = CreateButton("Xác nhận trả sách", Green, Color.White, 0, GCheck);
        btnConfirm.Dock = DockStyle.Fill;
        btnConfirm.Margin = new Padding(0, 4, 0, 4);
        btnConfirm.Click += (_, _) => _ = ConfirmReturnAsync(false);

        var btnPrint = CreateButton("Xác nhận & In phiếu", Primary, Color.White, 0, GPrint);
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
        RenderRows();
        RecalcSummary();
    }

    private void RenderRows()
    {
        dgvBooks.Rows.Clear();

        foreach (var book in _allBooks)
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
    private static BookCondition ConvertCondition(string condition) => condition?.Trim() switch
    {
        "Mới" => BookCondition.New,
        "Bình thường" => BookCondition.Good,
        "Cũ" or "Hư hỏng nhẹ" => BookCondition.Worn,
        "Hư hỏng" or "Hư hỏng nặng" => BookCondition.Damaged,
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

        var receiptLines = checkedRows.Select(row => new ReturnReceiptLine(
            row.Cells["colCopyCode"].Value?.ToString() ?? "",
            row.Cells["colTitle"].Value?.ToString() ?? "",
            row.Cells["colDue"].Value?.ToString() ?? "",
            row.Cells["colCondReturn"].Value?.ToString() ?? "Bình thường",
            ParseMoney(row.Cells["colLateFee"].Value),
            ParseMoney(row.Cells["colDamageFee"].Value))).ToList();

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

            if (print)
            {
                try
                {
                    ShowReturnPrintPreview(receiptLines, total, payMode, _currentReader);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Sách đã được trả thành công nhưng không thể mở bản in: {ex.Message}",
                        "Không thể in phiếu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            await SearchCurrentReaderAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể trả sách: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowReturnPrintPreview(
        IReadOnlyList<ReturnReceiptLine> items,
        decimal total,
        string paymentStatus,
        ReturnedBookReaderInfor? reader)
    {
        using var document = new PrintDocument
        {
            DocumentName = $"PhieuTra_{reader?.UserCode ?? "DG"}_{DateTime.Now:yyyyMMddHHmmss}"
        };
        document.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
        document.DefaultPageSettings.Margins = new Margins(55, 55, 55, 55);

        int nextItem = 0;
        document.BeginPrint += (_, _) => nextItem = 0;
        document.PrintPage += (_, e) =>
        {
            var g = e.Graphics!;
            using var fontHeader = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var fontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var fontBody = new Font("Segoe UI", 9F);
            using var fontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var pen = new Pen(Color.FromArgb(180, 185, 190));

            int x = e.MarginBounds.Left;
            int y = e.MarginBounds.Top;
            int width = e.MarginBounds.Width;

            TextRenderer.DrawText(g, "THƯ VIỆN LIBRA", fontHeader,
                new Rectangle(x, y, width, 22), Primary, TextFormatFlags.Left);
            y += 28;
            TextRenderer.DrawText(g, "PHIẾU TRẢ SÁCH", fontTitle,
                new Rectangle(x, y, width, 32), Color.Black, TextFormatFlags.HorizontalCenter);
            y += 38;
            TextRenderer.DrawText(g, $"Ngày trả: {DateTime.Now:dd/MM/yyyy HH:mm}    Hình thức phạt: {paymentStatus}",
                fontBody, new Rectangle(x, y, width, 22), Color.Black, TextFormatFlags.Left);
            y += 24;
            TextRenderer.DrawText(g, $"Độc giả: {reader?.FullName ?? "Không rõ"} ({reader?.UserCode ?? "-"})",
                fontBody, new Rectangle(x, y, width / 2, 22), Color.Black, TextFormatFlags.Left);
            TextRenderer.DrawText(g, $"Điện thoại: {reader?.Phone ?? "-"}",
                fontBody, new Rectangle(x + width / 2, y, width / 2, 22), Color.Black, TextFormatFlags.Left);
            y += 30;
            g.DrawLine(pen, x, y, x + width, y);
            y += 12;

            int[] weights = [6, 12, 27, 13, 13, 14, 15];
            int[] columnWidths = weights.Select(weight => width * weight / 100).ToArray();
            columnWidths[^1] = width - columnWidths.Take(columnWidths.Length - 1).Sum();
            string[] headings = ["STT", "Mã bản", "Tên sách", "Tình trạng trả", "Hạn trả", "Phạt trễ", "Phạt hỏng/mất"];

            void DrawRow(IReadOnlyList<string> cells, int rowHeight, bool isHeader)
            {
                if (isHeader)
                {
                    using var brush = new SolidBrush(Color.FromArgb(240, 242, 245));
                    g.FillRectangle(brush, x, y, width, rowHeight);
                }

                g.DrawRectangle(pen, x, y, width, rowHeight);
                int cellX = x;
                for (int i = 0; i < cells.Count; i++)
                {
                    TextRenderer.DrawText(g, cells[i], isHeader ? fontBold : fontBody,
                        new Rectangle(cellX + 3, y + 3, columnWidths[i] - 6, rowHeight - 6),
                        Color.Black,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    cellX += columnWidths[i];
                    if (i < cells.Count - 1)
                    {
                        g.DrawLine(pen, cellX, y, cellX, y + rowHeight);
                    }
                }

                y += rowHeight;
            }

            DrawRow(headings, 30, true);
            while (nextItem < items.Count && y + 26 <= e.MarginBounds.Bottom - 95)
            {
                var item = items[nextItem];
                DrawRow(
                    [
                        (nextItem + 1).ToString(),
                        item.CopyCode,
                        item.Title,
                        item.ConditionAfter,
                        item.DueDate,
                        $"{item.LateFee:N0} đ",
                        $"{item.DamageFee:N0} đ"
                    ],
                    26,
                    false);
                nextItem++;
            }

            if (nextItem < items.Count)
            {
                e.HasMorePages = true;
                return;
            }

            y += 14;
            TextRenderer.DrawText(g, $"Tổng số sách trả: {items.Count}    Tổng tiền phạt: {total:N0} đ",
                fontBold, new Rectangle(x, y, width, 24), Color.Black, TextFormatFlags.Left);
            y += 42;
            TextRenderer.DrawText(g, "Độc giả ký nhận", fontBold,
                new Rectangle(x, y, width / 2, 22), Color.Black, TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g, "Thủ thư xác nhận", fontBold,
                new Rectangle(x + width / 2, y, width / 2, 22), Color.Black, TextFormatFlags.HorizontalCenter);
        };

        using var preview = new PrintPreviewDialog
        {
            Document = document,
            Width = 1000,
            Height = 760,
            StartPosition = FormStartPosition.CenterParent
        };
        preview.ShowDialog(FindForm());
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
        RenderRows();
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
        RecalcSummary();
    }

    // =========================================================
    // HELPERS
    // =========================================================
    private Button CreateButton(string text, Color backColor, Color foreColor, int? width, string? glyph = null)
    {
        var b = new Button
        {
            Text = glyph != null ? " " + text.Trim() : text.Trim(),
            BackColor = backColor,
            ForeColor = foreColor,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, backColor != Color.White ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(4, 4, 0, 4),
            Padding = new Padding(10, 0, 12, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = false,
            Cursor = Cursors.Hand
        };

        if (width.HasValue && width.Value > 0)
        {
            b.AutoSize = false;
            b.Size = new Size(width.Value, 34);
        }
        else if (width == null)
        {
            b.AutoSize = true;
            b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            b.MinimumSize = new Size(0, 34);
            b.MaximumSize = new Size(0, 34);
        }

        if (glyph != null)
        {
            b.Image = GlyphImage(glyph, 13, foreColor);
            b.ImageAlign = ContentAlignment.MiddleLeft;
            b.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        b.FlatAppearance.BorderColor = backColor == Color.White ? BorderGray : backColor;
        if (backColor != Color.White) b.FlatAppearance.BorderSize = 0;
        return b;
    }

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