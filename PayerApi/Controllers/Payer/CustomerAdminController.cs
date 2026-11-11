using Microsoft.AspNetCore.Mvc;
using Ork.Services.Payment;


namespace Ork.Controllers.Payer
{
    [Route("Payer/[controller]")]
    public class CustomerAdminController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ICustomerAdminService _customerAdminService;

        public CustomerAdminController(
            IReportService reportService,
            ICustomerAdminService customerAdminService
            )
        {
            _reportService = reportService;
            _customerAdminService = customerAdminService;
        }

        [HttpGet("GetCustomerMetrics")]
        public async Task<IActionResult> GetCustomerMetrics()
        {
            var metrics = await _reportService.GetCustomerMetricsAsync();
            return Ok(metrics);
        }

        [HttpGet("Data")]
        public async Task<IActionResult> GetCustomerData(
            [FromQuery] string? search = "",
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 25
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid query parameters"
                });
                }
                
                search ??= "";

                if (search.Length > 100)
            {
                return BadRequest(new
                {
                    error = "Invalid Search"
                });
            }

            if (page < 1 || limit < 1 || limit > 25)
            {
                return BadRequest(new
                {
                    error = "Page must be a positive integer and the limit must be between 1 and 25"
                });
            }

            if (status != null &&
                status != "active" &&
                status != "expiring" &&
                status != "inactive")

            {
                return BadRequest(new
                {
                    error = "Invalid status"
                });
            }

            if (((long)page - 1) * limit > int.MaxValue)
            {
                return BadRequest(new
                {
                    error = "Requested page is too large"
                });
            }

            var result = await _customerAdminService.GetCustomerDataAsync(
                search,
                status,
                page,
                limit
            );

            return Ok(result);
        }
        
        [HttpGet("{id}/Details")]
        public async Task<IActionResult> GetCustomerDetails(
            [FromRoute] string id)
        {
            var details = await _customerAdminService.GetCustomerDetailsAsync(id);

            return Ok(details);
        }

        [HttpGet("{id}/Wallets")]
        public async Task<IActionResult> GetCustomerWallets(
            [FromRoute] string id,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 25
            )
        {
            var wallets = await _customerAdminService.GetCustomerWalletsAsync(id, page, limit);

            return Ok(wallets);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> SoftDeleteCustomer(
            [FromRoute] string id)
        {
            await _customerAdminService.SoftDeleteCustomerAsync(id);

            return NoContent();
        }

    }   
}