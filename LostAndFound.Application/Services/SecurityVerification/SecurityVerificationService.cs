using LostAndFound.Application.DTOs.SecurityVerification;
using LostAndFound.Application.Interfaces.Notifications;
using LostAndFound.Application.Interfaces.SecurityVerification;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.SecurityVerification;

public class SecurityVerificationService : ISecurityVerificationService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public SecurityVerificationService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<SecurityVerificationRequestResponse> CreateRequestAsync(int requestedBy, CreateSecurityVerificationRequestRequest request)
    {
        // Kiểm tra Case có tồn tại không
        var caseEntity = await _context.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId);
        
        if (caseEntity == null)
        {
            throw new ArgumentException("Không tìm thấy case.");
        }

        // Tạo verification request
        var verificationRequest = new SecurityVerificationRequest
        {
            CaseId = request.CaseId,
            RequestedBy = requestedBy,
            CreatedAt = DateTime.Now
        };

        _context.SecurityVerificationRequests.Add(verificationRequest);
        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRequestDataAsync(verificationRequest);

        // Gửi thông báo cho tất cả Security Officers: Có request mới cần xử lý
        var foundItemDescription = verificationRequest.Case?.FoundItem?.Description ?? "đồ vật";
        try
        {
            var securityOfficers = await _context.Users
                .Where(u => u.Role == "SecurityOfficer")
                .ToListAsync();

            foreach (var officer in securityOfficers)
            {
                await _notificationService.CreateNotificationAsync(
                    userId: officer.Id,
                    title: "Yêu cầu xác minh mới",
                    message: $"Có yêu cầu xác minh mới cho case ID {verificationRequest.CaseId} (Mô tả: {foundItemDescription}). Vui lòng xử lý.",
                    type: "SECURITY_VERIFICATION_REQUEST",
                    relatedEntityId: verificationRequest.Id,
                    relatedEntityType: "SECURITY_VERIFICATION_REQUEST"
                );
            }
        }
        catch (Exception)
        {
            // Log lỗi nhưng không làm fail toàn bộ flow
            // Có thể do database chưa có bảng notifications hoặc lỗi khác
        }

        return MapToResponse(verificationRequest);
    }

    public async Task<IEnumerable<SecurityVerificationRequestResponse>> GetPendingRequestsAsync()
    {
        // Lấy các requests chưa có decision nào (chờ xử lý)
        var requests = await _context.SecurityVerificationRequests
            .Include(r => r.Case)
                .ThenInclude(c => c.FoundItem)
            .Include(r => r.RequestedByNavigation)
            .Include(r => r.SecurityVerificationDecisions)
                .ThenInclude(d => d.SecurityOfficer)
            .Where(r => !r.SecurityVerificationDecisions.Any()) // Chưa có decision nào
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Load claims cho mỗi case
        var caseIds = requests.Select(r => r.CaseId).ToList();
        var claims = await _context.StudentClaims
            .Where(c => c.CaseId.HasValue && caseIds.Contains(c.CaseId.Value))
            .Include(c => c.Student)
            .ToListAsync();

        return requests.Select(r => MapToResponse(r, claims));
    }

    public async Task<SecurityVerificationRequestResponse?> GetRequestByIdAsync(int id)
    {
        var request = await _context.SecurityVerificationRequests
            .Include(r => r.Case)
                .ThenInclude(c => c.FoundItem)
            .Include(r => r.RequestedByNavigation)
            .Include(r => r.SecurityVerificationDecisions)
                .ThenInclude(d => d.SecurityOfficer)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return null;

        // Load claims cho case này
        var claims = await _context.StudentClaims
            .Where(c => c.CaseId == request.CaseId)
            .Include(c => c.Student)
            .ToListAsync();

        return MapToResponse(request, claims);
    }

    public async Task<SecurityVerificationRequestResponse> CreateDecisionAsync(int requestId, int securityOfficerId, CreateSecurityVerificationDecisionRequest decisionRequest)
    {
        // Validate decision
        if (decisionRequest.Decision != "APPROVED" && decisionRequest.Decision != "REJECTED")
        {
            throw new ArgumentException("Decision chỉ có thể là APPROVED hoặc REJECTED.");
        }

        // Kiểm tra request có tồn tại không
        var verificationRequest = await _context.SecurityVerificationRequests
            .Include(r => r.Case)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        
        if (verificationRequest == null)
        {
            throw new ArgumentException("Không tìm thấy verification request.");
        }

        // Kiểm tra claim có tồn tại và thuộc về case này không
        var claim = await _context.StudentClaims
            .FirstOrDefaultAsync(c => c.Id == decisionRequest.ClaimId && c.CaseId == verificationRequest.CaseId);
        
        if (claim == null)
        {
            throw new ArgumentException("Không tìm thấy claim hoặc claim không thuộc về case này.");
        }

        // Kiểm tra claim đã được quyết định chưa
        if (claim.Status != "PENDING")
        {
            throw new InvalidOperationException("Claim này đã được quyết định rồi.");
        }

        // Tạo decision
        var decision = new SecurityVerificationDecision
        {
            RequestId = requestId,
            SecurityOfficerId = securityOfficerId,
            Decision = decisionRequest.Decision,
            Note = decisionRequest.Note,
            EvidenceImageUrl = decisionRequest.EvidenceImageUrl,
            CreatedAt = DateTime.Now
        };

        _context.SecurityVerificationDecisions.Add(decision);
        await _context.SaveChangesAsync();

        // Tự động update claim status và case.successful_claim_id
        if (decisionRequest.Decision == "APPROVED")
        {
            // Update claim status
            claim.Status = "APPROVED";
            _context.StudentClaims.Update(claim);

            // Update case.successful_claim_id
            verificationRequest.Case.SuccessfulClaimId = claim.Id;
            _context.Cases.Update(verificationRequest.Case);

            // Reject các claims khác trong case này
            var otherClaims = await _context.StudentClaims
                .Where(c => c.CaseId == verificationRequest.CaseId && c.Id != claim.Id && c.Status == "PENDING")
                .ToListAsync();
            
            foreach (var otherClaim in otherClaims)
            {
                otherClaim.Status = "REJECTED";
            }
            
            if (otherClaims.Any())
            {
                _context.StudentClaims.UpdateRange(otherClaims);
            }

            // Tự động update Found Item status = 'RETURNED' khi có claim được approve
            var foundItem = await _context.StaffFoundItems
                .FirstOrDefaultAsync(f => f.Id == verificationRequest.Case.FoundItemId);
            
            if (foundItem != null)
            {
                foundItem.Status = "RETURNED";
                _context.StaffFoundItems.Update(foundItem);
            }
        }
        else if (decisionRequest.Decision == "REJECTED")
        {
            // Update claim status
            claim.Status = "REJECTED";
            _context.StudentClaims.Update(claim);
        }

        await _context.SaveChangesAsync();

        // Load related data for response
        await LoadRequestDataAsync(verificationRequest);

        // Load claims again
        var claims = await _context.StudentClaims
            .Where(c => c.CaseId == verificationRequest.CaseId)
            .Include(c => c.Student)
            .ToListAsync();

        // Gửi thông báo cho Staff member đã tạo request: Đã có quyết định từ Security Officer
        var decisionText = decisionRequest.Decision == "APPROVED" ? "được duyệt" : "bị từ chối";
        var foundItemDescription = verificationRequest.Case?.FoundItem?.Description ?? "đồ vật";
        try
        {
            await _notificationService.CreateNotificationAsync(
                userId: verificationRequest.RequestedBy,
                title: $"Yêu cầu xác minh đã {decisionText}",
                message: $"Yêu cầu xác minh của bạn cho case ID {verificationRequest.CaseId} (Mô tả: {foundItemDescription}) đã {decisionText} bởi Security Officer.",
                type: "SECURITY_VERIFICATION_DECISION",
                relatedEntityId: verificationRequest.Id,
                relatedEntityType: "SECURITY_VERIFICATION_REQUEST"
            );
        }
        catch (Exception)
        {
            // Log lỗi nhưng không làm fail toàn bộ flow
            // Có thể do database chưa có bảng notifications hoặc lỗi khác
        }

        return MapToResponse(verificationRequest, claims);
    }

    private async Task LoadRequestDataAsync(SecurityVerificationRequest request)
    {
        await _context.Entry(request)
            .Reference(r => r.Case)
            .LoadAsync();

        if (request.Case != null)
        {
            await _context.Entry(request.Case)
                .Reference(c => c.FoundItem)
                .LoadAsync();
        }

        await _context.Entry(request)
            .Reference(r => r.RequestedByNavigation)
            .LoadAsync();

        await _context.Entry(request)
            .Collection(r => r.SecurityVerificationDecisions)
            .Query()
            .Include(d => d.SecurityOfficer)
            .LoadAsync();
    }

    private static SecurityVerificationRequestResponse MapToResponse(
        SecurityVerificationRequest request,
        List<StudentClaim>? claims = null)
    {
        return new SecurityVerificationRequestResponse
        {
            Id = request.Id,
            CaseId = request.CaseId,
            CaseStatus = request.Case?.Status,
            FoundItemId = request.Case?.FoundItemId,
            FoundItemDescription = request.Case?.FoundItem?.Description,
            RequestedBy = request.RequestedBy,
            RequestedByName = request.RequestedByNavigation?.FullName,
            CreatedAt = request.CreatedAt,
            Decisions = request.SecurityVerificationDecisions.Select(d => new SecurityVerificationDecisionInfo
            {
                Id = d.Id,
                SecurityOfficerId = d.SecurityOfficerId,
                SecurityOfficerName = d.SecurityOfficer?.FullName,
                Decision = d.Decision,
                Note = d.Note,
                EvidenceImageUrl = d.EvidenceImageUrl,
                CreatedAt = d.CreatedAt
            }).ToList(),
            CaseClaims = (claims ?? new List<StudentClaim>())
                .Where(c => c.CaseId == request.CaseId)
                .Select(c => new CaseClaimInfo
                {
                    Id = c.Id,
                    StudentId = c.StudentId,
                    StudentName = c.Student?.FullName,
                    StudentCode = c.Student?.StudentCode,
                    Status = c.Status,
                    EvidenceImageUrl = c.EvidenceImageUrl,
                    CreatedAt = c.CreatedAt
                }).ToList()
        };
    }
}

