using System.Text.Json.Serialization;

namespace Ork.Models.Payer;

public class MonthlySubscriptionReportResponse
{
    [JsonPropertyName("Month")]
    public DateTime Month { get; set; }

    [JsonPropertyName("NewSubscriptions")]
    public int NewSubscriptions { get; set; }
}

