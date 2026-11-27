using System.Drawing.Drawing2D;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.ReservationManagement
{
    public partial class ReservationManagementControl : UserControl
    {
        // ====== Bảng màu đồng bộ với LibrarianFormMain ======
        private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
        private static readonly Color BgGray     = Color.FromArgb(240, 242, 245);
        private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
        private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
        private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
        private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
        private static readonly Color Green      = Color.FromArgb(34, 160, 90);
        private static readonly Color Red        = Color.FromArgb(200, 40, 40);
        private const string IconFont = "Segoe MDL2 Assets";

        // Glyph
        private const string GRefresh  = "\uE72C";
        private const string GSearch   = "\uE721";
        private const string GCheck    = "\uE73E";
        private const string GMail     = "\uE715";
        private const string GClose    = "\uE711";

        private readonly ReservationService _reservationService;

        public event Action? ReservationDataChanged;

        // ====== Controls danh sách trái ======
        private TextBox _txtSearch = null!;
        private ComboBox _cboStatusFilter = null!;
        private DataGridView _dgvReservations = null!;
        private Label _lblSummary = null!;

        // ====== Controls xử lý duyệt bên phải ======
        private ReservationDetailResponse? _currentDetail;
        private Label _lblResCode = null!;
        private Label _lblReaderInfo = null!;
        private Label _lblBookInfo = null!;
        private Label _lblTimeInfo = null!;
        private ComboBox _cboAvailableCopies = null!;
        private NumericUpDown _numHoldDays = null!;
        private TextBox _txtNotifyMessage = null!;
        private Button _btnApproveAndNotify = null!;
        private Button _btnResendNotify = null!;
        private Button _btnCancelRes = null!;

        public ReservationManagementControl(ReservationService reservationService)
        {
            _reservationService = reservationService;
            BackColor = BgGray;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
            Load += async (_, _) => await LoadReservationsAsync();
        }

        // ============================================================
        //  DỰNG GIAO DIỆN
        // ============================================================
        private void BuildUi()
        {
            SuspendLayout();

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = BgGray };
            pnlTop.Controls.Add(BuildFilterBar());
            pnlTop.Controls.Add(BuildHeader());

            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };

            var pnlRightHost = new Panel { Dock = DockStyle.Right, Width = 400, Padding = new Padding(10, 0, 0, 0) };
            pnlRightHost.Controls.Add(BuildApprovalGroup());

            var grpList = new GroupBox
            {
                Text = "Danh sách yêu cầu đặt trước sách (Ưu tiên theo thời gian đăng ký)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10)
            };
            _dgvReservations = BuildGrid();
            grpList.Controls.Add(_dgvReservations);

            pnlMain.Controls.Add(grpList);
            pnlMain.Controls.Add(pnlRightHost);

            Controls.Add(pnlMain);
            Controls.Add(pnlTop);

            ResumeLayout(true);
        }

        private Panel BuildHeader()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = SoftBlue };
            pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

            var lblTitle = new Label
            {
                Text = "QUẢN LÝ ĐẶT TRƯỚC SÁCH && THÔNG BÁO NHẬN SÁCH",
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(14, 8)
            };
            pnl.Controls.Add(lblTitle);

            _lblSummary = new Label
            {
                Text = "Đang tải danh sách đặt trước...",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(16, 32)
            };
            pnl.Controls.Add(_lblSummary);

            var btnRefresh = CreateActionButton("Làm mới dữ liệu", GRefresh, Primary, Color.White);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += async (_, _) => await LoadReservationsAsync();

            pnl.Controls.Add(btnRefresh);

            void LayoutHeader()
            {
                btnRefresh.Location = new Point(
                    Math.Max(lblTitle.Right + 16, pnl.ClientSize.Width - btnRefresh.Width - 12),
                    (pnl.ClientSize.Height - btnRefresh.Height) / 2
                );
            }

            pnl.Resize += (_, _) => LayoutHeader();
            LayoutHeader();

            return pnl;
        }

        private Panel BuildFilterBar()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(12, 8, 12, 8)
            };
            pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

            var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, ForeColor = TextGray };

            _txtSearch = new TextBox
            {
                Width = 260,
                PlaceholderText = "Mã đặt trước, tên độc giả hoặc tên sách..."
            };
            _txtSearch.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter) await LoadReservationsAsync();
            };

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = TextGray };

            _cboStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 195
            };
            _cboStatusFilter.Items.AddRange(new object[]
            {
                "Tất cả yêu cầu",
                "Chờ duyệt (Pending)",
                "Đã duyệt - Sẵn sàng lấy",
                "Đã hoàn tất (Đã mượn)",
                "Đã hủy / Hết hạn giữ"
            });
            _cboStatusFilter.SelectedIndex = 1;
            _cboStatusFilter.SelectedIndexChanged += async (_, _) => await LoadReservationsAsync();

            var btnSearch = CreateActionButton("Lọc danh sách", GSearch, SoftBlue, Primary);
            btnSearch.MinimumSize = new Size(0, 31);
            btnSearch.MaximumSize = new Size(0, 31);
            btnSearch.Click += async (_, _) => await LoadReservationsAsync();

            void LayoutFilterBar()
            {
                int cy = pnl.ClientSize.Height / 2;

                btnSearch.Location = new Point(pnl.ClientSize.Width - btnSearch.Width - 12, cy - btnSearch.Height / 2);
                _cboStatusFilter.Location = new Point(btnSearch.Left - _cboStatusFilter.Width - 12, cy - _cboStatusFilter.Height / 2);
                lblStatus.Location = new Point(_cboStatusFilter.Left - lblStatus.Width - 8, cy - lblStatus.Height / 2);

                lblSearch.Location = new Point(14, cy - lblSearch.Height / 2);
                _txtSearch.SetBounds(
                    lblSearch.Right + 10,
                    cy - _txtSearch.Height / 2,
                    Math.Max(150, lblStatus.Left - (lblSearch.Right + 10) - 16),
                    _txtSearch.Height
                );
            }

            pnl.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblStatus, _cboStatusFilter, btnSearch });
            pnl.Resize += (_, _) => LayoutFilterBar();
            LayoutFilterBar();
            return pnl;
        }

        private GroupBox BuildApprovalGroup()
        {
            var grp = new GroupBox
            {
                Text = "Duyệt phiếu && Gửi thông báo cho độc giả",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12),
                BackColor = Color.White
            };

            var pnlCard = new Panel
            {
                Location = new Point(14, 26),
                Size = new Size(364, 132),
                BackColor = SoftBlue,
                Padding = new Padding(10)
            };
            pnlCard.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);

            _lblResCode = new Label
            {
                Text = "Mã phiếu: --",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Primary,
                AutoSize = true,
                Location = new Point(10, 8)
            };
            _lblReaderInfo = new Label
            {
                Text = "Độc giả: Chưa chọn yêu cầu",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.Black,
                AutoSize = false,
                Size = new Size(344, 34),
                Location = new Point(10, 30)
            };
            _lblBookInfo = new Label
            {
                Text = "Sách đặt: --",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize = false,
                Size = new Size(344, 34),
                Location = new Point(10, 66)
            };
            _lblTimeInfo = new Label
            {
                Text = "Thời gian đặt: --",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextGray,
                AutoSize = false,
                Size = new Size(344, 20),
                Location = new Point(10, 104)
            };

            pnlCard.Controls.AddRange(new Control[] { _lblResCode, _lblReaderInfo, _lblBookInfo, _lblTimeInfo });
            grp.Controls.Add(pnlCard);

            int y = 170;

            grp.Controls.Add(new Label
            {
                Text = "1. Chọn bản sách vật lý sẵn sàng gán cho độc giả:",
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                Location = new Point(14, y)
            });
            y += 22;

            _cboAvailableCopies = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(14, y),
                Width = 364
            };
            _cboAvailableCopies.SelectedIndexChanged += (_, _) => UpdateNotificationTemplate();
            grp.Controls.Add(_cboAvailableCopies);
            y += 36;

            var lblHoldDays = new Label
            {
                Text = "2. Thời hạn giữ sách tại quầy (ngày):",
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(14, y + 3)
            };
            grp.Controls.Add(lblHoldDays);

            _numHoldDays = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 14,
                Value = 3,
                Location = new Point(250, y),
                Width = 128
            };
            _numHoldDays.ValueChanged += (_, _) => UpdateNotificationTemplate();
            grp.Controls.Add(_numHoldDays);
            y += 36;

            grp.Controls.Add(new Label
            {
                Text = "3. Nội dung thông báo gửi đến độc giả (Email / SMS):",
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                Location = new Point(14, y)
            });
            y += 22;

            _txtNotifyMessage = new TextBox
            {
                Location = new Point(14, y),
                Size = new Size(364, 96),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 8.8F)
            };
            grp.Controls.Add(_txtNotifyMessage);
            y += 108;

            _btnApproveAndNotify = CreateActionButton("Duyệt & Thông báo sách sẵn sàng", GCheck, Green, Color.White, 364);
            _btnApproveAndNotify.Height = 38;
            _btnApproveAndNotify.Location = new Point(14, y);
            _btnApproveAndNotify.Click += BtnApproveAndNotify_Click;
            grp.Controls.Add(_btnApproveAndNotify);
            y += 46;

            _btnResendNotify = CreateActionButton("Gửi lại thông báo", GMail, SoftBlue, Primary, 186);
            _btnResendNotify.Location = new Point(14, y);
            _btnResendNotify.Click += BtnResendNotify_Click;

            _btnCancelRes = CreateActionButton("Hủy yêu cầu", GClose, Color.White, Red, 168);
            _btnCancelRes.Location = new Point(210, y);
            _btnCancelRes.Click += BtnCancelRes_Click;

            grp.Controls.Add(_btnResendNotify);
            grp.Controls.Add(_btnCancelRes);

            // Tự giãn đều chiều ngang các thành phần khi resize
            grp.Resize += (_, _) =>
            {
                int contentW = Math.Max(260, grp.ClientSize.Width - 28);
                pnlCard.Width = contentW;
                _lblReaderInfo.Width = contentW - 20;
                _lblBookInfo.Width = contentW - 20;
                _lblTimeInfo.Width = contentW - 20;

                _cboAvailableCopies.Width = contentW;
                _numHoldDays.Left = 14 + contentW - _numHoldDays.Width;
                _txtNotifyMessage.Width = contentW;
                _btnApproveAndNotify.Width = contentW;

                int gap = 10;
                int resendW = (int)((contentW - gap) * 0.54);
                int cancelW = contentW - gap - resendW;
                _btnResendNotify.SetBounds(14, _btnResendNotify.Top, resendW, 34);
                _btnCancelRes.SetBounds(14 + resendW + gap, _btnCancelRes.Top, cancelW, 34);
            };

            SetApprovalPanelEnabled(false);
            return grp;
        }

        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
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
                RowTemplate = { Height = 42 },
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F)
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;

            g.Columns.Add("colId",       "ID");
            g.Columns.Add("colCode",     "Mã đặt trước");
            g.Columns.Add("colReader",   "Độc giả");
            g.Columns.Add("colBook",     "Đầu sách đặt");
            g.Columns.Add("colReserved", "Ngày đặt");
            g.Columns.Add("colAvail",    "Kho sẵn sàng");
            g.Columns.Add("colCopy",     "Bản sách gán");
            g.Columns.Add("colStatus",   "Trạng thái");

            g.Columns["colId"]!.Visible = false;
            g.Columns["colCode"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colCode"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            g.Columns["colCode"]!.FillWeight     = 80;
            g.Columns["colReader"]!.FillWeight   = 110;
            g.Columns["colBook"]!.FillWeight     = 130;
            g.Columns["colReserved"]!.FillWeight = 80;
            g.Columns["colAvail"]!.FillWeight    = 75;
            g.Columns["colCopy"]!.FillWeight     = 80;
            g.Columns["colStatus"]!.FillWeight   = 105;

            g.CellPainting += DgvReservations_CellPainting;
            g.SelectionChanged += DgvReservations_SelectionChanged;
            return g;
        }

        private void DgvReservations_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 7 || e.Graphics == null) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch
            {
                "Chờ duyệt"      => Orange,
                "Sẵn sàng lấy"   => Green,
                "Đã nhận sách"   => Primary,
                "Đã hủy"         => Red,
                "Hết hạn giữ"    => Red,
                _                => TextGray
            };

            var font = new Font("Segoe UI", 8.5F);
            string label = "●  " + text;
            var size = TextRenderer.MeasureText(label, font);
            var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 25) / 2, size.Width + 8, 25);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(rect, 12))
            using (var fill = new SolidBrush(Color.FromArgb(22, c)))
            using (var pen  = new Pen(Color.FromArgb(150, c)))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            }
            TextRenderer.DrawText(e.Graphics, label, font, rect, c,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            e.Handled = true;
        }

        // ============================================================
        //  TẢI DỮ LIỆU & GỌI SERVICE
        // ============================================================
        public void LoadReservations() => _ = LoadReservationsAsync();

        public async Task LoadReservationsAsync()
        {
            try
            {
                var filter = new ReservationFilter
                {
                    Keyword = _txtSearch.Text,
                    StatusFilterIndex = _cboStatusFilter.SelectedIndex
                };

                var summary = await _reservationService.GetReservations(filter);

                _lblSummary.Text =
                    $"Chờ duyệt: {summary.PendingCount:N0} yêu cầu  ·  Đang giữ chờ độc giả lấy: {summary.ReadyCount:N0} cuốn  ·  Hiển thị: {summary.Items.Count:N0} phiếu";

                _dgvReservations.Rows.Clear();
                foreach (var r in summary.Items)
                {
                    _dgvReservations.Rows.Add(
                        r.Id,
                        r.ReservationCode,
                        r.ReaderDisplay,
                        r.BookTitle,
                        r.ReservedAtText,
                        r.AvailableText,
                        r.AssignedCopyCode,
                        r.StatusText
                    );
                }

                if (_dgvReservations.Rows.Count > 0)
                    _dgvReservations.Rows[0].Selected = true;
                else
                    ClearApprovalPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách đặt trước sách:\n{ex.Message}", "Lỗi CSDL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DgvReservations_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dgvReservations.SelectedRows.Count == 0)
            {
                ClearApprovalPanel();
                return;
            }

            if (_dgvReservations.SelectedRows[0].Cells["colId"].Value is not int resId) return;
            await LoadReservationDetailAsync(resId);
        }

        private async Task LoadReservationDetailAsync(int reservationId)
        {
            try
            {
                var detail = await _reservationService.GetReservationDetail(reservationId);
                if (detail == null) return;

                _currentDetail = detail;
                _lblResCode.Text = $"Mã phiếu đặt: {detail.ReservationCode}";
                _lblReaderInfo.Text = $"Độc giả: {detail.ReaderFullName} ({detail.ReaderCode}) · {detail.ReaderContact}";
                _lblBookInfo.Text = $"Sách đặt: [{detail.BookCode}] {detail.BookTitle}";
                _lblTimeInfo.Text = detail.ExpiresAt != null
                    ? $"Đặt: {detail.ReservedAt:dd/MM/yyyy HH:mm}  ·  Hạn giữ: {detail.ExpiresAt:dd/MM/yyyy}"
                    : $"Đặt: {detail.ReservedAt:dd/MM/yyyy HH:mm}";

                _cboAvailableCopies.DataSource = null;
                _cboAvailableCopies.Items.Clear();

                if (detail.CopyOptions.Count > 0)
                {
                    _cboAvailableCopies.DataSource = detail.CopyOptions;
                    _cboAvailableCopies.DisplayMember = nameof(ReservationCopyOptionResponse.Display);
                    _cboAvailableCopies.ValueMember = nameof(ReservationCopyOptionResponse.Id);
                }
                else
                {
                    _cboAvailableCopies.Items.Add("-- Hiện không còn bản sách nào sẵn sàng trên kệ --");
                    _cboAvailableCopies.SelectedIndex = 0;
                }

                bool hasAvailable = !detail.HasAssignedCopy && detail.CopyOptions.Count > 0;
                _cboAvailableCopies.Enabled = detail.IsPending && hasAvailable;
                _numHoldDays.Enabled         = detail.IsPending && hasAvailable;
                _txtNotifyMessage.Enabled    = detail.IsPending || detail.IsReadyForPickup;
                _btnApproveAndNotify.Enabled = detail.IsPending && hasAvailable;
                _btnResendNotify.Enabled     = detail.IsReadyForPickup;
                _btnCancelRes.Enabled        = detail.IsPending || detail.IsReadyForPickup;

                UpdateNotificationTemplate(useSavedNote: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết phiếu đặt trước:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateNotificationTemplate(bool useSavedNote = false)
        {
            if (_currentDetail == null) return;

            if (useSavedNote && !string.IsNullOrWhiteSpace(_currentDetail.Note) && !_currentDetail.IsPending)
            {
                _txtNotifyMessage.Text = _currentDetail.Note;
                return;
            }

            string copyCode = (_cboAvailableCopies.SelectedItem as ReservationCopyOptionResponse)?.CopyCode ?? "bản sách đã giữ";
            _txtNotifyMessage.Text = _reservationService.BuildNotificationTemplate(
                _currentDetail.ReaderFullName,
                $"[{_currentDetail.BookCode}] {_currentDetail.BookTitle}",
                copyCode,
                (int)_numHoldDays.Value);
        }

        private async void BtnApproveAndNotify_Click(object? sender, EventArgs e)
        {
            if (_currentDetail == null) return;
            if (_cboAvailableCopies.SelectedItem is not ReservationCopyOptionResponse selectedCopy)
            {
                MessageBox.Show("Không có bản sách sẵn sàng trên kệ để duyệt phiếu này.", "Không thể duyệt",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var request = new ReservationApproveRequest
                {
                    ReservationId = _currentDetail.Id,
                    BookCopyId = selectedCopy.Id,
                    HoldDays = (int)_numHoldDays.Value,
                    NotifyMessage = _txtNotifyMessage.Text
                };

                var result = await _reservationService.ApproveAndNotify(request);

                MessageBox.Show(
                    $"Đã duyệt phiếu đặt trước {result.ReservationCode} thành công!\n\n" +
                    $"• Bản sách gán: {result.CopyCode}\n" +
                    $"• Giữ đến hết ngày: {result.ExpiresAt:dd/MM/yyyy}\n" +
                    $"• Đã gửi thông báo sách sẵn sàng tới độc giả ({result.TargetContact}).",
                    "Duyệt & Thông báo thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await LoadReservationsAsync();
                ReservationDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                string detail = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Lỗi khi duyệt phiếu đặt trước:\n{detail}", "Lỗi CSDL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnResendNotify_Click(object? sender, EventArgs e)
        {
            if (_currentDetail == null) return;

            try
            {
                string contact = await _reservationService.ResendNotification(_currentDetail.Id, _txtNotifyMessage.Text);
                MessageBox.Show($"Đã gửi lại thông báo nhắc nhận sách đến độc giả: {contact}.",
                    "Đã gửi thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi lại thông báo:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCancelRes_Click(object? sender, EventArgs e)
        {
            if (_currentDetail == null) return;

            var confirm = MessageBox.Show(
                "Bạn có chắc muốn hủy phiếu đặt trước này?\nNếu đã gán bản sách vật lý, sách sẽ được trả về trạng thái sẵn sàng trên kệ.",
                "Xác nhận hủy đặt trước",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                await _reservationService.CancelReservation(_currentDetail.Id);
                MessageBox.Show("Đã hủy phiếu đặt trước thành công.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadReservationsAsync();
                ReservationDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy phiếu đặt trước:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearApprovalPanel()
        {
            _currentDetail = null;
            _lblResCode.Text = "Mã phiếu: --";
            _lblReaderInfo.Text = "Độc giả: Chưa chọn yêu cầu";
            _lblBookInfo.Text = "Sách đặt: --";
            _lblTimeInfo.Text = "Thời gian đặt: --";
            _cboAvailableCopies.DataSource = null;
            _cboAvailableCopies.Items.Clear();
            _txtNotifyMessage.Clear();
            SetApprovalPanelEnabled(false);
        }

        private void SetApprovalPanelEnabled(bool enabled)
        {
            _cboAvailableCopies.Enabled = enabled;
            _numHoldDays.Enabled         = enabled;
            _txtNotifyMessage.Enabled    = enabled;
            _btnApproveAndNotify.Enabled = enabled;
            _btnResendNotify.Enabled     = enabled;
            _btnCancelRes.Enabled        = enabled;
        }

        private static Button CreateActionButton(string text, string glyph, Color back, Color fore, int? fixedWidth = null)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            using (var f = new Font(IconFont, 13F, GraphicsUnit.Pixel))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                TextRenderer.DrawText(
                    g,
                    glyph,
                    f,
                    new Rectangle(0, 0, 16, 16),
                    fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
                );
            }

            var btn = new Button
            {
                Text = " " + text.Trim().Replace("&", "&&"),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Image = bmp,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(8, 0, 10, 0),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                UseCompatibleTextRendering = false,
                Cursor = Cursors.Hand
            };

            if (fixedWidth.HasValue)
            {
                btn.AutoSize = false;
                btn.Size = new Size(fixedWidth.Value, 34);
            }
            else
            {
                btn.AutoSize = true;
                btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                btn.MinimumSize = new Size(0, 34);
                btn.MaximumSize = new Size(0, 34);
            }

            btn.FlatAppearance.BorderColor = BorderGray;
            if (back != Color.White && back != SoftBlue)
                btn.FlatAppearance.BorderSize = 0;

            return btn;
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
}