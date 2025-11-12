using OCR.Models.Domain;

namespace OCR.Repositories
{
    public interface ITextRepository
    {
        Task<Document?> GetByIdAsync(Guid id);
        Task SaveRecognizedTextAsync(RecognizedText text);
    }
}
