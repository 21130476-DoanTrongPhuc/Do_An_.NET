using System.Drawing.Drawing2D;

namespace LibraryManagement.Forms
{
    public partial class LibrarianFormMain : Form
    {
        // ====== Màu sắc ======
        private static readonly Color Primary = Color.FromArgb(19, 98, 160);
        private static readonly Color BorderGray = Color.FromArgb(205, 210, 218);
        private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
        private static readonly Color SoftBlue = Color.FromArgb(232, 240, 250);
        private static readonly Color Orange = Color.FromArgb(214, 140, 10);
        private static readonly Color Green = Color.FromArgb(34, 160, 90);
        private static readonly Color Red = Color.FromArgb(200, 40, 40);
        private const string IconFont = "Segoe MDL2 Assets";

        // Glyph (Segoe MDL2 Assets)
        private const string GHome = "\uE80F", GBook = "\uE82D", GPeople = "\uE716", GAdd = "\uE710",
            GForward = "\uE72A", GBack = "\uE72B", GRefresh = "\uE72C", GHelp = "\uE897",
            GWarn = "\uE7BA", GCalendar = "\uE787", GLibrary = "\uE8F1";

        public bool IsLogout { get; private set; }

        // Control cần truy cập sau
        private TableLayoutPanel tblStats = null!;
        private DataGridView dgvTrans = null!;
        private FlowLayoutPanel flowNotice = null!;
        private Label lblDate = null!;
        private Label lblUserName = null!;

        public LibrarianFormMain()
        {
            InitializeComponent();
            Text = "LIBRA - Quản lý thư viện";
            BuildUi();
            LoadData();
        }

        // ============================================================
        //  DỮ LIỆU - thay bằng BookService / Repository khi nối CSDL
        // ============================================================
        private void LoadData()
        {
            var d = DateTime.Now;
            string[] days = { "Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
            lblDate.Text = $"{days[(int)d.DayOfWeek]}, ngày {d:dd} tháng {d:MM} năm {d:yyyy}";
            lblUserName.Text = "Quản trị viên";

            // Thống kê hôm nay (Models/HomeSummary.cs)
            tblStats.Controls.Clear();
            AddStat(0, GBook, "Tổng đầu sách", "2.486", "+32 trong tháng", false);
            AddStat(1, GPeople, "Độc giả hoạt động", "1.248", "86 độc giả mới", false);
            AddStat(2, GForward, "Sách đang mượn", "386", "62 lượt hôm nay", false);
            AddStat(3, GWarn, "Phiếu quá hạn", "24", "Cần xử lý", true);

            // Giao dịch gần đây (Models/BorrowRecord.cs)
            dgvTrans.Rows.Clear();
            dgvTrans.Rows.Add("PM-240315", "Nguyễn Minh Anh", "Nhà giả kim", "15/03/2024", "29/03/2024", "Đang mượn");
            dgvTrans.Rows.Add("PM-240314", "Trần Hoàng Nam", "Đắc nhân tâm", "14/03/2024", "28/03/2024", "Sắp đến hạn");
            dgvTrans.Rows.Add("PM-240311", "Lê Thu Hà", "Lược sử thời gian", "11/03/2024", "25/03/2024", "Quá hạn");
            dgvTrans.Rows.Add("PM-240309", "Đỗ Hải Yến", "Mắt biếc", "09/03/2024", "23/03/2024", "Quá hạn");
            dgvTrans.ClearSelection();

            // Thông báo và nhắc việc (Models/Notification.cs)
            flowNotice.Controls.Clear();
            AddNotice(GWarn, Red, "24 phiếu đã quá hạn", "Cần liên hệ độc giả và xử lý", "Kiểm tra");
            AddNotice(GPeople, Primary, "38 thẻ sắp hết hạn", "Trong vòng 30 ngày tới", "Xem");
            AddNotice(GBook, Orange, "12 đầu sách sắp hết", "Số lượng khả dụng dưới 2 cuốn", "Xem");
        }

        // ============================================================
        //  DỰNG GIAO DIỆN
        // ============================================================
        private void BuildUi()
        {
            SuspendLayout();

            // ---- TabControl (Fill - add đầu tiên) ----
            var tabMain = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Padding = new Point(14, 4) };
            var tabOverview = new TabPage("Tổng quan") { BackColor = Color.FromArgb(240, 242, 245), Padding = new Padding(8) };
            tabMain.TabPages.Add(tabOverview);
            BuildOverview(tabOverview);
            Controls.Add(tabMain);

            Controls.Add(BuildStatusBar());      // Bottom
            Controls.Add(BuildToolbar());        // Top
            Controls.Add(BuildMenu());           // Top (trên cùng)

            ResumeLayout(true);
        }

        private MenuStrip BuildMenu()
        {
            var menu = new MenuStrip { BackColor = Color.White, Dock = DockStyle.Top };
            foreach (var t in new[] { "Hệ thống", "Danh mục", "Lưu thông", "Báo cáo", "Trợ giúp" })
                menu.Items.Add(new ToolStripMenuItem(t));
            MainMenuStrip = menu;
            return menu;
        }

        private Panel BuildToolbar()
        {
            var host = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.FromArgb(246, 247, 249) };
            host.Paint += (s, e) => e.Graphics.DrawLine(new Pen(BorderGray), 0, host.Height - 1, host.Width, host.Height - 1);

            var ts = new ToolStrip
            {
                Dock = DockStyle.Fill, GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.FromArgb(246, 247, 249),
                ImageScalingSize = new Size(24, 24), Padding = new Padding(6, 4, 0, 0)
            };
            AddTool(ts, "Trang chủ", GHome, () => Todo("Trang chủ"));
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Thêm đầu sách", GBook, () => Todo("Thêm đầu sách"));
            AddTool(ts, "Bản sách", GBook, () => Todo("Bản sách"));
            AddTool(ts, "Thêm độc giả", GPeople, () => Todo("Thêm độc giả"));
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Lập phiếu mượn", GForward, () => Todo("Lập phiếu mượn"));
            AddTool(ts, "Nhận trả sách", GBack, () => Todo("Nhận trả sách"));
            AddTool(ts, "Đặt trước", GCalendar, () => Todo("Đặt trước"));
            ts.Items.Add(new ToolStripSeparator());
            AddTool(ts, "Làm mới", GRefresh, LoadData);
            AddTool(ts, "Trợ giúp", GHelp, () => Todo("Trợ giúp"));

            // Khu người dùng bên phải
            var user = new Panel { Dock = DockStyle.Right, Width = 340 };
            user.Controls.Add(new Label { Text = "Người dùng:", AutoSize = true, ForeColor = TextGray, Font = new Font("Segoe UI", 9F), Location = new Point(8, 22) });
            lblUserName = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(84, 22) };
            user.Controls.Add(lblUserName);
            var btnLogout = new Button
            {
                Text = "Đăng xuất", FlatStyle = FlatStyle.Flat, BackColor = Color.White, Size = new Size(92, 32),
                Location = new Point(user.Width - 104, 14), Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.FlatAppearance.BorderColor = BorderGray;
            btnLogout.Click += btnLogout_Click;
            user.Controls.Add(btnLogout);

            host.Controls.Add(ts);
            host.Controls.Add(user);
            return host;
        }

        private StatusStrip BuildStatusBar()
        {
            var st = new StatusStrip { Dock = DockStyle.Bottom, SizingGrip = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Segoe UI", 8F) };
            st.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripStatusLabel("●") { ForeColor = Green },
                new ToolStripStatusLabel("Đã kết nối"),
                new ToolStripSeparator(),
                new ToolStripStatusLabel("Thư viện Trung tâm · Giờ mở cửa: 07:30 - 20:00"),
                new ToolStripStatusLabel { Spring = true },
                new ToolStripSeparator(),
                new ToolStripStatusLabel("Thủ thư"),
                new ToolStripSeparator(),
                new ToolStripStatusLabel("Phiên bản 2.4.0")
            });
            return st;
        }

        private void BuildOverview(TabPage page)
        {
            // ---------- Khối dưới: giao dịch + thông báo (Fill - add đầu) ----------
            var pnlBottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };

            var grpTrans = new GroupBox { Text = "Giao dịch gần đây", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 10) };
            dgvTrans = BuildGrid();
            grpTrans.Controls.Add(dgvTrans);

            var noticeHost = new Panel { Dock = DockStyle.Right, Width = 560, Padding = new Padding(12, 0, 0, 0) };
            var grpNotice = new GroupBox { Text = "Thông báo và nhắc việc", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 10) };
            flowNotice = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            flowNotice.Resize += (_, _) =>
            {
                foreach (Control c in flowNotice.Controls) c.Width = flowNotice.ClientSize.Width - 8;
            };
            grpNotice.Controls.Add(flowNotice);
            noticeHost.Controls.Add(grpNotice);

            pnlBottom.Controls.Add(grpTrans);   // Fill
            pnlBottom.Controls.Add(noticeHost); // Right

            // ---------- Khối giữa: thống kê + tác vụ nhanh ----------
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 168, Padding = new Padding(0, 10, 0, 0) };

            var grpStats = new GroupBox { Text = "Thống kê hôm nay", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 10) };
            tblStats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) tblStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grpStats.Controls.Add(tblStats);

            var quickHost = new Panel { Dock = DockStyle.Right, Width = 392, Padding = new Padding(12, 0, 0, 0) };
            var grpQuick = new GroupBox { Text = "Tác vụ nhanh", Dock = DockStyle.Fill, Padding = new Padding(6, 8, 6, 6) };
            var tblQuick = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            tblQuick.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblQuick.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblQuick.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblQuick.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblQuick.Controls.Add(QuickButton("Lập phiếu mượn", GForward), 0, 0);
            tblQuick.Controls.Add(QuickButton("Nhận trả sách", GBack), 1, 0);
            tblQuick.Controls.Add(QuickButton("Thêm đầu sách", GBook), 0, 1);
            tblQuick.Controls.Add(QuickButton("Thêm độc giả", GPeople), 1, 1);
            grpQuick.Controls.Add(tblQuick);
            quickHost.Controls.Add(grpQuick);

            pnlTop.Controls.Add(grpStats);  // Fill
            pnlTop.Controls.Add(quickHost); // Right

            // ---------- Khối đầu: tiêu đề ----------
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = SoftBlue };
            pnlHeader.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, pnlHeader.Width - 1, pnlHeader.Height - 1);
            pnlHeader.Controls.Add(new Label
            {
                Text = "TỔNG QUAN HOẠT ĐỘNG THƯ VIỆN", AutoSize = true, BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(14, 8)
            });
            lblDate = new Label { AutoSize = true, BackColor = Color.Transparent, ForeColor = TextGray, Font = new Font("Segoe UI", 8.5F), Location = new Point(16, 36) };
            pnlHeader.Controls.Add(lblDate);

            var btnRefresh = new Button
            {
                Text = "  Làm mới dữ liệu", FlatStyle = FlatStyle.Flat, BackColor = Color.White, Size = new Size(150, 34),
                Image = GlyphImage(GRefresh, 14, Color.Black), ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText, Font = new Font("Segoe UI", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefresh.Location = new Point(1000, 14);
            btnRefresh.FlatAppearance.BorderColor = BorderGray;
            btnRefresh.Click += (_, _) => LoadData();
            pnlHeader.Controls.Add(btnRefresh);
            pnlHeader.Resize += (_, _) => btnRefresh.Left = pnlHeader.Width - btnRefresh.Width - 12;

            // Thứ tự add: Fill -> Top (dưới) -> Top (trên cùng)
            page.Controls.Add(pnlBottom);
            page.Controls.Add(pnlTop);
            page.Controls.Add(pnlHeader);
        }

        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 228, 233), SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, EnableHeadersVisualStyles = false, RowTemplate = { Height = 42 },
                ColumnHeadersHeight = 36, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Font = new Font("Segoe UI", 9F)
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 243, 246);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 243, 246);
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 236, 248);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;

            g.Columns.Add("colId", "Mã phiếu");
            g.Columns.Add("colReader", "Độc giả");
            g.Columns.Add("colBook", "Tên sách");
            g.Columns.Add("colBorrow", "Ngày mượn");
            g.Columns.Add("colDue", "Hạn trả");
            g.Columns.Add("colStatus", "Trạng thái");
            g.Columns["colId"]!.DefaultCellStyle.ForeColor = Primary;
            g.Columns["colId"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.Columns["colId"]!.FillWeight = 70;
            g.Columns["colReader"]!.FillWeight = 90;
            g.Columns["colBook"]!.FillWeight = 100;
            g.Columns["colBorrow"]!.FillWeight = 75;
            g.Columns["colDue"]!.FillWeight = 75;
            g.Columns["colStatus"]!.FillWeight = 85;

            g.CellPainting += DgvTrans_CellPainting;
            return g;
        }

        // Vẽ nhãn trạng thái dạng "pill"
        private void DgvTrans_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 5 || e.Graphics == null) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            string text = e.Value?.ToString() ?? "";
            Color c = text switch
            {
                "Đang mượn" => Primary,
                "Sắp đến hạn" => Orange,
                "Quá hạn" => Red,
                _ => TextGray
            };

            var font = new Font("Segoe UI", 8.5F);
            string label = "●  " + text;
            var size = TextRenderer.MeasureText(label, font);
            var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 26) / 2, size.Width + 8, 26);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(rect, 13))
            using (var fill = new SolidBrush(Color.FromArgb(22, c)))
            using (var pen = new Pen(Color.FromArgb(150, c)))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            }
            TextRenderer.DrawText(e.Graphics, label, font, rect, c,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            e.Handled = true;
        }

        // ============================================================
        //  CÁC HÀM DỰNG THẺ
        // ============================================================
        private void AddStat(int col, string glyph, string caption, string value, string sub, bool danger)
        {
            var accent = danger ? Red : Primary;
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, col == 3 ? 0 : 10, 0) };
            card.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

            card.Controls.Add(new Label
            {
                Text = glyph, Font = new Font(IconFont, 18F), ForeColor = accent,
                BackColor = danger ? Color.FromArgb(253, 234, 234) : SoftBlue,
                TextAlign = ContentAlignment.MiddleCenter, Size = new Size(50, 50), Location = new Point(10, 22)
            });
            card.Controls.Add(new Label { Text = caption, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(60, 60, 60), AutoSize = true, Location = new Point(74, 14) });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 18F, FontStyle.Bold), AutoSize = true, Location = new Point(70, 30) });
            card.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoSize = true, Location = new Point(74, 70) });
            tblStats.Controls.Add(card, col, 0);
        }

        private Button QuickButton(string text, string glyph)
        {
            var b = new Button
            {
                Text = "  " + text, Dock = DockStyle.Fill, Margin = new Padding(4), FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(246, 247, 249), Font = new Font("Segoe UI", 9F),
                Image = GlyphImage(glyph, 16, Color.Black), ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft, TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = BorderGray;
            b.Click += (_, _) => Todo(text);
            return b;
        }

        private void AddNotice(string glyph, Color color, string title, string desc, string linkText)
        {
            var row = new Panel { Height = 66, Width = Math.Max(flowNotice.ClientSize.Width - 8, 200), Margin = new Padding(4, 0, 4, 10), BackColor = Color.White };
            row.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, row.Width - 1, row.Height - 1);
            row.Controls.Add(new Label { Text = glyph, Font = new Font(IconFont, 14F), ForeColor = color, AutoSize = true, Location = new Point(12, 20) });
            row.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), AutoSize = true, Location = new Point(52, 12) });
            row.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 8F), ForeColor = TextGray, AutoSize = true, Location = new Point(54, 36) });

            var link = new LinkLabel { Text = linkText, AutoSize = true, Font = new Font("Segoe UI", 8.5F), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            link.Location = new Point(row.Width - 70, 24);
            link.LinkClicked += (_, _) => Todo(title);
            row.Controls.Add(link);
            flowNotice.Controls.Add(row);
        }

        private void AddTool(ToolStrip ts, string text, string glyph, Action onClick)
        {
            var b = new ToolStripButton(text)
            {
                Image = GlyphImage(glyph, 20, Primary),
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                TextImageRelation = TextImageRelation.ImageAboveText,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(2, 0, 2, 2)
            };
            b.Click += (_, _) => onClick();
            ts.Items.Add(b);
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

        // TODO: thay bằng mở form tương ứng trong Forms/Books, Forms/Readers...
        private void Todo(string feature) =>
            MessageBox.Show($"Chức năng \"{feature}\" chưa được nối.", "LIBRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void btnLogout_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            IsLogout = true;
            Close();
        }
    }
}