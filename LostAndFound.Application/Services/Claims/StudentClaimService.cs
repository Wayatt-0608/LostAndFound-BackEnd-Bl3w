using LostAndFound.Application.DTOs.Claims;
using LostAndFound.Application.Interfaces.Claims;
using LostAndFound.Application.Interfaces.Notifications;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.Claims;

public class StudentClaimService : IStudentClaimService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public StudentClaimService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<StudentClaimResponse> CreateAsync(int studentId, CreateStudentClaimRequest request)
    {
        // Kiểm tra Found Item có tồn tại không
        var foundItem = await _context.StaffFoundItems
            .FirstOrDefaultAsync(f => f.Id == request.FoundItemId);
        
        if (foundItem == null)
        {
            throw new ArgumentException("Không tìm thấy đồ nhặt được.");
        }

        // Kiểm tra Found Item chưa được trả (status phải là STORED)
        if (foundItem.Status != "STORED")
        {
            throw new InvalidOperationException("Đồ nhặt được này đã được trả lại.");
        }

        // Kiểm tra Lost Report có thuộc về student này không (nếu có)
        if (request.LostReportId.HasValue)
        {
            var lostReport = await _context.StudentLostReports
                .FirstOrDefaultAsync(lr => lr.Id == request.LostReportId.Value && lr.StudentId == studentId);
            
            if (lostReport == null)
            {
                throw new ArgumentException("Lost Report không tồn tại hoặc không thuộc về bạn.");
            }
        }

        // Kiểm tra xem student đã claim found item này chưa
        var existingClaim = await _context.StudentClaims
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.FoundItemId == request.FoundItemId);
        
        if (existingClaim != null)
        {
            throw new InvalidOperationException("Bạn đã claim đồ nhặt được này rồi.");
        }

        // Tìm Case liên quan đến Found Item (load cả FoundItem và Campus để lấy thông tin)
        var caseEntity = await _context.Cases
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Campus)
            .FirstOrDefaultAsync(c => c.FoundItemId == request.FoundItemId);
        
        if (caseEntity == null)
        {
            throw new InvalidOperationException("Không tìm thấy case cho đồ nhặt được này.");
        }

        // Tạo claim mới
        var claim = new StudentClaim
        {
            StudentId = studentId,
            FoundItemId = request.FoundItemId,
            LostReportId = request.LostReportId,
            CaseId = caseEntity.Id,
            Status = "PENDING",
            CreatedAt = DateTime.Now
        };

        _context.StudentClaims.Add(claim);
        await _context.SaveChangesAsync();

        // Tự động update cases.total_claims++ và chuyển status sang IN_PROGRESS nếu đang ở OPEN
        caseEntity.TotalClaims = (caseEntity.TotalClaims ?? 0) + 1;
        if (caseEntity.Status == "OPEN" || string.IsNullOrEmpty(caseEntity.Status))
        {
            caseEntity.Status = "IN_PROGRESS";
        }
        _context.Cases.Update(caseEntity);
        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRelatedDataAsync(claim);

        // Gửi thông báo cho Student: Yêu cầu lên xác minh và nhận lại đồ
        var campusName = caseEntity.FoundItem?.Campus?.Name ?? "phòng ban";
        var campusDisplayName = campusName != "phòng ban" ? $"cơ sở {campusName}" : campusName;
        var foundItemDescription = caseEntity.FoundItem?.Description ?? "đồ vật";
        try
        {
            await _notificationService.CreateNotificationAsync(
                userId: studentId,
                title: "Yêu cầu xác minh và nhận lại đồ",
                message: $"Claim của bạn cho đồ vật \"{foundItemDescription}\" đã được tạo. Vui lòng lên {campusDisplayName} để xác minh và nhận lại đồ.",
                type: "NEW_CLAIM",
                relatedEntityId: claim.Id,
                relatedEntityType: "CLAIM"
            );
        }
        catch (Exception)
        {
            // Log lỗi nhưng không làm fail toàn bộ flow
        }

        return MapToResponse(claim);
    }

    public async Task<StudentClaimResponse> CreateClaimForStudentAsync(int lostReportId, int foundItemId)
    {
        // Lấy lost report
        var lostReport = await _context.StudentLostReports
            .FirstOrDefaultAsync(lr => lr.Id == lostReportId);
        
        if (lostReport == null)
        {
            throw new ArgumentException("Không tìm thấy báo mất.");
        }

        // Kiểm tra Found Item có tồn tại không
        var foundItem = await _context.StaffFoundItems
            .FirstOrDefaultAsync(f => f.Id == foundItemId);
        
        if (foundItem == null)
        {
            throw new ArgumentException("Không tìm thấy đồ nhặt được.");
        }

        // Kiểm tra Found Item chưa được trả (status phải là STORED)
        if (foundItem.Status != "STORED")
        {
            throw new InvalidOperationException("Đồ nhặt được này đã được trả lại.");
        }

        // Kiểm tra student đã claim found item này chưa
        var existingClaim = await _context.StudentClaims
            .FirstOrDefaultAsync(c => c.StudentId == lostReport.StudentId && c.FoundItemId == foundItemId);
        
        if (existingClaim != null)
        {
            throw new InvalidOperationException("Sinh viên này đã claim đồ nhặt được này rồi.");
        }

        // Tìm Case liên quan đến Found Item (load cả FoundItem và Campus)
        var caseEntity = await _context.Cases
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Campus)
            .FirstOrDefaultAsync(c => c.FoundItemId == foundItemId);
        
        if (caseEntity == null)
        {
            throw new InvalidOperationException("Không tìm thấy case cho đồ nhặt được này.");
        }

        // Tạo claim mới
        var claim = new StudentClaim
        {
            StudentId = lostReport.StudentId,
            FoundItemId = foundItemId,
            LostReportId = lostReportId,
            CaseId = caseEntity.Id,
            Status = "PENDING",
            CreatedAt = DateTime.Now
        };

        _context.StudentClaims.Add(claim);
        await _context.SaveChangesAsync();

        // Tự động update cases.total_claims++ và chuyển status sang IN_PROGRESS nếu đang ở OPEN
        caseEntity.TotalClaims = (caseEntity.TotalClaims ?? 0) + 1;
        if (caseEntity.Status == "OPEN" || string.IsNullOrEmpty(caseEntity.Status))
        {
            caseEntity.Status = "IN_PROGRESS";
        }
        _context.Cases.Update(caseEntity);
        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRelatedDataAsync(claim);

        // Gửi thông báo cho Student: Đã tìm thấy đồ, vui lòng lên nhận
        var campusName = caseEntity.FoundItem?.Campus?.Name ?? "phòng ban";
        var campusDisplayName = campusName != "phòng ban" ? $"cơ sở {campusName}" : campusName;
        try
        {
            await _notificationService.CreateNotificationAsync(
                userId: lostReport.StudentId,
                title: "Đã tìm thấy đồ của bạn",
                message: $"Đã tìm thấy đồ bạn báo mất. Vui lòng lên {campusDisplayName} để nhận lại đồ.",
                type: "CLAIM_MATCHED",
                relatedEntityId: claim.Id,
                relatedEntityType: "CLAIM"
            );
        }
        catch (Exception)
        {
            // Log lỗi nhưng không làm fail toàn bộ flow
            // Có thể do database chưa có bảng notifications hoặc lỗi khác
        }

        return MapToResponse(claim);
    }

    public async Task<StudentClaimResponse> ApproveClaimByStaffAsync(int claimId, int staffId)
    {
        // Kiểm tra claim có tồn tại không
        var claim = await _context.StudentClaims
            .Include(c => c.Case)
            .FirstOrDefaultAsync(c => c.Id == claimId);
        
        if (claim == null)
        {
            throw new ArgumentException("Không tìm thấy claim.");
        }

        // Kiểm tra claim đã được quyết định chưa
        if (claim.Status != "PENDING")
        {
            throw new InvalidOperationException("Claim này đã được quyết định rồi.");
        }

        // Kiểm tra case chỉ có 1 claim (điều kiện để Staff có thể approve trực tiếp)
        if (claim.CaseId == null)
        {
            throw new InvalidOperationException("Claim này chưa được gán vào case.");
        }

        var caseEntity = await _context.Cases
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Campus)
            .FirstOrDefaultAsync(c => c.Id == claim.CaseId.Value);
        
        if (caseEntity == null)
        {
            throw new ArgumentException("Không tìm thấy case.");
        }

        // Kiểm tra case chỉ có 1 claim (total_claims = 1)
        if (caseEntity.TotalClaims != 1)
        {
            throw new InvalidOperationException("Chỉ có thể approve trực tiếp khi case chỉ có 1 claim. Nếu có nhiều claims, vui lòng tạo Security Verification Request.");
        }

        // Approve claim (xác nhận student đã nhận đồ)
        claim.Status = "APPROVED";
        _context.StudentClaims.Update(claim);
        await _context.SaveChangesAsync();

        // Tự động tạo Return Receipt (xác nhận đã trả đồ)
        // Sử dụng evidence image của claim làm receipt image (nếu có)
        var receipt = new StaffReturnReceipt
        {
            CaseId = caseEntity.Id,
            ClaimId = claim.Id,
            StaffId = staffId,
            ReceiptImageUrl = claim.EvidenceImageUrl, // Dùng evidence image làm receipt image
            ReturnedAt = DateTime.Now
        };

        _context.StaffReturnReceipts.Add(receipt);

        // Update case.successful_claim_id và chuyển status sang COMPLETED
        caseEntity.SuccessfulClaimId = claim.Id;
        caseEntity.Status = "COMPLETED";
        caseEntity.ClosedAt = DateTime.Now;
        _context.Cases.Update(caseEntity);

        // Set Found Item status = 'RETURNED' (đã trả đồ)
        if (caseEntity.FoundItem != null)
        {
            caseEntity.FoundItem.Status = "RETURNED";
            _context.StaffFoundItems.Update(caseEntity.FoundItem);
        }

        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRelatedDataAsync(claim);

        // Gửi thông báo cho Student: Đã xác nhận nhận đồ
        var campusName = caseEntity.FoundItem?.Campus?.Name ?? "phòng ban";
        var campusDisplayName = campusName != "phòng ban" ? $"cơ sở {campusName}" : campusName;
        try
        {
            await _notificationService.CreateNotificationAsync(
                userId: claim.StudentId,
                title: "Đã xác nhận nhận đồ",
                message: $"Claim của bạn đã được xác nhận. Đồ vật đã được bàn giao thành công tại {campusDisplayName}.",
                type: "ITEM_RETURNED",
                relatedEntityId: claim.Id,
                relatedEntityType: "CLAIM"
            );
        }
        catch (Exception)
        {
            // Log lỗi nhưng không làm fail toàn bộ flow
        }

        return MapToResponse(claim);
    }

    public async Task<StudentClaimResponse> RejectClaimByStaffAsync(int claimId)
    {
        // Kiểm tra claim có tồn tại không
        var claim = await _context.StudentClaims
            .Include(c => c.Case)
            .FirstOrDefaultAsync(c => c.Id == claimId);
        
        if (claim == null)
        {
            throw new ArgumentException("Không tìm thấy claim.");
        }

        // Kiểm tra claim đã được quyết định chưa
        if (claim.Status != "PENDING")
        {
            throw new InvalidOperationException("Claim này đã được quyết định rồi.");
        }

        // Kiểm tra case chỉ có 1 claim (điều kiện để Staff có thể reject trực tiếp)
        if (claim.CaseId == null)
        {
            throw new InvalidOperationException("Claim này chưa được gán vào case.");
        }

        var caseEntity = await _context.Cases
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Campus)
            .FirstOrDefaultAsync(c => c.Id == claim.CaseId.Value);
        
        if (caseEntity == null)
        {
            throw new ArgumentException("Không tìm thấy case.");
        }

        // Kiểm tra case chỉ có 1 claim (total_claims = 1)
        if (caseEntity.TotalClaims != 1)
        {
            throw new InvalidOperationException("Chỉ có thể reject trực tiếp khi case chỉ có 1 claim. Nếu có nhiều claims, vui lòng tạo Security Verification Request.");
        }

        // Reject claim
        claim.Status = "REJECTED";
        _context.StudentClaims.Update(claim);

        // Khi reject claim duy nhất, case sẽ chuyển sang FAILED (không còn claims hợp lệ)
        caseEntity.Status = "FAILED";
        caseEntity.ClosedAt = DateTime.Now;
        _context.Cases.Update(caseEntity);

        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRelatedDataAsync(claim);

        // Gửi thông báo cho Student: Claim đã bị từ chối
        var foundItemDescription = caseEntity.FoundItem?.Description ?? "đồ vật";
        try
        {
            await _notificationService.CreateNotificationAsync(
                userId: claim.StudentId,
                title: "Claim đã bị từ chối",
                message: $"Claim của bạn cho đồ vật \"{foundItemDescription}\" đã bị từ chối. Vui lòng kiểm tra lại thông tin và liên hệ với bộ phận liên quan nếu cần hỗ trợ.",
                type: "CLAIM_REJECTED",
                relatedEntityId: claim.Id,
                relatedEntityType: "CLAIM"
            );
        }
        catch (Exception)
        {
            // Log lỗi nhưng không làm fail toàn bộ flow
            // Có thể do database chưa có bảng notifications hoặc lỗi khác
        }

        return MapToResponse(claim);
    }

    public async Task<IEnumerable<StudentClaimResponse>> GetMyClaimsAsync(int studentId)
    {
        var claims = await _context.StudentClaims
            .Where(c => c.StudentId == studentId)
            .Include(c => c.Student)
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Category)
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Campus)
            .Include(c => c.LostReport)
                .ThenInclude(lr => lr.Category)
            .Include(c => c.Case)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return claims.Select(MapToResponse);
    }

    public async Task<IEnumerable<StudentClaimResponse>> GetAllAsync(string? status = null, int? caseId = null)
    {
        var query = _context.StudentClaims
            .Include(c => c.Student)
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Category)
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Campus)
            .Include(c => c.LostReport)
                .ThenInclude(lr => lr.Category)
            .Include(c => c.Case)
            .AsQueryable();

        // Filter theo status nếu có
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }

        // Filter theo caseId nếu có
        if (caseId.HasValue)
        {
            query = query.Where(c => c.CaseId == caseId.Value);
        }

        var claims = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return claims.Select(MapToResponse);
    }

    public async Task<StudentClaimResponse?> GetByIdAsync(int id, int? studentId = null)
    {
        var query = _context.StudentClaims
            .Include(c => c.Student)
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Category)
            .Include(c => c.FoundItem)
                .ThenInclude(f => f.Campus)
            .Include(c => c.LostReport)
                .ThenInclude(lr => lr.Category)
            .Include(c => c.Case)
            .AsQueryable();

        // Nếu có studentId, chỉ lấy claim của student đó (cho Student role)
        if (studentId.HasValue)
        {
            query = query.Where(c => c.StudentId == studentId.Value);
        }

        var claim = await query.FirstOrDefaultAsync(c => c.Id == id);
        if (claim == null)
            return null;

        return MapToResponse(claim);
    }

    public async Task<StudentClaimResponse?> UpdateEvidenceAsync(int id, int studentId, string evidenceImageUrl)
    {
        // Chỉ cho phép update evidence của chính mình và status là PENDING
        var claim = await _context.StudentClaims
            .FirstOrDefaultAsync(c => c.Id == id && c.StudentId == studentId);

        if (claim == null)
            return null;

        if (claim.Status != "PENDING")
        {
            throw new InvalidOperationException("Chỉ có thể cập nhật evidence khi claim ở trạng thái PENDING.");
        }

        claim.EvidenceImageUrl = evidenceImageUrl;
        _context.StudentClaims.Update(claim);
        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRelatedDataAsync(claim);

        return MapToResponse(claim);
    }

    public async Task<StudentClaimResponse?> UpdateEvidenceByStaffAsync(int id, string evidenceImageUrl)
    {
        // Staff chỉ có thể upload evidence khi claim ở trạng thái PENDING
        var claim = await _context.StudentClaims
            .FirstOrDefaultAsync(c => c.Id == id);

        if (claim == null)
        {
            throw new ArgumentException("Không tìm thấy claim.");
        }

        // Chỉ cho phép upload evidence khi claim ở trạng thái PENDING
        if (claim.Status != "PENDING")
        {
            throw new InvalidOperationException("Chỉ có thể upload evidence khi claim ở trạng thái PENDING.");
        }

        claim.EvidenceImageUrl = evidenceImageUrl;
        _context.StudentClaims.Update(claim);
        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRelatedDataAsync(claim);

        return MapToResponse(claim);
    }

    private async Task LoadRelatedDataAsync(StudentClaim claim)
    {
        await _context.Entry(claim)
            .Reference(c => c.Student)
            .LoadAsync();

        await _context.Entry(claim)
            .Reference(c => c.FoundItem)
            .LoadAsync();

        if (claim.FoundItem != null)
        {
            await _context.Entry(claim.FoundItem)
                .Reference(f => f.Category)
                .LoadAsync();

            await _context.Entry(claim.FoundItem)
                .Reference(f => f.Campus)
                .LoadAsync();
        }

        if (claim.LostReportId.HasValue)
        {
            await _context.Entry(claim)
                .Reference(c => c.LostReport)
                .LoadAsync();
            
            if (claim.LostReport != null)
            {
                await _context.Entry(claim.LostReport)
                    .Reference(lr => lr.Category)
                    .LoadAsync();
            }
        }

        if (claim.CaseId.HasValue)
        {
            await _context.Entry(claim)
                .Reference(c => c.Case)
                .LoadAsync();
        }
    }

    private static StudentClaimResponse MapToResponse(StudentClaim claim)
    {
        return new StudentClaimResponse
        {
            Id = claim.Id,
            StudentId = claim.StudentId,
            StudentName = claim.Student?.FullName,
            StudentCode = claim.Student?.StudentCode,
            FoundItemId = claim.FoundItemId,
            FoundItemDescription = claim.FoundItem?.Description,
            FoundItemImageUrl = claim.FoundItem?.ImageUrl,
            FoundItemStatus = claim.FoundItem?.Status,
            FoundItemFoundDate = claim.FoundItem?.FoundDate,
            FoundItemFoundLocation = claim.FoundItem?.FoundLocation,
            FoundItemCategoryName = claim.FoundItem?.Category?.Name,
            FoundItemCampusName = claim.FoundItem?.Campus?.Name,
            FoundItemIdentifyingFeatures = claim.FoundItem?.IdentifyingFeatures,
            FoundItemClaimPassword = claim.FoundItem?.ClaimPassword,
            LostReportId = claim.LostReportId,
            LostReportDescription = claim.LostReport?.Description,
            LostReportLostDate = claim.LostReport?.LostDate,
            LostReportLostLocation = claim.LostReport?.LostLocation,
            LostReportImageUrl = claim.LostReport?.ImageUrl,
            LostReportCategoryName = claim.LostReport?.Category?.Name,
            LostReportIdentifyingFeatures = claim.LostReport?.IdentifyingFeatures,
            LostReportClaimPassword = claim.LostReport?.ClaimPassword,
            CaseId = claim.CaseId,
            CaseStatus = claim.Case?.Status,
            Status = claim.Status,
            EvidenceImageUrl = claim.EvidenceImageUrl,
            CreatedAt = claim.CreatedAt
        };
    }
}

