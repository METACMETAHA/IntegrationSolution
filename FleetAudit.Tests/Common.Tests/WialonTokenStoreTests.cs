using FleetAudit.Common.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace FleetAudit.Tests.Common.Tests
{
    /// <summary>
    /// The Wialon token must not be written to the <c>.config</c> next to the exe: the MSI installs per machine, so a standard
    /// user cannot write there. It lives encrypted (DPAPI, current user) in <c>%LocalAppData%\FleetAudit</c>.
    /// None of these tests touches the real user profile; they only compute the default path or use a temporary folder.
    /// </summary>
    [TestClass]
    [TestCategory("Common")]
    public class WialonTokenStoreTests
    {
        // Dashes keep it from looking like a real 64+ hex digit token to NoCommittedTokensTests.
        private const string Token = "test-token-0123456789abcdef-0123456789abcdef-0123456789abcdef";

        private string _directory;
        private string _filePath;

        [TestInitialize]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), "FleetAudit.Tests", Guid.NewGuid().ToString("N"));
            _filePath = Path.Combine(_directory, "nested", "wialon-token.dat");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }

        private static bool Contains(byte[] data, byte[] pattern)
        {
            for (var i = 0; i + pattern.Length <= data.Length; i++)
            {
                var j = 0;
                while (j < pattern.Length && data[i + j] == pattern[j])
                    j++;

                if (j == pattern.Length)
                    return true;
            }
            return false;
        }

        [TestMethod]
        public void DefaultFilePath_IsPerUserLocalApplicationData()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            Assert.AreEqual(Path.Combine(localAppData, "FleetAudit", "wialon-token.dat"), WialonTokenStore.DefaultFilePath);
            Assert.AreEqual(WialonTokenStore.DefaultFilePath, new WialonTokenStore().FilePath);
        }

        [TestMethod]
        public void Load_WhenNothingWasSaved_ReturnsNull()
        {
            Assert.IsNull(new WialonTokenStore(_filePath).Load());
        }

        [TestMethod]
        public void Save_CreatesTheMissingFolder_AndLoadReadsTheTokenBack()
        {
            new WialonTokenStore(_filePath).Save(Token);

            Assert.IsTrue(File.Exists(_filePath), "The folder and the file must be created on first save.");
            Assert.IsFalse(File.Exists(_filePath + ".tmp"), "The temporary file must be moved into place.");
            Assert.AreEqual(Token, new WialonTokenStore(_filePath).Load());
        }

        [TestMethod]
        public void Save_OverwritesThePreviousToken()
        {
            var store = new WialonTokenStore(_filePath);

            store.Save("first-token");
            store.Save(Token);

            Assert.AreEqual(Token, new WialonTokenStore(_filePath).Load());
            Assert.IsFalse(File.Exists(_filePath + ".tmp"), "The temporary file must be moved into place.");
        }

        [TestMethod]
        public void SavedFile_DoesNotContainThePlainTextToken()
        {
            new WialonTokenStore(_filePath).Save(Token);

            var bytes = File.ReadAllBytes(_filePath);

            Assert.IsTrue(bytes.Length > 0);
            Assert.IsFalse(Contains(bytes, Encoding.UTF8.GetBytes(Token)), "The token must not be stored as UTF-8 text.");
            Assert.IsFalse(Contains(bytes, Encoding.Unicode.GetBytes(Token)), "The token must not be stored as UTF-16 text.");
            Assert.IsFalse(Contains(bytes, Encoding.UTF8.GetBytes(Token.Substring(0, 16))), "No part of the token may be readable.");
        }

        [TestMethod]
        public void Load_WhenTheFileIsPlainText_ReturnsNull()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllText(_filePath, Token, new UTF8Encoding(false));

            Assert.IsNull(new WialonTokenStore(_filePath).Load());
        }

        [TestMethod]
        public void Load_WhenTheFileIsCorrupted_ReturnsNull()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            var store = new WialonTokenStore(_filePath);

            File.WriteAllBytes(_filePath, new byte[16]); // e.g. zero-filled after a power loss
            Assert.IsNull(store.Load());

            File.WriteAllBytes(_filePath, new byte[0]);
            Assert.IsNull(store.Load());

            File.WriteAllBytes(_filePath, Encoding.UTF8.GetBytes("not a protected blob"));
            Assert.IsNull(store.Load());
        }

        [TestMethod]
        public void Load_WhenTheFileIsTruncated_ReturnsNull()
        {
            var store = new WialonTokenStore(_filePath);
            store.Save(Token);

            var bytes = File.ReadAllBytes(_filePath);
            var truncated = new byte[bytes.Length / 2];
            Array.Copy(bytes, truncated, truncated.Length);
            File.WriteAllBytes(_filePath, truncated);

            Assert.IsNull(store.Load());
        }

        [TestMethod]
        public void Save_RepairsACorruptedFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllBytes(_filePath, new byte[16]);
            var store = new WialonTokenStore(_filePath);

            store.Save(Token);

            Assert.AreEqual(Token, store.Load());
        }

        [TestMethod]
        public void Clear_RemovesTheSavedToken_AndIsSafeWithoutAFileOrFolder()
        {
            var store = new WialonTokenStore(_filePath);
            store.Clear();

            store.Save(Token);
            store.Clear();

            Assert.IsFalse(File.Exists(_filePath));
            Assert.IsNull(store.Load());
        }

        [TestMethod]
        public void Save_WithABlankToken_RemovesTheSavedToken()
        {
            var store = new WialonTokenStore(_filePath);
            store.Save(Token);

            store.Save("  ");

            Assert.IsNull(store.Load());
        }

        [TestMethod]
        public void LoadOrLegacy_PrefersTheSavedToken()
        {
            var store = new WialonTokenStore(_filePath);
            store.Save(Token);

            Assert.AreEqual(Token, store.LoadOrLegacy("legacy-token"));
        }

        [TestMethod]
        public void LoadOrLegacy_WhenNothingWasSaved_FallsBackToTheLegacySetting()
        {
            var store = new WialonTokenStore(_filePath);

            Assert.AreEqual("legacy-token", store.LoadOrLegacy("legacy-token"));
            Assert.AreEqual(string.Empty, store.LoadOrLegacy(string.Empty));
            Assert.IsNull(store.LoadOrLegacy(null));
        }

        [TestMethod]
        public void LoadOrLegacy_WhenTheSavedFileIsCorrupted_FallsBackToTheLegacySetting()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllBytes(_filePath, new byte[16]);

            Assert.AreEqual("legacy-token", new WialonTokenStore(_filePath).LoadOrLegacy("legacy-token"));
        }

        [TestMethod]
        public void LoadOrLegacy_WithoutArguments_ReadsTheLegacyAppSetting()
        {
            // Nothing is saved for the temporary path, so the result is whatever the host's config says (usually nothing).
            Assert.AreEqual(
                System.Configuration.ConfigurationManager.AppSettings[WialonTokenStore.LegacySettingName],
                new WialonTokenStore(_filePath).LoadOrLegacy());
        }

        [TestMethod]
        public void Constructor_RequiresAPath_AndSaveRequiresAToken()
        {
            Assert.ThrowsException<ArgumentException>(() => new WialonTokenStore(" "));
            Assert.ThrowsException<ArgumentNullException>(() => new WialonTokenStore(_filePath).Save(null));
        }
    }
}
