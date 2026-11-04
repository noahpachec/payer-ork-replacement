using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ork.Entities.Payer
{
    public class Wallet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long WalletPK { get; set; }

        public string? SubscriptionId { get; set; }  // FK → Subscription.Id (nullable for first wallet)
        public Subscription? Subscription { get; set; }

        public string Index { get; set; }              // V = gVRK hex, unique
        public string Id { get; set; }                 // Ytmp → Y'', immutable after finalization
        public int DebitCounter { get; set; }           // Moved from License
        public bool IdFinalized { get; set; } = false;  // Once true, Id cannot change
        public string Status { get; set; } = "Pending"; // "Pending" or "Active"

        public long Expiry { get; set; }  // Unix epoch seconds — wallet expires after this time

        public byte[] VProof { get; set; }
        public byte[] YProof { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }
}
