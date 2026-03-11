
using Microsoft.AspNetCore.Http;

namespace OCR.Application.Abstractions
{
    public interface IFileStorage
    {
        Task<string> SaveFileAsync(IFormFile file, string fileName);
        Task DeleteFileAsync(string filePath);
     }
}
