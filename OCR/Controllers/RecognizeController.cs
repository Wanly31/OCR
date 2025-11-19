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
                throw new Exception($"Document with id {id} not found.");

            // Побудувати абсолютний шлях до файлу у папці Documents
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", $"{document.FileName}{document.FileExtension}");

            // Перевірити, чи файл існує
            if (!System.IO.File.Exists(filePath))
                throw new Exception($"File {filePath} not found.");

            // Викликати Azure OCR сервіс для розпізнавання
            string recognizedText = await ocrService.ReadDocumentAsync(filePath);

            // Зберегти результат у базі
            var recognized = new Recognize
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var TextDomain = await RecognizeRepository.GetAllAsync();

            if (TextDomain == null || !TextDomain.Any())
                throw new Exception("No recognized texts found.");

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var text = await RecognizeRepository.GetByIdTextAsync(id);

            if(text == null)
                throw new Exception($"Text with id {id} not found.");

            var textDto = new RecognizeDto
            {
                Id = text.Id,
                Text = text.Text
            };

            return Ok(textDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var recognizeModel = await RecognizeRepository.DeleteAsync(id);

            if(recognizeModel == null)
            {
                throw new Exception($"Recognized text with id: {id} not found");
            }

            return Ok("Recognized text deleted successfully");
        }

    }
}
