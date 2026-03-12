using System;
using System.Collections.Generic;

namespace TextMateSharp.Themes
{
    public class ThemeTrieElementRule
    {
        // _themeTrieElementRuleBrand: void;

        public int scopeDepth;
        public readonly List<string> parentScopes;
        public FontStyle fontStyle;
        public int foreground;
        public int background;
        public string name;

        public ThemeTrieElementRule(string name, int scopeDepth, List<string> parentScopes, FontStyle fontStyle, int foreground,
                int background)
        {
            this.name = name;
            this.scopeDepth = scopeDepth;
            this.parentScopes = parentScopes;
            this.fontStyle = fontStyle;
            this.foreground = foreground;
            this.background = background;
        }

        public ThemeTrieElementRule Clone()
        {
            return new ThemeTrieElementRule(this.name, this.scopeDepth, this.parentScopes, this.fontStyle, this.foreground,
                    this.background);
        }

        public static List<ThemeTrieElementRule> CloneArr(List<ThemeTrieElementRule> arr)
        {
            List<ThemeTrieElementRule> r = new List<ThemeTrieElementRule>(arr.Count);
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                r.Add(arr[i].Clone());
            }
            return r;
        }

        public void AcceptOverwrite(string name, int scopeDepth, FontStyle fontStyle, int foreground, int background)
        {
            if (this.scopeDepth > scopeDepth)
            {
                // console.log('how did this happen?');
            }
            else
            {
                this.scopeDepth = scopeDepth;
            }
            // console.log('TODO -> my depth: ' + this.scopeDepth + ', overwriting depth: ' + scopeDepth);
            if (fontStyle != FontStyle.NotSet)
            {
                this.fontStyle = fontStyle;
            }
            if (foreground != 0)
            {
                this.foreground = foreground;
            }
            if (background != 0)
            {
                this.background = background;
            }
            if (!string.IsNullOrEmpty(name))
            {
                this.name = name;
            }
        }

        /// <summary>
        /// Calculates a hash code for the current instance, which can be used for efficient storage and retrieval in
        /// hash-based collections.
        /// </summary>
        /// <remarks>The hash code is based on the values of the instance's fields, including mutable
        /// fields. If any field is modified after the instance is added to a hash-based collection, the hash code may
        /// change, which can affect collection behavior. It is recommended to avoid modifying fields used in the hash
        /// code while the instance is stored in such collections.</remarks>
        /// <returns>An integer that represents the hash code for the current instance. Equal instances produce the same hash
        /// code.</returns>
        public override int GetHashCode()
        {
            // Cannot cache: fields are mutable via AcceptOverwrite
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + scopeDepth;
                hash = (hash * 31) + (int)fontStyle;
                hash = (hash * 31) + foreground;
                hash = (hash * 31) + background;
                // Deep hash of parentScopes list contents instead of identity hash.
                // The original code called parentScopes.GetHashCode() which returns
                // the object identity hash - useless for value-based equality.
                hash = (hash * 31) + ParentScopesGetHashCode(parentScopes);
                return hash;
            }
        }

        /// <summary>
        /// Computes a value-based hash code for a list of strings by iterating
        /// each element. Returns 0 for null lists, matching the null-handling
        /// semantics in <see cref="ParentScopesEquals"/>.
        /// </summary>
        /// <param name="scopes">The list of strings to compute the hash code for.</param>
        /// <returns>The computed hash code for the list of strings.</returns>
        private static int ParentScopesGetHashCode(List<string> scopes)
        {
            if (scopes == null)
                return 0;

            unchecked
            {
                int hash = 17;
                for (int i = 0, count = scopes.Count; i < count; i++)
                {
                    string s = scopes[i];
                    hash = (hash * 31) + (s?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current ThemeTrieElementRule instance.
        /// </summary>
        /// <remarks>Equality is determined by comparing the values of scopeDepth, fontStyle, foreground,
        /// background, and the parentScopes collection. If the specified object is not a ThemeTrieElementRule, the
        /// method returns false.</remarks>
        /// <param name="obj">The object to compare with the current instance. This value can be null.</param>
        /// <returns>true if the specified object is a ThemeTrieElementRule and its property values are equal to those of the
        /// current instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is ThemeTrieElementRule other)
            {
                return scopeDepth == other.scopeDepth &&
                    fontStyle == other.fontStyle &&
                    foreground == other.foreground &&
                    background == other.background &&
                    ParentScopesEquals(parentScopes, other.parentScopes);
            }

            return false;
        }

        /// <summary>
        /// Deep element-by-element equality for parentScopes lists.
        /// </summary>
        /// <param name="a">First list to compare.</param>
        /// <param name="b">Second list to compare.</param>
        /// <returns>True if the lists are equal, false otherwise.</returns>
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
    }
}