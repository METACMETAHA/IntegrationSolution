using IntegrationSolution.Excel;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace UnityIoC
{
    public class Bootstrapper //: UnityBootstrapper
    {
        private static UnityContainer _container;

        public static void Startup()
        {
            _container = new UnityContainer();

            _container.RegisterType<ExcelPackage>();
            _container.RegisterType<IUnityContainer, UnityContainer>();
            _container.RegisterType<IExcel, ExcelBase>();
        }

        //protected override void ConfigureModuleCatalog()
        //{
        //    var catalog = (ModuleCatalog)ModuleCatalog;
        //    catalog.AddModule(typeof(IntegrationSolutionExcelModule));
        //}
    }
}
