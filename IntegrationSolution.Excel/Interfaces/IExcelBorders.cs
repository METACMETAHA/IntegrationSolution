using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Interfaces
{
    public interface IExcelBorders
    {
        ExcelCellAddress StartCell { get; }
        ExcelCellAddress EndCell { get; }

        void SetBorders();
    }
}
