namespace Ork.Shared
{
    // Copied from Ork.Shared/Settings.cs - only the constant used by WalletService is kept.
    public static class ExpiryConstants
    {
        // Short self-expiry window for a freshly created Pending wallet (prod).
        // A Pending wallet (created by GenWallet) is expected to be activated almost
        // immediately by the Stripe webhook (checkout.session.completed / invoice.payment_succeeded),
        // at which point ActivateWallet* stamps the AUTHORITATIVE sanctioned expiry. Until then it
        // must self-expire quickly so an unpaid/unactivated pending wallet cannot be abused.
        // 2h comfortably covers the gen->activation handoff while bounding abuse.
        public const long PendingWalletWindowSeconds = 2 * 60 * 60L; // 2 hours
    }
}
