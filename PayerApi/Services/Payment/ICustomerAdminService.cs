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

    Task<int> SoftDeleteCustomerAsync(string customerId);

    Task<int> SoftDeleteCustomersAsync(List<string> customerIds);

    Task<UpdateSubscriptionSettingsResponse?> UpdateSubscriptionSettingsAsync(
        string subscriptionId,
        int maxAccounts,
        int maxConcurrentAccountsPerMonth,
        bool allowVendorCreation
    );

    Task<UpdateWalletSettingsResponse?> UpdateWalletSettingsAsync(
        long walletPk,
        long expiry
    );
}