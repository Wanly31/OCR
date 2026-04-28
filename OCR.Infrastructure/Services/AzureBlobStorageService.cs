using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OCR.Application.Abstractions;

namespace OCR.Infrastructure.Services
{
    internal class AzureBlobStorageService : IFileStorage
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureBlobStorage:ConnectionString"]!;
            _containerName = configuration["AzureBlobStorage:ContainerName"]!;
        }

        private static string ExtractBlobName(string filePath)
        {
            if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(filePath);
                return uri.AbsolutePath.Split('/').Last();
            }
            return filePath;
        }

        private BlobClient GetBlobClient(string blobName)
        {
            var client = new BlobServiceClient(_connectionString);
            var containerClient = client.GetBlobContainerClient(_containerName);
            return containerClient.GetBlobClient(blobName);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string fileName)
        {
            var blobClient = GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            
            var options = new Azure.Storage.Blobs.Models.BlobUploadOptions
            {
                HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders 
                { 
                    ContentType = file.ContentType 
                }
            };
            
            await blobClient.UploadAsync(stream, options);

            return fileName;
        }

        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            if (Path.IsPathRooted(filePath) || filePath.Contains('\\'))
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Local file not found: {filePath}", filePath);

                return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            }

            var blobClient = GetBlobClient(ExtractBlobName(filePath));
            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }

        public Task<string> GetFileUrlAsync(string filePath)
        {
            if (Path.IsPathRooted(filePath) || filePath.Contains('\\'))
                return Task.FromResult(filePath);

            var blobName = ExtractBlobName(filePath);
            var blobClient = GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                ContentDisposition = "inline"
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return Task.FromResult(sasUri.ToString());
        }

        public Task DeleteFileAsync(string filePath)
        {

            if (Path.IsPathRooted(filePath) || filePath.Contains('\\'))
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return Task.CompletedTask;
            }

            var blobClient = GetBlobClient(ExtractBlobName(filePath));
            blobClient.DeleteIfExists();
            return Task.CompletedTask;
        }
    }
}
