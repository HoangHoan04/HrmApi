using System;
using System.Collections.Generic;
using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Chi nhánh của công ty
    /// </summary>
    public class BranchEntity : BaseEntity
    {
        /// <summary>
        /// Mã chi nhánh (duy nhất, dùng để nhận diện)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary> 
        /// Tên đầy đủ của chi nhánh
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tên viết tắt / tên hiển thị ngắn gọn của chi nhánh
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi nhánh (chức năng, quy mô, …)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Loại chi nhánh: Văn phòng, Nhà máy, Kho, Showroom, …
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Id công ty sở hữu chi nhánh này
        /// </summary>
        public Guid? CompanyId { get; set; } = null;

        /// <summary>
        /// Id chi nhánh cha (nếu chi nhánh này là chi nhánh phụ thuộc chi nhánh khác)
        /// </summary>
        public Guid? ParentBranchId { get; set; } = null;

        /// <summary>
        /// Có phải là trụ sở chính (Head Office) hay không
        /// </summary>
        public bool IsHeadQuarter { get; set; } = false;

        /// <summary>
        /// Địa chỉ đầy đủ của chi nhánh (số nhà, đường, phường/xã)
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Quốc gia
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Thành phố / Tỉnh
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Quận / Huyện
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// Phường / Xã
        /// </summary>
        public string? Ward { get; set; }

        /// <summary>
        /// Vĩ độ (dùng để hiển thị bản đồ, tính khoảng cách)
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Kinh độ (dùng để hiển thị bản đồ, tính khoảng cách)
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ của chi nhánh
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Email liên hệ của chi nhánh
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Số fax
        /// </summary>
        public string? Fax { get; set; }

        /// <summary>
        /// Đia chỉ IP của chi nhánh (dùng cho máy chấm công, thiết bị nội bộ, …)
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Id nhân viên quản lý / phụ trách chi nhánh (Trưởng chi nhánh)
        /// </summary>
        public Guid? ManagerId { get; set; }

        /// <summary>
        /// Họ tên người quản lý chi nhánh (lưu snapshot, phòng khi không liên kết được Employee)
        /// </summary>
        public string? ManagerName { get; set; }

        /// <summary>
        /// Số điện thoại người quản lý chi nhánh
        /// </summary>

        public string? ManagerPhone { get; set; }
        /// <summary>
        /// Mã số thuế riêng của chi nhánh (nếu hạch toán độc lập)
        /// </summary>

        public string? TaxCode { get; set; }
        /// <summary>
        /// Mã số đăng ký kinh doanh của chi nhánh (nếu có)
        /// </summary>

        public string? BusinessRegistrationCode { get; set; }
        /// <summary>
        /// Ngày chi nhánh bắt đầu hoạt động
        /// </summary>

        public DateTime? OpeningDate { get; set; }
        /// <summary>
        /// Ngày chi nhánh ngừng hoạt động (nếu đã đóng cửa)
        /// </summary>

        public DateTime? ClosingDate { get; set; }
        /// <summary>
        /// Trạng thái hoạt động: Hoạt động, Tạm ngừng, Đã đóng cửa, … (có thể dùng enum)
        /// </summary>

        public string? OperatingStatus { get; set; }
        /// <summary>
        /// Trạng thái kích hoạt trong hệ thống (true: đang sử dụng, false: vô hiệu – không xóa dữ liệu)
        /// </summary>

        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Có sử dụng module HRM (chấm công, nhân sự, …) cho chi nhánh này hay không
        /// </summary>

        public bool IsUsingHrm { get; set; }
        /// <summary>
        /// Thứ tự hiển thị chi nhánh trên danh sách / cây tổ chức
        /// </summary>

        public int DisplayOrder { get; set; }
        /// <summary>
        /// Nhóm tính lương áp dụng cho chi nhánh (liên kết bảng cấu hình lương)
        /// </summary>

        public string GroupSalary { get; set; } = string.Empty;
        /// <summary>
        /// Id chuẩn chấm công áp dụng riêng cho chi nhánh (nếu khác chuẩn của công ty)
        /// </summary>

        public Guid? TimeKeepingStandardId { get; set; }
        /// <summary>
        /// Số lượng nhân viên tối đa được phép tại chi nhánh (dùng cho kế hoạch nhân sự)
        /// </summary>

        public int? MaxEmployeeCapacity { get; set; }
        /// <summary>
        /// Múi giờ làm việc của chi nhánh (nếu khác múi giờ mặc định công ty)
        /// </summary>

        public string? TimeZone { get; set; }

        /// <summary>
        /// Danh sách chi nhánh con (nếu có cấu trúc chi nhánh cha - con)
        /// </summary>
        public List<BranchEntity> ChildBranches { get; set; } = new List<BranchEntity>();

        /// <summary>
        /// Danh sách bộ phận sản xuất / tổ (Part Master) thuộc chi nhánh
        /// </summary>
        public List<PartMasterEntity> PartMasters { get; set; } = new List<PartMasterEntity>();

        /// <summary>
        /// Danh sách phòng ban thuộc chi nhánh
        /// </summary>
        public List<DepartmentEntity> Departments { get; set; } = new List<DepartmentEntity>();
    }
}