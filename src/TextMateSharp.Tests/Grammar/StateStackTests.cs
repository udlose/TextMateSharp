using NUnit.Framework;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars;
using TextMateSharp.Internal.Rules;

namespace TextMateSharp.Tests.Grammar
{
    [TestFixture]
    public class StateStackTests
    {
        private const int RuleIdSingleDepth = 42;
        private const int RuleIdDepthTwo = 100;
        private const int RuleIdDepthThree = 200;
        private const int EnterPosition = 0;
        private const int AnchorPosition = 0;

        [Test]
        public void ToString_SingleDepthState_ReturnsFormattedString()
        {
            // Arrange
            StateStack stack = new StateStack(
                null,
                RuleId.Of(RuleIdSingleDepth),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            const string expectedOutput = "[(42)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_TwoDepthState_ReturnsFormattedStringWithBothRules()
        {
            // Arrange
            StateStack parent = new StateStack(
                null,
                RuleId.Of(RuleIdSingleDepth),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            StateStack stack = parent.Push(
                RuleId.Of(RuleIdDepthTwo),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            const string expectedOutput = "[(42), (100)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_ThreeDepthState_ReturnsFormattedStringWithAllRules()
        {
            // Arrange
            StateStack level1 = new StateStack(
                null,
                RuleId.Of(RuleIdSingleDepth),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            StateStack level2 = level1.Push(
                RuleId.Of(RuleIdDepthTwo),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            StateStack level3 = level2.Push(
                RuleId.Of(RuleIdDepthThree),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            const string expectedOutput = "[(42), (100), (200)]";

            // Act
            string result = level3.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_NullStaticInstance_ReturnsFormattedNoRuleString()
        {
            // Arrange
            StateStack stack = StateStack.NULL;
            const string expectedOutput = "[(0)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_StateWithNoRuleId_ReturnsFormattedNoRuleString()
        {
            // Arrange
            StateStack stack = new StateStack(
                null,
                RuleId.NO_RULE,
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            const string expectedOutput = "[(0)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_StateWithEndRuleId_ReturnsFormattedEndRuleString()
        {
            // Arrange
            StateStack stack = new StateStack(
                null,
                RuleId.END_RULE,
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            const string expectedOutput = "[(-1)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_StateWithWhileRuleId_ReturnsFormattedWhileRuleString()
        {
            // Arrange
            StateStack stack = new StateStack(
                null,
                RuleId.WHILE_RULE,
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            const string expectedOutput = "[(-2)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_BoundaryDepthZero_ReturnsNullStackString()
        {
            // Arrange - depth 0 returns StateStack.NULL
            const int depthZero = 0;
            StateStack stack = CreateStateStackWithDepth(depthZero);
            const string expectedOutput = "[(0)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
            Assert.AreSame(StateStack.NULL, stack);
        }

        [Test]
        public void ToString_BoundaryDepthOne_ReturnsSinglePushString()
        {
            // Arrange - depth 1 is one push on NULL
            const int depthOne = 1;
            StateStack stack = CreateStateStackWithDepth(depthOne);
            const string expectedOutput = "[(0), (0)]";

            // Act
            string result = stack.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ToString_BoundaryVeryLargeDepth_ReturnsFormattedStringWithAllLevels()
        {
            // Arrange - test large depth to verify performance and correctness
            const int veryLargeDepth = 100;
            StateStack stack = CreateStateStackWithDepth(veryLargeDepth);

            // Act
            string result = stack.ToString();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("[(0)"));
            Assert.IsTrue(result.EndsWith("(9900)]"));

            // Verify correct number of elements: NULL (1) + 100 pushes = 101 elements
            const int expectedCommaCount = 100;
            int actualCommaCount = result.Split(',').Length - 1;
            Assert.AreEqual(expectedCommaCount, actualCommaCount);
        }

        [Test]
        public void ToString_CalledMultipleTimes_ReturnsSameResult()
        {
            // Arrange
            StateStack stack = new StateStack(
                null,
                RuleId.Of(RuleIdSingleDepth),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            // Act
            string result1 = stack.ToString();
            string result2 = stack.ToString();
            string result3 = stack.ToString();

            // Assert
            Assert.AreEqual(result1, result2);
            Assert.AreEqual(result2, result3);
        }

        [Test]
        public void ToString_StackWithMixedRuleIds_ReturnsCorrectOrderFromRootToCurrent()
        {
            // Arrange
            StateStack root = new StateStack(
                null,
                RuleId.Of(RuleIdSingleDepth),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            StateStack middle = root.Push(
                RuleId.Of(RuleIdDepthTwo),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            StateStack current = middle.Push(
                RuleId.Of(RuleIdDepthThree),
                EnterPosition,
                AnchorPosition,
                false,
                null,
                CreateTestScopeStack(),
                CreateTestScopeStack());

            const string expectedOutput = "[(42), (100), (200)]";

            // Act
            string result = current.ToString();

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        #region Helper Methods

        private static AttributedScopeStack CreateTestScopeStack()
        {
            return new AttributedScopeStack(null, "test.scope", 0);
        }

        private static StateStack CreateStateStackWithDepth(int depth)
        {
            StateStack stack = StateStack.NULL;
            const int ruleIdDepthMultiplier = 100;
            for (int depthIndex = 0; depthIndex < depth; depthIndex++)
            {
                int ruleId = depthIndex * ruleIdDepthMultiplier;
                stack = stack.Push(
                    RuleId.Of(ruleId),
                    EnterPosition,
                    AnchorPosition,
                    false,
                    null,
                    CreateTestScopeStack(),
                    CreateTestScopeStack());
            }

            return stack;
        }

        #endregion
    }
}