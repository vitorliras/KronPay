using Domain.Entities;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infrastructure.Repositories;

public sealed class CreditCardRepository : ICreditCardRepository
{
    private readonly AppDbContext _context;

    public CreditCardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(CreditCard creditCard)
    {
        var result =  _context.CreditCards.Add(creditCard);
        return  result.State == EntityState.Added;
    }

    public  bool Update(CreditCard creditCard)
    {
        var result = _context.CreditCards.Update(creditCard);
        return result.State == EntityState.Modified;
    }

    public async Task<CreditCard?> GetByIdAsync(int id, int userId)
    {
        return await _context.CreditCards
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<IEnumerable<CreditCard>> GetAllAsync(int userId)
    {
        return await _context.CreditCards
            .Where(x => x.UserId == userId && x.Active)
            .ToListAsync();
    }

    public async Task<CreditCard?> GetByDescriptionAsync(string description, int userId)
    {
        return await _context.CreditCards
            .FirstOrDefaultAsync(x => x.Description == description && x.UserId == userId);
    }

}
