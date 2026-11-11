namespace Ork.Models.Payer;

public class CustomerWalletsResponse
{
    public List<CustomerWalletDetails> Data { get; set; } = new();
    public CustomerPagination Pagination { get; set; } = new();
}