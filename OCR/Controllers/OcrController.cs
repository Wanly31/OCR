using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using OCR.Models.Domain;
using OCR.Models.DTO;
using OCR.Repositories;
using OCR.Services;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Document = OCR.Models.Domain.Document;

namespace OCR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {

        private readonly RecognizeTextService _recognizeTextService;

        public OcrController(IDocumentRepository documentRepository, IRecognizeRepository recognizeRepository, IRecognizeTextRepository recognizeTextRepository, RecognizeTextService recognizeTextService)
        {
            DocumentRepository = documentRepository;
            RecognizeRepository = recognizeRepository;
            RecognizeTextRepository = recognizeTextRepository;
            _recognizeTextService = recognizeTextService;

        }

        public IDocumentRepository DocumentRepository { get; }
        public IRecognizeRepository RecognizeRepository { get; }
        public IRecognizeTextRepository RecognizeTextRepository { get; }

        [HttpPost("UploadAndRecognize")]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadRequestDto requestDto, [FromServices] AzureOcrService ocrService)
        {
            ValidateFileUpload(requestDto);

            if (ModelState.IsValid)
            {
                var documentDomainModel = new Document
                {
                    File = requestDto.File,
                    FileExtension = Path.GetExtension(requestDto.File.FileName),
                    FileSizeInBytes = requestDto.File.Length,
                    FileDescription = requestDto.FileDescription,
                    FileName = requestDto.FileName
                };

                await DocumentRepository.Upload(documentDomainModel);

                //logs

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", $"{documentDomainModel.FileName}{documentDomainModel.FileExtension}");

                //try catch and logs?
                string recognizedText = await ocrService.ReadDocumentAsync(filePath);
                var recognized = new Recognize
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentDomainModel.Id,
                    Text = recognizedText
                };

                await RecognizeRepository.SaveRecognizedTextAsync(recognized);

                if (string.IsNullOrWhiteSpace(recognized.Text))
                {
                    return BadRequest("Text content is empty");
                }

                var recogText = await _recognizeTextService.RecognizeText(recognized.Text);

                var recognizeTextDomain = new RecognizeText
                {
                    Id = Guid.NewGuid(),
                    FirstName = recogText.FirstName,
                    LastName = recogText.LastName,
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    DateDocument = recogText.DateDocument,
                    CreatedAt = DateTime.UtcNow,
                    RecognizedTextId = recognized.Id
                };

                await RecognizeTextRepository.SaveRecognizedTextAsync(recognizeTextDomain);

                return Ok(new
                {
                    Id = recognized.Id,
                    ExtractedDate = recogText.DateDocument?.ToString("yyyy-MM-dd"),
                    FirstName = recogText.FirstName,
                    LastName = recogText.LastName,
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    ProcessedAt = DateTime.UtcNow
                });
            }


            return BadRequest();

        }

        private void ValidateFileUpload(DocumentUploadRequestDto request)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".pdf", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName)))
            {
                ModelState.AddModelError("File", "Only .jpg, .jpeg, .pdf");
            }
            if (request.File.Length > 10385760)
            {
                ModelState.AddModelError("File", "File size cannot exceed 10MB");
            }
        }
    }
}
