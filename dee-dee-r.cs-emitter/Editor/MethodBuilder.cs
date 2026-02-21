using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# method definitions with parameters, generic types, and method bodies.
    /// </summary>
    /// <example>
    /// <code>
    /// // Simple void method
    /// MethodBuilder.Build(emitter, "Initialize", CsType.Void)
    ///     .WithBody(body => body
    ///         .Assign("_isReady", "true")
    ///         .Call("Setup"))
    ///     .Emit();
    ///
    /// // Generic method with constraint
    /// MethodBuilder.Build(emitter, "GetOrCreate", CsType.Of("T"))
    ///     .WithTypeParameter("T")
    ///     .WithTypeConstraint("T", "new()")
    ///     .WithParameter(CsType.String, "key")
    ///     .WithBody(body => body
    ///         .If("_cache.ContainsKey(key)",
    ///             then => then.Return("_cache[key]")))
    ///     .Emit();
    ///
    /// // Abstract method (no body)
    /// MethodBuilder.Build(emitter, "Execute", CsType.Void)
    ///     .WithAbstractModifier()
    ///     .WithParameter(CsType.Of("ExecutionContext"), "context")
    ///     .Emit();
    /// </code>
    /// </example>
    public sealed class MethodBuilder
    {
        /// <summary>
        /// Delegate for configuring the method body.
        /// </summary>
        /// <param name="body">The code block builder for the method body.</param>
        public delegate void MethodBodyBuilder(CodeBlockBuilder body);

        private readonly IndentEmitter _indentEmitter;
        private readonly string _methodName;
        private readonly CsType _returnType;
        private Visibility _visibility = Visibility.Public;
        private bool _isStatic;
        private bool _isOverride;
        private bool _isVirtual;
        private bool _isAbstract;
        private bool _isPartial;
        private readonly List<(CsType Type, string Name)> _parameters = new ();
        private readonly List<string> _typeParameters = new ();
        private readonly List<(string TypeParam, string Constraint)> _typeConstraints = new ();
        private readonly List<AttributeBuilder> _attributes = new();
        private MethodBodyBuilder _body;

        private XmlDocBuilder _xmlDoc;

        /// <summary>
        /// Configures the XML documentation for the method.
        /// </summary>
        /// <param name="configure">An action to configure the XML documentation builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithXmlDoc(Action<XmlDocBuilder> configure)
        {
            _xmlDoc = XmlDocBuilder.Build();
            configure(_xmlDoc);
            return this;
        }

        private MethodBuilder(IndentEmitter indentEmitter, string methodName, CsType returnType)
        {
            _indentEmitter = indentEmitter;
            _methodName = methodName;
            _returnType = returnType;
        }

        /// <summary>
        /// Creates a new instance of <see cref="MethodBuilder"/>.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>A new <see cref="MethodBuilder"/> instance.</returns>
        public static MethodBuilder Build(IndentEmitter indentEmitter, string methodName, CsType returnType)
            => new MethodBuilder(indentEmitter, methodName, returnType);

        /// <summary>
        /// Sets the visibility modifier for the method.
        /// </summary>
        /// <param name="visibility">The visibility level.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithVisibility(Visibility visibility)
        {
            _visibility = visibility;
            return this;
        }

        /// <summary>
        /// Sets the static modifier for the method.
        /// </summary>
        /// <param name="isStatic">True to make the method static; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithStaticModifier(bool isStatic = true)
        {
            _isStatic = isStatic;
            return this;
        }

        /// <summary>
        /// Sets the override modifier for the method.
        /// </summary>
        /// <param name="isOverride">True to make the method override; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithOverrideModifier(bool isOverride = true)
        {
            _isOverride = isOverride;
            return this;
        }

        /// <summary>
        /// Sets the virtual modifier for the method.
        /// </summary>
        /// <param name="isVirtual">True to make the method virtual; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithVirtualModifier(bool isVirtual = true)
        {
            _isVirtual = isVirtual;
            return this;
        }

        /// <summary>
        /// Sets the abstract modifier for the method.
        /// </summary>
        /// <param name="isAbstract">True to make the method abstract; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithAbstractModifier(bool isAbstract = true)
        {
            _isAbstract = isAbstract;
            return this;
        }

        /// <summary>
        /// Sets the partial modifier for the method.
        /// </summary>
        /// <param name="isPartial">True to make the method partial; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithPartialModifier(bool isPartial = true)
        {
            _isPartial = isPartial;
            return this;
        }

        /// <summary>
        /// Adds a parameter to the method.
        /// </summary>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithParameter(CsType type, string name)
        {
            _parameters.Add((type, name));
            return this;
        }

        /// <summary>
        /// Adds a generic type parameter to the method.
        /// </summary>
        /// <param name="typeParam">The name of the type parameter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithTypeParameter(string typeParam)
        {
            _typeParameters.Add(typeParam);
            return this;
        }

        /// <summary>
        /// Adds a generic constraint for a type parameter.
        /// </summary>
        /// <param name="typeParam">The type parameter name.</param>
        /// <param name="constraint">The constraint for the type parameter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithTypeConstraint(string typeParam, string constraint)
        {
            _typeConstraints.Add((typeParam, constraint));
            return this;
        }

        /// <summary>
        /// Configures the body of the method.
        /// </summary>
        /// <param name="body">A delegate to configure the method body.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithBody(MethodBodyBuilder body)
        {
            _body = body;
            return this;
        }

        /// <summary>
        /// Adds multiple parameters to the method from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="typeSelector">A function to extract the parameter type from each element.</param>
        /// <param name="nameSelector">A function to extract the parameter name from each element.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithParameters<T>(
            IEnumerable<T> source,
            Func<T, CsType> typeSelector,
            Func<T, string> nameSelector)
        {
            foreach (var item in source)
                _parameters.Add((typeSelector(item), nameSelector(item)));
            return this;
        }

        /// <summary>
        /// Adds multiple type parameters to the method from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="nameSelector">A function to extract the type parameter name from each element.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithTypeParameters<T>(
            IEnumerable<T> source,
            Func<T, string> nameSelector)
        {
            foreach (var item in source)
                _typeParameters.Add(nameSelector(item));
            return this;
        }

        /// <summary>
        /// Adds multiple type constraints to the method from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="typeParamSelector">A function to extract the type parameter name from each element.</param>
        /// <param name="constraintSelector">A function to extract the constraint from each element.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithTypeConstraints<T>(
            IEnumerable<T> source,
            Func<T, string> typeParamSelector,
            Func<T, string> constraintSelector)
        {
            foreach (var item in source)
                _typeConstraints.Add((typeParamSelector(item), constraintSelector(item)));
            return this;
        }
        
        /// <summary>
        /// Adds an attribute to the declaration.
        /// </summary>
        /// <param name="attributeName">The attribute name without brackets.</param>
        /// <param name="configure">An optional action to configure the attribute arguments.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public MethodBuilder WithAttribute(string attributeName, Action<AttributeBuilder> configure = null)
        {
            var builder = AttributeBuilder.Build(attributeName);
            configure?.Invoke(builder);
            _attributes.Add(builder);
            return this;
        }

        /// <summary>
        /// Emits the complete C# code for the method.
        /// </summary>
        /// <returns>A string containing the generated C# method code.</returns>
        public string Emit()
        {
            var sb = new StringBuilder();

            if (_xmlDoc != null)
                sb.Append(_xmlDoc.Emit(_indentEmitter));

            foreach (var attribute in _attributes)
                sb.Append(attribute.Emit(_indentEmitter));
            sb.AppendLine($"{_indentEmitter.Get()}{BuildSignature()}");

            if (_isAbstract || (_isPartial && _body == null))
                return sb.ToString().TrimEnd() + ";\n";

            sb.AppendLine($"{_indentEmitter.Get()}{{");
            _indentEmitter.Push();

            if (_body != null)
            {
                var bodyBuilder = new CodeBlockBuilder(_indentEmitter);
                _body(bodyBuilder);
                sb.Append(bodyBuilder.Emit());
            }

            _indentEmitter.Pop();
            sb.AppendLine($"{_indentEmitter.Get()}}}");

            return sb.ToString();
        }

        private string BuildSignature()
        {
            var sb = new StringBuilder();

            // Modifiers
            sb.Append(BuildModifiers());

            // Return type and name
            sb.Append($"{_returnType.Emit()} {_methodName}");

            // Generic type parameters
            if (_typeParameters.Count > 0)
                sb.Append($"<{string.Join(", ", _typeParameters)}>");

            // Parameters
            var paramList = _parameters.Select(p => $"{p.Type.Emit()} {p.Name}");
            sb.Append($"({string.Join(", ", paramList)})");

            // Generic constraints
            if (_typeConstraints.Count > 0)
            {
                foreach (var (typeParam, constraint) in _typeConstraints)
                    sb.Append($" {Constants.Where} {typeParam} : {constraint}");
            }

            return sb.ToString();
        }

        private string BuildModifiers()
        {
            var parts = new List<string> { Syntax.VisibilityToString(_visibility) };

            if (_isStatic) parts.Add(Constants.Static);
            if (_isVirtual) parts.Add(Constants.Virtual);
            if (_isOverride) parts.Add(Constants.Override);
            if (_isAbstract) parts.Add(Constants.Abstract);
            if (_isPartial) parts.Add(Constants.Partial);

            return string.Join(" ", parts) + " ";
        }
    }
}