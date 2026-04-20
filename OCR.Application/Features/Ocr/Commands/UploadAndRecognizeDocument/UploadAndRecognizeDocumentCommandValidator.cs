using FluentValidation;

namespace OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument;

public class UploadAndRecognizeDocumentCommandValidator
    : AbstractValidator<UploadAndRecognizeDocumentCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".pdf", ".png" };

    public UploadAndRecognizeDocumentCommandValidator()
    {
        RuleFor(x => x.File).NotNull();
        RuleFor(x => x.File.Length).LessThanOrEqualTo(10_385_760)
            .WithMessage("File size cannot exceed 10MB")
            .When(x => x.File != null);
        RuleFor(x => x.File.FileName)
            .Must(f => AllowedExtensions.Contains(Path.GetExtension(f).ToLower()))
            .WithMessage("Only .jpg, .jpeg, .pdf, .png")
            .When(x => x.File != null);
        RuleFor(x => x.FileName).NotEmpty();
    }
}
