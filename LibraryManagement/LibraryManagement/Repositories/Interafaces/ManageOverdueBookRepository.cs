using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;

namespace LibraryManagement.Repositories.Interafaces;

public interface ManageOverdueBookRepository
{
    Task<List<ManageOverdueBookInfo>> GetManageOverdueBooks(LibraryDbContext context, ManageOverdueBookFilter filter);
    Task AddReminderToUser(LibraryDbContext context, int idLoanDetail, ReminderChannel channel, string content);
}