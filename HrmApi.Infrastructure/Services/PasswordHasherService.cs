using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Permission;
using Microsoft.AspNetCore.Identity;

namespace HrmApi.Infrastructure.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly IPasswordHasher<UserEntity> _hasher;

        public PasswordHasherService(IPasswordHasher<UserEntity> hasher)
        {
            _hasher = hasher;
        }

        public string HashPassword(UserEntity user, string password)
        {
            return _hasher.HashPassword(user, password);
        }
    }
}
