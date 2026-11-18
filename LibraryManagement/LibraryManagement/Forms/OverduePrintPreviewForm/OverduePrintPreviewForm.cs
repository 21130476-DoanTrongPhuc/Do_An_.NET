using System.Drawing.Printing;

namespace LibraryManagement.Forms.OverduePrintPreviewForm
{
    public partial class OverduePrintPreviewForm : Form
    {
        private readonly List<object> _rows;
        private readonly decimal _penaltyPerDay;
        private PrintDocument _doc = null!;
        private int _printRow;

        // Reflection-friendly: nhận List<OverdueRow> từ ngoài (dùng object để tránh phụ thuộc kiểu nội bộ)
        public OverduePrintPreviewForm(System.Collections.IEnumerable rows, decimal penaltyPerDay)
        {
            InitializeComponent();
            Text = "Xem trước bản in - Danh sách đòi sách";
            Size = new Size(1000, 760);
            StartPosition = FormStartPosition.CenterParent;

            _rows = rows.Cast<object>().ToList();
            _penaltyPerDay = penaltyPerDay;
            BuildUi();
        }

        private void BuildUi()
        {
            _doc = new PrintDocument { DocumentName = "Danh sách đòi sách" };
            _doc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            _doc.DefaultPageSettings.Margins = new Margins(60, 60, 60, 60);
            _doc.PrintPage += Doc_PrintPage;

            var preview = new PrintPreviewControl
            {
                Dock = DockStyle.Fill,
                Document = _doc,
                Zoom = 0.9
            };

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };
            var btnPrint = new Button
            {
                Text = "  In",
                Size = new Size(100, 32),
                Location = new Point(8, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(19, 98, 160),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (_, _) =>
            {
                using var dlg = new PrintDialog { Document = _doc };
                if (dlg.ShowDialog(this) == DialogResult.OK) _doc.Print();
            };
            var btnClose = new Button
            {
                Text = "Đóng",
                Size = new Size(100, 32),
                Location = new Point(116, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnClose.Click += (_, _) => Close();

            toolbar.Controls.Add(btnPrint);
            toolbar.Controls.Add(btnClose);

            Controls.Add(preview);
            Controls.Add(toolbar);
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics!;
            var fontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
            var fontSub = new Font("Segoe UI", 10F, FontStyle.Italic);
            var fontBody = new Font("Segoe UI", 9F);
            var fontHeader = new Font("Segoe UI", 9F, FontStyle.Bold);
            var fontSmall = new Font("Segoe UI", 8F);

            int x = e.MarginBounds.Left;
            int y = e.MarginBounds.Top;
            int width = e.MarginBounds.Width;

            if (_printRow == 0)
            {
                // Tiêu đề
                TextRenderer.DrawText(g, "DANH SÁCH ĐỘC GIẢ CẦN THU HỒI SÁCH", fontTitle,
                    new Rectangle(x, y, width, 32), Color.Black, TextFormatFlags.HorizontalCenter);

                y += 34;
                TextRenderer.DrawText(g, $"Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSub,
                    new Rectangle(x, y, width, 22), Color.FromArgb(80, 80, 80), TextFormatFlags.HorizontalCenter);
                y += 30;

                // Header bảng
                int[] colW = { 26, 78, 62, 130, 86, 92, 70, 60 };
                string[] heads = { "STT", "Mã phiếu", "Mã ĐG", "Họ tên độc giả", "Mã vạch", "Tên sách", "Hạn trả", "Quá hạn" };
                DrawTableHeader(g, fontHeader, x, y, colW, heads);
                y += 26;
            }

            int[] cw = { 26, 78, 62, 130, 86, 92, 70, 60 };
            int rowH = 24;
            int stt = _printRow + 1;

            while (_printRow < _rows.Count)
            {
                if (y + rowH > e.MarginBounds.Bottom - 80)
                {
                    e.HasMorePages = true;
                    return;
                }

                var r = _rows[_printRow];
                string ticket = Get<string>(r, "TicketId");
                string readerId = Get<string>(r, "ReaderId");
                string name = Get<string>(r, "ReaderName");
                string copy = Get<string>(r, "CopyId");
                string title = Get<string>(r, "Title");
                DateTime due = Get<DateTime>(r, "DueDate");
                int days = Get<int>(r, "DaysOverdue");

                string[] cells = { stt.ToString(), ticket, readerId, name, copy, title,
                                   due.ToString("dd/MM/yyyy"), days + " ngày" };
                DrawTableRow(g, fontBody, x, y, cw, cells, _printRow % 2 == 0);

                y += rowH;
                _printRow++;
                stt++;
            }

            // Chữ ký thủ thư
            y += 30;
            if (y < e.MarginBounds.Bottom - 90)
            {
                int sigX = x + width - 240;
                TextRenderer.DrawText(g, "Người lập danh sách", fontHeader,
                    new Rectangle(sigX, y, 200, 20), Color.Black, TextFormatFlags.HorizontalCenter);
                TextRenderer.DrawText(g, "(Ký, ghi rõ họ tên)", fontSmall,
                    new Rectangle(sigX, y + 20, 200, 18), Color.FromArgb(110, 117, 128), TextFormatFlags.HorizontalCenter);
                y += 60;
                TextRenderer.DrawText(g, "..............................................", fontBody,
                    new Rectangle(sigX, y, 200, 20), Color.Black, TextFormatFlags.HorizontalCenter);
            }
        }

        private static void DrawTableHeader(Graphics g, Font font, int x, int y, int[] widths, string[] headers)
        {
            using var bg = new SolidBrush(Color.FromArgb(240, 242, 245));
            using var pen = new Pen(Color.FromArgb(180, 185, 190));
            int total = widths.Sum();
            g.FillRectangle(bg, x, y, total, 24);
            g.DrawRectangle(pen, x, y, total, 24);

            int cx = x;
            for (int i = 0; i < widths.Length; i++)
            {
                TextRenderer.DrawText(g, headers[i], font,
                    new Rectangle(cx + 3, y + 3, widths[i] - 4, 20), Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                g.DrawLine(pen, cx + widths[i], y, cx + widths[i], y + 24);
                cx += widths[i];
            }
        }

        private static void DrawTableRow(Graphics g, Font font, int x, int y, int[] widths, string[] cells, bool alt)
        {
            using var pen = new Pen(Color.FromArgb(220, 222, 226));
            int total = widths.Sum();

            if (alt)
            {
                using var bg = new SolidBrush(Color.FromArgb(248, 249, 250));
                g.FillRectangle(bg, x, y, total, 24);
            }
            g.DrawRectangle(pen, x, y, total, 24);

            int cx = x;
            for (int i = 0; i < widths.Length; i++)
            {
                TextRenderer.DrawText(g, cells[i], font,
                    new Rectangle(cx + 3, y + 4, widths[i] - 4, 18), Color.Black,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                g.DrawLine(pen, cx + widths[i], y, cx + widths[i], y + 24);
                cx += widths[i];
            }
        }

        private static T Get<T>(object obj, string prop)
        {
            var p = obj.GetType().GetProperty(prop);
            if (p == null) return default!;
            var v = p.GetValue(obj);
            return v is T t ? t : default!;
        }
    }
}