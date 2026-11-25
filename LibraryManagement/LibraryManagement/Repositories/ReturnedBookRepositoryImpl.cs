using LibraryManagement.Data;
using LibraryManagement.DTO.Request;
using LibraryManagement.DTO.Response;
using LibraryManagement.Enums;
using LibraryManagement.Filters;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interafaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;

namespace LibraryManagement.Repositories;

public class ReturnedBookRepositoryImpl : ReturnedBookRepository
{
    public async Task<ReturnedBookReaderInfor?> GetReaderInforAsync(LibraryDbContext context, string UserCode)
    {
        return await context.Users
            .Where(u => u.UserCode == UserCode)
            .Select(u => new ReturnedBookReaderInfor
            {
                FullName = u.FullName,
                UserCode = u.UserCode,
                Email = u.Email,
                Phone = u.Phone
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(LibraryDbContext context, int id, ReturnedBookAddRequest request)
    {
        var detail = await context.LoanDetails
                         .Include(d => d.Loan)
                         .Include(d => d.BookCopy)
                         .FirstOrDefaultAsync(d => d.Id == id)
                     ?? throw new InvalidOperationException("Không tìm thấy chi tiết mượn.");

        // if(detail.ReturnedAt != null) throw new InvalidOperationException("Sách này đã trả rồi.");
        var returnedAt = DateTime.Now;

        detail.ReturnedAt = returnedAt;
        detail.PhysicalConditionAfter = request.PhysicalConditionAfter;

        detail.BookCopy.CirculationStatus =
            request.PhysicalConditionAfter == BookCondition.Lost
                ? CirculationStatus.Maintenance
                : CirculationStatus.Available;

        var daysLate = Math.Max(0, (returnedAt.Date - detail.DueAt.Date).Days);
        if (daysLate > 0)
        {
            var fine = new Fines
            {
                IdLoanDetail = detail.Id,
                FineCode = string.Empty,
                ViolationType = ViolationType.Overdue,
                AmountVnd = daysLate * 5000,
                CreatedAt = DateTime.UtcNow
            };

            context.Fines.Add(fine);
            
            await context.SaveChangesAsync();

            fine.FineCode = $"FC{fine.Id:D6}";
        }
        await context.SaveChangesAsync();
    }
    
    

    public async Task<List<ReturnedBookInfor>> GetReturnedBookInfor(LibraryDbContext context, ReturnedBookFilter Filter)
        {
            var today = DateTime.Today;

            var query = context.LoanDetails
                .Where(ld => ld.ReturnedAt == null);

            if (!string.IsNullOrWhiteSpace(Filter.keyWord))
            {
                var kw = Filter.keyWord.Trim();

                var isExactLoan = await context.Loans
                    .AnyAsync(l => l.LoanCode == kw);
                
                if (isExactLoan)
                    query = query.Where(ld => ld.Loan.LoanCode == kw);
                else
                    query = query.Where(ld => ld.Loan.Reader.UserCode == kw);
            }

            var raw = await query
                .OrderBy(ld => ld.DueAt)
                .Select(ld => new
                {
                    LoanDetailId = ld.Id,          
                    ld.Loan.LoanCode,
                    ld.BookCopy.Book.Title,
                    ISBN = ld.BookCopy.Book.Isbn,
                    ld.BookCopy.CopyCode,
                    ld.PhysicalConditionBefore,
                    ld.DueAt
                })
                .AsNoTracking()
                .ToListAsync();

            return raw.Select(x =>
            {
                var daysLate = Math.Max(0, (today - x.DueAt).Days);
                return new ReturnedBookInfor
                {
                    LoanDetailId = x.LoanDetailId, // <--- GÁN VÀO DTO Ở ĐÂY
                    LoanCode = x.LoanCode,
                    Title = x.Title,
                    CopyCode = x.CopyCode,
                    ISBN = x.ISBN,
                    PhysicalConditionBefore = x.PhysicalConditionBefore,
                    DueAt = x.DueAt,
                    ReturnedAt = DateTime.Today,
                    NumberDaysLate = daysLate,
                    PhysicalConditionAfter = BookCondition.Good,
                    AmountPayDaysLate = daysLate * 5000
                };
            }).ToList();
        }
    }