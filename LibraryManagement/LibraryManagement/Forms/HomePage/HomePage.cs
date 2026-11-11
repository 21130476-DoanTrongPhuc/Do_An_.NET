using System.Drawing.Drawing2D;

namespace LibraryManagement.Forms
{
    public partial class HomePage : Form
    {
        // ====== Màu sắc theo thiết kế Figma ======
        private static readonly Color Primary = Color.FromArgb(19, 98, 160);
        private static readonly Color BorderGray = Color.FromArgb(217, 222, 229);
        private static readonly Color TextGray = Color.FromArgb(110, 117, 128);
        private static readonly Color Orange = Color.FromArgb(230, 145, 20);
        private static readonly Color Green = Color.FromArgb(34, 160, 90);
        private static readonly Color Red = Color.FromArgb(200, 50, 50);
        private const string IconFont = "Segoe MDL2 Assets"; // có sẵn trên Windows 10/11
        public bool IsLogout { get; private set; }

        // Glyph (Segoe MDL2 Assets)
        private const string GHome = "\uE80F",
            GSearch = "\uE721",
            GBook = "\uE82D",
            GHeart = "\uEB51",
            GHeartFill = "\uEB52",
            GClock = "\uE121",
            GUser = "\uE77B",
            GBell = "\uEA8F",
            GCheck = "\uE73E",
            GInfo = "\uE946",
            GHistory = "\uE81C";

        public HomePage()
        {
            InitializeComponent();
            BuildMenu();
            BuildToolbar();
            StyleStatic();
            BuildStatusBar();
            LoadData();

            // responsive: chia đều thẻ khi đổi kích thước
            flowBorrow.Resize += (_, _) => LayoutCards(flowBorrow, 2, flowBorrow.Height - 8);
            flowSuggest.Resize += (_, _) => LayoutCards(flowSuggest, 5, 82);
            flowNotice.Resize += (_, _) =>
            {
                foreach (Control c in flowNotice.Controls) c.Width = flowNotice.ClientSize.Width - 8;
            };
        }

        // ============================================================
        //  DỮ LIỆU - thay phần này bằng BookService / Repository của bạn
        // ============================================================
        private void LoadData()
        {
            // TODO: var summary = _service.GetHomeSummary(readerId);  (Models/HomeSummary.cs)
            string readerName = "Nguyễn Minh Anh", readerCode = "DG001", expire = "12/08/2025";

            lblName.Text = readerName;
            lblCode.Text = $"Mã độc giả: {readerCode} · Thẻ còn hạn đến {expire}";
            lblUserName.Text = readerName;
            lblUserCode.Text = $"Độc giả · {readerCode}";
            lblStatusReader.Text = $"Độc giả: {readerCode}";

            // Thẻ thống kê
            tblStats.Controls.Clear();
            AddStat(0, GBook, "02", "Sách đang mượn");
            AddStat(1, GClock, "04 ngày", "Đến hạn gần nhất");
            AddStat(2, GHeart, "06", "Sách yêu thích");
            AddStat(3, GCheck, "24", "Sách đã đọc");

            // Sách đang mượn (Models/BorrowRecord.cs)
            flowBorrow.Controls.Clear();
            var borrowed = new[]
            {
                (Title: "Nhà giả kim", Author: "Paulo Coelho", Code: "BK001", Due: "29/03/2024", Days: 4,
                    Color: Primary),
                (Title: "Tư duy nhanh và chậm", Author: "Daniel Kahneman", Code: "BK006", Due: "02/04/2024", Days: 8,
                    Color: Color.FromArgb(110, 80, 150)),
            };
            foreach (var b in borrowed)
            {
                bool soon = b.Days <= 5;
                flowBorrow.Controls.Add(CreateBookCard(b.Title, $"{b.Author} · {b.Code}", $"Hạn trả: {b.Due}",
                    b.Color, $"Còn {b.Days} ngày", soon ? Orange : Green, null));
            }

            LayoutCards(flowBorrow, 2, flowBorrow.Height - 8);

            // Thông báo (Models/Notification.cs)
            flowNotice.Controls.Clear();
            AddNotice(GBell, Primary, "Nhắc hạn trả sách", "\"Nhà giả kim\" sẽ đến hạn sau 4 ngày.");
            AddNotice(GCheck, Green, "Sách đặt trước đã sẵn sàng", "Vui lòng nhận trước ngày 28/03.");
            AddNotice(GInfo, Primary, "Cập nhật giờ mở cửa", "Thứ Hai - Thứ Bảy: 07:30 - 20:00.");

            // Gợi ý (Models/Book.cs)
            flowSuggest.Controls.Clear();
            var suggest = new[]
            {
                (T: "Đắc nhân tâm", A: "Dale Carnegie", Ok: true, C: Primary),
                (T: "Tuổi trẻ đáng giá bao nhiêu", A: "Rosie Nguyễn", Ok: true, C: Color.FromArgb(110, 80, 150)),
                (T: "Lược sử thời gian", A: "Stephen Hawking", Ok: false, C: Color.FromArgb(40, 140, 110)),
                (T: "Mắt biếc", A: "Nguyễn Nhật Ánh", Ok: true, C: Color.FromArgb(165, 90, 60)),
                (T: "Tư duy nhanh và chậm", A: "Daniel Kahneman", Ok: true, C: Primary),
            };
            foreach (var s in suggest)
                flowSuggest.Controls.Add(CreateBookCard(s.T, s.A, null, s.C,
                    s.Ok ? "Còn sách" : "Đã hết", s.Ok ? Green : Red, s.T));
            LayoutCards(flowSuggest, 5, 82);
        }

        // ============================================================
        //  MENU + TOOLBAR
        // ============================================================
        private void BuildMenu()
        {
            menuStrip.Items.Clear();
            foreach (var t in new[] { "Hệ thống", "Tra cứu", "Mượn sách", "Tài khoản", "Trợ giúp" })
                menuStrip.Items.Add(new ToolStripMenuItem(t));
        }

        private void BuildToolbar()
        {
            toolStrip.Items.Clear();
            toolStrip.Padding = new Padding(8, 2, 0, 0);
            AddTool("Trang chủ", GHome, 0, true, null);
            AddTool("Tra cứu sách", GSearch, 0, false, null);
            AddTool("Sách đang mượn", GBook, 2, false, null);
            AddTool("Sách yêu thích", GHeart, 0, false, null);
            AddTool("Lịch sử mượn", GHistory, 0, false, null);
            AddTool("Thông tin tài khoản", GUser, 0, false, null);
        }

        private void AddTool(string text, string glyph, int badge, bool active, EventHandler? onClick)
        {
            var btn = new ToolStripButton(text)
            {
                Image = GlyphImage(glyph, 20, Primary, badge),
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                TextImageRelation = TextImageRelation.ImageAboveText,
                Font = new Font("Segoe UI", 7.5F),
                Margin = new Padding(2, 2, 2, 2),
                Padding = new Padding(6, 0, 6, 0),
                BackColor = active ? Color.FromArgb(220, 235, 248) : Color.White,
            };
            if (onClick != null) btn.Click += onClick;
            toolStrip.Items.Add(btn);
        }

        private void StyleStatic()
        {
            // avatar tròn "MA"
            picAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new SolidBrush(Primary);
                e.Graphics.FillEllipse(br, 0, 0, picAvatar.Width - 1, picAvatar.Height - 1);
                TextRenderer.DrawText(e.Graphics, Initials(lblUserName.Text),
                    new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    new Rectangle(0, 0, picAvatar.Width, picAvatar.Height), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            lblBreadcrumb.Paint += (s, e) => { };
            // icon kính lúp trong ô tìm kiếm
            pnlSearch.Paint += (s, e) =>
                TextRenderer.DrawText(e.Graphics, GSearch, new Font(IconFont, 12F),
                    new Point(10, 12), TextGray);
            lnkAllBorrow.LinkClicked += (_, _) =>
            {
                /* TODO: mở danh sách sách đang mượn */
            };
        }

        // ============================================================
        //  CÁC HÀM DỰNG THẺ
        // ============================================================
        private void AddStat(int col, string glyph, string value, string caption)
        {
            var card = new Panel
                { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, col == 3 ? 0 : 12, 0) };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

            card.Controls.Add(new Label
            {
                Text = glyph, Font = new Font(IconFont, 16F), ForeColor = Primary, AutoSize = true,
                Location = new Point(14, 20)
            });
            card.Controls.Add(new Label
            {
                Text = value, Font = new Font("Segoe UI", 15F, FontStyle.Bold), AutoSize = true,
                Location = new Point(48, 8)
            });
            card.Controls.Add(new Label
            {
                Text = caption, Font = new Font("Segoe UI", 8F), ForeColor = TextGray, AutoSize = true,
                Location = new Point(50, 40)
            });
            tblStats.Controls.Add(card, col, 0);
        }

        private Panel CreateBookCard(string title, string subtitle, string? extra, Color cover,
            string status, Color statusColor, string? favoriteKey)
        {
            var card = new Panel
                { Height = 74, Width = 300, BackColor = Color.White, Margin = new Padding(0, 0, 8, 8) };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(BorderGray), 0, 0, card.Width - 1, card.Height - 1);

            // bìa sách
            var coverPnl = new Label
            {
                Text = GBook, Font = new Font(IconFont, 18F), ForeColor = Color.White, BackColor = cover,
                TextAlign = ContentAlignment.MiddleCenter, Size = new Size(44, 56), Location = new Point(8, 9)
            };
            card.Controls.Add(coverPnl);

            card.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true,
                Location = new Point(60, 8)
            });
            card.Controls.Add(new Label
            {
                Text = subtitle, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoSize = true,
                Location = new Point(60, 27)
            });
            if (extra != null)
                card.Controls.Add(new Label
                {
                    Text = extra, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoSize = true,
                    Location = new Point(60, 44)
                });

            // nhãn trạng thái dạng "pill"
            var pill = new Label
            {
                Text = "● " + status, Font = new Font("Segoe UI", 7.5F), ForeColor = statusColor,
                AutoSize = true, Padding = new Padding(4, 1, 4, 1), BackColor = Color.FromArgb(25, statusColor)
            };
            if (favoriteKey == null) // thẻ "đang mượn": pill bên phải
            {
                pill.Location = new Point(card.Width - 100, 24);
                pill.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else // thẻ gợi ý: pill dưới + nút tim bên phải
            {
                pill.Location = new Point(60, 50);
                var heart = new Button
                {
                    Text = GHeart, Font = new Font(IconFont, 11F), FlatStyle = FlatStyle.Flat, Size = new Size(34, 30),
                    Location = new Point(card.Width - 42, 8), Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Tag = false, Cursor = Cursors.Hand
                };
                heart.FlatAppearance.BorderColor = BorderGray;
                heart.Click += (_, _) =>
                {
                    bool fav = !(bool)heart.Tag;
                    heart.Tag = fav;
                    heart.Text = fav ? GHeartFill : GHeart;
                    heart.ForeColor = fav ? Red : Color.Black;
                    // TODO: lưu sách yêu thích (Models/FavoriteBook.cs)
                };
                card.Controls.Add(heart);
            }

            card.Controls.Add(pill);
            pill.BringToFront();
            return card;
        }

        private void AddNotice(string glyph, Color color, string title, string desc)
        {
            var row = new Panel
                { Height = 50, Width = flowNotice.ClientSize.Width - 8, Margin = new Padding(4, 0, 4, 0) };
            row.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(BorderGray), 0, row.Height - 1, row.Width, row.Height - 1);
            row.Controls.Add(new Label
            {
                Text = glyph, Font = new Font(IconFont, 13F), ForeColor = color, AutoSize = true,
                Location = new Point(6, 14)
            });
            row.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true,
                Location = new Point(40, 6)
            });
            row.Controls.Add(new Label
            {
                Text = desc, Font = new Font("Segoe UI", 7.5F), ForeColor = TextGray, AutoSize = true,
                Location = new Point(40, 26)
            });
            flowNotice.Controls.Add(row);
        }

        private static void LayoutCards(FlowLayoutPanel flow, int perRow, int height)
        {
            if (flow.Controls.Count == 0 || flow.ClientSize.Width <= 0) return;
            int w = (flow.ClientSize.Width - flow.Padding.Left - 8 * perRow) / perRow;
            foreach (Control c in flow.Controls)
            {
                c.Width = Math.Max(w, 120);
                c.Height = Math.Min(height, 74);
            }
        }

        // ============================================================
        //  TIỆN ÍCH
        // ============================================================
        private static Bitmap GlyphImage(string glyph, int size, Color color, int badge)
        {
            var bmp = new Bitmap(size + 14, size + 6);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var f = new Font(IconFont, size, GraphicsUnit.Pixel);
            TextRenderer.DrawText(g, glyph, f, new Rectangle(0, 0, size + 4, bmp.Height), color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            if (badge > 0)
            {
                using var br = new SolidBrush(Red);
                g.FillEllipse(br, bmp.Width - 15, 0, 15, 15);
                TextRenderer.DrawText(g, badge.ToString(), new Font("Segoe UI", 7F, FontStyle.Bold),
                    new Rectangle(bmp.Width - 15, 0, 15, 15), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            return bmp;
        }

        private static string Initials(string name)
        {
            var p = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return p.Length == 0 ? "" : (p[0][0].ToString() + p[^1][0]).ToUpper();
        }

        // ============================================================
        //  SỰ KIỆN
        // ============================================================
        private void btnSearch_Click(object? sender, EventArgs e)
        {
            // TODO: mở form tra cứu sách (Forms/Books) với từ khóa txtSearch.Text
            MessageBox.Show($"Tìm: {txtSearch.Text}", "Tra cứu");
        }

        private void txtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnSearch_Click(sender, e);
            }
        }

        private void btnLogout_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            IsLogout = true;
            Close();
        }
        private ToolStripStatusLabel lblStatusReader = null!;

        private void BuildStatusBar()
        {
            var status = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                SizingGrip = true,
                BackColor = Color.FromArgb(240, 242, 245),
                Font = new Font("Segoe UI", 8F)
            };

            // Bên trái: trạng thái kết nối
            var dot = new ToolStripStatusLabel("●") { ForeColor = Green, Margin = new Padding(4, 3, 0, 2) };
            var connected = new ToolStripStatusLabel("Đã kết nối") { Margin = new Padding(0, 3, 6, 2) };
            var hours = new ToolStripStatusLabel("Thư viện Trung tâm · Giờ mở cửa: 07:30 - 20:00");

            // Đẩy phần còn lại sang phải
            var spring = new ToolStripStatusLabel { Spring = true };

            // Bên phải: độc giả và phiên bản
            lblStatusReader = new ToolStripStatusLabel("Độc giả: DG001");
            var version = new ToolStripStatusLabel("Phiên bản 2.4.0");

            status.Items.AddRange(new ToolStripItem[]
            {
                dot, connected, new ToolStripSeparator(), hours,
                spring,
                new ToolStripSeparator(), lblStatusReader,
                new ToolStripSeparator(), version
            });

            Controls.Add(status);
        }
    }
}