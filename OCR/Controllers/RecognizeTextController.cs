using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Recognizers.Text;
using OCR.Domain.Entities;
using OCR.Application.DTOs;
using OCR.Domain.Interfaces;
using OCR.Infrastructure.Services;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecognizeTextController : ControllerBase
    {
        public readonly ILogger<RecognizeTextController> logger;

        public RecognizeTextController(IRecognizeTextRepository recognizeTextRepository, RecognizeTextService recognizeTextService, ILogger<RecognizeTextController> logger)
        {
            this.logger = logger;
            RecognizeTextRepository = recognizeTextRepository;
            RecognizeTextService = recognizeTextService;
        }

        public IRecognizeTextRepository RecognizeTextRepository { get; }
        public RecognizeTextService RecognizeTextService { get; }

        // TODO: These legacy endpoints need to be updated to work with new Patient model
        // For now, use OcrController.UploadAndRecognize and OcrController.ConfirmPatient instead

        /* DEPRECATED - Use OcrController.UploadAndRecognize instead
        [HttpPost("{id}")]
        public async Task<IActionResult> RecognizeTextAsync(Guid id)
        {
            try
            {
                var text = await RecognizeTextRepository.GetByIdAsync(id);
                if (text == null)
                {
                    return NotFound($"Text with id: {id} not found");
                }

                if (string.IsNullOrWhiteSpace(text.Text))
                {
                    return BadRequest("Text content is empty");
                }

                var recogText = await RecognizeTextService.RecognizeText(text.Text);

                var recognizeTextDomain = new RecognizeText
                {
                    Id = Guid.NewGuid(),
                    FirstName = recogText.FirstName,
                    LastName = recogText.LastName,
                    BirthDate = recogText.BirthDate,
                    Examination = recogText.Examination,
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    ContraindicatedMedicine = recogText.ContraindicatedMedicine,
                    ContraindicatedReason = recogText.ContraindicatedReason,
                    DateDocument = recogText.DateDocument,
                    CreatedAt = DateTime.UtcNow,
                    RecognizedTextId = id
                };

                await RecognizeTextRepository.SaveRecognizedTextAsync(recognizeTextDomain);

                return Ok(new
                {
                    Id = recogText.Id,
                    FirstName = recogText.FirstName,
                    LastName = recogText.LastName,
                    BirthDate = recogText.BirthDate,
                    Examination = recogText.Examination,
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    ContraindicatedMedicine = recogText.ContraindicatedMedicine,
                    ContraindicatedReason = recogText.ContraindicatedReason,
                    DateDocument = recogText.DateDocument,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                });


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error recognizing text for id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        */
       
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var TextDomain = await RecognizeTextRepository.GetAllAsync();
            if (TextDomain == null || !TextDomain.Any())
            {
                throw new Exception("No texts found");
            }

            // Tempor ary fix - include patient data via navigation property
            var textDto = TextDomain.Select(textDomain => new
            {
                textDomain.Id,
                FirstName = textDomain.Patient?.FirstName,
                LastName = textDomain.Patient?.LastName,
                textDomain.Medicine,
                textDomain.Treatment,
                textDomain.DateDocument,
                textDomain.CreatedAt
            }).ToList();

            return Ok(textDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var docDto = await RecognizeTextRepository.GetByIdTextAsync(id);
            if (docDto == null)
            {
                throw new Exception($"Text whith id: {id} not found");
            }

            // Temporary fix - include patient data via navigation property 
            var textDto = new
            {
                docDto.Id,
                FirstName = docDto.Patient?.FirstName,
                LastName = docDto.Patient?.LastName,
                docDto.Medicine,
                docDto.Treatment,
                docDto.DateDocument,
                docDto.CreatedAt
            };

            return Ok(textDto);
        }

        /* DEPRECATED - Patient data should be updated via PatientController
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecognizeTextDto updateRecognizeTextDto)
        {
            var textDomainModel = new RecognizeText
            {
                FirstName = updateRecognizeTextDto.FirstName,
                LastName = updateRecognizeTextDto.LastName,
                Medicine = updateRecognizeTextDto.Medicine,
                Treatment = updateRecognizeTextDto.Treatment,
                DateDocument = updateRecognizeTextDto.DateDocument
            };

            textDomainModel = await RecognizeTextRepository.UpdateAsync(id, textDomainModel);
            if(textDomainModel == null)
            {
                return NotFound($"Not found person whith id: {id}");
            }

            var textDto = new RecognizeTextDto
            {
                FirstName = textDomainModel.FirstName,
                LastName = textDomainModel.LastName,
                Medicine = textDomainModel.Medicine,
                Treatment = textDomainModel.Treatment,
                DateDocument = textDomainModel.DateDocument,
                CreatedAt = textDomainModel.CreatedAt
            };

            return Ok (textDto);
        }
        */

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var textModel = await RecognizeTextRepository.DeleteAsync(id);
            if (textModel == null)
            {
                throw new Exception($"Text whith {id} not found");
            }

            return Ok("Delete text succesfully");
        }

    }
}
