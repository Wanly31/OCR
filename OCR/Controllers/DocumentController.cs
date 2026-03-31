using Microsoft.AspNetCore.Mvc;
using MediatR;
using OCR.Application.Features.Documents.Commands.DeleteDocument;
using OCR.Application.Features.Documents.Queries.GetDocumentStream;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetFile(Guid id)
        {
            var result = await _mediator.Send(new GetDocumentStreamQuery(id));
            return File(result.FileStream, result.ContentType, result.FileName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var documentModel = await _mediator.Send(new DeleteDocumentCommand(id));

            return Ok(documentModel);
        }

    }
}
