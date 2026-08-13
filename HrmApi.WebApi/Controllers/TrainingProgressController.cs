using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Training;
using HrmApi.Application.Features.Training;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/training-progress")]
    public class TrainingProgressController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrainingProgressController(IMediator mediator) => _mediator = mediator;

        [HttpPost("summary")]
        [RequirePermission(PermissionCodes.TrainingView)]
        public async Task<ActionResult<List<TrainingProgressDto>>> Summary([FromBody] GetTrainingProgressSummaryQuery query)
            => Ok(await _mediator.Send(query));
    }
}
