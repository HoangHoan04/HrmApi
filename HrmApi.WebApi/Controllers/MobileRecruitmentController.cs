using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Features.Mobile;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [RequirePermission(PermissionCodes.MobileAccess)]
    [Route("api/v1/mobile/recruitment")]
    public class MobileRecruitmentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileRecruitmentController(IMediator mediator) => _mediator = mediator;

        [HttpPost("my-interviews")]
        public async Task<ActionResult<List<InterviewScheduleDto>>> MyInterviews([FromBody] GetMyInterviewSchedulesQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMyInterviewSchedulesQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
