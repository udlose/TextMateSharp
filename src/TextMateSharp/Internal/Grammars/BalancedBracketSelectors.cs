using System;
using System.Collections.Generic;

namespace TextMateSharp.Internal.Grammars
{
    public sealed class BalancedBracketSelectors
    {
        private readonly Predicate<List<string>>[] _balancedBracketScopes;
        private readonly Predicate<List<string>>[] _unbalancedBracketScopes;

        private bool _allowAny = false;

        public BalancedBracketSelectors(
            List<string> balancedBracketScopes,
            List<string> unbalancedBracketScopes)
        {
            _balancedBracketScopes = CreateBalancedBracketScopes(balancedBracketScopes);
            _unbalancedBracketScopes = CreateUnbalancedBracketScopes(unbalancedBracketScopes);
        }

        internal bool MatchesAlways()
        {
            return _allowAny && _unbalancedBracketScopes.Length == 0;
        }

        internal bool MatchesNever()
        {
            return !_allowAny && _balancedBracketScopes.Length == 0;
        }

        internal bool Match(List<string> scopes)
        {
            foreach (var excluder in _unbalancedBracketScopes)
            {
                if (excluder.Invoke(scopes))
                {
                    return false;
                }
            }

            foreach (var includer in _balancedBracketScopes)
            {
                if (includer.Invoke(scopes))
                {
                    return true;
                }
            }

            return _allowAny;
        }

        Predicate<List<string>>[] CreateBalancedBracketScopes(List<string> balancedBracketScopes)
        {
            List<Predicate<List<string>>> result = new List<Predicate<List<string>>>();

            foreach (string selector in balancedBracketScopes)
            {
                if ("*".Equals(selector))
                {
                    _allowAny = true;
                    return Array.Empty<Predicate<List<string>>>();
                }

                var matcher = Matcher.Matcher.CreateMatchers(selector);

                foreach (var matches in matcher)
                {
                    result.Add(matches.Matcher);
                }
            }

            return result.ToArray();
        }

        static Predicate<List<string>>[] CreateUnbalancedBracketScopes(List<string> unbalancedBracketScopes)
        {
            if (unbalancedBracketScopes.Count > 0)
            {
                // we have to add at least one element per unbalanced bracket scope
                var result = new List<Predicate<List<string>>>(unbalancedBracketScopes.Count);
                foreach (string selector in unbalancedBracketScopes)
                {
                    var matcher = Matcher.Matcher.CreateMatchers(selector);

                    foreach (var matches in matcher)
                    {
                        result.Add(matches.Matcher);
                    }
                }

                return result.ToArray();
            }

            // avoid allocating a List<T> and then an empty array if we have no unbalanced bracket scopes
            return Array.Empty<Predicate<List<string>>>();
        }
    }
}
