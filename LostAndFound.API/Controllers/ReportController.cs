using LostAndFound.Application.Interfaces.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Staff")] // Chỉ Staff mới xem được reports
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Thống kê cases (theo status, campus, thời gian)
    /// </summary>
    [HttpGet("cases-statistics")]
    public async Task<IActionResult> GetCasesStatistics(
        [FromQuery] int? campusId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var statistics = await _reportService.GetCasesStatisticsAsync(campusId, status, fromDate, toDate);
        return Ok(statistics);
    }

    /// <summary>
    /// Thống kê claims (theo status)
    /// </summary>
    [HttpGet("claims-statistics")]
    public async Task<IActionResult> GetClaimsStatistics()
    {
        var statistics = await _reportService.GetClaimsStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Thống kê found items (theo campus, category)
    /// </summary>
    [HttpGet("items-statistics")]
    public async Task<IActionResult> GetItemsStatistics(
        [FromQuery] int? campusId,
        [FromQuery] int? categoryId)
    {
        var statistics = await _reportService.GetItemsStatisticsAsync(campusId, categoryId);
        return Ok(statistics);
    }

    /// <summary>
    /// Tỷ lệ trả đồ thành công
    /// </summary>
    [HttpGet("return-rate")]
    public async Task<IActionResult> GetReturnRate()
    {
        var statistics = await _reportService.GetReturnRateAsync();
        return Ok(statistics);
    }
}

