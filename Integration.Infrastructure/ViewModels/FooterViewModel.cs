using IntegrationSolution.Common.Entities;
using IntegrationSolution.Common.Events;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.Infrastructure.ViewModels
{
    public class FooterViewModel : BindableBase
    {
        #region Properties
        private Error _status;
        public Error Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }
        #endregion Properties

        IUnityContainer cont;
        public FooterViewModel(IUnityContainer container, IEventAggregator ea)
        {
            cont = container;
            ea.GetEvent<StatusUpdateEvent>().Subscribe((error) => Status = error);
        }
    }
}
