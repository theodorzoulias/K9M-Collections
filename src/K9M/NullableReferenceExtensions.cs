using System.Runtime.CompilerServices;

namespace K9M.NullRef;

/// <summary>
/// Provides extension members for nullable struct references.
/// </summary>
/// <remarks>
/// These two properties are intended for nullable struct references, although they
/// appear on every struct, regardless of whether it's actually a reference.
/// For more details see this StackOverflow question:
/// <a href="https://stackoverflow.com/questions/79892295/how-to-simplify-checking-whether-a-reference-variable-is-null">How to simplify checking whether a reference variable is null?</a>
/// </remarks>
public static class NullableReferenceExtensions
{
    extension<T>(ref T self) where T : struct
    {
        /// <summary>
        /// Determines if the specified nullable struct reference is a null reference.
        /// </summary>
        /// <returns>
        /// true if the managed pointer is a null reference, otherwise false.
        /// </returns>
        /// <remarks>
        /// This property is intended for nullable struct references, although it
        /// appears on every struct, regardless of whether it's actually a reference.
        /// For more details see this StackOverflow question:
        /// <a href="https://stackoverflow.com/questions/79892295/how-to-simplify-checking-whether-a-reference-variable-is-null">How to simplify checking whether a reference variable is null?</a>
        /// </remarks>
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.IsNullRef(ref self);
        }

        /// <summary>
        /// Determines if the specified nullable struct reference is not a null reference.
        /// </summary>
        /// <returns>
        /// true if the managed pointer is not a null reference, otherwise false.
        /// </returns>
        /// <remarks>
        /// This property is intended for nullable struct references, although it
        /// appears on every struct, regardless of whether it's actually a reference.
        /// For more details see this StackOverflow question:
        /// <a href="https://stackoverflow.com/questions/79892295/how-to-simplify-checking-whether-a-reference-variable-is-null">How to simplify checking whether a reference variable is null?</a>
        /// </remarks>
        public bool IsNotNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !Unsafe.IsNullRef(ref self);
        }
    }
}
