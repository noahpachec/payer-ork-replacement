using Microsoft.EntityFrameworkCore;
using Ork.Entities.Payer;
using Ork.Helpers;
using Ork.Shared;
using Ork.Exceptions.Network;
using Microsoft.Extensions.Logging;

namespace Ork.Services.Payment;

// Copied from Ork/Ork/Services/Payment/WalletService.cs. All methods are DB-only;
// the unused Settings dependency has been dropped.
public class WalletService : IWalletService
{
    private readonly PayerDataContext _context;
    private readonly ILogger _logger;

    public WalletService(PayerDataContext context, ILogger<WalletService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void CreateWallet(string index, string id, byte[] vProof, byte[] yProof, string? customerId = null)
    {
        long now = Utils.GetEpochSeconds();

        // If customerId is provided, look up the customer's first subscription
        string? subscriptionId = null;
        if (!string.IsNullOrEmpty(customerId))
        {
            var subscription = _context.Subscriptions
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            if (subscription != null)
            {
                subscriptionId = subscription.Id;
            }
            else
            {
                _logger.LogWarning($"CreateWallet: No subscription found for customer {customerId}");
            }
        }

        if (subscriptionId != null)
        {
            int superseded = _context.Wallets
                .Where(w => w.SubscriptionId == subscriptionId && w.Status == "Pending")
                .ExecuteDelete();

            if (superseded > 0)
            {
                _logger.LogInformation(
                    "CreateWallet: superseded {Count} Pending wallet(s) on subscription {SubscriptionId}",
                    superseded, subscriptionId);
            }
        }

        // SHORT pending-window expiry. The authoritative expiry is stamped later by
        // ActivateWalletForNewSubscription / ActivatePendingWalletOnSubscription at
        // webhook-verified activation (= liveStripeCurrentPeriodEnd + grace + rotation).
        // Until then this Pending wallet self-expires quickly so an unpaid/unactivated
        // wallet cannot be abused.
        long expiry = now + ExpiryConstants.PendingWalletWindowSeconds;

        var wallet = new Wallet
        {
            Index = index,
            Id = id,
            VProof = vProof,
            YProof = yProof,
            CreatedAt = now,
            UpdatedAt = now,
            Expiry = expiry,
            SubscriptionId = subscriptionId,
            DebitCounter = 0,
            IdFinalized = false,
            Status = "Pending"
        };
        _context.Wallets.Add(wallet);
        _context.SaveChanges();
    }

    public void UpdateWalletId(string index, string newId, byte[] vProof, byte[] yProof, string? customerId = null)
    {
        var wallet = _context.Wallets.FirstOrDefault(w => w.Index == index);
        if (wallet == null) throw new WalletNotFoundForIndex(index);

        // Immutability guard: once Id is finalized, it cannot change
        if (wallet.IdFinalized)
            throw new WalletAlreadyFinalized(wallet.Id);

        wallet.Id = newId;
        wallet.VProof = vProof;
        wallet.YProof = yProof;
        wallet.UpdatedAt = Utils.GetEpochSeconds();
        wallet.IdFinalized = true;

        // If customerId is provided and wallet doesn't have a subscription yet, link it
        if (!string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(wallet.SubscriptionId))
        {
            var subscription = _context.Subscriptions
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            if (subscription != null)
            {
                wallet.SubscriptionId = subscription.Id;
            }
            else
            {
                _logger.LogWarning($"UpdateWalletId: No subscription found for customer {customerId}");
            }
        }

        _context.SaveChanges();
    }

    public Wallet? GetWallet(string index)
    {
        return _context.Wallets.AsNoTracking().FirstOrDefault(w => w.Index == index);
    }

    public Wallet? GetWalletById(string id)
    {
        return _context.Wallets.AsNoTracking().FirstOrDefault(w => w.Id == id);
    }

    // Exemplar addition (not in ork)
    public List<Wallet> GetAllWallets()
    {
        return _context.Wallets.AsNoTracking().ToList();
    }

}
