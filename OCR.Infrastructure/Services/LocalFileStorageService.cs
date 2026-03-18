using Microsoft.AspNetCore.Http;
using OCR.Application.Abstractions;

namespace OCR.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorage
    {
        public Task DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string fileName)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Documents");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return filePath;
        }
    }
}
