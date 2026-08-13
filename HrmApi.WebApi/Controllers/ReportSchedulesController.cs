using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Application.Features.Settings;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/report-schedule")]
    public class ReportSchedulesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ReportSchedulesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.ReportScheduleView)]
        public async Task<ActionResult<PagedResult<ReportScheduleDto>>> GetPaged([FromBody] GetReportSchedulesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.ReportScheduleView)]
        public async Task<ActionResult<ReportScheduleDto>> GetDetail([FromBody] GetReportScheduleByIdQuery query)
        {
            ReportScheduleDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy lịch báo cáo.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.ReportScheduleManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateReportScheduleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.ReportScheduleManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateReportScheduleCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch báo cáo.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.ReportScheduleManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteReportScheduleCommand command)
        {
            bool result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy lịch báo cáo.");
            return Ok(result);
        }

        [HttpPost("run-due")]
        [RequirePermission(PermissionCodes.ReportScheduleManage)]
        public async Task<ActionResult<int>> RunDue([FromBody] RunDueReportSchedulesCommand? command)
            => Ok(await _mediator.Send(command ?? new RunDueReportSchedulesCommand()));
    }
}
