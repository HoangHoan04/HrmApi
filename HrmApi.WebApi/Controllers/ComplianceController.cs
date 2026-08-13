using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
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
    [Route("api/v1/compliance")]
    public class ComplianceController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ComplianceController(IMediator mediator) => _mediator = mediator;

        [HttpPost("summary")]
        [RequirePermission(PermissionCodes.ComplianceView)]
        public async Task<ActionResult<ComplianceSummaryDto>> Summary([FromBody] GetComplianceSummaryQuery query)
            => Ok(await _mediator.Send(query));
    }
}
