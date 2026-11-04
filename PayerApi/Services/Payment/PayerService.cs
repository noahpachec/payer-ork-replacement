//
// Tide Protocol - Infrastructure for a TRUE Zero-Trust paradigm
// Copyright (C) 2022 Tide Foundation Ltd
//
// This program is free software and is subject to the terms of
// the Tide Community Open Code License as published by the
// Tide Foundation Limited. You may modify it and redistribute
// it in accordance with and subject to the terms of that License.
// This program is distributed WITHOUT WARRANTY of any kind,
// including without any implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.
// See the Tide Community Open Code License for more details.
// You should have received a copy of the Tide Community Open
// Code License along with this program.
// If not, see https://tide.org/licenses_tcoc2-0-0-en
//

namespace Ork.Services.Payment;
using Microsoft.EntityFrameworkCore;
using Ork.Entities.Payer;
using Ork.Exceptions.Network;
using Ork.Helpers;
using Ork.Models.Payer;
using System.Text.Json;

// Copied from Ork/Ork/Services/Payment/PayerService.cs, keeping only the methods that
// purely read/write the payer database. Omitted (crypto or business-rule logic):
// GetVRK, IsSessionRequestValid, IsSessionRequestValidByCustomer, IsLifecycleRequestValid,
// IsBoundRequestValid, GetWalletLifecycleState, CheckVRKisValid, CheckRequestedActionAllowed,
// IsActiveSubscriptionByCustomer.
public interface IPayerService
{
    string AddSubscription(string subscriptionData, string customerId, string authorizerPackHex);
    void AddValidation(RedemptContent content);
    string GetSubscriptionDetails(string obfGVVK);
    CustomerModel GetCustomerModel(string customerId);
    string GetStripeCustomerId(string obfGVVK);
    bool IsNewSubscription(string walletIndex);
    void ActivateWalletForNewSubscription(string customerId, string walletIndex, long sanctionedExpiry);
    void ActivatePendingWalletOnSubscription(string customerId, long sanctionedExpiry);
    void UpdateSubscriptionSettings(string customerId, int maxAccounts, int maxConcurrentAccountsPerMonth);

    // Customer-ID-based lookups for info/status endpoints (no wallet lookup needed)
    string GetSubscriptionDetailsByCustomer(string customerId);
    long GetActiveWalletExpiry(string customerId);

    // Exemplar addition (not in ork)
    List<Subscription> GetAllSubscriptions();
}

public class PayerService : IPayerService
{
    private PayerDataContext _context;
    private ILogger _logger;
    public PayerService(PayerDataContext context, ILogger<PayerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public string AddSubscription(string subscriptionData, string customerId, string authorizerPackHex)
    {
        // Deserialize subscription data
        SubscriptionTransportModel subModel = subscriptionData.DeserializeTo<SubscriptionTransportModel>();

        // Find existing customer or create a new one
        // GVZK stores the full AuthorizerPack hex (not the raw gVRK point), because
        // IsSessionRequestValid needs to call AuthorizerPack.From(customer.GVZK) to extract the signing key.
        var customerEntry = _context.Customers.SingleOrDefault(entry => entry.StripeCustomerId == customerId)
                          ?? new Customer { StripeCustomerId = customerId, GVZK = authorizerPackHex };

        // Check if the customer is newly created or already tracked in the database
        if (_context.Entry(customerEntry).State == EntityState.Detached)
        {
            _context.Customers.Add(customerEntry);
            _context.SaveChanges();
        }

        // Create the Subscription with a UUID Id
        Subscription subscription = new Subscription
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = customerEntry.StripeCustomerId,
            Customer = customerEntry,
            MaxAccounts = subModel.MaxAccounts,
            MaxConcurrentAccountsPerMonth = subModel.MaxConcurrentAccountsPerMonth,
            AllowVendorCreation = subModel.AllowVendorCreation,
            CreatedAt = Utils.GetEpochSeconds(),
        };

        // Add the subscription to the context
        _context.Subscriptions.Add(subscription);
        _context.SaveChanges();

        return subscription.Id;
    }

    public void AddValidation(RedemptContent content)
    {
        Validation newValidation = new Validation
        {
            Id = content.Digest.ToBase64String(),
            Action = content.Action,
            Key = content.Key,
            Timestamp = content.Timestamp
        };
        _context.PayerValidations.Add(newValidation);
        _context.SaveChanges();
    }

    public string GetSubscriptionDetails(string obfGVVK)
    {
        // Query directly without expiry check — this is an informational endpoint
        var wallet = _context.Wallets
            .Include(w => w.Subscription)
            .FirstOrDefault(w => w.Id == obfGVVK);
        if (wallet == null) throw new Exception("Could not find ObfGVVK in Database");
        var subscription = wallet.Subscription;
        if (subscription == null) throw new Exception("Wallet has no associated subscription");

        var subWithUsers = _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.SubscriptionUsers)
            .SingleOrDefault(s => s.Id == subscription.Id);
        if (subWithUsers == null) throw new Exception("Could not find Subscription in Database");

        return JsonSerializer.Serialize(new { currentUserAcc = subWithUsers.SubscriptionUsers.Count, expiryDate = wallet.Expiry });
    }

	public bool IsNewSubscription(string walletIndex)
	{
		// Check if a wallet exists with the given wallet Index (raw gVRK hex)
		var wallet = _context.Wallets
			.Include(w => w.Subscription)
			.FirstOrDefault(w => w.Index == walletIndex);
		if (wallet == null) return true;
		if (wallet.Subscription == null) return true;
		return false;
	}

	public CustomerModel GetCustomerModel(string stripeCustomerId)
    {
        var entry = _context.Customers
        .AsNoTracking()
        .SingleOrDefault(entry => entry.StripeCustomerId == stripeCustomerId);
        if (entry == null) throw new CustomerNotFoundException(stripeCustomerId);

        CustomerModel customerModel = new CustomerModel
        {
            StripeCustomerId = stripeCustomerId
        };
        return customerModel;
    }
    public string GetStripeCustomerId(string obfGVVK)
    {
        var wallet = FindWalletByIdentity(obfGVVK);
        if (wallet == null) throw new Exception("Could not find wallet in Database");
        if (wallet.Subscription == null) throw new Exception("Wallet has no associated subscription");

        return wallet.Subscription.CustomerId;
    }

	/// <summary>
	/// Links the most recently created unlinked wallet to the newest subscription for a given customer.
	/// Used after HandleCheckoutSessionCompleted when the wallet was created by GenWallet before checkout.
	/// </summary>
	public void ActivateWalletForNewSubscription(string customerId, string walletIndex, long expiry)
	{
		// Find the newest subscription for this customer
		var subscription = _context.Subscriptions
			.Where(s => s.CustomerId == customerId)
			.OrderByDescending(s => s.CreatedAt)
			.FirstOrDefault();

		if (subscription == null)
		{
			_logger.LogError($"ActivateWalletForNewSubscription: No subscription found for customer {customerId}");
			return;
		}

		// Find the unlinked wallet matching the specific wallet Index (gVRK)
		// to avoid linking the wrong wallet under concurrent checkouts
		var wallet = _context.Wallets
			.FirstOrDefault(w => w.SubscriptionId == null && w.Index == walletIndex);

		if (wallet == null)
		{
			// walletIndex is the raw gVRK hex (cryptographic material) — do not log its value.
			_logger.LogWarning($"ActivateWalletForNewSubscription: No unlinked wallet found for customer {customerId}");
			return;
		}

		wallet.SubscriptionId = subscription.Id;
		wallet.Status = "Active";
		if (expiry > Utils.GetEpochSeconds()) // cannot be in past
		{
			wallet.Expiry = expiry;
		}
		wallet.UpdatedAt = Utils.GetEpochSeconds();
		_context.SaveChanges();
	}

	/// <summary>
	/// Finds the most recently created Pending wallet on the active subscription for a given customer
	/// and marks it Active. Used after invoice.payment_succeeded for monthly renewals.
	/// </summary>
	public void ActivatePendingWalletOnSubscription(string customerId, long expiry)
	{
		var subscription = _context.Subscriptions
			.Include(s => s.Wallets)
			.Where(s => s.CustomerId == customerId)
			.OrderByDescending(s => s.CreatedAt)
			.FirstOrDefault();

		if (subscription == null)
		{
			_logger.LogError($"ActivatePendingWalletOnSubscription: No subscription found for customer {customerId}");
			return;
		}

		var pendingWallet = subscription.Wallets
			.Where(w => w.Status == "Pending")
			.OrderByDescending(w => w.CreatedAt)
			.FirstOrDefault();

		if (pendingWallet == null)
		{
			_logger.LogWarning($"ActivatePendingWalletOnSubscription: No pending wallet found for subscription {subscription.Id}");
			return;
		}

		pendingWallet.Status = "Active";
		if (expiry > Utils.GetEpochSeconds()) // cannot be in past
		{
			pendingWallet.Expiry = expiry;
		}
		pendingWallet.UpdatedAt = Utils.GetEpochSeconds();
		_context.SaveChanges();
	}

	/// <summary>
	/// Updates an existing subscription's settings (tier change).
	/// Used after invoice.payment_succeeded when the tier may have changed.
	/// </summary>
	public void UpdateSubscriptionSettings(string customerId, int maxAccounts, int maxConcurrentAccountsPerMonth)
	{
		var subscription = _context.Subscriptions
			.Where(s => s.CustomerId == customerId)
			.OrderByDescending(s => s.CreatedAt)
			.FirstOrDefault();

		if (subscription == null)
		{
			_logger.LogError($"UpdateSubscriptionSettings: No subscription found for customer {customerId}");
			return;
		}

		subscription.MaxAccounts = maxAccounts;
		subscription.MaxConcurrentAccountsPerMonth = maxConcurrentAccountsPerMonth;
		_context.SaveChanges();
	}

	/// <summary>
	/// Gets subscription details using the Stripe customer ID directly (no wallet lookup).
	/// Finds the customer's subscription and returns user count.
	/// </summary>
	public string GetSubscriptionDetailsByCustomer(string customerId)
	{
		var subscription = _context.Subscriptions
			.AsNoTracking()
			.Include(s => s.SubscriptionUsers)
			.Include(s => s.Wallets)
			.Where(s => s.CustomerId == customerId)
			.OrderByDescending(s => s.CreatedAt)
			.FirstOrDefault();
		if (subscription == null) throw new Exception("Could not find Subscription for customer: " + customerId);

		var activeWallet = subscription.Wallets?
			.Where(w => w.Status == "Active")
			.OrderByDescending(w => w.CreatedAt)
			.FirstOrDefault();

		return JsonSerializer.Serialize(new { currentUserAcc = subscription.SubscriptionUsers.Count, expiryDate = activeWallet?.Expiry ?? 0L });
	}

	/// <summary>
	/// Gets the expiry date of the active wallet for a customer's most recent subscription.
	/// Returns 0 if no subscription or active wallet is found.
	/// </summary>
	public long GetActiveWalletExpiry(string customerId)
	{
		var subscription = _context.Subscriptions
			.AsNoTracking()
			.Include(s => s.Wallets)
			.Where(s => s.CustomerId == customerId)
			.OrderByDescending(s => s.CreatedAt)
			.FirstOrDefault();

		var activeWallet = subscription?.Wallets?
			.Where(w => w.Status == "Active")
			.OrderByDescending(w => w.CreatedAt)
			.FirstOrDefault();

		return activeWallet?.Expiry ?? 0L;
	}

	/// <summary>
	/// Exemplar addition (not in ork). Returns every subscription without its navigations.
	/// </summary>
	public List<Subscription> GetAllSubscriptions()
	{
		return _context.Subscriptions.AsNoTracking().ToList();
	}

	/// <summary>
	/// Resolves a voucher identity (obfGVVK / Y'') to the correct wallet.
	/// Finds the wallet by its Id (Y'' after UpdateWallet).
	/// </summary>
	private Wallet? FindWalletByIdentity(string identity)
	{
		var wallet = _context.Wallets
			.Include(w => w.Subscription)
			.FirstOrDefault(w => w.Id == identity);

		if (wallet != null && wallet.Expiry < Utils.GetEpochSeconds())
			return null;

		return wallet;
	}

}
