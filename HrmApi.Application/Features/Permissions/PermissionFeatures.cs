using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.DTOs.Permission;
using MediatR;

namespace HrmApi.Application.Features.Permissions
{
    public class ListPermissionsQuery : IRequest<List<PermissionDto>>
    {
    }

    public class ListPermissionsQueryHandler : IRequestHandler<ListPermissionsQuery, List<PermissionDto>>
    {
        public Task<List<PermissionDto>> Handle(ListPermissionsQuery request, CancellationToken cancellationToken)
        {
            var items = RbacPermissionCatalog.Items
                .OrderBy(x => x.Module)
                .ThenBy(x => x.Action)
                .ThenBy(x => x.Code)
                .Select(ToDto)
                .ToList();

            return Task.FromResult(items);
        }

        private static PermissionDto ToDto(RbacPermissionCatalog.Item x) => new()
        {
            Code = x.Code,
            Name = x.Name,
            Module = x.Module,
            Action = x.Action,
            Description = x.Description,
            IsScopable = x.IsScopable,
        };
    }

    public class GetPermissionTreeQuery : IRequest<List<PermissionModuleTreeDto>>
    {
    }

    public class GetPermissionTreeQueryHandler : IRequestHandler<GetPermissionTreeQuery, List<PermissionModuleTreeDto>>
    {
        private static readonly Dictionary<string, (string ParentKey, string ParentName, string ItemName, int Order)> ModuleMeta =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["HOME"] = ("HOME", "Trang chủ", "Trang chủ", 1),

                ["ORGANIZATION"] = ("ORGANIZATION", "Tổ chức", "Tổ chức", 10),
                ["ORGANIZATION_COMPANY"] = ("ORGANIZATION", "Tổ chức", "Công ty", 11),
                ["ORGANIZATION_BRANCH"] = ("ORGANIZATION", "Tổ chức", "Chi nhánh", 12),
                ["ORGANIZATION_DEPARTMENT"] = ("ORGANIZATION", "Tổ chức", "Phòng ban", 13),
                ["ORGANIZATION_PART"] = ("ORGANIZATION", "Tổ chức", "Bộ phận", 14),
                ["ORGANIZATION_PART_MASTER"] = ("ORGANIZATION", "Tổ chức", "Bộ phận (master)", 15),
                ["ORGANIZATION_POSITION"] = ("ORGANIZATION", "Tổ chức", "Chức vụ", 16),
                ["ORGANIZATION_POSITION_MASTER"] = ("ORGANIZATION", "Tổ chức", "Chức vụ (master)", 17),

                ["HUMAN_RESOURCE"] = ("HUMAN_RESOURCE", "Nhân sự", "Nhân sự", 20),
                ["HUMAN_RESOURCE_EMPLOYEE"] = ("HUMAN_RESOURCE", "Nhân sự", "Nhân viên", 21),
                ["HUMAN_RESOURCE_CONTRACT_TYPE"] = ("HUMAN_RESOURCE", "Nhân sự", "Loại hợp đồng", 22),
                ["HUMAN_RESOURCE_CONTRACT"] = ("HUMAN_RESOURCE", "Nhân sự", "Hợp đồng", 23),
                ["HUMAN_RESOURCE_REVIEW_RENEWAL"] = ("HUMAN_RESOURCE", "Nhân sự", "Xét duyệt / gia hạn", 24),
                ["HUMAN_RESOURCE_TRANSFER"] = ("HUMAN_RESOURCE", "Nhân sự", "Điều chuyển", 25),

                ["OPERATE"] = ("OPERATE", "Vận hành", "Vận hành", 30),
                ["OPERATE_TIMEKEEPING_STANDARD"] = ("OPERATE", "Vận hành", "Chuẩn chấm công", 31),
                ["OPERATE_TIMEKEEPING"] = ("OPERATE", "Vận hành", "Chấm công", 32),
                ["OPERATE_ATTENDANCE_COMPLAINT"] = ("OPERATE", "Vận hành", "Khiếu nại chấm công", 33),
                ["OPERATE_DAY_OFF_CONFIG"] = ("OPERATE", "Vận hành", "Cấu hình ngày nghỉ", 34),
                ["OPERATE_PUBLIC_HOLIDAY"] = ("OPERATE", "Vận hành", "Ngày lễ", 35),
                ["OPERATE_LEAVE"] = ("OPERATE", "Vận hành", "Đơn nghỉ phép", 36),
                ["OPERATE_LEAVE_ALLOCATION"] = ("OPERATE", "Vận hành", "Phân bổ phép", 37),
                ["OPERATE_SHIFT"] = ("OPERATE", "Vận hành", "Ca làm việc", 38),
                ["OPERATE_WORK_SCHEDULE"] = ("OPERATE", "Vận hành", "Lịch làm việc", 39),
                ["OPERATE_WORK_PATTERN"] = ("OPERATE", "Vận hành", "Mẫu ca NV", 40),

                ["PAYROLL"] = ("PAYROLL", "Lương", "Lương", 50),
                ["PAYROLL_SALARY"] = ("PAYROLL", "Lương", "Bảng lương", 51),
                ["PAYROLL_CONFIG"] = ("PAYROLL", "Lương", "Cấu hình lương", 52),

                ["ROLE"] = ("SYSTEM", "Hệ thống", "Vai trò", 60),
                ["USER"] = ("SYSTEM", "Hệ thống", "Tài khoản", 61),
                ["ACTION_LOG"] = ("SYSTEM", "Hệ thống", "Nhật ký thao tác", 62),
                ["MOBILE"] = ("MOBILE", "Di động", "Ứng dụng di động", 70),
            };

        private static readonly Dictionary<string, string> ActionNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["VIEW"] = "Xem",
            ["CREATE"] = "Tạo mới",
            ["UPDATE"] = "Cập nhật",
            ["DELETE"] = "Xóa",
            ["DEACTIVATE"] = "Ngưng",
            ["ACTIVATE"] = "Kích hoạt",
            ["IMPORT_EXCEL"] = "Import Excel",
            ["EXPORT_EXCEL"] = "Export Excel",
            ["MANAGE"] = "Quản lý",
            ["APPROVE"] = "Duyệt",
            ["REJECT"] = "Từ chối",
            ["APPLY"] = "Áp dụng",
            ["CANCEL"] = "Hủy",
            ["ADJUST"] = "Điều chỉnh",
            ["REVIEW"] = "Duyệt",
            ["SUMMARIZE"] = "Tổng hợp",
            ["SIGN"] = "Ký",
            ["TERMINATE"] = "Chấm dứt",
            ["RENEW"] = "Gia hạn",
            ["MARK_PAID"] = "Đánh dấu đã trả",
            ["BULK_ASSIGN"] = "Gán hàng loạt",
            ["RESET_PASSWORD"] = "Đặt lại mật khẩu",
            ["ACCESS"] = "Truy cập",
        };

        public Task<List<PermissionModuleTreeDto>> Handle(GetPermissionTreeQuery request, CancellationToken cancellationToken)
        {
            var catalog = RbacPermissionCatalog.Items
                .Select(x =>
                {
                    ModuleMeta.TryGetValue(x.Module, out var meta);
                    string parentKey = meta.ParentKey ?? x.Module;
                    string parentName = meta.ParentName ?? x.Module;
                    string itemName = meta.ItemName ?? x.Module;
                    int order = meta.Order;
                    return new
                    {
                        Item = x,
                        ParentKey = parentKey,
                        ParentName = parentName,
                        ItemKey = x.Module,
                        ItemName = itemName,
                        Order = order,
                        ActionName = ActionNames.TryGetValue(x.Action, out string? an) ? an : x.Name,
                    };
                })
                .ToList();

            var tree = catalog
                .GroupBy(x => new { x.ParentKey, x.ParentName })
                .OrderBy(g => g.Min(x => x.Order))
                .Select(g =>
                {
                    var flat = g
                        .OrderBy(x => x.Order)
                        .ThenBy(x => x.Item.Action)
                        .Select(x => new PermissionDto
                        {
                            Code = x.Item.Code,
                            Name = x.Item.Name,
                            Module = x.Item.Module,
                            Action = x.Item.Action,
                            Description = x.Item.Description,
                            IsScopable = x.Item.IsScopable,
                        })
                        .ToList();

                    var items = g
                        .GroupBy(x => new { x.ItemKey, x.ItemName, x.Order })
                        .OrderBy(ig => ig.Key.Order)
                        .Select(ig => new PermissionItemNodeDto
                        {
                            Key = ig.Key.ItemKey,
                            Name = ig.Key.ItemName,
                            Actions = ig
                                .OrderBy(x => ActionSort(x.Item.Action))
                                .ThenBy(x => x.Item.Code)
                                .Select(x => new PermissionActionNodeDto
                                {
                                    Code = x.Item.Code,
                                    Name = x.Item.Name,
                                    Action = x.Item.Action,
                                    ActionName = x.ActionName,
                                    IsScopable = x.Item.IsScopable,
                                })
                                .ToList(),
                        })
                        .ToList();

                    return new PermissionModuleTreeDto
                    {
                        Module = g.Key.ParentKey,
                        ModuleName = g.Key.ParentName,
                        Items = items,
                        Permissions = flat,
                    };
                })
                .ToList();

            return Task.FromResult(tree);
        }

        private static int ActionSort(string action) => action.ToUpperInvariant() switch
        {
            "VIEW" => 1,
            "CREATE" => 2,
            "UPDATE" => 3,
            "DELETE" => 4,
            "DEACTIVATE" => 5,
            "ACTIVATE" => 6,
            "IMPORT_EXCEL" => 7,
            "EXPORT_EXCEL" => 8,
            "ADJUST" => 9,
            "SUMMARIZE" => 10,
            "APPROVE" => 11,
            "REJECT" => 12,
            "REVIEW" => 13,
            "APPLY" => 14,
            "CANCEL" => 15,
            "SIGN" => 16,
            "TERMINATE" => 17,
            "RENEW" => 18,
            "MARK_PAID" => 19,
            "BULK_ASSIGN" => 20,
            "RESET_PASSWORD" => 21,
            "MANAGE" => 90,
            "ACCESS" => 91,
            _ => 99,
        };
    }
}
