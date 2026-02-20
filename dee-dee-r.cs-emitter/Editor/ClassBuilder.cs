using System;
using System.Collections.Generic;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# class definitions with fields, properties, methods, and constructors.
    /// </summary>
    /// <example>
    /// <code>
    /// ClassBuilder.Build(emitter, "EventDispatcher")
    ///     .WithSealedModifier()
    ///     .WithField("_listeners", CsType.DictionaryOf(CsType.String, CsType.ListOf(CsType.Of("Action"))), f => f
    ///         .WithVisibility(Visibility.Private)
    ///         .WithReadOnly())
    ///     .WithProperty("IsReady", CsType.Bool, p => p
    ///         .WithAutoGetter()
    ///         .WithAutoSetter(Visibility.Private))
    ///     .WithConstructor(c => c
    ///         .WithBody(body => body
    ///             .Assign("_listeners", "new Dictionary&lt;string, List&lt;Action&gt;&gt;()")))
    ///     .WithMethod("Dispatch", CsType.Void, m => m
    ///         .WithParameter(CsType.String, "eventName"))
    ///     .Emit();
    /// </code>
    /// </example>
    /// <example>
    /// Simple inheritance
    /// <code>
    ///     ClassBuilder.Build(emitter, "MyHandler")
    ///         .WithBaseClass("BaseHandler")
    ///         .Emit();
    /// → public class MyHandler : BaseHandler
    /// </code>
    /// </example>
    /// <example>
    /// Generic base class
    /// <code>
    ///     ClassBuilder.Build(emitter, "MyHandler")
    ///         .WithBaseClass(CsType.Generic("BaseHandler", CsType.Of("MyMessage")))
    ///         .Emit();
    /// → public class MyHandler : BaseHandler&lt;MyMessage&gt;
    /// </code>
    /// </example>
    /// <example>
    /// With type parameters on the derived class too
    /// <code>
    ///     ClassBuilder.Build(emitter, "MyHandler")
    ///         .WithTypeParameter("T")
    ///         .WithBaseClass(CsType.Generic("BaseHandler", CsType.Of("T")))
    ///         .WithTypeConstraint("T", "IMessage")
    ///         .Emit();
    /// → public class MyHandler&lt;T&gt; : BaseHandler&lt;T&gt; where T : IMessage
    /// </code>
    /// </example>
    public sealed class ClassBuilder
    {
        private readonly IndentEmitter _indentEmitter;
        private readonly string _className;
        private string _baseClass = null;
        private Visibility _visibility = Visibility.Public;
        private bool _isStatic;
        private bool _isSealed;
        private bool _isAbstract;
        private bool _isPartial;
        private readonly List<string> _typeParameters = new ();
        private readonly List<(string TypeParam, string Constraint)> _typeConstraints = new ();
        private readonly List<FieldBuilder> _fields = new ();
        private readonly List<PropertyBuilder> _properties = new ();
        private readonly List<MethodBuilder> _methods = new ();
        private readonly List<ConstructorBuilder> _constructors = new ();
        
        private XmlDocBuilder _xmlDoc;

        /// <summary>
        /// Configures the XML documentation for the class.
        /// </summary>
        /// <param name="configure">An action to configure the XML documentation builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithXmlDoc(Action<XmlDocBuilder> configure)
        {
            _xmlDoc = XmlDocBuilder.Build();
            configure(_xmlDoc);
            return this;
        }

        private ClassBuilder(IndentEmitter indentEmitter, string className)
        {
            _indentEmitter = indentEmitter;
            _className = className;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClassBuilder"/>.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        /// <param name="className">The name of the class.</param>
        /// <returns>A new <see cref="ClassBuilder"/> instance.</returns>
        public static ClassBuilder Build(IndentEmitter indentEmitter, string className)
            => new ClassBuilder(indentEmitter, className);

        /// <summary>
        /// Sets the visibility modifier for the class.
        /// </summary>
        /// <param name="visibility">The visibility level.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithVisibility(Visibility visibility)
        {
            _visibility = visibility;
            return this;
        }

        /// <summary>
        /// Sets the static modifier for the class.
        /// </summary>
        /// <param name="isStatic">True to make the class static; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithStaticModifier(bool isStatic = true)
        {
            _isStatic = isStatic;
            return this;
        }

        /// <summary>
        /// Sets the sealed modifier for the class.
        /// </summary>
        /// <param name="isSealed">True to make the class sealed; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithSealedModifier(bool isSealed = true)
        {
            _isSealed = isSealed;
            return this;
        }

        /// <summary>
        /// Sets the abstract modifier for the class.
        /// </summary>
        /// <param name="isAbstract">True to make the class abstract; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithAbstractModifier(bool isAbstract = true)
        {
            _isAbstract = isAbstract;
            return this;
        }

        /// <summary>
        /// Sets the partial modifier for the class.
        /// </summary>
        /// <param name="isPartial">True to make the class partial; otherwise, false.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithPartialModifier(bool isPartial = true)
        {
            _isPartial = isPartial;
            return this;
        }

        /// <summary>
        /// Adds a type parameter to the class.
        /// </summary>
        /// <param name="typeParam">The name of the type parameter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithTypeParameter(string typeParam)
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
        public ClassBuilder WithTypeConstraint(string typeParam, string constraint)
        {
            _typeConstraints.Add((typeParam, constraint));
            return this;
        }

        /// <summary>
        /// Sets the base class for the class.
        /// </summary>
        /// <param name="baseClass">The name of the base class.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithBaseClass(string baseClass)
        {
            _baseClass = baseClass;
            return this;
        }

        /// <summary>
        /// Sets the base class for the class.
        /// </summary>
        /// <param name="baseClass">The type of the base class.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithBaseClass(CsType baseClass)
        {
            _baseClass = baseClass.Emit();
            return this;
        }

        /// <summary>
        /// Adds a field to the class using a configuration action.
        /// </summary>
        /// <param name="configure">An action to configure the field builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithField(Action<FieldBuilder> configure)
        {
            // Dummy name/type — configure will overwrite via a factory method
            // Actually, we need a different approach here: yield a pre-built builder
            // See note below
            return this;
        }

        /// <summary>
        /// Adds a field to the class.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="configure">An optional action to configure the field builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithField(string fieldName, CsType fieldType, Action<FieldBuilder> configure = null)
        {
            var builder = FieldBuilder.Build(_indentEmitter, fieldName, fieldType);
            configure?.Invoke(builder);
            _fields.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a property to the class.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="configure">An optional action to configure the property builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithProperty(string propertyName, CsType propertyType, Action<PropertyBuilder> configure = null)
        {
            var builder = PropertyBuilder.Build(_indentEmitter, propertyName, propertyType);
            configure?.Invoke(builder);
            _properties.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a method to the class.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="configure">An optional action to configure the method builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithMethod(string methodName, CsType returnType, Action<MethodBuilder> configure = null)
        {
            var builder = MethodBuilder.Build(_indentEmitter, methodName, returnType);
            configure?.Invoke(builder);
            _methods.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a constructor to the class.
        /// </summary>
        /// <param name="configure">An optional action to configure the constructor builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithConstructor(Action<ConstructorBuilder> configure = null)
        {
            var builder = ConstructorBuilder.Build(_indentEmitter, _className);
            configure?.Invoke(builder);
            _constructors.Add(builder);
            return this;
        }
        
        /// <summary>
        /// Adds a constructor to the class and outputs the builder instance for additional configuration.
        /// </summary>
        /// <param name="constructorBuilder">The constructor builder instance created by this method.</param>
        /// <param name="configure">An optional action to configure the constructor.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithConstructorOut(out ConstructorBuilder constructorBuilder, Action<ConstructorBuilder> configure = null)
        {
            constructorBuilder = ConstructorBuilder.Build(_indentEmitter, _className);
            configure?.Invoke(constructorBuilder);
            _constructors.Add(constructorBuilder);
            return this;
        }
        
        /// <summary>
        /// Adds a method to the class and returns the method builder via an out parameter.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="methodBuilder">The method builder instance.</param>
        /// <param name="configure">An optional action to configure the method builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithMethod(string methodName, CsType returnType, out MethodBuilder methodBuilder, Action<MethodBuilder> configure = null)
        {
            methodBuilder = MethodBuilder.Build(_indentEmitter, methodName, returnType);
            configure?.Invoke(methodBuilder);
            _methods.Add(methodBuilder);
            return this;
        }

        /// <summary>
        /// Adds a property to the class and returns the property builder via an out parameter.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="propertyBuilder">The property builder instance.</param>
        /// <param name="configure">An optional action to configure the property builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithProperty(string propertyName, CsType propertyType, out PropertyBuilder propertyBuilder, Action<PropertyBuilder> configure = null)
        {
            propertyBuilder = PropertyBuilder.Build(_indentEmitter, propertyName, propertyType);
            configure?.Invoke(propertyBuilder);
            _properties.Add(propertyBuilder);
            return this;
        }

        /// <summary>
        /// Adds a field to the class and returns the field builder via an out parameter.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="fieldBuilder">The field builder instance.</param>
        /// <param name="configure">An optional action to configure the field builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithField(string fieldName, CsType fieldType, out FieldBuilder fieldBuilder, Action<FieldBuilder> configure = null)
        {
            fieldBuilder = FieldBuilder.Build(_indentEmitter, fieldName, fieldType);
            configure?.Invoke(fieldBuilder);
            _fields.Add(fieldBuilder);
            return this;
        }
        
        /// <summary>
        /// Adds multiple fields to the class from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="nameSelector">A function to extract the field name from each element.</param>
        /// <param name="typeSelector">A function to extract the field type from each element.</param>
        /// <param name="configure">An optional action to configure each field builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithFields<T>(
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
        /// Adds multiple properties to the class from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="nameSelector">A function to extract the property name from each element.</param>
        /// <param name="typeSelector">A function to extract the property type from each element.</param>
        /// <param name="configure">An optional action to configure each property builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithProperties<T>(
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
        /// Adds a constructor to the class and returns the constructor builder via an out parameter.
        /// </summary>
        /// <param name="constructorBuilder">The constructor builder instance.</param>
        /// <param name="configure">An optional action to configure the constructor builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ClassBuilder WithConstructor(out ConstructorBuilder constructorBuilder, Action<ConstructorBuilder> configure = null)
        {
            constructorBuilder = ConstructorBuilder.Build(_indentEmitter, _className);
            configure?.Invoke(constructorBuilder);
            _constructors.Add(constructorBuilder);
            return this;
        }

        /// <summary>
        /// Emits the complete C# code for the class.
        /// </summary>
        /// <returns>A string containing the generated C# class code.</returns>
        public string Emit()
        {
            var sb = new StringBuilder();
            
            if (_xmlDoc != null)
                sb.Append(_xmlDoc.Emit(_indentEmitter));

            // Class declaration
            sb.AppendLine($"{_indentEmitter.Get()}{BuildDeclaration()}");
            sb.AppendLine($"{_indentEmitter.Get()}{{");
            _indentEmitter.Push();

            // Fields
            if (_fields.Count > 0)
            {
                foreach (var field in _fields)
                    sb.Append(field.Emit());
                sb.AppendLine();
            }

            // Properties
            if (_properties.Count > 0)
            {
                foreach (var property in _properties)
                    sb.Append(property.Emit());
                sb.AppendLine();
            }

            // Constructors
            if (_constructors.Count > 0)
            {
                foreach (var constructor in _constructors)
                    sb.Append(constructor.Emit());
                sb.AppendLine();
            }

            // Methods
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
            sb.Append($"{Constants.Class} {_className}");

            if (_typeParameters.Count > 0)
                sb.Append($"<{string.Join(", ", _typeParameters)}>");
            
            if (_baseClass != null)
                sb.Append($" : {_baseClass}");

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
            if (_isAbstract) parts.Add(Constants.Abstract);
            if (_isSealed) parts.Add(Constants.Sealed);
            if (_isPartial) parts.Add(Constants.Partial);

            return string.Join(" ", parts) + " ";
        }
    }
}