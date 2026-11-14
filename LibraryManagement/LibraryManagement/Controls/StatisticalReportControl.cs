#nullable enable

using System.ComponentModel;

namespace LibraryManagement.Controls;

partial class StatisticalReportControl
{
    private IContainer? components = null;

    private Label lblTitle = null!;
    private Label lblSubTitle = null!;
    private Panel pnlFilter = null!;
    private Label lblFrom = null!;
    private DateTimePicker dtpFrom = null!;
    private Label lblTo = null!;
    private DateTimePicker dtpTo = null!;
    private Button btnFilter = null!;
    private Button btnExportCsv = null!;

    private Panel pnlCardBooks = null!;
    private Label lblCardBooksTitle = null!;
    private Label lblCardBooksValue = null!;

    private Panel pnlCardReaders = null!;
    private Label lblCardReadersTitle = null!;
    private Label lblCardReadersValue = null!;

    private Panel pnlCardBorrowing = null!;
    private Label lblCardBorrowingTitle = null!;
    private Label lblCardBorrowingValue = null!;

    private Panel pnlCardOverdue = null!;
    private Label lblCardOverdueTitle = null!;
    private Label lblCardOverdueValue = null!;

    private Label lblTableTitle = null!;
    private DataGridView dgvReport = null!;

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
        lblTitle = new Label();
        lblSubTitle = new Label();
        pnlFilter = new Panel();
        lblFrom = new Label();
        dtpFrom = new DateTimePicker();
        lblTo = new Label();
        dtpTo = new DateTimePicker();
        btnFilter = new Button();
        btnExportCsv = new Button();

        pnlCardBooks = new Panel();
        lblCardBooksTitle = new Label();
        lblCardBooksValue = new Label();

        pnlCardReaders = new Panel();
        lblCardReadersTitle = new Label();
        lblCardReadersValue = new Label();

        pnlCardBorrowing = new Panel();
        lblCardBorrowingTitle = new Label();
        lblCardBorrowingValue = new Label();

        pnlCardOverdue = new Panel();
        lblCardOverdueTitle = new Label();
        lblCardOverdueValue = new Label();

        lblTableTitle = new Label();
        dgvReport = new DataGridView();

        pnlFilter.SuspendLayout();
        pnlCardBooks.SuspendLayout();
        pnlCardReaders.SuspendLayout();
        pnlCardBorrowing.SuspendLayout();
        pnlCardOverdue.SuspendLayout();
        ((ISupportInitialize)dgvReport).BeginInit();
        SuspendLayout();

        // lblTitle
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.ForeColor = Color.Black;
        lblTitle.Location = new Point(24, 18);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(275, 32);
        lblTitle.Text = "Báo cáo & Thống kê";

        // lblSubTitle
        lblSubTitle.AutoSize = true;
        lblSubTitle.Font = new Font("Segoe UI", 10F);
        lblSubTitle.ForeColor = Color.DimGray;
        lblSubTitle.Location = new Point(26, 54);
        lblSubTitle.Name = "lblSubTitle";
        lblSubTitle.Size = new Size(340, 19);
        lblSubTitle.Text = "Tổng hợp tình hình mượn trả sách và hoạt động thư viện";

        // pnlFilter
        pnlFilter.BackColor = Color.White;
        pnlFilter.BorderStyle = BorderStyle.FixedSingle;
        pnlFilter.Controls.Add(lblFrom);
        pnlFilter.Controls.Add(dtpFrom);
        pnlFilter.Controls.Add(lblTo);
        pnlFilter.Controls.Add(dtpTo);
        pnlFilter.Controls.Add(btnFilter);
        pnlFilter.Controls.Add(btnExportCsv);
        pnlFilter.Location = new Point(24, 86);
        pnlFilter.Name = "pnlFilter";
        pnlFilter.Size = new Size(920, 56);

        // lblFrom
        lblFrom.AutoSize = true;
        lblFrom.Font = new Font("Segoe UI", 10F);
        lblFrom.Location = new Point(16, 17);
        lblFrom.Text = "Từ ngày:";

        // dtpFrom
        dtpFrom.Font = new Font("Segoe UI", 10F);
        dtpFrom.Format = DateTimePickerFormat.Short;
        dtpFrom.Location = new Point(82, 14);
        dtpFrom.Size = new Size(140, 25);

        // lblTo
        lblTo.AutoSize = true;
        lblTo.Font = new Font("Segoe UI", 10F);
        lblTo.Location = new Point(242, 17);
        lblTo.Text = "Đến ngày:";

        // dtpTo
        dtpTo.Font = new Font("Segoe UI", 10F);
        dtpTo.Format = DateTimePickerFormat.Short;
        dtpTo.Location = new Point(316, 14);
        dtpTo.Size = new Size(140, 25);

        // btnFilter
        btnFilter.BackColor = Color.Black;
        btnFilter.Cursor = Cursors.Hand;
        btnFilter.FlatAppearance.BorderColor = Color.Black;
        btnFilter.FlatStyle = FlatStyle.Flat;
        btnFilter.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnFilter.ForeColor = Color.White;
        btnFilter.Location = new Point(480, 11);
        btnFilter.Size = new Size(145, 32);
        btnFilter.Text = "Lọc thống kê";
        btnFilter.UseVisualStyleBackColor = false;
        btnFilter.Click += btnFilter_Click;

        // btnExportCsv
        btnExportCsv.BackColor = Color.White;
        btnExportCsv.Cursor = Cursors.Hand;
        btnExportCsv.FlatAppearance.BorderColor = Color.Black;
        btnExportCsv.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnExportCsv.FlatStyle = FlatStyle.Flat;
        btnExportCsv.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnExportCsv.ForeColor = Color.Black;
        btnExportCsv.Location = new Point(745, 11);
        btnExportCsv.Size = new Size(155, 32);
        btnExportCsv.Text = "Xuất file CSV ➔";
        btnExportCsv.UseVisualStyleBackColor = false;
        btnExportCsv.Click += btnExportCsv_Click;

        // 4 KPI Cards (215px width each, 20px gap)
        SetupKpiCard(pnlCardBooks, lblCardBooksTitle, lblCardBooksValue, new Point(24, 158), "TỔNG ĐẦU SÁCH");
        SetupKpiCard(pnlCardReaders, lblCardReadersTitle, lblCardReadersValue, new Point(259, 158), "TỔNG ĐỘC GIẢ");
        SetupKpiCard(pnlCardBorrowing, lblCardBorrowingTitle, lblCardBorrowingValue, new Point(494, 158), "PHIẾU ĐANG MƯỢN");
        SetupKpiCard(pnlCardOverdue, lblCardOverdueTitle, lblCardOverdueValue, new Point(729, 158), "PHIẾU QUÁ HẠN");

        // lblTableTitle
        lblTableTitle.AutoSize = true;
        lblTableTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTableTitle.ForeColor = Color.Black;
        lblTableTitle.Location = new Point(24, 276);
        lblTableTitle.Text = "Chi tiết phiếu mượn trong kỳ";

        // dgvReport
        dgvReport.AllowUserToAddRows = false;
        dgvReport.AllowUserToDeleteRows = false;
        dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvReport.BackgroundColor = Color.White;
        dgvReport.BorderStyle = BorderStyle.FixedSingle;
        dgvReport.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        dgvReport.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
        dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        dgvReport.ColumnHeadersHeight = 38;
        dgvReport.DefaultCellStyle.BackColor = Color.White;
        dgvReport.DefaultCellStyle.ForeColor = Color.Black;
        dgvReport.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
        dgvReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 230, 230);
        dgvReport.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgvReport.EnableHeadersVisualStyles = false;
        dgvReport.GridColor = Color.LightGray;
        dgvReport.Location = new Point(24, 308);
        dgvReport.Name = "dgvReport";
        dgvReport.ReadOnly = true;
        dgvReport.RowHeadersVisible = false;
        dgvReport.RowTemplate.Height = 34;
        dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvReport.Size = new Size(920, 320);

        // StatisticalReportControl
        BackColor = Color.White;
        Controls.Add(dgvReport);
        Controls.Add(lblTableTitle);
        Controls.Add(pnlCardOverdue);
        Controls.Add(pnlCardBorrowing);
        Controls.Add(pnlCardReaders);
        Controls.Add(pnlCardBooks);
        Controls.Add(pnlFilter);
        Controls.Add(lblSubTitle);
        Controls.Add(lblTitle);
        ForeColor = Color.Black;
        Name = "StatisticalReportControl";
        Size = new Size(970, 650);
        Load += StatisticalReportControl_Load;

        pnlFilter.ResumeLayout(false);
        pnlFilter.PerformLayout();
        pnlCardBooks.ResumeLayout(false);
        pnlCardReaders.ResumeLayout(false);
        pnlCardBorrowing.ResumeLayout(false);
        pnlCardOverdue.ResumeLayout(false);
        ((ISupportInitialize)dgvReport).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private static void SetupKpiCard(Panel card, Label title, Label value, Point location, string titleText)
    {
        card.BackColor = Color.White;
        card.BorderStyle = BorderStyle.FixedSingle;
        card.Location = location;
        card.Size = new Size(215, 100);
        card.Controls.Add(value);
        card.Controls.Add(title);

        title.AutoSize = true;
        title.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        title.ForeColor = Color.DimGray;
        title.Location = new Point(14, 14);
        title.Text = titleText;

        value.AutoSize = true;
        value.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        value.ForeColor = Color.Black;
        value.Location = new Point(12, 42);
        value.Text = "0";
    }
}