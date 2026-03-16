using NUnit.Framework;
using System.Collections.Generic;
using TextMateSharp.Tests.Helpers;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Themes
{
    [TestFixture]
    public class ThemeTrieElementRuleTests
    {
        // Shared constants to avoid magic numbers scattered throughout tests
        private const int DefaultScopeDepth = 1;
        private const int DefaultForeground = 10;
        private const int DefaultBackground = 20;
        private const FontStyle DefaultFontStyle = FontStyle.Bold;
        private const string DefaultName = "test.rule";

        #region Constructor tests

        [Test]
        public void Constructor_AssignsAllFields()
        {
            // Arrange
            const string name = "rule.name";
            const int scopeDepth = 5;
            List<string> parentScopes = new List<string> { "source.cs", "meta.class" };
            const FontStyle fontStyle = FontStyle.Italic;
            const int foreground = 42;
            const int background = 99;

            // Act
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                name, scopeDepth, parentScopes, fontStyle, foreground, background);

            // Assert
            Assert.AreEqual(name, rule.name);
            Assert.AreEqual(scopeDepth, rule.scopeDepth);
            Assert.AreSame(parentScopes, rule.parentScopes);
            Assert.AreEqual(fontStyle, rule.fontStyle);
            Assert.AreEqual(foreground, rule.foreground);
            Assert.AreEqual(background, rule.background);
        }

        [Test]
        public void Constructor_NullName_AssignsNull()
        {
            // Arrange & Act
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                null, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Assert
            Assert.IsNull(rule.name);
        }

        [Test]
        public void Constructor_NullParentScopes_AssignsNull()
        {
            // Arrange & Act
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Assert
            Assert.IsNull(rule.parentScopes);
        }

        [Test]
        public void Constructor_EmptyParentScopes_AssignsEmptyList()
        {
            // Arrange
            List<string> emptyScopes = new List<string>();

            // Act
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, emptyScopes, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Assert
            Assert.AreSame(emptyScopes, rule.parentScopes);
            Assert.AreEqual(0, rule.parentScopes.Count);
        }

        [Test]
        public void Constructor_ZeroValues_AssignsAllZeros()
        {
            // Arrange & Act
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                null, 0, null, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual(0, rule.scopeDepth);
            Assert.AreEqual(FontStyle.NotSet, rule.fontStyle);
            Assert.AreEqual(0, rule.foreground);
            Assert.AreEqual(0, rule.background);
        }

        #endregion Constructor tests

        #region Clone tests

        [Test]
        public void Clone_ReturnsDifferentInstance()
        {
            // Arrange
            ThemeTrieElementRule original = CreateDefaultRule();

            // Act
            ThemeTrieElementRule cloned = original.Clone();

            // Assert
            Assert.IsFalse(ReferenceEquals(original, cloned));
        }

        [Test]
        public void Clone_CopiesAllValueFields()
        {
            // Arrange
            ThemeTrieElementRule original = new ThemeTrieElementRule(
                "test.name", 5, new List<string> { "source" }, FontStyle.Italic, 42, 99);

            // Act
            ThemeTrieElementRule cloned = original.Clone();

            // Assert
            Assert.AreEqual(original.name, cloned.name);
            Assert.AreEqual(original.scopeDepth, cloned.scopeDepth);
            Assert.AreEqual(original.fontStyle, cloned.fontStyle);
            Assert.AreEqual(original.foreground, cloned.foreground);
            Assert.AreEqual(original.background, cloned.background);
        }

        [Test]
        public void Clone_SharesParentScopesReference()
        {
            // Arrange - Clone shares the parentScopes list reference (shallow copy),
            // matching the Java upstream behavior where Clone does not deep-copy the list.
            List<string> parentScopes = new List<string> { "source.cs", "meta.class" };
            ThemeTrieElementRule original = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, parentScopes, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            ThemeTrieElementRule cloned = original.Clone();

            // Assert
            Assert.AreSame(original.parentScopes, cloned.parentScopes);
        }

        [Test]
        public void Clone_NullParentScopes_ClonesCorrectly()
        {
            // Arrange
            ThemeTrieElementRule original = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            ThemeTrieElementRule cloned = original.Clone();

            // Assert
            Assert.IsNull(cloned.parentScopes);
        }

        [Test]
        public void Clone_MutatingClone_DoesNotAffectOriginalValueFields()
        {
            // Arrange
            const string originalName = "original";
            const FontStyle originalFontStyle = FontStyle.Bold;
            const int originalForeground = 10;
            const int originalBackground = 20;
            ThemeTrieElementRule original = new ThemeTrieElementRule(
                originalName, 1, new List<string> { "source" },
                originalFontStyle, originalForeground, originalBackground);

            // Act
            ThemeTrieElementRule cloned = original.Clone();
            ReflectionTestHelper.SetProperty(cloned, "scopeDepth", 99);
            ReflectionTestHelper.SetProperty(cloned, "fontStyle", FontStyle.Strikethrough);
            ReflectionTestHelper.SetProperty(cloned, "foreground", 999);
            ReflectionTestHelper.SetProperty(cloned, "background", 888);
            ReflectionTestHelper.SetProperty(cloned, "name", "mutated");

            // Assert - original should be unaffected
            Assert.AreEqual(originalName, original.name);
            Assert.AreEqual(1, original.scopeDepth);
            Assert.AreEqual(originalFontStyle, original.fontStyle);
            Assert.AreEqual(originalForeground, original.foreground);
            Assert.AreEqual(originalBackground, original.background);
        }

        [Test]
        public void Clone_ProducesEqualRule()
        {
            // Arrange
            ThemeTrieElementRule original = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            ThemeTrieElementRule cloned = original.Clone();

            // Assert
            Assert.AreNotSame(original, cloned);
            Assert.IsTrue(original.Equals(cloned));
            Assert.AreEqual(original.GetHashCode(), cloned.GetHashCode());
        }

        #endregion Clone tests

        #region cloneArr tests

        [Test]
        public void CloneArr_ReturnsDifferentListInstance()
        {
            // Arrange
            List<ThemeTrieElementRule> original = new List<ThemeTrieElementRule>
            {
                CreateDefaultRule()
            };

            // Act
            List<ThemeTrieElementRule> cloned = ThemeTrieElementRule.CloneArr(original);

            // Assert
            Assert.IsFalse(ReferenceEquals(original, cloned));
        }

        [Test]
        public void CloneArr_PreservesCount()
        {
            // Arrange
            List<ThemeTrieElementRule> original = new List<ThemeTrieElementRule>
            {
                new ThemeTrieElementRule("r1", 1, null, FontStyle.Bold, 1, 2),
                new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 3, 4),
                new ThemeTrieElementRule("r3", 3, null, FontStyle.Underline, 5, 6)
            };

            // Act
            List<ThemeTrieElementRule> cloned = ThemeTrieElementRule.CloneArr(original);

            // Assert
            Assert.AreEqual(original.Count, cloned.Count);
        }

        [Test]
        public void CloneArr_EachElementIsDifferentInstance()
        {
            // Arrange
            List<ThemeTrieElementRule> original = new List<ThemeTrieElementRule>
            {
                new ThemeTrieElementRule("r1", 1, null, FontStyle.Bold, 1, 2),
                new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 3, 4)
            };

            // Act
            List<ThemeTrieElementRule> cloned = ThemeTrieElementRule.CloneArr(original);

            // Assert
            for (int i = 0; i < original.Count; i++)
            {
                Assert.IsFalse(ReferenceEquals(original[i], cloned[i]));
            }
        }

        [Test]
        public void CloneArr_EachElementIsEqual()
        {
            // Arrange
            List<ThemeTrieElementRule> original = new List<ThemeTrieElementRule>
            {
                new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 1, 2),
                new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 3, 4)
            };

            // Act
            List<ThemeTrieElementRule> cloned = ThemeTrieElementRule.CloneArr(original);

            // Assert
            for (int i = 0; i < original.Count; i++)
            {
                Assert.IsTrue(original[i].Equals(cloned[i]));
                Assert.AreEqual(original[i].GetHashCode(), cloned[i].GetHashCode());
            }
        }

        [Test]
        public void CloneArr_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            List<ThemeTrieElementRule> original = new List<ThemeTrieElementRule>();

            // Act
            List<ThemeTrieElementRule> cloned = ThemeTrieElementRule.CloneArr(original);

            // Assert
            Assert.AreEqual(0, cloned.Count);
            Assert.IsFalse(ReferenceEquals(original, cloned));
        }

        #endregion cloneArr tests

        #region AcceptOverwrite tests

        [Test]
        public void AcceptOverwrite_HigherScopeDepth_UpdatesScopeDepth()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite("new", 5, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual(5, rule.scopeDepth);
        }

        [Test]
        public void AcceptOverwrite_EqualScopeDepth_UpdatesScopeDepth()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, 5, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite("new", 5, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual(5, rule.scopeDepth);
        }

        [Test]
        public void AcceptOverwrite_LowerScopeDepth_DoesNotUpdateScopeDepth()
        {
            // Arrange - when incoming scopeDepth is lower, the existing depth is preserved
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, 10, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite("new", 5, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual(10, rule.scopeDepth);
        }

        [Test]
        public void AcceptOverwrite_NonNotSetFontStyle_UpdatesFontStyle()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, FontStyle.Bold, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, FontStyle.Italic, 0, 0);

            // Assert
            Assert.AreEqual(FontStyle.Italic, rule.fontStyle);
        }

        [Test]
        public void AcceptOverwrite_NotSetFontStyle_PreservesExistingFontStyle()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, FontStyle.Bold, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual(FontStyle.Bold, rule.fontStyle);
        }

        [Test]
        public void AcceptOverwrite_NonZeroForeground_UpdatesForeground()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 10, DefaultBackground);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, FontStyle.NotSet, 42, 0);

            // Assert
            Assert.AreEqual(42, rule.foreground);
        }

        [Test]
        public void AcceptOverwrite_ZeroForeground_PreservesExistingForeground()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 10, DefaultBackground);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual(10, rule.foreground);
        }

        [Test]
        public void AcceptOverwrite_NonZeroBackground_UpdatesBackground()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 10);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, FontStyle.NotSet, 0, 42);

            // Assert
            Assert.AreEqual(42, rule.background);
        }

        [Test]
        public void AcceptOverwrite_ZeroBackground_PreservesExistingBackground()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 10);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual(10, rule.background);
        }

        [Test]
        public void AcceptOverwrite_NonEmptyName_UpdatesName()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                "original.name", DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite("new.name", DefaultScopeDepth, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual("new.name", rule.name);
        }

        [Test]
        public void AcceptOverwrite_NullName_PreservesExistingName()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                "original.name", DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite(null, DefaultScopeDepth, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual("original.name", rule.name);
        }

        [Test]
        public void AcceptOverwrite_EmptyName_PreservesExistingName()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                "original.name", DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite("", DefaultScopeDepth, FontStyle.NotSet, 0, 0);

            // Assert
            Assert.AreEqual("original.name", rule.name);
        }

        [Test]
        public void AcceptOverwrite_AllFieldsChanged_UpdatesAllFields()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                "old", 1, null, FontStyle.Bold, 10, 20);

            // Act
            rule.AcceptOverwrite("new", 5, FontStyle.Italic, 42, 99);

            // Assert
            Assert.AreEqual("new", rule.name);
            Assert.AreEqual(5, rule.scopeDepth);
            Assert.AreEqual(FontStyle.Italic, rule.fontStyle);
            Assert.AreEqual(42, rule.foreground);
            Assert.AreEqual(99, rule.background);
        }

        [Test]
        public void AcceptOverwrite_AllNoOpValues_PreservesAllFields()
        {
            // Arrange - all "no-op" sentinel values: NotSet fontStyle, 0 fg/bg, null name
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                "original", 10, null, FontStyle.Bold, 42, 99);

            // Act
            rule.AcceptOverwrite(null, 5, FontStyle.NotSet, 0, 0);

            // Assert - scopeDepth doesn't update because 5 < 10
            Assert.AreEqual("original", rule.name);
            Assert.AreEqual(10, rule.scopeDepth);
            Assert.AreEqual(FontStyle.Bold, rule.fontStyle);
            Assert.AreEqual(42, rule.foreground);
            Assert.AreEqual(99, rule.background);
        }

        [Test]
        public void AcceptOverwrite_DoesNotModifyParentScopes()
        {
            // Arrange - AcceptOverwrite has no parentScopes parameter, so it should remain unchanged
            List<string> parentScopes = new List<string> { "source.cs" };
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, parentScopes, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite("new", 5, FontStyle.Italic, 42, 99);

            // Assert
            Assert.AreSame(parentScopes, rule.parentScopes);
            Assert.AreEqual(1, rule.parentScopes.Count);
            Assert.AreEqual("source.cs", rule.parentScopes[0]);
        }

        [Test]
        public void AcceptOverwrite_CalledMultipleTimes_LastWins()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                "initial", 0, null, FontStyle.NotSet, 0, 0);

            // Act
            rule.AcceptOverwrite("first", 1, FontStyle.Bold, 10, 20);
            rule.AcceptOverwrite("second", 2, FontStyle.Italic, 30, 40);
            rule.AcceptOverwrite("third", 3, FontStyle.Underline, 50, 60);

            // Assert
            Assert.AreEqual("third", rule.name);
            Assert.AreEqual(3, rule.scopeDepth);
            Assert.AreEqual(FontStyle.Underline, rule.fontStyle);
            Assert.AreEqual(50, rule.foreground);
            Assert.AreEqual(60, rule.background);
        }

        [Test]
        public void AcceptOverwrite_FontStyleNone_UpdatesFontStyle()
        {
            // Arrange - FontStyle.None is different from FontStyle.NotSet.
            // None means "explicitly no style", NotSet means "don't change".
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, FontStyle.Bold, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, FontStyle.None, 0, 0);

            // Assert
            Assert.AreEqual(FontStyle.None, rule.fontStyle);
        }

        [Test]
        public void AcceptOverwrite_CombinedFontStyles_UpdatesFontStyle()
        {
            // Arrange
            FontStyle combined = FontStyle.Bold | FontStyle.Italic | FontStyle.Underline;
            ThemeTrieElementRule rule = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, FontStyle.NotSet, DefaultForeground, DefaultBackground);

            // Act
            rule.AcceptOverwrite(DefaultName, DefaultScopeDepth, combined, 0, 0);

            // Assert
            Assert.AreEqual(combined, rule.fontStyle);
        }

        #endregion AcceptOverwrite tests

        #region Equals(object) tests

        [Test]
        public void Equals_Object_SameReference_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act
            bool result = rule.Equals((object)rule);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_Object_Null_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act
            bool result = rule.Equals((object)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_Object_DifferentType_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act
            bool result = rule.Equals("not a rule");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_Object_StructurallyEqual_ReturnsTrue()
        {
            // Arrange
            List<string> parentScopes = new List<string> { "source.cs", "meta.class" };
            ThemeTrieElementRule left = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, parentScopes, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, parentScopes, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion Equals(object) tests

        #region Equals deep comparison tests

        [Test]
        public void IEquatable_Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act
            bool result = rule.Equals(rule);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_Null_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act
            bool result = rule.Equals((ThemeTrieElementRule)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IEquatable_Equals_AllFieldsMatch_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_DifferentScopeDepth_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, 5, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_DifferentFontStyle_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Bold, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Italic, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_DifferentForeground_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 10, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 99, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_DifferentBackground_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 20);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 99);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_BothNullParentScopes_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_LeftNullParentScopes_RightNonNull_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_LeftNonNullParentScopes_RightNull_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_DifferentParentScopesLength_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source", "meta" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_DifferentParentScopesContent_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.js" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_SameParentScopesReference_ReturnsTrue()
        {
            // Arrange - both rules share the same List instance (common in Clone scenarios)
            List<string> sharedParentScopes = new List<string> { "source.cs", "meta.class" };
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, sharedParentScopes, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, sharedParentScopes, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_BothEmptyParentScopes_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string>(), DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string>(), DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_IsSymmetric()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.IsTrue(right.Equals(left));
        }

        [Test]
        public void IEquatable_Equals_IsTransitive()
        {
            // Arrange
            ThemeTrieElementRule a = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule b = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule c = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(c));
            Assert.IsTrue(a.Equals(c));
        }

        [Test]
        public void IEquatable_Equals_Different_Names_ReturnsFalse()
        {
            // Arrange - name differs but all other fields match.
            // The original Java implementation does not include name in equality.
            ThemeTrieElementRule left = new ThemeTrieElementRule("rule.one", DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule("rule.two", DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_UsedByEqualityComparerDefault()
        {
            // Arrange
            ThemeTrieElementRule key1 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule key2 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            EqualityComparer<ThemeTrieElementRule> comparer = EqualityComparer<ThemeTrieElementRule>.Default;

            // Act & Assert
            Assert.IsTrue(comparer.Equals(key1, key2));
            Assert.AreEqual(comparer.GetHashCode(key1), comparer.GetHashCode(key2));
        }

        [Test]
        public void IEquatable_Equals_ParentScopesComparedOrdinal()
        {
            // Arrange - ordinal comparison means case matters
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "Source.CS" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(left.Equals(right), "Ordinal comparison is case-sensitive");
        }

        [Test]
        public void IEquatable_Equals_ParentScopesWithNullElements_HandledCorrectly()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source", null, "meta" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source", null, "meta" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        #endregion Equals deep comparison tests

        #region GetHashCode tests

        [Test]
        public void GetHashCode_EqualRules_ReturnSameHashCode()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.cs", "meta.class" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.cs", "meta.class" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_IsConsistent()
        {
            // Arrange
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act
            int hash1 = rule.GetHashCode();
            int hash2 = rule.GetHashCode();
            int hash3 = rule.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(hash2, hash3);
        }

        [Test]
        public void GetHashCode_NullParentScopes_DoesNotThrow()
        {
            // Arrange - the original code would NRE on parentScopes.GetHashCode() when null
            ThemeTrieElementRule rule = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.DoesNotThrow(() => rule.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentScopeDepth_DifferentHash()
        {
            // Arrange
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, 2, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentFontStyle_DifferentHash()
        {
            // Arrange
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Bold, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Italic, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentForeground_DifferentHash()
        {
            // Arrange
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 1, DefaultBackground);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 2, DefaultBackground);

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentBackground_DifferentHash()
        {
            // Arrange
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 1);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 2);

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentParentScopes_DifferentHash()
        {
            // Arrange
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.js" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        [Test]
        public void GetHashCode_NullVsEmptyParentScopes_DifferentHash()
        {
            // Arrange - null and empty have different equality semantics, so hashes should differ
            ThemeTrieElementRule ruleWithNull = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule ruleWithEmpty = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string>(), DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.AreNotEqual(ruleWithNull.GetHashCode(), ruleWithEmpty.GetHashCode());
        }

        [Test]
        public void GetHashCode_AfterAcceptOverwrite_ReflectsNewValues()
        {
            // Arrange
            ThemeTrieElementRule rule = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Bold, 1, 2);
            int hashBefore = rule.GetHashCode();

            // Act
            rule.AcceptOverwrite("new.name", 5, FontStyle.Italic, 10, 20);

            // Assert
            Assert.AreNotEqual(hashBefore, rule.GetHashCode(),
                "Hash must change after AcceptOverwrite mutates fields - no stale cached hash");
        }

        [Test]
        public void GetHashCode_ParentScopesOrderMatters()
        {
            // Arrange - order is semantically significant for parentScopes
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "a", "b" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "b", "a" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.AreNotEqual(rule1.GetHashCode(), rule2.GetHashCode());
        }

        #endregion GetHashCode tests

        #region Helpers

        private static ThemeTrieElementRule CreateDefaultRule()
        {
            return new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
        }

        #endregion Helpers
    }
}