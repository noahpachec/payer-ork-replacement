using Microsoft.EntityFrameworkCore;
using Ork.Entities.Payer;

namespace Ork.Helpers
{
    public class PayerDataContext : DbContext
    {
        public PayerDataContext(DbContextOptions<PayerDataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer → Subscriptions (1:many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Subscriptions)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId);

            // Subscription → SubscriptionUsers (1:many)
            modelBuilder.Entity<SubscriptionUser>()
                .HasOne(su => su.Subscription)
                .WithMany(s => s.SubscriptionUsers)
                .HasForeignKey(su => su.SubscriptionId);

            // Subscription → Wallets (1:many)
            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.Subscription)
                .WithMany(s => s.Wallets)
                .HasForeignKey(w => w.SubscriptionId);

            // Wallet.Index is unique but not PK
            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.Index)
                .IsUnique();

            // Wallet.Id index for fast lookup (voucher resolution)
            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.Id);
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionUser> SubscriptionUsers { get; set; }
        public DbSet<Validation> PayerValidations { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
    }
}
