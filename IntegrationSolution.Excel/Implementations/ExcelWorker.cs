using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelWorker : IOperations, IDisposable
    {
        public ExcelPackage excel { get; private set; }

        public ExcelWorker(ExcelPackage excelPackage)
        {
            excel = excelPackage;
        }

        public void Dispose()
        {
            excel?.Dispose();
        }

        public void GetCars()
        {
            var workbook = excel.Workbook;
            var worksheet = workbook.Worksheets.First();
            var data = worksheet.Tables.First(); 
        }

    }
}
