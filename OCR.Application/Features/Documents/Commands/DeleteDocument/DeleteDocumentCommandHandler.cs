using MediatR;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;


namespace OCR.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, DeleteDocumentResult>
    {
        private readonly IDocumentRepository _documentRepository;
        
        public DeleteDocumentCommandHandler(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        
        public async Task<DeleteDocumentResult> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var dokumentModel = await _documentRepository.DeleteAsync(request.id);

            if (dokumentModel == null)
            {
                throw new NotFoundException("Document not found. Id: ", request.id);
            }

            return new DeleteDocumentResult(
                DeleteSuccessful: true);
        }
    }
}

