namespace LibraryManagement.Forms.FineCancelDialog
{
    public partial class FineCancelDialog : Form
    {
        public string Reason { get; private set; } = "";
        private TextBox txtReason = null!;
        private Label lblError = null!;

        public FineCancelDialog(FineManagement.FineManagement.FineRow row)
        {
            InitializeComponent();
            Text = "Hủy khoản phạt";
            Size = new Size(560, 320);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White; Font = new Font("Segoe UI", 9F);
            BuildUi(row);
        }

        private void BuildUi(FineManagement.FineManagement.FineRow row)
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(253, 234, 234)
            };
            header.Controls.Add(new Label
            {
                Text = $"Hủy — Phiếu {row.FineId}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 16)
            });
            Controls.Add(header);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            Controls.Add(body);

            body.Controls.Add(new Label
            {
                Text = $"Bạn có chắc chắn muốn hủy phiếu phạt {row.FineId}?\n" +
                       $"Tiền phạt hiện tại: {row.Amount:N0} đ\n" +
                       "Hành động này sẽ được ghi vào lịch sử.",
                AutoSize = false,
                Size = new Size(500, 70),
                Font = new Font("Segoe UI", 9F),
                Location = new Point(0, 4)
            });

            body.Controls.Add(new Label
            {
                Text = "Lý do hủy *", AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128), Location = new Point(0, 84)
            });
            txtReason = new TextBox { Location = new Point(0, 108), Width = 500, Multiline = true, Height = 70 };
            body.Controls.Add(txtReason);

            lblError = new Label { ForeColor = Color.FromArgb(200, 40, 40), AutoSize = true, Location = new Point(0, 186) };
            body.Controls.Add(lblError);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            Controls.Add(footer);
            var btnCancel = new Button { Text = "Không", Size = new Size(100, 34), Location = new Point(300, 12), FlatStyle = FlatStyle.Flat, BackColor = Color.White };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            footer.Controls.Add(btnCancel);
            var btnOk = new Button
            {
                Text = "Hủy khoản phạt", Size = new Size(140, 34), Location = new Point(408, 12),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(200, 40, 40),
                ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtReason.Text)) { lblError.Text = "Bắt buộc nhập lý do hủy."; return; }
                Reason = txtReason.Text.Trim();
                DialogResult = DialogResult.OK; Close();
            };
            footer.Controls.Add(btnOk);
        }
    }
}