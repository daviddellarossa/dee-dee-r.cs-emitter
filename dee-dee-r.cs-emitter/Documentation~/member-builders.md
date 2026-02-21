# Member Builders

Member builders are created through their parent `ClassBuilder` or `StructBuilder` via `WithField()`, `WithProperty()`, `WithConstructor()`, and `WithMethod()`. You do not instantiate them directly.

---

## FieldBuilder

`FieldBuilder` generates a single field declaration.

### Visibility

```csharp
f.WithVisibility(Visibility.Public)
```

Default: `Private`.

### Modifiers

| Method | Output keyword |
|---|---|
| `WithStaticModifier()` | `static` |
| `WithReadOnly()` | `readonly` |
| `WithConstModifier()` | `const` |

These methods accept an optional `bool` to toggle the modifier conditionally.

Note: const fields must be initialized with a value using `WithDefaultValue()`.
### Default value

```csharp
f.WithDefaultValue("0")
f.WithDefaultValue("new List<string>()")
```

The value is emitted as-is after `=`.

### XML documentation

```csharp
f.WithXmlDoc(doc => doc.WithSummary("The total number of registered items."))
```

### Examples

```csharp
// private int _count;
cls.WithField("_count", CsType.Int)

// private readonly List<string> _names;
cls.WithField("_names", CsType.ListOf(CsType.String), f => f
    .WithReadOnly())

// public static readonly Vector3 Up = Vector3.up;
cls.WithField("Up", CsType.Of("Vector3"), f => f
    .WithVisibility(Visibility.Public)
    .WithStaticModifier()
    .WithReadOnly()
    .WithDefaultValue("Vector3.up"))

// private int _health = 100;
cls.WithField("_health", CsType.Int, f => f
    .WithDefaultValue("100"))

// public const int MaxRetries = 3;
cls.WithField("MaxRetries", CsType.Int, f => f
    .WithVisibility(Visibility.Public)
    .WithConstModifier()
    .WithDefaultValue("3"))
```

---

## PropertyBuilder

`PropertyBuilder` generates a property declaration. Three property styles are supported: auto-implemented, expression-bodied, and full (explicit accessor bodies).

### Visibility

```csharp
p.WithVisibility(Visibility.Internal)
```

Default: `Public`.

### Static modifier

```csharp
p.WithStaticModifier()
```

### Auto-implemented properties

Call `WithAutoGetter()` and/or `WithAutoSetter()` to emit `get;` and `set;` accessors. Pass an optional `Visibility` to restrict an accessor's visibility.

```csharp
// public int Count { get; set; }
p.WithAutoGetter().WithAutoSetter()

// public int Count { get; private set; }
p.WithAutoGetter().WithAutoSetter(Visibility.Private)

// public int Count { get; }  (init-only via backing field)
p.WithAutoGetter()
```

### Default value (auto properties)

```csharp
// public int Count { get; set; } = 10;
p.WithAutoGetter().WithAutoSetter().WithDefaultValue("10")
```

### Expression-bodied getter

```csharp
// public int Count => _items.Count;
p.WithExpressionGetter("_items.Count")

// public static Config Instance => _instance;
p.WithStaticModifier().WithExpressionGetter("_instance")
```

### Full property with explicit accessor bodies

Use `WithGetter(AccessorBodyBuilder)` and `WithSetter(AccessorBodyBuilder)` when the accessor requires logic. The delegate receives a `CodeBlockBuilder`.

```csharp
// public int Health
// {
//     get { return _health; }
//     set { _health = Mathf.Clamp(value, 0, _maxHealth); }
// }
p.WithGetter(getter => getter
        .Return("_health"))
 .WithSetter(setter => setter
        .Assign("_health", "Mathf.Clamp(value, 0, _maxHealth)"))
```

A `Visibility` overload is available for explicit accessor bodies too:

```csharp
p.WithGetter(getter => getter.Return("_value"))
 .WithSetter(setter => setter.Assign("_value", "value"), Visibility.Protected)
// get { return _value; }
// protected set { _value = value; }
```

### XML documentation

```csharp
p.WithXmlDoc(doc => doc.WithSummary("The current health value, clamped to [0, MaxHealth]."))
```

### Accessor body and auto cannot be mixed per accessor

If you call `WithAutoGetter()` and then `WithGetter(...)`, the explicit body takes precedence. If you call `WithAutoSetter()` without a body delegate, an auto `set;` is emitted.

---

## ConstructorBuilder

`ConstructorBuilder` generates a constructor.

### Visibility

```csharp
c.WithVisibility(Visibility.Private)   // private constructor / singleton
```

Default: `Public`.

### Parameters

```csharp
// Single parameter
c.WithParameter(CsType.Of("ILogger"), "logger")

// Multiple parameters from a collection
c.WithParameters(
    myParams,
    p => p.Type,
    p => p.Name)
```

### Constructor initialiser calls

```csharp
// : base(logger)
c.WithBaseCall("logger")

// : this(defaultLogger)
c.WithThisCall("defaultLogger")
```

Only one initialiser call is emitted. If both `WithBaseCall` and `WithThisCall` are called, `base` takes precedence.

### Body

```csharp
c.WithBody(body => body
    .Assign("_logger", "logger")
    .Call("Initialize"))
```

See [CodeBlockBuilder](code-block-builder.md) for the full statement API.

### XML documentation

```csharp
c.WithXmlDoc(doc => doc
    .WithSummary("Initializes a new instance.")
    .WithParam("logger", "The logger to use."))
```

### Conditional constructor with WithConstructorIf

Use `WithConstructorIf()` on `StructBuilder` to conditionally add a constructor based on a boolean condition. This is particularly useful when generating code from models where a constructor may only be needed in certain cases (e.g., when parameters exist).

```csharp
structBuilder
    .WithConstructorIf(
        messageModel.MessageArgs.Parameters.Count > 0,
        ctor => ctor
            .WithParameters(messageModel.MessageArgs.Parameters,
                parameter => CsType.Of(parameter.TypeModel.Type.FullName),
                parameter => parameter.ToLocalVariableName())
            .WithBody(body =>
            {
                foreach (var parameter in messageModel.MessageArgs.Parameters)
                    body.Assign(parameter.ToPropertyName(), parameter.ToLocalVariableName());
            }))
```

### Example

```csharp
cls.WithConstructor(c => c
    .WithXmlDoc(doc => doc
        .WithSummary("Creates a new EventBus.")
        .WithParam("capacity", "Initial handler capacity."))
    .WithParameter(CsType.Int, "capacity")
    .WithBody(body => body
        .Assign("_handlers", "new Dictionary<string, Action>(capacity)")
        .Assign("IsReady", "true")))
```

---

## MethodBuilder

`MethodBuilder` generates a method declaration.

### Visibility

```csharp
m.WithVisibility(Visibility.Protected)
```

Default: `Public`.

### Modifiers

| Method | Output keyword |
|---|---|
| `WithStaticModifier()` | `static` |
| `WithVirtualModifier()` | `virtual` |
| `WithOverrideModifier()` | `override` |
| `WithAbstractModifier()` | `abstract` |
| `WithPartialModifier()` | `partial` |

Abstract and partial methods without a body emit a semicolon terminator instead of a block:

```csharp
// public abstract void Execute();
m.WithAbstractModifier()

// public partial void OnGenerated();
m.WithPartialModifier()
```

### Parameters

```csharp
m.WithParameter(CsType.String, "name")
m.WithParameter(CsType.Of("Transform"), "target")

// From a collection
m.WithParameters(
    myParams,
    p => p.Type,
    p => p.Name)
```

### Generic type parameters and constraints

```csharp
m.WithTypeParameter("T")
 .WithTypeConstraint("T", "class, new()")

// From collections
m.WithTypeParameters(myTypeParams, tp => tp.Name)
m.WithTypeConstraints(myConstraints, c => c.TypeParam, c => c.Constraint)
```

### Body

```csharp
m.WithBody(body => body
    .DeclareLocal("result", "Calculate()")
    .If("result > 0",
        then => then.Return("result"),
        else_ => else_.Return("0")))
```

Omit `WithBody` together with `WithAbstractModifier()` or `WithPartialModifier()` to emit a method with no body.

### XML documentation

```csharp
m.WithXmlDoc(doc => doc
    .WithSummary("Finds or creates a component of the given type.")
    .WithTypeParam("T", "The component type to look for.")
    .WithParam("go", "The target GameObject.")
    .WithReturns("The existing or newly added component."))
```

### Examples

```csharp
// Simple void method
cls.WithMethod("Reset", CsType.Void, m => m
    .WithBody(body => body
        .Assign("_count", "0")
        .Call("OnReset")))

// Static factory method
cls.WithMethod("Create", CsType.Of("MyClass"), m => m
    .WithStaticModifier()
    .WithBody(body => body
        .Return("new MyClass()")))

// Generic method with constraint
cls.WithMethod("GetOrAdd", CsType.Of("T"), m => m
    .WithTypeParameter("T")
    .WithTypeConstraint("T", "Component")
    .WithParameter(CsType.Of("GameObject"), "go")
    .WithBody(body => body
        .CallAndAssign("component", "go", "GetComponent<T>")
        .If("component == null",
            then => then.CallAndAssign("component", "go", "AddComponent<T>"))
        .Return("component")))

// Override method
cls.WithMethod("ToString", CsType.String, m => m
    .WithOverrideModifier()
    .WithBody(body => body
        .Return("$\"MyClass({_count})\"")))

// Abstract method (no body)
cls.WithMethod("Process", CsType.Void, m => m
    .WithAbstractModifier()
    .WithParameter(CsType.Of("Context"), "ctx"))
```
