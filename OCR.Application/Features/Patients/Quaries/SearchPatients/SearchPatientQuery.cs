using MediatR;

namespace OCR.Application.Features.Patients.Quaries.SearchPatients
{
    public record SearchPatientQuery(
        string FirstName,
        string? LastName,
        DateOnly? BirthDate,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<PaginatedResult<SearchPatientsResult>>;

    public record SearchPatientsResult(
        Guid Id,
        string FirstName,
        string? LastName,
        DateOnly? BirthDate,
        int TotalRecords
    );

    public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
}
