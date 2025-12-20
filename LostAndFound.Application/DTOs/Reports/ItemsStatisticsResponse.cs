namespace LostAndFound.Application.DTOs.Reports;

public class ItemsStatisticsResponse
{
    public int TotalItems { get; set; }
    public Dictionary<int, int> ItemsByCampus { get; set; } = new(); // CampusId -> Count
    public Dictionary<string, string> CampusNames { get; set; } = new(); // CampusId -> Name (as string key)
    public Dictionary<int, int> ItemsByCategory { get; set; } = new(); // CategoryId -> Count
    public Dictionary<string, string> CategoryNames { get; set; } = new(); // CategoryId -> Name (as string key)
}

