
using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.Forms.HomePage
{
    public partial class HomePage : Form
    {
        private Service _service;

        // Các Control giao diện
        private Panel pnlBanner;
        private Label lblFullName;
        private Label lblCardInfo;
        
        // Các Label thống kê
        private Label lblNum1, lblNum2, lblNum3, lblNum4;
        private Panel pnlStat1, pnlStat2, pnlStat3, pnlStat4;

        public HomePage()
        {
            InitializeComponent();
            _service = new Service();
            SetupCustomUI();
            LoadDataToUI();
        }

        private void SetupCustomUI()
        {
            this.Size = new Size(1100, 750);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Text = "Cổng thông tin đọc giả - Trang chủ";

            // 1. Banner
            pnlBanner = new Panel()
            {
                Location = new Point(20, 20),
                Size = new Size(1040, 100),
                BackColor = Color.FromArgb(15, 64, 120),
            };

            lblFullName = new Label()
            {
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };

            lblCardInfo = new Label()
            {
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(210, 225, 240),
                Location = new Point(25, 60),
                AutoSize = true
            };

            pnlBanner.Controls.Add(lblFullName);
            pnlBanner.Controls.Add(lblCardInfo);
            this.Controls.Add(pnlBanner);

            // 2. 4 Ô thống kê
            int startX = 20;
            int startY = 135;
            int cardWidth = 245;
            int cardHeight = 70;
            int spacing = 20;

            pnlStat1 = CreateStatCard(startX, startY, cardWidth, cardHeight, out lblNum1, "Sách đang mượn");
            pnlStat2 = CreateStatCard(startX + (cardWidth + spacing), startY, cardWidth, cardHeight, out lblNum2, "Đổi hạn gần nhất");
            pnlStat3 = CreateStatCard(startX + (cardWidth + spacing) * 2, startY, cardWidth, cardHeight, out lblNum3, "Sách yêu thích");
            pnlStat4 = CreateStatCard(startX + (cardWidth + spacing) * 3, startY, cardWidth, cardHeight, out lblNum4, "Sách đã đọc");

            this.Controls.Add(pnlStat1);
            this.Controls.Add(pnlStat2);
            this.Controls.Add(pnlStat3);
            this.Controls.Add(pnlStat4);
        }

        private Panel CreateStatCard(int x, int y, int width, int height, out Label lblNumRef, string title)
        {
            Panel pnl = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White
            };

            lblNumRef = new Label()
            {
                Text = "0",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 64, 120),
                Location = new Point(15, 10),
                AutoSize = true
            };

            Label lblTitle = new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(15, 38),
                AutoSize = true
            };

            pnl.Controls.Add(lblNumRef);
            pnl.Controls.Add(lblTitle);
            return pnl;
        }

        private void LoadDataToUI()
        {
            // Lấy thông tin độc giả (Mã giả lập: "DG001")
            Readers reader = _service.GetCurrentReader("DG001");
            if (reader != null)
            {
                lblFullName.Text = reader.FullName;
                lblCardInfo.Text = $"Mã đọc giả: {reader.ReaderId} - Thẻ còn hạn đến: {reader.CardExpiryDate:dd/MM/yyyy}";
            }

            // Lấy thông tin thống kê
            HomeSummary summary = _service.GetHomeSummary("DG001");
            if (summary != null)
            {
                lblNum1.Text = summary.BorrowingBooksCount.ToString("D2");
                lblNum2.Text = $"{summary.DaysToRenewal:D2} ngày";
                lblNum3.Text = summary.FavoriteBooksCount.ToString("D2");
                lblNum4.Text = summary.ReadBooksCount.ToString("D2");
            }
        }
    }
}