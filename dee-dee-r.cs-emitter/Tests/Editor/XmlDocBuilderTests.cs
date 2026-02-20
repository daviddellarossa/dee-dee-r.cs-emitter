using NUnit.Framework;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class XmlDocBuilderTests
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

        private string Emit(XmlDocBuilder builder) => builder.Emit(_emitter);

        private static string Lines(params string[] lines) 
            => string.Join("\n", lines) + "\n";

        // -------------------------------------------------------------------------
        // InheritDoc
        // -------------------------------------------------------------------------

        [Test]
        public void InheritDoc_EmitsInheritDocTag()
        {
            var doc = XmlDocBuilder.Build()
                .WithInheritDoc();

            Assert.That(Emit(doc), Is.EqualTo("/// <inheritdoc/>\n"));
        }

        [Test]
        public void InheritDoc_WithIndent_EmitsCorrectlyIndented()
        {
            _emitter.Push();
            var doc = XmlDocBuilder.Build()
                .WithInheritDoc();

            Assert.That(Emit(doc), Is.EqualTo("\t/// <inheritdoc/>\n"));
        }

        [Test]
        public void InheritDoc_IgnoresOtherTags()
        {
            var doc = XmlDocBuilder.Build()
                .WithSummary("Some summary.")
                .WithInheritDoc();

            Assert.That(Emit(doc), Is.EqualTo("/// <inheritdoc/>\n"));
        }

        // -------------------------------------------------------------------------
        // Summary
        // -------------------------------------------------------------------------

        [Test]
        public void Summary_SingleLine_EmitsCorrectly()
        {
            var doc = XmlDocBuilder.Build()
                .WithSummary("Manages an active sketch session.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <summary>",
                "/// Manages an active sketch session.",
                "/// </summary>")));
        }

        [Test]
        public void Summary_MultiLine_EmitsEachLineWithPrefix()
        {
            var doc = XmlDocBuilder.Build()
                .WithSummary("Line one.\nLine two.\nLine three.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <summary>",
                "/// Line one.",
                "/// Line two.",
                "/// Line three.",
                "/// </summary>")));
        }

        [Test]
        public void Summary_WithIndent_EmitsCorrectlyIndented()
        {
            _emitter.Push();
            var doc = XmlDocBuilder.Build()
                .WithSummary("A summary.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "\t/// <summary>",
                "\t/// A summary.",
                "\t/// </summary>")));
        }

        // -------------------------------------------------------------------------
        // Remarks
        // -------------------------------------------------------------------------

        [Test]
        public void Remarks_SingleLine_EmitsCorrectly()
        {
            var doc = XmlDocBuilder.Build()
                .WithRemarks("This class is generated. Do not edit manually.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <remarks>",
                "/// This class is generated. Do not edit manually.",
                "/// </remarks>")));
        }

        [Test]
        public void Remarks_MultiLine_EmitsEachLineWithPrefix()
        {
            var doc = XmlDocBuilder.Build()
                .WithRemarks("Line one.\nLine two.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <remarks>",
                "/// Line one.",
                "/// Line two.",
                "/// </remarks>")));
        }

        // -------------------------------------------------------------------------
        // Param
        // -------------------------------------------------------------------------

        [Test]
        public void Param_SingleLine_EmitsInline()
        {
            var doc = XmlDocBuilder.Build()
                .WithParam("position", "The world-space position.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <param name=\"position\">The world-space position.</param>")));
        }

        [Test]
        public void Param_MultiLine_EmitsBlock()
        {
            var doc = XmlDocBuilder.Build()
                .WithParam("position", "Line one.\nLine two.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <param name=\"position\">",
                "/// Line one.",
                "/// Line two.",
                "/// </param>")));
        }

        [Test]
        public void Param_Multiple_EmitsAllInOrder()
        {
            var doc = XmlDocBuilder.Build()
                .WithParam("position", "The position.")
                .WithParam("normal", "The normal.")
                .WithParam("scale", "The scale.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <param name=\"position\">The position.</param>",
                "/// <param name=\"normal\">The normal.</param>",
                "/// <param name=\"scale\">The scale.</param>")));
        }

        // -------------------------------------------------------------------------
        // TypeParam
        // -------------------------------------------------------------------------

        [Test]
        public void TypeParam_SingleLine_EmitsInline()
        {
            var doc = XmlDocBuilder.Build()
                .WithTypeParam("T", "The type to cast to.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <typeparam name=\"T\">The type to cast to.</typeparam>")));
        }

        [Test]
        public void TypeParam_Multiple_EmitsAllInOrder()
        {
            var doc = XmlDocBuilder.Build()
                .WithTypeParam("TKey", "The key type.")
                .WithTypeParam("TValue", "The value type.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <typeparam name=\"TKey\">The key type.</typeparam>",
                "/// <typeparam name=\"TValue\">The value type.</typeparam>")));
        }

        // -------------------------------------------------------------------------
        // Returns
        // -------------------------------------------------------------------------

        [Test]
        public void Returns_SingleLine_EmitsInline()
        {
            var doc = XmlDocBuilder.Build()
                .WithReturns("A non-negative integer.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <returns>A non-negative integer.</returns>")));
        }

        [Test]
        public void Returns_MultiLine_EmitsBlock()
        {
            var doc = XmlDocBuilder.Build()
                .WithReturns("Line one.\nLine two.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <returns>",
                "/// Line one.",
                "/// Line two.",
                "/// </returns>")));
        }

        // -------------------------------------------------------------------------
        // Exception
        // -------------------------------------------------------------------------

        [Test]
        public void Exception_SingleLine_EmitsInline()
        {
            var doc = XmlDocBuilder.Build()
                .WithException("InvalidOperationException", "Thrown if the session is not active.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <exception cref=\"InvalidOperationException\">Thrown if the session is not active.</exception>")));
        }

        [Test]
        public void Exception_Multiple_EmitsAllInOrder()
        {
            var doc = XmlDocBuilder.Build()
                .WithException("InvalidOperationException", "Thrown if not active.")
                .WithException("ArgumentNullException", "Thrown if argument is null.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <exception cref=\"InvalidOperationException\">Thrown if not active.</exception>",
                "/// <exception cref=\"ArgumentNullException\">Thrown if argument is null.</exception>")));
        }

        // -------------------------------------------------------------------------
        // Tag ordering
        // -------------------------------------------------------------------------

        [Test]
        public void TagOrdering_AllTags_EmitsInCorrectOrder()
        {
            var doc = XmlDocBuilder.Build()
                .WithSummary("A summary.")
                .WithRemarks("A remark.")
                .WithTypeParam("T", "The type.")
                .WithParam("value", "The value.")
                .WithReturns("The result.")
                .WithException("ArgumentException", "Bad argument.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <summary>",
                "/// A summary.",
                "/// </summary>",
                "/// <remarks>",
                "/// A remark.",
                "/// </remarks>",
                "/// <typeparam name=\"T\">The type.</typeparam>",
                "/// <param name=\"value\">The value.</param>",
                "/// <returns>The result.</returns>",
                "/// <exception cref=\"ArgumentException\">Bad argument.</exception>")));
        }

        [Test]
        public void TagOrdering_SummaryAndRemarks_EmitsSummaryFirst()
        {
            var doc = XmlDocBuilder.Build()
                .WithRemarks("A remark.")
                .WithSummary("A summary.");

            // Remarks registered first but summary should always come first
            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "/// <summary>",
                "/// A summary.",
                "/// </summary>",
                "/// <remarks>",
                "/// A remark.",
                "/// </remarks>")));
        }

        // -------------------------------------------------------------------------
        // Indentation with multiple tags
        // -------------------------------------------------------------------------

        [Test]
        public void MultipleTagsWithIndent_EmitsAllCorrectlyIndented()
        {
            _emitter.Push();
            _emitter.Push();

            var doc = XmlDocBuilder.Build()
                .WithSummary("A summary.")
                .WithParam("value", "The value.");

            Assert.That(Emit(doc), Is.EqualTo(Lines(
                "\t\t/// <summary>",
                "\t\t/// A summary.",
                "\t\t/// </summary>",
                "\t\t/// <param name=\"value\">The value.</param>")));
        }
    }
}