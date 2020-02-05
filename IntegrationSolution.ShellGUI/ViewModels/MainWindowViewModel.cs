using Integration.Flyouts;
using Integration.Flyouts.ViewModels;
using Integration.Infrastructure.Views.Account;
using Integration.Infrastructure.Views.Info;
using Integration.Infrastructure.Views.Logistics;
using IntegrationSolution.Common.Events;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using NotificationConstructor.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Unity;
using WialonBase.Interfaces;

namespace IntegrationSolution.ShellGUI.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly INotificationManager _notificationManager;
        private readonly Timer _timer;       
        

        #region Properties
        private bool _isConnectedNavigation;
        public bool IsConnectedNavigation
        {
            get { return _isConnectedNavigation; }
            set
            {
                IsEnabledNavigation = false;
                var res = false;
                if (value)
                    res = _container.Resolve<INavigationOperations>().TryConnect();
                else
                    res = _container.Resolve<INavigationOperations>().TryClose();
                if (res)
                {
                    if (!value)
                        _eventAggregator.GetEvent<WialonConnectionEvent>().Publish(false);
                    else
                        _eventAggregator.GetEvent<WialonConnectionEvent>().Publish(true);

                    SetProperty(ref _isConnectedNavigation, value);
                }
                else
                {
                    SetProperty(ref _isConnectedNavigation, false);
                }

                if (IsConnectedNavigation == true)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        _notificationManager.NotifySuccessAsync("Подключено к Wialon!");
                    }));
                    _timer.Start();
                }
                else
                    _timer.Stop();

                _eventAggregator.GetEvent<WialonConnectionEvent>().Publish(IsConnectedNavigation);
                IsEnabledNavigation = true;
            }
        }


        private bool _isEnabledNavigation;
        public bool IsEnabledNavigation
        {
            get { return _isEnabledNavigation; }
            set
            {
                SetProperty(ref _isEnabledNavigation, value);
            }
        }


        private HamburgerMenuItemCollection _menuItems;
        public HamburgerMenuItemCollection MenuItems
        {
            get { return _menuItems; }
            set
            {
                SetProperty(ref _menuItems, value);
            }
        }


        private HamburgerMenuItemCollection _menuOptionItems;
        public HamburgerMenuItemCollection MenuOptionItems
        {
            get { return _menuOptionItems; }
            set
            {
                SetProperty(ref _menuOptionItems, value);
            }
        }
        #endregion


        public MainWindowViewModel(IUnityContainer container, IEventAggregator ea)
        {
            _container = container;
            _notificationManager = _container.Resolve<INotificationManager>();
            _eventAggregator = ea;
            _timer = new Timer(840000); // 14min. Session live 15min. - 840000ms
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            
            ToggleFlyoutSettingsCommand = new DelegateCommand(ToggleSettings);
            
            IsEnabledNavigation = true;
            this.CreateMenuItems();
        }
        

        #region Commands
        public ICommand ToggleFlyoutSettingsCommand { get; private set; }
        private void ToggleSettings()
        {
            _container.Resolve<IModuleManager>().LoadModule(nameof(FlyoutsModule));            

            var settings = _container.Resolve<SettingsViewModel>();
            settings.IsOpen = !settings.IsOpen;
        }
        #endregion


        #region Helpers
        public void CreateMenuItems()
        {
            MenuItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = Application.Current.TryFindResource("appbar_home_garage_open"),
                    Label = "Главная",
                    ToolTip = "В разработке.",
                    Tag = _container.Resolve<UserControl>(nameof(HomeView))
                },
                new HamburgerMenuIconItem()
                {
                    Icon = Application.Current.TryFindResource("appbar_scale_unbalanced"),
                    Label = "Операции",
                    ToolTip = "SAP + Wialon.",
                    Tag = _container.Resolve<UserControl>(nameof(LogisticsQuizView))
                }
                //new HamburgerMenuIconItem()
                //{
                //    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.Settings},
                //    Label = "Settings",
                //    ToolTip = "The Application settings.",
                //    Tag = new SettingsViewModel(this)
                //}
            };

            MenuOptionItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = Application.Current.TryFindResource("appbar_information"),
                    Label = "Справка",
                    ToolTip = "Справка.",
                    Tag = _container.Resolve<UserControl>(nameof(InfoView))
                }
            };
        }

        // Timer for control Wialon connection
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_container.Resolve<INavigationOperations>().TryConnect())
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _notificationManager.NotifySuccessAsync("Сеанс подключения к системе Wialon продолжен!");
                }));
            }
            else
            {
                this.IsConnectedNavigation = false;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _notificationManager.NotifyInformationAsync("Сеанс подключения к системе Wialon истек!");
                }));

                _timer.Stop();
            }
            
        }
        #endregion
    }
}
