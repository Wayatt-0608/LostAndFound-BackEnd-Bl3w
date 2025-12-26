namespace LostAndFound.Application.DTOs.SecurityVerification;

public class SecurityVerificationRequestResponse
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string? CaseStatus { get; set; }
    public int? FoundItemId { get; set; }
    public string? FoundItemDescription { get; set; }
    public int RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<SecurityVerificationDecisionInfo> Decisions { get; set; } = new();
    public List<CaseClaimInfo> CaseClaims { get; set; } = new(); // Các claims trong case này
}

public class SecurityVerificationDecisionInfo
{
    public int Id { get; set; }
    public int SecurityOfficerId { get; set; }
    public string? SecurityOfficerName { get; set; }
    public string? Decision { get; set; }
    public string? Note { get; set; }
    public string? EvidenceImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CaseClaimInfo
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    public string? Status { get; set; }
    public string? EvidenceImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Found Item Info
    public int FoundItemId { get; set; }
    public string? FoundItemDescription { get; set; }
    public string? FoundItemImageUrl { get; set; }
    public string? FoundItemStatus { get; set; }
    public DateTime? FoundItemFoundDate { get; set; }
    public string? FoundItemFoundLocation { get; set; }
    public string? FoundItemCategoryName { get; set; }
    public string? FoundItemCampusName { get; set; }
    public string? FoundItemIdentifyingFeatures { get; set; }
    public string? FoundItemClaimPassword { get; set; }
    
    // Lost Report Info (nếu có match)
    public int? LostReportId { get; set; }
    public string? LostReportDescription { get; set; }
    public DateTime? LostReportLostDate { get; set; }
    public string? LostReportLostLocation { get; set; }
    public string? LostReportImageUrl { get; set; }
    public string? LostReportCategoryName { get; set; }
    public string? LostReportIdentifyingFeatures { get; set; }
    public string? LostReportClaimPassword { get; set; }
}

