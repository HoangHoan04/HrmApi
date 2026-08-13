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
    [Route("api/v1/training-course")]
    public class TrainingCoursesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrainingCoursesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.TrainingCourseView)]
        public async Task<ActionResult<PagedResult<TrainingCourseDto>>> GetPaged([FromBody] GetTrainingCoursesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.TrainingCourseView)]
        public async Task<ActionResult<TrainingCourseDto>> GetDetail([FromBody] GetTrainingCourseByIdQuery query)
        {
            TrainingCourseDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy khóa học.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.TrainingCourseCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateTrainingCourseCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.TrainingCourseUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateTrainingCourseCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy khóa học.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.TrainingCourseDelete)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteTrainingCourseCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy khóa học.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
