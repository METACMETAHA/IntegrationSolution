using Integration.Flyouts;
using Integration.Flyouts.ViewModels;
using Integration.Infrastructure.Views.Account;
using Integration.Infrastructure.Views.Logistics;
using IntegrationSolution.Common.Events;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Prism.Commands;
using Prism.Events;
using Prism.Modularity;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Controls;
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
            _eventAggregator = ea;
            ToggleFlyoutSettingsCommand = new DelegateCommand(ToggleSettings);

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
                    //Tag = new AboutViewModel(this)
                }
            };
        }
        #endregion
    }
}
