using HrmApi.Domain.Common;
using System;

namespace HrmApi.Domain.Entities.AuditLog
{
    public class ActionLogEntity : BaseEntity
    {
        public Guid CreatedById { get; set; }
        public string CreatedByCode { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string? CreatedNote { get; set; }
        public string? ActionType { get; set; }
        public Guid? EntityId { get; set; }
        public string? EntityName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
    }
}
