namespace LibraryManagement.Forms.FinePayDialog
{
    public partial class FinePayDialog : Form
    {
        public decimal PaidAmount { get; private set; }
        public string Method { get; private set; } = "Tiền mặt";

        private readonly FineManagement.FineManagement.FineRow _row;
        private TextBox txtAmount = null!;
        private RadioButton rdoCash = null!, rdoTransfer = null!;
        private Label lblError = null!;

        public FinePayDialog(FineManagement.FineManagement.FineRow row)
        {
            _row = row;
            InitializeComponent();
            Text = "Thu tiền phạt";
            Size = new Size(500, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White; Font = new Font("Segoe UI", 9F);
            BuildUi();
        }

        private void BuildUi()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.FromArgb(232, 240, 250) };
            header.Controls.Add(new Label
            {
                Text = $"Thu tiền — Phiếu {_row.FineId}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true, Location = new Point(16, 16)
            });
            Controls.Add(header);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            Controls.Add(body);

            body.Controls.Add(new Label
            {
                Text = $"Độc giả: {_row.ReaderName} ({_row.ReaderId})",
                AutoSize = true, Location = new Point(0, 6), Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            });
            body.Controls.Add(new Label
            {
                Text = $"Tiền phạt: {_row.Amount:N0} đ   ·   Đã trả: {_row.Paid:N0} đ   ·   Còn nợ: {_row.Remaining:N0} đ",
                AutoSize = true, ForeColor = Color.FromArgb(110, 117, 128),
                Location = new Point(0, 30)
            });

            body.Controls.Add(new Label
            {
                Text = "Số tiền thu (VNĐ)", AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128), Location = new Point(0, 70)
            });
            txtAmount = new TextBox
            {
                Location = new Point(160, 68), Width = 280,
                Text = _row.Remaining.ToString("N0"), Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            body.Controls.Add(txtAmount);

            body.Controls.Add(new Label
            {
                Text = "Phương thức", AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128), Location = new Point(0, 116)
            });
            rdoCash = new RadioButton { Text = "Tiền mặt", AutoSize = true, Location = new Point(160, 118), Checked = true };
            rdoTransfer = new RadioButton { Text = "Chuyển khoản", AutoSize = true, Location = new Point(260, 118) };
            body.Controls.Add(rdoCash); body.Controls.Add(rdoTransfer);

            lblError = new Label { ForeColor = Color.FromArgb(200, 40, 40), AutoSize = true, Location = new Point(0, 160) };
            body.Controls.Add(lblError);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            Controls.Add(footer);

            var btnCancel = new Button
            {
                Text = "Hủy", Size = new Size(100, 34), Location = new Point(260, 12),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            footer.Controls.Add(btnCancel);

            var btnOk = new Button
            {
                Text = "Xác nhận thu", Size = new Size(140, 34), Location = new Point(368, 12),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(34, 160, 90),
                ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (_, _) => TrySave();
            footer.Controls.Add(btnOk);
        }

        private void TrySave()
        {
            lblError.Text = "";
            if (!decimal.TryParse(txtAmount.Text.Trim().Replace(",", "").Replace(".", ""), out var amt) || amt <= 0)
            { lblError.Text = "Số tiền phải là số dương."; return; }
            if (amt > _row.Remaining) { lblError.Text = "Số tiền vượt quá số còn nợ."; return; }
            PaidAmount = amt;
            Method = rdoCash.Checked ? "Tiền mặt" : "Chuyển khoản";
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}