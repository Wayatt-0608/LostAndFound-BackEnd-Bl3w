using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LostAndFound.Application.DTOs.LostReports;
using LostAndFound.Application.Interfaces.LostReports;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.LostReports;

public class StudentLostReportService : IStudentLostReportService
{
    private readonly AppDbContext _context;

    public StudentLostReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StudentLostReportResponse> CreateAsync(int studentId, CreateStudentLostReportRequest request)
    {
        var report = new StudentLostReport
        {
            StudentId = studentId,
            CategoryId = request.CategoryId,
            Description = request.Description,
            LostDate = request.LostDate,
            LostLocation = request.LostLocation,
            ImageUrl = request.ImageUrl,
            IdentifyingFeatures = request.IdentifyingFeatures,
            ClaimPassword = request.ClaimPassword, // Lưu plain text để staff có thể thấy
            CreatedAt = DateTime.Now
        };

        _context.StudentLostReports.Add(report);
        await _context.SaveChangesAsync();

        // Load related data for response
        await _context.Entry(report)
            .Reference(r => r.Student)
            .LoadAsync();

        await _context.Entry(report)
            .Reference(r => r.Category)
            .LoadAsync();

        var hasClaims = await _context.StudentClaims
            .AnyAsync(c => c.LostReportId == report.Id);

        // Student không được xem sensitive data
        return MapToResponse(report, hasClaims, includeSensitiveData: false);
    }

    public async Task<IEnumerable<StudentLostReportResponse>> GetMyReportsAsync(int studentId)
    {
        // Lấy các lost report IDs đã có claim được approve (sẽ loại bỏ)
        var approvedClaimReportIds = await _context.StudentClaims
            .Where(c => c.LostReportId.HasValue && c.Status == "APPROVED")
            .Select(c => c.LostReportId!.Value)
            .Distinct()
            .ToListAsync();

        var reports = await _context.StudentLostReports
            .Where(r => r.StudentId == studentId && !approvedClaimReportIds.Contains(r.Id))
            .Include(r => r.Student)
            .Include(r => r.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reportIds = reports.Select(r => r.Id).ToList();
        var claimsCount = await _context.StudentClaims
            .Where(c => c.LostReportId.HasValue && reportIds.Contains(c.LostReportId.Value))
            .GroupBy(c => c.LostReportId)
            .ToDictionaryAsync(g => g.Key!.Value, g => g.Count() > 0);

        // Student không được xem sensitive data
        return reports.Select(r => MapToResponse(r, claimsCount.GetValueOrDefault(r.Id, false), includeSensitiveData: false));
    }

    public async Task<IEnumerable<StudentLostReportResponse>> GetAllAsync(int? categoryId = null, bool includeSensitiveData = true)
    {
        // Lấy các lost report IDs đã có claim được approve (sẽ loại bỏ)
        var approvedClaimReportIds = await _context.StudentClaims
            .Where(c => c.LostReportId.HasValue && c.Status == "APPROVED")
            .Select(c => c.LostReportId!.Value)
            .Distinct()
            .ToListAsync();

        var query = _context.StudentLostReports
            .Where(r => !approvedClaimReportIds.Contains(r.Id))
            .Include(r => r.Student)
            .Include(r => r.Category)
            .AsQueryable();

        // Filter theo category nếu có
        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reportIds = reports.Select(r => r.Id).ToList();
        var claimsCount = await _context.StudentClaims
            .Where(c => c.LostReportId.HasValue && reportIds.Contains(c.LostReportId.Value))
            .GroupBy(c => c.LostReportId)
            .ToDictionaryAsync(g => g.Key!.Value, g => g.Count() > 0);

        // Staff mặc định được xem sensitive data, nhưng có thể control qua parameter
        return reports.Select(r => MapToResponse(r, claimsCount.GetValueOrDefault(r.Id, false), includeSensitiveData));
    }

    public async Task<StudentLostReportResponse?> GetByIdAsync(int id, int? studentId = null, bool includeSensitiveData = false)
    {
        var query = _context.StudentLostReports
            .Include(r => r.Student)
            .Include(r => r.Category)
            .AsQueryable();

        // Nếu có studentId, chỉ lấy report của student đó (cho Student role)
        if (studentId.HasValue)
        {
            query = query.Where(r => r.StudentId == studentId.Value);
        }

        var report = await query.FirstOrDefaultAsync(r => r.Id == id);
        if (report == null)
            return null;

        var hasClaims = await _context.StudentClaims
            .AnyAsync(c => c.LostReportId == id);

        // includeSensitiveData được pass từ controller dựa trên role
        return MapToResponse(report, hasClaims, includeSensitiveData);
    }

    public async Task<StudentLostReportResponse?> UpdateAsync(int id, int studentId, UpdateStudentLostReportRequest request)
    {
        // Chỉ cho phép update report của chính mình và chưa có claim nào
        var report = await _context.StudentLostReports
            .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == studentId);

        if (report == null)
            return null;

        // Kiểm tra xem đã có claim chưa
        var hasClaims = await _context.StudentClaims
            .AnyAsync(c => c.LostReportId == id);

        if (hasClaims)
        {
            throw new InvalidOperationException("Không thể cập nhật báo mất đã có claim.");
        }

        report.CategoryId = request.CategoryId;
        report.Description = request.Description;
        report.LostDate = request.LostDate;
        report.LostLocation = request.LostLocation;
        report.IdentifyingFeatures = request.IdentifyingFeatures;

        // Chỉ cập nhật ImageUrl nếu có giá trị mới (nếu upload ảnh mới)
        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            report.ImageUrl = request.ImageUrl;
        }

        // Cập nhật password mới (plain text)
        if (request.ClaimPassword != null)
        {
            report.ClaimPassword = request.ClaimPassword;
        }

        _context.StudentLostReports.Update(report);
        await _context.SaveChangesAsync();

        // Load related data for response
        await _context.Entry(report)
            .Reference(r => r.Student)
            .LoadAsync();

        await _context.Entry(report)
            .Reference(r => r.Category)
            .LoadAsync();

        // Student không được xem sensitive data
        return MapToResponse(report, false, includeSensitiveData: false);
    }

    public async Task<bool> DeleteAsync(int id, int studentId)
    {
        // Chỉ cho phép delete report của chính mình và chưa có claim nào
        var report = await _context.StudentLostReports
            .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == studentId);

        if (report == null)
            return false;

        // Kiểm tra xem đã có claim chưa
        var hasClaims = await _context.StudentClaims
            .AnyAsync(c => c.LostReportId == id);

        if (hasClaims)
        {
            throw new InvalidOperationException("Không thể xóa báo mất đã có claim.");
        }

        _context.StudentLostReports.Remove(report);
        await _context.SaveChangesAsync();
        return true;
    }

    private static StudentLostReportResponse MapToResponse(StudentLostReport report, bool hasClaims, bool includeSensitiveData = false)
    {
        var response = new StudentLostReportResponse
        {
            Id = report.Id,
            StudentId = report.StudentId,
            StudentName = report.Student?.FullName,
            StudentCode = report.Student?.StudentCode,
            CategoryId = report.CategoryId,
            CategoryName = report.Category?.Name,
            Description = report.Description,
            LostDate = report.LostDate,
            LostLocation = report.LostLocation,
            ImageUrl = report.ImageUrl,
            CreatedAt = report.CreatedAt,
            HasClaims = hasClaims
        };

        // Chỉ hiển thị IdentifyingFeatures và ClaimPassword cho Staff/Security
        if (includeSensitiveData)
        {
            response.IdentifyingFeatures = report.IdentifyingFeatures;
            response.ClaimPassword = report.ClaimPassword; // Plain text để staff có thể thấy
        }

        return response;
    }
}


