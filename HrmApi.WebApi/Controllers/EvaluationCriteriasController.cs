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
    [Route("api/v1/evaluation-criteria")]
    public class EvaluationCriteriasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EvaluationCriteriasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.RecruitmentCriteriaView)]
        public async Task<ActionResult<PagedResult<EvaluationCriteriaDto>>> GetPaged([FromBody] GetEvaluationCriteriasPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RecruitmentCriteriaView)]
        public async Task<ActionResult<EvaluationCriteriaDto>> GetDetail([FromBody] GetEvaluationCriteriaByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy tiêu chí đánh giá.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RecruitmentCriteriaManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateEvaluationCriteriaCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RecruitmentCriteriaManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateEvaluationCriteriaCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy tiêu chí đánh giá.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.RecruitmentCriteriaManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteEvaluationCriteriaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy tiêu chí đánh giá.");
            return Ok(result);
        }
    }
}
