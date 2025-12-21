using Microsoft.AspNetCore.Http;

namespace LostAndFound.API.DTOs;

public class UpdateStudentLostReportFormRequest
{
    public int? CategoryId { get; set; }
    public string? Description { get; set; }
    public DateTime? LostDate { get; set; }
    public string? LostLocation { get; set; }
    public IFormFile? Image { get; set; }
}

