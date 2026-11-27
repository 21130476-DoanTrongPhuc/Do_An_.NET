using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Forms.Books.ReaderPortal;

public class CurrentLoansControl : UserControl
{
    private static readonly Color Primary = Color.FromArgb(19, 98, 160);
    private static readonly Color SoftBlue = Color.FromArgb(232, 240, 250);
    private static readonly Color BgGray = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);
    private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
    private static readonly Color Green = Color.FromArgb(34, 160, 90);
    private static readonly Color Orange = Color.FromArgb(230, 145, 20);
    private static readonly Color Red = Color.FromArgb(200, 50, 50);

    public event Action<int>? OnOpenBookDetail;
    public event Action? OnDataChanged;

    private Label _lblQuotaBadge = null!;
    private Label _lblDueSoonBadge = null!;
    private Label _lblOverdueBadge = null!;
    private DataGridView _dgvLoans = null!;
    private DataGridView _dgvReservations = null!;

    public CurrentLoansControl()
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

        // 1. Thanh tổng quan hạn mức mượn trên cùng
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
            Text = "SÁCH ĐANG MƯỢN & ĐẶT TRƯỚC",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 38, 48),
            AutoSize = false,
            AutoEllipsis = true,
            Size = new Size(300, 24),
            Location = new Point(14, 18)
        };

        _lblQuotaBadge = new Label
        {
            Text = "Đang mượn: 0/3 cuốn",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Primary,
            BackColor = SoftBlue,
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(340, 15)
        };

        _lblDueSoonBadge = new Label
        {
            Text = "Sắp đến hạn: 0",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Orange,
            BackColor = Color.FromArgb(255, 247, 232),
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(495, 15)
        };

        _lblOverdueBadge = new Label
        {
            Text = "Quá hạn: 0",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Red,
            BackColor = Color.FromArgb(254, 242, 242),
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(625, 15)
        };

        var btnRefresh = new Button
        {
            Text = "Làm mới",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Primary,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(96, 32),
            Location = new Point(topBar.Width - 110, 12),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderColor = BorderGray;
        btnRefresh.Click += (_, _) => ReloadData();

        void LayoutTopBar()
        {
            btnRefresh.Left = topBar.ClientSize.Width - btnRefresh.Width - 12;
            _lblOverdueBadge.Left = btnRefresh.Left - _lblOverdueBadge.Width - 8;
            _lblDueSoonBadge.Left = _lblOverdueBadge.Left - _lblDueSoonBadge.Width - 8;
            _lblQuotaBadge.Left = _lblDueSoonBadge.Left - _lblQuotaBadge.Width - 8;
            lblTitle.Width = Math.Max(120, _lblQuotaBadge.Left - lblTitle.Left - 12);
        }

        topBar.Resize += (_, _) => LayoutTopBar();
        _lblQuotaBadge.TextChanged += (_, _) => LayoutTopBar();
        _lblDueSoonBadge.TextChanged += (_, _) => LayoutTopBar();
        _lblOverdueBadge.TextChanged += (_, _) => LayoutTopBar();

        topBar.Controls.AddRange(new Control[]
        {
            lblTitle, _lblQuotaBadge, _lblDueSoonBadge, _lblOverdueBadge, btnRefresh
        });
        LayoutTopBar();

        // 2. Khu vực dưới: Danh sách phiếu đặt trước đang chờ
        var bottomHost = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 210,
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

        var lblResTitle = new Label
        {
            Text = "Sách đang đặt trước (Chờ duyệt / Sẵn sàng nhận tại quầy)",
            Dock = DockStyle.Top,
            Height = 26,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 42, 52)
        };

        _dgvReservations = CreateBaseGrid();
        _dgvReservations.Columns.Add("ReservationId", "ID");
        _dgvReservations.Columns["ReservationId"]!.Visible = false;
        _dgvReservations.Columns.Add("BookId", "BookId");
        _dgvReservations.Columns["BookId"]!.Visible = false;
        _dgvReservations.Columns.Add("ReservationCode", "Mã đặt trước");
        _dgvReservations.Columns.Add("Title", "Tên sách");
        _dgvReservations.Columns.Add("Author", "Tác giả");
        _dgvReservations.Columns.Add("ReservedAt", "Ngày đặt");
        _dgvReservations.Columns.Add("ExpiresAt", "Hạn giữ chỗ");
        _dgvReservations.Columns.Add("Status", "Trạng thái");

        var colCancelRes = new DataGridViewButtonColumn
        {
            Name = "CancelReserve",
            HeaderText = "Thao tác",
            Text = "Hủy đặt",
            UseColumnTextForButtonValue = true,
            FillWeight = 60
        };
        _dgvReservations.Columns.Add(colCancelRes);

        _dgvReservations.Columns["ReservationCode"]!.FillWeight = 80;
        _dgvReservations.Columns["Title"]!.FillWeight = 145;
        _dgvReservations.Columns["Author"]!.FillWeight = 100;
        _dgvReservations.Columns["ReservedAt"]!.FillWeight = 70;
        _dgvReservations.Columns["ExpiresAt"]!.FillWeight = 70;
        _dgvReservations.Columns["Status"]!.FillWeight = 90;
        _dgvReservations.CellClick += DgvReservations_CellClick;

        bottomCard.Controls.Add(_dgvReservations);
        bottomCard.Controls.Add(lblResTitle);
        bottomHost.Controls.Add(bottomCard);

        // 3. Khu vực chính ở giữa: Bảng sách đang mượn
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

        var lblLoansTitle = new Label
        {
            Text = "Danh sách sách đang mượn (Bấm \"Gia hạn\" nếu còn lượt gia hạn và chưa quá hạn)",
            Dock = DockStyle.Top,
            Height = 26,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 42, 52)
        };

        _dgvLoans = CreateBaseGrid();
        _dgvLoans.Columns.Add("LoanDetailId", "ID");
        _dgvLoans.Columns["LoanDetailId"]!.Visible = false;
        _dgvLoans.Columns.Add("BookId", "BookId");
        _dgvLoans.Columns["BookId"]!.Visible = false;
        _dgvLoans.Columns.Add("LoanCode", "Mã phiếu mượn");
        _dgvLoans.Columns.Add("CopyCode", "Mã bản sao");
        _dgvLoans.Columns.Add("Title", "Tên sách");
        _dgvLoans.Columns.Add("Author", "Tác giả");
        _dgvLoans.Columns.Add("BorrowedAt", "Ngày mượn");
        _dgvLoans.Columns.Add("DueAt", "Hạn trả");
        _dgvLoans.Columns.Add("TimeLeft", "Tình trạng hạn");
        _dgvLoans.Columns.Add("RenewalInfo", "Gia hạn");

        var colRenew = new DataGridViewButtonColumn
        {
            Name = "Renew",
            HeaderText = "Gia hạn",
            Text = "Gia hạn",
            UseColumnTextForButtonValue = true,
            FillWeight = 60
        };
        var colDetail = new DataGridViewButtonColumn
        {
            Name = "Detail",
            HeaderText = "Chi tiết",
            Text = "Xem sách",
            UseColumnTextForButtonValue = true,
            FillWeight = 60
        };
        _dgvLoans.Columns.Add(colRenew);
        _dgvLoans.Columns.Add(colDetail);

        _dgvLoans.Columns["LoanCode"]!.FillWeight = 75;
        _dgvLoans.Columns["CopyCode"]!.FillWeight = 65;
        _dgvLoans.Columns["Title"]!.FillWeight = 135;
        _dgvLoans.Columns["Author"]!.FillWeight = 95;
        _dgvLoans.Columns["BorrowedAt"]!.FillWeight = 68;
        _dgvLoans.Columns["DueAt"]!.FillWeight = 68;
        _dgvLoans.Columns["TimeLeft"]!.FillWeight = 85;
        _dgvLoans.Columns["RenewalInfo"]!.FillWeight = 55;
        _dgvLoans.CellClick += DgvLoans_CellClick;

        centerCard.Controls.Add(_dgvLoans);
        centerCard.Controls.Add(lblLoansTitle);
        centerHost.Controls.Add(centerCard);

        Controls.Add(centerHost);
        Controls.Add(bottomHost);
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

            var reader = context.Users.FirstOrDefault(u => u.Id == readerId);
            int maxCopies = reader?.MaxActiveCopies ?? 3;

            var activeLoans = context.LoanDetails
                .Include(d => d.Loan)
                .Include(d => d.BookCopy)
                    .ThenInclude(cp => cp.Book)
                        .ThenInclude(b => b.Category)
                .Where(d => d.Loan.IdReader == readerId && d.ReturnedAt == null)
                .OrderBy(d => d.DueAt)
                .ToList();

            _dgvLoans.Rows.Clear();
            int dueSoonCount = 0;
            int overdueCount = 0;

            foreach (var item in activeLoans)
            {
                int daysLeft = (item.DueAt.Date - DateTime.Today).Days;
                int maxRenewals = item.BookCopy.Book.Category?.MaxRenewals ?? 1;

                string statusText;
                Color statusColor;
                if (daysLeft < 0)
                {
                    overdueCount++;
                    statusText = $"Quá hạn {Math.Abs(daysLeft)} ngày";
                    statusColor = Red;
                }
                else if (daysLeft <= 3)
                {
                    dueSoonCount++;
                    statusText = daysLeft == 0 ? "Hết hạn hôm nay" : $"Còn {daysLeft} ngày";
                    statusColor = Orange;
                }
                else
                {
                    statusText = $"Còn {daysLeft} ngày";
                    statusColor = Green;
                }

                int rowIdx = _dgvLoans.Rows.Add(
                    item.Id,
                    item.BookCopy.IdBook,
                    item.Loan.LoanCode,
                    item.BookCopy.CopyCode,
                    item.BookCopy.Book.Title,
                    item.BookCopy.Book.Author,
                    item.Loan.BorrowedAt.ToString("dd/MM/yyyy"),
                    item.DueAt.ToString("dd/MM/yyyy"),
                    statusText,
                    $"{item.RenewalCount}/{maxRenewals}"
                );

                var cell = _dgvLoans.Rows[rowIdx].Cells["TimeLeft"];
                cell.Style.ForeColor = statusColor;
                cell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            _lblQuotaBadge.Text = $"Đang mượn: {activeLoans.Count}/{maxCopies} cuốn";
            _lblDueSoonBadge.Text = $"Sắp đến hạn: {dueSoonCount}";
            _lblOverdueBadge.Text = $"Quá hạn: {overdueCount}";

            _lblDueSoonBadge.Left = _lblQuotaBadge.Right + 10;
            _lblOverdueBadge.Left = _lblDueSoonBadge.Right + 10;

            // Tải danh sách đặt trước đang hoạt động
            var reservations = context.Reservations
                .Include(r => r.Book)
                .Where(r => r.IdReader == readerId &&
                            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.ReadyForPickup))
                .OrderByDescending(r => r.ReservedAt)
                .ToList();

            _dgvReservations.Rows.Clear();
            foreach (var r in reservations)
            {
                string stText = r.Status == ReservationStatus.ReadyForPickup ? "Sẵn sàng nhận" : "Đang chờ duyệt";
                int idx = _dgvReservations.Rows.Add(
                    r.Id,
                    r.IdBook,
                    r.ReservationCode,
                    r.Book.Title,
                    r.Book.Author,
                    r.ReservedAt.ToString("dd/MM/yyyy"),
                    r.ExpiresAt?.ToString("dd/MM/yyyy") ?? "—",
                    stText
                );

                var stCell = _dgvReservations.Rows[idx].Cells["Status"];
                stCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                stCell.Style.ForeColor = r.Status == ReservationStatus.ReadyForPickup ? Green : Primary;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh sách sách đang mượn:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvLoans_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        string colName = _dgvLoans.Columns[e.ColumnIndex].Name;

        if (colName == "Detail")
        {
            if (int.TryParse(_dgvLoans.Rows[e.RowIndex].Cells["BookId"].Value?.ToString(), out int bookId))
            {
                OnOpenBookDetail?.Invoke(bookId);
            }
            return;
        }

        if (colName == "Renew")
        {
            if (int.TryParse(_dgvLoans.Rows[e.RowIndex].Cells["LoanDetailId"].Value?.ToString(), out int loanDetailId))
            {
                RenewLoanDetail(loanDetailId);
            }
        }
    }

    private void RenewLoanDetail(int loanDetailId)
    {
        try
        {
            using var context = new LibraryDbContext();
            var detail = context.LoanDetails
                .Include(d => d.Loan)
                .Include(d => d.BookCopy)
                    .ThenInclude(cp => cp.Book)
                        .ThenInclude(b => b.Category)
                .FirstOrDefault(d => d.Id == loanDetailId && d.ReturnedAt == null);

            if (detail == null) return;

            if (detail.DueAt.Date < DateTime.Today)
            {
                MessageBox.Show("Sách đã quá hạn trả nên không thể gia hạn trực tuyến. Vui lòng mang sách đến quầy thư viện!",
                    "Không thể gia hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maxRenewals = detail.BookCopy.Book.Category?.MaxRenewals ?? 1;
            int maxLoanDays = detail.BookCopy.Book.Category?.MaxLoanDays ?? 14;

            if (detail.RenewalCount >= maxRenewals)
            {
                MessageBox.Show($"Cuốn sách này đã đạt số lần gia hạn tối đa ({maxRenewals} lần)!",
                    "Hết lượt gia hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int bookId = detail.BookCopy.IdBook;
            bool hasOtherReservation = context.Reservations.Any(r =>
                r.IdBook == bookId &&
                r.IdReader != Session.CurrentReaderId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.ReadyForPickup));

            if (hasOtherReservation)
            {
                MessageBox.Show("Đầu sách này đang có độc giả khác đặt trước nên không thể gia hạn thêm!",
                    "Không thể gia hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime newDueDate = detail.DueAt.AddDays(maxLoanDays);
            var confirm = MessageBox.Show(
                $"Xác nhận gia hạn cuốn \"{detail.BookCopy.Book.Title}\"?\n" +
                $"Hạn trả hiện tại: {detail.DueAt:dd/MM/yyyy}\n" +
                $"Hạn trả mới: {newDueDate:dd/MM/yyyy} (+{maxLoanDays} ngày)",
                "Xác nhận gia hạn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            detail.DueAt = newDueDate;
            detail.RenewalCount += 1;
            context.SaveChanges();

            MessageBox.Show($"Gia hạn thành công! Hạn trả mới là {newDueDate:dd/MM/yyyy}.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ReloadData();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi gia hạn sách:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvReservations_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (_dgvReservations.Columns[e.ColumnIndex].Name != "CancelReserve") return;

        if (!int.TryParse(_dgvReservations.Rows[e.RowIndex].Cells["ReservationId"].Value?.ToString(), out int resId))
            return;

        string resCode = _dgvReservations.Rows[e.RowIndex].Cells["ReservationCode"].Value?.ToString() ?? "";
        var confirm = MessageBox.Show($"Bạn có chắc muốn hủy phiếu đặt trước {resCode}?",
            "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            using var context = new LibraryDbContext();
            var reservation = context.Reservations.Find(resId);
            if (reservation != null && (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.ReadyForPickup))
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.CancelledAt = DateTime.Now;

                if (reservation.IdBookCopy.HasValue)
                {
                    var copy = context.BookCopies.Find(reservation.IdBookCopy.Value);
                    if (copy != null && copy.CirculationStatus == CirculationStatus.Reserved)
                    {
                        copy.CirculationStatus = CirculationStatus.Available;
                    }
                }

                context.SaveChanges();
            }

            ReloadData();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi hủy đặt trước:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

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
            RowTemplate = { Height = 36 },
            ColumnHeadersHeight = 34,
            EnableHeadersVisualStyles = false,
            Font = new Font("Segoe UI", 9F)
        };
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        return dgv;
    }
}