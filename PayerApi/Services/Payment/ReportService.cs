using Microsoft.EntityFrameworkCore;
using Ork.Helpers;
using Ork.Models.Payer;

namespace Ork.Services.Payment;

public class ReportService : IReportService
{
    private readonly PayerDataContext _context;

    public ReportService(PayerDataContext context)
    {
        _context = context;
    }

    public async Task<CustomerMetricsResponse> GetCustomerMetricsAsync()
    {
        var results = await _context.Database
            .SqlQuery<CustomerMetricsResponse>($"""
                WITH customer_status AS (

                    SELECT
                    c."StripeCustomerId" AS "CustomerId",



                        CASE
                            WHEN NOT EXISTS (
                                SELECT 1
                                FROM "Subscriptions" AS s
                                JOIN "Wallets" AS w
                                    ON w."SubscriptionId" = s."Id"
                                WHERE s."CustomerId" = c."StripeCustomerId"
                                    AND w."Status" = 'Active'
                                    AND w."Expiry" > EXTRACT(EPOCH FROM NOW())

                            ) THEN 'inactive'

                            WHEN EXISTS (
                                SELECT 1
                                FROM "Subscriptions" AS s
                                JOIN "Wallets" AS w
                                    ON w."SubscriptionId" = s."Id"
                                WHERE s."CustomerId" = c."StripeCustomerId"
                                    AND w."Status" = 'Active'
                                    AND w."Expiry" > EXTRACT(EPOCH FROM NOW())
                                    AND w."Expiry" <= EXTRACT(EPOCH FROM NOW()) + 864000

                            ) THEN 'expiring'

                            ELSE 'active'
                        END AS "Status"

                FROM "Customers" AS c
                WHERE c."DeletedAt" IS NULL
            )

            SELECT
                COUNT(*)::integer AS "Total",
                COUNT(*) FILTER (WHERE "Status" = 'active')::integer AS "Active",
                COUNT(*) FILTER (WHERE "Status" = 'expiring')::integer AS "Expiring",
                COUNT(*) FILTER (WHERE "Status" = 'inactive')::integer AS "Inactive"
            FROM customer_status;
            """).ToListAsync();

        return results[0];
    }
}

