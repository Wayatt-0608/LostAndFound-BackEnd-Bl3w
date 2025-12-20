using LostAndFound.Application.DTOs.ReturnReceipts;
using LostAndFound.Application.Interfaces.ReturnReceipts;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.ReturnReceipts;

public class StaffReturnReceiptService : IStaffReturnReceiptService
{
    private readonly AppDbContext _context;

    public StaffReturnReceiptService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StaffReturnReceiptResponse> CreateAsync(int staffId, CreateStaffReturnReceiptRequest request)
    {
        // Kiểm tra Case có tồn tại không
        var caseEntity = await _context.Cases
            .Include(c => c.FoundItem)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId);
        
        if (caseEntity == null)
        {
            throw new ArgumentException("Không tìm thấy case.");
        }

        // Kiểm tra Claim có tồn tại và thuộc về case này không
        var claim = await _context.StudentClaims
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.Id == request.ClaimId && c.CaseId == request.CaseId);
        
        if (claim == null)
        {
            throw new ArgumentException("Không tìm thấy claim hoặc claim không thuộc về case này.");
        }

        // Kiểm tra claim đã được approve chưa (phải được approve trước khi trả đồ)
        if (claim.Status != "APPROVED")
        {
            throw new InvalidOperationException("Chỉ có thể tạo biên bản trả đồ cho claim đã được approve.");
        }

        // Kiểm tra đã có return receipt cho claim này chưa
        var existingReceipt = await _context.StaffReturnReceipts
            .FirstOrDefaultAsync(r => r.ClaimId == request.ClaimId);
        
        if (existingReceipt != null)
        {
            throw new InvalidOperationException("Đã có biên bản trả đồ cho claim này rồi.");
        }

        // Tạo return receipt
        var receipt = new StaffReturnReceipt
        {
            CaseId = request.CaseId,
            ClaimId = request.ClaimId,
            StaffId = staffId,
            ReceiptImageUrl = request.ReceiptImageUrl,
            ReturnedAt = DateTime.Now
        };

        _context.StaffReturnReceipts.Add(receipt);

        // Tự động update: case.status = 'COMPLETED', found_item.status = 'RETURNED', claim.status = 'APPROVED'
        // (claim.status đã là 'APPROVED' rồi, nhưng đảm bảo nó được set)
        caseEntity.Status = "COMPLETED";
        caseEntity.ClosedAt = DateTime.Now;
        _context.Cases.Update(caseEntity);

        caseEntity.FoundItem.Status = "RETURNED";
        _context.StaffFoundItems.Update(caseEntity.FoundItem);

        claim.Status = "APPROVED"; // Đảm bảo claim status là APPROVED
        _context.StudentClaims.Update(claim);

        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRelatedDataAsync(receipt);

        return MapToResponse(receipt);
    }

    public async Task<IEnumerable<StaffReturnReceiptResponse>> GetAllAsync()
    {
        var receipts = await _context.StaffReturnReceipts
            .Include(r => r.Case)
                .ThenInclude(c => c.FoundItem)
            .Include(r => r.Claim)
                .ThenInclude(c => c.Student)
            .Include(r => r.Staff)
            .OrderByDescending(r => r.ReturnedAt)
            .ToListAsync();

        return receipts.Select(MapToResponse);
    }

    public async Task<StaffReturnReceiptResponse?> GetByIdAsync(int id)
    {
        var receipt = await _context.StaffReturnReceipts
            .Include(r => r.Case)
                .ThenInclude(c => c.FoundItem)
            .Include(r => r.Claim)
                .ThenInclude(c => c.Student)
            .Include(r => r.Staff)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt == null)
            return null;

        return MapToResponse(receipt);
    }

    private async Task LoadRelatedDataAsync(StaffReturnReceipt receipt)
    {
        await _context.Entry(receipt)
            .Reference(r => r.Case)
            .LoadAsync();

        if (receipt.Case != null)
        {
            await _context.Entry(receipt.Case)
                .Reference(c => c.FoundItem)
                .LoadAsync();
        }

        await _context.Entry(receipt)
            .Reference(r => r.Claim)
            .LoadAsync();

        if (receipt.Claim != null)
        {
            await _context.Entry(receipt.Claim)
                .Reference(c => c.Student)
                .LoadAsync();
        }

        await _context.Entry(receipt)
            .Reference(r => r.Staff)
            .LoadAsync();
    }

    private static StaffReturnReceiptResponse MapToResponse(StaffReturnReceipt receipt)
    {
        return new StaffReturnReceiptResponse
        {
            Id = receipt.Id,
            CaseId = receipt.CaseId,
            CaseStatus = receipt.Case?.Status,
            FoundItemId = receipt.Case?.FoundItemId,
            FoundItemDescription = receipt.Case?.FoundItem?.Description,
            ClaimId = receipt.ClaimId,
            ClaimStudentId = receipt.Claim?.StudentId,
            ClaimStudentName = receipt.Claim?.Student?.FullName,
            ClaimStudentCode = receipt.Claim?.Student?.StudentCode,
            ClaimStatus = receipt.Claim?.Status,
            StaffId = receipt.StaffId,
            StaffName = receipt.Staff?.FullName,
            ReturnedAt = receipt.ReturnedAt,
            ReceiptImageUrl = receipt.ReceiptImageUrl
        };
    }
}

