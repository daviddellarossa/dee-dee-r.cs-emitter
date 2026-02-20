# Advanced Patterns

## Out-parameter overloads

Every builder method that adds a member (`WithClass`, `WithStruct`, `WithField`, `WithProperty`, `WithConstructor`, `WithMethod`) has an overload that exposes the inner builder via an `out` parameter. This lets you capture the builder for reuse after the initial configuration call, without breaking the fluent chain.

```csharp
FileBuilder.Build("Generated/Foo.cs")
    .WithNamespace("MyProject")
    .WithClass("Foo", out ClassBuilder cls)
    .Save();

// cls is now available for further configuration
cls.WithField("_count", CsType.Int);
cls.WithMethod("Reset", CsType.Void, m => m
    .WithBody(body => body.Assign("_count", "0")));
```

Combining inline configuration with the `out` overload:

```csharp
.WithClass("Foo", out ClassBuilder cls, c => c.WithSealedModifier())
```

This is equivalent to:

```csharp
.WithClass("Foo", c => c.WithSealedModifier())
// and separately capturing cls
```

### When to use out parameters

Use out parameters when:

- You are building members dynamically (e.g., inside a loop) after the initial class definition.
- You want to share the same builder reference across multiple methods in your generator.
- You need to conditionally add members based on external data that isn't available at the time you call `WithClass`.

---

## Collection overloads

`WithFields<T>`, `WithProperties<T>`, `WithParameters<T>`, `WithTypeParameters<T>`, and `WithTypeConstraints<T>` accept an `IEnumerable<T>` and selector functions, making it easy to drive generation from a data model.

### Fields from a schema

```csharp
// Assume: record FieldDef(string Name, CsType Type, bool IsReadOnly)

cls.WithFields(
    schema.Fields,
    f => f.Name,
    f => f.Type,
    (def, builder) =>
    {
        if (def.IsReadOnly)
            builder.WithReadOnly();
    })
```

### Properties from a schema

```csharp
cls.WithProperties(
    schema.Properties,
    p => p.Name,
    p => p.Type,
    (def, builder) => builder
        .WithAutoGetter()
        .WithAutoSetter(def.IsPublicSetter ? null : Visibility.Private))
```

### Parameters from a collection

```csharp
// Assume: record ParamDef(CsType Type, string Name)

m.WithParameters(
    schema.Parameters,
    p => p.Type,
    p => p.Name)
```

---

## Complete worked example

The following generator reads a simple `MessageDef` schema and produces a fully documented, immutable message class with a constructor, read-only properties, and a `ToString` override.

```csharp
using System.Collections.Generic;
using DeeDeeR.CsEmitter;
using UnityEditor;

public static class MessageGenerator
{
    public record FieldDef(string Name, CsType Type, string Description);

    public record MessageDef(string Namespace, string ClassName, List<FieldDef> Fields);

    [MenuItem("Tools/Generate Messages")]
    public static void Generate()
    {
        var def = new MessageDef(
            Namespace: "MyProject.Messages",
            ClassName: "PlayerSpawnedMessage",
            Fields: new List<FieldDef>
            {
                new("PlayerId", CsType.Int,           "The unique identifier of the spawned player."),
                new("Position", CsType.Of("Vector3"), "The world-space spawn position."),
                new("TeamIndex", CsType.Int,          "The team the player belongs to."),
            });

        GenerateMessage(def);
        AssetDatabase.Refresh();
    }

    private static void GenerateMessage(MessageDef def)
    {
        FileBuilder.Build($"Assets/Generated/Messages/{def.ClassName}.cs")
            .WithUsing("UnityEngine")
            .WithNamespace(def.Namespace)
            .WithClass(def.ClassName, cls => cls
                .WithXmlDoc(doc => doc
                    .WithSummary($"Immutable message raised when a player spawns into the world."))
                .WithSealedModifier()

                // Read-only backing fields
                .WithFields(
                    def.Fields,
                    f => $"_{char.ToLower(f.Name[0])}{f.Name.Substring(1)}",
                    f => f.Type,
                    (fieldDef, builder) => builder.WithReadOnly())

                // Public expression-bodied properties
                .WithProperties(
                    def.Fields,
                    f => f.Name,
                    f => f.Type,
                    (fieldDef, builder) => builder
                        .WithXmlDoc(doc => doc.WithSummary(fieldDef.Description))
                        .WithExpressionGetter(
                            $"_{char.ToLower(fieldDef.Name[0])}{fieldDef.Name.Substring(1)}"))

                // Constructor
                .WithConstructor(c =>
                {
                    c.WithXmlDoc(doc =>
                    {
                        doc.WithSummary($"Initializes a new {def.ClassName}.");
                        foreach (var f in def.Fields)
                            doc.WithParam(
                                $"{char.ToLower(f.Name[0])}{f.Name.Substring(1)}",
                                f.Description);
                    });

                    c.WithParameters(
                        def.Fields,
                        f => f.Type,
                        f => $"{char.ToLower(f.Name[0])}{f.Name.Substring(1)}");

                    c.WithBody(body =>
                    {
                        foreach (var f in def.Fields)
                        {
                            var param = $"{char.ToLower(f.Name[0])}{f.Name.Substring(1)}";
                            body.Assign($"_{param}", param);
                        }
                    });
                })

                // ToString override
                .WithMethod("ToString", CsType.String, m => m
                    .WithXmlDoc(doc => doc.WithInheritDoc())
                    .WithOverrideModifier()
                    .WithBody(body =>
                    {
                        var parts = string.Join(", ",
                            def.Fields.ConvertAll(f => $"{f.Name}={{{f.Name}}}"));
                        body.Return($"$\"{def.ClassName}({parts})\"");
                    })))
            .Save();
    }
}
```

### Output

```csharp
using UnityEngine;

namespace MyProject.Messages
{
    /// <summary>
    /// Immutable message raised when a player spawns into the world.
    /// </summary>
    public sealed class PlayerSpawnedMessage
    {
        private readonly int _playerId;
        private readonly Vector3 _position;
        private readonly int _teamIndex;

        /// <summary>
        /// The unique identifier of the spawned player.
        /// </summary>
        public int PlayerId => _playerId;
        /// <summary>
        /// The world-space spawn position.
        /// </summary>
        public Vector3 Position => _position;
        /// <summary>
        /// The team the player belongs to.
        /// </summary>
        public int TeamIndex => _teamIndex;

        /// <summary>
        /// Initializes a new PlayerSpawnedMessage.
        /// </summary>
        /// <param name="playerId">The unique identifier of the spawned player.</param>
        /// <param name="position">The world-space spawn position.</param>
        /// <param name="teamIndex">The team the player belongs to.</param>
        public PlayerSpawnedMessage(int playerId, Vector3 position, int teamIndex)
        {
            _playerId = playerId;
            _position = position;
            _teamIndex = teamIndex;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"PlayerSpawnedMessage(PlayerId={PlayerId}, Position={Position}, TeamIndex={TeamIndex})";
        }

    }

}
```

---

## Generating multiple types in one file

A single `FileBuilder` can contain multiple classes and structs. They are emitted in the order they were added.

```csharp
FileBuilder.Build("Generated/Events.cs")
    .WithNamespace("MyProject.Events")
    .WithClass("PlayerJoinedEvent", cls => cls.WithSealedModifier())
    .WithClass("PlayerLeftEvent",   cls => cls.WithSealedModifier())
    .WithStruct("EventHeader",      s   => s.WithReadOnlyModifier())
    .Save();
```

> **Note:** C# allows multiple top-level types per file, but style guides typically prefer one type per file. Use this feature deliberately.

---

## Previewing output without saving

During development it can be useful to inspect the generated code before writing to disk:

```csharp
var code = FileBuilder.Build("preview")
    .WithNamespace("MyProject")
    .WithClass("Preview", cls => cls.WithSealedModifier())
    .Emit();

Debug.Log(code);
```

---

## Indentation

The package uses tabs for indentation. The shared `IndentEmitter` instance tracks depth and is passed automatically through nested builders — you never need to manage it manually.
