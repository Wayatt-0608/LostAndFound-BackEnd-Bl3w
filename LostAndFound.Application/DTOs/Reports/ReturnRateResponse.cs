namespace LostAndFound.Application.DTOs.Reports;

public class ReturnRateResponse
{
    public int TotalFoundItems { get; set; }
    public int ReturnedItems { get; set; }
    public int PendingItems { get; set; }
    public double ReturnRate { get; set; } // 0-1 (decimal)
    public double ReturnRatePercent { get; set; } // 0-100 (percentage)
}

