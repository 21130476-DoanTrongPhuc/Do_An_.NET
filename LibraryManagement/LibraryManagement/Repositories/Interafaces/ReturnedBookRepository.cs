using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Filters;

namespace LibraryManagement.Repositories.Interafaces;

public interface ReturnedBookRepository
{
    Task<ReturnedBookReaderInfor?> GetReaderInforAsync(LibraryDbContext context, String UserCode);
    Task UpdateAsync(LibraryDbContext context, int id, ReturnedBookAddRequest request);
    Task<List<ReturnedBookInfor>> GetReturnedBookInfor(LibraryDbContext context, ReturnedBookFilter Filter);
}