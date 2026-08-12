namespace HrmApi.Application.Common.Constants
{
    public static class RbacSeedIds
    {
        public static readonly Guid SystemActor = Guid.Parse("00000000-0000-0000-0000-000000000000");
        public static readonly Guid DefaultAdminUser = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public static readonly Guid RoleAdmin = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid RoleHr = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid RoleManager = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid RoleEmployee = Guid.Parse("10000000-0000-0000-0000-000000000004");
    }
}
