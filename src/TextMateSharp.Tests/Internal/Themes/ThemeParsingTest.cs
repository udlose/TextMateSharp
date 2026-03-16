using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Internal.Themes
{
    [TestFixture]
    internal class ThemeParsingTest
    {
        [TestCase()]
        public void Parse_Theme_Rule_Should_Work()
        {
            using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(THEME_JSON));

            using StreamReader reader = new StreamReader(memoryStream);
            var theme = ThemeReader.ReadThemeSync(reader);

            var actualThemeRules = ParsedTheme.ParseTheme(theme);

            var expectedThemeRules = new ParsedThemeRule[] {
                new ParsedThemeRule("", "", null, 0, FontStyle.NotSet, "#F8F8F2", "#272822"),
                new ParsedThemeRule("Michael", "source", null, 1, FontStyle.NotSet, null, "#100000"),
                new ParsedThemeRule("Michael", "something", null, 1, FontStyle.NotSet, null, "#100000"),
                new ParsedThemeRule("", "bar", null, 2, FontStyle.NotSet, null, "#010000"),
                new ParsedThemeRule("", "baz", null, 2, FontStyle.NotSet, null, "#010000"),
                new ParsedThemeRule("Dwight", "bar", new List<string>() {"bar", "selector", "source.css" }, 3, FontStyle.Bold, null, null),
                new ParsedThemeRule("Jim", "constant", null, 4, FontStyle.Italic, "#ff0000", null),
                new ParsedThemeRule("Stanley", "constant.numeric", null, 5, FontStyle.NotSet, "#00ff00", null),
                new ParsedThemeRule("Phyllis", "constant.numeric.hex", null, 6, FontStyle.Bold, null, null),
                new ParsedThemeRule("Ryan", "constant.numeric.oct", null, 7, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline, null, null),
                new ParsedThemeRule("Kelly", "constant.numeric.bin", null, 8, FontStyle.Bold | FontStyle.Strikethrough, null, null),
                new ParsedThemeRule("Toby", "constant.numeric.dec", null, 9, FontStyle.None, "#0000ff", null),
                new ParsedThemeRule("Creed", "foo", null, 10, FontStyle.None, "#CFA", null)
            };

            Assert.AreEqual(expectedThemeRules.Length, actualThemeRules.Count);

            for (int i = 0; i < actualThemeRules.Count; i++)
            {
                ParsedThemeRule expected = expectedThemeRules[i];
                ParsedThemeRule actual = actualThemeRules[i];
                Debug.WriteLine($"running test# {i}");
                Debug.WriteLine($"Expected: {expected}");
                Debug.WriteLine($"Actual: {actual}");
                Assert.AreEqual(expected, actual);
                Debug.WriteLine($"passed test# {i}");
            }
        }

        const string THEME_JSON =
            """
            { "settings": [
            { "name": "", "settings": {"foreground": "#F8F8F2", "background": "#272822" } },
            { "name": "Michael", "scope": "source, something", "settings": { "background": "#100000" } },
            { "name": "", "scope": ["bar", "baz"], "settings": { "background": "#010000" } },
            { "name": "Dwight", "scope": "source.css selector bar", "settings": { "fontStyle": "bold" } },
            { "name": "Jim", "scope": "constant", "settings": { "fontStyle": "italic", "foreground": "#ff0000" } },
            { "name": "Stanley", "scope": "constant.numeric", "settings": { "foreground": "#00ff00" } },
            { "name": "Phyllis", "scope": "constant.numeric.hex", "settings": { "fontStyle": "bold" } },
            { "name": "Ryan", "scope": "constant.numeric.oct", "settings": { "fontStyle": "bold italic underline" } },
            { "name": "Kelly", "scope": "constant.numeric.bin", "settings": { "fontStyle": "bold strikethrough" } },
            { "name": "Toby", "scope": "constant.numeric.dec", "settings": { "fontStyle": "", "foreground": "#0000ff" } },
            { "name": "Creed", "scope": "foo", "settings": { "fontStyle": "", "foreground": "#CFA" } }
            ]}
            """;
    }
}
