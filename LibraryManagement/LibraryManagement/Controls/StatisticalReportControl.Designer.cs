using System.Text;
using LibraryManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controls;

public partial class StatisticalReportControl : UserControl
{
    public StatisticalReportControl()
    {
        InitializeComponent();
        dtpFrom.Value = DateTime.Today.AddMonths(-1);
        dtpTo.Value = DateTime.Today;
    }

    private void StatisticalReportControl_Load(object? sender, EventArgs e)
    {
        LoadStatistics();
    }

    private void btnFilter_Click(object? sender, EventArgs e)
    {
        if (dtpFrom.Value.Date > dtpTo.Value.Date)
        {
            MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LoadStatistics();
    }

    private void LoadStatistics()
    {
        try
        {
            DateTime fromDate = dtpFrom.Value.Date;
            DateTime toDate = dtpTo.Value.Date.AddDays(1).AddTicks(-1);

            using var context = new LibraryDbContext();

            // 1. Thống kê tổng quan (KPI)
            int totalBooks = context.Books.Count();
            int totalReaders = context.Readers.Count();

            var loansInRange = context.Loans
                .Include(l => l.Reader)
                .Include(l => l.LoanDetails)
                .Where(l => l.BorrowDate >= fromDate && l.BorrowDate <= toDate)
                .OrderByDescending(l => l.BorrowDate)
                .ToList();

            int borrowingCount = loansInRange.Count(l => l.Status != "Đã trả đủ");
            int overdueCount = loansInRange.Count(l =>
                l.Status == "Quá hạn" || (l.Status != "Đã trả đủ" && l.DueDate.Date < DateTime.Today));

            lblCardBooksValue.Text = totalBooks.ToString("N0");
            lblCardReadersValue.Text = totalReaders.ToString("N0");
            lblCardBorrowingValue.Text = borrowingCount.ToString("N0");
            lblCardOverdueValue.Text = overdueCount.ToString("N0");

            // 2. Đổ dữ liệu chi tiết lên DataGridView
            var reportRows = loansInRange.Select(l =>
            {
                bool isOverdue = l.Status == "Quá hạn" || (l.Status != "Đã trả đủ" && l.DueDate.Date < DateTime.Today);
                return new
                {
                    MaPhieu = $"PM{l.Id:D4}",
                    MaDocGia = l.Reader?.ReaderCode ?? "N/A",
                    TenDocGia = l.Reader?.FullName ?? "Không xác định",
                    SoDienThoai = l.Reader?.Phone ?? "",
                    SoLuongSach = l.LoanDetails.Count,
                    NgayMuon = l.BorrowDate.ToString("dd/MM/yyyy"),
                    HanTra = l.DueDate.ToString("dd/MM/yyyy"),
                    TrangThai = isOverdue ? "Quá hạn" : l.Status
                };
            }).ToList();

            dgvReport.DataSource = reportRows;

            if (dgvReport.Columns.Count > 0)
            {
                dgvReport.Columns["MaPhieu"]!.HeaderText = "Mã phiếu";
                dgvReport.Columns["MaDocGia"]!.HeaderText = "Mã độc giả";
                dgvReport.Columns["TenDocGia"]!.HeaderText = "Họ tên độc giả";
                dgvReport.Columns["SoDienThoai"]!.HeaderText = "Số điện thoại";
                dgvReport.Columns["SoLuongSach"]!.HeaderText = "Số sách";
                dgvReport.Columns["NgayMuon"]!.HeaderText = "Ngày mượn";
                dgvReport.Columns["HanTra"]!.HeaderText = "Hạn trả";
                dgvReport.Columns["TrangThai"]!.HeaderText = "Trạng thái";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu thống kê: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnExportCsv_Click(object? sender, EventArgs e)
    {
        if (dgvReport.Rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất báo cáo!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "CSV file (*.csv)|*.csv",
            FileName = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        var headers = dgvReport.Columns.Cast<DataGridViewColumn>().Select(c => $"\"{c.HeaderText}\"");
        sb.AppendLine(string.Join(",", headers));

        foreach (DataGridViewRow row in dgvReport.Rows)
        {
            var cells = row.Cells.Cast<DataGridViewCell>().Select(c => $"\"{c.Value}\"");
            sb.AppendLine(string.Join(",", cells));
        }

        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show("Xuất file báo cáo thành công!", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}