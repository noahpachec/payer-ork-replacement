// THIS IS AN API --- SEPARATE FROM ENTITY FRAMEWORK


using Microsoft.AspNetCore.Mvc;
using Ork.Services.Payment;



// A namespace groups related classes and helps distinguish classes with the same name.
// This class belongs to "Ork.Controllers.Payer", so its full name is
// Ork.Controllers.Payer.SubscriptionController.
// The namespace does not define the API URL; the [Route(...)] attribute does.
namespace Ork.Controllers.Payer
{
	[Route("Payer/[controller]")]
	public class SubscriptionController : Controller
	{
		private IPayerService _payerService;
		private IWalletService _walletService;

		private IReportService _reportService;

		public SubscriptionController(IPayerService payerService, IWalletService walletService, IReportService reportService)
		{
			_payerService = payerService;
			_walletService = walletService;
			_reportService = reportService;
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
		// [HttpGet("Test")]
		// public IActionResult Test([FromQuery] string id)
		// {
		// 	var wallet = _walletService.GetWalletById(id);
		// 	if (wallet == null) return NotFound();
		// 	return Ok(wallet);
		// }

		[HttpGet("Subscriptions")]
		public IActionResult GetAllSubscriptions()
		{
			return Ok(_payerService.GetAllSubscriptions());
		}

		[HttpGet("MonthlySubscriptionsReport")]
		public async Task<IActionResult> GetMonthlySubscriptionsReport()
		{
			var report =
				await _reportService.GetMonthlySubscriptionReportAsync();

			return Ok(report);

		}


	}
}
