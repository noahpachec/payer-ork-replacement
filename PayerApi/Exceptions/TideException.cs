//
// Copied from Cryptide/Cryptide/Exceptions/TideException.cs (TideException base)
// and Ork/Ork/Exceptions/TideException.cs (Network hierarchy) - only the types
// thrown by the copied payer/wallet DB services are kept.
//

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ork.Errors;

namespace Ork.Exceptions
{
    public abstract class TideException : Exception
    {
        public string Code { get; }
        public string DisplayMessage { get; }
        public string? MessageKey { get; }
        public IReadOnlyDictionary<string, string>? MessageParams { get; }
        public string SourcePath { get; }
        public int SourceLine { get; }
        public int HttpStatus { get; }

        protected TideException(
            string code,
            string displayMessage,
            int httpStatus = 500,
            string? messageKey = null,
            IReadOnlyDictionary<string, string>? messageParams = null,
            Exception? innerException = null,
            [CallerFilePath] string sourcePath = "",
            [CallerLineNumber] int sourceLine = 0)
            : base(displayMessage, innerException)
        {
            Code = code;
            DisplayMessage = displayMessage;
            HttpStatus = httpStatus;
            MessageKey = messageKey;
            MessageParams = messageParams;
            SourcePath = sourcePath;
            SourceLine = sourceLine;
        }
    }

    namespace Network
    {

        public class WalletException : TideException
        {

            public WalletException(
                string code,
                string displayMessage,
                int httpStatus,
                [CallerFilePath] string sourcePath = "",
                [CallerLineNumber] int sourceLine = 0)
                : base(code, displayMessage, httpStatus,
                       sourcePath: sourcePath, sourceLine: sourceLine)
            { }

        }

        public class CustomerException : TideException
        {

            public CustomerException(
                string code,
                string displayMessage,
                int httpStatus,
                [CallerFilePath] string sourcePath = "",
                [CallerLineNumber] int sourceLine = 0)
                : base(code, displayMessage, httpStatus,
                       sourcePath: sourcePath, sourceLine: sourceLine)
            { }

        }

        public class WalletNotFoundForIndex : WalletException
        {
            public string Index { get; }

            public WalletNotFoundForIndex(
                string index,
                [CallerFilePath] string sourcePath = "",
                [CallerLineNumber] int sourceLine = 0)
                : base(TideErrorCodes.WALLET_NOT_FOUND_INDEX, $"Wallet not found for index: {index}", 403,
                       sourcePath: sourcePath, sourceLine: sourceLine)
            {
                Index = index;
            }
        }

        public class WalletAlreadyFinalized : WalletException
        {
            public string WalletIdentity { get; }

            public WalletAlreadyFinalized(
                string walletIdentity,
                [CallerFilePath] string sourcePath = "",
                [CallerLineNumber] int sourceLine = 0)
                : base(TideErrorCodes.WALLET_ALREADY_FINALIZED, $"Wallet with identity '{walletIdentity}' has already been finalised and cannot be changed/", 403,
                       sourcePath: sourcePath, sourceLine: sourceLine)
            {
                WalletIdentity = walletIdentity;
            }
        }

        public class CustomerNotFoundException : CustomerException
        {
            public string CustomerId { get; }

            public CustomerNotFoundException(
                string customerId,
                [CallerFilePath] string sourcePath = "",
                [CallerLineNumber] int sourceLine = 0)
                : base(TideErrorCodes.CUSTOMER_NOT_FOUND, $"Customer ID '{customerId}' could not be found in the database", 403,
                       sourcePath: sourcePath, sourceLine: sourceLine)
            {
                CustomerId = customerId;
            }
        }
    }
}
