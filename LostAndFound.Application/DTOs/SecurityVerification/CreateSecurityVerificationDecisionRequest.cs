namespace LostAndFound.Application.DTOs.SecurityVerification;

public class CreateSecurityVerificationDecisionRequest
{
    public int ClaimId { get; set; } // Claim nào được approve/reject
    public string Decision { get; set; } = null!; // APPROVED or REJECTED
    public string? Note { get; set; }
    public string? EvidenceImageUrl { get; set; } // Được set sau khi upload từ Controller
}

