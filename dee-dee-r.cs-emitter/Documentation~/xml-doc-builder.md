# XmlDocBuilder

`XmlDocBuilder` generates C# XML documentation comments (`///`). It is available on every builder type via a `WithXmlDoc(Action<XmlDocBuilder>)` method.

---

## Creating an XmlDocBuilder

`XmlDocBuilder` instances are created for you when you call `WithXmlDoc`. You do not instantiate it directly.

```csharp
cls.WithXmlDoc(doc => {
    // doc is an XmlDocBuilder
})
```

---

## Summary

```csharp
doc.WithSummary("Manages the lifecycle of active game sessions.")
```

Emits:

```
/// <summary>
/// Manages the lifecycle of active game sessions.
/// </summary>
```

Multi-line summaries are supported by embedding newline characters:

```csharp
doc.WithSummary("Manages the lifecycle of active game sessions.\nCall Initialize() before use.")
```

---

## Remarks

```csharp
doc.WithRemarks("This class is not thread-safe. Access it only from the main thread.")
```

Emits:

```
/// <remarks>
/// This class is not thread-safe. Access it only from the main thread.
/// </remarks>
```

---

## Parameters

```csharp
doc.WithParam("name", "The display name of the entity.")
doc.WithParam("position", "The world-space spawn position.")
```

Emits:

```
/// <param name="name">The display name of the entity.</param>
/// <param name="position">The world-space spawn position.</param>
```

Parameters are emitted in the order `WithParam` was called. As a convention, call `WithParam` in the same order as the actual method parameters.

---

## Type parameters

```csharp
doc.WithTypeParam("T", "The component type to resolve.")
```

Emits:

```
/// <typeparam name="T">The component type to resolve.</typeparam>
```

---

## Return value

```csharp
doc.WithReturns("The resolved component, or null if not found.")
```

Emits:

```
/// <returns>The resolved component, or null if not found.</returns>
```

---

## Exceptions

```csharp
doc.WithException("ArgumentNullException", "Thrown when key is null.")
doc.WithException("InvalidOperationException", "Thrown when the registry is not initialized.")
```

Emits:

```
/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
/// <exception cref="InvalidOperationException">Thrown when the registry is not initialized.</exception>
```

---

## Inherit doc

`WithInheritDoc()` short-circuits all other tags — only `<inheritdoc />` is emitted when it is set.

```csharp
doc.WithInheritDoc()
```

Emits:

```
/// <inheritdoc />
```

---

## Emission order

When multiple tags are present, they are always emitted in this fixed order, matching C# conventions:

1. `<summary>`
2. `<remarks>`
3. `<typeparam>` entries (in call order)
4. `<param>` entries (in call order)
5. `<returns>`
6. `<exception>` entries (in call order)

---

## Complete example

```csharp
cls.WithMethod("Resolve", CsType.Of("T"), m => m
    .WithXmlDoc(doc => doc
        .WithSummary("Resolves a registered service of the given type.")
        .WithRemarks("Returns the most recently registered instance if multiple exist.")
        .WithTypeParam("T", "The service interface type.")
        .WithParam("fallback", "A fallback value returned when no service is registered.")
        .WithReturns("The registered service, or fallback if none is found.")
        .WithException("InvalidOperationException", "Thrown when the container has been disposed."))
    .WithTypeParameter("T")
    .WithTypeConstraint("T", "class")
    .WithParameter(CsType.Of("T"), "fallback")
    .WithBody(body => body
        .Return("_container.TryResolve<T>() ?? fallback")))
```

Produces:

```csharp
/// <summary>
/// Resolves a registered service of the given type.
/// </summary>
/// <remarks>
/// Returns the most recently registered instance if multiple exist.
/// </remarks>
/// <typeparam name="T">The service interface type.</typeparam>
/// <param name="fallback">A fallback value returned when no service is registered.</param>
/// <returns>The registered service, or fallback if none is found.</returns>
/// <exception cref="InvalidOperationException">Thrown when the container has been disposed.</exception>
public T Resolve<T>(T fallback) where T : class
{
    return _container.TryResolve<T>() ?? fallback;
}
```
