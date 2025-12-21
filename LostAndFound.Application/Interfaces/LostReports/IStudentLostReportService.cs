using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LostAndFound.Application.DTOs.LostReports;

namespace LostAndFound.Application.Interfaces.LostReports;

public interface IStudentLostReportService
{
    Task<StudentLostReportResponse> CreateAsync(int studentId, CreateStudentLostReportRequest request);
    Task<IEnumerable<StudentLostReportResponse>> GetMyReportsAsync(int studentId);
    Task<IEnumerable<StudentLostReportResponse>> GetAllAsync(int? categoryId = null);
    Task<StudentLostReportResponse?> GetByIdAsync(int id, int? studentId = null);
    Task<StudentLostReportResponse?> UpdateAsync(int id, int studentId, UpdateStudentLostReportRequest request);
    Task<bool> DeleteAsync(int id, int studentId);
}


