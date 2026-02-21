using System.Collections.Generic;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// Represents a C# attribute to be emitted above a declaration.
    /// </summary>
    public sealed class AttributeBuilder
    {
        private readonly string _attributeName;
        private readonly List<string> _arguments = new();

        private AttributeBuilder(string attributeName)
        {
            _attributeName = attributeName;
        }

        /// <summary>
        /// Creates a new instance of <see cref="AttributeBuilder"/>.
        /// </summary>
        /// <param name="attributeName">The name of the attribute, without the Attribute suffix or brackets.</param>
        /// <returns>A new <see cref="AttributeBuilder"/> instance.</returns>
        public static AttributeBuilder Build(string attributeName)
            => new AttributeBuilder(attributeName);

        /// <summary>
        /// Adds a positional or named argument to the attribute.
        /// </summary>
        /// <param name="argument">The argument expression, e.g. "true" or "menuName = \"My/Menu\"".</param>
        /// <returns>This builder instance for method chaining.</returns>
        public AttributeBuilder WithArgument(string argument)
        {
            _arguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Emits the attribute as a string, e.g. [SerializeField] or [CreateAssetMenu(menuName = "My/Menu")].
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        /// <returns>A string containing the generated attribute.</returns>
        public string Emit(IndentEmitter indentEmitter)
        {
            if (_arguments.Count == 0)
                return $"{indentEmitter.Get()}[{_attributeName}]\n";

            return $"{indentEmitter.Get()}[{_attributeName}({string.Join(", ", _arguments)})]\n";
        }
    }
}