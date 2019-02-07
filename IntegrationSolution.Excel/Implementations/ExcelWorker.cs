using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Implementations
{
    public class ExcelWorker : IOperations
    {
        public ExcelPackage excel { get; private set; }

        public ExcelWorker(ExcelPackage excelPackage)
        {
            excel = excelPackage;
        }

        public void GetCars()
        {
            
        }
    }
}
