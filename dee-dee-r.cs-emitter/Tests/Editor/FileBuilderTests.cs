using System.Linq;
using NUnit.Framework;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class FileBuilderTests
    {
        private const string DummyPath = "test/output.cs";

        // -------------------------------------------------------------------------
        // Test helpers
        // -------------------------------------------------------------------------

        private static string Normalize(string code)
            => string.Join("\n", code
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l)));

        // -------------------------------------------------------------------------
        // Defaults
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_Defaults_EmitsEmptyFile()
        {
            var result = FileBuilder.Build(DummyPath).Emit();
            Assert.That(result.Trim(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Emit_Defaults_NoNamespace()
        {
            var result = FileBuilder.Build(DummyPath).Emit();
            Assert.That(result, Does.Not.Contain("namespace"));
        }

        [Test]
        public void Emit_Defaults_NoUsings()
        {
            var result = FileBuilder.Build(DummyPath).Emit();
            Assert.That(result, Does.Not.Contain("using"));
        }

        // -------------------------------------------------------------------------
        // Usings
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithSingleUsing_EmitsUsing()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithUsing("System")
                .Emit();

            Assert.That(result, Does.Contain("using System;"));
        }

        [Test]
        public void Emit_WithMultipleUsings_EmitsAllUsings()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithUsings("System", "System.Collections.Generic", "UnityEngine")
                .Emit();

            Assert.That(result, Does.Contain("using System;"));
            Assert.That(result, Does.Contain("using System.Collections.Generic;"));
            Assert.That(result, Does.Contain("using UnityEngine;"));
        }

        [Test]
        public void Emit_WithUsings_EmitsInAlphabeticalOrder()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithUsings("UnityEngine", "System", "System.Collections.Generic")
                .Emit();

            var lines = result.Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.StartsWith("using"))
                .ToList();

            Assert.That(lines[0], Is.EqualTo("using System;"));
            Assert.That(lines[1], Is.EqualTo("using System.Collections.Generic;"));
            Assert.That(lines[2], Is.EqualTo("using UnityEngine;"));
        }

        [Test]
        public void Emit_WithDuplicateUsings_EmitsOnlyOnce()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithUsing("System")
                .WithUsing("System")
                .Emit();

            var count = result.Split('\n')
                .Count(l => l.Trim() == "using System;");

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Emit_WithUsingsFromCollection_EmitsAllUsings()
        {
            var namespaces = new[] { "System", "System.Collections.Generic", "UnityEngine" };

            var result = FileBuilder.Build(DummyPath)
                .WithUsings(namespaces, n => n)
                .Emit();

            Assert.That(result, Does.Contain("using System;"));
            Assert.That(result, Does.Contain("using System.Collections.Generic;"));
            Assert.That(result, Does.Contain("using UnityEngine;"));
        }

        [Test]
        public void Emit_WithUsingsFromCollection_DeduplicatesUsings()
        {
            var namespaces = new[] { "System", "System", "UnityEngine" };

            var result = FileBuilder.Build(DummyPath)
                .WithUsings(namespaces, n => n)
                .Emit();

            var count = result.Split('\n')
                .Count(l => l.Trim() == "using System;");

            Assert.That(count, Is.EqualTo(1));
        }

        // -------------------------------------------------------------------------
        // Namespace
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithNamespace_EmitsNamespace()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithNamespace("MyProject.Generated")
                .Emit();

            Assert.That(result, Does.Contain("namespace MyProject.Generated"));
        }

        [Test]
        public void Emit_WithNamespace_EmitsOpenAndCloseBraces()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithNamespace("MyProject.Generated")
                .Emit();

            Assert.That(result, Does.Contain("{"));
            Assert.That(result, Does.Contain("}"));
        }

        [Test]
        public void Emit_WithoutNamespace_ClassAppearsAtTopLevel()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithClass("MyClass")
                .Emit();

            var lines = result.Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();

            Assert.That(lines.First(), Does.StartWith("public class MyClass"));
        }

        // -------------------------------------------------------------------------
        // Usings appear before namespace
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_UsingsAppearBeforeNamespace()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithUsing("System")
                .WithNamespace("MyProject.Generated")
                .Emit();

            var usingIndex = result.IndexOf("using System;");
            var namespaceIndex = result.IndexOf("namespace MyProject.Generated");

            Assert.That(usingIndex, Is.LessThan(namespaceIndex));
        }

        // -------------------------------------------------------------------------
        // Classes
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithClass_ContainsClass()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithClass("MyClass")
                .Emit();

            Assert.That(result, Does.Contain("public class MyClass"));
        }

        [Test]
        public void Emit_WithMultipleClasses_ContainsAllClasses()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithClass("ClassA")
                .WithClass("ClassB")
                .Emit();

            Assert.That(result, Does.Contain("public class ClassA"));
            Assert.That(result, Does.Contain("public class ClassB"));
        }

        [Test]
        public void Emit_WithClassInNamespace_ClassIsIndented()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithNamespace("MyProject.Generated")
                .WithClass("MyClass")
                .Emit();

            var classLine = result.Split('\n')
                .First(l => l.Contains("public class MyClass"));

            Assert.That(classLine, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithClassOutOverload_ClassIsPopulatedFromLoop()
        {
            var fields = new[]
            {
                (Name: "_count", Type: CsType.Int),
                (Name: "_name",  Type: CsType.String),
            };

            var fileBuilder = FileBuilder.Build(DummyPath)
                .WithNamespace("MyProject.Generated");

            fileBuilder.WithClass("MyClass", out var classBuilder);

            foreach (var field in fields)
                classBuilder.WithField(field.Name, field.Type);

            var result = fileBuilder.Emit();

            Assert.That(result, Does.Contain("int _count;"));
            Assert.That(result, Does.Contain("string _name;"));
        }

        // -------------------------------------------------------------------------
        // Structs
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithStruct_ContainsStruct()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithStruct("MyStruct")
                .Emit();

            Assert.That(result, Does.Contain("public struct MyStruct"));
        }

        [Test]
        public void Emit_WithMultipleStructs_ContainsAllStructs()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithStruct("StructA")
                .WithStruct("StructB")
                .Emit();

            Assert.That(result, Does.Contain("public struct StructA"));
            Assert.That(result, Does.Contain("public struct StructB"));
        }

        [Test]
        public void Emit_WithStructInNamespace_StructIsIndented()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithNamespace("MyProject.Generated")
                .WithStruct("MyStruct")
                .Emit();

            var structLine = result.Split('\n')
                .First(l => l.Contains("public struct MyStruct"));

            Assert.That(structLine, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithStructOutOverload_StructIsPopulatedFromLoop()
        {
            var fields = new[]
            {
                (Name: "Position", Type: CsType.Of("Vector3")),
                (Name: "Normal",   Type: CsType.Of("Vector3")),
            };

            var fileBuilder = FileBuilder.Build(DummyPath)
                .WithNamespace("MyProject.Generated");

            fileBuilder.WithStruct("MyStruct", out var structBuilder);

            foreach (var field in fields)
                structBuilder.WithField(field.Name, field.Type);

            var result = fileBuilder.Emit();

            Assert.That(result, Does.Contain("Vector3 Position;"));
            Assert.That(result, Does.Contain("Vector3 Normal;"));
        }

        // -------------------------------------------------------------------------
        // Mixed classes and structs
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithClassAndStruct_ContainsBoth()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithClass("MyClass")
                .WithStruct("MyStruct")
                .Emit();

            Assert.That(result, Does.Contain("public class MyClass"));
            Assert.That(result, Does.Contain("public struct MyStruct"));
        }

        [Test]
        public void Emit_ClassesAppearBeforeStructs()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithStruct("MyStruct")
                .WithClass("MyClass")
                .Emit();

            var classIndex = result.IndexOf("public class MyClass");
            var structIndex = result.IndexOf("public struct MyStruct");

            Assert.That(classIndex, Is.LessThan(structIndex));
        }

        // -------------------------------------------------------------------------
        // Emit is idempotent
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_CalledTwice_ProducesSameOutput()
        {
            var fileBuilder = FileBuilder.Build(DummyPath)
                .WithUsing("System")
                .WithNamespace("MyProject.Generated")
                .WithClass("MyClass");

            var first = fileBuilder.Emit();
            var second = fileBuilder.Emit();

            Assert.That(first, Is.EqualTo(second));
        }

        // -------------------------------------------------------------------------
        // Full integration
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_FullFile_ProducesCorrectStructure()
        {
            var result = FileBuilder.Build(DummyPath)
                .WithUsings("System", "System.Collections.Generic", "UnityEngine")
                .WithNamespace("MyProject.Generated")
                .WithClass("MyClass", cls => cls
                    .WithSealedModifier()
                    .WithXmlDoc(doc => doc.WithSummary("A generated class."))
                    .WithField("_count", CsType.Int, f => f
                        .WithVisibility(Visibility.Private))
                    .WithProperty("Count", CsType.Int, p => p
                        .WithAutoGetter()
                        .WithAutoSetter(Visibility.Private))
                    .WithConstructor(c => c
                        .WithBody(body => body.Assign("_count", "0")))
                    .WithMethod("Increment", CsType.Void, m => m
                        .WithBody(body => body.CompoundAssign("_count", "+", "1"))))
                .Emit();

            // Structure
            Assert.That(result, Does.Contain("using System;"));
            Assert.That(result, Does.Contain("using System.Collections.Generic;"));
            Assert.That(result, Does.Contain("using UnityEngine;"));
            Assert.That(result, Does.Contain("namespace MyProject.Generated"));
            Assert.That(result, Does.Contain("public sealed class MyClass"));

            // XmlDoc
            Assert.That(result, Does.Contain("/// <summary>"));
            Assert.That(result, Does.Contain("A generated class."));

            // Members
            Assert.That(result, Does.Contain("private int _count;"));
            Assert.That(result, Does.Contain("public int Count { get; private set; }"));
            Assert.That(result, Does.Contain("_count = 0;"));
            Assert.That(result, Does.Contain("_count += 1;"));

            // Ordering
            var usingIndex = result.IndexOf("using System;");
            var namespaceIndex = result.IndexOf("namespace MyProject.Generated");
            var classIndex = result.IndexOf("public sealed class MyClass");
            var fieldIndex = result.IndexOf("private int _count;");
            var propertyIndex = result.IndexOf("public int Count");
            var ctorIndex = result.IndexOf("MyClass()");
            var methodIndex = result.IndexOf("void Increment()");

            Assert.That(usingIndex, Is.LessThan(namespaceIndex));
            Assert.That(namespaceIndex, Is.LessThan(classIndex));
            Assert.That(classIndex, Is.LessThan(fieldIndex));
            Assert.That(fieldIndex, Is.LessThan(propertyIndex));
            Assert.That(propertyIndex, Is.LessThan(ctorIndex));
            Assert.That(ctorIndex, Is.LessThan(methodIndex));
        }
    }
}