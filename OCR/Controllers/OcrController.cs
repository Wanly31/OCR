using Microsoft.AspNetCore.Mvc;
using MediatR;
using OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument;
using OCR.Application.Features.Ocr.Commands.ConfirmPatient;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OcrController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("UploadAndRecognize")]
        public async Task<IActionResult> Upload([FromForm] UploadAndRecognizeDocumentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("ConfirmPatient")]
        public async Task<IActionResult> ConfirmPatient([FromBody] ConfirmPatientCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
