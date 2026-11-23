namespace LibraryManagement.DTO.Response;

public class FineManagementInfo
{
    public int FineId { get; set; }
    
    public string FineCode { get; set; } = "";

    public string? LoanCode { get; set; }

    public string ReaderCode { get; set; } = "";

    public string ReaderName { get; set; } = "";

    public string? BookTitle { get; set; }

    public string? CopyBarcode { get; set; }

    public string ViolationType { get; set; } = "";

    public string? Description { get; set; }

    /// <summary>Ngày tạo — cột "Ngày tạo" (colCreated)</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Người lập — cột "Người lập" (colBy)</summary>
    public string CreatedByName { get; set; } = "";

    /// <summary>Tiền phạt — cột "Tiền phạt" (colAmount)</summary>
    public int AmountVnd { get; set; }

    /// <summary>Đã trả — cột "Đã trả" (colPaid)</summary>
    public int PaidVnd { get; set; }

    /// <summary>Còn nợ — cột "Còn nợ" (colDebt)</summary>
    public int RemainingVnd => Math.Max(0, AmountVnd - PaidVnd);

    /// <summary>Trạng thái thanh toán — cột "Trạng thái" (colStatus)</summary>
    public string Status { get; set; } = "";
}