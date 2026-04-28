using MediatR;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;

namespace OCR.Application.Features.Documents.Queries.GetDocumentStream
{
    public class GetDocumentStreamQueryHandler : IRequestHandler<GetDocumentStreamQuery, DocumentStreamResult>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;
        
        public GetDocumentStreamQueryHandler(IDocumentRepository documentRepository, IFileStorage fileStorage)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
        }

        public async Task<DocumentStreamResult> Handle(GetDocumentStreamQuery request, CancellationToken cancellationToken)
        {
            var document = await _documentRepository.GetByIdAsync(request.Id);

            if (document == null)
            {
                throw new NotFoundException("Document not found, id: ", request.Id);
            }

            // Завантажуємо через IFileStorage — підтримує і Azure Blob, і локальний диск
            var fileStream = await _fileStorage.GetFileStreamAsync(document.FilePath);
            var contentType = GetContentType(document.FileExtension);

            return new DocumentStreamResult(
                FileStream: fileStream,
                ContentType: contentType,
                FileName: document.FileName
            );
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf"  => "application/pdf",
                ".png"  => "image/png",
                ".jpg"  => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif"  => "image/gif",
                ".webp" => "image/webp",
                ".bmp"  => "image/bmp",
                ".tiff" => "image/tiff",
                ".tif"  => "image/tiff",
                _       => "application/octet-stream"
            };
        }
    }
}
