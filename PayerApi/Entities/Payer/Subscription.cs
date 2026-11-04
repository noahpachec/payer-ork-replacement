using System.ComponentModel.DataAnnotations;

namespace Ork.Entities.Payer
{
    public class Subscription
    {
        [Key]
        public string Id { get; set; }  // UUID, generated via Guid.NewGuid().ToString()
        public string CustomerId { get; set; }  // FK → Customer.StripeCustomerId
        public Customer Customer { get; set; }

        public int MaxAccounts { get; set; }
        public int MaxConcurrentAccountsPerMonth { get; set; }
        public bool AllowVendorCreation { get; set; }
        public int VendorCreated { get; set; } = 0;
        public long CreatedAt { get; set; }

        public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
        public List<SubscriptionUser> SubscriptionUsers { get; set; } = new();
    }

    public class SubscriptionUser
    {
        [Key]
        public string UniqUID { get; set; }
        public string SubscriptionId { get; set; }  // FK → Subscription.Id
        public Subscription Subscription { get; set; }
        public long LastLoginTime { get; set; }
    }
}
