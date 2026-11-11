using Ork.Models.Payer;

namespace Ork.Services.Payment;

public interface IReportService
{
    Task<CustomerMetricsResponse> GetCustomerMetricsAsync();

    Task<List<MonthlySubscriptionReportResponse>> GetMonthlySubscriptionReportAsync();
}

