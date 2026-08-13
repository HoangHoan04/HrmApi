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
    [Route("api/v1/interview-schedule")]
    public class InterviewSchedulesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public InterviewSchedulesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewView)]
        public async Task<ActionResult<PagedResult<InterviewScheduleDto>>> GetPaged([FromBody] GetInterviewSchedulesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewView)]
        public async Task<ActionResult<InterviewScheduleDto>> GetDetail([FromBody] GetInterviewScheduleByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy lịch phỏng vấn.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateInterviewScheduleCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateInterviewScheduleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch phỏng vấn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewManage)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelInterviewScheduleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch phỏng vấn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("complete")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewManage)]
        public async Task<ActionResult<bool>> Complete([FromBody] CompleteInterviewScheduleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch phỏng vấn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("set-interviewers")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewManage)]
        public async Task<ActionResult<bool>> SetInterviewers([FromBody] SetInterviewersCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch phỏng vấn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("calendar-range")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewView)]
        public async Task<ActionResult<List<InterviewScheduleDto>>> CalendarRange([FromBody] GetInterviewCalendarRangeQuery query)
        {
            try { return Ok(await _mediator.Send(query)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("upsert-evaluations")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewManage)]
        public async Task<ActionResult<bool>> UpsertEvaluations([FromBody] UpsertInterviewEvaluationsCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy lịch phỏng vấn.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("evaluations")]
        [RequirePermission(PermissionCodes.RecruitmentInterviewView)]
        public async Task<ActionResult<List<EvaluationDto>>> GetEvaluations([FromBody] GetInterviewEvaluationsQuery query)
            => Ok(await _mediator.Send(query));
    }
}
