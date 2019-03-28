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
        //private SiteData _siteData;
        //public SiteData SiteData
        //{
        //    get { return _siteData; }
        //    set { SetProperty(ref _siteData, value); }
        //}
        #endregion Properties


        public VMLocalBase(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            
            SubmitCommand = new DelegateCommand(Submit, CanSubmit);
        }


        #region Commands
        public DelegateCommand SubmitCommand { get; private set; }

        protected abstract void Submit();

        protected bool CanSubmit()
        {
            return true;
        }
        #endregion Commands
    }
}
