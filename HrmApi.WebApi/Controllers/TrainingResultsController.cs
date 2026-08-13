using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
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
    [Route("api/v1/training-result")]
    public class TrainingResultsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrainingResultsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.TrainingResultView)]
        public async Task<ActionResult<PagedResult<TrainingResultDto>>> GetPaged([FromBody] GetTrainingResultsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.TrainingResultView)]
        public async Task<ActionResult<TrainingResultDto>> GetDetail([FromBody] GetTrainingResultByIdQuery query)
        {
            TrainingResultDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy kết quả đào tạo.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.TrainingResultManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateTrainingResultCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.TrainingResultManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateTrainingResultCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy kết quả đào tạo.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.TrainingResultManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteTrainingResultCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy kết quả đào tạo.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
