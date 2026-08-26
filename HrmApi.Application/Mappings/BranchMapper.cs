using HrmApi.Application.DTOs.Branch;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Mappings
{
    internal class BranchMapper
    {
        public static BranchDto ToDto(BranchEntity entity, string? companyName = null, string? parentBranchName = null)
        {
            return new BranchDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                ShortName = entity.ShortName,
                Description = entity.Description,
                Type = entity.Type,
                CompanyId = entity.CompanyId,
                CompanyName = companyName,
                ParentBranchId = entity.ParentBranchId,
                IsHeadQuarter = entity.IsHeadQuarter,
                Address = entity.Address,
                Country = entity.Country,
                City = entity.City,
                Ward = entity.Ward,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Fax = entity.Fax,
                IpAddress = entity.IpAddress,
                ManagerId = entity.ManagerId,
                ManagerName = entity.ManagerName,
                ManagerPhone = entity.ManagerPhone,
                TaxCode = entity.TaxCode,
                BusinessRegistrationCode = entity.BusinessRegistrationCode,
                OpeningDate = entity.OpeningDate,
                ClosingDate = entity.ClosingDate,
                OperatingStatus = entity.OperatingStatus,
                IsActive = entity.IsActive,
                IsUsingHrm = entity.IsUsingHrm,
                DisplayOrder = entity.DisplayOrder,
                GroupSalary = entity.GroupSalary,
                TimeKeepingStandardId = entity.TimeKeepingStandardId,
                MaxEmployeeCapacity = entity.MaxEmployeeCapacity,
                TimeZone = entity.TimeZone,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted,
                ParentBranchName = parentBranchName,
            };
        }

        public static void ApplyCommandFields(BranchEntity entity, BranchCommandFields fields)
        {
            entity.Code = fields.Code?.Trim() ?? entity.Code;
            entity.Name = fields.Name?.Trim() ?? entity.Name;
            entity.ShortName = TrimOrNull(fields.ShortName) ?? entity.ShortName;
            entity.Description = TrimOrNull(fields.Description) ?? entity.Description;
            entity.Type = TrimOrNull(fields.Type) ?? entity.Type;
            if (fields.CompanyId.HasValue)
            {
                entity.CompanyId = fields.CompanyId;
            }

            if (fields.ParentBranchId.HasValue)
            {
                entity.ParentBranchId = fields.ParentBranchId;
            }

            if (fields.IsHeadQuarter.HasValue)
            {
                entity.IsHeadQuarter = fields.IsHeadQuarter.Value;
            }

            entity.Address = TrimOrNull(fields.Address) ?? entity.Address;
            entity.Country = TrimOrNull(fields.Country);
            entity.City = TrimOrNull(fields.City);
            entity.Ward = TrimOrNull(fields.Ward);
            entity.Latitude = fields.Latitude;
            entity.Longitude = fields.Longitude;
            entity.PhoneNumber = TrimOrNull(fields.PhoneNumber);
            entity.Email = TrimOrNull(fields.Email);
            entity.Fax = TrimOrNull(fields.Fax);
            entity.IpAddress = TrimOrNull(fields.IpAddress) ?? entity.IpAddress;
            entity.ManagerId = fields.ManagerId;
            if (!fields.ManagerId.HasValue)
            {
                entity.ManagerName = TrimOrNull(fields.ManagerName);
                entity.ManagerPhone = TrimOrNull(fields.ManagerPhone);
            }
            else
            {
                entity.ManagerName = TrimOrNull(fields.ManagerName) ?? entity.ManagerName;
                entity.ManagerPhone = TrimOrNull(fields.ManagerPhone) ?? entity.ManagerPhone;
            }
            entity.TaxCode = TrimOrNull(fields.TaxCode) ?? entity.TaxCode;
            entity.BusinessRegistrationCode = TrimOrNull(fields.BusinessRegistrationCode) ?? entity.BusinessRegistrationCode;
            if (fields.OpeningDate.HasValue)
            {
                entity.OpeningDate = fields.OpeningDate;
            }

            if (fields.ClosingDate.HasValue)
            {
                entity.ClosingDate = fields.ClosingDate;
            }

            entity.OperatingStatus = TrimOrNull(fields.OperatingStatus) ?? entity.OperatingStatus;
            if (fields.IsActive.HasValue)
            {
                entity.IsActive = fields.IsActive.Value;
            }

            if (fields.IsUsingHrm.HasValue)
            {
                entity.IsUsingHrm = fields.IsUsingHrm.Value;
            }

            if (fields.DisplayOrder.HasValue)
            {
                entity.DisplayOrder = fields.DisplayOrder.Value;
            }

            entity.GroupSalary = TrimOrNull(fields.GroupSalary) ?? entity.GroupSalary;
            if (fields.TimeKeepingStandardId.HasValue)
            {
                entity.TimeKeepingStandardId = fields.TimeKeepingStandardId;
            }

            if (fields.MaxEmployeeCapacity.HasValue)
            {
                entity.MaxEmployeeCapacity = fields.MaxEmployeeCapacity;
            }

            entity.TimeZone = TrimOrNull(fields.TimeZone) ?? entity.TimeZone;
        }

        public static object ToLogObject(BranchEntity entity)
        {
            return new
            {
                entity.Id,
                entity.Code,
                entity.Name,
                entity.ShortName,
                entity.Description,
                entity.Type,
                entity.CompanyId,
                entity.ParentBranchId,
                entity.IsHeadQuarter,
                entity.Address,
                entity.Country,
                entity.City,
                entity.Ward,
                entity.Latitude,
                entity.Longitude,
                entity.PhoneNumber,
                entity.Email,
                entity.Fax,
                entity.IpAddress,
                entity.ManagerId,
                entity.ManagerName,
                entity.ManagerPhone,
                entity.TaxCode,
                entity.BusinessRegistrationCode,
                entity.OpeningDate,
                entity.ClosingDate,
                entity.OperatingStatus,
                entity.IsActive,
                entity.IsUsingHrm,
                entity.DisplayOrder,
                entity.GroupSalary,
                entity.TimeKeepingStandardId,
                entity.MaxEmployeeCapacity,
                entity.TimeZone
            };
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public class BranchCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? ParentBranchId { get; set; }
        public bool? IsHeadQuarter { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Ward { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Fax { get; set; }
        public string? IpAddress { get; set; }
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerPhone { get; set; }
        public string? TaxCode { get; set; }
        public string? BusinessRegistrationCode { get; set; }
        public DateTime? OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string? OperatingStatus { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsUsingHrm { get; set; }
        public int? DisplayOrder { get; set; }
        public string? GroupSalary { get; set; }
        public Guid? TimeKeepingStandardId { get; set; }
        public int? MaxEmployeeCapacity { get; set; }
        public string? TimeZone { get; set; }

    }
}
