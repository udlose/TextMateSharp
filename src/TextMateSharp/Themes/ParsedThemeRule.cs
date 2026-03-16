using System;
using System.Collections.Generic;
using System.Text;

namespace TextMateSharp.Themes
{
    public class ParsedThemeRule
    {
        public string scope { get; private set; }
        public string name { get; private set; }
        public List<string> parentScopes { get; }
        public int index { get; private set; }

        // -1 if not set.An or mask of `FontStyle` otherwise.
        public FontStyle fontStyle { get; private set; }
        public string foreground { get; private set; }
        public string background { get; private set; }

        public ParsedThemeRule(string name, string scope, List<string> parentScopes, int index, FontStyle fontStyle, string foreground, string background)
        {
            this.name = name;
            this.scope = scope;
            this.parentScopes = parentScopes;
            this.index = index;
            this.fontStyle = fontStyle;
            this.foreground = foreground;
            this.background = background;
        }

        public override int GetHashCode()
        {
            const int primeFactor = 31; // Common prime factor for multiply-accumulate hash code
            const int seed = 17; // Common seed for hash code computation (different from primeFactor to reduce collisions)
            unchecked
            {
                int result = seed + (int)fontStyle;
                result = (primeFactor * result) + index;
                result = (primeFactor * result) + (background != null ? StringComparer.Ordinal.GetHashCode(background) : 0);
                result = (primeFactor * result) + (foreground != null ? StringComparer.Ordinal.GetHashCode(foreground) : 0);
                result = (primeFactor * result) + (name != null ? StringComparer.Ordinal.GetHashCode(name) : 0);
                result = (primeFactor * result) + ParentScopesGetHashCode(parentScopes);
                result = (primeFactor * result) + (scope != null ? StringComparer.Ordinal.GetHashCode(scope) : 0);
                return result;
            }
        }

        public override string ToString()
        {
            const int initialSize = 256;
            var sb = new StringBuilder("ParsedThemeRule [name=", initialSize);
            sb.Append(name);
            sb.Append(", scope=");
            sb.Append(scope);
            sb.Append(", parentScopes=");
            if (parentScopes != null)
            {
                for (int i = 0; i < parentScopes.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(parentScopes[i]);
                }
            }
            sb.Append(", index=");
            sb.Append(index.ToString());
            sb.Append(", fontStyle=");
            sb.Append(fontStyle.ToString());
            sb.Append(", foreground=");
            sb.Append(foreground);
            sb.Append(", background=");
            sb.Append(background);
            sb.Append(']');
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (!(obj is ParsedThemeRule other))
                return false;

            return fontStyle == other.fontStyle &&
                index == other.index &&
                string.Equals(background, other.background, StringComparison.Ordinal) &&
                string.Equals(foreground, other.foreground, StringComparison.Ordinal) &&
                string.Equals(name, other.name, StringComparison.Ordinal) &&
                ParentScopesEquals(parentScopes, other.parentScopes) &&
                string.Equals(scope, other.scope, StringComparison.Ordinal);
        }

        /// <summary>
        /// Deep element-by-element equality for parentScopes lists using ordinal comparison.
        /// Replaces <c>Enumerable.SequenceEqual</c> to avoid enumerator allocation.
        /// </summary>
        internal static bool ParentScopesEquals(List<string> a, List<string> b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            int count = a.Count;
            if (count != b.Count)
                return false;

            for (int i = 0; i < count; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Computes a hash code for a parentScopes list using the same prime-factor
        /// mixing strategy as <see cref="GetHashCode"/>.
        /// </summary>
        internal static int ParentScopesGetHashCode(List<string> scopes)
        {
            if (scopes is null)
                return 0;

            unchecked
            {
                int hash = 17;
                for (int i = 0, count = scopes.Count; i < count; i++)
                {
                    string s = scopes[i];
                    hash = (hash * 31) + (s != null ? StringComparer.Ordinal.GetHashCode(s) : 0);
                }
                return hash;
            }
        }
    }
}