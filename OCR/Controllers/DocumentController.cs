using Microsoft.AspNetCore.Mvc;
using OCR.Domain.Entities;
using OCR.Application.DTOs;
using OCR.Application.Abstractions;
using MediatR;
using OCR.Application.Features.Documents.Commands.DeleteDocument;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentController(IDocumentRepository documentRepository, IMediator mediator)
        {
            DocumentRepository = documentRepository;
            _mediator = mediator;
        }

        public IDocumentRepository DocumentRepository { get; }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var document = await DocumentRepository.GetByIdAsync(id);
            
            if (document == null)
            {
                    return NotFound($"Document with id: {id} not found");
             
            }

            var documentDto = new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FileDescription = document.FileDescription,
                FileSizeInBytes = document.FileSizeInBytes
            };

            return Ok(documentDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var dokumentModel = await _mediator.Send(new DeleteDocumentCommand(id));

            return Ok(dokumentModel);
        }

    }
}
