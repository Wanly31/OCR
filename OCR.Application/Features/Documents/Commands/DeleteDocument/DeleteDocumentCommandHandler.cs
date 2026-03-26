using MediatR;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;
using OCR.Domain.Entities;

namespace OCR.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, DeleteDocumentResult>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IPatientRepository _patientRepository;
        private readonly IRecognizeRepository _recognizeRepository;
        private readonly IRecognizeTextRepository _recognizeTextRepository;

        public DeleteDocumentCommandHandler(
            IDocumentRepository documentRepository,
            IFileStorage fileStorage,
            IPatientRepository patientRepository,
            IRecognizeRepository recognizeRepository,
            IRecognizeTextRepository recognizeTextRepository)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
            _patientRepository = patientRepository;
            _recognizeRepository = recognizeRepository;
            _recognizeTextRepository = recognizeTextRepository;
        }


        public async Task<DeleteDocumentResult> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            // 1. отримуємо document
            var document = await _documentRepository.GetByIdAsync(request.id);

            if (document == null)
                throw new NotFoundException("Document not found", request.id);

            // 2. отримуємо recognize по DocumentId
            var recognize = await _recognizeRepository.GetByIdAsync(document.Id);

            RecognizeText? recognizeText = null;

            if (recognize != null)
            {
                recognizeText = recognize.RecognizedDocument;
            }

            // 3. видаляємо файл
            await _fileStorage.DeleteFileAsync(document.FilePath);

            // 4. видаляємо recognize
            if (recognize != null)
                await _recognizeRepository.DeleteAsync(recognize.Id);

            // 5. видаляємо recognizeText + patient (тільки якщо більше немає записів)
            if (recognizeText != null)
            {
                await _recognizeTextRepository.DeleteAsync(recognizeText.Id);

                // Видаляємо пацієнта тільки якщо більше немає медичних записів
                var remainingRecords = await _patientRepository.GetPatientHistoryAsync(recognizeText.PatientId);
                if (remainingRecords.Count == 0)
                {
                    await _patientRepository.DeleteAsync(recognizeText.PatientId);
                }
            }

            // 6. видаляємо document
            await _documentRepository.DeleteAsync(document.Id);

            return new DeleteDocumentResult(true);
        }
    }
}