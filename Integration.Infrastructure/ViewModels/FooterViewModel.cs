using Integration.Infrastructure.Constants;
using IntegrationSolution.Common.Entities;
using IntegrationSolution.Common.Events;
using log4net;
using log4net.Core;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Unity;

namespace Integration.Infrastructure.ViewModels
{
    public class FooterViewModel : BindableBase
    {
        private readonly ILog _logger;

        #region Properties
        private Error _status;
        public Error Status
        {
            get { return _status; }
            set
            {
                if (value != null && !string.IsNullOrWhiteSpace(value.ErrorDescription))
                {
                    #region WriteLog
                    if (value.IsError == true)
                        _logger.Error(value.ErrorDescription);
                    else
                        _logger.Debug(value.ErrorDescription);
                    #endregion

                    LogData.Add(value);
                    if (LogData.Count > 150)
                        LogData.Remove(LogData.First());
                }

                SetProperty(ref _status, value);
            }
        }

        private ConcurrentObservableCollection<Error> _logData;
        public ConcurrentObservableCollection<Error> LogData
        {
            get { return _logData; }
            set
            { SetProperty(ref _logData, value); }
        }
        #endregion Properties


        public FooterViewModel(IUnityContainer container, IEventAggregator ea)
        {
            _logger = LogManager.GetLogger(this.GetType());
            LogData = new ConcurrentObservableCollection<Error>();
            ea.GetEvent<StatusUpdateEvent>().Subscribe((error) => Status = error);
        }
    }
}
