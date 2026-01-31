using Domain.ValueObjects;
using Domain.ValueObjects.User;
using KronPay.Domain.Entities.Configuration;
using KronPay.Infra.Data.Mappings.Configuration;
using KronPay.Infra.Data.Seeds;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext
{

    public DbSet<User> Users => Set<User>();
    public DbSet<TypeTransaction> TypeTransactions => Set<TypeTransaction>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        #region User
        modelBuilder.Owned<Email>();
        modelBuilder.Owned<Cpf>();
        modelBuilder.Owned<Phone>();
        modelBuilder.Owned<Name>();
        #endregion

        #region TypeTransaction
        modelBuilder.ApplyConfiguration(new TypeTransactionMap());
        modelBuilder.Entity<TypeTransaction>().HasData(TypeTransactionSeed.Data);
        #endregion

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }


    

}
