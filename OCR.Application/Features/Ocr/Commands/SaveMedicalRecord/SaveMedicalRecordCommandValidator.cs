
using FluentValidation;

namespace OCR.Application.Features.Ocr.Commands.SaveMedicalRecord
{
    public class SaveMedicalRecordCommandValidator : AbstractValidator<SaveMedicalRecordCommand>
    {
        public SaveMedicalRecordCommandValidator()
        {
            RuleFor(x => x.RecognizedId)
                .NotEmpty().WithMessage("RecognizedId is required");

            RuleFor(x => x.RecognizedData)
                .NotNull().WithMessage("RecognizedData is required");

            When(x => !x.ExistingPatientId.HasValue, () =>
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty().WithMessage("FirstName is required when creating new patient");

                RuleFor(x => x.LastName)
                    .NotEmpty().WithMessage("LastName is required when creating new patient");
            });
        }
    }
}
