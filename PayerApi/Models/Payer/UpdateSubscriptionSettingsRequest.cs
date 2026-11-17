namespace Ork.Models.Payer;

public class UpdateSubscriptionSettingsRequest
{
    public int? MaxAccounts { get; set; }

    public int? MaxConcurrentAccountsPerMonth { get; set; }

    public bool? AllowVendorCreation { get; set; }
}