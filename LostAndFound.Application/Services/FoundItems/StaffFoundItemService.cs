using LostAndFound.Application.DTOs.FoundItems;
using LostAndFound.Application.Interfaces.Cases;
using LostAndFound.Application.Interfaces.FoundItems;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.FoundItems;

public class StaffFoundItemService : IStaffFoundItemService
{
    private readonly AppDbContext _context;
    private readonly ICaseService _caseService;

    public StaffFoundItemService(AppDbContext context, ICaseService caseService)
    {
        _context = context;
        _caseService = caseService;
    }

    public async Task<StaffFoundItemResponse> CreateAsync(int createdBy, CreateStaffFoundItemRequest request)
    {
        var foundItem = new StaffFoundItem
        {
            CreatedBy = createdBy,
            CategoryId = request.CategoryId,
            CampusId = request.CampusId,
            Description = request.Description,
            FoundDate = request.FoundDate,
            FoundLocation = request.FoundLocation,
            Status = "STORED", // Mặc định là STORED
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.Now
        };

        _context.StaffFoundItems.Add(foundItem);
        await _context.SaveChangesAsync();

        // Tự động tạo Case với status = 'OPEN'
        await _caseService.CreateCaseForFoundItemAsync(foundItem.Id, foundItem.CampusId);

        // Load related data for response
        await _context.Entry(foundItem)
            .Reference(f => f.CreatedByNavigation)
            .LoadAsync();

        await _context.Entry(foundItem)
            .Reference(f => f.Category)
            .LoadAsync();

        await _context.Entry(foundItem)
            .Reference(f => f.Campus)
            .LoadAsync();

        return MapToResponse(foundItem);
    }

    public async Task<IEnumerable<StaffFoundItemResponse>> GetAllAsync(int? campusId = null, string? status = null, int? categoryId = null)
    {
        var query = _context.StaffFoundItems
            .Include(f => f.CreatedByNavigation)
            .Include(f => f.Category)
            .Include(f => f.Campus)
            .AsQueryable();

        // Apply filters
        if (campusId.HasValue)
        {
            query = query.Where(f => f.CampusId == campusId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(f => f.Status == status);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.CategoryId == categoryId.Value);
        }

        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return items.Select(MapToResponse);
    }

    public async Task<StaffFoundItemResponse?> GetByIdAsync(int id)
    {
        var foundItem = await _context.StaffFoundItems
            .Include(f => f.CreatedByNavigation)
            .Include(f => f.Category)
            .Include(f => f.Campus)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (foundItem == null)
            return null;

        return MapToResponse(foundItem);
    }

    public async Task<StaffFoundItemResponse?> UpdateAsync(int id, UpdateStaffFoundItemRequest request)
    {
        var foundItem = await _context.StaffFoundItems.FindAsync(id);
        if (foundItem == null)
            return null;

        // Chỉ cho phép update nếu status là STORED (chưa trả đồ)
        if (foundItem.Status != "STORED")
        {
            throw new InvalidOperationException("Chỉ có thể cập nhật đồ nhặt được đang ở trạng thái STORED.");
        }

        if (request.CategoryId.HasValue)
        {
            foundItem.CategoryId = request.CategoryId.Value;
        }

        if (request.CampusId.HasValue)
        {
            foundItem.CampusId = request.CampusId.Value;
        }

        foundItem.Description = request.Description;
        foundItem.FoundDate = request.FoundDate;
        foundItem.FoundLocation = request.FoundLocation;

        // Chỉ cập nhật ImageUrl nếu có giá trị mới
        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            foundItem.ImageUrl = request.ImageUrl;
        }

        _context.StaffFoundItems.Update(foundItem);
        await _context.SaveChangesAsync();

        // Load related data for response
        await _context.Entry(foundItem)
            .Reference(f => f.CreatedByNavigation)
            .LoadAsync();

        await _context.Entry(foundItem)
            .Reference(f => f.Category)
            .LoadAsync();

        await _context.Entry(foundItem)
            .Reference(f => f.Campus)
            .LoadAsync();

        return MapToResponse(foundItem);
    }

    public async Task<StaffFoundItemResponse?> UpdateStatusAsync(int id, string status)
    {
        // Validate status
        if (status != "STORED" && status != "RETURNED")
        {
            throw new ArgumentException("Status chỉ có thể là STORED hoặc RETURNED.");
        }

        var foundItem = await _context.StaffFoundItems.FindAsync(id);
        if (foundItem == null)
            return null;

        foundItem.Status = status;
        _context.StaffFoundItems.Update(foundItem);
        await _context.SaveChangesAsync();

        // Load related data for response
        await _context.Entry(foundItem)
            .Reference(f => f.CreatedByNavigation)
            .LoadAsync();

        await _context.Entry(foundItem)
            .Reference(f => f.Category)
            .LoadAsync();

        await _context.Entry(foundItem)
            .Reference(f => f.Campus)
            .LoadAsync();

        return MapToResponse(foundItem);
    }

    private static StaffFoundItemResponse MapToResponse(StaffFoundItem item)
    {
        return new StaffFoundItemResponse
        {
            Id = item.Id,
            CreatedBy = item.CreatedBy,
            CreatedByName = item.CreatedByNavigation?.FullName,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name,
            CampusId = item.CampusId,
            CampusName = item.Campus?.Name,
            Description = item.Description,
            FoundDate = item.FoundDate,
            FoundLocation = item.FoundLocation,
            Status = item.Status,
            ImageUrl = item.ImageUrl,
            CreatedAt = item.CreatedAt
        };
    }
}

