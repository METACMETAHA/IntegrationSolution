using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Interfaces
{
    public interface IExcel
    {
        ExcelPackage Excel { get; }
        ExcelWorkbook Workbook { get; }
        ExcelWorksheet CurrentWorkSheet { get; }

        ExcelWorksheet this[string name] { get; }
        ExcelWorksheet AddWorksheet(string name);
        ExcelWorksheet MoveToWorkSheet(string name);

        void TryOpen();
        void Save();
    }
}
