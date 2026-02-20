using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# constructor definitions.
    /// </summary>
    public sealed class ConstructorBuilder
    {
        /// <summary>
        /// Delegate for configuring the constructor body.
        /// </summary>
        /// <param name="body">The code block builder for the constructor body.</param>
        public delegate void ConstructorBodyBuilder(CodeBlockBuilder body);

        private readonly IndentEmitter _indentEmitter;
        private readonly string _className;
        private Visibility _visibility = Visibility.Public;
        private readonly List<(CsType Type, string Name)> _parameters = new ();
        private string _baseCall;
        private string _thisCall;
        private ConstructorBodyBuilder _body;

        private XmlDocBuilder _xmlDoc;

        /// <summary>
        /// Configures the XML documentation for the constructor.
        /// </summary>
        /// <param name="configure">An action to configure the XML documentation builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ConstructorBuilder WithXmlDoc(Action<XmlDocBuilder> configure)
        {
            _xmlDoc = XmlDocBuilder.Build();
            configure(_xmlDoc);
            return this;
        }

        private ConstructorBuilder(IndentEmitter indentEmitter, string className)
        {
            _indentEmitter = indentEmitter;
            _className = className;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConstructorBuilder"/>.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        /// <param name="className">The name of the class this constructor belongs to.</param>
        /// <returns>A new <see cref="ConstructorBuilder"/> instance.</returns>
        public static ConstructorBuilder Build(IndentEmitter indentEmitter, string className)
            => new ConstructorBuilder(indentEmitter, className);

        /// <summary>
        /// Sets the visibility modifier for the constructor.
        /// </summary>
        /// <param name="visibility">The visibility level.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ConstructorBuilder WithVisibility(Visibility visibility)
        {
            _visibility = visibility;
            return this;
        }

        /// <summary>
        /// Adds a parameter to the constructor.
        /// </summary>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ConstructorBuilder WithParameter(CsType type, string name)
        {
            _parameters.Add((type, name));
            return this;
        }

        /// <summary>
        /// Adds a base constructor call.
        /// </summary>
        /// <param name="args">The arguments to pass to the base constructor.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ConstructorBuilder WithBaseCall(params string[] args)
        {
            _baseCall = $"{Constants.Base}({string.Join(", ", args)})";
            return this;
        }

        /// <summary>
        /// Adds a call to another constructor in the same class (constructor chaining).
        /// </summary>
        /// <param name="args">The arguments to pass to the other constructor.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ConstructorBuilder WithThisCall(params string[] args)
        {
            _thisCall = $"{Constants.This}({string.Join(", ", args)})";
            return this;
        }

        /// <summary>
        /// Configures the body of the constructor.
        /// </summary>
        /// <param name="body">A delegate to configure the constructor body.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ConstructorBuilder WithBody(ConstructorBodyBuilder body)
        {
            _body = body;
            return this;
        }

        /// <summary>
        /// Adds multiple parameters to the constructor from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="typeSelector">A function to extract the parameter type from each element.</param>
        /// <param name="nameSelector">A function to extract the parameter name from each element.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public ConstructorBuilder WithParameters<T>(
            IEnumerable<T> source,
            Func<T, CsType> typeSelector,
            Func<T, string> nameSelector)
        {
            foreach (var item in source)
                _parameters.Add((typeSelector(item), nameSelector(item)));
            return this;
        }

        /// <summary>
        /// Emits the complete C# code for the constructor.
        /// </summary>
        /// <returns>A string containing the generated C# constructor code.</returns>
        public string Emit()
        {
            var sb = new StringBuilder();
            
            if (_xmlDoc != null)
                sb.Append(_xmlDoc.Emit(_indentEmitter));

            var paramList = _parameters.Select(p => $"{p.Type.Emit()} {p.Name}");
            var signature = $"{Syntax.VisibilityToString(_visibility)} {_className}({string.Join(", ", paramList)})";

            // Chain call
            if (_baseCall != null)
                signature += $" : {_baseCall}";
            else if (_thisCall != null)
                signature += $" : {_thisCall}";

            sb.AppendLine($"{_indentEmitter.Get()}{signature}");
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
    }
}