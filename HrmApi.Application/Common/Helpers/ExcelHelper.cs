using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace HrmApi.Application.Common.Helpers
{
    public static class ExcelHelper
    {
        public static readonly XLColor HeaderBackgroundColor = XLColor.FromHtml("#5fbaff");

        public static void WriteStyledHeaderCell(IXLWorksheet worksheet, int col, string title, bool required)
        {
            IXLCell cell = worksheet.Cell(1, col);
            cell.Style.Fill.BackgroundColor = HeaderBackgroundColor;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = false;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            if (required)
            {
                var richText = cell.GetRichText();
                richText.ClearText();
                var textPart = richText.AddText(title);
                textPart.FontColor = XLColor.Black;
                textPart.Bold = true;

                var starPart = richText.AddText(" *");
                starPart.FontColor = XLColor.Red;
                starPart.Bold = true;
            }
            else
            {
                cell.Value = title;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.Black;
            }
        }

        public static void WriteReferenceSheet(
            IXLWorkbook workbook,
            string sheetName,
            string codeHeader,
            string nameHeader,
            IEnumerable<(string Code, string Name)> items)
        {
            IXLWorksheet ws = workbook.Worksheets.Add(sheetName);
            WriteStyledHeaderCell(ws, 1, codeHeader, false);
            WriteStyledHeaderCell(ws, 2, nameHeader, false);
            ws.Row(1).Height = 28;

            int row = 2;
            foreach (var item in items)
            {
                var c1 = ws.Cell(row, 1);
                c1.Value = item.Code ?? string.Empty;
                c1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                var c2 = ws.Cell(row, 2);
                c2.Value = item.Name ?? string.Empty;
                c2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                row++;
            }

            ApplyColumnWidths(ws);
            FreezeHeaderRow(ws);
        }

        public static void ApplyColumnWidths(IXLWorksheet worksheet)
        {
            var usedColumns = worksheet.ColumnsUsed();
            foreach (var column in usedColumns)
            {
                int maxTextLength = 0;
                foreach (var cell in column.CellsUsed())
                {
                    string text = string.Empty;
                    if (cell.HasRichText)
                    {
                        text = cell.GetRichText().Text;
                    }
                    else
                    {
                        try
                        {
                            text = cell.GetFormattedString();
                        }
                        catch
                        {
                        }

                        if (string.IsNullOrEmpty(text))
                        {
                            text = cell.Value.ToString() ?? string.Empty;
                        }
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        foreach (var line in lines)
                        {
                            if (line.Length > maxTextLength)
                            {
                                maxTextLength = line.Length;
                            }
                        }
                    }
                }

                try
                {
                    _ = column.AdjustToContents();
                }
                catch
                {
                }

                double contentBasedWidth = Math.Ceiling(maxTextLength * 1.15) + 5;
                double calculatedWidth = Math.Max(column.Width + 4, contentBasedWidth);
                column.Width = Math.Max(calculatedWidth, 16);
            }
        }

        public static void FreezeHeaderRow(IXLWorksheet worksheet)
        {
            worksheet.SheetView.FreezeRows(1);
        }

        public static string GetCellString(IXLRangeRow row, int column)
        {
            IXLCell cell = row.Cell(column);
            if (cell.IsEmpty())
            {
                return string.Empty;
            }

            try
            {
                var text = cell.GetFormattedString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Trim();
                }
            }
            catch
            {
            }

            return cell.Value.ToString()?.Trim() ?? string.Empty;
        }

        public static DateTime? ParseDate(IXLCell cell)
        {
            if (cell.IsEmpty())
            {
                return null;
            }

            if (cell.TryGetValue(out DateTime dateValue))
            {
                return DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);
            }

            string text = string.Empty;
            try
            {
                text = cell.GetFormattedString().Trim();
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                text = cell.Value.ToString()?.Trim() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string[] formats =
            [
                "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy",
                "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy",
                "yyyy-MM-dd", "yyyy/MM/dd",
                "MM/dd/yyyy", "M/d/yyyy",
                "dd.MM.yyyy", "d.M.yyyy",
                "yyyy.MM.dd"
            ];

            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime exactDate))
            {
                return DateTime.SpecifyKind(exactDate, DateTimeKind.Utc);
            }

            if (DateTime.TryParse(text, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out DateTime viDate))
            {
                return DateTime.SpecifyKind(viDate, DateTimeKind.Utc);
            }

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            if (DateTime.TryParse(text, out DateTime localParsed))
            {
                return DateTime.SpecifyKind(localParsed, DateTimeKind.Utc);
            }

            return null;
        }

        public static bool? ParseBool(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out bool boolValue)) return boolValue;
            var text = cell.GetString().Trim().ToLower();
            return text switch
            {
                "1" or "true" or "có" or "co" or "yes" => true,
                "0" or "false" or "không" or "khong" or "no" => false,
                _ => null
            };
        }

        public static int? ParseInt(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out int intValue)) return intValue;
            if (int.TryParse(cell.GetString(), out var parsed)) return parsed;
            return null;
        }

        public static Guid? ParseGuid(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            var text = cell.GetString().Trim();
            return Guid.TryParse(text, out var parsed) ? parsed : null;
        }
    }
}
