using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Tests.Resources;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Internal.Grammars
{
    [TestFixture]
    public class LineTokenizerTests
    {
        #region While-Edge Tests (CheckWhileConditions + WhileHasBackReferences)

        /// <summary>
        /// Exercises CheckWhileConditions: WhileStack population (lines 496-503),
        /// while pattern match (r != null, line 511), capture processing (lines 521-527),
        /// and End > linePos advancement (lines 528-532).
        /// Line 1 ">> hello" triggers the begin pattern, pushing a BeginWhileRule.
        /// Line 2 ">> world" enters CheckWhileConditions, finds the BeginWhileRule on the
        /// stack, compiles the while pattern ^(\s*)(>>), matches at position 0 with
        /// End = 2 > linePos = 0, advancing linePos to 2.
        /// </summary>
        [Test]
        public void WhileEdge_ContinuationWithCaptures_AdvancesLinePos()
        {
            // arrange
            Registry.Registry registry = CreateRegistryForWhileEdge();
            IGrammar grammar = registry.LoadGrammar("source.test.while");

            // act - line 1 opens the while block, line 2 continues it
            ITokenizeLineResult result1 = grammar.TokenizeLine(">> hello");
            ITokenizeLineResult result2 = grammar.TokenizeLine(">> world", result1.RuleStack, TimeSpan.MaxValue);

            // assert - line 2 has 3 tokens: >> marker, space, identifier
            Assert.AreEqual(2, result2.RuleStack.Depth);
            Assert.AreEqual(3, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 2,
                "source.test.while", "meta.block.continuation.test", "keyword.operator.continuation.test");
            AssertTokenValuesAreEqual(result2.Tokens[1], 2, 3,
                "source.test.while", "meta.block.continuation.test");
            AssertTokenValuesAreEqual(result2.Tokens[2], 3, 8,
                "source.test.while", "meta.block.continuation.test", "variable.other.test");
        }

        /// <summary>
        /// Exercises CheckWhileConditions: while captures producing scoped tokens.
        /// The whileCaptures definition names group 1 (whitespace) and group 2 (>>),
        /// which should produce distinct scopes through HandleCaptures inside
        /// CheckWhileConditions (lines 524-525).
        /// </summary>
        [Test]
        public void WhileEdge_WhileCapturesProduceScopes()
        {
            // arrange
            Registry.Registry registry = CreateRegistryForWhileEdge();
            IGrammar grammar = registry.LoadGrammar("source.test.while");

            // act - indented continuation: "  >> content"
            ITokenizeLineResult result1 = grammar.TokenizeLine("  >> start");
            ITokenizeLineResult result2 = grammar.TokenizeLine("  >> still going", result1.RuleStack, TimeSpan.MaxValue);

            // assert - line 2 has 6 tokens: whitespace capture, >> capture, space, "still", space, "going"
            Assert.AreEqual(2, result2.RuleStack.Depth);
            Assert.AreEqual(6, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 2,
                "source.test.while", "meta.block.continuation.test", "punctuation.whitespace.leading.test");
            AssertTokenValuesAreEqual(result2.Tokens[1], 2, 4,
                "source.test.while", "meta.block.continuation.test", "keyword.operator.continuation.test");
            AssertTokenValuesAreEqual(result2.Tokens[2], 4, 5,
                "source.test.while", "meta.block.continuation.test");
            AssertTokenValuesAreEqual(result2.Tokens[3], 5, 10,
                "source.test.while", "meta.block.continuation.test", "variable.other.test");
        }

        /// <summary>
        /// Exercises CheckWhileConditions: r == null path (lines 535-538) -
        /// while pattern fails to match, stack pops, break.
        /// Line 1 opens the while block with ">> start".
        /// Line 2 "plain text" does NOT match the while pattern ^(\s*)(>>), so r is null,
        /// the BeginWhileRule is popped from the stack, and the break exits the loop.
        /// Line 2 should be tokenized as plain content outside the while block.
        /// </summary>
        [Test]
        public void WhileEdge_WhilePatternFails_StackPops()
        {
            // arrange
            Registry.Registry registry = CreateRegistryForWhileEdge();
            IGrammar grammar = registry.LoadGrammar("source.test.while");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine(">> start");
            ITokenizeLineResult result2 = grammar.TokenizeLine("plain text", result1.RuleStack, TimeSpan.MaxValue);

            // assert - depth drops to 1, content is outside while block
            Assert.AreEqual(1, result2.RuleStack.Depth);
            Assert.AreEqual(3, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 5,
                "source.test.while", "variable.other.test");
            AssertTokenValuesAreEqual(result2.Tokens[1], 5, 6,
                "source.test.while");
            AssertTokenValuesAreEqual(result2.Tokens[2], 6, 10,
                "source.test.while", "variable.other.test");
        }

        /// <summary>
        /// Exercises ScanNext line 203-207: WhileHasBackReferences path.
        /// The while-backref rule has while pattern "\\1" (back-reference to captured
        /// delimiter). When begin matches "WHILE-FOO", group 1 captures "FOO".
        /// Since the while pattern contains \1, WhileHasBackReferences is true,
        /// and ScanNext calls WithEndRule(GetWhileWithResolvedBackReferences(...))
        /// to store the resolved pattern "FOO" on the stack.
        /// </summary>
        [Test]
        public void WhileEdge_WhileHasBackReferences_ResolvesOnBegin()
        {
            // arrange
            Registry.Registry registry = CreateRegistryForWhileEdge();
            IGrammar grammar = registry.LoadGrammar("source.test.while");

            // act - line 1 triggers begin with back-reference resolution
            ITokenizeLineResult result1 = grammar.TokenizeLine("WHILE-FOO content");

            // assert - 4 tokens: "WHILE-" begin, "FOO" delimiter, space, "content" identifier
            Assert.AreEqual(2, result1.RuleStack.Depth);
            Assert.AreEqual(4, result1.Tokens.Length);
            AssertTokenValuesAreEqual(result1.Tokens[0], 0, 6,
                "source.test.while", "meta.while.backref.test", "keyword.control.while.begin.test");
            AssertTokenValuesAreEqual(result1.Tokens[1], 6, 9,
                "source.test.while", "meta.while.backref.test", "keyword.control.while.begin.test",
                "entity.name.tag.delimiter.test");
            AssertTokenValuesAreEqual(result1.Tokens[2], 9, 10,
                "source.test.while", "meta.while.backref.test");
            AssertTokenValuesAreEqual(result1.Tokens[3], 10, 17,
                "source.test.while", "meta.while.backref.test", "variable.other.test");
        }

        /// <summary>
        /// Exercises CheckWhileConditions with resolved back-reference on continuation line.
        /// After "WHILE-FOO" on line 1, the resolved while pattern is "FOO".
        /// Line 2 "FOO more content" starts with "FOO" which matches the resolved pattern,
        /// so the while check succeeds and the block continues.
        /// Line 3 "no match" has no occurrence of "FOO", so while fails and pops.
        /// </summary>
        [Test]
        public void WhileEdge_WhileBackReference_ContinuationMatchesResolvedPattern()
        {
            // arrange
            Registry.Registry registry = CreateRegistryForWhileEdge();
            IGrammar grammar = registry.LoadGrammar("source.test.while");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine("WHILE-FOO content");
            ITokenizeLineResult result2 = grammar.TokenizeLine("FOO more content", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("no match", result2.RuleStack, TimeSpan.MaxValue);

            // assert - line 2: still inside while-backref block, FOO matched as continuation
            Assert.AreEqual(2, result2.RuleStack.Depth);
            Assert.AreEqual(5, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 3,
                "source.test.while", "meta.while.backref.test", "keyword.control.while.continuation.test");
            AssertTokenValuesAreEqual(result2.Tokens[1], 3, 4,
                "source.test.while", "meta.while.backref.test");
            AssertTokenValuesAreEqual(result2.Tokens[2], 4, 8,
                "source.test.while", "meta.while.backref.test", "variable.other.test");

            // assert - line 3: popped out, depth 1, no while scope
            Assert.AreEqual(1, result3.RuleStack.Depth);
            Assert.AreEqual(3, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 2,
                "source.test.while", "variable.other.test");
        }

        /// <summary>
        /// Exercises a different back-reference delimiter to confirm the resolution is
        /// dynamic. "WHILE-BAR" captures "BAR", and the while pattern resolves to "BAR".
        /// Line 2 "BAR more" continues (BAR found at position 0).
        /// Line 3 "done" has no occurrence of BAR anywhere, so while fails and pops.
        /// NOTE: The while pattern \1 resolves to an unanchored match, so "not BAR"
        /// would still match BAR at position 4 - the termination line must contain
        /// NO occurrence of the delimiter at all.
        /// </summary>
        [Test]
        public void WhileEdge_WhileBackReference_DifferentDelimiter()
        {
            // arrange
            Registry.Registry registry = CreateRegistryForWhileEdge();
            IGrammar grammar = registry.LoadGrammar("source.test.while");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine("WHILE-BAR stuff");
            ITokenizeLineResult result2 = grammar.TokenizeLine("BAR more", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("done", result2.RuleStack, TimeSpan.MaxValue);

            // assert - line 2: continues with BAR matched as continuation at [0,3)
            Assert.AreEqual(2, result2.RuleStack.Depth);
            Assert.AreEqual(3, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 3,
                "source.test.while", "meta.while.backref.test", "keyword.control.while.continuation.test");
            AssertTokenValuesAreEqual(result2.Tokens[1], 3, 4,
                "source.test.while", "meta.while.backref.test");
            AssertTokenValuesAreEqual(result2.Tokens[2], 4, 8,
                "source.test.while", "meta.while.backref.test", "variable.other.test");

            // assert - line 3: "done" has no BAR, while fails, depth drops to 1
            Assert.AreEqual(1, result3.RuleStack.Depth);
            Assert.AreEqual(1, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 4,
                "source.test.while", "variable.other.test");
        }

        #endregion While-Edge Tests (CheckWhileConditions + WhileHasBackReferences)

        #region Single-Line Tokenization (ScanNext - basic flow, no prior state)

        [Test]
        public void TokenizeLine_UsingStatement_ProducesKeywordNamespaceAndTerminator()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("using System;");

            // assert
            Assert.AreEqual(4, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 5, "source.cs", "keyword.other.using.cs");
            AssertTokenValuesAreEqual(result.Tokens[1], 5, 6, "source.cs");
            AssertTokenValuesAreEqual(result.Tokens[2], 6, 12, "source.cs", "entity.name.type.namespace.cs");
            AssertTokenValuesAreEqual(result.Tokens[3], 12, 13, "source.cs", "punctuation.terminator.statement.cs");
        }

        [Test]
        public void TokenizeLine_NullPreviousState_ProducesValidResultWithNonNullRuleStack()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("int x = 5;", null, TimeSpan.MaxValue);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(8, result.Tokens.Length);
            Assert.AreEqual(1, result.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_MethodSignature_ProducesThirteenContiguousTokens()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "public static void Main() { }";

            // act
            ITokenizeLineResult result = grammar.TokenizeLine(line);

            // assert
            Assert.AreEqual(13, result.Tokens.Length);
            AssertAllTokensCoverLine(result.Tokens, line.Length);
        }

        #endregion Single-Line Tokenization

        #region No-Match Path (ScanNext r == null branch)

        [Test]
        public void TokenizeLine_EmptyString_ProducesSingleRootScopeToken()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("");

            // assert
            Assert.AreEqual(1, result.Tokens.Length);
            Assert.AreEqual(0, result.Tokens[0].StartIndex);
            Assert.IsNotNull(result.RuleStack);
        }

        [Test]
        public void TokenizeLine_WhitespaceOnly_ProducesSingleTokenWithRootScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("    ");

            // assert - single token [0,5): EndIndex is 5 (not 4) because Grammar.TokenizeLine
            // appends \n internally and GetResult does not strip the newline when it merges
            // into the only token rather than appearing as a separate token.
            Assert.AreEqual(1, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 5, "source.cs");
        }

        #endregion No-Match Path

        #region Multi-Line Tokenization (CheckWhileConditions path)

        [Test]
        public void TokenizeLine_MultiLineBlockComment_SecondLineIsEntirelyCommentScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine("/* start of comment");
            ITokenizeLineResult result2 = grammar.TokenizeLine("   still in comment", result1.RuleStack, TimeSpan.MaxValue);

            // assert - single token on line 2, entirely inside comment.block.cs
            Assert.AreEqual(1, result2.Tokens.Length);
            Assert.IsTrue(result2.Tokens[0].Scopes.Contains("comment.block.cs"),
                "First token on continuation line should be inside comment.block.cs");
        }

        [Test]
        public void TokenizeLine_MultiLineBlockComment_ThirdLineClosesComment()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine("/* start");
            ITokenizeLineResult result2 = grammar.TokenizeLine("   middle", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("   end */", result2.RuleStack, TimeSpan.MaxValue);

            // assert - depth goes from 2 (inside comment) back to 1 (root) after closing
            Assert.AreEqual(2, result1.RuleStack.Depth);
            Assert.AreEqual(1, result3.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_MultiLineVerbatimString_SecondLineContainsStringScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine("string s = @\"multi");
            ITokenizeLineResult result2 = grammar.TokenizeLine("line string\";", result1.RuleStack, TimeSpan.MaxValue);

            // assert - 3 tokens on line 2, first token is still inside string scope
            Assert.AreEqual(3, result2.Tokens.Length);
            Assert.IsTrue(result2.Tokens[0].Scopes.Contains("string.quoted.double.cs"),
                "First token on continuation line of verbatim string should carry string.quoted.double.cs");
        }

        [Test]
        public void TokenizeLine_MultiLineCodeBlock_Line1_ProducesStorageModifierAndFunctionName()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act - "public int Compute()"
            ITokenizeLineResult result = grammar.TokenizeLine("public int Compute()");

            // assert - known positions from GrammarTests.Parse_Multiline_Text
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 6, "source.cs", "storage.modifier.cs");
            AssertTokenValuesAreEqual(result.Tokens[2], 7, 10, "source.cs", "keyword.type.cs");
            AssertTokenValuesAreEqual(result.Tokens[4], 11, 18, "source.cs", "entity.name.function.cs");
            AssertTokenValuesAreEqual(result.Tokens[5], 18, 19, "source.cs", "punctuation.parenthesis.open.cs");
            AssertTokenValuesAreEqual(result.Tokens[6], 19, 20, "source.cs", "punctuation.parenthesis.close.cs");
        }

        [Test]
        public void TokenizeLine_MultiLineCodeBlock_Line2_ProducesOpenCurlyBrace()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("public int Compute()");

            // act - "{"
            ITokenizeLineResult result2 = grammar.TokenizeLine("{", result1.RuleStack, TimeSpan.MaxValue);

            // assert
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 1, "source.cs", "punctuation.curlybrace.open.cs");
        }

        [Test]
        public void TokenizeLine_MultiLineCodeBlock_Line3_ProducesReturnKeywordAndArithmetic()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("public int Compute()");
            ITokenizeLineResult result2 = grammar.TokenizeLine("{", result1.RuleStack, TimeSpan.MaxValue);

            // act - "    return 5 + 8;"
            ITokenizeLineResult result3 = grammar.TokenizeLine("    return 5 + 8;", result2.RuleStack, TimeSpan.MaxValue);

            // assert - known positions from GrammarTests.Parse_Multiline_Text
            AssertTokenValuesAreEqual(result3.Tokens[1], 4, 10, "source.cs", "keyword.control.flow.return.cs");
            AssertTokenValuesAreEqual(result3.Tokens[3], 11, 12, "source.cs", "constant.numeric.decimal.cs");
            AssertTokenValuesAreEqual(result3.Tokens[5], 13, 14, "source.cs", "keyword.operator.arithmetic.cs");
            AssertTokenValuesAreEqual(result3.Tokens[7], 15, 16, "source.cs", "constant.numeric.decimal.cs");
            AssertTokenValuesAreEqual(result3.Tokens[8], 16, 17, "source.cs", "punctuation.terminator.statement.cs");
        }

        [Test]
        public void TokenizeLine_MultiLineCodeBlock_Line4_ProducesCloseCurlyBrace()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("public int Compute()");
            ITokenizeLineResult result2 = grammar.TokenizeLine("{", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("    return 5 + 8;", result2.RuleStack, TimeSpan.MaxValue);

            // act - "}"
            ITokenizeLineResult result4 = grammar.TokenizeLine("}", result3.RuleStack, TimeSpan.MaxValue);

            // assert
            AssertTokenValuesAreEqual(result4.Tokens[0], 0, 1, "source.cs", "punctuation.curlybrace.close.cs");
        }

        #endregion Multi-Line Tokenization

        #region BeginEnd Rule Matching (ScanNext - BeginEndRule branch)

        [Test]
        public void TokenizeLine_StringLiteral_ProducesKeywordVariableAssignmentAndStringTokens()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("string s = \"hello\";");

            // assert - exact token layout from diagnostic dump
            Assert.AreEqual(10, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 6, "source.cs", "keyword.type.cs");
            AssertTokenValuesAreEqual(result.Tokens[1], 6, 7, "source.cs");
            AssertTokenValuesAreEqual(result.Tokens[2], 7, 8, "source.cs", "entity.name.variable.local.cs");
            AssertTokenValuesAreEqual(result.Tokens[3], 8, 9, "source.cs");
            AssertTokenValuesAreEqual(result.Tokens[4], 9, 10, "source.cs", "keyword.operator.assignment.cs");
            AssertTokenValuesAreEqual(result.Tokens[5], 10, 11, "source.cs");
            AssertTokenValuesAreEqual(result.Tokens[6], 11, 12, "source.cs", "string.quoted.double.cs", "punctuation.definition.string.begin.cs");
            AssertTokenValuesAreEqual(result.Tokens[7], 12, 17, "source.cs", "string.quoted.double.cs");
            AssertTokenValuesAreEqual(result.Tokens[8], 17, 18, "source.cs", "string.quoted.double.cs", "punctuation.definition.string.end.cs");
            AssertTokenValuesAreEqual(result.Tokens[9], 18, 19, "source.cs", "punctuation.terminator.statement.cs");
        }

        [Test]
        public void TokenizeLine_SingleLineBlockComment_FirstTokenHasCommentAndPunctuationScopes()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("/* comment */");

            // assert - 3 tokens: open punctuation, comment body, close punctuation
            Assert.AreEqual(3, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 2, "source.cs", "comment.block.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result.Tokens[1], 2, 11, "source.cs", "comment.block.cs");
            AssertTokenValuesAreEqual(result.Tokens[2], 11, 13, "source.cs", "comment.block.cs", "punctuation.definition.comment.cs");
        }

        [Test]
        public void TokenizeLine_OpenBrace_LastTokenIsOpenCurlyBrace()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("class Foo {");

            // assert - 5 tokens, last is open brace at [10,11)
            Assert.AreEqual(5, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 5, "source.cs", "keyword.other.class.cs");
            AssertTokenValuesAreEqual(result.Tokens[2], 6, 9, "source.cs", "entity.name.type.class.cs");
            AssertTokenValuesAreEqual(result.Tokens[4], 10, 11, "source.cs", "punctuation.curlybrace.open.cs");
        }

        [Test]
        public void TokenizeLine_CloseBrace_AfterClassDeclaration_ProducesCloseBraceToken()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("class Foo {");

            // act
            ITokenizeLineResult result2 = grammar.TokenizeLine("}", result1.RuleStack, TimeSpan.MaxValue);

            // assert
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 1, "source.cs", "punctuation.curlybrace.close.cs");
        }

        #endregion BeginEnd Rule Matching

        #region MatchRule Matching (ScanNext - MatchRule branch, pop immediately)

        [Test]
        public void TokenizeLine_ReturnKeyword_ProducesKeywordControlScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult setup = grammar.TokenizeLine("void M() {");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("    return 42;", setup.RuleStack, TimeSpan.MaxValue);

            // assert - "return" at token index 1 (after leading whitespace)
            AssertTokenValuesAreEqual(result.Tokens[1], 4, 10, "source.cs", "keyword.control.flow.return.cs");
        }

        [Test]
        public void TokenizeLine_NumericLiteral_ProducesConstantNumericScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult setup = grammar.TokenizeLine("void M() {");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("    return 5 + 8;", setup.RuleStack, TimeSpan.MaxValue);

            // assert - "5" at known position
            AssertTokenValuesAreEqual(result.Tokens[3], 11, 12, "source.cs", "constant.numeric.decimal.cs");
        }

        [Test]
        public void TokenizeLine_ArithmeticOperator_ProducesOperatorScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult setup = grammar.TokenizeLine("void M() {");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("    return 5 + 8;", setup.RuleStack, TimeSpan.MaxValue);

            // assert - "+" at known position
            AssertTokenValuesAreEqual(result.Tokens[5], 13, 14, "source.cs", "keyword.operator.arithmetic.cs");
        }

        #endregion MatchRule Matching

        #region Injection Rules (MatchRuleOrInjections - MatchInjections path)

        [Test]
        public void TokenizeLine_InjectedGrammar_ProducesExpectedTokenCount()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithInjections();
            IGrammar grammar = registry.LoadGrammar("source.ts");

            // act - matches GrammarTests.Injected_Grammars_Should_Generate_Tokens
            ITokenizeLineResult result = grammar.TokenizeLine("@Component({template:`<a href='' ></a>`})");

            // assert
            Assert.AreEqual(11, result.Tokens.Length);
        }

        [Test]
        public void TokenizeLine_InjectedGrammar_DecoratorPunctuationAtCorrectPosition()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithInjections();
            IGrammar grammar = registry.LoadGrammar("source.ts");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("@Component({template:`<a href='' ></a>`})");

            // assert
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 1,
                "source.ts", "meta.decorator.ts", "punctuation.decorator.ts");
        }

        [Test]
        public void TokenizeLine_InjectedGrammar_FunctionNameAtCorrectPosition()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithInjections();
            IGrammar grammar = registry.LoadGrammar("source.ts");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("@Component({template:`<a href='' ></a>`})");

            // assert
            AssertTokenValuesAreEqual(result.Tokens[1], 1, 10,
                "source.ts", "meta.decorator.ts", "entity.name.function.ts");
        }

        [Test]
        public void TokenizeLine_InjectedGrammar_TemplateStringBeginAtCorrectPosition()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithInjections();
            IGrammar grammar = registry.LoadGrammar("source.ts");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("@Component({template:`<a href='' ></a>`})");

            // assert
            AssertTokenValuesAreEqual(result.Tokens[6], 21, 22,
                "source.ts", "meta.decorator.ts", "meta.object-literal.ts",
                "meta.object.member.ts", "string.template.ts",
                "punctuation.definition.string.template.begin.ts");
        }

        [Test]
        public void TokenizeLine_NoInjections_ProducesValidTokensViaNonInjectionPath()
        {
            // arrange - registry WITHOUT injections
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("int x = 5;");

            // assert
            Assert.AreEqual(8, result.Tokens.Length);
            Assert.AreEqual(1, result.RuleStack.Depth);
        }

        #endregion Injection Rules

        #region Time Limit / StoppedEarly (Scan - timeout branch)

        [Test]
        public void TokenizeLine_ZeroTimeLimit_ProducesNonNullResultAndRuleStack()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "public static async Task<Dictionary<string, List<int>>> Method(string p1, int p2) { return null; }";

            // act
            ITokenizeLineResult result = grammar.TokenizeLine(line, null, TimeSpan.Zero);

            // assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.RuleStack, "Rule stack should be non-null even when time limit is exceeded");
            Assert.IsNotNull(result.Tokens, "Tokens should be non-null even when time limit is exceeded");
        }

        [Test]
        public void TokenizeLine_MaxTimeLimit_ProducesThirteenContiguousTokens()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "public static void Main() { }";

            // act
            ITokenizeLineResult result = grammar.TokenizeLine(line, null, TimeSpan.MaxValue);

            // assert
            Assert.AreEqual(13, result.Tokens.Length);
            AssertAllTokensCoverLine(result.Tokens, line.Length);
        }

        [Test]
        public void TokenizeLine_ZeroTimeLimit_ProducesNoMoreTokensThanFullTokenization()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "public static async Task<Dictionary<string, List<int>>> Method(string p1, int p2) { return null; }";

            // act
            ITokenizeLineResult fullResult = grammar.TokenizeLine(line, null, TimeSpan.MaxValue);
            ITokenizeLineResult partialResult = grammar.TokenizeLine(line, null, TimeSpan.Zero);

            // assert
            Assert.IsTrue(partialResult.Tokens.Length <= fullResult.Tokens.Length,
                "Partial tokenization (count={0}) should not exceed full tokenization (count={1})", partialResult.Tokens.Length, fullResult.Tokens.Length);
        }

        #endregion Time Limit / StoppedEarly

        #region Unicode Content (ReadOnlyMemory<char> slicing)

        [Test]
        public void TokenizeLine_UnicodeStringLiteral_StringContentAtCorrectPosition()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act - matches GrammarTests.Parse_Unicode_String_Should_Generate_Tokens
            ITokenizeLineResult result = grammar.TokenizeLine("string s = \"chars: 安定させる\";");

            // assert - string content at token[7]
            Assert.IsTrue(result.Tokens[7].Scopes.Contains("string.quoted.double.cs"),
                "Token 7 should contain string.quoted.double.cs");
            Assert.AreEqual(12, result.Tokens[7].StartIndex);
            Assert.AreEqual(24, result.Tokens[7].EndIndex);
        }

        [Test]
        public void TokenizeLine_UnicodeStringLiteral_ClosingQuoteAtCorrectPosition()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("string s = \"chars: 安定させる\";");

            // assert - closing quote at token[8]
            Assert.IsTrue(result.Tokens[8].Scopes.Contains("punctuation.definition.string.end.cs"),
                "Token 8 should contain punctuation.definition.string.end.cs");
            Assert.AreEqual(24, result.Tokens[8].StartIndex);
            Assert.AreEqual(25, result.Tokens[8].EndIndex);
        }

        [Test]
        public void TokenizeLine_UnicodeComment_ProducesTwoTokensWithCorrectScopes()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act - matches GrammarTests.Parse_Unicode_Comments_Should_Generate_Tokens
            ITokenizeLineResult result = grammar.TokenizeLine("//安安安");

            // assert
            Assert.AreEqual(2, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 2,
                "source.cs", "comment.line.double-slash.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result.Tokens[1], 2, 5,
                "source.cs", "comment.line.double-slash.cs");
        }

        [Test]
        public void TokenizeLine_MixedAsciiAndUnicode_AllTokensAreContiguous()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "// ASCII and 日本語 mixed";

            // act
            ITokenizeLineResult result = grammar.TokenizeLine(line);

            // assert - 2 tokens: punctuation + comment body (same as pure-ASCII comment)
            Assert.AreEqual(2, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 2,
                "source.cs", "comment.line.double-slash.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result.Tokens[1], 2, 22,
                "source.cs", "comment.line.double-slash.cs");
        }

        #endregion Unicode Content

        #region Binary Tokenization (TokenizeLine2 - emitBinaryTokens path)

        [Test]
        public void TokenizeLine2_UsingStatement_ProducesFourBinaryTokenEntries()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult2 result = grammar.TokenizeLine2("using System;");

            // assert - binary tokens come in [startIndex, metadata] pairs
            Assert.AreEqual(4, result.Tokens.Length);
            Assert.AreEqual(0, result.Tokens.Length % 2, "Binary tokens should come in pairs");
        }

        [Test]
        public void TokenizeLine2_NullPreviousState_ProducesTwelveBinaryTokenEntries()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult2 result = grammar.TokenizeLine2("int x = 5;", null, TimeSpan.MaxValue);

            // assert
            Assert.AreEqual(12, result.Tokens.Length);
            Assert.IsNotNull(result.RuleStack);
        }

        [Test]
        public void TokenizeLine2_MultiLineBlockComment_ProducesTwoBinaryTokenEntriesPerLine()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult2 result1 = grammar.TokenizeLine2("/* start");
            ITokenizeLineResult2 result2 = grammar.TokenizeLine2("end */", result1.RuleStack, TimeSpan.MaxValue);

            // assert
            Assert.AreEqual(2, result1.Tokens.Length);
            Assert.AreEqual(2, result2.Tokens.Length);
            Assert.IsNotNull(result1.RuleStack);
            Assert.IsNotNull(result2.RuleStack);
        }

        #endregion Binary Tokenization

        #region HandleCaptures (exercised through capture groups)

        [Test]
        public void TokenizeLine_LineComment_PunctuationCapturedAsSeparateToken()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("// This is a comment");

            // assert - "//" captured as punctuation.definition.comment.cs
            Assert.IsTrue(result.Tokens.Length >= 2, "Expected at least punctuation + comment body");
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 2,
                "source.cs", "comment.line.double-slash.cs", "punctuation.definition.comment.cs");
        }

        [Test]
        public void TokenizeLine_MethodDeclaration_FunctionNameAtCorrectPosition()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("void Calculate()");

            // assert - exact token layout from diagnostic dump
            Assert.AreEqual(5, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 4, "source.cs", "keyword.type.cs");
            AssertTokenValuesAreEqual(result.Tokens[1], 4, 5, "source.cs");
            AssertTokenValuesAreEqual(result.Tokens[2], 5, 14, "source.cs", "entity.name.function.cs");
            AssertTokenValuesAreEqual(result.Tokens[3], 14, 15, "source.cs", "punctuation.parenthesis.open.cs");
            AssertTokenValuesAreEqual(result.Tokens[4], 15, 16, "source.cs", "punctuation.parenthesis.close.cs");
        }

        #endregion HandleCaptures

        #region Endless Loop Detection (ScanNext - no-advance safety checks)

        [Test]
        public void TokenizeLine_DeeplyNestedGenerics_CompletesWithoutHanging()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "Dictionary<List<Tuple<int, string, List<HashSet<long>>>>, Dictionary<string, int>> d;";

            // act - if endless loop detection fails, this would hang
            ITokenizeLineResult result = grammar.TokenizeLine(line, null, TimeSpan.FromSeconds(5));

            // assert - the primary assertion is that the act phase completed within 5 seconds.
            // If the endless-loop detection in ScanNext fails, this test hangs and times out.
            // The exact token count depends on grammar rule interactions with deeply nested
            // generics, so we verify a non-null result with a valid rule stack.
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.RuleStack);
            Assert.AreEqual(1, result.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_EmptyBraces_CompletesWithoutInfiniteLoop()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("{}", null, TimeSpan.FromSeconds(5));

            // assert - two tokens: open brace + close brace
            Assert.AreEqual(2, result.Tokens.Length);
            Assert.IsTrue(result.Tokens[0].Scopes.Contains("punctuation.curlybrace.open.cs"),
                "First token should be open brace");
            Assert.IsTrue(result.Tokens[1].Scopes.Contains("punctuation.curlybrace.close.cs"),
                "Second token should be close brace");
        }

        #endregion Endless Loop Detection

        #region CSS Grammar (exercises different rule patterns)

        [Test]
        public void TokenizeLine_CssPropertyRule_ProducesTwelveTokens()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.css");
            const string line = "body { margin: 25px; }";

            // act - matches GrammarTests.Parse_Css_Statement_Should_Generate_Tokens
            ITokenizeLineResult result = grammar.TokenizeLine(line);

            // assert
            Assert.AreEqual(12, result.Tokens.Length);
            AssertAllTokensCoverLine(result.Tokens, line.Length);
        }

        [Test]
        public void TokenizeLine_CssMultiLine_EachLineProducesTokens()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.css");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine(".container {");
            ITokenizeLineResult result2 = grammar.TokenizeLine("    color: red;", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("}", result2.RuleStack, TimeSpan.MaxValue);

            // assert
            Assert.AreEqual(4, result1.Tokens.Length);
            Assert.AreEqual(6, result2.Tokens.Length);
            Assert.AreEqual(1, result3.Tokens.Length);
        }

        #endregion CSS Grammar

        #region Batch Grammar (minimal grammar, simple rules)

        [Test]
        public void TokenizeLine_BatchComment_ProducesTwoTokens()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.batchfile");

            // act - matches GrammarTests.Parse_Batch_Statement_Should_Generate_Tokens
            ITokenizeLineResult result = grammar.TokenizeLine("REM echo off");

            // assert
            Assert.AreEqual(2, result.Tokens.Length);
        }

        #endregion Batch Grammar

        #region First Line Detection (isFirstLine parameter)

        [Test]
        public void TokenizeLine_FirstLineThenSecondLine_StateDepthDecreasesAfterClosingBrace()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine("namespace Foo");
            ITokenizeLineResult result2 = grammar.TokenizeLine("{", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("}", result2.RuleStack, TimeSpan.MaxValue);

            // assert - exact depths from diagnostic dump
            Assert.AreEqual(2, result1.RuleStack.Depth);
            Assert.AreEqual(3, result2.RuleStack.Depth);
            Assert.AreEqual(1, result3.RuleStack.Depth);
        }

        #endregion First Line Detection

        #region Edge Cases

        [Test]
        public void TokenizeLine_Semicolon_ProducesSingleTerminatorToken()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine(";");

            // assert
            Assert.AreEqual(1, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 1, "source.cs", "punctuation.terminator.statement.cs");
        }

        [Test]
        public void TokenizeLine_VeryLongStringLiteral_ProducesTenTokens()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            string longLine = "string s = \"" + new string('a', 1_000) + "\";";

            // act
            ITokenizeLineResult result = grammar.TokenizeLine(longLine, null, TimeSpan.FromSeconds(10));

            // assert - same 10-token structure as short string literal
            Assert.AreEqual(10, result.Tokens.Length);
        }

        [Test]
        public void TokenizeLine_SameInputTwice_ProducesIdenticalTokenCounts()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "public class Foo { }";

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine(line);
            ITokenizeLineResult result2 = grammar.TokenizeLine(line);

            // assert
            Assert.AreEqual(result1.Tokens.Length, result2.Tokens.Length, "Token count should be deterministic");
        }

        [Test]
        public void TokenizeLine_SameInputTwice_FirstAndLastTokenIndicesMatch()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            const string line = "int x = 42;";

            // act
            ITokenizeLineResult result1 = grammar.TokenizeLine(line);
            ITokenizeLineResult result2 = grammar.TokenizeLine(line);

            // assert - first token
            Assert.AreEqual(result1.Tokens[0].StartIndex, result2.Tokens[0].StartIndex);
            Assert.AreEqual(result1.Tokens[0].EndIndex, result2.Tokens[0].EndIndex);

            // assert - last token
            int lastIdx = result1.Tokens.Length - 1;
            Assert.AreEqual(result1.Tokens[lastIdx].StartIndex, result2.Tokens[lastIdx].StartIndex);
            Assert.AreEqual(result1.Tokens[lastIdx].EndIndex, result2.Tokens[lastIdx].EndIndex);
        }

        [Test]
        public void TokenizeLine_EscapeSequenceInCharLiteral_ProducesTenTokensWithEscapeScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("char c = '\\n';");

            // assert - exact token layout from diagnostic dump
            Assert.AreEqual(10, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 4, "source.cs", "keyword.type.cs");
            AssertTokenValuesAreEqual(result.Tokens[6], 9, 10, "source.cs", "string.quoted.single.cs", "punctuation.definition.char.begin.cs");
            AssertTokenValuesAreEqual(result.Tokens[7], 10, 12, "source.cs", "string.quoted.single.cs", "constant.character.escape.cs");
            AssertTokenValuesAreEqual(result.Tokens[8], 12, 13, "source.cs", "string.quoted.single.cs", "punctuation.definition.char.end.cs");
            AssertTokenValuesAreEqual(result.Tokens[9], 13, 14, "source.cs", "punctuation.terminator.statement.cs");
        }

        #endregion Edge Cases

        #region CheckWhileConditions - WhileCaptures and line advancement

        [Test]
        public void TokenizeLine_LineCommentContinuation_SecondLineProducesWhileCapturesAndCommentBody()
        {
            // arrange - line comments use BeginWhileRule with while pattern "^(\s*)(//).*$"
            // The highlighted CheckWhileConditions block fires on line 2 when the while
            // pattern matches: it calls Produce, HandleCaptures (WhileCaptures), Produce,
            // and advances linePos past the captured "//" punctuation.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("// line one");

            // act
            ITokenizeLineResult result2 = grammar.TokenizeLine("// line two", result1.RuleStack, TimeSpan.MaxValue);

            // assert - 2 tokens: punctuation "//" + comment body, same structure as line 1
            Assert.AreEqual(2, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 2,
                "source.cs", "comment.line.double-slash.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result2.Tokens[1], 2, 11,
                "source.cs", "comment.line.double-slash.cs");
        }

        [Test]
        public void TokenizeLine_LineCommentContinuation_ThirdLineContinuesWhilePattern()
        {
            // arrange - exercises repeated while-condition matching across 3 lines
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("// line one");
            ITokenizeLineResult result2 = grammar.TokenizeLine("// line two", result1.RuleStack, TimeSpan.MaxValue);

            // act
            ITokenizeLineResult result3 = grammar.TokenizeLine("// line three", result2.RuleStack, TimeSpan.MaxValue);

            // assert - same 2-token structure on third consecutive comment line
            Assert.AreEqual(2, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 2,
                "source.cs", "comment.line.double-slash.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result3.Tokens[1], 2, 13,
                "source.cs", "comment.line.double-slash.cs");
        }

        [Test]
        public void TokenizeLine_LineCommentContinuation_NonCommentLineBreaksWhileLoop()
        {
            // arrange - when the while pattern fails to match, CheckWhileConditions
            // pops the stack (the else branch: stack = whileRule.Stack.Pop())
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("// line one");
            ITokenizeLineResult result2 = grammar.TokenizeLine("// line two", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("// line three", result2.RuleStack, TimeSpan.MaxValue);

            // act - this line does NOT start with "//" so the while condition fails
            ITokenizeLineResult result4 = grammar.TokenizeLine("not a comment", result3.RuleStack, TimeSpan.MaxValue);

            // assert - no longer in comment scope, tokenized as identifiers
            Assert.AreEqual(5, result4.Tokens.Length);
            AssertTokenValuesAreEqual(result4.Tokens[0], 0, 3, "source.cs", "variable.other.readwrite.cs");
            Assert.AreEqual(1, result4.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_DocCommentContinuation_WhileCapturesProduceXmlTagScopes()
        {
            // arrange - doc comments (///) use BeginWhileRule with while pattern "^(\s*)(///)"
            // The CheckWhileConditions highlighted block processes WhileCaptures which include
            // the "///" punctuation, then ScanNext processes the XML tag content.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("/// <summary>");

            // act - second doc comment line exercises CheckWhileConditions with WhileCaptures
            ITokenizeLineResult result2 = grammar.TokenizeLine("/// doc line two", result1.RuleStack, TimeSpan.MaxValue);

            // assert - 2 tokens: "///" punctuation + doc text
            Assert.AreEqual(2, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 3,
                "source.cs", "comment.block.documentation.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result2.Tokens[1], 3, 16,
                "source.cs", "comment.block.documentation.cs");
        }

        [Test]
        public void TokenizeLine_DocCommentContinuation_ClosingTagLineProducesXmlScopes()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("/// <summary>");
            ITokenizeLineResult result2 = grammar.TokenizeLine("/// doc line two", result1.RuleStack, TimeSpan.MaxValue);

            // act - closing XML tag on third line
            ITokenizeLineResult result3 = grammar.TokenizeLine("/// </summary>", result2.RuleStack, TimeSpan.MaxValue);

            // assert - 5 tokens: "///" punctuation, space, "</", "summary", ">"
            Assert.AreEqual(5, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 3,
                "source.cs", "comment.block.documentation.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result3.Tokens[2], 4, 6,
                "source.cs", "comment.block.documentation.cs", "meta.tag.cs", "punctuation.definition.tag.cs");
            AssertTokenValuesAreEqual(result3.Tokens[3], 6, 13,
                "source.cs", "comment.block.documentation.cs", "meta.tag.cs",
                "entity.name.tag.cs", "entity.name.tag.localname.cs");
            AssertTokenValuesAreEqual(result3.Tokens[4], 13, 14,
                "source.cs", "comment.block.documentation.cs", "meta.tag.cs", "punctuation.definition.tag.cs");
        }

        [Test]
        public void TokenizeLine_DocCommentContinuation_NonDocLineBreaksWhileAndTokenizesNormally()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("/// <summary>");
            ITokenizeLineResult result2 = grammar.TokenizeLine("/// doc line two", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("/// </summary>", result2.RuleStack, TimeSpan.MaxValue);

            // act - non-doc-comment line breaks the while condition
            ITokenizeLineResult result4 = grammar.TokenizeLine("void M() {}", result3.RuleStack, TimeSpan.MaxValue);

            // assert - back to normal C# tokenization
            Assert.AreEqual(8, result4.Tokens.Length);
            AssertTokenValuesAreEqual(result4.Tokens[0], 0, 4, "source.cs", "keyword.type.cs");
            AssertTokenValuesAreEqual(result4.Tokens[2], 5, 6, "source.cs", "entity.name.function.cs");
            Assert.AreEqual(1, result4.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_IndentedCommentContinuation_WhileCapturesIncludeLeadingWhitespace()
        {
            // arrange - the while pattern captures leading whitespace as group 1
            // (punctuation.whitespace.comment.leading.cs), exercising the WhileCaptures
            // handling in the highlighted block with a non-empty whitespace capture.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("    // indented comment");

            // act - continuation line with same indentation
            ITokenizeLineResult result2 = grammar.TokenizeLine("    // still indented", result1.RuleStack, TimeSpan.MaxValue);

            // assert - 3 tokens: leading whitespace, "//" punctuation, comment body
            Assert.AreEqual(3, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 4,
                "source.cs", "punctuation.whitespace.comment.leading.cs");
            AssertTokenValuesAreEqual(result2.Tokens[1], 4, 6,
                "source.cs", "comment.line.double-slash.cs", "punctuation.definition.comment.cs");
            AssertTokenValuesAreEqual(result2.Tokens[2], 6, 21,
                "source.cs", "comment.line.double-slash.cs");
        }

        [Test]
        public void TokenizeLine_IndentedCommentContinuation_NonCommentLineBreaksWhile()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("    // indented comment");
            ITokenizeLineResult result2 = grammar.TokenizeLine("    // still indented", result1.RuleStack, TimeSpan.MaxValue);

            // act - line without "//" breaks the while condition
            ITokenizeLineResult result3 = grammar.TokenizeLine("not indented", result2.RuleStack, TimeSpan.MaxValue);

            // assert - normal identifier tokenization, no comment scopes
            Assert.AreEqual(3, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 3, "source.cs", "variable.other.readwrite.cs");
            Assert.AreEqual(1, result3.RuleStack.Depth);
        }

        #endregion CheckWhileConditions - WhileCaptures and line advancement

        #region CheckWhileConditions - r == null else branch (stack pop on no match)

        [Test]
        public void TokenizeLine_LineCommentThenBlankLine_WhileConditionFailsAndPopsStack()
        {
            // arrange - after a line comment, an empty line causes FindNextMatchSync
            // to return null in CheckWhileConditions, hitting the else branch:
            //   stack = whileRule.Stack.Pop();
            //   break;
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("// comment");

            // act - blank line breaks the while condition via r == null
            ITokenizeLineResult result2 = grammar.TokenizeLine("", result1.RuleStack, TimeSpan.MaxValue);

            // assert - single root-scope token, stack popped back to depth 1
            Assert.AreEqual(1, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 1, "source.cs");
            Assert.AreEqual(1, result2.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_LineCommentThenCodeLine_WhileConditionFailsAndTokenizesNormally()
        {
            // arrange - exercises the same r == null else branch, then normal tokenization
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("// comment");

            // act - code line causes while pattern to fail
            ITokenizeLineResult result2 = grammar.TokenizeLine("x = 5;", result1.RuleStack, TimeSpan.MaxValue);

            // assert - normal C# tokenization after stack pop
            Assert.AreEqual(6, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 1, "source.cs", "variable.other.readwrite.cs");
            AssertTokenValuesAreEqual(result2.Tokens[2], 2, 3, "source.cs", "keyword.operator.assignment.cs");
            AssertTokenValuesAreEqual(result2.Tokens[4], 4, 5, "source.cs", "constant.numeric.decimal.cs");
            AssertTokenValuesAreEqual(result2.Tokens[5], 5, 6, "source.cs", "punctuation.terminator.statement.cs");
            Assert.AreEqual(1, result2.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_DocCommentThenBlankLine_WhileConditionFailsAndPopsStack()
        {
            // arrange - doc comments (///) also use BeginWhileRule; blank line triggers
            // the same r == null else branch in CheckWhileConditions
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.cs");
            ITokenizeLineResult result1 = grammar.TokenizeLine("/// <summary>");

            // act - blank line breaks the while condition
            ITokenizeLineResult result2 = grammar.TokenizeLine("", result1.RuleStack, TimeSpan.MaxValue);

            // assert - single root-scope token, stack popped back to depth 1
            Assert.AreEqual(1, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 1, "source.cs");
            Assert.AreEqual(1, result2.RuleStack.Depth);
        }

        #endregion CheckWhileConditions - r == null else branch (stack pop on no match)

        #region EndHasBackReferences (ScanNext - BeginEndRule back-reference resolution)

        [Test]
        public void TokenizeLine_ShellHeredoc_BeginLineProducesThreeTokensWithHeredocScopes()
        {
            // arrange - shell heredoc "cat <<EOF" uses begin pattern "(<<)\s*\\?([^;&<\s]+)"
            // with end pattern "^(\\2)" - the \\2 back-reference triggers EndHasBackReferences
            // in ScanNext, which calls GetEndWithResolvedBackReferences to resolve \\2 to "EOF".
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.shell");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("cat <<EOF");

            // assert - 3 tokens: "cat " root, "<<" operator, "EOF" token
            Assert.AreEqual(3, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 4, "source.shell");
            AssertTokenValuesAreEqual(result.Tokens[1], 4, 6,
                "source.shell", "string.unquoted.heredoc.expanded.shell", "keyword.operator.heredoc.shell");
            AssertTokenValuesAreEqual(result.Tokens[2], 6, 9,
                "source.shell", "string.unquoted.heredoc.expanded.shell", "keyword.control.heredoc-token.shell");
            Assert.AreEqual(2, result.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_ShellHeredoc_ContentLineIsEntirelyInsideHeredocScope()
        {
            // arrange - line 2 is inside the heredoc body. The end rule has been resolved
            // via EndHasBackReferences to match literal "EOF" instead of back-reference \\2.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.shell");
            ITokenizeLineResult result1 = grammar.TokenizeLine("cat <<EOF");

            // act
            ITokenizeLineResult result2 = grammar.TokenizeLine("hello world", result1.RuleStack, TimeSpan.MaxValue);

            // assert - single token spanning full line + newline, inside heredoc scope
            Assert.AreEqual(1, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 12,
                "source.shell", "string.unquoted.heredoc.expanded.shell");
            Assert.AreEqual(2, result2.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_ShellHeredoc_ClosingDelimiterPopsToDepthOne()
        {
            // arrange - the closing "EOF" line triggers the resolved end pattern.
            // This is the payoff: the back-reference \\2 was resolved to "EOF" in line 1,
            // and now the end rule matches "EOF" on line 3, popping the heredoc scope.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.shell");
            ITokenizeLineResult result1 = grammar.TokenizeLine("cat <<EOF");
            ITokenizeLineResult result2 = grammar.TokenizeLine("hello world", result1.RuleStack, TimeSpan.MaxValue);

            // act - closing delimiter
            ITokenizeLineResult result3 = grammar.TokenizeLine("EOF", result2.RuleStack, TimeSpan.MaxValue);

            // assert - single token with heredoc-token scope, stack pops back to depth 1
            Assert.AreEqual(1, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 3,
                "source.shell", "string.unquoted.heredoc.expanded.shell", "keyword.control.heredoc-token.shell");
            Assert.AreEqual(1, result3.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_ShellHeredoc_DifferentDelimiter_ResolvesBackReferenceCorrectly()
        {
            // arrange - using a different delimiter name proves the back-reference resolution
            // is dynamic (not hardcoded to "EOF"). The \\2 in end pattern resolves to "MY_DATA".
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.shell");
            ITokenizeLineResult result1 = grammar.TokenizeLine("cat <<MY_DATA");
            ITokenizeLineResult result2 = grammar.TokenizeLine("some content", result1.RuleStack, TimeSpan.MaxValue);

            // act - "EOF" should NOT close the heredoc, "MY_DATA" should
            ITokenizeLineResult resultWrongDelim = grammar.TokenizeLine("EOF", result2.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult resultRightDelim = grammar.TokenizeLine("MY_DATA", result2.RuleStack, TimeSpan.MaxValue);

            // assert - wrong delimiter stays at depth 2 (still inside heredoc)
            Assert.AreEqual(2, resultWrongDelim.RuleStack.Depth,
                "Wrong delimiter should NOT pop the heredoc scope");
            // assert - correct delimiter pops back to depth 1
            Assert.AreEqual(1, resultRightDelim.RuleStack.Depth,
                "Correct delimiter should pop the heredoc scope");
        }

        [Test]
        public void TokenizeLine_ShellIndentedHeredoc_TabStrippedEndPattern_ResolvesBackReference()
        {
            // arrange - "cat <<-DELIM" uses the indented heredoc pattern with
            // end "^\\t*(\\2)" - tabs are stripped before matching the back-reference.
            // This exercises EndHasBackReferences with a slightly different end pattern.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.shell");
            ITokenizeLineResult result1 = grammar.TokenizeLine("cat <<-DELIM");

            // assert - 4 tokens: "cat " root, "<<" operator, "-" separator, "DELIM" token
            Assert.AreEqual(4, result1.Tokens.Length);
            AssertTokenValuesAreEqual(result1.Tokens[0], 0, 4, "source.shell");
            AssertTokenValuesAreEqual(result1.Tokens[1], 4, 6,
                "source.shell", "string.unquoted.heredoc.expanded.no-indent.shell", "keyword.operator.heredoc.shell");
            AssertTokenValuesAreEqual(result1.Tokens[3], 7, 12,
                "source.shell", "string.unquoted.heredoc.expanded.no-indent.shell", "keyword.control.heredoc-token.shell");
            Assert.AreEqual(2, result1.RuleStack.Depth);

            // act - closing with tab-prefixed delimiter
            ITokenizeLineResult result2 = grammar.TokenizeLine("  indented content", result1.RuleStack, TimeSpan.MaxValue);
            ITokenizeLineResult result3 = grammar.TokenizeLine("\tDELIM", result2.RuleStack, TimeSpan.MaxValue);

            // assert - tab-prefixed DELIM pops the heredoc
            Assert.AreEqual(2, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 1,
                "source.shell", "string.unquoted.heredoc.expanded.no-indent.shell");
            AssertTokenValuesAreEqual(result3.Tokens[1], 1, 6,
                "source.shell", "string.unquoted.heredoc.expanded.no-indent.shell", "keyword.control.heredoc-token.shell");
            Assert.AreEqual(1, result3.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_ShellQuotedHeredoc_SingleQuotesPreventExpansion()
        {
            // arrange - "cat <<'MARKER'" uses quoted heredoc pattern (no variable expansion).
            // Still has EndHasBackReferences via \\3 in end pattern.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.shell");
            ITokenizeLineResult result1 = grammar.TokenizeLine("cat <<'MARKER'");

            // assert - 5 tokens: "cat " root, "<<" operator, "'" quote, "MARKER" token, "'" quote
            Assert.AreEqual(5, result1.Tokens.Length);
            AssertTokenValuesAreEqual(result1.Tokens[0], 0, 4, "source.shell");
            AssertTokenValuesAreEqual(result1.Tokens[1], 4, 6,
                "source.shell", "string.unquoted.heredoc.shell", "keyword.operator.heredoc.shell");
            AssertTokenValuesAreEqual(result1.Tokens[3], 7, 13,
                "source.shell", "string.unquoted.heredoc.shell", "keyword.control.heredoc-token.shell");
            Assert.AreEqual(2, result1.RuleStack.Depth);

            // act - content with $expansion that should NOT be expanded
            ITokenizeLineResult result2 = grammar.TokenizeLine("no $expansion here", result1.RuleStack, TimeSpan.MaxValue);

            // assert - single token, no variable scope (quotes suppress expansion)
            Assert.AreEqual(1, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 19,
                "source.shell", "string.unquoted.heredoc.shell");

            // act - closing delimiter pops
            ITokenizeLineResult result3 = grammar.TokenizeLine("MARKER", result2.RuleStack, TimeSpan.MaxValue);

            // assert
            Assert.AreEqual(1, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 6,
                "source.shell", "string.unquoted.heredoc.shell", "keyword.control.heredoc-token.shell");
            Assert.AreEqual(1, result3.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_RubyHeredoc_SquigglyVariant_BeginLinePushesHeredocScope()
        {
            // arrange - Ruby heredoc requires assignment context to be recognized.
            // "x = <<~CONTENT" uses the squiggly heredoc variant with end "\\s*\\2$\\n?"
            // back-reference - triggers EndHasBackReferences in ScanNext.
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.ruby");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("x = <<~CONTENT");

            // assert - 4 tokens: "x " identifier, "=" assignment, " " space, "<<~CONTENT" heredoc begin
            Assert.AreEqual(4, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 2, "source.ruby");
            AssertTokenValuesAreEqual(result.Tokens[1], 2, 3,
                "source.ruby", "keyword.operator.assignment.ruby");
            AssertTokenValuesAreEqual(result.Tokens[2], 3, 4, "source.ruby");
            AssertTokenValuesAreEqual(result.Tokens[3], 4, 14,
                "source.ruby", "string.unquoted.heredoc.ruby", "punctuation.definition.string.begin.ruby");
            Assert.AreEqual(2, result.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_RubyHeredoc_ContentLineIsInsideHeredocStringScope()
        {
            // arrange
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.ruby");
            ITokenizeLineResult result1 = grammar.TokenizeLine("x = <<~CONTENT");

            // act
            ITokenizeLineResult result2 = grammar.TokenizeLine("  indented ruby heredoc", result1.RuleStack, TimeSpan.MaxValue);

            // assert - single token spanning content + newline, inside heredoc scope
            Assert.AreEqual(1, result2.Tokens.Length);
            AssertTokenValuesAreEqual(result2.Tokens[0], 0, 24,
                "source.ruby", "string.unquoted.heredoc.ruby");
            Assert.AreEqual(2, result2.RuleStack.Depth);
        }

        [Test]
        public void TokenizeLine_RubyHeredoc_ClosingDelimiterPopsToDepthOne()
        {
            // arrange - full 3-line lifecycle: begin -> content -> end.
            // The resolved back-reference in the end pattern matches "CONTENT".
            Registry.Registry registry = CreateRegistry();
            IGrammar grammar = registry.LoadGrammar("source.ruby");
            ITokenizeLineResult result1 = grammar.TokenizeLine("x = <<~CONTENT");
            ITokenizeLineResult result2 = grammar.TokenizeLine("  indented ruby heredoc", result1.RuleStack, TimeSpan.MaxValue);

            // act
            ITokenizeLineResult result3 = grammar.TokenizeLine("CONTENT", result2.RuleStack, TimeSpan.MaxValue);

            // assert - single token with end punctuation scope, depth pops to 1
            Assert.AreEqual(1, result3.Tokens.Length);
            AssertTokenValuesAreEqual(result3.Tokens[0], 0, 7,
                "source.ruby", "string.unquoted.heredoc.ruby", "punctuation.definition.string.end.ruby");
            Assert.AreEqual(1, result3.RuleStack.Depth);
        }

        #endregion EndHasBackReferences (ScanNext - BeginEndRule back-reference resolution)

        #region Synthetic Injection Tests (MatchRuleOrInjections + MatchInjections branch coverage)

        /// <summary>
        /// Branch: MatchRuleOrInjections line 310
        /// injectionResultScore &lt; matchResultScore - injection starts at an earlier position.
        /// Input "@TAG rest": injection matches @TAG at position 0, base grammar matches
        /// identifier "rest" at position 5. Injection wins because 0 &lt; 5.
        /// </summary>
        [Test]
        public void SyntheticInjection_InjectionWinsOnScore_EarlierPosition()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithSyntheticInjections();
            IGrammar grammar = registry.LoadGrammar("source.test.base");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("@TAG rest");

            // assert - 3 tokens: @TAG injection, space, "rest" identifier
            Assert.AreEqual(3, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 4,
                "source.test.base", "entity.name.tag.injected.priority");
            AssertTokenValuesAreEqual(result.Tokens[1], 4, 5,
                "source.test.base");
            AssertTokenValuesAreEqual(result.Tokens[2], 5, 9,
                "source.test.base", "variable.other.test");
        }

        /// <summary>
        /// Branch: MatchRuleOrInjections line 311
        /// injectionResult.IsPriorityMatch &amp;&amp; injectionResultScore == matchResultScore.
        /// Input "KEYWORD": both base grammar and priority injection match at position 0.
        /// The L: prefix injection (priority -1) wins the tie-break.
        /// </summary>
        [Test]
        public void SyntheticInjection_InjectionWinsOnPriorityTie()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithSyntheticInjections();
            IGrammar grammar = registry.LoadGrammar("source.test.base");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("KEYWORD");

            // assert - single token with injection scope, NOT base keyword.control.test
            Assert.AreEqual(1, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 7,
                "source.test.base", "keyword.injected.priority");
        }

        /// <summary>
        /// Branch: MatchRuleOrInjections line 317
        /// return matchResult - base grammar wins over injection.
        /// Uses registry with only normal injection (priority 0, no L: prefix).
        /// Input "KEYWORD INJECT2": base matches KEYWORD at 0, normal injection matches
        /// INJECT2 at 8. injectionScore (8) is NOT less than matchScore (0),
        /// and isPriorityMatch is false, so base wins.
        /// Note: INJECT2 is then tokenized by the base grammar as an identifier (variable.other.test)
        /// because the injection's match was at a later position and didn't win.
        /// </summary>
        [Test]
        public void SyntheticInjection_BaseWinsOverInjection()
        {
            // arrange - normal injection only (priority 0), no L: prefix
            Registry.Registry registry = CreateRegistryWithNormalInjectionOnly();
            IGrammar grammar = registry.LoadGrammar("source.test.base");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("KEYWORD INJECT2");

            // assert - 3 tokens: KEYWORD from base, space, INJECT2 as identifier
            Assert.AreEqual(3, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 7,
                "source.test.base", "keyword.control.test");
            AssertTokenValuesAreEqual(result.Tokens[1], 7, 8,
                "source.test.base");
            AssertTokenValuesAreEqual(result.Tokens[2], 8, 15,
                "source.test.base", "variable.other.test");
        }

        /// <summary>
        /// Branch: MatchInjections line 350
        /// matchRating > bestMatchRating - skip worse injection.
        /// Input " @TAG INJECT2" (leading space): priority injection (processed first due to
        /// sort order) matches @TAG at position 1. Normal injection matches INJECT2 at
        /// position 6. Since 6 > 1, the normal injection is skipped.
        /// After the first ScanNext iteration consumes @TAG, the base grammar then picks
        /// up INJECT2 as a regular identifier (variable.other.test) on the next iteration.
        /// </summary>
        [Test]
        public void SyntheticInjection_SkipWorseInjection()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithSyntheticInjections();
            IGrammar grammar = registry.LoadGrammar("source.test.base");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine(" @TAG INJECT2");

            // assert - 4 tokens: space, @TAG injection, space, INJECT2 as base identifier
            Assert.AreEqual(4, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 1,
                "source.test.base");
            AssertTokenValuesAreEqual(result.Tokens[1], 1, 5,
                "source.test.base", "entity.name.tag.injected.priority");
            AssertTokenValuesAreEqual(result.Tokens[2], 5, 6,
                "source.test.base");
            AssertTokenValuesAreEqual(result.Tokens[3], 6, 13,
                "source.test.base", "variable.other.test");
        }

        /// <summary>
        /// Branches: MatchRuleOrInjections line 300-303 AND MatchInjections line 362-365.
        /// matchResult == null (base grammar has no match) AND bestMatchRating == linePos
        /// (early break). Input "@@" contains no word characters, so the base grammar
        /// finds nothing. The priority injection matches via @+ at position 0 == linePos,
        /// which triggers the early break in MatchInjections. The null matchResult then
        /// causes MatchRuleOrInjections to return the injection result directly.
        /// </summary>
        [Test]
        public void SyntheticInjection_BaseNoMatch_InjectionWins_WithEarlyBreak()
        {
            // arrange
            Registry.Registry registry = CreateRegistryWithSyntheticInjections();
            IGrammar grammar = registry.LoadGrammar("source.test.base");

            // act
            ITokenizeLineResult result = grammar.TokenizeLine("@@");

            // assert - single token: injection's @+ matched "@@" at [0,2)
            Assert.AreEqual(1, result.Tokens.Length);
            AssertTokenValuesAreEqual(result.Tokens[0], 0, 2,
                "source.test.base", "punctuation.injected.priority");
        }

        #endregion Synthetic Injection Tests (MatchRuleOrInjections + MatchInjections branch coverage)

        #region TestRegistry implementations

        /// <summary>
        /// Test registry without injection support. Exercises the MatchRuleOrInjections
        /// early-return path (injections.Count == 0).
        /// </summary>
        private class LineTokenizerTestRegistry : IRegistryOptions
        {
            public IRawTheme GetTheme(string scopeName)
            {
                using var stream = ResourceReader.OpenStream(GetFilePath(scopeName));
                using var reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }

            public IRawGrammar GetGrammar(string scopeName)
            {
                string filePath = GetFilePath(scopeName);
                if (filePath == null)
                    return null;

                using var stream = ResourceReader.OpenStream(filePath);
                using var reader = new StreamReader(stream);
                return GrammarReader.ReadGrammarSync(reader);
            }

            ICollection<string> IRegistryOptions.GetInjections(string scopeName) => null;

            private static string GetFilePath(string scopeName)
            {
                return scopeName switch
                {
                    "source.batchfile" => "batchfile.tmLanguage.json",
                    "source.css" => "css.tmLanguage.json",
                    "source.cs" => "csharp.tmLanguage.json",
                    "source.js" => "JavaScript.tmLanguage.json",
                    "source.ruby" => "ruby.tmLanguage.json",
                    "source.shell" => "shell-unix-bash.tmLanguage.json",
                    "text.html.basic" => "html.json",
                    _ => null
                };
            }

            IRawTheme IRegistryOptions.GetDefaultTheme()
            {
                using Stream stream = ResourceReader.OpenStream("dark_vs.json");
                using StreamReader reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }
        }

        /// <summary>
        /// Test registry WITH injection support for TypeScript + Angular templates.
        /// Exercises the MatchInjections code path.
        /// </summary>
        private class LineTokenizerTestRegistryWithInjections : IRegistryOptions
        {
            public IRawTheme GetTheme(string scopeName)
            {
                using var stream = ResourceReader.OpenStream(GetFilePath(scopeName));
                using var reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }

            public IRawGrammar GetGrammar(string scopeName)
            {
                string filePath = GetFilePath(scopeName);
                if (filePath == null)
                    return null;

                using var stream = ResourceReader.OpenStream(filePath);
                using var reader = new StreamReader(stream);
                return GrammarReader.ReadGrammarSync(reader);
            }

            ICollection<string> IRegistryOptions.GetInjections(string scopeName) => new List<string>() { "template.ng", "styles.ng" };

            private static string GetFilePath(string scopeName)
            {
                return scopeName switch
                {
                    "source.batchfile" => "batchfile.tmLanguage.json",
                    "source.css" => "css.tmLanguage.json",
                    "source.cs" => "csharp.tmLanguage.json",
                    "source.js" => "JavaScript.tmLanguage.json",
                    "source.ruby" => "ruby.tmLanguage.json",
                    "source.shell" => "shell-unix-bash.tmLanguage.json",
                    "text.html.basic" => "html.json",
                    "source.ts" => "TypeScript.tmLanguage.json",
                    "template.ng" => "template.ng.json",
                    "styles.ng" => "styles.ng.json",
                    _ => null
                };
            }

            IRawTheme IRegistryOptions.GetDefaultTheme()
            {
                using Stream stream = ResourceReader.OpenStream("dark_vs.json");
                using StreamReader reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }
        }

        /// <summary>
        /// Test registry for while-edge synthetic grammar testing.
        /// Maps source.test.while to the while-edge grammar file.
        /// </summary>
        private class LineTokenizerTestRegistryForWhileEdge : IRegistryOptions
        {
            public IRawTheme GetTheme(string scopeName)
            {
                using var stream = ResourceReader.OpenStream(GetFilePath(scopeName));
                using var reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }

            public IRawGrammar GetGrammar(string scopeName)
            {
                string filePath = GetFilePath(scopeName);
                if (filePath == null)
                    return null;

                using var stream = ResourceReader.OpenStream(filePath);
                using var reader = new StreamReader(stream);
                return GrammarReader.ReadGrammarSync(reader);
            }

            ICollection<string> IRegistryOptions.GetInjections(string scopeName) => null;

            private static string GetFilePath(string scopeName)
            {
                return scopeName switch
                {
                    "source.test.while" => "test-while-edge.tmLanguage.json",
                    _ => null
                };
            }

            IRawTheme IRegistryOptions.GetDefaultTheme()
            {
                using Stream stream = ResourceReader.OpenStream("dark_vs.json");
                using StreamReader reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }
        }

        /// <summary>
        /// Test registry with BOTH synthetic injections (priority + normal).
        /// Priority injection has L: prefix (priority -1), normal injection has no prefix (priority 0).
        /// Exercises: injection wins on score, injection wins on priority tie,
        /// skip worse injection, early break at linePos.
        /// </summary>
        private class LineTokenizerTestRegistryWithSyntheticInjections : IRegistryOptions
        {
            public IRawTheme GetTheme(string scopeName)
            {
                using var stream = ResourceReader.OpenStream(GetFilePath(scopeName));
                using var reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }

            public IRawGrammar GetGrammar(string scopeName)
            {
                string filePath = GetFilePath(scopeName);
                if (filePath == null)
                    return null;

                using var stream = ResourceReader.OpenStream(filePath);
                using var reader = new StreamReader(stream);
                return GrammarReader.ReadGrammarSync(reader);
            }

            ICollection<string> IRegistryOptions.GetInjections(string scopeName) =>
                new List<string>() { "test.injection.priority", "test.injection.normal" };

            private static string GetFilePath(string scopeName)
            {
                return scopeName switch
                {
                    "source.test.base" => "test-injection-base.tmLanguage.json",
                    "test.injection.priority" => "test-injection-priority.tmLanguage.json",
                    "test.injection.normal" => "test-injection-normal.tmLanguage.json",
                    _ => null
                };
            }

            IRawTheme IRegistryOptions.GetDefaultTheme()
            {
                using Stream stream = ResourceReader.OpenStream("dark_vs.json");
                using StreamReader reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }
        }

        /// <summary>
        /// Test registry with only the NORMAL synthetic injection (priority 0, no L: prefix).
        /// Exercises: base-wins-over-injection path where both match but base has better/equal
        /// position and injection has no priority advantage.
        /// </summary>
        private class LineTokenizerTestRegistryWithNormalInjectionOnly : IRegistryOptions
        {
            public IRawTheme GetTheme(string scopeName)
            {
                using var stream = ResourceReader.OpenStream(GetFilePath(scopeName));
                using var reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }

            public IRawGrammar GetGrammar(string scopeName)
            {
                string filePath = GetFilePath(scopeName);
                if (filePath == null)
                    return null;

                using var stream = ResourceReader.OpenStream(filePath);
                using var reader = new StreamReader(stream);
                return GrammarReader.ReadGrammarSync(reader);
            }

            ICollection<string> IRegistryOptions.GetInjections(string scopeName) =>
                new List<string>() { "test.injection.normal" };

            private static string GetFilePath(string scopeName)
            {
                return scopeName switch
                {
                    "source.test.base" => "test-injection-base.tmLanguage.json",
                    "test.injection.normal" => "test-injection-normal.tmLanguage.json",
                    _ => null
                };
            }

            IRawTheme IRegistryOptions.GetDefaultTheme()
            {
                using Stream stream = ResourceReader.OpenStream("dark_vs.json");
                using StreamReader reader = new StreamReader(stream);
                return ThemeReader.ReadThemeSync(reader);
            }
        }

        #endregion TestRegistry implementations

        #region Helper Infrastructure

        private static Registry.Registry CreateRegistry() => new(new LineTokenizerTestRegistry());

        private static Registry.Registry CreateRegistryWithInjections() => new(new LineTokenizerTestRegistryWithInjections());

        private static Registry.Registry CreateRegistryForWhileEdge() => new(new LineTokenizerTestRegistryForWhileEdge());

        private static Registry.Registry CreateRegistryWithSyntheticInjections() =>
            new(new LineTokenizerTestRegistryWithSyntheticInjections());

        private static Registry.Registry CreateRegistryWithNormalInjectionOnly() =>
            new(new LineTokenizerTestRegistryWithNormalInjectionOnly());

        /// <summary>
        /// Validates a token's start index, end index, and full scope list.
        /// Matches the pattern used in <see cref="GrammarTests"/>.
        /// </summary>
        private static void AssertTokenValuesAreEqual(IToken token, int startIndex, int endIndex, params string[] scopes)
        {
            Assert.AreEqual(startIndex, token.StartIndex, "Unexpected token startIndex");
            Assert.AreEqual(endIndex, token.EndIndex, "Unexpected token endIndex");
            Assert.AreEqual(scopes.Length, token.Scopes.Count, "Unexpected scope count");

            for (int i = 0; i < scopes.Length; i++)
            {
                Assert.AreEqual(scopes[i], token.Scopes[i], "Unexpected scope at index " + i);
            }
        }

        /// <summary>
        /// Validates that a contiguous range of tokens covers the entire line from index 0
        /// to <paramref name="lineLength"/> with no gaps or overlaps.
        /// </summary>
        private static void AssertAllTokensCoverLine(IToken[] tokens, int lineLength)
        {
            Assert.IsTrue(tokens.Length > 0, "Expected at least one token");
            Assert.AreEqual(0, tokens[0].StartIndex, "First token should start at 0");

            for (int i = 1; i < tokens.Length; i++)
            {
                Assert.AreEqual(tokens[i - 1].EndIndex, tokens[i].StartIndex,
                    "Gap between token {0} (end={1}) and token {2} (start={3})", i - 1, tokens[i - 1].EndIndex, i, tokens[i].StartIndex);
            }

            Assert.AreEqual(lineLength, tokens[^1].EndIndex, "Last token should end at line length");
        }

        #endregion Helper Infrastructure
    }
}