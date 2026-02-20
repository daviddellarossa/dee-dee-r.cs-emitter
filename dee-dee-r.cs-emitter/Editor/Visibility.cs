namespace DeeDeeR.CsEmitter
{
    /// <summary>
    /// Represents the visibility levels for C# type members.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Public visibility - accessible from any code.
        /// </summary>
        Public,

        /// <summary>
        /// Private visibility - accessible only within the containing type.
        /// </summary>
        Private,

        /// <summary>
        /// Protected visibility - accessible within the containing type and derived types.
        /// </summary>
        Protected,

        /// <summary>
        /// Internal visibility - accessible within the same assembly.
        /// </summary>
        Internal,

        /// <summary>
        /// Protected internal visibility - accessible within the same assembly or from derived types.
        /// </summary>
        ProtectedInternal
    }
}