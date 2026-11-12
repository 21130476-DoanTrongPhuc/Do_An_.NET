using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms;

partial class LoginForm
{
    private IContainer components = null;

    private System.Windows.Forms.Panel cardPanel;
    private Label lblLogo;
    private System.Windows.Forms.Label lblSubtitle;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblRoleTitle;
    private System.Windows.Forms.RadioButton rdoReader;
    private System.Windows.Forms.RadioButton rdoLibrarian;
    private Label lblUsernameTitle;
    private TextBox txtUsername;
    private Label lblPasswordTitle;
    private TextBox txtPassword;
    private CheckBox chkRemember;
    private LinkLabel lblForgotPass;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Button btnRegister;

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
        btnRegister = new System.Windows.Forms.Button();
        lblSubtitle = new System.Windows.Forms.Label();
        cardPanel.SuspendLayout();
        SuspendLayout();
        // 
        // cardPanel
        // 
        cardPanel.BackColor = System.Drawing.Color.FromArgb(((int)((byte)30)), ((int)((byte)30)), ((int)((byte)30)));
        cardPanel.Controls.Add(lblLogo);
        cardPanel.Controls.Add(lblTitle);
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
        cardPanel.Controls.Add(btnRegister);
        cardPanel.ForeColor = System.Drawing.SystemColors.Control;
        cardPanel.Location = new System.Drawing.Point(149, 40);
        cardPanel.Name = "cardPanel";
        cardPanel.Size = new System.Drawing.Size(492, 520);
        cardPanel.TabIndex = 0;
        // 
        // lblLogo
        // 
        lblLogo.AutoSize = true;
        lblLogo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblLogo.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)168)), ((int)((byte)0)));
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
        // lblRoleTitle
        // 
        lblRoleTitle.AutoSize = true;
        lblRoleTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)200)), ((int)((byte)200)), ((int)((byte)200)));
        lblRoleTitle.Location = new System.Drawing.Point(18, 150);
        lblRoleTitle.Name = "lblRoleTitle";
        lblRoleTitle.Size = new System.Drawing.Size(126, 25);
        lblRoleTitle.TabIndex = 3;
        lblRoleTitle.Text = "Loại tài khoản:";
        // 
        // rdoReader
        // 
        rdoReader.AutoSize = true;
        rdoReader.ForeColor = System.Drawing.Color.White;
        rdoReader.Location = new System.Drawing.Point(161, 150);
        rdoReader.Name = "rdoReader";
        rdoReader.Size = new System.Drawing.Size(180, 29);
        rdoReader.TabIndex = 4;
        rdoReader.Text = "Người mượn sách";
        rdoReader.CheckedChanged += rdoReader_CheckedChanged;
        // 
        // rdoLibrarian
        // 
        rdoLibrarian.AutoSize = true;
        rdoLibrarian.ForeColor = System.Drawing.Color.White;
        rdoLibrarian.Location = new System.Drawing.Point(357, 150);
        rdoLibrarian.Name = "rdoLibrarian";
        rdoLibrarian.Size = new System.Drawing.Size(98, 29);
        rdoLibrarian.TabIndex = 5;
        rdoLibrarian.Text = "Thủ thư";
        rdoLibrarian.CheckedChanged += rdoLibrarian_CheckedChanged;
        // 
        // lblUsernameTitle
        // 
        lblUsernameTitle.AutoSize = true;
        lblUsernameTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)200)), ((int)((byte)200)), ((int)((byte)200)));
        lblUsernameTitle.Location = new System.Drawing.Point(30, 190);
        lblUsernameTitle.Name = "lblUsernameTitle";
        lblUsernameTitle.Size = new System.Drawing.Size(191, 25);
        lblUsernameTitle.TabIndex = 6;
        lblUsernameTitle.Text = "Mã độc giả hoặc email";
        // 
        // txtUsername
        // 
        txtUsername.BackColor = System.Drawing.Color.FromArgb(((int)((byte)45)), ((int)((byte)45)), ((int)((byte)45)));
        txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtUsername.ForeColor = System.Drawing.Color.White;
        txtUsername.Location = new System.Drawing.Point(30, 215);
        txtUsername.Name = "txtUsername";
        txtUsername.Size = new System.Drawing.Size(400, 37);
        txtUsername.TabIndex = 7;
        // 
        // lblPasswordTitle
        // 
        lblPasswordTitle.AutoSize = true;
        lblPasswordTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)200)), ((int)((byte)200)), ((int)((byte)200)));
        lblPasswordTitle.Location = new System.Drawing.Point(30, 260);
        lblPasswordTitle.Name = "lblPasswordTitle";
        lblPasswordTitle.Size = new System.Drawing.Size(86, 25);
        lblPasswordTitle.TabIndex = 8;
        lblPasswordTitle.Text = "Mật khẩu";
        // 
        // txtPassword
        // 
        txtPassword.BackColor = System.Drawing.Color.FromArgb(((int)((byte)45)), ((int)((byte)45)), ((int)((byte)45)));
        txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtPassword.ForeColor = System.Drawing.Color.White;
        txtPassword.Location = new System.Drawing.Point(30, 285);
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new System.Drawing.Size(400, 37);
        txtPassword.TabIndex = 9;
        txtPassword.UseSystemPasswordChar = true;
        // 
        // chkRemember
        // 
        chkRemember.AutoSize = true;
        chkRemember.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)170)), ((int)((byte)170)), ((int)((byte)170)));
        chkRemember.Location = new System.Drawing.Point(30, 335);
        chkRemember.Name = "chkRemember";
        chkRemember.Size = new System.Drawing.Size(191, 29);
        chkRemember.TabIndex = 10;
        chkRemember.Text = "Ghi nhớ đăng nhập";
        // 
        // lblForgotPass
        // 
        lblForgotPass.AutoSize = true;
        lblForgotPass.LinkColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)168)), ((int)((byte)0)));
        lblForgotPass.Location = new System.Drawing.Point(313, 336);
        lblForgotPass.Name = "lblForgotPass";
        lblForgotPass.Size = new System.Drawing.Size(142, 25);
        lblForgotPass.TabIndex = 11;
        lblForgotPass.TabStop = true;
        lblForgotPass.Text = "Quên mật khẩu?";
        // 
        // btnLogin
        // 
        btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)168)), ((int)((byte)0)));
        btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnLogin.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnLogin.ForeColor = System.Drawing.Color.Black;
        btnLogin.Location = new System.Drawing.Point(30, 390);
        btnLogin.Name = "btnLogin";
        btnLogin.Size = new System.Drawing.Size(400, 45);
        btnLogin.TabIndex = 12;
        btnLogin.Text = "Đăng nhập ➔";
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.Click += btnLogin_Click;
        // 
        // btnRegister
        // 
        btnRegister.BackColor = System.Drawing.Color.FromArgb(((int)((byte)30)), ((int)((byte)30)), ((int)((byte)30)));
        btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
        btnRegister.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)168)), ((int)((byte)0)));
        btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnRegister.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnRegister.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)168)), ((int)((byte)0)));
        btnRegister.Location = new System.Drawing.Point(30, 445);
        btnRegister.Name = "btnRegister";
        btnRegister.Size = new System.Drawing.Size(400, 45);
        btnRegister.TabIndex = 13;
        btnRegister.Text = "Đăng ký tài khoản";
        btnRegister.UseVisualStyleBackColor = false;
        btnRegister.Click += btnRegister_Click;
        // 
        // lblSubtitle
        // 
        lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)170)), ((int)((byte)170)), ((int)((byte)170)));
        lblSubtitle.Location = new System.Drawing.Point(192, 566);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new System.Drawing.Size(438, 25);
        lblSubtitle.TabIndex = 2;
        lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // LoginForm
        // 
        BackColor = System.Drawing.Color.FromArgb(((int)((byte)18)), ((int)((byte)18)), ((int)((byte)18)));
        ClientSize = new System.Drawing.Size(800, 600);
        Controls.Add(cardPanel);
        Controls.Add(lblSubtitle);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "LIBRA - Đăng nhập hệ thống";
        Load += LoginForm_Load;
        cardPanel.ResumeLayout(false);
        cardPanel.PerformLayout();
        ResumeLayout(false);
    }
}