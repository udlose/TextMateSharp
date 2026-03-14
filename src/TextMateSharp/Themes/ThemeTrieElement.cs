using System;
using System.Collections.Generic;

using TextMateSharp.Internal.Utils;

namespace TextMateSharp.Themes
{
    public sealed class ThemeTrieElement : IEquatable<ThemeTrieElement>
    {

        // _themeTrieElementBrand: void;


        /// <summary>
        /// Readonly references to mutable state used by this trie element.
        /// <para>
        /// Note: <see cref="mainRule"/>, <see cref="rulesWithParentScopes"/> and <see cref="children"/>
        /// are not deeply immutable. The fields themselves are readonly (they cannot be
        /// reassigned), but the referenced <see cref="ThemeTrieElementRule"/> instances and
        /// the contents of the collections are mutated by methods such as Insert and DoInsertHere
        /// (for example via <c>AcceptOverwrite</c> on <see cref="ThemeTrieElementRule"/>).
        /// </para>
        /// </summary>
        private readonly ThemeTrieElementRule mainRule;
        private readonly List<ThemeTrieElementRule> rulesWithParentScopes;
        private readonly Dictionary<string /* segment */, ThemeTrieElement> children;

        // Static sort comparison to avoid delegate allocation per Sort call.
        // The lambda captures nothing, so a static field holds the single delegate instance.
        private static readonly Comparison<ThemeTrieElementRule> _comparisonBySpecificity =
            (a, b) => CmpBySpecificity(a, b);

        public ThemeTrieElement(ThemeTrieElementRule mainRule) :
            this(mainRule, new List<ThemeTrieElementRule>(), new Dictionary<string /* segment */, ThemeTrieElement>())
        {
        }

        public ThemeTrieElement(ThemeTrieElementRule mainRule, List<ThemeTrieElementRule> rulesWithParentScopes) :
                this(mainRule, rulesWithParentScopes, new Dictionary<string /* segment */, ThemeTrieElement>())
        {

        }

        public ThemeTrieElement(ThemeTrieElementRule mainRule, List<ThemeTrieElementRule> rulesWithParentScopes,
                Dictionary<string /* segment */, ThemeTrieElement> children)
        {
            this.mainRule = mainRule;
            this.rulesWithParentScopes = rulesWithParentScopes;
            this.children = children;
        }

        private static List<ThemeTrieElementRule> SortBySpecificity(List<ThemeTrieElementRule> arr)
        {
            if (arr.Count == 1)
            {
                return arr;
            }
            arr.Sort(_comparisonBySpecificity);
            return arr;
        }

        private static int CmpBySpecificity(ThemeTrieElementRule a, ThemeTrieElementRule b)
        {
            if (a.scopeDepth == b.scopeDepth)
            {
                List<string> aParentScopes = a.parentScopes;
                List<string> bParentScopes = b.parentScopes;
                int aParentScopesLen = aParentScopes == null ? 0 : aParentScopes.Count;
                int bParentScopesLen = bParentScopes == null ? 0 : bParentScopes.Count;
                if (aParentScopesLen == bParentScopesLen)
                {
                    for (int i = 0; i < aParentScopesLen; i++)
                    {
                        int aLen = aParentScopes[i].Length;
                        int bLen = bParentScopes[i].Length;
                        if (aLen != bLen)
                        {
                            return bLen - aLen;
                        }
                    }
                }
                return bParentScopesLen - aParentScopesLen;
            }
            return b.scopeDepth - a.scopeDepth;
        }

        public List<ThemeTrieElementRule> Match(string scope)
        {
            List<ThemeTrieElementRule> arr;
            if ("".Equals(scope))
            {
                arr = new List<ThemeTrieElementRule>(rulesWithParentScopes.Count + 1);
                arr.Add(this.mainRule);
                arr.AddRange(this.rulesWithParentScopes);
                return ThemeTrieElement.SortBySpecificity(arr);
            }

            int dotIndex = scope.IndexOf('.');
            string head;
            string tail;
            if (dotIndex == -1)
            {
                head = scope;
                tail = "";
            }
            else
            {
                head = scope.SubstringAtIndexes(0, dotIndex);
                tail = scope.Substring(dotIndex + 1);
            }

            if (children.TryGetValue(head, out ThemeTrieElement value))
            {
                return value.Match(tail);
            }

            arr = new List<ThemeTrieElementRule>(rulesWithParentScopes.Count + 1);
            if (this.mainRule.foreground > 0)
                arr.Add(this.mainRule);
            arr.AddRange(this.rulesWithParentScopes);
            return ThemeTrieElement.SortBySpecificity(arr);
        }

        public void Insert(string name, int scopeDepth, string scope, List<string> parentScopes, FontStyle fontStyle, int foreground,
                int background)
        {
            if ("".Equals(scope))
            {
                this.DoInsertHere(name, scopeDepth, parentScopes, fontStyle, foreground, background);
                return;
            }

            int dotIndex = scope.IndexOf('.');
            string head;
            string tail;
            if (dotIndex == -1)
            {
                head = scope;
                tail = "";
            }
            else
            {
                head = scope.SubstringAtIndexes(0, dotIndex);
                tail = scope.Substring(dotIndex + 1);
            }

            ThemeTrieElement child;
            if (children.TryGetValue(head, out ThemeTrieElement value))
            {
                child = value;
            }
            else
            {
                child = new ThemeTrieElement(this.mainRule.Clone(),
                        ThemeTrieElementRule.CloneArr(this.rulesWithParentScopes));
                this.children[head] = child;
            }

            child.Insert(name, scopeDepth + 1, tail, parentScopes, fontStyle, foreground, background);
        }

        private void DoInsertHere(string name, int scopeDepth, List<string> parentScopes, FontStyle fontStyle, int foreground,
                int background)
        {

            if (parentScopes == null)
            {
                // Merge into the main rule
                this.mainRule.AcceptOverwrite(name, scopeDepth, fontStyle, foreground, background);
                return;
            }

            // Try to merge into existing rule
            foreach (ThemeTrieElementRule rule in this.rulesWithParentScopes)
            {
                if (StringUtils.StrArrCmp(rule.parentScopes, parentScopes) == 0)
                {
                    // bingo! => we get to merge this into an existing one
                    rule.AcceptOverwrite(rule.name, scopeDepth, fontStyle, foreground, background);
                    return;
                }
            }

            // Must add a new rule

            // Inherit from main rule
            if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(mainRule.name))
            {
                name = mainRule.name;
            }
            if (fontStyle == FontStyle.NotSet)
            {
                fontStyle = this.mainRule.fontStyle;
            }
            if (foreground == 0)
            {
                foreground = this.mainRule.foreground;
            }
            if (background == 0)
            {
                background = this.mainRule.background;
            }

            this.rulesWithParentScopes.Add(
                new ThemeTrieElementRule(name, scopeDepth, parentScopes, fontStyle, foreground, background));
        }

        /// <summary>
        /// Calculates a hash code for the current instance, suitable for use in hash-based collections.
        /// </summary>
        /// <remarks>The hash code is computed based on the main rule, the list of rules with parent
        /// scopes, and the children dictionary. Because these collections may be mutated after construction, the hash
        /// code is not cached and may change if the instance is modified. Use caution when relying on hash codes for
        /// mutable objects.</remarks>
        /// <returns>A 32-bit signed integer that represents the hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            // Prime-factor mixing for better hash distribution.
            // Cannot cache: children dictionary and rulesWithParentScopes list
            // are mutated by Insert/DoInsertHere, and mainRule fields are mutated
            // by AcceptOverwrite.
            unchecked
            {
                int hash = 17;
                // Deep hash of mainRule using its IEquatable-based GetHashCode
                hash = (hash * 31) + (mainRule?.GetHashCode() ?? 0);
                // Deep hash of rulesWithParentScopes list contents
                hash = (hash * 31) + RuleListGetHashCode(rulesWithParentScopes);
                // Deep hash of children dictionary contents
                hash = (hash * 31) + ChildrenGetHashCode(children);
                return hash;
            }
        }

        /// <summary>
        /// Calculates a hash code for the specified list of theme rules.
        /// </summary>
        /// <remarks>The hash code is computed by combining the hash codes of individual rules using
        /// unchecked arithmetic. This approach prevents overflow exceptions and ensures efficient hash code generation
        /// for use in hash-based collections.</remarks>
        /// <param name="rules">The list of theme rules for which to compute the hash code. If null, a hash code of 0 is returned.</param>
        /// <returns>An integer representing the combined hash code of the provided theme rules.</returns>
        private static int RuleListGetHashCode(List<ThemeTrieElementRule> rules)
        {
            if (rules == null)
                return 0;

            unchecked
            {
                int hash = 17;
                for (int i = 0, count = rules.Count; i < count; i++)
                {
                    ThemeTrieElementRule rule = rules[i];
                    hash = (hash * 31) + (rule?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }

        /// <summary>
        /// Computes a value-based hash code for the children dictionary by combining
        /// hash codes of all key-value pairs. Uses additive accumulation for pair combination so that
        /// hash is order-independent (dictionary enumeration order is not guaranteed).
        /// </summary>
        /// <param name="dict">The dictionary of children for which to compute the hash code. If null, a hash code of 0 is returned.</param>
        /// <returns>An integer representing the combined hash code of the provided dictionary.</returns>
        private static int ChildrenGetHashCode(Dictionary<string, ThemeTrieElement> dict)
        {
            if (dict == null)
                return 0;

            unchecked
            {
                // addition is commutative - order-independent accumulation for dictionaries
                int hash = 0;
                foreach (KeyValuePair<string, ThemeTrieElement> kvp in dict)
                {
                    int pairHash = kvp.Key?.GetHashCode() ?? 0;
                    pairHash = (pairHash * 31) + (kvp.Value?.GetHashCode() ?? 0);

                    // Add pairHash to the total hash. Using addition instead of XOR to
                    // reduce hash collisions in cases where keys and values have similar hash codes.
                    hash += pairHash;
                }
                return hash;
            }
        }

        /// <summary>
        /// Single entry point for all equality checks. Contains the ReferenceEquals
        /// and null guards so they exist in exactly one place.
        /// </summary>
        private static bool Equals(ThemeTrieElement a, ThemeTrieElement b)
        {
            // Use ReferenceEquals to avoid infinite recursion through operator ==
            if (ReferenceEquals(a, b))
                return true;

            // Use is null checks to guard against infinite recursion through operator ==
            if (a is null || b is null)
                return false;

            // mainRule: uses ThemeTrieElementRuleEqualityComparer for value-based equality of mainRule properties
            if (!ThemeTrieElementRuleEqualityComparer.Default.Equals(a.mainRule, b.mainRule))
                return false;

            // rulesWithParentScopes: deep element-by-element comparison
            if (!RuleListEquals(a.rulesWithParentScopes, b.rulesWithParentScopes))
                return false;

            // children: deep dictionary comparison (keys + recursive value equality)
            if (!ChildrenEquals(a.children, b.children))
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether the specified ThemeTrieElement is equal to the current ThemeTrieElement.
        /// </summary>
        /// <param name="other">The ThemeTrieElement to compare with the current instance.</param>
        /// <returns>true if the specified ThemeTrieElement is equal to the current ThemeTrieElement; otherwise, false.</returns>
        public bool Equals(ThemeTrieElement other)
        {
            return Equals(this, other);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current ThemeTrieElement instance.
        /// </summary>
        /// <remarks>This method overrides Object.Equals to provide a type-specific equality comparison
        /// for ThemeTrieElement instances. Use this method to check for value equality rather than reference
        /// equality.</remarks>
        /// <param name="obj">The object to compare with the current instance. This value can be null.</param>
        /// <returns>true if the specified object is a ThemeTrieElement and is equal to the current instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ThemeTrieElement other)
                return Equals(this, other);

            return false;
        }

        /// <summary>
        /// Determines whether two ThemeTrieElement instances are equal.
        /// </summary>
        /// <remarks>This operator uses the Equals(object) method to evaluate equality. When implementing
        /// equality operators, it is important to override both Equals(object) and GetHashCode() to ensure consistent
        /// behavior.</remarks>
        /// <param name="left">The first ThemeTrieElement instance to compare for equality.</param>
        /// <param name="right">The second ThemeTrieElement instance to compare for equality.</param>
        /// <returns>true if the two ThemeTrieElement instances are equal; otherwise, false.</returns>
        public static bool operator ==(ThemeTrieElement left, ThemeTrieElement right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether two ThemeTrieElement instances are not equal.
        /// </summary>
        /// <remarks>This operator uses the Equals method to perform the comparison.</remarks>
        /// <param name="left">The first ThemeTrieElement instance to compare.</param>
        /// <param name="right">The second ThemeTrieElement instance to compare.</param>
        /// <returns>true if the two instances are not equal; otherwise, false.</returns>
        public static bool operator !=(ThemeTrieElement left, ThemeTrieElement right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Deep element-by-element equality for lists of <see cref="ThemeTrieElementRule"/>.
        /// </summary>
        /// <param name="a">The first list of theme rules to compare.</param>
        /// <param name="b">The second list of theme rules to compare.</param>
        /// <returns>true if the two lists are equal; otherwise, false.</returns>
        private static bool RuleListEquals(List<ThemeTrieElementRule> a, List<ThemeTrieElementRule> b)
        {
            // Use ReferenceEquals to avoid infinite recursion through operator ==
            if (ReferenceEquals(a, b))
                return true;

            // Use is null checks to guard against infinite recursion through operator ==
            if (a is null || b is null)
                return false;

            int count = a.Count;
            if (count != b.Count)
                return false;

            for (int i = 0; i < count; i++)
            {
                // Uses ThemeTrieElementRuleEqualityComparer for value-based equality of ThemeTrieElementRule properties
                if (!ThemeTrieElementRuleEqualityComparer.Default.Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Deep equality for children dictionaries. Compares key sets and
        /// recursively compares values via the private static Equals method.
        /// </summary>
        /// <param name="a">The first dictionary of theme elements to compare.</param>
        /// <param name="b">The second dictionary of theme elements to compare.</param>
        /// <returns>true if the two dictionaries are equal; otherwise, false.</returns>
        private static bool ChildrenEquals(Dictionary<string, ThemeTrieElement> a, Dictionary<string, ThemeTrieElement> b)
        {
            // Use ReferenceEquals to avoid infinite recursion through operator ==
            if (ReferenceEquals(a, b))
                return true;

            // Use is null checks to guard against infinite recursion through operator ==
            if (a is null || b is null)
                return false;

            if (a.Count != b.Count)
                return false;

            foreach (KeyValuePair<string, ThemeTrieElement> kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out ThemeTrieElement bValue))
                    return false;

                // Uses ThemeTrieElement operator == which dispatches
                // to the private static Equals(ThemeTrieElement, ThemeTrieElement) -
                // recursive through the trie
                if (kvp.Value != bValue)
                    return false;
            }

            return true;
        }
    }
}