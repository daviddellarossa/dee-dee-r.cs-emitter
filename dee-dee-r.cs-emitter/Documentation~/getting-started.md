# Getting Started

## Requirements

- Unity 6000.3 or later
- Editor scripts only (the package assembly is Editor-only)

---

## Installation

### Via Package Manager (Git URL)

1. Open **Window > Package Manager**.
2. Click the **+** button and select **Add package from git URL…**
3. Enter the repository URL and confirm.

### Via local path

Copy or symlink the `dee-dee-r.cs-emitter` folder into your project's `Packages/` directory. Unity picks it up automatically on the next Editor refresh.

---

## Namespace

Add the following `using` directive to any Editor script that uses the package:

```csharp
using DeeDeeR.CsEmitter;
```

---

## Your first generated file

The entry point for all code generation is `FileBuilder`. Call `FileBuilder.Build(path)` to create a builder, configure it with method chaining, then call `Save()` to write the file.

```csharp
using DeeDeeR.CsEmitter;
using UnityEditor;

public static class MyGenerator
{
    [MenuItem("Tools/Generate HelloWorld")]
    public static void Generate()
    {
        FileBuilder.Build("Assets/Generated/HelloWorld.cs")
            .WithUsing("UnityEngine")
            .WithNamespace("MyProject.Generated")
            .WithClass("HelloWorld", cls => cls
                .WithSealedModifier()
                .WithMethod("Greet", CsType.Void, m => m
                    .WithBody(body => body
                        .Call("Debug", "Log", "\"Hello, world!\""))))
            .Save();

        AssetDatabase.Refresh();
    }
}
```

Running **Tools > Generate HelloWorld** creates the following file at `Assets/Generated/HelloWorld.cs`:

```csharp
using UnityEngine;

namespace MyProject.Generated
{
    public sealed class HelloWorld
    {
        public void Greet()
        {
            Debug.Log("Hello, world!");
        }

    }

}
```

> **Note:** `using` statements are sorted alphabetically by `FileBuilder.Emit()`. Duplicate usings are silently ignored.

> **Tip:** Call `AssetDatabase.Refresh()` after `Save()` so the Unity Editor detects the new file immediately.

---

## Getting the generated string without saving

Use `Emit()` instead of `Save()` to retrieve the file content as a string, for example to preview it or write it via a custom mechanism:

```csharp
string code = FileBuilder.Build("irrelevant")
    .WithNamespace("MyProject")
    .WithClass("Temp")
    .Emit();

Debug.Log(code);
```

---

## Saving to a different path

`SaveTo(filePath)` writes to an arbitrary path, ignoring the path passed to `Build()`:

```csharp
FileBuilder.Build("unused")
    .WithNamespace("MyProject")
    .WithClass("MyClass")
    .SaveTo(Application.dataPath + "/Generated/MyClass.cs");
```

`SaveTo()` automatically creates any missing directories.

---

## Next steps

- [FileBuilder and CsType](file-and-type-builders.md) — full reference for file-level options and type representation.
- [ClassBuilder and StructBuilder](class-and-struct-builders.md) — modifiers, generics, and member layout.
- [Member Builders](member-builders.md) — fields, properties, constructors, and methods.
- [CodeBlockBuilder](code-block-builder.md) — generating code statements inside bodies.
