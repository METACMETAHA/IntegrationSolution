using Integration.Flyouts.ViewModels;
using IntegrationSolution.Common.Events;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;
using WialonBase.Interfaces;

namespace IntegrationSolution.ShellGUI.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;


        #region Properties
        private bool _isConnectedNavigation;
        public bool IsConnectedNavigation
        {
            get { return _isConnectedNavigation; }
            set
            {
                var res = false;
                if (value)
                    res = _container.Resolve<INavigationOperations>().TryConnect();
                else
                    res = _container.Resolve<INavigationOperations>().TryClose();
                if (res == true)
                {
                    if (value == false)
                        _eventAggregator.GetEvent<WialonConnectionEvent>().Publish(false);
                    else
                        _eventAggregator.GetEvent<WialonConnectionEvent>().Publish(true);

                    SetProperty(ref _isConnectedNavigation, value);
                }
                else
                {
                    var wnd = (MetroWindow)Application.Current.MainWindow;
                    wnd.ShowMessageAsync("Ошибка!", "Проблема в подключении, обратитесь в поддержку.");

                    SetProperty(ref _isConnectedNavigation, false);
                }
                
                _eventAggregator.GetEvent<WialonConnectionEvent>().Publish(IsConnectedNavigation);
            }
        }
        #endregion


        public MainWindowViewModel(IUnityContainer container, IEventAggregator ea)
        {
            _container = container;
            _eventAggregator = ea;
            ToggleFlyoutSettingsCommand = new DelegateCommand(ToggleSettings);
        }


        #region Commands
        public ICommand ToggleFlyoutSettingsCommand { get; private set; }
        private void ToggleSettings()
        {
            var settings = _container.Resolve<SettingsViewModel>();
            settings.IsOpen = !settings.IsOpen;
        }
        #endregion
    }
}
