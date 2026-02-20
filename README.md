# dee-dee-r.cs-emitter

Development repository for the **C# Emitter** Unity package — a fluent builder library for generating C# source code programmatically from Editor scripts.

For package documentation, see [dee-dee-r.cs-emitter/README.md](dee-dee-r.cs-emitter/README.md).

---

## Installation

### Option 1 — Git URL (Package Manager UI)

1. Open **Window > Package Manager** in the Unity Editor.
2. Click the **+** button in the top-left corner and select **Add package from git URL…**
3. Enter the URL of this repository followed by the path to the package folder:
   ```
   https://github.com/DeeDeeR/dee-dee-r.cs-emitter.git?path=dee-dee-r.cs-emitter
   ```
4. Click **Add**. Unity will fetch and import the package automatically.

### Option 2 — Git URL (manifest.json)

Open `Packages/manifest.json` in your project and add the following entry to the `dependencies` object:

```json
"dee-dee-r.cs-emitter": "https://github.com/daviddellarossa/dee-dee-r.cs-emitter.git?path=dee-dee-r.cs-emitter"
```

### Option 3 — Local file path

Clone this repository anywhere on your machine, then add the following entry to your project's `Packages/manifest.json`, adjusting the path to match your local clone:

```json
"dee-dee-r.cs-emitter": "file:/path/to/dee-dee-r.cs-emitter/dee-dee-r.cs-emitter"
```

This option is useful if you want to modify the package source alongside your project.

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
