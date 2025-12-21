using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostAndFound.Application.DTOs.LostReports;

public class UpdateStudentLostReportRequest
{
    public int? CategoryId { get; set; }
    public string? Description { get; set; }
    public DateTime? LostDate { get; set; }
    public string? LostLocation { get; set; }
    public string? ImageUrl { get; set; } // Được set sau khi upload từ Controller
}


