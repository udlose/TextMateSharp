using System;
using TextMateSharp.Grammars;

namespace TextMateSharp.Model
{
    public sealed class TMState : IEquatable<TMState>
    {
        private readonly TMState _parentEmbedderState;
        private IStateStack _ruleStack;

        public TMState(TMState parentEmbedderState, IStateStack ruleStack)
        {
            this._parentEmbedderState = parentEmbedderState;
            this._ruleStack = ruleStack;
        }

        public void SetRuleStack(IStateStack ruleStack)
        {
            this._ruleStack = ruleStack;
        }

        public IStateStack GetRuleStack()
        {
            return _ruleStack;
        }

        public TMState Clone()
        {
            TMState parentEmbedderStateClone = this._parentEmbedderState != null ?
                this._parentEmbedderState.Clone() : null;

            return new TMState(parentEmbedderStateClone, this._ruleStack);
        }

        /// <summary>
        /// Determines whether the specified <see cref="TMState"/> instance is equal to the current instance.
        /// Compares both the parent embedder state chain and the rule stack for structural equality.
        /// </summary>
        /// <param name="other">The <see cref="TMState"/> instance to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified instance is equal to the current instance; otherwise, <c>false</c>.</returns>
        public bool Equals(TMState other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // Use object.Equals for null-safe value equality,
            // matching Java upstream's Objects.equals() semantics.
            // _parentEmbedderState comparison leverages TMState's own IEquatable<TMState>
            // because object.Equals dispatches to the typed Equals when available.
            // _ruleStack comparison leverages StateStack's IEquatable<StateStack>
            // through its Equals(object) override.
            return Equals(_parentEmbedderState, other._parentEmbedderState) &&
                   Equals(_ruleStack, other._ruleStack);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="TMState"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.
        /// Must be of type <see cref="TMState"/> to be considered for equality.</param>
        /// <returns><c>true</c> if the specified object is a <see cref="TMState"/> and is equal
        /// to the current instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is TMState other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for the current instance using multiply-accumulate (factor 31)
        /// for good distribution.
        /// </summary>
        /// <remarks>
        /// Cannot be precomputed because <c>_ruleStack</c> is mutable via <see cref="SetRuleStack"/>.
        /// </remarks>
        /// <returns>An integer hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                const int primeFactor = 31; // Common prime factor for multiply-accumulate hash code
                int hash = _parentEmbedderState?.GetHashCode() ?? 0;
                hash = (hash * primeFactor) + (_ruleStack?.GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="TMState"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="TMState"/> instance to compare.</param>
        /// <param name="right">The second <see cref="TMState"/> instance to compare.</param>
        /// <returns><c>true</c> if the specified instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(TMState left, TMState right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="TMState"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="TMState"/> instance to compare.</param>
        /// <param name="right">The second <see cref="TMState"/> instance to compare.</param>
        /// <returns><c>true</c> if the specified instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(TMState left, TMState right)
        {
            return !(left == right);
        }
    }
}