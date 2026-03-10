using OCR.Domain.Entities;

namespace OCR.Domain.Interfaces
{
    public interface IRecognizeRepository
    {
        Task<Document?> GetByIdAsync(Guid id);
        Task<Recognize> GetByIdTextAsync(Guid id); 
        Task<List<Recognize>> GetAllAsync();
        Task SaveRecognizedTextAsync(Recognize text);
        Task<Recognize> DeleteAsync(Guid id);
        Task<Recognize> UpdateAsync(Guid id, Recognize textDomainModel);
    }
}
