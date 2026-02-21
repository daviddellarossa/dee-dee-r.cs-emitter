using System;
using System.Linq;
using NUnit.Framework;
using DeeDeeR.CsEmitter;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class StructBuilderTests
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

        private static int IndentLevel(string line)
            => line.TakeWhile(c => c == '\t').Count();

        // -------------------------------------------------------------------------
        // Defaults
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_Defaults_EmitsPublicStruct()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("public struct MyStruct"));
        }

        [Test]
        public void Emit_Defaults_EmitsOpenAndCloseBraces()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(str, Does.Contain("{"));
            Assert.That(str, Does.Contain("}"));
        }

        [Test]
        public void Emit_Defaults_NoReadOnlyModifier()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(str, Does.Not.Contain("readonly"));
        }

        [Test]
        public void Emit_Defaults_NoPartialModifier()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(str, Does.Not.Contain("partial"));
        }

        [Test]
        public void Emit_Defaults_UsesStructKeyword()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(str, Does.Contain("struct"));
        }

        [Test]
        public void Emit_Defaults_DoesNotContainClassKeyword()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(str, Does.Not.Contain("class"));
        }

        // -------------------------------------------------------------------------
        // Visibility
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_InternalVisibility_EmitsInternal()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithVisibility(Visibility.Internal)
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("internal struct MyStruct"));
        }

        [Test]
        public void Emit_PrivateVisibility_EmitsPrivate()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithVisibility(Visibility.Private)
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("private struct MyStruct"));
        }

        // -------------------------------------------------------------------------
        // Modifiers
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithReadOnlyModifier_EmitsReadOnly()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithReadOnlyModifier()
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("public readonly struct MyStruct"));
        }

        [Test]
        public void Emit_WithPartialModifier_EmitsPartial()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithPartialModifier()
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("public partial struct MyStruct"));
        }

        [Test]
        public void Emit_WithReadOnlyAndPartial_EmitsBothModifiers()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithReadOnlyModifier()
                .WithPartialModifier()
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("public readonly partial struct MyStruct"));
        }

        [Test]
        public void Emit_ModifierOrder_ReadOnlyBeforePartial()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithReadOnlyModifier()
                .WithPartialModifier()
                .Emit();

            var declaration = Normalize(str).Split('\n').First();
            var readonlyIndex = declaration.IndexOf("readonly");
            var partialIndex = declaration.IndexOf("partial");

            Assert.That(readonlyIndex, Is.LessThan(partialIndex));
        }

        // -------------------------------------------------------------------------
        // Generics
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_SingleTypeParameter_EmitsCorrectly()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithTypeParameter("T")
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("public struct MyStruct<T>"));
        }

        [Test]
        public void Emit_MultipleTypeParameters_EmitsAllInOrder()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithTypeParameter("TKey")
                .WithTypeParameter("TValue")
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("public struct MyStruct<TKey, TValue>"));
        }

        [Test]
        public void Emit_TypeParameterWithConstraint_EmitsWhereClause()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithTypeParameter("T")
                .WithTypeConstraint("T", "unmanaged")
                .Emit();

            Assert.That(str, Does.Contain("where T : unmanaged"));
        }

        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithField_ContainsField()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithField("Position", CsType.Of("Vector3"))
                .Emit();

            Assert.That(str, Does.Contain("Vector3 Position;"));
        }

        [Test]
        public void Emit_WithMultipleFields_ContainsAllFields()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithField("Position", CsType.Of("Vector3"))
                .WithField("Normal", CsType.Of("Vector3"))
                .WithField("Scale", CsType.Float)
                .Emit();

            Assert.That(str, Does.Contain("Vector3 Position;"));
            Assert.That(str, Does.Contain("Vector3 Normal;"));
            Assert.That(str, Does.Contain("float Scale;"));
        }

        [Test]
        public void Emit_WithField_FieldIsIndented()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithField("Position", CsType.Of("Vector3"))
                .Emit();

            var fieldLine = str.Split('\n').First(l => l.Contains("Vector3 Position;"));
            Assert.That(IndentLevel(fieldLine), Is.GreaterThan(0));
        }

        [Test]
        public void Emit_WithFieldsFromCollection_ContainsAllFields()
        {
            var fields = new[]
            {
                (Name: "Position", Type: CsType.Of("Vector3")),
                (Name: "Normal",   Type: CsType.Of("Vector3")),
                (Name: "Scale",    Type: CsType.Float),
            };

            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithFields(fields, f => f.Name, f => f.Type)
                .Emit();

            Assert.That(str, Does.Contain("Vector3 Position;"));
            Assert.That(str, Does.Contain("Vector3 Normal;"));
            Assert.That(str, Does.Contain("float Scale;"));
        }

        [Test]
        public void Emit_WithFieldsFromLoop_ContainsAllFields()
        {
            var fields = new[]
            {
                (Name: "X", Type: CsType.Float),
                (Name: "Y", Type: CsType.Float),
                (Name: "Z", Type: CsType.Float),
            };

            var str = StructBuilder.Build(_emitter, "MyStruct");

            foreach (var field in fields)
                str.WithField(field.Name, field.Type, out _);

            Assert.That(str.Emit(), Does.Contain("float X;"));
            Assert.That(str.Emit(), Does.Contain("float Y;"));
            Assert.That(str.Emit(), Does.Contain("float Z;"));
        }

        // -------------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithProperty_ContainsProperty()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithProperty("Scale", CsType.Float, p => p.WithAutoGetter())
                .Emit();

            Assert.That(str, Does.Contain("float Scale"));
        }

        [Test]
        public void Emit_WithPropertiesFromCollection_ContainsAllProperties()
        {
            var properties = new[]
            {
                (Name: "X", Type: CsType.Float),
                (Name: "Y", Type: CsType.Float),
            };

            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithProperties(properties, p => p.Name, p => p.Type,
                    (item, p) => p.WithAutoGetter());

            Assert.That(structBuilder.Emit(), Does.Contain("float X"));
            Assert.That(structBuilder.Emit(), Does.Contain("float Y"));
        }

        // -------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithConstructorOutFromOutOverload_ContainsConstructor()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct");
            structBuilder.WithConstructorOut(out var ctor);
            ctor.WithParameter(CsType.Of("Vector3"), "position")
                .WithBody(body => body.Assign("Position", "position"));

            Assert.That(structBuilder.Emit(), Does.Contain("MyStruct("));
            Assert.That(structBuilder.Emit(), Does.Contain("Position = position;"));
        }
        
        // -------------------------------------------------------------------------
        // WithConstructorIf
        // -------------------------------------------------------------------------

        [Test]
        public void WithConstructorIf_ConditionTrue_AddsConstructor()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructorIf(true, ctor => ctor
                    .WithParameter(CsType.Of("Vector3"), "position")
                    .WithBody(body => body.Assign("Position", "position")));

            var result = structBuilder.Emit();
            Assert.That(result, Does.Contain("MyStruct("));
            Assert.That(result, Does.Contain("Position = position;"));
        }

        [Test]
        public void WithConstructorIf_ConditionFalse_DoesNotAddConstructor()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructorIf(false, ctor => ctor
                    .WithParameter(CsType.Of("Vector3"), "position")
                    .WithBody(body => body.Assign("Position", "position")));

            var result = structBuilder.Emit();
            Assert.That(result, Does.Not.Contain("MyStruct("));
        }

        [Test]
        public void WithConstructorIf_ConditionFalse_DoesNotThrow()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructorIf(false, ctor => { });

            Assert.DoesNotThrow(() => structBuilder.Emit());
        }

        [Test]
        public void WithConstructorIf_ConditionTrue_EmptyConstructor_ThrowsInvalidOperationException()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructorIf(true, ctor => { });

            Assert.Throws<InvalidOperationException>(() => structBuilder.Emit());
        }

        [Test]
        public void WithConstructorIf_EmptyCollection_ConditionFalse_DoesNotAddConstructor()
        {
            var parameters = new (CsType Type, string Name)[0];

            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructorIf(
                    parameters.Length > 0,
                    ctor => ctor
                        .WithParameters(parameters, p => p.Type, p => p.Name));

            var result = structBuilder.Emit();
            Assert.That(result, Does.Not.Contain("MyStruct("));
        }

        [Test]
        public void WithConstructorIf_NonEmptyCollection_ConditionTrue_AddsConstructor()
        {
            var parameters = new[]
            {
                (Type: CsType.Of("Vector3"), Name: "position"),
                (Type: CsType.Float, Name: "scale"),
            };

            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructorIf(
                    parameters.Length > 0,
                    ctor => ctor
                        .WithParameters(parameters, p => p.Type, p => p.Name)
                        .WithBody(body =>
                        {
                            foreach (var p in parameters)
                                body.Assign(p.Name.ToUpper()[0] + p.Name.Substring(1), p.Name);
                        }));

            var result = structBuilder.Emit();
            Assert.That(result, Does.Contain("MyStruct(Vector3 position, float scale)"));
        }

        [Test]
        public void WithConstructorIf_ReturnsSameBuilderInstance_ForChaining()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct");

            var returned = structBuilder.WithConstructorIf(false, ctor => { });

            Assert.That(returned, Is.SameAs(structBuilder));
        }

        [Test]
        public void WithConstructorIf_ConditionTrue_ReturnsSameBuilderInstance_ForChaining()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct");

            var returned = structBuilder.WithConstructorIf(true, ctor => ctor
                .WithParameter(CsType.Float, "scale")
                .WithBody(body => body.Assign("Scale", "scale")));

            Assert.That(returned, Is.SameAs(structBuilder));
        }
        
        // -------------------------------------------------------------------------
        // Constructor validation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithEmptyConstructor_ThrowsInvalidOperationException()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructor();

            Assert.Throws<InvalidOperationException>(() => structBuilder.Emit());
        }

        [Test]
        public void Emit_WithConstructorWithParameters_DoesNotThrow()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructor(c => c
                    .WithParameter(CsType.Of("Vector3"), "position")
                    .WithBody(body => body.Assign("Position", "position")));

            Assert.DoesNotThrow(() => structBuilder.Emit());
        }

        [Test]
        public void Emit_WithNoConstructor_DoesNotThrow()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithField("Position", CsType.Of("Vector3"));

            Assert.DoesNotThrow(() => structBuilder.Emit());
        }

        [Test]
        public void Emit_WithEmptyConstructorFromOutOverload_ThrowsInvalidOperationException()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct");
            structBuilder.WithConstructorOut(out var ctor);
            // No parameters added to ctor

            Assert.Throws<InvalidOperationException>(() => structBuilder.Emit());
        }

        [Test]
        public void Emit_WithConstructorPopulatedFromEmptyCollection_DoesNotThrow()
        {
            var emptyCollection = new (CsType Type, string Name)[0];

            var structBuilder = StructBuilder.Build(_emitter, "MyStruct");

            if (emptyCollection.Length > 0)
            {
                structBuilder.WithConstructorOut(out var ctor);
                ctor.WithParameters(emptyCollection, p => p.Type, p => p.Name);
            }

            Assert.DoesNotThrow(() => structBuilder.Emit());
        }

        [Test]
        public void Emit_WithEmptyConstructor_ErrorMessageContainsStructName()
        {
            var structBuilder = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructor();

            var ex = Assert.Throws<InvalidOperationException>(() => structBuilder.Emit());
            Assert.That(ex.Message, Does.Contain("MyStruct"));
        }
        
        // -------------------------------------------------------------------------
        // Methods
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithMethod_ContainsMethod()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithMethod("Normalize", CsType.Of("Vector3"))
                .Emit();

            Assert.That(str, Does.Contain("Vector3 Normalize()"));
        }

        [Test]
        public void Emit_WithMultipleMethods_ContainsAllMethods()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithMethod("Normalize", CsType.Of("Vector3"))
                .WithMethod("Scale", CsType.Of("Vector3"))
                .Emit();

            Assert.That(str, Does.Contain("Vector3 Normalize()"));
            Assert.That(str, Does.Contain("Vector3 Scale()"));
        }

        // -------------------------------------------------------------------------
        // Member ordering
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_MemberOrdering_FieldsBeforeProperties()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithProperty("Scale", CsType.Float, p => p.WithAutoGetter())
                .WithField("_scale", CsType.Float)
                .Emit();

            var fieldIndex = str.IndexOf("float _scale;");
            var propertyIndex = str.IndexOf("float Scale");

            Assert.That(fieldIndex, Is.LessThan(propertyIndex));
        }

        [Test]
        public void Emit_MemberOrdering_PropertiesBeforeConstructors()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithConstructor(c => c
                    .WithParameter(CsType.Float, "scale")
                    .WithBody(body => body.Assign("Scale", "scale")))
                .WithProperty("Scale", CsType.Float, p => p.WithAutoGetter())
                .Emit();

            var propertyIndex = str.IndexOf("float Scale");
            var ctorIndex = str.IndexOf("public MyStruct(");

            Assert.That(propertyIndex, Is.LessThan(ctorIndex));
        }

        [Test]
        public void Emit_MemberOrdering_ConstructorsBeforeMethods()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithMethod("Normalize", CsType.Of("Vector3"))
                .WithConstructor(c => c
                    .WithParameter(CsType.Of("Vector3"), "position")
                    .WithBody(body => body.Assign("Position", "position")))
                .Emit();

            var ctorIndex = str.IndexOf("public MyStruct(");
            var methodIndex = str.IndexOf("Vector3 Normalize()");

            Assert.That(ctorIndex, Is.LessThan(methodIndex));
        }

        // -------------------------------------------------------------------------
        // Indentation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithOneIndentLevel_StartsWithOneTab()
        {
            _emitter.Push();

            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(str, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            var expectedIndent = _emitter.Get();

            StructBuilder.Build(_emitter, "MyStruct")
                .WithField("Position", CsType.Of("Vector3"))
                .WithProperty("Scale", CsType.Float, p => p.WithAutoGetter())
                .WithConstructor(c => c
                    .WithParameter(CsType.Of("Vector3"), "position")
                    .WithBody(body => body.Assign("Position", "position")))
                .WithMethod("Normalize", CsType.Of("Vector3"))
                .Emit();

            Assert.That(_emitter.Get(), Is.EqualTo(expectedIndent));
        }

        // -------------------------------------------------------------------------
        // ReadOnly struct with fields
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_ReadOnlyStructWithReadOnlyFields_EmitsCorrectly()
        {
            var str = StructBuilder.Build(_emitter, "SketchPoint")
                .WithReadOnlyModifier()
                .WithField("Position", CsType.Of("Vector3"), f => f
                    .WithVisibility(Visibility.Public)
                    .WithReadOnly())
                .WithField("Normal", CsType.Of("Vector3"), f => f
                    .WithVisibility(Visibility.Public)
                    .WithReadOnly())
                .Emit();

            Assert.That(Normalize(str), Does.StartWith("public readonly struct SketchPoint"));
            Assert.That(str, Does.Contain("public readonly Vector3 Position;"));
            Assert.That(str, Does.Contain("public readonly Vector3 Normal;"));
        }

        // -------------------------------------------------------------------------
        // XmlDoc
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithXmlDoc_DocAppearsBeforeDeclaration()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .WithXmlDoc(doc => doc.WithSummary("My struct."))
                .Emit();

            var summaryIndex = str.IndexOf("/// <summary>");
            var declarationIndex = str.IndexOf("public struct MyStruct");

            Assert.That(summaryIndex, Is.LessThan(declarationIndex));
        }

        [Test]
        public void Emit_WithoutXmlDoc_ContainsNoTripleSlash()
        {
            var str = StructBuilder.Build(_emitter, "MyStruct")
                .Emit();

            Assert.That(str, Does.Not.Contain("///"));
        }
    }
}