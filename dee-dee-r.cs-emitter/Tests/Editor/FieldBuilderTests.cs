using System.Linq;
using NUnit.Framework;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class FieldBuilderTests
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
        public void Emit_Defaults_EmitsPrivateField()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("private int _myField;"));
        }

        [Test]
        public void Emit_Defaults_NoStaticModifier()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(field, Does.Not.Contain("static"));
        }

        [Test]
        public void Emit_Defaults_NoReadOnlyModifier()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(field, Does.Not.Contain("readonly"));
        }

        [Test]
        public void Emit_Defaults_NoDefaultValue()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(field, Does.Not.Contain("="));
        }

        // -------------------------------------------------------------------------
        // Visibility
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_PublicVisibility_EmitsPublic()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .WithVisibility(Visibility.Public)
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("public int _myField;"));
        }

        [Test]
        public void Emit_ProtectedVisibility_EmitsProtected()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .WithVisibility(Visibility.Protected)
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("protected int _myField;"));
        }

        [Test]
        public void Emit_InternalVisibility_EmitsInternal()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .WithVisibility(Visibility.Internal)
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("internal int _myField;"));
        }

        [Test]
        public void Emit_ProtectedInternalVisibility_EmitsProtectedInternal()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .WithVisibility(Visibility.ProtectedInternal)
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("protected internal int _myField;"));
        }

        // -------------------------------------------------------------------------
        // Modifiers
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithStaticModifier_EmitsStatic()
        {
            var field = FieldBuilder.Build(_emitter, "MyField", CsType.Int)
                .WithVisibility(Visibility.Public)
                .WithStaticModifier()
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("public static int MyField;"));
        }

        [Test]
        public void Emit_WithReadOnlyModifier_EmitsReadOnly()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .WithReadOnly()
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("private readonly int _myField;"));
        }

        [Test]
        public void Emit_WithStaticAndReadOnly_EmitsCorrectOrder()
        {
            var field = FieldBuilder.Build(_emitter, "MyField", CsType.Of("Vector3"))
                .WithVisibility(Visibility.Public)
                .WithStaticModifier()
                .WithReadOnly()
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("public static readonly Vector3 MyField;"));
        }
        
        // -------------------------------------------------------------------------
        // Const modifier
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithConstModifier_EmitsConst()
        {
            var field = FieldBuilder.Build(_emitter, "MyConst", CsType.String)
                .WithConstModifier()
                .Emit();

            Assert.That(field, Does.Contain("const"));
        }

        [Test]
        public void Emit_WithConstModifier_EmitsCorrectDeclaration()
        {
            var field = FieldBuilder.Build(_emitter, "ResourcePath", CsType.String)
                .WithVisibility(Visibility.Private)
                .WithConstModifier()
                .WithDefaultValue("\"MessageBusProvider\"")
                .Emit();

            Assert.That(Normalize(field),
                Is.EqualTo("private const string ResourcePath = \"MessageBusProvider\";"));
        }

        [Test]
        public void Emit_WithConstModifier_DoesNotEmitStatic()
        {
            var field = FieldBuilder.Build(_emitter, "MyConst", CsType.String)
                .WithConstModifier()
                .Emit();

            Assert.That(field, Does.Not.Contain("static"));
        }

        [Test]
        public void Emit_WithConstModifier_DoesNotEmitReadOnly()
        {
            var field = FieldBuilder.Build(_emitter, "MyConst", CsType.String)
                .WithConstModifier()
                .WithReadOnly()
                .Emit();

            Assert.That(field, Does.Not.Contain("readonly"));
        }

        [Test]
        public void Emit_WithConstModifier_ConstAppearsAfterVisibility()
        {
            var field = FieldBuilder.Build(_emitter, "MyConst", CsType.String)
                .WithVisibility(Visibility.Public)
                .WithConstModifier()
                .Emit();

            var declaration = Normalize(field);
            var publicIndex = declaration.IndexOf("public");
            var constIndex = declaration.IndexOf("const");

            Assert.That(publicIndex, Is.LessThan(constIndex));
        }

        [Test]
        public void Emit_WithConstModifierPublic_EmitsCorrectly()
        {
            var field = FieldBuilder.Build(_emitter, "MaxCount", CsType.Int)
                .WithVisibility(Visibility.Public)
                .WithConstModifier()
                .WithDefaultValue("100")
                .Emit();

            Assert.That(Normalize(field),
                Is.EqualTo("public const int MaxCount = 100;"));
        }

        // -------------------------------------------------------------------------
        // Types
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_GenericType_EmitsCorrectly()
        {
            var field = FieldBuilder.Build(_emitter, "_items",
                    CsType.ListOf(CsType.Of("Vector3")))
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("private List<Vector3> _items;"));
        }

        [Test]
        public void Emit_NestedGenericType_EmitsCorrectly()
        {
            var field = FieldBuilder.Build(_emitter, "_map",
                    CsType.DictionaryOf(CsType.String, CsType.ListOf(CsType.Int)))
                .Emit();

            Assert.That(Normalize(field),
                Is.EqualTo("private Dictionary<string, List<int>> _map;"));
        }

        // -------------------------------------------------------------------------
        // Default value
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithDefaultValue_EmitsAssignment()
        {
            var field = FieldBuilder.Build(_emitter, "_count", CsType.Int)
                .WithDefaultValue("0")
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("private int _count = 0;"));
        }

        [Test]
        public void Emit_WithDefaultValueAndStaticReadOnly_EmitsCorrectly()
        {
            var field = FieldBuilder.Build(_emitter, "DefaultPosition", CsType.Of("Vector3"))
                .WithVisibility(Visibility.Public)
                .WithStaticModifier()
                .WithReadOnly()
                .WithDefaultValue("Vector3.zero")
                .Emit();

            Assert.That(Normalize(field),
                Is.EqualTo("public static readonly Vector3 DefaultPosition = Vector3.zero;"));
        }

        // -------------------------------------------------------------------------
        // Indentation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithOneIndentLevel_EmitsWithOneTab()
        {
            _emitter.Push();

            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(field, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithTwoIndentLevels_EmitsWithTwoTabs()
        {
            _emitter.Push();
            _emitter.Push();

            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(field, Does.StartWith("\t\t"));
        }

        [Test]
        public void Emit_WithIndent_NormalizedContentUnchanged()
        {
            _emitter.Push();

            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(Normalize(field), Is.EqualTo("private int _myField;"));
        }

        // -------------------------------------------------------------------------
        // XmlDoc
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithXmlDoc_EmitsDocBeforeDeclaration()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .WithXmlDoc(doc => doc.WithSummary("My field."))
                .Emit();

            var summaryIndex = field.IndexOf("/// <summary>");
            var declarationIndex = field.IndexOf("private int _myField;");

            Assert.That(summaryIndex, Is.LessThan(declarationIndex));
        }

        [Test]
        public void Emit_WithXmlDoc_ContainsSummaryContent()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .WithXmlDoc(doc => doc.WithSummary("My field."))
                .Emit();

            Assert.That(field, Does.Contain("My field."));
        }

        [Test]
        public void Emit_WithoutXmlDoc_ContainsNoTripleSlash()
        {
            var field = FieldBuilder.Build(_emitter, "_myField", CsType.Int)
                .Emit();

            Assert.That(field, Does.Not.Contain("///"));
        }
    }
}