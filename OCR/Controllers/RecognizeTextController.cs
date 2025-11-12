using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCR.Repositories;
using OCR.Services;

namespace OCR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecognizeTextController : ControllerBase
    {
        public RecognizeTextController(IRecognizeTextRepository recognizeTextRepository) 
        {
            RecognizeTextRepository = recognizeTextRepository;
        }

        public IRecognizeTextRepository RecognizeTextRepository { get; }

        [HttpPost("{id}")]
        public async Task<IActionResult> RecognizeTextAsync(Guid id, [FromServices] RecognizeTextService recognizeText)
        {
            var text = await RecognizeTextRepository.GetByIdAsync(id);
            if (text == null)
            {
                return NotFound("Recognized text not found.");
            }

            var textResult = text.Text;

            string recogText = await recognizeText.RecognizeText(textResult);

            return Ok(recogText);
        }
    }
}
