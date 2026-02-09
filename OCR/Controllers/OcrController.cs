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
        private readonly AzureOcrService ocrService;
        private readonly ILogger _logger;

        public OcrController(
            IDocumentRepository documentRepository, 
            IRecognizeRepository recognizeRepository, 
            IRecognizeTextRepository recognizeTextRepository, 
            RecognizeTextService recognizeTextService,
            AzureOcrService ocrService,
            ILogger<OcrController> logger)
        {
            DocumentRepository = documentRepository;
            RecognizeRepository = recognizeRepository;
            RecognizeTextRepository = recognizeTextRepository;
            _recognizeTextService = recognizeTextService;
            this.ocrService = ocrService;
            _logger = logger;

        }

        public IDocumentRepository DocumentRepository { get; }
        public IRecognizeRepository RecognizeRepository { get; }
        public IRecognizeTextRepository RecognizeTextRepository { get; }

        [HttpPost("UploadAndRecognize")]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadRequestDto requestDto)
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

                _logger.LogInformation("Starting file upload");

                try
                {
                    await DocumentRepository.Upload(documentDomainModel);
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file");
                    return BadRequest("Error uploading file");
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", $"{documentDomainModel.FileName}{documentDomainModel.FileExtension}");

                _logger.LogInformation("File uploaded successfully, starting OCR processing");
                
                string recognizedText;
                try
                {
                    recognizedText = await ocrService.ReadDocumentAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during OCR processing");
                    return BadRequest($"Error during OCR processing: {ex.Message}");
                }

                var recognized = new Recognize
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentDomainModel.Id,
                    Text = recognizedText
                };

                _logger.LogInformation("Saving recognized text to repository");
                try
                {
                    await RecognizeRepository.SaveRecognizedTextAsync(recognized);
                }
                catch 
                {
                    _logger.LogError("Error saving recognized text to repository");
                    return BadRequest("Error saving recognized text");
                }


                if (string.IsNullOrWhiteSpace(recognized.Text))
                {
                    _logger.LogWarning("Recognized text is empty");
                    return BadRequest("Text content is empty");
                }

                _logger.LogInformation("Extracting structured data from recognized text");
                RecognizeText recogText;
                try
                {
                    recogText = await _recognizeTextService.RecognizeText(recognized.Text);
                }
                catch
                {
                    _logger.LogError("Error extracting structured data from recognized text");
                    return BadRequest("Error extracting structured data from recognized text");
                }


                var recognizeTextDomain = new RecognizeText
                {
                    Id = Guid.NewGuid(),
                    FirstName = recogText.FirstName,
                    LastName = recogText.LastName,
                    BirthDate = recogText.BirthDate,
                    Examination = recogText.Examination,
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    ContraindicatedMedicine = recogText.ContraindicatedMedicine,
                    ContraindicatedReason = recogText.ContraindicatedReason,
                    DateDocument = recogText.DateDocument,
                    CreatedAt = DateTime.UtcNow,
                    RecognizedTextId = recognized.Id
                };

                _logger.LogInformation("Saving extracted structured data to repository");
                try
                {
                    await RecognizeTextRepository.SaveRecognizedTextAsync(recognizeTextDomain);

                }
                catch
                {
                    _logger.LogError("Error saving extracted structured data to repository");
                    return BadRequest("Error saving extracted structured data");
                }

                return Ok(new
                {
                    Id = recognized.Id,
                    FirstName = recogText.FirstName,
                    LastName = recogText.LastName,
                    BirthDate = recogText.BirthDate,
                    Examination = recogText.Examination,
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    ContraindicatedMedicine = recogText.ContraindicatedMedicine,
                    ContraindicatedReason = recogText.ContraindicatedReason,
                    DateDocument = recogText.DateDocument,
                    CreatedAt = DateTime.UtcNow,
                });
            }


            return BadRequest();

        }

        private void ValidateFileUpload(DocumentUploadRequestDto request)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".pdf", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName)))
            {
                ModelState.AddModelError("File", "Only .jpg, .jpeg, .pdf, .png");
            }
            if (request.File.Length > 10385760)
            {
                ModelState.AddModelError("File", "File size cannot exceed 10MB");
            }
        }
    }
}
