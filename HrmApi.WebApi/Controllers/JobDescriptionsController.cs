using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Features.Recruitment;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/job-description")]
    public class JobDescriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public JobDescriptionsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.RecruitmentJdView)]
        public async Task<ActionResult<PagedResult<JobDescriptionDto>>> GetPaged([FromBody] GetJobDescriptionsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RecruitmentJdView)]
        public async Task<ActionResult<JobDescriptionDto>> GetDetail([FromBody] GetJobDescriptionByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy JD.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RecruitmentJdCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateJobDescriptionCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RecruitmentJdUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateJobDescriptionCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy JD.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.RecruitmentJdDelete)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteJobDescriptionCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy JD.");
            return Ok(result);
        }
    }
}
