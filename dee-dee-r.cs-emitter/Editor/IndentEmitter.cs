using System;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// Manages indentation levels for code generation, using tabs as the indentation character.
    /// </summary>
    /// <example>
    /// <code>
    /// sb.AppendLine($"{_indentEmitter.Get()}{{");
    /// _indentEmitter.Push();
    /// // ... emit body ...
    /// _indentEmitter.Pop();
    /// sb.AppendLine($"{_indentEmitter.Get()}}");
    /// </code>
    /// <code>
    /// sb.AppendLine(_indentEmitter.Line("{"));
    /// _indentEmitter.Push();
    /// // ... emit body ...
    /// _indentEmitter.Pop();
    /// sb.AppendLine(_indentEmitter.Line("}"));
    /// </code>
    /// </example>
    public sealed class IndentEmitter
    {
        private int _count;
        private const char IndentChar = '\t';

        /// <summary>
        /// Resets the indentation level to zero.
        /// </summary>
        public void Reset() => _count = 0;

        /// <summary>
        /// Gets the current indentation string based on the current indentation level.
        /// </summary>
        /// <returns>A string containing the appropriate number of tab characters.</returns>
        public string Get() => new string(IndentChar, _count);

        /// <summary>
        /// Increases the indentation level by one.
        /// </summary>
        public void Push() => _count++;

        /// <summary>
        /// Decreases the indentation level by one. The indentation level will not go below zero.
        /// </summary>
        public void Pop()
        {
            _count--;
            _count = Math.Max(0, _count);
        }

        /// <summary>
        /// Returns a line with the current indentation prepended to the specified content.
        /// </summary>
        /// <param name="content">The content to prepend the indentation to.</param>
        /// <returns>A string with the current indentation followed by the content.</returns>
        public string Line(string content) => $"{Get()}{content}";
    }
}