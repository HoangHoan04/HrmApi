using HrmApi.Domain.Entities.Permission;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IPasswordHasherService
    {
        string HashPassword(UserEntity user, string password);
    }
}
