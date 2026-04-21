using MediatR;
using OCR.Application.Abstractions;

namespace OCR.Application.Features.Patients.Quaries.GetPatientHistory
{
    public class GetPatientHistoryQueryHandler : IRequestHandler<GetPatientHistoryQuery, IEnumerable<GetPatientHistoryResult>>
    {

        private readonly IPatientRepository _patientRepository;

        public GetPatientHistoryQueryHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<GetPatientHistoryResult>> Handle(GetPatientHistoryQuery request, CancellationToken cancellationToken)
        {
            var records = await _patientRepository.GetPatientHistoryAsync(request.id);


            return records.Select(rt => new GetPatientHistoryResult(
                Id: rt.Id,
                PatientId: rt.PatientId,
                FirstName: rt.Patient.FirstName,
                LastName: rt.Patient.LastName,
                Examination: rt.Examination,
                Medicine: rt.Medicine,
                Treatment: rt.Treatment,
                ContraindicatedMedicine: rt.ContraindicatedMedicine,
                ContraindicatedReason: rt.ContraindicatedReason,
                DateDocument: rt.DateDocument,
                CreatedAt: rt.CreatedAt,
                DocumentId: rt.RecognizedText.DocumentId
            ));
        }
    }
}
