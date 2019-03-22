using IntegrationSolution.Entities.Helpers;
using IntegrationSolution.Entities.Implementations;
using IntegrationSolution.Entities.Implementations.Fuel;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using System;
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
            
            for (int i = 0; i < data.Count(); i++)
            {
                var obj = data.ElementAt(i);
                System.Console.WriteLine(obj.StateNumber + "\t" + obj.UnitModel);

                var dangerTrips = obj.TripsWithMileageDeviation();
                if (dangerTrips?.Count > 0)
                {
                    foreach (var item in dangerTrips)
                    {
                        System.Console.WriteLine("\t\t" + item.TotalMileage + "km");
                    }
                }
            }

            System.Console.WriteLine("Count:\t" + data.Count());
        }
    }
}
