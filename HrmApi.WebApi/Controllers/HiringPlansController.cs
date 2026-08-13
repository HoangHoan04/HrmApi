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
    [Route("api/v1/hiring-plan")]
    public class HiringPlansController : ControllerBase
    {
        private readonly IMediator _mediator;
        public HiringPlansController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.RecruitmentPlanView)]
        public async Task<ActionResult<PagedResult<HiringPlanDto>>> GetPaged([FromBody] GetHiringPlansPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RecruitmentPlanView)]
        public async Task<ActionResult<HiringPlanDto>> GetDetail([FromBody] GetHiringPlanByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy kế hoạch tuyển dụng.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RecruitmentPlanCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateHiringPlanCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RecruitmentPlanUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateHiringPlanCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy kế hoạch tuyển dụng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("set-criteria")]
        [RequirePermission(PermissionCodes.RecruitmentPlanUpdate)]
        public async Task<ActionResult<bool>> SetCriteria([FromBody] SetHiringPlanCriteriaCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy kế hoạch tuyển dụng.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
