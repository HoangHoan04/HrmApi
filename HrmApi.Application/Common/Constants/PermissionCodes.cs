using System;
using System.Linq;
using System.Reflection;

namespace HrmApi.Application.Common.Constants
{
    public static class PermissionCodes
    {
        // ── HOME ──────────────────────────────────────────────
        public const string HomeView = "HOME_VIEW";

        // ── ORGANIZATION ───────────────────────────────
        public const string OrgView = "ORGANIZATION_VIEW";
        public const string OrgManage = "ORGANIZATION_MANAGE";

        // ORGANIZATION - COMPANY
        public const string OrgCompanyView = "ORGANIZATION_COMPANY_VIEW";
        public const string OrgCompanyManage = "ORGANIZATION_COMPANY_MANAGE";
        public const string OrgCompanyCreate = "ORGANIZATION_COMPANY_CREATE";
        public const string OrgCompanyUpdate = "ORGANIZATION_COMPANY_UPDATE";
        public const string OrgCompanyDeactivate = "ORGANIZATION_COMPANY_DEACTIVATE";
        public const string OrgCompanyActivate = "ORGANIZATION_COMPANY_ACTIVATE";
        public const string OrgCompanyImportExcel = "ORGANIZATION_COMPANY_IMPORT_EXCEL";
        public const string OrgCompanyExportExcel = "ORGANIZATION_COMPANY_EXPORT_EXCEL";

        // ORGANIZATION - BRANCH
        public const string OrgBranchView = "ORGANIZATION_BRANCH_VIEW";
        public const string OrgBranchManage = "ORGANIZATION_BRANCH_MANAGE";
        public const string OrgBranchCreate = "ORGANIZATION_BRANCH_CREATE";
        public const string OrgBranchUpdate = "ORGANIZATION_BRANCH_UPDATE";
        public const string OrgBranchDeactivate = "ORGANIZATION_BRANCH_DEACTIVATE";
        public const string OrgBranchActivate = "ORGANIZATION_BRANCH_ACTIVATE";
        public const string OrgBranchImportExcel = "ORGANIZATION_BRANCH_IMPORT_EXCEL";
        public const string OrgBranchExportExcel = "ORGANIZATION_BRANCH_EXPORT_EXCEL";

        // ORGANIZATION - DEPARTMENT
        public const string OrgDepartmentView = "ORGANIZATION_DEPARTMENT_VIEW";
        public const string OrgDepartmentManage = "ORGANIZATION_DEPARTMENT_MANAGE";
        public const string OrgDepartmentCreate = "ORGANIZATION_DEPARTMENT_CREATE";
        public const string OrgDepartmentUpdate = "ORGANIZATION_DEPARTMENT_UPDATE";
        public const string OrgDepartmentDeactivate = "ORGANIZATION_DEPARTMENT_DEACTIVATE";
        public const string OrgDepartmentActivate = "ORGANIZATION_DEPARTMENT_ACTIVATE";
        public const string OrgDepartmentImportExcel = "ORGANIZATION_DEPARTMENT_IMPORT_EXCEL";
        public const string OrgDepartmentExportExcel = "ORGANIZATION_DEPARTMENT_EXPORT_EXCEL";

        // ORGANIZATION - POSITION
        public const string OrgPositionView = "ORGANIZATION_POSITION_VIEW";
        public const string OrgPositionManage = "ORGANIZATION_POSITION_MANAGE";
        public const string OrgPositionCreate = "ORGANIZATION_POSITION_CREATE";
        public const string OrgPositionUpdate = "ORGANIZATION_POSITION_UPDATE";
        public const string OrgPositionDeactivate = "ORGANIZATION_POSITION_DEACTIVATE";
        public const string OrgPositionActivate = "ORGANIZATION_POSITION_ACTIVATE";
        public const string OrgPositionImportExcel = "ORGANIZATION_POSITION_IMPORT_EXCEL";
        public const string OrgPositionExportExcel = "ORGANIZATION_POSITION_EXPORT_EXCEL";

        // ORGANIZATION - POSITION_MASTER
        public const string OrgPositionMasterView = "ORGANIZATION_POSITION_MASTER_VIEW";
        public const string OrgPositionMasterManage = "ORGANIZATION_POSITION_MASTER_MANAGE";
        public const string OrgPositionMasterCreate = "ORGANIZATION_POSITION_MASTER_CREATE";
        public const string OrgPositionMasterUpdate = "ORGANIZATION_POSITION_MASTER_UPDATE";
        public const string OrgPositionMasterDeactivate = "ORGANIZATION_POSITION_MASTER_DEACTIVATE";
        public const string OrgPositionMasterActivate = "ORGANIZATION_POSITION_MASTER_ACTIVATE";
        public const string OrgPositionMasterImportExcel = "ORGANIZATION_POSITION_MASTER_IMPORT_EXCEL";
        public const string OrgPositionMasterExportExcel = "ORGANIZATION_POSITION_MASTER_EXPORT_EXCEL";

        // ORGANIZATION - PART
        public const string OrgPartView = "ORGANIZATION_PART_VIEW";
        public const string OrgPartManage = "ORGANIZATION_PART_MANAGE";
        public const string OrgPartCreate = "ORGANIZATION_PART_CREATE";
        public const string OrgPartUpdate = "ORGANIZATION_PART_UPDATE";
        public const string OrgPartDeactivate = "ORGANIZATION_PART_DEACTIVATE";
        public const string OrgPartActivate = "ORGANIZATION_PART_ACTIVATE";
        public const string OrgPartImportExcel = "ORGANIZATION_PART_IMPORT_EXCEL";
        public const string OrgPartExportExcel = "ORGANIZATION_PART_EXPORT_EXCEL";

        // ORGANIZATION - PART_MASTER
        public const string OrgPartMasterView = "ORGANIZATION_PART_MASTER_VIEW";
        public const string OrgPartMasterManage = "ORGANIZATION_PART_MASTER_MANAGE";
        public const string OrgPartMasterCreate = "ORGANIZATION_PART_MASTER_CREATE";
        public const string OrgPartMasterUpdate = "ORGANIZATION_PART_MASTER_UPDATE";
        public const string OrgPartMasterDeactivate = "ORGANIZATION_PART_MASTER_DEACTIVATE";
        public const string OrgPartMasterActivate = "ORGANIZATION_PART_MASTER_ACTIVATE";
        public const string OrgPartMasterImportExcel = "ORGANIZATION_PART_MASTER_IMPORT_EXCEL";
        public const string OrgPartMasterExportExcel = "ORGANIZATION_PART_MASTER_EXPORT_EXCEL";

        // ── HUMAN RESOURCE ────────────────────────────
        public const string HrView = "HUMAN_RESOURCE_VIEW";
        public const string HrManage = "HUMAN_RESOURCE_MANAGE";

        // HR - EMPLOYEE
        public const string HrEmployeeView = "HUMAN_RESOURCE_EMPLOYEE_VIEW";
        public const string HrEmployeeManage = "HUMAN_RESOURCE_EMPLOYEE_MANAGE";
        public const string HrEmployeeCreate = "HUMAN_RESOURCE_EMPLOYEE_CREATE";
        public const string HrEmployeeUpdate = "HUMAN_RESOURCE_EMPLOYEE_UPDATE";
        public const string HrEmployeeDeactivate = "HUMAN_RESOURCE_EMPLOYEE_DEACTIVATE";
        public const string HrEmployeeActivate = "HUMAN_RESOURCE_EMPLOYEE_ACTIVATE";
        public const string HrEmployeeImportExcel = "HUMAN_RESOURCE_EMPLOYEE_IMPORT_EXCEL";
        public const string HrEmployeeExportExcel = "HUMAN_RESOURCE_EMPLOYEE_EXPORT_EXCEL";

        // HR - CONTRACT_TYPE
        public const string HrContractTypeView = "HUMAN_RESOURCE_CONTRACT_TYPE_VIEW";
        public const string HrContractTypeManage = "HUMAN_RESOURCE_CONTRACT_TYPE_MANAGE";
        public const string HrContractTypeCreate = "HUMAN_RESOURCE_CONTRACT_TYPE_CREATE";
        public const string HrContractTypeUpdate = "HUMAN_RESOURCE_CONTRACT_TYPE_UPDATE";
        public const string HrContractTypeDeactivate = "HUMAN_RESOURCE_CONTRACT_TYPE_DEACTIVATE";
        public const string HrContractTypeActivate = "HUMAN_RESOURCE_CONTRACT_TYPE_ACTIVATE";

        // HR - CONTRACT
        public const string HrContractView = "HUMAN_RESOURCE_CONTRACT_VIEW";
        public const string HrContractManage = "HUMAN_RESOURCE_CONTRACT_MANAGE";
        public const string HrContractCreate = "HUMAN_RESOURCE_CONTRACT_CREATE";
        public const string HrContractUpdate = "HUMAN_RESOURCE_CONTRACT_UPDATE";
        public const string HrContractSign = "HUMAN_RESOURCE_CONTRACT_SIGN";
        public const string HrContractTerminate = "HUMAN_RESOURCE_CONTRACT_TERMINATE";
        public const string HrContractRenew = "HUMAN_RESOURCE_CONTRACT_RENEW";

        // HR - REVIEW_RENEWAL
        public const string HrReviewRenewalView = "HUMAN_RESOURCE_REVIEW_RENEWAL_VIEW";
        public const string HrReviewRenewalManage = "HUMAN_RESOURCE_REVIEW_RENEWAL_MANAGE";
        public const string HrReviewRenewalCreate = "HUMAN_RESOURCE_REVIEW_RENEWAL_CREATE";
        public const string HrReviewRenewalUpdate = "HUMAN_RESOURCE_REVIEW_RENEWAL_UPDATE";
        public const string HrReviewRenewalApprove = "HUMAN_RESOURCE_REVIEW_RENEWAL_APPROVE";
        public const string HrReviewRenewalReject = "HUMAN_RESOURCE_REVIEW_RENEWAL_REJECT";
        public const string HrReviewRenewalApply = "HUMAN_RESOURCE_REVIEW_RENEWAL_APPLY";

        // HR - TRANSFER
        public const string HrTransferView = "HUMAN_RESOURCE_TRANSFER_VIEW";
        public const string HrTransferManage = "HUMAN_RESOURCE_TRANSFER_MANAGE";
        public const string HrTransferCreate = "HUMAN_RESOURCE_TRANSFER_CREATE";
        public const string HrTransferUpdate = "HUMAN_RESOURCE_TRANSFER_UPDATE";
        public const string HrTransferApprove = "HUMAN_RESOURCE_TRANSFER_APPROVE";
        public const string HrTransferReject = "HUMAN_RESOURCE_TRANSFER_REJECT";
        public const string HrTransferApply = "HUMAN_RESOURCE_TRANSFER_APPLY";
        public const string HrTransferCancel = "HUMAN_RESOURCE_TRANSFER_CANCEL";

        // ── OPERATE ───────────────────────────────────
        public const string OperateView = "OPERATE_VIEW";
        public const string OperateManage = "OPERATE_MANAGE";

        // OPERATE - TIMEKEEPING_STANDARD
        public const string OperateTimekeepingStandardView = "OPERATE_TIMEKEEPING_STANDARD_VIEW";
        public const string OperateTimekeepingStandardManage = "OPERATE_TIMEKEEPING_STANDARD_MANAGE";
        public const string OperateTimekeepingStandardCreate = "OPERATE_TIMEKEEPING_STANDARD_CREATE";
        public const string OperateTimekeepingStandardUpdate = "OPERATE_TIMEKEEPING_STANDARD_UPDATE";
        public const string OperateTimekeepingStandardDeactivate = "OPERATE_TIMEKEEPING_STANDARD_DEACTIVATE";
        public const string OperateTimekeepingStandardActivate = "OPERATE_TIMEKEEPING_STANDARD_ACTIVATE";

        // OPERATE - TIMEKEEPING
        public const string OperateTimekeepingView = "OPERATE_TIMEKEEPING_VIEW";
        public const string OperateTimekeepingManage = "OPERATE_TIMEKEEPING_MANAGE";
        public const string OperateTimekeepingAdjust = "OPERATE_TIMEKEEPING_ADJUST";
        public const string OperateTimekeepingSummarize = "OPERATE_TIMEKEEPING_SUMMARIZE";

        // OPERATE - ATTENDANCE_COMPLAINT
        public const string OperateAttendanceComplaintView = "OPERATE_ATTENDANCE_COMPLAINT_VIEW";
        public const string OperateAttendanceComplaintManage = "OPERATE_ATTENDANCE_COMPLAINT_MANAGE";
        public const string OperateAttendanceComplaintCreate = "OPERATE_ATTENDANCE_COMPLAINT_CREATE";
        public const string OperateAttendanceComplaintReview = "OPERATE_ATTENDANCE_COMPLAINT_REVIEW";

        // OPERATE - DAY_OFF_CONFIG
        public const string OperateDayOffConfigView = "OPERATE_DAY_OFF_CONFIG_VIEW";
        public const string OperateDayOffConfigManage = "OPERATE_DAY_OFF_CONFIG_MANAGE";
        public const string OperateDayOffConfigCreate = "OPERATE_DAY_OFF_CONFIG_CREATE";
        public const string OperateDayOffConfigUpdate = "OPERATE_DAY_OFF_CONFIG_UPDATE";
        public const string OperateDayOffConfigDeactivate = "OPERATE_DAY_OFF_CONFIG_DEACTIVATE";
        public const string OperateDayOffConfigActivate = "OPERATE_DAY_OFF_CONFIG_ACTIVATE";

        // OPERATE - PUBLIC_HOLIDAY
        public const string OperatePublicHolidayView = "OPERATE_PUBLIC_HOLIDAY_VIEW";
        public const string OperatePublicHolidayManage = "OPERATE_PUBLIC_HOLIDAY_MANAGE";
        public const string OperatePublicHolidayCreate = "OPERATE_PUBLIC_HOLIDAY_CREATE";
        public const string OperatePublicHolidayUpdate = "OPERATE_PUBLIC_HOLIDAY_UPDATE";
        public const string OperatePublicHolidayDeactivate = "OPERATE_PUBLIC_HOLIDAY_DEACTIVATE";
        public const string OperatePublicHolidayActivate = "OPERATE_PUBLIC_HOLIDAY_ACTIVATE";

        // OPERATE - LEAVE
        public const string OperateLeaveView = "OPERATE_LEAVE_VIEW";
        public const string OperateLeaveManage = "OPERATE_LEAVE_MANAGE";
        public const string OperateLeaveCreate = "OPERATE_LEAVE_CREATE";
        public const string OperateLeaveApprove = "OPERATE_LEAVE_APPROVE";
        public const string OperateLeaveReject = "OPERATE_LEAVE_REJECT";
        public const string OperateLeaveCancel = "OPERATE_LEAVE_CANCEL";

        // OPERATE - LEAVE_ALLOCATION
        public const string OperateLeaveAllocationView = "OPERATE_LEAVE_ALLOCATION_VIEW";
        public const string OperateLeaveAllocationManage = "OPERATE_LEAVE_ALLOCATION_MANAGE";
        public const string OperateLeaveAllocationCreate = "OPERATE_LEAVE_ALLOCATION_CREATE";
        public const string OperateLeaveAllocationUpdate = "OPERATE_LEAVE_ALLOCATION_UPDATE";

        // OPERATE - SHIFT
        public const string OperateShiftView = "OPERATE_SHIFT_VIEW";
        public const string OperateShiftManage = "OPERATE_SHIFT_MANAGE";
        public const string OperateShiftCreate = "OPERATE_SHIFT_CREATE";
        public const string OperateShiftUpdate = "OPERATE_SHIFT_UPDATE";
        public const string OperateShiftDeactivate = "OPERATE_SHIFT_DEACTIVATE";
        public const string OperateShiftActivate = "OPERATE_SHIFT_ACTIVATE";

        // OPERATE - WORK_SCHEDULE
        public const string OperateWorkScheduleView = "OPERATE_WORK_SCHEDULE_VIEW";
        public const string OperateWorkScheduleManage = "OPERATE_WORK_SCHEDULE_MANAGE";
        public const string OperateWorkScheduleCreate = "OPERATE_WORK_SCHEDULE_CREATE";
        public const string OperateWorkScheduleUpdate = "OPERATE_WORK_SCHEDULE_UPDATE";
        public const string OperateWorkScheduleDeactivate = "OPERATE_WORK_SCHEDULE_DEACTIVATE";

        // OPERATE - WORK_PATTERN
        public const string OperateWorkPatternView = "OPERATE_WORK_PATTERN_VIEW";
        public const string OperateWorkPatternManage = "OPERATE_WORK_PATTERN_MANAGE";
        public const string OperateWorkPatternCreate = "OPERATE_WORK_PATTERN_CREATE";
        public const string OperateWorkPatternUpdate = "OPERATE_WORK_PATTERN_UPDATE";
        public const string OperateWorkPatternDeactivate = "OPERATE_WORK_PATTERN_DEACTIVATE";
        public const string OperateWorkPatternBulkAssign = "OPERATE_WORK_PATTERN_BULK_ASSIGN";

        // ── PAYROLL ───────────────────────────────────────────
        public const string PayrollView = "PAYROLL_VIEW";
        public const string PayrollManage = "PAYROLL_MANAGE";

        // PAYROLL - SALARY RUN
        public const string PayrollSalaryView = "PAYROLL_SALARY_VIEW";
        public const string PayrollSalaryManage = "PAYROLL_SALARY_MANAGE";
        public const string PayrollSalaryCreate = "PAYROLL_SALARY_CREATE";
        public const string PayrollSalaryUpdate = "PAYROLL_SALARY_UPDATE";
        public const string PayrollSalaryApprove = "PAYROLL_SALARY_APPROVE";
        public const string PayrollSalaryMarkPaid = "PAYROLL_SALARY_MARK_PAID";
        public const string PayrollSalaryCancel = "PAYROLL_SALARY_CANCEL";

        // PAYROLL - CONFIG
        public const string PayrollConfigView = "PAYROLL_CONFIG_VIEW";
        public const string PayrollConfigManage = "PAYROLL_CONFIG_MANAGE";
        public const string PayrollConfigCreate = "PAYROLL_CONFIG_CREATE";
        public const string PayrollConfigUpdate = "PAYROLL_CONFIG_UPDATE";
        public const string PayrollConfigDeactivate = "PAYROLL_CONFIG_DEACTIVATE";
        public const string PayrollConfigActivate = "PAYROLL_CONFIG_ACTIVATE";

        // ── SYSTEM / ROLE MANAGER ─────────────────────────────
        public const string RoleView = "ROLE_VIEW";
        public const string RoleManage = "ROLE_MANAGE";
        public const string RoleCreate = "ROLE_CREATE";
        public const string RoleUpdate = "ROLE_UPDATE";
        public const string RoleDelete = "ROLE_DELETE";

        public const string UserView = "USER_VIEW";
        public const string UserManage = "USER_MANAGE";
        public const string UserCreate = "USER_CREATE";
        public const string UserUpdate = "USER_UPDATE";
        public const string UserDelete = "USER_DELETE";
        public const string UserResetPassword = "USER_RESET_PASSWORD";

        public const string ActionLogView = "ACTION_LOG_VIEW";

        // ── MOBILE ────────────────────────────────────────────
        public const string MobileAccess = "MOBILE_ACCESS";

        [Obsolete("Use OperateTimekeepingView")]
        public const string TimekeepingView = OperateTimekeepingView;
        [Obsolete("Use OperateTimekeepingAdjust")]
        public const string TimekeepingAdjust = OperateTimekeepingAdjust;
        [Obsolete("Use OperateTimekeepingManage")]
        public const string TimekeepingManage = OperateTimekeepingManage;
        [Obsolete("Use OperateShiftView")]
        public const string ShiftView = OperateShiftView;
        [Obsolete("Use OperateShiftCreate")]
        public const string ShiftCreate = OperateShiftCreate;
        [Obsolete("Use OperateShiftUpdate")]
        public const string ShiftUpdate = OperateShiftUpdate;
        [Obsolete("Use OperateShiftManage")]
        public const string ShiftManage = OperateShiftManage;
        [Obsolete("Use OperateLeaveView")]
        public const string LeaveView = OperateLeaveView;
        [Obsolete("Use OperateLeaveCreate")]
        public const string LeaveCreate = OperateLeaveCreate;
        [Obsolete("Use OperateLeaveApprove")]
        public const string LeaveApprove = OperateLeaveApprove;
        [Obsolete("Use OperateLeaveManage")]
        public const string LeaveManage = OperateLeaveManage;
        [Obsolete("Use OperateAttendanceComplaintView")]
        public const string AttendanceComplaintView = OperateAttendanceComplaintView;
        [Obsolete("Use OperateAttendanceComplaintCreate")]
        public const string AttendanceComplaintCreate = OperateAttendanceComplaintCreate;
        [Obsolete("Use OperateAttendanceComplaintReview")]
        public const string AttendanceComplaintReview = OperateAttendanceComplaintReview;
        [Obsolete("Use HrContractView")]
        public const string ContractView = HrContractView;
        [Obsolete("Use HrContractCreate")]
        public const string ContractCreate = HrContractCreate;
        [Obsolete("Use HrContractUpdate")]
        public const string ContractUpdate = HrContractUpdate;
        [Obsolete("Use HrContractManage")]
        public const string ContractManage = HrContractManage;
        [Obsolete("Use PayrollSalaryCreate")]
        public const string PayrollCreate = PayrollSalaryCreate;

        public static readonly string[] All = typeof(PermissionCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Where(f => f.GetCustomAttribute<ObsoleteAttribute>() == null)
            .Select(f => (string)f.GetRawConstantValue()!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
    }

    public static class RoleCodes
    {
        public const string Admin = "ADMIN";
        public const string Hr = "HR";
        public const string Manager = "MANAGER";
        public const string Employee = "EMPLOYEE";
    }

    public static class DataScopes
    {
        public const string All = "ALL";
        public const string Branch = "BRANCH";
        public const string Department = "DEPARTMENT";
        public const string Own = "OWN";
    }

    public static class ClaimTypesEx
    {
        public const string Permission = "permission";
        public const string UserType = "UserType";
    }
}
