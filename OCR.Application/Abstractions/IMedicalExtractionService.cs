using OCR.Application.DTOs;

namespace OCR.Application.Abstractions
{
    public interface IMedicalExtractionService
    {
        Task<RecognizedTextResultDto> ExtractMedicalDataAsync(string text);
    }
}
