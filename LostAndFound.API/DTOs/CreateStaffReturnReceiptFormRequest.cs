using Microsoft.AspNetCore.Http;

namespace LostAndFound.API.DTOs;

public class CreateStaffReturnReceiptFormRequest
{
    public int CaseId { get; set; }
    public int ClaimId { get; set; }
    public IFormFile? ReceiptImage { get; set; }
}

