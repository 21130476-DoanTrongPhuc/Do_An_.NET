using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms.LoginForm;

partial class LoginForm
{
    private IContainer components = null;

    private System.Windows.Forms.Panel cardPanel;
    private Label lblLogo;
    private Label lblSubtitle;
    private Label lblTitle;
    private System.Windows.Forms.Label lblRoleTitle;
    private RadioButton rdoReader;
    private System.Windows.Forms.RadioButton rdoLibrarian;
    private Label lblUsernameTitle;
    private System.Windows.Forms.TextBox txtUsername;
    private Label lblPasswordTitle;
    private TextBox txtPassword;
    private CheckBox chkRemember;
    private System.Windows.Forms.LinkLabel lblForgotPass;
    private Button btnLogin;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        cardPanel = new System.Windows.Forms.Panel();
        lblLogo = new System.Windows.Forms.Label();
        lblTitle = new System.Windows.Forms.Label();
        lblSubtitle = new System.Windows.Forms.Label();
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
        cardPanel.SuspendLayout();
        SuspendLayout();
        
        cardPanel.BackColor = System.Drawing.Color.FromArgb(((int)((byte)22)), ((int)((byte)27)), ((int)((byte)34)));
        cardPanel.Controls.Add(lblLogo);
        cardPanel.Controls.Add(lblTitle);
        cardPanel.Controls.Add(lblSubtitle);
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
        cardPanel.Location = new System.Drawing.Point(115, 39);
        cardPanel.Name = "cardPanel";
        cardPanel.Size = new System.Drawing.Size(484, 507);
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
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.White;
        lblTitle.Location = new System.Drawing.Point(30, 65);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(368, 48);
        lblTitle.TabIndex = 1;
        lblTitle.Text = "Đăng nhập hệ thống";
        // 
        // lblSubtitle
        // 
        lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)140)), ((int)((byte)150)), ((int)((byte)165)));
        lblSubtitle.Location = new System.Drawing.Point(30, 105);
        lblSubtitle.MaximumSize = new System.Drawing.Size(400, 0);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new System.Drawing.Size(100, 0);
        lblSubtitle.TabIndex = 2;
        lblSubtitle.Text = "Tra cứu, quản lý và kết nối tri thức trong một hệ thống.";
        // 
        // lblRoleTitle
        // 
        lblRoleTitle.AutoSize = true;
        lblRoleTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)180)), ((int)((byte)190)), ((int)((byte)200)));
        lblRoleTitle.Location = new System.Drawing.Point(8, 147);
        lblRoleTitle.Name = "lblRoleTitle";
        lblRoleTitle.Size = new System.Drawing.Size(126, 25);
        lblRoleTitle.TabIndex = 3;
        lblRoleTitle.Text = "Loại tài khoản:";
        // 
        // rdoReader
        // 
        rdoReader.AutoSize = true;
        rdoReader.ForeColor = System.Drawing.Color.White;
        rdoReader.Location = new System.Drawing.Point(140, 143);
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
        rdoLibrarian.Location = new System.Drawing.Point(346, 143);
        rdoLibrarian.Name = "rdoLibrarian";
        rdoLibrarian.Size = new System.Drawing.Size(98, 29);
        rdoLibrarian.TabIndex = 5;
        rdoLibrarian.Text = "Thủ thư";
        rdoLibrarian.CheckedChanged += rdoLibrarian_CheckedChanged;
        // 
        // lblUsernameTitle
        // 
        lblUsernameTitle.AutoSize = true;
        lblUsernameTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)180)), ((int)((byte)190)), ((int)((byte)200)));
        lblUsernameTitle.Location = new System.Drawing.Point(30, 190);
        lblUsernameTitle.Name = "lblUsernameTitle";
        lblUsernameTitle.Size = new System.Drawing.Size(191, 25);
        lblUsernameTitle.TabIndex = 6;
        lblUsernameTitle.Text = "Mã độc giả hoặc email";
        // 
        // txtUsername
        // 
        txtUsername.BackColor = System.Drawing.Color.FromArgb(((int)((byte)32)), ((int)((byte)38)), ((int)((byte)46)));
        txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtUsername.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtUsername.ForeColor = System.Drawing.Color.White;
        txtUsername.Location = new System.Drawing.Point(30, 218);
        txtUsername.Name = "txtUsername";
        txtUsername.Size = new System.Drawing.Size(400, 34);
        txtUsername.TabIndex = 7;
        // 
        // lblPasswordTitle
        // 
        lblPasswordTitle.AutoSize = true;
        lblPasswordTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)180)), ((int)((byte)190)), ((int)((byte)200)));
        lblPasswordTitle.Location = new System.Drawing.Point(30, 265);
        lblPasswordTitle.Name = "lblPasswordTitle";
        lblPasswordTitle.Size = new System.Drawing.Size(86, 25);
        lblPasswordTitle.TabIndex = 8;
        lblPasswordTitle.Text = "Mật khẩu";
        // 
        // txtPassword
        // 
        txtPassword.BackColor = System.Drawing.Color.FromArgb(((int)((byte)32)), ((int)((byte)38)), ((int)((byte)46)));
        txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtPassword.ForeColor = System.Drawing.Color.White;
        txtPassword.Location = new System.Drawing.Point(30, 290);
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new System.Drawing.Size(400, 34);
        txtPassword.TabIndex = 9;
        txtPassword.UseSystemPasswordChar = true;
        // 
        // chkRemember
        // 
        chkRemember.AutoSize = true;
        chkRemember.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)140)), ((int)((byte)150)), ((int)((byte)165)));
        chkRemember.Location = new System.Drawing.Point(30, 340);
        chkRemember.Name = "chkRemember";
        chkRemember.Size = new System.Drawing.Size(191, 29);
        chkRemember.TabIndex = 10;
        chkRemember.Text = "Ghi nhớ đăng nhập";
        // 
        // lblForgotPass
        // 
        lblForgotPass.AutoSize = true;
        lblForgotPass.LinkColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)168)), ((int)((byte)0)));
        lblForgotPass.Location = new System.Drawing.Point(288, 344);
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
        btnLogin.Size = new System.Drawing.Size(400, 42);
        btnLogin.TabIndex = 12;
        btnLogin.Text = "Đăng nhập ➔";
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.Click += btnLogin_Click;
        // 
        // LoginForm
        // 
        BackColor = System.Drawing.Color.FromArgb(((int)((byte)18)), ((int)((byte)20)), ((int)((byte)24)));
        ClientSize = new System.Drawing.Size(800, 600);
        Controls.Add(cardPanel);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "LIBRA - Đăng nhập hệ thống";
        Load += LoginForm_Load;
        cardPanel.ResumeLayout(false);
        cardPanel.PerformLayout();
        ResumeLayout(false);
    }
}