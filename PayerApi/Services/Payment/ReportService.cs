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

    public async Task<List<MonthlySubscriptionReportResponse>> GetMonthlySubscriptionReportAsync()
    {
        var year = DateTime.UtcNow.Year;

        var results = new List<MonthlySubscriptionReportResponse>();

        for (var month = 1; month <= 12; month ++)
        {
            var monthStart = new DateTimeOffset(
                year, month, 1, 0, 0, 0, TimeSpan.Zero);

            var nextMonthStart = monthStart.AddMonths(1);

            var startTimestamp = monthStart.ToUnixTimeSeconds();

            var endTimeStamp = nextMonthStart.ToUnixTimeSeconds();

            var count = await _context.Subscriptions
                .CountAsync(subscription =>
                subscription.CreatedAt >= startTimestamp &&
                subscription.CreatedAt < endTimeStamp);

            results.Add(new MonthlySubscriptionReportResponse
            {
                Month = monthStart.UtcDateTime,
                NewSubscriptions = count
            });

        }

        return results;
    }
}
