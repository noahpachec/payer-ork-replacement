using Microsoft.EntityFrameworkCore;
using Ork.Entities.Payer;
using Ork.Exceptions.Network;
using Ork.Helpers;
using Ork.Models.Payer;

namespace Ork.Services.Payment;

public class CustomerAdminService : ICustomerAdminService
{
    private readonly PayerDataContext _context;

    public CustomerAdminService(PayerDataContext context)
    {
        _context = context;
    }

    public async Task<CustomerDataResponse> GetCustomerDataAsync(
        string search,
        string? status,
        int page,
        int limit
    )
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowEnd = now + (10L * 24 * 60 * 60);

        var query = _context.Customers
            .Where(customer => customer.DeletedAt == null)
            .Where(customer =>
                EF.Functions.ILike(
                    customer.StripeCustomerId,
                    $"%{search}%"))
                .Select(customer => new CustomerDataRow
                {
                    CustomerId = customer.StripeCustomerId,
                    
                    SubscriptionCount = customer.Subscriptions.Count(),

                    Status = !customer.Subscriptions.Any(subscription =>
                        subscription.Wallets.Any(wallet =>
                            wallet.Status == "Active" &&
                            wallet.Expiry > now))
                        ? "inactive"

                        : customer.Subscriptions.Any(subscription =>
                            subscription.Wallets.Any(wallet =>
                                wallet.Status == "Active" &&
                                wallet.Expiry > now &&
                                wallet.Expiry <= windowEnd))
                            ? "expiring"

                            : "active"
                });
        if (status != null)
        {
            query = query.Where(customer => customer.Status == status);    
        }

        var total = await query.CountAsync();

        var offset = checked((page - 1) * limit);

        var data = await query
        .OrderBy(customer => customer.CustomerId)
        .Skip(offset)
        .Take(limit)
        .ToListAsync();

        return new CustomerDataResponse
        {
            Data = data,
            Pagination = new CustomerPagination
            {
                Page = page,
                Limit = limit,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            }
        };
    }

    public async Task<CustomerDetailsResponse> GetCustomerDetailsAsync(
        string  customerId)
    {
        var subscriptions = _context.Subscriptions
            .Where(subscription => subscription.CustomerId == customerId);
        
        var wallets = subscriptions
            .SelectMany(subscription => subscription.Wallets);
        
        var lastLoginTime = await subscriptions
            .SelectMany(subscription => subscription.SubscriptionUsers)
            .Select(user => (long?)user.LastLoginTime)
            .MaxAsync();

        var latestWallet = await wallets
        .OrderByDescending(wallet => wallet.CreatedAt)
        .ThenByDescending(wallet => wallet.WalletPK)
        .Select(wallet => new CustomerWalletDetails
        {
            WalletPK = wallet.WalletPK,
            SubscriptionId = wallet.SubscriptionId,
            Index = wallet.Index,
            Id = wallet.Id,
            DebitCounter = wallet.DebitCounter,
            Status = wallet.Status,
            Expiry = wallet.Expiry,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        })
        .FirstOrDefaultAsync();

        var subscriptionDetails = await subscriptions
            .OrderByDescending(subscription => subscription.CreatedAt)
            .ThenBy(subscription => subscription.Id)
            .Select(subscription => new CustomerSubscriptionDetails
            {
                SubscriptionId = subscription.Id,
                MaxAccounts = subscription.MaxAccounts,
                MaxConcurrentAccountsPerMonth = subscription.MaxConcurrentAccountsPerMonth,
                AllowVendorCreation = subscription.AllowVendorCreation,
                VendorCreated = subscription.VendorCreated,
                CreatedAt = subscription.CreatedAt

            })
            .FirstOrDefaultAsync();

        var walletCount = await wallets.CountAsync();

        return new CustomerDetailsResponse
        {
            LastLoginTime = lastLoginTime,
            LatestWallet = latestWallet,
            Subscription = subscriptionDetails,
            WalletCount = walletCount
        };

    }

    public async Task <CustomerWalletsResponse> GetCustomerWalletsAsync(
        string customerId,
        int page,
        int limit)
    {
        var orderedWallets = _context.Subscriptions
            .Where(subscription => subscription.CustomerId == customerId)
            .SelectMany(subscription => subscription.Wallets)
            .OrderByDescending(wallet => wallet.CreatedAt)
            .ThenByDescending(wallet => wallet.WalletPK);

        var total = await orderedWallets.CountAsync();

        var offset = checked((page - 1) * limit);

        var data = await orderedWallets
            .Skip(offset)
            .Take(limit)
            .Select(wallet => new CustomerWalletDetails
            {
                WalletPK = wallet.WalletPK,
                SubscriptionId = wallet.SubscriptionId,
                Index = wallet.Index,
                Id = wallet.Id,
                DebitCounter = wallet.DebitCounter,
                Status = wallet.Status,
                Expiry = wallet.Expiry,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt,
            })
            .ToListAsync();

        return new CustomerWalletsResponse
        {
            Data = data,
            Pagination = new CustomerPagination
            {
                Page = page,
                Limit = limit,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            }
        };
    }
}