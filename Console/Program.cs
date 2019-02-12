using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Excel.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityIoC;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Startup();

            var file = @"..\..\export.xlsx";

            ExcelWorker ex = new ExcelWorker(new OfficeOpenXml.ExcelPackage(new FileInfo(file)));
            var data = ex.GetVehicle<Car>();
            System.Console.WriteLine(data.Count());
        }
    }
}
