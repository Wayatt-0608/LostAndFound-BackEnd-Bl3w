namespace LostAndFound.Application.DTOs.ReturnReceipts;

public class StaffReturnReceiptResponse
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string? CaseStatus { get; set; }
    public int? FoundItemId { get; set; }
    public string? FoundItemDescription { get; set; }
    public int ClaimId { get; set; }
    public int? ClaimStudentId { get; set; }
    public string? ClaimStudentName { get; set; }
    public string? ClaimStudentCode { get; set; }
    public string? ClaimStatus { get; set; }
    public int StaffId { get; set; }
    public string? StaffName { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? ReceiptImageUrl { get; set; }
}

