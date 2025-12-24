namespace LostAndFound.Application.DTOs.FoundItems;

public class UpdateStaffFoundItemRequest
{
    public int? CategoryId { get; set; }
    public int? CampusId { get; set; }
    public string? Description { get; set; }
    public DateTime? FoundDate { get; set; }
    public string? FoundLocation { get; set; }
    public string? ImageUrl { get; set; } // Được set sau khi upload từ Controller
    public string? IdentifyingFeatures { get; set; }
    public string? ClaimPassword { get; set; } // Plain text, will be hashed in service
}

