using FluentValidation;

namespace OCR.Application.Features.Ocr.Quaries.GetRecognizeResultById
{
    public class GetRecognizeResultByIdQueryValidator : AbstractValidator<GetRecognizeResultByIdQuery>
    {
        public GetRecognizeResultByIdQueryValidator()
        {
            RuleFor(x => x.id)
                .NotEmpty().WithMessage("Document id is required");
        }
    }
}
