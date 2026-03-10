using OCR.Domain.Entities;

namespace OCR.Domain.Interfaces 
{
    public interface IDocumentRepository
    {
        Task<Document> Upload(Document document);
        Task<List<Document>> GetAllAsync();
        Task<Document> GetByIdAsync(Guid id);
        Task<Document> DeleteAsync(Guid id);
        Task<Document> UpdateAsync(Guid id, Document documentDomainModel);
    }
}
