using System;
using System.Collections.Generic;
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
    [Route("api/v1/candidate")]
    public class CandidatesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CandidatesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateView)]
        public async Task<ActionResult<PagedResult<CandidateDto>>> GetPaged([FromBody] GetCandidatesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateView)]
        public async Task<ActionResult<CandidateDto>> GetDetail([FromBody] GetCandidateByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy ứng viên.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCandidateCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateCandidateCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy ứng viên.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("change-status")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateUpdate)]
        public async Task<ActionResult<bool>> ChangeStatus([FromBody] ChangeCandidateStatusCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy ứng viên.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("hire-prefill")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateView)]
        public async Task<ActionResult<CandidateHirePrefillDto>> HirePrefill([FromBody] GetCandidateHirePrefillQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy ứng viên.");
            return Ok(result);
        }

        [HttpPost("link-employee")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateUpdate)]
        public async Task<ActionResult<bool>> LinkEmployee([FromBody] LinkCandidateEmployeeCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy ứng viên.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("status-summary")]
        [RequirePermission(PermissionCodes.RecruitmentPipelineView)]
        public async Task<ActionResult<List<CandidateStatusSummaryDto>>> StatusSummary([FromBody] GetCandidateStatusSummaryQuery query)
            => Ok(await _mediator.Send(query));
    }
}
