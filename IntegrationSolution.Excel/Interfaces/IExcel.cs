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
        ExcelWorksheet WorkSheet { get; }

        void TryOpen();
        void Save();
    }
}
