using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;
using OCR.Models.Domain;
using OCR.Models.DTO;
using OCR.Repositories;
using OCR.Services;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var DocumentsDomain = await DocumentRepository.GetAllAsync();
            if (DocumentsDomain == null || DocumentsDomain.Count == 0)
            {
                throw new Exception("No documents found");
            }

            var documentDto = new List<DocumentDto>();

            foreach (var documentDomain in DocumentsDomain)
            {
                documentDto.Add(new DocumentDto
                {
                    FileName = documentDomain.FileName,
                    FileDescription = documentDomain.FileDescription,
                    File = documentDomain.File,
                    FileSizeInBytes = documentDomain.FileSizeInBytes
                }
                );
            }

            return Ok(documentDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var document = await DocumentRepository.GetByIdAsync(id);
            
            if (document == null)
            {
                    return NotFound($"Document with id: {id} not found");
             
            }

            var documentDto = new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FileDescription = document.FileDescription,
                FileSizeInBytes = document.FileSizeInBytes
            };

            return Ok(documentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequestDto updateDocumentRequestDto)
        {
            var DocumentDomainModel = new Document
            {
                FileName = updateDocumentRequestDto.FileName,
                FileDescription = updateDocumentRequestDto.FileDescription
            };

            DocumentDomainModel = await DocumentRepository.UpdateAsync(id, DocumentDomainModel);

            if (DocumentDomainModel == null)
            {
                return NotFound($"Document with id: {id} not found");
            }

            var documentDto = new DocumentDto
            {
                FileName = DocumentDomainModel.FileName,
                FileDescription = DocumentDomainModel.FileDescription,
                FileSizeInBytes = DocumentDomainModel.FileSizeInBytes
            };

            return Ok(documentDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var dokumentModel = await DocumentRepository.DeleteAsync(id);

            if (dokumentModel == null)
            {
                throw new Exception($"Document with id: {id} not found");
            }

            return Ok( "Document deleted successfully");
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
