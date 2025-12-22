using Microsoft.AspNetCore.Http;

namespace LostAndFound.API.DTOs;

public class UpdateStudentClaimEvidenceFormRequest
{
    public IFormFile EvidenceImage { get; set; } = null!;
}

