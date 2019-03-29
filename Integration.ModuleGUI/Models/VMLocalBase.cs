using IntegrationSolution.Common.ModulesExtension.Implementations;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.ModuleGUI.Models
{
    public abstract class VMLocalBase : ViewModelBase
    {
        #region Properties
        private CommonModuleData _moduleData;
        public CommonModuleData ModuleData
        {
            get { return _moduleData; }
            set { SetProperty(ref _moduleData, value); }
        }
        #endregion Properties


        public VMLocalBase(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            ModuleData = container.Resolve<CommonModuleData>();
        }


        #region Commands
        #endregion Commands
    }
}
