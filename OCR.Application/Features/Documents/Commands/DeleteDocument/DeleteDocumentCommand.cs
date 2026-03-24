

using MediatR;

namespace OCR.Application.Features.Documents.Commands.DeleteDocument
{
    public record DeleteDocumentCommand(Guid id) : IRequest<DeleteDocumentResult>;

    public record DeleteDocumentResult(
        bool DeleteSuccessful);
}
