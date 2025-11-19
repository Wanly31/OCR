using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OCR.Models.Domain;
using OCR.Models.DTO;
using OCR.Repositories;
using OCR.Services;

namespace OCR.Controllers
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
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    DateDocument = recogText.DateDocument,
                    CreatedAt = DateTime.UtcNow,
                    RecognizedTextId = id
                };

                await RecognizeTextRepository.SaveRecognizedTextAsync(recognizeTextDomain);

                return Ok(new
                {
                    Id = id,
                    ExtractedDate = recogText.DateDocument?.ToString("yyyy-MM-dd"),
                    FirstName = recogText.FirstName,
                    LastName = recogText.LastName,
                    Medicine = recogText.Medicine,
                    Treatment = recogText.Treatment,
                    ProcessedAt = DateTime.UtcNow
                });


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error recognizing text for id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var TextDomain = await RecognizeTextRepository.GetAllAsync();
            if (TextDomain == null || !TextDomain.Any())
            {
                throw new Exception("No texts found");
            }

            var textDto = new List<RecognizeTextDto>();

            foreach (var textDomain in TextDomain)
            {
                textDto.Add(new RecognizeTextDto
                {
                    Id = textDomain.Id,
                    FirstName = textDomain.FirstName,
                    LastName = textDomain.LastName,
                    Medicine = textDomain.Medicine,
                    Treatment = textDomain.Treatment,
                    DateDocument = textDomain.DateDocument,
                    CreatedAt = textDomain.CreatedAt
                });
            }
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

            var textDto = new RecognizeTextDto
            {
                Id = docDto.Id,
                FirstName = docDto.FirstName,
                LastName = docDto.LastName,
                Medicine = docDto.Medicine,
                Treatment = docDto.Treatment,
                DateDocument = docDto.DateDocument,
                CreatedAt = docDto.CreatedAt

            };

            return Ok(textDto);
        }

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
