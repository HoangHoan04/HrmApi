using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Salary;
using HrmApi.Application.Features.Salaries.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [RequirePermission(PermissionCodes.MobileAccess)]
    [Route("api/v1/mobile/salary")]
    public class MobileSalaryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MobileSalaryController(IMediator mediator) => _mediator = mediator;

        [HttpPost("my-list")]
        public async Task<ActionResult<List<SalaryDto>>> MyList([FromBody] GetMySalariesQuery? query)
        {
            try { return Ok(await _mediator.Send(query ?? new GetMySalariesQuery())); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("detail")]
        public async Task<ActionResult<SalaryDto>> Detail([FromBody] GetMySalaryDetailQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                if (result == null) return NotFound("Không tìm thấy phiếu lương.");
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
