using Microsoft.AspNetCore.Http;
using OCR.Application.Abstractions;

namespace OCR.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorage
    {
        public Task DeleteFileAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFileAsync(IFormFile file, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
