using MediatR;
using Microsoft.AspNetCore.Mvc;
using OCR.Application.Features.Ocr.Commands.SaveMedicalRecord;
using OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument;
using OCR.Application.Features.Ocr.Quaries.GetRecognizeResultById;

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

        [HttpPost("SaveMedicalRecord")]
        public async Task<IActionResult> SaveMedicalRecord([FromBody] SaveMedicalRecordCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("GetRecognizeResultById")]
        public async Task<IActionResult> GetById(Guid Id)
        {
            var result = await _mediator.Send(new GetRecognizeResultByIdQuery(Id));
            return Ok(result);
        }
    }
}
