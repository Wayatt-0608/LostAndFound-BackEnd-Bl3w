using LostAndFound.Application.DTOs.Reports;

namespace LostAndFound.Application.Interfaces.Reports;

public interface IReportService
{
    Task<CasesStatisticsResponse> GetCasesStatisticsAsync(int? campusId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<ClaimsStatisticsResponse> GetClaimsStatisticsAsync();
    Task<ItemsStatisticsResponse> GetItemsStatisticsAsync(int? campusId = null, int? categoryId = null);
    Task<ReturnRateResponse> GetReturnRateAsync();
}

