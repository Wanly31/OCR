using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;

namespace OCR.Services
{
    public class AzureOcrService
    {
        private readonly string endpoint;
        private readonly string key;

        public AzureOcrService(IConfiguration configuration)
        {
            endpoint = configuration["AzureComputerVision:Endpoint"]!;
            key = configuration["AzureComputerVision:Key"]!;
        }

        private ComputerVisionClient Authenticate()
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
        }

        public async Task<string> ReadDocumentAsync(string filePath)
        {
            var client = Authenticate();

            using var stream = File.OpenRead(filePath);
            var textHeaders = await client.ReadInStreamAsync(stream);

            string operationLocation = textHeaders.OperationLocation;
            string operationId = operationLocation.Split('/').Last();

            ReadOperationResult results;
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
                await Task.Delay(1000);
            }
            while (results.Status == OperationStatusCodes.Running);

            var sb = new StringBuilder();
            foreach (var page in results.AnalyzeResult.ReadResults)
                foreach (var line in page.Lines)
                    sb.AppendLine(line.Text);

            return sb.ToString();
        }
    }
}

