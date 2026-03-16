using System.Collections.Generic;

namespace TextMateSharp.Themes
{
    /// <summary>
    /// Provides value-based equality comparison for <see cref="ParsedThemeRule"/>
    /// instances. Compares fontStyle, index, background, foreground, name,
    /// parentScopes (deep element-by-element), and scope - all using ordinal
    /// string comparison to match <see cref="ParsedThemeRule.GetHashCode"/>.
    /// <para>
    /// This comparer exists because <see cref="ParsedThemeRule"/> is not sealed
    /// (the upstream Java class is not <c>final</c>), so implementing
    /// <c>IEquatable&lt;T&gt;</c> directly on the class would violate SonarQube
    /// rule S4035. The comparer provides the same typed, boxing-free equality
    /// contract without requiring the class to be sealed.
    /// </para>
    /// <para>
    /// Note: <c>name</c> is included in the equality contract as a known,
    /// intentional divergence from the upstream eclipse-tm4e Java source,
    /// which does not have a <c>name</c> field.
    /// </para>
    /// </summary>
    public sealed class ParsedThemeRuleEqualityComparer : IEqualityComparer<ParsedThemeRule>
    {
        /// <summary>
        /// Singleton instance. Use this instead of allocating new comparers.
        /// </summary>
        public static readonly ParsedThemeRuleEqualityComparer Default = new ParsedThemeRuleEqualityComparer();

        public bool Equals(ParsedThemeRule x, ParsedThemeRule y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.fontStyle == y.fontStyle &&
                   x.index == y.index &&
                   string.Equals(x.background, y.background, System.StringComparison.Ordinal) &&
                   string.Equals(x.foreground, y.foreground, System.StringComparison.Ordinal) &&
                   string.Equals(x.name, y.name, System.StringComparison.Ordinal) &&
                   ParsedThemeRule.ParentScopesEquals(x.parentScopes, y.parentScopes) &&
                   string.Equals(x.scope, y.scope, System.StringComparison.Ordinal);
        }

        public int GetHashCode(ParsedThemeRule obj)
        {
            if (obj is null)
            {
                return 0;
            }

            // Delegates to the same prime-factor mixing logic as ParsedThemeRule.GetHashCode()
            // to ensure hash consistency between the comparer and the override.
            return obj.GetHashCode();
        }
    }
}