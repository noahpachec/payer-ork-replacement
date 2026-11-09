using Microsoft.EntityFrameworkCore;
using Ork.Helpers;
using Ork.Models.Payer;

namespace Ork.Services.Payment;

public class ReportService : IReportService
{
    private readonly PayerDataContext _context;

    public ReportService(PayerDataContext context)
    {
        _context = context;
    }

    public async Task<CustomerMetricsResponse> GetCustomerMetricsAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowEnd = now + (10L * 24 * 60 * 60);

        var customers = _context.Customers
            .Where(customer => customer.DeletedAt == null);

        var customersWithValidWallets = customers
            .Where(customer =>
                customer.Subscriptions.Any(subscription =>
                    subscription.Wallets.Any(wallet =>
                        wallet.Status == "Active" &&
                        wallet.Expiry > now)));
        
        var customersWithExpiringWallets = customers
            .Where(customer =>
                customer.Subscriptions.Any(subscription =>
                    subscription.Wallets.Any(wallet =>
                        wallet.Status == "Active" &&
                        wallet.Expiry > now &&
                        wallet.Expiry <= windowEnd)));
        
        var total = await customers.CountAsync();

        var valid = await customersWithValidWallets.CountAsync();

        var expiring = await customersWithExpiringWallets.CountAsync();

        return new CustomerMetricsResponse
        {
            Total = total,
            Active = valid - expiring,
            Expiring = expiring,
            Inactive = total - valid
        };
    }
}
