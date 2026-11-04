namespace Ork.Models.Payer
{
    // Copied from Cryptide/Cryptide/Models/Vouchers.cs - plain data models only
    // (RedemptContent no longer derives from Cryptide's ModelBase).
    public class RedemptContent
    {
        public long Timestamp { get; set; }
        public string Action { get; set; }
        public string Key { get; set; }
        public byte[] Digest { get; set; }
    }

    public class SubscriptionTransportModel
    {
        public string GVRK { get; set; }
        public int MaxAccounts { get; set; }
        public int MaxConcurrentAccountsPerMonth { get; set; }
        public bool AllowVendorCreation { get; set; }
    }
}
