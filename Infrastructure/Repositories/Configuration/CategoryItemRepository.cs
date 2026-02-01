using Domain.Entities;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infrastructure.Repositories;

public sealed class CategoryItemRepository : ICategoryItemRepository
{
    private readonly AppDbContext _context;

    public CategoryItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(CategoryItem item)
    {
        var result =  _context.CategoryItems.Add(item);
        return  result.State == EntityState.Added;
    }

    public  bool UpdateAsync(CategoryItem item)
    {
        var result = _context.CategoryItems.Update(item);
        return result.State == EntityState.Modified;
    }

    public async Task<CategoryItem?> GetByIdAsync(int id, int categoryId)
    {
        return await _context.CategoryItems
            .FirstOrDefaultAsync(x => x.Id == id && x.CategoryId == categoryId);
    }

    public async Task<IEnumerable<CategoryItem>> GetAllAsync(int categoryId)
    {
        return await _context.CategoryItems
            .Where(x => x.CategoryId == categoryId && x.Active)
            .ToListAsync();
    }

    public async Task<CategoryItem?> GetByDescriptionAsync(string description, int categoryId)
    {
        return await _context.CategoryItems
            .FirstOrDefaultAsync(x => x.Description == description && x.CategoryId == categoryId);
    }
}
