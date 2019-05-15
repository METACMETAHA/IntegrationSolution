using IntegrationSolution.Excel.Common;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelStyle : IExcelStyle
    {
        public CssExcelStorage ExcelCssResources { get; private set; }

        public ExcelStyle()
        {
            ExcelCssResources = new CssExcelStorage();
        }


        public void AddHeader(ExcelWorksheet WorkSheet, int row, int column, string Title)
        {
            try
            {
                WorkSheet.SetValue(row, column, Title);

                WorkSheet.Column(column).Width = 22;
                WorkSheet.Cells[row, column].Style.Font.SetFromFont(ExcelCssResources.HeadersFont);

                WorkSheet.Cells[row, column].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                WorkSheet.Cells[row, column].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

                WorkSheet.Cells[row, column].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                WorkSheet.Cells[row, column].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                WorkSheet.Cells[row, column].Style.WrapText = true;

                SetCellsColor(WorkSheet.Cells[row, column], ExcelCssResources.HeadersBackgroundColor);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(nameof(ExcelStyle)).Error(ex.Message);
            }
        }


        public void SetHeaders(ExcelWorksheet WorkSheet, List<string[]> headerRow, int row = 1)
        {
            try
            {
                string headerRange = $"A{row}:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + row;
                WorkSheet.Cells[headerRange].LoadFromArrays(headerRow);
                WorkSheet.Cells[headerRange].Style.Font.SetFromFont(ExcelCssResources.HeadersFont);
                WorkSheet.Cells[headerRange].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                WorkSheet.Cells[headerRange].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                WorkSheet.Cells[headerRange].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                WorkSheet.Cells[headerRange].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                WorkSheet.Cells[headerRange].Style.WrapText = true;

                SetCellsColor(WorkSheet.Cells[headerRange], ExcelCssResources.HeadersBackgroundColor);
                WorkSheet.DefaultColWidth = 22;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(nameof(ExcelStyle)).Error(ex.Message);
            }
        }


        public void SetCellsColor(ExcelRange excelRange, Color color, ExcelFillStyle fillType = ExcelFillStyle.Gray125)
        {
            excelRange.Style.Fill.PatternType = fillType;
            excelRange.Style.Fill.PatternColor.SetColor(color);
            excelRange.Style.Fill.BackgroundColor.SetColor(color);
        }
    }
}
