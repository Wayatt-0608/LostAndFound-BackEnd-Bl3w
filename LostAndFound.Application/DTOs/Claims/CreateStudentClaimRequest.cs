namespace LostAndFound.Application.DTOs.Claims;

public class CreateStudentClaimRequest
{
    public int FoundItemId { get; set; }
    public int? LostReportId { get; set; } // Optional: có thể link với lost report
}

