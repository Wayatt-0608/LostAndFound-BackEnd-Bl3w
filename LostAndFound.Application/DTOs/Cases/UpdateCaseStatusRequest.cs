namespace LostAndFound.Application.DTOs.Cases;

public class UpdateCaseStatusRequest
{
    public string Status { get; set; } = null!; // OPEN, IN_PROGRESS, COMPLETED, FAILED
}

