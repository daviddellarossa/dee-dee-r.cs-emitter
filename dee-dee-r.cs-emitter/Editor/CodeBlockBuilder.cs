using System;
using System.Collections.Generic;
using System.Text;

namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// A fluent builder for generating C# code blocks containing statements like assignments, method calls, loops, and conditionals.
    /// </summary>
    /// <example>
    /// <code>
    /// body => body
    ///     .DeclareLocal("instance", "new MyClass()")
    ///     .Assign("_instance", "instance")
    ///     .Call("_instance", "Setup", "true", "42")
    ///     .ForEach(CsType.Of("Vector3"), "point", "_points", forEach => forEach
    ///         .Call("ProcessPoint", "point"))
    ///     .If("_instance.IsReady",
    ///         thenBody => thenBody
    ///             .Call("OnReady")
    ///             .Assign("_isInitialized", "true"),
    ///         elseBody => elseBody
    ///             .Call("LogError", "\"Not ready\""))
    /// </code>
    /// </example>
    public sealed class CodeBlockBuilder
    {
        private readonly IndentEmitter _indentEmitter;
        private readonly List<IStatement> _statements = new List<IStatement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBlockBuilder"/> class.
        /// </summary>
        /// <param name="indentEmitter">The indentation emitter for formatting.</param>
        public CodeBlockBuilder(IndentEmitter indentEmitter)
        {
            _indentEmitter = indentEmitter;
        }

        /// <summary>
        /// Adds a local variable declaration with an explicit type.
        /// </summary>
        /// <param name="type">The type of the local variable.</param>
        /// <param name="name">The name of the local variable.</param>
        /// <param name="value">The optional initialization value.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder DeclareLocal(CsType type, string name, string value = null)
        {
            _statements.Add(new LocalDeclarationStatement(type, name, value));
            return this;
        }

        /// <summary>
        /// Adds a local variable declaration using type inference (var).
        /// </summary>
        /// <param name="name">The name of the local variable.</param>
        /// <param name="value">The optional initialization value.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder DeclareLocal(string name, string value = null)
        {
            _statements.Add(new LocalDeclarationStatement(null, name, value));
            return this;
        }

        /// <summary>
        /// Adds an assignment statement.
        /// </summary>
        /// <param name="target">The target variable or field to assign to.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder Assign(string target, string value)
        {
            _statements.Add(new AssignmentStatement(target, value));
            return this;
        }

        /// <summary>
        /// Adds a compound assignment statement (e.g., +=, -=, *=, etc.).
        /// </summary>
        /// <param name="target">The target variable or field.</param>
        /// <param name="op">The operator (e.g., "+", "-", "*").</param>
        /// <param name="value">The value to apply.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder CompoundAssign(string target, string op, string value)
        {
            _statements.Add(new CompoundAssignmentStatement(target, op, value));
            return this;
        }

        /// <summary>
        /// Adds a method call statement without a target (static or local method call).
        /// </summary>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder Call(string methodName, params string[] args)
        {
            _statements.Add(new MethodCallStatement(null, methodName, args));
            return this;
        }

        /// <summary>
        /// Adds a method call statement on a target object.
        /// </summary>
        /// <param name="target">The target object to call the method on.</param>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder Call(string target, string methodName, params string[] args)
        {
            _statements.Add(new MethodCallStatement(target, methodName, args));
            return this;
        }

        /// <summary>
        /// Adds a method call and assigns the result to a new variable using type inference (var).
        /// </summary>
        /// <param name="resultName">The name of the variable to store the result.</param>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder CallAndAssign(string resultName, string methodName, params string[] args)
        {
            _statements.Add(new CallAndAssignStatement(null, resultName, null, methodName, args));
            return this;
        }

        /// <summary>
        /// Adds a method call on a target object and assigns the result to a new variable using type inference (var).
        /// </summary>
        /// <param name="resultName">The name of the variable to store the result.</param>
        /// <param name="target">The target object to call the method on.</param>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder CallAndAssign(string resultName, string target, string methodName, params string[] args)
        {
            _statements.Add(new CallAndAssignStatement(target, resultName, null, methodName, args));
            return this;
        }

        /// <summary>
        /// Adds a method call and assigns the result to a new variable with an explicit type.
        /// </summary>
        /// <param name="resultType">The type of the result variable.</param>
        /// <param name="resultName">The name of the variable to store the result.</param>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder CallAndAssign(CsType resultType, string resultName, string methodName, params string[] args)
        {
            _statements.Add(new CallAndAssignStatement(null, resultName, resultType, methodName, args));
            return this;
        }

        /// <summary>
        /// Adds a return statement.
        /// </summary>
        /// <param name="value">The optional value to return.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder Return(string value = null)
        {
            _statements.Add(new ReturnStatement(value));
            return this;
        }

        /// <summary>
        /// Adds an if statement with optional else block.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="thenBody">An action to configure the then block.</param>
        /// <param name="elseBody">An optional action to configure the else block.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder If(string condition, Action<CodeBlockBuilder> thenBody, Action<CodeBlockBuilder> elseBody = null)
        {
            _statements.Add(new IfStatement(condition, thenBody, elseBody));
            return this;
        }

        /// <summary>
        /// Adds a foreach loop statement.
        /// </summary>
        /// <param name="itemType">The type of the loop variable.</param>
        /// <param name="itemName">The name of the loop variable.</param>
        /// <param name="collection">The collection to iterate over.</param>
        /// <param name="body">An action to configure the loop body.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder ForEach(CsType itemType, string itemName, string collection, Action<CodeBlockBuilder> body)
        {
            _statements.Add(new ForEachStatement(itemType, itemName, collection, body));
            return this;
        }

        /// <summary>
        /// Adds a for loop statement.
        /// </summary>
        /// <param name="initializer">The loop initializer (e.g., "int i = 0").</param>
        /// <param name="condition">The loop condition (e.g., "i &lt; count").</param>
        /// <param name="iterator">The loop iterator (e.g., "i++").</param>
        /// <param name="body">An action to configure the loop body.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder For(string initializer, string condition, string iterator, Action<CodeBlockBuilder> body)
        {
            _statements.Add(new ForStatement(initializer, condition, iterator, body));
            return this;
        }

        /// <summary>
        /// Adds a raw statement line for custom code not covered by other methods.
        /// </summary>
        /// <param name="line">The raw code line to add.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public CodeBlockBuilder Raw(string line)
        {
            _statements.Add(new RawStatement(line));
            return this;
        }

        /// <summary>
        /// Emits the complete code block as a string.
        /// </summary>
        /// <returns>A string containing the generated code block.</returns>
        public string Emit()
        {
            var sb = new StringBuilder();
            foreach (var statement in _statements)
                sb.Append(statement.Emit(_indentEmitter));
            return sb.ToString();
        }

        // -------------------------------------------------------------------------
        // Statement interfaces and implementations
        // -------------------------------------------------------------------------

        private interface IStatement
        {
            string Emit(IndentEmitter indentEmitter);
        }

        private sealed class LocalDeclarationStatement : IStatement
        {
            private readonly CsType _type;
            private readonly string _name;
            private readonly string _value;

            public LocalDeclarationStatement(CsType type, string name, string value)
            {
                _type = type;
                _name = name;
                _value = value;
            }

            public string Emit(IndentEmitter ie)
            {
                var typePart = _type != null ? _type.Emit() : Constants.Var;
                return _value != null
                    ? $"{ie.Get()}{typePart} {_name} = {_value};\n"
                    : $"{ie.Get()}{typePart} {_name};\n";
            }
        }

        private sealed class AssignmentStatement : IStatement
        {
            private readonly string _target;
            private readonly string _value;

            public AssignmentStatement(string target, string value)
            {
                _target = target;
                _value = value;
            }

            public string Emit(IndentEmitter ie)
                => $"{ie.Get()}{_target} = {_value};\n";
        }

        private sealed class CompoundAssignmentStatement : IStatement
        {
            private readonly string _target;
            private readonly string _op;
            private readonly string _value;

            public CompoundAssignmentStatement(string target, string op, string value)
            {
                _target = target;
                _op = op;
                _value = value;
            }

            public string Emit(IndentEmitter ie)
                => $"{ie.Get()}{_target} {_op}= {_value};\n";
        }

        private sealed class MethodCallStatement : IStatement
        {
            private readonly string _target;
            private readonly string _methodName;
            private readonly string[] _args;

            public MethodCallStatement(string target, string methodName, string[] args)
            {
                _target = target;
                _methodName = methodName;
                _args = args;
            }

            public string Emit(IndentEmitter ie)
            {
                var call = _target != null
                    ? $"{_target}.{_methodName}({string.Join(", ", _args)})"
                    : $"{_methodName}({string.Join(", ", _args)})";
                return $"{ie.Get()}{call};\n";
            }
        }

        private sealed class CallAndAssignStatement : IStatement
        {
            private readonly string _target;
            private readonly string _resultName;
            private readonly CsType _resultType;
            private readonly string _methodName;
            private readonly string[] _args;

            public CallAndAssignStatement(string target, string resultName, CsType resultType, string methodName, string[] args)
            {
                _target = target;
                _resultName = resultName;
                _resultType = resultType;
                _methodName = methodName;
                _args = args;
            }

            public string Emit(IndentEmitter ie)
            {
                var typePart = _resultType != null ? _resultType.Emit() : Constants.Var;
                var call = _target != null
                    ? $"{_target}.{_methodName}({string.Join(", ", _args)})"
                    : $"{_methodName}({string.Join(", ", _args)})";
                return $"{ie.Get()}{typePart} {_resultName} = {call};\n";
            }
        }

        private sealed class ReturnStatement : IStatement
        {
            private readonly string _value;

            public ReturnStatement(string value) => _value = value;

            public string Emit(IndentEmitter ie)
                => _value != null
                    ? $"{ie.Get()}{Constants.Return} {_value};\n"
                    : $"{ie.Get()}{Constants.Return};\n";
        }

        private sealed class IfStatement : IStatement
        {
            private readonly string _condition;
            private readonly Action<CodeBlockBuilder> _thenBody;
            private readonly Action<CodeBlockBuilder> _elseBody;

            public IfStatement(string condition, Action<CodeBlockBuilder> thenBody, Action<CodeBlockBuilder> elseBody)
            {
                _condition = condition;
                _thenBody = thenBody;
                _elseBody = elseBody;
            }

            public string Emit(IndentEmitter ie)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{ie.Get()}{Constants.If} ({_condition})");
                sb.AppendLine($"{ie.Get()}{{");
                ie.Push();
                var thenBuilder = new CodeBlockBuilder(ie);
                _thenBody(thenBuilder);
                sb.Append(thenBuilder.Emit());
                ie.Pop();
                sb.AppendLine($"{ie.Get()}}}");

                if (_elseBody != null)
                {
                    sb.AppendLine($"{ie.Get()}{Constants.Else}");
                    sb.AppendLine($"{ie.Get()}{{");
                    ie.Push();
                    var elseBuilder = new CodeBlockBuilder(ie);
                    _elseBody(elseBuilder);
                    sb.Append(elseBuilder.Emit());
                    ie.Pop();
                    sb.AppendLine($"{ie.Get()}}}");
                }

                return sb.ToString();
            }
        }

        private sealed class ForEachStatement : IStatement
        {
            private readonly CsType _itemType;
            private readonly string _itemName;
            private readonly string _collection;
            private readonly Action<CodeBlockBuilder> _body;

            public ForEachStatement(CsType itemType, string itemName, string collection, Action<CodeBlockBuilder> body)
            {
                _itemType = itemType;
                _itemName = itemName;
                _collection = collection;
                _body = body;
            }

            public string Emit(IndentEmitter ie)
            {
                var sb = new StringBuilder();
                var typePart = _itemType != null ? _itemType.Emit() : Constants.Var;
                sb.AppendLine($"{ie.Get()}{Constants.Foreach} ({typePart} {_itemName} {Constants.In} {_collection})");
                sb.AppendLine($"{ie.Get()}{{");
                ie.Push();
                var bodyBuilder = new CodeBlockBuilder(ie);
                _body(bodyBuilder);
                sb.Append(bodyBuilder.Emit());
                ie.Pop();
                sb.AppendLine($"{ie.Get()}}}");
                return sb.ToString();
            }
        }

        private sealed class ForStatement : IStatement
        {
            private readonly string _initializer;
            private readonly string _condition;
            private readonly string _iterator;
            private readonly Action<CodeBlockBuilder> _body;

            public ForStatement(string initializer, string condition, string iterator, Action<CodeBlockBuilder> body)
            {
                _initializer = initializer;
                _condition = condition;
                _iterator = iterator;
                _body = body;
            }

            public string Emit(IndentEmitter ie)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{ie.Get()}{Constants.For} ({_initializer}; {_condition}; {_iterator})");
                sb.AppendLine($"{ie.Get()}{{");
                ie.Push();
                var bodyBuilder = new CodeBlockBuilder(ie);
                _body(bodyBuilder);
                sb.Append(bodyBuilder.Emit());
                ie.Pop();
                sb.AppendLine($"{ie.Get()}}}");
                return sb.ToString();
            }
        }

        private sealed class RawStatement : IStatement
        {
            private readonly string _line;
            public RawStatement(string line) => _line = line;
            public string Emit(IndentEmitter ie) => $"{ie.Get()}{_line}\n";
        }
    }
}