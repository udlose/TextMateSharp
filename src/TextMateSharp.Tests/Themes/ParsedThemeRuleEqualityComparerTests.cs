using NUnit.Framework;
using System.Collections.Generic;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Themes
{
    [TestFixture]
    public class ParsedThemeRuleEqualityComparerTests
    {
        private const string DefaultName = "test.rule";
        private const string DefaultScope = "source.cs";
        private const int DefaultIndex = 1;
        private const FontStyle DefaultFontStyle = FontStyle.Bold;
        private const string DefaultForeground = "#FF0000";
        private const string DefaultBackground = "#000000";

        private ParsedThemeRuleEqualityComparer _comparer;

        [SetUp]
        public void SetUp()
        {
            _comparer = ParsedThemeRuleEqualityComparer.Default;
        }

        private static ParsedThemeRule CreateDefaultRule()
        {
            return new ParsedThemeRule(
                DefaultName,
                DefaultScope,
                new List<string> { "meta.block" },
                DefaultIndex,
                DefaultFontStyle,
                DefaultForeground,
                DefaultBackground);
        }

        private static ParsedThemeRule CreateDefaultRuleWithParentScopes(List<string> parentScopes)
        {
            return new ParsedThemeRule(
                DefaultName,
                DefaultScope,
                parentScopes,
                DefaultIndex,
                DefaultFontStyle,
                DefaultForeground,
                DefaultBackground);
        }

        #region Default instance tests

        [Test]
        public void Default_ReturnsSingletonInstance()
        {
            // Arrange & Act
            ParsedThemeRuleEqualityComparer instance1 = ParsedThemeRuleEqualityComparer.Default;
            ParsedThemeRuleEqualityComparer instance2 = ParsedThemeRuleEqualityComparer.Default;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void Default_IsNotNull()
        {
            // Act & Assert
            Assert.IsNotNull(ParsedThemeRuleEqualityComparer.Default);
        }

        #endregion Default instance tests

        #region Equals tests - reference and null

        [Test]
        public void Equals_BothNull_ReturnsTrue()
        {
            // Arrange
            ParsedThemeRule left = null;
            ParsedThemeRule right = null;

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_LeftNull_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = null;
            ParsedThemeRule right = CreateDefaultRule();

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_RightNull_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = null;

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            ParsedThemeRule rule = CreateDefaultRule();

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(rule, rule));
        }

        #endregion Equals tests - reference and null

        #region Equals tests - field comparison

        [Test]
        public void Equals_DifferentScope_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = new ParsedThemeRule(
                DefaultName,
                "source.java",
                new List<string> { "meta.block" },
                DefaultIndex,
                DefaultFontStyle,
                DefaultForeground,
                DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentName_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = new ParsedThemeRule(
                "other.rule",
                DefaultScope,
                new List<string> { "meta.block" },
                DefaultIndex,
                DefaultFontStyle,
                DefaultForeground,
                DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentIndex_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = new ParsedThemeRule(
                DefaultName,
                DefaultScope,
                new List<string> { "meta.block" },
                999,
                DefaultFontStyle,
                DefaultForeground,
                DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentFontStyle_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = new ParsedThemeRule(
                DefaultName,
                DefaultScope,
                new List<string> { "meta.block" },
                DefaultIndex,
                FontStyle.Italic,
                DefaultForeground,
                DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentForeground_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = new ParsedThemeRule(
                DefaultName,
                DefaultScope,
                new List<string> { "meta.block" },
                DefaultIndex,
                DefaultFontStyle,
                "#00FF00",
                DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentBackground_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = new ParsedThemeRule(
                DefaultName,
                DefaultScope,
                new List<string> { "meta.block" },
                DefaultIndex,
                DefaultFontStyle,
                DefaultForeground,
                "#FFFFFF");

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_AllFieldsMatch_ReturnsTrue()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = CreateDefaultRule();

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
        }

        #endregion Equals tests - field comparison

        #region Equals tests - parentScopes deep comparison

        [Test]
        public void Equals_BothParentScopesNull_ReturnsTrue()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRuleWithParentScopes(null);
            ParsedThemeRule right = CreateDefaultRuleWithParentScopes(null);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_LeftParentScopesNull_RightNonNull_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRuleWithParentScopes(null);
            ParsedThemeRule right = CreateDefaultRuleWithParentScopes(new List<string> { "meta.block" });

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_LeftParentScopesNonNull_RightNull_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRuleWithParentScopes(new List<string> { "meta.block" });
            ParsedThemeRule right = CreateDefaultRuleWithParentScopes(null);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_ParentScopesDifferentCount_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRuleWithParentScopes(new List<string> { "meta.block" });
            ParsedThemeRule right = CreateDefaultRuleWithParentScopes(new List<string> { "meta.block", "source.cs" });

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_ParentScopesDifferentContent_ReturnsFalse()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRuleWithParentScopes(new List<string> { "meta.block" });
            ParsedThemeRule right = CreateDefaultRuleWithParentScopes(new List<string> { "meta.class" });

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_ParentScopesCaseSensitive_ReturnsFalse()
        {
            // Arrange - ordinal comparison means case matters
            ParsedThemeRule left = CreateDefaultRuleWithParentScopes(new List<string> { "meta.block" });
            ParsedThemeRule right = CreateDefaultRuleWithParentScopes(new List<string> { "Meta.Block" });

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        #endregion Equals tests - parentScopes deep comparison

        #region Equals tests - symmetry and transitivity

        [Test]
        public void Equals_IsSymmetric()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = CreateDefaultRule();

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
            Assert.IsTrue(_comparer.Equals(right, left));
        }

        [Test]
        public void Equals_IsTransitive()
        {
            // Arrange
            ParsedThemeRule a = CreateDefaultRule();
            ParsedThemeRule b = CreateDefaultRule();
            ParsedThemeRule c = CreateDefaultRule();

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(a, b));
            Assert.IsTrue(_comparer.Equals(b, c));
            Assert.IsTrue(_comparer.Equals(a, c));
        }

        #endregion Equals tests - symmetry and transitivity

        #region GetHashCode tests

        [Test]
        public void GetHashCode_NullRule_ReturnsZero()
        {
            // Act & Assert
            Assert.AreEqual(0, _comparer.GetHashCode(null));
        }

        [Test]
        public void GetHashCode_EqualRules_ReturnSameHash()
        {
            // Arrange
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = CreateDefaultRule();

            // Act & Assert
            Assert.AreEqual(_comparer.GetHashCode(left), _comparer.GetHashCode(right));
        }

        [Test]
        public void GetHashCode_ConsistentWithEquals()
        {
            // Arrange - if Equals is true, hash codes must match
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = CreateDefaultRule();

            // Act
            bool areEqual = _comparer.Equals(left, right);
            int leftHash = _comparer.GetHashCode(left);
            int rightHash = _comparer.GetHashCode(right);

            // Assert
            Assert.IsTrue(areEqual);
            Assert.AreEqual(leftHash, rightHash);
        }

        [Test]
        public void GetHashCode_ConsistentWithObjectGetHashCode()
        {
            // Arrange - comparer delegates to obj.GetHashCode()
            ParsedThemeRule rule = CreateDefaultRule();

            // Act & Assert
            Assert.AreEqual(rule.GetHashCode(), _comparer.GetHashCode(rule));
        }

        [Test]
        public void GetHashCode_DifferentRules_ReturnDifferentHash()
        {
            // Arrange - not guaranteed by contract, but a quality check
            ParsedThemeRule left = CreateDefaultRule();
            ParsedThemeRule right = new ParsedThemeRule(
                "other.name",
                "source.java",
                new List<string> { "meta.class" },
                99,
                FontStyle.Italic,
                "#00FF00",
                "#FFFFFF");

            // Act & Assert
            Assert.AreNotEqual(_comparer.GetHashCode(left), _comparer.GetHashCode(right));
        }

        #endregion GetHashCode tests

        #region Dictionary and HashSet integration tests

        [Test]
        public void Dictionary_FindsValueByEqualKey()
        {
            // Arrange
            var dictionary = new Dictionary<ParsedThemeRule, string>(_comparer);
            ParsedThemeRule key = CreateDefaultRule();
            ParsedThemeRule lookup = CreateDefaultRule();
            dictionary[key] = "found";

            // Act
            bool found = dictionary.TryGetValue(lookup, out string value);

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual("found", value);
        }

        [Test]
        public void HashSet_DeduplicatesEqualRules()
        {
            // Arrange
            var set = new HashSet<ParsedThemeRule>(_comparer);
            ParsedThemeRule first = CreateDefaultRule();
            ParsedThemeRule duplicate = CreateDefaultRule();

            // Act
            set.Add(first);
            set.Add(duplicate);

            // Assert
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void HashSet_RetainsDifferentRules()
        {
            // Arrange
            var set = new HashSet<ParsedThemeRule>(_comparer);
            ParsedThemeRule first = CreateDefaultRule();
            ParsedThemeRule different = new ParsedThemeRule(
                DefaultName,
                "source.java",
                new List<string> { "meta.block" },
                DefaultIndex,
                DefaultFontStyle,
                DefaultForeground,
                DefaultBackground);

            // Act
            set.Add(first);
            set.Add(different);

            // Assert
            Assert.AreEqual(2, set.Count);
        }

        #endregion Dictionary and HashSet integration tests
    }
}