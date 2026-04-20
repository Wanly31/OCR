using FluentAssertions;
using OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument;
using Xunit;

namespace OCR.UnitTests.Validators;

public class UploadAndRecognizeDocumentCommandValidatorTests
{
    [Fact]
    public void Should_Fail_When_File_Is_Missing()
    {
        var validator = new UploadAndRecognizeDocumentCommandValidator();
        var command = new UploadAndRecognizeDocumentCommand(null!, null, "user-1");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}

