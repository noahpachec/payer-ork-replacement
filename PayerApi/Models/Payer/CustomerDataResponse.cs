using System.Text.Json.Serialization;

namespace Ork.Models.Payer;

public class CustomerDataRow
{

    [JsonPropertyName("Status")]
    public string Status { get; set; } = "";
    
    [JsonPropertyName("CustomerId")]
    public string CustomerId { get; set; } = "";

    [JsonPropertyName("SubscriptionCount")]
    public int SubscriptionCount { get; set; }


}

public class CustomerPagination
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

public class CustomerDataResponse
{
    public List<CustomerDataRow> Data { get; set; } = new();
    public CustomerPagination Pagination {get; set;} = new();
}