using Microsoft.AspNetCore.Mvc;
using MediatR;
using OCR.Application.Features.Documents.Commands.DeleteDocument;
using OCR.Application.Features.Documents.Queries.GetDocumentFile;

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


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var documentModel = await _mediator.Send(new GetDocumentFileQuery(id));

            return Ok(documentModel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var documentModel = await _mediator.Send(new DeleteDocumentCommand(id));

            return Ok(documentModel);
        }

    }
}
