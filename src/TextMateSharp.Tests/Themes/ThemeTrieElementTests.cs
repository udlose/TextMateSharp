using NUnit.Framework;
using System.Collections.Generic;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Themes
{
    [TestFixture]
    public class ThemeTrieElementTests
    {
        // Shared constants to avoid magic numbers scattered throughout tests
        private const int DefaultScopeDepth = 0;
        private const int DefaultForeground = 10;
        private const int DefaultBackground = 20;
        private const FontStyle DefaultFontStyle = FontStyle.Bold;
        private const string DefaultName = "test.rule";

        #region Constructor tests

        [Test]
        public void Constructor_SingleParam_MatchEmptyScope_ReturnsOnlyMainRule()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();

            // Act
            ThemeTrieElement element = new ThemeTrieElement(mainRule);
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert - single-param constructor creates empty rules and children
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mainRule, result[0]);
        }

        [Test]
        public void Constructor_TwoParams_MatchEmptyScope_ReturnsMainRuleAndRules()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            List<ThemeTrieElementRule> rules = new List<ThemeTrieElementRule>
            {
                new ThemeTrieElementRule("r1", 0, new List<string> { "source" }, FontStyle.Italic, 5, 6)
            };

            // Act
            ThemeTrieElement element = new ThemeTrieElement(mainRule, rules);
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert - returns mainRule + rules (same scopeDepth so order may vary)
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Constructor_ThreeParams_MatchChildScope_DescendsIntoChild()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElementRule childMainRule = new ThemeTrieElementRule(
                "child", 1, null, FontStyle.Italic, 5, 6);
            ThemeTrieElement child = new ThemeTrieElement(childMainRule);
            Dictionary<string, ThemeTrieElement> children = new Dictionary<string, ThemeTrieElement>
            {
                { "keyword", child }
            };

            // Act
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule>(), children);
            List<ThemeTrieElementRule> result = element.Match("keyword");

            // Assert - descends into child, Match("") on child returns childMainRule
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(childMainRule, result[0]);
        }

        #endregion Constructor tests

        #region Match - empty scope tests

        [Test]
        public void Match_EmptyScope_NoRulesWithParentScopes_ReturnsOnlyMainRule()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mainRule, result[0]);
        }

        [Test]
        public void Match_EmptyScope_WithRulesWithParentScopes_ReturnsAllRulesSorted()
        {
            // Arrange - mainRule at scopeDepth 0, added rule at scopeDepth 0 (same depth)
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule extraRule = new ThemeTrieElementRule(
                "r1", 0, new List<string> { "source" }, FontStyle.Italic, 5, 6);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { extraRule });

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert - both rules returned, sorted by specificity
            // extraRule has 1 parentScope, mainRule has 0, so extraRule sorts first
            Assert.AreEqual(2, result.Count);
            Assert.AreSame(extraRule, result[0]);
            Assert.AreSame(mainRule, result[1]);
        }

        [Test]
        public void Match_EmptyScope_AlwaysIncludesMainRule_RegardlessOfForeground()
        {
            // Arrange - empty scope path always includes mainRule, even if foreground == 0
            // (the foreground > 0 check only applies to the non-empty scope fallback path)
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.NotSet, 0, 0);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mainRule, result[0]);
        }

        #endregion Match - empty scope tests

        #region Match - single segment scope tests (no dot)

        [Test]
        public void Match_SingleSegment_ChildExists_DescendsIntoChild()
        {
            // Arrange
            ThemeTrieElementRule childMainRule = new ThemeTrieElementRule(
                "child", 1, null, FontStyle.Italic, 42, 99);
            ThemeTrieElement child = new ThemeTrieElement(childMainRule);
            Dictionary<string, ThemeTrieElement> children = new Dictionary<string, ThemeTrieElement>
            {
                { "keyword", child }
            };
            ThemeTrieElement element = new ThemeTrieElement(
                CreateDefaultMainRule(), new List<ThemeTrieElementRule>(), children);

            // Act
            List<ThemeTrieElementRule> result = element.Match("keyword");

            // Assert - descends into child, then matches "" on child
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(childMainRule, result[0]);
        }

        [Test]
        public void Match_SingleSegment_NoChild_ForegroundPositive_IncludesMainRule()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, DefaultFontStyle, 10, DefaultBackground);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            List<ThemeTrieElementRule> result = element.Match("keyword");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mainRule, result[0]);
        }

        [Test]
        public void Match_SingleSegment_NoChild_ForegroundZero_ExcludesMainRule()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, DefaultFontStyle, 0, DefaultBackground);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            List<ThemeTrieElementRule> result = element.Match("keyword");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Match_SingleSegment_NoChild_ForegroundZero_StillIncludesRulesWithParentScopes()
        {
            // Arrange - mainRule excluded (foreground==0), but rulesWithParentScopes still included
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, DefaultFontStyle, 0, 0);
            ThemeTrieElementRule extraRule = new ThemeTrieElementRule(
                "r1", 1, new List<string> { "source" }, FontStyle.Italic, 5, 6);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { extraRule });

            // Act
            List<ThemeTrieElementRule> result = element.Match("nomatch");

            // Assert - mainRule excluded, extraRule included
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(extraRule, result[0]);
        }

        [Test]
        public void Match_SingleSegment_NoChild_ForegroundPositive_IncludesBothMainAndRules()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, DefaultFontStyle, 10, DefaultBackground);
            ThemeTrieElementRule extraRule = new ThemeTrieElementRule(
                "r1", 1, new List<string> { "source" }, FontStyle.Italic, 5, 6);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { extraRule });

            // Act
            List<ThemeTrieElementRule> result = element.Match("nomatch");

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        #endregion Match - single segment scope tests (no dot)

        #region Match - dotted scope tests

        [Test]
        public void Match_DottedScope_ChildExistsForHead_DescendsRecursively()
        {
            // Arrange - build a trie with "keyword" -> "control" grandchild
            ThemeTrieElementRule grandchildRule = new ThemeTrieElementRule(
                "gc", 2, null, FontStyle.Underline, 77, 88);
            ThemeTrieElement grandchild = new ThemeTrieElement(grandchildRule);

            ThemeTrieElement child = new ThemeTrieElement(
                CreateDefaultMainRule().Clone(),
                new List<ThemeTrieElementRule>(),
                new Dictionary<string, ThemeTrieElement> { { "control", grandchild } });

            ThemeTrieElement root = new ThemeTrieElement(
                CreateDefaultMainRule(),
                new List<ThemeTrieElementRule>(),
                new Dictionary<string, ThemeTrieElement> { { "keyword", child } });

            // Act - "keyword.control" splits to head="keyword", tail="control"
            List<ThemeTrieElementRule> result = root.Match("keyword.control");

            // Assert - descends keyword -> control, matches "" on grandchild
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(grandchildRule, result[0]);
        }

        [Test]
        public void Match_DottedScope_NoChildForHead_FallsBackToCurrentNode()
        {
            // Arrange - no child for "keyword"
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, DefaultFontStyle, 10, DefaultBackground);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - "keyword.control" has no matching child
            List<ThemeTrieElementRule> result = element.Match("keyword.control");

            // Assert - falls back, includes mainRule (foreground > 0)
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mainRule, result[0]);
        }

        [Test]
        public void Match_DottedScope_ChildExistsForHead_TailHasNoChild_FallsBackAtChildLevel()
        {
            // Arrange - "keyword" child exists but has no "unknown" grandchild
            ThemeTrieElementRule childMainRule = new ThemeTrieElementRule(
                "child", 1, null, FontStyle.Italic, 42, 99);
            ThemeTrieElement child = new ThemeTrieElement(childMainRule);

            ThemeTrieElement root = new ThemeTrieElement(
                CreateDefaultMainRule(),
                new List<ThemeTrieElementRule>(),
                new Dictionary<string, ThemeTrieElement> { { "keyword", child } });

            // Act - "keyword.unknown" descends into child, then falls back at child level
            List<ThemeTrieElementRule> result = root.Match("keyword.unknown");

            // Assert - child's mainRule has foreground=42 > 0, so included
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(childMainRule, result[0]);
        }

        [Test]
        public void Match_MultiDottedScope_DescendsThroughMultipleLevels()
        {
            // Arrange - three levels: keyword -> control -> flow
            ThemeTrieElementRule leafRule = new ThemeTrieElementRule(
                "leaf", 3, null, FontStyle.Strikethrough, 55, 66);
            ThemeTrieElement leaf = new ThemeTrieElement(leafRule);

            ThemeTrieElement mid = new ThemeTrieElement(
                CreateDefaultMainRule().Clone(),
                new List<ThemeTrieElementRule>(),
                new Dictionary<string, ThemeTrieElement> { { "flow", leaf } });

            ThemeTrieElement root = new ThemeTrieElement(
                CreateDefaultMainRule(),
                new List<ThemeTrieElementRule>(),
                new Dictionary<string, ThemeTrieElement>
                {
                    { "keyword", new ThemeTrieElement(
                        CreateDefaultMainRule().Clone(),
                        new List<ThemeTrieElementRule>(),
                        new Dictionary<string, ThemeTrieElement> { { "control", mid } }) }
                });

            // Act
            List<ThemeTrieElementRule> result = root.Match("keyword.control.flow");

            // Assert - reached the leaf
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(leafRule, result[0]);
        }

        #endregion Match - dotted scope tests

        #region Match - sort by specificity tests

        [Test]
        public void Match_SortBySpecificity_HigherScopeDepthFirst()
        {
            // Arrange - two rules at different scopeDepths
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule deeperRule = new ThemeTrieElementRule(
                "r1", 3, new List<string> { "source" }, FontStyle.Italic, 5, 6);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { deeperRule });

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert - deeperRule (scopeDepth=3) sorts before mainRule (scopeDepth=1)
            Assert.AreEqual(2, result.Count);
            Assert.AreSame(deeperRule, result[0]);
            Assert.AreSame(mainRule, result[1]);
        }

        [Test]
        public void Match_SortBySpecificity_SameDepth_MoreParentScopesFirst()
        {
            // Arrange - same scopeDepth, different parentScopes count
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule oneParent = new ThemeTrieElementRule(
                "r1", 1, new List<string> { "source" }, FontStyle.Italic, 5, 6);
            ThemeTrieElementRule twoParents = new ThemeTrieElementRule(
                "r2", 1, new List<string> { "source", "meta" }, FontStyle.Bold, 7, 8);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { oneParent, twoParents });

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert - twoParents first, then oneParent, then mainRule (null = 0 parents)
            Assert.AreEqual(3, result.Count);
            Assert.AreSame(twoParents, result[0]);
            Assert.AreSame(oneParent, result[1]);
            Assert.AreSame(mainRule, result[2]);
        }

        [Test]
        public void Match_SortBySpecificity_SameDepthAndCount_LongerParentScopeStringFirst()
        {
            // Arrange - same scopeDepth, same parentScopes count, different string lengths
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule shortScope = new ThemeTrieElementRule(
                "r1", 1, new List<string> { "src" }, FontStyle.Italic, 5, 6);
            ThemeTrieElementRule longScope = new ThemeTrieElementRule(
                "r2", 1, new List<string> { "source.csharp" }, FontStyle.Bold, 7, 8);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { shortScope, longScope });

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert - longScope ("source.csharp" len=14) before shortScope ("src" len=3)
            Assert.AreEqual(3, result.Count);
            Assert.AreSame(longScope, result[0]);
            Assert.AreSame(shortScope, result[1]);
        }

        [Test]
        public void Match_SortBySpecificity_NullParentScopes_TreatedAsZeroLength()
        {
            // Arrange - null parentScopes treated as length 0 in comparison
            ThemeTrieElementRule ruleWithNull = new ThemeTrieElementRule(
                "r1", 1, null, FontStyle.Bold, 5, 6);
            ThemeTrieElementRule ruleWithScopes = new ThemeTrieElementRule(
                "r2", 1, new List<string> { "source" }, FontStyle.Italic, 7, 8);
            ThemeTrieElement element = new ThemeTrieElement(
                ruleWithNull, new List<ThemeTrieElementRule> { ruleWithScopes });

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert - ruleWithScopes (1 parent) before ruleWithNull (0 parents)
            Assert.AreEqual(2, result.Count);
            Assert.AreSame(ruleWithScopes, result[0]);
            Assert.AreSame(ruleWithNull, result[1]);
        }

        [Test]
        public void Match_SortBySpecificity_SingleRule_SkipsSorting()
        {
            // Arrange - SortBySpecificity short-circuits when count == 1
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mainRule, result[0]);
        }

        [Test]
        public void Match_SortBySpecificity_SameDepthCountAndLengths_StableOrder()
        {
            // Arrange - completely equal specificity: same depth, same count, same lengths
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 1, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
            ThemeTrieElementRule rule1 = new ThemeTrieElementRule(
                "r1", 1, new List<string> { "aaa" }, FontStyle.Bold, 5, 6);
            ThemeTrieElementRule rule2 = new ThemeTrieElementRule(
                "r2", 1, new List<string> { "bbb" }, FontStyle.Italic, 7, 8);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { rule1, rule2 });

            // Act - should not throw; all three have 1 parentScope of length 3
            List<ThemeTrieElementRule> result = element.Match("");

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        #endregion Match - sort by specificity tests

        #region Insert - scope routing tests

        [Test]
        public void Insert_EmptyScope_NullParentScopes_OverwritesMainRule()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                "old", 0, null, FontStyle.NotSet, 0, 0);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - empty scope + null parentScopes -> AcceptOverwrite on mainRule
            element.Insert("new", 1, "", null, FontStyle.Bold, 5, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FontStyle.Bold, result[0].fontStyle);
            Assert.AreEqual(5, result[0].foreground);
            Assert.AreEqual(6, result[0].background);
        }

        [Test]
        public void Insert_EmptyScope_WithParentScopes_AddsNewRule()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element = new ThemeTrieElement(mainRule);
            List<string> parentScopes = new List<string> { "source" };

            // Act
            element.Insert("r1", 1, "", parentScopes, FontStyle.Italic, 5, 6);

            // Assert - mainRule + 1 new rule
            List<ThemeTrieElementRule> result = element.Match("");
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Insert_EmptyScope_DuplicateParentScopes_MergesViaAcceptOverwrite()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element = new ThemeTrieElement(mainRule);
            List<string> parentScopes = new List<string> { "source" };

            // Act - first insert creates rule, second merges into it
            element.Insert("r1", 1, "", parentScopes, FontStyle.Bold, 5, 6);
            element.Insert("r2", 2, "", parentScopes, FontStyle.Italic, 7, 8);

            // Assert - still only 2 rules (mainRule + 1 merged rule), not 3
            List<ThemeTrieElementRule> result = element.Match("");
            Assert.AreEqual(2, result.Count);

            // The merged rule should have the values from the second insert
            ThemeTrieElementRule mergedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual(FontStyle.Italic, mergedRule.fontStyle);
            Assert.AreEqual(7, mergedRule.foreground);
            Assert.AreEqual(8, mergedRule.background);
        }

        [Test]
        public void Insert_EmptyScope_DifferentParentScopes_AddsSeparateRules()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - different parentScopes -> two separate rules
            element.Insert("r1", 1, "", new List<string> { "source" }, FontStyle.Bold, 5, 6);
            element.Insert("r2", 1, "", new List<string> { "meta" }, FontStyle.Italic, 7, 8);

            // Assert - mainRule + 2 separate rules
            List<ThemeTrieElementRule> result = element.Match("");
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void Insert_SingleSegment_NoExistingChild_CreatesChildWithClonedMainRule()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                "parent", 0, null, FontStyle.Bold, 10, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - creates child for "keyword" with cloned mainRule
            element.Insert("r1", 0, "keyword", null, FontStyle.Italic, 5, 6);

            // Assert - the child exists and was populated.
            // Matching "keyword.unknown" falls back to child node, returns child's mainRule
            // which is a clone of parent's mainRule, overwritten by the insert.
            List<ThemeTrieElementRule> result = element.Match("keyword.unknown");
            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
            // The child's mainRule should have foreground=5 from the overwrite
            Assert.AreEqual(5, result[0].foreground);
        }

        [Test]
        public void Insert_SingleSegment_ExistingChild_ReusesChild()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - two inserts to same head scope, second should reuse child
            element.Insert("r1", 0, "keyword", null, FontStyle.Bold, 5, 6);
            element.Insert("r2", 0, "keyword", new List<string> { "source" }, FontStyle.Italic, 7, 8);

            // Assert - child for "keyword" has 1 mainRule + 1 rulesWithParentScopes
            List<ThemeTrieElementRule> result = element.Match("keyword");
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Insert_DottedScope_CreatesNestedChildren()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.NotSet, 0, 0);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - "keyword.control" -> creates "keyword" child, then "control" grandchild
            element.Insert("r1", 0, "keyword.control", null, FontStyle.Bold, 5, 6);

            // Assert - match "keyword.control" reaches the grandchild
            List<ThemeTrieElementRule> result = element.Match("keyword.control");
            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
        }

        [Test]
        public void Insert_DottedScope_ScopeDepthIncrements()
        {
            // Arrange - Insert passes scopeDepth+1 to the child's Insert call
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.NotSet, 0, 0);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - initial scopeDepth=0 for "keyword.control.flow"
            // keyword child gets Insert with scopeDepth=1
            // control grandchild gets Insert with scopeDepth=2
            // flow great-grandchild gets Insert with scopeDepth=3
            element.Insert("r1", 0, "keyword.control.flow", null, FontStyle.Bold, 5, 6);

            // Assert - match reaches the deepest node
            List<ThemeTrieElementRule> result = element.Match("keyword.control.flow");
            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
        }

        #endregion Insert - scope routing tests

        #region Insert - inheritance tests (DoInsertHere)

        [Test]
        public void Insert_InheritsMainRuleName_WhenInsertNameIsEmpty()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                "inherited.name", 0, null, FontStyle.Bold, 10, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - empty name + non-null parentScopes triggers name inheritance
            element.Insert("", 1, "", new List<string> { "source" }, FontStyle.Bold, 5, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            Assert.AreEqual(2, result.Count);
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual("inherited.name", insertedRule.name);
        }

        [Test]
        public void Insert_InheritsMainRuleName_WhenInsertNameIsNull()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                "inherited.name", 0, null, FontStyle.Bold, 10, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            element.Insert(null, 1, "", new List<string> { "source" }, FontStyle.Bold, 5, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual("inherited.name", insertedRule.name);
        }

        [Test]
        public void Insert_DoesNotInheritName_WhenInsertNameIsNonEmpty()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                "parent.name", 0, null, FontStyle.Bold, 10, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            element.Insert("own.name", 1, "", new List<string> { "source" }, FontStyle.Bold, 5, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual("own.name", insertedRule.name);
        }

        [Test]
        public void Insert_InheritsFontStyle_WhenNotSet()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.Italic, 10, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            element.Insert("r1", 1, "", new List<string> { "source" }, FontStyle.NotSet, 5, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual(FontStyle.Italic, insertedRule.fontStyle);
        }

        [Test]
        public void Insert_DoesNotInheritFontStyle_WhenExplicitlySet()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.Italic, 10, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act - FontStyle.None is explicit (not NotSet)
            element.Insert("r1", 1, "", new List<string> { "source" }, FontStyle.None, 5, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual(FontStyle.None, insertedRule.fontStyle);
        }

        [Test]
        public void Insert_InheritsForeground_WhenZero()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.Bold, 42, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            element.Insert("r1", 1, "", new List<string> { "source" }, FontStyle.Bold, 0, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual(42, insertedRule.foreground);
        }

        [Test]
        public void Insert_DoesNotInheritForeground_WhenNonZero()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.Bold, 42, 20);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            element.Insert("r1", 1, "", new List<string> { "source" }, FontStyle.Bold, 99, 6);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual(99, insertedRule.foreground);
        }

        [Test]
        public void Insert_InheritsBackground_WhenZero()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.Bold, 10, 99);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            element.Insert("r1", 1, "", new List<string> { "source" }, FontStyle.Bold, 5, 0);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual(99, insertedRule.background);
        }

        [Test]
        public void Insert_DoesNotInheritBackground_WhenNonZero()
        {
            // Arrange
            ThemeTrieElementRule mainRule = new ThemeTrieElementRule(
                DefaultName, 0, null, FontStyle.Bold, 10, 99);
            ThemeTrieElement element = new ThemeTrieElement(mainRule);

            // Act
            element.Insert("r1", 1, "", new List<string> { "source" }, FontStyle.Bold, 5, 77);

            // Assert
            List<ThemeTrieElementRule> result = element.Match("");
            ThemeTrieElementRule insertedRule = result[0].parentScopes != null ? result[0] : result[1];
            Assert.AreEqual(77, insertedRule.background);
        }

        #endregion Insert - inheritance tests (DoInsertHere)

        #region Insert - child cloning tests

        [Test]
        public void Insert_NewChild_ClonesRulesWithParentScopes()
        {
            // Arrange - parent has rulesWithParentScopes; new child should clone them
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElementRule parentRule = new ThemeTrieElementRule(
                "pr", 1, new List<string> { "meta" }, FontStyle.Italic, 5, 6);
            ThemeTrieElement element = new ThemeTrieElement(
                mainRule, new List<ThemeTrieElementRule> { parentRule });

            // Act - insert to "keyword" creates child that clones parent's rulesWithParentScopes
            element.Insert("r1", 0, "keyword", null, FontStyle.Bold, 7, 8);

            // Assert - child's match on "" should return cloned mainRule + cloned parentRule + overwrite
            List<ThemeTrieElementRule> result = element.Match("keyword");
            Assert.IsNotNull(result);
            // Child has: cloned mainRule (overwritten by insert), cloned parentRule
            Assert.GreaterOrEqual(result.Count, 1);
        }

        #endregion Insert - child cloning tests

        #region IEquatable<ThemeTrieElement> tests

        [Test]
        public void IEquatable_Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElement element = CreateDefaultElement();

            // Act & Assert
            Assert.IsTrue(element.Equals(element));
        }

        [Test]
        public void IEquatable_Equals_Null_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElement element = CreateDefaultElement();

            // Act & Assert
            Assert.IsFalse(element.Equals((ThemeTrieElement)null));
        }

        [Test]
        public void IEquatable_Equals_SameMainRule_EmptyRulesAndChildren_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_DifferentMainRule_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElement left = new ThemeTrieElement(
                new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Bold, 1, 2));
            ThemeTrieElement right = new ThemeTrieElement(
                new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Italic, 3, 4));

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_DifferentChildrenKeys_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            right.Insert("r1", 0, "comment", null, FontStyle.Bold, 1, 2);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_NestedChildren_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword.control", null, FontStyle.Bold, 1, 2);
            right.Insert("r1", 0, "keyword.control", null, FontStyle.Bold, 1, 2);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void IEquatable_Equals_NestedChildrenDifferAtGrandchild_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword.control", null, FontStyle.Bold, 1, 2);
            right.Insert("r1", 0, "keyword.control", null, FontStyle.Italic, 3, 4);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_MultipleChildren_InsertionOrderIrrelevant()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            left.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);

            right.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);
            right.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void IEquatable_Equals_IsSymmetric()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.IsTrue(right.Equals(left));
        }

        [Test]
        public void IEquatable_Equals_IsTransitive()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement a = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement b = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement c = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(c));
            Assert.IsTrue(a.Equals(c));
        }

        [Test]
        public void IEquatable_Equals_UsedByEqualityComparerDefault()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement key1 = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement key2 = new ThemeTrieElement(mainRule.Clone());

            EqualityComparer<ThemeTrieElement> comparer = EqualityComparer<ThemeTrieElement>.Default;

            // Act & Assert
            Assert.IsTrue(comparer.Equals(key1, key2));
            Assert.AreEqual(comparer.GetHashCode(key1), comparer.GetHashCode(key2));
        }

        #endregion IEquatable<ThemeTrieElement> tests

        #region Equals(object) tests

        [Test]
        public void Equals_Object_Null_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElement element = CreateDefaultElement();

            // Act & Assert
            Assert.IsFalse(element.Equals((object)null));
        }

        [Test]
        public void Equals_Object_DifferentType_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElement element = CreateDefaultElement();

            // Act & Assert
            Assert.IsFalse(element.Equals("not an element"));
        }

        [Test]
        public void Equals_Object_StructurallyEqual_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.IsTrue(left.Equals((object)right));
        }

        #endregion Equals(object) tests

        #region Operator == and != tests

        [Test]
        public void OperatorEquals_BothNull_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElement left = null;
            ThemeTrieElement right = null;

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_LeftNull_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElement left = null;
            ThemeTrieElement right = CreateDefaultElement();

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_RightNull_ReturnsFalse()
        {
            // Arrange
            ThemeTrieElement left = CreateDefaultElement();
            ThemeTrieElement right = null;

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_SameReference_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElement element = CreateDefaultElement();

            // Act & Assert
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(element == element);
            Assert.IsFalse(element != element);
#pragma warning restore CS1718
        }

        [Test]
        public void OperatorEquals_StructurallyEqual_ReturnsTrue()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_IsSymmetric()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsTrue(right == left);
        }

        #endregion Operator == and != tests

        #region GetHashCode tests

        [Test]
        public void GetHashCode_EqualElements_ReturnSameHashCode()
        {
            // Arrange
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_IsConsistent()
        {
            // Arrange
            ThemeTrieElement element = CreateDefaultElement();

            // Act
            int hash1 = element.GetHashCode();
            int hash2 = element.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_DifferentMainRule_DifferentHash()
        {
            // Arrange
            ThemeTrieElement element1 = new ThemeTrieElement(
                new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Bold, 1, 2));
            ThemeTrieElement element2 = new ThemeTrieElement(
                new ThemeTrieElementRule(DefaultName, DefaultScopeDepth, null, FontStyle.Italic, 3, 4));

            // Act
            int hash1 = element1.GetHashCode();
            int hash2 = element2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void GetHashCode_AfterInsert_ReflectsNewState()
        {
            // Arrange
            ThemeTrieElement element = CreateDefaultElement();
            int hashBefore = element.GetHashCode();

            // Act
            element.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);

            // Assert
            Assert.AreNotEqual(hashBefore, element.GetHashCode(),
                "Hash must change after Insert - no stale cached hash");
        }

        [Test]
        public void GetHashCode_ChildrenInsertionOrderIrrelevant()
        {
            // Arrange - additive dictionary hashing is order-independent
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            left.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);

            right.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);
            right.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentRulesWithParentScopes_DifferentHash()
        {
            // Arrange - exercises RuleListGetHashCode with different rule content
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element1 = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6)
                });
            ThemeTrieElement element2 = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Italic, 7, 8)
                });

            // Act & Assert
            Assert.AreNotEqual(element1.GetHashCode(), element2.GetHashCode());
        }

        [Test]
        public void GetHashCode_EmptyVsPopulatedRulesWithParentScopes_DifferentHash()
        {
            // Arrange - exercises RuleListGetHashCode empty list vs non-empty list
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement withoutRules = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement withRules = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6)
                });

            // Act & Assert
            Assert.AreNotEqual(withoutRules.GetHashCode(), withRules.GetHashCode());
        }

        [Test]
        public void GetHashCode_SameRulesWithParentScopes_SameHash()
        {
            // Arrange - exercises RuleListGetHashCode produces same hash for equal content
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6),
                    new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 7, 8)
                });
            ThemeTrieElement right = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6),
                    new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 7, 8)
                });

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_EmptyVsPopulatedChildren_DifferentHash()
        {
            // Arrange - exercises ChildrenGetHashCode empty dict vs non-empty dict
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement withoutChildren = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement withChildren = new ThemeTrieElement(mainRule.Clone());
            withChildren.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);

            // Act & Assert
            Assert.AreNotEqual(withoutChildren.GetHashCode(), withChildren.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentChildrenContent_DifferentHash()
        {
            // Arrange - exercises ChildrenGetHashCode with different key-value pairs
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement element1 = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement element2 = new ThemeTrieElement(mainRule.Clone());

            element1.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            element2.Insert("r1", 0, "keyword", null, FontStyle.Italic, 3, 4);

            // Act & Assert
            Assert.AreNotEqual(element1.GetHashCode(), element2.GetHashCode());
        }

        [Test]
        public void GetHashCode_MultipleChildren_SameContent_SameHash()
        {
            // Arrange - exercises ChildrenGetHashCode additive accumulation with multiple entries
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            left.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);
            left.Insert("r3", 0, "string", null, FontStyle.Underline, 5, 6);

            right.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            right.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);
            right.Insert("r3", 0, "string", null, FontStyle.Underline, 5, 6);

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        #endregion GetHashCode tests

        #region RuleListEquals coverage via Equals tests

        [Test]
        public void Equals_SameMainRuleAndChildren_DifferentRulesContent_ReturnsFalse()
        {
            // Arrange - exercises RuleListEquals returning false due to element content difference.
            // Same mainRule, same (empty) children, different rulesWithParentScopes content.
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6)
                });
            ThemeTrieElement right = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Italic, 7, 8)
                });

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void Equals_SameMainRuleAndChildren_SameRulesContent_ReturnsTrue()
        {
            // Arrange - exercises RuleListEquals returning true for matching content
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6)
                });
            ThemeTrieElement right = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6)
                });

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
        }

        [Test]
        public void Equals_SameMainRule_DifferentRulesCount_ReturnsFalse()
        {
            // Arrange - exercises RuleListEquals count mismatch branch
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, null, FontStyle.Bold, 5, 6)
                });
            ThemeTrieElement right = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, null, FontStyle.Bold, 5, 6),
                    new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 7, 8)
                });

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void Equals_SameMainRule_SameRulesCount_DifferentRuleAtSecondIndex_ReturnsFalse()
        {
            // Arrange - exercises RuleListEquals element-by-element loop: first matches, second differs
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElementRule sharedRule = new ThemeTrieElementRule("shared", 1, null, FontStyle.Bold, 5, 6);
            ThemeTrieElement left = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    sharedRule.Clone(),
                    new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 7, 8)
                });
            ThemeTrieElement right = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    sharedRule.Clone(),
                    new ThemeTrieElementRule("r2", 2, null, FontStyle.Underline, 9, 10)
                });

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void Equals_BothEmptyRulesWithParentScopes_ReturnsTrue()
        {
            // Arrange - exercises RuleListEquals with two empty lists (count == 0, loop body never entered)
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone(), new List<ThemeTrieElementRule>());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone(), new List<ThemeTrieElementRule>());

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
        }

        [Test]
        public void Equals_MultipleRulesAllMatch_ReturnsTrue()
        {
            // Arrange - exercises full RuleListEquals loop with multiple matching elements
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6),
                    new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 7, 8),
                    new ThemeTrieElementRule("r3", 3, new List<string> { "meta", "class" }, FontStyle.Underline, 9, 10)
                });
            ThemeTrieElement right = new ThemeTrieElement(
                mainRule.Clone(),
                new List<ThemeTrieElementRule>
                {
                    new ThemeTrieElementRule("r1", 1, new List<string> { "source" }, FontStyle.Bold, 5, 6),
                    new ThemeTrieElementRule("r2", 2, null, FontStyle.Italic, 7, 8),
                    new ThemeTrieElementRule("r3", 3, new List<string> { "meta", "class" }, FontStyle.Underline, 9, 10)
                });

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        #endregion RuleListEquals coverage via Equals tests

        #region ChildrenEquals coverage via Equals tests

        [Test]
        public void Equals_BothEmptyChildren_ReturnsTrue()
        {
            // Arrange - exercises ChildrenEquals with two empty dictionaries (count == 0, foreach body never entered)
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
        }

        [Test]
        public void Equals_DifferentChildrenCount_ReturnsFalse()
        {
            // Arrange - exercises ChildrenEquals count mismatch branch
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);

            right.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            right.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void Equals_SameChildrenCount_KeyNotFound_ReturnsFalse()
        {
            // Arrange - exercises ChildrenEquals TryGetValue returning false (key not found).
            // Same count (1 each) but different keys.
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            right.Insert("r1", 0, "comment", null, FontStyle.Bold, 1, 2);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void Equals_SameChildrenKeys_DifferentValues_ReturnsFalse()
        {
            // Arrange - exercises ChildrenEquals value inequality branch (kvp.Value != bValue)
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            right.Insert("r1", 0, "keyword", null, FontStyle.Italic, 3, 4);

            // Act & Assert
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void Equals_MultipleChildrenAllMatch_ReturnsTrue()
        {
            // Arrange - exercises ChildrenEquals full foreach loop with multiple matching entries
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            left.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);
            left.Insert("r3", 0, "string", null, FontStyle.Underline, 5, 6);

            right.Insert("r1", 0, "keyword", null, FontStyle.Bold, 1, 2);
            right.Insert("r2", 0, "comment", null, FontStyle.Italic, 3, 4);
            right.Insert("r3", 0, "string", null, FontStyle.Underline, 5, 6);

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void Equals_SameKeys_ValuesRecursivelyDifferAtDepthTwo_ReturnsFalse()
        {
            // Arrange - exercises recursive ChildrenEquals through operator !=.
            // Direct children share the same key "keyword", but at depth 2
            // the grandchild values differ.
            ThemeTrieElementRule mainRule = CreateDefaultMainRule();
            ThemeTrieElement left = new ThemeTrieElement(mainRule.Clone());
            ThemeTrieElement right = new ThemeTrieElement(mainRule.Clone());

            left.Insert("r1", 0, "keyword.control", null, FontStyle.Bold, 1, 2);
            left.Insert("r2", 0, "keyword.other", null, FontStyle.Italic, 3, 4);

            right.Insert("r1", 0, "keyword.control", null, FontStyle.Bold, 1, 2);
            right.Insert("r2", 0, "keyword.other", null, FontStyle.Underline, 5, 6);

            // Act & Assert - "keyword" children exist on both sides, but "keyword.other" grandchildren differ
            Assert.IsFalse(left.Equals(right));
        }

        #endregion ChildrenEquals coverage via Equals tests

        #region Helpers

        private static ThemeTrieElementRule CreateDefaultMainRule()
        {
            return new ThemeTrieElementRule(
                DefaultName, DefaultScopeDepth, null, DefaultFontStyle, DefaultForeground, DefaultBackground);
        }

        private static ThemeTrieElement CreateDefaultElement()
        {
            return new ThemeTrieElement(CreateDefaultMainRule());
        }

        #endregion Helpers
    }
}