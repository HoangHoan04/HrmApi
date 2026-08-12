using System.Collections.Generic;

namespace HrmApi.Application.DTOs.Auth
{
    public class AuthContextDto
    {
        public List<string> Roles { get; set; } = [];
        public List<string> Permissions { get; set; } = [];
    }
}
