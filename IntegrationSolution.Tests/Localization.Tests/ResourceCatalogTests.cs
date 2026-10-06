using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace IntegrationSolution.Tests.Localization.Tests
{
    /// <summary>
    /// Guards the translation catalog: every language has every string, and translations keep the placeholders.
    /// </summary>
    [TestClass]
    [TestCategory("Localization")]
    public class ResourceCatalogTests
    {
        private static readonly Regex Placeholder = new Regex(@"(?<!\{)\{(\d+)(?:,\s*-?\d+)?(?::[^{}]*)?\}", RegexOptions.Compiled);

        private static Dictionary<string, string> ReadTable(string cultureName)
        {
            var culture = cultureName.Length == 0 ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName);
            var set = Strings.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
            Assert.IsNotNull(set, $"Resource set for '{cultureName}' was not found (satellite assembly missing?).");

            return set.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => x.Value as string);
        }

        private static IEnumerable<string> TranslatedCultures =>
            LocalizationService.CreateDefaultLanguages()
                .Select(x => x.CultureName)
                .Where(x => x != LocalizationService.DefaultCultureName);

        [TestMethod]
        public void NeutralResourcesLanguage_IsEnglish()
        {
            var attribute = typeof(Strings).Assembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>();

            Assert.IsNotNull(attribute);
            Assert.AreEqual(LocalizationService.DefaultCultureName, attribute.CultureName);
        }

        [TestMethod]
        public void EveryLanguage_TranslatesEveryKey()
        {
            var neutral = ReadTable(string.Empty);
            Assert.IsTrue(neutral.Count > 0);

            foreach (var culture in TranslatedCultures)
            {
                var table = ReadTable(culture);

                var missing = neutral.Keys.Except(table.Keys).OrderBy(x => x).ToList();
                var extra = table.Keys.Except(neutral.Keys).OrderBy(x => x).ToList();

                Assert.AreEqual(0, missing.Count, $"'{culture}' is missing: {string.Join(", ", missing)}");
                Assert.AreEqual(0, extra.Count, $"'{culture}' has keys unknown to the neutral catalog: {string.Join(", ", extra)}");
            }
        }

        [TestMethod]
        public void NoValue_IsEmpty()
        {
            foreach (var culture in new[] { string.Empty }.Concat(TranslatedCultures))
            {
                var empty = ReadTable(culture).Where(x => string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Key).ToList();
                Assert.AreEqual(0, empty.Count, $"'{culture}' has empty values: {string.Join(", ", empty)}");
            }
        }

        [TestMethod]
        public void Translations_KeepThePlaceholders()
        {
            var neutral = ReadTable(string.Empty);

            foreach (var culture in TranslatedCultures)
            {
                var table = ReadTable(culture);
                foreach (var entry in neutral)
                {
                    string translated;
                    if (!table.TryGetValue(entry.Key, out translated))
                        continue;

                    CollectionAssert.AreEquivalent(
                        PlaceholderIndexes(entry.Value),
                        PlaceholderIndexes(translated),
                        $"Placeholders differ for '{entry.Key}' in '{culture}'.");
                }
            }
        }

        [TestMethod]
        public void FormatStrings_AreValidInEveryLanguage()
        {
            foreach (var culture in new[] { string.Empty }.Concat(TranslatedCultures))
            {
                foreach (var entry in ReadTable(culture))
                {
                    var indexes = PlaceholderIndexes(entry.Value);
                    if (indexes.Count == 0)
                        continue;

                    var args = Enumerable.Range(0, indexes.Max() + 1).Select(i => (object)(i + 0.5)).ToArray();
                    try
                    {
                        string.Format(CultureInfo.InvariantCulture, entry.Value, args);
                    }
                    catch (FormatException ex)
                    {
                        Assert.Fail($"'{entry.Key}' ({culture}) is not a valid format string: {ex.Message}");
                    }
                }
            }
        }

        [TestMethod]
        public void StronglyTypedClass_MatchesTheCatalog()
        {
            var neutralKeys = ReadTable(string.Empty).Keys.OrderBy(x => x, StringComparer.Ordinal).ToList();
            var properties = typeof(Strings)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.PropertyType == typeof(string))
                .Select(x => x.Name)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            CollectionAssert.AreEqual(neutralKeys, properties,
                "Strings.Designer.cs is out of sync with Strings.resx (run the ResX custom tool).");
        }

        private static List<int> PlaceholderIndexes(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<int>();

            // "{{" / "}}" are escaped braces, not placeholders.
            var unescaped = value.Replace("{{", string.Empty).Replace("}}", string.Empty);
            return Placeholder.Matches(unescaped).Cast<Match>()
                .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}
