using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Models;
using HrmApi.Application.Features.ActionLogs.Queries;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HrmApi.WebApi.Controllers
{
    [Authorize]
    [RequirePermission(PermissionCodes.ActionLogView)]
    [ApiController]
    [Route("api/v1/action-logs")]
    public class ActionLogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ActionLogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ActionLogDto>>> GetPaged([FromQuery] GetActionLogsQuery query)
        {
            if (query == null)
            {
                query = new GetActionLogsQuery();
            }
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
