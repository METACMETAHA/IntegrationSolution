using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace IntegrationSolution.Localization.Markup
{
    internal static class LocalizationBindingHelper
    {
        /// <summary>
        /// One-way binding to <c>LocalizationService.Instance[key]</c>; it refreshes automatically
        /// when the UI language changes. The explicit Source makes it independent of the DataContext,
        /// so it also works in templates, styles, context menus, flyouts and child windows.
        /// </summary>
        public static Binding CreateKeyBinding(string key)
        {
            return new Binding
            {
                Source = LocalizationService.Instance,
                Path = new PropertyPath("[" + key + "]"),
                Mode = BindingMode.OneWay,
            };
        }

        /// <summary>
        /// Returns the binding (expression) for dependency-property and setter targets.
        /// Plain CLR properties cannot hold bindings, so they receive the current text instead of failing to load.
        /// </summary>
        public static object ProvideValue(BindingBase binding, IServiceProvider serviceProvider, Func<object> snapshot)
        {
            var target = serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            // Dependency properties (including template content, where WPF defers the binding per instance)
            // and style/trigger setters accept bindings directly.
            if (target == null || target.TargetProperty is DependencyProperty || target.TargetObject is SetterBase)
                return binding.ProvideValue(serviceProvider);

            var property = target.TargetProperty as PropertyInfo;
            if (property != null && property.PropertyType != typeof(object)
                && property.PropertyType.IsAssignableFrom(binding.GetType()))
            {
                // E.g. a BindingBase property of another markup extension.
                return binding;
            }

            Trace.TraceWarning(
                $"Localization: '{target.TargetObject?.GetType().Name}.{property?.Name}' is not a dependency property; " +
                "the localized text will not refresh when the language changes.");
            return snapshot();
        }
    }
}
