using Integration.Flyouts.Implementations;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Interfaces;
using NotificationConstructor.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Flyouts.ViewModels
{
    public class SettingsViewModel : FlyoutVMBase
    {
        public SettingsViewModel(
            AppConfiguration settings,
            INotificationManager notificationManager) 
            : base(
                  settings,
                  notificationManager)
        {
            this.Header = "Настройки";
        }
    }
}
