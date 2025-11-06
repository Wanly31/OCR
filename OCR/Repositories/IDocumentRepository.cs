using OCR.Models.Domain;

namespace OCR.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> Upload(Document document);
        Task<Document?> GetByIdAsync(Guid id);
        Task SaveRecognizedTextAsync(RecognizedText text);

    }
}
