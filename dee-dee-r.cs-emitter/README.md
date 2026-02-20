# dee-dee-r.cs-emitter

A fluent builder library for programmatically generating C# source code in Unity Editor scripts. Build complete `.cs` files — classes, structs, methods, properties, fields, constructors, and XML documentation — through a chainable API with proper indentation and formatting handled automatically.

**Namespace:** `DeeDeeR.CsEmitter`
**Unity Version:** 6000.3+

---

## Installation

Add the package via the Unity Package Manager using the git URL, or place the package folder under your project's `Packages/` directory.

---

## Quick Start

```csharp
using DeeDeeR.CsEmitter;

FileBuilder.Build("Generated/EventDispatcher.cs")
    .WithUsings("System", "System.Collections.Generic")
    .WithNamespace("MyProject.Generated")
    .WithClass("EventDispatcher", cls => cls
        .WithSealedModifier()
        .WithField("_listeners", CsType.DictionaryOf(CsType.String, CsType.ListOf(CsType.Of("Action"))), f => f
            .WithReadOnly())
        .WithConstructor(c => c
            .WithBody(body => body
                .Assign("_listeners", "new Dictionary<string, List<Action>>()")))
        .WithMethod("Dispatch", CsType.Void, m => m
            .WithVisibility(Visibility.Public)
            .WithParameter(CsType.String, "eventName")
            .WithBody(body => body
                .If("_listeners.TryGetValue(eventName, out var handlers)",
                    then => then.ForEach(CsType.Of("Action"), "handler", "handlers", forEach => forEach
                        .Call("handler", "Invoke"))))))
    .Save();
```

---

## API Reference

### `FileBuilder`

Top-level builder for generating a complete `.cs` file.

```csharp
FileBuilder.Build("Generated/MyClass.cs")
    .WithUsing("UnityEngine")
    .WithUsings("System", "System.Collections.Generic")
    .WithNamespace("MyProject")
    .WithClass("MyClass", cls => { ... })
    .WithStruct("MyStruct", s => { ... })
    .Save();                          // save to path specified in Build()
    // .SaveTo("Other/Path.cs")       // save to a different path
    // .Emit()                        // return file content as string
```

| Method | Description |
|---|---|
| `Build(relativePath)` | Create a new FileBuilder targeting the given path |
| `WithUsing(name)` | Add a single `using` statement |
| `WithUsings(params names)` | Add multiple `using` statements |
| `WithUsings<T>(source, nameSelector)` | Add usings from a collection |
| `WithNamespace(name)` | Set the file namespace |
| `WithClass(name, configure?)` | Add a class (optional `out ClassBuilder` overload) |
| `WithStruct(name, configure?)` | Add a struct (optional `out StructBuilder` overload) |
| `Emit()` | Return the generated file content as a `string` |
| `Save()` | Write to the path specified in `Build()` |
| `SaveTo(filePath)` | Write to a custom file path |

---

### `ClassBuilder`

Generates a `class` declaration.

```csharp
.WithClass("MyClass", cls => cls
    .WithVisibility(Visibility.Public)
    .WithSealedModifier()
    .WithTypeParameter("T")
    .WithTypeConstraint("T", "new()")
    .WithXmlDoc(doc => doc.WithSummary("My generated class."))
    .WithField("_value", CsType.Int, f => { ... })
    .WithProperty("Value", CsType.Int, p => { ... })
    .WithConstructor(c => { ... })
    .WithMethod("DoWork", CsType.Void, m => { ... }))
```

**Modifiers:** `WithStaticModifier()`, `WithSealedModifier()`, `WithAbstractModifier()`, `WithPartialModifier()`

**Batch helpers:** `WithFields<T>(source, ...)`, `WithProperties<T>(source, ...)` — add multiple members from a collection using selector functions.

All member methods have an `out` parameter overload to capture the builder instance after configuration.

---

### `StructBuilder`

Generates a `struct` declaration. Same API as `ClassBuilder` with `WithReadOnlyModifier()` instead of sealed/abstract.

```csharp
.WithStruct("Point3D", s => s
    .WithReadOnlyModifier()
    .WithField("X", CsType.Float, f => f.WithVisibility(Visibility.Public).WithReadOnly())
    .WithField("Y", CsType.Float, f => f.WithVisibility(Visibility.Public).WithReadOnly())
    .WithConstructor(c => c
        .WithParameter(CsType.Float, "x")
        .WithParameter(CsType.Float, "y")
        .WithBody(body => body
            .Assign("X", "x")
            .Assign("Y", "y"))))
```

---

### `FieldBuilder`

Generates a field declaration.

```csharp
.WithField("_count", CsType.Int, f => f
    .WithVisibility(Visibility.Private)
    .WithReadOnly()
    .WithDefaultValue("0"))

// public static readonly Vector3 Origin = Vector3.zero;
.WithField("Origin", CsType.Of("Vector3"), f => f
    .WithVisibility(Visibility.Public)
    .WithStaticModifier()
    .WithReadOnly()
    .WithDefaultValue("Vector3.zero"))
```

Default visibility is `Private`.

---

### `PropertyBuilder`

Generates a property declaration.

```csharp
// public int Count { get; private set; }
.WithProperty("Count", CsType.Int, p => p
    .WithAutoGetter()
    .WithAutoSetter(Visibility.Private))

// public static CombatDef Combat => Runtime.Combat;
.WithProperty("Combat", CsType.Of("CombatDef"), p => p
    .WithStaticModifier()
    .WithExpressionGetter("Runtime.Combat"))

// Explicit accessor bodies
.WithProperty("Value", CsType.Int, p => p
    .WithGetter(getter => getter.Return("_value"))
    .WithSetter(setter => setter.Assign("_value", "value")))
```

---

### `MethodBuilder`

Generates a method declaration.

```csharp
.WithMethod("GetOrCreate", CsType.Of("T"), m => m
    .WithVisibility(Visibility.Public)
    .WithTypeParameter("T")
    .WithTypeConstraint("T", "new()")
    .WithParameter(CsType.String, "key")
    .WithXmlDoc(doc => doc
        .WithSummary("Gets a cached instance or creates a new one.")
        .WithTypeParam("T", "The type to create.")
        .WithParam("key", "The cache key.")
        .WithReturns("The cached or newly created instance."))
    .WithBody(body => body
        .If("_cache.ContainsKey(key)",
            then => then.Return("_cache[key]"))
        .DeclareLocal("instance", "new T()")
        .Assign("_cache[key]", "instance")
        .Return("instance")))
```

**Modifiers:** `WithStaticModifier()`, `WithOverrideModifier()`, `WithVirtualModifier()`, `WithAbstractModifier()`, `WithPartialModifier()`

Abstract methods emit no body. Batch helpers: `WithParameters<T>(source, ...)`, `WithTypeParameters<T>(source, ...)`, `WithTypeConstraints<T>(source, ...)`.

---

### `ConstructorBuilder`

Generates a constructor declaration.

```csharp
.WithConstructor(c => c
    .WithVisibility(Visibility.Public)
    .WithParameter(CsType.Of("ILogger"), "logger")
    .WithBaseCall("logger")         // : base(logger)
    // .WithThisCall("default")     // : this(default)
    .WithBody(body => body
        .Assign("_logger", "logger")))
```

Batch helper: `WithParameters<T>(source, ...)`.

---

### `CodeBlockBuilder`

Generates code statements inside method, constructor, and accessor bodies.

```csharp
body => body
    .DeclareLocal("result", "ComputeValue()")          // var result = ComputeValue();
    .DeclareLocal(CsType.Int, "count", "0")            // int count = 0;
    .Assign("_field", "result")                        // _field = result;
    .CompoundAssign("_total", "+=", "result")          // _total += result;
    .Call("Log", "\"done\"")                           // Log("done");
    .Call("_service", "Process", "result")             // _service.Process(result);
    .CallAndAssign("output", "_service", "Get", "id")  // var output = _service.Get(id);
    .If("result > 0",
        then => then.Return("result"),
        else_ => else_.Return("0"))
    .ForEach(CsType.Of("Item"), "item", "_items", loop => loop
        .Call("item", "Update"))
    .For("int i = 0", "i < count", "i++", loop => loop
        .Call("Process", "i"))
    .Return("result")
    .Raw("// fallthrough")                             // arbitrary raw line
```

---

### `CsType`

Represents a C# type, including generics.

```csharp
CsType.Void                                           // void
CsType.Int                                            // int
CsType.Float                                          // float
CsType.Bool                                           // bool
CsType.String                                         // string
CsType.Of("Vector3")                                  // Vector3
CsType.ListOf(CsType.Of("Vector3"))                   // List<Vector3>
CsType.DictionaryOf(CsType.String, CsType.Int)        // Dictionary<string, int>
CsType.Generic("HashSet", CsType.String)              // HashSet<string>
CsType.Generic("Dictionary",
    CsType.String,
    CsType.ListOf(CsType.Int))                        // Dictionary<string, List<int>>
```

---

### `XmlDocBuilder`

Generates `///` XML documentation comments. Available on all builder types via `WithXmlDoc(...)`.

```csharp
.WithXmlDoc(doc => doc
    .WithSummary("Dispatches the named event to all registered listeners.")
    .WithParam("eventName", "The name of the event to dispatch.")
    .WithTypeParam("T", "The event payload type.")
    .WithReturns("True if any listeners were invoked.")
    .WithRemarks("Listeners are invoked in registration order.")
    .WithException("InvalidOperationException", "Thrown if the dispatcher is not initialized.")
    .WithInheritDoc())   // emits <inheritdoc /> instead of other tags
```

---

### `Visibility`

```csharp
Visibility.Public
Visibility.Private
Visibility.Protected
Visibility.Internal
Visibility.ProtectedInternal
```

---

## Design Notes

- **Editor-only** — all code lives under an `Editor` assembly and is intended for use in Editor tooling and code generators, not at runtime.
- **Shared `IndentEmitter`** — indentation state flows through all nested builders automatically; tabs are used for indentation.
- **Out-parameter overloads** — every `WithClass`, `WithField`, `WithMethod`, etc. has an `out` overload for capturing the builder after inline configuration.
- **Collection overloads** — batch `WithFields<T>`, `WithProperties<T>`, `WithParameters<T>` helpers accept `IEnumerable<T>` with selector lambdas to reduce boilerplate when generating members from a data model.

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
