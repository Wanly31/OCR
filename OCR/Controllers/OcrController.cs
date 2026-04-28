using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCR.Application.Features.Ocr.Commands.SaveMedicalRecord;
using OCR.Application.Features.Ocr.Commands.UploadAndRecognizeDocument;
using OCR.Application.Features.Ocr.Quaries.GetRecognizeResultById;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OcrController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OcrController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("UploadAndRecognize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] UploadAndRecognizeDocumentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("SaveMedicalRecord")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveMedicalRecord([FromBody] SaveMedicalRecordCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("GetRecognizeResultById/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetRecognizeResultByIdQuery(id));
            return Ok(result);
        }

    }
}
