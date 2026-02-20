using NUnit.Framework;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class IndentEmitterTests
    {
        private IndentEmitter _emitter;

        [SetUp]
        public void SetUp()
        {
            _emitter = new IndentEmitter();
        }

        // -------------------------------------------------------------------------
        // Initial state
        // -------------------------------------------------------------------------

        [Test]
        public void Get_InitialState_ReturnsEmptyString()
        {
            Assert.That(_emitter.Get(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Line_InitialState_ReturnsContentOnly()
        {
            Assert.That(_emitter.Line("public int X;"), Is.EqualTo("public int X;"));
        }

        // -------------------------------------------------------------------------
        // Push
        // -------------------------------------------------------------------------

        [Test]
        public void Get_AfterOnePush_ReturnsOneTab()
        {
            _emitter.Push();
            Assert.That(_emitter.Get(), Is.EqualTo("\t"));
        }

        [Test]
        public void Get_AfterTwoPushes_ReturnsTwoTabs()
        {
            _emitter.Push();
            _emitter.Push();
            Assert.That(_emitter.Get(), Is.EqualTo("\t\t"));
        }

        [Test]
        public void Get_AfterThreePushes_ReturnsThreeTabs()
        {
            _emitter.Push();
            _emitter.Push();
            _emitter.Push();
            Assert.That(_emitter.Get(), Is.EqualTo("\t\t\t"));
        }

        [Test]
        public void Line_AfterOnePush_ReturnsTabbedContent()
        {
            _emitter.Push();
            Assert.That(_emitter.Line("public int X;"), Is.EqualTo("\tpublic int X;"));
        }

        [Test]
        public void Line_AfterTwoPushes_ReturnsTwoTabbedContent()
        {
            _emitter.Push();
            _emitter.Push();
            Assert.That(_emitter.Line("public int X;"), Is.EqualTo("\t\tpublic int X;"));
        }

        // -------------------------------------------------------------------------
        // Pop
        // -------------------------------------------------------------------------

        [Test]
        public void Get_AfterPushAndPop_ReturnsEmptyString()
        {
            _emitter.Push();
            _emitter.Pop();
            Assert.That(_emitter.Get(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Get_AfterTwoPushesAndOnePop_ReturnsOneTab()
        {
            _emitter.Push();
            _emitter.Push();
            _emitter.Pop();
            Assert.That(_emitter.Get(), Is.EqualTo("\t"));
        }

        [Test]
        public void Get_AfterTwoPushesAndTwoPops_ReturnsEmptyString()
        {
            _emitter.Push();
            _emitter.Push();
            _emitter.Pop();
            _emitter.Pop();
            Assert.That(_emitter.Get(), Is.EqualTo(string.Empty));
        }

        // -------------------------------------------------------------------------
        // Pop underflow guard
        // -------------------------------------------------------------------------

        [Test]
        public void Pop_BelowZero_DoesNotGoNegative()
        {
            _emitter.Pop();
            Assert.That(_emitter.Get(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Pop_MultipleBelowZero_DoesNotGoNegative()
        {
            _emitter.Pop();
            _emitter.Pop();
            _emitter.Pop();
            Assert.That(_emitter.Get(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Pop_AfterPushThenMultiplePopsBelow_DoesNotGoNegative()
        {
            _emitter.Push();
            _emitter.Pop();
            _emitter.Pop();
            _emitter.Pop();
            Assert.That(_emitter.Get(), Is.EqualTo(string.Empty));
        }

        // -------------------------------------------------------------------------
        // Reset
        // -------------------------------------------------------------------------

        [Test]
        public void Reset_AfterMultiplePushes_ReturnsEmptyString()
        {
            _emitter.Push();
            _emitter.Push();
            _emitter.Push();
            _emitter.Reset();
            Assert.That(_emitter.Get(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Reset_AfterPushes_AllowsPushAgain()
        {
            _emitter.Push();
            _emitter.Push();
            _emitter.Reset();
            _emitter.Push();
            Assert.That(_emitter.Get(), Is.EqualTo("\t"));
        }

        // -------------------------------------------------------------------------
        // Interleaved push/pop sequences
        // -------------------------------------------------------------------------

        [Test]
        public void Get_InterleavedPushPop_TracksCorrectly()
        {
            _emitter.Push(); // 1
            _emitter.Push(); // 2
            _emitter.Pop();  // 1
            _emitter.Push(); // 2
            _emitter.Push(); // 3
            _emitter.Pop();  // 2
            Assert.That(_emitter.Get(), Is.EqualTo("\t\t"));
        }

        [Test]
        public void Line_InterleavedPushPop_ReturnsCorrectIndent()
        {
            _emitter.Push(); // 1
            _emitter.Push(); // 2
            _emitter.Pop();  // 1
            Assert.That(_emitter.Line("void Foo()"), Is.EqualTo("\tvoid Foo()"));
        }
    }
}