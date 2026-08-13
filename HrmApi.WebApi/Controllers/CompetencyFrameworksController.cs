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
    [Route("api/v1/competency")]
    public class CompetencyFrameworksController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CompetencyFrameworksController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PerformanceCompetencyView)]
        public async Task<ActionResult<PagedResult<CompetencyFrameworkDto>>> GetPaged([FromBody] GetCompetencyFrameworksPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.PerformanceCompetencyView)]
        public async Task<ActionResult<CompetencyFrameworkDto>> GetDetail([FromBody] GetCompetencyFrameworkByIdQuery query)
        {
            CompetencyFrameworkDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy khung năng lực.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PerformanceCompetencyManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCompetencyFrameworkCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.PerformanceCompetencyManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateCompetencyFrameworkCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy khung năng lực.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.PerformanceCompetencyManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteCompetencyFrameworkCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy khung năng lực.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
