using System;
using System.Collections.Generic;

using TextMateSharp.Internal.Utils;

namespace TextMateSharp.Themes
{
    public sealed class ThemeTrieElement : IEquatable<ThemeTrieElement>
    {

        // _themeTrieElementBrand: void;

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
                hash = (hash * 31) + ((mainRule?.GetHashCode()) ?? 0);
                // Deep hash of rulesWithParentScopes list contents
                hash = (hash * 31) + RuleListGetHashCode(rulesWithParentScopes);
                // Deep hash of children dictionary contents
                hash = (hash * 31) + ChildrenGetHashCode(children);
                return hash;
            }
        }

        /// <summary>
        /// Computes a value-based hash code for a list of <see cref="ThemeTrieElementRule"/>
        /// by iterating each element. The original code called List.GetHashCode() which
        /// returns the object identity hash - useless for value-based equality.
        /// </summary>
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
        /// hash codes of all key-value pairs. Uses XOR for pair combination so that
        /// hash is order-independent (dictionary enumeration order is not guaranteed).
        /// </summary>
        private static int ChildrenGetHashCode(Dictionary<string, ThemeTrieElement> dict)
        {
            if (dict == null)
                return 0;

            unchecked
            {
                // XOR is commutative - order-independent accumulation for dictionaries
                int hash = 0;
                foreach (KeyValuePair<string, ThemeTrieElement> kvp in dict)
                {
                    int pairHash = kvp.Key?.GetHashCode() ?? 0;
                    pairHash = (pairHash * 31) + (kvp.Value?.GetHashCode() ?? 0);
                    hash ^= pairHash;
                }
                return hash;
            }
        }

        /// <summary>
        /// Single entry point for all equality checks. Contains the ReferenceEquals
        /// and null guards so they exist in exactly one place - matching the
        /// StateStack/AttributedScopeStack convention.
        /// </summary>
        private static bool Equals(ThemeTrieElement a, ThemeTrieElement b)
        {
            // Use ReferenceEquals to avoid infinite recursion through operator ==
            if (ReferenceEquals(a, b))
                return true;

            // Use is null checks to guard against infinite recursion through operator ==
            if (a is null || b is null)
                return false;

            // mainRule: uses ThemeTrieElementRule private static Equals via operator ==
            if (a.mainRule != b.mainRule)
                return false;

            // rulesWithParentScopes: deep element-by-element comparison
            if (!RuleListEquals(a.rulesWithParentScopes, b.rulesWithParentScopes))
                return false;

            // children: deep dictionary comparison (keys + recursive value equality)
            if (!ChildrenEquals(a.children, b.children))
                return false;

            return true;
        }

        public bool Equals(ThemeTrieElement other)
        {
            return Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            if (obj is ThemeTrieElement other)
                return Equals(this, other);

            return false;
        }

        public static bool operator ==(ThemeTrieElement left, ThemeTrieElement right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ThemeTrieElement left, ThemeTrieElement right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Deep element-by-element equality for lists of <see cref="ThemeTrieElementRule"/>.
        /// The original code used Object.Equals which performed reference equality
        /// on the List object - never a deep comparison.
        /// </summary>
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
                // Uses ThemeTrieElementRule operator == which dispatches
                // to its private static Equals(ThemeTrieElementRule, ThemeTrieElementRule)
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Deep equality for children dictionaries. Compares key sets and
        /// recursively compares values via the private static Equals method.
        /// The original code used Object.Equals which performed reference equality
        /// on the Dictionary object - never a deep comparison.
        /// </summary>
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