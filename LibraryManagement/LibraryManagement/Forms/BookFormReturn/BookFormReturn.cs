using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.BookFormReturn;

using System.Drawing.Drawing2D;


public partial class BookFormReturn : UserControl
{
    private readonly ReturnedBookServiceImpl _returnBookService;
    private ReturnedBookReaderInfor? _currentReader;
    private ReturnedBookInfor? _bookInfor;
    
    // ====== Màu sắc (dùng chung với LibrarianFormMain) ======
    private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
    private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
    private static readonly Color Green      = Color.FromArgb(34, 160, 90);
    private static readonly Color Red        = Color.FromArgb(200, 40, 40);
    private const string IconFont = "Segoe MDL2 Assets";

    private const string GSearch = "\uE721", GBook = "\uE82D",
        GRefresh = "\uE72C", GCheck = "\uE73E", GPrint = "\uE749";

    private const decimal PenaltyPerDay = 2000m;

    // Control
    private TextBox txtSearch = null!;
    private Panel pnlReaderInfo = null!;
    private DataGridView dgvBooks = null!;
    private Label lblTotalBooks = null!;
    private Label lblTotalLateFee = null!;
    private Label lblTotalDamageFee = null!;
    private Label lblGrandTotal = null!;
    private RadioButton rdoPaid = null!;
    private RadioButton rdoDebt = null!;

    // Sự kiện để host form biết khi cần đóng
    public event EventHandler? CloseRequested;

    public BookFormReturn(ReturnedBookServiceImpl returnBookService)
    {
        InitializeComponent();

        _returnBookService = returnBookService;
        
        BuildUi();
    }

    // ============================================================
    //  DỮ LIỆU MẪU
    // ============================================================
    private void LoadReaderInfo(ReturnedBookReaderInfor reader)
    {
        _currentReader = reader;
        pnlReaderInfo.Controls.Clear();
        AddReaderField("Họ và tên:", reader.FullName, 0);
        AddReaderField("Mã độc giả:", reader.UserCode, 1);
        AddReaderField("Email:", reader.Email, 2);
        AddReaderField("Điện thoại:", reader.Phone, 3);
        // AddReaderBadge("Nợ phạt cũ: 12.000 đ", Red, 4);
    }

    private void LoadBooks(List<ReturnedBookInfor>? books)
    {
        dgvBooks.Rows.Clear();

        foreach (var book in books)
        {
            int index = dgvBooks.Rows.Add();
            
            var row = dgvBooks.Rows[index];

            row.Cells["colLoanDetailId"].Value = book.LoanDetailId;

            row.Cells["colCheck"].Value = false;

            row.Cells["colTicket"].Value = book.LoanCode;

            row.Cells["colTitle"].Value = book.Title;

            row.Cells["colCondBorrow"].Value = book.PhysicalConditionBefore.ToString();

            row.Cells["colDue"].Value = book.DueAt == DateTime.MinValue 
                ? "" 
                : book.DueAt.ToString("dd/MM/yyyy");

            row.Cells["colReturn"].Value =
                book.ReturnedAt?.ToString("dd/MM/yyyy") ?? "";

            row.Cells["colLate"].Value = book.NumberDaysLate;

            row.Cells["colCondReturn"].Value = "Bình thường";

            row.Cells["colLateFee"].Value = book.AmountPayDaysLate.ToString("N0") + " đ";

            row.Cells["colDamageFee"].Value = "0 đ";

            row.Cells["colNote"].Value = "";
            
            UpdateRowFees(index);
        }
    }

    private void AddBook(string ticketId, string copyId, string title, string condition,
                         DateTime due, DateTime retDate)
    {
        int idx = dgvBooks.Rows.Add();
        var row = dgvBooks.Rows[idx];
        row.Cells["colCheck"].Value = false;
        row.Cells["colTicket"].Value = ticketId;
        row.Cells["colCopy"].Value = copyId;
        row.Cells["colTitle"].Value = title;
        row.Cells["colCondBorrow"].Value = condition;
        row.Cells["colDue"].Value = due.ToString("dd/MM/yyyy");
        row.Cells["colReturn"].Value = retDate.ToString("dd/MM/yyyy");

        int late = Math.Max(0, (retDate - due).Days);
        row.Cells["colLate"].Value = late;

        var cbo = new DataGridViewComboBoxCell();
        cbo.Items.AddRange("Bình thường", "Hư hỏng", "Mất sách");
        cbo.Value = "Bình thường";
        row.Cells["colCondReturn"] = cbo;

        row.Cells["colLateFee"].Value = (late * PenaltyPerDay).ToString("N0") + " đ";
        row.Cells["colDamageFee"].Value = "0 đ";
        row.Cells["colNote"].Value = "";
    }

    // ============================================================
    //  DỰNG GIAO DIỆN (giống form cũ, không có form-border)
    // ============================================================
    private void BuildUi()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(240, 242, 245);
        Font = new Font("Segoe UI", 9F);

        Controls.Add(BuildFooter());    // Bottom
        Controls.Add(BuildTableArea()); // Fill
        Controls.Add(BuildHeader());    // Top
        ResumeLayout(true);
    }

    // ---------- 1. HEADER ----------
    private Panel BuildHeader()
    {
        var host = new Panel { Dock = DockStyle.Top, Height = 178, Padding = new Padding(12, 12, 12, 0) };

        var grpSearch = new GroupBox
        {
            Text = "Tra cứu",
            Dock = DockStyle.Left,
            Width = 380,
            Padding = new Padding(12, 10, 12, 12)
        };

        txtSearch = new TextBox
        {
            PlaceholderText = "Nhập mã độc giả, mã phiếu mượn hoặc quét mã vạch...",
            Location = new Point(14, 30),
            Width = 240,
            Font = new Font("Segoe UI", 9F)
        };
        txtSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { DoSearch(); e.SuppressKeyPress = true; }
        };

        var btnSearch = new Button
        {
            Text = "  Tra cứu",
            Location = new Point(262, 28),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GSearch, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += (_, _) => DoSearch();

        grpSearch.Controls.Add(txtSearch);
        grpSearch.Controls.Add(btnSearch);
        grpSearch.Controls.Add(new Label
        {
            Text = "Ví dụ: DG-00125  ·  PM001  ·  BC-0001",
            Location = new Point(14, 68),
            AutoSize = true,
            ForeColor = TextGray,
            Font = new Font("Segoe UI", 8F)
        });

        var grpInfo = new GroupBox
        {
            Text = "Thông tin độc giả",
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 10, 14, 10)
        };
        pnlReaderInfo = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        grpInfo.Controls.Add(pnlReaderInfo);

        var rightHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 0, 0, 0) };
        rightHost.Controls.Add(grpInfo);

        host.Controls.Add(rightHost);
        host.Controls.Add(grpSearch);
        return host;
    }

    private void AddReaderField(string caption, string value, int index)
    {
        int col = index % 2;
        int row = index / 2;
        int x = 8 + col * 340;
        int y = 6 + row * 34;

        pnlReaderInfo.Controls.Add(new Label
        {
            Text = caption, AutoSize = true, ForeColor = TextGray,
            Font = new Font("Segoe UI", 8.5F), Location = new Point(x, y + 6)
        });
        pnlReaderInfo.Controls.Add(new Label
        {
            Text = value, AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Location = new Point(x + 90, y + 4)
        });
    }

    private void AddReaderBadge(string text, Color color, int index)
    {
        pnlReaderInfo.Controls.Add(new Label
        {
            Text = "●  " + text,
            Size = new Size(260, 26),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = color,
            BackColor = Color.FromArgb(22, color),
            Location = new Point(8 + (index % 2) * 340, 6 + (index / 2) * 34)
        });
    }

    private static BookCondition ConvertCondition(string condition)
    {
        return condition switch
        {
            "Bình thường" => BookCondition.Good,
            "Hư hỏng nhẹ" => BookCondition.MinorDamage,
            "Hư hỏng nặng" => BookCondition.MajorDamage,
            "Mất sách" => BookCondition.Lost,
            _ => BookCondition.Good
        };
    }
    
    private async Task ConfirmReturnAsync(bool print)
{
    var checkedRows = dgvBooks.Rows
        .Cast<DataGridViewRow>()
        .Where(r => Convert.ToBoolean(
            r.Cells["colCheck"].Value ?? false))
        .ToList();

    if (checkedRows.Count == 0)
    {
        MessageBox.Show("Vui lòng chọn ít nhất một cuốn sách.");
        return;
    }

    decimal total = checkedRows.Sum(row =>
        ParseMoney(row.Cells["colLateFee"].Value) +
        ParseMoney(row.Cells["colDamageFee"].Value)
    );

    string payMode = rdoPaid.Checked
        ? "Đã thu tiền"
        : "Ghi nợ phạt";

    string msg =
        $"Xác nhận trả {checkedRows.Count} cuốn sách.\n" +
        $"Tổng phạt: {total:N0} đ ({payMode}).";

    if (MessageBox.Show(
        msg,
        "Xác nhận trả sách",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question) != DialogResult.Yes)
    {
        return;
    }

    try
    {
        foreach (var row in checkedRows)
        {
            int loanDetailId = Convert.ToInt32(
                row.Cells["colLoanDetailId"].Value
            );

            string condition = row.Cells["colCondReturn"]
                .Value?.ToString() ?? "Bình thường";

            var request = new ReturnedBookAddRequest
            {
                PhysicalConditionAfter =
                    ConvertCondition(condition)
            };

            await _returnBookService.ReturnAsync(
                loanDetailId,
                request
            );
        }

        MessageBox.Show(
            "Đã xác nhận trả sách thành công.",
            "LIBRA",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );

        if (print)
        {
            // TODO: Gọi chức năng in phiếu trả sách
        }

        await SearchCurrentReaderAsync();
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Không thể trả sách: {ex.Message}",
            "Lỗi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }

    async Task SearchCurrentReaderAsync()
    {
        if (_currentReader == null)
            return;

        var filter = new ReturnedBookFilter
        {
            keyWord = _currentReader.UserCode
        };

        var books = await _returnBookService
            .GetReturneedBookInfor(filter);

        LoadBooks(books);
    }
}

    private async void DoSearch()
    {
        string userCode = txtSearch.Text.Trim();
        
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
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

            var filter = new ReturnedBookFilter
            {
                keyWord = userCode
            };

            var books = await _returnBookService.GetReturneedBookInfor(filter);
            
            LoadBooks(books);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // ---------- 2. DATA TABLE ----------
    private GroupBox BuildTableArea()
    {
        var grp = new GroupBox
        {
            Text = "Danh sách sách đang mượn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10),
            Margin = new Padding(12, 0, 12, 0)
        };
        dgvBooks = BuildGrid();
        grp.Controls.Add(dgvBooks);
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

        g.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "colCheck", HeaderText = "", FillWeight = 28, FlatStyle = FlatStyle.Standard
        });
        
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colLoanDetailId",
            Visible = false
        });

        AddTextCol(g, "colTicket", "Mã phiếu", 55);
        AddTextCol(g, "colCopy", "Mã vạch", 65);
        AddTextCol(g, "colTitle", "Tên sách", 150);
        AddTextCol(g, "colCondBorrow", "Tình trạng lúc mượn", 100);
        AddTextCol(g, "colDue", "Hạn trả", 65);
        AddTextCol(g, "colReturn", "Ngày trả", 65);
        AddTextCol(g, "colLate", "Số ngày trễ", 55);
        g.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "colCondReturn",
            HeaderText = "Tình trạng khi trả",
            FillWeight = 100,
            FlatStyle = FlatStyle.Flat,
            DataSource = new[]
            {
                "Bình thường",
                "Hư hỏng",
                "Hư hỏng nặng",
                "Mất sách"
            }
        });
        AddTextCol(g, "colLateFee", "Phạt trễ hạn", 75);
        AddTextCol(g, "colDamageFee", "Phạt hư hỏng/mất", 85);
        AddTextCol(g, "colNote", "Ghi chú", 100);

        g.Columns["colTicket"]!.DefaultCellStyle.ForeColor = Primary;
        g.Columns["colTicket"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        g.Columns["colLate"]!.DefaultCellStyle.ForeColor = Red;

        g.CellPainting += DgvBooks_CellPainting;
        g.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (g.IsCurrentCellDirty && g.CurrentCell is DataGridViewCheckBoxCell)
                g.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        g.CellValueChanged += DgvBooks_CellValueChanged;
        g.DataError += (_, e) => e.ThrowException = false;
        g.CellEndEdit += (_, e) =>
        {
            if (e.ColumnIndex == g.Columns["colDamageFee"]!.Index)
                CommitDamageFee(e.RowIndex);
        };

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
        bool needDamageFee = cond == "Hư hỏng" || cond == "Mất sách";
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

    // ---------- 3. FOOTER ----------
    private Panel BuildFooter()
    {
        var host = new Panel { Dock = DockStyle.Bottom, Height = 128, Padding = new Padding(12, 8, 12, 12) };
        var grpSummary = new GroupBox
        {
            Text = "Tổng kết", Dock = DockStyle.Fill, Padding = new Padding(16, 10, 16, 10)
        };

        var left = new Panel { Dock = DockStyle.Left, Width = 560 };
        lblTotalBooks = AddSummaryLine(left, "Tổng số sách trả:", "0", 0);
        lblTotalLateFee = AddSummaryLine(left, "Tổng phạt trễ hạn:", "0 đ", 1);
        lblTotalDamageFee = AddSummaryLine(left, "Tổng phạt hư hỏng/mất:", "0 đ", 2);

        lblGrandTotal = new Label
        {
            Text = "Tổng thanh toán:  0 đ",
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Red,
            Location = new Point(10, 88)
        };
        left.Controls.Add(lblGrandTotal);

        rdoPaid = new RadioButton { Text = "Đã thu tiền", AutoSize = true, Location = new Point(340, 92), Checked = true };
        rdoDebt = new RadioButton { Text = "Ghi nợ phạt", AutoSize = true, Location = new Point(450, 92) };
        left.Controls.Add(rdoPaid);
        left.Controls.Add(rdoDebt);

        var right = new Panel { Dock = DockStyle.Right, Width = 360, Padding = new Padding(0, 24, 0, 0) };

        var btnReset = new Button
        {
            Text = "  Làm mới",
            Size = new Size(120, 38),
            Location = new Point(0, 20),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            Image = GlyphImage(GRefresh, 14, Color.Black),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        btnReset.FlatAppearance.BorderColor = BorderGray;
        btnReset.Click += (_, _) => ResetForm();

        var btnConfirm = new Button
        {
            Text = "  Xác nhận trả sách",
            Size = new Size(160, 38),
            Location = new Point(126, 20),
            FlatStyle = FlatStyle.Flat,
            BackColor = Green,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GCheck, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        btnConfirm.FlatAppearance.BorderSize = 0;
        btnConfirm.Click += (_, _) => ConfirmReturnAsync(false);

        var btnPrint = new Button
        {
            Text = "  Xác nhận & In phiếu",
            Size = new Size(180, 38),
            Location = new Point(126, 66),
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Image = GlyphImage(GPrint, 14, Color.White),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += (_, _) => ConfirmReturnAsync(true);

        right.Controls.Add(btnReset);
        right.Controls.Add(btnConfirm);
        right.Controls.Add(btnPrint);

        grpSummary.Controls.Add(right);
        grpSummary.Controls.Add(left);
        host.Controls.Add(grpSummary);
        return host;
    }

    private Label AddSummaryLine(Panel parent, string caption, string value, int row)
    {
        parent.Controls.Add(new Label
        {
            Text = caption, AutoSize = true, ForeColor = TextGray,
            Font = new Font("Segoe UI", 9F), Location = new Point(10, 8 + row * 24)
        });
        var lbl = new Label
        {
            Text = value, AutoSize = true,
            Font = new Font("Segoe UI", 9F), ForeColor = Color.Black,
            Location = new Point(210, 8 + row * 24)
        };
        parent.Controls.Add(lbl);
        return lbl;
    }

    // ============================================================
    //  TỔNG KẾT & HÀNH ĐỘNG
    // ============================================================
    private void RecalcSummary()
    {
        int count = 0;
        decimal lateFee = 0;
        decimal damageFee = 0;

        foreach (DataGridViewRow row in dgvBooks.Rows)
        {
            if (row.IsNewRow)
                continue;

            // Kiểm tra checkbox
            bool isChecked = row.Cells["colCheck"].Value is bool value && value;

            if (!isChecked)
                continue;

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
    }

    private static decimal ParseMoney(object? value)
    {
        string s = (value?.ToString() ?? "0").Replace(" đ", "").Replace(",", "").Trim();
        return decimal.TryParse(s, out var v) ? v : 0;
    }

    private void ResetForm()
    {
        txtSearch.Clear();
        pnlReaderInfo.Controls.Clear();
        dgvBooks.Rows.Clear();
        RecalcSummary();
    }

    private void ConfirmReturn(bool print)
    {
        var checkedRows = dgvBooks.Rows.Cast<DataGridViewRow>()
            .Where(r => Convert.ToBoolean(r.Cells["colCheck"].Value ?? false))
            .ToList();

        if (checkedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng tích chọn ít nhất một cuốn sách để trả.",
                "Nhận trả sách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal total = ParseMoney(lblGrandTotal.Text.Replace("Tổng thanh toán:", ""));
        string payMode = rdoPaid.Checked ? "Đã thu tiền" : "Ghi nợ phạt";

        string msg = $"Xác nhận trả {checkedRows.Count} cuốn sách.\n" +
                     $"Tổng phạt: {total:N0} đ  ({(total > 0 ? payMode : "Không phạt")}).";
        if (print) msg += "\n\nSau khi xác nhận sẽ in phiếu trả sách.";

        if (MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            != DialogResult.Yes) return;

        // TODO: ReturnService.Save(...) khi nối CSDL
        MessageBox.Show("Đã xác nhận trả sách thành công." +
            (print ? "\n(Phiếu trả sách đã được gửi tới máy in.)" : ""),
            "LIBRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

        ResetForm();
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
