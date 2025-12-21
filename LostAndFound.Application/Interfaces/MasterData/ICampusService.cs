using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LostAndFound.Application.DTOs.MasterData;

namespace LostAndFound.Application.Interfaces.MasterData;

public interface ICampusService
{
    Task<IEnumerable<CampusResponse>> GetAllAsync();
    Task<CampusResponse?> GetByIdAsync(int id);
    Task<CampusResponse> CreateAsync(CreateCampusRequest request);
    Task<CampusResponse?> UpdateAsync(int id, UpdateCampusRequest request);
    Task<bool> DeleteAsync(int id);
}


