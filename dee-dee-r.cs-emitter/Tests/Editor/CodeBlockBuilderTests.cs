using System.Linq;
using NUnit.Framework;
using DeeDeeR.CsEmitter;

namespace DeeDeeR.CsEmitter.Tests.Editor
{
    [TestFixture]
    public sealed class CodeBlockBuilderTests
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
        // Empty block
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_EmptyBlock_EmitsEmptyString()
        {
            var block = new CodeBlockBuilder(_emitter);
            Assert.That(block.Emit(), Is.EqualTo(string.Empty));
        }

        // -------------------------------------------------------------------------
        // Local declaration
        // -------------------------------------------------------------------------

        [Test]
        public void DeclareLocal_WithTypeAndValue_EmitsTypedDeclaration()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.DeclareLocal(CsType.Int, "count", "0");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("int count = 0;"));
        }

        [Test]
        public void DeclareLocal_WithVarAndValue_EmitsVarDeclaration()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.DeclareLocal("instance", "new MyClass()");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("var instance = new MyClass();"));
        }

        [Test]
        public void DeclareLocal_WithTypeNoValue_EmitsDeclarationWithoutAssignment()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.DeclareLocal(CsType.Int, "count");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("int count;"));
        }

        [Test]
        public void DeclareLocal_WithVarNoValue_EmitsVarDeclarationWithoutAssignment()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.DeclareLocal("count");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("var count;"));
        }

        // -------------------------------------------------------------------------
        // Assignment
        // -------------------------------------------------------------------------

        [Test]
        public void Assign_EmitsAssignment()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Assign("_myField", "value");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("_myField = value;"));
        }

        [Test]
        public void Assign_ComplexExpression_EmitsCorrectly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Assign("_instance", "new MyClass(42, \"hello\")");
            Assert.That(Normalize(block.Emit()),
                Is.EqualTo("_instance = new MyClass(42, \"hello\");"));
        }

        // -------------------------------------------------------------------------
        // Compound assignment
        // -------------------------------------------------------------------------

        [Test]
        public void CompoundAssign_PlusEquals_EmitsCorrectly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CompoundAssign("_count", "+", "1");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("_count += 1;"));
        }

        [Test]
        public void CompoundAssign_MinusEquals_EmitsCorrectly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CompoundAssign("_count", "-", "1");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("_count -= 1;"));
        }

        [Test]
        public void CompoundAssign_MultiplyEquals_EmitsCorrectly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CompoundAssign("_value", "*", "2");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("_value *= 2;"));
        }

        // -------------------------------------------------------------------------
        // Method call
        // -------------------------------------------------------------------------

        [Test]
        public void Call_NoTarget_EmitsMethodCall()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Call("DoSomething");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("DoSomething();"));
        }

        [Test]
        public void Call_NoTargetWithArgs_EmitsMethodCallWithArgs()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Call("DoSomething", "arg1", "arg2");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("DoSomething(arg1, arg2);"));
        }

        [Test]
        public void Call_WithTarget_EmitsTargetedMethodCall()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CallOn("_instance", "DoSomething");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("_instance.DoSomething();"));
        }

        [Test]
        public void Call_WithTargetAndArgs_EmitsTargetedMethodCallWithArgs()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CallOn("_instance", "DoSomething", "arg1", "arg2");
            Assert.That(Normalize(block.Emit()),
                Is.EqualTo("_instance.DoSomething(arg1, arg2);"));
        }

        // -------------------------------------------------------------------------
        // Call and assign
        // -------------------------------------------------------------------------

        [Test]
        public void CallAndAssign_NoTarget_EmitsVarAssignment()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CallAndAssign("result", "GetValue");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("var result = GetValue();"));
        }

        [Test]
        public void CallAndAssign_WithTarget_EmitsTargetedVarAssignment()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CallAndAssignOn("result", "_instance", "GetValue");
            Assert.That(Normalize(block.Emit()),
                Is.EqualTo("var result = _instance.GetValue();"));
        }

        [Test]
        public void CallAndAssign_WithType_EmitsTypedAssignment()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CallAndAssign(CsType.Int, "result", "GetCount");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("int result = GetCount();"));
        }

        [Test]
        public void CallAndAssign_WithArgs_EmitsArgsCorrectly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.CallAndAssign("result", "GetValue", "arg1", "arg2");
            Assert.That(Normalize(block.Emit()),
                Is.EqualTo("var result = GetValue(arg1, arg2);"));
        }

        // -------------------------------------------------------------------------
        // Return
        // -------------------------------------------------------------------------

        [Test]
        public void Return_WithValue_EmitsReturnWithValue()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Return("_myField");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("return _myField;"));
        }

        [Test]
        public void Return_WithoutValue_EmitsReturnOnly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Return();
            Assert.That(Normalize(block.Emit()), Is.EqualTo("return;"));
        }

        [Test]
        public void Return_WithExpression_EmitsCorrectly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Return("a + b");
            Assert.That(Normalize(block.Emit()), Is.EqualTo("return a + b;"));
        }

        // -------------------------------------------------------------------------
        // Raw
        // -------------------------------------------------------------------------

        [Test]
        public void Raw_EmitsLineVerbatim()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.Raw("throw new NotImplementedException();");
            Assert.That(Normalize(block.Emit()),
                Is.EqualTo("throw new NotImplementedException();"));
        }

        [Test]
        public void Raw_WithIndent_EmitsWithCorrectIndentation()
        {
            _emitter.Push();
            var block = new CodeBlockBuilder(_emitter);
            block.Raw("throw new NotImplementedException();");
            Assert.That(block.Emit(), Does.StartWith("\t"));
        }

        // -------------------------------------------------------------------------
        // Statement ordering
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_MultipleStatements_EmitsInOrder()
        {
            var block = new CodeBlockBuilder(_emitter);
            block
                .DeclareLocal("instance", "new MyClass()")
                .Assign("_instance", "instance")
                .Call("_instance.Setup")
                .Return("true");

            var output = block.Emit();
            var declareIndex = output.IndexOf("var instance");
            var assignIndex = output.IndexOf("_instance = instance;");
            var callIndex = output.IndexOf("_instance.Setup();");
            var returnIndex = output.IndexOf("return true;");

            Assert.That(declareIndex, Is.LessThan(assignIndex));
            Assert.That(assignIndex, Is.LessThan(callIndex));
            Assert.That(callIndex, Is.LessThan(returnIndex));
        }

        // -------------------------------------------------------------------------
        // If statement
        // -------------------------------------------------------------------------

        [Test]
        public void If_EmitsCondition()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.If("_isReady", then => then.Return("true"));
            Assert.That(block.Emit(), Does.Contain("if (_isReady)"));
        }

        [Test]
        public void If_EmitsThenBody()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.If("_isReady", then => then.Return("true"));
            Assert.That(block.Emit(), Does.Contain("return true;"));
        }

        [Test]
        public void If_WithElse_EmitsElseBlock()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.If("_isReady",
                then => then.Return("true"),
                else_ => else_.Return("false"));

            Assert.That(block.Emit(), Does.Contain("else"));
            Assert.That(block.Emit(), Does.Contain("return false;"));
        }

        [Test]
        public void If_WithoutElse_DoesNotEmitElse()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.If("_isReady", then => then.Return("true"));
            Assert.That(block.Emit(), Does.Not.Contain("else"));
        }

        [Test]
        public void If_ThenBodyIsIndented_MoreThanIfKeyword()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.If("_isReady", then => then.Return("true"));

            var output = block.Emit();
            var ifLine = output.Split('\n').First(l => l.Contains("if ("));
            var returnLine = output.Split('\n').First(l => l.Contains("return true;"));

            Assert.That(IndentLevel(returnLine), Is.GreaterThan(IndentLevel(ifLine)));
        }

        [Test]
        public void If_Nested_EmitsCorrectly()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.If("_isReady",
                then => then.If("_isValid",
                    inner => inner.Return("true")));

            Assert.That(block.Emit(), Does.Contain("if (_isReady)"));
            Assert.That(block.Emit(), Does.Contain("if (_isValid)"));
            Assert.That(block.Emit(), Does.Contain("return true;"));
        }

        [Test]
        public void If_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            var expectedIndent = _emitter.Get();
            var block = new CodeBlockBuilder(_emitter);
            block.If("_isReady", then => then.Return("true"));
            block.Emit();
            Assert.That(_emitter.Get(), Is.EqualTo(expectedIndent));
        }

        // -------------------------------------------------------------------------
        // ForEach statement
        // -------------------------------------------------------------------------

        [Test]
        public void ForEach_EmitsForEachKeyword()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.ForEach(CsType.Of("Vector3"), "point", "_points",
                body => body.Call("Process", "point"));
            Assert.That(block.Emit(), Does.Contain("foreach"));
        }

        [Test]
        public void ForEach_EmitsTypedIterator()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.ForEach(CsType.Of("Vector3"), "point", "_points",
                body => body.Call("Process", "point"));
            Assert.That(Normalize(block.Emit()),
                Does.StartWith("foreach (Vector3 point in _points)"));
        }

        [Test]
        public void ForEach_WithVarType_EmitsVarIterator()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.ForEach(null, "point", "_points",
                body => body.Call("Process", "point"));
            Assert.That(Normalize(block.Emit()),
                Does.StartWith("foreach (var point in _points)"));
        }

        [Test]
        public void ForEach_EmitsBody()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.ForEach(CsType.Of("Vector3"), "point", "_points",
                body => body.Call("Process", "point"));
            Assert.That(block.Emit(), Does.Contain("Process(point);"));
        }

        [Test]
        public void ForEach_BodyIsIndented_MoreThanForEachKeyword()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.ForEach(CsType.Of("Vector3"), "point", "_points",
                body => body.Call("Process", "point"));

            var output = block.Emit();
            var forEachLine = output.Split('\n').First(l => l.Contains("foreach"));
            var bodyLine = output.Split('\n').First(l => l.Contains("Process(point);"));

            Assert.That(IndentLevel(bodyLine), Is.GreaterThan(IndentLevel(forEachLine)));
        }

        [Test]
        public void ForEach_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            var expectedIndent = _emitter.Get();
            var block = new CodeBlockBuilder(_emitter);
            block.ForEach(CsType.Of("Vector3"), "point", "_points",
                body => body.Call("Process", "point"));
            block.Emit();
            Assert.That(_emitter.Get(), Is.EqualTo(expectedIndent));
        }

        // -------------------------------------------------------------------------
        // For statement
        // -------------------------------------------------------------------------

        [Test]
        public void For_EmitsForKeyword()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.For("int i = 0", "i < 10", "i++",
                body => body.Call("Process", "i"));
            Assert.That(block.Emit(), Does.Contain("for"));
        }

        [Test]
        public void For_EmitsCorrectSignature()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.For("int i = 0", "i < 10", "i++",
                body => body.Call("Process", "i"));
            Assert.That(Normalize(block.Emit()),
                Does.StartWith("for (int i = 0; i < 10; i++)"));
        }

        [Test]
        public void For_EmitsBody()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.For("int i = 0", "i < 10", "i++",
                body => body.Call("Process", "i"));
            Assert.That(block.Emit(), Does.Contain("Process(i);"));
        }

        [Test]
        public void For_BodyIsIndented_MoreThanForKeyword()
        {
            var block = new CodeBlockBuilder(_emitter);
            block.For("int i = 0", "i < 10", "i++",
                body => body.Call("Process", "i"));

            var output = block.Emit();
            var forLine = output.Split('\n').First(l => l.Contains("for ("));
            var bodyLine = output.Split('\n').First(l => l.Contains("Process(i);"));

            Assert.That(IndentLevel(bodyLine), Is.GreaterThan(IndentLevel(forLine)));
        }

        [Test]
        public void For_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            var expectedIndent = _emitter.Get();
            var block = new CodeBlockBuilder(_emitter);
            block.For("int i = 0", "i < 10", "i++",
                body => body.Call("Process", "i"));
            block.Emit();
            Assert.That(_emitter.Get(), Is.EqualTo(expectedIndent));
        }

        // -------------------------------------------------------------------------
        // Indentation
        // -------------------------------------------------------------------------

        [Test]
        public void Emit_WithOneIndentLevel_StatementsStartWithOneTab()
        {
            _emitter.Push();
            var block = new CodeBlockBuilder(_emitter);
            block.Assign("_x", "1");
            Assert.That(block.Emit(), Does.StartWith("\t"));
        }

        [Test]
        public void Emit_WithTwoIndentLevels_StatementsStartWithTwoTabs()
        {
            _emitter.Push();
            _emitter.Push();
            var block = new CodeBlockBuilder(_emitter);
            block.Assign("_x", "1");
            Assert.That(block.Emit(), Does.StartWith("\t\t"));
        }

        [Test]
        public void Emit_IndentEmitterRestoredAfterEmit_IndentLevelUnchanged()
        {
            var expectedIndent = _emitter.Get();
            var block = new CodeBlockBuilder(_emitter);
            block
                .If("_isReady", then => then.Return("true"))
                .ForEach(CsType.Int, "i", "_items", body => body.Call("Process", "i"))
                .For("int i = 0", "i < 10", "i++", body => body.Call("Process", "i"));
            block.Emit();
            Assert.That(_emitter.Get(), Is.EqualTo(expectedIndent));
        }
    }
}