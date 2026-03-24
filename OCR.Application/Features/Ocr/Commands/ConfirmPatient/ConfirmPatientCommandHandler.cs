using MediatR;
using Microsoft.Extensions.Logging;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;
using OCR.Domain.Entities;

namespace OCR.Application.Features.Ocr.Commands.ConfirmPatient
{
    public class ConfirmPatientCommandHandler : IRequestHandler<ConfirmPatientCommand, ConfirmPatientResult>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger _logger;
        private readonly IRecognizeTextRepository _recognizeTextRepository;

        public ConfirmPatientCommandHandler(
            ILogger<ConfirmPatientCommandHandler> logger,
            IPatientRepository patientRepository,
            IRecognizeTextRepository recognizeTextRepository
            )
        {
            _logger = logger;
            _patientRepository = patientRepository;
            _recognizeTextRepository = recognizeTextRepository;
        }

        public async Task<ConfirmPatientResult> Handle(ConfirmPatientCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Patient confirmation request received. ExistingPatientId: {request.ExistingPatientId}");

            Patient patient;

            if (request.ExistingPatientId.HasValue)
            {
                // Use existing patient
                patient = await _patientRepository.GetByIdAsync(request.ExistingPatientId.Value);
                if (patient == null)
                {

                    throw new NotFoundException(nameof(patient), request.ExistingPatientId.Value);
                }


            }
            else
            {
                // Create new patient
                patient = await _patientRepository.CreateAsync(new Patient
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate
                });
                _logger.LogInformation($"Created new patient: {patient.Id}");
            }

            // Create medical record linked to patient
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

            try
            {
                await _recognizeTextRepository.SaveRecognizedTextAsync(recognizeText);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving recognized text to repository: {ex.Message}");
            }

            return new ConfirmPatientResult(
                Id: patient.Id,
                FirstName: request.FirstName,
                LastName: request.LastName,
                BirthDate: request.BirthDate);
           
        }
    }
}
