using LostAndFound.Application.DTOs.Reports;
using LostAndFound.Application.Interfaces.Reports;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CasesStatisticsResponse> GetCasesStatisticsAsync(int? campusId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Cases.AsQueryable();

        // Filter by campus
        if (campusId.HasValue)
        {
            query = query.Where(c => c.CampusId == campusId.Value);
        }

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }

        // Filter by date range (openedAt)
        if (fromDate.HasValue)
        {
            query = query.Where(c => c.OpenedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(c => c.OpenedAt <= toDate.Value);
        }

        var cases = await query.ToListAsync();

        // Get all campuses for mapping
        var campuses = await _context.Campuses.ToListAsync();
        var campusNames = campuses.ToDictionary(c => c.Id.ToString(), c => c.Name ?? "");

        // Statistics by status
        var casesByStatus = cases
            .GroupBy(c => c.Status ?? "UNKNOWN")
            .ToDictionary(g => g.Key, g => g.Count());

        // Statistics by campus
        var casesByCampus = cases
            .GroupBy(c => c.CampusId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Count cases opened/closed in period
        var casesOpenedInPeriod = fromDate.HasValue || toDate.HasValue
            ? cases.Count(c => c.OpenedAt.HasValue &&
                (!fromDate.HasValue || c.OpenedAt >= fromDate.Value) &&
                (!toDate.HasValue || c.OpenedAt <= toDate.Value))
            : cases.Count;

        var casesClosedInPeriod = cases.Count(c => c.ClosedAt.HasValue &&
            (!fromDate.HasValue || c.ClosedAt >= fromDate.Value) &&
            (!toDate.HasValue || c.ClosedAt <= toDate.Value));

        return new CasesStatisticsResponse
        {
            TotalCases = cases.Count,
            CasesByStatus = casesByStatus,
            CasesByCampus = casesByCampus,
            CampusNames = campusNames,
            CasesOpenedInPeriod = casesOpenedInPeriod,
            CasesClosedInPeriod = casesClosedInPeriod
        };
    }

    public async Task<ClaimsStatisticsResponse> GetClaimsStatisticsAsync()
    {
        var claims = await _context.StudentClaims.ToListAsync();

        var claimsByStatus = claims
            .GroupBy(c => c.Status ?? "UNKNOWN")
            .ToDictionary(g => g.Key, g => g.Count());

        return new ClaimsStatisticsResponse
        {
            TotalClaims = claims.Count,
            ClaimsByStatus = claimsByStatus
        };
    }

    public async Task<ItemsStatisticsResponse> GetItemsStatisticsAsync(int? campusId = null, int? categoryId = null)
    {
        var query = _context.StaffFoundItems.AsQueryable();

        if (campusId.HasValue)
        {
            query = query.Where(i => i.CampusId == campusId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        var items = await query.ToListAsync();

        // Get all campuses and categories for mapping
        var campuses = await _context.Campuses.ToListAsync();
        var campusNames = campuses.ToDictionary(c => c.Id.ToString(), c => c.Name ?? "");

        var categories = await _context.ItemCategories.ToListAsync();
        var categoryNames = categories.ToDictionary(c => c.Id.ToString(), c => c.Name ?? "");

        // Statistics by campus
        var itemsByCampus = items
            .GroupBy(i => i.CampusId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Statistics by category
        var itemsByCategory = items
            .Where(i => i.CategoryId.HasValue)
            .GroupBy(i => i.CategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return new ItemsStatisticsResponse
        {
            TotalItems = items.Count,
            ItemsByCampus = itemsByCampus,
            CampusNames = campusNames,
            ItemsByCategory = itemsByCategory,
            CategoryNames = categoryNames
        };
    }

    public async Task<ReturnRateResponse> GetReturnRateAsync()
    {
        var allItems = await _context.StaffFoundItems.ToListAsync();

        var totalFoundItems = allItems.Count;
        var returnedItems = allItems.Count(i => i.Status == "RETURNED");
        var pendingItems = allItems.Count(i => i.Status == "STORED");

        var returnRate = totalFoundItems > 0
            ? (double)returnedItems / totalFoundItems
            : 0;

        return new ReturnRateResponse
        {
            TotalFoundItems = totalFoundItems,
            ReturnedItems = returnedItems,
            PendingItems = pendingItems,
            ReturnRate = returnRate,
            ReturnRatePercent = returnRate * 100
        };
    }
}

