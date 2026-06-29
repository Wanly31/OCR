using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OCR.Infrastructure.HealthCheck
{
    public class AzureOcrHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
			try
			{
				return HealthCheckResult.Unhealthy("Azure OCR is umvailable");
			}
			catch (Exception ex)
			{

				return HealthCheckResult.Unhealthy("Azure OCR is unvailable");
			}
        }
    }
}
