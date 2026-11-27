using System.Drawing.Printing;
using LibraryManagement.DTO.Response;

namespace LibraryManagement.Forms.OverduePrintPreviewForm
{
    public partial class OverduePrintPreviewForm : Form
    {
        private readonly List<ManageOverdueBookInfo> _rows;
        private readonly decimal _penaltyPerDay;
        private PrintDocument _doc = null!;
        private PrintPreviewControl _preview = null!;
        private int _printRow;

        public OverduePrintPreviewForm(IEnumerable<ManageOverdueBookInfo> rows, decimal penaltyPerDay = 5000m)
        {
            Text = "Xem trước bản in - Danh sách sách quá hạn";
            Size = new Size(1050, 780);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 9F);

            _rows = rows.ToList();
            _penaltyPerDay = penaltyPerDay;
            BuildUi();
        }

        private void BuildUi()
        {
            _doc = new PrintDocument { DocumentName = "Danh sách sách quá hạn" };
            _doc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            _doc.DefaultPageSettings.Margins = new Margins(45, 45, 50, 50);

            // Reset lại dòng bắt đầu in mỗi khi render preview hoặc in thật
            _doc.BeginPrint += (_, _) => _printRow = 0;
            _doc.PrintPage += Doc_PrintPage;

            _preview = new PrintPreviewControl
            {
                Dock = DockStyle.Fill,
                Document = _doc,
                Zoom = 0.95,
                BackColor = Color.FromArgb(220, 224, 230)
            };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White,
                Padding = new Padding(12, 9, 12, 9)
            };
            toolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(205, 210, 218));
                e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            var btnPrint = new Button
            {
                Text = "  In danh sách",
                Size = new Size(125, 34),
                Location = new Point(12, 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(19, 98, 160),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (_, _) =>
            {
                using var dlg = new PrintDialog { Document = _doc, UseEXDialog = true };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _doc.Print();
                }
            };

            var cboZoom = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(148, 11),
                Width = 120,
                Font = new Font("Segoe UI", 9F)
            };
            cboZoom.Items.AddRange(new object[] { "Thu phóng 75%", "Thu phóng 95%", "Thu phóng 120%", "Vừa trang" });
            cboZoom.SelectedIndex = 1;
            cboZoom.SelectedIndexChanged += (_, _) =>
            {
                switch (cboZoom.SelectedIndex)
                {
                    case 0: _preview.AutoZoom = false; _preview.Zoom = 0.75; break;
                    case 1: _preview.AutoZoom = false; _preview.Zoom = 0.95; break;
                    case 2: _preview.AutoZoom = false; _preview.Zoom = 1.20; break;
                    case 3: _preview.AutoZoom = true; break;
                }
            };

            var lblCount = new Label
            {
                Text = $"Tổng cộng: {_rows.Count} phiếu quá hạn",
                AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128),
                Location = new Point(285, 17)
            };

            var btnClose = new Button
            {
                Text = "Đóng",
                Size = new Size(95, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnClose.Click += (_, _) => Close();

            toolbar.Controls.AddRange(new Control[] { btnPrint, cboZoom, lblCount, btnClose });
            toolbar.Resize += (_, _) => btnClose.Location = new Point(toolbar.Width - btnClose.Width - 12, 9);
            btnClose.Location = new Point(toolbar.Width - btnClose.Width - 12, 9);

            Controls.Add(_preview);
            Controls.Add(toolbar);
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics!;
            using var fontOrg    = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var fontTitle  = new Font("Segoe UI", 15F, FontStyle.Bold);
            using var fontSub    = new Font("Segoe UI", 9.5F, FontStyle.Italic);
            using var fontBody   = new Font("Segoe UI", 8.5F);
            using var fontHeader = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            using var fontSmall  = new Font("Segoe UI", 8F);

            int x = e.MarginBounds.Left;
            int y = e.MarginBounds.Top;
            int width = e.MarginBounds.Width;

            // Tổng độ rộng các cột = 737 (vừa khít khổ A4 trừ lề 45*2 = 737)
            int[] colW = { 34, 72, 65, 130, 72, 144, 72, 62, 86 };
            string[] heads = { "STT", "Mã phiếu", "Mã ĐG", "Họ tên độc giả", "Mã sách", "Tên sách", "Hạn trả", "Quá hạn", "Tiền phạt" };

            if (_printRow == 0)
            {
                TextRenderer.DrawText(g, "THƯ VIỆN TRUNG TÂM LIBRA", fontOrg,
                    new Rectangle(x, y, width, 22), Color.FromArgb(19, 98, 160), TextFormatFlags.Left);
                y += 26;

                TextRenderer.DrawText(g, "DANH SÁCH ĐỘC GIẢ QUÁ HẠN CẦN THU HỒI SÁCH", fontTitle,
                    new Rectangle(x, y, width, 30), Color.Black, TextFormatFlags.HorizontalCenter);
                y += 32;

                TextRenderer.DrawText(g, $"Ngày lập báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSub,
                    new Rectangle(x, y, width, 20), Color.FromArgb(90, 95, 105), TextFormatFlags.HorizontalCenter);
                y += 28;
            }

            // Vẽ tiêu đề bảng ở đầu mỗi trang
            DrawTableHeader(g, fontHeader, x, y, colW, heads);
            y += 26;

            int rowH = 26;
            int stt = _printRow + 1;

            while (_printRow < _rows.Count)
            {
                if (y + rowH > e.MarginBounds.Bottom - 90)
                {
                    e.HasMorePages = true;
                    return;
                }

                var r = _rows[_printRow];
                string[] cells =
                {
                    stt.ToString(),
                    r.LoanCode,
                    r.UserCode,
                    r.FullName,
                    r.CopyCode,
                    r.Title,
                    r.DueDay.ToString("dd/MM/yyyy"),
                    $"{r.OverdueDays} ngày",
                    $"{r.FinePay:N0} đ"
                };

                DrawTableRow(g, fontBody, x, y, colW, cells, _printRow % 2 == 1);

                y += rowH;
                _printRow++;
                stt++;
            }

            // Dòng tổng kết & Chữ ký thủ thư ở trang cuối
            y += 12;
            double totalFine = _rows.Sum(r => r.FinePay);
            TextRenderer.DrawText(g, $"Tổng số sách quá hạn: {_rows.Count} cuốn   |   Tổng tiền phạt tạm tính: {totalFine:N0} đ",
                fontHeader, new Rectangle(x, y, width, 22), Color.Black, TextFormatFlags.Right);

            y += 30;
            if (y < e.MarginBounds.Bottom - 85)
            {
                int sigX = x + width - 220;
                TextRenderer.DrawText(g, "Người lập danh sách", fontHeader,
                    new Rectangle(sigX, y, 210, 20), Color.Black, TextFormatFlags.HorizontalCenter);
                TextRenderer.DrawText(g, "(Ký và ghi rõ họ tên)", fontSmall,
                    new Rectangle(sigX, y + 20, 210, 18), Color.FromArgb(110, 117, 128), TextFormatFlags.HorizontalCenter);
                y += 65;
                TextRenderer.DrawText(g, "................................................", fontBody,
                    new Rectangle(sigX, y, 210, 20), Color.Black, TextFormatFlags.HorizontalCenter);
            }

            e.HasMorePages = false;
        }

        private static void DrawTableHeader(Graphics g, Font font, int x, int y, int[] widths, string[] headers)
        {
            using var bg = new SolidBrush(Color.FromArgb(232, 240, 250));
            using var pen = new Pen(Color.FromArgb(180, 185, 190));
            int total = widths.Sum();
            g.FillRectangle(bg, x, y, total, 26);
            g.DrawRectangle(pen, x, y, total, 26);

            int cx = x;
            for (int i = 0; i < widths.Length; i++)
            {
                TextRenderer.DrawText(g, headers[i], font,
                    new Rectangle(cx + 2, y + 3, widths[i] - 4, 20), Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                g.DrawLine(pen, cx + widths[i], y, cx + widths[i], y + 26);
                cx += widths[i];
            }
        }

        private static void DrawTableRow(Graphics g, Font font, int x, int y, int[] widths, string[] cells, bool alt)
        {
            using var pen = new Pen(Color.FromArgb(215, 218, 224));
            int total = widths.Sum();

            if (alt)
            {
                using var bg = new SolidBrush(Color.FromArgb(248, 249, 251));
                g.FillRectangle(bg, x, y, total, 26);
            }
            g.DrawRectangle(pen, x, y, total, 26);

            int cx = x;
            for (int i = 0; i < widths.Length; i++)
            {
                var align = (i == 0 || i == 6 || i == 7)
                    ? TextFormatFlags.HorizontalCenter
                    : (i == 8 ? TextFormatFlags.Right : TextFormatFlags.Left);

                TextRenderer.DrawText(g, cells[i], font,
                    new Rectangle(cx + 4, y + 4, widths[i] - 8, 18), Color.Black,
                    align | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                g.DrawLine(pen, cx + widths[i], y, cx + widths[i], y + 26);
                cx += widths[i];
            }
        }
    }
}