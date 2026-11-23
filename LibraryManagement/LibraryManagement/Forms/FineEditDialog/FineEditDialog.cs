using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.Forms.FineEditDialog
{
    /// <summary>
    /// Dialog chỉnh sửa thông tin phiếu phạt (trừ thanh toán).
    /// Cho phép sửa: độc giả, mã phiếu mượn, loại vi phạm, số tiền, lý do.
    /// Không cho sửa: mã phạt, ngày tạo, người lập.
    /// </summary>
    public partial class FineEditDialog : Form
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
        private Label lblCurrentInfo = null!;

        private readonly FineManagement.FineManagement.FineRow _row;

        public FineEditDialog(FineManagement.FineManagement.FineRow row)
        {
            _row = row ?? throw new ArgumentNullException(nameof(row));

            InitializeComponent();
            Text = "Chỉnh sửa phiếu phạt";
            Size = new Size(640, 620);
            MinimumSize = new Size(640, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            BuildUi();
            LoadCurrentValues();
        }

        private void BuildUi()
        {
            var body = BuildBody();
            var footer = BuildFooter();
            var header = BuildHeader();

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
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
                Text = $"Chỉnh sửa phiếu phạt {_row.FineId}",
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
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
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
                Text = "Lưu thay đổi",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(214, 140, 10),   // cam — phân biệt với tạo mới
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

            // Hàng thông tin hiện tại (readonly)
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblCurrentInfo = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(110, 117, 128),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };
            tbl.Controls.Add(lblCurrentInfo, 0, 0);
            tbl.SetColumnSpan(lblCurrentInfo, 2);

            // Các field
            txtReaderId   = AddField(tbl, "Mã độc giả *");
            txtReaderName = AddField(tbl, "Tên độc giả");
            txtTicket     = AddField(tbl, "Mã phiếu mượn");

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

        // ====== Load giá trị hiện tại ======
        private void LoadCurrentValues()
        {
            txtReaderId.Text   = _row.ReaderId;
            txtReaderName.Text = _row.ReaderName;
            txtTicket.Text     = _row.TicketId;
            txtAmount.Text     = _row.Amount.ToString("N0");
            txtReason.Text     = _row.Reason;

            int idx = (int)_row.Type;
            if (idx >= 0 && idx < cboType.Items.Count)
                cboType.SelectedIndex = idx;

            lblCurrentInfo.Text = $"Mã phạt: {_row.FineId}  ·  Ngày tạo: {_row.CreatedAt:dd/MM/yyyy HH:mm}  ·  " +
                                  $"Người lập: {_row.CreatedBy}  ·  Đã trả: {_row.Paid:N0} đ";
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

            // Không cho giảm số tiền xuống dưới số đã trả
            if (amt < _row.Paid)
            {
                lblError.Text = $"Số tiền phạt không được nhỏ hơn số đã trả ({_row.Paid:N0} đ).";
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