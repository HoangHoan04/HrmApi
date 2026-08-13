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
    [Route("api/v1/hiring-source")]
    public class HiringSourcesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public HiringSourcesController(IMediator mediator) => _mediator = mediator;

        /// <summary>Danh sách nguồn (dropdown UV + màn quản lý). Sync catalog hệ thống khi gọi.</summary>
        [HttpPost("list")]
        [RequirePermission(PermissionCodes.RecruitmentCandidateView)]
        public async Task<ActionResult<List<HiringSourceDto>>> List([FromBody] GetHiringSourcesQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RecruitmentSourceView)]
        public async Task<ActionResult<HiringSourceDto>> Detail([FromBody] GetHiringSourceDetailQuery query)
        {
            HiringSourceDto? dto = await _mediator.Send(query);
            if (dto == null) return NotFound("Không tìm thấy nguồn tuyển.");
            return Ok(dto);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RecruitmentSourceManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateHiringSourceCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RecruitmentSourceManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateHiringSourceCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy nguồn tuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.RecruitmentSourceManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteHiringSourceCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy nguồn tuyển.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
