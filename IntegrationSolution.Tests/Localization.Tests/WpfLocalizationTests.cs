using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Markup;
using IntegrationSolution.Localization.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IntegrationSolution.Tests.Localization.Tests
{
    /// <summary>
    /// Tests of the WPF-facing pieces (weak events, format converter). Windows only.
    /// </summary>
    [TestClass]
    [TestCategory("Localization")]
    public class WpfLocalizationTests
    {
        private CultureScope _cultureScope;

        [TestInitialize]
        public void Setup()
        {
            _cultureScope = new CultureScope();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cultureScope.Dispose();
        }

        private sealed class Listener
        {
            public int Calls { get; private set; }

            public void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
            {
                Calls++;
            }
        }

        [TestMethod]
        public void LanguageChangedEventManager_DeliversAndUnsubscribes()
        {
            var service = new LocalizationService(Strings.ResourceManager, new InMemoryLanguagePreferenceStore());
            service.Initialize();
            var listener = new Listener();

            LanguageChangedEventManager.AddHandler(service, listener.OnLanguageChanged);
            service.ChangeLanguage("uk");
            LanguageChangedEventManager.RemoveHandler(service, listener.OnLanguageChanged);
            service.ChangeLanguage("ru");

            Assert.AreEqual(1, listener.Calls);
        }

        [TestMethod]
        public void LanguageChangedEventManager_DoesNotKeepListenerAlive()
        {
            var service = new LocalizationService(Strings.ResourceManager, new InMemoryLanguagePreferenceStore());
            var reference = Subscribe(service);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(reference.IsAlive, "The service must not root its listeners.");
        }

        private static WeakReference Subscribe(ILocalizationService service)
        {
            var listener = new Listener();
            LanguageChangedEventManager.AddHandler(service, listener.OnLanguageChanged);
            return new WeakReference(listener);
        }

        [TestMethod]
        public void LocalizedFormatConverter_FormatsWithBindingCulture()
        {
            var converter = LocalizedFormatConverter.Instance;
            var culture = CultureInfo.InvariantCulture;

            Assert.AreEqual("12.5 km", converter.Convert(new object[] { "{0} km", 12.5 }, typeof(string), false, culture));
            Assert.AreEqual("a / b", converter.Convert(new object[] { "{0} / {1}", "a", "b" }, typeof(string), false, culture));
        }

        [TestMethod]
        public void LocalizedFormatConverter_HandlesUnsetAndNullLikeStringFormat()
        {
            var converter = LocalizedFormatConverter.Instance;
            var culture = CultureInfo.InvariantCulture;

            Assert.AreSame(DependencyProperty.UnsetValue,
                converter.Convert(new object[] { "{0} km", DependencyProperty.UnsetValue }, typeof(string), false, culture),
                "Unresolved source -> FallbackValue.");
            Assert.IsNull(converter.Convert(new object[] { "{0} km", null }, typeof(string), true, culture),
                "Null source with TargetNullValue -> TargetNullValue.");
            Assert.AreEqual(" km", converter.Convert(new object[] { "{0} km", null }, typeof(string), false, culture),
                "Null source without TargetNullValue is formatted as empty text, like StringFormat.");
            Assert.AreSame(DependencyProperty.UnsetValue,
                converter.Convert(new object[] { null, 1 }, typeof(string), false, culture));
            Assert.AreEqual("{0 broken", converter.Convert(new object[] { "{0 broken", 1 }, typeof(string), false, culture));
        }

        [TestMethod]
        public void LocExtension_WithoutTarget_ReturnsLiveBinding()
        {
            var value = new LocExtension("Settings_Header").ProvideValue(null);

            var binding = value as Binding;
            Assert.IsNotNull(binding);
            Assert.AreSame(LocalizationService.Instance, binding.Source);
            Assert.AreEqual("[Settings_Header]", binding.Path.Path);
            Assert.AreEqual(BindingMode.OneWay, binding.Mode);
        }

        [TestMethod]
        public void LocFormatExtension_WithoutTarget_ReturnsMultiBinding()
        {
            var value = new LocFormatExtension("Settings_Header") { Arg0 = new Binding("X"), TargetNullValue = "-" }.ProvideValue(null);

            var multiBinding = value as MultiBinding;
            Assert.IsNotNull(multiBinding);
            Assert.AreEqual(2, multiBinding.Bindings.Count, "Format binding + one argument.");
            Assert.AreEqual("-", multiBinding.TargetNullValue);
        }

        [TestMethod]
        public void LocExtension_RequiresKey()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new LocExtension().ProvideValue(null));
            Assert.ThrowsException<InvalidOperationException>(() => new LocFormatExtension().ProvideValue(null));
        }
    }
}
