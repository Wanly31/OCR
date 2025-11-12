using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCR.Models.Domain;
using OCR.Models.DTO;
using OCR.Repositories;
using OCR.Services;

namespace OCR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecognizeController : ControllerBase
    {
        public RecognizeController(IRecognizeRepository recognizeRepository)
        {
            RecognizeRepository = recognizeRepository;
        }

        public IRecognizeRepository RecognizeRepository { get; }
        [HttpPost("{id}")]
        public async Task<IActionResult> Recognize(Guid id, [FromServices] AzureOcrService ocrService)
        {
            // Знайти документ у базі
            var document = await RecognizeRepository.GetByIdAsync(id);
            if (document == null)
                return NotFound("Document not found.");

            // Побудувати абсолютний шлях до файлу у папці Documents
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", $"{document.FileName}{document.FileExtension}");

            // Перевірити, чи файл існує
            if (!System.IO.File.Exists(filePath))
                return NotFound($"File not found at path: {filePath}");

            // Викликати Azure OCR сервіс для розпізнавання
            string recognizedText = await ocrService.ReadDocumentAsync(filePath);

            // Зберегти результат у базі
            var recognized = new Models.Domain.Recognize
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Text = recognizedText
            };

            await RecognizeRepository.SaveRecognizedTextAsync(recognized);

            // Повернути результат клієнту
            return Ok(new
            {
                DocumentId = document.Id,
                Text = recognizedText
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var text = await RecognizeRepository.GetByIdTextAsync(id);

            var textDto = new RecognizeDto
            {
                Id = text.Id,
                Text = text.Text
            };

            return Ok(textDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var TextDomain = await RecognizeRepository.GetAllAsync();

            var textDto = new List<RecognizeDto>();

            foreach (var textDomain in TextDomain)
            {
                textDto.Add(new RecognizeDto
                {
                    Id = textDomain.Id,
                    Text = textDomain.Text
                });
            }
                return Ok(textDto);
          
            
            
        }

    }
}
