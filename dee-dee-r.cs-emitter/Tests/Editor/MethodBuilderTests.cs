using System;
using System.Linq;
using NUnit.Framework;
using DeeDeeR.CsEmitter;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class MethodBuilderTests
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
        public void Emit_Defaults_EmitsPublicMethod()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.Contain("public"));
        }

        [Test]
        public void Emit_Defaults_EmitsVoidReturnType()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.Contain("void"));
        }

        [Test]
        public void Emit_Defaults_EmitsMethodName()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.Contain("MyMethod"));
        }

        [Test]
        public void Emit_Defaults_EmitsEmptyBody()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(Normalize(method), Is.EqualTo("public void MyMethod()\n{" +
                                                       "\n}"));
        }

        [Test]
        public void Emit_Defaults_NoStaticModifier()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.Not.Contain("static"));
        }

        // -------------------------------------------------------------------------
        // Visibility
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_PrivateVisibility_EmitsPrivate()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithVisibility(Visibility.Private)
                .Emit();

            Assert.That(method, Does.Contain("private"));
        }

        [Test]
        public void Emit_ProtectedVisibility_EmitsProtected()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithVisibility(Visibility.Protected)
                .Emit();

            Assert.That(method, Does.Contain("protected"));
        }

        [Test]
        public void Emit_InternalVisibility_EmitsInternal()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithVisibility(Visibility.Internal)
                .Emit();

            Assert.That(method, Does.Contain("internal"));
        }

        // -------------------------------------------------------------------------
        // Modifiers
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithStaticModifier_EmitsStatic()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithStaticModifier()
                .Emit();

            Assert.That(method, Does.Contain("static"));
        }

        [Test]
        public void Emit_WithVirtualModifier_EmitsVirtual()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithVirtualModifier()
                .Emit();

            Assert.That(method, Does.Contain("virtual"));
        }

        [Test]
        public void Emit_WithOverrideModifier_EmitsOverride()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithOverrideModifier()
                .Emit();

            Assert.That(method, Does.Contain("override"));
        }

        [Test]
        public void Emit_WithAbstractModifier_EmitsAbstract()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAbstractModifier()
                .Emit();

            Assert.That(method, Does.Contain("abstract"));
        }

        [Test]
        public void Emit_WithPartialModifier_EmitsPartial()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithPartialModifier()
                .Emit();

            Assert.That(method, Does.Contain("partial"));
        }

        [Test]
        public void Emit_ModifierOrder_EmitsInCorrectOrder()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithVisibility(Visibility.Public)
                .WithStaticModifier()
                .Emit();

            Assert.That(Normalize(method), Does.StartWith("public static void MyMethod()"));
        }

        // -------------------------------------------------------------------------
        // Abstract and partial — no body
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_AbstractMethod_EmitsSemicolon()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAbstractModifier()
                .Emit();

            Assert.That(Normalize(method), Is.EqualTo("public abstract void MyMethod();"));
        }

        [Test]
        public void Emit_AbstractMethod_EmitsNoBody()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAbstractModifier()
                .Emit();

            Assert.That(method, Does.Not.Contain("{"));
        }

        [Test]
        public void Emit_PartialMethodWithoutBody_EmitsSemicolon()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithPartialModifier()
                .Emit();

            Assert.That(Normalize(method), Is.EqualTo("public partial void MyMethod();"));
        }

        [Test]
        public void Emit_PartialMethodWithBody_EmitsFullBlock()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithPartialModifier()
                .WithBody(body => body.Call("DoSomething"))
                .Emit();

            Assert.That(method, Does.Contain("{"));
            Assert.That(method, Does.Contain("DoSomething();"));
        }

        // -------------------------------------------------------------------------
        // Return types
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_IntReturnType_EmitsInt()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Int)
                .Emit();

            Assert.That(method, Does.Contain("int"));
        }

        [Test]
        public void Emit_GenericReturnType_EmitsCorrectly()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod",
                    CsType.ListOf(CsType.Of("Vector3")))
                .Emit();

            Assert.That(method, Does.Contain("List<Vector3>"));
        }

        [Test]
        public void Emit_CustomReturnType_EmitsCorrectly()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Of("MyClass"))
                .Emit();

            Assert.That(method, Does.Contain("MyClass"));
        }

        // -------------------------------------------------------------------------
        // Parameters
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_SingleParameter_EmitsCorrectly()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithParameter(CsType.Int, "count")
                .Emit();

            Assert.That(Normalize(method), Does.StartWith("public void MyMethod(int count)"));
        }

        [Test]
        public void Emit_MultipleParameters_EmitsAllInOrder()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithParameter(CsType.Int, "count")
                .WithParameter(CsType.String, "name")
                .WithParameter(CsType.Bool, "flag")
                .Emit();

            Assert.That(Normalize(method),
                Does.StartWith("public void MyMethod(int count, string name, bool flag)"));
        }

        [Test]
        public void Emit_GenericParameter_EmitsCorrectly()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithParameter(CsType.ListOf(CsType.Of("Vector3")), "points")
                .Emit();

            Assert.That(method, Does.Contain("List<Vector3> points"));
        }

        [Test]
        public void Emit_ParametersFromCollection_EmitsAllCorrectly()
        {
            var parameters = new[]
            {
                (Type: CsType.Int, Name: "count"),
                (Type: CsType.String, Name: "label"),
            };

            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithParameters(parameters, p => p.Type, p => p.Name)
                .Emit();

            Assert.That(Normalize(method),
                Does.StartWith("public void MyMethod(int count, string label)"));
        }

        // -------------------------------------------------------------------------
        // Generics
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_SingleTypeParameter_EmitsCorrectly()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Of("T"))
                .WithTypeParameter("T")
                .Emit();

            Assert.That(Normalize(method), Does.StartWith("public T MyMethod<T>()"));
        }

        [Test]
        public void Emit_MultipleTypeParameters_EmitsAllInOrder()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Of("TOut"))
                .WithTypeParameter("TIn")
                .WithTypeParameter("TOut")
                .Emit();

            Assert.That(Normalize(method), Does.StartWith("public TOut MyMethod<TIn, TOut>()"));
        }

        [Test]
        public void Emit_TypeParameterWithConstraint_EmitsWhereClause()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Of("T"))
                .WithTypeParameter("T")
                .WithTypeConstraint("T", "new()")
                .Emit();

            Assert.That(Normalize(method),
                Does.StartWith("public T MyMethod<T>() where T : new()"));
        }

        [Test]
        public void Emit_MultipleTypeConstraints_EmitsAllWhereClause()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Of("T"))
                .WithTypeParameter("T")
                .WithTypeConstraint("T", "class")
                .WithTypeConstraint("T", "new()")
                .Emit();

            Assert.That(method, Does.Contain("where T : class"));
            Assert.That(method, Does.Contain("where T : new()"));
        }

        // -------------------------------------------------------------------------
        // Body
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithBody_EmitsBodyContent()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithBody(body => body
                    .Assign("_myField", "42"))
                .Emit();

            Assert.That(method, Does.Contain("_myField = 42;"));
        }

        [Test]
        public void Emit_WithReturnStatement_EmitsReturn()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Int)
                .WithBody(body => body
                    .Return("42"))
                .Emit();

            Assert.That(method, Does.Contain("return 42;"));
        }

        [Test]
        public void Emit_WithMultipleStatements_EmitsAllInOrder()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithBody(body => body
                    .Assign("_a", "1")
                    .Assign("_b", "2")
                    .Call("DoSomething"))
                .Emit();

            var aIndex = method.IndexOf("_a = 1;");
            var bIndex = method.IndexOf("_b = 2;");
            var callIndex = method.IndexOf("DoSomething();");

            Assert.That(aIndex, Is.LessThan(bIndex));
            Assert.That(bIndex, Is.LessThan(callIndex));
        }

        [Test]
        public void Emit_BodyIsIndented_BodyContentIndentedMoreThanSignature()
        {
            _emitter.Push();

            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithBody(body => body
                    .Assign("_myField", "42"))
                .Emit();

            var signatureLine = method.Split('\n')
                .First(l => l.Contains("MyMethod"));
            var bodyLine = method.Split('\n')
                .First(l => l.Contains("_myField = 42;"));

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

            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithTwoIndentLevels_StartsWithTwoTabs()
        {
            _emitter.Push();
            _emitter.Push();

            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.StartWith("\t\t"));
        }

        [Test]
        public void Emit_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            _emitter.Push();
            var expectedIndent = _emitter.Get();

            MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
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
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithXmlDoc(doc => doc.WithSummary("Does something."))
                .Emit();

            var summaryIndex = method.IndexOf("/// <summary>");
            var signatureIndex = method.IndexOf("public void MyMethod()");

            Assert.That(summaryIndex, Is.LessThan(signatureIndex));
        }

        [Test]
        public void Emit_WithXmlDocAndParams_ContainsParamTags()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithParameter(CsType.Int, "count")
                .WithXmlDoc(doc => doc
                    .WithSummary("Does something.")
                    .WithParam("count", "The count."))
                .Emit();

            Assert.That(method, Does.Contain("<param name=\"count\">The count.</param>"));
        }

        [Test]
        public void Emit_WithoutXmlDoc_ContainsNoTripleSlash()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.Not.Contain("///"));
        }
        
        // -------------------------------------------------------------------------
        // Attributes
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithAttribute_EmitsAttributeBeforeSignature()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAttribute("Obsolete")
                .Emit();

            var attributeIndex = method.IndexOf("[Obsolete]");
            var signatureIndex = method.IndexOf("public void MyMethod()");

            Assert.That(attributeIndex, Is.LessThan(signatureIndex));
        }

        [Test]
        public void Emit_WithAttribute_ContainsAttribute()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAttribute("Obsolete")
                .Emit();

            Assert.That(method, Does.Contain("[Obsolete]"));
        }

        [Test]
        public void Emit_WithAttributeWithArguments_EmitsCorrectly()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAttribute("Obsolete", attr => attr
                    .WithArgument("\"Use NewMethod instead.\""))
                .Emit();

            Assert.That(method, Does.Contain("[Obsolete(\"Use NewMethod instead.\")]"));
        }

        [Test]
        public void Emit_WithMultipleAttributes_EmitsAllInOrder()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAttribute("Obsolete")
                .WithAttribute("ContextMenu", attr => attr
                    .WithArgument("\"Run MyMethod\""))
                .Emit();

            var obsoleteIndex = method.IndexOf("[Obsolete]");
            var contextMenuIndex = method.IndexOf("[ContextMenu(");

            Assert.That(method, Does.Contain("[Obsolete]"));
            Assert.That(method, Does.Contain("[ContextMenu(\"Run MyMethod\")]"));
            Assert.That(obsoleteIndex, Is.LessThan(contextMenuIndex));
        }

        [Test]
        public void Emit_WithAttributeAndXmlDoc_AttributeAppearsBeforeXmlDoc()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .WithAttribute("Obsolete")
                .WithXmlDoc(doc => doc.WithSummary("My method."))
                .Emit();

            var attributeIndex = method.IndexOf("[Obsolete]");
            var xmlDocIndex = method.IndexOf("///");

            Assert.That(attributeIndex, Is.LessThan(xmlDocIndex));
        }

        [Test]
        public void Emit_WithoutAttribute_NoSquareBrackets()
        {
            var method = MethodBuilder.Build(_emitter, "MyMethod", CsType.Void)
                .Emit();

            Assert.That(method, Does.Not.Contain("["));
            Assert.That(method, Does.Not.Contain("]"));
        }
    }
}