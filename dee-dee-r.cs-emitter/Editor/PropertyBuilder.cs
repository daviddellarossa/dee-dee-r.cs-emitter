using System;
using System.Collections.Generic;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# property definitions with getters, setters, and default values.
    /// </summary>
    /// <example>
    /// <code>
    /// // public int MyProp { get; set; }
    /// PropertyBuilder.Build(emitter, "MyProp", CsType.Int)
    ///     .WithAutoGetter()
    ///     .WithAutoSetter()
    ///     .Emit();
    ///
    /// // public int MyProp { get; private set; }
    /// PropertyBuilder.Build(emitter, "MyProp", CsType.Int)
    ///     .WithAutoGetter()
    ///     .WithAutoSetter(Visibility.Private)
    ///     .Emit();
    ///
    /// // Expression-bodied property
    /// PropertyBuilder.Build(emitter, "Combat", CsType.Of("CombatDef"))
    ///     .WithStaticModifier()
    ///     .WithExpressionGetter("Runtime.Combat")
    ///     .Emit();
    ///
    /// // Explicit getter body
    /// PropertyBuilder.Build(emitter, "MyProp", CsType.Int)
    ///     .WithGetter(getter => getter
    ///         .Return("_myField"))
    ///     .WithSetter(setter => setter
    ///         .Assign("_myField", "value"))
    ///     .Emit();
    /// </code>
    /// </example>
    public sealed class PropertyBuilder
    {
        /// <summary>
        /// Delegate for configuring property accessor bodies (getter/setter).
        /// </summary>
        /// <param name="body">The code block builder for the accessor body.</param>
        public delegate void AccessorBodyBuilder(CodeBlockBuilder body);

        private readonly IndentEmitter _indentEmitter;
        private readonly string _propertyName;
        private readonly CsType _propertyType;
        private Visibility _visibility = Visibility.Public;
        private bool _isStatic;
        private AccessorBodyBuilder _getter;
        private AccessorBodyBuilder _setter;
        private Visibility? _getterVisibility;
        private Visibility? _setterVisibility;
        private bool _hasGetter = false;
        private bool _hasSetter = false;
        private string _defaultValue;

        private XmlDocBuilder _xmlDoc;
        private string _expressionBody;

        /// <summary>
        /// Configures the XML documentation for the property.
        /// </summary>
        /// <param name="configure">An action to configure the XML documentation builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithXmlDoc(Action<XmlDocBuilder> configure)
        {
            _xmlDoc = XmlDocBuilder.Build();
            configure(_xmlDoc);
            return this;
        }

        private PropertyBuilder(IndentEmitter indentEmitter, string propertyName, CsType propertyType)
        {
            _indentEmitter = indentEmitter;
            _propertyName = propertyName;
            _propertyType = propertyType;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PropertyBuilder"/>.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <returns>A new <see cref="PropertyBuilder"/> instance.</returns>
        public static PropertyBuilder Build(IndentEmitter indentEmitter, string propertyName, CsType propertyType)
            => new PropertyBuilder(indentEmitter, propertyName, propertyType);

        /// <summary>
        /// Sets the visibility modifier for the property.
        /// </summary>
        /// <param name="visibility">The visibility level.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithVisibility(Visibility visibility)
        {
            _visibility = visibility;
            return this;
        }

        /// <summary>
        /// Sets the static modifier for the property.
        /// </summary>
        /// <param name="isStatic">True to make the property static; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithStaticModifier(bool isStatic = true)
        {
            _isStatic = isStatic;
            return this;
        }

        /// <summary>
        /// Sets the default value for the property.
        /// </summary>
        /// <param name="defaultValue">The default value expression.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithDefaultValue(string defaultValue)
        {
            _defaultValue = defaultValue;
            return this;
        }

        /// <summary>
        /// Adds an auto-implemented getter to the property.
        /// </summary>
        /// <param name="visibility">The optional visibility modifier for the getter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithAutoGetter(Visibility? visibility = null)
        {
            _hasGetter = true;
            _getter = null;
            _getterVisibility = visibility;
            return this;
        }

        /// <summary>
        /// Adds an auto-implemented setter to the property.
        /// </summary>
        /// <param name="visibility">The optional visibility modifier for the setter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithAutoSetter(Visibility? visibility = null)
        {
            _hasSetter = true;
            _setter = null;
            _setterVisibility = visibility;
            return this;
        }

        /// <summary>
        /// Adds a getter with an explicit body to the property.
        /// </summary>
        /// <param name="getter">A delegate to configure the getter body.</param>
        /// <param name="visibility">The optional visibility modifier for the getter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithGetter(AccessorBodyBuilder getter, Visibility? visibility = null)
        {
            _hasGetter = true;
            _getter = getter;
            _getterVisibility = visibility;
            return this;
        }

        /// <summary>
        /// Adds a setter with an explicit body to the property.
        /// </summary>
        /// <param name="setter">A delegate to configure the setter body.</param>
        /// <param name="visibility">The optional visibility modifier for the setter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithSetter(AccessorBodyBuilder setter, Visibility? visibility = null)
        {
            _hasSetter = true;
            _setter = setter;
            _setterVisibility = visibility;
            return this;
        }

        /// <summary>
        /// Adds an expression-bodied getter to the property.
        /// </summary>
        /// <param name="expression">The expression for the getter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public PropertyBuilder WithExpressionGetter(string expression)
        {
            _expressionBody = expression;
            return this;
        }

        
        /// <summary>
        /// Emits the complete C# code for the property.
        /// </summary>
        /// <returns>A string containing the generated C# property code.</returns>
        public string Emit()
        {
            if (_expressionBody != null && (_hasGetter || _hasSetter))
                throw new InvalidOperationException(
                    $"Property '{_propertyName}': cannot combine an expression body with a getter or setter.");
            var sb = new StringBuilder();

            if (_xmlDoc != null)
                sb.Append(_xmlDoc.Emit(_indentEmitter));

            var isAutoProperty = _getter == null && _setter == null && _expressionBody == null;
            var isExpressionProperty = _expressionBody != null;

            if (isExpressionProperty)
                sb.Append(EmitExpressionProperty());
            else if (isAutoProperty)
                sb.Append(EmitAutoProperty());
            else
                sb.Append(EmitFullProperty());

            return sb.ToString();
        }

        private string EmitExpressionProperty()
        {
            var modifiers = BuildModifiers();
            return $"{_indentEmitter.Get()}{modifiers}{_propertyType.Emit()} {_propertyName} => {_expressionBody};\n";
        }

        private string EmitAutoProperty()
        {
            var sb = new StringBuilder();
            var modifiers = BuildModifiers();

            var getterPart = HasGetter() ? $"{VisibilityPrefix(_getterVisibility)}{Constants.Get}; " : string.Empty;
            var setterPart = HasSetter() ? $"{VisibilityPrefix(_setterVisibility)}{Constants.Set}; " : string.Empty;

            var defaultPart = _defaultValue != null ? $" = {_defaultValue};" : string.Empty;

            sb.AppendLine($"{_indentEmitter.Get()}{modifiers}{_propertyType.Emit()} {_propertyName} {{ {getterPart}{setterPart}}}{defaultPart}");

            return sb.ToString();
        }

        private string EmitFullProperty()
        {
            var sb = new StringBuilder();
            var modifiers = BuildModifiers();

            sb.AppendLine($"{_indentEmitter.Get()}{modifiers}{_propertyType.Emit()} {_propertyName}");
            sb.AppendLine($"{_indentEmitter.Get()}{{");
            _indentEmitter.Push();

            if (HasGetter())
            {
                sb.AppendLine($"{_indentEmitter.Get()}{VisibilityPrefix(_getterVisibility)}{Constants.Get}");
                sb.AppendLine($"{_indentEmitter.Get()}{{");
                _indentEmitter.Push();

                if (_getter != null)
                {
                    var bodyBuilder = new CodeBlockBuilder(_indentEmitter);
                    _getter(bodyBuilder);
                    sb.Append(bodyBuilder.Emit());
                }

                _indentEmitter.Pop();
                sb.AppendLine($"{_indentEmitter.Get()}}}");
            }

            if (HasSetter())
            {
                sb.AppendLine($"{_indentEmitter.Get()}{VisibilityPrefix(_setterVisibility)}{Constants.Set}");
                sb.AppendLine($"{_indentEmitter.Get()}{{");
                _indentEmitter.Push();

                if (_setter != null)
                {
                    var bodyBuilder = new CodeBlockBuilder(_indentEmitter);
                    _setter(bodyBuilder);
                    sb.Append(bodyBuilder.Emit());
                }

                _indentEmitter.Pop();
                sb.AppendLine($"{_indentEmitter.Get()}}}");
            }

            _indentEmitter.Pop();
            sb.AppendLine($"{_indentEmitter.Get()}}}");

            return sb.ToString();
        }

        private bool HasGetter() => _hasGetter;
        private bool HasSetter() => _hasSetter;

        private string BuildModifiers()
        {
            var parts = new List<string> { Syntax.VisibilityToString(_visibility) };
            if (_isStatic) parts.Add(Constants.Static);
            return string.Join(" ", parts) + " ";
        }

        private static string VisibilityPrefix(Visibility? visibility)
            => visibility.HasValue ? Syntax.VisibilityToString(visibility.Value) + " " : string.Empty;
    }
}