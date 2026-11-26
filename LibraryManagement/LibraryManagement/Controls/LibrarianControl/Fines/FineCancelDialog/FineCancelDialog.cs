using System;
using System.Drawing;
using System.Windows.Forms;

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
            Size = new Size(580, 380);
            MinimumSize = new Size(580, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White; Font = new Font("Segoe UI", 9F);
            BuildUi(row);
        }

        private void BuildUi(FineManagement.FineManagement.FineRow row)
        {
            var body = BuildBody(row);
            var footer = BuildFooter();
            var header = BuildHeader(row);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private Panel BuildHeader(FineManagement.FineManagement.FineRow row)
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(253, 234, 234),
                Padding = new Padding(16, 0, 16, 0)
            };
            header.Controls.Add(new Label
            {
                Text = $"Hủy — Phiếu {row.FineId}",
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
                Text = "Hủy khoản phạt",
                Size = new Size(150, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(8, 0, 0, 0)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtReason.Text)) { lblError.Text = "Bắt buộc nhập lý do hủy."; return; }
                Reason = txtReason.Text.Trim();
                DialogResult = DialogResult.OK; Close();
            };
            flow.Controls.Add(btnOk);

            var btnCancel = new Button
            {
                Text = "Không",
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

        private Panel BuildBody(FineManagement.FineManagement.FineRow row)
        {
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16)
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 0,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Hàng 1: thông báo
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tbl.Controls.Add(new Label
            {
                Text = $"Bạn có chắc chắn muốn hủy phiếu phạt {row.FineId}?\n" +
                       $"Tiền phạt hiện tại: {row.Amount:N0} đ\n" +
                       "Hành động này sẽ được ghi vào lịch sử.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 0, 12)
            }, 0, 0);

            // Hàng 2: label lý do
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tbl.Controls.Add(new Label
            {
                Text = "Lý do hủy *",
                AutoSize = true,
                ForeColor = Color.FromArgb(110, 117, 128),
                Margin = new Padding(0, 0, 0, 4)
            }, 0, 1);

            // Hàng 3: textbox
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            txtReason = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0)
            };
            tbl.Controls.Add(txtReason, 0, 2);

            // Hàng 4: lỗi
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblError = new Label
            {
                ForeColor = Color.FromArgb(200, 40, 40),
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };
            tbl.Controls.Add(lblError, 0, 3);

            body.Controls.Add(tbl);
            return body;
        }
    }
}