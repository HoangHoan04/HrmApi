using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HrmApi.Application.Features.Integrations
{
    public class PunchCsvRowDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime PunchTime { get; set; }
        public string Type { get; set; } = PunchType.In;
    }

    public class ImportPunchCsvResultDto
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportPunchCsvCommand : IRequest<ImportPunchCsvResultDto>
    {
        public string? Content { get; set; }
        public List<PunchCsvRowDto>? Rows { get; set; }
    }

    public class ImportPunchCsvCommandHandler : IRequestHandler<ImportPunchCsvCommand, ImportPunchCsvResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAttendanceRuleService _rules;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public ImportPunchCsvCommandHandler(
            IApplicationDbContext context,
            IAttendanceRuleService rules,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _rules = rules;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<ImportPunchCsvResultDto> Handle(ImportPunchCsvCommand request, CancellationToken cancellationToken)
        {
            List<PunchCsvRowDto> rows = request.Rows ?? [];
            if (rows.Count == 0 && !string.IsNullOrWhiteSpace(request.Content))
            {
                rows = ParseContent(request.Content);
            }

            ImportPunchCsvResultDto result = new() { TotalRows = rows.Count };
            if (rows.Count == 0)
            {
                throw new InvalidOperationException("Không có dòng punch để import.");
            }

            List<string> codes = rows.Select(x => x.EmployeeCode.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Dictionary<string, EmployeeEntity> employees = await _context.EmployeeEntities
                .Where(x => !x.IsDeleted && codes.Contains(x.Code))
                .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

            int imported = 0;
            foreach (PunchCsvRowDto row in rows)
            {
                try
                {
                    string code = (row.EmployeeCode ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(code))
                    {
                        result.Skipped++;
                        result.Errors.Add("Thiếu EmployeeCode.");
                        continue;
                    }

                    if (!employees.TryGetValue(code, out EmployeeEntity? employee))
                    {
                        result.Skipped++;
                        result.Errors.Add($"Không tìm thấy NV mã {code}.");
                        continue;
                    }

                    string type = (row.Type ?? PunchType.In).Trim().ToUpperInvariant();
                    if (type is not (PunchType.In or PunchType.Out or PunchType.CheckIn or PunchType.CheckOut or "I" or "O"))
                    {
                        result.Skipped++;
                        result.Errors.Add($"{code}: Type phải là IN/OUT.");
                        continue;
                    }

                    bool isIn = type is PunchType.In or PunchType.CheckIn or "I";
                    DateTime punchUtc = row.PunchTime.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(row.PunchTime, DateTimeKind.Utc)
                        : row.PunchTime.ToUniversalTime();
                    DateOnly workDate = DateOnly.FromDateTime(punchUtc);

                    TimekeepingEntity record = await _rules.GetOrCreateTodayRecordAsync(employee, workDate, cancellationToken);
                    if (isIn)
                    {
                        record.CheckInAt = punchUtc;
                    }
                    else
                    {
                        record.CheckOutAt = punchUtc;
                    }

                    record.IsManualAdjusted = true;
                    record.Note = string.IsNullOrWhiteSpace(record.Note)
                        ? "Import CSV punch"
                        : record.Note;
                    record.UpdatedAt = DateTime.UtcNow;
                    record.UpdatedBy = _currentUser.UserId;

                    WorkWindowResult window = await _rules.ResolveWorkWindowAsync(employee, workDate, cancellationToken);
                    AttendanceStandardResult standard = await _rules.ResolveStandardAsync(
                        record.BranchId ?? window.BranchId ?? employee.BranchId,
                        employee.CompanyId,
                        cancellationToken);
                    _rules.ComputeStatus(record, window, standard);
                    await _rules.FinalizeOtAndNightAsync(record, window, standard, cancellationToken);
                    imported++;
                }
                catch (Exception ex)
                {
                    result.Skipped++;
                    result.Errors.Add($"{row.EmployeeCode}: {ex.Message}");
                }
            }

            if (imported > 0)
            {
                _ = await _context.SaveChangesAsync(cancellationToken);
                await _actionLog.LogActionAsync(
                    ActionType.CREATE,
                    "TimekeepingEntity",
                    null,
                    null,
                    new { imported, result.TotalRows },
                    "Import punch CSV");
            }

            result.Imported = imported;
            if (result.Errors.Count > 50)
            {
                result.Errors = result.Errors.Take(50).Append($"... và {result.Errors.Count - 50} lỗi khác").ToList();
            }

            return result;
        }

        private static List<PunchCsvRowDto> ParseContent(string content)
        {
            string trimmed = content.Trim();
            return trimmed.StartsWith('[') || trimmed.StartsWith('{') ? ParseJson(trimmed) : ParseCsv(trimmed);
        }

        private static List<PunchCsvRowDto> ParseJson(string content)
        {
            JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
            if (content.StartsWith('['))
            {
                return JsonSerializer.Deserialize<List<PunchCsvRowDto>>(content, options) ?? [];
            }

            List<PunchCsvRowDto> list = [];
            foreach (string line in content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                PunchCsvRowDto? row = JsonSerializer.Deserialize<PunchCsvRowDto>(line, options);
                if (row != null)
                {
                    list.Add(row);
                }
            }
            return list;
        }

        private static List<PunchCsvRowDto> ParseCsv(string content)
        {
            List<PunchCsvRowDto> list = [];
            string[] lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length == 0)
            {
                return list;
            }

            int start = 0;
            Dictionary<string, int> header = new(StringComparer.OrdinalIgnoreCase);
            string[] first = SplitCsvLine(lines[0]);
            if (first.Any(x => x.Equals("EmployeeCode", StringComparison.OrdinalIgnoreCase)
                || x.Equals("PunchTime", StringComparison.OrdinalIgnoreCase)
                || x.Equals("Type", StringComparison.OrdinalIgnoreCase)))
            {
                for (int i = 0; i < first.Length; i++)
                {
                    header[first[i].Trim()] = i;
                }

                start = 1;
            }
            else
            {
                header["EmployeeCode"] = 0;
                header["PunchTime"] = 1;
                header["Type"] = 2;
            }

            for (int li = start; li < lines.Length; li++)
            {
                string[] cols = SplitCsvLine(lines[li]);
                if (cols.Length == 0)
                {
                    continue;
                }

                string code = GetCol(cols, header, "EmployeeCode");
                string timeRaw = GetCol(cols, header, "PunchTime");
                string type = GetCol(cols, header, "Type");
                if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(timeRaw))
                {
                    continue;
                }

                if (!TryParsePunchTime(timeRaw, out DateTime punchTime))
                {
                    throw new InvalidOperationException($"Dòng {li + 1}: PunchTime không hợp lệ ({timeRaw}).");
                }

                list.Add(new PunchCsvRowDto
                {
                    EmployeeCode = code,
                    PunchTime = punchTime,
                    Type = string.IsNullOrWhiteSpace(type) ? "IN" : type,
                });
            }

            return list;
        }

        private static string GetCol(string[] cols, Dictionary<string, int> header, string name)
        {
            return !header.TryGetValue(name, out int idx) || idx < 0 || idx >= cols.Length ? string.Empty : cols[idx].Trim().Trim('"');
        }

        private static string[] SplitCsvLine(string line)
        {
            List<string> parts = [];
            StringBuilder sb = new();
            bool inQuotes = false;
            foreach (char c in line)
            {
                if (c == '"') { inQuotes = !inQuotes; continue; }
                if (c == ',' && !inQuotes) { parts.Add(sb.ToString()); _ = sb.Clear(); continue; }
                _ = sb.Append(c);
            }
            parts.Add(sb.ToString());
            return parts.ToArray();
        }

        private static bool TryParsePunchTime(string raw, out DateTime punchTime)
        {
            punchTime = default;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            string[] formats =
            [
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-dd HH:mm",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd-MM-yyyy HH:mm:ss",
                "dd-MM-yyyy HH:mm",
            ];
            return DateTime.TryParseExact(raw.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out punchTime) || DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out punchTime);
        }
    }
}
