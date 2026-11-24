using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Repositories;
using LibraryManagement.Repositories.Interafaces;
using LibraryManagement.Services;
using Microsoft.EntityFrameworkCore;
using ContentAlignment = System.Drawing.ContentAlignment;

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

        // ====== Model UI ======
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
            public int BeforeAmount { get; set; }
            public int AfterAmount { get; set; }
            public string Reason { get; set; } = "";
            public string Actor { get; set; } = "";
        }

        public sealed class FineRow
        {
            public int DbId { get; set; }
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

        // ====== Service ======
        private readonly FineService _fineService;
        private readonly LibraryDbContext _dbContext;

        // ====== Data ======
        private List<FineRow> _allRows = new();
        private List<FineRow> _filtered = new();
        private FineRow? _drawerRow;
        private bool _isLoading;

        // ====== Control ======
        private Label lblKpiTotal = null!;
        private Label lblKpiPaid  = null!;
        private Label lblKpiDebt  = null!;

        private TextBox txtSearch   = null!;
        private ComboBox cboType    = null!;
        private ComboBox cboStatus  = null!;
        private CheckBox chkUseDate = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo   = null!;

        private DataGridView dgvFines = null!;

        private Panel pnlDrawer        = null!;
        private Label lblDrawerTitle   = null!;
        private Label lblDrawerSub     = null!;
        private TabControl tabDrawer   = null!;
        private DataGridView dgvPayments = null!;
        private FlowLayoutPanel flowAudit = null!;

        public FineManagement()
        {
            InitializeComponent();

            // TODO: thay bằng DI thật nếu dự án có
            _dbContext = new LibraryDbContext();
            var repo = new FineRepositoryImpl();
            _fineService = new FineService(_dbContext, repo);

            BuildUi();
            _ = LoadDataAsync();
        }

        // ============================================================
        //  MAPPING (inline — không cần file mapper)
        // ============================================================
        private static FineType MapType(ViolationType? t) => t switch
        {
            ViolationType.Overdue => FineType.Overdue,
            ViolationType.Damaged => FineType.Damaged,
            ViolationType.Lost    => FineType.Lost,
            _                     => FineType.Other
        };

        private static ViolationType MapType(FineType t) => t switch
        {
            FineType.Overdue => ViolationType.Overdue,
            FineType.Damaged => ViolationType.Damaged,
            FineType.Lost    => ViolationType.Lost,
            _                => ViolationType.Other
        };

        private static FineStatus MapStatus(LibraryManagement.Enums.FineStatus? s) => s switch
        {
            LibraryManagement.Enums.FineStatus.Unpaid  => FineStatus.Unpaid,
            LibraryManagement.Enums.FineStatus.Partial => FineStatus.Partial,
            LibraryManagement.Enums.FineStatus.Paid    => FineStatus.Paid,
            _                                           => FineStatus.Cancelled
        };

        private static FineRow ToFineRow(FineManagementInfo dto) => new()
        {
            DbId       = dto.FineId,
            FineId     = dto.FineCode,
            TicketId   = dto.LoanCode ?? "",
            ReaderId   = dto.ReaderCode,
            ReaderName = dto.ReaderName,
            BookTitle  = dto.BookTitle ?? "",
            CopyId     = dto.CopyBarcode ?? "",
            Type       = MapType(dto.ViolationType),
            Reason     = dto.Description ?? "",
            CreatedAt  = dto.CreatedAt,
            Amount     = dto.AmountVnd,   // int → decimal implicit
            Paid       = dto.PaidVnd,     // int → decimal implicit
            Status     = MapStatus(dto.Status)
        };

        // ============================================================
        //  LOAD DATA
        // ============================================================
        private async Task LoadDataAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                Cursor = Cursors.WaitCursor;
                var list = await _fineService.GetFineManagementInfoAsync(new FineManagementFilter());
                _allRows = list.Select(ToFineRow).ToList();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được danh sách phiếu phạt:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                _isLoading = false;
            }
        }

        // ============================================================
        //  RESOLVE LoanDetailId TỪ TicketId + CopyId
        // ============================================================
        private async Task<int?> ResolveLoanDetailIdAsync(string loanCode, string? copyCode)
        {
            if (string.IsNullOrWhiteSpace(loanCode)) return null;

            var q = _dbContext.LoanDetails
                .Include(ld => ld.Loan)
                .Include(ld => ld.BookCopy)
                .Where(ld => ld.Loan.LoanCode == loanCode);

            if (!string.IsNullOrWhiteSpace(copyCode))
                q = q.Where(ld => ld.BookCopy.CopyCode == copyCode);

            var ld = await q.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            return ld?.Id;
        }

        // ============================================================
        //  DỰNG GIAO DIỆN (giữ nguyên như file cũ)
        // ============================================================
        private void BuildUi()
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 9F);

            Controls.Add(BuildTableArea());
            Controls.Add(BuildToolbar());
            Controls.Add(BuildKpiArea());
            Controls.Add(BuildDrawer());

            ResumeLayout(true);
        }

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

        private Panel BuildToolbar()
        {
            var host = new Panel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(12, 8, 12, 0) };
            var grp = new GroupBox { Text = "Tìm kiếm & Bộ lọc", Dock = DockStyle.Fill, Padding = new Padding(12, 6, 12, 6) };

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0), Margin = new Padding(0) };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var row1 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0) };
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));

            txtSearch = new TextBox
            {
                PlaceholderText = "Tìm theo mã phạt, mã ĐG, tên ĐG, mã phiếu mượn, mã vạch...",
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 4, 8, 4)
            };
            txtSearch.TextChanged += (_, _) => ApplyFilter();
            row1.Controls.Add(txtSearch, 0, 0);

            var btnReset = new Button
            {
                Text = "  Làm mới", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                BackColor = Color.White, Font = new Font("Segoe UI", 9F),
                Image = GlyphImage(GRefresh, 14, Color.Black),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Margin = new Padding(0, 4, 8, 4)
            };
            btnReset.FlatAppearance.BorderColor = BorderGray;
            btnReset.Click += async (_, _) => { ResetFilter(); await LoadDataAsync(); };
            row1.Controls.Add(btnReset, 1, 0);

            var btnAdd = new Button
            {
                Text = "  Tạo khoản phạt", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                BackColor = Primary, ForeColor = Color.White,
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

            var row2 = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false, AutoScroll = true,
                Margin = new Padding(0), Padding = new Padding(0, 2, 0, 0)
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
                Text = "Ngày tạo:", AutoSize = true, ForeColor = TextGray,
                Font = new Font("Segoe UI", 8.5F), Margin = new Padding(12, 7, 4, 0)
            };
            row2.Controls.Add(chkUseDate);

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short, Width = 110,
                Enabled = false, Value = DateTime.Today.AddDays(-30),
                Margin = new Padding(0, 3, 0, 0)
            };
            row2.Controls.Add(dtpFrom);

            row2.Controls.Add(new Label { Text = "–", AutoSize = true, ForeColor = TextGray, Margin = new Padding(4, 7, 4, 0) });

            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short, Width = 110,
                Enabled = false, Value = DateTime.Today,
                Margin = new Padding(0, 3, 0, 0)
            };
            row2.Controls.Add(dtpTo);

            chkUseDate.CheckedChanged += (_, _) =>
            {
                dtpFrom.Enabled = dtpTo.Enabled = chkUseDate.Checked;
                ApplyFilter();
            };
            dtpFrom.ValueChanged += (_, _) => ApplyFilter();
            dtpTo.ValueChanged   += (_, _) => ApplyFilter();

            tbl.Controls.Add(row2, 0, 1);

            grp.Controls.Add(tbl);
            host.Controls.Add(grp);
            return host;
        }

        private static Label FilterLabel(string text) => new()
        {
            Text = text, AutoSize = true, ForeColor = Color.FromArgb(110, 117, 128),
            Font = new Font("Segoe UI", 8.5F), Margin = new Padding(0, 10, 6, 0)
        };

        private static ComboBox MakeComboFilter(int width, string[] items)
        {
            var c = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Width = width,
                Font = new Font("Segoe UI", 9F), Margin = new Padding(0, 6, 12, 0)
            };
            c.Items.AddRange(items);
            c.SelectedIndex = 0;
            return c;
        }

        private GroupBox BuildTableArea()
        {
            var grp = new GroupBox
            {
                Text = "Danh sách phiếu phạt", Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10), Margin = new Padding(12)
            };
            dgvFines = BuildGrid();
            grp.Controls.Add(dgvFines);
            return grp;
        }

        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, RowHeadersVisible = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 228, 233),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 40 }, ColumnHeadersHeight = 38,
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

            var colCheck = new DataGridViewCheckBoxColumn
            {
                Name = "colSelect", HeaderText = "", FillWeight = 30, Width = 30,
                ReadOnly = false, Resizable = DataGridViewTriState.False
            };
            g.Columns.Add(colCheck);
            g.Columns["colSelect"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.Columns["colSelect"]!.DefaultCellStyle.Padding = new Padding(0);

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

            g.Columns["colId"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colId"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colAmount"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colDebt"]!.DefaultCellStyle.ForeColor = Red;
            g.Columns["colDebt"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colCreated"]!.DefaultCellStyle.Format = "dd/MM/yyyy";

            g.CellBeginEdit += (_, e) =>
            {
                if (g.Columns[e.ColumnIndex].Name != "colSelect")
                    e.Cancel = true;
            };
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
            }
            dgvFines.ClearSelection();
        }

        private FineRow? GetCheckedRow()
        {
            var checkedRows = dgvFines.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Cells["colSelect"].Value is bool b && b).ToList();
            if (checkedRows.Count == 0) return null;
            if (checkedRows.Count > 1)
                MessageBox.Show("Chỉ được chọn 1 dòng để auto-fill. Sẽ dùng dòng đầu tiên.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return checkedRows[0].Tag as FineRow;
        }

        // ============================================================
        //  PAINT (giữ nguyên)
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
                    FineStatus.Unpaid  => ("Chưa thanh toán", Red),
                    FineStatus.Partial => ("Thanh toán 1 phần", Orange),
                    FineStatus.Paid    => ("Đã thanh toán", Green),
                    _                  => ("Đã hủy", Gray)
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
            using (var pen  = new Pen(Color.FromArgb(150, color)))
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
            using (var pen  = new Pen(Color.FromArgb(120, color)))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
            using var f = new Font(IconFont, 12F);
            TextRenderer.DrawText(g, glyph, f, rect, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        // ============================================================
        //  FILTER (client-side — giữ nguyên)
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
            decimal paid  = _filtered.Sum(r => r.Paid);
            decimal debt  = _filtered.Sum(r => r.Remaining);
            lblKpiTotal.Text = total.ToString("N0") + " đ";
            lblKpiPaid.Text  = paid.ToString("N0") + " đ";
            lblKpiDebt.Text  = debt.ToString("N0") + " đ";
        }

        private void ResetFilter()
        {
            txtSearch.Clear();
            cboType.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;
            chkUseDate.Checked = false;
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

        private void ShowMoreMenu(FineRow row, int rowIndex, int colIndex)
        {
            try
            {
                var menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };

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

                bool canEdit = row.Status != FineStatus.Cancelled && row.Status != FineStatus.Paid;
                mnuEdit.Enabled = canEdit;
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
        private async void ShowCreateFineDialog()
        {
            var checkedRow = GetCheckedRow();
            using var dlg = new FineCreateDialog.FineCreateDialog(checkedRow);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
        
            // ⭐ Validate: bắt buộc phải có mã phiếu mượn để resolve LoanDetailId
            if (string.IsNullOrWhiteSpace(dlg.TicketId))
            {
                MessageBox.Show("Vui lòng nhập Mã phiếu mượn — hệ thống cần mã này để xác định phiếu mượn chi tiết.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        
            int? loanDetailId = await ResolveLoanDetailIdAsync(dlg.TicketId, checkedRow?.CopyId);
            if (loanDetailId == null)
            {
                MessageBox.Show(
                    "Không tìm thấy phiếu mượn chi tiết tương ứng.\n" +
                    "Vui lòng kiểm tra lại Mã phiếu mượn / Mã vạch sách.",
                    "Không tìm thấy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        
            var req = new CreateFineManagementRequest
            {
                LoanDetailId  = loanDetailId.Value,
                ViolationType = MapType(dlg.FineType),
                FineAmount    = dlg.Amount,      // int — khớp backend mới
                Note          = dlg.Reason
            };
        
            try
            {
                await _fineService.CreateFineAsync(req);
                await LoadDataAsync();
                MessageBox.Show("Đã tạo phiếu phạt.", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tạo phiếu phạt thất bại:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
            foreach (DataGridViewRow r in dgvFines.Rows)
                r.Cells["colSelect"].Value = false;
        }

        // ============================================================
        //  MODAL: THU TIỀN
        // ============================================================
        private async void ShowPayDialog(FineRow row)
        {
            if (row.DbId <= 0)
            {
                MessageBox.Show("Phiếu phạt chưa có DbId — không thể thu tiền.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var dlg = new FinePayDialog.FinePayDialog(row);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            PaymentMethod method = dlg.Method == "Chuyển khoản"
                ? PaymentMethod.Card
                : PaymentMethod.Cash;

            try
            {
                // dlg.PaidAmount là int — khớp PayFineAsync(int amount, ...)
                await _fineService.PayFineAsync(row.DbId, dlg.PaidAmount, method, null);
                await LoadDataAsync();

                if (_drawerRow != null && _drawerRow.DbId == row.DbId)
                {
                    var updated = _allRows.FirstOrDefault(x => x.DbId == row.DbId);
                    if (updated != null) await OpenDrawerAsync(updated);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thu tiền thất bại:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  MODAL: CHỈNH SỬA
        // ============================================================
        private async void ShowEditDialog(FineRow row)
{
    if (row.Status == FineStatus.Cancelled || row.Status == FineStatus.Paid)
    {
        MessageBox.Show("Không thể chỉnh sửa phiếu đã hủy hoặc đã thanh toán đầy đủ.",
            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    using var dlg = new FineEditDialog.FineEditDialog(row);
    if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

    // ⭐ Validate: cần TicketId để resolve LoanDetailId
    if (string.IsNullOrWhiteSpace(dlg.TicketId))
    {
        MessageBox.Show("Vui lòng nhập Mã phiếu mượn — hệ thống cần mã này để xác định phiếu mượn chi tiết.",
            "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    int? loanDetailId = await ResolveLoanDetailIdAsync(dlg.TicketId, row.CopyId);
    if (loanDetailId == null)
    {
        MessageBox.Show("Không tìm thấy phiếu mượn chi tiết tương ứng.",
            "Không tìm thấy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // ⭐ Nếu người dùng sửa Mã độc giả / Tên độc giả, cảnh báo không được lưu
    if (!string.Equals(dlg.ReaderId, row.ReaderId, StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(dlg.ReaderName, row.ReaderName, StringComparison.Ordinal))
    {
        MessageBox.Show(
            "Mã độc giả và Tên độc giả được suy ra từ Mã phiếu mượn nên không được lưu trực tiếp.\n" +
            "Nếu muốn đổi độc giả, vui lòng đổi Mã phiếu mượn.",
            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    var req = new UpdateFineManagementRequest
    {
        Id            = row.DbId,
        LoanDetailId  = loanDetailId.Value,
        ViolationType = MapType(dlg.FineType),
        FineAmount    = dlg.Amount,      // int — khớp backend
        Note          = dlg.Reason
    };

    try
    {
        await _fineService.UpdateFineAsync(req);
        await LoadDataAsync();
        if (_drawerRow != null && _drawerRow.DbId == row.DbId)
        {
            var updated = _allRows.FirstOrDefault(x => x.DbId == row.DbId);
            if (updated != null) await OpenDrawerAsync(updated);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Chỉnh sửa thất bại:\n" + ex.Message,
            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        // ============================================================
        //  MODAL: ĐIỀU CHỈNH MỨC PHẠT
        // ============================================================
        private async void ShowAdjustDialog(FineRow row)
        {
            using var dlg = new FineAdjustDialog.FineAdjustDialog(row);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            try
            {
                // dlg.NewAmount là int — khớp AdjustFineAmountAsync(int fineId, int fineAmount, ...)
                await _fineService.AdjustFineAmountAsync(row.DbId, dlg.NewAmount, dlg.Reason);
                await LoadDataAsync();
                if (_drawerRow != null && _drawerRow.DbId == row.DbId)
                {
                    var updated = _allRows.FirstOrDefault(x => x.DbId == row.DbId);
                    if (updated != null) await OpenDrawerAsync(updated);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Điều chỉnh thất bại:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  MODAL: HỦY
        // ============================================================
        private async void ShowCancelDialog(FineRow row)
        {
            if (row.DbId <= 0)
            {
                MessageBox.Show("Phiếu phạt chưa có DbId — không thể hủy.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var dlg = new FineCancelDialog.FineCancelDialog(row);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

            try
            {
                await _fineService.CancelFineAsync(row.DbId, dlg.Reason);
                await LoadDataAsync();
                if (_drawerRow != null && _drawerRow.DbId == row.DbId)
                {
                    var updated = _allRows.FirstOrDefault(x => x.DbId == row.DbId);
                    if (updated != null) await OpenDrawerAsync(updated);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hủy phiếu thất bại:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            pnlDrawer = new Panel { Dock = DockStyle.Right, Width = 0, BackColor = Color.White, Padding = new Padding(0) };
            pnlDrawer.Paint += (s, e) =>
            {
                using var pen = new Pen(BorderGray);
                e.Graphics.DrawLine(pen, 0, 0, 0, pnlDrawer.Height);
            };

            var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = SoftBlue, Padding = new Padding(14, 10, 14, 10) };
            lblDrawerTitle = new Label
            {
                Text = "Chi tiết phiếu phạt", Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true, Location = new Point(14, 12)
            };
            lblDrawerSub = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8.5F), ForeColor = TextGray,
                AutoSize = true, Location = new Point(14, 40)
            };
            var btnClose = new Button
            {
                Text = "", Size = new Size(30, 30),
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

            tabDrawer = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Padding = new Point(14, 4) };
            var tabPay   = new TabPage("Lịch sử thanh toán") { BackColor = Color.White, Padding = new Padding(8) };
            var tabAudit = new TabPage("Lịch sử điều chỉnh & Hủy") { BackColor = Color.White, Padding = new Padding(8) };

            dgvPayments = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 228, 233),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 34 }, ColumnHeadersHeight = 34,
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
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.White
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

        private void OpenDrawer(FineRow row) => _ = OpenDrawerAsync(row);

        private static string MapAuditAction(FineAuditAction action) => action switch
        {
            FineAuditAction.Created  => "Tạo mới",
            FineAuditAction.Adjusted => "Điều chỉnh",
            FineAuditAction.Cancelled => "Hủy phiếu",
            FineAuditAction.Paid     => "Thu tiền",
            _                        => action.ToString()
        };
        
        private async Task OpenDrawerAsync(FineRow row)
        {
            _drawerRow = row;
            lblDrawerTitle.Text = $"Phiếu phạt {row.FineId}";
            lblDrawerSub.Text = $"{row.ReaderName} · {row.ReaderId}  —  {row.Amount:N0} đ / còn nợ {row.Remaining:N0} đ";

            // Nạp payments + audits từ DB (nếu Service đã có GetFineDetailAsync)
            try
            {
                var detail = await _fineService.GetFineDetailAsync(row.DbId);
                row.Payments = detail?.Payments.Select(p => new PaymentRecord
                {
                    No        = p.No,
                    PaidAt    = p.PaidAt,
                    Amount    = (decimal)p.AmountVnd,
                    Method    = p.PaymentMethod.ToString(),
                    Collector = "Thủ thư"
                }).ToList() ?? new();

                row.Audits = detail?.Audits.Select(a => new AuditRecord
                {
                    At           = a.At,
                    Action       = MapAuditAction(a.Action),   
                    BeforeAmount = a.BeforeAmount,            
                    AfterAmount  = a.AfterAmount,
                    Reason       = a.Reason ?? "",
                    Actor        = a.ActorName ?? "Thủ thư"
                }).ToList() ?? new();
            }
            catch
            {
                // Nếu Service chưa có GetFineDetailAsync → bỏ qua, drawer hiển thị trống
            }

            dgvPayments.Rows.Clear();
            foreach (var p in row.Payments)
            {
                int i = dgvPayments.Rows.Add();
                dgvPayments.Rows[i].Cells["colNo"].Value        = p.No;
                dgvPayments.Rows[i].Cells["colAt"].Value        = p.PaidAt.ToString("dd/MM/yyyy HH:mm");
                dgvPayments.Rows[i].Cells["colAmt"].Value       = p.Amount.ToString("N0") + " đ";
                dgvPayments.Rows[i].Cells["colMethod"].Value    = p.Method;
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
                flowAudit.Controls.Add(BuildAuditItem(a));

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
                "Tạo mới"    => Primary,
                "Điều chỉnh" => Orange,
                "Chỉnh sửa"  => Color.FromArgb(100, 130, 200),
                "Hủy phiếu"  => Red,
                "Thu tiền"   => Green,          // ⭐ thêm
                _            => TextGray
            };
            var panel = new Panel
            {
                Height = 100, BackColor = Color.FromArgb(250, 251, 252),
                Margin = new Padding(4, 0, 4, 8), Padding = new Padding(10)
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

            // ⭐ Đổi nhãn theo loại action
            string amountText;
            if (a.Action == "Thu tiền")
            {
                amountText = $"Nợ: {a.BeforeAmount:N0} đ  →  {a.AfterAmount:N0} đ";
            }
            else if (a.BeforeAmount == a.AfterAmount)
            {
                amountText = $"Mức tiền: {a.AfterAmount:N0} đ";
            }
            else
            {
                amountText = $"Mức tiền: {a.BeforeAmount:N0} đ  →  {a.AfterAmount:N0} đ";
            }

            panel.Controls.Add(new Label
            {
                Text = amountText,
                Font = new Font("Segoe UI", 8.5F), AutoSize = true, Location = new Point(14, 34)
            });
            panel.Controls.Add(new Label
            {
                Text = "Lý do: " + a.Reason,
                Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = false, Size = new Size(panel.Width - 28, 22),
                Location = new Point(14, 54), AutoEllipsis = true
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