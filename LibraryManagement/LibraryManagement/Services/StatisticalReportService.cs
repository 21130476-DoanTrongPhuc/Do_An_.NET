
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibraryManagement.Data;
using LibraryManagement.DTO.Response;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public class StatisticalReportService
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;
    private readonly StatisticalReportRepository _reportRepository;

    private const int DailyFineRateVnd = 5000;

    public StatisticalReportService(
        IDbContextFactory<LibraryDbContext> contextFactory,
        StatisticalReportRepository reportRepository)
    {
        _contextFactory = contextFactory;
        _reportRepository = reportRepository;
    }

    public (DateTime FromDate, DateTime ToDate) GetDateRangeByPreset(
        int presetIndex,
        DateTime referenceDate)
    {
        var today = referenceDate.Date;

        return presetIndex switch
        {
            0 => (today, today),

            1 => (today.AddDays(-6), today), 

            2 => (
                new DateTime(today.Year, today.Month, 1),
                today
            ),

            3 => (
                new DateTime(
                    today.Year,
                    ((today.Month - 1) / 3) * 3 + 1,
                    1
                ),
                today
            ),

            4 => (
                new DateTime(today.Year, 1, 1),
                today
            ),

            _ => (today, today)
        };
    }

    public async Task<StatisticalReportDto> GetFullReport(
        DateTime fromDate,
        DateTime toDate)
    {
        var start = fromDate.Date;
        var endExclusive = toDate.Date.AddDays(1);

        if (start > toDate.Date)
        {
            throw new ArgumentException(
                "Ngày bắt đầu không được lớn hơn ngày kết thúc."
            );
        }

        var today = DateTime.Today;
        
        await using var context =
            await _contextFactory.CreateDbContextAsync();
        
        var kpi = await _reportRepository.GetKpiSummary(
            context,
            start,
            endExclusive,
            today
        );

        var monthlyStats = await _reportRepository.GetMonthlyStats(
            context,
            toDate.Year
        );

        var rawCategories = await _reportRepository.GetCategoryStats(
            context
        );

        var categoryStats = rawCategories
            .Take(6)
            .ToList();

        if (rawCategories.Count > 6)
        {
            categoryStats.Add(new CategoryStatDto
            {
                CategoryName = "Thể loại khác",
                BookCount = rawCategories
                    .Skip(6)
                    .Sum(x => x.BookCount)
            });
        }

        var overdueItems = await _reportRepository.GetOverdueItems(
            context,
            today
        );

        foreach (var item in overdueItems)
        {
            item.LateDays = Math.Max(
                0,
                (today - item.DueAt.Date).Days
            );

            item.EstimatedFine =
                item.LateDays * DailyFineRateVnd;
        }

        var topBooks = await _reportRepository.GetTopBorrowedBooks(
            context,
            start,
            endExclusive,
            20
        );

        for (int i = 0; i < topBooks.Count; i++)
        {
            topBooks[i].Rank = i + 1;
        }

        var lowStockBooks = await _reportRepository.GetLowStockBooks(
            context,
            2
        );

        foreach (var item in lowStockBooks)
        {
            item.StatusText =
                item.TotalCopies == 0
                    ? "Chưa nhập bản sách"
                    : item.AvailableCopies == 0
                        ? "Hết sách trên kệ"
                        : "Sắp hết (còn 1)";
        }

        var fines = await _reportRepository.GetRecentFines(
            context,
            50
        );

        return new StatisticalReportDto
        {
            Kpi = kpi,
            MonthlyStats = monthlyStats,
            CategoryStats = categoryStats,
            OverdueItems = overdueItems,
            TopBooks = topBooks,
            LowStockBooks = lowStockBooks,
            FineDetails = fines
        };
    }

    public void ExportToCsv(
        string filePath,
        IEnumerable<string> headers,
        IEnumerable<IEnumerable<string>> rows)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Đường dẫn file không hợp lệ.");
        }

        var sb = new StringBuilder();

        sb.AppendLine(
            string.Join(",", headers.Select(EscapeCsvValue))
        );
        
        foreach (var row in rows)
        {
            sb.AppendLine(
                string.Join(",", row.Select(EscapeCsvValue))
            );
        }

        File.WriteAllText(
            filePath,
            sb.ToString(),
            new UTF8Encoding(true)
        );
    }

    private static string EscapeCsvValue(string value)
    {
        value ??= string.Empty;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
