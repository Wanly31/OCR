using OCR.Models.Domain;

namespace OCR.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> Upload(Document document);
        Task<List<Document>> GetAllAsync();
        Task<Document> GetByIdAsync(Guid id);
    }
}
