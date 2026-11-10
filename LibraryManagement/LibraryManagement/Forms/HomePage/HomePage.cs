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

        // Các khối chính
        private Panel pnlBanner;
        private Label lblFullName;
        private Label lblCardInfo;
        
        private Label lblNum1, lblNum2, lblNum3, lblNum4;
        private Panel pnlStat1, pnlStat2, pnlStat3, pnlStat4;

        private Panel pnlBorrowSection;
        private Panel pnlNotificationSection;
        private Panel pnlRecommendSection;

        public HomePage()
        {
            InitializeComponent();
            _service = new Service();
            SetupCustomUI();
            LoadDataToUI();
        }

        private void SetupCustomUI()
        {
            // Thiết lập cửa sổ ban đầu và hỗ trợ co giãn
            this.Size = new Size(1180, 820);
            this.MinimumSize = new Size(1000, 700);
            this.AutoScroll = true;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Text = "Cổng thông tin độc giả - Trang chủ";

            // 1. Banner Độc giả (Neo giãn rộng theo chiều ngang)
            pnlBanner = new Panel()
            {
                Location = new Point(20, 20),
                Size = new Size(1120, 90),
                BackColor = Color.FromArgb(15, 64, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblFullName = new Label()
            {
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            lblCardInfo = new Label()
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(210, 225, 240),
                Location = new Point(20, 52),
                AutoSize = true
            };

            pnlBanner.Controls.Add(lblFullName);
            pnlBanner.Controls.Add(lblCardInfo);
            this.Controls.Add(pnlBanner);

            // 2. Thống kê 4 Card (Chia đều tỷ lệ tự động co giãn ngang)
            int startX = 20;
            int startY = 125;
            int totalWidth = 1120;
            int spacing = 15;
            int cardWidth = (totalWidth - (spacing * 3)) / 4; // Tự động tính toán độ rộng chuẩn
            int cardHeight = 70;

            pnlStat1 = CreateStatCard(startX, startY, cardWidth, cardHeight, out lblNum1, "Sách đang mượn", AnchorStyles.Top | AnchorStyles.Left);
            pnlStat2 = CreateStatCard(startX + (cardWidth + spacing), startY, cardWidth, cardHeight, out lblNum2, "Đến hạn gần nhất", AnchorStyles.Top | AnchorStyles.Left);
            pnlStat3 = CreateStatCard(startX + (cardWidth + spacing) * 2, startY, cardWidth, cardHeight, out lblNum3, "Sách yêu thích", AnchorStyles.Top | AnchorStyles.Left);
            pnlStat4 = CreateStatCard(startX + (cardWidth + spacing) * 3, startY, cardWidth, cardHeight, out lblNum4, "Sách đã đọc", AnchorStyles.Top | AnchorStyles.Left);

            // Tùy chỉnh anchor co giãn cho 2 ô bên phải để không bị lệch khi phóng to màn hình
            pnlStat3.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            pnlStat4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // Ô cuối tự co dãn theo lề phải

            this.Controls.Add(pnlStat1);
            this.Controls.Add(pnlStat2);
            this.Controls.Add(pnlStat3);
            this.Controls.Add(pnlStat4);

            // 3. Sách đang mượn & Thông báo
            CreateBorrowSection(20, 210);
            CreateNotificationSection(755, 210);

            // 4. Gợi ý dành cho bạn
            CreateRecommendSection(20, 470);
        }

        private Panel CreateStatCard(int x, int y, int width, int height, out Label lblNumRef, string title, AnchorStyles anchor)
        {
            Panel pnl = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                Anchor = anchor
            };

            lblNumRef = new Label()
            {
                Text = "00",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 64, 120),
                Location = new Point(15, 10),
                AutoSize = true
            };

            Label lblTitle = new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(15, 40),
                AutoSize = true
            };

            pnl.Controls.Add(lblNumRef);
            pnl.Controls.Add(lblTitle);
            return pnl;
        }

        private void CreateBorrowSection(int x, int y)
        {
            pnlBorrowSection = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(720, 240),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSectionTitle = new Label()
            {
                Text = "Sách đang mượn",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(0, 0),
                AutoSize = true
            };
            pnlBorrowSection.Controls.Add(lblSectionTitle);

            // 2 Thẻ sách mượn chia đều diện tích
            Panel card1 = CreateBookBorrowCard(0, 30, 350, 160, "Nhà giả kim", "Paulo Coelho · BK001", "Hạn trả: 29/03/2024", "• Còn 4 ngày", Color.FromArgb(240, 160, 75));
            Panel card2 = CreateBookBorrowCard(370, 30, 350, 160, "Tư duy nhanh và chậm", "Daniel Kahneman · BK006", "Hạn trả: 02/04/2024", "• Còn 8 ngày", Color.FromArgb(80, 180, 120));

            card2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            pnlBorrowSection.Controls.Add(card1);
            pnlBorrowSection.Controls.Add(card2);

            LinkLabel lblViewAll = new LinkLabel()
            {
                Text = "Xem tất cả sách đang mượn →",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                LinkColor = Color.FromArgb(15, 64, 120),
                Location = new Point(0, 205),
                AutoSize = true
            };
            pnlBorrowSection.Controls.Add(lblViewAll);

            this.Controls.Add(pnlBorrowSection);
        }

        private Panel CreateBookBorrowCard(int x, int y, int w, int h, string name, string author, string date, string tag, Color tagColor)
        {
            Panel card = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.White
            };

            Panel imgBox = new Panel()
            {
                Location = new Point(15, 15),
                Size = new Size(50, 65),
                BackColor = Color.FromArgb(30, 100, 160)
            };

            Label lblName = new Label()
            {
                Text = name,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(75, 15),
                AutoSize = true
            };

            Label lblAuthor = new Label()
            {
                Text = author,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(75, 38),
                AutoSize = true
            };

            Label lblDate = new Label()
            {
                Text = date,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(75, 58),
                AutoSize = true
            };

            Label lblTag = new Label()
            {
                Text = tag,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = tagColor,
                Location = new Point(75, 120),
                AutoSize = true
            };

            card.Controls.Add(imgBox);
            card.Controls.Add(lblName);
            card.Controls.Add(lblAuthor);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblTag);

            return card;
        }

        private void CreateNotificationSection(int x, int y)
        {
            pnlNotificationSection = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(385, 240),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right // Ghim sát góc phải màn hình
            };

            Label lblTitle = new Label()
            {
                Text = "Thông báo",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Location = new Point(15, 12),
                AutoSize = true
            };
            pnlNotificationSection.Controls.Add(lblTitle);

            AddNotiItem(pnlNotificationSection, 45, "Nhắc hạn trả sách", "\"Nhà giả kim\" sẽ đến hạn sau 4 ngày.", Color.FromArgb(200, 60, 60));
            AddNotiItem(pnlNotificationSection, 105, "Sách đặt trước đã sẵn sàng", "Vui lòng nhận trước ngày 28/03.", Color.FromArgb(40, 140, 80));
            AddNotiItem(pnlNotificationSection, 165, "Cập nhật giờ mở cửa", "Thứ Hai - Thứ Bảy: 07:30 - 20:00.", Color.FromArgb(15, 64, 120));

            this.Controls.Add(pnlNotificationSection);
        }

        private void AddNotiItem(Panel parent, int y, string header, string sub, Color color)
        {
            Label lblHeader = new Label()
            {
                Text = header,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(15, y),
                AutoSize = true
            };

            Label lblSub = new Label()
            {
                Text = sub,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(15, y + 20),
                AutoSize = true
            };

            parent.Controls.Add(lblHeader);
            parent.Controls.Add(lblSub);
        }

        private void CreateRecommendSection(int x, int y)
        {
            pnlRecommendSection = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(1120, 220),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblTitle = new Label()
            {
                Text = "Gợi ý dành cho bạn",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = true
            };
            pnlRecommendSection.Controls.Add(lblTitle);

            string[] books = { "Đắc nhân tâm", "Tuổi trẻ đáng giá bao nhiêu", "Lược sử thời gian", "Mắt biếc" };
            string[] authors = { "Dale Carnegie", "Rosie Nguyễn", "Stephen Hawking", "Nguyễn Nhật Ánh" };
            string[] status = { "• Còn sách", "• Còn sách", "• Đã hết", "• Còn sách" };
            Color[] statusColors = { Color.Green, Color.Green, Color.Red, Color.Green };

            int itemWidth = 265;
            int space = 20;

            for (int i = 0; i < books.Length; i++)
            {
                Panel card = new Panel()
                {
                    Location = new Point(i * (itemWidth + space), 30),
                    Size = new Size(itemWidth, 160),
                    BackColor = Color.White
                };

                if (i == 3)
                {
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }

                Panel imgBox = new Panel()
                {
                    Location = new Point(15, 15),
                    Size = new Size(45, 60),
                    BackColor = Color.FromArgb(40, 100, 150)
                };

                Label lblName = new Label()
                {
                    Text = books[i],
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Location = new Point(70, 15),
                    AutoSize = true
                };

                Label lblAuthor = new Label()
                {
                    Text = authors[i],
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = Color.Gray,
                    Location = new Point(70, 38),
                    AutoSize = true
                };

                Label lblStatus = new Label()
                {
                    Text = status[i],
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = statusColors[i],
                    Location = new Point(70, 120),
                    AutoSize = true
                };

                card.Controls.Add(imgBox);
                card.Controls.Add(lblName);
                card.Controls.Add(lblAuthor);
                card.Controls.Add(lblStatus);

                pnlRecommendSection.Controls.Add(card);
            }

            this.Controls.Add(pnlRecommendSection);
        }

        private void LoadDataToUI()
        {
            Readers reader = _service.GetCurrentReader("DG001");
            if (reader != null)
            {
                lblFullName.Text = $"Xin chào, {reader.FullName}";
                lblCardInfo.Text = $"Mã độc giả: {reader.ReaderId}  •  Thẻ còn hạn đến {reader.CardExpiryDate:dd/MM/yyyy}";
            }

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