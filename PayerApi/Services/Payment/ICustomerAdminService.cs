using Ork.Models.Payer;

namespace Ork.Services.Payment;

public interface ICustomerAdminService
{
    Task<CustomerDataResponse> GetCustomerDataAsync(
        string search,
        string? status,
        int page,
        int limit
    );
    
    Task<CustomerDetailsResponse> GetCustomerDetailsAsync(
        string customerId
    );

    Task<CustomerWalletsResponse> GetCustomerWalletsAsync(
        string customerId,
        int page,
        int limit
    );


}