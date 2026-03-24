using MediatR;
using OCR.Application.DTOs;

namespace OCR.Application.Features.Ocr.Commands.ConfirmPatient
{
    public record ConfirmPatientCommand
    (
        Guid? ExistingPatientId,
        string FirstName,
        string? LastName,
        DateOnly? BirthDate,
        Guid RecognizedId,
        RecognizedDataDto RecognizedData
    ) : IRequest<ConfirmPatientResult>;

    public record ConfirmPatientResult
    (
        Guid Id,
        string FirstName,
        string LastName,
        DateOnly? BirthDate
    );

}
