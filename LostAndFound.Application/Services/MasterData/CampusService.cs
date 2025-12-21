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

public class CampusService : ICampusService
{
    private readonly AppDbContext _context;

    public CampusService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampusResponse>> GetAllAsync()
    {
        var campuses = await _context.Campuses
            .OrderBy(c => c.Name)
            .ToListAsync();

        return campuses.Select(c => new CampusResponse
        {
            Id = c.Id,
            Name = c.Name,
            Location = c.Location
        });
    }

    public async Task<CampusResponse?> GetByIdAsync(int id)
    {
        var campus = await _context.Campuses.FindAsync(id);
        if (campus == null)
            return null;

        return new CampusResponse
        {
            Id = campus.Id,
            Name = campus.Name,
            Location = campus.Location
        };
    }

    public async Task<CampusResponse> CreateAsync(CreateCampusRequest request)
    {
        var campus = new Campus
        {
            Name = request.Name,
            Location = request.Location
        };

        _context.Campuses.Add(campus);
        await _context.SaveChangesAsync();

        return new CampusResponse
        {
            Id = campus.Id,
            Name = campus.Name,
            Location = campus.Location
        };
    }

    public async Task<CampusResponse?> UpdateAsync(int id, UpdateCampusRequest request)
    {
        var campus = await _context.Campuses.FindAsync(id);
        if (campus == null)
            return null;

        campus.Name = request.Name;
        campus.Location = request.Location;

        _context.Campuses.Update(campus);
        await _context.SaveChangesAsync();

        return new CampusResponse
        {
            Id = campus.Id,
            Name = campus.Name,
            Location = campus.Location
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var campus = await _context.Campuses.FindAsync(id);
        if (campus == null)
            return false;

        _context.Campuses.Remove(campus);
        await _context.SaveChangesAsync();
        return true;
    }
}


