using MediatR;

namespace OCR.Application.Features.Patients.Quaries.GetPatientHistory
{
    public record GetPatientHistoryQuery(Guid id) : IRequest<IEnumerable<GetPatientHistoryResult>>;

    public record GetPatientHistoryResult
    (
        Guid Id,
        Guid PatientId,
        string FirstName,
        string LastName,
        string? Examination,
        string? Medicine,
        string? Treatment,
        string? ContraindicatedMedicine,
        string? ContraindicatedReason,
        DateOnly? DateDocument,
        DateTime? CreatedAt
    );
}

