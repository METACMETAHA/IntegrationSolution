using Integration.Flyouts.Implementations;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Flyouts.ViewModels
{
    public class SettingsViewModel : FlyoutVMBase
    {
        public SettingsViewModel(AppConfiguration settings) : base(settings)
        {
            this.Header = "Настройки";
        }
    }
}
