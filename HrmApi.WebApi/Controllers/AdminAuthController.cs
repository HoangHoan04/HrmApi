using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Permission;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/admin/auth")]
    public class AdminAuthController : AuthController
    {
        public AdminAuthController(
            IApplicationDbContext context,
            IPasswordHasher<UserEntity> passwordHasher,
            IConfiguration configuration,
            IEmailService emailService)
            : base(context, passwordHasher, configuration, emailService)
        {
        }
    }
}
