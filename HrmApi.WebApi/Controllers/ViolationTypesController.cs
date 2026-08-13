using System;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Discipline;
using HrmApi.Application.Features.Discipline;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/violation-type")]
    public class ViolationTypesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ViolationTypesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.DisciplineTypeView)]
        public async Task<ActionResult<PagedResult<ViolationTypeDto>>> GetPaged([FromBody] GetViolationTypesPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("detail")]
        [RequirePermission(PermissionCodes.DisciplineTypeView)]
        public async Task<ActionResult<ViolationTypeDto>> GetDetail([FromBody] GetViolationTypeByIdQuery query)
        {
            ViolationTypeDto? result = await _mediator.Send(query);
            if (result == null) return NotFound("Không tìm thấy loại vi phạm.");
            return Ok(result);
        }

        [HttpPost("create")]
        [RequirePermission(PermissionCodes.DisciplineTypeManage)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateViolationTypeCommand command)
        {
            try { return Ok(await _mediator.Send(command)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("update")]
        [RequirePermission(PermissionCodes.DisciplineTypeManage)]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateViolationTypeCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy loại vi phạm.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("delete")]
        [RequirePermission(PermissionCodes.DisciplineTypeManage)]
        public async Task<ActionResult<bool>> Delete([FromBody] DeleteViolationTypeCommand command)
        {
            try
            {
                bool result = await _mediator.Send(command);
                if (!result) return NotFound("Không tìm thấy loại vi phạm.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
