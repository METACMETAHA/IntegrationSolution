using IntegrationSolution.Excel.Common;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

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
            _container.RegisterSingleton<StyleExcel>();
        }
    }
}
