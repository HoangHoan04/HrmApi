namespace HrmApi.Application.DTOs.Permission
{
    public class PermissionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsScopable { get; set; }
    }

    public class PermissionActionNodeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public bool IsScopable { get; set; } = true;
    }

    public class PermissionItemNodeDto
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<PermissionActionNodeDto> Actions { get; set; } = [];
    }

    public class PermissionModuleTreeDto
    {
        public string Module { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public List<PermissionItemNodeDto> Items { get; set; } = [];

        public List<PermissionDto> Permissions { get; set; } = [];
    }
}
