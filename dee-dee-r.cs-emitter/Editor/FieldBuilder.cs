using System;
using System.Collections.Generic;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# field definitions.
    /// </summary>
    /// <example>
    /// <code>
    /// // private int _myField;
    /// FieldBuilder.Build(emitter, "_myField", CsType.Int)
    ///     .Emit();
    ///
    /// // public static readonly Vector3 DefaultPosition = Vector3.zero;
    /// FieldBuilder.Build(emitter, "DefaultPosition", CsType.Of("Vector3"))
    ///     .WithVisibility(Visibility.Public)
    ///     .WithStaticModifier()
    ///     .WithReadOnly()
    ///     .WithDefaultValue("Vector3.zero")
    ///     .Emit();
    /// </code>
    /// </example>
    public sealed class FieldBuilder
    {
        private readonly IndentEmitter _indentEmitter;
        private readonly string _fieldName;
        private readonly CsType _fieldType;
        private Visibility _visibility = Visibility.Private;
        private bool _isStatic;
        private bool _isReadOnly;
        private string _defaultValue;
        private bool _isConst = false;
        
        private XmlDocBuilder _xmlDoc;

        /// <summary>
        /// Configures the XML documentation for the field.
        /// </summary>
        /// <param name="configure">An action to configure the XML documentation builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FieldBuilder WithXmlDoc(Action<XmlDocBuilder> configure)
        {
            _xmlDoc = XmlDocBuilder.Build();
            configure(_xmlDoc);
            return this;
        }

        private FieldBuilder(IndentEmitter indentEmitter, string fieldName, CsType fieldType)
        {
            _indentEmitter = indentEmitter;
            _fieldName = fieldName;
            _fieldType = fieldType;
        }

        /// <summary>
        /// Creates a new instance of <see cref="FieldBuilder"/>.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <returns>A new <see cref="FieldBuilder"/> instance.</returns>
        public static FieldBuilder Build(IndentEmitter indentEmitter, string fieldName, CsType fieldType)
            => new FieldBuilder(indentEmitter, fieldName, fieldType);

        /// <summary>
        /// Sets the visibility modifier for the field.
        /// </summary>
        /// <param name="visibility">The visibility level.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FieldBuilder WithVisibility(Visibility visibility)
        {
            _visibility = visibility;
            return this;
        }

        /// <summary>
        /// Sets the static modifier for the field.
        /// </summary>
        /// <param name="isStatic">True to make the field static; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FieldBuilder WithStaticModifier(bool isStatic = true)
        {
            _isStatic = isStatic;
            return this;
        }
        
        /// <summary>
        /// Sets the const modifier for the field. Implies static.
        /// </summary>
        /// <param name="isConst">True to make the field const; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FieldBuilder WithConstModifier(bool isConst = true)
        {
            _isConst = isConst;
            if (isConst) _isStatic = true;
            return this;
        }

        /// <summary>
        /// Sets the readonly modifier for the field.
        /// </summary>
        /// <param name="isReadOnly">True to make the field readonly; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FieldBuilder WithReadOnly(bool isReadOnly = true)
        {
            _isReadOnly = isReadOnly;
            return this;
        }

        /// <summary>
        /// Sets the default value for the field.
        /// </summary>
        /// <param name="defaultValue">The default value expression.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FieldBuilder WithDefaultValue(string defaultValue)
        {
            _defaultValue = defaultValue;
            return this;
        }

        /// <summary>
        /// Emits the complete C# code for the field.
        /// </summary>
        /// <returns>A string containing the generated C# field code.</returns>
        public string Emit()
        {
            if (_isConst && _defaultValue == null)
                throw new InvalidOperationException(
                    $"Field '{_fieldName}': const fields must have a default value.");
            
            var sb = new StringBuilder();
            
            if (_xmlDoc != null)
                sb.Append(_xmlDoc.Emit(_indentEmitter));

            var modifiers = BuildModifiers();
            var declaration = _defaultValue != null
                ? $"{modifiers}{_fieldType.Emit()} {_fieldName} = {_defaultValue};"
                : $"{modifiers}{_fieldType.Emit()} {_fieldName};";

            sb.AppendLine($"{_indentEmitter.Get()}{declaration}");

            return sb.ToString();
        }

        private string BuildModifiers()
        {
            var parts = new List<string>();

            parts.Add(VisibilityToString(_visibility));

            if (_isConst)
            {
                parts.Add(Constants.Const);
            }
            else
            {
                if (_isStatic) parts.Add(Constants.Static);
                if (_isReadOnly) parts.Add(Constants.Readonly);
            }

            return string.Join(" ", parts) + " ";
        }

        private static string VisibilityToString(Visibility visibility) => visibility switch
        {
            Visibility.Public => Constants.Public,
            Visibility.Private => Constants.Private,
            Visibility.Protected => Constants.Protected,
            Visibility.Internal => Constants.Internal,
            Visibility.ProtectedInternal => $"{Constants.Protected} {Constants.Internal}",
            _ => Constants.Private
        };
    }
}