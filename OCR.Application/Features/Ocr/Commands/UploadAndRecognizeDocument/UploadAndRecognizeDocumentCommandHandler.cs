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
        UploadAndRecognizeDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting file upload and OCR processing");

        // 1. Зберігаємо файл через IFileStorage
        var extension = Path.GetExtension(request.File.FileName);
        string newName = Guid.NewGuid().ToString() + extension;
       
        string blobName = await _fileStorage.SaveFileAsync(request.File, newName);
        _logger.LogInformation("File saved to blob: {BlobName}", blobName);

        // 2. Створюємо доменну сутність Document (зберігаємо SAS URL)
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            FileDescription = request.FileDescription,
            FileExtension = extension,
            FileSizeInBytes = request.File.Length,
            FilePath = blobName
        };

        await _documentRepo.Upload(document);

        // 3. Отримуємо свіжий URL для OCR (SAS URL, дійсний 1 год)
        string fileUrl = await _fileStorage.GetFileUrlAsync(blobName);
        _logger.LogInformation("Got file URL for OCR: {Url}", fileUrl);

        // 4. OCR розпізнавання тексту з файлу
        string recognizedText = await _ocrProvider.RecognizeTextFromFileAsync(fileUrl);

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

        var similarPatientDtos = similarPatients.Any()
            ? similarPatients.Select(p => new SimilarPatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                BirthDate = p.BirthDate,
                RecordCount = p.MedicalRecords?.Count ?? 0
            })
            : null;

        return new UploadAndRecognizeDocumentResult(
            RequiresConfirmation: true,
            RecognizedId: recognizeResult.Id,
            RecognizeData: extractedData,
            RecordStatus: Domain.Enums.RecordStatus.Pending,
            SimilarPatients: similarPatientDtos,
            FilePath: blobName,
            DocumentId: document.Id
        );
    }
}
