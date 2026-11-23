using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories;

public class ManageOverdueRepositoryImpl : ManageOverdueBookRepository
{
    public async Task<List<ManageOverdueBookInfo>> GetManageOverdueBooks(LibraryDbContext context, ManageOverdueBookFilter filter)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        var query = context.LoanDetails
            .Where(ld => ld.ReturnedAt == null && ld.DueAt.Date < today);

        if (!string.IsNullOrWhiteSpace(filter.KeyWord))
        {
            var kw = filter.KeyWord.Trim();

            query = query.Where(ld =>
                    ld.Loan.LoanCode.Contains(kw) ||
                    ld.Loan.Reader.UserCode.Contains(kw) ||
                    ld.Loan.Reader.FullName.Contains(kw) ||
                    (ld.Loan.Reader.Phone != null &&
                     ld.Loan.Reader.Phone.Contains(kw)) ||
                    (ld.Loan.Reader.Email != null &&
                     ld.Loan.Reader.Email.Contains(kw)) ||
                    ld.BookCopy.CopyCode.Contains(kw)
            );
        }
        
        if (filter.DueFrom.HasValue &&
            filter.DueTo.HasValue &&
            filter.DueFrom.Value.Date > filter.DueTo.Value.Date)
        {
            throw new ArgumentException(
                "Ngày bắt đầu hạn trả không được lớn hơn ngày kết thúc."
            );
        }

        switch (filter.OverduePeriod)
        {
            case OverduePeriod.All:
                break;
            
            case OverduePeriod.From1To7Days:

                query = query.Where(ld =>
                    ld.DueAt.Date >= today.AddDays(-7) &&
                    ld.DueAt.Date < today);

                break;
            
            case OverduePeriod.From8To30Days:

                query = query.Where(ld =>
                    ld.DueAt.Date >= today.AddDays(-30) &&
                    ld.DueAt.Date < today.AddDays(-7));

                break;
            
            case OverduePeriod.Over30Days:

                query = query.Where(ld =>
                    ld.DueAt.Date < today.AddDays(-30));

                break;
        }

        if (filter.DueFrom.HasValue)
        {
            var dueFrom = filter.DueFrom.Value.Date;

            query = query.Where(ld =>
                ld.DueAt.Date >= dueFrom);
        }

        if (filter.DueTo.HasValue)
        {
            var dueTo = filter.DueTo.Value.Date;

            query = query.Where(ld =>
                ld.DueAt.Date <= dueTo);
        }
        
        if (filter.BorrowedFrom.HasValue)
        {
            var borrowedFrom = filter.BorrowedFrom.Value.Date;

            query = query.Where(ld =>
                ld.Loan.BorrowedAt.Date >= borrowedFrom);
        }

        if (filter.BorrowedTo.HasValue)
        {
            var borrowedTo = filter.BorrowedTo.Value.Date;

            query = query.Where(ld =>
                ld.Loan.BorrowedAt.Date <= borrowedTo);
        }
        
        switch (filter.Status)
        {
            case ReminderFilterStatus.All:
                break;
            
            case ReminderFilterStatus.NeverReminded:

                query = query.Where(ld =>
                    !context.Reminders.Any(r =>
                        r.IdLoanDetail == ld.Id &&
                        r.Status == ReminderStatus.Sent));

                break;
            
            case ReminderFilterStatus.RemindedToday:

                query = query.Where(ld =>
                    context.Reminders.Any(r =>
                        r.IdLoanDetail == ld.Id &&
                        r.Status == ReminderStatus.Sent));

                break;
            
            case ReminderFilterStatus.NotRemindedToday:

                query = query.Where(ld =>
                    !context.Reminders.Any(r =>
                        r.IdLoanDetail == ld.Id &&
                        r.Status == ReminderStatus.Sent &&
                        r.SentAt >= today && 
                        r.SentAt < tomorrow));

                break;
        }
        
        var raw = await query
            .OrderBy(ld => ld.DueAt)
            .Select(ld => new
            {
                LoanDetailId = ld.Id,
                ld.Loan.LoanCode,
                ld.Loan.Reader.UserCode,
                ld.Loan.Reader.FullName,
                ld.Loan.Reader.Phone,
                ld.Loan.Reader.Email,
                ld.BookCopy.CopyCode,
                ld.BookCopy.Book.Title,
                ld.Loan.BorrowedAt,
                ld.DueAt,
                
                LatestReminderAt = context.Reminders
                    .Where(r =>
                        r.IdLoanDetail == ld.Id &&
                        r.Status == ReminderStatus.Sent)
                    .Max(r => (DateTime?)r.SentAt)
            })
            .AsNoTracking()
            .ToListAsync();
        
        return raw.Select(x =>
        {
            ReminderEmailPhone status;

            if (x.LatestReminderAt == null)
            {
                status = ReminderEmailPhone.NeverReminded;
            }
            else if (x.LatestReminderAt.Value.Date == DateTime.Today)
            {
                status = ReminderEmailPhone.RemindedToday;
            }
            else
            {
                status = ReminderEmailPhone.RemindedBefore;
            }

            
            var overdueDay = Math.Max(0, (DateTime.Today - x.DueAt).Days);
            return new ManageOverdueBookInfo()
            {
                IdLoanDetail = x.LoanDetailId,
                LoanCode = x.LoanCode,
                UserCode = x.UserCode,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.Phone,
                CopyCode = x.CopyCode,
                Title = x.Title,
                BorrowedDay = x.BorrowedAt,
                DueDay = x.DueAt,
                OverdueDays = overdueDay,
                FinePay = 5000*overdueDay,
                ReminderStatus = status,
                LatestReminderAt = x.LatestReminderAt
            };
        }).ToList();
    }

    public async  Task AddReminderToUser(LibraryDbContext context, int idLoanDetail, ReminderChannel channel, string content)
    {
        var now = DateTime.Now;

        var reminders = new List<Reminders>();

        if (channel == ReminderChannel.Email || channel == ReminderChannel.Both)
        {
            reminders.Add(new Reminders
            {
                IdLoanDetail = idLoanDetail,
                Channel = ReminderChannel.Email,
                Status = ReminderStatus.Sent,
                SentAt = now,
                Content = content,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (channel == ReminderChannel.SMS || channel == ReminderChannel.Both)
        {
            reminders.Add(new Reminders
            {
                IdLoanDetail = idLoanDetail,
                Channel = ReminderChannel.SMS,
                Status = ReminderStatus.Sent,
                SentAt = now,
                Content = content,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await context.Reminders.AddRangeAsync(reminders);
        await context.SaveChangesAsync();
    }
}