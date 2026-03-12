using System;
using System.Collections.Generic;

using NUnit.Framework;

using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Themes
{
    [TestFixture]
    public class ThemeTrieElementRuleEqualityComparerTests
    {
        private const int DefaultScopeDepth = 1;
        private const int DefaultForeground = 10;
        private const int DefaultBackground = 20;
        private const FontStyle DefaultFontStyle = FontStyle.Bold;
        private const string DefaultName = "test.rule";

        private ThemeTrieElementRuleEqualityComparer _comparer;

        [SetUp]
        public void SetUp()
        {
            _comparer = ThemeTrieElementRuleEqualityComparer.Default;
        }

        #region Default instance tests

        [Test]
        public void Default_ReturnsSingletonInstance()
        {
            // Arrange & Act
            ThemeTrieElementRuleEqualityComparer instance1 = ThemeTrieElementRuleEqualityComparer.Default;
            ThemeTrieElementRuleEqualityComparer instance2 = ThemeTrieElementRuleEqualityComparer.Default;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void Default_IsNotNull()
        {
            // Act & Assert
            Assert.IsNotNull(ThemeTrieElementRuleEqualityComparer.Default);
        }

        #endregion Default instance tests

        #region Equals tests — reference and null

        [Test]
        public void Equals_BothNull_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule left = null;
            ThemeTrieElementRule right = null;

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_LeftNull_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = null;
            ThemeTrieElementRule right = CreateDefaultRule();

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_RightNull_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = CreateDefaultRule();
            ThemeTrieElementRule right = null;

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(rule, rule));
        }

        #endregion Equals tests — reference and null

        #region Equals tests — field-by-field comparison

        [Test]
        public void Equals_AllFieldsMatch_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentScopeDepth_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, 5, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentFontStyle_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Bold, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Italic, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentForeground_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 10, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, 99, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentBackground_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 20);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, 99);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_NameNotPartOfEquality()
        {
            // Arrange — name differs but all equality-relevant fields match
            ThemeTrieElementRule left = new ThemeTrieElementRule("rule.one", DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule("rule.two", DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right), "Name is not part of equality comparison");
        }

        #endregion Equals tests — field-by-field comparison

        #region Equals tests — parentScopes deep comparison

        [Test]
        public void Equals_BothNullParentScopes_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_LeftNullParentScopes_RightNonNull_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentParentScopesCount_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source", "meta" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_DifferentParentScopesContent_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.js" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_SameParentScopesReference_ReturnsTrue()
        {
            // Arrange — shared list instance (common in Clone scenarios)
            List<string> shared = new List<string> { "source.cs", "meta.class" };
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, shared, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, shared, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
        }

        [Test]
        public void Equals_ParentScopesComparedOrdinal_CaseSensitive()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "Source.CS" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source.cs" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsFalse(_comparer.Equals(left, right));
        }

        #endregion Equals tests — parentScopes deep comparison

        #region Equals tests — symmetry and transitivity

        [Test]
        public void Equals_IsSymmetric()
        {
            // Arrange
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
            Assert.IsTrue(_comparer.Equals(right, left));
        }

        [Test]
        public void Equals_IsTransitive()
        {
            // Arrange
            ThemeTrieElementRule a = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule b = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule c = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(a, b));
            Assert.IsTrue(_comparer.Equals(b, c));
            Assert.IsTrue(_comparer.Equals(a, c));
        }

        #endregion Equals tests — symmetry and transitivity

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
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.AreEqual(_comparer.GetHashCode(left), _comparer.GetHashCode(right));
        }

        [Test]
        public void GetHashCode_ConsistentWithEquals()
        {
            // Arrange — if Equals returns true, hashes must match
            ThemeTrieElementRule left = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source", "meta" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule right = new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, new List<string> { "source", "meta" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act & Assert
            Assert.IsTrue(_comparer.Equals(left, right));
            Assert.AreEqual(_comparer.GetHashCode(left), _comparer.GetHashCode(right));
        }

        [Test]
        public void GetHashCode_ConsistentWithObjectGetHashCode()
        {
            // Arrange — comparer delegates to obj.GetHashCode(), so both should agree
            ThemeTrieElementRule rule = CreateDefaultRule();

            // Act & Assert
            Assert.AreEqual(rule.GetHashCode(), _comparer.GetHashCode(rule));
        }

        [Test]
        public void GetHashCode_DifferentRules_DifferentHash()
        {
            // Arrange
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, 1, null, FontStyle.Bold, 10, 20);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, 2, null, FontStyle.Italic, 30, 40);

            // Act & Assert
            Assert.AreNotEqual(_comparer.GetHashCode(rule1), _comparer.GetHashCode(rule2));
        }

        #endregion GetHashCode tests

        #region Dictionary and HashSet integration tests

        [Test]
        public void Comparer_WorksAsDictionaryKeyComparer()
        {
            // Arrange — verifies the comparer works with Dictionary<TKey, TValue>
            Dictionary<ThemeTrieElementRule, string> dict =
                new Dictionary<ThemeTrieElementRule, string>(_comparer);

            ThemeTrieElementRule key1 = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule key2 = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            dict[key1] = "value1";

            // Assert — key2 is a different instance but structurally equal, so lookup succeeds
            Assert.IsTrue(dict.ContainsKey(key2));
            Assert.AreEqual("value1", dict[key2]);
        }

        [Test]
        public void Comparer_WorksAsHashSetComparer()
        {
            // Arrange — verifies the comparer works with HashSet<T>
            HashSet<ThemeTrieElementRule> set =
                new HashSet<ThemeTrieElementRule>(_comparer);

            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, new List<string> { "source" }, DefaultFontStyle, DefaultForeground, DefaultBackground);

            // Act
            set.Add(rule1);

            // Assert — rule2 is structurally equal, so Add returns false and count stays 1
            Assert.IsFalse(set.Add(rule2));
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void Comparer_DifferentRulesAreDistinctInHashSet()
        {
            // Arrange
            HashSet<ThemeTrieElementRule> set =
                new HashSet<ThemeTrieElementRule>(_comparer);

            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(DefaultName, 1, null, FontStyle.Bold, 10, 20);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(DefaultName, 2, null, FontStyle.Italic, 30, 40);

            // Act
            set.Add(rule1);
            set.Add(rule2);

            // Assert
            Assert.AreEqual(2, set.Count);
        }

        #endregion Dictionary and HashSet integration tests

        #region Helpers

        private static ThemeTrieElementRule CreateDefaultRule()
        {
            return new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
        }

        #endregion Helpers
    }
}