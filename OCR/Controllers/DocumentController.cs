using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCR.Models.Domain;
using OCR.Models.DTO;
using OCR.Repositories;

namespace OCR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {

        public DocumentController(IDocumentRepository documentRepository)
        {
            DocumentRepository = documentRepository;
        }

        public IDocumentRepository DocumentRepository { get; }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadRequestDto request)
        {
            ValidateFileUpload(request);

            if (ModelState.IsValid)
            {
                var documentDomainModel = new Document
                {
                    File = request.File,
                    FileExtension = Path.GetExtension(request.File.FileName),
                    FileSizeInBytes = request.File.Length,
                    FileDescription = request.FileDescription,
                    FileName = request.FileName
                };

                await DocumentRepository.Upload(documentDomainModel);

                return Ok(documentDomainModel);
            }

            return BadRequest(ModelState);
        }

        private void ValidateFileUpload(DocumentUploadRequestDto request)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".pdf", ".png" };

            if(allowedExtensions.Contains(Path.GetExtension(request.File.FileName)) == false)
            {
                ModelState.AddModelError("File", "Only .jpg, .jpeg, .pdf");
            }
            if(request.File.Length > 10385760)
            {
                ModelState.AddModelError("File", "File size cannot exceed 10MB");
            }
        }
    }
}
