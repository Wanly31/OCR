using MediatR;

namespace OCR.Application.Features.Documents.Queries.GetDocumentStream
{
    public record GetDocumentStreamQuery(Guid Id) : IRequest<DocumentStreamResult> { }

    public record DocumentStreamResult(
        Stream FileStream,
        string ContentType,
        string FileName
    );
}
