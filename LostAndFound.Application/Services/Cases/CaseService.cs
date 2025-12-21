using LostAndFound.Application.DTOs.Cases;
using LostAndFound.Application.Interfaces.Cases;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.Cases;

public class CaseService : ICaseService
{
    private readonly AppDbContext _context;

    public CaseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Case> CreateCaseForFoundItemAsync(int foundItemId, int campusId)
    {
        // Kiểm tra xem đã có case cho found item này chưa
        var existingCase = await _context.Cases
            .FirstOrDefaultAsync(c => c.FoundItemId == foundItemId);
        
        if (existingCase != null)
        {
            return existingCase; // Trả về case đã tồn tại
        }

        var newCase = new Case
        {
            FoundItemId = foundItemId,
            CampusId = campusId,
            Status = "OPEN",
            TotalClaims = 0,
            OpenedAt = DateTime.Now
        };

        _context.Cases.Add(newCase);
        await _context.SaveChangesAsync();

        return newCase;
    }

    public async Task<IEnumerable<CaseResponse>> GetAllAsync(int? campusId = null, string? status = null)
    {
        var query = _context.Cases
            .Include(c => c.FoundItem)
            .Include(c => c.Campus)
            .AsQueryable();

        // Apply filters
        if (campusId.HasValue)
        {
            query = query.Where(c => c.CampusId == campusId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }

        var cases = await query
            .OrderByDescending(c => c.OpenedAt)
            .ToListAsync();

        var caseIds = cases.Select(c => c.Id).ToList();
        
        // Load claims and verification requests for all cases
        var claims = await _context.StudentClaims
            .Where(cl => cl.CaseId.HasValue && caseIds.Contains(cl.CaseId.Value))
            .Include(cl => cl.Student)
            .ToListAsync();

        var verificationRequests = await _context.SecurityVerificationRequests
            .Where(vr => caseIds.Contains(vr.CaseId))
            .Include(vr => vr.RequestedByNavigation)
            .ToListAsync();

        return cases.Select(c => MapToResponse(c, claims, verificationRequests));
    }

    public async Task<CaseResponse?> GetByIdAsync(int id)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.FoundItem)
            .Include(c => c.Campus)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (caseEntity == null)
            return null;

        // Load claims for this case
        var claims = await _context.StudentClaims
            .Where(cl => cl.CaseId == id)
            .Include(cl => cl.Student)
            .ToListAsync();

        // Load verification requests for this case
        var verificationRequests = await _context.SecurityVerificationRequests
            .Where(vr => vr.CaseId == id)
            .Include(vr => vr.RequestedByNavigation)
            .ToListAsync();

        return MapToResponse(caseEntity, claims, verificationRequests);
    }

    public async Task<CaseResponse?> UpdateStatusAsync(int id, string status)
    {
        // Validate status
        var validStatuses = new[] { "OPEN", "IN_PROGRESS", "COMPLETED", "FAILED" };
        if (!validStatuses.Contains(status))
        {
            throw new ArgumentException($"Status chỉ có thể là: {string.Join(", ", validStatuses)}");
        }

        var caseEntity = await _context.Cases.FindAsync(id);
        if (caseEntity == null)
            return null;

        caseEntity.Status = status;

        // Set ClosedAt nếu status là COMPLETED hoặc FAILED
        if (status == "COMPLETED" || status == "FAILED")
        {
            caseEntity.ClosedAt = DateTime.Now;
        }
        else
        {
            caseEntity.ClosedAt = null; // Reset nếu chuyển về status khác
        }

        _context.Cases.Update(caseEntity);
        await _context.SaveChangesAsync();

        // Load related data for response
        await _context.Entry(caseEntity)
            .Reference(c => c.FoundItem)
            .LoadAsync();

        await _context.Entry(caseEntity)
            .Reference(c => c.Campus)
            .LoadAsync();

        var claims = await _context.StudentClaims
            .Where(cl => cl.CaseId == id)
            .Include(cl => cl.Student)
            .ToListAsync();

        var verificationRequests = await _context.SecurityVerificationRequests
            .Where(vr => vr.CaseId == id)
            .Include(vr => vr.RequestedByNavigation)
            .ToListAsync();

        return MapToResponse(caseEntity, claims, verificationRequests);
    }

    private static CaseResponse MapToResponse(
        Case caseEntity,
        List<StudentClaim> claims,
        List<SecurityVerificationRequest> verificationRequests)
    {
        return new CaseResponse
        {
            Id = caseEntity.Id,
            FoundItemId = caseEntity.FoundItemId,
            FoundItemDescription = caseEntity.FoundItem?.Description,
            FoundItemImageUrl = caseEntity.FoundItem?.ImageUrl,
            CampusId = caseEntity.CampusId,
            CampusName = caseEntity.Campus?.Name,
            Status = caseEntity.Status,
            TotalClaims = caseEntity.TotalClaims,
            SuccessfulClaimId = caseEntity.SuccessfulClaimId,
            OpenedAt = caseEntity.OpenedAt,
            ClosedAt = caseEntity.ClosedAt,
            Claims = claims.Where(c => c.CaseId == caseEntity.Id).Select(c => new CaseClaimInfo
            {
                Id = c.Id,
                StudentId = c.StudentId,
                StudentName = c.Student?.FullName,
                StudentCode = c.Student?.StudentCode,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList(),
            VerificationRequests = verificationRequests.Where(vr => vr.CaseId == caseEntity.Id).Select(vr => new CaseVerificationRequestInfo
            {
                Id = vr.Id,
                RequestedBy = vr.RequestedBy,
                RequestedByName = vr.RequestedByNavigation?.FullName,
                CreatedAt = vr.CreatedAt
            }).ToList()
        };
    }
}

