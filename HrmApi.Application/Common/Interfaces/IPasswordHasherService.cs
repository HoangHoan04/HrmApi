using HrmApi.Domain.Entities.Permission;

namespace HrmApi.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction over password hashing to avoid coupling the Application layer to ASP.NET Identity directly.
    /// </summary>
    public interface IPasswordHasherService
    {
        string HashPassword(UserEntity user, string password);
    }
}
