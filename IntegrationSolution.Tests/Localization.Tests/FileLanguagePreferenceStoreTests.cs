using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Resources;
using IntegrationSolution.Localization.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace IntegrationSolution.Tests.Localization.Tests
{
    [TestClass]
    [TestCategory("Localization")]
    public class FileLanguagePreferenceStoreTests
    {
        private string _directory;
        private string _filePath;
        private CultureScope _cultureScope;

        [TestInitialize]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), "IntegrationSolution.Tests", Guid.NewGuid().ToString("N"));
            _filePath = Path.Combine(_directory, "nested", "ui-language.txt");
            _cultureScope = new CultureScope();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cultureScope.Dispose();
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }

        [TestMethod]
        public void DefaultFilePath_IsPerUserLocalApplicationData()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            StringAssert.StartsWith(FileLanguagePreferenceStore.DefaultFilePath, localAppData);
            StringAssert.EndsWith(FileLanguagePreferenceStore.DefaultFilePath, Path.Combine("IntegrationSolution", "ui-language.txt"));
        }

        [TestMethod]
        public void Load_WhenNothingWasSaved_ReturnsNull()
        {
            Assert.IsNull(new FileLanguagePreferenceStore(_filePath).Load());
        }

        [TestMethod]
        public void Save_CreatesTheFolder_AndOverwritesThePreviousValue()
        {
            var store = new FileLanguagePreferenceStore(_filePath);

            store.Save("uk");
            store.Save("ru");

            Assert.AreEqual("ru", new FileLanguagePreferenceStore(_filePath).Load());
            Assert.AreEqual("ru", File.ReadAllText(_filePath));
            Assert.IsFalse(File.Exists(_filePath + ".tmp"), "The temporary file must be moved into place.");
        }

        [TestMethod]
        public void Load_IgnoresSurroundingWhitespaceAndByteOrderMark()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllText(_filePath, "  uk\r\n", new UTF8Encoding(true));

            Assert.AreEqual("uk", new FileLanguagePreferenceStore(_filePath).Load());
        }

        [TestMethod]
        public void Service_WithCorruptFile_FallsBackToEnglish_AndRepairsItOnTheNextSwitch()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllBytes(_filePath, new byte[16]); // e.g. zero-filled after a power loss
            var service = new LocalizationService(Strings.ResourceManager, new FileLanguagePreferenceStore(_filePath));

            service.Initialize();
            Assert.AreEqual("en", service.CurrentLanguage.CultureName);

            service.ChangeLanguage("uk");
            Assert.AreEqual("uk", new FileLanguagePreferenceStore(_filePath).Load());
        }

        [TestMethod]
        public void Constructor_RequiresAPath()
        {
            Assert.ThrowsException<ArgumentException>(() => new FileLanguagePreferenceStore(" "));
            Assert.ThrowsException<ArgumentNullException>(() => new FileLanguagePreferenceStore(_filePath).Save(null));
        }
    }
}
