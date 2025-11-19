using OCR.Models.Domain;

namespace OCR.Repositories
{
    public interface IRecognizeRepository
    {
        Task<Document?> GetByIdAsync(Guid id);
        Task<Recognize> GetByIdTextAsync(Guid id); 
        Task<List<Recognize>> GetAllAsync();
        Task SaveRecognizedTextAsync(Recognize text);
        Task<Recognize> DeleteAsync(Guid id);
    }
}
