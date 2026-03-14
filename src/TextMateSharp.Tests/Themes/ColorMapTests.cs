using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TextMateSharp.Themes;

namespace TextMateSharp.Tests.Themes
{
    [TestFixture]
    public class ColorMapTests
    {
        #region GetId tests

        [Test]
        public void GetId_NullColor_ReturnsZero()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            int id = map.GetId(null);

            // Assert
            Assert.AreEqual(0, id);
        }

        [Test]
        public void GetId_FirstColor_ReturnsOne()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            int id = map.GetId("#FF0000");

            // Assert
            Assert.AreEqual(1, id);
        }

        [Test]
        public void GetId_SameColorTwice_ReturnsSameId()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            int first = map.GetId("#FF0000");
            int second = map.GetId("#FF0000");

            // Assert
            Assert.AreEqual(first, second);
        }

        [Test]
        public void GetId_CaseInsensitive_ReturnsSameId()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            int lower = map.GetId("#ff0000");
            int upper = map.GetId("#FF0000");

            // Assert
            Assert.AreEqual(lower, upper);
        }

        [Test]
        public void GetId_DifferentColors_ReturnsDifferentIds()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            int red = map.GetId("#FF0000");
            int blue = map.GetId("#0000FF");

            // Assert
            Assert.AreNotEqual(red, blue);
        }

        #endregion GetId tests

        #region GetColor tests

        [Test]
        public void GetColor_ValidId_ReturnsUpperCaseColor()
        {
            // Arrange
            ColorMap map = new ColorMap();
            int id = map.GetId("#ff0000");

            // Act
            string color = map.GetColor(id);

            // Assert
            Assert.AreEqual("#FF0000", color);
        }

        [Test]
        public void GetColor_InvalidId_ReturnsNull()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            string color = map.GetColor(999);

            // Assert
            Assert.IsNull(color);
        }

        #endregion GetColor tests

        #region GetColorMap tests

        [Test]
        public void GetColorMap_EmptyMap_ReturnsEmptyCollection()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.IsNotNull(colors);
            Assert.AreEqual(0, colors.Count);
        }

        [Test]
        public void GetColorMap_SingleColor_ReturnsCollectionWithOneColor()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.AreEqual(1, colors.Count);
            CollectionAssert.Contains(colors, "#FF0000");
        }

        [Test]
        public void GetColorMap_MultipleColors_ReturnsAllColors()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");
            map.GetId("#00FF00");
            map.GetId("#0000FF");

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.AreEqual(3, colors.Count);
            CollectionAssert.Contains(colors, "#FF0000");
            CollectionAssert.Contains(colors, "#00FF00");
            CollectionAssert.Contains(colors, "#0000FF");
        }

        [Test]
        public void GetColorMap_DuplicateColorCalls_ReturnsUniqueColors()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");
            map.GetId("#FF0000");
            map.GetId("#00FF00");

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.AreEqual(2, colors.Count);
            CollectionAssert.Contains(colors, "#FF0000");
            CollectionAssert.Contains(colors, "#00FF00");
        }

        [Test]
        public void GetColorMap_ColorsInUpperCase_ReturnsUpperCaseColors()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#ff0000");
            map.GetId("#00ff00");

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.AreEqual(2, colors.Count);
            Assert.AreEqual("#FF0000", colors.ElementAt(0));
            Assert.AreEqual("#00FF00", colors.ElementAt(1));
        }

        [Test]
        public void GetColorMap_Dictionary_IsCaseSensitive()
        {
            // Arrange - GetId normalizes to upper case, so only one entry for different cases
            ColorMap map = new ColorMap();
            map.GetId("#ff0000");
            map.GetId("#FF0000");

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.AreEqual(1, colors.Count);
            CollectionAssert.Contains(colors, "#FF0000");
        }

        [Test]
        public void GetColorMap_NullColorNotIncluded_ReturnsOnlyNonNullColors()
        {
            // Arrange - GetId(null) returns 0 and doesn't add to the map
            ColorMap map = new ColorMap();
            map.GetId(null);
            map.GetId("#FF0000");

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.AreEqual(1, colors.Count);
            CollectionAssert.Contains(colors, "#FF0000");
        }

        [Test]
        public void GetColorMap_AfterAddingColors_ReflectsCurrentState()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");
            ICollection<string> colorsBefore = map.GetColorMap();
            int countBefore = colorsBefore.Count;

            // Act
            map.GetId("#00FF00");
            ICollection<string> colorsAfter = map.GetColorMap();

            // Assert
            Assert.AreEqual(1, countBefore);
            Assert.AreEqual(2, colorsAfter.Count);
            CollectionAssert.Contains(colorsAfter, "#FF0000");
            CollectionAssert.Contains(colorsAfter, "#00FF00");
        }

        [Test]
        public void GetColorMap_CaseInsensitiveDuplicates_ReturnsSingleEntry()
        {
            // Arrange - GetId normalizes to uppercase and treats case-insensitive as duplicates
            ColorMap map = new ColorMap();
            map.GetId("#ff0000");
            map.GetId("#FF0000");
            map.GetId("#Ff0000");

            // Act
            ICollection<string> colors = map.GetColorMap();

            // Assert
            Assert.AreEqual(1, colors.Count);
            CollectionAssert.Contains(colors, "#FF0000");
        }

        [Test]
        public void GetColorMap_ReturnsReadOnlyView_ModificationsReflected()
        {
            // Arrange - the returned collection is the dictionary's Keys collection
            // which reflects changes to the underlying dictionary
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");
            ICollection<string> colors = map.GetColorMap();
            int countBefore = colors.Count;

            // Act - modify the map after getting the collection reference
            map.GetId("#00FF00");

            // Assert - the same collection reference now shows updated count
            Assert.AreEqual(1, countBefore);
            Assert.AreEqual(2, colors.Count);
        }

        #endregion GetColorMap tests

        #region Equals (object) tests

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");

            // Act
            bool result = map.Equals((object)map);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_EquivalentMaps_ReturnsTrue()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            right.GetId("#FF0000");
            right.GetId("#00FF00");

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            bool result = map.Equals((object)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            bool result = map.Equals(42);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentColors_ReturnsFalse()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            right.GetId("#0000FF");

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_IsSymmetric()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            right.GetId("#FF0000");

            // Act & Assert
            Assert.IsTrue(left.Equals((object)right));
            Assert.IsTrue(right.Equals((object)left));
        }

        [Test]
        public void Equals_BothEmpty_ReturnsTrue()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentInsertionOrder_ReturnsFalse()
        {
            ColorMap left = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            left.GetId("#0000FF");

            ColorMap right = new ColorMap();
            // Insert in reverse order
            right.GetId("#0000FF");
            right.GetId("#00FF00");
            right.GetId("#FF0000");

            // Act & Assert - same color set, but IDs will differ due to insertion order
            // so _lastColorId matches but specific ID assignments differ
            // This means Equals should return FALSE because the mappings are different
            Assert.IsFalse(left.Equals((object)right));
        }

        [Test]
        public void Equals_SameColorsSameOrder_ReturnsTrue()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            left.GetId("#0000FF");
            right.GetId("#FF0000");
            right.GetId("#00FF00");
            right.GetId("#0000FF");

            // Act
            bool result = left.Equals((object)right);

            // Assert
            Assert.IsTrue(result);
        }

        //[Test]
        //public void Equals_DifferentSizeDictionaries_ReturnsFalse()
        //{
        //    // Arrange
        //    ColorMap left = new ColorMap();
        //    ColorMap right = new ColorMap();
        //    left.GetId("#FF0000");
        //    left.GetId("#00FF00");
        //    right.GetId("#FF0000");
        //    // Act
        //    bool result = left.Equals((object)right);
        //    // Assert
        //    Assert.IsFalse(result);
        //}

        #endregion Equals (object) tests

        #region IEquatable<ColorMap> tests

        [Test]
        public void IEquatable_Equals_IsReflexive()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");

            // Act
#pragma warning disable CS1718 // Comparison made to same variable
            bool result = map.Equals(map);
#pragma warning restore CS1718 // Comparison made to same variable
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_IsSymmetric()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            right.GetId("#FF0000");

            // Act & Assert
            Assert.IsTrue(left.Equals(right));
            Assert.IsTrue(right.Equals(left));
        }

        [Test]
        public void IEquatable_Equals_IsTransitive()
        {
            // Arrange
            ColorMap a = new ColorMap();
            ColorMap b = new ColorMap();
            ColorMap c = new ColorMap();
            a.GetId("#FF0000");
            b.GetId("#FF0000");
            c.GetId("#FF0000");

            // Act & Assert
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(c));
            Assert.IsTrue(a.Equals(c));
        }

        [Test]
        public void IEquatable_Equals_Null_ReturnsFalse()
        {
            // Arrange
            ColorMap map = new ColorMap();

            // Act
            bool result = map.Equals((ColorMap)null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IEquatable_Equals_SameColorsDifferentInsertionOrder_ReturnsFalse()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            left.GetId("#0000FF");
            // Insert in reverse order
            right.GetId("#0000FF");
            right.GetId("#00FF00");
            right.GetId("#FF0000");

            // Act & Assert - same color set, but IDs will differ due to insertion order
            // so _lastColorId matches but specific ID assignments differ
            // This means Equals should return FALSE because the mappings are different
            Assert.IsFalse(left.Equals(right));
        }

        [Test]
        public void IEquatable_Equals_SameColorsSameOrder_ReturnsTrue()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            left.GetId("#0000FF");
            right.GetId("#FF0000");
            right.GetId("#00FF00");
            right.GetId("#0000FF");

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_CaseInsensitive_ReturnsTrue()
        {
            // Arrange - GetId normalizes to upper case
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#ff0000");
            right.GetId("#FF0000");

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IEquatable_Equals_UsedByEqualityComparerDefault()
        {
            // Arrange
            ColorMap key1 = new ColorMap();
            ColorMap key2 = new ColorMap();
            key1.GetId("#FF0000");
            key2.GetId("#FF0000");

            EqualityComparer<ColorMap> comparer = EqualityComparer<ColorMap>.Default;

            // Act & Assert
            Assert.IsTrue(comparer.Equals(key1, key2));
            Assert.AreEqual(comparer.GetHashCode(key1), comparer.GetHashCode(key2));
        }

        #endregion IEquatable<ColorMap> tests

        #region Operator == and != tests

        [Test]
        public void OperatorEquals_SameInstance_ReturnsTrue()
        {
            // Arrange
            ColorMap map = new ColorMap();
            map.GetId("#FF0000");

            // Act & Assert
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(map == map);
            Assert.IsFalse(map != map);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Test]
        public void OperatorEquals_StructurallyEqual_ReturnsTrue()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            right.GetId("#FF0000");

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_DifferentMaps_ReturnsFalse()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            right.GetId("#0000FF");

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_BothNull_ReturnsTrue()
        {
            // Arrange
            ColorMap left = null;
            ColorMap right = null;

            // Act & Assert
            Assert.IsTrue(left == right);
            Assert.IsFalse(left != right);
        }

        [Test]
        public void OperatorEquals_LeftNull_ReturnsFalse()
        {
            // Arrange
            ColorMap left = null;
            ColorMap right = new ColorMap();

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        [Test]
        public void OperatorEquals_RightNull_ReturnsFalse()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = null;

            // Act & Assert
            Assert.IsFalse(left == right);
            Assert.IsTrue(left != right);
        }

        #endregion Operator == and != tests

        #region GetHashCode tests

        [Test]
        public void GetHashCode_EqualInstances_ReturnSameValue()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            right.GetId("#FF0000");
            right.GetId("#00FF00");

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_EmptyMaps_ReturnSameValue()
        {
            // Arrange
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_AfterMutation_ReflectsMutation()
        {
            // Arrange - hash is computed on the fly, so mutation is reflected
            ColorMap map = new ColorMap();
            int hashBefore = map.GetHashCode();

            // Act
            map.GetId("#FF0000");
            int hashAfter = map.GetHashCode();

            // Assert
            Assert.AreNotEqual(hashBefore, hashAfter);
        }

        [Test]
        public void GetHashCode_CaseInsensitive_ReturnsSameValue()
        {
            // Arrange - GetId normalizes to upper case
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#ff0000");
            right.GetId("#FF0000");

            // Act & Assert
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        #endregion GetHashCode tests

        #region HashCode / Equals contract tests

        [Test]
        public void HashCodeEqualsContract_EqualObjects_SameHashCode()
        {
            // If two objects are equal, they must have the same hash code
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            right.GetId("#FF0000");
            right.GetId("#00FF00");

            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void HashCodeEqualsContract_WorksWithDictionary()
        {
            // Arrange
            ColorMap key1 = new ColorMap();
            ColorMap key2 = new ColorMap();
            key1.GetId("#FF0000");
            key2.GetId("#FF0000");

            Dictionary<ColorMap, int> dictionary = new Dictionary<ColorMap, int>();
            dictionary[key1] = 42;

            // Act & Assert - key2 is structurally equal to key1
            Assert.IsTrue(dictionary.ContainsKey(key2));
            Assert.AreEqual(42, dictionary[key2]);
        }

        [Test]
        public void HashCodeEqualsContract_WorksWithHashSet()
        {
            // Arrange
            ColorMap item1 = new ColorMap();
            ColorMap item2 = new ColorMap();
            item1.GetId("#FF0000");
            item2.GetId("#FF0000");

            HashSet<ColorMap> hashSet = new HashSet<ColorMap>();
            hashSet.Add(item1);

            // Act & Assert - item2 is structurally equal to item1
            Assert.IsTrue(hashSet.Contains(item2));
            Assert.IsFalse(hashSet.Add(item2));
        }

        #endregion HashCode / Equals contract tests

        #region Equals edge case tests for specific code paths

        [Test]
        public void Equals_DifferentColorIds_SameMappingCount_ReturnsFalse()
        {
            // Arrange - tests the _lastColorId comparison branch
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();
            left.GetId("#FF0000");
            left.GetId("#00FF00");
            right.GetId("#FF0000");
            // right has fewer colors, so different _lastColorId

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameKeysButDifferentValues_ReturnsFalse()
        {
            // Arrange - tests the value comparison branch in the foreach loop
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();

            // Add colors in different order to get different ID assignments
            left.GetId("#FF0000");  // Gets ID 1 in left
            left.GetId("#00FF00");  // Gets ID 2 in left

            right.GetId("#00FF00"); // Gets ID 1 in right
            right.GetId("#FF0000"); // Gets ID 2 in right

            // Both have same colors but different ID mappings
            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsFalse(result, "Maps with same colors but different ID assignments should not be equal");
        }

        [Test]
        public void Equals_LeftHasKeyNotInRight_ReturnsFalse()
        {
            // Arrange - tests the TryGetValue failure branch
            // This tests the specific "if (!other._color2id.TryGetValue(...)) return false;" line
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();

            left.GetId("#FF0000");
            left.GetId("#00FF00");
            left.GetId("#0000FF");

            right.GetId("#FF0000");
            right.GetId("#00FF00");
            right.GetId("#FFFF00"); // Different third color

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsFalse(result, "Maps with different color sets should not be equal");
        }

        [Test]
        public void Equals_SubsetOfColors_ReturnsFalse()
        {
            // Arrange - tests count comparison and dictionary key existence
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();

            left.GetId("#FF0000");
            left.GetId("#00FF00");
            left.GetId("#0000FF");

            right.GetId("#FF0000");
            right.GetId("#00FF00");
            // right is missing #0000FF

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsFalse(result, "Map should not equal its subset");
        }

        [Test]
        public void Equals_EmptyVsNonEmpty_ReturnsFalse()
        {
            // Arrange - tests count comparison branch
            ColorMap empty = new ColorMap();
            ColorMap nonEmpty = new ColorMap();
            nonEmpty.GetId("#FF0000");

            // Act
            bool emptyEqualsNonEmpty = empty.Equals(nonEmpty);
            bool nonEmptyEqualsEmpty = nonEmpty.Equals(empty);

            // Assert
            Assert.IsFalse(emptyEqualsNonEmpty, "Empty map should not equal non-empty map");
            Assert.IsFalse(nonEmptyEqualsEmpty, "Non-empty map should not equal empty map");
        }

        [Test]
        public void Equals_SameColorsInsertedTwice_StillEqual()
        {
            // Arrange - duplicate insertions should not affect equality
            ColorMap left = new ColorMap();
            ColorMap right = new ColorMap();

            left.GetId("#FF0000");
            left.GetId("#FF0000"); // Duplicate - should not change state

            right.GetId("#FF0000");

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.IsTrue(result, "Duplicate color insertions should not affect equality");
        }

        #endregion Equals edge case tests for specific code paths
    }
}