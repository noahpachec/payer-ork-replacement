using Microsoft.AspNetCore.Mvc;
using Ork.Services.Payment;

namespace Ork.Controllers.Payer
{
	[Route("Payer/[controller]")]
	public class SubscriptionController : Controller
	{
		private IPayerService _payerService;
		private IWalletService _walletService;

		public SubscriptionController(IPayerService payerService, IWalletService walletService)
		{
			_payerService = payerService;
			_walletService = walletService;
		}

		// Exemplar endpoint: proves the EF connection by running PayerService's
		// Subscriptions + SubscriptionUsers + Wallets query for a customer.
		[HttpGet("GetSubscriptionDetails")]
		public IActionResult GetSubscriptionDetails([FromQuery] string customerId)
		{
			return Content(_payerService.GetSubscriptionDetailsByCustomer(customerId), "application/json");
		}

		// Exemplar endpoint: proves the EF connection by running WalletService's
		// Wallets lookup by Id (obfGVVK / Y'').
		[HttpGet("Test")]
		public IActionResult Test([FromQuery] string id)
		{
			var wallet = _walletService.GetWalletById(id);
			if (wallet == null) return NotFound();
			return Ok(wallet);
		}

		[HttpGet("GetAllSubscriptions")]
		public IActionResult GetAllSubscriptions()
		{
			return Ok(_payerService.GetAllSubscriptions());
		}

		[HttpGet("GetAllWallets")]
		public IActionResult GetAllWallets()
		{
			return Ok(_walletService.GetAllWallets());
		}

	}
}
