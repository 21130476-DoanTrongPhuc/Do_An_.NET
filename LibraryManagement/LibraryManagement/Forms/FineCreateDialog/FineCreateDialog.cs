using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms.FineCreateDialog
{
    public partial class FineCreateDialog : Form
    {
        // ====== Properties trả về ======
        public string ReaderId { get; private set; } = "";
        public string ReaderName { get; private set; } = "";
        public string TicketId { get; private set; } = "";
        public FineManagement.FineManagement.FineType FineType { get; private set; }
        public string Reason { get; private set; } = "";
        public decimal Amount { get; private set; }

        // ====== Controls ======
        private TextBox txtReaderId = null!;
        private TextBox txtReaderName = null!;
        private TextBox txtTicket = null!;
        private TextBox txtAmount = null!;
        private TextBox txtReason = null!;
        private ComboBox cboType = null!;
        private Label lblError = null!;

        // ====== Constructor ======
        public FineCreateDialog(FineManagement.FineManagement.FineRow? sourceRow = null)
        {
            InitializeComponent();

            Text = "Tạo khoản phạt thủ công";
            Size = new Size(640, 560);
            MinimumSize = new Size(640, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            // BƯỚC 1: dựng UI — phải chạy trước để các control được khởi tạo
            BuildUi();

            // BƯỚC 2: auto-fill nếu có dòng nguồn
            if (sourceRow != null)
                ApplySourceRow(sourceRow);
        }

        // ====== Dựng UI ======
        private void BuildUi()
        {
            // Thứ tự add: Fill → Bottom → Top
            var body = BuildBody();
            var footer = BuildFooter();
            var header = BuildHeader();

            Controls.Add(body);      // Fill
            Controls.Add(footer);    // Bottom
            Controls.Add(header);    // Top
        }

        private Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(232, 240, 250),
                Padding = new Padding(16, 0, 16, 0)
            };
            header.Controls.Add(new Label
            {
                Text = "Tạo khoản phạt thủ công",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });
            return header;
        }

        private Panel BuildFooter()
        {
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // spacer
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));  // Hủy
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));  // Lưu
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tbl.Controls.Add(new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) }, 0, 0);

            var btnCancel = new Button
            {
                Text = "Hủy",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            tbl.Controls.Add(btnCancel, 1, 0);

            var btnSave = new Button
            {
                Text = "Lưu phiếu phạt",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(19, 98, 160),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (_, _) => TrySave();
            tbl.Controls.Add(btnSave, 2, 0);

            footer.Controls.Add(tbl);
            return footer;
        }

        private Panel BuildBody()
        {
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16),
                AutoScroll = true
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Các field — LƯU Ý: gán vào biến field ngay khi tạo
            txtReaderId   = AddField(tbl, "Mã độc giả *");
            txtReaderName = AddField(tbl, "Tên độc giả");
            txtTicket     = AddField(tbl, "Mã phiếu mượn");

            // Combo loại vi phạm
            AddLabelRow(tbl, "Loại vi phạm *");
            cboType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 3, 0, 3)
            };
            cboType.Items.AddRange(new object[] {
                "Trả sách quá hạn", "Làm hư hỏng sách", "Làm mất sách", "Vi phạm nội quy khác"
            });
            cboType.SelectedIndex = 0;
            tbl.Controls.Add(cboType, 1, tbl.RowCount - 1);

            txtAmount = AddField(tbl, "Số tiền phạt (VNĐ) *");
            txtReason = AddField(tbl, "Lý do / Ghi chú *", multiline: true);

            // Hàng lỗi
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblError = new Label
            {
                ForeColor = Color.FromArgb(200, 40, 40),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 0)
            };
            tbl.Controls.Add(lblError, 1, tbl.RowCount - 1);

            body.Controls.Add(tbl);
            return body;
        }

        private static void AddLabelRow(TableLayoutPanel tbl, string caption)
        {
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tbl.Controls.Add(new Label
            {
                Text = caption,
                AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 8, 8, 4)
            }, 0, tbl.RowCount - 1);
        }

        private static TextBox AddField(TableLayoutPanel tbl, string caption, bool multiline = false)
        {
            AddLabelRow(tbl, caption);

            var tb = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = multiline,
                Height = multiline ? 70 : 26,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 3, 0, 3)
            };
            if (multiline) tb.ScrollBars = ScrollBars.Vertical;

            tbl.Controls.Add(tb, 1, tbl.RowCount - 1);
            return tb;
        }

        // ====== Auto-fill từ dòng được chọn ======
        private void ApplySourceRow(FineManagement.FineManagement.FineRow row)
        {
            if (row == null) return;

            if (txtReaderId != null)
            {
                txtReaderId.Text = row.ReaderId ?? "";
                txtReaderId.BackColor = Color.FromArgb(245, 250, 255);  // xanh nhạt
            }
            if (txtReaderName != null)
            {
                txtReaderName.Text = row.ReaderName ?? "";
                txtReaderName.BackColor = Color.FromArgb(245, 250, 255);
            }
            if (txtTicket != null)
            {
                txtTicket.Text = row.TicketId ?? "";
                txtTicket.BackColor = Color.FromArgb(245, 250, 255);
            }

            if (cboType != null)
            {
                int idx = (int)row.Type;
                if (idx >= 0 && idx < cboType.Items.Count)
                    cboType.SelectedIndex = idx;
            }
        }

        // ====== Lưu ======
        private void TrySave()
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtReaderId.Text))
            {
                lblError.Text = "Vui lòng nhập Mã độc giả.";
                return;
            }

            if (!decimal.TryParse(
                    txtAmount.Text.Trim().Replace(",", "").Replace(".", ""),
                    out var amt) || amt <= 0)
            {
                lblError.Text = "Số tiền phạt phải là số dương.";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                lblError.Text = "Vui lòng nhập lý do.";
                return;
            }

            ReaderId   = txtReaderId.Text.Trim();
            ReaderName = txtReaderName.Text.Trim();
            TicketId   = txtTicket.Text.Trim();
            FineType   = (FineManagement.FineManagement.FineType)cboType.SelectedIndex;
            Amount     = amt;
            Reason     = txtReason.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}