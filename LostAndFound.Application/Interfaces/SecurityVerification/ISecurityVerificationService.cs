using LostAndFound.Application.DTOs.SecurityVerification;

namespace LostAndFound.Application.Interfaces.SecurityVerification;

public interface ISecurityVerificationService
{
    Task<SecurityVerificationRequestResponse> CreateRequestAsync(int requestedBy, CreateSecurityVerificationRequestRequest request);
    Task<IEnumerable<SecurityVerificationRequestResponse>> GetPendingRequestsAsync();
    Task<SecurityVerificationRequestResponse?> GetRequestByIdAsync(int id);
    Task<SecurityVerificationRequestResponse> CreateDecisionAsync(int requestId, int securityOfficerId, CreateSecurityVerificationDecisionRequest decisionRequest);
}

