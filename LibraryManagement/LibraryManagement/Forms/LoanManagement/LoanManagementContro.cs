using LibraryManagement.Services;

namespace LibraryManagement.Forms.LoanManagement;

public class LoanManagementControl : UserControl
{
    private static readonly Color Primary = Color.FromArgb(19, 98, 160);
    private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
    private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
    private static readonly Color SoftBlue = Color.FromArgb(232, 240, 250);
    private static readonly Color Green = Color.FromArgb(34, 160, 90);
    private static readonly Color Orange = Color.FromArgb(214, 140, 10);
    private static readonly Color Red = Color.FromArgb(200, 40, 40);

    private readonly LoanManagementService _service = new();
    public event Action? LoanDataChanged;

    // Tab 1: Tạo phiếu mượn (2.1, 2.2, 2.3)
    private TextBox _txtScanCard = null!;
    private Label _lblReaderSummary = null!;
    private Label _lblEligibilityBanner = null!;
    private TextBox _txtScanCopy = null!;
    private DataGridView _dgvCart = null!;
    private TextBox _txtLoanNote = null!;
    private Label _lblCartSummary = null!;
    private Button _btnPrintLastLoan = null!;

    private ReaderBorrowContextDto? _currentReader;
    private readonly List<BorrowCartItemDto> _cart = new();
    private int _lastCreatedLoanId;

    // Tab 2: Quản lý phiếu mượn (2.4)
    private TextBox _txtSearchLoan = null!;
    private ComboBox _cboStatusFilter = null!;
    private CheckBox _chkFilterDate = null!;
    private DateTimePicker _dtpFrom = null!;
    private DateTimePicker _dtpTo = null!;
    private DataGridView _dgvLoans = null!;
    private DataGridView _dgvLoanDetails = null!;
    private Label _lblDetailHeader = null!;

    private List<LoanSummaryDto> _loans = new();
    private List<LoanDetailItemDto> _currentDetails = new();

    public LoanManagementControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(240, 242, 245);
        Font = new Font("Segoe UI", 9F);
        BuildUi();
        LoadLoansList();
    }

    private void BuildUi()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(12, 5)
        };

        var tabCreate = new TabPage("2.1 - 2.3. Lập phiếu mượn")
        {
            BackColor = Color.FromArgb(240, 242, 245),
            Padding = new Padding(10)
        };
        BuildCreateLoanTab(tabCreate);

        var tabManage = new TabPage("2.4. Quản lý phiếu mượn & Lịch sử")
        {
            BackColor = Color.FromArgb(240, 242, 245),
            Padding = new Padding(10)
        };
        BuildManageLoansTab(tabManage);

        tabs.TabPages.Add(tabCreate);
        tabs.TabPages.Add(tabManage);
        tabs.SelectedIndexChanged += (_, _) =>
        {
            if (tabs.SelectedIndex == 1) LoadLoansList();
        };

        Controls.Add(tabs);
    }

    private void BuildCreateLoanTab(TabPage page)
    {
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 170, Padding = new Padding(0, 0, 0, 8) };

        var grpReader = new GroupBox
        {
            Text = "Quét mã thẻ độc giả (2.1)",
            Dock = DockStyle.Left,
            Width = 420,
            Padding = new Padding(12, 10, 12, 10),
            BackColor = Color.White
        };

        _txtScanCard = new TextBox
        {
            Location = new Point(14, 28),
            Width = 285,
            PlaceholderText = "Quét mã thẻ (VD: RC00001) hoặc mã ĐG..."
        };
        _txtScanCard.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ScanReader();
            }
        };

        var btnFindReader = MakeButton("Kiểm tra", Primary, Color.White, 100);
        btnFindReader.Location = new Point(306, 26);
        btnFindReader.Click += (_, _) => ScanReader();

        _lblReaderSummary = new Label
        {
            Location = new Point(14, 64),
            Size = new Size(392, 88),
            Text = "Chưa chọn độc giả. Quét mã thẻ để kiểm tra thông tin.",
            ForeColor = TextGray
        };

        grpReader.Controls.AddRange([_txtScanCard, btnFindReader, _lblReaderSummary]);

        var hostCondition = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 0, 0) };
        var grpCondition = new GroupBox
        {
            Text = "Kiểm tra tự động điều kiện mượn (2.2)",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            BackColor = Color.White
        };

        _lblEligibilityBanner = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Hệ thống tự động kiểm tra: hiệu lực thẻ, trạng thái khóa, nợ phạt, hạn mức mượn và quy định thư viện.",
            ForeColor = TextGray
        };
        grpCondition.Controls.Add(_lblEligibilityBanner);
        hostCondition.Controls.Add(grpCondition);

        pnlTop.Controls.Add(hostCondition);
        pnlTop.Controls.Add(grpReader);

        var pnlScanBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = SoftBlue,
            Padding = new Padding(12, 10, 12, 10)
        };
        pnlScanBar.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlScanBar.Width - 1, pnlScanBar.Height - 1);

        var lblScanBook = new Label
        {
            Text = "Quét mã vạch bản sách:",
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(12, 16)
        };

        _txtScanCopy = new TextBox
        {
            Location = new Point(185, 13),
            Width = 260,
            PlaceholderText = "Quét mã bản sách (VD: CP00001)..."
        };
        _txtScanCopy.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ScanBookCopy();
            }
        };

        var btnAddCopy = MakeButton("Thêm sách vào phiếu", Primary, Color.White, 150);
        btnAddCopy.Location = new Point(455, 11);
        btnAddCopy.Click += (_, _) => ScanBookCopy();

        var btnRemoveCopy = MakeButton("Xóa sách chọn", Color.White, Red, 120);
        btnRemoveCopy.Location = new Point(613, 11);
        btnRemoveCopy.Click += (_, _) => RemoveSelectedCartItem();

        pnlScanBar.Controls.AddRange([lblScanBook, _txtScanCopy, btnAddCopy, btnRemoveCopy]);

        _dgvCart = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 34 },
            ColumnHeadersHeight = 34
        };
        _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCopyCode", HeaderText = "Mã bản sách", ReadOnly = true, FillWeight = 75 });
        _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Tên đầu sách", ReadOnly = true, FillWeight = 160 });
        _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAuthor", HeaderText = "Tác giả", ReadOnly = true, FillWeight = 100 });
        _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCond", HeaderText = "Tình trạng", ReadOnly = true, FillWeight = 65 });
        _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDays", HeaderText = "Số ngày", ReadOnly = true, FillWeight = 55 });
        _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDueAt", HeaderText = "Hạn trả (dd/MM/yyyy)", ReadOnly = false, FillWeight = 90 });
        _dgvCart.CellEndEdit += DgvCart_CellEndEdit;

        var pnlFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        pnlFooter.Paint += (_, e) => e.Graphics.DrawLine(new Pen(BorderGray), 0, 0, pnlFooter.Width, 0);

        pnlFooter.Controls.Add(new Label { Text = "Ghi chú:", AutoSize = true, Location = new Point(12, 22) });
        _txtLoanNote = new TextBox { Location = new Point(72, 18), Width = 280, PlaceholderText = "Ghi chú phiếu mượn..." };

        _lblCartSummary = new Label
        {
            Location = new Point(365, 22),
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Primary,
            Text = "Số sách mượn: 0 cuốn"
        };

        var btnReset = MakeButton("Làm mới", Color.White, Color.Black, 95);
        btnReset.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnReset.Location = new Point(pnlFooter.Width - 415, 15);
        btnReset.Click += (_, _) => ResetCreateForm();

        _btnPrintLastLoan = MakeButton("Xem / In phiếu", SoftBlue, Primary, 125);
        _btnPrintLastLoan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnPrintLastLoan.Location = new Point(pnlFooter.Width - 310, 15);
        _btnPrintLastLoan.Enabled = false;
        _btnPrintLastLoan.Click += (_, _) =>
        {
            if (_lastCreatedLoanId > 0) PrintLoanReceipt(_lastCreatedLoanId);
        };

        var btnConfirmLoan = MakeButton("Xác nhận giao dịch mượn", Green, Color.White, 170);
        btnConfirmLoan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnConfirmLoan.Location = new Point(pnlFooter.Width - 178, 15);
        btnConfirmLoan.Click += (_, _) => ConfirmCreateLoan();

        pnlFooter.Controls.AddRange([_txtLoanNote, _lblCartSummary, btnReset, _btnPrintLastLoan, btnConfirmLoan]);

        page.Controls.Add(_dgvCart);
        page.Controls.Add(pnlFooter);
        page.Controls.Add(pnlScanBar);
        page.Controls.Add(pnlTop);
    }

    private void ScanReader()
    {
        var ctx = _service.FindAndValidateReader(_txtScanCard.Text, _cart.Count);
        if (ctx == null)
        {
            _currentReader = null;
            _lblReaderSummary.ForeColor = Red;
            _lblReaderSummary.Text = "Không tìm thấy độc giả hoặc thẻ thư viện.";
            _lblEligibilityBanner.ForeColor = TextGray;
            _lblEligibilityBanner.Text = "Vui lòng kiểm tra lại mã thẻ (VD: RC00001) hoặc mã độc giả (VD: RD0001).";
            return;
        }

        _currentReader = ctx;
        RefreshReaderPanel();
        if (ctx.IsEligible) _txtScanCopy.Focus();
    }

    private void RefreshReaderPanel()
    {
        if (_currentReader == null) return;
        _currentReader = _service.FindAndValidateReader(_currentReader.UserCode, _cart.Count)!;

        _lblReaderSummary.ForeColor = Color.Black;
        _lblReaderSummary.Text =
            $"Họ tên: {_currentReader.FullName} ({_currentReader.UserCode})   |   Thẻ: {_currentReader.CardCode} ({_currentReader.CardStatus})\n" +
            $"Hạn thẻ: {(_currentReader.CardExpiresAt?.ToString("dd/MM/yyyy") ?? "—")}   |   SĐT: {_currentReader.Phone ?? "—"}\n" +
            $"Đang mượn: {_currentReader.ActiveBorrowedCount}/{_currentReader.MaxActiveCopies} cuốn   |   Nợ phạt: {_currentReader.UnpaidFineVnd:N0} đ";

        if (_currentReader.IsEligible)
        {
            _lblEligibilityBanner.ForeColor = Green;
            _lblEligibilityBanner.Text =
                "✔ ĐỦ ĐIỀU KIỆN MƯỢN SÁCH\n" +
                $"• Có thể mượn thêm tối đa {_currentReader.MaxActiveCopies - _currentReader.ActiveBorrowedCount - _cart.Count} cuốn nữa.";
        }
        else
        {
            _lblEligibilityBanner.ForeColor = Red;
            _lblEligibilityBanner.Text =
                "✖ KHÔNG ĐỦ ĐIỀU KIỆN MƯỢN SÁCH:\n• " +
                string.Join("\n• ", _currentReader.ValidationMessages);
        }
    }

    private void ScanBookCopy()
    {
        if (_currentReader == null)
        {
            MessageBox.Show("Vui lòng quét mã thẻ độc giả trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtScanCard.Focus();
            return;
        }

        if (!_currentReader.IsEligible)
        {
            MessageBox.Show("Độc giả không đủ điều kiện mượn sách:\n• " +
                string.Join("\n• ", _currentReader.ValidationMessages),
                "Từ chối mượn sách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_currentReader.ActiveBorrowedCount + _cart.Count >= _currentReader.MaxActiveCopies)
        {
            MessageBox.Show($"Đã đạt giới hạn mượn tối đa {_currentReader.MaxActiveCopies} cuốn.",
                "Vượt hạn mức", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var item = _service.ValidateAndPrepareCopy(_txtScanCopy.Text, _currentReader.ReaderId, _cart);
            _cart.Add(item);
            _txtScanCopy.Clear();
            _txtScanCopy.Focus();
            BindCartGrid();
            RefreshReaderPanel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể thêm bản sách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtScanCopy.SelectAll();
            _txtScanCopy.Focus();
        }
    }

    private void RemoveSelectedCartItem()
    {
        if (_dgvCart.CurrentRow == null || _dgvCart.CurrentRow.Index < 0) return;
        int idx = _dgvCart.CurrentRow.Index;
        if (idx < _cart.Count)
        {
            _cart.RemoveAt(idx);
            BindCartGrid();
            RefreshReaderPanel();
        }
    }

    private void DgvCart_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _cart.Count) return;
        if (_dgvCart.Columns[e.ColumnIndex].Name != "colDueAt") return;

        string raw = _dgvCart.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
        if (DateTime.TryParseExact(raw.Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var newDue)
            && newDue.Date >= DateTime.Today)
        {
            _cart[e.RowIndex].DueAt = newDue.Date;
            _cart[e.RowIndex].MaxLoanDays = (newDue.Date - DateTime.Today).Days;
            _dgvCart.Rows[e.RowIndex].Cells["colDays"].Value = _cart[e.RowIndex].MaxLoanDays;
        }
        else
        {
            MessageBox.Show("Ngày hạn trả phải theo định dạng dd/MM/yyyy và từ hôm nay trở đi.",
                "Sai định dạng ngày", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _dgvCart.Rows[e.RowIndex].Cells["colDueAt"].Value = _cart[e.RowIndex].DueAt.ToString("dd/MM/yyyy");
        }
    }

    private void BindCartGrid()
    {
        _dgvCart.Rows.Clear();
        foreach (var item in _cart)
        {
            _dgvCart.Rows.Add(
                item.CopyCode,
                item.Title,
                item.Author,
                item.PhysicalCondition,
                item.MaxLoanDays,
                item.DueAt.ToString("dd/MM/yyyy")
            );
        }
        _lblCartSummary.Text = $"Số sách mượn: {_cart.Count} cuốn";
    }

    private void ConfirmCreateLoan()
    {
        if (_currentReader == null || _cart.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn độc giả và thêm ít nhất 1 bản sách.", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var createdLoan = _service.CreateLoanTransaction(_currentReader.ReaderId, _cart, _txtLoanNote.Text);
            _lastCreatedLoanId = createdLoan.Id;
            _btnPrintLastLoan.Enabled = true;

            var askPrint = MessageBox.Show(
                $"Lập phiếu mượn '{createdLoan.LoanCode}' thành công!\nBạn có muốn xem / in phiếu mượn ngay không?",
                "Thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            _cart.Clear();
            _txtLoanNote.Clear();
            BindCartGrid();
            RefreshReaderPanel();
            LoanDataChanged?.Invoke();

            if (askPrint == DialogResult.Yes)
                PrintLoanReceipt(createdLoan.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Giao dịch thất bại (đã hoàn tác dữ liệu):\n{ex.Message}",
                "Lỗi giao dịch", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ResetCreateForm()
    {
        _currentReader = null;
        _cart.Clear();
        _txtScanCard.Clear();
        _txtScanCopy.Clear();
        _txtLoanNote.Clear();
        BindCartGrid();
        _lblReaderSummary.ForeColor = TextGray;
        _lblReaderSummary.Text = "Chưa chọn độc giả. Quét mã thẻ để kiểm tra thông tin.";
        _lblEligibilityBanner.ForeColor = TextGray;
        _lblEligibilityBanner.Text = "Hệ thống tự động kiểm tra điều kiện mượn sách.";
        _txtScanCard.Focus();
    }

    // ==================== 2.4. QUẢN LÝ PHIẾU MƯỢN ====================

    private void BuildManageLoansTab(TabPage page)
    {
        var pnlFilter = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Color.White,
            Padding = new Padding(8, 7, 8, 5)
        };

        _txtSearchLoan = new TextBox { Width = 230, PlaceholderText = "Mã phiếu, mã/tên độc giả, mã thẻ..." };
        _txtSearchLoan.TextChanged += (_, _) => LoadLoansList();

        _cboStatusFilter = new ComboBox { Width = 155, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboStatusFilter.Items.AddRange(["Tất cả", "Đang mượn", "Đã trả một phần", "Đã trả đủ", "Quá hạn"]);
        _cboStatusFilter.SelectedIndex = 0;
        _cboStatusFilter.SelectedIndexChanged += (_, _) => LoadLoansList();

        _chkFilterDate = new CheckBox { Text = "Ngày mượn:", AutoSize = true, Padding = new Padding(6, 4, 0, 0) };
        _chkFilterDate.CheckedChanged += (_, _) => LoadLoansList();

        _dtpFrom = new DateTimePicker { Width = 105, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-1) };
        _dtpTo = new DateTimePicker { Width = 105, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        _dtpFrom.ValueChanged += (_, _) => { if (_chkFilterDate.Checked) LoadLoansList(); };
        _dtpTo.ValueChanged += (_, _) => { if (_chkFilterDate.Checked) LoadLoansList(); };

        var btnPrintSelected = MakeButton("Xem / In phiếu", SoftBlue, Primary, 120);
        btnPrintSelected.Click += (_, _) =>
        {
            if (GetSelectedLoan() is { } loan) PrintLoanReceipt(loan.LoanId);
        };

        var btnReaderHistory = MakeButton("Lịch sử mượn độc giả", Color.White, Primary, 155);
        btnReaderHistory.Click += (_, _) => ShowSelectedReaderHistory();

        pnlFilter.Controls.AddRange([
            new Label { Text = "Tìm:", AutoSize = true, Padding = new Padding(0, 6, 2, 0) },
            _txtSearchLoan,
            new Label { Text = "Trạng thái:", AutoSize = true, Padding = new Padding(8, 6, 2, 0) },
            _cboStatusFilter,
            _chkFilterDate,
            _dtpFrom,
            new Label { Text = "đến", AutoSize = true, Padding = new Padding(2, 6, 2, 0) },
            _dtpTo,
            btnPrintSelected,
            btnReaderHistory
        ]);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 240,
            Panel1MinSize = 140,
            Panel2MinSize = 150
        };

        _dgvLoans = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 32 }
        };
        _dgvLoans.Columns.Add("colLoanCode", "Mã phiếu");
        _dgvLoans.Columns.Add("colReaderCode", "Mã ĐG");
        _dgvLoans.Columns.Add("colReaderName", "Họ tên độc giả");
        _dgvLoans.Columns.Add("colCardCode", "Mã thẻ");
        _dgvLoans.Columns.Add("colBorrowedAt", "Ngày mượn");
        _dgvLoans.Columns.Add("colProgress", "Đã trả");
        _dgvLoans.Columns.Add("colLoanStatus", "Trạng thái");
        _dgvLoans.Columns.Add("colNote", "Ghi chú");
        _dgvLoans.SelectionChanged += (_, _) => LoadSelectedLoanDetails();
        split.Panel1.Controls.Add(_dgvLoans);

        var pnlDetailTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 42,
            BackColor = SoftBlue,
            Padding = new Padding(10, 6, 10, 6)
        };

        _lblDetailHeader = new Label
        {
            Text = "Chi tiết các bản sách thuộc phiếu mượn",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Primary,
            Location = new Point(10, 11)
        };

        var btnReturnSingleBook = MakeButton("Trả cuốn sách chọn", Green, Color.White, 155);
        btnReturnSingleBook.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnReturnSingleBook.Location = new Point(pnlDetailTop.Width - 165, 5);
        btnReturnSingleBook.Click += (_, _) => ReturnSelectedCopyInLoan();

        pnlDetailTop.Controls.AddRange([_lblDetailHeader, btnReturnSingleBook]);

        _dgvLoanDetails = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 32 }
        };
        _dgvLoanDetails.Columns.Add("colDetailCopy", "Mã bản sách");
        _dgvLoanDetails.Columns.Add("colDetailTitle", "Tên đầu sách");
        _dgvLoanDetails.Columns.Add("colDetailAuthor", "Tác giả");
        _dgvLoanDetails.Columns.Add("colCondBefore", "Tình trạng mượn");
        _dgvLoanDetails.Columns.Add("colDue", "Hạn trả");
        _dgvLoanDetails.Columns.Add("colReturned", "Ngày trả");
        _dgvLoanDetails.Columns.Add("colItemStatus", "Trạng thái");

        split.Panel2.Controls.Add(_dgvLoanDetails);
        split.Panel2.Controls.Add(pnlDetailTop);

        page.Controls.Add(split);
        page.Controls.Add(pnlFilter);
    }

    private void LoadLoansList()
    {
        DateTime? from = _chkFilterDate.Checked ? _dtpFrom.Value.Date : null;
        DateTime? to = _chkFilterDate.Checked ? _dtpTo.Value.Date : null;
        string status = _cboStatusFilter.SelectedItem?.ToString() ?? "Tất cả";

        _loans = _service.SearchLoans(_txtSearchLoan.Text, status, from, to);

        _dgvLoans.Rows.Clear();
        foreach (var l in _loans)
        {
            int idx = _dgvLoans.Rows.Add(
                l.LoanCode,
                l.ReaderCode,
                l.ReaderName,
                l.CardCode,
                l.BorrowedAt.ToString("dd/MM/yyyy HH:mm"),
                $"{l.ReturnedCopies}/{l.TotalCopies}",
                l.Status,
                l.Note
            );

            var cell = _dgvLoans.Rows[idx].Cells["colLoanStatus"];
            cell.Style.ForeColor = l.Status switch
            {
                "Đã trả đủ" => Green,
                "Quá hạn" => Red,
                "Đã trả một phần" => Orange,
                _ => Primary
            };
            cell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        if (_dgvLoans.Rows.Count > 0)
            _dgvLoans.Rows[0].Selected = true;
        else
        {
            _currentDetails.Clear();
            _dgvLoanDetails.Rows.Clear();
            _lblDetailHeader.Text = "Không có phiếu mượn phù hợp";
        }
    }

    private LoanSummaryDto? GetSelectedLoan()
    {
        if (_dgvLoans.CurrentRow == null || _dgvLoans.CurrentRow.Index < 0) return null;
        int idx = _dgvLoans.CurrentRow.Index;
        return idx < _loans.Count ? _loans[idx] : null;
    }

    private void LoadSelectedLoanDetails()
    {
        var loan = GetSelectedLoan();
        if (loan == null) return;

        _lblDetailHeader.Text = $"Chi tiết phiếu {loan.LoanCode} — Độc giả: {loan.ReaderName} ({loan.ReaderCode}) — {loan.Status}";
        _currentDetails = _service.GetLoanDetails(loan.LoanId);

        _dgvLoanDetails.Rows.Clear();
        foreach (var d in _currentDetails)
        {
            int idx = _dgvLoanDetails.Rows.Add(
                d.CopyCode,
                d.Title,
                d.Author,
                d.PhysicalConditionBefore,
                d.DueAt.ToString("dd/MM/yyyy"),
                d.ReturnedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa trả",
                d.ItemStatus
            );

            var cell = _dgvLoanDetails.Rows[idx].Cells["colItemStatus"];
            cell.Style.ForeColor = d.ItemStatus switch
            {
                "Đã trả" => Green,
                "Quá hạn" => Red,
                _ => Primary
            };
            cell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }

    private void ReturnSelectedCopyInLoan()
    {
        if (_dgvLoanDetails.CurrentRow == null || _dgvLoanDetails.CurrentRow.Index < 0)
        {
            MessageBox.Show("Vui lòng chọn một cuốn sách trong bảng chi tiết để trả.",
                "Chưa chọn sách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int idx = _dgvLoanDetails.CurrentRow.Index;
        if (idx >= _currentDetails.Count) return;

        var detail = _currentDetails[idx];
        if (detail.ReturnedAt != null)
        {
            MessageBox.Show($"Cuốn '{detail.Title}' ({detail.CopyCode}) đã được trả.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new Form
        {
            Text = $"Trả từng cuốn - {detail.CopyCode}",
            Size = new Size(400, 220),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblBook = new Label
        {
            Location = new Point(16, 14),
            Size = new Size(360, 38),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Text = $"Sách: {detail.Title} ({detail.CopyCode})"
        };

        var lblCond = new Label { Text = "Tình trạng sau trả:", Location = new Point(16, 62), AutoSize = true };
        var cboCond = new ComboBox { Location = new Point(145, 58), Width = 215, DropDownStyle = ComboBoxStyle.DropDownList };
        cboCond.Items.AddRange(["New", "Good", "Worn", "Damaged"]);
        cboCond.SelectedItem = detail.PhysicalConditionBefore is "New" or "Good" or "Worn"
            ? detail.PhysicalConditionBefore : "Good";

        var lblNote = new Label { Text = "Ghi chú:", Location = new Point(16, 98), AutoSize = true };
        var txtNote = new TextBox { Location = new Point(145, 94), Width = 215 };

        var btnOk = MakeButton("Xác nhận trả", Green, Color.White, 120);
        btnOk.Location = new Point(145, 136);
        btnOk.DialogResult = DialogResult.OK;

        var btnCancel = MakeButton("Hủy", Color.White, Color.Black, 85);
        btnCancel.Location = new Point(275, 136);
        btnCancel.DialogResult = DialogResult.Cancel;

        dlg.Controls.AddRange([lblBook, lblCond, cboCond, lblNote, txtNote, btnOk, btnCancel]);
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _service.ReturnSingleLoanDetail(detail.LoanDetailId, cboCond.SelectedItem?.ToString() ?? "Good", txtNote.Text);
            MessageBox.Show($"Đã trả cuốn '{detail.Title}' ({detail.CopyCode}) thành công.",
                "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadLoansList();
            LoanDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi trả sách", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowSelectedReaderHistory()
    {
        var loan = GetSelectedLoan();
        if (loan == null)
        {
            MessageBox.Show("Vui lòng chọn một phiếu mượn để tra cứu lịch sử độc giả.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var history = _service.GetReaderBorrowHistory(loan.ReaderId);

        using var dlg = new Form
        {
            Text = $"Lịch sử mượn sách — {loan.ReaderName} ({loan.ReaderCode})",
            Size = new Size(900, 500),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White
        };

        var lblHeader = new Label
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = SoftBlue,
            Padding = new Padding(12, 10, 12, 0),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Primary,
            Text = $"Độc giả: {loan.ReaderName} ({loan.ReaderCode})   |   Tổng số lượt mượn: {history.Count} cuốn"
        };

        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 30 }
        };
        dgv.Columns.Add("cLoan", "Mã phiếu");
        dgv.Columns.Add("cCopy", "Mã bản sách");
        dgv.Columns.Add("cTitle", "Tên đầu sách");
        dgv.Columns.Add("cBorrow", "Ngày mượn");
        dgv.Columns.Add("cDue", "Hạn trả");
        dgv.Columns.Add("cReturn", "Ngày trả");
        dgv.Columns.Add("cStatus", "Trạng thái");

        foreach (var h in history)
        {
            dgv.Rows.Add(
                h.LoanCode,
                h.CopyCode,
                h.Title,
                h.BorrowedAt.ToString("dd/MM/yyyy"),
                h.DueAt.ToString("dd/MM/yyyy"),
                h.ReturnedAt?.ToString("dd/MM/yyyy") ?? "—",
                h.ItemStatus
            );
        }

        dlg.Controls.Add(dgv);
        dlg.Controls.Add(lblHeader);
        dlg.ShowDialog(this);
    }

    private void PrintLoanReceipt(int loanId)
    {
        try
        {
            var receipt = _service.GetLoanReceipt(loanId);
            using var preview = new LoanPrintPreviewForm(receipt);
            preview.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể mở phiếu mượn:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Button MakeButton(string text, Color back, Color fore, int width)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(width, 32),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Margin = new Padding(4, 0, 4, 0),
            Cursor = Cursors.Hand
        };
        if (back == Color.White)
            btn.FlatAppearance.BorderColor = BorderGray;
        else
            btn.FlatAppearance.BorderSize = 0;
        return btn;
    }
}