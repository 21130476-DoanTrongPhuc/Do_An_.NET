using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Helpers;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Forms.Books.ReaderPortal;

public class BorrowHistoryControl : UserControl
{
    private static readonly Color Primary = Color.FromArgb(19, 98, 160);
    private static readonly Color SoftBlue = Color.FromArgb(232, 240, 250);
    private static readonly Color BgGray = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);
    private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
    private static readonly Color Green = Color.FromArgb(34, 160, 90);
    private static readonly Color Orange = Color.FromArgb(230, 145, 20);
    private static readonly Color Red = Color.FromArgb(200, 50, 50);

    private const int LateFeePerDayVnd = 5000;

    public event Action<int>? OnOpenBookDetail;

    private Label _lblTotalBadge = null!;
    private Label _lblReturnedBadge = null!;
    private Label _lblUnpaidFineBadge = null!;
    private TextBox _txtKeyword = null!;
    private ComboBox _cboStatus = null!;
    private DataGridView _dgvHistory = null!;
    private DataGridView _dgvFines = null!;

    private List<HistoryRowItem> _allItems = new();

    public BorrowHistoryControl()
    {
        Dock = DockStyle.Fill;
        BackColor = BgGray;
        Padding = new Padding(12, 0, 12, 12);

        BuildUi();
        ReloadData();
    }

    private void BuildUi()
    {
        SuspendLayout();

        // 1. Thanh tiêu đề + thống kê nhanh
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.White
        };
        topBar.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, topBar.Width - 1, topBar.Height - 1);

        var lblTitle = new Label
        {
            Text = "LỊCH SỬ MƯỢN TRẢ & TIỀN PHẠT",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 38, 48),
            AutoSize = true,
            Location = new Point(14, 18)
        };

        _lblTotalBadge = new Label
        {
            Text = "Tổng lượt mượn: 0",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Primary,
            BackColor = SoftBlue,
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(310, 15)
        };

        _lblReturnedBadge = new Label
        {
            Text = "Đã trả: 0",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Green,
            BackColor = Color.FromArgb(236, 250, 242),
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(455, 15)
        };

        _lblUnpaidFineBadge = new Label
        {
            Text = "Phạt chưa thanh toán: 0 đ",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Red,
            BackColor = Color.FromArgb(254, 242, 242),
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(555, 15)
        };

        topBar.Controls.AddRange(new Control[]
        {
            lblTitle, _lblTotalBadge, _lblReturnedBadge, _lblUnpaidFineBadge
        });

        // 2. Thanh tìm kiếm & lọc trạng thái
        var filterHost = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            Padding = new Padding(0, 10, 0, 0)
        };

        var filterBar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        filterBar.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, filterBar.Width - 1, filterBar.Height - 1);

        _txtKeyword = new TextBox
        {
            Font = new Font("Segoe UI", 9.5F),
            PlaceholderText = "Tìm tên sách, tác giả, mã phiếu mượn hoặc mã bản sao...",
            Size = new Size(340, 25),
            Location = new Point(12, 10)
        };
        _txtKeyword.TextChanged += (_, _) => ApplyFilter();

        var lblStatus = new Label
        {
            Text = "Trạng thái:",
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(368, 13)
        };

        _cboStatus = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9F),
            Size = new Size(170, 25),
            Location = new Point(440, 10)
        };
        _cboStatus.Items.AddRange(new object[]
        {
            "Tất cả trạng thái",
            "Đã trả",
            "Đang mượn",
            "Quá hạn",
            "Có tiền phạt"
        });
        _cboStatus.SelectedIndex = 0;
        _cboStatus.SelectedIndexChanged += (_, _) => ApplyFilter();

        var btnRefresh = new Button
        {
            Text = "Làm mới",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Primary,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(96, 30),
            Location = new Point(filterBar.Width - 110, 8),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderColor = BorderGray;
        btnRefresh.Click += (_, _) => ReloadData();

        void LayoutFilterBar()
        {
            btnRefresh.Left = filterBar.ClientSize.Width - btnRefresh.Width - 12;
            _cboStatus.Left = btnRefresh.Left - _cboStatus.Width - 12;
            lblStatus.Left = _cboStatus.Left - lblStatus.Width - 8;
            _txtKeyword.Width = Math.Max(140, lblStatus.Left - _txtKeyword.Left - 12);
            lblStatus.Top = (filterBar.ClientSize.Height - lblStatus.Height) / 2;
        }

        filterBar.Resize += (_, _) => LayoutFilterBar();
        filterBar.Controls.AddRange(new Control[] { _txtKeyword, lblStatus, _cboStatus, btnRefresh });
        LayoutFilterBar();
        filterHost.Controls.Add(filterBar);

        // 3. Khung dưới: Danh sách biên lai phạt & thanh toán
        var bottomHost = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 195,
            Padding = new Padding(0, 10, 0, 0)
        };

        var bottomCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        bottomCard.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, bottomCard.Width - 1, bottomCard.Height - 1);

        var lblFineTitle = new Label
        {
            Text = "Chi tiết các khoản phạt & tình trạng thanh toán",
            Dock = DockStyle.Top,
            Height = 26,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 42, 52)
        };

        _dgvFines = CreateBaseGrid();
        _dgvFines.Columns.Add("FineCode", "Mã phiếu phạt");
        _dgvFines.Columns.Add("LoanCode", "Mã phiếu mượn");
        _dgvFines.Columns.Add("Title", "Tên sách");
        _dgvFines.Columns.Add("ViolationType", "Lý do phạt");
        _dgvFines.Columns.Add("CreatedAt", "Ngày lập");
        _dgvFines.Columns.Add("AmountVnd", "Tổng phạt");
        _dgvFines.Columns.Add("PaidVnd", "Đã đóng");
        _dgvFines.Columns.Add("RemainingVnd", "Còn nợ");
        _dgvFines.Columns.Add("PayStatus", "Trạng thái");

        _dgvFines.Columns["FineCode"]!.FillWeight = 75;
        _dgvFines.Columns["LoanCode"]!.FillWeight = 75;
        _dgvFines.Columns["Title"]!.FillWeight = 135;
        _dgvFines.Columns["ViolationType"]!.FillWeight = 90;
        _dgvFines.Columns["CreatedAt"]!.FillWeight = 70;
        _dgvFines.Columns["AmountVnd"]!.FillWeight = 70;
        _dgvFines.Columns["PaidVnd"]!.FillWeight = 70;
        _dgvFines.Columns["RemainingVnd"]!.FillWeight = 70;
        _dgvFines.Columns["PayStatus"]!.FillWeight = 85;

        bottomCard.Controls.Add(_dgvFines);
        bottomCard.Controls.Add(lblFineTitle);
        bottomHost.Controls.Add(bottomCard);

        // 4. Khung chính giữa: Bảng lịch sử mượn trả
        var centerHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0)
        };

        var centerCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        centerCard.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, centerCard.Width - 1, centerCard.Height - 1);

        var lblGridTitle = new Label
        {
            Text = "Danh sách chi tiết các lượt mượn sách",
            Dock = DockStyle.Top,
            Height = 26,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 42, 52)
        };

        _dgvHistory = CreateBaseGrid();
        _dgvHistory.Columns.Add("BookId", "BookId");
        _dgvHistory.Columns["BookId"]!.Visible = false;
        _dgvHistory.Columns.Add("LoanCode", "Mã phiếu mượn");
        _dgvHistory.Columns.Add("CopyCode", "Mã bản sao");
        _dgvHistory.Columns.Add("Title", "Tên sách");
        _dgvHistory.Columns.Add("BorrowedAt", "Ngày mượn");
        _dgvHistory.Columns.Add("DueAt", "Hạn trả");
        _dgvHistory.Columns.Add("ReturnedAt", "Ngày trả thực tế");
        _dgvHistory.Columns.Add("Status", "Trạng thái");
        _dgvHistory.Columns.Add("FineSummary", "Tiền phạt");

        var colDetail = new DataGridViewButtonColumn
        {
            Name = "Detail",
            HeaderText = "Chi tiết",
            Text = "Xem sách",
            UseColumnTextForButtonValue = true,
            FillWeight = 58
        };
        _dgvHistory.Columns.Add(colDetail);

        _dgvHistory.Columns["LoanCode"]!.FillWeight = 75;
        _dgvHistory.Columns["CopyCode"]!.FillWeight = 65;
        _dgvHistory.Columns["Title"]!.FillWeight = 145;
        _dgvHistory.Columns["BorrowedAt"]!.FillWeight = 68;
        _dgvHistory.Columns["DueAt"]!.FillWeight = 68;
        _dgvHistory.Columns["ReturnedAt"]!.FillWeight = 75;
        _dgvHistory.Columns["Status"]!.FillWeight = 72;
        _dgvHistory.Columns["FineSummary"]!.FillWeight = 90;

        _dgvHistory.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_dgvHistory.Columns[e.ColumnIndex].Name == "Detail" &&
                int.TryParse(_dgvHistory.Rows[e.RowIndex].Cells["BookId"].Value?.ToString(), out int bookId))
            {
                OnOpenBookDetail?.Invoke(bookId);
            }
        };
        _dgvHistory.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 &&
                int.TryParse(_dgvHistory.Rows[e.RowIndex].Cells["BookId"].Value?.ToString(), out int bookId))
            {
                OnOpenBookDetail?.Invoke(bookId);
            }
        };

        centerCard.Controls.Add(_dgvHistory);
        centerCard.Controls.Add(lblGridTitle);
        centerHost.Controls.Add(centerCard);

        Controls.Add(centerHost);
        Controls.Add(bottomHost);
        Controls.Add(filterHost);
        Controls.Add(topBar);

        ResumeLayout(true);
    }

    public void ReloadData()
    {
        if (Session.CurrentReaderId <= 0) return;

        try
        {
            int readerId = Session.CurrentReaderId;
            using var context = new LibraryDbContext();

            var details = context.LoanDetails
                .Include(d => d.Loan)
                .Include(d => d.BookCopy)
                    .ThenInclude(cp => cp.Book)
                .Include(d => d.Fines)
                    .ThenInclude(f => f.FinePayments)
                .Where(d => d.Loan.IdReader == readerId)
                .OrderByDescending(d => d.Loan.BorrowedAt)
                .ToList();

            _allItems = details.Select(d =>
            {
                var fines = d.Fines ?? Enumerable.Empty<Fines>();
                int dbFine = fines.Sum(f => f.AmountVnd);
                int paid = fines.SelectMany(f => f.FinePayments ?? Enumerable.Empty<FinePayments>())
                                .Sum(p => p.AmountVnd);

                int overdueDays = d.ReturnedAt == null && d.DueAt.Date < DateTime.Today
                    ? (DateTime.Today - d.DueAt.Date).Days
                    : 0;

                int totalFine = dbFine > 0 ? dbFine : overdueDays * LateFeePerDayVnd;
                bool isEstimated = dbFine == 0 && overdueDays > 0;

                string status = d.ReturnedAt != null
                    ? "Đã trả"
                    : (d.DueAt.Date < DateTime.Today ? "Quá hạn" : "Đang mượn");

                return new HistoryRowItem
                {
                    BookId = d.BookCopy.IdBook,
                    LoanCode = d.Loan.LoanCode,
                    CopyCode = d.BookCopy.CopyCode,
                    Title = d.BookCopy.Book.Title,
                    Author = d.BookCopy.Book.Author,
                    BorrowedAt = d.Loan.BorrowedAt,
                    DueAt = d.DueAt,
                    ReturnedAt = d.ReturnedAt,
                    Status = status,
                    TotalFine = totalFine,
                    PaidFine = paid,
                    IsEstimatedFine = isEstimated
                };
            }).ToList();

            int returnedCount = _allItems.Count(x => x.Status == "Đã trả");
            int totalUnpaid = _allItems.Sum(x => Math.Max(0, x.TotalFine - x.PaidFine));

            _lblTotalBadge.Text = $"Tổng lượt mượn: {_allItems.Count}";
            _lblReturnedBadge.Text = $"Đã trả: {returnedCount}";
            _lblUnpaidFineBadge.Text = $"Phạt chưa thanh toán: {totalUnpaid:N0} đ";

            _lblReturnedBadge.Left = _lblTotalBadge.Right + 10;
            _lblUnpaidFineBadge.Left = _lblReturnedBadge.Right + 10;

            // Nạp bảng biên lai phạt chính thức trong CSDL
            _dgvFines.Rows.Clear();
            var allFines = details
                .SelectMany(d => (d.Fines ?? Enumerable.Empty<Fines>()).Select(f => new { Detail = d, Fine = f }))
                .OrderByDescending(x => x.Fine.CreatedAt)
                .ToList();

            foreach (var x in allFines)
            {
                int paidAmt = (x.Fine.FinePayments ?? Enumerable.Empty<FinePayments>()).Sum(p => p.AmountVnd);
                int remain = Math.Max(0, x.Fine.AmountVnd - paidAmt);
                bool isPaid = remain == 0;

                int idx = _dgvFines.Rows.Add(
                    x.Fine.FineCode,
                    x.Detail.Loan.LoanCode,
                    x.Detail.BookCopy.Book.Title,
                    FormatViolation(x.Fine.ViolationType),
                    x.Fine.CreatedAt.ToString("dd/MM/yyyy"),
                    $"{x.Fine.AmountVnd:N0} đ",
                    $"{paidAmt:N0} đ",
                    $"{remain:N0} đ",
                    isPaid ? "Đã thanh toán" : "Còn nợ phạt"
                );

                var stCell = _dgvFines.Rows[idx].Cells["PayStatus"];
                stCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                stCell.Style.ForeColor = isPaid ? Green : Red;
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải lịch sử mượn trả:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilter()
    {
        if (_cboStatus.SelectedIndex < 0) return;

        string keyword = _txtKeyword.Text.Trim().ToLowerInvariant();
        string selectedStatus = _cboStatus.SelectedItem?.ToString() ?? "Tất cả trạng thái";

        IEnumerable<HistoryRowItem> query = _allItems;

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(x =>
                x.Title.ToLowerInvariant().Contains(keyword) ||
                x.Author.ToLowerInvariant().Contains(keyword) ||
                x.LoanCode.ToLowerInvariant().Contains(keyword) ||
                x.CopyCode.ToLowerInvariant().Contains(keyword));
        }

        query = selectedStatus switch
        {
            "Đã trả" => query.Where(x => x.Status == "Đã trả"),
            "Đang mượn" => query.Where(x => x.Status == "Đang mượn"),
            "Quá hạn" => query.Where(x => x.Status == "Quá hạn"),
            "Có tiền phạt" => query.Where(x => x.TotalFine > 0),
            _ => query
        };

        _dgvHistory.Rows.Clear();
        foreach (var item in query)
        {
            int remain = Math.Max(0, item.TotalFine - item.PaidFine);
            string fineText = item.TotalFine == 0
                ? "0 đ"
                : item.IsEstimatedFine
                    ? $"{item.TotalFine:N0} đ (Tạm tính)"
                    : remain == 0
                        ? $"{item.TotalFine:N0} đ (Đã đóng)"
                        : $"Nợ {remain:N0} đ";

            int idx = _dgvHistory.Rows.Add(
                item.BookId,
                item.LoanCode,
                item.CopyCode,
                item.Title,
                item.BorrowedAt.ToString("dd/MM/yyyy"),
                item.DueAt.ToString("dd/MM/yyyy"),
                item.ReturnedAt?.ToString("dd/MM/yyyy") ?? "Chưa trả",
                item.Status,
                fineText
            );

            var row = _dgvHistory.Rows[idx];
            row.Cells["Status"].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            row.Cells["Status"].Style.ForeColor = item.Status switch
            {
                "Đã trả" => Green,
                "Quá hạn" => Red,
                _ => Primary
            };

            if (item.TotalFine > 0)
            {
                row.Cells["FineSummary"].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                row.Cells["FineSummary"].Style.ForeColor = remain > 0 ? Red : Green;
            }
        }
    }

        private static string FormatViolation(ViolationType type) => type switch
    {
        ViolationType.Overdue => "Trả sách quá hạn",
        ViolationType.Damaged => "Làm hư hỏng sách",
        ViolationType.Lost => "Làm mất sách",
        ViolationType.Other => "Vi phạm khác",
        _ => type.ToString()
    };                            

    private static DataGridView CreateBaseGrid()
    {
        var dgv = new DataGridView
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
            RowTemplate = { Height = 35 },
            ColumnHeadersHeight = 34,
            EnableHeadersVisualStyles = false,
            Font = new Font("Segoe UI", 9F)
        };
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        return dgv;
    }

    private sealed class HistoryRowItem
    {
        public int BookId { get; set; }
        public string LoanCode { get; set; } = string.Empty;
        public string CopyCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime BorrowedAt { get; set; }
        public DateTime DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalFine { get; set; }
        public int PaidFine { get; set; }
        public bool IsEstimatedFine { get; set; }
    }
}