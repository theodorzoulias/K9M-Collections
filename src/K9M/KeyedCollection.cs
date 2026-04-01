using K9M.NullRef;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace K9M;

public partial class KeyedCollection<TKey, TItem> : ICollection<TItem>
    where TKey : notnull where TItem : notnull
{
    public partial KeyedCollection(Func<TItem, TKey> keySelector) : this(keySelector, comparer: null) { }

    public partial KeyedCollection(Func<TItem, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _keySelector = keySelector;
        _keyComparer = AltIdentityEqualityComparer<TKey>.GetSelfOrCachedOrNew(comparer);
        InitializeToZeroCapacity();
    }

    public partial KeyedCollection(Func<TItem, TKey> keySelector, int capacity) : this(keySelector)
        => EnsureCapacity(capacity);

    public partial KeyedCollection(Func<TItem, TKey> keySelector, int capacity, IEqualityComparer<TKey>? comparer) : this(keySelector, comparer)
        => EnsureCapacity(capacity);

    public partial KeyedCollection(Func<TItem, TKey> keySelector, IEnumerable<TItem> items) : this(keySelector)
        => AddRangeEnumerable(items, KeyExistsBehavior.Throw);

    public partial KeyedCollection(Func<TItem, TKey> keySelector, IEnumerable<TItem> items, IEqualityComparer<TKey>? comparer) : this(keySelector, comparer)
        => AddRangeEnumerable(items, KeyExistsBehavior.Throw);

    public partial int Count => _dataCount;

    public partial bool IsEmpty => Count == 0;

    public partial Func<TItem, TKey> KeySelector => _keySelector;

    public partial IEqualityComparer<TKey> KeyComparer
    {
        get
        {
            if (_keyComparer is AltIdentityEqualityComparer<TKey> alt)
                return alt.Parent;
            Debug.Assert(_keyComparer is IEqualityComparer<TKey>);
            return (IEqualityComparer<TKey>)_keyComparer;
        }
    }

    public partial int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _entries.Length;
    }

    public partial bool ContainsKey(TKey key) => ContainsKey(_keyComparer, key);

    public partial bool ContainsItem(TItem item) => ContainsItem(item, default);

    public partial bool ContainsItem(TItem item, IEqualityComparer<TItem>? comparer)
    {
        ref Entry entry = ref FindEntry(ExtractKey(item), out _, out _, out _, out _);
        if (entry.IsNull) return false;
        comparer ??= EqualityComparer<TItem>.Default;
        return comparer.Equals(item, entry.Item);
    }

    public partial ref TItem this[TKey key]
        => ref GetItem(_keyComparer, key);

    public partial bool TryGetItem(TKey key, [MaybeNullWhen(false)] out TItem item)
        => TryGetItem(_keyComparer, key, out item);

    public partial ref TItem GetItemRef(TKey key)
        => ref GetItemRef(_keyComparer, key, out _);

    public partial ref TItem GetItemRef(TKey key, out bool exists)
        => ref GetItemRef(_keyComparer, key, out exists);

    public partial ref TItem Add(TItem item)
    {
        TKey key = ExtractKey(item);
        if (!TryAddInternal(key, item, out int entryIndex))
            ThrowHelper.ArgumentException_TheKeyAlreadyExists(key);
        return ref _entries[entryIndex].Item;
    }

    public partial bool TryAdd(TItem item) => TryAddInternal(ExtractKey(item), item, out _);

    public partial ref TItem GetOrAdd(TItem item)
    {
        TryAddInternal(ExtractKey(item), item, out int entryIndex);
        return ref _entries[entryIndex].Item;
    }

    public partial ref TItem GetOrAdd(TItem item, out bool added)
    {
        added = TryAddInternal(ExtractKey(item), item, out int entryIndex);
        return ref _entries[entryIndex].Item;
    }

    public partial ref TItem GetOrAdd(TKey key, Func<TKey, TItem> itemFactory)
        => ref GetOrAdd(_keyComparer, key, itemFactory, out _);

    public partial ref TItem GetOrAdd(TKey key, Func<TKey, TItem> itemFactory, out bool added)
        => ref GetOrAdd(_keyComparer, key, itemFactory, out added);

    public partial ref TItem GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TItem> itemFactory, TArg factoryArgument)
        => ref GetOrAdd(_keyComparer, key, itemFactory, factoryArgument, out _);

    public partial ref TItem GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TItem> itemFactory, TArg factoryArgument, out bool added)
        => ref GetOrAdd(_keyComparer, key, itemFactory, factoryArgument, out added);

    public partial ref TItem AddOrReplace(TItem item)
        => ref AddOrReplace(item, out _, out _);

    public partial ref TItem AddOrReplace(TItem item, out bool replaced)
        => ref AddOrReplace(item, out replaced, out _);

    public partial ref TItem AddOrReplace(TItem item, out bool replaced, out TItem originalItem)
    {
        ref Entry entry = ref FindEntry(ExtractKey(item), out uint hashCode, out int bucketIndex, out int entryIndex, out _);
        if (entry.IsNotNull)
        {
            replaced = true; originalItem = entry.Item; entry.Item = item;
            return ref _entries[entryIndex].Item;
        }
        AddNewInternal(hashCode, item, bucketIndex, out entryIndex);
        replaced = false; originalItem = default!;
        return ref _entries[entryIndex].Item;
    }

    public partial ref TItem AddOrReplace(TKey key, Func<TKey, TItem> addItemFactory, Func<TKey, TItem, TItem> replaceItemFactory)
        => ref AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, out _, out _);

    public partial ref TItem AddOrReplace(TKey key, Func<TKey, TItem> addItemFactory, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced)
        => ref AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, out replaced, out _);

    public partial ref TItem AddOrReplace(TKey key, Func<TKey, TItem> addItemFactory, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced, [MaybeNull] out TItem originalItem)
        => ref AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, out replaced, out originalItem);

    public partial ref TItem AddOrReplace<TArg>(TKey key, Func<TKey, TArg, TItem> addItemFactory, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument)
        => ref AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, factoryArgument, out _, out _);

    public partial ref TItem AddOrReplace<TArg>(TKey key, Func<TKey, TArg, TItem> addItemFactory, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced)
        => ref AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, factoryArgument, out replaced, out _);

    public partial ref TItem AddOrReplace<TArg>(TKey key, Func<TKey, TArg, TItem> addItemFactory, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, [MaybeNull] out TItem originalItem)
        => ref AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, factoryArgument, out replaced, out originalItem);

    public partial bool TryReplace(TItem item)
    {
        ref Entry entry = ref FindEntry(ExtractKey(item), out _, out _, out _, out _);
        if (entry.IsNull) return false;
        entry.Item = item;
        return true;
    }

    public partial ref TItem TryReplace(TItem item, out bool replaced)
        => ref TryReplace(item, out replaced, out _);

    public partial ref TItem TryReplace(TItem item, out bool replaced, out TItem originalItem)
    {
        ref Entry entry = ref FindEntry(ExtractKey(item), out _, out _, out _, out _);
        if (entry.IsNull) { replaced = false; originalItem = default!; return ref Unsafe.NullRef<TItem>(); }
        originalItem = entry.Item;
        entry.Item = item;
        replaced = true;
        return ref entry.Item;
    }

    public partial bool TryReplace(TKey key, Func<TKey, TItem, TItem> replaceItemFactory)
        => !Unsafe.IsNullRef(ref TryReplace(_keyComparer, key, replaceItemFactory, out bool replaced, out _));

    public partial ref TItem TryReplace(TKey key, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced)
        => ref TryReplace(_keyComparer, key, replaceItemFactory, out replaced, out _);

    public partial ref TItem TryReplace(TKey key, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced, [MaybeNull] out TItem originalItem)
        => ref TryReplace(_keyComparer, key, replaceItemFactory, out replaced, out originalItem);

    public partial bool TryReplace<TArg>(TKey key, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument)
        => !Unsafe.IsNullRef(ref TryReplace(_keyComparer, key, replaceItemFactory, factoryArgument, out bool replaced, out _));

    public partial ref TItem TryReplace<TArg>(TKey key, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced)
        => ref TryReplace(_keyComparer, key, replaceItemFactory, factoryArgument, out replaced, out _);

    public partial ref TItem TryReplace<TArg>(TKey key, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, [MaybeNull] out TItem originalItem)
        => ref TryReplace(_keyComparer, key, replaceItemFactory, factoryArgument, out replaced, out originalItem);

    public partial bool TryRemove(TKey key)
        => TryRemove(_keyComparer, key, out _);

    public partial bool TryRemove(TKey key, [MaybeNullWhen(false)] out TItem removedItem)
        => TryRemove(_keyComparer, key, out removedItem);

    public partial void Clear()
    {
        if (_reservedLength == 0) return;
        Array.Clear(_entries, 0, _reservedLength);
        Array.Clear(_buckets);
        _reservedLength = 0;
        _dataCount = 0;
        _freeList = EndOfChain;
    }

    public partial int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity > Capacity)
            Grow(capacity, GrowStrategy.ExactNumber);
        return Capacity;
    }

    public partial void TrimExcess() => TrimExcess(Count);

    public partial void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        if (capacity >= Capacity) return;
        Resize(capacity);
    }

    private void CopyTo(TItem[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, array.Length);
        if (array.Length - index < Count)
            ThrowHelper.ArgumentException_DestinationArrayNotLongEnough();
        for (int i = 0; i < _reservedLength; i++)
        {
            ref Entry entry = ref _entries[i];
            if (entry.Next <= 0) continue;
            array[index++] = entry.Item;
        }
    }

    public partial TItem[] ToArray()
    {
        if (Count == 0) return Array.Empty<TItem>();
        TItem[] array = new TItem[Count];
        CopyTo(array, 0);
        return array;
    }

    public partial IReadOnlyDictionary<TKey, TItem> AsReadOnlyDictionary() => new ReadOnlyKeyedCollection<TKey, TItem>(this);

    bool ICollection<TItem>.IsReadOnly => false;

    bool ICollection<TItem>.Contains(TItem item) => ContainsItem(item);
    void ICollection<TItem>.CopyTo(TItem[] array, int arrayIndex) => CopyTo(array, arrayIndex);
    void ICollection<TItem>.Add(TItem item) => Add(item);
    bool ICollection<TItem>.Remove(TItem item) => TryRemove(ExtractKey(item));

    public partial Enumerator GetEnumerator() => new(this);
    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => EnumerateItems();
    IEnumerator IEnumerable.GetEnumerator() => EnumerateItems();

    public partial bool TryGetAlternateLookup<TAlternateKey>(out AlternateLookup<TAlternateKey> lookup) where TAlternateKey : notnull, allows ref struct
    {
        if (_keyComparer is IAlternateEqualityComparer<TAlternateKey, TKey> selfAlternate)
        {
            lookup = new AlternateLookup<TAlternateKey>(this, selfAlternate);
            return true;
        }
        if (_keyComparer is AltIdentityEqualityComparer<TKey> alt)
        {
            if (alt.TryGetAlternateEqualityComparer(out IAlternateEqualityComparer<TAlternateKey, TKey>? alternateKeyComparer))
            {
                lookup = new AlternateLookup<TAlternateKey>(this, alternateKeyComparer);
                return true;
            }
        }
        lookup = default;
        return false;
    }

    public partial AlternateLookup<TAlternateKey> GetAlternateLookup<TAlternateKey>() where TAlternateKey : notnull, allows ref struct
    {
        if (!TryGetAlternateLookup<TAlternateKey>(out var alternateLookup))
        {
            ThrowHelper.InvalidOperationException($"The collection's comparer is not compatible with {typeof(TAlternateKey)}.");
        }
        return alternateLookup;
    }
}
