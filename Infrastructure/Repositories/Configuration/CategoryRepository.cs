using Domain.Entities.Configuration;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(Category category)
    {
        var result =  _context.Categories.Add(category);
        return  result.State == EntityState.Added;
    }

    public  bool Update(Category category)
    {
        var result = _context.Categories.Update(category);
        return result.State == EntityState.Modified;
    }

    public async Task<Category?> GetByIdAsync(int id, int userId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<IEnumerable<Category>> GetAllAsync(int userId)
    {
        return await _context.Categories
            .Where(x => x.UserId == userId && x.Active)
            .ToListAsync();
    }

    public async Task<Category?> GetByDescriptionAsync(string description, int userId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(x => x.Description == description && x.UserId == userId);
    }
}
