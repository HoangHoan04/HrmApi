using HrmApi.Application.Common.Constants;
using HrmApi.Application.Features.EmployeeWorkPatterns;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/employee-work-pattern")]
    public class EmployeeWorkPatternsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EmployeeWorkPatternsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pagination")]
        [RequirePermission(PermissionCodes.OperateWorkPatternView)]
        public async Task<ActionResult> Pagination([FromBody] GetEmployeeWorkPatternsPagedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("upsert")]
        [RequirePermission(PermissionCodes.OperateWorkPatternManage)]
        public async Task<ActionResult<Guid>> Upsert([FromBody] UpsertEmployeeWorkPatternCommand command)
            => Ok(await _mediator.Send(command));

        [HttpPost("bulk-upsert")]
        [RequirePermission(PermissionCodes.OperateWorkPatternBulkAssign)]
        public async Task<ActionResult<BulkEmployeeWorkPatternResult>> BulkUpsert(
            [FromBody] BulkUpsertEmployeeWorkPatternCommand command)
            => Ok(await _mediator.Send(command));

        [HttpPost("deactivate")]
        [RequirePermission(PermissionCodes.OperateWorkPatternDeactivate)]
        public async Task<ActionResult<bool>> Deactivate([FromBody] DeactivateEmployeeWorkPatternCommand command)
            => Ok(await _mediator.Send(command));
    }
}
