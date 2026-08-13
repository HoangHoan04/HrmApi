using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
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
    [Route("api/v1/headcount")]
    public class HeadcountsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public HeadcountsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("tree")]
        [RequirePermission(PermissionCodes.RecruitmentHeadcountView)]
        public async Task<ActionResult<List<HeadcountNodeDto>>> GetTree([FromBody] GetHeadcountTreeQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("upsert-row")]
        [RequirePermission(PermissionCodes.RecruitmentHeadcountUpdate)]
        public async Task<ActionResult<bool>> UpsertRow([FromBody] UpsertHeadcountRowCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
