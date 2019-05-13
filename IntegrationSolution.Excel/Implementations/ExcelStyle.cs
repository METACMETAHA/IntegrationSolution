using IntegrationSolution.Excel.Common;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelStyle : IExcelStyle
    {
        private readonly CssExcelStorage _styleExcel;

        public ExcelStyle()
        {
            _styleExcel = new CssExcelStorage();
        }

        public void AddHeader(ExcelWorksheet WorkSheet, int row, int column, string Title)
        {
            try
            {
                WorkSheet.SetValue(row, column, Title);

                WorkSheet.Column(column).Width = 22;
                WorkSheet.Cells[row, column].Style.Font.SetFromFont(_styleExcel.HeadersFont);

                WorkSheet.Cells[row, column].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                WorkSheet.Cells[row, column].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

                WorkSheet.Cells[row, column].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                WorkSheet.Cells[row, column].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                WorkSheet.Cells[row, column].Style.WrapText = true;

                WorkSheet.Cells[row, column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Gray125;
                WorkSheet.Cells[row, column].Style.Fill.PatternColor.SetColor(_styleExcel.HeadersBackgroundColor);
                WorkSheet.Cells[row, column].Style.Fill.BackgroundColor.SetColor(_styleExcel.HeadersBackgroundColor);
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
                WorkSheet.Cells[headerRange].Style.Font.SetFromFont(_styleExcel.HeadersFont);
                WorkSheet.Cells[headerRange].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                WorkSheet.Cells[headerRange].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                WorkSheet.Cells[headerRange].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                WorkSheet.Cells[headerRange].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                WorkSheet.Cells[headerRange].Style.WrapText = true;
                WorkSheet.Cells[headerRange].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Gray125;
                WorkSheet.Cells[headerRange].Style.Fill.PatternColor.SetColor(_styleExcel.HeadersBackgroundColor);
                WorkSheet.Cells[headerRange].Style.Fill.BackgroundColor.SetColor(_styleExcel.HeadersBackgroundColor);
                WorkSheet.DefaultColWidth = 22;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(nameof(ExcelStyle)).Error(ex.Message);
            }
        }
    }
}
