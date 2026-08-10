using HrmApi.Domain.Common;

namespace HrmApi.Domain.Entities.Organization
{
    /// <summary>
    /// Mẫu Chức danh (Position Master) — định nghĩa danh mục chức danh dùng chung
    /// (ví dụ: "Nhân viên vận hành máy", "Tổ trưởng") kèm theo quy tắc giờ làm việc,
    /// chấm công mặc định áp dụng cho chức danh đó.
    /// Mỗi phòng ban / tổ sẽ tạo ra các bản ghi <see cref="PositionEntity"/> cụ thể tham chiếu tới mẫu này.
    /// </summary>
    public class PositionMasterEntity : BaseEntity
    {

        /// <summary>
        /// Mã mẫu chức danh (duy nhất, dùng để nhận diện)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên mẫu chức danh
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chức danh (nhiệm vụ, yêu cầu công việc, …)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id công ty sở hữu mẫu chức danh này
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Id chi nhánh sở hữu mẫu chức danh này (null nếu áp dụng chung cho mọi chi nhánh của công ty)
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Có giới hạn số giờ làm việc theo chức danh này hay không
        /// </summary>
        public bool IsLimitHoursWorking { get; set; }

        /// <summary>
        /// Giá trị giới hạn giờ làm (dạng chuỗi vì có thể lưu biểu thức/khoảng thay vì 1 số cố định,
        /// ví dụ "8", "8-10"). Chỉ có ý nghĩa khi <see cref="IsLimitHoursWorking"/> = true.
        /// </summary>
        public string? Limit { get; set; }

        /// <summary>
        /// Số giờ làm việc chuẩn trong ngày/ca của chức danh này
        /// </summary>
        public int? WorkingHour { get; set; }

        /// <summary>
        /// Số giờ làm việc tối thiểu để được tính công đầy đủ (dùng khi chấm công)
        /// </summary>
        public int? MinimumWorkingHour { get; set; }

        /// <summary>
        /// Giờ bắt đầu ca làm việc chuẩn
        /// </summary>
        public TimeSpan? HourWorkingStart { get; set; }

        /// <summary>
        /// Giờ kết thúc ca làm việc chuẩn
        /// </summary>
        public TimeSpan? HourWorkingEnd { get; set; }

        /// <summary>
        /// Chức danh này có áp dụng chấm công (bằng máy chấm công / hệ thống) hay không
        /// </summary>
        public bool IsTimeKeeping { get; set; }

        /// <summary>
        /// Giờ bắt đầu khung chụp/quét chấm công đầu ca (Snapshot) — dùng để xác định đi trễ
        /// </summary>
        public TimeSpan? HourSnapShotStart { get; set; }

        /// <summary>
        /// Giờ kết thúc khung chụp/quét chấm công cuối ca (Snapshot) — dùng để xác định về sớm
        /// </summary>
        public TimeSpan? HourSnapShotEnd { get; set; }

        /// <summary>
        /// Cho phép vượt chuẩn chấm công (đi trễ/về sớm ngoài khung quy định) mà không bị tính vi phạm
        /// </summary>
        public bool IsAllowOverTimekeepingStandard { get; set; }

        /// <summary>
        /// Chức danh này có được phép hoán đổi (swap) với chức danh khác hay không
        /// </summary>
        public bool IsSwapPosition { get; set; }

        /// <summary>
        /// Danh sách Id các chức danh (PositionMaster) có thể chuyển đổi tới,
        /// lưu dạng chuỗi các Guid phân tách bằng dấu phẩy. Chỉ có ý nghĩa khi <see cref="IsSwapPosition"/> = true.
        /// </summary>
        public string? TargetChangePositionIds { get; set; }

        /// <summary>
        /// Việc tuyển ứng viên vào chức danh này có cần phê duyệt hay không
        /// </summary>
        public bool IsApprovedWhenHiringCandidate { get; set; }

        /// <summary>
        /// Ứng viên vào chức danh này có bắt buộc phải qua vòng phỏng vấn thứ 2 hay không
        /// </summary>
        public bool IsHadASecondInterview { get; set; }

        /// <summary>
        /// Nhân viên giữ chức danh này khi xin nghỉ phép có cần phê duyệt hay không
        /// </summary>
        public bool IsApprovedDayOff { get; set; }

        /// <summary>
        /// Số lượng định biên chuẩn cho chức danh này (theo quy hoạch nhân sự)
        /// </summary>
        public int? QuantityStandard { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt (true: đang sử dụng, false: vô hiệu – không xóa dữ liệu)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị trên danh sách chọn mẫu chức danh
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Danh sách chức danh cụ thể (tại từng phòng ban/tổ) được tạo từ mẫu này
        /// </summary>
        public List<PositionEntity> Positions { get; set; } = [];

        /// <summary>
        /// Navigation property tới công ty sở hữu mẫu chức danh này (CompanyEntity)
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh sở hữu mẫu chức danh này (BranchEntity)
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }
    }
}