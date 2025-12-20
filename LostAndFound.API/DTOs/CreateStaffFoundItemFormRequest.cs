using Microsoft.AspNetCore.Http;

namespace LostAndFound.API.DTOs;

public class CreateStaffFoundItemFormRequest
{
    public int? CategoryId { get; set; }
    public int CampusId { get; set; }
    public string? Description { get; set; }
    public DateTime? FoundDate { get; set; }
    public IFormFile? Image { get; set; }
}

