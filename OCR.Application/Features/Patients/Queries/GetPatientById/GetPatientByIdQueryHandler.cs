using MediatR;
using Microsoft.Extensions.Logging;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;
using OCR.Application.DTOs;

namespace OCR.Application.Features.Patients.Quaries.GetPatientById
{
    public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, GetPatientByIdResult>
    {

        private readonly IPatientRepository _patientRepository;

        public GetPatientByIdQueryHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<GetPatientByIdResult> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(request.id);

            if (patient == null)
            {
                throw new NotFoundException($"Patient with ID not found", request.id);
            }

            return new GetPatientByIdResult
            (
                Id: patient.Id,
                FirstName: patient.FirstName,
                LastName: patient.LastName,
                BirthDate: patient.BirthDate,
                TotalRecords: patient.MedicalRecords?.Count ?? 0
            );


        }
    }
}