using Domain.Entities.Configuration;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infrastructure.Repositories;

public sealed class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly AppDbContext _context;

    public PaymentMethodRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(PaymentMethod paymentMethod)
    {
        var result =  _context.PaymentMethods.Add(paymentMethod);
        return  result.State == EntityState.Added;
    }

    public  bool Update(PaymentMethod paymentMethod)
    {
        var result = _context.PaymentMethods.Update(paymentMethod);
        return result.State == EntityState.Modified;
    }

    public async Task<PaymentMethod?> GetByIdAsync(int id, int userId)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllAsync(int userId)
    {
        return await _context.PaymentMethods
            .Where(x => x.UserId == userId && x.Active)
            .ToListAsync();
    }

    public async Task<PaymentMethod?> GetByDescriptionAsync(string description, int userId)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(x => x.Description == description && x.UserId == userId);
    }

}
