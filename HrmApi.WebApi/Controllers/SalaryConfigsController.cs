using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Salary;
using HrmApi.Application.Features.SalaryConfigs.Commands;
using HrmApi.Application.Features.SalaryConfigs.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/salary-config")]
    public class SalaryConfigsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SalaryConfigsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.PayrollConfigView)]
        public async Task<ActionResult<PagedResult<SalaryConfigDto>>> GetPaged([FromBody] GetSalaryConfigsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.PayrollConfigView)]
        public async Task<ActionResult<SalaryConfigDto>> GetDetail([FromBody] GetSalaryConfigByIdQuery query)
        {
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy cấu hình lương.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.PayrollConfigCreate)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSalaryConfigCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.PayrollConfigUpdate)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateSalaryConfigCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy cấu hình lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activate")]
        [RequirePermission(PermissionCodes.PayrollConfigActivate)]
        public async Task<ActionResult<bool>> Activate([FromBody] ActivateSalaryConfigCommand command)
        {
            command.IsActive = true;
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình lương.");
            return Ok(result);
        }

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.PayrollConfigDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] ActivateSalaryConfigCommand command)
        {
            command.IsActive = false;
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Không tìm thấy cấu hình lương.");
            return Ok(result);
        }

        [HttpPost("select-box")]
        [RequirePermission(PermissionCodes.PayrollConfigView)]
        public async Task<ActionResult<List<SalaryConfigSelectBoxDto>>> SelectBox([FromBody] GetSalaryConfigSelectBoxQuery? query)
            => Ok(await _mediator.Send(query ?? new GetSalaryConfigSelectBoxQuery()));
    }
}
