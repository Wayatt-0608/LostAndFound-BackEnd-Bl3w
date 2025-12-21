namespace LostAndFound.Application.DTOs.Cases;

public class CaseResponse
{
    public int Id { get; set; }
    public int FoundItemId { get; set; }
    public string? FoundItemDescription { get; set; }
    public string? FoundItemImageUrl { get; set; }
    public int CampusId { get; set; }
    public string? CampusName { get; set; }
    public string? Status { get; set; }
    public int? TotalClaims { get; set; }
    public int? SuccessfulClaimId { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public List<CaseClaimInfo> Claims { get; set; } = new();
    public List<CaseVerificationRequestInfo> VerificationRequests { get; set; } = new();
}

public class CaseClaimInfo
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CaseVerificationRequestInfo
{
    public int Id { get; set; }
    public int RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public DateTime? CreatedAt { get; set; }
}

