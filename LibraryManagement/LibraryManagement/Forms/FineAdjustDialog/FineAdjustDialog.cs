using System;
using System.Drawing;
using System.Windows.Forms;

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
            Size = new Size(600, 440);
            MinimumSize = new Size(600, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            BuildUi();
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
                BackColor = Color.FromArgb(254, 244, 226),
                Padding = new Padding(16, 0, 16, 0)
            };
            header.Controls.Add(new Label
            {
                Text = $"Điều chỉnh — Phiếu {_row.FineId}",
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

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0)
            };

            var btnOk = new Button
            {
                Text = "Lưu điều chỉnh",
                Size = new Size(150, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(214, 140, 10),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(8, 0, 0, 0)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (_, _) => TrySave();
            flow.Controls.Add(btnOk);

            var btnCancel = new Button
            {
                Text = "Hủy",
                Size = new Size(100, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 218);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            flow.Controls.Add(btnCancel);

            footer.Controls.Add(flow);
            return footer;
        }

        private Panel BuildBody()
        {
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16)
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Hàng 0: mức hiện tại (span 2 cột)
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lblCurrent = new Label
            {
                Text = $"Mức phạt hiện tại: {_row.Amount:N0} đ",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 12)
            };
            tbl.Controls.Add(lblCurrent, 0, 0);
            tbl.SetColumnSpan(lblCurrent, 2);

            // Hàng 1: label + textbox mức mới
            AddLabel(tbl, "Mức phạt mới (VNĐ) *");
            txtAmount = new TextBox
            {
                Dock = DockStyle.Fill,
                Text = _row.Amount.ToString("N0"),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Margin = new Padding(0, 3, 0, 3)
            };
            tbl.Controls.Add(txtAmount, 1, tbl.RowCount - 1);

            // Hàng 2: label lý do
            AddLabel(tbl, "Lý do điều chỉnh *");

            // Hàng 3: textbox lý do (multiline)
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            txtReason = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 3, 0, 3)
            };
            tbl.Controls.Add(txtReason, 1, tbl.RowCount - 1);

            // Hàng 4: lỗi (span 2 cột)
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblError = new Label
            {
                ForeColor = Color.FromArgb(200, 40, 40),
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };
            tbl.Controls.Add(lblError, 0, tbl.RowCount - 1);
            tbl.SetColumnSpan(lblError, 2);

            body.Controls.Add(tbl);
            return body;
        }

        private static void AddLabel(TableLayoutPanel tbl, string caption)
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

        private void TrySave()
        {
            lblError.Text = "";

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