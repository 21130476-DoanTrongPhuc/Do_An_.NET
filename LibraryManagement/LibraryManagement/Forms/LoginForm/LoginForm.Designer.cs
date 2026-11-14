#nullable enable

using System.ComponentModel;

namespace LibraryManagement.Forms;

partial class LoginForm
{
    private IContainer? components = null;

    private System.Windows.Forms.Panel cardPanel = null!;
    private System.Windows.Forms.Label lblLogo = null!;
    private System.Windows.Forms.Label lblSubtitle = null!;
    private System.Windows.Forms.Label lblTitle = null!;
    private System.Windows.Forms.Label lblRoleTitle = null!;
    private System.Windows.Forms.RadioButton rdoReader = null!;
    private System.Windows.Forms.RadioButton rdoLibrarian = null!;
    private System.Windows.Forms.Label lblUsernameTitle = null!;
    private System.Windows.Forms.TextBox txtUsername = null!;
    private System.Windows.Forms.Label lblPasswordTitle = null!;
    private System.Windows.Forms.TextBox txtPassword = null!;
    private System.Windows.Forms.CheckBox chkRemember = null!;
    private System.Windows.Forms.LinkLabel lblForgotPass = null!;
    private System.Windows.Forms.Button btnLogin = null!;
    private System.Windows.Forms.Button btnRegister = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
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
        cardPanel = new System.Windows.Forms.Panel();
        lblLogo = new System.Windows.Forms.Label();
        lblTitle = new System.Windows.Forms.Label();
        btnRegister = new System.Windows.Forms.Button();
        lblRoleTitle = new System.Windows.Forms.Label();
        rdoReader = new System.Windows.Forms.RadioButton();
        rdoLibrarian = new System.Windows.Forms.RadioButton();
        lblUsernameTitle = new System.Windows.Forms.Label();
        txtUsername = new System.Windows.Forms.TextBox();
        lblPasswordTitle = new System.Windows.Forms.Label();
        txtPassword = new System.Windows.Forms.TextBox();
        chkRemember = new System.Windows.Forms.CheckBox();
        lblForgotPass = new System.Windows.Forms.LinkLabel();
        btnLogin = new System.Windows.Forms.Button();
        lblSubtitle = new System.Windows.Forms.Label();
        cardPanel.SuspendLayout();
        SuspendLayout();
        // 
        // cardPanel
        // 
        cardPanel.BackColor = System.Drawing.Color.White;
        cardPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        cardPanel.Controls.Add(lblLogo);
        cardPanel.Controls.Add(lblTitle);
        cardPanel.Controls.Add(btnRegister);
        cardPanel.Controls.Add(lblRoleTitle);
        cardPanel.Controls.Add(rdoReader);
        cardPanel.Controls.Add(rdoLibrarian);
        cardPanel.Controls.Add(lblUsernameTitle);
        cardPanel.Controls.Add(txtUsername);
        cardPanel.Controls.Add(lblPasswordTitle);
        cardPanel.Controls.Add(txtPassword);
        cardPanel.Controls.Add(chkRemember);
        cardPanel.Controls.Add(lblForgotPass);
        cardPanel.Controls.Add(btnLogin);
        cardPanel.ForeColor = System.Drawing.Color.Black;
        cardPanel.Location = new System.Drawing.Point(157, 25);
        cardPanel.Name = "cardPanel";
        cardPanel.Size = new System.Drawing.Size(484, 590);
        cardPanel.TabIndex = 0;
        // 
        // lblLogo
        // 
        lblLogo.AutoSize = true;
        lblLogo.BackColor = System.Drawing.Color.White;
        lblLogo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblLogo.ForeColor = System.Drawing.Color.Black;
        lblLogo.Location = new System.Drawing.Point(30, 25);
        lblLogo.Name = "lblLogo";
        lblLogo.Size = new System.Drawing.Size(123, 32);
        lblLogo.TabIndex = 0;
        lblLogo.Text = "📖 LIBRA";
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.BackColor = System.Drawing.Color.White;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.Black;
        lblTitle.Location = new System.Drawing.Point(150, 65);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(205, 48);
        lblTitle.TabIndex = 1;
        lblTitle.Text = "Đăng nhập";
        // 
        // btnRegister
        // 
        btnRegister.BackColor = System.Drawing.Color.White;
        btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
        btnRegister.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        btnRegister.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnRegister.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnRegister.ForeColor = System.Drawing.Color.Black;
        btnRegister.Location = new System.Drawing.Point(30, 474);
        btnRegister.Name = "btnRegister";
        btnRegister.Size = new System.Drawing.Size(400, 45);
        btnRegister.TabIndex = 13;
        btnRegister.Text = "Đăng ký tài khoản";
        btnRegister.UseVisualStyleBackColor = false;
        btnRegister.Click += btnRegister_Click;
        // 
        // lblRoleTitle
        // 
        lblRoleTitle.AutoSize = true;
        lblRoleTitle.BackColor = System.Drawing.Color.White;
        lblRoleTitle.ForeColor = System.Drawing.Color.Black;
        lblRoleTitle.Location = new System.Drawing.Point(25, 150);
        lblRoleTitle.Name = "lblRoleTitle";
        lblRoleTitle.Size = new System.Drawing.Size(126, 25);
        lblRoleTitle.TabIndex = 3;
        lblRoleTitle.Text = "Loại tài khoản:";
        // 
        // rdoReader
        // 
        rdoReader.AutoSize = true;
        rdoReader.BackColor = System.Drawing.Color.White;
        rdoReader.ForeColor = System.Drawing.Color.Black;
        rdoReader.Location = new System.Drawing.Point(161, 148);
        rdoReader.Name = "rdoReader";
        rdoReader.Size = new System.Drawing.Size(180, 29);
        rdoReader.TabIndex = 4;
        rdoReader.Text = "Người mượn sách";
        rdoReader.UseVisualStyleBackColor = false;
        rdoReader.CheckedChanged += rdoReader_CheckedChanged;
        // 
        // rdoLibrarian
        // 
        rdoLibrarian.AutoSize = true;
        rdoLibrarian.BackColor = System.Drawing.Color.White;
        rdoLibrarian.ForeColor = System.Drawing.Color.Black;
        rdoLibrarian.Location = new System.Drawing.Point(352, 148);
        rdoLibrarian.Name = "rdoLibrarian";
        rdoLibrarian.Size = new System.Drawing.Size(98, 29);
        rdoLibrarian.TabIndex = 5;
        rdoLibrarian.Text = "Thủ thư";
        rdoLibrarian.UseVisualStyleBackColor = false;
        rdoLibrarian.CheckedChanged += rdoLibrarian_CheckedChanged;
        // 
        // lblUsernameTitle
        // 
        lblUsernameTitle.AutoSize = true;
        lblUsernameTitle.BackColor = System.Drawing.Color.White;
        lblUsernameTitle.ForeColor = System.Drawing.Color.Black;
        lblUsernameTitle.Location = new System.Drawing.Point(30, 190);
        lblUsernameTitle.Name = "lblUsernameTitle";
        lblUsernameTitle.Size = new System.Drawing.Size(191, 25);
        lblUsernameTitle.TabIndex = 6;
        lblUsernameTitle.Text = "Mã độc giả hoặc email";
        // 
        // txtUsername
        // 
        txtUsername.BackColor = System.Drawing.Color.White;
        txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtUsername.ForeColor = System.Drawing.Color.Black;
        txtUsername.Location = new System.Drawing.Point(30, 220);
        txtUsername.Name = "txtUsername";
        txtUsername.Size = new System.Drawing.Size(400, 37);
        txtUsername.TabIndex = 7;
        // 
        // lblPasswordTitle
        // 
        lblPasswordTitle.AutoSize = true;
        lblPasswordTitle.BackColor = System.Drawing.Color.White;
        lblPasswordTitle.ForeColor = System.Drawing.Color.Black;
        lblPasswordTitle.Location = new System.Drawing.Point(30, 265);
        lblPasswordTitle.Name = "lblPasswordTitle";
        lblPasswordTitle.Size = new System.Drawing.Size(86, 25);
        lblPasswordTitle.TabIndex = 8;
        lblPasswordTitle.Text = "Mật khẩu";
        // 
        // txtPassword
        // 
        txtPassword.BackColor = System.Drawing.Color.White;
        txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtPassword.ForeColor = System.Drawing.Color.Black;
        txtPassword.Location = new System.Drawing.Point(30, 295);
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new System.Drawing.Size(400, 37);
        txtPassword.TabIndex = 9;
        txtPassword.UseSystemPasswordChar = true;
        // 
        // chkRemember
        // 
        chkRemember.AutoSize = true;
        chkRemember.BackColor = System.Drawing.Color.White;
        chkRemember.ForeColor = System.Drawing.Color.Black;
        chkRemember.Location = new System.Drawing.Point(30, 342);
        chkRemember.Name = "chkRemember";
        chkRemember.Size = new System.Drawing.Size(191, 29);
        chkRemember.TabIndex = 10;
        chkRemember.Text = "Ghi nhớ đăng nhập";
        chkRemember.UseVisualStyleBackColor = false;
        // 
        // lblForgotPass
        // 
        lblForgotPass.ActiveLinkColor = System.Drawing.Color.DimGray;
        lblForgotPass.AutoSize = true;
        lblForgotPass.BackColor = System.Drawing.Color.White;
        lblForgotPass.LinkColor = System.Drawing.Color.Black;
        lblForgotPass.Location = new System.Drawing.Point(288, 344);
        lblForgotPass.Name = "lblForgotPass";
        lblForgotPass.Size = new System.Drawing.Size(142, 25);
        lblForgotPass.TabIndex = 11;
        lblForgotPass.TabStop = true;
        lblForgotPass.Text = "Quên mật khẩu?";
        lblForgotPass.VisitedLinkColor = System.Drawing.Color.Black;
        lblForgotPass.LinkClicked += lblForgotPass_LinkClicked;
        // 
        // btnLogin
        // 
        btnLogin.BackColor = System.Drawing.Color.White;
        btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
        btnLogin.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnLogin.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnLogin.ForeColor = System.Drawing.Color.Black;
        btnLogin.Location = new System.Drawing.Point(30, 402);
        btnLogin.Name = "btnLogin";
        btnLogin.Size = new System.Drawing.Size(400, 45);
        btnLogin.TabIndex = 12;
        btnLogin.Text = "Đăng nhập ➔";
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.Click += btnLogin_Click;
        // 
        // lblSubtitle
        // 
        lblSubtitle.BackColor = System.Drawing.Color.White;
        lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = System.Drawing.Color.Black;
        lblSubtitle.Location = new System.Drawing.Point(180, 625);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new System.Drawing.Size(438, 25);
        lblSubtitle.TabIndex = 2;
        lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // LoginForm
        // 
        AcceptButton = btnLogin;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(800, 667);
        Controls.Add(cardPanel);
        Controls.Add(lblSubtitle);
        ForeColor = System.Drawing.Color.Black;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "LIBRA - Đăng nhập hệ thống";
        Load += LoginForm_Load;
        cardPanel.ResumeLayout(false);
        cardPanel.PerformLayout();
        ResumeLayout(false);
    }
}