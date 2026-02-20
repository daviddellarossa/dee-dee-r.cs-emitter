using System.Linq;
using NUnit.Framework;
using DeeDeeR.CsEmitter;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class ConstructorBuilderTests
    {
        private IndentEmitter _emitter;

        [SetUp]
        public void SetUp()
        {
            _emitter = new IndentEmitter();
        }

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
        public void Emit_Defaults_EmitsPublicConstructor()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(ctor, Does.Contain("public"));
        }

        [Test]
        public void Emit_Defaults_EmitsClassName()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(ctor, Does.Contain("MyClass"));
        }

        [Test]
        public void Emit_Defaults_EmitsEmptyParameterList()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(Normalize(ctor), Does.StartWith("public MyClass()"));
        }

        [Test]
        public void Emit_Defaults_EmitsEmptyBody()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(Normalize(ctor), Is.EqualTo("public MyClass()\n{" +
                                                     "\n}"));
        }

        // -------------------------------------------------------------------------
        // Visibility
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_PrivateVisibility_EmitsPrivate()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithVisibility(Visibility.Private)
                .Emit();

            Assert.That(ctor, Does.Contain("private"));
        }

        [Test]
        public void Emit_ProtectedVisibility_EmitsProtected()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithVisibility(Visibility.Protected)
                .Emit();

            Assert.That(ctor, Does.Contain("protected"));
        }

        [Test]
        public void Emit_InternalVisibility_EmitsInternal()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithVisibility(Visibility.Internal)
                .Emit();

            Assert.That(ctor, Does.Contain("internal"));
        }

        // -------------------------------------------------------------------------
        // Parameters
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_SingleParameter_EmitsCorrectly()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .Emit();

            Assert.That(Normalize(ctor), Does.StartWith("public MyClass(int count)"));
        }

        [Test]
        public void Emit_MultipleParameters_EmitsAllInOrder()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .WithParameter(CsType.String, "name")
                .WithParameter(CsType.Bool, "flag")
                .Emit();

            Assert.That(Normalize(ctor),
                Does.StartWith("public MyClass(int count, string name, bool flag)"));
        }

        [Test]
        public void Emit_GenericParameter_EmitsCorrectly()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.ListOf(CsType.Of("Vector3")), "points")
                .Emit();

            Assert.That(ctor, Does.Contain("List<Vector3> points"));
        }

        [Test]
        public void Emit_ParametersFromCollection_EmitsAllCorrectly()
        {
            var parameters = new[]
            {
                (Type: CsType.Int, Name: "count"),
                (Type: CsType.String, Name: "label"),
            };

            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameters(parameters, p => p.Type, p => p.Name)
                .Emit();

            Assert.That(Normalize(ctor),
                Does.StartWith("public MyClass(int count, string label)"));
        }

        // -------------------------------------------------------------------------
        // Base and this chaining
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithBaseCall_EmitsBaseChain()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .WithBaseCall("count")
                .Emit();

            Assert.That(Normalize(ctor),
                Does.StartWith("public MyClass(int count) : base(count)"));
        }

        [Test]
        public void Emit_WithBaseCallMultipleArgs_EmitsAllArgs()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .WithParameter(CsType.String, "name")
                .WithBaseCall("count", "name")
                .Emit();

            Assert.That(Normalize(ctor),
                Does.StartWith("public MyClass(int count, string name) : base(count, name)"));
        }

        [Test]
        public void Emit_WithThisCall_EmitsThisChain()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithThisCall("0", "\"default\"")
                .Emit();

            Assert.That(Normalize(ctor),
                Does.StartWith("public MyClass() : this(0, \"default\")"));
        }

        [Test]
        public void Emit_WithEmptyBaseCall_EmitsEmptyBase()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithBaseCall()
                .Emit();

            Assert.That(Normalize(ctor),
                Does.StartWith("public MyClass() : base()"));
        }

        // -------------------------------------------------------------------------
        // Body
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithBody_EmitsBodyContent()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .WithBody(body => body
                    .Assign("_count", "count"))
                .Emit();

            Assert.That(ctor, Does.Contain("_count = count;"));
        }

        [Test]
        public void Emit_WithMultipleAssignments_EmitsAllInOrder()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .WithParameter(CsType.String, "name")
                .WithBody(body => body
                    .Assign("_count", "count")
                    .Assign("_name", "name"))
                .Emit();

            var countIndex = ctor.IndexOf("_count = count;");
            var nameIndex = ctor.IndexOf("_name = name;");

            Assert.That(countIndex, Is.LessThan(nameIndex));
        }

        [Test]
        public void Emit_BodyIsIndented_BodyContentIndentedMoreThanSignature()
        {
            _emitter.Push();

            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .WithBody(body => body
                    .Assign("_count", "count"))
                .Emit();

            var signatureLine = ctor.Split('\n')
                .First(l => l.Contains("MyClass("));
            var bodyLine = ctor.Split('\n')
                .First(l => l.Contains("_count = count;"));

            var signatureIndent = signatureLine.TakeWhile(c => c == '\t').Count();
            var bodyIndent = bodyLine.TakeWhile(c => c == '\t').Count();

            Assert.That(bodyIndent, Is.GreaterThan(signatureIndent));
        }

        // -------------------------------------------------------------------------
        // Indentation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithOneIndentLevel_StartsWithOneTab()
        {
            _emitter.Push();

            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(ctor, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithTwoIndentLevels_StartsWithTwoTabs()
        {
            _emitter.Push();
            _emitter.Push();

            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(ctor, Does.StartWith("\t\t"));
        }

        [Test]
        public void Emit_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            _emitter.Push();
            var expectedIndent = _emitter.Get();

            ConstructorBuilder.Build(_emitter, "MyClass")
                .WithBody(body => body.Assign("_x", "1"))
                .Emit();

            Assert.That(_emitter.Get(), Is.EqualTo(expectedIndent));
        }

        // -------------------------------------------------------------------------
        // XmlDoc
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithXmlDoc_DocAppearsBeforeSignature()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithXmlDoc(doc => doc.WithSummary("Initializes a new instance."))
                .Emit();

            var summaryIndex = ctor.IndexOf("/// <summary>");
            var signatureIndex = ctor.IndexOf("public MyClass()");

            Assert.That(summaryIndex, Is.LessThan(signatureIndex));
        }

        [Test]
        public void Emit_WithXmlDocAndParams_ContainsParamTags()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .WithParameter(CsType.Int, "count")
                .WithXmlDoc(doc => doc
                    .WithSummary("Initializes a new instance.")
                    .WithParam("count", "The count."))
                .Emit();

            Assert.That(ctor, Does.Contain("<param name=\"count\">The count.</param>"));
        }

        [Test]
        public void Emit_WithoutXmlDoc_ContainsNoTripleSlash()
        {
            var ctor = ConstructorBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(ctor, Does.Not.Contain("///"));
        }
    }
}