# dee-dee-r.cs-emitter

Development repository for the **C# Emitter** Unity package — a fluent builder library for generating C# source code programmatically from Editor scripts.

For package documentation, see [dee-dee-r.cs-emitter/README.md](dee-dee-r.cs-emitter/README.md).

---

## Repository structure

```
dee-dee-r.cs-emitter/
├── dee-dee-r.cs-emitter/   # The Unity package
└── projects/
    └── cs-emitter-main/    # Companion Unity project for development and testing
```

### `dee-dee-r.cs-emitter/`

The package itself. This is the folder you add to a Unity project via the Package Manager.

### `projects/cs-emitter-main/`

A Unity 6000.3 project used to develop and test the package. It references the package directly from the local filesystem:

```json
"dee-dee-r.cs-emitter": "file:../../../dee-dee-r.cs-emitter"
```

Open this project in Unity to run the package's Editor test suite or to iterate on the package code with live reload.
