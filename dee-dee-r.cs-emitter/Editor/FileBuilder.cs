using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# source code files with usings, namespaces, classes, and structs.
    /// </summary>
    /// <example>
    /// <code>
    /// CsFileBuilder.Build(ctx, "Generated/MyClass.cs")
    ///     .WithUsings("System.Collections.Generic", "UnityEngine")
    ///     .WithNamespace("MyProject.Generated")
    ///     .WithClass("MyClass", cls => cls
    ///         .WithSealedModifier()
    ///         .WithField("_data", CsType.Int))
    ///     .Save();
    /// </code>
    /// </example>
    public sealed class FileBuilder
    {
        private readonly IndentEmitter _indentEmitter = new ();
        private readonly List<string> _usings = new ();
        private string _namespace;
        private readonly List<ClassBuilder> _classes = new ();
        private readonly List<StructBuilder> _structs = new ();
        private readonly string _relativePath;

        private FileBuilder(string relativePath)
        {
            _relativePath = relativePath;
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileBuilder"/>.
        /// </summary>
        /// <param name="relativePath">The relative path where the file will be saved.</param>
        /// <returns>A new <see cref="FileBuilder"/> instance.</returns>
        public static FileBuilder Build(string relativePath)
            => new FileBuilder(relativePath);

        /// <summary>
        /// Adds a using directive for the specified namespace.
        /// </summary>
        /// <param name="namespaceName">The namespace to add.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithUsing(string namespaceName)
        {
            if (!_usings.Contains(namespaceName))
                _usings.Add(namespaceName);
            return this;
        }

        /// <summary>
        /// Adds multiple using directives for the specified namespaces.
        /// </summary>
        /// <param name="namespaceNames">The namespaces to add.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithUsings(params string[] namespaceNames)
        {
            foreach (var ns in namespaceNames)
                WithUsing(ns);
            return this;
        }

        /// <summary>
        /// Adds multiple using directives by selecting namespace names from a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="nameSelector">A function to extract the namespace name from each element.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithUsings<T>(IEnumerable<T> source, Func<T, string> nameSelector)
        {
            foreach (var item in source)
                WithUsing(nameSelector(item));
            return this;
        }

        /// <summary>
        /// Sets the namespace for the generated file.
        /// </summary>
        /// <param name="namespaceName">The namespace name.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithNamespace(string namespaceName)
        {
            _namespace = namespaceName;
            return this;
        }

        /// <summary>
        /// Adds a class to the file.
        /// </summary>
        /// <param name="className">The name of the class.</param>
        /// <param name="configure">An optional action to configure the class builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithClass(string className, Action<ClassBuilder> configure = null)
        {
            var builder = ClassBuilder.Build(_indentEmitter, className);
            configure?.Invoke(builder);
            _classes.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a struct to the file.
        /// </summary>
        /// <param name="structName">The name of the struct.</param>
        /// <param name="configure">An optional action to configure the struct builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithStruct(string structName, Action<StructBuilder> configure = null)
        {
            var builder = StructBuilder.Build(_indentEmitter, structName);
            configure?.Invoke(builder);
            _structs.Add(builder);
            return this;
        }

        /// <summary>
        /// Adds a class to the file and returns the class builder via an out parameter.
        /// </summary>
        /// <param name="className">The name of the class.</param>
        /// <param name="classBuilder">The class builder instance.</param>
        /// <param name="configure">An optional action to configure the class builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithClass(string className, out ClassBuilder classBuilder, Action<ClassBuilder> configure = null)
        {
            classBuilder = ClassBuilder.Build(_indentEmitter, className);
            configure?.Invoke(classBuilder);
            _classes.Add(classBuilder);
            return this;
        }

        /// <summary>
        /// Adds a struct to the file and returns the struct builder via an out parameter.
        /// </summary>
        /// <param name="structName">The name of the struct.</param>
        /// <param name="structBuilder">The struct builder instance.</param>
        /// <param name="configure">An optional action to configure the struct builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public FileBuilder WithStruct(string structName, out StructBuilder structBuilder, Action<StructBuilder> configure = null)
        {
            structBuilder = StructBuilder.Build(_indentEmitter, structName);
            configure?.Invoke(structBuilder);
            _structs.Add(structBuilder);
            return this;
        }

        // -------------------------------------------------------------------------
        // Emit and Save
        // -------------------------------------------------------------------------

        public string Emit()
        {
            _indentEmitter.Reset();
            var sb = new StringBuilder();

            // Usings
            if (_usings.Count > 0)
            {
                foreach (var u in _usings.OrderBy(u => u))
                    sb.AppendLine($"{Constants.Using} {u};");
                sb.AppendLine();
            }

            // Namespace open
            var hasNamespace = !string.IsNullOrWhiteSpace(_namespace);
            if (hasNamespace)
            {
                sb.AppendLine($"{Constants.Namespace} {_namespace}");
                sb.AppendLine("{");
                _indentEmitter.Push();
            }

            // Classes
            foreach (var cls in _classes)
            {
                sb.Append(cls.Emit());
                sb.AppendLine();
            }

            // Structs
            foreach (var str in _structs)
            {
                sb.Append(str.Emit());
                sb.AppendLine();
            }

            // Namespace close
            if (hasNamespace)
            {
                _indentEmitter.Pop();
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        public void Save()
        {
            this.SaveTo(this._relativePath);
        }

        public void SaveTo(string filePath)
        {
            var content = Emit();
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, content, Encoding.UTF8);
        }
    }
}