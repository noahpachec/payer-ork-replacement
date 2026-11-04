//
// Copied from Ork/Ork/Errors/TideErrorCodes.cs - only the codes thrown by the
// copied payer/wallet DB services are kept.
//

namespace Ork.Errors
{
    public static class TideErrorCodes
    {
        // --- Wallet lifecycle ---
        public const string WALLET_NOT_FOUND_INDEX = "TIDE-ORK-WALLET-NOT_FOUND_INDEX";
        public const string WALLET_ALREADY_FINALIZED = "TIDE-ORK-WALLET-ALREADY_FINALIZED";

        // Customer
        public const string CUSTOMER_NOT_FOUND = "TIDE-ORK-CUSTOMER-NOT_FOUND";
    }
}
