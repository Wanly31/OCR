using MediatR;
using OCR.Application.DTOs;

namespace OCR.Application.Features.Ocr.Quaries.GetRecognizeResultById
{
    public record GetRecognizeResultByIdQuery(Guid id) : IRequest<RecognizeTextResult>;

    public record RecognizeTextResult
(
    Guid Id,
    string FirstName,
    string LastName,
    string Medicine,
    string Treatment,
    DateOnly? DateDocument,
    DateTime? CreatedAt
);
}
