using System;
using System.Linq;
using NUnit.Framework;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class PropertyBuilderTests
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
        public void Emit_Defaults_EmitsPublicProperty()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .Emit();

            Assert.That(property, Does.Contain("public"));
            Assert.That(property, Does.Contain("int"));
            Assert.That(property, Does.Contain("MyProperty"));
        }

        [Test]
        public void Emit_Defaults_NoStaticModifier()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .Emit();

            Assert.That(property, Does.Not.Contain("static"));
        }

        [Test]
        public void Emit_Defaults_NoDefaultValue()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .Emit();

            Assert.That(property, Does.Not.Contain("="));
        }

        // -------------------------------------------------------------------------
        // Visibility
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_PublicVisibility_EmitsPublic()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithVisibility(Visibility.Public)
                .WithAutoGetter()
                .Emit();

            Assert.That(property, Does.Contain("public"));
        }

        [Test]
        public void Emit_PrivateVisibility_EmitsPrivate()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithVisibility(Visibility.Private)
                .WithAutoGetter()
                .Emit();

            Assert.That(property, Does.Contain("private"));
        }

        [Test]
        public void Emit_InternalVisibility_EmitsInternal()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithVisibility(Visibility.Internal)
                .WithAutoGetter()
                .Emit();

            Assert.That(property, Does.Contain("internal"));
        }

        // -------------------------------------------------------------------------
        // Auto property
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_AutoGetterOnly_EmitsGetOnly()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .Emit();

            Assert.That(Normalize(property), Is.EqualTo("public int MyProperty { get; }"));
        }

        [Test]
        public void Emit_AutoGetterAndSetter_EmitsGetSet()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithAutoSetter()
                .Emit();

            Assert.That(Normalize(property), Is.EqualTo("public int MyProperty { get; set; }"));
        }

        [Test]
        public void Emit_AutoGetterWithPrivateSetter_EmitsPrivateSet()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithAutoSetter(Visibility.Private)
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public int MyProperty { get; private set; }"));
        }

        [Test]
        public void Emit_AutoGetterWithProtectedSetter_EmitsProtectedSet()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithAutoSetter(Visibility.Protected)
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public int MyProperty { get; protected set; }"));
        }

        [Test]
        public void Emit_AutoSetterOnly_EmitsSetOnly()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoSetter()
                .Emit();

            Assert.That(Normalize(property), Is.EqualTo("public int MyProperty { set; }"));
        }

        // -------------------------------------------------------------------------
        // Auto property with default value
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_AutoGetterSetterWithDefaultValue_EmitsAssignment()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithAutoSetter()
                .WithDefaultValue("42")
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public int MyProperty { get; set; } = 42;"));
        }

        [Test]
        public void Emit_AutoGetterOnlyWithDefaultValue_EmitsAssignment()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithDefaultValue("0")
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public int MyProperty { get; } = 0;"));
        }

        // -------------------------------------------------------------------------
        // Static modifier
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithStaticModifier_EmitsStatic()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithStaticModifier()
                .WithAutoGetter()
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public static int MyProperty { get; }"));
        }

        // -------------------------------------------------------------------------
        // Expression-bodied property
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_ExpressionGetter_EmitsArrowSyntax()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithExpressionGetter("_myField")
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public int MyProperty => _myField;"));
        }

        [Test]
        public void Emit_ExpressionGetterWithStaticModifier_EmitsCorrectly()
        {
            var property = PropertyBuilder.Build(_emitter, "Combat", CsType.Of("CombatDef"))
                .WithStaticModifier()
                .WithExpressionGetter("Runtime.Combat")
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public static CombatDef Combat => Runtime.Combat;"));
        }

        [Test]
        public void Emit_ExpressionGetterWithPrivateVisibility_EmitsCorrectly()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithVisibility(Visibility.Private)
                .WithExpressionGetter("_myField")
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("private int MyProperty => _myField;"));
        }

        // -------------------------------------------------------------------------
        // Full property with explicit bodies
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_ExplicitGetter_ContainsGetKeyword()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithGetter(getter => getter.Return("_myField"))
                .Emit();

            Assert.That(property, Does.Contain("get"));
        }

        [Test]
        public void Emit_ExplicitGetter_ContainsReturnStatement()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithGetter(getter => getter.Return("_myField"))
                .Emit();

            Assert.That(property, Does.Contain("return _myField;"));
        }

        [Test]
        public void Emit_ExplicitSetter_ContainsSetKeyword()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithGetter(getter => getter.Return("_myField"))
                .WithSetter(setter => setter.Assign("_myField", "value"))
                .Emit();

            Assert.That(property, Does.Contain("set"));
        }

        [Test]
        public void Emit_ExplicitSetter_ContainsAssignment()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithGetter(getter => getter.Return("_myField"))
                .WithSetter(setter => setter.Assign("_myField", "value"))
                .Emit();

            Assert.That(property, Does.Contain("_myField = value;"));
        }

        [Test]
        public void Emit_ExplicitGetterWithPrivateVisibility_EmitsPrivateGet()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithGetter(getter => getter.Return("_myField"), Visibility.Private)
                .Emit();

            Assert.That(property, Does.Contain("private get"));
        }

        [Test]
        public void Emit_ExplicitSetterWithPrivateVisibility_EmitsPrivateSet()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithGetter(getter => getter.Return("_myField"))
                .WithSetter(setter => setter.Assign("_myField", "value"), Visibility.Private)
                .Emit();

            Assert.That(property, Does.Contain("private set"));
        }

        // -------------------------------------------------------------------------
        // Generic types
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_GenericType_EmitsCorrectly()
        {
            var property = PropertyBuilder.Build(_emitter, "Items",
                    CsType.ListOf(CsType.Of("Vector3")))
                .WithAutoGetter()
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public List<Vector3> Items { get; }"));
        }

        [Test]
        public void Emit_NestedGenericType_EmitsCorrectly()
        {
            var property = PropertyBuilder.Build(_emitter, "Map",
                    CsType.DictionaryOf(CsType.String, CsType.ListOf(CsType.Int)))
                .WithAutoGetter()
                .WithAutoSetter(Visibility.Private)
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public Dictionary<string, List<int>> Map { get; private set; }"));
        }

        // -------------------------------------------------------------------------
        // Indentation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithOneIndentLevel_StartsWithOneTab()
        {
            _emitter.Push();

            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .Emit();

            Assert.That(property, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithTwoIndentLevels_StartsWithTwoTabs()
        {
            _emitter.Push();
            _emitter.Push();

            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .Emit();

            Assert.That(property, Does.StartWith("\t\t"));
        }

        [Test]
        public void Emit_AutoPropertyWithIndent_NormalizedContentUnchanged()
        {
            _emitter.Push();

            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithAutoSetter()
                .Emit();

            Assert.That(Normalize(property),
                Is.EqualTo("public int MyProperty { get; set; }"));
        }

        // -------------------------------------------------------------------------
        // XmlDoc
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithXmlDoc_DocAppearsBeforeDeclaration()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithXmlDoc(doc => doc.WithSummary("My property."))
                .Emit();

            var summaryIndex = property.IndexOf("/// <summary>");
            var declarationIndex = property.IndexOf("public int MyProperty");

            Assert.That(summaryIndex, Is.LessThan(declarationIndex));
        }

        [Test]
        public void Emit_WithXmlDoc_ContainsSummaryContent()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .WithXmlDoc(doc => doc.WithSummary("My property."))
                .Emit();

            Assert.That(property, Does.Contain("My property."));
        }

        [Test]
        public void Emit_WithoutXmlDoc_ContainsNoTripleSlash()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithAutoGetter()
                .Emit();

            Assert.That(property, Does.Not.Contain("///"));
        }

        // -------------------------------------------------------------------------
        // Expression body guard
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_ExpressionBodyWithExplicitGetter_ThrowsInvalidOperationException()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithExpressionGetter("_myField")
                .WithGetter(getter => getter.Return("_myField"));

            Assert.Throws<InvalidOperationException>(() => property.Emit());
        }

        [Test]
        public void Emit_ExpressionBodyWithSetter_ThrowsInvalidOperationException()
        {
            var property = PropertyBuilder.Build(_emitter, "MyProperty", CsType.Int)
                .WithExpressionGetter("_myField")
                .WithSetter(setter => setter.Assign("_myField", "value"));

            Assert.Throws<InvalidOperationException>(() => property.Emit());
        }
    }
}