using MediatR;

namespace OCR.Application.Features.Patients.Quaries.SearchPatients
{
    public record SearchPatientQuery(
        string FirstName,
        string? LastName,
        DateOnly? BirthDate) : IRequest<IEnumerable<SearchPatientsResult>>;

    public record SearchPatientsResult(
        Guid Id,
        string FirstName,
        string? LastName,
        DateOnly? BirthDate,
        int TotalRecords
    );
}
