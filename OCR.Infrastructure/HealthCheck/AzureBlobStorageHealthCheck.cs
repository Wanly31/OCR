using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OCR.Infrastructure.HealthCheck
{
    public class AzureBlobStorageHealthCheck(BlobContainerClient containerClient) : IHealthCheck
    {


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await containerClient.ExistsAsync(cancellationToken);

                if (exists.Value)
                {
                    return HealthCheckResult.Healthy("Azure Blob Storage is available");
                }

                return HealthCheckResult.Unhealthy("Blob container not exists");
            }
            catch(Exception ex)
            {
                return HealthCheckResult.Unhealthy("Blob storage is unvailable", ex);
            }
        }
    }
}
