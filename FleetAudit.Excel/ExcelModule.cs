using FleetAudit.Excel.Common;
using FleetAudit.Excel.Implementations;
using FleetAudit.Excel.Interfaces;
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

namespace FleetAudit.Excel
{
    public class ExcelModule : IModule
    {
        private UnityContainer _container;

        public ExcelModule(UnityContainer container)
        {
            _container = container;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IExcelStyle, ExcelStyle>();
            containerRegistry.RegisterSingleton<HeaderNames>();

            containerRegistry.Register<ExcelPackage>();
            containerRegistry.Register<IExcel, ExcelBase>();
            containerRegistry.Register<ICarOperations, ExcelCarOperations>(); 
            containerRegistry.Register<IExcelWriter, ExcelReportWriter>();
            
        }
    }
}
