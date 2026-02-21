using NUnit.Framework;
using DeeDeeR.CsEmitter;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class AttributeBuilderTests
    {
        private IndentEmitter _emitter;

        [SetUp]
        public void SetUp()
        {
            _emitter = new IndentEmitter();
        }

        // -------------------------------------------------------------------------
        // No arguments
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_NoArguments_EmitsBracketedName()
        {
            var attr = AttributeBuilder.Build("SerializeField")
                .Emit(_emitter);

            Assert.That(attr.Trim(), Is.EqualTo("[SerializeField]"));
        }

        [Test]
        public void Emit_NoArguments_NoParentheses()
        {
            var attr = AttributeBuilder.Build("SerializeField")
                .Emit(_emitter);

            Assert.That(attr, Does.Not.Contain("("));
            Assert.That(attr, Does.Not.Contain(")"));
        }

        // -------------------------------------------------------------------------
        // With arguments
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_SingleArgument_EmitsCorrectly()
        {
            var attr = AttributeBuilder.Build("Range")
                .WithArgument("0")
                .WithArgument("100")
                .Emit(_emitter);

            Assert.That(attr.Trim(), Is.EqualTo("[Range(0, 100)]"));
        }

        [Test]
        public void Emit_NamedArguments_EmitsCorrectly()
        {
            var attr = AttributeBuilder.Build("CreateAssetMenu")
                .WithArgument("menuName = \"DeeDeeR/MessageBroker/Provider\"")
                .WithArgument("fileName = \"MessageBrokerProvider\"")
                .Emit(_emitter);

            Assert.That(attr.Trim(),
                Is.EqualTo("[CreateAssetMenu(menuName = \"DeeDeeR/MessageBroker/Provider\", fileName = \"MessageBrokerProvider\")]"));
        }

        [Test]
        public void Emit_MultipleArguments_EmitsAllInOrder()
        {
            var attr = AttributeBuilder.Build("MyAttribute")
                .WithArgument("arg1")
                .WithArgument("arg2")
                .WithArgument("arg3")
                .Emit(_emitter);

            var argIndex1 = attr.IndexOf("arg1");
            var argIndex2 = attr.IndexOf("arg2");
            var argIndex3 = attr.IndexOf("arg3");

            Assert.That(argIndex1, Is.LessThan(argIndex2));
            Assert.That(argIndex2, Is.LessThan(argIndex3));
        }

        // -------------------------------------------------------------------------
        // Indentation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithOneIndentLevel_StartsWithOneTab()
        {
            _emitter.Push();

            var attr = AttributeBuilder.Build("SerializeField")
                .Emit(_emitter);

            Assert.That(attr, Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithTwoIndentLevels_StartsWithTwoTabs()
        {
            _emitter.Push();
            _emitter.Push();

            var attr = AttributeBuilder.Build("SerializeField")
                .Emit(_emitter);

            Assert.That(attr, Does.StartWith("\t\t"));
        }
    }
}