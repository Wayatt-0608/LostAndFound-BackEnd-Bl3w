using Microsoft.AspNetCore.Http;

namespace LostAndFound.API.DTOs;

public class CreateSecurityVerificationDecisionFormRequest
{
    public int ClaimId { get; set; }
    public string Decision { get; set; } = null!; // APPROVED or REJECTED
    public string? Note { get; set; }
    public IFormFile? EvidenceImage { get; set; }
}

