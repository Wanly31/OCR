using MediatR;
using Microsoft.Extensions.Logging;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;
using OCR.Domain.Entities;
using OCR.Domain.Enums;

namespace OCR.Application.Features.Ocr.Commands.SaveMedicalRecord
{
    public class SaveMedicalRecordCommandHandler : IRequestHandler<SaveMedicalRecordCommand, SaveMedicalRecordResult>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<SaveMedicalRecordCommandHandler> _logger;
        private readonly IRecognizeTextRepository _recognizeTextRepository;
        private readonly IRecognizeRepository _recognizeRepository;

        public SaveMedicalRecordCommandHandler(
            ILogger<SaveMedicalRecordCommandHandler> logger,
            IPatientRepository patientRepository,
            IRecognizeTextRepository recognizeTextRepository,
            IRecognizeRepository recognizeRepository
            )
        {
            _logger = logger;
            _patientRepository = patientRepository;
            _recognizeTextRepository = recognizeTextRepository;
            _recognizeRepository = recognizeRepository;
        }

        public async Task<SaveMedicalRecordResult> Handle(SaveMedicalRecordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
    "Patient confirmation request received. ExistingPatientId: {ExistingPatientId}",
    request.ExistingPatientId);


            //Fix: ApplicationException(need to add BadRequestException)
            if (request.RecognizedData == null)
                throw new ApplicationException("Recognized data is required");

            Patient patient;

            if (request.ExistingPatientId.HasValue)
            {
                patient = await _patientRepository.GetByIdAsync(request.ExistingPatientId.Value);

                if (patient == null)
                    throw new NotFoundException("Patient", request.ExistingPatientId.Value);
            }
            else
            {
                patient = await _patientRepository.CreateAsync(new Patient
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate
                });

                _logger.LogInformation("Created new patient {PatientId}", patient.Id);
            }

            // update recognize status
            var recognize = await _recognizeRepository.GetByIdTextAsync(request.RecognizedId);
            if (recognize == null)
                throw new NotFoundException("Recognize", request.RecognizedId);

            recognize.Status = RecordStatus.Confirmed;
            await _recognizeRepository.UpdateAsync(recognize.Id, recognize);

            // create medical record
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

            await _recognizeTextRepository.SaveRecognizedTextAsync(recognizeText);

            _logger.LogInformation("Medical record saved successfully for patient {PatientId}", patient.Id);

            return new SaveMedicalRecordResult(
                Id: patient.Id,
                FirstName: patient.FirstName,
                LastName: patient.LastName,
                BirthDate: patient.BirthDate
            );

        }
    }
}
