using Microsoft.AspNetCore.Mvc;
using Ork.Services.Payment;


namespace Ork.Controllers.Payer
{
    [Route("Payer/[controller]")]
    public class CustomerAdminController : Controller
    {
        private IReportService _reportService;

        public CustomerAdminController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("GetCustomerMetrics")]
        public async Task<IActionResult> GetCustomerMetrics()
        {
            var metrics = await _reportService.GetCustomerMetricsAsync();
            return Ok(metrics);
        }

    }  
}