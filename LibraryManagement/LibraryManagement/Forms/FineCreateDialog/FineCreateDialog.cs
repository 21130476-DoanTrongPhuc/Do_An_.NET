namespace LibraryManagement.Forms.FineCreateDialog
{
    public partial class FineCreateDialog : Form
    {
        public string ReaderId { get; private set; } = "";
        public string ReaderName { get; private set; } = "";
        public string TicketId { get; private set; } = "";
        public string BookTitle { get; private set; } = "";
        public string CopyId { get; private set; } = "";
        public FineManagement.FineManagement.FineType FineType { get; private set; }
        public string Reason { get; private set; } = "";
        public decimal Amount { get; private set; }

        private TextBox txtReaderId = null!, txtReaderName = null!, txtTicket = null!,
                        txtBook = null!, txtCopy = null!, txtAmount = null!, txtReason = null!;
        private ComboBox cboType = null!;
        private Label lblError = null!;

        public FineCreateDialog()
        {
            InitializeComponent();
            Text = "Tạo khoản phạt thủ công";
            Size = new Size(620, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
        }

        private void BuildUi()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.FromArgb(232, 240, 250) };
            header.Controls.Add(new Label
            {
                Text = "Tạo khoản phạt thủ công",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true, Location = new Point(16, 16)
            });
            Controls.Add(header);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            Controls.Add(body);

            int y = 8;
            txtReaderId   = AddField(body, "Mã độc giả",   ref y);
            txtReaderName = AddField(body, "Tên độc giả",  ref y);
            txtTicket     = AddField(body, "Mã phiếu mượn (nếu có)", ref y);
            txtBook       = AddField(body, "Tên sách (nếu có)", ref y);
            txtCopy       = AddField(body, "Mã vạch sách (nếu có)", ref y);

            body.Controls.Add(new Label
            {
                Text = "Loại vi phạm", AutoSize = true, ForeColor = Color.FromArgb(110, 117, 128),
                Location = new Point(0, y)
            });
            cboType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(160, y - 2), Width = 380
            };
            cboType.Items.AddRange(new object[] {
                "Trả sách quá hạn", "Làm hư hỏng sách", "Làm mất sách", "Vi phạm nội quy khác"
            });
            cboType.SelectedIndex = 0;
            body.Controls.Add(cboType);
            y += 38;

            txtAmount = AddField(body, "Số tiền phạt (VNĐ)", ref y);
            txtReason = AddField(body, "Lý do / Ghi chú", ref y, multiline: true);

            lblError = new Label
            {
                ForeColor = Color.FromArgb(200, 40, 40), AutoSize = true,
                Location = new Point(0, y + 4)
            };
            body.Controls.Add(lblError);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            Controls.Add(footer);

            var btnCancel = new Button
            {
                Text = "Hủy", Size = new Size(100, 34), Location = new Point(380, 12),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            footer.Controls.Add(btnCancel);

            var btnSave = new Button
            {
                Text = "Lưu phiếu phạt", Size = new Size(140, 34), Location = new Point(488, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(19, 98, 160), ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (_, _) => TrySave();
            footer.Controls.Add(btnSave);
        }

        private static TextBox AddField(Panel parent, string caption, ref int y, bool multiline = false)
        {
            parent.Controls.Add(new Label
            {
                Text = caption, AutoSize = true, ForeColor = Color.FromArgb(110, 117, 128),
                Location = new Point(0, y)
            });
            var tb = new TextBox
            {
                Location = new Point(160, y - 2), Width = 380,
                Multiline = multiline, Height = multiline ? 60 : 24
            };
            parent.Controls.Add(tb);
            y += multiline ? 74 : 38;
            return tb;
        }

        private void TrySave()
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtReaderId.Text)) { lblError.Text = "Vui lòng nhập Mã độc giả."; return; }
            if (!decimal.TryParse(txtAmount.Text.Trim().Replace(",", "").Replace(".", ""), out var amt) || amt <= 0)
            { lblError.Text = "Số tiền phạt phải là số dương."; return; }
            if (string.IsNullOrWhiteSpace(txtReason.Text)) { lblError.Text = "Vui lòng nhập lý do."; return; }

            ReaderId = txtReaderId.Text.Trim();
            ReaderName = txtReaderName.Text.Trim();
            TicketId = txtTicket.Text.Trim();
            BookTitle = txtBook.Text.Trim();
            CopyId = txtCopy.Text.Trim();
            FineType = (FineManagement.FineManagement.FineType)cboType.SelectedIndex;
            Amount = amt;
            Reason = txtReason.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}