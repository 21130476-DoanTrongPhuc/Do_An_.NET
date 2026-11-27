using LibraryManagement.Data;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class StatisticalReportRepositoryImpl : StatisticalReportRepository
{
    public async Task<KpiSummaryDto> GetKpiSummary(LibraryDbContext context, DateTime fromDate, DateTime toDate, DateTime today)
    {
        int totalBooks = await context.Books.CountAsync();
        int availableCopies = await context.BookCopies.CountAsync(c => c.CirculationStatus == CirculationStatus.Available);

        int borrowedInRange = await context.LoanDetails.CountAsync(ld => ld.Loan.BorrowedAt >= fromDate && ld.Loan.BorrowedAt <= toDate);
        int returnedInRange = await context.LoanDetails.CountAsync(ld => ld.ReturnedAt >= fromDate && ld.ReturnedAt <= toDate);

        int currentOverdue = await context.LoanDetails.CountAsync(ld => ld.ReturnedAt == null && ld.DueAt.Date < today);

        int totalFineAmount = await context.Fines.SumAsync(f => f.AmountVnd);
        int totalFineCount =  await context.Fines.CountAsync();
        
        return new KpiSummaryDto
        {
            TotalBooks = totalBooks,
            AvailableCopies = availableCopies,
            BorrowedInRange = borrowedInRange,
            ReturnedInRange = returnedInRange,
            CurrentOverdue = currentOverdue,
            TotalFineAmount = totalFineAmount,
            TotalFineCount = totalFineCount
        };
    }

    public async Task<List<MonthlyStatDto>> GetMonthlyStats(LibraryDbContext context, int year)
    {
        var yearDetails = await context.LoanDetails
            .Where(ld => ld.Loan.BorrowedAt.Year == year || (ld.ReturnedAt != null && ld.ReturnedAt.Value.Year == year))
            .Select(ld => new { ld.Loan.BorrowedAt, ld.ReturnedAt })
            .ToListAsync();

        var result = new List<MonthlyStatDto>(12);
        for (int m = 1; m <= 12; m++)
        {
            int bCount = yearDetails.Count(x => x.BorrowedAt.Year == year && x.BorrowedAt.Month == m);
            int rCount = yearDetails.Count(x => x.ReturnedAt != null && x.ReturnedAt.Value.Year == year && x.ReturnedAt.Value.Month == m);
            
            result.Add(new MonthlyStatDto
            {
                Label = $"T{m}",
                BorrowCount = bCount,
                ReturnCount = rCount
            });
        }

        return result;
    }

    public async Task<List<CategoryStatDto>> GetCategoryStats(LibraryDbContext context)
    {
        return await context.Books
            .GroupBy(b => b.Category != null ? b.Category.CategoryName : "Chưa phân loại")
            .Select(g => new CategoryStatDto
            {
                CategoryName = g.Key,
                BookCount = g.Count()
            })
            .OrderByDescending(x => x.BookCount)
            .ToListAsync();
    }

    public async Task<List<OverdueItemDto>> GetOverdueItems(LibraryDbContext context, DateTime today)
    {
        return await context.LoanDetails
            .Where(ld => ld.ReturnedAt == null && ld.DueAt.Date < today)
            .OrderBy(ld => ld.DueAt)
            .Select(ld => new OverdueItemDto
            {
                LoanCode = ld.Loan.LoanCode,
                ReaderCode = ld.Loan.Reader.UserCode,
                ReaderName = ld.Loan.Reader.FullName,
                BookTitle = ld.BookCopy.Book.Title,
                CopyCode = ld.BookCopy.CopyCode,
                BorrowedAt = ld.Loan.BorrowedAt,
                DueAt = ld.DueAt
            })
            .ToListAsync();
    }

    public async Task<List<TopBookDto>> GetTopBorrowedBooks(LibraryDbContext context, DateTime fromDate, DateTime toDate, int topCount = 20)
    {
        var topBooks = await context.LoanDetails
            .Where(ld => ld.Loan.BorrowedAt >= fromDate && ld.Loan.BorrowedAt <= toDate)
            .GroupBy(ld => new
            {
                ld.BookCopy.Book.Id,
                ld.BookCopy.Book.Title,
                CategoryName = ld.BookCopy.Book.Category != null ? ld.BookCopy.Book.Category.CategoryName : "Khác"
            })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Title,
                g.Key.CategoryName,
                BorrowCount = g.Count()
            })
            .OrderByDescending(x => x.BorrowCount)
            .Take(topCount)
            .ToListAsync();
       
        var bookIds = topBooks.Select(b => b.Id).ToList();
        var copyCounts = context.BookCopies
            .Where(c => bookIds.Contains(c.IdBook))
            .GroupBy(c => c.IdBook)
            .Select(g => new
            {
                BookId = g.Key,
                Total = g.Count(),
                Available = g.Count(c => c.CirculationStatus == CirculationStatus.Available)
            })
            .ToDictionary(x => x.BookId);

        return topBooks.Select(b =>
        {
            copyCounts.TryGetValue(b.Id, out var counts);
            return new TopBookDto
            {
                BookId = b.Id,
                Title = b.Title,
                CategoryName = b.CategoryName,
                BorrowCount = b.BorrowCount,
                AvailableCopies = counts?.Available ?? 0,
                TotalCopies = counts?.Total ?? 0
            };
        }).ToList();
    }

    public async Task<List<LowStockBookDto>> GetLowStockBooks(LibraryDbContext context, int threshold = 2)
    {
        return await context.Books
            .Select(b => new LowStockBookDto
            {
                Title = b.Title,
                CategoryName = b.Category != null ? b.Category.CategoryName : "Khác",
                TotalCopies = b.Copies.Count(),
                AvailableCopies = b.Copies.Count(c => c.CirculationStatus == CirculationStatus.Available),
                BorrowedCopies = b.Copies.Count(c => c.CirculationStatus == CirculationStatus.Borrowed)
            })
            .Where(x => x.AvailableCopies < threshold)
            .OrderBy(x => x.AvailableCopies)
            .ToListAsync();
    }

    public async Task<List<FineDetailDto>> GetRecentFines(LibraryDbContext context, int limit = 50)
    {
        return await context.Fines
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(limit)
            .Select(f => new FineDetailDto
            {
                FineCode = $"PT-{f.Id:D4}",
                LoanCode = f.LoanDetail != null && f.LoanDetail.Loan != null ? f.LoanDetail.Loan.LoanCode : "N/A",
                ReaderName = f.LoanDetail != null && f.LoanDetail.Loan != null && f.LoanDetail.Loan.Reader != null
                    ? f.LoanDetail.Loan.Reader.FullName : "N/A",
                BookTitle = f.LoanDetail != null && f.LoanDetail.BookCopy != null && f.LoanDetail.BookCopy.Book != null
                    ? f.LoanDetail.BookCopy.Book.Title : "N/A",
                Reason = f.ViolationType == ViolationType.Other ? "Vi phạm quy định mượn trả" : f.ViolationType.ToString(),
                AmountVnd = f.AmountVnd
            })
            .ToListAsync();
    }
}