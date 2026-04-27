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


        public async Task<string> SaveFileAsync(IFormFile file, string fileName)
        {

            var client = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = client.GetBlobContainerClient(_containerName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);
            
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        public Task DeleteFileAsync(string filePath)
        {

            //TODO: Delete file
            throw new NotImplementedException();
        }
    }
}