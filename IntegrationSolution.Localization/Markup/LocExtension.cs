using System;
using System.Windows.Markup;

namespace IntegrationSolution.Localization.Markup
{
    /// <summary>
    /// Localized text that follows runtime language switching.
    /// </summary>
    /// <example>
    /// <code>
    /// xmlns:loc="clr-namespace:IntegrationSolution.Localization.Markup;assembly=IntegrationSolution.Localization"
    /// &lt;Button Content="{loc:Loc Shell_SettingsButton}" /&gt;
    /// </code>
    /// </example>
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class LocExtension : MarkupExtension
    {
        public LocExtension()
        { }

        public LocExtension(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Resource key in Strings.resx.
        /// </summary>
        [ConstructorArgument("key")]
        public string Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new InvalidOperationException("Loc: the resource Key must be specified.");

            var key = Key;
            return LocalizationBindingHelper.ProvideValue(
                LocalizationBindingHelper.CreateKeyBinding(key),
                serviceProvider,
                () => LocalizationService.Instance[key]);
        }
    }
}
