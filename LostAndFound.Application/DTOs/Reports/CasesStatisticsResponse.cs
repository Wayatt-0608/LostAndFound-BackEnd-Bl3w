namespace LostAndFound.Application.DTOs.Reports;

public class CasesStatisticsResponse
{
    public int TotalCases { get; set; }
    public Dictionary<string, int> CasesByStatus { get; set; } = new();
    public Dictionary<int, int> CasesByCampus { get; set; } = new(); // CampusId -> Count
    public Dictionary<string, string> CampusNames { get; set; } = new(); // CampusId -> Name (as string key)
    public int CasesOpenedInPeriod { get; set; }
    public int CasesClosedInPeriod { get; set; }
}

