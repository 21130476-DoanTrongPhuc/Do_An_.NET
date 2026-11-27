using LibraryManagement.Data;
using LibraryManagement.DTO.Response;

namespace LibraryManagement.Repositories.Interafaces;

public interface StatisticalReportRepository
{
    Task<KpiSummaryDto> GetKpiSummary(LibraryDbContext context, DateTime fromDate, DateTime toDate, DateTime today);
    Task<List<MonthlyStatDto>> GetMonthlyStats(LibraryDbContext context,int year);
    Task<List<CategoryStatDto>> GetCategoryStats(LibraryDbContext context);
    Task<List<OverdueItemDto>> GetOverdueItems(LibraryDbContext context,DateTime today);
    Task<List<TopBookDto>> GetTopBorrowedBooks(LibraryDbContext context,DateTime fromDate, DateTime toDate, int topCount = 20);
    Task<List<LowStockBookDto>> GetLowStockBooks(LibraryDbContext context,int threshold = 2);
    Task<List<FineDetailDto>> GetRecentFines(LibraryDbContext context,int limit = 50);
}