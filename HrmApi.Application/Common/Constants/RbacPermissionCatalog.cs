using System;
using System.Collections.Generic;
using System.Linq;

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
            var list = new List<Item>();

            void A(string code, string name, string module, string action, bool scopable = true)
                => list.Add(new Item(code, name, module, action, null, scopable));

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
            A(PermissionCodes.HrTransferManage, "Quản lý", "HUMAN_RESOURCE_TRANSFER", "MANAGE");

            // OPERATE 
            A(PermissionCodes.OperateView, "Xem", "OPERATE", "VIEW");
            A(PermissionCodes.OperateManage, "Quản lý", "OPERATE", "MANAGE");

            A(PermissionCodes.OperateTimekeepingStandardView, "Xem", "OPERATE_TIMEKEEPING_STANDARD", "VIEW");
            A(PermissionCodes.OperateTimekeepingStandardCreate, "Tạo mới", "OPERATE_TIMEKEEPING_STANDARD", "CREATE");
            A(PermissionCodes.OperateTimekeepingStandardUpdate, "Cập nhật", "OPERATE_TIMEKEEPING_STANDARD", "UPDATE");
            A(PermissionCodes.OperateTimekeepingStandardDeactivate, "Ngưng", "OPERATE_TIMEKEEPING_STANDARD", "DEACTIVATE");
            A(PermissionCodes.OperateTimekeepingStandardActivate, "Kích hoạt", "OPERATE_TIMEKEEPING_STANDARD", "ACTIVATE");
            A(PermissionCodes.OperateTimekeepingStandardManage, "Quản lý", "OPERATE_TIMEKEEPING_STANDARD", "MANAGE");

            A(PermissionCodes.OperateTimekeepingView, "Xem", "OPERATE_TIMEKEEPING", "VIEW");
            A(PermissionCodes.OperateTimekeepingAdjust, "Điều chỉnh", "OPERATE_TIMEKEEPING", "ADJUST");
            A(PermissionCodes.OperateTimekeepingSummarize, "Tổng hợp", "OPERATE_TIMEKEEPING", "SUMMARIZE");
            A(PermissionCodes.OperateTimekeepingManage, "Quản lý", "OPERATE_TIMEKEEPING", "MANAGE");

            A(PermissionCodes.OperateAttendanceComplaintView, "Xem", "OPERATE_ATTENDANCE_COMPLAINT", "VIEW");
            A(PermissionCodes.OperateAttendanceComplaintCreate, "Tạo mới", "OPERATE_ATTENDANCE_COMPLAINT", "CREATE");
            A(PermissionCodes.OperateAttendanceComplaintReview, "Duyệt", "OPERATE_ATTENDANCE_COMPLAINT", "REVIEW");
            A(PermissionCodes.OperateAttendanceComplaintManage, "Quản lý", "OPERATE_ATTENDANCE_COMPLAINT", "MANAGE");

            A(PermissionCodes.OperateDayOffConfigView, "Xem", "OPERATE_DAY_OFF_CONFIG", "VIEW");
            A(PermissionCodes.OperateDayOffConfigCreate, "Tạo mới", "OPERATE_DAY_OFF_CONFIG", "CREATE");
            A(PermissionCodes.OperateDayOffConfigUpdate, "Cập nhật", "OPERATE_DAY_OFF_CONFIG", "UPDATE");
            A(PermissionCodes.OperateDayOffConfigDeactivate, "Ngưng", "OPERATE_DAY_OFF_CONFIG", "DEACTIVATE");
            A(PermissionCodes.OperateDayOffConfigActivate, "Kích hoạt", "OPERATE_DAY_OFF_CONFIG", "ACTIVATE");
            A(PermissionCodes.OperateDayOffConfigManage, "Quản lý", "OPERATE_DAY_OFF_CONFIG", "MANAGE");

            A(PermissionCodes.OperatePublicHolidayView, "Xem", "OPERATE_PUBLIC_HOLIDAY", "VIEW");
            A(PermissionCodes.OperatePublicHolidayCreate, "Tạo mới", "OPERATE_PUBLIC_HOLIDAY", "CREATE");
            A(PermissionCodes.OperatePublicHolidayUpdate, "Cập nhật", "OPERATE_PUBLIC_HOLIDAY", "UPDATE");
            A(PermissionCodes.OperatePublicHolidayDeactivate, "Ngưng", "OPERATE_PUBLIC_HOLIDAY", "DEACTIVATE");
            A(PermissionCodes.OperatePublicHolidayActivate, "Kích hoạt", "OPERATE_PUBLIC_HOLIDAY", "ACTIVATE");
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
            A(PermissionCodes.OperateShiftManage, "Quản lý", "OPERATE_SHIFT", "MANAGE");

            A(PermissionCodes.OperateWorkScheduleView, "Xem", "OPERATE_WORK_SCHEDULE", "VIEW");
            A(PermissionCodes.OperateWorkScheduleCreate, "Tạo mới", "OPERATE_WORK_SCHEDULE", "CREATE");
            A(PermissionCodes.OperateWorkScheduleUpdate, "Cập nhật", "OPERATE_WORK_SCHEDULE", "UPDATE");
            A(PermissionCodes.OperateWorkScheduleDeactivate, "Ngưng", "OPERATE_WORK_SCHEDULE", "DEACTIVATE");
            A(PermissionCodes.OperateWorkScheduleManage, "Quản lý", "OPERATE_WORK_SCHEDULE", "MANAGE");

            A(PermissionCodes.OperateWorkPatternView, "Xem", "OPERATE_WORK_PATTERN", "VIEW");
            A(PermissionCodes.OperateWorkPatternCreate, "Tạo mới", "OPERATE_WORK_PATTERN", "CREATE");
            A(PermissionCodes.OperateWorkPatternUpdate, "Cập nhật", "OPERATE_WORK_PATTERN", "UPDATE");
            A(PermissionCodes.OperateWorkPatternDeactivate, "Ngưng", "OPERATE_WORK_PATTERN", "DEACTIVATE");
            A(PermissionCodes.OperateWorkPatternBulkAssign, "Gán hàng loạt", "OPERATE_WORK_PATTERN", "BULK_ASSIGN");
            A(PermissionCodes.OperateWorkPatternManage, "Quản lý", "OPERATE_WORK_PATTERN", "MANAGE");

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

            // SYSTEM
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

            A(PermissionCodes.ActionLogView, "Xem", "ACTION_LOG", "VIEW", false);
            A(PermissionCodes.MobileAccess, "Truy cập", "MOBILE", "ACCESS", false);

            return list;
        }

        private static string[] BuildHrCodes() =>
        [
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
            PermissionCodes.UserView,
            PermissionCodes.ActionLogView,
            PermissionCodes.MobileAccess,
        ];

        private static string[] BuildManagerCodes() =>
        [
            PermissionCodes.HomeView,
            PermissionCodes.HrEmployeeView,
            PermissionCodes.OperateTimekeepingView,
            PermissionCodes.OperateAttendanceComplaintView,
            PermissionCodes.OperateLeaveView,
            PermissionCodes.OperateLeaveApprove,
            PermissionCodes.OperateLeaveReject,
            PermissionCodes.MobileAccess,
        ];

        private static string[] BuildEmployeeCodes() =>
        [
            PermissionCodes.HomeView,
            PermissionCodes.MobileAccess,
            PermissionCodes.OperateTimekeepingView,
            PermissionCodes.OperateLeaveView,
            PermissionCodes.OperateLeaveCreate,
            PermissionCodes.PayrollSalaryView,
            PermissionCodes.OperateAttendanceComplaintCreate,
        ];
    }
}
