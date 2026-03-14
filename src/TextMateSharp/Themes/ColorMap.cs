using System;
using System.Collections.Generic;

namespace TextMateSharp.Themes
{
    public sealed class ColorMap : IEquatable<ColorMap>
    {

        private int _lastColorId;
        private readonly Dictionary<string /* color */, int? /* ID color */ > _color2id;

        public ColorMap()
        {
            this._lastColorId = 0;
            this._color2id = new Dictionary<string, int?>();
        }

        public int GetId(string color)
        {
            if (color == null)
            {
                return 0;
            }
            color = color.ToUpper();
            this._color2id.TryGetValue(color, out var value);
            if (value != null)
            {
                return value.Value;
            }
            value = ++this._lastColorId;
            this._color2id[color] = value;
            return value.Value;
        }

        public string GetColor(int id)
        {
            foreach (string color in _color2id.Keys)
            {
                if (_color2id.TryGetValue(color, out var value) && value.HasValue && value.Value == id)
                {
                    return color;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a collection containing the keys of the color map.
        /// </summary>
        /// <returns>A collection of strings representing the keys in the color map. The collection is empty if no colors are
        /// mapped.</returns>
        public ICollection<string> GetColorMap()
        {
            return this._color2id.Keys;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ColorMap"/> instance is equal to the current instance.
        /// Compares both the color-to-ID mappings and the last assigned color ID for structural equality.
        /// </summary>
        /// <param name="other">The <see cref="ColorMap"/> instance to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified instance is equal to the current instance; otherwise, <c>false</c>.</returns>
        public bool Equals(ColorMap other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (_lastColorId != other._lastColorId)
            {
                return false;
            }

            if (_color2id.Count != other._color2id.Count)
            {
                return false;
            }

            // Compare dictionary entries: every key in this must exist in other with same value
            foreach (KeyValuePair<string, int?> kvp in _color2id)
            {
                if (!other._color2id.TryGetValue(kvp.Key, out int? otherValue))
                {
                    return false;
                }

                if (kvp.Value != otherValue)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="ColorMap"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.
        /// Must be of type <see cref="ColorMap"/> to be considered for equality.</param>
        /// <returns><c>true</c> if the specified object is a <see cref="ColorMap"/> and is equal
        /// to the current instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ColorMap other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Returns a content-based hash code for the current instance using multiply-accumulate
        /// (factor 31) for good distribution.
        /// </summary>
        /// <remarks>
        /// Cannot be precomputed because the dictionary is mutable via <see cref="GetId"/>.
        /// Uses addition-based accumulation for dictionary entries (order-independent)
        /// and multiply-accumulate for combining with <c>_lastColorId</c>.
        /// </remarks>
        /// <returns>An integer hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // Dictionary entries are unordered, so we use addition (commutative)
                // to produce an order-independent hash for the key-value pairs
                int entriesHash = 0;
                foreach (KeyValuePair<string, int?> kvp in _color2id)
                {
                    int pairHash = (kvp.Key == null ? 0 : StringComparer.Ordinal.GetHashCode(kvp.Key));
                    pairHash = (pairHash * 31) + (kvp.Value ?? 0);
                    entriesHash += pairHash;
                }

                int hash = entriesHash;
                hash = (hash * 31) + _lastColorId;
                return hash;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="ColorMap"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ColorMap"/> instance to compare.</param>
        /// <param name="right">The second <see cref="ColorMap"/> instance to compare.</param>
        /// <returns><c>true</c> if the specified instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ColorMap left, ColorMap right)
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
        /// Determines whether two <see cref="ColorMap"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ColorMap"/> instance to compare.</param>
        /// <param name="right">The second <see cref="ColorMap"/> instance to compare.</param>
        /// <returns><c>true</c> if the specified instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(ColorMap left, ColorMap right)
        {
            return !(left == right);
        }
    }
}
