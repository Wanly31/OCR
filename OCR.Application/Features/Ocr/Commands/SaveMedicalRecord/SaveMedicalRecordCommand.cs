using MediatR;
using OCR.Application.DTOs;

namespace OCR.Application.Features.Ocr.Commands.SaveMedicalRecord
{
    public record SaveMedicalRecordCommand
    (
        Guid? ExistingPatientId,
        string FirstName,
        string? LastName,
        DateOnly? BirthDate,
        Guid RecognizedId,
        RecognizedDataDto RecognizedData
    ) : IRequest<SaveMedicalRecordResult>;

    public record SaveMedicalRecordResult
    (
        Guid Id,
        string FirstName,
        string? LastName,
        DateOnly? BirthDate
    );

}
