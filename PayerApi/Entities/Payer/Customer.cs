using System.ComponentModel.DataAnnotations;

namespace Ork.Entities.Payer
{
    public class Customer
    {
        [Key]
        public string StripeCustomerId { get; set; }
        public string GVZK { get; set; }          // Keep this field (session validation)
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
