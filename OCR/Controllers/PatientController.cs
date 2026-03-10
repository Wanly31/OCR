using Microsoft.AspNetCore.Mvc;
using OCR.Domain.Entities;
using OCR.Application.DTOs;          
using OCR.Domain.Interfaces;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<PatientController> _logger;

        public PatientController(IPatientRepository patientRepository, ILogger<PatientController> logger)
        {
            _patientRepository = patientRepository;
            _logger = logger;
        }

        // POST /api/Patient/search
        [HttpPost("search")]
        public async Task<IActionResult> SearchSimilar([FromBody] PatientSearchRequestDto request)
        {
            _logger.LogInformation($"Searching for similar patients: {request.FirstName} {request.LastName}");

            var patients = await _patientRepository.SearchSimilarAsync(
                request.FirstName,
                request.LastName,
                request.BirthDate
            );

            var response = patients.Select(p => new PatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                BirthDate = p.BirthDate,
                TotalRecords = p.MedicalRecords?.Count ?? 0
            });

            return Ok(response);
        }

        // GET /api/Patient/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            
            if (patient == null)
            {
                return NotFound(new { message = $"Patient with ID {id} not found" });
            }

            var dto = new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                TotalRecords = patient.MedicalRecords?.Count ?? 0
            };

            return Ok(dto);
        }

        // GET /api/Patient/{id}/history
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetPatientHistory(Guid id)
        {
            _logger.LogInformation($"Retrieving history for patient: {id}");

            var history = await _patientRepository.GetPatientHistoryAsync(id);

            return Ok(history);
        }
    }
}
