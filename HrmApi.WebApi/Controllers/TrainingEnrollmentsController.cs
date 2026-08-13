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
    [Route("api/v1/training-enrollment")]
    public class TrainingEnrollmentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrainingEnrollmentsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.TrainingEnrollmentView)]
        public async Task<ActionResult<PagedResult<TrainingEnrollmentDto>>> GetPaged([FromBody] GetTrainingEnrollmentsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.TrainingEnrollmentView)]
        public async Task<ActionResult<TrainingEnrollmentDto>> GetDetail([FromBody] GetTrainingEnrollmentByIdQuery query)
        {
            TrainingEnrollmentDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy đăng ký đào tạo.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.TrainingEnrollmentManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateTrainingEnrollmentCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.TrainingEnrollmentManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateTrainingEnrollmentCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đăng ký đào tạo.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.TrainingEnrollmentManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteTrainingEnrollmentCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đăng ký đào tạo.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
