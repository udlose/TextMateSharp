using NUnit.Framework;
using System.Collections.Generic;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Rules;
using TextMateSharp.Model;

namespace TextMateSharp.Tests.Model
{
    [TestFixture]
    public class TMStateTests
    {
        // Shared RuleId instances - RuleId.Of() creates new instances;
        // tests must share a single RuleId instance to match production behavior
        private static readonly RuleId _testRuleId = RuleId.Of(1);
        private static readonly RuleId _testRuleIdAlt = RuleId.Of(2);

        #region Constructor tests

        [Test]
        public void Constructor_BothNull_CreatesInstance()
        {
            // Arrange & Act
            TMState state = new TMState(null, null);

            // Assert
            Assert.IsNull(state.GetRuleStack());
        }

        [Test]
        public void Constructor_WithRuleStack_StoresRuleStack()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();

            // Act
            TMState state = new TMState(null, ruleStack);

            // Assert
            Assert.AreSame(ruleStack, state.GetRuleStack());
        }

        #endregion Constructor tests

        #region SetRuleStack / GetRuleStack tests

        [Test]
        public void SetRuleStack_UpdatesRuleStack()
        {
            // Arrange
            StateStack original = CreateRuleStack();
            StateStack replacement = CreateRuleStackAlt();
            TMState state = new TMState(null, original);

            // Act
            state.SetRuleStack(replacement);

            // Assert
            Assert.AreSame(replacement, state.GetRuleStack());
        }

        [Test]
        public void SetRuleStack_Null_SetsToNull()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act
            state.SetRuleStack(null);

            // Assert
            Assert.IsNull(state.GetRuleStack());
        }

        #endregion SetRuleStack / GetRuleStack tests

        #region Clone tests

        [Test]
        public void Clone_ReturnsNewInstance()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act
            TMState cloned = state.Clone();

            // Assert
            Assert.AreNotSame(state, cloned);
        }

        [Test]
        public void Clone_SharesRuleStack()
        {
            // Arrange - Clone shares the ruleStack reference (shallow copy), matching upstream
            StateStack ruleStack = CreateRuleStack();
            TMState state = new TMState(null, ruleStack);

            // Act
            TMState cloned = state.Clone();

            // Assert
            Assert.AreSame(ruleStack, cloned.GetRuleStack());
        }

        [Test]
        public void Clone_ClonesParentEmbedderState()
        {
            // Arrange
            TMState parent = new TMState(null, CreateRuleStack());
            TMState child = new TMState(parent, CreateRuleStack());

            // Act
            TMState cloned = child.Clone();

            // Assert - parent is cloned (deep copy), not shared
            Assert.IsTrue(cloned.Equals(child));
        }

        [Test]
        public void Clone_NullParent_ReturnsStateWithNullParent()
        {
            // Arrange
            TMState state = new TMState(null, null);

            // Act
            TMState cloned = state.Clone();

            // Assert
            Assert.IsTrue(cloned.Equals(state));
        }

        #endregion Clone tests

        #region Equals (object) tests

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act
            bool result = state.Equals((object)state);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_EquivalentObjects_ReturnsTrue()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState left = new TMState(null, ruleStack);
            TMState right = new TMState(null, ruleStack);

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act
            bool result = state.Equals((object)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act
            bool result = state.Equals(42);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentRuleStack_ReturnsFalse()
        {
            // Arrange
            TMState left = new TMState(null, CreateRuleStack());
            TMState right = new TMState(null, CreateRuleStackAlt());

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_IsSymmetric()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState left = new TMState(null, ruleStack);
            TMState right = new TMState(null, ruleStack);

            // Act
            bool leftEqualsRight = left.Equals((object)right);
            bool rightEqualsLeft = right.Equals((object)left);

            // Assert
            Assert.IsTrue(leftEqualsRight);
            Assert.IsTrue(rightEqualsLeft);
        }

        [Test]
        public void Equals_BothNullRuleStacks_ReturnsTrue()
        {
            // Arrange - matches Tokenizer.GetInitialState() which passes (null, null)
            TMState left = new TMState(null, null);
            TMState right = new TMState(null, null);

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentParentEmbedderState_ReturnsFalse()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState parentA = new TMState(null, CreateRuleStack());
            TMState parentB = new TMState(null, CreateRuleStackAlt());
            TMState left = new TMState(parentA, ruleStack);
            TMState right = new TMState(parentB, ruleStack);

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_OneNullParentOneNonNull_ReturnsFalse()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState parent = new TMState(null, CreateRuleStack());
            TMState left = new TMState(parent, ruleStack);
            TMState right = new TMState(null, ruleStack);

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion Equals (object) tests

        #region IEquatable<TMState> tests

        [Test]
        public void IEquatable_Equals_IsReflexive()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act
#pragma warning disable CS1718 // Comparison made to same variable
            bool result = state.Equals(state);
#pragma warning restore CS1718 // Comparison made to same variable

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_IsSymmetric()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState left = new TMState(null, ruleStack);
            TMState right = new TMState(null, ruleStack);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.IsTrue(right.Equals(left));
        }

        [Test]
        public void IEquatable_Equals_IsTransitive()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState a = new TMState(null, ruleStack);
            TMState b = new TMState(null, ruleStack);
            TMState c = new TMState(null, ruleStack);

            // Act & Assert
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(c));
            Assert.IsTrue(a.Equals(c));
        }

        [Test]
        public void IEquatable_Equals_Null_ReturnsFalse()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act
            bool result = state.Equals((TMState)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IEquatable_Equals_StructurallyEqual_ReturnsTrue()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState left = new TMState(null, ruleStack);
            TMState right = new TMState(null, ruleStack);

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_DifferentStack_ReturnsFalse()
        {
            // Arrange
            TMState left = new TMState(null, CreateRuleStack());
            TMState right = new TMState(null, CreateRuleStackAlt());

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IEquatable_Equals_WithEqualParents_ReturnsTrue()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState parentA = new TMState(null, ruleStack);
            TMState parentB = new TMState(null, ruleStack);
            TMState left = new TMState(parentA, ruleStack);
            TMState right = new TMState(parentB, ruleStack);

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_UsedByEqualityComparerDefault()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState key1 = new TMState(null, ruleStack);
            TMState key2 = new TMState(null, ruleStack);

            EqualityComparer<TMState> comparer = EqualityComparer<TMState>.Default;

            // Act & Assert
            Assert.IsTrue(comparer.Equals(key1, key2));
            Assert.AreEqual(comparer.GetHashCode(key1), comparer.GetHashCode(key2));
        }

        #endregion IEquatable<TMState> tests

        #region Operator == and != tests

        [Test]
        public void OperatorEquals_SameInstance_ReturnsTrue()
        {
            // Arrange
            TMState state = new TMState(null, CreateRuleStack());

            // Act & Assert
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(state == state);
            Assert.IsFalse(state != state);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Test]
        public void OperatorEquals_StructurallyEqual_ReturnsTrue()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState left = new TMState(null, ruleStack);
            TMState right = new TMState(null, ruleStack);

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_DifferentStacks_ReturnsFalse()
        {
            // Arrange
            TMState left = new TMState(null, CreateRuleStack());
            TMState right = new TMState(null, CreateRuleStackAlt());

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_BothNull_ReturnsTrue()
        {
            // Arrange
            TMState left = null;
            TMState right = null;

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_LeftNull_ReturnsFalse()
        {
            // Arrange
            TMState left = null;
            TMState right = new TMState(null, CreateRuleStack());

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_RightNull_ReturnsFalse()
        {
            // Arrange
            TMState left = new TMState(null, CreateRuleStack());
            TMState right = null;

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        #endregion Operator == and != tests

        #region GetHashCode tests

        [Test]
        public void GetHashCode_EqualInstances_ReturnSameValue()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState left = new TMState(null, ruleStack);
            TMState right = new TMState(null, ruleStack);

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_BothNullFields_DoesNotThrow()
        {
            // Arrange - matches Tokenizer.GetInitialState() which passes (null, null)
            TMState state = new TMState(null, null);

            // Act & Assert - should not throw NullReferenceException
            Assert.DoesNotThrow(() => state.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentRuleStacks_LikelyDifferent()
        {
            // Arrange
            TMState left = new TMState(null, CreateRuleStack());
            TMState right = new TMState(null, CreateRuleStackAlt());

            // Act & Assert - hash collision is possible but unlikely for distinct inputs
            Assert.AreNotEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_WithParent_IncludesParentInHash()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState withoutParent = new TMState(null, ruleStack);
            TMState parent = new TMState(null, CreateRuleStackAlt());
            TMState withParent = new TMState(parent, ruleStack);

            // Act & Assert - different parent states should produce different hashes
            Assert.AreNotEqual(withoutParent.GetHashCode(), withParent.GetHashCode());
        }

        [Test]
        public void GetHashCode_EqualParents_ReturnSameValue()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState parentA = new TMState(null, ruleStack);
            TMState parentB = new TMState(null, ruleStack);
            TMState left = new TMState(parentA, ruleStack);
            TMState right = new TMState(parentB, ruleStack);

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_AfterSetRuleStack_ReflectsMutation()
        {
            // Arrange - hash is computed on the fly, so mutation is reflected
            TMState state = new TMState(null, CreateRuleStack());
            int hashBefore = state.GetHashCode();

            // Act
            state.SetRuleStack(CreateRuleStackAlt());
            int hashAfter = state.GetHashCode();

            // Assert - different rule stacks should produce different hashes
            Assert.AreNotEqual(hashBefore, hashAfter);
        }

        #endregion GetHashCode tests

        #region HashCode / Equals contract tests

        [Test]
        public void HashCodeEqualsContract_EqualObjects_SameHashCode()
        {
            // If two objects are equal, they must have the same hash code
            StateStack ruleStack = CreateRuleStack();
            TMState left = new TMState(null, ruleStack);
            TMState right = new TMState(null, ruleStack);

            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void HashCodeEqualsContract_WorksWithDictionary()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState key1 = new TMState(null, ruleStack);
            TMState key2 = new TMState(null, ruleStack);
            Dictionary<TMState, int> dictionary = new Dictionary<TMState, int>
            {
                [key1] = 42
            };

            // Act & Assert - key2 is structurally equal to key1
            Assert.IsTrue(dictionary.ContainsKey(key2));
            Assert.AreEqual(42, dictionary[key2]);
        }

        [Test]
        public void HashCodeEqualsContract_WorksWithHashSet()
        {
            // Arrange
            StateStack ruleStack = CreateRuleStack();
            TMState item1 = new TMState(null, ruleStack);
            TMState item2 = new TMState(null, ruleStack);

            HashSet<TMState> hashSet = new HashSet<TMState>();
            hashSet.Add(item1);

            // Act & Assert - item2 is structurally equal to item1
            Assert.IsTrue(hashSet.Contains(item2));
            Assert.IsFalse(hashSet.Add(item2));
        }

        #endregion HashCode / Equals contract tests

        #region Helper methods

        private static StateStack CreateRuleStack()
        {
            return new StateStack(
                null,
                _testRuleId,
                0,
                0,
                false,
                null,
                null,
                null);
        }

        private static StateStack CreateRuleStackAlt()
        {
            return new StateStack(
                null,
                _testRuleIdAlt,
                0,
                0,
                false,
                null,
                null,
                null);
        }

        #endregion Helper methods
    }
}