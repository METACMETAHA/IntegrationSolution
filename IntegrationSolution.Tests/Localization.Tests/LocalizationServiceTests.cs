using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace IntegrationSolution.Tests.Localization.Tests
{
    [TestClass]
    [TestCategory("Localization")]
    public class LocalizationServiceTests
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

        private static LocalizationService CreateService(InMemoryLanguagePreferenceStore store)
        {
            return new LocalizationService(Strings.ResourceManager, store);
        }

        [TestMethod]
        public void SupportedLanguages_AreEnglishUkrainianRussian_WithNativeNames()
        {
            var service = CreateService(new InMemoryLanguagePreferenceStore());

            CollectionAssert.AreEqual(
                new[] { "en", "uk", "ru" },
                service.SupportedLanguages.Select(x => x.CultureName).ToArray());
            CollectionAssert.AreEqual(
                new[] { "English", "Українська", "Русский" },
                service.SupportedLanguages.Select(x => x.NativeName).ToArray());
            Assert.AreEqual("en", service.DefaultLanguage.CultureName);
        }

        [TestMethod]
        public void Initialize_WithoutSavedPreference_UsesEnglishByDefault()
        {
            var service = CreateService(new InMemoryLanguagePreferenceStore());

            service.Initialize();

            Assert.AreEqual("en", service.CurrentLanguage.CultureName);
            Assert.AreEqual("en", Thread.CurrentThread.CurrentUICulture.Name);
            Assert.AreEqual("Settings", service["Settings_Header"]);
        }

        [DataTestMethod]
        [DataRow("uk", "uk", "Налаштування")]
        [DataRow("ru", "ru", "Настройки")]
        [DataRow("en", "en", "Settings")]
        [DataRow("uk-UA", "uk", "Налаштування")]
        [DataRow("ru-RU", "ru", "Настройки")]
        [DataRow("en-US", "en", "Settings")]
        [DataRow(" RU ", "ru", "Настройки")]
        public void Initialize_WithSavedPreference_AppliesIt(string saved, string expectedCulture, string expectedHeader)
        {
            var service = CreateService(new InMemoryLanguagePreferenceStore(saved));

            service.Initialize();

            Assert.AreEqual(expectedCulture, service.CurrentLanguage.CultureName);
            Assert.AreEqual(expectedCulture, Thread.CurrentThread.CurrentUICulture.Name);
            Assert.AreEqual(expectedCulture, CultureInfo.DefaultThreadCurrentUICulture.Name);
            Assert.AreEqual(expectedCulture, Strings.Culture.Name);
            Assert.AreEqual(expectedHeader, service["Settings_Header"]);
            Assert.AreEqual(expectedHeader, Strings.Settings_Header);
        }

        [DataTestMethod]
        [DataRow("de")]
        [DataRow("xx-not-a-culture")]
        [DataRow("")]
        [DataRow("   ")]
        public void Initialize_WithUnsupportedPreference_FallsBackToEnglish(string saved)
        {
            var store = new InMemoryLanguagePreferenceStore(saved);
            var service = CreateService(store);

            service.Initialize();

            Assert.AreEqual("en", service.CurrentLanguage.CultureName);
            Assert.AreEqual(0, store.SaveCalls.Count, "Initialize must not overwrite the stored preference.");
        }

        [TestMethod]
        public void Initialize_WhenStoreFails_FallsBackToEnglish()
        {
            var store = new InMemoryLanguagePreferenceStore { LoadException = new InvalidOperationException("corrupt user.config") };
            var service = CreateService(store);

            service.Initialize();

            Assert.AreEqual("en", service.CurrentLanguage.CultureName);
        }

        [TestMethod]
        public void ChangeLanguage_SwitchesPersistsAndNotifies()
        {
            var store = new InMemoryLanguagePreferenceStore();
            var service = CreateService(store);
            service.Initialize();

            var changedProperties = new List<string>();
            LanguageChangedEventArgs languageChanged = null;
            service.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);
            service.LanguageChanged += (s, e) => languageChanged = e;

            var result = service.ChangeLanguage("ru");

            Assert.IsTrue(result);
            Assert.AreEqual("ru", service.CurrentLanguage.CultureName);
            Assert.AreEqual("ru", store.Saved);
            Assert.AreEqual("Настройки", service["Settings_Header"]);
            CollectionAssert.Contains(changedProperties, LocalizationService.IndexerPropertyName,
                "Indexer bindings ({loc:Loc}) refresh only when 'Item[]' is raised.");
            CollectionAssert.Contains(changedProperties, nameof(ILocalizationService.CurrentLanguage));
            Assert.IsNotNull(languageChanged);
            Assert.AreEqual("en", languageChanged.OldLanguage.CultureName);
            Assert.AreEqual("ru", languageChanged.NewLanguage.CultureName);
        }

        [TestMethod]
        public void ChangeLanguage_ToActiveLanguage_ReturnsFalseWithoutSideEffects()
        {
            var store = new InMemoryLanguagePreferenceStore("uk");
            var service = CreateService(store);
            service.Initialize();
            var raised = 0;
            service.LanguageChanged += (s, e) => raised++;

            var result = service.ChangeLanguage("uk-UA");

            Assert.IsFalse(result);
            Assert.AreEqual(0, raised);
            Assert.AreEqual(0, store.SaveCalls.Count);
        }

        [TestMethod]
        public void ChangeLanguage_Unsupported_ThrowsAndKeepsCurrentLanguage()
        {
            var service = CreateService(new InMemoryLanguagePreferenceStore("uk"));
            service.Initialize();

            Assert.ThrowsException<ArgumentException>(() => service.ChangeLanguage("de"));
            Assert.ThrowsException<ArgumentException>(() => service.ChangeLanguage(null));
            Assert.AreEqual("uk", service.CurrentLanguage.CultureName);
        }

        [TestMethod]
        public void ChangeLanguage_WhenSaveFails_StillSwitchesForThisSession()
        {
            var store = new InMemoryLanguagePreferenceStore { SaveException = new UnauthorizedAccessException() };
            var service = CreateService(store);
            service.Initialize();

            var result = service.ChangeLanguage("uk");

            Assert.IsTrue(result);
            Assert.AreEqual("uk", service.CurrentLanguage.CultureName);
            Assert.AreEqual("Налаштування", service["Settings_Header"]);
        }

        [TestMethod]
        public void ChangeLanguage_DoesNotTouchFormattingCulture()
        {
            var formatting = CultureInfo.GetCultureInfo("de-DE");
            Thread.CurrentThread.CurrentCulture = formatting;
            var service = CreateService(new InMemoryLanguagePreferenceStore());
            service.Initialize();

            service.ChangeLanguage("uk");
            service.ChangeLanguage("en");

            Assert.AreEqual(formatting, Thread.CurrentThread.CurrentCulture,
                "Only the UI language may change; number/date parsing of Excel data depends on CurrentCulture.");
        }

        [TestMethod]
        public void GetString_MissingKey_ReturnsVisibleMarker()
        {
            var service = CreateService(new InMemoryLanguagePreferenceStore());

            Assert.AreEqual("[No_Such_Key]", service["No_Such_Key"]);
            Assert.AreEqual(string.Empty, service.GetString(null));
        }

        [TestMethod]
        public void GetString_MissingTranslation_FallsBackToNeutralEnglish()
        {
            var resources = new DictionaryResourceManager(new Dictionary<string, Dictionary<string, string>>
            {
                [""] = new Dictionary<string, string> { ["Area_Only"] = "English only" },
                ["uk"] = new Dictionary<string, string>(),
            });
            var service = new LocalizationService(resources, new InMemoryLanguagePreferenceStore("uk"));
            service.Initialize();

            Assert.AreEqual("English only", service["Area_Only"]);
        }

        [TestMethod]
        public void Format_UsesLocalizedFormatAndRegionalCulture()
        {
            var resources = new DictionaryResourceManager(new Dictionary<string, Dictionary<string, string>>
            {
                [""] = new Dictionary<string, string> { ["Area_Trips"] = "Trips: {0:N1}", ["Area_Broken"] = "Broken {0" },
                ["uk"] = new Dictionary<string, string> { ["Area_Trips"] = "Поїздок: {0:N1}" },
            });
            var service = new LocalizationService(resources, new InMemoryLanguagePreferenceStore());
            service.Initialize();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Assert.AreEqual("Trips: 1,234.5", service.Format("Area_Trips", 1234.5));
            service.ChangeLanguage("uk");
            Assert.AreEqual("Поїздок: 1,234.5", service.Format("Area_Trips", 1234.5));
            Assert.AreEqual("Broken {0", service.Format("Area_Broken", 1), "An invalid translation must not crash the UI.");
        }

        [TestMethod]
        public void Constructor_RejectsUnsupportedDefaultLanguage()
        {
            Assert.ThrowsException<ArgumentException>(() => new LocalizationService(
                Strings.ResourceManager,
                new InMemoryLanguagePreferenceStore(),
                LocalizationService.CreateDefaultLanguages(),
                "de"));
        }

        [TestMethod]
        public void Instance_IsSingleton()
        {
            Assert.AreSame(LocalizationService.Instance, LocalizationService.Instance);
        }
    }
}
