using IntegrationSolution.Tests.Localization.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntegrationSolution.Tests.Configuration.Tests
{
    /// <summary>
    /// Keeps Wialon access tokens out of the repository: the user enters the token in Settings > Wialon,
    /// and the integration tests read it from the WIALON_TOKEN environment variable.
    /// </summary>
    [TestClass]
    [TestCategory("Configuration")]
    public class NoCommittedTokensTests
    {
        // Wialon access tokens are 72 hexadecimal characters.
        private static readonly Regex TokenLike = new Regex(@"[0-9A-Fa-f]{64,}", RegexOptions.Compiled);
        private static readonly Regex TokenSetting = new Regex(@"<add\s+key=""Token""\s+value=""([^""]*)""\s*/>", RegexOptions.Compiled);

        private static string SolutionRoot()
        {
            var root = XamlLocalizationTests.FindSolutionRoot();
            if (root == null)
                Assert.Inconclusive("Solution sources were not found next to the test binaries.");
            return root;
        }

        private static List<string> SourceFiles(string root, params string[] patterns)
        {
            var separator = Path.DirectorySeparatorChar;
            return patterns
                .SelectMany(pattern => Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
                .Where(x => !x.Contains(separator + "bin" + separator)
                         && !x.Contains(separator + "obj" + separator)
                         && !x.Contains(separator + "packages" + separator))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
        }

        [TestMethod]
        public void ShellConfig_ShipsAnEmptyTokenKey()
        {
            var config = File.ReadAllText(Path.Combine(SolutionRoot(), "IntegrationSolution.ShellGUI", "App.config"));
            var tokens = TokenSetting.Matches(config);

            // AppConfiguration only overwrites existing keys, so Settings > Wialon needs the key to save a token.
            Assert.AreEqual(1, tokens.Count, "App.config must have exactly one 'Token' key in appSettings.");
            Assert.AreEqual(string.Empty, tokens[0].Groups[1].Value, "App.config must not ship a Wialon token.");
        }

        [TestMethod]
        public void NoSourceOrConfigFile_ContainsAToken()
        {
            var root = SolutionRoot();
            var files = SourceFiles(root, "*.cs", "*.config");
            var offenders = files
                .Where(x => TokenLike.IsMatch(File.ReadAllText(x)))
                .Select(x => x.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar))
                .ToList();

            Assert.IsTrue(files.Count > 0, "No sources found - is the scan looking at the right folder?");
            Assert.AreEqual(0, offenders.Count,
                "Token-like values found in: " + string.Join(", ", offenders) + ". Revoke the token in Wialon and remove it from the file.");
        }
    }
}
