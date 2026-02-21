# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.0.1-exp.5] - 2026-02-21

### Added

- `StructBuilder.WithConstructorIf` — conditionally adds a constructor to a struct based on a boolean condition, useful when generating code from models where constructors may only be needed in certain cases.

### Fixed

- `StructBuilder` constructor validation now properly enforces that explicit struct constructors must have at least one parameter, as required by C#.
- `FieldBuilder.WithDefaultValue` validation for const fields moved to `Emit()` time, throwing `InvalidOperationException` when const fields have empty or whitespace default values.

## [0.0.1-exp.4] - 2026-02-21

### Added

- `FieldBuilder.WithConstModifier` — adds the `const` modifier to field declarations, making them compile-time constants.

## [0.0.1-exp.3] - 2026-02-21

### Added

- `CodeBlockBuilder.BlankLine` — emits an empty line to visually separate logical sections of code within method bodies.

## [0.0.1-exp.2] - 2026-02-20

### Added

- `FileBuilder.RelativePath` — public readonly property exposing the relative path where the file will be saved.
- `ClassBuilder.WithBaseClass` — added support for class inheritance; XML documentation added for both overloads (string and CsType).

## [0.0.1-exp.1] - 2026-02-20

### Added

- Initial release.

- `FileBuilder` — top-level builder for generating complete `.cs` files. Supports adding `using` statements (single, variadic, and collection overloads), setting a namespace, and adding classes and structs. Output can be returned as a string via `Emit()`, written to the path provided at construction via `Save()`, or written to an arbitrary path via `SaveTo()`.

- `ClassBuilder` — generates `class` declarations with configurable visibility, and `static`, `sealed`, `abstract`, and `partial` modifiers. Supports generic type parameters with `where` constraints. Members are added via `WithField()`, `WithProperty()`, `WithMethod()`, and `WithConstructor()`, each available with an `out` parameter overload and a collection overload (`WithFields<T>`, `WithProperties<T>`).

- `StructBuilder` — generates `struct` declarations. Same member API as `ClassBuilder` with a `readonly` modifier instead of sealed/abstract.

- `MethodBuilder` — generates method declarations with configurable visibility and `static`, `override`, `virtual`, `abstract`, and `partial` modifiers. Supports generic type parameters and `where` constraints (single and collection overloads). Parameters are added individually or from a collection. Abstract methods emit no body.

- `PropertyBuilder` — generates property declarations with auto accessors (`WithAutoGetter`, `WithAutoSetter`), expression-bodied getters (`WithExpressionGetter`), and explicit getter/setter bodies (`WithGetter`, `WithSetter`). Supports per-accessor visibility and a static modifier.

- `FieldBuilder` — generates field declarations with configurable visibility (default `private`), `static` and `readonly` modifiers, and an optional default value.

- `ConstructorBuilder` — generates constructor declarations with configurable visibility, parameters (single and collection overloads), a `base(...)` or `this(...)` initialiser call, and a configurable body.

- `CodeBlockBuilder` — generates imperative code statements for use inside method, constructor, and property accessor bodies:
  - `DeclareLocal` — `var` or explicitly typed local variable declaration.
  - `Assign` — simple assignment statement.
  - `CompoundAssign` — compound assignment (`+=`, `-=`, etc.).
  - `Call` — method call, with or without a target object.
  - `CallAndAssign` — method call whose return value is captured into a new local (`var` or explicit type).
  - `Return` — return statement, with or without a value.
  - `If` / `else` — conditional block.
  - `ForEach` — `foreach` loop over a collection.
  - `For` — `for` loop with explicit initialiser, condition, and iterator.
  - `Raw` — escape hatch for emitting arbitrary code lines.

- `CsType` — immutable value representing a C# type, including nested generics. Predefined constants: `Void`, `Int`, `Float`, `Bool`, `String`. Factory methods: `Of(name)` for non-generic types, `Generic(name, ...typeArgs)` for arbitrary generics, `ListOf(elementType)` for `List<T>`, and `DictionaryOf(keyType, valueType)` for `Dictionary<TKey, TValue>`.

- `XmlDocBuilder` — generates `///` XML documentation comments. Supports `<summary>`, `<remarks>`, `<param>`, `<typeparam>`, `<returns>`, `<exception>`, and `<inheritdoc>`. Available on all builder types via `WithXmlDoc(...)`.

- `IndentEmitter` — manages tab-based indentation state shared across nested builders. Exposes `Push()`, `Pop()`, `Get()`, and `Line(content)`.

- `Visibility` enum — `Public`, `Private`, `Protected`, `Internal`, `ProtectedInternal`.

- NUnit Editor test suites for `CsType`, `IndentEmitter`, and `XmlDocBuilder`.
