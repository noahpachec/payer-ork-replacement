using System.Text.Json.Serialization;

namespace Ork.Models.Payer;

public class CustomerMetricsResponse
{
    [JsonPropertyName("Total")]
    public int Total { get; set; }

    [JsonPropertyName("Active")]
    public int Active { get; set; }

    [JsonPropertyName("Expiring")]
    public int Expiring { get; set; }

    [JsonPropertyName("Inactive")]
    public int Inactive { get; set; }
}