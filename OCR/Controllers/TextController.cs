using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCR.Models.Domain;
using OCR.Repositories;
using OCR.Services;

namespace OCR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextController : ControllerBase
    {
        public TextController(ITextRepository textRepository)
        {
            TextRepository = textRepository;
        }

        public ITextRepository TextRepository { get; }
        [HttpPost("{id}")]
        public async Task<IActionResult> Recognize(Guid id, [FromServices] AzureOcrService ocrService)
        {
            // Знайти документ у базі
            var document = await TextRepository.GetByIdAsync(id);
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
            var recognized = new RecognizedText
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Text = recognizedText
            };

            await TextRepository.SaveRecognizedTextAsync(recognized);

            // Повернути результат клієнту
            return Ok(new
            {
                DocumentId = document.Id,
                Text = recognizedText
            });
        }

    }
}
