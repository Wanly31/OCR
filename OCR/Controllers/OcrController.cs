using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using OCR.Domain.Entities;
using OCR.Application.DTOs;
using OCR.Domain.Interfaces;
using OCR.Infrastructure.Services;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Document = OCR.Domain.Entities.Document;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {

        private readonly RecognizeTextService _recognizeTextService;
        private readonly AzureOcrService ocrService;
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger _logger;

        public OcrController(
            IDocumentRepository documentRepository, 
            IRecognizeRepository recognizeRepository, 
            IRecognizeTextRepository recognizeTextRepository, 
            RecognizeTextService recognizeTextService,
            AzureOcrService ocrService,
            IPatientRepository patientRepository,
            ILogger<OcrController> logger)
        {
            DocumentRepository = documentRepository;
            RecognizeRepository = recognizeRepository;
            RecognizeTextRepository = recognizeTextRepository;
            _recognizeTextService = recognizeTextService;
            this.ocrService = ocrService;
            _patientRepository = patientRepository;
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
                RecognizedTextResultDto recogText;
                try
                {
                    recogText = await _recognizeTextService.RecognizeText(recognized.Text);
                }
                catch
                {
                    _logger.LogError("Error extracting structured data from recognized text");
                    return BadRequest("Error extracting structured data from recognized text");
                }

                // Search for similar patients
                var similarPatients = await _patientRepository.SearchSimilarAsync(
                    recogText.FirstName ?? "",
                    recogText.LastName,
                    recogText.BirthDate
                );

                if (similarPatients.Any())
                {
                    // Found similar patients - return them for user confirmation
                    _logger.LogInformation($"Found {similarPatients.Count} similar patients");
                    return Ok(new
                    {
                        requiresConfirmation = true,
                        recognizedId = recognized.Id,
                        recognizedData = recogText,
                        similarPatients = similarPatients.Select(p => new
                        {
                            p.Id,
                            p.FirstName,
                            p.LastName,
                            p.BirthDate,
                            recordCount = p.MedicalRecords?.Count ?? 0
                        })
                    });
                }

                // No similar patients - create new patient automatically
                var newPatient = await _patientRepository.CreateAsync(new Patient
                {
                    FirstName = recogText.FirstName ?? "Unknown",
                    LastName = recogText.LastName,
                    BirthDate = recogText.BirthDate
                });

                var recognizeTextDomain = new RecognizeText
                {
                    Id = Guid.NewGuid(),
                    PatientId = newPatient.Id,
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

        [HttpPost("ConfirmPatient")]
        public async Task<IActionResult> ConfirmPatient([FromBody] PatientConfirmationDto request)
        {
            _logger.LogInformation($"Patient confirmation request received. ExistingPatientId: {request.ExistingPatientId}");

            Patient patient;

            if (request.ExistingPatientId.HasValue)
            {
                // Use existing patient
                patient = await _patientRepository.GetByIdAsync(request.ExistingPatientId.Value);
                if (patient == null)
                {
                    return NotFound(new { message = $"Patient with ID {request.ExistingPatientId.Value} not found" });
                }
                _logger.LogInformation($"Using existing patient: {patient.Id}");
            }
            else
            {
                // Create new patient
                patient = await _patientRepository.CreateAsync(new Patient
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate
                });
                _logger.LogInformation($"Created new patient: {patient.Id}");
            }

            // Create medical record linked to patient
            var recognizeText = new RecognizeText
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                Examination = request.RecognizedData.Examination,
                Medicine = request.RecognizedData.Medicine,
                Treatment = request.RecognizedData.Treatment,
                ContraindicatedMedicine = request.RecognizedData.ContraindicatedMedicine,
                ContraindicatedReason = request.RecognizedData.ContraindicatedReason,
                DateDocument = request.RecognizedData.DateDocument,
                RecognizedTextId = request.RecognizedId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await RecognizeTextRepository.SaveRecognizedTextAsync(recognizeText);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving recognized text to repository: {ex.Message}");
                return BadRequest("Error saving recognized text");
            }

            return Ok(new
            {
                patient = new
                {
                    patient.Id,
                    patient.FirstName,
                    patient.LastName,
                    patient.BirthDate
                },
                record = recognizeText
            });
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
