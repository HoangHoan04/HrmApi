using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Salary;
using HrmApi.Application.Features.Salaries.Commands;
using HrmApi.Application.Features.Salaries.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/salary")]
    public class SalariesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SalariesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<ActionResult<PagedResult<SalaryDto>>> GetPaged([FromBody] GetSalariesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.PayrollSalaryView)]
        public async Task<ActionResult<SalaryDto>> GetDetail([FromBody] GetSalaryByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy phiếu lương.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PayrollSalaryCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSalaryCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.PayrollSalaryUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateSalaryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("approve")]
        [RequirePermission(PermissionCodes.PayrollSalaryApprove)]
        public async Task<ActionResult<bool>> Approve([FromBody] ApproveSalaryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("mark-paid")]
        [RequirePermission(PermissionCodes.PayrollSalaryMarkPaid)]
        public async Task<ActionResult<bool>> MarkPaid([FromBody] MarkSalaryPaidCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cancel")]
        [RequirePermission(PermissionCodes.PayrollSalaryCancel)]
        public async Task<ActionResult<bool>> Cancel([FromBody] CancelSalaryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
