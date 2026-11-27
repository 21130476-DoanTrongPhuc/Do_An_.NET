namespace LibraryManagement.DTO.Response;

public class StatisticalReportDto
{
    public KpiSummaryDto Kpi { get; set; } = new();
    public List<MonthlyStatDto> MonthlyStats { get; set; } = new();
    public List<CategoryStatDto> CategoryStats { get; set; } = new();
    public List<OverdueItemDto> OverdueItems { get; set; } = new();
    public List<TopBookDto> TopBooks { get; set; } = new();
    public List<LowStockBookDto> LowStockBooks { get; set; } = new();
    public List<FineDetailDto> FineDetails { get; set; } = new();
}