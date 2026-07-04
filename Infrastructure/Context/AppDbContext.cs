using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Card;
using Domain.Entities.banks;
using Domain.Entities.Configuration;
using Domain.Entities.Planning;
using Domain.Entities.Transactions;
using Infrastructure.Mappings;
using Infrastructure.Mappings.banks;
using Infrastructure.Mappings.Transactions;
using KronPay.Domain.Entities.Configuration;
using KronPay.Domain.Entities.Users;
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
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionGroup> TransactionsGroups => Set<TransactionGroup>();
    public DbSet<TypeUser> TyoeUsers => Set<TypeUser>();
    public DbSet<BankConnection> BankConnections { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<CardPurchase> CardPurchases => Set<CardPurchase>();
    public DbSet<CardInvoice> CardInvoices => Set<CardInvoice>();
    public DbSet<CardInstallment> CardInstallments => Set<CardInstallment>();
    public DbSet<PlannedCommitment> PlannedCommitments => Set<PlannedCommitment>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    
        modelBuilder.ApplyConfiguration(new UserMapping());

        #region TypeTransaction
        modelBuilder.ApplyConfiguration(new TypeTransactionMap());
        modelBuilder.Entity<TypeTransaction>().HasData(TypeTransactionSeed.Data);
        #endregion

        #region StatusTransaction
        modelBuilder.ApplyConfiguration(new StatusTransactionMap());
        modelBuilder.Entity<StatusTransaction>().HasData(StatusTransactionMapSeed.Data);
        #endregion

        #region TypeUser
        modelBuilder.ApplyConfiguration(new TypeUserMap());
        modelBuilder.Entity<TypeUser>().HasData(UserTypeSeed.Data);
        #endregion

        modelBuilder.ApplyConfiguration(new CategoryMap());

        modelBuilder.ApplyConfiguration(new PaymentMethodMap());

        modelBuilder.ApplyConfiguration(new CreditCardMap());

        modelBuilder.ApplyConfiguration(new TransactionMap());

        modelBuilder.ApplyConfiguration(new TransactionGroupMap());

        modelBuilder.ApplyConfiguration(new BankMap());
        modelBuilder.Entity<Bank>().HasData(BankSeed.Data);


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }


    

}
