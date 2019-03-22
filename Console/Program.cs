using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Implementations.Fuel;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityIoC;

namespace Console
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Bootstrapper.Startup();

            var fileMain = @"..\..\Main2.xlsx";
            var file = @"..\..\export.xlsx";

            ICarOperations excel = new ExcelCarOperations(new OfficeOpenXml.ExcelPackage(new FileInfo(fileMain)));
            var data = excel.GetVehicles();

            ICarOperations ex = new ExcelCarOperations(new OfficeOpenXml.ExcelPackage(new FileInfo(file)));
            for (int i = 0; i < data.Count(); i++)
            {
                var v = data.ElementAtOrDefault(i);
                if(v != null)
                    ex.FillVehicleAvaliableData(ref v);
            }
            
            for (int i = 0; i < 20; i++)
            {
                System.Console.WriteLine(data.ElementAt(i).StateNumber + "\t" + data.ElementAt(i).UnitModel);
            }

            System.Console.WriteLine("Count:\t" + data.Count());
        }
    }
}
