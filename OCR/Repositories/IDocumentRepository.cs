using OCR.Models.Domain;

namespace OCR.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> Upload(Document document);
    }
}
