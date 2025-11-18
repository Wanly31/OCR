using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        public RecognizeTextController(IRecognizeTextRepository recognizeTextRepository, RecognizeTextService recognizeTextService)
        {
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
    }

}
