namespace DeeDeeR.CsEmitter
{
    namespace DeeDeeR.CsEmitter
    {
        internal interface IClassMember
        {
            string Emit();
        }

        internal sealed class FieldMember : IClassMember
        {
            private readonly FieldBuilder _builder;
            public FieldMember(FieldBuilder builder) => _builder = builder;
            public string Emit() => _builder.Emit();
        }

        internal sealed class PropertyMember : IClassMember
        {
            private readonly PropertyBuilder _builder;
            public PropertyMember(PropertyBuilder builder) => _builder = builder;
            public string Emit() => _builder.Emit();
        }

        internal sealed class MethodMember : IClassMember
        {
            private readonly MethodBuilder _builder;
            public MethodMember(MethodBuilder builder) => _builder = builder;
            public string Emit() => _builder.Emit();
        }

        internal sealed class ConstructorMember : IClassMember
        {
            private readonly ConstructorBuilder _builder;
            public ConstructorMember(ConstructorBuilder builder) => _builder = builder;
            public bool HasParameters => _builder.HasParameters;
            public string Emit() => _builder.Emit();
        }

        internal sealed class RawMember : IClassMember
        {
            private readonly string _line;
            public RawMember(string line) => _line = line;
            public string Emit() => _line + "\n";
        }
    }
}