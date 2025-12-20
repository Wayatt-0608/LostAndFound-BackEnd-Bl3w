namespace LostAndFound.Application.DTOs.ReturnReceipts;

public class CreateStaffReturnReceiptRequest
{
    public int CaseId { get; set; }
    public int ClaimId { get; set; }
    public string? ReceiptImageUrl { get; set; } // Được set sau khi upload từ Controller
}

