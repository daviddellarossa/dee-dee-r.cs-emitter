# CodeBlockBuilder

`CodeBlockBuilder` generates the imperative statements inside method bodies, constructor bodies, and property accessor bodies. It is passed to you as a parameter in body-configuration delegates:

```csharp
m.WithBody(body => {
    // body is a CodeBlockBuilder
})
```

All methods return `this` for chaining.

---

## Local variable declarations

### `DeclareLocal(string name, string? value)`

Emits a `var` declaration.

```csharp
body.DeclareLocal("index", "0")
// → var index = 0;

body.DeclareLocal("result")
// → var result;
```

### `DeclareLocal(CsType type, string name, string? value)`

Emits an explicitly typed declaration.

```csharp
body.DeclareLocal(CsType.Int, "count", "0")
// → int count = 0;

body.DeclareLocal(CsType.Of("StringBuilder"), "sb", "new StringBuilder()")
// → StringBuilder sb = new StringBuilder();
```

---

## Assignments

### `Assign(string target, string value)`

```csharp
body.Assign("_isReady", "true")
// → _isReady = true;

body.Assign("transform.position", "Vector3.zero")
// → transform.position = Vector3.zero;
```

### `CompoundAssign(string target, string op, string value)`

`op` is the operator character(s) without `=`:

```csharp
body.CompoundAssign("_count", "+", "1")
// → _count += 1;

body.CompoundAssign("_health", "-", "damage")
// → _health -= damage;
```

---

## Method calls

### `Call(string methodName, params string[] args)`

Calls a method without a target (local method, static import, or method on `this`):

```csharp
body.Call("Initialize")
// → Initialize();

body.Call("Debug.Log", "\"hello\"")
// → Debug.Log("hello");

body.Call("Validate", "name", "value")
// → Validate(name, value);
```

### `Call(string target, string methodName, params string[] args)`

Calls a method on a target object:

```csharp
body.Call("_service", "Process", "data")
// → _service.Process(data);

body.Call("gameObject", "SetActive", "false")
// → gameObject.SetActive(false);
```

---

## Call and assign

### `CallAndAssign(string resultName, string methodName, params string[] args)`

Calls a method and captures the return value in a new `var` local:

```csharp
body.CallAndAssign("result", "Compute", "input")
// → var result = Compute(input);
```

### `CallAndAssign(string resultName, string target, string methodName, params string[] args)`

Calls a method on a target and captures the result:

```csharp
body.CallAndAssign("component", "go", "GetComponent<Rigidbody>")
// → var component = go.GetComponent<Rigidbody>();
```

### `CallAndAssign(CsType resultType, string resultName, string methodName, params string[] args)`

Explicit type for the result variable:

```csharp
body.CallAndAssign(CsType.Of("Rigidbody"), "rb", "GetComponent<Rigidbody>")
// → Rigidbody rb = GetComponent<Rigidbody>();
```

---

## Return

### `Return(string? value)`

```csharp
body.Return("result")
// → return result;

body.Return()
// → return;

body.Return("true")
// → return true;
```

---

## Conditionals

### `If(string condition, Action<CodeBlockBuilder> then, Action<CodeBlockBuilder>? else)`

```csharp
// if only
body.If("_isReady",
    then => then.Call("Execute"))

// if / else
body.If("value > 0",
    then => then.Assign("_sign", "1"),
    else_ => else_.Assign("_sign", "-1"))

// nested
body.If("_cache != null",
    then => then.If("_cache.ContainsKey(key)",
        hit => hit.Return("_cache[key]")))
```

The condition string is emitted as-is inside `if (...)`.

---

## Loops

### `ForEach(CsType itemType, string itemName, string collection, Action<CodeBlockBuilder> body)`

```csharp
body.ForEach(CsType.Of("Transform"), "child", "transform", loop => loop
    .Call("child.gameObject", "SetActive", "false"))
// →
// foreach (Transform child in transform)
// {
//     child.gameObject.SetActive(false);
// }
```

### `For(string initializer, string condition, string iterator, Action<CodeBlockBuilder> body)`

```csharp
body.For("int i = 0", "i < _items.Count", "i++", loop => loop
    .Call("Process", "_items[i]"))
// →
// for (int i = 0; i < _items.Count; i++)
// {
//     Process(_items[i]);
// }
```

All three clauses (`initializer`, `condition`, `iterator`) are emitted as-is.

---

## Raw lines

### `Raw(string line)`

Emits any string verbatim at the current indentation level. Use this for statements not covered by other methods.

```csharp
body.Raw("throw new NotImplementedException();")
body.Raw("await Task.Delay(100);")
body.Raw("_ = StartCoroutine(MyCoroutine());")
```

---

## Formatting

### `BlankLine()`

Emits an empty line to visually separate logical sections of code.

```csharp
MethodBuilder.Build(_emitter, "Initialize", CsType.Void)
    .WithBody(body => body
        .If("_initialized", then => then.Return())
        .Assign("_initialized", "true")
        .BlankLine()
        .DeclareLocal("scheduler", "GetComponent<FrameSchedulerBehaviour>() ?? gameObject.AddComponent<FrameSchedulerBehaviour>()")
        .Assign("Scheduler", "scheduler")
        .BlankLine()
        .Call("InitializeGeneratedCategories"))
    .Emit();
```

---

## Combining statements

All `CodeBlockBuilder` methods chain, and nested bodies use the same `IndentEmitter`, so indentation is managed automatically.

```csharp
m.WithBody(body => body
    .DeclareLocal("count", "_items.Count")
    .If("count == 0",
        then => then.Return())
    .ForEach(CsType.Of("Item"), "item", "_items", loop => loop
        .Call("item", "Update")
        .If("item.IsExpired",
            expired => expired
                .Call("_pool", "Return", "item")
                .Call("_items", "Remove", "item")))
    .CompoundAssign("_processedFrames", "+", "1"))
```

Produces:

```csharp
public void Tick()
{
    var count = _items.Count;
    if (count == 0)
    {
        return;
    }
    foreach (Item item in _items)
    {
        item.Update();
        if (item.IsExpired)
        {
            _pool.Return(item);
            _items.Remove(item);
        }
    }
    _processedFrames += 1;
}
```
