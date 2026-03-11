using OCR.Domain.Entities;

namespace OCR.Application.Abstractions
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
