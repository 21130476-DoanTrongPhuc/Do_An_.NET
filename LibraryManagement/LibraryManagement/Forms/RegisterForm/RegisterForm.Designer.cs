using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms;

partial class RegisterForm
{
    private IContainer components = null;

    private Panel cardPanel;
    private Label lblLogo;
    private Label lblTitle;
    private Label lblSubtitle;
    private Label lblFullName;
    private TextBox txtFullName;
    private Label lblEmail;
    private TextBox txtEmail;
    private Label lblPhone;
    private TextBox txtPhone;
    private Label lblPassword;
    private TextBox txtPassword;
    private Label lblConfirmPassword;
    private TextBox txtConfirmPassword;
    private Button btnRegister;
    private Button btnBack;

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
        lblSubtitle = new Label();
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
        cardPanel.SuspendLayout();
        SuspendLayout();

        cardPanel.BackColor = Color.FromArgb(30, 30, 30);
        cardPanel.Controls.Add(lblLogo);
        cardPanel.Controls.Add(lblTitle);
        cardPanel.Controls.Add(lblSubtitle);
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
        cardPanel.Location = new Point(170, 30);
        cardPanel.Name = "cardPanel";
        cardPanel.Size = new Size(460, 620);
        cardPanel.TabIndex = 0;

        lblLogo.AutoSize = true;
        lblLogo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblLogo.ForeColor = Color.FromArgb(255, 168, 0);
        lblLogo.Location = new Point(30, 20);
        lblLogo.Name = "lblLogo";
        lblLogo.Size = new Size(123, 28);
        lblLogo.Text = "📖 LIBRA";

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(110, 55);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(240, 41);
        lblTitle.Text = "Đăng ký tài khoản";

        lblSubtitle.Font = new Font("Segoe UI", 9F);
        lblSubtitle.ForeColor = Color.FromArgb(170, 170, 170);
        lblSubtitle.Location = new Point(30, 98);
        lblSubtitle.Size = new Size(400, 25);
        lblSubtitle.Text = "Tạo thẻ độc giả trực tuyến để mượn và tra cứu sách.";
        lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

        lblFullName.AutoSize = true;
        lblFullName.ForeColor = Color.FromArgb(200, 200, 200);
        lblFullName.Location = new Point(30, 135);
        lblFullName.Name = "lblFullName";
        lblFullName.Size = new Size(80, 20);
        lblFullName.Text = "Họ và tên";

        txtFullName.BackColor = Color.FromArgb(45, 45, 45);
        txtFullName.BorderStyle = BorderStyle.FixedSingle;
        txtFullName.Font = new Font("Segoe UI", 11F);
        txtFullName.ForeColor = Color.White;
        txtFullName.Location = new Point(30, 158);
        txtFullName.Name = "txtFullName";
        txtFullName.Size = new Size(400, 32);

        lblEmail.AutoSize = true;
        lblEmail.ForeColor = Color.FromArgb(200, 200, 200);
        lblEmail.Location = new Point(30, 200);
        lblEmail.Name = "lblEmail";
        lblEmail.Size = new Size(46, 20);
        lblEmail.Text = "Email";

        txtEmail.BackColor = Color.FromArgb(45, 45, 45);
        txtEmail.BorderStyle = BorderStyle.FixedSingle;
        txtEmail.Font = new Font("Segoe UI", 11F);
        txtEmail.ForeColor = Color.White;
        txtEmail.Location = new Point(30, 223);
        txtEmail.Name = "txtEmail";
        txtEmail.Size = new Size(400, 32);

        lblPhone.AutoSize = true;
        lblPhone.ForeColor = Color.FromArgb(200, 200, 200);
        lblPhone.Location = new Point(30, 265);
        lblPhone.Name = "lblPhone";
        lblPhone.Size = new Size(98, 20);
        lblPhone.Text = "Số điện thoại";

        txtPhone.BackColor = Color.FromArgb(45, 45, 45);
        txtPhone.BorderStyle = BorderStyle.FixedSingle;
        txtPhone.Font = new Font("Segoe UI", 11F);
        txtPhone.ForeColor = Color.White;
        txtPhone.Location = new Point(30, 288);
        txtPhone.Name = "txtPhone";
        txtPhone.Size = new Size(400, 32);

        lblPassword.AutoSize = true;
        lblPassword.ForeColor = Color.FromArgb(200, 200, 200);
        lblPassword.Location = new Point(30, 330);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(70, 20);
        lblPassword.Text = "Mật khẩu";

        txtPassword.BackColor = Color.FromArgb(45, 45, 45);
        txtPassword.BorderStyle = BorderStyle.FixedSingle;
        txtPassword.Font = new Font("Segoe UI", 11F);
        txtPassword.ForeColor = Color.White;
        txtPassword.Location = new Point(30, 353);
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new Size(400, 32);
        txtPassword.UseSystemPasswordChar = true;

        lblConfirmPassword.AutoSize = true;
        lblConfirmPassword.ForeColor = Color.FromArgb(200, 200, 200);
        lblConfirmPassword.Location = new Point(30, 395);
        lblConfirmPassword.Name = "lblConfirmPassword";
        lblConfirmPassword.Size = new Size(135, 20);
        lblConfirmPassword.Text = "Xác nhận mật khẩu";

        txtConfirmPassword.BackColor = Color.FromArgb(45, 45, 45);
        txtConfirmPassword.BorderStyle = BorderStyle.FixedSingle;
        txtConfirmPassword.Font = new Font("Segoe UI", 11F);
        txtConfirmPassword.ForeColor = Color.White;
        txtConfirmPassword.Location = new Point(30, 418);
        txtConfirmPassword.Name = "txtConfirmPassword";
        txtConfirmPassword.Size = new Size(400, 32);
        txtConfirmPassword.UseSystemPasswordChar = true;

        btnRegister.BackColor = Color.FromArgb(255, 168, 0);
        btnRegister.Cursor = Cursors.Hand;
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRegister.FlatStyle = FlatStyle.Flat;
        btnRegister.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnRegister.ForeColor = Color.Black;
        btnRegister.Location = new Point(30, 480);
        btnRegister.Name = "btnRegister";
        btnRegister.Size = new Size(400, 45);
        btnRegister.Text = "Hoàn tất đăng ký ➔";
        btnRegister.UseVisualStyleBackColor = false;
        btnRegister.Click += btnRegister_Click;

        btnBack.BackColor = Color.FromArgb(30, 30, 30);
        btnBack.Cursor = Cursors.Hand;
        btnBack.FlatAppearance.BorderColor = Color.FromArgb(255, 168, 0);
        btnBack.FlatAppearance.BorderSize = 1;
        btnBack.FlatStyle = FlatStyle.Flat;
        btnBack.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnBack.ForeColor = Color.FromArgb(255, 168, 0);
        btnBack.Location = new Point(30, 535);
        btnBack.Name = "btnBack";
        btnBack.Size = new Size(400, 45);
        btnBack.Text = "Quay lại đăng nhập";
        btnBack.UseVisualStyleBackColor = false;
        btnBack.Click += btnBack_Click;

        BackColor = Color.FromArgb(18, 18, 18);
        ClientSize = new Size(800, 680);
        Controls.Add(cardPanel);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LIBRA - Đăng ký độc giả";
        cardPanel.ResumeLayout(false);
        cardPanel.PerformLayout();
        ResumeLayout(false);
    }
}