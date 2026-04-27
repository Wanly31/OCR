using MediatR;
using OCR.Application.Abstractions;

namespace OCR.Application.Features.Patients.Quaries.SearchPatients
{
    public class SearchPatientQueryHandler
    : IRequestHandler<SearchPatientQuery, PaginatedResult<SearchPatientsResult>>
    {
        private readonly IPatientRepository _patientRepository;

        public SearchPatientQueryHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PaginatedResult<SearchPatientsResult>> Handle(
            SearchPatientQuery request,
            CancellationToken cancellationToken)
        {
            var patients = await _patientRepository.SearchSimilarAsync(
                request.FirstName,
                request.LastName,
                request.BirthDate
            );

            var totalCount = patients.Count();

            var pagedPatients = patients
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var items = pagedPatients.Select(p => new SearchPatientsResult
            (
                Id: p.Id,
                FirstName: p.FirstName,
                LastName: p.LastName,
                BirthDate: p.BirthDate,
                TotalRecords: p.MedicalRecords?.Count ?? 0
            ));

            return new PaginatedResult<SearchPatientsResult>(
                Items: items,
                TotalCount: totalCount,
                Page: request.Page,
                PageSize: request.PageSize
            );
        }
    }
}
