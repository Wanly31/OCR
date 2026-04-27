using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Configuration;
using OCR.Application.Abstractions;
using System.Text;

namespace OCR.Infrastructure.Services
{
    public class AzureOcrService : IOcrProvider
    {
        private readonly string endpoint;
        private readonly string key;

        public AzureOcrService(IConfiguration configuration)
        {
            endpoint = configuration["AzureComputerVision:Endpoint"]!;
            key = configuration["AzureComputerVision:Key"]!;
        }

        private ImageAnalysisClient Authenticate()
        {
            ImageAnalysisClient client =  new ImageAnalysisClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));
            return client;
        }

        /// <summary>
        /// Extract text from a document using OCR
        /// </summary>
        /// <param name="filePath">Path to the document file</param>
        /// <returns>Returns recognized text from the image</returns>
        public async Task<string> RecognizeTextFromFileAsync(string uriString)
        {
            var client = Authenticate();
            
            ImageAnalysisResult result = await client.AnalyzeAsync(
                new Uri(uriString),
                VisualFeatures.Read);

            var sb = new StringBuilder();

            if (result.Read != null) {
                foreach (var block in result.Read.Blocks)
                {
                    foreach (var line in block.Lines)
                    {
                        sb.AppendLine(line.Text);
                    } 
                }
            }
            return sb.ToString();
        }

    }
}

