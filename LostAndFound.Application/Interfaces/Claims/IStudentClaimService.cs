using LostAndFound.Application.DTOs.Claims;

namespace LostAndFound.Application.Interfaces.Claims;

public interface IStudentClaimService
{
    Task<StudentClaimResponse> CreateAsync(int studentId, CreateStudentClaimRequest request);
    Task<StudentClaimResponse> CreateClaimForStudentAsync(int lostReportId, int foundItemId);
    Task<StudentClaimResponse> ApproveClaimByStaffAsync(int claimId);
    Task<IEnumerable<StudentClaimResponse>> GetMyClaimsAsync(int studentId);
    Task<IEnumerable<StudentClaimResponse>> GetAllAsync(string? status = null, int? caseId = null);
    Task<StudentClaimResponse?> GetByIdAsync(int id, int? studentId = null);
    Task<StudentClaimResponse?> UpdateEvidenceAsync(int id, int studentId, string evidenceImageUrl);
}

