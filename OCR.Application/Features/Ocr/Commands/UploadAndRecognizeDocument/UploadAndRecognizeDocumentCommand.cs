using MediatR;
using Microsoft.AspNetCore.Http;
using OCR.Application.DTOs;

namespace OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument
{
    public record UploadAndRecognizeDocumentCommand
    (
        IFormFile File,
        string FileName,
        string? FileDescription
    ) : IRequest<UploadAndRecognizeDocumentResult>;

    public record UploadAndRecognizeDocumentResult(
        bool RequiresConfirmation,
        Guid RecognizedId,
        RecognizedTextResultDto RecognizeData,
        IEnumerable<SimilarPatientDto>? SimilarPatients = null);
    
}
