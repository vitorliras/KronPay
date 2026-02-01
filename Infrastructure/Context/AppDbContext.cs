using Domain.Entities;
using Domain.Entities.Configuration;
using Domain.ValueObjects;
using Domain.ValueObjects.User;
using Infrastructure.Mappings;
using KronPay.Domain.Entities.Configuration;
using KronPay.Infra.Data.Mappings.Configuration;
using KronPay.Infra.Data.Seeds;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext
{

    public DbSet<User> Users => Set<User>();
    public DbSet<TypeTransaction> TypeTransactions => Set<TypeTransaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryItem> CategoryItems => Set<CategoryItem>();
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();


    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        #region User
        //modelBuilder.Owned<Email>();
        //modelBuilder.Owned<Cpf>();
        //modelBuilder.Owned<Phone>();
        //modelBuilder.Owned<Name>();
        modelBuilder.ApplyConfiguration(new UserMapping());

        #endregion

        #region TypeTransaction
        modelBuilder.ApplyConfiguration(new TypeTransactionMap());
        modelBuilder.Entity<TypeTransaction>().HasData(TypeTransactionSeed.Data);
        #endregion

        #region TypePaymentMethod
        modelBuilder.ApplyConfiguration(new TypePaymentMethodMap());
        modelBuilder.Entity<TypePaymentMethod>().HasData(TypePaymentMethodMapSeed.Data);
        #endregion

        modelBuilder.ApplyConfiguration(new CategoryMap());

        modelBuilder.ApplyConfiguration(new PaymentMethodMap());

        modelBuilder.ApplyConfiguration(new CreditCardMap());



        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }


    

}
