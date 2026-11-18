using System.Text.Json.Serialization;

namespace Ork.Models.Payer;


public class UpdateWalletSettingsResponse
{
    [JsonPropertyName("WalletPK")]
    public long? WalletPK { get; set; }

    [JsonPropertyName("Expiry")]
    public long? Expiry { get; set; }

    [JsonPropertyName("UpdatedAt")]
    public long? UpdatedAt { get; set; }
}