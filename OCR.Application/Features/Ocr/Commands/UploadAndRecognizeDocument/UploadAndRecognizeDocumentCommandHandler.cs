using MediatR;
using Microsoft.Extensions.Logging;
using OCR.Application.Abstractions;
using OCR.Application.DTOs;
using OCR.Domain.Entities;

namespace OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument;

public class UploadAndRecognizeDocumentCommandHandler
    : IRequestHandler<UploadAndRecognizeDocumentCommand, UploadAndRecognizeDocumentResult>
{
    private readonly IDocumentRepository _documentRepo;
    private readonly IRecognizeRepository _recognizeRepo;
    private readonly IRecognizeTextRepository _recognizeTextRepo;
    private readonly IOcrProvider _ocrProvider;
    private readonly IMedicalExtractionService _extractionService;
    private readonly IPatientRepository _patientRepo;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<UploadAndRecognizeDocumentCommandHandler> _logger;

    public UploadAndRecognizeDocumentCommandHandler(
        IDocumentRepository documentRepo,
        IRecognizeRepository recognizeRepo,
        IRecognizeTextRepository recognizeTextRepo,
        IOcrProvider ocrProvider,
        IMedicalExtractionService extractionService,
        IPatientRepository patientRepo,
        IFileStorage fileStorage,
        ILogger<UploadAndRecognizeDocumentCommandHandler> logger)
    {
        _documentRepo = documentRepo;
        _recognizeRepo = recognizeRepo;
        _recognizeTextRepo = recognizeTextRepo;
        _ocrProvider = ocrProvider;
        _extractionService = extractionService;
        _patientRepo = patientRepo;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<UploadAndRecognizeDocumentResult> Handle(
        UploadAndRecognizeDocumentCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Starting file upload and OCR processing");

        // 1. Зберігаємо файл через IFileStorage
        var extension = Path.GetExtension(request.File.FileName);
        string newName = Guid.NewGuid().ToString() + extension;
        string savedFilePath = await _fileStorage.SaveFileAsync(request.File, newName);

        // 2. Створюємо доменну сутність Document
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            FileDescription = request.FileDescription,
            FileExtension = extension,
            FileSizeInBytes = request.File.Length,
            FilePath = savedFilePath // Шлях куди IFileStorage зберіг файл
        };

        await _documentRepo.Upload(document);

        // 3. OCR розпознавання тексту з файлу
        string recognizedText = await _ocrProvider.RecognizeTextFromFileAsync(savedFilePath);

        if (string.IsNullOrWhiteSpace(recognizedText))
        {
            _logger.LogWarning("Recognized text is empty");
            throw new ApplicationException("Text content recognized from file is empty.");
        }

        // 4. Збереження розпізнаного сирого тексту
        var recognizeResult = new Recognize
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Text = recognizedText
        };
        await _recognizeRepo.SaveRecognizedTextAsync(recognizeResult);

        // 5. Витягування медичних даних із сирого тексту (Entities & NLP)
        _logger.LogInformation("Extracting structured data from recognized text");
        var extractedData = await _extractionService.ExtractMedicalDataAsync(recognizedText);

        // 6. Пошук подібних пацієнтів
        var similarPatients = await _patientRepo.SearchSimilarAsync(
            extractedData.FirstName ?? "",
            extractedData.LastName,
            extractedData.BirthDate
        );

        if (similarPatients.Any())
        {
            _logger.LogInformation("Found {Count} similar patients", similarPatients.Count);
            var similarPatientDtos = similarPatients.Select(p => new SimilarPatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                BirthDate = p.BirthDate,
                RecordCount = p.MedicalRecords?.Count ?? 0
            });


            // Повертаємо інформацію, що потрібне підтвердження (RequiresConfirmation = true)
            return new UploadAndRecognizeDocumentResult(
                RequiresConfirmation: true,
                RecognizedId: recognizeResult.Id,
                RecognizeData: extractedData,
                SimilarPatients: similarPatientDtos
            );
        }
            
        //ДОРОБИТИ: Зупинитися, дати користувачу редагувати інформацію


        // 7. Якщо пацієнтів не знайдено - створюємо нового
        var newPatient = await _patientRepo.CreateAsync(new Patient
        {
            FirstName = extractedData.FirstName ?? "Unknown",
            LastName = extractedData.LastName,
            BirthDate = extractedData.BirthDate
        });

        // 8. Створюємо фінальний медичний запис і підв'язуємо до нового пацієнта
        var recognizeTextDomain = new RecognizeText
        {
            Id = Guid.NewGuid(),
            PatientId = newPatient.Id,
            Examination = extractedData.Examination,
            Medicine = extractedData.Medicine,
            Treatment = extractedData.Treatment,
            ContraindicatedMedicine = extractedData.ContraindicatedMedicine,
            ContraindicatedReason = extractedData.ContraindicatedReason,
            DateDocument = extractedData.DateDocument,
            CreatedAt = DateTime.UtcNow,
            RecognizedTextId = recognizeResult.Id
        };

        await _recognizeTextRepo.SaveRecognizedTextAsync(recognizeTextDomain);

        _logger.LogInformation("Successfully completed document processing for new patient {PatientId}", newPatient.Id);

        // Повертаємо успішний результат без потреби підтвердження
        return new UploadAndRecognizeDocumentResult(
            RequiresConfirmation: false,
            RecognizedId: recognizeResult.Id,
            RecognizeData: extractedData,
            SimilarPatients: null
        );
    }
}
