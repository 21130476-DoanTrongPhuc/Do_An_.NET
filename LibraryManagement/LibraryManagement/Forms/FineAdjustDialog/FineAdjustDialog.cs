

namespace LibraryManagement.Forms.FineAdjustDialog
{
    public partial class FineAdjustDialog : Form
    {
        public decimal NewAmount { get; private set; }
        public string Reason { get; private set; } = "";

        private readonly FineManagement.FineManagement.FineRow _row;
        private TextBox txtAmount = null!;
        private TextBox txtReason = null!;
        private Label lblError = null!;

        public FineAdjustDialog(FineManagement.FineManagement.FineRow row)
        {
            _row = row;
            InitializeComponent();
            Text = "Điều chỉnh mức phạt";
            Size = new Size(560, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
        }

        private void BuildUi()
        {
            // --- Header ---
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(254, 244, 226)
            };
            header.Controls.Add(new Label
            {
                Text = $"Điều chỉnh — Phiếu {_row.FineId}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 16)
            });
            Controls.Add(header);

            // --- Body ---
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 16, 20, 16)
            };
            Controls.Add(body);

            body.Controls.Add(new Label
            {
                Text = $"Mức phạt hiện tại: {_row.Amount:N0} đ",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(0, 4)
            });

            body.Controls.Add(new Label
            {
                Text = "Mức phạt mới (VNĐ)",
                AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128),
                Location = new Point(0, 44)
            });

            txtAmount = new TextBox
            {
                Location = new Point(160, 42),
                Width = 340,
                Text = _row.Amount.ToString("N0"),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            body.Controls.Add(txtAmount);

            body.Controls.Add(new Label
            {
                Text = "Lý do điều chỉnh *",
                AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128),
                Location = new Point(0, 84)
            });

            txtReason = new TextBox
            {
                Location = new Point(160, 82),
                Width = 340,
                Multiline = true,
                Height = 80
            };
            body.Controls.Add(txtReason);

            lblError = new Label
            {
                ForeColor = Color.FromArgb(200, 40, 40),
                AutoSize = true,
                Location = new Point(0, 176)
            };
            body.Controls.Add(lblError);

            // --- Footer ---
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.White
            };
            Controls.Add(footer);

            var btnCancel = new Button
            {
                Text = "Hủy",
                Size = new Size(100, 34),
                Location = new Point(300, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            footer.Controls.Add(btnCancel);

            var btnOk = new Button
            {
                Text = "Lưu điều chỉnh",
                Size = new Size(140, 34),
                Location = new Point(408, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(214, 140, 10),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (_, _) => TrySave();
            footer.Controls.Add(btnOk);
        }

        private void TrySave()
        {
            lblError.Text = "";

            // Parse số tiền, cho phép nhập "10,000", "10000", "10.000"
            string raw = txtAmount.Text.Trim().Replace(",", "").Replace(".", "");
            if (!decimal.TryParse(raw, out var amt) || amt < 0)
            {
                lblError.Text = "Số tiền không hợp lệ.";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                lblError.Text = "Bắt buộc nhập lý do điều chỉnh.";
                return;
            }

            if (amt == _row.Amount)
            {
                lblError.Text = "Mức phạt mới phải khác mức hiện tại.";
                return;
            }

            NewAmount = amt;
            Reason = txtReason.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}