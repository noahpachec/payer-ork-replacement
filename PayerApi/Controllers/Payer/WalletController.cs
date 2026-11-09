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
	public class WalletController : Controller
	{
		private IPayerService _payerService;
		private IWalletService _walletService;

		public WalletController(IPayerService payerService, IWalletService walletService)
		{
			_payerService = payerService;
			_walletService = walletService;
		}

		[HttpGet("GetAllWallets")]
		public IActionResult GetAllWallets()
		{
			return Ok(_walletService.GetAllWallets());
		}

	}
}
