using IntegrationSolution.Localization.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntegrationSolution.Tests.Localization.Tests
{
    /// <summary>
    /// Static checks over the XAML sources of the solution.
    /// </summary>
    [TestClass]
    [TestCategory("Localization")]
    public class XamlLocalizationTests
    {
        private const string LocNamespace = "clr-namespace:IntegrationSolution.Localization.Markup;assembly=IntegrationSolution.Localization";

        private static readonly Regex LocUsage = new Regex(@"\{loc:(?:Loc|LocFormat)\s+(?:Key\s*=\s*)?([A-Za-z0-9_]+)", RegexOptions.Compiled);
        private static readonly Regex XmlComment = new Regex(@"<!--.*?-->", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex Cyrillic = new Regex(@"[Ѐ-ӿ]", RegexOptions.Compiled);

        internal static string FindSolutionRoot()
        {
            var fromEnvironment = Environment.GetEnvironmentVariable("INTEGRATIONSOLUTION_ROOT");
            if (!string.IsNullOrEmpty(fromEnvironment) && File.Exists(Path.Combine(fromEnvironment, "IntegrationSolution.sln")))
                return fromEnvironment;

            for (var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
            {
                if (File.Exists(Path.Combine(dir.FullName, "IntegrationSolution.sln")))
                    return dir.FullName;
            }

            return null;
        }

        private static List<string> XamlFiles()
        {
            var root = FindSolutionRoot();
            if (root == null)
                Assert.Inconclusive("Solution sources were not found next to the test binaries.");

            var separator = Path.DirectorySeparatorChar;
            return Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories)
                .Where(x => !x.Contains(separator + "bin" + separator)
                         && !x.Contains(separator + "obj" + separator)
                         && !x.Contains(separator + "packages" + separator))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
        }

        [TestMethod]
        public void EveryLocalizationKeyUsedInXaml_Exists()
        {
            var missing = new List<string>();
            var used = 0;

            foreach (var file in XamlFiles())
            {
                foreach (Match match in LocUsage.Matches(File.ReadAllText(file)))
                {
                    used++;
                    var key = match.Groups[1].Value;
                    if (Strings.ResourceManager.GetString(key, CultureInfo.InvariantCulture) == null)
                        missing.Add($"{Path.GetFileName(file)}: {key}");
                }
            }

            Assert.IsTrue(used > 0, "No {loc:...} usages found - is the scan looking at the right folder?");
            Assert.AreEqual(0, missing.Count, "Unknown resource keys: " + string.Join("; ", missing));
        }

        [TestMethod]
        public void XamlUsingLoc_DeclaresTheLocalizationNamespace()
        {
            var wrong = XamlFiles()
                .Where(file =>
                {
                    var text = File.ReadAllText(file);
                    return text.Contains("{loc:") && !text.Contains("xmlns:loc=\"" + LocNamespace + "\"");
                })
                .Select(Path.GetFileName)
                .ToList();

            Assert.AreEqual(0, wrong.Count, "Missing/incorrect xmlns:loc in: " + string.Join(", ", wrong));
        }

        /// <summary>
        /// UI text must come from Strings.resx; hard-coded Russian in XAML would not switch language.
        /// </summary>
        [TestMethod]
        public void Xaml_ContainsNoHardCodedCyrillicText()
        {
            var offenders = new List<string>();

            foreach (var file in XamlFiles())
            {
                var withoutComments = XmlComment.Replace(File.ReadAllText(file), m => new string('\n', m.Value.Count(c => c == '\n')));
                var lines = withoutComments.Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    if (Cyrillic.IsMatch(lines[i]))
                        offenders.Add($"{Path.GetFileName(file)}:{i + 1}: {lines[i].Trim()}");
                }
            }

            Assert.AreEqual(0, offenders.Count, "Hard-coded text found:\n" + string.Join("\n", offenders));
        }
    }
}
