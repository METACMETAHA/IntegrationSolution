using IntegrationSolution.Entities.SelfEntities;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelReportWriter : IExcelWriter
    {
        public IExcelStyle ExcelDecorator { get; private set; }


        public ExcelReportWriter(IExcelStyle excelStyle)
        {
            ExcelDecorator = excelStyle;
        }


        public void CreateReportDiffMileage(string path, List<IntegratedVehicleInfo> valuePairs, double BadPercent = 5)
        {
            using (ExcelPackage excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("Разница показаний одометров");
                var headerRow = new List<string[]>()
                {
                    new string[] { "Гос.номер", "Модель", "Пробег всего по Wialon", "Кол-во поездок", "Показания одометра (SAP)", "Показания одометра (Wialon)",
                        "Разница показателей одометра", "Процент расхождения" }
                };

                int row = 1;
                ExcelDecorator.SetHeaders(worksheet, headerRow, row++);

                foreach (var item in valuePairs)
                {
                    var data = new List<object[]>()
                    {
                        new object[] 
                        {
                            item.StateNumber,
                            item.Model,
                            item.TotalWialon_Mileage,
                            item.CountTrips,
                            item.SAP_Mileage,
                            item.Wialon_Mileage,
                            item.DifferenceMileage,
                            item.PercentDifference
                        }
                    };
                    string headerRange = $"A{row}:" + Char.ConvertFromUtf32(data[0].Length + 64) + row;

                    worksheet.Column(3).Style.Numberformat.Format = "0.00";
                    worksheet.Column(4).Style.Numberformat.Format = "0";
                    worksheet.Column(5).Style.Numberformat.Format = "0.00";
                    worksheet.Column(6).Style.Numberformat.Format = "0.00";
                    worksheet.Column(7).Style.Numberformat.Format = "0.00";
                    worksheet.Column(8).Style.Numberformat.Format = "#0.00%";

                    if (item.SAP_Mileage > item.Wialon_Mileage)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 7], ExcelDecorator.ExcelCssResources.GreenColor);

                    if (item.PercentDifference >= BadPercent)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.RedColor);

                    worksheet.Cells[headerRange].LoadFromArrays(data);
                    
                    row++;
                }

                if (File.Exists(path))
                    File.Delete(path);
                var excelFile = new System.IO.FileInfo(path);
                excel.SaveAs(excelFile);
            }
        }
    }
}
