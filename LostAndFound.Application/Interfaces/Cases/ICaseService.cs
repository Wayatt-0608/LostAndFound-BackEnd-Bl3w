using LostAndFound.Application.DTOs.Cases;
using LostAndFound.Domain.Entities;

namespace LostAndFound.Application.Interfaces.Cases;

public interface ICaseService
{
    Task<Case> CreateCaseForFoundItemAsync(int foundItemId, int campusId);
    Task<IEnumerable<CaseResponse>> GetAllAsync(int? campusId = null, string? status = null);
    Task<CaseResponse?> GetByIdAsync(int id);
    Task<CaseResponse?> UpdateStatusAsync(int id, string status);
}

