using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;

namespace LibraryManagement.Forms.FineManagement
{
    public partial class FineManagement : UserControl
    {
        // ====== Màu sắc ======
        private static readonly Color Primary    = Color.FromArgb(19, 98, 160);
        private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
        private static readonly Color TextGray   = Color.FromArgb(110, 117, 128);
        private static readonly Color SoftBlue   = Color.FromArgb(232, 240, 250);
        private static readonly Color Orange     = Color.FromArgb(214, 140, 10);
        private static readonly Color Green      = Color.FromArgb(34, 160, 90);
        private static readonly Color Red        = Color.FromArgb(200, 40, 40);
        private static readonly Color Gray       = Color.FromArgb(140, 146, 156);
        private const string IconFont = "Segoe MDL2 Assets";

        // Glyph
        private const string GSearch  = "\uE721";
        private const string GRefresh = "\uE72C";
        private const string GAdd     = "\uE710";
        private const string GMoney   = "\uE8C7";
        private const string GWarn    = "\uE7BA";
        private const string GWallet  = "\uE8C7";
        private const string GMore    = "\uE712";
        private const string GClose   = "\uE711";
        private const string GHistory = "\uE81C";
        private const string GPrint   = "\uE749";
        private const string GEdit    = "\uE70F";
        private const string GCancel  = "\uE711";
        private const string GPay     = "\uE8C7";
        private const string GChevron = "\uE76C";

        // ====== Model ======
        public enum FineType { Overdue, Damaged, Lost, Other }
        public enum FineStatus { Unpaid, Partial, Paid, Cancelled }

        public sealed class PaymentRecord
        {
            public int No { get; set; }
            public DateTime PaidAt { get; set; }
            public decimal Amount { get; set; }
            public string Method { get; set; } = "Tiền mặt";
            public string Collector { get; set; } = "";
        }

        public sealed class AuditRecord
        {
            public DateTime At { get; set; }
            public string Action { get; set; } = "";
            public decimal BeforeAmount { get; set; }
            public decimal AfterAmount { get; set; }
            public string Reason { get; set; } = "";
            public string Actor { get; set; } = "";
        }

        public sealed class FineRow
        {
            public string FineId { get; set; } = "";
            public string TicketId { get; set; } = "";
            public string ReaderId { get; set; } = "";
            public string ReaderName { get; set; } = "";
            public string BookTitle { get; set; } = "";
            public string CopyId { get; set; } = "";
            public FineType Type { get; set; }
            public string Reason { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string CreatedBy { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal Paid { get; set; }
            public decimal Remaining => Math.Max(0, Amount - Paid);
            public FineStatus Status { get; set; }
            public List<PaymentRecord> Payments { get; set; } = new();
            public List<AuditRecord> Audits { get; set; } = new();
        }

        // ====== Data ======
        private List<FineRow> _allRows = new();
        private List<FineRow> _filtered = new();
        private FineRow? _drawerRow;

        // ====== Control ======
        private Label lblKpiTotal = null!;
        private Label lblKpiPaid = null!;
        private Label lblKpiDebt = null!;

        private TextBox txtSearch = null!;
        private ComboBox cboType = null!;
        private ComboBox cboStatus = null!;
        private CheckBox chkUseDate = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;

        private DataGridView dgvFines = null!;

        // Drawer
        private Panel pnlDrawer = null!;
        private Label lblDrawerTitle = null!;
        private Label lblDrawerSub = null!;
        private TabControl tabDrawer = null!;
        private DataGridView dgvPayments = null!;
        private FlowLayoutPanel flowAudit = null!;

        public FineManagement()
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
            _allRows = new List<FineRow>
            {
                MakeFine("PP0015", "PM012", "DG005", "Nguyễn Minh Anh", "Nhà giả kim", "BC-0012",
                    FineType.Overdue, "Trễ 5 ngày", DateTime.Today.AddDays(-10), "Thủ thư A",
                    10000, 5000, FineStatus.Partial, payments: 1),
                MakeFine("PP0016", "PM018", "DG021", "Trần Hoàng Nam", "Đắc nhân tâm", "BC-0045",
                    FineType.Damaged, "Rách bìa và ướt 10 trang đầu", DateTime.Today.AddDays(-8), "Thủ thư B",
                    50000, 50000, FineStatus.Paid),
                MakeFine("PP0017", "PM021", "DG033", "Lê Thu Hà", "Lược sử thời gian", "BC-0067",
                    FineType.Overdue, "Trễ 12 ngày", DateTime.Today.AddDays(-7), "Thủ thư A",
                    24000, 0, FineStatus.Unpaid),
                MakeFine("PP0018", "PM025", "DG041", "Đỗ Hải Yến", "Mắt biếc", "BC-0081",
                    FineType.Lost, "Mất sách, đền 100% giá bìa", DateTime.Today.AddDays(-6), "Thủ thư C",
                    120000, 60000, FineStatus.Partial, payments: 2),
                MakeFine("PP0019", "PM031", "DG052", "Phạm Quốc Bảo", "Sapiens", "BC-0093",
                    FineType.Other, "Vi phạm nội quy: làm ồn", DateTime.Today.AddDays(-4), "Thủ thư B",
                    20000, 0, FineStatus.Unpaid),
                MakeFine("PP0020", "PM033", "DG058", "Vũ Thanh Tùng", "", "",
                    FineType.Other, "Làm mất thẻ độc giả", DateTime.Today.AddDays(-3), "Thủ thư A",
                    30000, 30000, FineStatus.Paid),
                MakeFine("PP0021", "PM037", "DG063", "Ngô Lan Phương", "Cà phê cùng Tony", "BC-0117",
                    FineType.Damaged, "Bìa cong, gáy bong keo", DateTime.Today.AddDays(-2), "Thủ thư C",
                    35000, 0, FineStatus.Unpaid),
                MakeFine("PP0022", "PM040", "DG071", "Bùi Anh Khoa", "Tư duy nhanh và chậm", "BC-0124",
                    FineType.Overdue, "Trễ 18 ngày", DateTime.Today.AddDays(-1), "Thủ thư B",
                    36000, 0, FineStatus.Cancelled),
            };
        }

        private FineRow MakeFine(string id, string ticket, string readerId, string readerName,
            string title, string copy, FineType type, string reason, DateTime created, string by,
            decimal amount, decimal paid, FineStatus status, int payments = 0)
        {
            var r = new FineRow
            {
                FineId = id, TicketId = ticket, ReaderId = readerId, ReaderName = readerName,
                BookTitle = title, CopyId = copy, Type = type, Reason = reason,
                CreatedAt = created, CreatedBy = by, Amount = amount, Paid = paid, Status = status
            };

            if (payments > 0)
            {
                decimal each = Math.Round(paid / payments);
                for (int i = 0; i < payments; i++)
                {
                    decimal val = (i == payments - 1) ? paid - each * (payments - 1) : each;
                    r.Payments.Add(new PaymentRecord
                    {
                        No = i + 1,
                        PaidAt = created.AddDays(i + 1).AddHours(9 + i),
                        Amount = val,
                        Method = i % 2 == 0 ? "Tiền mặt" : "Chuyển khoản",
                        Collector = by
                    });
                }
            }

            r.Audits.Add(new AuditRecord
            {
                At = created, Action = "Tạo mới", BeforeAmount = 0, AfterAmount = amount,
                Reason = "Khởi tạo phiếu phạt", Actor = by
            });
            if (paid > 0 && paid < amount)
            {
                r.Audits.Add(new AuditRecord
                {
                    At = created.AddDays(1).AddHours(10), Action = "Điều chỉnh", BeforeAmount = amount,
                    AfterAmount = amount, Reason = "Xác nhận thanh toán một phần", Actor = by
                });
            }
            if (status == FineStatus.Cancelled)
            {
                r.Audits.Add(new AuditRecord
                {
                    At = created.AddDays(1).AddHours(15), Action = "Hủy phiếu",
                    BeforeAmount = amount, AfterAmount = 0,
                    Reason = "Độc giả khiếu nại, đã đối chiếu và xác nhận không vi phạm", Actor = "Quản trị viên"
                });
            }
            return r;
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

            // THỨ TỰ ADD QUAN TRỌNG (WinForms: add sau → dock trước):
            // 1. Fill  (table)   — add trước
            // 2. Top   (toolbar) — add sau → dock trên table
            // 3. Top   (KPI)     — add sau → dock trên toolbar
            // 4. Right (drawer)  — add sau → dock phải
            Controls.Add(BuildTableArea());   // Fill
            Controls.Add(BuildToolbar());     // Top (dưới)
            Controls.Add(BuildKpiArea());     // Top (trên)
            Controls.Add(BuildDrawer());      // Right

            ResumeLayout(true);
        }

        // ---------- 1. KPI ----------
        private Panel BuildKpiArea()
        {
            var host = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(12, 12, 12, 0) };
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            for (int i = 0; i < 3; i++) tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tbl.Controls.Add(KpiCard(GMoney, Primary, "Tổng tiền phạt phát sinh", "0 đ", 0), 0, 0);
            tbl.Controls.Add(KpiCard(GWallet, Green, "Tổng tiền đã thu", "0 đ", 1), 1, 0);
            tbl.Controls.Add(KpiCard(GWarn, Red, "Tổng tiền độc giả còn nợ", "0 đ", 2), 2, 0);

            host.Controls.Add(tbl);
            return host;
        }

        private Panel KpiCard(string glyph, Color accent, string caption, string value, int index)
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
            card.Controls.Add(new Label
            {
                Text = glyph, Font = new Font(IconFont, 20F), ForeColor = accent,
                BackColor = Color.FromArgb(22, accent),
                TextAlign = ContentAlignment.MiddleCenter, Size = new Size(56, 56),
                Location = new Point(16, 22)
            });
            card.Controls.Add(new Label
            {
                Text = caption, Font = new Font("Segoe UI", 9F), ForeColor = TextGray,
                AutoSize = true, Location = new Point(86, 20)
            });
            var lbl = new Label
            {
                Text = value, Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = accent, AutoSize = true, Location = new Point(84, 44)
            };
            card.Controls.Add(lbl);

            if (index == 0) lblKpiTotal = lbl;
            else if (index == 1) lblKpiPaid = lbl;
            else lblKpiDebt = lbl;
            return card;
        }

        // ---------- 2. TOOLBAR ----------
        private Panel BuildToolbar()
        {
            var host = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                Padding = new Padding(12, 8, 12, 0)
            };

            var grp = new GroupBox
            {
                Text = "Tìm kiếm & Bộ lọc",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 6, 12, 6)
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            // ---------- Hàng 1: search + nút ----------
            var row1 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0)
            };
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));

            txtSearch = new TextBox
            {
                PlaceholderText = "Tìm theo mã phạt, mã ĐG, tên ĐG, mã phiếu mượn, mã vạch...",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 4, 8, 4)
            };
            txtSearch.TextChanged += (_, _) => ApplyFilter();
            row1.Controls.Add(txtSearch, 0, 0);

            var btnReset = new Button
            {
                Text = "  Làm mới",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Image = GlyphImage(GRefresh, 14, Color.Black),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Margin = new Padding(0, 4, 8, 4)
            };
            btnReset.FlatAppearance.BorderColor = BorderGray;
            btnReset.Click += (_, _) => ResetFilter();
            row1.Controls.Add(btnReset, 1, 0);

            var btnAdd = new Button
            {
                Text = "  Tạo khoản phạt",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Image = GlyphImage(GAdd, 14, Color.White),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Margin = new Padding(0, 4, 0, 4)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (_, _) => ShowCreateFineDialog();
            row1.Controls.Add(btnAdd, 2, 0);

            tbl.Controls.Add(row1, 0, 0);

            // ---------- Hàng 2: filter ----------
            var row2 = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0),
                Padding = new Padding(0, 2, 0, 0)
            };

            row2.Controls.Add(FilterLabel("Loại vi phạm:"));
            cboType = MakeComboFilter(180, new[] {
                "Tất cả", "Trả sách quá hạn", "Làm hư hỏng sách", "Làm mất sách", "Vi phạm nội quy khác"
            });
            cboType.SelectedIndexChanged += (_, _) => ApplyFilter();
            row2.Controls.Add(cboType);

            row2.Controls.Add(FilterLabel("Trạng thái:"));
            cboStatus = MakeComboFilter(170, new[] {
                "Tất cả", "Chưa thanh toán", "Thanh toán 1 phần", "Đã thanh toán", "Đã hủy"
            });
            cboStatus.SelectedIndexChanged += (_, _) => ApplyFilter();
            row2.Controls.Add(cboStatus);

            chkUseDate = new CheckBox
            {
                Text = "Ngày tạo:",
                AutoSize = true,
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F),
                Margin = new Padding(12, 7, 4, 0)
            };
            row2.Controls.Add(chkUseDate);

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Enabled = false,
                Value = DateTime.Today.AddDays(-30),
                Margin = new Padding(0, 3, 0, 0)
            };
            row2.Controls.Add(dtpFrom);

            row2.Controls.Add(new Label
            {
                Text = "–",
                AutoSize = true,
                ForeColor = TextGray,
                Margin = new Padding(4, 7, 4, 0)
            });

            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Enabled = false,
                Value = DateTime.Today,
                Margin = new Padding(0, 3, 0, 0)
            };
            row2.Controls.Add(dtpTo);

            chkUseDate.CheckedChanged += (_, _) =>
            {
                dtpFrom.Enabled = dtpTo.Enabled = chkUseDate.Checked;
                ApplyFilter();
            };
            dtpFrom.ValueChanged += (_, _) => ApplyFilter();
            dtpTo.ValueChanged += (_, _) => ApplyFilter();

            tbl.Controls.Add(row2, 0, 1);

            grp.Controls.Add(tbl);
            host.Controls.Add(grp);
            return host;
        }

        private static Label FilterLabel(string text) => new()
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(110, 117, 128),
            Font = new Font("Segoe UI", 8.5F),
            Margin = new Padding(0, 10, 6, 0)
        };

        private static ComboBox MakeComboFilter(int width, string[] items)
        {
            var c = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = width,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 6, 12, 0)
            };
            c.Items.AddRange(items);
            c.SelectedIndex = 0;
            return c;
        }

        // ---------- 3. TABLE ----------
        private GroupBox BuildTableArea()
        {
            var grp = new GroupBox
            {
                Text = "Danh sách phiếu phạt",
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10),
                Margin = new Padding(12)
            };
            dgvFines = BuildGrid();
            grp.Controls.Add(dgvFines);
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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
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

            // ===== CỘT CHECKBOX — ĐẦU TIÊN =====
            var colCheck = new DataGridViewCheckBoxColumn
            {
                Name = "colSelect",
                HeaderText = "",
                FillWeight = 30,
                Width = 30,
                ReadOnly = false,
                Resizable = DataGridViewTriState.False
            };
            g.Columns.Add(colCheck);
            g.Columns["colSelect"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.Columns["colSelect"]!.DefaultCellStyle.Padding = new Padding(0);

            // ===== CÁC CỘT CÒN LẠI — ĐỦ 14 CỘT =====
            AddCol(g, "colId",       "Mã phạt",             60);
            AddCol(g, "colTicket",   "Mã phiếu mượn",       75);
            AddCol(g, "colReaderId", "Mã ĐG",               65);
            AddCol(g, "colReader",   "Tên độc giả",        120);
            AddCol(g, "colBook",     "Tên sách",           130);
            AddCol(g, "colCopy",     "Mã vạch",             70);
            AddCol(g, "colType",     "Loại vi phạm",        90);
            AddCol(g, "colReason",   "Lý do / Ghi chú",    140);
            AddCol(g, "colCreated",  "Ngày tạo",            85);
            AddCol(g, "colBy",       "Người lập",           85);
            AddCol(g, "colAmount",   "Tiền phạt",           80);
            AddCol(g, "colPaid",     "Đã trả",              70);
            AddCol(g, "colDebt",     "Còn nợ",              70);
            AddCol(g, "colStatus",   "Trạng thái",          90);
            AddCol(g, "colActions",  "Thao tác",           100);

            // ===== STYLE =====
            g.Columns["colId"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colId"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colAmount"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colDebt"]!.DefaultCellStyle.ForeColor = Red;
            g.Columns["colDebt"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colCreated"]!.DefaultCellStyle.Format = "dd/MM/yyyy";

            // ===== CHẶN EDIT CÁC CỘT KHÁC =====
            g.CellBeginEdit += (_, e) =>
            {
                if (g.Columns[e.ColumnIndex].Name != "colSelect")
                    e.Cancel = true;
            };

            // ===== COMMIT NGAY KHI CLICK CHECKBOX =====
            g.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (g.IsCurrentCellDirty)
                    g.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            g.CellPainting += DgvFines_CellPainting;
            g.CellMouseClick += DgvFines_CellMouseClick;
            
            g.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (g.Columns[e.ColumnIndex].Name != "colSelect") return;

                // Nếu vừa tick true → bỏ tick tất cả dòng khác
                bool isChecked = g.Rows[e.RowIndex].Cells["colSelect"].Value is bool b && b;
                if (!isChecked) return;

                foreach (DataGridViewRow r in g.Rows)
                {
                    if (r.Index == e.RowIndex) continue;
                    if (r.Cells["colSelect"].Value is bool other && other)
                        r.Cells["colSelect"].Value = false;
                }
            };

            return g;
        }

        private static void AddCol(DataGridView g, string name, string header, float weight)
        {
            g.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name, HeaderText = header, FillWeight = weight
            });
        }

        private void RenderGrid()
        {
            dgvFines.Rows.Clear();
            foreach (var r in _filtered)
            {
                int idx = dgvFines.Rows.Add();
                var row = dgvFines.Rows[idx];
                row.Tag = r;
                row.Cells["colSelect"].Value   = false;
                row.Cells["colId"].Value       = r.FineId;
                row.Cells["colTicket"].Value   = string.IsNullOrEmpty(r.TicketId) ? "—" : r.TicketId;
                row.Cells["colReaderId"].Value = r.ReaderId;
                row.Cells["colReader"].Value   = r.ReaderName;
                row.Cells["colBook"].Value     = string.IsNullOrEmpty(r.BookTitle) ? "Không áp dụng" : r.BookTitle;
                row.Cells["colCopy"].Value     = string.IsNullOrEmpty(r.CopyId) ? "—" : r.CopyId;
                row.Cells["colReason"].Value   = r.Reason;
                row.Cells["colCreated"].Value  = r.CreatedAt;
                row.Cells["colBy"].Value       = r.CreatedBy;
                row.Cells["colAmount"].Value   = r.Amount.ToString("N0") + " đ";
                row.Cells["colPaid"].Value     = r.Paid.ToString("N0") + " đ";
                row.Cells["colDebt"].Value     = r.Remaining.ToString("N0") + " đ";
                // colType, colStatus, colActions được vẽ trong CellPainting
            }
            dgvFines.ClearSelection();
        }

        /// <summary>
        /// Trả về dòng đang được tick checkbox (nếu có).
        /// Nếu nhiều dòng được tick, trả về dòng đầu tiên + cảnh báo.
        /// </summary>
        private FineRow? GetCheckedRow()
        {
            var checkedRows = dgvFines.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["colSelect"].Value is bool b && b)
                .ToList();

            if (checkedRows.Count == 0) return null;

            if (checkedRows.Count > 1)
            {
                MessageBox.Show("Chỉ được chọn 1 dòng để auto-fill. Sẽ dùng dòng đầu tiên.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return checkedRows[0].Tag as FineRow;
        }

        // ============================================================
        //  PAINT
        // ============================================================
        private void DgvFines_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics == null) return;
            string colName = dgvFines.Columns[e.ColumnIndex].Name;
            var row = dgvFines.Rows[e.RowIndex].Tag as FineRow;
            if (row == null) return;

            if (colName == "colType")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
                var (text, color) = row.Type switch
                {
                    FineType.Overdue => ("Quá hạn", Orange),
                    FineType.Damaged => ("Hư hỏng", Color.FromArgb(180, 90, 30)),
                    FineType.Lost    => ("Mất sách", Red),
                    _                => ("Khác", Gray)
                };
                DrawPill(e.Graphics, e.CellBounds, text, color);
                e.Handled = true;
            }
            else if (colName == "colStatus")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
                var (text, color) = row.Status switch
                {
                    FineStatus.Unpaid    => ("Chưa thanh toán", Red),
                    FineStatus.Partial   => ("Thanh toán 1 phần", Orange),
                    FineStatus.Paid      => ("Đã thanh toán", Green),
                    _                    => ("Đã hủy", Gray)
                };
                DrawPill(e.Graphics, e.CellBounds, text, color, fontSize: 8F);
                e.Handled = true;
            }
            else if (colName == "colActions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
                DrawActionButtons(e.Graphics, e.CellBounds, row);
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

        private void DrawActionButtons(Graphics g, Rectangle bounds, FineRow row)
        {
            int cx = bounds.X + bounds.Width / 2;
            bool canPay = row.Status == FineStatus.Unpaid || row.Status == FineStatus.Partial;

            var moreRect = new Rectangle(cx - 4, bounds.Y + (bounds.Height - 28) / 2, 28, 28);
            DrawIconButton(g, moreRect, GMore, TextGray);

            if (canPay)
            {
                var payRect = new Rectangle(cx - 46, bounds.Y + (bounds.Height - 28) / 2, 40, 28);
                DrawIconButton(g, payRect, GPay, Primary);
            }
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
        //  FILL TABLE
        // ============================================================
        private void ApplyFilter()
        {
            string q = txtSearch.Text.Trim().ToLowerInvariant();
            int typeIdx = cboType.SelectedIndex;
            int statusIdx = cboStatus.SelectedIndex;
            bool useDate = chkUseDate.Checked;
            var from = dtpFrom.Value.Date;
            var to = dtpTo.Value.Date;

            _filtered = _allRows.Where(r =>
            {
                if (!string.IsNullOrEmpty(q))
                {
                    string hay = $"{r.FineId} {r.ReaderId} {r.ReaderName} {r.TicketId} {r.CopyId}".ToLowerInvariant();
                    if (!hay.Contains(q)) return false;
                }
                if (typeIdx > 0 && (int)r.Type != typeIdx - 1) return false;
                if (statusIdx > 0 && (int)r.Status != statusIdx - 1) return false;
                if (useDate && (r.CreatedAt.Date < from || r.CreatedAt.Date > to)) return false;
                return true;
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

            RenderGrid();
            RecalcKpi();
        }

        private void RecalcKpi()
        {
            decimal total = _filtered.Sum(r => r.Amount);
            decimal paid = _filtered.Sum(r => r.Paid);
            decimal debt = _filtered.Sum(r => r.Remaining);

            lblKpiTotal.Text = total.ToString("N0") + " đ";
            lblKpiPaid.Text = paid.ToString("N0") + " đ";
            lblKpiDebt.Text = debt.ToString("N0") + " đ";
        }

        private void ResetFilter()
        {
            txtSearch.Clear();
            cboType.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;
            chkUseDate.Checked = false;
            ApplyFilter();
        }

        // ============================================================
        //  CLICK HANDLER
        // ============================================================
        private void DgvFines_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.Button != MouseButtons.Left) return;

            var row = dgvFines.Rows[e.RowIndex].Tag as FineRow;
            if (row == null) return;

            string colName = dgvFines.Columns[e.ColumnIndex].Name;

            // Click checkbox → bỏ qua, để DataGridView tự toggle
            if (colName == "colSelect") return;

            if (colName == "colId")
            {
                OpenDrawer(row);
                return;
            }

            if (colName != "colActions") return;

            int relX = e.Location.X;
            int cellW = dgvFines.Columns[e.ColumnIndex].Width;
            int cx = cellW / 2;

            bool canPay = row.Status == FineStatus.Unpaid || row.Status == FineStatus.Partial;
            bool onPayIcon  = canPay && relX >= cx - 50 && relX <= cx - 2;
            bool onMoreIcon = relX >= cx - 6 && relX <= cx + 30;

            if (onPayIcon)
            {
                ShowPayDialog(row);
                return;
            }

            ShowMoreMenu(row, e.RowIndex, e.ColumnIndex);
        }

        // ============================================================
        //  MENU ...
        // ============================================================
        private void ShowMoreMenu(FineRow row, int rowIndex, int colIndex)
        {
            try
            {
                var menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };

                // MỚI: chỉnh sửa toàn bộ thông tin
                var mnuEdit = new ToolStripMenuItem("  Chỉnh sửa thông tin",
                    GlyphImage(GEdit, 14, Color.Black), (_, _) => ShowEditDialog(row));

                var mnuAdjust = new ToolStripMenuItem("  Điều chỉnh mức phạt",
                    GlyphImage(GEdit, 14, Color.Orange), (_, _) => ShowAdjustDialog(row));
                var mnuCancel = new ToolStripMenuItem("  Hủy khoản phạt",
                    GlyphImage(GCancel, 14, Color.Red), (_, _) => ShowCancelDialog(row));
                var mnuHistory = new ToolStripMenuItem("  Xem lịch sử",
                    GlyphImage(GHistory, 14, Color.Black), (_, _) => OpenDrawer(row));
                var mnuPrint = new ToolStripMenuItem("  In biên lai",
                    GlyphImage(GPrint, 14, Color.Black), (_, _) => PrintReceipt(row));

                // Chỉ cho sửa khi chưa hủy và chưa thanh toán xong
                bool canEdit = row.Status != FineStatus.Cancelled && row.Status != FineStatus.Paid;
                mnuEdit.Enabled = canEdit;

                // Điều chỉnh mức phạt cũng chỉ khi chưa hủy
                mnuAdjust.Enabled = row.Status == FineStatus.Unpaid || row.Status == FineStatus.Partial;
                mnuCancel.Enabled = row.Status != FineStatus.Cancelled;

                menu.Items.Add(mnuEdit);
                menu.Items.Add(mnuAdjust);
                menu.Items.Add(mnuCancel);
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.Add(mnuHistory);
                menu.Items.Add(mnuPrint);

                var cellRect = dgvFines.GetCellDisplayRectangle(colIndex, rowIndex, false);
                var pt = dgvFines.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                menu.Show(pt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được menu thao tác:\n" + ex,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  MODAL: TẠO KHOẢN PHẠT
        // ============================================================
        private void ShowCreateFineDialog()
        {
            var checkedRow = GetCheckedRow();

            using var dlg = new FineCreateDialog.FineCreateDialog(checkedRow);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            var row = new FineRow
            {
                FineId = $"PP{(_allRows.Count + 1):D4}",
                ReaderId = dlg.ReaderId,           // ← lấy từ dialog (đã có thể sửa)
                ReaderName = dlg.ReaderName,       // ← lấy từ dialog
                TicketId = dlg.TicketId,           // ← lấy từ dialog
                BookTitle = checkedRow?.BookTitle ?? "",   // ← sách/vạch vẫn từ dòng nguồn
                CopyId    = checkedRow?.CopyId ?? "",
                Type = dlg.FineType,               // ← lấy từ dialog (đã có thể sửa)
                Reason = dlg.Reason,
                Amount = dlg.Amount,
                Paid = 0,
                Status = FineStatus.Unpaid,
                CreatedAt = DateTime.Now,
                CreatedBy = "Quản trị viên"
            };
            row.Audits.Add(new AuditRecord
            {
                At = DateTime.Now, Action = "Tạo mới", BeforeAmount = 0, AfterAmount = row.Amount,
                Reason = "Khởi tạo phiếu phạt thủ công", Actor = "Quản trị viên"
            });
            _allRows.Add(row);
            ApplyFilter();

            foreach (DataGridViewRow r in dgvFines.Rows)
                r.Cells["colSelect"].Value = false;
        }

        // ============================================================
        //  MODAL: THU TIỀN
        // ============================================================
        private void ShowPayDialog(FineRow row)
        {
            using var dlg = new FinePayDialog.FinePayDialog(row);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            row.Paid += dlg.PaidAmount;
            row.Payments.Add(new PaymentRecord
            {
                No = row.Payments.Count + 1,
                PaidAt = DateTime.Now,
                Amount = dlg.PaidAmount,
                Method = dlg.Method,
                Collector = "Quản trị viên"
            });
            row.Status = row.Paid >= row.Amount ? FineStatus.Paid : FineStatus.Partial;

            row.Audits.Add(new AuditRecord
            {
                At = DateTime.Now, Action = "Điều chỉnh", BeforeAmount = row.Amount,
                AfterAmount = row.Amount,
                Reason = $"Thu tiền ({dlg.Method})", Actor = "Quản trị viên"
            });

            ApplyFilter();
            if (_drawerRow == row) OpenDrawer(row);
        }
        
        private void ShowEditDialog(FineRow row)
{
    if (row.Status == FineStatus.Cancelled)
    {
        MessageBox.Show("Không thể chỉnh sửa phiếu đã hủy.",
            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }
    if (row.Status == FineStatus.Paid)
    {
        MessageBox.Show("Không thể chỉnh sửa phiếu đã thanh toán đầy đủ.",
            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    using var dlg = new FineEditDialog.FineEditDialog(row);
    if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

    // Lưu giá trị cũ để ghi audit
    decimal oldAmount = row.Amount;
    string oldReaderId = row.ReaderId;
    string oldReaderName = row.ReaderName;
    string oldTicketId = row.TicketId;
    FineType oldType = row.Type;
    string oldReason = row.Reason;

    // Cập nhật
    row.ReaderId = dlg.ReaderId;
    row.ReaderName = dlg.ReaderName;
    row.TicketId = dlg.TicketId;
    row.Type = dlg.FineType;
    row.Amount = dlg.Amount;
    row.Reason = dlg.Reason;

    // Cập nhật lại status nếu cần (trường hợp sửa Amount)
    if (row.Status != FineStatus.Cancelled)
    {
        if (row.Paid >= row.Amount) row.Status = FineStatus.Paid;
        else if (row.Paid > 0) row.Status = FineStatus.Partial;
        else row.Status = FineStatus.Unpaid;
    }

    // Ghi audit
    var changes = new List<string>();
    if (oldReaderId != row.ReaderId) changes.Add($"Mã ĐG: {oldReaderId} → {row.ReaderId}");
    if (oldReaderName != row.ReaderName) changes.Add($"Tên ĐG: {oldReaderName} → {row.ReaderName}");
    if (oldTicketId != row.TicketId) changes.Add($"Mã phiếu: {oldTicketId} → {row.TicketId}");
    if (oldType != row.Type) changes.Add($"Loại: {oldType} → {row.Type}");
    if (oldAmount != row.Amount) changes.Add($"Tiền: {oldAmount:N0} → {row.Amount:N0} đ");
    if (oldReason != row.Reason) changes.Add("Lý do: đã thay đổi");

    row.Audits.Add(new AuditRecord
    {
        At = DateTime.Now,
        Action = "Chỉnh sửa",
        BeforeAmount = oldAmount,
        AfterAmount = row.Amount,
        Reason = changes.Count > 0 ? string.Join("; ", changes) : "Chỉnh sửa thông tin phiếu phạt",
        Actor = "Quản trị viên"
    });

    ApplyFilter();
    if (_drawerRow == row) OpenDrawer(row);
}

        private void ShowAdjustDialog(FineRow row)
        {
            using var dlg = new FineAdjustDialog.FineAdjustDialog(row);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            row.Audits.Add(new AuditRecord
            {
                At = DateTime.Now, Action = "Điều chỉnh",
                BeforeAmount = row.Amount, AfterAmount = dlg.NewAmount,
                Reason = dlg.Reason, Actor = "Quản trị viên"
            });
            row.Amount = dlg.NewAmount;
            if (row.Paid >= row.Amount) row.Status = FineStatus.Paid;
            else if (row.Paid > 0) row.Status = FineStatus.Partial;
            else row.Status = FineStatus.Unpaid;

            ApplyFilter();
            if (_drawerRow == row) OpenDrawer(row);
        }

        private void ShowCancelDialog(FineRow row)
        {
            using var dlg = new FineCancelDialog.FineCancelDialog(row);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            row.Audits.Add(new AuditRecord
            {
                At = DateTime.Now, Action = "Hủy phiếu",
                BeforeAmount = row.Amount, AfterAmount = 0,
                Reason = dlg.Reason, Actor = "Quản trị viên"
            });
            row.Amount = 0;
            row.Status = FineStatus.Cancelled;

            ApplyFilter();
            if (_drawerRow == row) OpenDrawer(row);
        }

        private void PrintReceipt(FineRow row)
        {
            MessageBox.Show($"Đang mở xem trước biên lai {row.FineId}...\n(Tính năng in sẽ nối với PrintService)",
                "In biên lai", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ============================================================
        //  DRAWER
        // ============================================================
        private Panel BuildDrawer()
        {
            pnlDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 0,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            pnlDrawer.Paint += (s, e) =>
            {
                using var pen = new Pen(BorderGray);
                e.Graphics.DrawLine(pen, 0, 0, 0, pnlDrawer.Height);
            };

            var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = SoftBlue, Padding = new Padding(14, 10, 14, 10) };
            lblDrawerTitle = new Label
            {
                Text = "Chi tiết phiếu phạt",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true, Location = new Point(14, 12)
            };
            lblDrawerSub = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5F), ForeColor = TextGray,
                AutoSize = true, Location = new Point(14, 40)
            };
            var btnClose = new Button
            {
                Text = "",
                Size = new Size(30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Image = GlyphImage(GClose, 14, Color.Black)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => CloseDrawer();
            header.Controls.Add(lblDrawerTitle);
            header.Controls.Add(lblDrawerSub);
            header.Controls.Add(btnClose);
            header.Resize += (_, _) => btnClose.Location = new Point(header.Width - 40, 10);

            tabDrawer = new TabControl
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Padding = new Point(14, 4)
            };
            var tabPay = new TabPage("Lịch sử thanh toán") { BackColor = Color.White, Padding = new Padding(8) };
            var tabAudit = new TabPage("Lịch sử điều chỉnh & Hủy") { BackColor = Color.White, Padding = new Padding(8) };

            dgvPayments = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 228, 233),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 34 },
                ColumnHeadersHeight = 34,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F)
            };
            dgvPayments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
            dgvPayments.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvPayments.DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
            dgvPayments.Columns.Add("colNo", "Lần");
            dgvPayments.Columns.Add("colAt", "Ngày giờ thu");
            dgvPayments.Columns.Add("colAmt", "Số tiền");
            dgvPayments.Columns.Add("colMethod", "Phương thức");
            dgvPayments.Columns.Add("colCollector", "Người thu");
            dgvPayments.Columns["colNo"]!.FillWeight = 30;
            dgvPayments.Columns["colAt"]!.FillWeight = 100;
            dgvPayments.Columns["colAmt"]!.FillWeight = 70;
            dgvPayments.Columns["colMethod"]!.FillWeight = 70;
            dgvPayments.Columns["colCollector"]!.FillWeight = 80;
            dgvPayments.Columns["colAmt"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            tabPay.Controls.Add(dgvPayments);

            flowAudit = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = true, BackColor = Color.White
            };
            flowAudit.Resize += (_, _) =>
            {
                foreach (Control c in flowAudit.Controls) c.Width = flowAudit.ClientSize.Width - 8;
            };
            tabAudit.Controls.Add(flowAudit);

            tabDrawer.TabPages.Add(tabPay);
            tabDrawer.TabPages.Add(tabAudit);

            pnlDrawer.Controls.Add(tabDrawer);
            pnlDrawer.Controls.Add(header);
            return pnlDrawer;
        }

        private void OpenDrawer(FineRow row)
        {
            _drawerRow = row;
            lblDrawerTitle.Text = $"Phiếu phạt {row.FineId}";
            lblDrawerSub.Text = $"{row.ReaderName} · {row.ReaderId}  —  {row.Amount:N0} đ / còn nợ {row.Remaining:N0} đ";

            dgvPayments.Rows.Clear();
            foreach (var p in row.Payments)
            {
                int i = dgvPayments.Rows.Add();
                dgvPayments.Rows[i].Cells["colNo"].Value = p.No;
                dgvPayments.Rows[i].Cells["colAt"].Value = p.PaidAt.ToString("dd/MM/yyyy HH:mm");
                dgvPayments.Rows[i].Cells["colAmt"].Value = p.Amount.ToString("N0") + " đ";
                dgvPayments.Rows[i].Cells["colMethod"].Value = p.Method;
                dgvPayments.Rows[i].Cells["colCollector"].Value = p.Collector;
            }
            dgvPayments.ClearSelection();
            if (row.Payments.Count == 0)
            {
                int i = dgvPayments.Rows.Add();
                dgvPayments.Rows[i].Cells["colNo"].Value = "—";
                dgvPayments.Rows[i].Cells["colAt"].Value = "Chưa có lần thanh toán nào";
            }

            flowAudit.Controls.Clear();
            foreach (var a in row.Audits.OrderByDescending(x => x.At))
            {
                flowAudit.Controls.Add(BuildAuditItem(a));
            }

            pnlDrawer.Width = 480;
        }

        private void CloseDrawer()
        {
            pnlDrawer.Width = 0;
            _drawerRow = null;
        }

        private Panel BuildAuditItem(AuditRecord a)
        {
            var color = a.Action switch
            {
                "Tạo mới" => Primary,
                "Điều chỉnh" => Orange,
                "Chỉnh sửa" => Color.FromArgb(100, 130, 200),   // MỚI: xanh lam
                "Hủy phiếu" => Red,
                _ => TextGray
            };
            var panel = new Panel
            {
                Height = 100,
                BackColor = Color.FromArgb(250, 251, 252),
                Margin = new Padding(4, 0, 4, 8),
                Padding = new Padding(10)
            };
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(BorderGray);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                using var bar = new SolidBrush(color);
                e.Graphics.FillRectangle(bar, 0, 0, 4, panel.Height);
            };

            panel.Controls.Add(new Label
            {
                Text = a.Action, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = color, AutoSize = true, Location = new Point(14, 8)
            });
            panel.Controls.Add(new Label
            {
                Text = a.At.ToString("dd/MM/yyyy HH:mm"),
                Font = new Font("Segoe UI", 8F), ForeColor = TextGray,
                AutoSize = true, Location = new Point(150, 11)
            });
            panel.Controls.Add(new Label
            {
                Text = a.BeforeAmount == a.AfterAmount
                    ? $"Mức tiền: {a.AfterAmount:N0} đ"
                    : $"Mức tiền: {a.BeforeAmount:N0} đ  →  {a.AfterAmount:N0} đ",
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true, Location = new Point(14, 34)
            });
            panel.Controls.Add(new Label
            {
                Text = "Lý do: " + a.Reason,
                Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = false, Size = new Size(panel.Width - 28, 22),
                Location = new Point(14, 54),
                AutoEllipsis = true
            });
            panel.Controls.Add(new Label
            {
                Text = "Người thao tác: " + a.Actor,
                Font = new Font("Segoe UI", 8F), ForeColor = TextGray,
                AutoSize = true, Location = new Point(14, 78)
            });
            return panel;
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
    }
}