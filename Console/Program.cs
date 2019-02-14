using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Implementations.Fuel;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel.Implementations;
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
        static void Main(string[] args)
        {
            Bootstrapper.Startup();

            var file = @"..\..\export.xlsx";

            ExcelWorker ex = new ExcelWorker(new OfficeOpenXml.ExcelPackage(new FileInfo(file)));
            var data = ex.GetVehicle<Car>();

            IFuel ff = new FuelBase<Gas>("Gas");
            
            for (int i = 0; i < 20; i++)
            {
                System.Console.WriteLine(data.ElementAt(i).StateNumber + "\t" + data.ElementAt(i).UnitModel + "\t" + data.ElementAt(i).UnitNumber);
            }

            System.Console.WriteLine("Count:\t" + data.Count());
        }
    }
}
