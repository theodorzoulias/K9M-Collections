using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace K9M.Internal;

internal static class ThrowHelper
{
    [DoesNotReturn] public static void InvalidOperationException(string message)
    {
        throw new InvalidOperationException(message);
    }

    public static void InvalidOperationException_FactoryReturnedIncompatibleKey(string factoryDelegateName)
    {
        throw new InvalidOperationException($"The {factoryDelegateName} produced an item with different key than the original.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvalidOperationException_IfCollectionWasModified(ushort capturedVersion, ushort currentVersion)
    {
        if (capturedVersion != currentVersion)
            InvalidOperationException("The collection was modified.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvalidOperationException_IfCollectionWasModified(DualVersion capturedVersion, DualVersion currentVersion)
    {
        if (capturedVersion != currentVersion)
            InvalidOperationException("The collection was modified.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvalidOperationException_IfChainCorrupted(int iterations, int exclusiveLimit)
    {
        if (iterations >= exclusiveLimit)
            ThrowHelper.InvalidOperationException("The chain of entries forms a loop.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvalidOperationException_IfFactoryReturnedNullItem<TItem>(TItem item)
    {
        if (item is null) InvalidOperationException("The factory delegate returned a null item.");
    }

    public static void ArgumentException_CollectionContainsNullItem(string paramName)
    {
        throw new ArgumentException("The collection contains a null item.", paramName);
    }

    [DoesNotReturn] public static void ArgumentNullException(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ArgumentException_WhenKeyExists<TKey>(TKey key, bool tryAddResult, KeyExistsBehavior keyExistsBehavior)
    {
        if (!tryAddResult && keyExistsBehavior == KeyExistsBehavior.Throw)
            ArgumentException_TheKeyAlreadyExists(key);
    }

    [DoesNotReturn] public static void ArgumentException_TheKeyAlreadyExists<TKey>(TKey key)
    {
        throw new ArgumentException($"An item with the same key already exists in the collection. Key: {key}");
    }

    [DoesNotReturn] public static void ArgumentException_DestinationArrayNotLongEnough()
    {
        throw new ArgumentException($"Destination array is not long enough to copy all the items in the collection. Check array index and length.");
    }

    [DoesNotReturn] public static void KeyNotFoundException()
    {
        throw new KeyNotFoundException($"The given key was not present in the collection.");
    }

    [DoesNotReturn] public static void ObjectDisposedException_Enumerator(Type type)
    {
        throw new ObjectDisposedException(type?.Name);
    }
}
