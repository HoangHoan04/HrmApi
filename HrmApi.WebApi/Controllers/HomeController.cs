using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Home;
using HrmApi.Application.Features.Home;
using HrmApi.WebApi.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/home")]
    public class HomeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Dashboard tổng quan HR (số liệu thực).</summary>
        [HttpPost("dashboard")]
        [RequirePermission(PermissionCodes.HomeView)]
        public async Task<ActionResult<HomeDashboardDto>> GetDashboard([FromBody] GetHomeDashboardQuery? query)
        {
            var result = await _mediator.Send(query ?? new GetHomeDashboardQuery());
            return Ok(result);
        }
    }
}
