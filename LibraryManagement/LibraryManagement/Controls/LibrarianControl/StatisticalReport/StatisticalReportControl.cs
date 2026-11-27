using System.Drawing.Drawing2D;
using LibraryManagement.DTO;
using LibraryManagement.DTO.Response;
using LibraryManagement.Services;

namespace LibraryManagement.Controls
{
    public partial class StatisticalReportControl : UserControl
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
        private static readonly Color Purple     = Color.FromArgb(124, 58, 237);
        private const string IconFont = "Segoe MDL2 Assets";

        // Glyph
        private const string GRefresh  = "\uE72C";
        private const string GExport   = "\uEDE1";
        private const string GBook     = "\uE82D";
        private const string GForward  = "\uE72A";
        private const string GWarn     = "\uE7BA";
        private const string GMoney    = "\uE8C7";
        private const string GFilter   = "\uE71C";

        // ====== Dependencies ======
        private readonly StatisticalReportService _reportService;

        // ====== Controls ======
        private ComboBox _cboPreset = null!;
        private DateTimePicker _dtpFrom = null!;
        private DateTimePicker _dtpTo = null!;
        private TableLayoutPanel _tblKpi = null!;
        private Panel _pnlBarChart = null!;
        private Panel _pnlPieChart = null!;
        private TabControl _tabDetails = null!;
        private DataGridView _dgvOverdue = null!;
        private DataGridView _dgvTopBooks = null!;
        private DataGridView _dgvLowStock = null!;
        private DataGridView _dgvFines = null!;

        // ====== Dữ liệu biểu đồ ======
        private List<MonthlyStatDto> _monthlyStats = new();
        private List<(string CategoryName, int BookCount, Color SliceColor)> _categoryStats = new();
        
        public StatisticalReportControl(StatisticalReportService reportService)
        {
            _reportService = reportService;
            BackColor = BgGray;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
            Load += (_, _) => ApplyPresetAndLoad();
        }

        // ============================================================
        //  DỰNG GIAO DIỆN
        // ============================================================
        private void BuildUi()
        {
            SuspendLayout();

            var pnlTopArea = new Panel { Dock = DockStyle.Top, Height = 114, BackColor = BgGray };
            pnlTopArea.Controls.Add(BuildFilterBar());
            pnlTopArea.Controls.Add(BuildHeader());

            var pnlKpiHost = new Panel { Dock = DockStyle.Top, Height = 108, Padding = new Padding(0, 8, 0, 0) };
            _tblKpi = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) _tblKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _tblKpi.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlKpiHost.Controls.Add(_tblKpi);

            var pnlChartsHost = new Panel { Dock = DockStyle.Top, Height = 275, Padding = new Padding(0, 8, 0, 0) };
            var tblCharts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tblCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            tblCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            tblCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var grpBar = new GroupBox
            {
                Text = "Xu hướng mượn, trả sách theo tháng (năm hiện tại)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10),
                Margin = new Padding(0, 0, 6, 0)
            };
            _pnlBarChart = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.White };
            _pnlBarChart.Paint += PnlBarChart_Paint;
            _pnlBarChart.Resize += (_, _) => _pnlBarChart.Invalidate();
            grpBar.Controls.Add(_pnlBarChart);

            var grpPie = new GroupBox
            {
                Text = "Cơ cấu đầu sách theo Thể loại",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10),
                Margin = new Padding(6, 0, 0, 0)
            };
            _pnlPieChart = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.White };
            _pnlPieChart.Paint += PnlPieChart_Paint;
            _pnlPieChart.Resize += (_, _) => _pnlPieChart.Invalidate();
            grpPie.Controls.Add(_pnlPieChart);

            tblCharts.Controls.Add(grpBar, 0, 0);
            tblCharts.Controls.Add(grpPie, 1, 0);
            pnlChartsHost.Controls.Add(tblCharts);

            var pnlBottomHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            _tabDetails = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Padding = new Point(12, 4) };

            var tabOverdue = new TabPage("1. Danh sách quá hạn trả") { BackColor = Color.White, Padding = new Padding(8) };
            _dgvOverdue = CreateBaseGrid();
            SetupOverdueColumns(_dgvOverdue);
            tabOverdue.Controls.Add(_dgvOverdue);

            var tabTopBooks = new TabPage("2. Top sách mượn nhiều nhất") { BackColor = Color.White, Padding = new Padding(8) };
            _dgvTopBooks = CreateBaseGrid();
            SetupTopBooksColumns(_dgvTopBooks);
            tabTopBooks.Controls.Add(_dgvTopBooks);

            var tabLowStock = new TabPage("3. Cảnh báo tồn kho thấp") { BackColor = Color.White, Padding = new Padding(8) };
            _dgvLowStock = CreateBaseGrid();
            SetupLowStockColumns(_dgvLowStock);
            tabLowStock.Controls.Add(_dgvLowStock);

            var tabFines = new TabPage("4. Chi tiết khoản phạt") { BackColor = Color.White, Padding = new Padding(8) };
            _dgvFines = CreateBaseGrid();
            SetupFinesColumns(_dgvFines);
            tabFines.Controls.Add(_dgvFines);

            _tabDetails.TabPages.AddRange(new[] { tabOverdue, tabTopBooks, tabLowStock, tabFines });
            pnlBottomHost.Controls.Add(_tabDetails);

            Controls.Add(pnlBottomHost);
            Controls.Add(pnlChartsHost);
            Controls.Add(pnlKpiHost);
            Controls.Add(pnlTopArea);

            ResumeLayout(true);
        }

        private Panel BuildHeader()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = SoftBlue };
            pnl.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnl.Width - 1, pnl.Height - 1);

            pnl.Controls.Add(new Label
            {
                Text = "BÁO CÁO & THỐNG KÊ HOẠT ĐỘNG THƯ VIỆN",
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(14, 7)
            });
            pnl.Controls.Add(new Label
            {
                Text = "Phân tích lưu thông mượn/trả, tình trạng kho sách, cảnh báo quá hạn và doanh thu tiền phạt",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(16, 31)
            });

            var btnExport = CreateActionButton("  Xuất Excel (CSV)", GExport, Color.White, Color.Black, 185);
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExport.Click += BtnExport_Click;

            var btnRefresh = CreateActionButton("  Làm mới", GRefresh, Primary, Color.White, 115);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (_, _) => LoadReportData();

            pnl.Controls.Add(btnExport);
            pnl.Controls.Add(btnRefresh);
            pnl.Resize += (_, _) =>
            {
                btnRefresh.Location = new Point(pnl.Width - btnRefresh.Width - 12, 11);
                btnExport.Location  = new Point(btnRefresh.Left - btnExport.Width - 8, 11);
            };

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

            var lblPreset = new Label { Text = "Kỳ báo cáo:", AutoSize = true, Location = new Point(14, 16), ForeColor = TextGray };
            _cboPreset = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 12),
                Width = 145
            };
            _cboPreset.Items.AddRange(new object[]
            {
                "Hôm nay", "7 ngày qua", "Tháng này", "Quý này", "Năm nay", "Tùy chọn"
            });
            _cboPreset.SelectedIndex = 2;
            _cboPreset.SelectedIndexChanged += (_, _) =>
            {
                if (_cboPreset.SelectedIndex != 5) ApplyPresetAndLoad();
            };

            var lblFrom = new Label { Text = "Từ ngày:", AutoSize = true, Location = new Point(0, 16), ForeColor = TextGray };
            _dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Location = new Point(0, 12),
                Width = 115
            };
            _dtpFrom.ValueChanged += (_, _) => { if (_dtpFrom.Focused) _cboPreset.SelectedIndex = 5; };

            var lblTo = new Label { Text = "Đến ngày:", AutoSize = true, Location = new Point(0, 16), ForeColor = TextGray };
            _dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Location = new Point(0, 12),
                Width = 115
            };
            _dtpTo.ValueChanged += (_, _) => { if (_dtpTo.Focused) _cboPreset.SelectedIndex = 5; };

            var btnFilter = CreateActionButton("  Lọc dữ liệu", GFilter, SoftBlue, Primary, 150);
            btnFilter.Location = new Point(0, 9);
            btnFilter.Height = 31;
            btnFilter.Click += (_, _) => LoadReportData();

            void LayoutFilterBar()
            {
                lblPreset.Left = 14;
                _cboPreset.Left = lblPreset.Right + 8;
                lblFrom.Left = _cboPreset.Right + 16;
                _dtpFrom.Left = lblFrom.Right + 8;
                lblTo.Left = _dtpFrom.Right + 16;
                _dtpTo.Left = lblTo.Right + 8;
                btnFilter.Left = pnl.ClientSize.Width - btnFilter.Width - 12;
                lblPreset.Top = lblFrom.Top = lblTo.Top = (pnl.ClientSize.Height - lblPreset.Height) / 2;
            }

            pnl.Controls.AddRange(new Control[] { lblPreset, _cboPreset, lblFrom, _dtpFrom, lblTo, _dtpTo, btnFilter });
            pnl.Resize += (_, _) => LayoutFilterBar();
            LayoutFilterBar();
            return pnl;
        }

        // ============================================================
        //  HIỂN THỊ DỮ LIỆU TỪ SERVICE
        // ============================================================
        private void ApplyPresetAndLoad()
        {
            var (from, to) = _reportService.GetDateRangeByPreset(_cboPreset.SelectedIndex, DateTime.Today);
            _dtpFrom.Value = from;
            _dtpTo.Value = to;
            LoadReportData();
        }

        public async Task LoadReportData()
        {
            try
            {
                var report = await _reportService.GetFullReport(_dtpFrom.Value, _dtpTo.Value);

                // 1. KPI Cards
                var kpi = report.Kpi;
                _tblKpi.Controls.Clear();
                AddKpiCard(0, GBook, "Tài nguyên kho sách", $"{kpi.AvailableCopies:N0} / {kpi.TotalCopies:N0}",
                    $"Sẵn sàng trên {kpi.TotalBooks:N0} đầu sách", Primary, SoftBlue);
                AddKpiCard(1, GForward, "Lưu thông trong kỳ", $"{kpi.BorrowedInRange:N0} lượt",
                    $"Đã trả: {kpi.ReturnedInRange:N0} cuốn", Green, Color.FromArgb(232, 248, 239));
                AddKpiCard(2, GWarn, "Sách đang quá hạn", $"{kpi.CurrentOverdue:N0} cuốn",
                    kpi.CurrentOverdue > 0 ? "Cần thu hồi khẩn cấp" : "Không có sách trễ hạn",
                    kpi.CurrentOverdue > 0 ? Red : Green,
                    kpi.CurrentOverdue > 0 ? Color.FromArgb(253, 234, 234) : Color.FromArgb(232, 248, 239));
                AddKpiCard(3, GMoney, "Tổng tiền phạt ghi nhận", $"{kpi.TotalFineAmount:N0} đ",
                    $"Tổng cộng {kpi.TotalFineCount:N0} phiếu phạt", Orange, Color.FromArgb(254, 245, 231));

                // 2. Biểu đồ cột
                _monthlyStats = report.MonthlyStats;
                _pnlBarChart.Invalidate();

                // 3. Biểu đồ tròn (gắn màu hiển thị UI)
                Color[] palette = { Primary, Green, Orange, Purple, Color.FromArgb(14, 165, 233), Color.FromArgb(236, 72, 153), TextGray };
                _categoryStats = report.CategoryStats
                    .Select((c, idx) => (c.CategoryName, c.BookCount, palette[Math.Min(idx, palette.Length - 1)]))
                    .ToList();
                _pnlPieChart.Invalidate();

                // 4. Bảng 1: Quá hạn
                _dgvOverdue.Rows.Clear();
                foreach (var item in report.OverdueItems)
                {
                    _dgvOverdue.Rows.Add(
                        item.LoanCode,
                        $"{item.ReaderName} ({item.ReaderCode})",
                        $"{item.BookTitle} [{item.CopyCode}]",
                        item.BorrowedAt.ToString("dd/MM/yyyy"),
                        item.DueAt.ToString("dd/MM/yyyy"),
                        $"{item.LateDays} ngày",
                        $"{item.EstimatedFine:N0} đ"
                    );
                }

                // 5. Bảng 2: Top sách
                _dgvTopBooks.Rows.Clear();
                foreach (var b in report.TopBooks)
                {
                    _dgvTopBooks.Rows.Add(
                        $"#{b.Rank}",
                        b.Title,
                        b.CategoryName,
                        b.BorrowCount.ToString("N0"),
                        $"{b.AvailableCopies} / {b.TotalCopies} cuốn"
                    );
                }

                // 6. Bảng 3: Tồn kho thấp
                _dgvLowStock.Rows.Clear();
                foreach (var s in report.LowStockBooks)
                {
                    _dgvLowStock.Rows.Add(s.Title, s.CategoryName, s.TotalCopies, s.BorrowedCopies, s.AvailableCopies, s.StatusText);
                }

                // 7. Bảng 4: Tiền phạt
                _dgvFines.Rows.Clear();
                foreach (var f in report.FineDetails)
                {
                    _dgvFines.Rows.Add(f.FineCode, f.LoanCode, f.ReaderName, f.BookTitle, f.Reason, $"{f.AmountVnd:N0} đ");
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Cảnh báo bộ lọc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu báo cáo thống kê:\n{ex.Message}",
                    "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  VẼ BIỂU ĐỒ GDI+
        // ============================================================
        private void PnlBarChart_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (_monthlyStats.Count == 0) return;

            int padLeft = 42, padRight = 16, padTop = 30, padBottom = 28;
            var plot = new Rectangle(padLeft, padTop, _pnlBarChart.Width - padLeft - padRight, _pnlBarChart.Height - padTop - padBottom);
            if (plot.Width <= 40 || plot.Height <= 40) return;

            using (var b1 = new SolidBrush(Primary))
            using (var b2 = new SolidBrush(Green))
            using (var fLegend = new Font("Segoe UI", 8F))
            {
                g.FillRectangle(b1, padLeft, 8, 12, 12);
                g.DrawString("Lượt mượn", fLegend, Brushes.Black, padLeft + 16, 6);

                g.FillRectangle(b2, padLeft + 95, 8, 12, 12);
                g.DrawString("Lượt trả", fLegend, Brushes.Black, padLeft + 111, 6);
            }

            int maxVal = Math.Max(5, _monthlyStats.Max(x => Math.Max(x.BorrowCount, x.ReturnCount)));
            maxVal = ((maxVal + 4) / 5) * 5;

            using var gridPen = new Pen(Color.FromArgb(232, 235, 240));
            using var axisFont = new Font("Segoe UI", 7.5F);
            using var grayBrush = new SolidBrush(TextGray);

            int steps = 5;
            for (int i = 0; i <= steps; i++)
            {
                int y = plot.Bottom - (plot.Height * i / steps);
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                int val = maxVal * i / steps;
                var sz = g.MeasureString(val.ToString(), axisFont);
                g.DrawString(val.ToString(), axisFont, grayBrush, plot.Left - sz.Width - 6, y - sz.Height / 2);
            }

            float groupWidth = (float)plot.Width / _monthlyStats.Count;
            float barWidth = Math.Min(14f, (groupWidth - 10f) / 2f);

            using var brushBorrow = new SolidBrush(Primary);
            using var brushReturn = new SolidBrush(Green);

            for (int i = 0; i < _monthlyStats.Count; i++)
            {
                var item = _monthlyStats[i];
                float centerX = plot.Left + groupWidth * i + groupWidth / 2f;

                float hBorrow = (float)item.BorrowCount / maxVal * plot.Height;
                float hReturn = (float)item.ReturnCount / maxVal * plot.Height;

                g.FillRectangle(brushBorrow, centerX - barWidth - 1, plot.Bottom - hBorrow, barWidth, hBorrow);
                g.FillRectangle(brushReturn, centerX + 1, plot.Bottom - hReturn, barWidth, hReturn);

                var lblSz = g.MeasureString(item.Label, axisFont);
                g.DrawString(item.Label, axisFont, grayBrush, centerX - lblSz.Width / 2f, plot.Bottom + 5);
            }
        }

        private void PnlPieChart_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int total = _categoryStats.Sum(x => x.BookCount);
            if (total == 0)
            {
                using var f = new Font("Segoe UI", 9F);
                g.DrawString("Chưa có dữ liệu thể loại sách", f, Brushes.Gray, 20, 20);
                return;
            }

            int diameter = Math.Min(_pnlPieChart.Height - 24, (int)(_pnlPieChart.Width * 0.45));
            if (diameter < 60) return;

            var pieRect = new Rectangle(14, (_pnlPieChart.Height - diameter) / 2, diameter, diameter);
            float startAngle = -90f;

            foreach (var slice in _categoryStats)
            {
                float sweepAngle = (float)slice.BookCount / total * 360f;
                using var brush = new SolidBrush(slice.SliceColor);
                g.FillPie(brush, pieRect, startAngle, sweepAngle);
                using var pen = new Pen(Color.White, 1.5f);
                g.DrawPie(pen, pieRect, startAngle, sweepAngle);
                startAngle += sweepAngle;
            }

            int hole = (int)(diameter * 0.52);
            var holeRect = new Rectangle(pieRect.X + (diameter - hole) / 2, pieRect.Y + (diameter - hole) / 2, hole, hole);
            g.FillEllipse(Brushes.White, holeRect);

            using (var fTotal = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString($"{total}\nđầu sách", fTotal, Brushes.Black, holeRect, sf);
            }

            int legendX = pieRect.Right + 16;
            int legendY = 16;
            using var fLegend = new Font("Segoe UI", 8.2F);

            foreach (var slice in _categoryStats)
            {
                if (legendY + 22 > _pnlPieChart.Height) break;
                double pct = (double)slice.BookCount / total * 100;
                using var brush = new SolidBrush(slice.SliceColor);
                g.FillRectangle(brush, legendX, legendY + 3, 10, 10);

                string text = $"{slice.CategoryName} ({slice.BookCount} - {pct:0.#}%)";
                g.DrawString(text, fLegend, Brushes.Black, legendX + 16, legendY);
                legendY += 23;
            }
        }

        // ============================================================
        //  CẤU HÌNH BẢNG DỮ LIỆU & XUẤT FILE
        // ============================================================
        private static DataGridView CreateBaseGrid()
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
                RowTemplate = { Height = 36 },
                ColumnHeadersHeight = 34,
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
            return g;
        }

        private static void SetupOverdueColumns(DataGridView g)
        {
            g.Columns.Add("c1", "Mã phiếu");
            g.Columns.Add("c2", "Độc giả");
            g.Columns.Add("c3", "Sách đang mượn");
            g.Columns.Add("c4", "Ngày mượn");
            g.Columns.Add("c5", "Hạn trả");
            g.Columns.Add("c6", "Số ngày trễ");
            g.Columns.Add("c7", "Phạt dự kiến");

            g.Columns["c1"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["c6"]!.DefaultCellStyle.ForeColor = Red;
            g.Columns["c6"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["c1"]!.FillWeight = 70;
            g.Columns["c2"]!.FillWeight = 110;
            g.Columns["c3"]!.FillWeight = 130;
            g.Columns["c4"]!.FillWeight = 70;
            g.Columns["c5"]!.FillWeight = 70;
            g.Columns["c6"]!.FillWeight = 70;
            g.Columns["c7"]!.FillWeight = 80;
        }

        private static void SetupTopBooksColumns(DataGridView g)
        {
            g.Columns.Add("c1", "Hạng");
            g.Columns.Add("c2", "Tên đầu sách");
            g.Columns.Add("c3", "Thể loại");
            g.Columns.Add("c4", "Tổng lượt mượn");
            g.Columns.Add("c5", "Tồn kho hiện tại");

            g.Columns["c1"]!.FillWeight = 45;
            g.Columns["c2"]!.FillWeight = 180;
            g.Columns["c3"]!.FillWeight = 100;
            g.Columns["c4"]!.FillWeight = 80;
            g.Columns["c5"]!.FillWeight = 90;
            g.Columns["c4"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["c4"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private static void SetupLowStockColumns(DataGridView g)
        {
            g.Columns.Add("c1", "Tên đầu sách");
            g.Columns.Add("c2", "Thể loại");
            g.Columns.Add("c3", "Tổng bản");
            g.Columns.Add("c4", "Đang mượn");
            g.Columns.Add("c5", "Sẵn sàng");
            g.Columns.Add("c6", "Cảnh báo");

            g.Columns["c1"]!.FillWeight = 170;
            g.Columns["c2"]!.FillWeight = 100;
            g.Columns["c3"]!.FillWeight = 60;
            g.Columns["c4"]!.FillWeight = 60;
            g.Columns["c5"]!.FillWeight = 60;
            g.Columns["c6"]!.FillWeight = 90;
            g.Columns["c6"]!.DefaultCellStyle.ForeColor = Orange;
            g.Columns["c6"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private static void SetupFinesColumns(DataGridView g)
        {
            g.Columns.Add("c1", "Mã phạt");
            g.Columns.Add("c2", "Mã phiếu mượn");
            g.Columns.Add("c3", "Độc giả");
            g.Columns.Add("c4", "Sách liên quan");
            g.Columns.Add("c5", "Lý do phạt");
            g.Columns.Add("c6", "Số tiền");

            g.Columns["c1"]!.FillWeight = 65;
            g.Columns["c2"]!.FillWeight = 75;
            g.Columns["c3"]!.FillWeight = 105;
            g.Columns["c4"]!.FillWeight = 120;
            g.Columns["c5"]!.FillWeight = 130;
            g.Columns["c6"]!.FillWeight = 75;
            g.Columns["c6"]!.DefaultCellStyle.ForeColor = Red;
            g.Columns["c6"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            var activeGrid = _tabDetails.SelectedIndex switch
            {
                0 => _dgvOverdue,
                1 => _dgvTopBooks,
                2 => _dgvLowStock,
                _ => _dgvFines
            };

            if (activeGrid.Rows.Count == 0)
            {
                MessageBox.Show("Bảng hiện tại không có dữ liệu để xuất.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel CSV (*.csv)|*.csv",
                FileName = $"BaoCaoThuVien_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var headers = activeGrid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText);
                var rows = activeGrid.Rows.Cast<DataGridViewRow>()
                    .Select(r => r.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString() ?? string.Empty));

                _reportService.ExportToCsv(sfd.FileName, headers, rows);
                MessageBox.Show("Xuất báo cáo thành công!", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  HELPER UI
        // ============================================================
        private void AddKpiCard(int col, string glyph, string caption, string value, string sub, Color accent, Color softBg)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, col == 3 ? 0 : 10, 0)
            };
            card.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

            card.Controls.Add(new Label
            {
                Text = glyph,
                Font = new Font(IconFont, 17F),
                ForeColor = accent,
                BackColor = softBg,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(46, 46),
                Location = new Point(12, 24)
            });
            card.Controls.Add(new Label { Text = caption, Font = new Font("Segoe UI", 8.5F), ForeColor = TextGray, AutoSize = true, Location = new Point(70, 12) });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.Black, AutoSize = true, Location = new Point(68, 31) });
            card.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 8F), ForeColor = accent, AutoSize = true, Location = new Point(70, 65) });

            _tblKpi.Controls.Add(card, col, 0);
        }

        private static Button CreateActionButton(string text, string glyph, Color back, Color fore, int width)
        {
            var bmp = new Bitmap(18, 18);
            using (var g = Graphics.FromImage(bmp))
            using (var f = new Font(IconFont, 13, GraphicsUnit.Pixel))
            {
                TextRenderer.DrawText(g, glyph, f, new Rectangle(0, -1, 18, 18), fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Size = new Size(width, 34),
                Image = bmp,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = BorderGray;
            return btn;
        }

        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }
    }
}