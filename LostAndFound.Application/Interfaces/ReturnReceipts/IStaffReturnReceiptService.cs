using LostAndFound.Application.DTOs.ReturnReceipts;

namespace LostAndFound.Application.Interfaces.ReturnReceipts;

public interface IStaffReturnReceiptService
{
    Task<StaffReturnReceiptResponse> CreateAsync(int staffId, CreateStaffReturnReceiptRequest request);
    Task<IEnumerable<StaffReturnReceiptResponse>> GetAllAsync();
    Task<StaffReturnReceiptResponse?> GetByIdAsync(int id);
}

