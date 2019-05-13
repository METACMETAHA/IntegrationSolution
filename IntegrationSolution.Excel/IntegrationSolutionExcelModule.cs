using IntegrationSolution.Excel.Common;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;

namespace IntegrationSolution.Excel
{
    public class IntegrationSolutionExcelModule : IModule
    {
        private UnityContainer _container;

        public IntegrationSolutionExcelModule(UnityContainer container)
        {
            _container = container;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IExcelStyle, ExcelStyle>();

            containerRegistry.Register<ExcelPackage>();
            containerRegistry.Register<IExcel, ExcelBase>();
            containerRegistry.Register<ICarOperations, ExcelCarOperations>(); 
            containerRegistry.Register<IExcelWriter, ExcelReportWriter>();
            
        }
    }
}
