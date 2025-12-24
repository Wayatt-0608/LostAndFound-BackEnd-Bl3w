using LostAndFound.Application.DTOs.FoundItems;

namespace LostAndFound.Application.Interfaces.FoundItems;

public interface IStaffFoundItemService
{
    Task<StaffFoundItemResponse> CreateAsync(int createdBy, CreateStaffFoundItemRequest request);
    Task<IEnumerable<StaffFoundItemResponse>> GetAllAsync(int? campusId = null, string? status = null, int? categoryId = null, bool includeSensitiveData = true);
    Task<StaffFoundItemResponse?> GetByIdAsync(int id, bool includeSensitiveData = true);
    Task<StaffFoundItemResponse?> UpdateAsync(int id, UpdateStaffFoundItemRequest request);
    Task<StaffFoundItemResponse?> UpdateStatusAsync(int id, string status);
}

