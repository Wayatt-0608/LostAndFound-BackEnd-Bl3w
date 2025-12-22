namespace LostAndFound.Application.DTOs.Claims;

public class StudentClaimResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    public int FoundItemId { get; set; }
    public string? FoundItemDescription { get; set; }
    public string? FoundItemImageUrl { get; set; }
    public string? FoundItemStatus { get; set; }
    public int? LostReportId { get; set; }
    public string? LostReportDescription { get; set; }
    public int? CaseId { get; set; }
    public string? CaseStatus { get; set; }
    public string? Status { get; set; }
    public string? EvidenceImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}

