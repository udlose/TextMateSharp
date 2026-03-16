using System;
using System.Reflection;

namespace TextMateSharp.Tests.Helpers
{
    /// <summary>
    /// Provides reflection-based utilities for unit test setup.
    /// These methods allow tests to set private-set properties and private fields
    /// without modifying production code visibility (e.g., making setters internal).
    ///
    /// WARNING: Use only in test projects. Do not use in production code.
    /// </summary>
    public static class ReflectionTestHelper
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Sets an instance property value via reflection, bypassing access modifiers.
        /// Handles properties with private setters (e.g., <c>public int Foo { get; private set; }</c>).
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="instance">The object instance whose property will be set.</param>
        /// <param name="propertyName">The exact name of the property (case-sensitive).</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instance"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="propertyName"/> is null or whitespace.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the property is not found or has no setter.
        /// </exception>
        /// <example>
        /// <code>
        /// var rule = new ThemeTrieElementRule();
        /// ReflectionTestHelper.SetProperty(rule, "fontStyle", FontStyle.Bold);
        /// ReflectionTestHelper.SetProperty(rule, "foreground", 7);
        /// </code>
        /// </example>
        public static void SetProperty<T>(object instance, string propertyName, T value)
        {
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

            Type type = instance.GetType();

            PropertyInfo property = type.GetProperty(propertyName, InstanceFlags);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Property '{propertyName}' not found on type '{type.FullName}'. " +
                    "Verify the property name is spelled correctly and is case-sensitive.");
            }

            MethodInfo setter = property.GetSetMethod(nonPublic: true);

            if (setter == null)
            {
                throw new InvalidOperationException(
                    $"Property '{propertyName}' on type '{type.FullName}' has no setter (read-only property). " +
                    "Consider using SetField to target the compiler-generated backing field instead.");
            }

            setter.Invoke(instance, new object[] { value });
        }

        /// <summary>
        /// Sets an instance field value via reflection, bypassing access modifiers.
        /// Handles private, protected, and internal fields including compiler-generated
        /// backing fields (e.g., <c>&lt;PropertyName&gt;k__BackingField</c>).
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="instance">The object instance whose field will be set.</param>
        /// <param name="fieldName">The exact name of the field (case-sensitive).</param>
        /// <param name="value">The value to assign to the field.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instance"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fieldName"/> is null or whitespace.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the field is not found on the type or any of its base types.
        /// </exception>
        /// <example>
        /// <code>
        /// var element = new ThemeTrieElement();
        /// ReflectionTestHelper.SetField(element, "_children", new Dictionary&lt;string, ThemeTrieElement&gt;());
        ///
        /// // Compiler-generated backing field example:
        /// ReflectionTestHelper.SetField(element, "&lt;scopeDepth&gt;k__BackingField", 3);
        /// </code>
        /// </example>
        public static void SetField<T>(object instance, string fieldName, T value)
        {
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

            Type type = instance.GetType();

            // Walk the type hierarchy to find the field - private fields
            // declared on a base class are not returned by GetField on the derived type.
            FieldInfo field = null;
            Type current = type;

            while (current != null)
            {
                field = current.GetField(fieldName, InstanceFlags);

                if (field != null)
                    break;

                current = current.BaseType;
            }

            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Field '{fieldName}' not found on type '{type.FullName}' or any of its base types. " +
                    "Verify the field name is spelled correctly and is case-sensitive. " +
                    "For auto-property backing fields, use the format '<PropertyName>k__BackingField'.");
            }

            field.SetValue(instance, value);
        }
    }
}