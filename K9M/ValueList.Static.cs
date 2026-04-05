using System;
using System.Collections.Generic;

namespace K9M;

/// <summary>
/// Provides static methods for creating ValueList&lt;T&gt; instances.
/// </summary>
public static class ValueList
{
    /// <summary>
    /// Creates a new ValueList&lt;T&gt; instance that is backed by the specified array.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="array">The array to use as the backing storage of the collection.</param>
    /// <param name="count">The number of items in the collection.</param>
    /// <returns>A new collection that is backed by the specified array.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The count is negative, or the count is greater than the length of the array.</exception>
    /// <remarks>
    /// The ownership of the array is transfered to the collection. The array
    /// should no longer be manipulated directly by external code.
    /// </remarks>
    public static ValueList<T> FromArray<T>(T[]? array, int count)
    {
        return new ValueList<T>(array, count);
    }

    /// <summary>
    /// Creates a new ValueList&lt;T&gt; instance that is backed by the specified array.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="array">The array to use as the backing storage of the collection.</param>
    /// <returns>A new collection that is backed by the specified array.</returns>
    /// <remarks>
    /// The ownership of the array is transfered to the collection. The array
    /// should no longer be manipulated directly by external code.
    /// </remarks>
    public static ValueList<T> FromArray<T>(T[]? array)
    {
        return new ValueList<T>(array, array?.Length ?? 0);
    }
}
