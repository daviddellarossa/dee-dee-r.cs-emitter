# ClassBuilder and StructBuilder

Both builders share the same structure: modifiers, optional generics, and members (fields, properties, constructors, methods). The key differences are that `ClassBuilder` supports `static`, `sealed`, and `abstract` modifiers, while `StructBuilder` supports `readonly`.

---

## ClassBuilder

`ClassBuilder` generates a `class` declaration.

### Creating a ClassBuilder

`ClassBuilder` instances are created through `FileBuilder.WithClass()`. You do not instantiate `ClassBuilder` directly.

```csharp
FileBuilder.Build("Generated/MyClass.cs")
    .WithNamespace("MyProject")
    .WithClass("MyClass", cls => {
        // configure cls here
    });
```

---

### Visibility

```csharp
cls.WithVisibility(Visibility.Internal)   // internal class MyClass
```

Default: `Public`.

---

### Modifiers

| Method | Output keyword |
|---|---|
| `WithStaticModifier()` | `static` |
| `WithSealedModifier()` | `sealed` |
| `WithAbstractModifier()` | `abstract` |
| `WithPartialModifier()` | `partial` |

All modifier methods accept an optional `bool` parameter to conditionally apply the modifier:

```csharp
cls.WithSealedModifier(isSingleton)   // sealed only when true
```

Modifiers are combined in declaration order: visibility → static → abstract → sealed → partial.

---

### Generic type parameters and constraints

```csharp
cls
    .WithTypeParameter("T")
    .WithTypeParameter("TState")
    .WithTypeConstraint("T", "class")
    .WithTypeConstraint("TState", "new()")
```

Generates:

```csharp
public class MyClass<T, TState> where T : class where TState : new()
```

Multiple constraints for the same type parameter are not merged automatically — use a single `WithTypeConstraint` call with a comma-separated constraint string if needed:

```csharp
.WithTypeConstraint("T", "class, IDisposable")
// → where T : class, IDisposable
```

---

### Inheritance

Use `WithBaseClass()` to specify a base class. It accepts either a string or a `CsType` instance.

#### Simple inheritance

```csharp
ClassBuilder.Build(emitter, "MyHandler")
    .WithBaseClass("BaseHandler")
    .Emit();
```

Generates:

```csharp
public class MyHandler : BaseHandler
```

#### Generic base class

```csharp
ClassBuilder.Build(emitter, "MyHandler")
    .WithBaseClass(CsType.Generic("BaseHandler", CsType.Of("MyMessage")))
    .Emit();
```

Generates:

```csharp
public class MyHandler : BaseHandler<MyMessage>
```

#### With type parameters on the derived class

```csharp
ClassBuilder.Build(emitter, "MyHandler")
    .WithTypeParameter("T")
    .WithBaseClass(CsType.Generic("BaseHandler", CsType.Of("T")))
    .WithTypeConstraint("T", "IMessage")
    .Emit();
```

Generates:

```csharp
public class MyHandler<T> : BaseHandler<T> where T : IMessage
```

---

### Adding members

All member-adding methods are optional and can be combined in any order. Within the emitted class, members are always output in this fixed order regardless of the order they were added:

1. Fields
2. Properties
3. Constructors
4. Methods

#### Fields

```csharp
// Inline configuration
cls.WithField("_count", CsType.Int, f => f.WithReadOnly())

// With out parameter
cls.WithField("_count", CsType.Int, out FieldBuilder countField)

// From a collection
cls.WithFields(
    myData,
    item => item.Name,
    item => item.Type,
    (item, f) => f.WithReadOnly())
```

#### Properties

```csharp
cls.WithProperty("Count", CsType.Int, p => p.WithAutoGetter())
cls.WithProperty("Count", CsType.Int, out PropertyBuilder countProp)
cls.WithProperties(myData, item => item.Name, item => item.Type)
```

#### Constructors

```csharp
cls.WithConstructor(c => c
    .WithParameter(CsType.String, "name")
    .WithBody(body => body.Assign("_name", "name")))

cls.WithConstructor(out ConstructorBuilder ctor)
```

#### Methods

```csharp
cls.WithMethod("Reset", CsType.Void, m => m
    .WithBody(body => body.Assign("_count", "0")))

cls.WithMethod("Reset", CsType.Void, out MethodBuilder resetMethod)
```

---

### Attributes

Add attributes to the class using `WithAttribute()`. Supports both parameterless and parameterized attributes.

```csharp
// Parameterless attribute
cls.WithAttribute("Serializable")

// Parameterized attribute
cls.WithAttribute("ObsoleteAttribute", attr => attr.WithArgument("\"Use NewClass instead\""))

// Multiple attributes
cls.WithAttribute("Serializable")
   .WithAttribute("DebuggerDisplay", attr => attr.WithArgument("\"Count = {Count}\""))
```

Generates:

```csharp
[Serializable]
[DebuggerDisplay("Count = {Count}")]
public class MyClass
```

---

### Raw text

Use `WithRaw()` to insert arbitrary text at any point in the class body. This is useful for preprocessor directives, pragma statements, or other constructs not directly supported by the builder API.

```csharp
cls.WithField("_cached", CsType.Of("MessageBusRuntime"), f => f
        .WithVisibility(Visibility.Private))
    .WithRaw("#if UNITY_EDITOR")
    .WithMethod("OnDisable", CsType.Void, m => m
        .WithVisibility(Visibility.Private)
        .WithBody(body => body
            .Assign("_cached", "null")))
    .WithRaw("#endif")
```

Generates:

```csharp
public class MyClass
{
    private MessageBusRuntime _cached;
#if UNITY_EDITOR
    private void OnDisable()
    {
        _cached = null;
    }
#endif
}
```

**Note**: Members, including raw text added with `WithRaw`, are emitted in declaration order. Raw text appears exactly where it is declared among other members, so use this feature carefully to maintain readable generated code.

---

### XML documentation

```csharp
cls.WithXmlDoc(doc => doc
    .WithSummary("Manages the event lifecycle.")
    .WithTypeParam("T", "The event payload type."))
```

See [XmlDocBuilder](xml-doc-builder.md) for the full API.

---

### Complete example

```csharp
FileBuilder.Build("Generated/Registry.cs")
    .WithUsings("System", "System.Collections.Generic")
    .WithNamespace("MyProject.Generated")
    .WithClass("Registry", cls => cls
        .WithXmlDoc(doc => doc.WithSummary("A generic key-value registry."))
        .WithSealedModifier()
        .WithTypeParameter("TKey")
        .WithTypeParameter("TValue")
        .WithTypeConstraint("TKey", "notnull")
        .WithField("_store", CsType.DictionaryOf(CsType.Of("TKey"), CsType.Of("TValue")), f => f
            .WithReadOnly())
        .WithProperty("Count", CsType.Int, p => p
            .WithExpressionGetter("_store.Count"))
        .WithConstructor(c => c
            .WithBody(body => body
                .Assign("_store", "new Dictionary<TKey, TValue>()")))
        .WithMethod("Register", CsType.Void, m => m
            .WithParameter(CsType.Of("TKey"), "key")
            .WithParameter(CsType.Of("TValue"), "value")
            .WithBody(body => body
                .Assign("_store[key]", "value")))
        .WithMethod("TryGet", CsType.Bool, m => m
            .WithParameter(CsType.Of("TKey"), "key")
            .WithParameter(CsType.Of("TValue"), "result")
            .WithBody(body => body
                .Return("_store.TryGetValue(key, out result)"))))
    .Save();
```

---

## StructBuilder

`StructBuilder` generates a `struct` declaration. Its API is identical to `ClassBuilder` except:

- `WithReadOnlyModifier()` replaces `WithSealedModifier()` / `WithAbstractModifier()` / `WithStaticModifier()`.
- There is no static modifier.

### Modifiers

| Method | Output keyword |
|---|---|
| `WithReadOnlyModifier()` | `readonly` |
| `WithPartialModifier()` | `partial` |

### Attributes

Add attributes to structs using `WithAttribute()`:

```csharp
s.WithAttribute("Serializable")
 .WithAttribute("StructLayout", attr => attr.WithArgument("LayoutKind.Sequential"))
```

Generates:

```csharp
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Bounds2D
```

### Raw text

Use `WithRaw()` to insert arbitrary text within the struct body:

```csharp
s.WithField("Value", CsType.Int)
 .WithRaw("#if DEBUG")
 .WithMethod("Validate", CsType.Void, m => m
     .WithBody(body => body.Call("Debug.Assert", "Value >= 0")))
 .WithRaw("#endif")
```

See the [Raw text section](#raw-text) in ClassBuilder for more details.

### Example

```csharp
FileBuilder.Build("Generated/Bounds2D.cs")
    .WithNamespace("MyProject.Generated")
    .WithStruct("Bounds2D", s => s
        .WithXmlDoc(doc => doc.WithSummary("An axis-aligned 2D bounding box."))
        .WithReadOnlyModifier()
        .WithField("Min", CsType.Of("Vector2"), f => f
            .WithVisibility(Visibility.Public)
            .WithReadOnly())
        .WithField("Max", CsType.Of("Vector2"), f => f
            .WithVisibility(Visibility.Public)
            .WithReadOnly())
        .WithProperty("Size", CsType.Of("Vector2"), p => p
            .WithExpressionGetter("Max - Min"))
        .WithConstructor(c => c
            .WithParameter(CsType.Of("Vector2"), "min")
            .WithParameter(CsType.Of("Vector2"), "max")
            .WithBody(body => body
                .Assign("Min", "min")
                .Assign("Max", "max"))))
    .Save();
```

Produces:

```csharp
namespace MyProject.Generated
{
    /// <summary>
    /// An axis-aligned 2D bounding box.
    /// </summary>
    public readonly struct Bounds2D
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public Vector2 Size => Max - Min;

        public Bounds2D(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

    }

}
```
