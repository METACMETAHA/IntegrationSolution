using FleetAudit.Common.Entities;
using FleetAudit.Common.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace FleetAudit.Tests.Common.Tests
{
    /// <summary>
    /// <c>sys.dat</c> must not live next to the exe: the MSI installs per machine, so a standard user cannot write there.
    /// None of these tests touches the real user profile; they only compute the default path or use a temporary folder.
    /// </summary>
    [TestClass]
    [TestCategory("Common")]
    public class SerializableTests
    {
        [Serializable]
        public class Sample
        {
            public string Value { get; set; }
        }

        private string _directory;

        [TestInitialize]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), "FleetAudit.Tests", Guid.NewGuid().ToString("N"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }

        [TestMethod]
        public void ResolveFilePath_PutsSysDatIntoPerUserLocalApplicationData()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var path = new Serializable<Sample>(SerializeConfigDTO.DefaultFileName).ResolveFilePath();

            Assert.AreEqual(Path.Combine(localAppData, "FleetAudit", "sys.dat"), path);
        }

        [TestMethod]
        public void ResolveFilePath_KeepsAnAbsolutePath()
        {
            var absolute = Path.Combine(_directory, "custom.dat");

            Assert.AreEqual(absolute, new Serializable<Sample>(absolute).ResolveFilePath());
        }

        [TestMethod]
        public void Serialize_CreatesTheMissingFolder_AndDeserializeReadsTheDataBack()
        {
            var path = Path.Combine(_directory, "nested", "sys.dat");

            new Serializable<Sample>(path).Serialize(new Sample { Value = "kept" });

            Assert.IsTrue(File.Exists(path), "The folder and the file must be created on first save.");
            Assert.AreEqual("kept", new Serializable<Sample>(path).Deserialize().Value);
        }

        [TestMethod]
        public void Deserialize_WhenNothingWasSaved_ReturnsNull()
        {
            var path = Path.Combine(_directory, "missing", "sys.dat");

            Assert.IsNull(new Serializable<Sample>(path).Deserialize());
        }
    }
}
