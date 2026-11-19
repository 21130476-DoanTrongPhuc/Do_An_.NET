using System.Drawing.Printing;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.LoanManagement;

public class LoanPrintPreviewForm : Form
{
    private readonly LoanReceiptDto _receipt;
    private PrintDocument _doc = null!;

    public LoanPrintPreviewForm(LoanReceiptDto receipt)
    {
        _receipt = receipt;
        Text = $"Thông tin & Bản in Phiếu mượn - {_receipt.LoanCode}";
        Size = new Size(920, 740);
        StartPosition = FormStartPosition.CenterParent;
        BuildUi();
    }

    private void BuildUi()
    {
        _doc = new PrintDocument { DocumentName = $"PhieuMuon_{_receipt.LoanCode}" };
        _doc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
        _doc.DefaultPageSettings.Margins = new Margins(55, 55, 55, 55);
        _doc.PrintPage += Doc_PrintPage;

        var preview = new PrintPreviewControl
        {
            Dock = DockStyle.Fill,
            Document = _doc,
            Zoom = 0.95
        };

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };
        var btnPrint = new Button
        {
            Text = "In phiếu mượn",
            Size = new Size(130, 32),
            Location = new Point(12, 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(19, 98, 160),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += (_, _) =>
        {
            using var dlg = new PrintDialog { Document = _doc };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                _doc.Print();
        };

        var btnClose = new Button
        {
            Text = "Đóng",
            Size = new Size(90, 32),
            Location = new Point(152, 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
        btnClose.Click += (_, _) => Close();

        toolbar.Controls.AddRange([btnPrint, btnClose]);
        Controls.Add(preview);
        Controls.Add(toolbar);
    }

    private void Doc_PrintPage(object sender, PrintPageEventArgs e)
    {
        var g = e.Graphics!;
        using var fontHeader = new Font("Segoe UI", 11F, FontStyle.Bold);
        using var fontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
        using var fontBold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        using var fontBody = new Font("Segoe UI", 9.5F);
        using var fontSmall = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        using var pen = new Pen(Color.FromArgb(180, 185, 190));

        int x = e.MarginBounds.Left;
        int y = e.MarginBounds.Top;
        int w = e.MarginBounds.Width;

        TextRenderer.DrawText(g, "THƯ VIỆN TRUNG TÂM LIBRA", fontHeader,
            new Rectangle(x, y, w, 24), Color.FromArgb(19, 98, 160), TextFormatFlags.Left);
        y += 28;

        TextRenderer.DrawText(g, "PHIẾU MƯỢN SÁCH", fontTitle,
            new Rectangle(x, y, w, 34), Color.Black, TextFormatFlags.HorizontalCenter);
        y += 34;

        TextRenderer.DrawText(g, $"Mã phiếu: {_receipt.LoanCode}   |   Ngày mượn: {_receipt.BorrowedAt:dd/MM/yyyy HH:mm}",
            fontBold, new Rectangle(x, y, w, 22), Color.FromArgb(60, 60, 60), TextFormatFlags.HorizontalCenter);
        y += 32;

        g.DrawLine(pen, x, y, x + w, y);
        y += 12;

        TextRenderer.DrawText(g, $"Độc giả: {_receipt.ReaderName} ({_receipt.ReaderCode})", fontBody,
            new Rectangle(x, y, w / 2, 22), Color.Black, TextFormatFlags.Left);
        TextRenderer.DrawText(g, $"Mã thẻ thư viện: {_receipt.CardCode}", fontBody,
            new Rectangle(x + w / 2, y, w / 2, 22), Color.Black, TextFormatFlags.Left);
        y += 24;

        TextRenderer.DrawText(g, $"Số điện thoại: {_receipt.ReaderPhone}", fontBody,
            new Rectangle(x, y, w / 2, 22), Color.Black, TextFormatFlags.Left);
        TextRenderer.DrawText(g, $"Thủ thư lập phiếu: {_receipt.LibrarianName}", fontBody,
            new Rectangle(x + w / 2, y, w / 2, 22), Color.Black, TextFormatFlags.Left);
        y += 28;

        int[] colW = [36, 95, 265, 110, 95, 115];
        string[] heads = ["STT", "Mã bản sách", "Tên đầu sách", "Tình trạng", "Hạn trả", "Trạng thái"];

        using (var bg = new SolidBrush(Color.FromArgb(240, 242, 245)))
        {
            g.FillRectangle(bg, x, y, colW.Sum(), 28);
            g.DrawRectangle(pen, x, y, colW.Sum(), 28);
        }

        int cx = x;
        for (int i = 0; i < colW.Length; i++)
        {
            TextRenderer.DrawText(g, heads[i], fontBold, new Rectangle(cx + 4, y + 4, colW[i] - 6, 20),
                Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            g.DrawLine(pen, cx + colW[i], y, cx + colW[i], y + 28);
            cx += colW[i];
        }
        y += 28;

        for (int idx = 0; idx < _receipt.Items.Count; idx++)
        {
            var item = _receipt.Items[idx];
            string[] rowCells =
            [
                (idx + 1).ToString(),
                item.CopyCode,
                item.Title,
                item.PhysicalConditionBefore,
                item.DueAt.ToString("dd/MM/yyyy"),
                item.ItemStatus
            ];

            g.DrawRectangle(pen, x, y, colW.Sum(), 26);
            cx = x;
            for (int i = 0; i < colW.Length; i++)
            {
                TextRenderer.DrawText(g, rowCells[i], fontBody, new Rectangle(cx + 4, y + 3, colW[i] - 6, 20),
                    Color.Black, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                g.DrawLine(pen, cx + colW[i], y, cx + colW[i], y + 26);
                cx += colW[i];
            }
            y += 26;
        }

        y += 18;
        TextRenderer.DrawText(g, $"Tổng số sách mượn: {_receipt.Items.Count} cuốn.", fontBold,
            new Rectangle(x, y, w, 22), Color.Black, TextFormatFlags.Left);

        y += 45;
        int half = w / 2;
        TextRenderer.DrawText(g, "Độc giả ký nhận", fontBold, new Rectangle(x, y, half, 22), Color.Black, TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g, "Thủ thư lập phiếu", fontBold, new Rectangle(x + half, y, half, 22), Color.Black, TextFormatFlags.HorizontalCenter);
        y += 20;
        TextRenderer.DrawText(g, "(Ký và ghi rõ họ tên)", fontSmall, new Rectangle(x, y, half, 20), Color.Gray, TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g, "(Ký và ghi rõ họ tên)", fontSmall, new Rectangle(x + half, y, half, 20), Color.Gray, TextFormatFlags.HorizontalCenter);
    }
}