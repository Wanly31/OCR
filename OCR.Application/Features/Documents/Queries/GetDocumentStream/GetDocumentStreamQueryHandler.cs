using MediatR;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;

namespace OCR.Application.Features.Documents.Queries.GetDocumentStream
{
    public class GetDocumentStreamQueryHandler : IRequestHandler<GetDocumentStreamQuery, DocumentStreamResult>
    {
        private readonly IDocumentRepository _documentRepository;

        public GetDocumentStreamQueryHandler(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<DocumentStreamResult> Handle(GetDocumentStreamQuery request, CancellationToken cancellationToken)
        {
            var document = await _documentRepository.GetByIdAsync(request.Id);

            if (document == null)
                throw new NotFoundException("Document not found, id: ", request.Id);

            if (!File.Exists(document.FilePath))
                throw new NotFoundException("File not found on disk, id: ", request.Id);

            var contentType = GetContentType(document.FileExtension);
            var fileStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

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
