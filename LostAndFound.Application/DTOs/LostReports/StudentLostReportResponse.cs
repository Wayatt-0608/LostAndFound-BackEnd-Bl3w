using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostAndFound.Application.DTOs.LostReports;

public class StudentLostReportResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentCode { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public DateTime? LostDate { get; set; }
    public string? LostLocation { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool HasClaims { get; set; } // Có claim nào chưa
    public string? IdentifyingFeatures { get; set; } // Chỉ hiển thị cho Staff/Security
    public string? ClaimPassword { get; set; } // Chỉ hiển thị cho Staff/Security
}


