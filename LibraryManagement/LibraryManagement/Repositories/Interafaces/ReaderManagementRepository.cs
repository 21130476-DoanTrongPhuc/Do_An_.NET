using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;

namespace LibraryManagement.Repositories.Interafaces;

public interface ReaderManagementRepository
{
    Task<ReaderListSummaryResponse> GetReaders(LibraryDbContext context, ReaderFilter filter);
    Task<ReaderDetailResponse?> GetReaderDetail(LibraryDbContext context, int readerId);
    Task UpdateReaderInfo(LibraryDbContext context, ReaderUpdateRequest request);
    Task<DateTime> ExtendOrIssueCard(LibraryDbContext context, int readerId, int months);
    Task<(string FullName, bool IsNowActive)> ToggleLockAccount(LibraryDbContext context, int readerId);
}