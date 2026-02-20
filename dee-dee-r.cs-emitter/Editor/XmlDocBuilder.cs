using System;
using System.Collections.Generic;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for creating XML documentation comments for C# code generation.
    /// </summary>
    public sealed class XmlDocBuilder
    {
        private string _summary;
        private string _remarks;
        private string _returns;
        private bool _inheritDoc;
        private readonly List<(string Name, string Description)> _params = new ();
        private readonly List<(string Name, string Description)> _typeParams = new ();
        private readonly List<(string CRef, string Description)> _exceptions = new ();

        private XmlDocBuilder() { }

        /// <summary>
        /// Creates a new instance of <see cref="XmlDocBuilder"/>.
        /// </summary>
        /// <returns>A new <see cref="XmlDocBuilder"/> instance.</returns>
        public static XmlDocBuilder Build() => new XmlDocBuilder();

        /// <summary>
        /// Sets the summary documentation.
        /// </summary>
        /// <param name="summary">The summary text.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public XmlDocBuilder WithSummary(string summary)
        {
            _summary = summary;
            return this;
        }

        /// <summary>
        /// Sets the remarks documentation.
        /// </summary>
        /// <param name="remarks">The remarks text.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public XmlDocBuilder WithRemarks(string remarks)
        {
            _remarks = remarks;
            return this;
        }

        /// <summary>
        /// Sets the return value documentation.
        /// </summary>
        /// <param name="returns">The description of the return value.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public XmlDocBuilder WithReturns(string returns)
        {
            _returns = returns;
            return this;
        }

        /// <summary>
        /// Adds a parameter documentation entry.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="description">The parameter description.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public XmlDocBuilder WithParam(string name, string description)
        {
            _params.Add((name, description));
            return this;
        }

        /// <summary>
        /// Adds a type parameter documentation entry.
        /// </summary>
        /// <param name="name">The type parameter name.</param>
        /// <param name="description">The type parameter description.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public XmlDocBuilder WithTypeParam(string name, string description)
        {
            _typeParams.Add((name, description));
            return this;
        }

        /// <summary>
        /// Adds an exception documentation entry.
        /// </summary>
        /// <param name="cref">The exception type reference.</param>
        /// <param name="description">The description of when the exception is thrown.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public XmlDocBuilder WithException(string cref, string description)
        {
            _exceptions.Add((cref, description));
            return this;
        }

        /// <summary>
        /// Marks the documentation to inherit from a base member.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public XmlDocBuilder WithInheritDoc()
        {
            _inheritDoc = true;
            return this;
        }

        /// <summary>
        /// Emits the XML documentation comments as a formatted string.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for proper formatting.</param>
        /// <returns>A string containing the formatted XML documentation comments.</returns>
        public string Emit(IndentEmitter indentEmitter)
        {
            // inheritdoc short-circuits everything else
            if (_inheritDoc)
                return $"{indentEmitter.Get()}/// <{Constants.InheritDoc}/>\n";

            var sb = new StringBuilder();
            var indent = indentEmitter.Get();

            if (_summary != null)
                EmitBlock(sb, indent, Constants.Summary, _summary);

            if (_remarks != null)
                EmitBlock(sb, indent, Constants.Remarks, _remarks);

            foreach (var (name, description) in _typeParams)
                EmitInlineOrBlock(sb, indent, Constants.TypeParam, $"{Constants.Name}=\"{name}\"", description);

            foreach (var (name, description) in _params)
                EmitInlineOrBlock(sb, indent, Constants.Param, $"{Constants.Name}=\"{name}\"", description);

            if (_returns != null)
                EmitInlineOrBlock(sb, indent, Constants.Returns, null, _returns);

            foreach (var (cref, description) in _exceptions)
                EmitInlineOrBlock(sb, indent, Constants.Exception, $"{Constants.Cref}=\"{cref}\"", description);

            return sb.ToString();
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        // Multi-line block:
        // /// <summary>
        // /// Line one.
        // /// Line two.
        // /// </summary>
        private static void EmitBlock(StringBuilder sb, string indent, string tag, string content)
        {
            sb.AppendLine($"{indent}/// <{tag}>");
            foreach (var line in SplitLines(content))
                sb.AppendLine($"{indent}/// {line}");
            sb.AppendLine($"{indent}/// </{tag}>");
        }

        // Single-line if content fits on one line, block if multiline:
        // /// <param name="x">The x value.</param>
        // vs
        // /// <param name="x">
        // /// Line one.
        // /// Line two.
        // /// </param>
        private static void EmitInlineOrBlock(StringBuilder sb, string indent, string tag, string attributes, string content)
        {
            var openTag = attributes != null ? $"<{tag} {attributes}>" : $"<{tag}>";
            var closeTag = $"</{tag}>";
            var lines = SplitLines(content);

            if (lines.Length == 1)
            {
                sb.AppendLine($"{indent}/// {openTag}{lines[0]}{closeTag}");
            }
            else
            {
                sb.AppendLine($"{indent}/// {openTag}");
                foreach (var line in lines)
                    sb.AppendLine($"{indent}/// {line}");
                sb.AppendLine($"{indent}/// {closeTag}");
            }
        }

        private static string[] SplitLines(string content)
            => content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    }
}