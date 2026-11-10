using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace Ork.Models.Payer;

public class CustomerDetailsResponse
{
    [JsonPropertyName("LastLoginTime")]
    public long? LastLoginTime { get; set; }

    [JsonPropertyName("LatestWallet")]
    public CustomerWalletDetails? LatestWallet { get; set; }

    [JsonPropertyName("Subscription")]
    public CustomerSubscriptionDetails? Subscription { get; set; }

    [JsonPropertyName("WalletCount")]
    public int WalletCount { get; set; }

}

public class CustomerWalletDetails
{
    [JsonPropertyName("WalletPK")]
    public long WalletPK { get; set;}

    [JsonPropertyName("SubscriptionId")]
    public string? SubscriptionId { get; set; }

    [JsonPropertyName("Index")]
    public string Index { get; set; } = "";

    [JsonPropertyName("Id")]
    public string Id { get; set;} = "";

    [JsonPropertyName("DebitCounter")]
    public int DebitCounter { get; set; }

    [JsonPropertyName("Status")]
    public string Status { get; set;} = "";

    [JsonPropertyName("Expiry")]
    public long Expiry { get; set; }

    [JsonPropertyName("CreatedAt")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("UpdatedAt")]
    public long UpdatedAt { get; set; } 
}

public class CustomerSubscriptionDetails
{
    [JsonPropertyName("SubscriptionId")]
    public string SubscriptionId { get; set; } = "";

    [JsonPropertyName("MaxAccounts")]
    public int MaxAccounts { get; set; }

    [JsonPropertyName("MaxConcurrentAccountsPerMonth")]
    public int MaxConcurrentAccountsPerMonth { get; set; }

    [JsonPropertyName("AllowVendorCreation")]
    public bool AllowVendorCreation { get; set; }

    [JsonPropertyName("VendorCreated")]
    public int VendorCreated { get; set; }

    [JsonPropertyName("CreatedAt")]
    public long CreatedAt { get; set; }
}

