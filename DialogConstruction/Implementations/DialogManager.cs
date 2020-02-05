using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DialogConstruction.Interfaces;
using IntegrationSolution.Common.Enums;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Unity;

namespace DialogConstruction.Implementations
{
    public class DialogManager : IDialogManager
    {
        private readonly IUnityContainer _container;

        public DialogManager(IUnityContainer container)
        { _container = container; }

        public async Task ShowDialogAsync(DialogViewModel viewModel, MetroDialogSettings settings = null)
        {
            //var view = _container. //ViewLocator.GetViewForViewModel(viewModel);
            var view = _container.Resolve<BaseMetroDialog>();

            var dialog = view;// as BaseMetroDialog;
            if (dialog == null)
            {
                throw new InvalidOperationException($"The view {view.GetType()} belonging to view model {viewModel.GetType()} does not inherit from {typeof(BaseMetroDialog)}");
            }

            dialog.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/FlatButton.xaml")
            });

            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();
            await firstMetroWindow.ShowMetroDialogAsync(dialog, settings);
            await viewModel.Task;
            await firstMetroWindow.HideMetroDialogAsync(dialog, settings);
        }

        public async Task ShowDialogAsync(DialogNamesEnum View, MetroDialogSettings settings = null)
        {
            var view = _container.Resolve<BaseMetroDialog>(View.ToString());
            var dialog = view as BaseMetroDialog;
            if (dialog == null)
            {
                throw new InvalidOperationException($"The view {view.GetType()} belonging to view model {dialog.DataContext.GetType()} does not inherit from {typeof(BaseMetroDialog)}");
            }

            dialog.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/FlatButton.xaml")
            });

            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();

            await firstMetroWindow.ShowMetroDialogAsync(dialog, settings);
            await (dialog.DataContext as DialogViewModel).Task;
            await firstMetroWindow.HideMetroDialogAsync(dialog, settings);
        }

        public Task ShowDialogAsync<TViewModel>(MetroDialogSettings settings = null) where TViewModel : DialogViewModel
        {
            var viewModel = _container.Resolve<TViewModel>();
            return ShowDialogAsync(viewModel, settings);
        }

        public async Task<TResult> ShowDialogAsync<TResult>(DialogViewModel<TResult> viewModel, MetroDialogSettings settings = null)
        {
            //var view = ViewLocator.GetViewForViewModel(viewModel);
            var view = _container.Resolve<BaseMetroDialog>();

            var dialog = view;//as BaseMetroDialog;
            if (dialog == null)
            {
                throw new InvalidOperationException($"The view {view.GetType()} belonging to view model {viewModel.GetType()} does not inherit from {typeof(BaseMetroDialog)}");
            }

            dialog.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/FlatButton.xaml")
            });

            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();
            
            await firstMetroWindow.ShowMetroDialogAsync(dialog, settings);
            var result = await viewModel.Task;
            await firstMetroWindow.HideMetroDialogAsync(dialog, settings);

            return result;
        }
        
        public async Task<TResult> ShowDialogAsync<TResult>(DialogNamesEnum View, MetroDialogSettings settings = null)
        {
            var view = _container.Resolve<BaseMetroDialog>(View.ToString());
            var dialog = view as BaseMetroDialog;
            if (dialog == null)
            {
                throw new InvalidOperationException($"The view {view.GetType()} belonging to view model {dialog.DataContext.GetType()} does not inherit from {typeof(BaseMetroDialog)}");
            }

            dialog.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/FlatButton.xaml")
            });

            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();

            await firstMetroWindow.ShowMetroDialogAsync(dialog, settings);
            var result = await (dialog.DataContext as DialogViewModel<TResult>).Task;
            await firstMetroWindow.HideMetroDialogAsync(dialog, settings);

            return result;
        }

        public async Task<TResult> ShowDialogAsync<TResult>(DialogNamesEnum View, 
            DialogViewModel<TResult> ViewModel, MetroDialogSettings settings = null)
        {
            var view = _container.Resolve<BaseMetroDialog>(View.ToString());
            var dialog = view as BaseMetroDialog;
            if (dialog == null)
            {
                throw new InvalidOperationException($"The view {view.GetType()} belonging to view model {dialog.DataContext.GetType()} does not inherit from {typeof(BaseMetroDialog)}");
            }

            dialog.DataContext = ViewModel;
            dialog.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/FlatButton.xaml")
            });

            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();

            await firstMetroWindow.ShowMetroDialogAsync(dialog, settings);
            var result = await (dialog.DataContext as DialogViewModel<TResult>).Task;
            await firstMetroWindow.HideMetroDialogAsync(dialog, settings);

            return result;
        }

        public Task<TResult> ShowDialogAsync<TViewModel, TResult>(MetroDialogSettings settings = null) where TViewModel : DialogViewModel<TResult>
        {
            var viewModel = _container.Resolve<TViewModel>();

            return ShowDialogAsync(viewModel, settings);
        }

        public Task<MessageDialogResult> ShowMessageBox(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();
            return firstMetroWindow.ShowMessageAsync(title, message, style, settings);
        }
        
    }
}
