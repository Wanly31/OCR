using MediatR;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;

namespace OCR.Application.Features.Ocr.Quaries.GetRecognizeResultById
{
    public class GetRecognizeResultByIdQueryHandler : IRequestHandler<GetRecognizeResultByIdQuery, RecognizeTextResult>
    {
        private readonly IRecognizeTextRepository _recognizeTextRepository;

        public GetRecognizeResultByIdQueryHandler(IRecognizeTextRepository recognizeTextRepository)
        {
            _recognizeTextRepository = recognizeTextRepository;

        }

        public async Task<RecognizeTextResult> Handle(GetRecognizeResultByIdQuery request, CancellationToken cancellationToken)
        {
            var docDto = await _recognizeTextRepository.GetByIdTextAsync(request.id);
            if (docDto == null)
            {
                throw new NotFoundException("З таким ід нема", request.id);
            }

            // Temporary fix - include patient data via navigation property 
            return new RecognizeTextResult(
                Id: docDto.Id,
                FirstName: docDto.Patient?.FirstName,
                LastName: docDto.Patient?.LastName,
                Medicine: docDto.Medicine,
                Treatment: docDto.Treatment,
                DateDocument: docDto.DateDocument,
                CreatedAt: docDto.CreatedAt
            );
        }
    }
}
