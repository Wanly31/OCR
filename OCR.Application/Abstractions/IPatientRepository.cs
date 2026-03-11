using OCR.Domain.Entities;

namespace OCR.Application.Abstractions
{ 
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(Guid id);
        Task<List<Patient>> SearchSimilarAsync(string firstName, string? lastName, DateOnly? birthDate);
        Task<List<RecognizeText>> GetPatientHistoryAsync(Guid patientId);
        Task<Patient> CreateAsync(Patient patient);
        Task<Patient?> UpdateAsync(Guid id, Patient patient);
    }
}
