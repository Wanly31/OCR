using OCR.Models.Domain;
using OCR.Models.DTO;

namespace OCR.Repositories
{
    public interface IRecognizeTextRepository
    {
        Task<Recognize> GetByIdAsync(Guid id);
        Task<RecognizeText> GetByIdTextAsync(Guid id);
        Task<List<RecognizeText>> GetAllAsync();

        Task SaveRecognizedTextAsync(RecognizeText text);
        Task<RecognizeText> DeleteAsync(Guid id);
        Task<RecognizeText> UpdateAsync(Guid id, RecognizeText textDomainModel);
    }
}
