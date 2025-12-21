namespace LostAndFound.Application.DTOs.FoundItems;

public class UpdateStaffFoundItemStatusRequest
{
    public string Status { get; set; } = null!; // STORED or RETURNED
}

