using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public sealed class PostgresAppDbContext : AppDbContext
{
    public PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options)
        : base(options)
    {
    }
}
