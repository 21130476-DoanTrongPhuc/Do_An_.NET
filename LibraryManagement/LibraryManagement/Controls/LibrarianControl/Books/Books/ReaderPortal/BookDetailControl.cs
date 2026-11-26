using LibraryManagement.Data;
using LibraryManagement.Enums;
using LibraryManagement.Helpers;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Forms.Books.ReaderPortal;

public class BookDetailControl : UserControl
{
    private static readonly Color Primary = Color.FromArgb(19, 98, 160);
    private static readonly Color PrimaryDark = Color.FromArgb(14, 76, 126);
    private static readonly Color SoftBlue = Color.FromArgb(232, 240, 250);
    private static readonly Color BgGray = Color.FromArgb(240, 242, 245);
    private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);
    private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
    private static readonly Color Green = Color.FromArgb(34, 160, 90);
    private static readonly Color Orange = Color.FromArgb(230, 145, 20);
    private static readonly Color Red = Color.FromArgb(200, 50, 50);

    private const string IconFont = "Segoe MDL2 Assets";
    private const string GBook = "\uE82D";
    private const string GHeart = "\uEB51";
    private const string GHeartFill = "\uEB52";

    public event Action? OnBackRequested;
    public event Action? OnDataChanged;

    private int _currentBookId;
    private bool _isFavorite;
    private Reservations? _activeReservation;

    private Label _lblBookCode = null!;
    private Label _lblCategoryBadge = null!;
    private Label _lblTitle = null!;
    private Label _lblAuthor = null!;
    private Label _lblPublisher = null!;
    private Label _lblIsbn = null!;
    private Label _lblLoanRule = null!;
    private Label _lblAvailability = null!;
    private TextBox _txtDescription = null!;
    private TextBox _txtReserveNote = null!;
    private Button _btnFavorite = null!;
    private Button _btnReserve = null!;
    private Label _lblReserveStatus = null!;
    private DataGridView _dgvCopies = null!;

    public BookDetailControl(int bookId = 0)
    {
        Dock = DockStyle.Fill;
        BackColor = BgGray;
        Padding = new Padding(12, 0, 12, 12);

        BuildUi();

        if (bookId > 0)
        {
            LoadBook(bookId);
        }
    }

    private void BuildUi()
    {
        SuspendLayout();

        // 1. Thanh điều hướng trên cùng
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.White
        };
        topBar.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, topBar.Width - 1, topBar.Height - 1);

        var btnBack = new Button
        {
            Text = "← Quay lại danh sách",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Primary,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(165, 32),
            Location = new Point(12, 8),
            Cursor = Cursors.Hand
        };
        btnBack.FlatAppearance.BorderColor = BorderGray;
        btnBack.Click += (_, _) => OnBackRequested?.Invoke();

        var lblHeaderHint = new Label
        {
            Text = "THÔNG TIN CHI TIẾT ĐẦU SÁCH & DANH SÁCH BẢN SAO",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 52, 64),
            AutoSize = true,
            Location = new Point(192, 15)
        };

        _btnFavorite = new Button
        {
            Text = "♡ Yêu thích",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(50, 55, 65),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(135, 32),
            Location = new Point(topBar.Width - 148, 8),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };
        _btnFavorite.FlatAppearance.BorderColor = BorderGray;
        _btnFavorite.Click += (_, _) => ToggleFavorite();

        topBar.Controls.AddRange(new Control[] { btnBack, lblHeaderHint, _btnFavorite });

        // 2. Thẻ thông tin đầu sách + đặt trước
        var infoHost = new Panel
        {
            Dock = DockStyle.Top,
            Height = 250,
            Padding = new Padding(0, 10, 0, 0)
        };

        var infoCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        infoCard.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, infoCard.Width - 1, infoCard.Height - 1);

        var coverBox = new Label
        {
            Text = GBook,
            Font = new Font(IconFont, 34F),
            ForeColor = Color.White,
            BackColor = PrimaryDark,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(110, 150),
            Location = new Point(16, 16)
        };

        _lblBookCode = new Label
        {
            Text = "BK000",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Primary,
            BackColor = SoftBlue,
            AutoSize = true,
            Padding = new Padding(6, 2, 6, 2),
            Location = new Point(142, 16)
        };

        _lblCategoryBadge = new Label
        {
            Text = "Thể loại",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 70, 85),
            BackColor = Color.FromArgb(238, 240, 244),
            AutoSize = true,
            Padding = new Padding(6, 2, 6, 2),
            Location = new Point(215, 16)
        };

        _lblAvailability = new Label
        {
            Text = "● Đang tải...",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Green,
            AutoSize = true,
            Location = new Point(390, 18)
        };

        _lblTitle = new Label
        {
            Text = "Tên đầu sách",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 32, 42),
            AutoEllipsis = true,
            Size = new Size(520, 28),
            Location = new Point(142, 44),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _lblAuthor = new Label
        {
            Text = "Tác giả: ...",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(55, 62, 72),
            AutoSize = true,
            Location = new Point(142, 76)
        };

        _lblPublisher = new Label
        {
            Text = "Nhà xuất bản: ...",
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(142, 99)
        };

        _lblIsbn = new Label
        {
            Text = "ISBN: ...",
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(142, 121)
        };

        _lblLoanRule = new Label
        {
            Text = "Quy định mượn: ...",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Primary,
            AutoSize = true,
            Location = new Point(142, 144)
        };

        // Khung mô tả bên phải
        var lblDescTitle = new Label
        {
            Text = "Giới thiệu nội dung:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 52, 64),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(infoCard.Width - 360, 16)
        };

        _txtDescription = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(248, 249, 251),
            Font = new Font("Segoe UI", 9F),
            Size = new Size(344, 126),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(infoCard.Width - 360, 38)
        };

        // Thanh đặt trước ở đáy thẻ thông tin
        var reserveBar = new Panel
        {
            Height = 52,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(248, 250, 252)
        };
        reserveBar.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(BorderGray), 0, 0, reserveBar.Width, 0);

        var lblNote = new Label
        {
            Text = "Ghi chú đặt trước:",
            Font = new Font("Segoe UI", 9F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(16, 17)
        };

        _txtReserveNote = new TextBox
        {
            Font = new Font("Segoe UI", 9F),
            PlaceholderText = "Ví dụ: Cần mượn phục vụ đồ án cuối kỳ (không bắt buộc)...",
            Size = new Size(320, 25),
            Location = new Point(lblNote.Right + 12, 13)
        };

        _btnReserve = new Button
        {
            Text = "Đặt trước sách này",
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(165, 32),
            Location = new Point(464, 10),
            Cursor = Cursors.Hand
        };
        _btnReserve.FlatAppearance.BorderSize = 0;
        _btnReserve.Click += (_, _) => HandleReservationAction();

        _lblReserveStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Orange,
            AutoSize = true,
            Location = new Point(642, 17)
        };

        void LayoutReserveNote()
        {
            _txtReserveNote.Left = lblNote.Right + 12;
            _txtReserveNote.Width = Math.Max(140, _btnReserve.Left - _txtReserveNote.Left - 12);
        }
        reserveBar.Resize += (_, _) => LayoutReserveNote();

        reserveBar.Controls.AddRange(new Control[] { lblNote, _txtReserveNote, _btnReserve, _lblReserveStatus });
        LayoutReserveNote();

        infoCard.Resize += (_, _) =>
        {
            lblDescTitle.Left = infoCard.Width - 360;
            _txtDescription.Left = infoCard.Width - 360;
            _lblTitle.Width = Math.Max(200, _txtDescription.Left - 158);
        };

        infoCard.Controls.AddRange(new Control[]
        {
            coverBox, _lblBookCode, _lblCategoryBadge, _lblAvailability,
            _lblTitle, _lblAuthor, _lblPublisher, _lblIsbn, _lblLoanRule,
            lblDescTitle, _txtDescription, reserveBar
        });
        infoHost.Controls.Add(infoCard);

        // 3. Bảng danh sách các bản sao vật lý (BookCopies)
        var gridHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0)
        };

        var gridCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        gridCard.Paint += (_, e) =>
            e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, gridCard.Width - 1, gridCard.Height - 1);

        var lblGridTitle = new Label
        {
            Text = "Danh sách bản sao trong thư viện (Vị trí kệ & tình trạng lưu thông)",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 42, 52)
        };

        _dgvCopies = new DataGridView
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
            RowTemplate = { Height = 34 },
            ColumnHeadersHeight = 34,
            EnableHeadersVisualStyles = false,
            Font = new Font("Segoe UI", 9F)
        };
        _dgvCopies.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
        _dgvCopies.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        _dgvCopies.Columns.Add("CopyCode", "Mã bản sao");
        _dgvCopies.Columns.Add("ShelfLocation", "Vị trí kệ");
        _dgvCopies.Columns.Add("PhysicalCondition", "Tình trạng vật lý");
        _dgvCopies.Columns.Add("CirculationStatus", "Trạng thái lưu thông");
        _dgvCopies.Columns.Add("Note", "Ghi chú");

        _dgvCopies.Columns["CopyCode"]!.FillWeight = 75;
        _dgvCopies.Columns["ShelfLocation"]!.FillWeight = 85;
        _dgvCopies.Columns["PhysicalCondition"]!.FillWeight = 80;
        _dgvCopies.Columns["CirculationStatus"]!.FillWeight = 85;
        _dgvCopies.Columns["Note"]!.FillWeight = 130;

        gridCard.Controls.Add(_dgvCopies);
        gridCard.Controls.Add(lblGridTitle);
        gridHost.Controls.Add(gridCard);

        Controls.Add(gridHost);
        Controls.Add(infoHost);
        Controls.Add(topBar);

        ResumeLayout(true);
    }

    public void LoadBook(int bookId)
    {
        _currentBookId = bookId;

        try
        {
            using var context = new LibraryDbContext();
            var book = context.Books
                .Include(b => b.Category)
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.Id == bookId);

            if (book == null)
            {
                MessageBox.Show("Không tìm thấy thông tin đầu sách này!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                OnBackRequested?.Invoke();
                return;
            }

            int totalCopies = book.Copies.Count;
            int availableCopies = book.Copies.Count(c => c.CirculationStatus == CirculationStatus.Available);

            _lblBookCode.Text = book.BookCode;
            _lblCategoryBadge.Text = book.Category?.CategoryName ?? "Chưa phân loại";
            _lblCategoryBadge.Left = _lblBookCode.Right + 8;

            _lblAvailability.Text = availableCopies > 0
                ? $"● Còn {availableCopies}/{totalCopies} bản sao sẵn sàng"
                : $"● Đã hết sách (0/{totalCopies} bản sao)";
            _lblAvailability.ForeColor = availableCopies > 0 ? Green : Red;
            _lblAvailability.Left = _lblCategoryBadge.Right + 14;

            _lblTitle.Text = book.Title;
            _lblAuthor.Text = $"Tác giả: {book.Author}";
            string yearText = book.PublishedYear > 0 ? $" · Năm XB: {book.PublishedYear}" : "";
            _lblPublisher.Text = $"Nhà xuất bản: {book.Publisher ?? "Chưa cập nhật"}{yearText}";
            _lblIsbn.Text = $"Mã ISBN: {book.Isbn ?? "Chưa cập nhật"}";

            int maxDays = book.Category?.MaxLoanDays ?? 14;
            int maxRenewals = book.Category?.MaxRenewals ?? 1;
            _lblLoanRule.Text = $"Thời hạn mượn tối đa: {maxDays} ngày · Gia hạn tối đa: {maxRenewals} lần";

            _txtDescription.Text = string.IsNullOrWhiteSpace(book.Description)
                ? "Chưa có mô tả chi tiết cho đầu sách này."
                : book.Description;

            // Kiểm tra trạng thái yêu thích
            int readerId = Session.CurrentReaderId;
            _isFavorite = readerId > 0 && context.FavoriteBooks
                .Any(f => f.ReaderId == readerId && f.BookId == bookId);
            UpdateFavoriteButtonUi();

            // Kiểm tra trạng thái đặt trước hiện tại của độc giả
            _activeReservation = readerId > 0
                ? context.Reservations.FirstOrDefault(r =>
                    r.IdReader == readerId &&
                    r.IdBook == bookId &&
                    (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.ReadyForPickup))
                : null;
            UpdateReservationUi();

            // Nạp danh sách bản sao
            _dgvCopies.Rows.Clear();
            foreach (var cp in book.Copies.OrderBy(c => c.CopyCode))
            {
                int idx = _dgvCopies.Rows.Add(
                    cp.CopyCode,
                    string.IsNullOrWhiteSpace(cp.ShelfLocation) ? "Chưa xếp kệ" : cp.ShelfLocation,
                    FormatCondition(cp.PhysicalCondition),
                    FormatCirculationStatus(cp.CirculationStatus),
                    cp.Notes ?? ""
                );

                var statusCell = _dgvCopies.Rows[idx].Cells["CirculationStatus"];
                statusCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                statusCell.Style.ForeColor = cp.CirculationStatus switch
                {
                    CirculationStatus.Available   => Green,
                    CirculationStatus.Borrowed    => Orange,
                    CirculationStatus.Reserved    => Primary,
                    CirculationStatus.Maintenance => Orange,
                    // CirculationStatus.Lost        => Red,
                    CirculationStatus.Disposed    => Red,
                    _                             => Red
                };
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải chi tiết sách:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ToggleFavorite()
    {
        if (Session.CurrentReaderId <= 0 || _currentBookId <= 0) return;

        try
        {
            using var context = new LibraryDbContext();
            var existing = context.FavoriteBooks
                .FirstOrDefault(f => f.ReaderId == Session.CurrentReaderId && f.BookId == _currentBookId);

            if (existing != null)
            {
                context.FavoriteBooks.Remove(existing);
                _isFavorite = false;
            }
            else
            {
                context.FavoriteBooks.Add(new FavoriteBook
                {
                    ReaderId = Session.CurrentReaderId,
                    BookId = _currentBookId,
                    AddedDate = DateTime.Now
                });
                _isFavorite = true;
            }

            context.SaveChanges();
            UpdateFavoriteButtonUi();
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể cập nhật yêu thích:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleReservationAction()
    {
        if (Session.CurrentReaderId <= 0 || _currentBookId <= 0) return;

        try
        {
            using var context = new LibraryDbContext();
            int readerId = Session.CurrentReaderId;

            // Trường hợp đang có phiếu đặt trước -> cho phép hủy đặt trước
            if (_activeReservation != null)
            {
                var confirm = MessageBox.Show(
                    $"Bạn muốn hủy phiếu đặt trước {_activeReservation.ReservationCode}?",
                    "Xác nhận hủy đặt trước",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                var entity = context.Reservations.Find(_activeReservation.Id);
                if (entity != null && (entity.Status == ReservationStatus.Pending || entity.Status == ReservationStatus.ReadyForPickup))
                {
                    entity.Status = ReservationStatus.Cancelled;
                    entity.CancelledAt = DateTime.Now;

                    if (entity.IdBookCopy.HasValue)
                    {
                        var copy = context.BookCopies.Find(entity.IdBookCopy.Value);
                        if (copy != null && copy.CirculationStatus == CirculationStatus.Reserved)
                        {
                            copy.CirculationStatus = CirculationStatus.Available;
                        }
                    }

                    context.SaveChanges();
                }

                LoadBook(_currentBookId);
                OnDataChanged?.Invoke();
                return;
            }

            // Kiểm tra thẻ độc giả hợp lệ
            var activeCard = context.ReaderCards
                .FirstOrDefault(c => c.IdReader == readerId && c.Status == ReaderCardStatus.Active);
            if (activeCard == null || activeCard.ExpiresAt.Date < DateTime.Today)
            {
                MessageBox.Show("Thẻ độc giả của bạn chưa kích hoạt hoặc đã hết hạn, không thể đặt trước sách!",
                    "Không đủ điều kiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra xem độc giả có đang mượn chính đầu sách này không
            bool alreadyBorrowing = context.LoanDetails
                .Include(d => d.Loan)
                .Include(d => d.BookCopy)
                .Any(d => d.Loan.IdReader == readerId &&
                          d.ReturnedAt == null &&
                          d.BookCopy.IdBook == _currentBookId);

            if (alreadyBorrowing)
            {
                MessageBox.Show("Bạn đang mượn một bản sao của đầu sách này rồi!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string code = $"RSV{DateTime.Now:yyyyMMddHHmmss}";
            var reservation = new Reservations
            {
                ReservationCode = code,
                IdReader = readerId,
                IdBook = _currentBookId,
                ReservedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(3),
                Status = ReservationStatus.Pending,
                Note = _txtReserveNote.Text.Trim()
            };

            context.Reservations.Add(reservation);
            context.SaveChanges();

            _txtReserveNote.Clear();
            MessageBox.Show($"Đặt trước thành công!\nMã phiếu đặt: {code}\nHiệu lực giữ chỗ đến: {reservation.ExpiresAt:dd/MM/yyyy}",
                "Đặt trước sách", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadBook(_currentBookId);
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể xử lý đặt trước:\n{ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateFavoriteButtonUi()
    {
        if (_isFavorite)
        {
            _btnFavorite.Text = "♥ Đã yêu thích";
            _btnFavorite.ForeColor = Red;
            _btnFavorite.BackColor = Color.FromArgb(254, 242, 242);
        }
        else
        {
            _btnFavorite.Text = "♡ Yêu thích";
            _btnFavorite.ForeColor = Color.FromArgb(50, 55, 65);
            _btnFavorite.BackColor = Color.White;
        }
    }

    private void UpdateReservationUi()
    {
        if (_activeReservation != null)
        {
            _btnReserve.Text = "Hủy đặt trước";
            _btnReserve.BackColor = Red;
            _txtReserveNote.Enabled = false;
            string st = _activeReservation.Status == ReservationStatus.ReadyForPickup
                ? "Sẵn sàng nhận tại quầy"
                : "Đang chờ duyệt";
            _lblReserveStatus.Text = $"Bạn đã đặt trước ({_activeReservation.ReservationCode}) · Trạng thái: {st}";
            _lblReserveStatus.ForeColor = Green;
        }
        else
        {
            _btnReserve.Text = "Đặt trước sách này";
            _btnReserve.BackColor = Primary;
            _txtReserveNote.Enabled = true;
            _lblReserveStatus.Text = "";
        }
    }

    private static string FormatCondition(BookCondition condition) => condition switch
    {
        BookCondition.New     => "Mới",
        BookCondition.Good    => "Tốt",
        BookCondition.Worn    => "Cũ / Sờn",
        BookCondition.Damaged => "Hư hỏng",
        BookCondition.Lost    => "Bị mất",
        _                     => condition.ToString()   // ✅ enum → string
    };
    private static string FormatCirculationStatus(CirculationStatus status) => status switch
    {
        CirculationStatus.Available   => "Sẵn sàng cho mượn",
        CirculationStatus.Borrowed    => "Đang được mượn",
        CirculationStatus.Reserved    => "Đã đặt trước",
        CirculationStatus.Maintenance => "Đang bảo trì",
        // CirculationStatus.Lost        => "Báo mất",
        CirculationStatus.Disposed    => "Đã thanh lý",
        _                             => status.ToString()   // ✅ enum → string
    };
}