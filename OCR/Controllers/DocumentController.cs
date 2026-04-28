using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using OCR.Application.Features.Documents.Commands.DeleteDocument;
using OCR.Application.Features.Documents.Queries.GetDocumentStream;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("{id}/file")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFile(Guid id)
        {
            var result = await _mediator.Send(new GetDocumentStreamQuery(id));
            // Без FileName — браузер відображає inline (не примусово скачує)
            return File(result.FileStream, result.ContentType);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var documentModel = await _mediator.Send(new DeleteDocumentCommand(id));

            return Ok(documentModel);
        }

    }
}
