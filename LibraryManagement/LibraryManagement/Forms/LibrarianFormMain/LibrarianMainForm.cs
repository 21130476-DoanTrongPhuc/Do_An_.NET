using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibraryManagement.Forms.LibrarianFormMain
{
    public partial class LibrarianMainForm : Form
    {
        private int hoveredCloseTabIndex = -1;

        public LibrarianMainForm()
        {
            InitializeComponent();
        }

        // =====================================================
        // 0. CHỈ CHẠY LOGIC RUNTIME KHI FORM THỰC SỰ MỞ
        // =====================================================

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Không chạy khi đang trong Designer
            if (DesignMode) return;

            ConfigureNavigation();
            ConfigureMenuEvents();
            // Tab events đã đăng ký trong InitializeComponent
        }

        // =====================================================
        // 1. NAVIGATION BUTTON EVENTS
        // =====================================================

        private void ConfigureNavigation()
        {
            this.btnOverview.Click += (sender, e) =>
            {
                this.tabMain.SelectedTab = this.tabOverview;
            };

            this.btnBooks.Click += (sender, e) =>
                SelectOrCreateTab("Quản lý đầu sách", "books");

            this.btnBookCopies.Click += (sender, e) =>
                SelectOrCreateTab("Sách vật lý / bản sao", "book-copies");

            this.btnBorrowers.Click += (sender, e) =>
                SelectOrCreateTab("Quản lý độc giả", "borrowers");

            this.btnBorrowReturn.Click += (sender, e) =>
                SelectOrCreateTab("Danh sách phiếu", "loan-list");

            this.btnBorrowReceipt.Click += (sender, e) =>
                SelectOrCreateTab("Lập phiếu mượn", "borrow");

            this.btnReturnReceipt.Click += (sender, e) =>
                SelectOrCreateTab("Nhận trả sách", "return");

            this.btnReports.Click += (sender, e) =>
                SelectOrCreateTab("Báo cáo thống kê", "reports");

            this.btnLogout.Click += (sender, e) => Logout();
        }

        // =====================================================
        // 2. MENU EVENTS
        // =====================================================

        private void ConfigureMenuEvents()
        {
            this.menuLogout.Click += (sender, e) => Logout();
            this.menuExit.Click += (sender, e) => ExitApplication();

            this.menuBooks.Click += (sender, e) =>
                SelectOrCreateTab("Quản lý đầu sách", "books");

            this.menuBookCopies.Click += (sender, e) =>
                SelectOrCreateTab("Sách vật lý / bản sao", "book-copies");

            this.menuCategories.Click += (sender, e) =>
                SelectOrCreateTab("Thể loại sách", "categories");

            this.menuPublishers.Click += (sender, e) =>
                SelectOrCreateTab("Nhà xuất bản", "publishers");

            this.menuBorrowers.Click += (sender, e) =>
                SelectOrCreateTab("Quản lý độc giả", "borrowers");

            this.menuLibraryCards.Click += (sender, e) =>
                SelectOrCreateTab("Thẻ thư viện", "library-cards");

            this.menuLoanList.Click += (sender, e) =>
                SelectOrCreateTab("Danh sách phiếu", "loan-list");

            this.menuBorrow.Click += (sender, e) =>
                SelectOrCreateTab("Lập phiếu mượn", "borrow");

            this.menuReturn.Click += (sender, e) =>
                SelectOrCreateTab("Nhận trả sách", "return");

            this.menuReservations.Click += (sender, e) =>
                SelectOrCreateTab("Đặt trước sách", "reservations");

            this.menuOverdueBooks.Click += (sender, e) =>
                SelectOrCreateTab("Quản lý sách quá hạn", "overdue-books");

            this.menuFineReceipts.Click += (sender, e) =>
                SelectOrCreateTab("Quản lý phiếu phạt", "fine-receipts");

            this.menuFinePayment.Click += (sender, e) =>
                SelectOrCreateTab("Thanh toán tiền phạt", "fine-payment");

            this.menuInventory.Click += (sender, e) =>
                SelectOrCreateTab("Quản lý kiểm kê", "inventory");

            this.menuPolicies.Click += (sender, e) =>
                SelectOrCreateTab("Chính sách thư viện", "policies");

            this.menuLibrarianAccounts.Click += (sender, e) =>
                SelectOrCreateTab("Tài khoản thủ thư", "librarian-accounts");

            this.menuStatistics.Click += (sender, e) =>
                SelectOrCreateTab("Báo cáo thống kê", "reports");

            this.menuAbout.Click += (sender, e) => ShowAbout();
        }

        // =====================================================
        // 3. CREATE OR SELECT TAB
        // =====================================================

        private void SelectOrCreateTab(string title, string pageKey)
        {
            foreach (TabPage existingPage in this.tabMain.TabPages)
            {
                if (existingPage.Tag != null &&
                    existingPage.Tag.ToString() == pageKey)
                {
                    this.tabMain.SelectedTab = existingPage;
                    return;
                }
            }

            TabPage newTab = new TabPage();
            newTab.Text = title;
            newTab.Tag = pageKey;
            newTab.BackColor = Color.FromArgb(245, 247, 250);
            newTab.Padding = new Padding(8);

            Control content = CreatePageContent(pageKey);
            content.Dock = DockStyle.Fill;
            newTab.Controls.Add(content);

            this.tabMain.TabPages.Add(newTab);
            this.tabMain.SelectedTab = newTab;
            this.tabMain.Invalidate();
        }

        // =====================================================
        // 4. CREATE PAGE CONTENT
        // =====================================================

        private Control CreatePageContent(string pageKey)
        {
            switch (pageKey)
            {
                case "book-copies":
                    // ⚠️ Nếu BookCopyControl chưa tồn tại, tạm dùng placeholder
                    // để Designer/Form vẫn chạy được. Khi có control rồi, mở comment.
                    return new BookCopyControl();
                    
                    return CreatePlaceholder(
                        "Sách vật lý / bản sao",
                        "Quản lý thông tin sách vật lý, tình trạng và vị trí."
                    );

                case "books":
                    return CreatePlaceholder("Quản lý đầu sách",
                        "Quản lý thông tin thư mục, số lượng sách vật lý được theo dõi riêng.");

                case "borrowers":
                    return CreatePlaceholder("Quản lý độc giả",
                        "Quản lý thông tin và hồ sơ người mượn sách.");

                case "loan-list":
                    return CreatePlaceholder("Danh sách phiếu",
                        "Theo dõi các phiếu mượn và tình trạng lưu thông sách.");

                case "borrow":
                    return CreatePlaceholder("Lập phiếu mượn",
                        "Tạo phiếu mượn sách cho độc giả.");

                case "return":
                    return CreatePlaceholder("Nhận trả sách",
                        "Tiếp nhận sách được độc giả trả lại.");

                case "categories":
                    return CreatePlaceholder("Thể loại sách",
                        "Quản lý danh mục thể loại sách.");

                case "publishers":
                    return CreatePlaceholder("Nhà xuất bản",
                        "Quản lý thông tin nhà xuất bản.");

                case "library-cards":
                    return CreatePlaceholder("Thẻ thư viện",
                        "Quản lý thẻ thư viện của độc giả.");

                case "reservations":
                    return CreatePlaceholder("Đặt trước sách",
                        "Quản lý các yêu cầu đặt trước sách.");

                case "overdue-books":
                    return CreatePlaceholder("Quản lý sách quá hạn",
                        "Theo dõi các sách đã quá hạn trả.");

                case "fine-receipts":
                    return CreatePlaceholder("Quản lý phiếu phạt",
                        "Quản lý các phiếu phạt phát sinh.");

                case "fine-payment":
                    return CreatePlaceholder("Thanh toán tiền phạt",
                        "Tiếp nhận và ghi nhận thanh toán tiền phạt.");

                case "inventory":
                    return CreatePlaceholder("Quản lý kiểm kê",
                        "Theo dõi và thực hiện kiểm kê sách trong thư viện.");

                case "policies":
                    return CreatePlaceholder("Chính sách thư viện",
                        "Quản lý các quy định và chính sách của thư viện.");

                case "librarian-accounts":
                    return CreatePlaceholder("Tài khoản thủ thư",
                        "Quản lý tài khoản nhân viên thư viện.");

                case "reports":
                    return CreatePlaceholder("Báo cáo thống kê",
                        "Tổng hợp và thống kê hoạt động của thư viện.");

                default:
                    return CreatePlaceholder("Chức năng",
                        "Giao diện chức năng đang được phát triển.");
            }
        }

        // =====================================================
        // 5. PLACEHOLDER
        // =====================================================

        private Control CreatePlaceholder(string title, string description)
        {
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(245, 247, 250);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(30, 55, 85);
            lblTitle.Location = new Point(20, 20);

            Label lblDescription = new Label();
            lblDescription.Text = description;
            lblDescription.AutoSize = true;
            lblDescription.Font = new Font("Segoe UI", 10F);
            lblDescription.ForeColor = Color.FromArgb(100, 115, 135);
            lblDescription.Location = new Point(23, 70);

            Panel contentPanel = new Panel();
            contentPanel.BackColor = Color.White;
            contentPanel.Location = new Point(16, 115);
            contentPanel.Size = new Size(900, 200);
            contentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Label lblComingSoon = new Label();
            lblComingSoon.Text = "Giao diện chức năng đang được phát triển...";
            lblComingSoon.AutoSize = true;
            lblComingSoon.Font = new Font("Segoe UI", 11F);
            lblComingSoon.ForeColor = Color.Gray;
            lblComingSoon.Location = new Point(25, 25);

            contentPanel.Controls.Add(lblComingSoon);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblDescription);
            mainPanel.Controls.Add(contentPanel);

            return mainPanel;
        }

        // =====================================================
        // 6. DRAW TAB
        // =====================================================

        private void TabMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Guard: không vẽ khi đang trong Designer để tránh crash
            if (DesignMode) return;
            if (e.Index < 0 || e.Index >= this.tabMain.TabPages.Count) return;

            TabPage page = this.tabMain.TabPages[e.Index];
            Rectangle tabRect = this.tabMain.GetTabRect(e.Index);

            bool isSelected = this.tabMain.SelectedIndex == e.Index;
            bool isOverview = page == this.tabOverview;

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Nền
            Color backgroundColor = isSelected
                ? Color.White
                : Color.FromArgb(240, 242, 245);

            using (SolidBrush backgroundBrush = new SolidBrush(backgroundColor))
            {
                graphics.FillRectangle(backgroundBrush, tabRect);
            }

            // Gạch chân tab đang chọn
            if (isSelected)
            {
                using (Pen borderPen = new Pen(Color.FromArgb(0, 120, 215), 2))
                {
                    graphics.DrawLine(borderPen,
                        tabRect.Left, tabRect.Bottom - 1,
                        tabRect.Right, tabRect.Bottom - 1);
                }
            }

            // Text
            Rectangle textRect = new Rectangle(
                tabRect.X + 12,
                tabRect.Y,
                tabRect.Width - (isOverview ? 20 : 48),
                tabRect.Height);

            TextRenderer.DrawText(
                graphics,
                page.Text,
                new Font("Segoe UI", 9.5F),
                textRect,
                Color.FromArgb(35, 45, 60),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (isOverview) return;

            // Nút đóng
            Rectangle closeRect = GetTabCloseRectangle(tabRect);
            bool isHovered = hoveredCloseTabIndex == e.Index;

            if (isHovered)
            {
                using (SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(255, 225, 225)))
                {
                    graphics.FillEllipse(hoverBrush, closeRect);
                }
            }

            Color closeColor = isHovered
                ? Color.FromArgb(210, 40, 40)
                : Color.FromArgb(100, 100, 100);

            float centerX = closeRect.Left + closeRect.Width / 2f;
            float centerY = closeRect.Top + closeRect.Height / 2f;
            float offset = 4F;

            using (Pen closePen = new Pen(closeColor, 1.8F))
            {
                closePen.StartCap = LineCap.Round;
                closePen.EndCap = LineCap.Round;

                graphics.DrawLine(closePen,
                    centerX - offset, centerY - offset,
                    centerX + offset, centerY + offset);
                graphics.DrawLine(closePen,
                    centerX + offset, centerY - offset,
                    centerX - offset, centerY + offset);
            }
        }

        // =====================================================
        // 7. CLOSE RECT
        // =====================================================

        private Rectangle GetTabCloseRectangle(Rectangle tabRect)
        {
            int size = 22;
            return new Rectangle(
                tabRect.Right - 32,
                tabRect.Top + (tabRect.Height - size) / 2,
                size,
                size);
        }

        // =====================================================
        // 8. MOUSE EVENTS
        // =====================================================

        private void TabMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (DesignMode) return;
            if (e.Button != MouseButtons.Left) return;

            for (int i = 0; i < this.tabMain.TabPages.Count; i++)
            {
                Rectangle tabRect = this.tabMain.GetTabRect(i);
                if (!tabRect.Contains(e.Location)) continue;

                TabPage page = this.tabMain.TabPages[i];
                if (page == this.tabOverview) return;

                Rectangle closeRect = GetTabCloseRectangle(tabRect);
                if (closeRect.Contains(e.Location))
                {
                    this.tabMain.TabPages.Remove(page);
                    page.Dispose();
                    hoveredCloseTabIndex = -1;
                    this.tabMain.Invalidate();
                    return;
                }
            }
        }

        private void TabMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (DesignMode) return;

            int newHoveredIndex = -1;

            for (int i = 0; i < this.tabMain.TabPages.Count; i++)
            {
                TabPage page = this.tabMain.TabPages[i];
                if (page == this.tabOverview) continue;

                Rectangle closeRect = GetTabCloseRectangle(this.tabMain.GetTabRect(i));
                if (closeRect.Contains(e.Location))
                {
                    newHoveredIndex = i;
                    break;
                }
            }

            if (hoveredCloseTabIndex != newHoveredIndex)
            {
                hoveredCloseTabIndex = newHoveredIndex;
                this.tabMain.Invalidate();
            }
        }

        private void TabMain_MouseLeave(object sender, EventArgs e)
        {
            if (DesignMode) return;
            hoveredCloseTabIndex = -1;
            this.tabMain.Invalidate();
        }

        // =====================================================
        // 9. LOGOUT
        // =====================================================

        private void Logout()
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show(
                    "Bạn đã đăng xuất khỏi hệ thống.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.Close();
            }
        }

        // =====================================================
        // 10. EXIT
        // =====================================================

        private void ExitApplication()
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát chương trình không?",
                "Xác nhận thoát",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        // =====================================================
        // 11. ABOUT
        // =====================================================

        private void ShowAbout()
        {
            MessageBox.Show(
                "LIBRA - HỆ THỐNG QUẢN LÝ THƯ VIỆN\n\n" +
                "Phiên bản: 2.4.0\n" +
                "Ứng dụng quản lý hoạt động thư viện.",
                "Giới thiệu phần mềm",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}