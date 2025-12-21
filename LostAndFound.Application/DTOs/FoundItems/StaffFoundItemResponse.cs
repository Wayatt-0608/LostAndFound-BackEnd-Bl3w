namespace LostAndFound.Application.DTOs.FoundItems;

public class StaffFoundItemResponse
{
    public int Id { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int CampusId { get; set; }
    public string? CampusName { get; set; }
    public string? Description { get; set; }
    public DateTime? FoundDate { get; set; }
    public string? FoundLocation { get; set; }
    public string? Status { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}

