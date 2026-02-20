# C# Emitter

**C# Emitter** (`dee-dee-r.cs-emitter`) is an Editor-only Unity package that provides a fluent builder API for generating C# source code programmatically. Instead of concatenating strings or wrestling with templates, you compose complete `.cs` files using chainable builder methods that handle syntax, indentation, and formatting automatically.

**Namespace:** `DeeDeeR.CsEmitter`

---

## In this documentation

| Page | Description |
|---|---|
| [Getting Started](getting-started.md) | Installation, first steps, and a complete walkthrough |
| [FileBuilder and CsType](file-and-type-builders.md) | Creating files, `using` directives, namespaces, and representing C# types |
| [ClassBuilder and StructBuilder](class-and-struct-builders.md) | Declaring classes and structs with modifiers and generics |
| [Member Builders](member-builders.md) | Fields, properties, constructors, and methods |
| [CodeBlockBuilder](code-block-builder.md) | Generating statements inside method and constructor bodies |
| [XmlDocBuilder](xml-doc-builder.md) | Attaching `///` XML documentation to any member |
| [Advanced Patterns](advanced-patterns.md) | Out-parameter overloads, collection helpers, and a complete worked example |

---

## Quick example

The following generates a sealed class with a field, a property, a constructor, and a method, then saves it to disk:

```csharp
using DeeDeeR.CsEmitter;

FileBuilder.Build("Generated/EventBus.cs")
    .WithUsings("System", "System.Collections.Generic")
    .WithNamespace("MyProject.Generated")
    .WithClass("EventBus", cls => cls
        .WithSealedModifier()
        .WithField("_handlers", CsType.DictionaryOf(CsType.String, CsType.Of("Action")), f => f
            .WithReadOnly())
        .WithProperty("IsReady", CsType.Bool, p => p
            .WithAutoGetter()
            .WithAutoSetter(Visibility.Private))
        .WithConstructor(c => c
            .WithBody(body => body
                .Assign("_handlers", "new Dictionary<string, Action>()")
                .Assign("IsReady", "true")))
        .WithMethod("Publish", CsType.Void, m => m
            .WithParameter(CsType.String, "eventName")
            .WithBody(body => body
                .If("_handlers.TryGetValue(eventName, out var handler)",
                    then => then.Call("handler", "Invoke")))))
    .Save();
```

---

## Package overview

| Type | Role |
|---|---|
| `FileBuilder` | Top-level builder — generates a complete `.cs` file |
| `ClassBuilder` | Generates a `class` declaration |
| `StructBuilder` | Generates a `struct` declaration |
| `FieldBuilder` | Generates a field declaration |
| `PropertyBuilder` | Generates a property declaration |
| `ConstructorBuilder` | Generates a constructor |
| `MethodBuilder` | Generates a method |
| `CodeBlockBuilder` | Generates imperative statements inside bodies |
| `CsType` | Represents a C# type, including generics |
| `XmlDocBuilder` | Generates `///` XML documentation comments |
| `Visibility` | Enum of C# access modifiers |
| `IndentEmitter` | Manages tab-based indentation (internal infrastructure) |

> **Note:** All builder types live in an `Editor` assembly and are available only in Editor code. They are not included in player builds.
