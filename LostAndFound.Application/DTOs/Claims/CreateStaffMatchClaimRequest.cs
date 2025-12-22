namespace LostAndFound.Application.DTOs.Claims;

public class CreateStaffMatchClaimRequest
{
    public int LostReportId { get; set; }
    public int FoundItemId { get; set; }
}

