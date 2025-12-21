using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LostAndFound.Application.DTOs.MasterData;

namespace LostAndFound.Application.Interfaces.MasterData;

public interface IItemCategoryService
{
    Task<IEnumerable<ItemCategoryResponse>> GetAllAsync();
    Task<ItemCategoryResponse?> GetByIdAsync(int id);
    Task<ItemCategoryResponse> CreateAsync(CreateItemCategoryRequest request);
    Task<ItemCategoryResponse?> UpdateAsync(int id, UpdateItemCategoryRequest request);
    Task<bool> DeleteAsync(int id);
}


