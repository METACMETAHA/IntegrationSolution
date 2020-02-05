using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace IntegrationSolution.Common.Helpers
{
    public static class ConfigureContainerHelper
    {
        public static void RegisterView<TView, TViewModel>(this IUnityContainer _container, string viewName, LifetimeManager lifetime = null)
            where TView : UserControl
            where TViewModel : BindableBase
        {
            if (string.IsNullOrEmpty(viewName)) throw new ArgumentNullException(nameof(viewName));
            if (!_container.IsRegistered<TViewModel>())
                _container.RegisterType<TViewModel>();

            if (lifetime == null)
                lifetime = new TransientLifetimeManager();

            _container.RegisterType<UserControl, TView>(viewName,
                lifetime,
                new InjectionProperty("DataContext", new ResolvedParameter<TViewModel>()));
        }
    }
}
