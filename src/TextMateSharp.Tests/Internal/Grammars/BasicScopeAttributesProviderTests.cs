using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using TextMateSharp.Internal.Grammars;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Internal.Grammars
{
    [TestFixture]
    public class BasicScopeAttributesProviderTests
    {
        [Test]
        public void Constructor_GetDefaultAttributes_ReturnsCachedDefaultAttributes_AndReadsThemeDefaultsOnce()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .Setup(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule));

            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 7,
                themeProvider: themeProvider.Object,
                embeddedLanguages: null);

            // Act
            BasicScopeAttributes first = provider.GetDefaultAttributes();
            BasicScopeAttributes second = provider.GetDefaultAttributes();

            // Assert
            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
            themeProvider.Verify(p => p.GetDefaults(), Times.Once);
            themeProvider.Verify(p => p.ThemeMatch(It.IsAny<string[]>()), Times.Never);
        }

        [Test]
        public void GetBasicScopeAttributes_NullScope_ReturnsSharedInstance_AndDoesNotQueryThemeProvider()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .Setup(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule));

            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 123,
                themeProvider: themeProvider.Object,
                embeddedLanguages: null);

            // Act
            BasicScopeAttributes first = provider.GetBasicScopeAttributes(null);
            BasicScopeAttributes second = provider.GetBasicScopeAttributes(null);

            // Assert
            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
            themeProvider.Verify(p => p.GetDefaults(), Times.Once);
            themeProvider.Verify(p => p.ThemeMatch(It.IsAny<string[]>()), Times.Never);
        }

        [Test]
        public void GetBasicScopeAttributes_SameScope_ReturnsCachedInstance()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .Setup(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule));
            themeProvider
                .Setup(provider => provider.ThemeMatch(It.IsAny<string[]>()))
                .Returns(new List<ThemeTrieElementRule>());

            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 1,
                themeProvider: themeProvider.Object,
                embeddedLanguages: null);

            // Act
            BasicScopeAttributes first = provider.GetBasicScopeAttributes("comment.line.double-slash");
            BasicScopeAttributes second = provider.GetBasicScopeAttributes("comment.line.double-slash");

            // Assert
            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
            themeProvider.Verify(
                p => p.ThemeMatch(
                    It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "comment.line.double-slash")),
                Times.Once);
        }

        [Test]
        public void GetBasicScopeAttributes_DifferentScopes_ReturnDifferentInstances()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .Setup(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule));
            themeProvider
                .Setup(provider => provider.ThemeMatch(It.IsAny<string[]>()))
                .Returns(new List<ThemeTrieElementRule>());

            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 1,
                themeProvider: themeProvider.Object,
                embeddedLanguages: null);

            // Act
            BasicScopeAttributes first = provider.GetBasicScopeAttributes("comment.line.double-slash");
            BasicScopeAttributes second = provider.GetBasicScopeAttributes("string.quoted.double");

            // Assert
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreNotSame(first, second);

            themeProvider.Verify(
                p => p.ThemeMatch(
                    It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "comment.line.double-slash")),
                Times.Once);

            themeProvider.Verify(
                p => p.ThemeMatch(
                    It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "string.quoted.double")),
                Times.Once);
        }

        [Test]
        public void GetBasicScopeAttributes_PassesSingleRequestedScope_ToThemeProvider()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .Setup(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule));
            themeProvider
                .Setup(provider => provider.ThemeMatch(It.IsAny<string[]>()))
                .Returns(new List<ThemeTrieElementRule>());

            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 10,
                themeProvider: themeProvider.Object,
                embeddedLanguages: null);

            // Act
            BasicScopeAttributes result = provider.GetBasicScopeAttributes("entity.name.function");

            // Assert
            Assert.IsNotNull(result);
            themeProvider.Verify(
                p => p.ThemeMatch(
                    It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "entity.name.function")),
                Times.Once);
        }

        [Test]
        public void OnDidChangeTheme_RebuildsDefaultAttributes()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .SetupSequence(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule))
                .Returns(default(ThemeTrieElementRule));

            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 55,
                themeProvider: themeProvider.Object,
                embeddedLanguages: null);

            BasicScopeAttributes beforeThemeChange = provider.GetDefaultAttributes();

            // Act
            provider.OnDidChangeTheme();
            BasicScopeAttributes afterThemeChange = provider.GetDefaultAttributes();

            // Assert
            Assert.IsNotNull(beforeThemeChange);
            Assert.IsNotNull(afterThemeChange);
            Assert.AreNotSame(beforeThemeChange, afterThemeChange);
            themeProvider.Verify(p => p.GetDefaults(), Times.Exactly(2));
        }

        [Test]
        public void OnDidChangeTheme_ClearsCachedScopeAttributes()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .SetupSequence(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule))
                .Returns(default(ThemeTrieElementRule));
            themeProvider
                .SetupSequence(provider => provider.ThemeMatch(It.IsAny<string[]>()))
                .Returns(new List<ThemeTrieElementRule>())
                .Returns(new List<ThemeTrieElementRule>());

            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 1,
                themeProvider: themeProvider.Object,
                embeddedLanguages: null);

            BasicScopeAttributes beforeThemeChange = provider.GetBasicScopeAttributes("string.quoted.double");

            // Act
            provider.OnDidChangeTheme();
            BasicScopeAttributes afterThemeChange = provider.GetBasicScopeAttributes("string.quoted.double");

            // Assert
            Assert.IsNotNull(beforeThemeChange);
            Assert.IsNotNull(afterThemeChange);
            Assert.AreNotSame(beforeThemeChange, afterThemeChange);
            themeProvider.Verify(p => p.GetDefaults(), Times.Exactly(2));
            themeProvider.Verify(
                p => p.ThemeMatch(
                    It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "string.quoted.double")),
                Times.Exactly(2));
        }

        [Test]
        public void Constructor_DoesNotQueryThemeMatch()
        {
            // Arrange
            Mock<IThemeProvider> themeProvider = new Mock<IThemeProvider>(MockBehavior.Strict);
            themeProvider
                .Setup(provider => provider.GetDefaults())
                .Returns(default(ThemeTrieElementRule));

            // Act
            BasicScopeAttributesProvider provider = new BasicScopeAttributesProvider(
                initialLanguage: 42,
                themeProvider: themeProvider.Object,
                embeddedLanguages: new Dictionary<string, int>
                {
                    ["source.js"] = 5
                });

            // Assert
            Assert.IsNotNull(provider);
            themeProvider.Verify(p => p.GetDefaults(), Times.Once);
            themeProvider.Verify(p => p.ThemeMatch(It.IsAny<string[]>()), Times.Never);
        }
    }
}
