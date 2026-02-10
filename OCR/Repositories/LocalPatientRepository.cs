using Microsoft.EntityFrameworkCore;
using OCR.Data;
using OCR.Models.Domain;

namespace OCR.Repositories
{
    public class LocalPatientRepository : IPatientRepository
    {
        private readonly OCRDbContext _dbContext;
        private readonly ILogger<LocalPatientRepository> _logger;

        public LocalPatientRepository(OCRDbContext dbContext, ILogger<LocalPatientRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Patient?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Patients
                .Include(p => p.MedicalRecords)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Patient>> SearchSimilarAsync(string firstName, string? lastName, DateOnly? birthDate)
        {
            var query = _dbContext.Patients
                .Include(p => p.MedicalRecords)
                .AsQueryable();

            // Пошук за ім'ям з case-insensitive порівнянням
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(p => p.FirstName.ToLower() == firstName.ToLower());
            }

            // Якщо є прізвище - додаємо фільтр
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(p => p.LastName != null && p.LastName.ToLower() == lastName.ToLower());
            }

            // Якщо є дата народження - додаємо фільтр
            if (birthDate.HasValue)
            {
                query = query.Where(p => p.BirthDate == birthDate);
            }

            var results = await query.ToListAsync();
            
            _logger.LogInformation($"Found {results.Count} similar patients for '{firstName} {lastName}' born {birthDate}");
            
            return results;
        }

        public async Task<List<RecognizeText>> GetPatientHistoryAsync(Guid patientId)
        {
            return await _dbContext.RecognizedDocuments
                .Where(rt => rt.PatientId == patientId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            patient.Id = Guid.NewGuid();
            patient.CreatedAt = DateTime.UtcNow;

            await _dbContext.Patients.AddAsync(patient);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Created new patient: {patient.Id} - {patient.FirstName} {patient.LastName}");

            return patient;
        }

        public async Task<Patient?> UpdateAsync(Guid id, Patient patient)
        {
            var existing = await _dbContext.Patients.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning($"Patient not found: {id}");
                return null;
            }

            existing.FirstName = patient.FirstName;
            existing.LastName = patient.LastName;
            existing.BirthDate = patient.BirthDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Updated patient: {id}");

            return existing;
        }
    }
}
