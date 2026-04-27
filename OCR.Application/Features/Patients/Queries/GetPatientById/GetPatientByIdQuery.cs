using MediatR;

namespace OCR.Application.Features.Patients.Quaries.GetPatientById
{
    public record GetPatientByIdQuery(Guid id) : IRequest<GetPatientByIdResult>;

    public record GetPatientByIdResult
    (
        Guid Id, 
        string FirstName,
        string? LastName,
        DateOnly? BirthDate, 
        int TotalRecords 
    );
}
