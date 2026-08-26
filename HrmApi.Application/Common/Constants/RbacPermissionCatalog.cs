namespace HrmApi.Application.Common.Constants
{
    public static class RbacPermissionCatalog
    {
        public sealed record Item(
            string Code,
            string Name,
            string Module,
            string Action,
            string? Description = null,
            bool IsScopable = true);

        public static readonly IReadOnlyList<Item> Items = BuildItems();

        public static readonly string[] HrCodes = BuildHrCodes();
        public static readonly string[] ManagerCodes = BuildManagerCodes();
        public static readonly string[] EmployeeCodes = BuildEmployeeCodes();

        private static List<Item> BuildItems()
        {
            List<Item> list = [];

            void A(string code, string name, string module, string action, bool scopable = true)
            {
                list.Add(new Item(code, name, module, action, null, scopable));
            }

            // HOME
            A(PermissionCodes.HomeView, "Xem", "HOME", "VIEW", false);

            // ORGANIZATION 
            A(PermissionCodes.OrgView, "Xem", "ORGANIZATION", "VIEW");
            A(PermissionCodes.OrgManage, "Quản lý", "ORGANIZATION", "MANAGE");

            void OrgCrud(string module, string view, string manage, string create, string update, string deactivate, string activate, string importExcel, string exportExcel)
            {
                A(view, "Xem", module, "VIEW");
                A(create, "Tạo mới", module, "CREATE");
                A(update, "Cập nhật", module, "UPDATE");
                A(deactivate, "Ngưng", module, "DEACTIVATE");
                A(activate, "Kích hoạt", module, "ACTIVATE");
                A(importExcel, "Import Excel", module, "IMPORT_EXCEL");
                A(exportExcel, "Export Excel", module, "EXPORT_EXCEL");
                A(manage, "Quản lý", module, "MANAGE");
            }

            OrgCrud("ORGANIZATION_COMPANY",
                PermissionCodes.OrgCompanyView, PermissionCodes.OrgCompanyManage,
                PermissionCodes.OrgCompanyCreate, PermissionCodes.OrgCompanyUpdate,
                PermissionCodes.OrgCompanyDeactivate, PermissionCodes.OrgCompanyActivate,
                PermissionCodes.OrgCompanyImportExcel, PermissionCodes.OrgCompanyExportExcel);
            OrgCrud("ORGANIZATION_BRANCH",
                PermissionCodes.OrgBranchView, PermissionCodes.OrgBranchManage,
                PermissionCodes.OrgBranchCreate, PermissionCodes.OrgBranchUpdate,
                PermissionCodes.OrgBranchDeactivate, PermissionCodes.OrgBranchActivate,
                PermissionCodes.OrgBranchImportExcel, PermissionCodes.OrgBranchExportExcel);
            OrgCrud("ORGANIZATION_DEPARTMENT",
                PermissionCodes.OrgDepartmentView, PermissionCodes.OrgDepartmentManage,
                PermissionCodes.OrgDepartmentCreate, PermissionCodes.OrgDepartmentUpdate,
                PermissionCodes.OrgDepartmentDeactivate, PermissionCodes.OrgDepartmentActivate,
                PermissionCodes.OrgDepartmentImportExcel, PermissionCodes.OrgDepartmentExportExcel);
            OrgCrud("ORGANIZATION_POSITION",
                PermissionCodes.OrgPositionView, PermissionCodes.OrgPositionManage,
                PermissionCodes.OrgPositionCreate, PermissionCodes.OrgPositionUpdate,
                PermissionCodes.OrgPositionDeactivate, PermissionCodes.OrgPositionActivate,
                PermissionCodes.OrgPositionImportExcel, PermissionCodes.OrgPositionExportExcel);
            OrgCrud("ORGANIZATION_POSITION_MASTER",
                PermissionCodes.OrgPositionMasterView, PermissionCodes.OrgPositionMasterManage,
                PermissionCodes.OrgPositionMasterCreate, PermissionCodes.OrgPositionMasterUpdate,
                PermissionCodes.OrgPositionMasterDeactivate, PermissionCodes.OrgPositionMasterActivate,
                PermissionCodes.OrgPositionMasterImportExcel, PermissionCodes.OrgPositionMasterExportExcel);
            OrgCrud("ORGANIZATION_PART",
                PermissionCodes.OrgPartView, PermissionCodes.OrgPartManage,
                PermissionCodes.OrgPartCreate, PermissionCodes.OrgPartUpdate,
                PermissionCodes.OrgPartDeactivate, PermissionCodes.OrgPartActivate,
                PermissionCodes.OrgPartImportExcel, PermissionCodes.OrgPartExportExcel);
            OrgCrud("ORGANIZATION_PART_MASTER",
                PermissionCodes.OrgPartMasterView, PermissionCodes.OrgPartMasterManage,
                PermissionCodes.OrgPartMasterCreate, PermissionCodes.OrgPartMasterUpdate,
                PermissionCodes.OrgPartMasterDeactivate, PermissionCodes.OrgPartMasterActivate,
                PermissionCodes.OrgPartMasterImportExcel, PermissionCodes.OrgPartMasterExportExcel);

            // HR 
            A(PermissionCodes.HrView, "Xem", "HUMAN_RESOURCE", "VIEW");
            A(PermissionCodes.HrManage, "Quản lý", "HUMAN_RESOURCE", "MANAGE");

            A(PermissionCodes.HrEmployeeView, "Xem", "HUMAN_RESOURCE_EMPLOYEE", "VIEW");
            A(PermissionCodes.HrEmployeeCreate, "Tạo mới", "HUMAN_RESOURCE_EMPLOYEE", "CREATE");
            A(PermissionCodes.HrEmployeeUpdate, "Cập nhật", "HUMAN_RESOURCE_EMPLOYEE", "UPDATE");
            A(PermissionCodes.HrEmployeeDeactivate, "Ngưng", "HUMAN_RESOURCE_EMPLOYEE", "DEACTIVATE");
            A(PermissionCodes.HrEmployeeActivate, "Kích hoạt", "HUMAN_RESOURCE_EMPLOYEE", "ACTIVATE");
            A(PermissionCodes.HrEmployeeImportExcel, "Import Excel", "HUMAN_RESOURCE_EMPLOYEE", "IMPORT_EXCEL");
            A(PermissionCodes.HrEmployeeExportExcel, "Export Excel", "HUMAN_RESOURCE_EMPLOYEE", "EXPORT_EXCEL");
            A(PermissionCodes.HrEmployeeManage, "Quản lý", "HUMAN_RESOURCE_EMPLOYEE", "MANAGE");

            A(PermissionCodes.HrContractTypeView, "Xem", "HUMAN_RESOURCE_CONTRACT_TYPE", "VIEW");
            A(PermissionCodes.HrContractTypeCreate, "Tạo mới", "HUMAN_RESOURCE_CONTRACT_TYPE", "CREATE");
            A(PermissionCodes.HrContractTypeUpdate, "Cập nhật", "HUMAN_RESOURCE_CONTRACT_TYPE", "UPDATE");
            A(PermissionCodes.HrContractTypeDeactivate, "Ngưng", "HUMAN_RESOURCE_CONTRACT_TYPE", "DEACTIVATE");
            A(PermissionCodes.HrContractTypeActivate, "Kích hoạt", "HUMAN_RESOURCE_CONTRACT_TYPE", "ACTIVATE");
            A(PermissionCodes.HrContractTypeImportExcel, "Import Excel", "HUMAN_RESOURCE_CONTRACT_TYPE", "IMPORT_EXCEL");
            A(PermissionCodes.HrContractTypeExportExcel, "Export Excel", "HUMAN_RESOURCE_CONTRACT_TYPE", "EXPORT_EXCEL");
            A(PermissionCodes.HrContractTypeManage, "Quản lý", "HUMAN_RESOURCE_CONTRACT_TYPE", "MANAGE");

            A(PermissionCodes.HrContractView, "Xem", "HUMAN_RESOURCE_CONTRACT", "VIEW");
            A(PermissionCodes.HrContractCreate, "Tạo mới", "HUMAN_RESOURCE_CONTRACT", "CREATE");
            A(PermissionCodes.HrContractUpdate, "Cập nhật", "HUMAN_RESOURCE_CONTRACT", "UPDATE");
            A(PermissionCodes.HrContractSign, "Ký", "HUMAN_RESOURCE_CONTRACT", "SIGN");
            A(PermissionCodes.HrContractTerminate, "Chấm dứt", "HUMAN_RESOURCE_CONTRACT", "TERMINATE");
            A(PermissionCodes.HrContractRenew, "Gia hạn", "HUMAN_RESOURCE_CONTRACT", "RENEW");
            A(PermissionCodes.HrContractManage, "Quản lý", "HUMAN_RESOURCE_CONTRACT", "MANAGE");

            A(PermissionCodes.HrReviewRenewalView, "Xem", "HUMAN_RESOURCE_REVIEW_RENEWAL", "VIEW");
            A(PermissionCodes.HrReviewRenewalCreate, "Tạo mới", "HUMAN_RESOURCE_REVIEW_RENEWAL", "CREATE");
            A(PermissionCodes.HrReviewRenewalUpdate, "Cập nhật", "HUMAN_RESOURCE_REVIEW_RENEWAL", "UPDATE");
            A(PermissionCodes.HrReviewRenewalApprove, "Duyệt", "HUMAN_RESOURCE_REVIEW_RENEWAL", "APPROVE");
            A(PermissionCodes.HrReviewRenewalReject, "Từ chối", "HUMAN_RESOURCE_REVIEW_RENEWAL", "REJECT");
            A(PermissionCodes.HrReviewRenewalApply, "Áp dụng", "HUMAN_RESOURCE_REVIEW_RENEWAL", "APPLY");
            A(PermissionCodes.HrReviewRenewalManage, "Quản lý", "HUMAN_RESOURCE_REVIEW_RENEWAL", "MANAGE");

            A(PermissionCodes.HrTransferView, "Xem", "HUMAN_RESOURCE_TRANSFER", "VIEW");
            A(PermissionCodes.HrTransferCreate, "Tạo mới", "HUMAN_RESOURCE_TRANSFER", "CREATE");
            A(PermissionCodes.HrTransferUpdate, "Cập nhật", "HUMAN_RESOURCE_TRANSFER", "UPDATE");
            A(PermissionCodes.HrTransferApprove, "Duyệt", "HUMAN_RESOURCE_TRANSFER", "APPROVE");
            A(PermissionCodes.HrTransferReject, "Từ chối", "HUMAN_RESOURCE_TRANSFER", "REJECT");
            A(PermissionCodes.HrTransferApply, "Áp dụng", "HUMAN_RESOURCE_TRANSFER", "APPLY");
            A(PermissionCodes.HrTransferCancel, "Hủy", "HUMAN_RESOURCE_TRANSFER", "CANCEL");
            A(PermissionCodes.HrTransferImportExcel, "Import Excel", "HUMAN_RESOURCE_TRANSFER", "IMPORT_EXCEL");
            A(PermissionCodes.HrTransferExportExcel, "Export Excel", "HUMAN_RESOURCE_TRANSFER", "EXPORT_EXCEL");
            A(PermissionCodes.HrTransferManage, "Quản lý", "HUMAN_RESOURCE_TRANSFER", "MANAGE");

            // OPERATE 
            A(PermissionCodes.OperateView, "Xem", "OPERATE", "VIEW");
            A(PermissionCodes.OperateManage, "Quản lý", "OPERATE", "MANAGE");

            A(PermissionCodes.OperateTimekeepingStandardView, "Xem", "OPERATE_TIMEKEEPING_STANDARD", "VIEW");
            A(PermissionCodes.OperateTimekeepingStandardCreate, "Tạo mới", "OPERATE_TIMEKEEPING_STANDARD", "CREATE");
            A(PermissionCodes.OperateTimekeepingStandardUpdate, "Cập nhật", "OPERATE_TIMEKEEPING_STANDARD", "UPDATE");
            A(PermissionCodes.OperateTimekeepingStandardDeactivate, "Ngưng", "OPERATE_TIMEKEEPING_STANDARD", "DEACTIVATE");
            A(PermissionCodes.OperateTimekeepingStandardActivate, "Kích hoạt", "OPERATE_TIMEKEEPING_STANDARD", "ACTIVATE");
            A(PermissionCodes.OperateTimekeepingStandardImportExcel, "Import Excel", "OPERATE_TIMEKEEPING_STANDARD", "IMPORT_EXCEL");
            A(PermissionCodes.OperateTimekeepingStandardExportExcel, "Export Excel", "OPERATE_TIMEKEEPING_STANDARD", "EXPORT_EXCEL");
            A(PermissionCodes.OperateTimekeepingStandardManage, "Quản lý", "OPERATE_TIMEKEEPING_STANDARD", "MANAGE");

            A(PermissionCodes.OperateTimekeepingView, "Xem", "OPERATE_TIMEKEEPING", "VIEW");
            A(PermissionCodes.OperateTimekeepingAdjust, "Điều chỉnh", "OPERATE_TIMEKEEPING", "ADJUST");
            A(PermissionCodes.OperateTimekeepingSummarize, "Tổng hợp", "OPERATE_TIMEKEEPING", "SUMMARIZE");
            A(PermissionCodes.OperateTimekeepingManage, "Quản lý", "OPERATE_TIMEKEEPING", "MANAGE");

            A(PermissionCodes.OperateAttendanceComplaintView, "Xem", "OPERATE_ATTENDANCE_COMPLAINT", "VIEW");
            A(PermissionCodes.OperateAttendanceComplaintCreate, "Tạo mới", "OPERATE_ATTENDANCE_COMPLAINT", "CREATE");
            A(PermissionCodes.OperateAttendanceComplaintReview, "Duyệt", "OPERATE_ATTENDANCE_COMPLAINT", "REVIEW");
            A(PermissionCodes.OperateAttendanceComplaintManage, "Quản lý", "OPERATE_ATTENDANCE_COMPLAINT", "MANAGE");
            A(PermissionCodes.OperateOvertimeView, "Xem", "OPERATE_OVERTIME", "VIEW");
            A(PermissionCodes.OperateOvertimeCreate, "Tạo mới", "OPERATE_OVERTIME", "CREATE");
            A(PermissionCodes.OperateOvertimeApprove, "Duyệt", "OPERATE_OVERTIME", "APPROVE");
            A(PermissionCodes.OperateOvertimeManage, "Quản lý", "OPERATE_OVERTIME", "MANAGE");

            A(PermissionCodes.OperateDayOffConfigView, "Xem", "OPERATE_DAY_OFF_CONFIG", "VIEW");
            A(PermissionCodes.OperateDayOffConfigCreate, "Tạo mới", "OPERATE_DAY_OFF_CONFIG", "CREATE");
            A(PermissionCodes.OperateDayOffConfigUpdate, "Cập nhật", "OPERATE_DAY_OFF_CONFIG", "UPDATE");
            A(PermissionCodes.OperateDayOffConfigDeactivate, "Ngưng", "OPERATE_DAY_OFF_CONFIG", "DEACTIVATE");
            A(PermissionCodes.OperateDayOffConfigActivate, "Kích hoạt", "OPERATE_DAY_OFF_CONFIG", "ACTIVATE");
            A(PermissionCodes.OperateDayOffConfigImportExcel, "Import Excel", "OPERATE_DAY_OFF_CONFIG", "IMPORT_EXCEL");
            A(PermissionCodes.OperateDayOffConfigExportExcel, "Export Excel", "OPERATE_DAY_OFF_CONFIG", "EXPORT_EXCEL");
            A(PermissionCodes.OperateDayOffConfigManage, "Quản lý", "OPERATE_DAY_OFF_CONFIG", "MANAGE");

            A(PermissionCodes.OperatePublicHolidayView, "Xem", "OPERATE_PUBLIC_HOLIDAY", "VIEW");
            A(PermissionCodes.OperatePublicHolidayCreate, "Tạo mới", "OPERATE_PUBLIC_HOLIDAY", "CREATE");
            A(PermissionCodes.OperatePublicHolidayUpdate, "Cập nhật", "OPERATE_PUBLIC_HOLIDAY", "UPDATE");
            A(PermissionCodes.OperatePublicHolidayDeactivate, "Ngưng", "OPERATE_PUBLIC_HOLIDAY", "DEACTIVATE");
            A(PermissionCodes.OperatePublicHolidayActivate, "Kích hoạt", "OPERATE_PUBLIC_HOLIDAY", "ACTIVATE");
            A(PermissionCodes.OperatePublicHolidayImportExcel, "Import Excel", "OPERATE_PUBLIC_HOLIDAY", "IMPORT_EXCEL");
            A(PermissionCodes.OperatePublicHolidayExportExcel, "Export Excel", "OPERATE_PUBLIC_HOLIDAY", "EXPORT_EXCEL");
            A(PermissionCodes.OperatePublicHolidayManage, "Quản lý", "OPERATE_PUBLIC_HOLIDAY", "MANAGE");

            A(PermissionCodes.OperateLeaveView, "Xem", "OPERATE_LEAVE", "VIEW");
            A(PermissionCodes.OperateLeaveCreate, "Tạo mới", "OPERATE_LEAVE", "CREATE");
            A(PermissionCodes.OperateLeaveApprove, "Duyệt", "OPERATE_LEAVE", "APPROVE");
            A(PermissionCodes.OperateLeaveReject, "Từ chối", "OPERATE_LEAVE", "REJECT");
            A(PermissionCodes.OperateLeaveCancel, "Hủy", "OPERATE_LEAVE", "CANCEL");
            A(PermissionCodes.OperateLeaveManage, "Quản lý", "OPERATE_LEAVE", "MANAGE");

            A(PermissionCodes.OperateLeaveAllocationView, "Xem", "OPERATE_LEAVE_ALLOCATION", "VIEW");
            A(PermissionCodes.OperateLeaveAllocationCreate, "Tạo mới", "OPERATE_LEAVE_ALLOCATION", "CREATE");
            A(PermissionCodes.OperateLeaveAllocationUpdate, "Cập nhật", "OPERATE_LEAVE_ALLOCATION", "UPDATE");
            A(PermissionCodes.OperateLeaveAllocationManage, "Quản lý", "OPERATE_LEAVE_ALLOCATION", "MANAGE");

            A(PermissionCodes.OperateShiftView, "Xem", "OPERATE_SHIFT", "VIEW");
            A(PermissionCodes.OperateShiftCreate, "Tạo mới", "OPERATE_SHIFT", "CREATE");
            A(PermissionCodes.OperateShiftUpdate, "Cập nhật", "OPERATE_SHIFT", "UPDATE");
            A(PermissionCodes.OperateShiftDeactivate, "Ngưng", "OPERATE_SHIFT", "DEACTIVATE");
            A(PermissionCodes.OperateShiftActivate, "Kích hoạt", "OPERATE_SHIFT", "ACTIVATE");
            A(PermissionCodes.OperateShiftImportExcel, "Import Excel", "OPERATE_SHIFT", "IMPORT_EXCEL");
            A(PermissionCodes.OperateShiftExportExcel, "Export Excel", "OPERATE_SHIFT", "EXPORT_EXCEL");
            A(PermissionCodes.OperateShiftManage, "Quản lý", "OPERATE_SHIFT", "MANAGE");

            A(PermissionCodes.OperateWorkScheduleView, "Xem", "OPERATE_WORK_SCHEDULE", "VIEW");
            A(PermissionCodes.OperateWorkScheduleCreate, "Tạo mới", "OPERATE_WORK_SCHEDULE", "CREATE");
            A(PermissionCodes.OperateWorkScheduleUpdate, "Cập nhật", "OPERATE_WORK_SCHEDULE", "UPDATE");
            A(PermissionCodes.OperateWorkScheduleDeactivate, "Ngưng", "OPERATE_WORK_SCHEDULE", "DEACTIVATE");
            A(PermissionCodes.OperateWorkScheduleImportExcel, "Import Excel", "OPERATE_WORK_SCHEDULE", "IMPORT_EXCEL");
            A(PermissionCodes.OperateWorkScheduleExportExcel, "Export Excel", "OPERATE_WORK_SCHEDULE", "EXPORT_EXCEL");
            A(PermissionCodes.OperateWorkScheduleManage, "Quản lý", "OPERATE_WORK_SCHEDULE", "MANAGE");

            A(PermissionCodes.OperateWorkPatternView, "Xem", "OPERATE_WORK_PATTERN", "VIEW");
            A(PermissionCodes.OperateWorkPatternCreate, "Tạo mới", "OPERATE_WORK_PATTERN", "CREATE");
            A(PermissionCodes.OperateWorkPatternUpdate, "Cập nhật", "OPERATE_WORK_PATTERN", "UPDATE");
            A(PermissionCodes.OperateWorkPatternDeactivate, "Ngưng", "OPERATE_WORK_PATTERN", "DEACTIVATE");
            A(PermissionCodes.OperateWorkPatternBulkAssign, "Gán hàng loạt", "OPERATE_WORK_PATTERN", "BULK_ASSIGN");
            A(PermissionCodes.OperateWorkPatternImportExcel, "Import Excel", "OPERATE_WORK_PATTERN", "IMPORT_EXCEL");
            A(PermissionCodes.OperateWorkPatternExportExcel, "Export Excel", "OPERATE_WORK_PATTERN", "EXPORT_EXCEL");
            A(PermissionCodes.OperateWorkPatternManage, "Quản lý", "OPERATE_WORK_PATTERN", "MANAGE");

            // DISCIPLINE
            A(PermissionCodes.DisciplineView, "Xem", "DISCIPLINE", "VIEW");
            A(PermissionCodes.DisciplineTypeView, "Xem", "DISCIPLINE_TYPE", "VIEW");
            A(PermissionCodes.DisciplineTypeManage, "Quản lý", "DISCIPLINE_TYPE", "MANAGE");
            A(PermissionCodes.DisciplineViolationView, "Xem", "DISCIPLINE_VIOLATION", "VIEW");
            A(PermissionCodes.DisciplineViolationCreate, "Tạo mới", "DISCIPLINE_VIOLATION", "CREATE");
            A(PermissionCodes.DisciplineViolationUpdate, "Cập nhật", "DISCIPLINE_VIOLATION", "UPDATE");
            A(PermissionCodes.DisciplineViolationApprove, "Duyệt", "DISCIPLINE_VIOLATION", "APPROVE");
            A(PermissionCodes.DisciplineViolationDelete, "Xóa", "DISCIPLINE_VIOLATION", "DELETE");

            // PERFORMANCE
            A(PermissionCodes.PerformanceView, "Xem", "PERFORMANCE", "VIEW");
            A(PermissionCodes.PerformanceCycleView, "Xem", "PERFORMANCE_CYCLE", "VIEW");
            A(PermissionCodes.PerformanceCycleCreate, "Tạo mới", "PERFORMANCE_CYCLE", "CREATE");
            A(PermissionCodes.PerformanceCycleUpdate, "Cập nhật", "PERFORMANCE_CYCLE", "UPDATE");
            A(PermissionCodes.PerformanceGoalView, "Xem", "PERFORMANCE_GOAL", "VIEW");
            A(PermissionCodes.PerformanceGoalManage, "Quản lý", "PERFORMANCE_GOAL", "MANAGE");
            A(PermissionCodes.PerformanceResultView, "Xem", "PERFORMANCE_RESULT", "VIEW");
            A(PermissionCodes.PerformanceResultManage, "Quản lý", "PERFORMANCE_RESULT", "MANAGE");
            A(PermissionCodes.PerformanceCompetencyView, "Xem", "PERFORMANCE_COMPETENCY", "VIEW");
            A(PermissionCodes.PerformanceCompetencyManage, "Quản lý", "PERFORMANCE_COMPETENCY", "MANAGE");
            A(PermissionCodes.Performance360View, "Xem", "PERFORMANCE_360", "VIEW");
            A(PermissionCodes.Performance360Manage, "Quản lý", "PERFORMANCE_360", "MANAGE");

            // TRAINING
            A(PermissionCodes.TrainingView, "Xem", "TRAINING", "VIEW");
            A(PermissionCodes.TrainingCourseView, "Xem", "TRAINING_COURSE", "VIEW");
            A(PermissionCodes.TrainingCourseCreate, "Tạo mới", "TRAINING_COURSE", "CREATE");
            A(PermissionCodes.TrainingCourseUpdate, "Cập nhật", "TRAINING_COURSE", "UPDATE");
            A(PermissionCodes.TrainingCourseDelete, "Xóa", "TRAINING_COURSE", "DELETE");
            A(PermissionCodes.TrainingEnrollmentView, "Xem", "TRAINING_ENROLLMENT", "VIEW");
            A(PermissionCodes.TrainingEnrollmentManage, "Quản lý", "TRAINING_ENROLLMENT", "MANAGE");
            A(PermissionCodes.TrainingResultView, "Xem", "TRAINING_RESULT", "VIEW");
            A(PermissionCodes.TrainingResultManage, "Quản lý", "TRAINING_RESULT", "MANAGE");

            // PAYROLL
            A(PermissionCodes.PayrollView, "Xem", "PAYROLL", "VIEW");
            A(PermissionCodes.PayrollManage, "Quản lý", "PAYROLL", "MANAGE");

            A(PermissionCodes.PayrollSalaryView, "Xem", "PAYROLL_SALARY", "VIEW");
            A(PermissionCodes.PayrollSalaryCreate, "Tạo mới", "PAYROLL_SALARY", "CREATE");
            A(PermissionCodes.PayrollSalaryUpdate, "Cập nhật", "PAYROLL_SALARY", "UPDATE");
            A(PermissionCodes.PayrollSalaryApprove, "Duyệt", "PAYROLL_SALARY", "APPROVE");
            A(PermissionCodes.PayrollSalaryMarkPaid, "Đánh dấu đã trả", "PAYROLL_SALARY", "MARK_PAID");
            A(PermissionCodes.PayrollSalaryCancel, "Hủy", "PAYROLL_SALARY", "CANCEL");
            A(PermissionCodes.PayrollSalaryManage, "Quản lý", "PAYROLL_SALARY", "MANAGE");

            A(PermissionCodes.PayrollConfigView, "Xem", "PAYROLL_CONFIG", "VIEW");
            A(PermissionCodes.PayrollConfigCreate, "Tạo mới", "PAYROLL_CONFIG", "CREATE");
            A(PermissionCodes.PayrollConfigUpdate, "Cập nhật", "PAYROLL_CONFIG", "UPDATE");
            A(PermissionCodes.PayrollConfigDeactivate, "Ngưng", "PAYROLL_CONFIG", "DEACTIVATE");
            A(PermissionCodes.PayrollConfigActivate, "Kích hoạt", "PAYROLL_CONFIG", "ACTIVATE");
            A(PermissionCodes.PayrollConfigManage, "Quản lý", "PAYROLL_CONFIG", "MANAGE");

            A(PermissionCodes.PayrollAllowanceView, "Xem", "PAYROLL_ALLOWANCE", "VIEW");
            A(PermissionCodes.PayrollAllowanceManage, "Quản lý", "PAYROLL_ALLOWANCE", "MANAGE");
            A(PermissionCodes.PayrollAdvanceView, "Xem", "PAYROLL_ADVANCE", "VIEW");
            A(PermissionCodes.PayrollAdvanceCreate, "Tạo mới", "PAYROLL_ADVANCE", "CREATE");
            A(PermissionCodes.PayrollAdvanceApprove, "Duyệt", "PAYROLL_ADVANCE", "APPROVE");
            A(PermissionCodes.PayrollAdvanceManage, "Quản lý", "PAYROLL_ADVANCE", "MANAGE");
            A(PermissionCodes.PayrollAdjustmentView, "Xem", "PAYROLL_ADJUSTMENT", "VIEW");
            A(PermissionCodes.PayrollAdjustmentManage, "Quản lý", "PAYROLL_ADJUSTMENT", "MANAGE");

            // RECRUITMENT
            A(PermissionCodes.RecruitmentView, "Xem", "RECRUITMENT", "VIEW");
            A(PermissionCodes.RecruitmentManage, "Quản lý", "RECRUITMENT", "MANAGE");
            A(PermissionCodes.RecruitmentHeadcountView, "Xem", "RECRUITMENT_HEADCOUNT", "VIEW");
            A(PermissionCodes.RecruitmentHeadcountUpdate, "Cập nhật", "RECRUITMENT_HEADCOUNT", "UPDATE");
            A(PermissionCodes.RecruitmentJdView, "Xem", "RECRUITMENT_JD", "VIEW");
            A(PermissionCodes.RecruitmentJdCreate, "Tạo mới", "RECRUITMENT_JD", "CREATE");
            A(PermissionCodes.RecruitmentJdUpdate, "Cập nhật", "RECRUITMENT_JD", "UPDATE");
            A(PermissionCodes.RecruitmentJdDelete, "Xóa", "RECRUITMENT_JD", "DELETE");
            A(PermissionCodes.RecruitmentCriteriaView, "Xem", "RECRUITMENT_CRITERIA", "VIEW");
            A(PermissionCodes.RecruitmentCriteriaManage, "Quản lý", "RECRUITMENT_CRITERIA", "MANAGE");
            A(PermissionCodes.RecruitmentSourceView, "Xem", "RECRUITMENT_SOURCE", "VIEW");
            A(PermissionCodes.RecruitmentSourceManage, "Quản lý", "RECRUITMENT_SOURCE", "MANAGE");
            A(PermissionCodes.RecruitmentRequestView, "Xem", "RECRUITMENT_REQUEST", "VIEW");
            A(PermissionCodes.RecruitmentRequestCreate, "Tạo mới", "RECRUITMENT_REQUEST", "CREATE");
            A(PermissionCodes.RecruitmentRequestUpdate, "Cập nhật", "RECRUITMENT_REQUEST", "UPDATE");
            A(PermissionCodes.RecruitmentRequestApprove, "Duyệt", "RECRUITMENT_REQUEST", "APPROVE");
            A(PermissionCodes.RecruitmentPlanView, "Xem", "RECRUITMENT_PLAN", "VIEW");
            A(PermissionCodes.RecruitmentPlanCreate, "Tạo mới", "RECRUITMENT_PLAN", "CREATE");
            A(PermissionCodes.RecruitmentPlanUpdate, "Cập nhật", "RECRUITMENT_PLAN", "UPDATE");
            A(PermissionCodes.RecruitmentCandidateView, "Xem", "RECRUITMENT_CANDIDATE", "VIEW");
            A(PermissionCodes.RecruitmentCandidateCreate, "Tạo mới", "RECRUITMENT_CANDIDATE", "CREATE");
            A(PermissionCodes.RecruitmentCandidateUpdate, "Cập nhật", "RECRUITMENT_CANDIDATE", "UPDATE");
            A(PermissionCodes.RecruitmentInterviewView, "Xem", "RECRUITMENT_INTERVIEW", "VIEW");
            A(PermissionCodes.RecruitmentInterviewManage, "Quản lý", "RECRUITMENT_INTERVIEW", "MANAGE");
            A(PermissionCodes.RecruitmentPipelineView, "Xem", "RECRUITMENT_PIPELINE", "VIEW");

            // ASSET
            A(PermissionCodes.AssetView, "Xem", "ASSET", "VIEW");
            A(PermissionCodes.AssetManage, "Quản lý", "ASSET", "MANAGE");

            A(PermissionCodes.AssetInventoryView, "Xem", "ASSET_INVENTORY", "VIEW");
            A(PermissionCodes.AssetInventoryCreate, "Tạo mới", "ASSET_INVENTORY", "CREATE");
            A(PermissionCodes.AssetInventoryUpdate, "Cập nhật", "ASSET_INVENTORY", "UPDATE");
            A(PermissionCodes.AssetInventoryImportExcel, "Import Excel", "ASSET_INVENTORY", "IMPORT_EXCEL");
            A(PermissionCodes.AssetInventoryExportExcel, "Export Excel", "ASSET_INVENTORY", "EXPORT_EXCEL");
            A(PermissionCodes.AssetInventoryManage, "Quản lý", "ASSET_INVENTORY", "MANAGE");

            A(PermissionCodes.AssetTypeView, "Xem", "ASSET_TYPE", "VIEW");
            A(PermissionCodes.AssetTypeCreate, "Tạo mới", "ASSET_TYPE", "CREATE");
            A(PermissionCodes.AssetTypeUpdate, "Cập nhật", "ASSET_TYPE", "UPDATE");
            A(PermissionCodes.AssetTypeImportExcel, "Import Excel", "ASSET_TYPE", "IMPORT_EXCEL");
            A(PermissionCodes.AssetTypeExportExcel, "Export Excel", "ASSET_TYPE", "EXPORT_EXCEL");
            A(PermissionCodes.AssetTypeManage, "Quản lý", "ASSET_TYPE", "MANAGE");

            A(PermissionCodes.AssetTicketView, "Xem", "ASSET_TICKET", "VIEW");
            A(PermissionCodes.AssetTicketCreate, "Tạo mới", "ASSET_TICKET", "CREATE");
            A(PermissionCodes.AssetTicketUpdate, "Cập nhật", "ASSET_TICKET", "UPDATE");
            A(PermissionCodes.AssetTicketComplete, "Hoàn tất", "ASSET_TICKET", "COMPLETE");
            A(PermissionCodes.AssetTicketCancel, "Hủy phiếu", "ASSET_TICKET", "CANCEL");
            A(PermissionCodes.AssetTicketImportExcel, "Import Excel", "ASSET_TICKET", "IMPORT_EXCEL");
            A(PermissionCodes.AssetTicketExportExcel, "Export Excel", "ASSET_TICKET", "EXPORT_EXCEL");
            A(PermissionCodes.AssetTicketManage, "Quản lý", "ASSET_TICKET", "MANAGE");

            // SYSTEM / ROLE
            A(PermissionCodes.RoleView, "Xem", "ROLE", "VIEW");
            A(PermissionCodes.RoleCreate, "Tạo mới", "ROLE", "CREATE");
            A(PermissionCodes.RoleUpdate, "Cập nhật", "ROLE", "UPDATE");
            A(PermissionCodes.RoleDelete, "Xóa", "ROLE", "DELETE");
            A(PermissionCodes.RoleManage, "Quản lý", "ROLE", "MANAGE");

            A(PermissionCodes.UserView, "Xem", "USER", "VIEW");
            A(PermissionCodes.UserCreate, "Tạo mới", "USER", "CREATE");
            A(PermissionCodes.UserUpdate, "Cập nhật", "USER", "UPDATE");
            A(PermissionCodes.UserDelete, "Xóa", "USER", "DELETE");
            A(PermissionCodes.UserResetPassword, "Đặt lại mật khẩu", "USER", "RESET_PASSWORD");
            A(PermissionCodes.UserManage, "Quản lý", "USER", "MANAGE");

            A(PermissionCodes.SystemSettingView, "Xem", "SYSTEM_SETTING", "VIEW", false);
            A(PermissionCodes.SystemSettingsView, "Xem cấu hình hệ thống", "SYSTEM_SETTINGS", "VIEW", false);
            A(PermissionCodes.SystemSettingsManage, "Quản lý cấu hình hệ thống", "SYSTEM_SETTINGS", "MANAGE", false);
            A(PermissionCodes.ReportScheduleView, "Xem lịch báo cáo", "REPORT_SCHEDULE", "VIEW", false);
            A(PermissionCodes.ReportScheduleManage, "Quản lý lịch báo cáo", "REPORT_SCHEDULE", "MANAGE", false);
            A(PermissionCodes.ComplianceView, "Xem tuân thủ", "COMPLIANCE", "VIEW", false);
            A(PermissionCodes.WorkflowView, "Xem workflow", "WORKFLOW", "VIEW", false);
            A(PermissionCodes.WorkflowManage, "Quản lý workflow", "WORKFLOW", "MANAGE", false);
            A(PermissionCodes.WorkflowInbox, "Hộp thư duyệt workflow", "WORKFLOW", "INBOX", false);
            A(PermissionCodes.ActionLogView, "Xem", "ACTION_LOG", "VIEW", false);
            A(PermissionCodes.MobileAccess, "Truy cập", "MOBILE", "ACCESS", false);

            return list;
        }

        private static string[] BuildHrCodes()
        {
            return [
            PermissionCodes.HomeView,
            PermissionCodes.OrgView, PermissionCodes.OrgManage,
            PermissionCodes.OrgCompanyView, PermissionCodes.OrgCompanyManage, PermissionCodes.OrgCompanyCreate, PermissionCodes.OrgCompanyUpdate, PermissionCodes.OrgCompanyActivate, PermissionCodes.OrgCompanyDeactivate, PermissionCodes.OrgCompanyImportExcel, PermissionCodes.OrgCompanyExportExcel,
            PermissionCodes.OrgBranchView, PermissionCodes.OrgBranchManage, PermissionCodes.OrgBranchCreate, PermissionCodes.OrgBranchUpdate, PermissionCodes.OrgBranchActivate, PermissionCodes.OrgBranchDeactivate, PermissionCodes.OrgBranchImportExcel, PermissionCodes.OrgBranchExportExcel,
            PermissionCodes.OrgDepartmentView, PermissionCodes.OrgDepartmentManage, PermissionCodes.OrgDepartmentCreate, PermissionCodes.OrgDepartmentUpdate, PermissionCodes.OrgDepartmentActivate, PermissionCodes.OrgDepartmentDeactivate, PermissionCodes.OrgDepartmentImportExcel, PermissionCodes.OrgDepartmentExportExcel,
            PermissionCodes.OrgPositionView, PermissionCodes.OrgPositionManage, PermissionCodes.OrgPositionCreate, PermissionCodes.OrgPositionUpdate, PermissionCodes.OrgPositionActivate, PermissionCodes.OrgPositionDeactivate, PermissionCodes.OrgPositionImportExcel, PermissionCodes.OrgPositionExportExcel,
            PermissionCodes.OrgPositionMasterView, PermissionCodes.OrgPositionMasterManage, PermissionCodes.OrgPositionMasterCreate, PermissionCodes.OrgPositionMasterUpdate, PermissionCodes.OrgPositionMasterActivate, PermissionCodes.OrgPositionMasterDeactivate, PermissionCodes.OrgPositionMasterImportExcel, PermissionCodes.OrgPositionMasterExportExcel,
            PermissionCodes.OrgPartView, PermissionCodes.OrgPartManage, PermissionCodes.OrgPartCreate, PermissionCodes.OrgPartUpdate, PermissionCodes.OrgPartActivate, PermissionCodes.OrgPartDeactivate, PermissionCodes.OrgPartImportExcel, PermissionCodes.OrgPartExportExcel,
            PermissionCodes.OrgPartMasterView, PermissionCodes.OrgPartMasterManage, PermissionCodes.OrgPartMasterCreate, PermissionCodes.OrgPartMasterUpdate, PermissionCodes.OrgPartMasterActivate, PermissionCodes.OrgPartMasterDeactivate, PermissionCodes.OrgPartMasterImportExcel, PermissionCodes.OrgPartMasterExportExcel,
            PermissionCodes.HrView, PermissionCodes.HrManage,
            PermissionCodes.HrEmployeeView, PermissionCodes.HrEmployeeManage, PermissionCodes.HrEmployeeCreate, PermissionCodes.HrEmployeeUpdate, PermissionCodes.HrEmployeeActivate, PermissionCodes.HrEmployeeDeactivate, PermissionCodes.HrEmployeeImportExcel, PermissionCodes.HrEmployeeExportExcel,
            PermissionCodes.HrContractTypeView, PermissionCodes.HrContractTypeManage, PermissionCodes.HrContractTypeCreate, PermissionCodes.HrContractTypeUpdate, PermissionCodes.HrContractTypeActivate, PermissionCodes.HrContractTypeDeactivate,
            PermissionCodes.HrContractView, PermissionCodes.HrContractManage, PermissionCodes.HrContractCreate, PermissionCodes.HrContractUpdate, PermissionCodes.HrContractSign, PermissionCodes.HrContractTerminate, PermissionCodes.HrContractRenew,
            PermissionCodes.HrReviewRenewalView, PermissionCodes.HrReviewRenewalManage, PermissionCodes.HrReviewRenewalCreate, PermissionCodes.HrReviewRenewalUpdate, PermissionCodes.HrReviewRenewalApprove, PermissionCodes.HrReviewRenewalReject, PermissionCodes.HrReviewRenewalApply,
            PermissionCodes.HrTransferView, PermissionCodes.HrTransferManage, PermissionCodes.HrTransferCreate, PermissionCodes.HrTransferUpdate, PermissionCodes.HrTransferApprove, PermissionCodes.HrTransferReject, PermissionCodes.HrTransferApply, PermissionCodes.HrTransferCancel,
            PermissionCodes.OperateView, PermissionCodes.OperateManage,
            PermissionCodes.OperateTimekeepingStandardView, PermissionCodes.OperateTimekeepingStandardManage, PermissionCodes.OperateTimekeepingStandardCreate, PermissionCodes.OperateTimekeepingStandardUpdate, PermissionCodes.OperateTimekeepingStandardActivate, PermissionCodes.OperateTimekeepingStandardDeactivate,
            PermissionCodes.OperateTimekeepingView, PermissionCodes.OperateTimekeepingManage, PermissionCodes.OperateTimekeepingAdjust, PermissionCodes.OperateTimekeepingSummarize,
            PermissionCodes.OperateAttendanceComplaintView, PermissionCodes.OperateAttendanceComplaintManage, PermissionCodes.OperateAttendanceComplaintReview,
            PermissionCodes.OperateOvertimeView, PermissionCodes.OperateOvertimeCreate, PermissionCodes.OperateOvertimeApprove, PermissionCodes.OperateOvertimeManage,
            PermissionCodes.OperateDayOffConfigView, PermissionCodes.OperateDayOffConfigManage, PermissionCodes.OperateDayOffConfigCreate, PermissionCodes.OperateDayOffConfigUpdate, PermissionCodes.OperateDayOffConfigActivate, PermissionCodes.OperateDayOffConfigDeactivate,
            PermissionCodes.OperatePublicHolidayView, PermissionCodes.OperatePublicHolidayManage, PermissionCodes.OperatePublicHolidayCreate, PermissionCodes.OperatePublicHolidayUpdate, PermissionCodes.OperatePublicHolidayActivate, PermissionCodes.OperatePublicHolidayDeactivate,
            PermissionCodes.OperateLeaveView, PermissionCodes.OperateLeaveManage, PermissionCodes.OperateLeaveCreate, PermissionCodes.OperateLeaveApprove, PermissionCodes.OperateLeaveReject, PermissionCodes.OperateLeaveCancel,
            PermissionCodes.OperateLeaveAllocationView, PermissionCodes.OperateLeaveAllocationManage, PermissionCodes.OperateLeaveAllocationCreate, PermissionCodes.OperateLeaveAllocationUpdate,
            PermissionCodes.OperateShiftView, PermissionCodes.OperateShiftManage, PermissionCodes.OperateShiftCreate, PermissionCodes.OperateShiftUpdate, PermissionCodes.OperateShiftActivate, PermissionCodes.OperateShiftDeactivate,
            PermissionCodes.OperateWorkScheduleView, PermissionCodes.OperateWorkScheduleManage, PermissionCodes.OperateWorkScheduleCreate, PermissionCodes.OperateWorkScheduleUpdate, PermissionCodes.OperateWorkScheduleDeactivate,
            PermissionCodes.OperateWorkPatternView, PermissionCodes.OperateWorkPatternManage, PermissionCodes.OperateWorkPatternCreate, PermissionCodes.OperateWorkPatternUpdate, PermissionCodes.OperateWorkPatternDeactivate, PermissionCodes.OperateWorkPatternBulkAssign,
            PermissionCodes.PayrollView, PermissionCodes.PayrollManage,
            PermissionCodes.PayrollSalaryView, PermissionCodes.PayrollSalaryManage, PermissionCodes.PayrollSalaryCreate, PermissionCodes.PayrollSalaryUpdate, PermissionCodes.PayrollSalaryApprove, PermissionCodes.PayrollSalaryMarkPaid, PermissionCodes.PayrollSalaryCancel,
            PermissionCodes.PayrollConfigView, PermissionCodes.PayrollConfigManage, PermissionCodes.PayrollConfigCreate, PermissionCodes.PayrollConfigUpdate, PermissionCodes.PayrollConfigActivate, PermissionCodes.PayrollConfigDeactivate,
            PermissionCodes.PayrollAllowanceView, PermissionCodes.PayrollAllowanceManage,
            PermissionCodes.PayrollAdvanceView, PermissionCodes.PayrollAdvanceCreate, PermissionCodes.PayrollAdvanceApprove, PermissionCodes.PayrollAdvanceManage,
            PermissionCodes.PayrollAdjustmentView, PermissionCodes.PayrollAdjustmentManage,
            PermissionCodes.RecruitmentView, PermissionCodes.RecruitmentManage,
            PermissionCodes.RecruitmentHeadcountView, PermissionCodes.RecruitmentHeadcountUpdate,
            PermissionCodes.RecruitmentJdView, PermissionCodes.RecruitmentJdCreate, PermissionCodes.RecruitmentJdUpdate, PermissionCodes.RecruitmentJdDelete,
            PermissionCodes.RecruitmentCriteriaView, PermissionCodes.RecruitmentCriteriaManage,
            PermissionCodes.RecruitmentSourceView, PermissionCodes.RecruitmentSourceManage,
            PermissionCodes.RecruitmentRequestView, PermissionCodes.RecruitmentRequestCreate, PermissionCodes.RecruitmentRequestUpdate, PermissionCodes.RecruitmentRequestApprove,
            PermissionCodes.RecruitmentPlanView, PermissionCodes.RecruitmentPlanCreate, PermissionCodes.RecruitmentPlanUpdate,
            PermissionCodes.RecruitmentCandidateView, PermissionCodes.RecruitmentCandidateCreate, PermissionCodes.RecruitmentCandidateUpdate,
            PermissionCodes.RecruitmentInterviewView, PermissionCodes.RecruitmentInterviewManage,
            PermissionCodes.RecruitmentPipelineView,
            PermissionCodes.DisciplineView,
            PermissionCodes.DisciplineTypeView, PermissionCodes.DisciplineTypeManage,
            PermissionCodes.DisciplineViolationView, PermissionCodes.DisciplineViolationCreate, PermissionCodes.DisciplineViolationUpdate, PermissionCodes.DisciplineViolationApprove, PermissionCodes.DisciplineViolationDelete,
            PermissionCodes.PerformanceView,
            PermissionCodes.PerformanceCycleView, PermissionCodes.PerformanceCycleCreate, PermissionCodes.PerformanceCycleUpdate,
            PermissionCodes.PerformanceGoalView, PermissionCodes.PerformanceGoalManage,
            PermissionCodes.PerformanceResultView, PermissionCodes.PerformanceResultManage,
            PermissionCodes.PerformanceCompetencyView, PermissionCodes.PerformanceCompetencyManage,
            PermissionCodes.Performance360View, PermissionCodes.Performance360Manage,
            PermissionCodes.TrainingView,
            PermissionCodes.TrainingCourseView, PermissionCodes.TrainingCourseCreate, PermissionCodes.TrainingCourseUpdate, PermissionCodes.TrainingCourseDelete,
            PermissionCodes.TrainingEnrollmentView, PermissionCodes.TrainingEnrollmentManage,
            PermissionCodes.TrainingResultView, PermissionCodes.TrainingResultManage,
            PermissionCodes.UserView,
            PermissionCodes.ActionLogView,
            PermissionCodes.SystemSettingsView, PermissionCodes.SystemSettingsManage,
            PermissionCodes.ReportScheduleView, PermissionCodes.ReportScheduleManage,
            PermissionCodes.ComplianceView,
            PermissionCodes.WorkflowView, PermissionCodes.WorkflowManage, PermissionCodes.WorkflowInbox,
            PermissionCodes.MobileAccess,
        ];
        }

        private static string[] BuildManagerCodes()
        {
            return [
            PermissionCodes.HomeView,
            PermissionCodes.HrEmployeeView,
            PermissionCodes.OperateTimekeepingView,
            PermissionCodes.OperateAttendanceComplaintView,
            PermissionCodes.OperateOvertimeView,
            PermissionCodes.OperateOvertimeApprove,
            PermissionCodes.OperateLeaveView,
            PermissionCodes.OperateLeaveApprove,
            PermissionCodes.OperateLeaveReject,
            PermissionCodes.WorkflowView, PermissionCodes.WorkflowInbox,
            PermissionCodes.MobileAccess,
        ];
        }

        private static string[] BuildEmployeeCodes()
        {
            return [
            PermissionCodes.HomeView,
            PermissionCodes.MobileAccess,
            PermissionCodes.OperateTimekeepingView,
            PermissionCodes.OperateLeaveView,
            PermissionCodes.OperateLeaveCreate,
            PermissionCodes.PayrollSalaryView,
            PermissionCodes.OperateAttendanceComplaintCreate,
            PermissionCodes.OperateOvertimeView,
            PermissionCodes.OperateOvertimeCreate,
            PermissionCodes.WorkflowInbox,
        ];
        }
    }
}
