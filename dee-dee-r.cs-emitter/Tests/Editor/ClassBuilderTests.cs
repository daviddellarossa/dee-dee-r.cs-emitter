using System.Linq;
using NUnit.Framework;
using DeeDeeR.CsEmitter;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class ClassBuilderTests
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
        public void Emit_Defaults_EmitsPublicClass()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public class MyClass"));
        }

        [Test]
        public void Emit_Defaults_EmitsOpenAndCloseBraces()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(cls, Does.Contain("{"));
            Assert.That(cls, Does.Contain("}"));
        }

        [Test]
        public void Emit_Defaults_NoStaticModifier()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(cls, Does.Not.Contain("static"));
        }

        [Test]
        public void Emit_Defaults_NoSealedModifier()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(cls, Does.Not.Contain("sealed"));
        }

        [Test]
        public void Emit_Defaults_NoAbstractModifier()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(cls, Does.Not.Contain("abstract"));
        }

        [Test]
        public void Emit_Defaults_NoPartialModifier()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(cls, Does.Not.Contain("partial"));
        }

        // -------------------------------------------------------------------------
        // Visibility
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_InternalVisibility_EmitsInternal()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithVisibility(Visibility.Internal)
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("internal class MyClass"));
        }

        [Test]
        public void Emit_PrivateVisibility_EmitsPrivate()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithVisibility(Visibility.Private)
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("private class MyClass"));
        }

        // -------------------------------------------------------------------------
        // Modifiers
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithSealedModifier_EmitsSealed()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithSealedModifier()
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public sealed class MyClass"));
        }

        [Test]
        public void Emit_WithStaticModifier_EmitsStatic()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithStaticModifier()
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public static class MyClass"));
        }

        [Test]
        public void Emit_WithAbstractModifier_EmitsAbstract()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithAbstractModifier()
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public abstract class MyClass"));
        }

        [Test]
        public void Emit_WithPartialModifier_EmitsPartial()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithPartialModifier()
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public partial class MyClass"));
        }

        [Test]
        public void Emit_ModifierOrder_SealedAfterPublic()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithSealedModifier()
                .Emit();

            var declaration = Normalize(cls).Split('\n').First();
            var publicIndex = declaration.IndexOf("public");
            var sealedIndex = declaration.IndexOf("sealed");

            Assert.That(publicIndex, Is.LessThan(sealedIndex));
        }

        // -------------------------------------------------------------------------
        // Generics
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_SingleTypeParameter_EmitsCorrectly()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithTypeParameter("T")
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public class MyClass<T>"));
        }

        [Test]
        public void Emit_MultipleTypeParameters_EmitsAllInOrder()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithTypeParameter("TKey")
                .WithTypeParameter("TValue")
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public class MyClass<TKey, TValue>"));
        }

        [Test]
        public void Emit_TypeParameterWithConstraint_EmitsWhereClause()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithTypeParameter("T")
                .WithTypeConstraint("T", "new()")
                .Emit();

            Assert.That(cls, Does.Contain("where T : new()"));
        }

        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithField_ContainsField()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithField("_count", CsType.Int)
                .Emit();

            Assert.That(cls, Does.Contain("int _count;"));
        }

        [Test]
        public void Emit_WithMultipleFields_ContainsAllFields()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithField("_count", CsType.Int)
                .WithField("_name", CsType.String)
                .Emit();

            Assert.That(cls, Does.Contain("int _count;"));
            Assert.That(cls, Does.Contain("string _name;"));
        }

        [Test]
        public void Emit_WithField_FieldIsIndented()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithField("_count", CsType.Int)
                .Emit();

            var fieldLine = cls.Split('\n').First(l => l.Contains("int _count;"));
            Assert.That(IndentLevel(fieldLine), Is.GreaterThan(0));
        }

        [Test]
        public void Emit_WithFieldsFromCollection_ContainsAllFields()
        {
            var fields = new[]
            {
                (Name: "_count", Type: CsType.Int),
                (Name: "_name", Type: CsType.String),
                (Name: "_flag", Type: CsType.Bool),
            };

            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithFields(fields, f => f.Name, f => f.Type)
                .Emit();

            Assert.That(cls, Does.Contain("int _count;"));
            Assert.That(cls, Does.Contain("string _name;"));
            Assert.That(cls, Does.Contain("bool _flag;"));
        }

        // -------------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithProperty_ContainsProperty()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithProperty("Count", CsType.Int, p => p.WithAutoGetter())
                .Emit();

            Assert.That(cls, Does.Contain("int Count"));
        }

        [Test]
        public void Emit_WithMultipleProperties_ContainsAllProperties()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithProperty("Count", CsType.Int, p => p.WithAutoGetter())
                .WithProperty("Name", CsType.String, p => p
                    .WithAutoGetter()
                    .WithAutoSetter(Visibility.Private))
                .Emit();

            Assert.That(cls, Does.Contain("int Count"));
            Assert.That(cls, Does.Contain("string Name"));
        }

        [Test]
        public void Emit_WithPropertiesFromCollection_ContainsAllProperties()
        {
            var properties = new[]
            {
                (Name: "Count", Type: CsType.Int),
                (Name: "Name", Type: CsType.String),
            };

            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithProperties(properties, p => p.Name, p => p.Type,
                    (item, p) => p.WithAutoGetter())
                .Emit();

            Assert.That(cls, Does.Contain("int Count"));
            Assert.That(cls, Does.Contain("string Name"));
        }

        // -------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithConstructor_ContainsConstructor()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithConstructor(c => c
                    .WithBody(body => body.Assign("_count", "0")))
                .Emit();

            Assert.That(cls, Does.Contain("MyClass()"));
            Assert.That(cls, Does.Contain("_count = 0;"));
        }

        [Test]
        public void Emit_WithMultipleConstructors_ContainsBoth()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithConstructor()
                .WithConstructor(c => c.WithParameter(CsType.Int, "count"))
                .Emit();

            var ctorCount = cls.Split('\n')
                .Count(l => l.TrimStart().StartsWith("public MyClass("));

            Assert.That(ctorCount, Is.EqualTo(2));
        }

        // -------------------------------------------------------------------------
        // Methods
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithMethod_ContainsMethod()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithMethod("DoSomething", CsType.Void)
                .Emit();

            Assert.That(cls, Does.Contain("void DoSomething()"));
        }

        [Test]
        public void Emit_WithMultipleMethods_ContainsAllMethods()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithMethod("MethodA", CsType.Void)
                .WithMethod("MethodB", CsType.Int)
                .Emit();

            Assert.That(cls, Does.Contain("void MethodA()"));
            Assert.That(cls, Does.Contain("int MethodB()"));
        }

        [Test]
        public void Emit_WithMethod_MethodIsIndented()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithMethod("DoSomething", CsType.Void)
                .Emit();

            var methodLine = cls.Split('\n').First(l => l.Contains("void DoSomething()"));
            Assert.That(IndentLevel(methodLine), Is.GreaterThan(0));
        }

        // -------------------------------------------------------------------------
        // Member ordering
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_MemberOrdering_FieldsBeforeProperties()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithProperty("Count", CsType.Int, p => p.WithAutoGetter())
                .WithField("_count", CsType.Int)
                .Emit();

            var fieldIndex = cls.IndexOf("int _count;");
            var propertyIndex = cls.IndexOf("int Count");

            Assert.That(fieldIndex, Is.LessThan(propertyIndex));
        }

        [Test]
        public void Emit_MemberOrdering_PropertiesBeforeConstructors()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithConstructor()
                .WithProperty("Count", CsType.Int, p => p.WithAutoGetter())
                .Emit();

            var propertyIndex = cls.IndexOf("int Count");
            var ctorIndex = cls.IndexOf("public MyClass()");

            Assert.That(propertyIndex, Is.LessThan(ctorIndex));
        }

        [Test]
        public void Emit_MemberOrdering_ConstructorsBeforeMethods()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithMethod("DoSomething", CsType.Void)
                .WithConstructor()
                .Emit();

            var ctorIndex = cls.IndexOf("public MyClass()");
            var methodIndex = cls.IndexOf("void DoSomething()");

            Assert.That(ctorIndex, Is.LessThan(methodIndex));
        }

        // -------------------------------------------------------------------------
        // Indentation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithOneIndentLevel_StartsWithOneTab()
        {
            _emitter.Push();

            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(cls, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            var expectedIndent = _emitter.Get();

            ClassBuilder.Build(_emitter, "MyClass")
                .WithField("_count", CsType.Int)
                .WithProperty("Count", CsType.Int, p => p.WithAutoGetter())
                .WithConstructor()
                .WithMethod("DoSomething", CsType.Void)
                .Emit();

            Assert.That(_emitter.Get(), Is.EqualTo(expectedIndent));
        }

        // -------------------------------------------------------------------------
        // XmlDoc
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithXmlDoc_DocAppearsBeforeDeclaration()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .WithXmlDoc(doc => doc.WithSummary("My class."))
                .Emit();

            var summaryIndex = cls.IndexOf("/// <summary>");
            var declarationIndex = cls.IndexOf("public class MyClass");

            Assert.That(summaryIndex, Is.LessThan(declarationIndex));
        }

        [Test]
        public void Emit_WithoutXmlDoc_ContainsNoTripleSlash()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            Assert.That(cls, Does.Not.Contain("///"));
        }

        // -------------------------------------------------------------------------
        // Out overloads
        // -------------------------------------------------------------------------

        [Test]
        public void WithField_OutOverload_ReturnsSameBuilder()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass");
            cls.WithField("_count", CsType.Int, out var fieldBuilder);

            Assert.That(fieldBuilder, Is.Not.Null);
            Assert.That(cls.Emit(), Does.Contain("int _count;"));
        }

        [Test]
        public void WithProperty_OutOverload_ReturnsSameBuilder()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass");
            cls.WithProperty("Count", CsType.Int, out var propertyBuilder);
            propertyBuilder.WithAutoGetter();

            Assert.That(propertyBuilder, Is.Not.Null);
            Assert.That(cls.Emit(), Does.Contain("int Count"));
        }

        [Test]
        public void WithMethod_OutOverload_ReturnsSameBuilder()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass");
            cls.WithMethod("DoSomething", CsType.Void, out var methodBuilder);
            methodBuilder.WithBody(body => body.Call("Execute"));

            Assert.That(methodBuilder, Is.Not.Null);
            Assert.That(cls.Emit(), Does.Contain("void DoSomething()"));
            Assert.That(cls.Emit(), Does.Contain("Execute();"));
        }

        [Test]
        public void WithConstructor_OutOverload_ReturnsSameBuilder()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass");
            cls.WithConstructor(out var ctorBuilder);
            ctorBuilder.WithParameter(CsType.Int, "count");

            Assert.That(ctorBuilder, Is.Not.Null);
            Assert.That(cls.Emit(), Does.Contain("MyClass(int count)"));
        }

        [Test]
        public void WithField_OutOverload_FieldsFromLoop_ContainsAllFields()
        {
            var fields = new[]
            {
                (Name: "_count", Type: CsType.Int),
                (Name: "_name", Type: CsType.String),
                (Name: "_flag", Type: CsType.Bool),
            };

            var cls = ClassBuilder.Build(_emitter, "MyClass");

            foreach (var field in fields)
                cls.WithField(field.Name, field.Type, out _);

            Assert.That(cls.Emit(), Does.Contain("int _count;"));
            Assert.That(cls.Emit(), Does.Contain("string _name;"));
            Assert.That(cls.Emit(), Does.Contain("bool _flag;"));
        }
        
        // -------------------------------------------------------------------------
        // Inheritance
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithBaseClass_EmitsBaseClass()
        {
            var cls = ClassBuilder.Build(_emitter, "MyHandler")
                .WithBaseClass("BaseHandler")
                .Emit();

            Assert.That(cls, Does.Contain(": BaseHandler"));
        }

        [Test]
        public void Emit_WithBaseClass_BaseClassAppearsAfterClassName()
        {
            var cls = ClassBuilder.Build(_emitter, "MyHandler")
                .WithBaseClass("BaseHandler")
                .Emit();

            var classNameIndex = cls.IndexOf("MyHandler");
            var baseClassIndex = cls.IndexOf("BaseHandler");

            Assert.That(classNameIndex, Is.LessThan(baseClassIndex));
        }

        [Test]
        public void Emit_WithBaseClass_EmitsCorrectDeclaration()
        {
            var cls = ClassBuilder.Build(_emitter, "MyHandler")
                .WithBaseClass("BaseHandler")
                .Emit();

            Assert.That(Normalize(cls), Does.StartWith("public class MyHandler : BaseHandler"));
        }

        [Test]
        public void Emit_WithGenericBaseClass_EmitsCorrectly()
        {
            var cls = ClassBuilder.Build(_emitter, "MyHandler")
                .WithBaseClass(CsType.Generic("BaseHandler", CsType.Of("MyMessage")))
                .Emit();

            Assert.That(Normalize(cls),
                Does.StartWith("public class MyHandler : BaseHandler<MyMessage>"));
        }

        [Test]
        public void Emit_WithGenericBaseClassAndTypeParameter_EmitsCorrectly()
        {
            var cls = ClassBuilder.Build(_emitter, "MyHandler")
                .WithTypeParameter("T")
                .WithBaseClass(CsType.Generic("BaseHandler", CsType.Of("T")))
                .WithTypeConstraint("T", "IMessage")
                .Emit();

            Assert.That(Normalize(cls),
                Does.StartWith("public class MyHandler<T> : BaseHandler<T>"));
            Assert.That(cls, Does.Contain("where T : IMessage"));
        }

        [Test]
        public void Emit_WithBaseClassAndTypeConstraint_ConstraintAppearsAfterBaseClass()
        {
            var cls = ClassBuilder.Build(_emitter, "MyHandler")
                .WithTypeParameter("T")
                .WithBaseClass(CsType.Generic("BaseHandler", CsType.Of("T")))
                .WithTypeConstraint("T", "IMessage")
                .Emit();

            var baseClassIndex = cls.IndexOf("BaseHandler<T>");
            var constraintIndex = cls.IndexOf("where T : IMessage");

            Assert.That(baseClassIndex, Is.LessThan(constraintIndex));
        }

        [Test]
        public void Emit_WithSealedModifierAndBaseClass_EmitsCorrectly()
        {
            var cls = ClassBuilder.Build(_emitter, "MyHandler")
                .WithSealedModifier()
                .WithBaseClass("BaseHandler")
                .Emit();

            Assert.That(Normalize(cls),
                Does.StartWith("public sealed class MyHandler : BaseHandler"));
        }

        [Test]
        public void Emit_WithoutBaseClass_NoColonInDeclaration()
        {
            var cls = ClassBuilder.Build(_emitter, "MyClass")
                .Emit();

            var declarationLine = cls.Split('\n').First(l => l.Contains("class MyClass"));
            Assert.That(declarationLine, Does.Not.Contain(":"));
        }
    }
}