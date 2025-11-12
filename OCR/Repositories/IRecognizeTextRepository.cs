using OCR.Models.Domain;

namespace OCR.Repositories
{
    public interface IRecognizeTextRepository
    {
        Task<Recognize> GetByIdAsync(Guid id);
        Task<RecognizeText> GetByIdTextAsync(Guid id);
        Task<List<RecognizeText>> GetAllAsync();
    }
}
