using IntegrationSolution.Excel.Common;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public abstract class ExcelBase : IExcel, IExcelBorders, IDisposable
    {
        #region Properties
        public ExcelPackage Excel { get; protected set; }
        public ExcelWorkbook Workbook { get; private set; }
        public ExcelWorksheet WorkSheet { get; private set; }
        public ExcelCellAddress StartCell { get; private set; }
        public ExcelCellAddress EndCell { get; private set; }
        #endregion

        private StyleExcel _styleExcel;


        public ExcelBase(ExcelPackage excelPackage)
        {
            Excel = excelPackage;
            _styleExcel = new StyleExcel();
            TryClearFromPathList();
        }


        public void TryClearFromPathList()
        {
            if (StaticHelper.GetHeadersAddress(this, HeaderNames.PathListStatus).Count == 0)
                return;

            var rows = StaticHelper.GetRowsWithValue(this,
                PathListData.PathListStatusDictionary[IntegrationSolution.Common.Enums.PathListStatusEnum.Miv],
                HeaderNames.PathListStatus);

            foreach (var item in rows)
            {
                try
                {
                    WorkSheet.DeleteRow(item.Row);
                }
                catch (Exception)
                { }
            }
        }


        /// <summary>
        /// This function is trying to open Excel and set the next values: workbook, worksheet, startCell, endCell
        /// </summary>
        public void TryOpen()
        {
            try
            {
                Workbook = Excel?.Workbook;
                WorkSheet = Workbook?.Worksheets.First();

                SetBorders();
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// This function sets borders: startCell, endCell
        /// </summary>
        public void SetBorders()
        {
            if (WorkSheet == null)
                TryOpen();

            StartCell = WorkSheet.Dimension.Start;
            EndCell = WorkSheet.Dimension.End;
        }


        public void Save()
        {
            this.Excel.Save();
        }


        public void Dispose()
        {
            Excel?.Dispose();
        }


        public void AddHeader(int row, int column, string Title)
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
                this.SetBorders();
            }
            catch (Exception)
            {
                
            }
        }
    }
}
