namespace Ork.Models.Payer;

public class BulkDeleteCustomersRequest
{
    public List<string> CustomerIds { get; set; } = new();
}