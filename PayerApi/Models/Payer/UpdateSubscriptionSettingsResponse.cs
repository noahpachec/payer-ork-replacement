using System.Text.Json.Serialization;

namespace Ork.Models.Payer;

public class UpdateSubscriptionSettingsResponse
{
    [JsonPropertyName("SubscriptionId")]
    public string SubscriptionId { get; set; } = "";

    [JsonPropertyName("MaxAccounts")]
    public int MaxAccounts { get; set; }

    [JsonPropertyName("MaxConcurrentAccountsPerMonth")]
    public int MaxConcurrentAccountsPerMonth { get; set; }

    [JsonPropertyName("AllowVendorCreation")]
    public bool AllowVendorCreation { get; set; }
}