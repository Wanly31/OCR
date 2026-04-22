using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OCR.Application.Abstractions;
using OCR.Application.DTOs;
using OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument;
using OCR.Domain.Entities;
using OCR.Domain.Enums;
using Xunit;

namespace OCR.UnitTests.Application.Ocr;

public class UploadAndRecognizeDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _documentRepoMock;
    private readonly Mock<IRecognizeRepository> _recognizeRepoMock;
    private readonly Mock<IRecognizeTextRepository> _recognizeTextRepoMock;
    private readonly Mock<IOcrProvider> _ocrProviderMock;
    private readonly Mock<IMedicalExtractionService> _extractionServiceMock;
    private readonly Mock<IPatientRepository> _patientRepoMock;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<ILogger<UploadAndRecognizeDocumentCommandHandler>> _loggerMock;
    private readonly UploadAndRecognizeDocumentCommandHandler _handler;

    public UploadAndRecognizeDocumentCommandHandlerTests()
    {
        _documentRepoMock = new Mock<IDocumentRepository>();
        _recognizeRepoMock = new Mock<IRecognizeRepository>();
        _recognizeTextRepoMock = new Mock<IRecognizeTextRepository>();
        _ocrProviderMock = new Mock<IOcrProvider>();
        _extractionServiceMock = new Mock<IMedicalExtractionService>();
        _patientRepoMock = new Mock<IPatientRepository>();
        _fileStorageMock = new Mock<IFileStorage>();
        _loggerMock = new Mock<ILogger<UploadAndRecognizeDocumentCommandHandler>>();

        _handler = new UploadAndRecognizeDocumentCommandHandler(
            _documentRepoMock.Object,
            _recognizeRepoMock.Object,
            _recognizeTextRepoMock.Object,
            _ocrProviderMock.Object,
            _extractionServiceMock.Object,
            _patientRepoMock.Object,
            _fileStorageMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldProcessAndReturnSuccessResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.pdf");
        fileMock.Setup(f => f.Length).Returns(1024);

        var command = new UploadAndRecognizeDocumentCommand(fileMock.Object, "Test File", "A test document");

        _fileStorageMock.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("/saved/path/test.pdf");

        _ocrProviderMock.Setup(x => x.RecognizeTextFromFileAsync("/saved/path/test.pdf"))
            .ReturnsAsync("John Smith 01/01/1990 Aspirin");

        var extractedData = new RecognizedTextResultDto
        {
            FirstName = "John",
            LastName = "Smith",
            BirthDate = new DateOnly(1990, 1, 1),
            Medicine = "Aspirin"
        };

        _extractionServiceMock.Setup(x => x.ExtractMedicalDataAsync("John Smith 01/01/1990 Aspirin"))
            .ReturnsAsync(extractedData);

        _patientRepoMock.Setup(x => x.SearchSimilarAsync("John", "Smith", new DateOnly(1990, 1, 1)))
            .ReturnsAsync(new List<Patient>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequiresConfirmation.Should().BeTrue();
        result.RecordStatus.Should().Be(RecordStatus.Pending);
        result.SimilarPatients.Should().BeNull();
        result.FilePath.Should().Be("/saved/path/test.pdf");
        result.RecognizeData.Should().BeEquivalentTo(extractedData);

        _fileStorageMock.Verify(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Once);
        _documentRepoMock.Verify(x => x.Upload(It.IsAny<Document>()), Times.Once);
        _ocrProviderMock.Verify(x => x.RecognizeTextFromFileAsync(It.IsAny<string>()), Times.Once);
        _recognizeRepoMock.Verify(x => x.SaveRecognizedTextAsync(It.IsAny<Recognize>()), Times.Once);
        _extractionServiceMock.Verify(x => x.ExtractMedicalDataAsync(It.IsAny<string>()), Times.Once);
        _patientRepoMock.Verify(x => x.SearchSimilarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyOcrResult_ShouldThrowApplicationException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("empty.pdf");

        var command = new UploadAndRecognizeDocumentCommand(fileMock.Object, "Empty File", null);

        _fileStorageMock.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("/saved/path/empty.pdf");

        _ocrProviderMock.Setup(x => x.RecognizeTextFromFileAsync("/saved/path/empty.pdf"))
            .ReturnsAsync(string.Empty);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Text content recognized from file is empty.");

        _documentRepoMock.Verify(x => x.Upload(It.IsAny<Document>()), Times.Once);
        _recognizeRepoMock.Verify(x => x.SaveRecognizedTextAsync(It.IsAny<Recognize>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SimilarPatientsFound_ShouldReturnSimilarPatientsList()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.pdf");

        var command = new UploadAndRecognizeDocumentCommand(fileMock.Object, "Test File", null);

        _fileStorageMock.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("/saved/path/test.pdf");

        _ocrProviderMock.Setup(x => x.RecognizeTextFromFileAsync(It.IsAny<string>()))
            .ReturnsAsync("Some recognized text");

        var extractedData = new RecognizedTextResultDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            BirthDate = new DateOnly(1985, 5, 5)
        };

        _extractionServiceMock.Setup(x => x.ExtractMedicalDataAsync(It.IsAny<string>()))
            .ReturnsAsync(extractedData);

        var similarPatient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            BirthDate = new DateOnly(1985, 5, 5),
            MedicalRecords = new List<RecognizeText> { new RecognizeText { Id = Guid.NewGuid() } }
        };

        _patientRepoMock.Setup(x => x.SearchSimilarAsync("Jane", "Doe", new DateOnly(1985, 5, 5)))
            .ReturnsAsync(new List<Patient> { similarPatient });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SimilarPatients.Should().NotBeNullOrEmpty();
        result.SimilarPatients!.Should().HaveCount(1);
        
        var returnedPatient = result.SimilarPatients!.First();
        returnedPatient.Id.Should().Be(similarPatient.Id);
        returnedPatient.FirstName.Should().Be(similarPatient.FirstName);
        returnedPatient.LastName.Should().Be(similarPatient.LastName);
        returnedPatient.RecordCount.Should().Be(1);
    }
}
