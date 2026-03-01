using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace K9M.Internal;

/// <summary>
/// A comparer that implements the IAlternateEqualityComparer&lt;T1,T2&gt; interface, where the two
/// generic types are identical. It wraps a standard IEqualityComparer&lt;T&gt; instance, supplied in
/// the constructor of the collection. This comparer makes it possible to share implementations
/// between the main collection and the AlternateLookup.
/// </summary>
internal class AltIdentityEqualityComparer<TKey> : IAlternateEqualityComparer<TKey, TKey>
{
    public static AltIdentityEqualityComparer<TKey> Default = new(EqualityComparer<TKey>.Default);

    public static IAlternateEqualityComparer<TKey, TKey> GetSelfOrCachedOrNew(IEqualityComparer<TKey> comparer)
    {
        if (comparer is null || ReferenceEquals(comparer, EqualityComparer<TKey>.Default))
            return AltIdentityEqualityComparer<TKey>.Default;
        if (typeof(TKey) == typeof(string))
        {
            IEqualityComparer<string> stringComparer = Unsafe
                .As<IEqualityComparer<string>>(comparer);
            AltIdentityEqualityComparer<string> altIdentity = GetCachedAltIdentityStringComparer(stringComparer);
            if (altIdentity is not null)
                return Unsafe.As<AltIdentityEqualityComparer<TKey>>(altIdentity);
        }
        // Allow clients to supply comparers that already implement the alternate identity interface.
        // Native .NET comparers do not implement the alternate identity interface
        // (at least not in .NET 10), but implementing a custom one from scratch is quite easy.
        if (comparer is IAlternateEqualityComparer<TKey, TKey> self)
            return self;
        return new AltIdentityEqualityComparer<TKey>(comparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AltIdentityEqualityComparer<string> GetCachedAltIdentityStringComparer(
        IEqualityComparer<string> comparer)
    {
        Debug.Assert(comparer is not null);
        // Avoid instantiating the static System.StringComparer class, unless the comparer
        // implements the IAlternateEqualityComparer{ReadOnlySpan{char},string} interface.
        // Only then it's possible to be one of the predefined singletons.
        if (comparer is not IAlternateEqualityComparer<ReadOnlySpan<char>, string>)
            return null;
        if (ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase))
            return StringComparersCache.OrdinalIgnoreCase;
        if (ReferenceEquals(comparer, StringComparer.Ordinal))
            return StringComparersCache.Ordinal;
        if (ReferenceEquals(comparer, StringComparer.InvariantCultureIgnoreCase))
            return StringComparersCache.InvariantCultureIgnoreCase;
        if (ReferenceEquals(comparer, StringComparer.InvariantCulture))
            return StringComparersCache.InvariantCulture;
        return null;
    }

    private static class StringComparersCache
    {
        public static AltIdentityEqualityComparer<string> OrdinalIgnoreCase = new AltIdentityEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
        public static AltIdentityEqualityComparer<string> Ordinal = new AltIdentityEqualityComparer<string>(StringComparer.Ordinal);
        public static AltIdentityEqualityComparer<string> InvariantCultureIgnoreCase = new AltIdentityEqualityComparer<string>(StringComparer.InvariantCultureIgnoreCase);
        public static AltIdentityEqualityComparer<string> InvariantCulture = new AltIdentityEqualityComparer<string>(StringComparer.InvariantCulture);
    }

    private readonly IEqualityComparer<TKey> _parent;

    public AltIdentityEqualityComparer(IEqualityComparer<TKey> parent)
    {
        Debug.Assert(parent is not null);
        _parent = parent;
    }

    public IEqualityComparer<TKey> Parent => _parent;

    public bool Equals(TKey key, TKey other) => _parent.Equals(key, other);
    public int GetHashCode(TKey key) => _parent.GetHashCode(key);
    public TKey Create(TKey key) => key;

    public bool TryGetAlternateEqualityComparer<TAlternate>(
        out IAlternateEqualityComparer<TAlternate, TKey> result)
        where TAlternate : allows ref struct
    {
        if (_parent is IAlternateEqualityComparer<TAlternate, TKey> alternate)
        {
            result = alternate;
            return true;
        }
        result = default;
        return false;
    }
}
