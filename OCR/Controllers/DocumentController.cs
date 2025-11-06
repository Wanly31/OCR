using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("{id}/recognize")]
        public async Task<IActionResult> Recognize(Guid id, [FromServices] AzureOcrService ocrService)
        {
            // 1️⃣ Знайти документ у базі
            var document = await DocumentRepository.GetByIdAsync(id);
            if (document == null)
                return NotFound("Document not found.");

            // 2️⃣ Побудувати абсолютний шлях до файлу у папці Documents
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", $"{document.FileName}{document.FileExtension}");

            // 3️⃣ Перевірити, чи файл існує
            if (!System.IO.File.Exists(filePath))
                return NotFound($"File not found at path: {filePath}");

            // 4️⃣ Викликати Azure OCR сервіс для розпізнавання
            string recognizedText = await ocrService.ReadDocumentAsync(filePath);

            // 5️⃣ Зберегти результат у базі
            var recognized = new RecognizedText
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Text = recognizedText
            };

            await DocumentRepository.SaveRecognizedTextAsync(recognized);

            // 6️⃣ Повернути результат клієнту
            return Ok(new
            {
                DocumentId = document.Id,
                Text = recognizedText
            });
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
