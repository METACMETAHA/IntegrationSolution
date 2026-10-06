using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace IntegrationSolution.Localization.Markup
{
    /// <summary>
    /// Localized composite format (a replacement for <c>Binding.StringFormat</c> with hard-coded text):
    /// the format string comes from Strings.resx and the placeholders from bindings.
    /// Both the text (on language switch) and the values (on property change) refresh automatically.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;TextBlock Text="{loc:LocFormat Common_KilometersFormat, Arg0={Binding TotalMileage}, FallbackValue='-', TargetNullValue='-'}" /&gt;
    /// </code>
    /// With Strings.resx: <c>Common_KilometersFormat = "{0} km"</c>.
    /// </example>
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class LocFormatExtension : MarkupExtension
    {
        public LocFormatExtension()
        { }

        public LocFormatExtension(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Resource key of the composite format string (e.g. "Trips: {0}").
        /// </summary>
        [ConstructorArgument("key")]
        public string Key { get; set; }

        /// <summary>Value for the <c>{0}</c> placeholder.</summary>
        public BindingBase Arg0 { get; set; }

        /// <summary>Value for the <c>{1}</c> placeholder.</summary>
        public BindingBase Arg1 { get; set; }

        /// <summary>Value for the <c>{2}</c> placeholder.</summary>
        public BindingBase Arg2 { get; set; }

        /// <summary>Value for the <c>{3}</c> placeholder.</summary>
        public BindingBase Arg3 { get; set; }

        /// <summary>
        /// Shown when an argument binding cannot be resolved (same meaning as <see cref="BindingBase.FallbackValue"/>).
        /// </summary>
        public object FallbackValue { get; set; }

        /// <summary>
        /// Shown when an argument value is <c>null</c> (same meaning as <see cref="BindingBase.TargetNullValue"/>).
        /// When not set, <c>null</c> arguments are formatted as empty text, like <c>StringFormat</c> does.
        /// </summary>
        public object TargetNullValue { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new InvalidOperationException("LocFormat: the resource Key must be specified.");

            var multiBinding = new MultiBinding
            {
                Mode = BindingMode.OneWay,
                Converter = LocalizedFormatConverter.Instance,
                ConverterParameter = TargetNullValue != null,
            };

            // Index 0 is always the localized format string.
            multiBinding.Bindings.Add(LocalizationBindingHelper.CreateKeyBinding(Key));

            foreach (var arg in new[] { Arg0, Arg1, Arg2, Arg3 })
            {
                if (arg != null)
                    multiBinding.Bindings.Add(arg);
            }

            if (FallbackValue != null)
                multiBinding.FallbackValue = FallbackValue;
            if (TargetNullValue != null)
                multiBinding.TargetNullValue = TargetNullValue;

            var key = Key;
            return LocalizationBindingHelper.ProvideValue(
                multiBinding,
                serviceProvider,
                () => LocalizationService.Instance[key]);
        }
    }
}
