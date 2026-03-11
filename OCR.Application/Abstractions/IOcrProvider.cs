
namespace OCR.Application.Abstractions
{
    public interface IOcrProvider
    {
        Task<string> RecognizeTextFromFileAsync(string filePath);
    }
}
