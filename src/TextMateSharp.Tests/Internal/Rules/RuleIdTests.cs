using NUnit.Framework;
using System;
using System.Collections.Generic;
using TextMateSharp.Internal.Rules;

namespace TextMateSharp.Tests.Internal.Rules
{
    [TestFixture]
    public class RuleIdTests
    {
        #region Factory and sentinel tests

        [Test]
        public void Of_ValidId_CreatesRuleId()
        {
            // Arrange & Act
            RuleId ruleId = RuleId.Of(42);

            // Assert
            Assert.AreEqual(42, ruleId.Id);
        }

        [Test]
        public void Of_Zero_CreatesRuleId()
        {
            // Arrange & Act
            RuleId ruleId = RuleId.Of(0);

            // Assert
            Assert.AreEqual(0, ruleId.Id);
        }

        [Test]
        public void Of_NegativeId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => RuleId.Of(-1));
        }

        [Test]
        public void NO_RULE_HasIdZero()
        {
            Assert.AreEqual(0, RuleId.NO_RULE.Id);
        }

        [Test]
        public void END_RULE_HasIdNegativeOne()
        {
            Assert.AreEqual(-1, RuleId.END_RULE.Id);
        }

        [Test]
        public void WHILE_RULE_HasIdNegativeTwo()
        {
            Assert.AreEqual(-2, RuleId.WHILE_RULE.Id);
        }

        #endregion Factory and sentinel tests

        #region Equals (object) tests

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            RuleId ruleId = RuleId.Of(1);

            // Act
            bool result = ruleId.Equals((object)ruleId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_SameId_DifferentInstances_ReturnsTrue()
        {
            // Arrange - RuleId.Of creates new instances each time
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentId_ReturnsFalse()
        {
            // Arrange
            RuleId left = RuleId.Of(1);
            RuleId right = RuleId.Of(2);

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            RuleId ruleId = RuleId.Of(1);

            // Act
            bool result = ruleId.Equals((object)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            RuleId ruleId = RuleId.Of(1);

            // Act
            bool result = ruleId.Equals(1);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_IsSymmetric()
        {
            // Arrange
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            // Act & Assert
            Assert.IsTrue(left.Equals((object)right));
            Assert.IsTrue(right.Equals((object)left));
        }

        [Test]
        public void Equals_SentinelConstants_EqualToSameId()
        {
            // Arrange - NO_RULE has Id=0, so Of(0) should be equal
            RuleId fromFactory = RuleId.Of(0);

            // Act
            bool result = RuleId.NO_RULE.Equals((object)fromFactory);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion Equals (object) tests

        #region IEquatable<RuleId> tests

        [Test]
        public void IEquatable_Equals_IsReflexive()
        {
            // Arrange
            RuleId ruleId = RuleId.Of(1);

            // Act
            bool result = ruleId.Equals(ruleId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_IsSymmetric()
        {
            // Arrange
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.IsTrue(right.Equals(left));
        }

        [Test]
        public void IEquatable_Equals_IsTransitive()
        {
            // Arrange
            RuleId a = RuleId.Of(42);
            RuleId b = RuleId.Of(42);
            RuleId c = RuleId.Of(42);

            // Act & Assert
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(c));
            Assert.IsTrue(a.Equals(c));
        }

        [Test]
        public void IEquatable_Equals_Null_ReturnsFalse()
        {
            // Arrange
            RuleId ruleId = RuleId.Of(1);

            // Act
            bool result = ruleId.Equals((RuleId)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IEquatable_Equals_SameId_ReturnsTrue()
        {
            // Arrange
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_DifferentId_ReturnsFalse()
        {
            // Arrange
            RuleId left = RuleId.Of(1);
            RuleId right = RuleId.Of(2);

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IEquatable_Equals_UsedByEqualityComparerDefault()
        {
            // Arrange
            RuleId key1 = RuleId.Of(42);
            RuleId key2 = RuleId.Of(42);

            EqualityComparer<RuleId> comparer = EqualityComparer<RuleId>.Default;

            // Act & Assert
            Assert.IsTrue(comparer.Equals(key1, key2));
            Assert.AreEqual(comparer.GetHashCode(key1), comparer.GetHashCode(key2));
        }

        #endregion IEquatable<RuleId> tests

        #region Operator == and != tests

        [Test]
        public void OperatorEquals_SameInstance_ReturnsTrue()
        {
            // Arrange
            RuleId ruleId = RuleId.Of(1);

            // Act & Assert
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(ruleId == ruleId);
            Assert.IsFalse(ruleId != ruleId);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Test]
        public void OperatorEquals_SameId_DifferentInstances_ReturnsTrue()
        {
            // Arrange - key behavioral change: == now uses value equality, not reference equality
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_DifferentId_ReturnsFalse()
        {
            // Arrange
            RuleId left = RuleId.Of(1);
            RuleId right = RuleId.Of(2);

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_BothNull_ReturnsTrue()
        {
            // Arrange
            RuleId left = null;
            RuleId right = null;

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_LeftNull_ReturnsFalse()
        {
            // Arrange
            RuleId left = null;
            RuleId right = RuleId.Of(1);

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_RightNull_ReturnsFalse()
        {
            // Arrange
            RuleId left = RuleId.Of(1);
            RuleId right = null;

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_SentinelNO_RULE_EqualToOfZero()
        {
            // Arrange - NO_RULE has Id=0
            RuleId fromFactory = RuleId.Of(0);

            // Act & Assert
            Assert.IsTrue(RuleId.NO_RULE == fromFactory);
            Assert.IsFalse(RuleId.NO_RULE != fromFactory);
        }

        [Test]
        public void OperatorNotEquals_SentinelConstants_AreDistinct()
        {
            // Act & Assert - END_RULE (-1) != WHILE_RULE (-2) != NO_RULE (0)
            Assert.IsTrue(RuleId.END_RULE != RuleId.WHILE_RULE);
            Assert.IsTrue(RuleId.END_RULE != RuleId.NO_RULE);
            Assert.IsTrue(RuleId.WHILE_RULE != RuleId.NO_RULE);
        }

        #endregion Operator == and != tests

        #region NotEquals (legacy) tests

        [Test]
        public void NotEquals_SameId_ReturnsFalse()
        {
            // Arrange
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            // Act
            bool result = left.NotEquals(right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void NotEquals_DifferentId_ReturnsTrue()
        {
            // Arrange
            RuleId left = RuleId.Of(1);
            RuleId right = RuleId.Of(2);

            // Act
            bool result = left.NotEquals(right);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion NotEquals (legacy) tests

        #region GetHashCode tests

        [Test]
        public void GetHashCode_EqualInstances_ReturnSameValue()
        {
            // Arrange
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_ReturnsId()
        {
            // Arrange - for a single-field identity type, the Id itself is a perfect hash
            RuleId ruleId = RuleId.Of(42);

            // Act & Assert
            Assert.AreEqual(42, ruleId.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentIds_ReturnDifferentValues()
        {
            // Arrange
            RuleId left = RuleId.Of(1);
            RuleId right = RuleId.Of(2);

            // Act & Assert
            Assert.AreNotEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_SentinelNO_RULE_MatchesOfZero()
        {
            // Arrange
            RuleId fromFactory = RuleId.Of(0);

            // Act & Assert
            Assert.AreEqual(RuleId.NO_RULE.GetHashCode(), fromFactory.GetHashCode());
        }

        #endregion GetHashCode tests

        #region HashCode / Equals contract tests

        [Test]
        public void HashCodeEqualsContract_EqualObjects_SameHashCode()
        {
            // If two objects are equal, they must have the same hash code
            RuleId left = RuleId.Of(42);
            RuleId right = RuleId.Of(42);

            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void HashCodeEqualsContract_WorksWithDictionary()
        {
            // Arrange
            RuleId key1 = RuleId.Of(42);
            RuleId key2 = RuleId.Of(42);

            Dictionary<RuleId, string> dictionary = new Dictionary<RuleId, string>
            {
                [key1] = "found"
            };

            // Act & Assert - key2 is structurally equal to key1
            Assert.IsTrue(dictionary.ContainsKey(key2));
            Assert.AreEqual("found", dictionary[key2]);
        }

        [Test]
        public void HashCodeEqualsContract_WorksWithHashSet()
        {
            // Arrange
            RuleId item1 = RuleId.Of(42);
            RuleId item2 = RuleId.Of(42);

            HashSet<RuleId> hashSet = new HashSet<RuleId>();
            hashSet.Add(item1);

            // Act & Assert - item2 is structurally equal to item1
            Assert.IsTrue(hashSet.Contains(item2));
            Assert.IsFalse(hashSet.Add(item2));
        }

        #endregion HashCode / Equals contract tests

        #region ToString tests

        [Test]
        public void ToString_ReturnsIdAsString()
        {
            // Arrange
            RuleId ruleId = RuleId.Of(42);

            // Act & Assert
            Assert.AreEqual("42", ruleId.ToString());
        }

        #endregion ToString tests
    }
}