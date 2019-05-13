using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Interfaces
{
    public interface IExcelStyle
    {
        void AddHeader(ExcelWorksheet WorkSheet, int row, int column, string Title);

        void SetHeaders(ExcelWorksheet WorkSheet, List<string[]> headerRow, int row = 1);
    }
}
