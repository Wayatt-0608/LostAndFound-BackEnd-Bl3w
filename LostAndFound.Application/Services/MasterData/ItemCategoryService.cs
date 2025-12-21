using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LostAndFound.Application.DTOs.MasterData;
using LostAndFound.Application.Interfaces.MasterData;
using LostAndFound.Domain.Entities;
using LostAndFound.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Application.Services.MasterData;

public class ItemCategoryService : IItemCategoryService
{
    private readonly AppDbContext _context;

    public ItemCategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemCategoryResponse>> GetAllAsync()
    {
        var categories = await _context.ItemCategories
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new ItemCategoryResponse
        {
            Id = c.Id,
            Name = c.Name
        });
    }

    public async Task<ItemCategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _context.ItemCategories.FindAsync(id);
        if (category == null)
            return null;

        return new ItemCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<ItemCategoryResponse> CreateAsync(CreateItemCategoryRequest request)
    {
        var category = new ItemCategory
        {
            Name = request.Name
        };

        _context.ItemCategories.Add(category);
        await _context.SaveChangesAsync();

        return new ItemCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<ItemCategoryResponse?> UpdateAsync(int id, UpdateItemCategoryRequest request)
    {
        var category = await _context.ItemCategories.FindAsync(id);
        if (category == null)
            return null;

        category.Name = request.Name;

        _context.ItemCategories.Update(category);
        await _context.SaveChangesAsync();

        return new ItemCategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.ItemCategories.FindAsync(id);
        if (category == null)
            return false;

        _context.ItemCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}


