using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms.LibrarianFormMain
{
    partial class LibrarianMainForm
    {
        private IContainer components = null;

        // MenuStrip
        private System.Windows.Forms.MenuStrip menuMain;

        private ToolStripMenuItem menuSystem;
        private ToolStripMenuItem menuCatalog;
        private ToolStripMenuItem menuCirculation;
        private ToolStripMenuItem menuFine;
        private ToolStripMenuItem menuLibrary;
        private ToolStripMenuItem menuReports;
        private ToolStripMenuItem menuHelp;

        // Menu System
        private ToolStripMenuItem menuLogout;
        private ToolStripMenuItem menuExit;

        // Menu Catalog
        private ToolStripMenuItem menuBooks;
        private ToolStripMenuItem menuBookCopies;
        private ToolStripMenuItem menuCategories;
        private ToolStripMenuItem menuPublishers;
        private ToolStripMenuItem menuBorrowers;
        private ToolStripMenuItem menuLibraryCards;

        // Menu Circulation
        private ToolStripMenuItem menuLoanList;
        private ToolStripMenuItem menuBorrow;
        private ToolStripMenuItem menuReturn;
        private ToolStripMenuItem menuReservations;

        // Menu Fine
        private ToolStripMenuItem menuOverdueBooks;
        private ToolStripMenuItem menuFineReceipts;
        private ToolStripMenuItem menuFinePayment;

        // Menu Library
        private ToolStripMenuItem menuInventory;
        private ToolStripMenuItem menuPolicies;
        private ToolStripMenuItem menuLibrarianAccounts;

        // Menu Reports and Help
        private ToolStripMenuItem menuStatistics;
        private ToolStripMenuItem menuAbout;

        // Navigation
        private System.Windows.Forms.FlowLayoutPanel panelNavigation;
        private System.Windows.Forms.Button btnOverview;
        private System.Windows.Forms.Button btnBooks;
        private System.Windows.Forms.Button btnBookCopies;
        private System.Windows.Forms.Button btnBorrowers;
        private System.Windows.Forms.Button btnBorrowReturn;
        private System.Windows.Forms.Button btnBorrowReceipt;
        private System.Windows.Forms.Button btnReturnReceipt;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnLogout;

        // Main layout
        private TableLayoutPanel rootLayout;

        // TabControl
        private TabControl tabMain;
        private TabPage tabOverview;

        // Overview
        private Panel panelOverview;
        private Label lblOverviewTitle;
        private Label lblOverviewDescription;

        // StatusStrip
        private StatusStrip statusMain;
        private ToolStripStatusLabel statusMessage;
        private ToolStripStatusLabel statusLibrarian;
        private ToolStripStatusLabel statusVersion;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuMain = new System.Windows.Forms.MenuStrip();
            menuSystem = new System.Windows.Forms.ToolStripMenuItem();
            menuLogout = new System.Windows.Forms.ToolStripMenuItem();
            menuExit = new System.Windows.Forms.ToolStripMenuItem();
            menuCatalog = new System.Windows.Forms.ToolStripMenuItem();
            menuBooks = new System.Windows.Forms.ToolStripMenuItem();
            menuBookCopies = new System.Windows.Forms.ToolStripMenuItem();
            menuCategories = new System.Windows.Forms.ToolStripMenuItem();
            menuPublishers = new System.Windows.Forms.ToolStripMenuItem();
            menuBorrowers = new System.Windows.Forms.ToolStripMenuItem();
            menuLibraryCards = new System.Windows.Forms.ToolStripMenuItem();
            menuCirculation = new System.Windows.Forms.ToolStripMenuItem();
            menuLoanList = new System.Windows.Forms.ToolStripMenuItem();
            menuBorrow = new System.Windows.Forms.ToolStripMenuItem();
            menuReturn = new System.Windows.Forms.ToolStripMenuItem();
            menuReservations = new System.Windows.Forms.ToolStripMenuItem();
            menuFine = new System.Windows.Forms.ToolStripMenuItem();
            menuOverdueBooks = new System.Windows.Forms.ToolStripMenuItem();
            menuFineReceipts = new System.Windows.Forms.ToolStripMenuItem();
            menuFinePayment = new System.Windows.Forms.ToolStripMenuItem();
            menuLibrary = new System.Windows.Forms.ToolStripMenuItem();
            menuInventory = new System.Windows.Forms.ToolStripMenuItem();
            menuPolicies = new System.Windows.Forms.ToolStripMenuItem();
            menuLibrarianAccounts = new System.Windows.Forms.ToolStripMenuItem();
            menuReports = new System.Windows.Forms.ToolStripMenuItem();
            menuStatistics = new System.Windows.Forms.ToolStripMenuItem();
            menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            menuAbout = new System.Windows.Forms.ToolStripMenuItem();
            panelNavigation = new System.Windows.Forms.FlowLayoutPanel();
            btnOverview = new System.Windows.Forms.Button();
            btnBooks = new System.Windows.Forms.Button();
            btnBookCopies = new System.Windows.Forms.Button();
            btnBorrowers = new System.Windows.Forms.Button();
            btnBorrowReturn = new System.Windows.Forms.Button();
            btnBorrowReceipt = new System.Windows.Forms.Button();
            btnReturnReceipt = new System.Windows.Forms.Button();
            btnReports = new System.Windows.Forms.Button();
            btnLogout = new System.Windows.Forms.Button();
            rootLayout = new System.Windows.Forms.TableLayoutPanel();
            tabMain = new System.Windows.Forms.TabControl();
            tabOverview = new System.Windows.Forms.TabPage();
            panelOverview = new System.Windows.Forms.Panel();
            lblOverviewTitle = new System.Windows.Forms.Label();
            lblOverviewDescription = new System.Windows.Forms.Label();
            statusMain = new System.Windows.Forms.StatusStrip();
            statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            statusLibrarian = new System.Windows.Forms.ToolStripStatusLabel();
            statusVersion = new System.Windows.Forms.ToolStripStatusLabel();
            menuMain.SuspendLayout();
            panelNavigation.SuspendLayout();
            rootLayout.SuspendLayout();
            tabMain.SuspendLayout();
            tabOverview.SuspendLayout();
            panelOverview.SuspendLayout();
            statusMain.SuspendLayout();
            SuspendLayout();
            // 
            // menuMain
            // 
            menuMain.BackColor = System.Drawing.Color.White;
            menuMain.Dock = System.Windows.Forms.DockStyle.Fill;
            menuMain.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            menuMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuSystem, menuCatalog, menuCirculation, menuFine, menuLibrary, menuReports, menuHelp });
            menuMain.Location = new System.Drawing.Point(0, 0);
            menuMain.Name = "menuMain";
            menuMain.Padding = new System.Windows.Forms.Padding(6, 3, 0, 3);
            menuMain.Size = new System.Drawing.Size(1646, 40);
            menuMain.TabIndex = 0;
            // 
            // menuSystem
            // 
            menuSystem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuLogout, menuExit });
            menuSystem.Name = "menuSystem";
            menuSystem.Size = new System.Drawing.Size(88, 34);
            menuSystem.Text = "Hệ thống";
            // 
            // menuLogout
            // 
            menuLogout.Name = "menuLogout";
            menuLogout.Size = new System.Drawing.Size(164, 26);
            menuLogout.Text = "Đăng xuất";
            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.Size = new System.Drawing.Size(164, 26);
            menuExit.Text = "Thoát";
            // 
            // menuCatalog
            // 
            menuCatalog.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuBooks, menuBookCopies, menuCategories, menuPublishers, menuBorrowers, menuLibraryCards });
            menuCatalog.Name = "menuCatalog";
            menuCatalog.Size = new System.Drawing.Size(95, 34);
            menuCatalog.Text = "Danh mục";
            // 
            // menuBooks
            // 
            menuBooks.Name = "menuBooks";
            menuBooks.Size = new System.Drawing.Size(236, 26);
            menuBooks.Text = "Quản lý đầu sách";
            // 
            // menuBookCopies
            // 
            menuBookCopies.Name = "menuBookCopies";
            menuBookCopies.Size = new System.Drawing.Size(236, 26);
            menuBookCopies.Text = "Sách vật lý / bản sao";
            // 
            // menuCategories
            // 
            menuCategories.Name = "menuCategories";
            menuCategories.Size = new System.Drawing.Size(236, 26);
            menuCategories.Text = "Thể loại sách";
            // 
            // menuPublishers
            // 
            menuPublishers.Name = "menuPublishers";
            menuPublishers.Size = new System.Drawing.Size(236, 26);
            menuPublishers.Text = "Nhà xuất bản";
            // 
            // menuBorrowers
            // 
            menuBorrowers.Name = "menuBorrowers";
            menuBorrowers.Size = new System.Drawing.Size(236, 26);
            menuBorrowers.Text = "Quản lý độc giả";
            // 
            // menuLibraryCards
            // 
            menuLibraryCards.Name = "menuLibraryCards";
            menuLibraryCards.Size = new System.Drawing.Size(236, 26);
            menuLibraryCards.Text = "Thẻ thư viện";
            // 
            // menuCirculation
            // 
            menuCirculation.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuLoanList, menuBorrow, menuReturn, menuReservations });
            menuCirculation.Name = "menuCirculation";
            menuCirculation.Size = new System.Drawing.Size(95, 34);
            menuCirculation.Text = "Lưu thông";
            // 
            // menuLoanList
            // 
            menuLoanList.Name = "menuLoanList";
            menuLoanList.Size = new System.Drawing.Size(209, 26);
            menuLoanList.Text = "Danh sách phiếu";
            // 
            // menuBorrow
            // 
            menuBorrow.Name = "menuBorrow";
            menuBorrow.Size = new System.Drawing.Size(209, 26);
            menuBorrow.Text = "Lập phiếu mượn";
            // 
            // menuReturn
            // 
            menuReturn.Name = "menuReturn";
            menuReturn.Size = new System.Drawing.Size(209, 26);
            menuReturn.Text = "Nhận trả sách";
            // 
            // menuReservations
            // 
            menuReservations.Name = "menuReservations";
            menuReservations.Size = new System.Drawing.Size(209, 26);
            menuReservations.Text = "Đặt trước sách";
            // 
            // menuFine
            // 
            menuFine.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuOverdueBooks, menuFineReceipts, menuFinePayment });
            menuFine.Name = "menuFine";
            menuFine.Size = new System.Drawing.Size(161, 34);
            menuFine.Text = "Quá hạn / Tiền phạt";
            // 
            // menuOverdueBooks
            // 
            menuOverdueBooks.Name = "menuOverdueBooks";
            menuOverdueBooks.Size = new System.Drawing.Size(243, 26);
            menuOverdueBooks.Text = "Quản lý sách quá hạn";
            // 
            // menuFineReceipts
            // 
            menuFineReceipts.Name = "menuFineReceipts";
            menuFineReceipts.Size = new System.Drawing.Size(243, 26);
            menuFineReceipts.Text = "Quản lý phiếu phạt";
            // 
            // menuFinePayment
            // 
            menuFinePayment.Name = "menuFinePayment";
            menuFinePayment.Size = new System.Drawing.Size(243, 26);
            menuFinePayment.Text = "Thanh toán tiền phạt";
            // 
            // menuLibrary
            // 
            menuLibrary.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuInventory, menuPolicies, menuLibrarianAccounts });
            menuLibrary.Name = "menuLibrary";
            menuLibrary.Size = new System.Drawing.Size(148, 34);
            menuLibrary.Text = "Hệ thống thư viện";
            // 
            // menuInventory
            // 
            menuInventory.Name = "menuInventory";
            menuInventory.Size = new System.Drawing.Size(230, 26);
            menuInventory.Text = "Quản lý kiểm kê";
            // 
            // menuPolicies
            // 
            menuPolicies.Name = "menuPolicies";
            menuPolicies.Size = new System.Drawing.Size(230, 26);
            menuPolicies.Text = "Chính sách thư viện";
            // 
            // menuLibrarianAccounts
            // 
            menuLibrarianAccounts.Name = "menuLibrarianAccounts";
            menuLibrarianAccounts.Size = new System.Drawing.Size(230, 26);
            menuLibrarianAccounts.Text = "Tài khoản thủ thư";
            // 
            // menuReports
            // 
            menuReports.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuStatistics });
            menuReports.Name = "menuReports";
            menuReports.Size = new System.Drawing.Size(78, 34);
            menuReports.Text = "Báo cáo";
            // 
            // menuStatistics
            // 
            menuStatistics.Name = "menuStatistics";
            menuStatistics.Size = new System.Drawing.Size(213, 26);
            menuStatistics.Text = "Báo cáo thống kê";
            // 
            // menuHelp
            // 
            menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuAbout });
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new System.Drawing.Size(82, 34);
            menuHelp.Text = "Trợ giúp";
            // 
            // menuAbout
            // 
            menuAbout.Name = "menuAbout";
            menuAbout.Size = new System.Drawing.Size(241, 26);
            menuAbout.Text = "Giới thiệu phần mềm";
            // 
            // panelNavigation
            // 
            panelNavigation.AutoScroll = true;
            panelNavigation.BackColor = System.Drawing.Color.FromArgb(((int)((byte)250)), ((int)((byte)250)), ((int)((byte)250)));
            panelNavigation.Controls.Add(btnOverview);
            panelNavigation.Controls.Add(btnBooks);
            panelNavigation.Controls.Add(btnBookCopies);
            panelNavigation.Controls.Add(btnBorrowers);
            panelNavigation.Controls.Add(btnBorrowReturn);
            panelNavigation.Controls.Add(btnBorrowReceipt);
            panelNavigation.Controls.Add(btnReturnReceipt);
            panelNavigation.Controls.Add(btnReports);
            panelNavigation.Controls.Add(btnLogout);
            panelNavigation.Dock = System.Windows.Forms.DockStyle.Fill;
            panelNavigation.Location = new System.Drawing.Point(3, 44);
            panelNavigation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            panelNavigation.Name = "panelNavigation";
            panelNavigation.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            panelNavigation.Size = new System.Drawing.Size(1640, 59);
            panelNavigation.TabIndex = 1;
            panelNavigation.WrapContents = false;
            // 
            // btnOverview
            // 
            btnOverview.Location = new System.Drawing.Point(9, 11);
            btnOverview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnOverview.Name = "btnOverview";
            btnOverview.Size = new System.Drawing.Size(118, 31);
            btnOverview.TabIndex = 0;
            btnOverview.Text = "Tổng quan";
            // 
            // btnBooks
            // 
            btnBooks.Location = new System.Drawing.Point(133, 11);
            btnBooks.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnBooks.Name = "btnBooks";
            btnBooks.Size = new System.Drawing.Size(144, 31);
            btnBooks.TabIndex = 1;
            btnBooks.Text = "Quản lý đầu sách";
            // 
            // btnBookCopies
            // 
            btnBookCopies.Location = new System.Drawing.Point(283, 11);
            btnBookCopies.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnBookCopies.Name = "btnBookCopies";
            btnBookCopies.Size = new System.Drawing.Size(159, 31);
            btnBookCopies.TabIndex = 2;
            btnBookCopies.Text = "Quản lý sách vật lý";
            // 
            // btnBorrowers
            // 
            btnBorrowers.Location = new System.Drawing.Point(448, 11);
            btnBorrowers.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnBorrowers.Name = "btnBorrowers";
            btnBorrowers.Size = new System.Drawing.Size(128, 31);
            btnBorrowers.TabIndex = 3;
            btnBorrowers.Text = "Quản lý độc giả";
            // 
            // btnBorrowReturn
            // 
            btnBorrowReturn.Location = new System.Drawing.Point(582, 11);
            btnBorrowReturn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnBorrowReturn.Name = "btnBorrowReturn";
            btnBorrowReturn.Size = new System.Drawing.Size(132, 31);
            btnBorrowReturn.TabIndex = 4;
            btnBorrowReturn.Text = "Danh sách phiếu";
            // 
            // btnBorrowReceipt
            // 
            btnBorrowReceipt.Location = new System.Drawing.Point(720, 11);
            btnBorrowReceipt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnBorrowReceipt.Name = "btnBorrowReceipt";
            btnBorrowReceipt.Size = new System.Drawing.Size(135, 31);
            btnBorrowReceipt.TabIndex = 5;
            btnBorrowReceipt.Text = "Lập phiếu mượn";
            // 
            // btnReturnReceipt
            // 
            btnReturnReceipt.Location = new System.Drawing.Point(861, 11);
            btnReturnReceipt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnReturnReceipt.Name = "btnReturnReceipt";
            btnReturnReceipt.Size = new System.Drawing.Size(113, 31);
            btnReturnReceipt.TabIndex = 6;
            btnReturnReceipt.Text = "Nhận trả sách";
            // 
            // btnReports
            // 
            btnReports.Location = new System.Drawing.Point(980, 11);
            btnReports.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnReports.Name = "btnReports";
            btnReports.Size = new System.Drawing.Size(137, 31);
            btnReports.TabIndex = 7;
            btnReports.Text = "Báo cáo thống kê";
            // 
            // btnLogout
            // 
            btnLogout.BackColor = System.Drawing.Color.White;
            btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            btnLogout.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)((byte)215)), ((int)((byte)220)), ((int)((byte)226)));
            btnLogout.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)240)), ((int)((byte)240)));
            btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLogout.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnLogout.ForeColor = System.Drawing.Color.Firebrick;
            btnLogout.Location = new System.Drawing.Point(1123, 8);
            btnLogout.Margin = new System.Windows.Forms.Padding(3, 1, 6, 1);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new System.Drawing.Size(114, 45);
            btnLogout.TabIndex = 8;
            btnLogout.Text = "Đăng xuất";
            btnLogout.UseVisualStyleBackColor = false;
            // 
            // rootLayout
            // 
            rootLayout.BackColor = System.Drawing.Color.White;
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Controls.Add(menuMain, 0, 0);
            rootLayout.Controls.Add(panelNavigation, 0, 1);
            rootLayout.Controls.Add(tabMain, 0, 2);
            rootLayout.Controls.Add(statusMain, 0, 3);
            rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            rootLayout.Location = new System.Drawing.Point(0, 0);
            rootLayout.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            rootLayout.Name = "rootLayout";
            rootLayout.RowCount = 4;
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            rootLayout.Size = new System.Drawing.Size(1646, 1055);
            rootLayout.TabIndex = 0;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabOverview);
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            tabMain.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            tabMain.ItemSize = new System.Drawing.Size(190, 32);
            tabMain.Location = new System.Drawing.Point(3, 111);
            tabMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new System.Drawing.Size(1640, 907);
            tabMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            tabMain.TabIndex = 2;
            tabMain.DrawItem += TabMain_DrawItem;
            tabMain.MouseDown += TabMain_MouseDown;
            tabMain.MouseLeave += TabMain_MouseLeave;
            tabMain.MouseMove += TabMain_MouseMove;
            // 
            // tabOverview
            // 
            tabOverview.BackColor = System.Drawing.Color.FromArgb(((int)((byte)245)), ((int)((byte)247)), ((int)((byte)250)));
            tabOverview.Controls.Add(panelOverview);
            tabOverview.Location = new System.Drawing.Point(4, 36);
            tabOverview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            tabOverview.Name = "tabOverview";
            tabOverview.Padding = new System.Windows.Forms.Padding(9, 11, 9, 11);
            tabOverview.Size = new System.Drawing.Size(1632, 867);
            tabOverview.TabIndex = 0;
            tabOverview.Text = "Tổng quan";
            // 
            // panelOverview
            // 
            panelOverview.BackColor = System.Drawing.Color.FromArgb(((int)((byte)245)), ((int)((byte)247)), ((int)((byte)250)));
            panelOverview.Controls.Add(lblOverviewTitle);
            panelOverview.Controls.Add(lblOverviewDescription);
            panelOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            panelOverview.Location = new System.Drawing.Point(9, 11);
            panelOverview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            panelOverview.Name = "panelOverview";
            panelOverview.Size = new System.Drawing.Size(1614, 845);
            panelOverview.TabIndex = 0;
            // 
            // lblOverviewTitle
            // 
            lblOverviewTitle.AutoSize = true;
            lblOverviewTitle.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            lblOverviewTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)30)), ((int)((byte)55)), ((int)((byte)85)));
            lblOverviewTitle.Location = new System.Drawing.Point(29, 33);
            lblOverviewTitle.Name = "lblOverviewTitle";
            lblOverviewTitle.Size = new System.Drawing.Size(616, 50);
            lblOverviewTitle.TabIndex = 0;
            lblOverviewTitle.Text = "LIBRA - Hệ thống quản lý thư viện";
            // 
            // lblOverviewDescription
            // 
            lblOverviewDescription.AutoSize = true;
            lblOverviewDescription.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblOverviewDescription.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)100)), ((int)((byte)115)), ((int)((byte)135)));
            lblOverviewDescription.Location = new System.Drawing.Point(33, 107);
            lblOverviewDescription.Name = "lblOverviewDescription";
            lblOverviewDescription.Size = new System.Drawing.Size(510, 23);
            lblOverviewDescription.TabIndex = 1;
            lblOverviewDescription.Text = "Chọn một chức năng trên thanh điều hướng để bắt đầu làm việc.";
            // 
            // statusMain
            // 
            statusMain.AutoSize = false;
            statusMain.BackColor = System.Drawing.Color.White;
            statusMain.Dock = System.Windows.Forms.DockStyle.Fill;
            statusMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusMessage, statusLibrarian, statusVersion });
            statusMain.Location = new System.Drawing.Point(0, 1022);
            statusMain.Name = "statusMain";
            statusMain.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusMain.Size = new System.Drawing.Size(1646, 33);
            statusMain.SizingGrip = false;
            statusMain.TabIndex = 3;
            // 
            // statusMessage
            // 
            statusMessage.ForeColor = System.Drawing.Color.DimGray;
            statusMessage.Name = "statusMessage";
            statusMessage.Size = new System.Drawing.Size(1439, 27);
            statusMessage.Spring = true;
            statusMessage.Text = "● Bản mẫu giao diện · Chưa kết nối cơ sở dữ liệu";
            // 
            // statusLibrarian
            // 
            statusLibrarian.Name = "statusLibrarian";
            statusLibrarian.Size = new System.Drawing.Size(108, 27);
            statusLibrarian.Text = "Thủ thư: admin";
            // 
            // statusVersion
            // 
            statusVersion.Name = "statusVersion";
            statusVersion.Size = new System.Drawing.Size(82, 27);
            statusVersion.Text = "LIBRA 2.4.0";
            // 
            // LibrarianMainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(((int)((byte)245)), ((int)((byte)247)), ((int)((byte)250)));
            ClientSize = new System.Drawing.Size(1646, 1055);
            Controls.Add(rootLayout);
            MainMenuStrip = menuMain;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MinimumSize = new System.Drawing.Size(1255, 918);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "LIBRA - Hệ thống quản lý thư viện";
            menuMain.ResumeLayout(false);
            menuMain.PerformLayout();
            panelNavigation.ResumeLayout(false);
            rootLayout.ResumeLayout(false);
            rootLayout.PerformLayout();
            tabMain.ResumeLayout(false);
            tabOverview.ResumeLayout(false);
            panelOverview.ResumeLayout(false);
            panelOverview.PerformLayout();
            statusMain.ResumeLayout(false);
            statusMain.PerformLayout();
            ResumeLayout(false);
        }

        // Helper: cấu hình button nav thường (tránh lặp code)
        private void ConfigureNavButton(Button btn, string name, string text, int width)
        {
            btn.BackColor = Color.White;
            btn.Cursor = Cursors.Hand;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 226);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 247, 255);
            btn.Font = new Font("Segoe UI", 9F);
            btn.ForeColor = Color.FromArgb(35, 45, 60);
            btn.Margin = new Padding(3, 1, 5, 1);
            btn.Name = name;
            btn.Size = new Size(width, 34);
            btn.Text = text;
            btn.UseVisualStyleBackColor = false;
        }
    }
}