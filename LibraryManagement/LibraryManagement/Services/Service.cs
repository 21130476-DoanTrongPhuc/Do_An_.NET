using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    public class Service
    {
        public Readers GetCurrentReader(string readerId)
        {
            return new Readers
            {
                ReaderId = readerId,
                FullName = "Nguyễn Minh Anh",
                CardExpiryDate = new DateTime(2025, 8, 12)
            };
        }

        public HomeSummary GetHomeSummary(string readerId)
        {
            return new HomeSummary
            {
                BorrowingBooksCount = 2,
                DaysToRenewal = 4,
                FavoriteBooksCount = 6,
                ReadBooksCount = 24
            };
        }
    }
    
}
