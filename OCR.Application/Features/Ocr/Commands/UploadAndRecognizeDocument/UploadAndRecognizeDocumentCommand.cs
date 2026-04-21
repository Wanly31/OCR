using MediatR;
using Microsoft.AspNetCore.Http;
using OCR.Application.DTOs;
using OCR.Domain.Enums;

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
        Guid DocumentId,
        RecognizedTextResultDto RecognizeData,
        RecordStatus RecordStatus,
        string FilePath,
        IEnumerable<SimilarPatientDto>? SimilarPatients = null);
    
}
