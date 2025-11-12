using OCR.Models.Domain;

namespace OCR.Repositories
{
    public interface IRecognizeTextRepository
    {
        Task<RecognizedText> GetByIdAsync(Guid id);
    }
}
