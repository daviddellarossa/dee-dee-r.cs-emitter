# FileBuilder and CsType

## FileBuilder

`FileBuilder` is the top-level builder. It produces a complete `.cs` file — `using` directives, a namespace block, and any number of classes and structs.

### Creating a builder

```csharp
FileBuilder.Build(string relativePath)
```

`relativePath` is the path used by `Save()`. It is not used by `Emit()` or `SaveTo()`.

---

### Adding `using` directives

| Method | Description |
|---|---|
| `WithUsing(string name)` | Adds a single `using` statement. Duplicate names are ignored. |
| `WithUsings(params string[] names)` | Adds multiple `using` statements. |
| `WithUsings<T>(IEnumerable<T> source, Func<T, string> nameSelector)` | Adds `using` statements derived from a collection. |

`using` statements are sorted alphabetically in the output regardless of the order they are added.

```csharp
FileBuilder.Build("Generated/Foo.cs")
    .WithUsing("UnityEngine")
    .WithUsings("System", "System.Collections.Generic")
    .WithUsings(myTypes, t => t.Namespace);
```

---

### Setting the namespace

```csharp
.WithNamespace("MyProject.Generated")
```

Omitting `WithNamespace` emits types at the file scope (no namespace block).

---

### Adding classes and structs

```csharp
// Inline configuration
.WithClass("MyClass", cls => cls.WithSealedModifier())

// With out parameter to capture the builder for later use
.WithClass("MyClass", out ClassBuilder cls)

// Both: capture and configure inline
.WithClass("MyClass", out ClassBuilder cls, c => c.WithSealedModifier())

// Structs follow the same pattern
.WithStruct("MyStruct", s => s.WithReadOnlyModifier())
.WithStruct("MyStruct", out StructBuilder s)
```

Multiple classes and structs can be added to a single file. They are emitted in the order they were added.

---

### Output methods

| Method | Description |
|---|---|
| `Emit()` | Returns the generated file content as a `string`. |
| `Save()` | Writes to the path provided in `Build()`. Creates missing directories. |
| `SaveTo(string filePath)` | Writes to an arbitrary path. Creates missing directories. |

---

## CsType

`CsType` represents a C# type — primitive or generic — and is used wherever a type must be specified (fields, properties, method return types, parameters, local variables).

### Predefined types

```csharp
CsType.Void     // void
CsType.Int      // int
CsType.Float    // float
CsType.Bool     // bool
CsType.String   // string
```

### Creating types

```csharp
// Non-generic named type
CsType.Of("Vector3")              // Vector3
CsType.Of("GameObject")           // GameObject

// Generic type with arbitrary type arguments
CsType.Generic("HashSet", CsType.String)                         // HashSet<string>
CsType.Generic("Tuple", CsType.Int, CsType.String)               // Tuple<int, string>

// Convenience helpers
CsType.ListOf(CsType.Of("Transform"))                            // List<Transform>
CsType.DictionaryOf(CsType.String, CsType.Int)                   // Dictionary<string, int>

// Nested generics
CsType.DictionaryOf(CsType.String, CsType.ListOf(CsType.Int))    // Dictionary<string, List<int>>
```

### Properties

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | The base type name. |
| `IsGeneric` | `bool` | `true` if the type has one or more type arguments. |
| `TypeArguments` | `IReadOnlyList<CsType>` | The generic type arguments. Empty for non-generic types. |

### Emitting

```csharp
CsType.DictionaryOf(CsType.String, CsType.Int).Emit()
// → "Dictionary<string, int>"

CsType.Of("Vector3").ToString()
// → "Vector3"
```

`Emit()` and `ToString()` return the same value.

---

## Visibility

The `Visibility` enum is used by all builders that accept an access modifier.

| Value | C# keyword |
|---|---|
| `Visibility.Public` | `public` |
| `Visibility.Private` | `private` |
| `Visibility.Protected` | `protected` |
| `Visibility.Internal` | `internal` |
| `Visibility.ProtectedInternal` | `protected internal` |

Default visibility varies by builder:

| Builder | Default |
|---|---|
| `ClassBuilder` | `Public` |
| `StructBuilder` | `Public` |
| `FieldBuilder` | `Private` |
| `PropertyBuilder` | `Public` |
| `MethodBuilder` | `Public` |
| `ConstructorBuilder` | `Public` |
