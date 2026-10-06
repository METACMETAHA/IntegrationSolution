using Integration.Flyouts;
using Integration.Flyouts.ViewModels;
using Integration.Infrastructure.Views.Account;
using Integration.Infrastructure.Views.Info;
using Integration.Infrastructure.Views.Logistics;
using IntegrationSolution.Common.Events;
using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Resources;
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
        private readonly ILocalizationService _localization;
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
                        _notificationManager.NotifySuccessAsync(Strings.Shell_ConnectedToWialon);
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
            _localization = _container.Resolve<ILocalizationService>();
            _eventAggregator = ea;
            _timer = new Timer(840000); // 14min. Session live 15min. - 840000ms
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            
            ToggleFlyoutSettingsCommand = new DelegateCommand(ToggleSettings);
            
            IsEnabledNavigation = true;
            this.CreateMenuItems();

            LanguageChangedEventManager.AddHandler(_localization, OnLanguageChanged);
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
                    Label = Strings.Shell_MenuHome,
                    ToolTip = Strings.Shell_MenuHomeToolTip,
                    Tag = _container.Resolve<UserControl>(nameof(HomeView))
                },
                new HamburgerMenuIconItem()
                {
                    Icon = Application.Current.TryFindResource("appbar_scale_unbalanced"),
                    Label = Strings.Shell_MenuOperations,
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
                    Label = Strings.Shell_MenuHelp,
                    ToolTip = Strings.Shell_MenuHelpToolTip,
                    Tag = _container.Resolve<UserControl>(nameof(InfoView))
                }
            };
        }

        /// <summary>
        /// Menu items are created in code, so their texts are refreshed in place
        /// (re-creating the items would reset the selected page).
        /// </summary>
        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            SetMenuTexts(MenuItems, 0, Strings.Shell_MenuHome, Strings.Shell_MenuHomeToolTip);
            SetMenuTexts(MenuItems, 1, Strings.Shell_MenuOperations, null);
            SetMenuTexts(MenuOptionItems, 0, Strings.Shell_MenuHelp, Strings.Shell_MenuHelpToolTip);
        }

        private static void SetMenuTexts(HamburgerMenuItemCollection items, int index, string label, string toolTip)
        {
            var item = items != null && index < items.Count ? items[index] as HamburgerMenuItem : null;
            if (item == null)
                return;

            item.Label = label;
            if (toolTip != null)
                item.ToolTip = toolTip;
        }

        // Timer for control Wialon connection
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_container.Resolve<INavigationOperations>().TryConnect())
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _notificationManager.NotifySuccessAsync(Strings.Shell_WialonSessionExtended);
                }));
            }
            else
            {
                this.IsConnectedNavigation = false;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _notificationManager.NotifyInformationAsync(Strings.Shell_WialonSessionExpired);
                }));

                _timer.Stop();
            }
            
        }
        #endregion
    }
}
