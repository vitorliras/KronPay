using Domain.interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infra.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email);
    }

    public async Task<User?> GetByCpfAsync(string cpf)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Cpf.Value == cpf);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.Value == username);
    }

    public async Task<bool> AddAsync(User user)
    {
        var result = await _context.Users.AddAsync(user);
        return  result.State == EntityState.Added;
    }

    public bool Update(User user)
    {
        var result =  _context.Users.Update(user);
        return result.State == EntityState.Modified;
    }
}
