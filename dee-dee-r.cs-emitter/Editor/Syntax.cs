namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// Provides utility methods for syntax conversion in code generation.
    /// </summary>
    public static class Syntax
    {
        /// <summary>
        /// Converts a <see cref="Visibility"/> enum value to its corresponding C# keyword string.
        /// </summary>
        /// <param name="visibility">The visibility level to convert.</param>
        /// <returns>A string containing the C# visibility keyword(s).</returns>
        public static string VisibilityToString(Visibility visibility) => visibility switch
        {
            Visibility.Public => Constants.Public,
            Visibility.Private => Constants.Private,
            Visibility.Protected => Constants.Protected,
            Visibility.Internal => Constants.Internal,
            Visibility.ProtectedInternal => $"{Constants.Protected} {Constants.Internal}",
            _ => Constants.Private
        };
    }
}