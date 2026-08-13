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
    [Route("api/v1/training-course-material")]
    public class TrainingCourseMaterialsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TrainingCourseMaterialsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.TrainingCourseView)]
        public async Task<ActionResult<PagedResult<CourseMaterialDto>>> GetPaged([FromBody] GetCourseMaterialsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.TrainingCourseView)]
        public async Task<ActionResult<CourseMaterialDto>> GetDetail([FromBody] GetCourseMaterialByIdQuery query)
        {
            CourseMaterialDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy tài liệu khóa học.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.TrainingCourseUpdate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCourseMaterialCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.TrainingCourseUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateCourseMaterialCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy tài liệu khóa học.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.TrainingCourseUpdate)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteCourseMaterialCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy tài liệu khóa học.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
