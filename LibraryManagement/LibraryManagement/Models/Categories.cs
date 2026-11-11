namespace LibraryManagement.Models;

public class Categories
{
    public int Id { get; set; }
    
    public int? IdParentCategory { get; set; }
    
    public string CategoryCode { get; set; } = string.Empty!;
    
    public string CategoryName { get; set; } = string.Empty!;
    
    public int MaxLoanDays { get; set; }
    
    public int MaxRenewals { get; set; }

    public string Status { get; set; } = string.Empty;
    
    public string CreatedAt { get; set; } = string.Empty;
    
    public string UpdatedAt { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    
    public Categories? ParentCategory { get; set; }
    
    public ICollection<Categories> Children { get; set; } = new List<Categories>();
    
    
    public ICollection<Book> Books { get; set; } = new List<Book>();
}