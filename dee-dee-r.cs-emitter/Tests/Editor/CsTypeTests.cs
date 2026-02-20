using NUnit.Framework;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class CsTypeTests
    {
        // -------------------------------------------------------------------------
        // Non-generic types
        // -------------------------------------------------------------------------

        [Test]
        public void Of_SimpleName_EmitsName()
        {
            var type = CsType.Of("Vector3");
            Assert.That(type.Emit(), Is.EqualTo("Vector3"));
        }

        [Test]
        public void Of_PrimitiveAlias_EmitsAlias()
        {
            var type = CsType.Of("int");
            Assert.That(type.Emit(), Is.EqualTo("int"));
        }

        [Test]
        public void IsGeneric_NonGenericType_ReturnsFalse()
        {
            var type = CsType.Of("Vector3");
            Assert.That(type.IsGeneric, Is.False);
        }

        [Test]
        public void Name_ReturnsCorrectName()
        {
            var type = CsType.Of("Texture2D");
            Assert.That(type.Name, Is.EqualTo("Texture2D"));
        }

        [Test]
        public void TypeArguments_NonGenericType_ReturnsEmpty()
        {
            var type = CsType.Of("Vector3");
            Assert.That(type.TypeArguments, Is.Empty);
        }

        // -------------------------------------------------------------------------
        // Built-in shorthands
        // -------------------------------------------------------------------------

        [Test]
        public void Void_EmitsVoid()
        {
            Assert.That(CsType.Void.Emit(), Is.EqualTo("void"));
        }

        [Test]
        public void Int_EmitsInt()
        {
            Assert.That(CsType.Int.Emit(), Is.EqualTo("int"));
        }

        [Test]
        public void Float_EmitsFloat()
        {
            Assert.That(CsType.Float.Emit(), Is.EqualTo("float"));
        }

        [Test]
        public void Bool_EmitsBool()
        {
            Assert.That(CsType.Bool.Emit(), Is.EqualTo("bool"));
        }

        [Test]
        public void String_EmitsString()
        {
            Assert.That(CsType.String.Emit(), Is.EqualTo("string"));
        }

        // -------------------------------------------------------------------------
        // Generic types
        // -------------------------------------------------------------------------

        [Test]
        public void Generic_SingleTypeArgument_EmitsCorrectly()
        {
            var type = CsType.Generic("List", CsType.Of("Vector3"));
            Assert.That(type.Emit(), Is.EqualTo("List<Vector3>"));
        }

        [Test]
        public void Generic_TwoTypeArguments_EmitsCorrectly()
        {
            var type = CsType.Generic("Dictionary", CsType.String, CsType.Int);
            Assert.That(type.Emit(), Is.EqualTo("Dictionary<string, int>"));
        }

        [Test]
        public void IsGeneric_GenericType_ReturnsTrue()
        {
            var type = CsType.Generic("List", CsType.Int);
            Assert.That(type.IsGeneric, Is.True);
        }

        [Test]
        public void TypeArguments_GenericType_ReturnsArguments()
        {
            var type = CsType.Generic("Dictionary", CsType.String, CsType.Int);
            Assert.That(type.TypeArguments.Count, Is.EqualTo(2));
        }

        // -------------------------------------------------------------------------
        // Nested generics
        // -------------------------------------------------------------------------

        [Test]
        public void Generic_NestedGenericArgument_EmitsCorrectly()
        {
            var type = CsType.Generic("Dictionary",
                CsType.String,
                CsType.ListOf(CsType.Int));
            Assert.That(type.Emit(), Is.EqualTo("Dictionary<string, List<int>>"));
        }

        [Test]
        public void Generic_DeeplyNestedGenericArgument_EmitsCorrectly()
        {
            var type = CsType.Generic("Dictionary",
                CsType.String,
                CsType.Generic("Dictionary",
                    CsType.Int,
                    CsType.ListOf(CsType.Of("Vector3"))));
            Assert.That(type.Emit(), Is.EqualTo("Dictionary<string, Dictionary<int, List<Vector3>>>"));
        }

        // -------------------------------------------------------------------------
        // Convenience factories
        // -------------------------------------------------------------------------

        [Test]
        public void ListOf_EmitsCorrectly()
        {
            var type = CsType.ListOf(CsType.Of("Vector3"));
            Assert.That(type.Emit(), Is.EqualTo("List<Vector3>"));
        }

        [Test]
        public void DictionaryOf_EmitsCorrectly()
        {
            var type = CsType.DictionaryOf(CsType.String, CsType.Of("MyClass"));
            Assert.That(type.Emit(), Is.EqualTo("Dictionary<string, MyClass>"));
        }

        // -------------------------------------------------------------------------
        // ToString
        // -------------------------------------------------------------------------

        [Test]
        public void ToString_NonGeneric_SameAsEmit()
        {
            var type = CsType.Of("Vector3");
            Assert.That(type.ToString(), Is.EqualTo(type.Emit()));
        }

        [Test]
        public void ToString_Generic_SameAsEmit()
        {
            var type = CsType.DictionaryOf(CsType.String, CsType.Int);
            Assert.That(type.ToString(), Is.EqualTo(type.Emit()));
        }
    }
}