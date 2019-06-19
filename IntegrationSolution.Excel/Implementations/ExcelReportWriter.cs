using IntegrationSolution.Entities.Helpers;
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
                    new string[] { "Подразделение", "Гос.номер", "Модель", "Тип",
                        "Пробег всего по Wialon", "Кол-во превышений скоростного режима",
                        "Кол-во поездок (SAP)", "Кол-во поездок (Wialon)",
                        "Показания одометра (SAP)", "Показания одометра (Wialon)",
                        "Разница показателей одометра", "Процент расхождения" }
                };

                worksheet.Column(5).Style.Numberformat.Format = "0.00";
                worksheet.Column(6).Style.Numberformat.Format = "0";
                worksheet.Column(7).Style.Numberformat.Format = "0";
                worksheet.Column(8).Style.Numberformat.Format = "0";
                worksheet.Column(9).Style.Numberformat.Format = "0.00";
                worksheet.Column(10).Style.Numberformat.Format = "0.00";
                worksheet.Column(11).Style.Numberformat.Format = "0.00";
                worksheet.Column(12).Style.Numberformat.Format = "0.00%";

                int row = 1;
                ExcelDecorator.SetHeaders(worksheet, headerRow, row++);

                foreach (var item in valuePairs)
                {
                    var data = new List<object[]>()
                    {
                        new object[] 
                        {
                            item.StructureName,
                            item.StateNumber,
                            item.UnitModel,
                            item.Type,
                            item.WialonMileageTotal,
                            item.CountSpeedViolations,
                            item.CountTrips.SAP,
                            item.CountTrips.Wialon,
                            item.IndicatorMileage.SAP,
                            item.IndicatorMileage.Wialon,
                            item.IndicatorMileage.Difference,
                            item.PercentDifference
                        }
                    };
                    string headerRange = $"A{row}:" + Char.ConvertFromUtf32(data[0].Length + 64) + row;
                    
                    if (item.CountSpeedViolations > 0)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 6], ExcelDecorator.ExcelCssResources.LightRedColor);

                    if (item.CountTrips.SAP > item.CountTrips.Wialon)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.RedColor);

                    if (item.IndicatorMileage.SAP > item.IndicatorMileage.Wialon)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 11], ExcelDecorator.ExcelCssResources.GreenColor);

                    if (item.PercentDifference*100 >= BadPercent)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 12], ExcelDecorator.ExcelCssResources.RedColor);

                    worksheet.Cells[headerRange].LoadFromArrays(data);
                    
                    row++;
                }

                if (File.Exists(path))
                    File.Delete(path);
                var excelFile = new System.IO.FileInfo(path);
                excel.SaveAs(excelFile);
            }
        }


        public void CreateReportDiffMileageWithDetails(string path, List<IntegratedVehicleInfoDetails> valuePairs, double BadPercent = 5)
        {
            using (ExcelPackage excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("Разница показаний одометров");
                var headerRow = new List<string[]>()
                {
                    new string[] { "Подразделение", "Гос.номер", "Модель", "Тип",
                        "Пробег всего по Wialon", "Кол-во превышений скоростного режима",
                        "Дата выезда", "Начало (время SAP)", "Начало (время Wialon)", "Начало (локация)",
                        "Конец (время SAP)", "Конец (время Wialon)", "Конец (локация)",
                        "Показания одометра (SAP)", "Показания одометра (Wialon)",
                        "Разница показателей одометра", "Процент расхождения", "Водитель", "Табельный номер водителя" }
                };

                #region Columns` Style
                worksheet.Column(5).Style.Numberformat.Format = "0.00";
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Style.Numberformat.Format = "0";
                worksheet.Column(7).Style.Numberformat.Format = "dd-mm-yyyy"; // "Date of trip"
                worksheet.Column(7).Width = 15;
                worksheet.Column(8).Style.Numberformat.Format = "hh:mm"; // Begin trip (SAP)
                worksheet.Column(9).Style.Numberformat.Format = "hh:mm"; // Begin trip (Wialon)
                worksheet.Column(11).Style.Numberformat.Format = "hh:mm"; // End trip (SAP)
                worksheet.Column(12).Style.Numberformat.Format = "hh:mm"; // End trip (Wialon)
                worksheet.Column(14).Style.Numberformat.Format = "0.00";
                worksheet.Column(15).Style.Numberformat.Format = "0.00";
                worksheet.Column(16).Style.Numberformat.Format = "0.00";
                worksheet.Column(17).Style.Numberformat.Format = "0.00%";
                #endregion

                int row = 1;
                ExcelDecorator.SetHeaders(worksheet, headerRow, row++);

                foreach (var item in valuePairs)
                {
                    var data = new List<object[]>()
                    {
                        new object[]
                        {
                            item.StructureName, item.StateNumber, item.UnitModel, item.Type,
                            item.WialonMileageTotal, item.CountSpeedViolations,
                            null, item.CountTrips.SAP,
                            item.CountTrips.Wialon,
                            null, null, null, null,
                            item.IndicatorMileage.SAP,
                            item.IndicatorMileage.Wialon,
                            item.IndicatorMileage.Difference,
                            item.PercentDifference, item.TripsSAP?.ToLookup(x => x.Driver.UnitNumber).Count, null
                        }
                    };
                    string headerRange = $"A{row}:" + Char.ConvertFromUtf32(data[0].Length + 64) + row;

                    ExcelDecorator.SetCellsColor(worksheet.Cells[headerRange], ExcelDecorator.ExcelCssResources.LightYellowColor);

                    //if (item.CountSpeedViolations > 0)
                    //    ExcelDecorator.SetCellsColor(worksheet.Cells[row, 6], ExcelDecorator.ExcelCssResources.LightRedColor);

                    //if (item.CountTrips.SAP > item.CountTrips.Wialon)
                    //    ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.RedColor);

                    if (item.IndicatorMileage.SAP > item.IndicatorMileage.Wialon)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 17], ExcelDecorator.ExcelCssResources.GreenColor);

                    if (item.PercentDifference * 100 >= BadPercent)
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 17], ExcelDecorator.ExcelCssResources.RedColor);
                    
                    worksheet.Cells[row, 8, row, 10].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 11, row, 13].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 18].Style.Numberformat.Format = "0";
                    worksheet.Row(row).Style.Font.SetFromFont(new Font("Times New Roman", 12, FontStyle.Bold));

                    worksheet.Cells[headerRange].LoadFromArrays(data);

                    row++;

                    int LSap = item.TripsSAP?.Count() ?? 0;
                    int LWialon = item.TripsWialon?.Count() ?? 0;
                    item.TripsSAP = item.TripsSAP?.OrderBy(x => x.DepartureFromGarageDate);
                    item.TripsWialon = item.TripsWialon?.OrderBy(x => x.Begin);

                    for (int ISap = 0, IWln = 0; ISap < LSap || IWln < LWialon;)
                    {
                        List<object[]> dataDetails = new List<object[]>();
                        var sapElement = item.TripsSAP?.ElementAtOrDefault(ISap);
                        var wlnElement = item.TripsWialon?.ElementAtOrDefault(IWln);

                        if (sapElement != null && wlnElement != null &&
                            sapElement.DepartureFromGarageDate.Date == wlnElement.Begin.Date)
                        {
                            dataDetails = new List<object[]>()
                            {
                                new object[]
                                {
                                    null, null, null, null,
                                    null, wlnElement.SpeedViolation?.Count(),
                                    wlnElement.Begin.Date,
                                    sapElement.DepartureFromGarageDate.TimeOfDay, wlnElement.Begin.TimeOfDay, wlnElement.LocationBegin,
                                    sapElement.ReturnToGarageDate.TimeOfDay, wlnElement.Finish.TimeOfDay, wlnElement.LocationFinish,
                                    sapElement.TotalMileage, wlnElement.Mileage,
                                    sapElement.TotalMileage - wlnElement.Mileage,
                                    sapElement.TotalMileage.GetPercentFrom(wlnElement.Mileage),
                                    sapElement.Driver.ToString(),
                                    sapElement.Driver.UnitNumber
                                }
                            };

                            if (sapElement.TotalMileage.GetPercentFrom(wlnElement.Mileage) * 100 > BadPercent
                                && sapElement.TotalMileage < wlnElement.Mileage)
                                ExcelDecorator.SetCellsColor(worksheet.Cells[row, 17], ExcelDecorator.ExcelCssResources.LightGreenColor);
                            else if (sapElement.TotalMileage.GetPercentFrom(wlnElement.Mileage) * 100 > BadPercent
                                && sapElement.TotalMileage > wlnElement.Mileage)
                                ExcelDecorator.SetCellsColor(worksheet.Cells[row, 17], ExcelDecorator.ExcelCssResources.LightRedColor);

                            IWln++;
                            ISap++;
                        }
                        else if ( (sapElement != null &&
                            sapElement.DepartureFromGarageDate.Date < wlnElement?.Begin.Date) ||
                            (sapElement != null && wlnElement == null))
                        {
                            dataDetails = new List<object[]>()
                            {
                                new object[]
                                {
                                    null, null, null, null,
                                    null, null,
                                    sapElement.DepartureFromGarageDate.Date,
                                    sapElement.DepartureFromGarageDate.TimeOfDay, null, null,
                                    sapElement.ReturnToGarageDate.TimeOfDay, null, null,
                                    sapElement.TotalMileage, null,
                                    null, null,
                                    sapElement.Driver.ToString(),
                                    sapElement.Driver.UnitNumber
                                }
                            };
                            
                            ISap++;
                        }
                        else if ( (wlnElement != null &&
                            sapElement?.DepartureFromGarageDate.Date > wlnElement.Begin.Date) ||
                            (wlnElement != null && sapElement == null))
                        {
                            dataDetails = new List<object[]>()
                            {
                                new object[]
                                {
                                    null, null, null, null,
                                    null, wlnElement.SpeedViolation?.Count(),
                                    wlnElement.Begin.Date,
                                    null, wlnElement.Begin.TimeOfDay, wlnElement.LocationBegin,
                                    null, wlnElement.Finish.TimeOfDay, wlnElement.LocationFinish,
                                    null, wlnElement.Mileage,
                                    null, null, null, null
                                }
                            };

                            IWln++;
                        }

                        if (!dataDetails.Any())
                            continue;

                        string headerRangelocal = $"A{row}:" + Char.ConvertFromUtf32(dataDetails[0].Length + 64) + row;
                        worksheet.Cells[headerRangelocal].LoadFromArrays(dataDetails);
                        worksheet.Row(row++).OutlineLevel = 1;
                    }
                }

                if (File.Exists(path))
                    File.Delete(path);
                var excelFile = new System.IO.FileInfo(path);
                excel.SaveAs(excelFile);
            }
        }
    }
}
