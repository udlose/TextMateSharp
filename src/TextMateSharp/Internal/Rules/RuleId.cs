using System;

namespace TextMateSharp.Internal.Rules
{
    public sealed class RuleId : IEquatable<RuleId>
    {
        public static readonly RuleId NO_RULE = new RuleId(0);

        /**
         * This is a special constant to indicate that the end regexp matched.
         */
        public static readonly RuleId END_RULE = new RuleId(-1);

        /**
         * This is a special constant to indicate that the while regexp matched.
         */
        public static readonly RuleId WHILE_RULE = new RuleId(-2);

        public static RuleId Of(int id)
        {
            if (id < 0)
                throw new ArgumentException("[id] must be > 0");
            return new RuleId(id);
        }

        public int Id { get; }

        private RuleId(int id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Determines whether the specified <see cref="RuleId"/> instance is equal to the current instance.
        /// Two <see cref="RuleId"/> instances are equal if they have the same <see cref="Id"/> value.
        /// </summary>
        /// <param name="other">The <see cref="RuleId"/> instance to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified instance has the same Id; otherwise, <c>false</c>.</returns>
        public bool Equals(RuleId other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="RuleId"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.
        /// Must be of type <see cref="RuleId"/> to be considered for equality.</param>
        /// <returns><c>true</c> if the specified object is a <see cref="RuleId"/> and has the same Id;
        /// otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is RuleId other)
            {
                return Equals(other);
            }

            return false;
        }

        [Obsolete("Use the '!=' operator instead.")]
        public bool NotEquals(RuleId otherRule)
        {
            return Id != otherRule.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Determines whether two <see cref="RuleId"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="RuleId"/> instance to compare.</param>
        /// <param name="right">The second <see cref="RuleId"/> instance to compare.</param>
        /// <returns><c>true</c> if the specified instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(RuleId left, RuleId right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Id == right.Id;
        }

        /// <summary>
        /// Determines whether two <see cref="RuleId"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="RuleId"/> instance to compare.</param>
        /// <param name="right">The second <see cref="RuleId"/> instance to compare.</param>
        /// <returns><c>true</c> if the specified instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(RuleId left, RuleId right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
