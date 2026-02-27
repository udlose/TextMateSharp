using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using TextMateSharp.Internal.Themes;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Themes;

[TestFixture]
public class ParsedThemeTests
{
    [Test]
    public void ParseInclude_SourceGetIncludeReturnsNull_ReturnsEmptyListAndSetsThemeIncludeToNull()
    {
        // Arrange
        Mock<IRawTheme> mockSource = new Mock<IRawTheme>();
        mockSource.Setup(s => s.GetInclude()).Returns((string)null);
        Mock<IRegistryOptions> mockRegistryOptions = new Mock<IRegistryOptions>();
        const int priority = 5;

        // Act
        List<ParsedThemeRule> result = ParsedTheme.ParseInclude(mockSource.Object, mockRegistryOptions.Object, priority, out IRawTheme themeInclude);

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.IsEmpty(result);
        Assert.IsNull(themeInclude);
        mockRegistryOptions.Verify(r => r.GetTheme(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ParseInclude_SourceGetIncludeReturnsEmpty_ReturnsEmptyListAndSetsThemeIncludeToNull()
    {
        // Arrange
        Mock<IRawTheme> mockSource = new Mock<IRawTheme>();
        mockSource.Setup(s => s.GetInclude()).Returns(string.Empty);
        Mock<IRegistryOptions> mockRegistryOptions = new Mock<IRegistryOptions>();
        const int priority = 10;

        // Act
        List<ParsedThemeRule> result = ParsedTheme.ParseInclude(mockSource.Object, mockRegistryOptions.Object, priority, out IRawTheme themeInclude);

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.IsEmpty(result);
        Assert.IsNull(themeInclude);
        mockRegistryOptions.Verify(r => r.GetTheme(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ParseInclude_GetThemeReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        const string includeString = "valid-include-name";
        Mock<IRawTheme> mockSource = new Mock<IRawTheme>();
        mockSource.Setup(s => s.GetInclude()).Returns(includeString);
        Mock<IRegistryOptions> mockRegistryOptions = new Mock<IRegistryOptions>();
        mockRegistryOptions.Setup(r => r.GetTheme(includeString)).Returns((IRawTheme)null);
        const int priority = 0;

        // Act
        List<ParsedThemeRule> result = ParsedTheme.ParseInclude(mockSource.Object, mockRegistryOptions.Object, priority, out IRawTheme themeInclude);

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.IsEmpty(result);
        Assert.IsNull(themeInclude);
        mockRegistryOptions.Verify(r => r.GetTheme(includeString), Times.Once);
    }

    [Test]
    public void ParseInclude_ValidIncludeAndTheme_ReturnsParseThemeResult()
    {
        // Arrange
        const string includeString = "dark-theme";
        Mock<IRawTheme> mockSource = new Mock<IRawTheme>();
        mockSource.Setup(s => s.GetInclude()).Returns(includeString);

        Mock<IRawTheme> mockIncludedTheme = new Mock<IRawTheme>();
        mockIncludedTheme.Setup(t => t.GetSettings()).Returns(new List<IRawThemeSetting>());
        mockIncludedTheme.Setup(t => t.GetTokenColors()).Returns(new List<IRawThemeSetting>());

        Mock<IRegistryOptions> mockRegistryOptions = new Mock<IRegistryOptions>();
        mockRegistryOptions.Setup(r => r.GetTheme(includeString)).Returns(mockIncludedTheme.Object);
        const int priority = 1;

        // Act
        List<ParsedThemeRule> result = ParsedTheme.ParseInclude(mockSource.Object, mockRegistryOptions.Object, priority, out IRawTheme themeInclude);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(mockIncludedTheme.Object, themeInclude);
        mockRegistryOptions.Verify(r => r.GetTheme(includeString), Times.Once);
    }

    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(int.MaxValue)]
    public void ParseInclude_VariousPriorityValues_PassesPriorityToParseTheme(int priority)
    {
        // Arrange
        const string includeString = "test-theme";
        const string expectedScope = "scope1";
        const string expectedForeground = "#123456";
        const int expectedRuleCount = 1;
        const int expectedRuleIndex = 0;

        Mock<IRawTheme> mockSource = new Mock<IRawTheme>();
        mockSource.Setup(s => s.GetInclude()).Returns(includeString);

        ThemeRaw includedTheme = new ThemeRaw
        {
            ["tokenColors"] = new List<IRawThemeSetting>
            {
                new ThemeRaw
                {
                    ["scope"] = expectedScope,
                    ["settings"] = new ThemeRaw
                    {
                        ["foreground"] = expectedForeground
                    }
                }
            }
        };

        Mock<IRegistryOptions> mockRegistryOptions = new Mock<IRegistryOptions>();
        mockRegistryOptions.Setup(r => r.GetTheme(includeString)).Returns(includedTheme);

        // Act
        List<ParsedThemeRule> result = ParsedTheme.ParseInclude(
            mockSource.Object,
            mockRegistryOptions.Object,
            priority,
            out IRawTheme themeInclude);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedRuleCount, result.Count);
        Assert.AreSame(includedTheme, themeInclude);
        mockRegistryOptions.Verify(r => r.GetTheme(includeString), Times.Once);

        ParsedThemeRule rule = result[0];
        Assert.AreEqual(expectedScope, rule.scope);
        Assert.AreEqual(expectedRuleIndex, rule.index);
        Assert.AreEqual(expectedForeground, rule.foreground);
        Assert.AreEqual(FontStyle.NotSet, rule.fontStyle);
    }

    [TestCase(" ")]
    [TestCase("  ")]
    [TestCase("\t")]
    [TestCase("\n")]
    [TestCase("\r\n")]
    public void ParseInclude_SourceGetIncludeReturnsWhitespace_ReturnsEmptyListAndSetsThemeIncludeToNull(string whitespace)
    {
        // Arrange
        Mock<IRawTheme> mockSource = new Mock<IRawTheme>();
        mockSource.Setup(s => s.GetInclude()).Returns(whitespace);
        Mock<IRegistryOptions> mockRegistryOptions = new Mock<IRegistryOptions>();
        mockRegistryOptions.Setup(r => r.GetTheme(whitespace)).Returns((IRawTheme)null);
        const int priority = 0;

        // Act
        List<ParsedThemeRule> result = ParsedTheme.ParseInclude(mockSource.Object, mockRegistryOptions.Object, priority, out IRawTheme themeInclude);

        // Assert
        Assert.IsNotNull(result);
        CollectionAssert.IsEmpty(result);
        Assert.IsNull(themeInclude);
        // Note: string.IsNullOrEmpty does NOT treat whitespace as empty, so GetTheme should be called
        mockRegistryOptions.Verify(r => r.GetTheme(whitespace), Times.Once);
    }

    [TestCase("theme-with-dashes")]
    [TestCase("theme_with_underscores")]
    [TestCase("theme.with.dots")]
    [TestCase("theme/with/slashes")]
    [TestCase("themeWithMixedCase")]
    [TestCase("very-long-theme-name-that-exceeds-normal-length-expectations-for-testing-purposes")]
    public void ParseInclude_VariousIncludeStringFormats_PassesCorrectlyToGetTheme(string includeString)
    {
        // Arrange
        Mock<IRawTheme> mockSource = new Mock<IRawTheme>();
        mockSource.Setup(s => s.GetInclude()).Returns(includeString);

        Mock<IRawTheme> mockIncludedTheme = new Mock<IRawTheme>();
        mockIncludedTheme.Setup(t => t.GetSettings()).Returns(new List<IRawThemeSetting>());
        mockIncludedTheme.Setup(t => t.GetTokenColors()).Returns(new List<IRawThemeSetting>());

        Mock<IRegistryOptions> mockRegistryOptions = new Mock<IRegistryOptions>();
        mockRegistryOptions.Setup(r => r.GetTheme(includeString)).Returns(mockIncludedTheme.Object);
        const int priority = 0;

        // Act
        List<ParsedThemeRule> result = ParsedTheme.ParseInclude(mockSource.Object, mockRegistryOptions.Object, priority, out IRawTheme themeInclude);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(mockIncludedTheme.Object, themeInclude);
        mockRegistryOptions.Verify(r => r.GetTheme(includeString), Times.Once);
    }
}