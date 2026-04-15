using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using OCR.Application.Features.Patients.Quaries.GetPatientById;
using OCR.Application.Features.Patients.Quaries.GetPatientHistory;
using OCR.Application.Features.Patients.Quaries.SearchPatients;

namespace OCR.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PatientController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/Patient/search
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchSimilar([FromBody] SearchPatientQuery quaries)
        {
            var result = await _mediator.Send(quaries);
            return Ok(result);
        }

        // GET /api/Patient/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetPatientByIdQuery(id));
            return Ok(result);
        }

        // GET /api/Patient/{id}/history
        [HttpGet("{id:guid}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPatientHistory(Guid id)
        {

            var history = await _mediator.Send(new GetPatientHistoryQuery(id));
            return Ok(history);
        }
    }
}
