using System.Collections.Generic;

namespace TextMateSharp.Themes
{
    /// <summary>
    /// Provides value-based equality comparison for <see cref="ThemeTrieElementRule"/>
    /// instances. Compares scopeDepth, fontStyle, foreground, background, and performs
    /// deep element-by-element comparison of parentScopes.
    /// <para>
    /// This comparer exists because <see cref="ThemeTrieElementRule"/> is not sealed
    /// (it is part of the public API), so implementing <c>IEquatable&lt;T&gt;</c> directly
    /// on the class would violate SonarQube rule S4035. The comparer provides the same
    /// typed, boxing-free equality contract without requiring the class to be sealed.
    /// </para>
    /// </summary>
    public sealed class ThemeTrieElementRuleEqualityComparer : IEqualityComparer<ThemeTrieElementRule>
    {
        /// <summary>
        /// Singleton instance. Use this instead of allocating new comparers.
        /// </summary>
        public static readonly ThemeTrieElementRuleEqualityComparer Default = new ThemeTrieElementRuleEqualityComparer();

        /// <summary>
        /// Determines whether two ThemeTrieElementRule instances are equal by comparing their relevant properties.
        /// </summary>
        /// <remarks>Equality is determined by comparing the scope depth, font style, foreground,
        /// background, and parent scopes of each instance. Reference equality is checked first, followed by property
        /// comparisons. This method can be used to support collections or algorithms that require custom equality logic
        /// for ThemeTrieElementRule objects.</remarks>
        /// <param name="x">The first ThemeTrieElementRule instance to compare. Can be null.</param>
        /// <param name="y">The second ThemeTrieElementRule instance to compare. Can be null.</param>
        /// <returns>true if the specified ThemeTrieElementRule instances are equal; otherwise, false.</returns>
        public bool Equals(ThemeTrieElementRule x, ThemeTrieElementRule y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.name == y.name &&
                x.scopeDepth == y.scopeDepth &&
                x.fontStyle == y.fontStyle &&
                x.foreground == y.foreground &&
                x.background == y.background &&
                ThemeTrieElementRule.ParentScopesEquals(x.parentScopes, y.parentScopes);
        }

        /// <summary>
        /// Calculates a hash code for the specified ThemeTrieElementRule object.
        /// </summary>
        /// <remarks>This method delegates to the GetHashCode method of the ThemeTrieElementRule class to
        /// ensure consistent hash codes between the comparer and the override.</remarks>
        /// <param name="obj">The ThemeTrieElementRule object for which to generate a hash code. If the object is null, a hash code of 0
        /// is returned.</param>
        /// <returns>An integer representing the hash code of the specified ThemeTrieElementRule object.</returns>
        public int GetHashCode(ThemeTrieElementRule obj)
        {
            if (obj is null)
                return 0;

            // Delegates to the same prime-factor mixing logic as ThemeTrieElementRule.GetHashCode()
            // to ensure hash consistency between the comparer and the override.
            return obj.GetHashCode();
        }
    }
}