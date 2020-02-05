using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
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

        // Report about difference between SAP and Wialon 
        public void CreateReportDiffMileage(
            string path,
            List<IntegratedVehicleInfo> valuePairs,
            double BadPercent = 5,
            List<IVehicleSAP> sapCars = null,
            List<CarWialon> wialonCars = null)
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
                        "Анализ пробегов", "Процент расхождения" }
                };

                worksheet.Column(5).Width = worksheet.Column(5).Width * 1.5;
                worksheet.Column(5).Style.Numberformat.Format = "0.00";
                worksheet.Column(6).Style.Numberformat.Format = "0";
                worksheet.Column(7).Style.Numberformat.Format = "0";
                worksheet.Column(8).Style.Numberformat.Format = "0";
                worksheet.Column(9).Style.Numberformat.Format = "0.00";
                worksheet.Column(10).Style.Numberformat.Format = "0.00";
                worksheet.Column(11).Style.Numberformat.Format = "0.00";
                worksheet.Column(12).Width = 15;
                worksheet.Column(12).Style.Numberformat.Format = "0.00%";
                

                int row = 1;
                ExcelDecorator.SetHeaders(worksheet, headerRow, row++);
                
                // Freeze first row
                worksheet.View.FreezePanes(2, 1);

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
                    
                    if ((item.PercentDifference * 100 >= BadPercent)
                        && (item.IndicatorMileage.SAP > item.IndicatorMileage.Wialon))
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 12], ExcelDecorator.ExcelCssResources.RedColor);

                    if ((item.PercentDifference * 100 >= BadPercent)
                        && (item.IndicatorMileage.SAP < item.IndicatorMileage.Wialon))
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 12], ExcelDecorator.ExcelCssResources.YellowColor);

                    worksheet.Cells[headerRange].LoadFromArrays(data);

                    row++;
                }

                AddSAPCarsWorksheet(excel, sapCars);
                AddWialonCarsWorksheet(excel, wialonCars);

                if (File.Exists(path))
                    File.Delete(path);
                var excelFile = new System.IO.FileInfo(path);
                excel.SaveAs(excelFile);
            }
        }


        // Report WITH details about difference between SAP and Wialon
        public void CreateReportDiffMileageWithDetails(
            string path,
            List<IntegratedVehicleInfoDetails> valuePairs,
            double BadPercent = 5,
            List<IVehicleSAP> sapCars = null,
            List<CarWialon> wialonCars = null)
        {
            using (ExcelPackage excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("Разница показаний одометров");
                worksheet.OutLineSummaryBelow = false;

                var headerRow = new List<string[]>()
                {
                    new string[] { "Подразделение", "Гос.номер", "Модель", "Тип", //4
                        "Показания одометра (SAP)", "Показания одометра (Wialon)", //6
                        "Анализ пробегов", "Процент расхождения", //8
                        "Дата выезда", "Начало (время SAP)", "Начало (время Wialon)", "Начало (локация)", //12
                        "Конец (время SAP)", "Конец (время Wialon)", "Конец (локация)", //15
                        "Пробег всего по Wialon", "Кол-во превышений скоростного режима", //17
                        "Водитель", "Табельный номер водителя" } //19
                };

                #region Columns` Style
                worksheet.Column(16).Style.Numberformat.Format = "0.00";
                worksheet.Column(16).Width = 25;
                worksheet.Column(17).Style.Numberformat.Format = "0";
                worksheet.Column(9).Style.Numberformat.Format = "dd-mm-yyyy"; // "Date of trip"
                worksheet.Column(9).Width = 15;
                worksheet.Column(10).Style.Numberformat.Format = "hh:mm"; // Begin trip (SAP)
                worksheet.Column(11).Style.Numberformat.Format = "hh:mm"; // Begin trip (Wialon)
                worksheet.Column(13).Style.Numberformat.Format = "hh:mm"; // End trip (SAP)
                worksheet.Column(14).Style.Numberformat.Format = "hh:mm"; // End trip (Wialon)
                worksheet.Column(5).Style.Numberformat.Format = "0.00";
                worksheet.Column(6).Style.Numberformat.Format = "0.00";
                worksheet.Column(7).Style.Numberformat.Format = "0.00";
                worksheet.Column(8).Width = 15;
                worksheet.Column(8).Style.Numberformat.Format = "0.00%";
                #endregion

                int row = 1;
                ExcelDecorator.SetHeaders(worksheet, headerRow, row++);
                
                // Freeze first row
                worksheet.View.FreezePanes(2, 1);


                foreach (var item in valuePairs)
                {                    
                    var data = new List<object[]>()
                    {
                        new object[]
                        {
                            item.StructureName, item.StateNumber, item.UnitModel, item.Type,
                            item.IndicatorMileage.SAP,
                            item.IndicatorMileage.Wialon,
                            item.IndicatorMileage.Difference,
                            item.PercentDifference,
                            null, item.CountTrips.SAP,
                            item.CountTrips.Wialon,
                            null, null, null, null,
                            item.WialonMileageTotal, item.CountSpeedViolations,
                            item.TripsSAP?.ToLookup(x => x.Driver.UnitNumber).Count, null
                        }
                    };
                    string headerRange = $"A{row}:" + Char.ConvertFromUtf32(data[0].Length + 64) + row;

                    ExcelDecorator.SetCellsColor(worksheet.Cells[headerRange], ExcelDecorator.ExcelCssResources.LightYellowColor);

                    //if (item.CountSpeedViolations > 0)
                    //    ExcelDecorator.SetCellsColor(worksheet.Cells[row, 17], ExcelDecorator.ExcelCssResources.LightRedColor);

                    //if (item.CountTrips.SAP > item.CountTrips.Wialon)
                    //    ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.RedColor);
                    

                    if ((item.PercentDifference * 100 >= BadPercent)
                        && (item.IndicatorMileage.SAP > item.IndicatorMileage.Wialon))
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.RedColor);

                    if ((item.PercentDifference * 100 >= BadPercent)
                        && (item.IndicatorMileage.SAP < item.IndicatorMileage.Wialon))
                        ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.YellowColor);

                    worksheet.Cells[row, 9, row, 11].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 15, row, 17].Style.Numberformat.Format = "0";
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
                                    sapElement.TotalMileage, wlnElement.Mileage,
                                    sapElement.TotalMileage - wlnElement.Mileage,
                                    sapElement.TotalMileage.GetPercentFrom(wlnElement.Mileage),
                                    wlnElement.Begin.Date,
                                    sapElement.DepartureFromGarageDate.TimeOfDay, wlnElement.Begin.TimeOfDay, wlnElement.LocationBegin,
                                    sapElement.ReturnToGarageDate.TimeOfDay, wlnElement.Finish.TimeOfDay, wlnElement.LocationFinish,                                    
                                    null, wlnElement.SpeedViolation?.Count(),
                                    sapElement.Driver.ToString(),
                                    sapElement.Driver.UnitNumber
                                }
                            };

                            if (sapElement.TotalMileage.GetPercentFrom(wlnElement.Mileage) * 100 > BadPercent
                                && sapElement.TotalMileage < wlnElement.Mileage)
                                ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.LightYellowColor);
                            else if (sapElement.TotalMileage.GetPercentFrom(wlnElement.Mileage) * 100 > BadPercent
                                && sapElement.TotalMileage > wlnElement.Mileage)
                                ExcelDecorator.SetCellsColor(worksheet.Cells[row, 8], ExcelDecorator.ExcelCssResources.LightRedColor);

                            IWln++;
                            ISap++;
                        }
                        else if ((sapElement != null &&
                            sapElement.DepartureFromGarageDate.Date < wlnElement?.Begin.Date) ||
                            (sapElement != null && wlnElement == null))
                        {
                            dataDetails = new List<object[]>()
                            {
                                new object[]
                                {
                                    null, null, null, null,
                                    sapElement.TotalMileage, null,
                                    null, null,                                    
                                    sapElement.DepartureFromGarageDate.Date,
                                    sapElement.DepartureFromGarageDate.TimeOfDay, null, null,
                                    sapElement.ReturnToGarageDate.TimeOfDay, null, null,
                                    null, null,
                                    sapElement.Driver.ToString(),
                                    sapElement.Driver.UnitNumber
                                }
                            };

                            ISap++;
                        }
                        else if ((wlnElement != null &&
                            sapElement?.DepartureFromGarageDate.Date > wlnElement.Begin.Date) ||
                            (wlnElement != null && sapElement == null))
                        {
                            dataDetails = new List<object[]>()
                            {
                                new object[]
                                {
                                    null, null, null, null,
                                    null, wlnElement.Mileage,
                                    null, null,
                                    wlnElement.Begin.Date,
                                    null, wlnElement.Begin.TimeOfDay, wlnElement.LocationBegin,
                                    null, wlnElement.Finish.TimeOfDay, wlnElement.LocationFinish,                                    
                                    null, wlnElement.SpeedViolation?.Count(),
                                    null, null
                                }
                            };

                            IWln++;
                        }

                        if (!dataDetails.Any())
                            continue;

                        string headerRangelocal = $"A{row}:" + Char.ConvertFromUtf32(dataDetails[0].Length + 64) + row;
                        worksheet.Cells[headerRangelocal].LoadFromArrays(dataDetails);

                        worksheet.Row(row).OutlineLevel = 1;
                        row++;
                    }
                }

                AddSAPCarsWorksheet(excel, sapCars);
                AddWialonCarsWorksheet(excel, wialonCars);

                if (File.Exists(path))
                    File.Delete(path);
                var excelFile = new System.IO.FileInfo(path);
                excel.SaveAs(excelFile);
            }
        }


        private void AddSAPCarsWorksheet(ExcelPackage excel, List<IVehicleSAP> sapCars)
        {
            #region Filling distinct cars
            if (sapCars != null)
            {
                var worksheet = excel.Workbook.Worksheets.Add("ТС с ПЛ без пробега");
                worksheet.OutLineSummaryBelow = false;

                var headerRow = new List<string[]>()
                    {
                        new string[] {
                            "Подразделение",
                            "Гос.номер", "Модель", "Тип",
                            "Дата выезда", "Начало (время SAP)", "Конец (время SAP)",
                            "Показания одометра (SAP)",
                            "Водитель", "Табельный номер водителя" }
                    };


                #region Columns` Style
                worksheet.Column(5).Style.Numberformat.Format = "dd-mm-yyyy"; // "Date of trip"
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Style.Numberformat.Format = "hh:mm"; // Begin trip 
                worksheet.Column(7).Style.Numberformat.Format = "hh:mm"; // End trip 
                worksheet.Column(8).Style.Numberformat.Format = "0.00";
                #endregion

                var row = 1;
                ExcelDecorator.SetHeaders(worksheet, headerRow, row++);

                for (int index = 0; index < sapCars.Count; index++)
                {
                    var data = new List<object[]>()
                        {
                            new object[]
                            {
                                sapCars[index].StructureName,
                                sapCars[index].StateNumber, sapCars[index].UnitModel, sapCars[index].Type,
                                sapCars[index].Trips?.Count, null, null,
                                sapCars[index].TripResulted?.TotalMileage,
                                sapCars[index].Trips?.ToLookup(x => x.Driver.UnitNumber)?.Count, null
                            }
                        };

                    worksheet.Cells[row, 5].Style.Numberformat.Format = "0"; // Total trips 

                    string headerRange = $"A{row}:" + Char.ConvertFromUtf32(data[0].Length + 64) + row;
                    ExcelDecorator.SetCellsColor(worksheet.Cells[headerRange], ExcelDecorator.ExcelCssResources.LightYellowColor);

                    worksheet.Row(row).Style.Font.SetFromFont(new Font("Times New Roman", 12, FontStyle.Bold));
                    worksheet.Cells[headerRange].LoadFromArrays(data);

                    row++;
                    if(sapCars[index].Trips != null)
                        foreach (var item in sapCars[index].Trips)
                        {
                            var dataDetails = new List<object[]>()
                            {
                                new object[]
                                {
                                    null,
                                    null, null, null,
                                    item.DepartureFromGarageDate.Date, item.DepartureFromGarageDate.TimeOfDay, item.ReturnToGarageDate.TimeOfDay,
                                    item.TotalMileage,
                                    item.Driver, item.Driver?.UnitNumber
                                }
                            };

                            if (!dataDetails.Any())
                                continue;

                            string headerRangelocal = $"A{row}:" + Char.ConvertFromUtf32(dataDetails[0].Length + 64) + row;
                            worksheet.Cells[headerRangelocal].LoadFromArrays(dataDetails);
                            worksheet.Row(row++).OutlineLevel = 1;
                        }
                }

            }
            #endregion
        }

        private void AddWialonCarsWorksheet(ExcelPackage excel, List<CarWialon> wialonCars)
        {
            #region Filling distinct cars
            if (wialonCars != null)
            {
                var worksheet = excel.Workbook.Worksheets.Add("ТС с пробегом без ПЛ");
                worksheet.OutLineSummaryBelow = false;

                var headerRow = new List<string[]>()
                    {
                        new string[] { "ID", "Гос.номер" }
                    };

                var row = 1;
                ExcelDecorator.SetHeaders(worksheet, headerRow, row++);

                for (int index = 0; index < wialonCars.Count; index++)
                {
                    var data = new List<object[]>()
                        {
                            new object[] { wialonCars[index].ID.ToString(), wialonCars[index].StateNumber }
                        };
                    
                    string headerRange = $"A{row}:" + Char.ConvertFromUtf32(data[0].Length + 64) + row;
                    ExcelDecorator.SetCellsColor(worksheet.Cells[headerRange], ExcelDecorator.ExcelCssResources.LightYellowColor);

                    worksheet.Row(row).Style.Font.SetFromFont(new Font("Times New Roman", 12, FontStyle.Bold));
                    worksheet.Cells[headerRange].LoadFromArrays(data);
                    row++;
                    
                }

            }
            #endregion
        }
    }
}
