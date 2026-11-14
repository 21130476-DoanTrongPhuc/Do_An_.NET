#nullable enable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms.ForgotPasswordForm;

partial class ForgotPasswordForm
{
    private IContainer? components = null;

    private Panel cardPanel = null!;
    private Label lblLogo = null!;
    private Label lblTitle = null!;
    private Label lblIdentifier = null!;
    private TextBox txtIdentifier = null!;
    private Button btnSendOtp = null!;
    private Label lblOtp = null!;
    private TextBox txtOtp = null!;
    private Label lblNewPassword = null!;
    private TextBox txtNewPassword = null!;
    private Label lblConfirmPassword = null!;
    private TextBox txtConfirmPassword = null!;
    private Button btnResetPassword = null!;
    private Button btnBack = null!;

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
        lblIdentifier = new Label();
        txtIdentifier = new TextBox();
        btnSendOtp = new Button();
        lblOtp = new Label();
        txtOtp = new TextBox();
        lblNewPassword = new Label();
        txtNewPassword = new TextBox();
        lblConfirmPassword = new Label();
        txtConfirmPassword = new TextBox();
        btnResetPassword = new Button();
        btnBack = new Button();
        cardPanel.SuspendLayout();
        SuspendLayout();

        // cardPanel
        cardPanel.BackColor = Color.White;
        cardPanel.BorderStyle = BorderStyle.FixedSingle;
        cardPanel.Controls.Add(lblLogo);
        cardPanel.Controls.Add(lblTitle);
        cardPanel.Controls.Add(lblIdentifier);
        cardPanel.Controls.Add(txtIdentifier);
        cardPanel.Controls.Add(btnSendOtp);
        cardPanel.Controls.Add(lblOtp);
        cardPanel.Controls.Add(txtOtp);
        cardPanel.Controls.Add(lblNewPassword);
        cardPanel.Controls.Add(txtNewPassword);
        cardPanel.Controls.Add(lblConfirmPassword);
        cardPanel.Controls.Add(txtConfirmPassword);
        cardPanel.Controls.Add(btnResetPassword);
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
        lblTitle.Location = new Point(130, 55);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(235, 48);
        lblTitle.TabIndex = 1;
        lblTitle.Text = "Quên mật khẩu";

        // lblIdentifier
        lblIdentifier.AutoSize = true;
        lblIdentifier.BackColor = Color.White;
        lblIdentifier.ForeColor = Color.Black;
        lblIdentifier.Location = new Point(30, 112);
        lblIdentifier.Name = "lblIdentifier";
        lblIdentifier.Size = new Size(175, 25);
        lblIdentifier.TabIndex = 2;
        lblIdentifier.Text = "Số điện thoại hoặc Email";

        // txtIdentifier
        txtIdentifier.BackColor = Color.White;
        txtIdentifier.BorderStyle = BorderStyle.FixedSingle;
        txtIdentifier.Font = new Font("Segoe UI", 11F);
        txtIdentifier.ForeColor = Color.Black;
        txtIdentifier.Location = new Point(30, 138);
        txtIdentifier.Name = "txtIdentifier";
        txtIdentifier.Size = new Size(270, 34);
        txtIdentifier.TabIndex = 3;

        // btnSendOtp
        btnSendOtp.BackColor = Color.White;
        btnSendOtp.Cursor = Cursors.Hand;
        btnSendOtp.FlatAppearance.BorderColor = Color.Black;
        btnSendOtp.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnSendOtp.FlatStyle = FlatStyle.Flat;
        btnSendOtp.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSendOtp.ForeColor = Color.Black;
        btnSendOtp.Location = new Point(310, 137);
        btnSendOtp.Name = "btnSendOtp";
        btnSendOtp.Size = new Size(120, 35);
        btnSendOtp.TabIndex = 4;
        btnSendOtp.Text = "Gửi mã OTP";
        btnSendOtp.UseVisualStyleBackColor = false;
        btnSendOtp.Click += btnSendOtp_Click;

        // lblOtp
        lblOtp.AutoSize = true;
        lblOtp.BackColor = Color.White;
        lblOtp.ForeColor = Color.Black;
        lblOtp.Location = new Point(30, 184);
        lblOtp.Name = "lblOtp";
        lblOtp.Size = new Size(140, 25);
        lblOtp.TabIndex = 5;
        lblOtp.Text = "Mã OTP (6 chữ số)";

        // txtOtp
        txtOtp.BackColor = Color.White;
        txtOtp.BorderStyle = BorderStyle.FixedSingle;
        txtOtp.Font = new Font("Segoe UI", 11F);
        txtOtp.ForeColor = Color.Black;
        txtOtp.Location = new Point(30, 210);
        txtOtp.MaxLength = 6;
        txtOtp.Name = "txtOtp";
        txtOtp.Size = new Size(400, 34);
        txtOtp.TabIndex = 6;

        // lblNewPassword
        lblNewPassword.AutoSize = true;
        lblNewPassword.BackColor = Color.White;
        lblNewPassword.ForeColor = Color.Black;
        lblNewPassword.Location = new Point(30, 256);
        lblNewPassword.Name = "lblNewPassword";
        lblNewPassword.Size = new Size(110, 25);
        lblNewPassword.TabIndex = 7;
        lblNewPassword.Text = "Mật khẩu mới";

        // txtNewPassword
        txtNewPassword.BackColor = Color.White;
        txtNewPassword.BorderStyle = BorderStyle.FixedSingle;
        txtNewPassword.Font = new Font("Segoe UI", 11F);
        txtNewPassword.ForeColor = Color.Black;
        txtNewPassword.Location = new Point(30, 282);
        txtNewPassword.Name = "txtNewPassword";
        txtNewPassword.Size = new Size(400, 34);
        txtNewPassword.TabIndex = 8;
        txtNewPassword.UseSystemPasswordChar = true;

        // lblConfirmPassword
        lblConfirmPassword.AutoSize = true;
        lblConfirmPassword.BackColor = Color.White;
        lblConfirmPassword.ForeColor = Color.Black;
        lblConfirmPassword.Location = new Point(30, 328);
        lblConfirmPassword.Name = "lblConfirmPassword";
        lblConfirmPassword.Size = new Size(175, 25);
        lblConfirmPassword.TabIndex = 9;
        lblConfirmPassword.Text = "Xác nhận mật khẩu mới";

        // txtConfirmPassword
        txtConfirmPassword.BackColor = Color.White;
        txtConfirmPassword.BorderStyle = BorderStyle.FixedSingle;
        txtConfirmPassword.Font = new Font("Segoe UI", 11F);
        txtConfirmPassword.ForeColor = Color.Black;
        txtConfirmPassword.Location = new Point(30, 354);
        txtConfirmPassword.Name = "txtConfirmPassword";
        txtConfirmPassword.Size = new Size(400, 34);
        txtConfirmPassword.TabIndex = 10;
        txtConfirmPassword.UseSystemPasswordChar = true;

        // btnResetPassword
        btnResetPassword.BackColor = Color.White;
        btnResetPassword.Cursor = Cursors.Hand;
        btnResetPassword.FlatAppearance.BorderColor = Color.Black;
        btnResetPassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnResetPassword.FlatStyle = FlatStyle.Flat;
        btnResetPassword.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnResetPassword.ForeColor = Color.Black;
        btnResetPassword.Location = new Point(30, 412);
        btnResetPassword.Name = "btnResetPassword";
        btnResetPassword.Size = new Size(400, 45);
        btnResetPassword.TabIndex = 11;
        btnResetPassword.Text = "Đặt lại mật khẩu ➔";
        btnResetPassword.UseVisualStyleBackColor = false;
        btnResetPassword.Click += btnResetPassword_Click;

        // btnBack
        btnBack.BackColor = Color.White;
        btnBack.Cursor = Cursors.Hand;
        btnBack.FlatAppearance.BorderColor = Color.Black;
        btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnBack.FlatStyle = FlatStyle.Flat;
        btnBack.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnBack.ForeColor = Color.Black;
        btnBack.Location = new Point(30, 472);
        btnBack.Name = "btnBack";
        btnBack.Size = new Size(400, 45);
        btnBack.TabIndex = 12;
        btnBack.Text = "Quay lại đăng nhập";
        btnBack.UseVisualStyleBackColor = false;
        btnBack.Click += btnBack_Click;

        // ForgotPasswordForm
        BackColor = Color.White;
        ClientSize = new Size(800, 667);
        Controls.Add(cardPanel);
        ForeColor = Color.Black;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LIBRA - Quên mật khẩu";
        cardPanel.ResumeLayout(false);
        cardPanel.PerformLayout();
        ResumeLayout(false);
    }
}