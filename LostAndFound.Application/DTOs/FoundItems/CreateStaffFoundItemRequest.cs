namespace LostAndFound.Application.DTOs.FoundItems;

public class CreateStaffFoundItemRequest
{
    public int? CategoryId { get; set; }
    public int CampusId { get; set; }
    public string? Description { get; set; }
    public DateTime? FoundDate { get; set; }
    public string? ImageUrl { get; set; } // Được set sau khi upload từ Controller
}

