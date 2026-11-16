#nullable enable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms.RegisterForm;

partial class RegisterForm
{
    private IContainer? components = null;

    private Panel cardPanel = null!;
    private Label lblLogo = null!;
    private Label lblTitle = null!;
    private Label lblFullName = null!;
    private TextBox txtFullName = null!;
    private Label lblEmail = null!;
    private TextBox txtEmail = null!;
    private Label lblPhone = null!;
    private TextBox txtPhone = null!;
    private Label lblPassword = null!;
    private TextBox txtPassword = null!;
    private Label lblConfirmPassword = null!;
    private TextBox txtConfirmPassword = null!;
    private Button btnRegister = null!;
    private Button btnBack = null!;
    private Label lblSubtitle = null!;

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
        cardPanel = new Panel();
        lblLogo = new Label();
        lblTitle = new Label();
        lblFullName = new Label();
        txtFullName = new TextBox();
        lblEmail = new Label();
        txtEmail = new TextBox();
        lblPhone = new Label();
        txtPhone = new TextBox();
        lblPassword = new Label();
        txtPassword = new TextBox();
        lblConfirmPassword = new Label();
        txtConfirmPassword = new TextBox();
        btnRegister = new Button();
        btnBack = new Button();
        lblSubtitle = new Label();
        cardPanel.SuspendLayout();
        SuspendLayout();

        // cardPanel
        cardPanel.BackColor = Color.White;
        cardPanel.BorderStyle = BorderStyle.FixedSingle;
        cardPanel.Controls.Add(lblLogo);
        cardPanel.Controls.Add(lblTitle);
        cardPanel.Controls.Add(lblFullName);
        cardPanel.Controls.Add(txtFullName);
        cardPanel.Controls.Add(lblEmail);
        cardPanel.Controls.Add(txtEmail);
        cardPanel.Controls.Add(lblPhone);
        cardPanel.Controls.Add(txtPhone);
        cardPanel.Controls.Add(lblPassword);
        cardPanel.Controls.Add(txtPassword);
        cardPanel.Controls.Add(lblConfirmPassword);
        cardPanel.Controls.Add(txtConfirmPassword);
        cardPanel.Controls.Add(btnRegister);
        cardPanel.Controls.Add(btnBack);
        cardPanel.ForeColor = Color.Black;
        cardPanel.Location = new Point(157, 25);
        cardPanel.Name = "cardPanel";
        cardPanel.Size = new Size(484, 590);
        cardPanel.TabIndex = 0;

        // lblLogo
        lblLogo.AutoSize = true;
        lblLogo.BackColor = Color.White;
        lblLogo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblLogo.ForeColor = Color.Black;
        lblLogo.Location = new Point(30, 20);
        lblLogo.Name = "lblLogo";
        lblLogo.Size = new Size(123, 32);
        lblLogo.TabIndex = 0;
        lblLogo.Text = "📖 LIBRA";

        // lblTitle
        lblTitle.AutoSize = true;
        lblTitle.BackColor = Color.White;
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.ForeColor = Color.Black;
        lblTitle.Location = new Point(165, 52);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(165, 48);
        lblTitle.TabIndex = 1;
        lblTitle.Text = "Đăng ký";

        // lblFullName
        lblFullName.AutoSize = true;
        lblFullName.BackColor = Color.White;
        lblFullName.ForeColor = Color.Black;
        lblFullName.Location = new Point(30, 102);
        lblFullName.Name = "lblFullName";
        lblFullName.Size = new Size(85, 25);
        lblFullName.TabIndex = 2;
        lblFullName.Text = "Họ và tên";

        // txtFullName
        txtFullName.BackColor = Color.White;
        txtFullName.BorderStyle = BorderStyle.FixedSingle;
        txtFullName.Font = new Font("Segoe UI", 11F);
        txtFullName.ForeColor = Color.Black;
        txtFullName.Location = new Point(30, 126);
        txtFullName.Name = "txtFullName";
        txtFullName.Size = new Size(400, 32);
        txtFullName.TabIndex = 3;

        // lblEmail
        lblEmail.AutoSize = true;
        lblEmail.BackColor = Color.White;
        lblEmail.ForeColor = Color.Black;
        lblEmail.Location = new Point(30, 165);
        lblEmail.Name = "lblEmail";
        lblEmail.Size = new Size(51, 25);
        lblEmail.TabIndex = 4;
        lblEmail.Text = "Email";

        // txtEmail
        txtEmail.BackColor = Color.White;
        txtEmail.BorderStyle = BorderStyle.FixedSingle;
        txtEmail.Font = new Font("Segoe UI", 11F);
        txtEmail.ForeColor = Color.Black;
        txtEmail.Location = new Point(30, 189);
        txtEmail.Name = "txtEmail";
        txtEmail.Size = new Size(400, 32);
        txtEmail.TabIndex = 5;

        // lblPhone
        lblPhone.AutoSize = true;
        lblPhone.BackColor = Color.White;
        lblPhone.ForeColor = Color.Black;
        lblPhone.Location = new Point(30, 228);
        lblPhone.Name = "lblPhone";
        lblPhone.Size = new Size(105, 25);
        lblPhone.TabIndex = 6;
        lblPhone.Text = "Số điện thoại";

        // txtPhone
        txtPhone.BackColor = Color.White;
        txtPhone.BorderStyle = BorderStyle.FixedSingle;
        txtPhone.Font = new Font("Segoe UI", 11F);
        txtPhone.ForeColor = Color.Black;
        txtPhone.Location = new Point(30, 252);
        txtPhone.Name = "txtPhone";
        txtPhone.Size = new Size(400, 32);
        txtPhone.TabIndex = 7;

        // lblPassword
        lblPassword.AutoSize = true;
        lblPassword.BackColor = Color.White;
        lblPassword.ForeColor = Color.Black;
        lblPassword.Location = new Point(30, 291);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(86, 25);
        lblPassword.TabIndex = 8;
        lblPassword.Text = "Mật khẩu";

        // txtPassword
        txtPassword.BackColor = Color.White;
        txtPassword.BorderStyle = BorderStyle.FixedSingle;
        txtPassword.Font = new Font("Segoe UI", 11F);
        txtPassword.ForeColor = Color.Black;
        txtPassword.Location = new Point(30, 315);
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new Size(400, 32);
        txtPassword.TabIndex = 9;
        txtPassword.UseSystemPasswordChar = true;

        // lblConfirmPassword
        lblConfirmPassword.AutoSize = true;
        lblConfirmPassword.BackColor = Color.White;
        lblConfirmPassword.ForeColor = Color.Black;
        lblConfirmPassword.Location = new Point(30, 354);
        lblConfirmPassword.Name = "lblConfirmPassword";
        lblConfirmPassword.Size = new Size(154, 25);
        lblConfirmPassword.TabIndex = 10;
        lblConfirmPassword.Text = "Xác nhận mật khẩu";

        // txtConfirmPassword
        txtConfirmPassword.BackColor = Color.White;
        txtConfirmPassword.BorderStyle = BorderStyle.FixedSingle;
        txtConfirmPassword.Font = new Font("Segoe UI", 11F);
        txtConfirmPassword.ForeColor = Color.Black;
        txtConfirmPassword.Location = new Point(30, 378);
        txtConfirmPassword.Name = "txtConfirmPassword";
        txtConfirmPassword.Size = new Size(400, 32);
        txtConfirmPassword.TabIndex = 11;
        txtConfirmPassword.UseSystemPasswordChar = true;

        // btnRegister
        btnRegister.BackColor = Color.White;
        btnRegister.Cursor = Cursors.Hand;
        btnRegister.FlatAppearance.BorderColor = Color.Black;
        btnRegister.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnRegister.FlatStyle = FlatStyle.Flat;
        btnRegister.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnRegister.ForeColor = Color.Black;
        btnRegister.Location = new Point(30, 428);
        btnRegister.Name = "btnRegister";
        btnRegister.Size = new Size(400, 44);
        btnRegister.TabIndex = 12;
        btnRegister.Text = "Đăng ký ➔";
        btnRegister.UseVisualStyleBackColor = false;
        btnRegister.Click += btnRegister_Click;

        // btnBack
        btnBack.BackColor = Color.White;
        btnBack.Cursor = Cursors.Hand;
        btnBack.FlatAppearance.BorderColor = Color.Black;
        btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnBack.FlatStyle = FlatStyle.Flat;
        btnBack.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnBack.ForeColor = Color.Black;
        btnBack.Location = new Point(30, 485);
        btnBack.Name = "btnBack";
        btnBack.Size = new Size(400, 44);
        btnBack.TabIndex = 13;
        btnBack.Text = "Quay lại đăng nhập";
        btnBack.UseVisualStyleBackColor = false;
        btnBack.Click += btnBack_Click;

        // lblSubtitle
        lblSubtitle.BackColor = Color.White;
        lblSubtitle.Font = new Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = Color.Black;
        lblSubtitle.Location = new Point(180, 625);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(438, 25);
        lblSubtitle.TabIndex = 2;
        lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

        // RegisterForm
        BackColor = Color.White;
        ClientSize = new Size(800, 667);
        Controls.Add(cardPanel);
        Controls.Add(lblSubtitle);
        ForeColor = Color.Black;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LIBRA - Đăng ký tài khoản";
        cardPanel.ResumeLayout(false);
        cardPanel.PerformLayout();
        ResumeLayout(false);
    }
}