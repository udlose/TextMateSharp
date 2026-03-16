using NUnit.Framework;
using System.Collections.Generic;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Themes
{
    [TestFixture]
    public class ParsedThemeRuleTests
    {
        #region Test constants

        private const string SampleName = "test.rule";
        private const string SampleScope = "source.cs";
        private const int SampleIndex = 5;
        private const FontStyle SampleFontStyle = FontStyle.Bold;
        private const string SampleForeground = "#FF0000";
        private const string SampleBackground = "#00FF00";

        private const string AlternateName = "alternate.rule";
        private const string AlternateScope = "meta.test";
        private const int AlternateIndex = 10;
        private const FontStyle AlternateFontStyle = FontStyle.Italic;
        private const string AlternateForeground = "#0000FF";
        private const string AlternateBackground = "#FFFF00";

        #endregion

        #region Constructor tests

        [Test]
        public void Constructor_AssignsAllProperties()
        {
            // arrange
            List<string> parentScopes = new List<string> { "parent1", "parent2" };

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                parentScopes,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.AreEqual(SampleName, rule.name);
            Assert.AreEqual(SampleScope, rule.scope);
            Assert.AreSame(parentScopes, rule.parentScopes);
            Assert.AreEqual(SampleIndex, rule.index);
            Assert.AreEqual(SampleFontStyle, rule.fontStyle);
            Assert.AreEqual(SampleForeground, rule.foreground);
            Assert.AreEqual(SampleBackground, rule.background);
        }

        [Test]
        public void Constructor_AllowsNullName()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                null,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.IsNull(rule.name);
        }

        [Test]
        public void Constructor_AllowsNullParentScopes()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.IsNull(rule.parentScopes);
        }

        [Test]
        public void Constructor_AllowsNullForeground()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            // assert
            Assert.IsNull(rule.foreground);
        }

        [Test]
        public void Constructor_AllowsNullBackground()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            // assert
            Assert.IsNull(rule.background);
        }

        [Test]
        public void Constructor_AllowsEmptyParentScopesList()
        {
            // arrange
            List<string> emptyParentScopes = new List<string>();

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                emptyParentScopes,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.IsNotNull(rule.parentScopes);
            Assert.AreEqual(0, rule.parentScopes.Count);
        }

        [Test]
        public void Constructor_AllowsNullScope()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.IsNull(rule.scope);
        }

        [Test]
        public void Constructor_AllowsNotSetFontStyle()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                FontStyle.NotSet,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.AreEqual(FontStyle.NotSet, rule.fontStyle);
        }

        [Test]
        public void Constructor_AllowsZeroIndex()
        {
            // arrange
            const int zeroIndex = 0;

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                zeroIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.AreEqual(zeroIndex, rule.index);
        }

        [Test]
        public void Constructor_AllowsNegativeIndex()
        {
            // arrange
            const int negativeIndex = -1;

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                negativeIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.AreEqual(negativeIndex, rule.index);
        }

        [Test]
        public void Constructor_AllowsWhitespaceInScope()
        {
            // arrange
            const string scopeWithWhitespace = "  source.cs  ";

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, scopeWithWhitespace, new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // assert
            Assert.AreEqual(scopeWithWhitespace, rule.scope);
        }

        [Test]
        public void Equals_ScopeWithDifferentWhitespace_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName, "source.cs", new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName, " source.cs ", new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // act/assert
            Assert.IsFalse(rule1.Equals(rule2));
        }

        [Test]
        public void Constructor_AllowsSpecialCharactersInScope()
        {
            // arrange
            const string scopeWithSpecialChars = "source.c++.@#$%^&*()";

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, scopeWithSpecialChars, new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // assert
            Assert.AreEqual(scopeWithSpecialChars, rule.scope);
        }

        [Test]
        public void Constructor_AllowsEmptyStringScope()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, string.Empty, new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // assert
            Assert.AreEqual(string.Empty, rule.scope);
        }

        [Test]
        public void Equals_EmptyStringVsNull_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName, string.Empty, new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName, null, new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // act/assert
            Assert.IsFalse(rule1.Equals(rule2));
        }

        [Test]
        public void Constructor_AllowsEmptyStringForeground()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, SampleScope, new List<string>(), SampleIndex,
                SampleFontStyle, string.Empty, SampleBackground);

            // assert
            Assert.AreEqual(string.Empty, rule.foreground);
        }

        [Test]
        public void Constructor_AllowsEmptyStringBackground()
        {
            // arrange/act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, SampleScope, new List<string>(), SampleIndex,
                SampleFontStyle, SampleForeground, string.Empty);

            // assert
            Assert.AreEqual(string.Empty, rule.background);
        }

        [Test]
        public void Constructor_AllowsCombinedFontStyles()
        {
            // arrange
            const FontStyle combinedStyle = FontStyle.Bold | FontStyle.Italic;

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                combinedStyle,
                SampleForeground,
                SampleBackground);

            // assert
            Assert.AreEqual(combinedStyle, rule.fontStyle);
        }

        [Test]
        public void Constructor_AllowsParentScopesWithEmptyStrings()
        {
            // arrange
            List<string> parentScopesWithEmpty = new List<string> { "parent1", "", "parent2" };

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, SampleScope, parentScopesWithEmpty, SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // assert
            Assert.AreEqual(3, rule.parentScopes.Count);
            Assert.AreEqual("", rule.parentScopes[1]);
        }

        [Test]
        public void Constructor_AllowsParentScopesWithNullElements()
        {
            // arrange
            List<string> parentScopesWithNull = new List<string> { "parent1", null, "parent2" };

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, SampleScope, parentScopesWithNull, SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // assert
            Assert.AreEqual(3, rule.parentScopes.Count);
            Assert.IsNull(rule.parentScopes[1]);
        }

        [Test]
        public void Constructor_AllowsLargeParentScopesList()
        {
            // arrange
            const int largeListSize = 1000;
            List<string> largeParentScopes = new List<string>(largeListSize);
            for (int i = 0; i < largeListSize; i++)
            {
                largeParentScopes.Add($"scope{i}");
            }

            // act
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, SampleScope, largeParentScopes, SampleIndex,
                SampleFontStyle, SampleForeground, SampleBackground);

            // assert
            Assert.AreEqual(largeListSize, rule.parentScopes.Count);
        }

        #endregion Constructor tests

        #region GetHashCode tests

        [Test]
        public void GetHashCode_IsDeterministic_During_LifetimeOfApplication()
        {
            // arrange
            ParsedThemeRule rule = CreateSampleRule();

            // act
            int firstHash = rule.GetHashCode();
            int secondHash = rule.GetHashCode();

            // assert
            Assert.AreEqual(firstHash, secondHash);
        }

        [Test]
        public void GetHashCode_EqualObjects_ProduceSameHashCode()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = CreateSampleRule();

            // act
            int hash1 = rule1.GetHashCode();
            int hash2 = rule2.GetHashCode();

            // assert
            Assert.IsTrue(rule1.Equals(rule2));
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_BothNameNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                null,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);
            ParsedThemeRule rule2 = new ParsedThemeRule(
                null,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            // Note: Not guaranteed, but should happen most of the time
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_OneNameNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                null,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);
            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            // Note: Not guaranteed, but should happen most of the time
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_BothScopeNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            // Note: Not guaranteed, but should happen most of the time
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_OneScopeNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            // Note: Not guaranteed, but should happen most of the time
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_BothForegroundNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            // act/assert
            // Note: Not guaranteed, but should happen most of the time
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_OneForegroundNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            // Note: Not guaranteed, but should happen most of the time
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_BothBackgroundNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            // act/assert
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_OneBackgroundNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_BothParentScopesNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_OneParentScopesNull_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_ParentScopesDifferentOrder_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent2", "parent1" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_ParentScopesDifferentLength_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_EmptyParentScopes_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentParentScopesOrder_ButSameContent_ProducesDifferentHashCodes()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent2", "parent1" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentParentScopesContent_ProducesDifferentHashCodes()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent3", "parent4" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_SameParentScopesListReference_ProduceSameHashCode()
        {
            // arrange
            List<string> sharedParentScopes = new List<string> { "parent1", "parent2" };

            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                sharedParentScopes,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                sharedParentScopes,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            int hash1 = rule1.GetHashCode();
            int hash2 = rule2.GetHashCode();

            // assert
            Assert.IsTrue(rule1.Equals(rule2));
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_ParentScopesWithDifferentValues_ProducesDifferentHashCodes()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "scope.a", "scope.b", "scope.c" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "scope.a", "scope.x", "scope.c" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            // Different value at index 1
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentCombinedFontStyles_ProducesDifferentHashCodes()
        {
            // arrange
            const FontStyle style1 = FontStyle.Bold | FontStyle.Italic;
            const FontStyle style2 = FontStyle.Bold | FontStyle.Underline;

            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName, SampleScope, new List<string>(), SampleIndex,
                style1, SampleForeground, SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName, SampleScope, new List<string>(), SampleIndex,
                style2, SampleForeground, SampleBackground);

            // act/assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_NullBackground_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            // act/assert
            Assert.DoesNotThrow(() => _ = rule.GetHashCode());
        }

        [Test]
        public void GetHashCode_NullForeground_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            // act/assert
            Assert.DoesNotThrow(() => _ = rule.GetHashCode());
        }

        [Test]
        public void GetHashCode_NullParentScopes_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.DoesNotThrow(() => _ = rule.GetHashCode());
        }

        [Test]
        public void GetHashCode_NullScope_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act/assert
            Assert.DoesNotThrow(() => _ = rule.GetHashCode());
        }

        [Test]
        public void GetHashCode_WithNullProperties_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(null, null, null, 0, 0, null, null);

            // act/assert
            Assert.DoesNotThrow(() => _ = rule.GetHashCode());
        }

        [Test]
        public void GetHashCode_CanBeUsedAsDictionaryKey()
        {
            // arrange
            ParsedThemeRule key1 = CreateSampleRule();
            ParsedThemeRule key2 = CreateSampleRule();
            Dictionary<ParsedThemeRule, string> dictionary = new Dictionary<ParsedThemeRule, string>
            {
                [key1] = "VALUE"
            };

            // act
            bool found = dictionary.TryGetValue(key2, out string value);

            // assert
            Assert.IsTrue(found);
            Assert.AreEqual("VALUE", value);
        }

        [Test]
        public void GetHashCode_CanBeUsedInHashSet()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = CreateSampleRule();
            HashSet<ParsedThemeRule> hashSet = new HashSet<ParsedThemeRule> { rule1 };

            // act
            bool containsRule2 = hashSet.Contains(rule2);
            bool addedRule2 = hashSet.Add(rule2);

            // assert
            Assert.IsTrue(containsRule2, "HashSet should contain structurally equal object");
            Assert.IsFalse(addedRule2, "HashSet should not add duplicate equal object");
            Assert.AreEqual(1, hashSet.Count, "HashSet should contain only one item");
        }

        #endregion GetHashCode tests

        #region Equals tests

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule = CreateSampleRule();

            // act
            bool result = rule.Equals(null);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule = CreateSampleRule();
            object other = 42;

            // act
            bool result = rule.Equals(other);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule = CreateSampleRule();

            // act
            bool result = rule.Equals(rule);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_StructurallyEqualObjects_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = CreateSampleRule();

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_SameParentScopesListReference_ReturnsTrue()
        {
            // arrange
            List<string> sharedParentScopes = new List<string> { "parent1", "parent2" };

            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                sharedParentScopes,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                sharedParentScopes,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentName_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = new ParsedThemeRule(
                AlternateName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentScope_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                AlternateScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentIndex_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                AlternateIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentFontStyle_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                AlternateFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentForeground_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                AlternateForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentBackground_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                AlternateBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_BothParentScopesNull_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OneParentScopesNull_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ParentScopesDifferentOrder_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent2", "parent1" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ParentScopesDifferentLength_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_EmptyParentScopes_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_BothForegroundNull_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OneForegroundNull_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_BothBackgroundNull_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OneBackgroundNull_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_BothScopeNull_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OneScopeNull_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_BothNameNull_ReturnsTrue()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                null,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);
            ParsedThemeRule rule2 = new ParsedThemeRule(
                null,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);
            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_OneNameNull_ReturnsFalse()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                null,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);
            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);
            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_ParentScopesWithDuplicates_ComparesCorrectly()
        {
            // arrange
            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName, SampleScope, new List<string> { "parent1", "parent1" },
                SampleIndex, SampleFontStyle, SampleForeground, SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName, SampleScope, new List<string> { "parent1", "parent1" },
                SampleIndex, SampleFontStyle, SampleForeground, SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentCombinedFontStyles_ReturnsFalse()
        {
            // arrange
            const FontStyle style1 = FontStyle.Bold | FontStyle.Italic;
            const FontStyle style2 = FontStyle.Bold | FontStyle.Underline;

            ParsedThemeRule rule1 = new ParsedThemeRule(
                SampleName, SampleScope, new List<string>(), SampleIndex,
                style1, SampleForeground, SampleBackground);

            ParsedThemeRule rule2 = new ParsedThemeRule(
                SampleName, SampleScope, new List<string>(), SampleIndex,
                style2, SampleForeground, SampleBackground);

            // act
            bool result = rule1.Equals(rule2);

            // assert
            Assert.IsFalse(result);
        }

        #endregion Equals tests

        #region ToString tests

        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            // arrange
            ParsedThemeRule rule = CreateSampleRule();

            // act
            string result = rule.ToString();

            // assert
            Assert.IsTrue(result.Contains("scope=" + SampleScope));
            Assert.IsTrue(result.Contains("index=" + SampleIndex));
            Assert.IsTrue(result.Contains("fontStyle=" + SampleFontStyle));
            Assert.IsTrue(result.Contains("foreground=" + SampleForeground));
            Assert.IsTrue(result.Contains("background=" + SampleBackground));
            Assert.IsTrue(result.Contains("parent1"));
            Assert.IsTrue(result.Contains("parent2"));
        }

        [Test]
        public void ToString_ReturnsSameResultForEqualObjects()
        {
            // arrange
            ParsedThemeRule rule1 = CreateSampleRule();
            ParsedThemeRule rule2 = CreateSampleRule();
            // act
            string result1 = rule1.ToString();
            string result2 = rule2.ToString();
            // assert
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void ToString_NullParentScopes_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                null,
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            string result = rule.ToString();

            // assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ToString_EmptyParentScopes_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            string result = rule.ToString();

            // assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ToString_NullForeground_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                null,
                SampleBackground);

            // act
            string result = rule.ToString();

            // assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ToString_NullBackground_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                null);

            // act
            string result = rule.ToString();

            // assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ToString_NullScope_DoesNotThrow()
        {
            // arrange
            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName,
                null,
                new List<string>(),
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);

            // act
            string result = rule.ToString();

            // assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ToString_CombinedFontStyles_DoesNotThrow()
        {
            // arrange
            const FontStyle allStyles = FontStyle.Bold | FontStyle.Italic |
                                        FontStyle.Underline | FontStyle.Strikethrough;

            ParsedThemeRule rule = new ParsedThemeRule(
                SampleName, SampleScope, new List<string>(), SampleIndex,
                allStyles, SampleForeground, SampleBackground);

            // act
            string result = rule.ToString();

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(allStyles.ToString()));
        }

        #endregion ToString tests

        #region Helper methods

        private static ParsedThemeRule CreateSampleRule()
        {
            return new ParsedThemeRule(
                SampleName,
                SampleScope,
                new List<string> { "parent1", "parent2" },
                SampleIndex,
                SampleFontStyle,
                SampleForeground,
                SampleBackground);
        }

        #endregion Helper methods
    }
}