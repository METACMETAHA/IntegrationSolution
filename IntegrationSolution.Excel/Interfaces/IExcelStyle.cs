using IntegrationSolution.Excel.Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Interfaces
{
    public interface IExcelStyle
    {
        CssExcelStorage ExcelCssResources { get; }

        void AddHeader(ExcelWorksheet WorkSheet, int row, int column, string Title);

        void SetHeaders(ExcelWorksheet WorkSheet, List<string[]> headerRow, int row = 1);

        void SetCellsColor(ExcelRange excelRange, Color color, ExcelFillStyle fillType = ExcelFillStyle.Gray125);
        
    }
}
