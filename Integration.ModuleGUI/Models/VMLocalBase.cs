using IntegrationSolution.Common.Events;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using MahApps.Metro.Controls;
using Prism.Events;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using Unity;
using WialonBase.Interfaces;

namespace Integration.ModuleGUI.Models
{
    public abstract class VMLocalBase : ViewModelBase
    {
        protected INavigationOperations _wialonContext;

        #region Properties
        private CommonModuleData _moduleData;
        public CommonModuleData ModuleData
        {
            get { return _moduleData; }
            set { SetProperty(ref _moduleData, value); }
        }
        #endregion Properties


        public VMLocalBase(
            IUnityContainer container, 
            IEventAggregator ea) : base(container, ea)
        {
            ModuleData = container.Resolve<CommonModuleData>();
            _wialonContext = _container.Resolve<INavigationOperations>();
        }


        #region Events
        public override void OnEnter()
        {
            base.OnEnter();
            _eventAggregator.GetEvent<WialonConnectionEvent>().Subscribe(UpdateNavigationConnection);
        }

        public override void OnExit()
        {
            base.OnExit();
            _eventAggregator.GetEvent<WialonConnectionEvent>().Unsubscribe(UpdateNavigationConnection);
        }
        #endregion


        #region EventActions
        protected async void UpdateNavigationConnection(bool obj)
        {
            if (obj)
                _wialonContext = _container.Resolve<INavigationOperations>();
            else
                _wialonContext = null;
        }
        #endregion EventActions
    }
}
