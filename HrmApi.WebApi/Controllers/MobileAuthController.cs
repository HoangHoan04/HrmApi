using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Permission;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/mobile/auth")]
    public class MobileAuthController : AuthController
    {
        public MobileAuthController(
            IApplicationDbContext context,
            IPasswordHasher<UserEntity> passwordHasher,
            IConfiguration configuration,
            IEmailService emailService,
            IAuthContextService authContext,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(context, passwordHasher, configuration, emailService, authContext, httpClientFactory, loggerFactory)
        {
        }
    }
}
