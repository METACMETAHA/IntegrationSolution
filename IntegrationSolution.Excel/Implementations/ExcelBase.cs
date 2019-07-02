using IntegrationSolution.Excel.Common;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace IntegrationSolution.Excel.Implementations
{
    public abstract class ExcelBase : IExcel, IExcelBorders, IDisposable
    {
        #region Properties
        public ExcelPackage Excel { get; protected set; }
        public ExcelWorkbook Workbook { get; private set; }
        public ExcelWorksheet CurrentWorkSheet { get; private set; }
        public ExcelCellAddress StartCell { get; private set; }
        public ExcelCellAddress EndCell { get; private set; }
        public IExcelStyle ExcelDecorator { get; private set; }

        public ExcelWorksheet this[string name]
        {
            get { return Workbook.Worksheets[name]; }
        }
        #endregion


        protected IUnityContainer container;

        public ExcelBase(ExcelPackage excelPackage, IUnityContainer unityContainer)
        {
            Excel = excelPackage;
            container = unityContainer;
            ExcelDecorator = container.Resolve<IExcelStyle>();
            TryClearFromPathList();
            new StaticHelper(unityContainer);
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
                    CurrentWorkSheet.DeleteRow(item.Row);
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
                CurrentWorkSheet = Workbook?.Worksheets.First();

                SetBorders();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// This function sets borders: startCell, endCell
        /// </summary>
        public void SetBorders()
        {
            if (CurrentWorkSheet == null)
                TryOpen();

            StartCell = CurrentWorkSheet.Dimension.Start;
            EndCell = CurrentWorkSheet.Dimension.End;
        }


        public void Save()
        {
            this.Excel.Save();
        }


        public ExcelWorksheet AddWorksheet(string name)
        {
            return Workbook.Worksheets.Add(name);
        }


        public ExcelWorksheet MoveToWorkSheet(string name)
        {
            var worksheet = this[name];
            if (worksheet != null)
            {
                CurrentWorkSheet = worksheet;
                return CurrentWorkSheet;
            }
            return null;
        }


        public void Dispose()
        {
            Excel?.Dispose();
        }
    }
}
