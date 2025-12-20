namespace LostAndFound.Application.DTOs.Reports;

public class ClaimsStatisticsResponse
{
    public int TotalClaims { get; set; }
    public Dictionary<string, int> ClaimsByStatus { get; set; } = new();
}

