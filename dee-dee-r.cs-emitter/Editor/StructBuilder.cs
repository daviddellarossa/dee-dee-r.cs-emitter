using System;
using System.Collections.Generic;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# struct definitions with fields, properties, methods, and constructors.
    /// </summary>
    /// <example>
    /// <code>
    /// StructBuilder.Build(emitter, "Point3D")
    ///     .WithField("X", CsType.Float, f => f.WithVisibility(Visibility.Public).WithReadOnly())
    ///     .WithField("Y", CsType.Float, f => f.WithVisibility(Visibility.Public).WithReadOnly())
    ///     .WithField("Z", CsType.Float, f => f.WithVisibility(Visibility.Public).WithReadOnly())
    ///     .WithConstructor(c => c
    ///         .WithParameter(CsType.Float, "x")
    ///         .WithParameter(CsType.Float, "y")
    ///         .WithParameter(CsType.Float, "z")
    ///         .WithBody(body => body
    ///             .Assign("X", "x")
    ///             .Assign("Y", "y")
    ///             .Assign("Z", "z")))
    ///     .Emit();
    /// </code>
    /// </example>
    public sealed class StructBuilder
    {
        private readonly IndentEmitter _indentEmitter;
        private readonly string _structName;
        private Visibility _visibility = Visibility.Public;
        private bool _isReadOnly;
        private bool _isPartial;
        private readonly List<string> _typeParameters = new ();
        private readonly List<(string TypeParam, string Constraint)> _typeConstraints = new ();
        private readonly List<FieldBuilder> _fields = new ();
        private readonly List<PropertyBuilder> _properties = new ();
        private readonly List<MethodBuilder> _methods = new ();
        private readonly List<ConstructorBuilder> _constructors = new ();

        private XmlDocBuilder _xmlDoc;

        /// <summary>
        /// Configures the XML documentation for the struct.
        /// </summary>
        /// <param name="configure">An action to configure the XML documentation builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithXmlDoc(Action<XmlDocBuilder> configure)
        {
            _xmlDoc = XmlDocBuilder.Build();
            configure(_xmlDoc);
            return this;
        }

        private StructBuilder(IndentEmitter indentEmitter, string structName)
        {
            _indentEmitter = indentEmitter;
            _structName = structName;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StructBuilder"/>.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        /// <param name="structName">The name of the struct.</param>
        /// <returns>A new <see cref="StructBuilder"/> instance.</returns>
        public static StructBuilder Build(IndentEmitter indentEmitter, string structName)
            => new StructBuilder(indentEmitter, structName);

        /// <summary>
        /// Sets the visibility modifier for the struct.
        /// </summary>
        /// <param name="visibility">The visibility level.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithVisibility(Visibility visibility)
        {
            _visibility = visibility;
            return this;
        }

        /// <summary>
        /// Sets the readonly modifier for the struct.
        /// </summary>
        /// <param name="isReadOnly">True to make the struct readonly; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithReadOnlyModifier(bool isReadOnly = true)
        {
            _isReadOnly = isReadOnly;
            return this;
        }

        /// <summary>
        /// Sets the partial modifier for the struct.
        /// </summary>
        /// <param name="isPartial">True to make the struct partial; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithPartialModifier(bool isPartial = true)
        {
            _isPartial = isPartial;
            return this;
        }

        /// <summary>
        /// Adds a type parameter to the struct.
        /// </summary>
        /// <param name="typeParam">The name of the type parameter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithTypeParameter(string typeParam)
        {
            _typeParameters.Add(typeParam);
            return this;
        }

        /// <summary>
        /// Adds a type constraint for a type parameter.
        /// </summary>
        /// <param name="typeParam">The type parameter name.</param>
        /// <param name="constraint">The constraint for the type parameter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithTypeConstraint(string typeParam, string constraint)
        {
            _typeConstraints.Add((typeParam, constraint));
            return this;
        }

        /// <summary>
        /// Adds a field to the struct.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="configure">An optional action to configure the field builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithField(string fieldName, CsType fieldType, Action<FieldBuilder> configure = null)
        {
            var builder = FieldBuilder.Build(_indentEmitter, fieldName, fieldType);
            configure?.Invoke(builder);
            _fields.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a property to the struct.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="configure">An optional action to configure the property builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithProperty(string propertyName, CsType propertyType, Action<PropertyBuilder> configure = null)
        {
            var builder = PropertyBuilder.Build(_indentEmitter, propertyName, propertyType);
            configure?.Invoke(builder);
            _properties.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a method to the struct.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="configure">An optional action to configure the method builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithMethod(string methodName, CsType returnType, Action<MethodBuilder> configure = null)
        {
            var builder = MethodBuilder.Build(_indentEmitter, methodName, returnType);
            configure?.Invoke(builder);
            _methods.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a constructor to the struct.
        /// </summary>
        /// <param name="configure">An optional action to configure the constructor builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithConstructor(Action<ConstructorBuilder> configure = null)
        {
            var builder = ConstructorBuilder.Build(_indentEmitter, _structName);
            configure?.Invoke(builder);
            _constructors.Add(builder);
            return this;
        }
        
        /// <summary>
        /// Adds a constructor to the struct only if the specified condition is true.
        /// Useful for conditionally adding constructors when driving from a collection.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="configure">An action to configure the constructor builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// structBuilder
        ///     .WithConstructorIf(
        ///         messageModel.MessageArgs.Parameters.Count > 0,
        ///         ctor => ctor
        ///             .WithParameters(messageModel.MessageArgs.Parameters,
        ///                 parameter => CsType.Of(parameter.TypeModel.Type.FullName),
        ///                 parameter => parameter.ToLocalVariableName())
        ///             .WithBody(body =>
        ///             {
        ///                 foreach (var parameter in messageModel.MessageArgs.Parameters)
        ///                     body.Assign(parameter.ToPropertyName(), parameter.ToLocalVariableName());
        ///             }));
        /// </code>
        /// </example>
        public StructBuilder WithConstructorIf(bool condition, Action<ConstructorBuilder> configure = null)
        {
            if (!condition)
                return this;

            return WithConstructor(configure);
        }
        
        /// <summary>
        /// Adds a constructor to the struct and outputs the builder instance for additional configuration.
        /// </summary>
        /// <param name="constructorBuilder">The constructor builder instance created by this method.</param>
        /// <param name="configure">An optional action to configure the constructor.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithConstructorOut(out ConstructorBuilder constructorBuilder, Action<ConstructorBuilder> configure = null)
        {
            constructorBuilder = ConstructorBuilder.Build(_indentEmitter, _structName);
            configure?.Invoke(constructorBuilder);
            _constructors.Add(constructorBuilder);
            return this;
        }
        
        /// <summary>
        /// Adds multiple fields to the struct from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="nameSelector">A function to extract the field name from each element.</param>
        /// <param name="typeSelector">A function to extract the field type from each element.</param>
        /// <param name="configure">An optional action to configure each field builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithFields<T>(
            IEnumerable<T> source,
            Func<T, string> nameSelector,
            Func<T, CsType> typeSelector,
            Action<T, FieldBuilder> configure = null)
        {
            foreach (var item in source)
            {
                var name = nameSelector(item);
                var type = typeSelector(item);
                var builder = FieldBuilder.Build(_indentEmitter, name, type);
                configure?.Invoke(item, builder);
                _fields.Add(builder);
            }
            return this;
        }

        /// <summary>
        /// Adds multiple properties to the struct from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="nameSelector">A function to extract the property name from each element.</param>
        /// <param name="typeSelector">A function to extract the property type from each element.</param>
        /// <param name="configure">An optional action to configure each property builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithProperties<T>(
            IEnumerable<T> source,
            Func<T, string> nameSelector,
            Func<T, CsType> typeSelector,
            Action<T, PropertyBuilder> configure = null)
        {
            foreach (var item in source)
            {
                var name = nameSelector(item);
                var type = typeSelector(item);
                var builder = PropertyBuilder.Build(_indentEmitter, name, type);
                configure?.Invoke(item, builder);
                _properties.Add(builder);
            }
            return this;
        }

        /// <summary>
        /// Adds a method to the struct and returns the method builder via an out parameter.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="methodBuilder">The method builder instance.</param>
        /// <param name="configure">An optional action to configure the method builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithMethod(string methodName, CsType returnType, out MethodBuilder methodBuilder, Action<MethodBuilder> configure = null)
        {
            methodBuilder = MethodBuilder.Build(_indentEmitter, methodName, returnType);
            configure?.Invoke(methodBuilder);
            _methods.Add(methodBuilder);
            return this;
        }

        /// <summary>
        /// Adds a property to the struct and returns the property builder via an out parameter.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="propertyBuilder">The property builder instance.</param>
        /// <param name="configure">An optional action to configure the property builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithProperty(string propertyName, CsType propertyType, out PropertyBuilder propertyBuilder, Action<PropertyBuilder> configure = null)
        {
            propertyBuilder = PropertyBuilder.Build(_indentEmitter, propertyName, propertyType);
            configure?.Invoke(propertyBuilder);
            _properties.Add(propertyBuilder);
            return this;
        }

        /// <summary>
        /// Adds a field to the struct and returns the field builder via an out parameter.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="fieldBuilder">The field builder instance.</param>
        /// <param name="configure">An optional action to configure the field builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public StructBuilder WithField(string fieldName, CsType fieldType, out FieldBuilder fieldBuilder, Action<FieldBuilder> configure = null)
        {
            fieldBuilder = FieldBuilder.Build(_indentEmitter, fieldName, fieldType);
            configure?.Invoke(fieldBuilder);
            _fields.Add(fieldBuilder);
            return this;
        }

        /// <summary>
        /// Emits the complete C# code for the struct.
        /// </summary>
        /// <returns>A string containing the generated C# struct code.</returns>
        public string Emit()
        {
            var sb = new StringBuilder();
            
            if (_xmlDoc != null)
                sb.Append(_xmlDoc.Emit(_indentEmitter));

            sb.AppendLine($"{_indentEmitter.Get()}{BuildDeclaration()}");
            sb.AppendLine($"{_indentEmitter.Get()}{{");
            _indentEmitter.Push();

            if (_fields.Count > 0)
            {
                foreach (var field in _fields)
                    sb.Append(field.Emit());
                sb.AppendLine();
            }

            if (_properties.Count > 0)
            {
                foreach (var property in _properties)
                    sb.Append(property.Emit());
                sb.AppendLine();
            }
            
            if (_constructors.Count > 0)
            {
                foreach (var constructor in _constructors)
                {
                    if (!constructor.HasParameters)
                        throw new InvalidOperationException(
                            $"Struct '{_structName}': explicit constructors must have at least one parameter. Remove the constructor or add parameters to it.");

                    sb.Append(constructor.Emit());
                }
                sb.AppendLine();
            }

            if (_methods.Count > 0)
            {
                foreach (var method in _methods)
                {
                    sb.Append(method.Emit());
                    sb.AppendLine();
                }
            }

            _indentEmitter.Pop();
            sb.AppendLine($"{_indentEmitter.Get()}}}");

            return sb.ToString();
        }

        private string BuildDeclaration()
        {
            var sb = new StringBuilder();

            sb.Append(BuildModifiers());
            sb.Append($"{Constants.Struct} {_structName}");

            if (_typeParameters.Count > 0)
                sb.Append($"<{string.Join(", ", _typeParameters)}>");

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

            if (_isReadOnly) parts.Add(Constants.Readonly);
            if (_isPartial) parts.Add(Constants.Partial);

            return string.Join(" ", parts) + " ";
        }
    }
}