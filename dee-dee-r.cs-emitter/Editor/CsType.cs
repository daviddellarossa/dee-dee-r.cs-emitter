using System;
using System.Collections.Generic;
using System.Linq;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// Represents a C# type with support for generics.
    /// </summary>
    /// <example>
    /// <code>
    /// CsType.Int                                          // "int"
    /// CsType.Of("Vector3")                                // "Vector3"
    /// CsType.ListOf(CsType.Of("Vector3"))                 // "List&lt;Vector3&gt;"
    /// CsType.DictionaryOf(CsType.String, CsType.Int)      // "Dictionary&lt;string, int&gt;"
    /// CsType.Generic("Dictionary",                        // "Dictionary&lt;string, List&lt;int&gt;&gt;"
    ///     CsType.String,
    ///     CsType.ListOf(CsType.Int))
    /// </code>
    /// </example>
    public sealed class CsType
    {
        private readonly string _name;
        private readonly IReadOnlyList<CsType> _typeArguments;

        /// <summary>
        /// Gets a value indicating whether this type is a generic type with type arguments.
        /// </summary>
        public bool IsGeneric => _typeArguments.Count > 0;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the type arguments for generic types.
        /// </summary>
        public IReadOnlyList<CsType> TypeArguments => _typeArguments;

        private CsType(string name, IReadOnlyList<CsType> typeArguments)
        {
            _name = name;
            _typeArguments = typeArguments;
        }

        /// <summary>
        /// Creates a simple non-generic type.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        /// <returns>A non-generic <see cref="CsType"/>.</returns>
        public static CsType Of(string name)
            => new CsType(name, Array.Empty<CsType>());

        /// <summary>
        /// Creates a generic type with one or more type arguments.
        /// </summary>
        /// <param name="name">The name of the generic type.</param>
        /// <param name="typeArguments">The type arguments for the generic type.</param>
        /// <returns>A generic <see cref="CsType"/>.</returns>
        public static CsType Generic(string name, params CsType[] typeArguments)
            => new CsType(name, typeArguments);

        /// <summary>
        /// Gets a <see cref="CsType"/> representing the <c>void</c> type.
        /// </summary>
        public static CsType Void => Of(Constants.Void);

        /// <summary>
        /// Gets a <see cref="CsType"/> representing the <c>int</c> type.
        /// </summary>
        public static CsType Int => Of(Constants.Int);

        /// <summary>
        /// Gets a <see cref="CsType"/> representing the <c>float</c> type.
        /// </summary>
        public static CsType Float => Of(Constants.Float);

        /// <summary>
        /// Gets a <see cref="CsType"/> representing the <c>bool</c> type.
        /// </summary>
        public static CsType Bool => Of(Constants.Bool);

        /// <summary>
        /// Gets a <see cref="CsType"/> representing the <c>string</c> type.
        /// </summary>
        public static CsType String => Of(Constants.String);

        /// <summary>
        /// Creates a <see cref="CsType"/> representing a <c>List&lt;T&gt;</c> with the specified element type.
        /// </summary>
        /// <param name="elementType">The type of elements in the list.</param>
        /// <returns>A <see cref="CsType"/> representing <c>List&lt;elementType&gt;</c>.</returns>
        public static CsType ListOf(CsType elementType) => Generic(Constants.List, elementType);

        /// <summary>
        /// Creates a <see cref="CsType"/> representing a <c>Dictionary&lt;TKey, TValue&gt;</c> with the specified key and value types.
        /// </summary>
        /// <param name="keyType">The type of keys in the dictionary.</param>
        /// <param name="valueType">The type of values in the dictionary.</param>
        /// <returns>A <see cref="CsType"/> representing <c>Dictionary&lt;keyType, valueType&gt;</c>.</returns>
        public static CsType DictionaryOf(CsType keyType, CsType valueType) => Generic(Constants.Dictionary, keyType, valueType);

        /// <summary>
        /// Emits the C# code representation of this type.
        /// </summary>
        /// <returns>A string containing the C# code representation of the type.</returns>
        public string Emit()
        {
            if (!IsGeneric)
                return _name;

            var args = string.Join(", ", _typeArguments.Select(t => t.Emit()));
            return $"{_name}<{args}>";
        }

        /// <summary>
        /// Returns a string that represents the current type.
        /// </summary>
        /// <returns>A string representation of the type.</returns>
        public override string ToString() => Emit();
    }
}