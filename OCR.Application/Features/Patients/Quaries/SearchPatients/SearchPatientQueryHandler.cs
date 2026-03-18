
using MediatR;
using Microsoft.Extensions.Logging;
using OCR.Application.Abstractions;
using OCR.Application.DTOs;

namespace OCR.Application.Features.Patients.Quaries.SearchPatients
{
    public class SearchPatientQueryHandler : IRequestHandler<SearchPatientQuery, IEnumerable<SearchPatientsResult>>
    {
        private readonly IPatientRepository _patientRepository;


        public SearchPatientQueryHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<SearchPatientsResult>> Handle(SearchPatientQuery request, CancellationToken cancellationToken)
        {
            var patients = await _patientRepository.SearchSimilarAsync(
                request.FirstName,
                request.LastName,
                request.BirthDate
            );

            var response = patients.Select(p => new SearchPatientsResult
            (
                Id: p.Id,
                FirstName: p.FirstName,
                LastName: p.LastName,
                BirthDate: p.BirthDate,
                TotalRecords: p.MedicalRecords?.Count ?? 0
            ));

            return response;
        }
    }
}
