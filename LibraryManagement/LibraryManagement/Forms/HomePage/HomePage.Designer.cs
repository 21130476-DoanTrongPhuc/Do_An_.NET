namespace LibraryManagement.Forms
{
    partial class HomePage
    {
        private System.ComponentModel.IContainer components = null;
 
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
 
        #region Windows Form Designer generated code
 
        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.pnlUser = new System.Windows.Forms.Panel();
            this.picAvatar = new System.Windows.Forms.Panel();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblUserCode = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.pnlBreadcrumb = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlSuggestHost = new System.Windows.Forms.Panel();
            this.grpSuggest = new System.Windows.Forms.GroupBox();
            this.flowSuggest = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlMiddle = new System.Windows.Forms.Panel();
            this.grpBorrow = new System.Windows.Forms.GroupBox();
            this.flowBorrow = new System.Windows.Forms.FlowLayoutPanel();
            this.lnkAllBorrow = new System.Windows.Forms.LinkLabel();
            this.pnlNoticeHost = new System.Windows.Forms.Panel();
            this.grpNotice = new System.Windows.Forms.GroupBox();
            this.flowNotice = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlStatsHost = new System.Windows.Forms.Panel();
            this.tblStats = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBanner = new System.Windows.Forms.Panel();
            this.lblHello = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblCode = new System.Windows.Forms.Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.pnlToolbar.SuspendLayout();
            this.pnlUser.SuspendLayout();
            this.pnlBreadcrumb.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlSuggestHost.SuspendLayout();
            this.grpSuggest.SuspendLayout();
            this.pnlMiddle.SuspendLayout();
            this.grpBorrow.SuspendLayout();
            this.pnlNoticeHost.SuspendLayout();
            this.grpNotice.SuspendLayout();
            this.pnlStatsHost.SuspendLayout();
            this.pnlBanner.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip
            //
            this.menuStrip.BackColor = System.Drawing.Color.White;
            this.menuStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuStrip.Name = "menuStrip";
            //
            // toolStrip
            //
            this.toolStrip.BackColor = System.Drawing.Color.White;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip.Name = "toolStrip";
            //
            // lblUserName / lblUserCode / picAvatar / btnLogout
            //
            this.picAvatar.Location = new System.Drawing.Point(14, 10);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(40, 40);
            this.lblUserName.AutoSize = true;
            this.lblUserName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblUserName.Location = new System.Drawing.Point(62, 12);
            this.lblUserName.Name = "lblUserName";
            this.lblUserCode.AutoSize = true;
            this.lblUserCode.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUserCode.ForeColor = System.Drawing.Color.Gray;
            this.lblUserCode.Location = new System.Drawing.Point(62, 32);
            this.lblUserCode.Name = "lblUserCode";
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Location = new System.Drawing.Point(206, 14);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(84, 32);
            this.btnLogout.Text = "Đăng xuất";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            //
            // pnlUser
            //
            this.pnlUser.Controls.Add(this.picAvatar);
            this.pnlUser.Controls.Add(this.lblUserName);
            this.pnlUser.Controls.Add(this.lblUserCode);
            this.pnlUser.Controls.Add(this.btnLogout);
            this.pnlUser.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlUser.Name = "pnlUser";
            this.pnlUser.Size = new System.Drawing.Size(310, 60);
            //
            // pnlToolbar
            //
            this.pnlToolbar.BackColor = System.Drawing.Color.White;
            this.pnlToolbar.Controls.Add(this.toolStrip);
            this.pnlToolbar.Controls.Add(this.pnlUser);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1100, 60);
            //
            // lblBreadcrumb
            //
            this.lblBreadcrumb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblBreadcrumb.Text = "Cổng độc giả  ›  Trang chủ";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlBreadcrumb
            //
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);
            this.pnlBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Name = "pnlBreadcrumb";
            this.pnlBreadcrumb.Size = new System.Drawing.Size(1100, 36);
            //
            // flowSuggest / grpSuggest / pnlSuggestHost
            //
            this.flowSuggest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSuggest.Name = "flowSuggest";
            this.flowSuggest.Padding = new System.Windows.Forms.Padding(6, 4, 0, 0);
            this.grpSuggest.Controls.Add(this.flowSuggest);
            this.grpSuggest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSuggest.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpSuggest.Name = "grpSuggest";
            this.grpSuggest.Text = "Gợi ý dành cho bạn";
            this.pnlSuggestHost.Controls.Add(this.grpSuggest);
            this.pnlSuggestHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSuggestHost.Name = "pnlSuggestHost";
            this.pnlSuggestHost.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            //
            // lnkAllBorrow / flowBorrow / grpBorrow
            //
            this.lnkAllBorrow.AutoSize = true;
            this.lnkAllBorrow.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lnkAllBorrow.Location = new System.Drawing.Point(12, 130);
            this.lnkAllBorrow.Name = "lnkAllBorrow";
            this.lnkAllBorrow.Text = "Xem tất cả sách đang mượn →";
            this.flowBorrow.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowBorrow.Height = 96;
            this.flowBorrow.Name = "flowBorrow";
            this.flowBorrow.Padding = new System.Windows.Forms.Padding(6, 4, 0, 0);
            this.grpBorrow.Controls.Add(this.lnkAllBorrow);
            this.grpBorrow.Controls.Add(this.flowBorrow);
            this.grpBorrow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBorrow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpBorrow.Name = "grpBorrow";
            this.grpBorrow.Text = "Sách đang mượn";
            //
            // flowNotice / grpNotice / pnlNoticeHost
            //
            this.flowNotice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowNotice.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowNotice.Name = "flowNotice";
            this.flowNotice.WrapContents = false;
            this.grpNotice.Controls.Add(this.flowNotice);
            this.grpNotice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpNotice.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpNotice.Name = "grpNotice";
            this.grpNotice.Text = "Thông báo";
            this.pnlNoticeHost.Controls.Add(this.grpNotice);
            this.pnlNoticeHost.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlNoticeHost.Name = "pnlNoticeHost";
            this.pnlNoticeHost.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.pnlNoticeHost.Size = new System.Drawing.Size(400, 100);
            //
            // pnlMiddle
            //
            this.pnlMiddle.Controls.Add(this.grpBorrow);
            this.pnlMiddle.Controls.Add(this.pnlNoticeHost);
            this.pnlMiddle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMiddle.Name = "pnlMiddle";
            this.pnlMiddle.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.pnlMiddle.Size = new System.Drawing.Size(1076, 212);
            //
            // tblStats / pnlStatsHost
            //
            this.tblStats.ColumnCount = 4;
            for (int i = 0; i < 4; i++)
                this.tblStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblStats.Name = "tblStats";
            this.tblStats.RowCount = 1;
            this.tblStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlStatsHost.Controls.Add(this.tblStats);
            this.pnlStatsHost.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStatsHost.Name = "pnlStatsHost";
            this.pnlStatsHost.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.pnlStatsHost.Size = new System.Drawing.Size(1076, 88);
            //
            // pnlBanner
            //
            this.lblHello.AutoSize = true;
            this.lblHello.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblHello.ForeColor = System.Drawing.Color.White;
            this.lblHello.Location = new System.Drawing.Point(16, 10);
            this.lblHello.Name = "lblHello";
            this.lblHello.Text = "Xin chào,";
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblName.ForeColor = System.Drawing.Color.White;
            this.lblName.Location = new System.Drawing.Point(14, 26);
            this.lblName.Name = "lblName";
            this.lblCode.AutoSize = true;
            this.lblCode.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCode.ForeColor = System.Drawing.Color.White;
            this.lblCode.Location = new System.Drawing.Point(16, 64);
            this.lblCode.Name = "lblCode";
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Location = new System.Drawing.Point(40, 14);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Tìm nhanh tên sách, tác giả hoặc ISBN...";
            this.txtSearch.Size = new System.Drawing.Size(380, 19);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.btnSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(21, 134, 200);
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(480, 6);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 32);
            this.btnSearch.Text = "Tìm sách";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            this.pnlSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.pnlSearch.BackColor = System.Drawing.Color.White;
            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Controls.Add(this.btnSearch);
            this.pnlSearch.Location = new System.Drawing.Point(500, 24);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(568, 44);
            this.pnlBanner.BackColor = System.Drawing.Color.FromArgb(19, 98, 160);
            this.pnlBanner.Controls.Add(this.lblHello);
            this.pnlBanner.Controls.Add(this.lblName);
            this.pnlBanner.Controls.Add(this.lblCode);
            this.pnlBanner.Controls.Add(this.pnlSearch);
            this.pnlBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBanner.Name = "pnlBanner";
            this.pnlBanner.Size = new System.Drawing.Size(1076, 92);
            //
            // pnlContent  (thứ tự Add: Fill trước, Top sau cùng nằm trên cùng)
            //
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.pnlContent.Controls.Add(this.pnlSuggestHost);
            this.pnlContent.Controls.Add(this.pnlMiddle);
            this.pnlContent.Controls.Add(this.pnlStatsHost);
            this.pnlContent.Controls.Add(this.pnlBanner);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(12, 0, 12, 12);
            //
            // HomePage
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlBreadcrumb);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(980, 640);
            this.Name = "HomePage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LIBRA - Cổng thông tin độc giả";
            this.pnlToolbar.ResumeLayout(false);
            this.pnlUser.ResumeLayout(false);
            this.pnlUser.PerformLayout();
            this.pnlBreadcrumb.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlSuggestHost.ResumeLayout(false);
            this.grpSuggest.ResumeLayout(false);
            this.pnlMiddle.ResumeLayout(false);
            this.grpBorrow.ResumeLayout(false);
            this.grpBorrow.PerformLayout();
            this.pnlNoticeHost.ResumeLayout(false);
            this.grpNotice.ResumeLayout(false);
            this.pnlStatsHost.ResumeLayout(false);
            this.pnlBanner.ResumeLayout(false);
            this.pnlBanner.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
 
        #endregion
 
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.Panel pnlUser;
        private System.Windows.Forms.Panel picAvatar;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblUserCode;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel pnlBreadcrumb;
        private System.Windows.Forms.Label lblBreadcrumb;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlSuggestHost;
        private System.Windows.Forms.GroupBox grpSuggest;
        private System.Windows.Forms.FlowLayoutPanel flowSuggest;
        private System.Windows.Forms.Panel pnlMiddle;
        private System.Windows.Forms.GroupBox grpBorrow;
        private System.Windows.Forms.FlowLayoutPanel flowBorrow;
        private System.Windows.Forms.LinkLabel lnkAllBorrow;
        private System.Windows.Forms.Panel pnlNoticeHost;
        private System.Windows.Forms.GroupBox grpNotice;
        private System.Windows.Forms.FlowLayoutPanel flowNotice;
        private System.Windows.Forms.Panel pnlStatsHost;
        private System.Windows.Forms.TableLayoutPanel tblStats;
        private System.Windows.Forms.Panel pnlBanner;
        private System.Windows.Forms.Label lblHello;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
    }
}