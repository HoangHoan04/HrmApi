using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Performance;
using HrmApi.Application.Features.Performance;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/performance-360")]
    public class Performance360ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public Performance360ReviewsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.Performance360View)]
        public async Task<ActionResult<PagedResult<Performance360ReviewDto>>> GetPaged([FromBody] GetPerformance360ReviewsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.Performance360View)]
        public async Task<ActionResult<Performance360ReviewDto>> GetDetail([FromBody] GetPerformance360ReviewByIdQuery query)
        {
            Performance360ReviewDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy đánh giá 360.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.Performance360Manage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePerformance360ReviewCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.Performance360Manage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdatePerformance360ReviewCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đánh giá 360.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.Performance360Manage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeletePerformance360ReviewCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy đánh giá 360.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
