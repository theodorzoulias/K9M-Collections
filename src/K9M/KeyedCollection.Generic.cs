using K9M.NullRef;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace K9M;

// Member implementations based on the generic IAlternateEqualityComparer.
public partial class KeyedCollection<TKey, TItem>
{
    private bool ContainsKey<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer, TTKey key) where TTKey : allows ref struct
    {
        return FindEntry(comparer, key, out _, out _, out _, out _).IsNotNull;
    }

    private bool TryGetItem<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, [MaybeNullWhen(false)] out TItem item) where TTKey : allows ref struct
    {
        ref Entry entry = ref FindEntry<TTKey>(comparer, key, out _, out _, out _, out _);
        if (entry.IsNull) { item = default; return false; }
        item = entry.Item;
        return true;
    }

    private ref TItem GetItemRef<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, out bool exists) where TTKey : allows ref struct
    {
        ref Entry entry = ref FindEntry<TTKey>(comparer, key, out _, out _, out int entryIndex, out _);
        if (entry.IsNull) { exists = false; return ref Unsafe.NullRef<TItem>(); }
        exists = true;
        return ref _entries[entryIndex].Item;
    }

    private ref TItem GetItem<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key) where TTKey : allows ref struct
    {
        ref Entry entry = ref FindEntry<TTKey>(comparer, key, out _, out _, out int entryIndex, out _);
        if (entry.IsNull) ThrowHelper.KeyNotFoundException();
        return ref _entries[entryIndex].Item;
    }

    private ref TItem GetOrAdd<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, Func<TTKey, TItem> itemFactory, out bool added) where TTKey : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(itemFactory);
        ref Entry entry = ref FindEntry<TTKey>(comparer, key, out uint hashCode, out int bucketIndex, out _, out _);
        if (entry.IsNotNull) { added = false; return ref entry.Item; }
        DualVersion capturedVersion = _version;
        TItem newItem = itemFactory(key);
        AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(itemFactory));
        AddNewInternal(hashCode, newItem, bucketIndex, out int entryIndex);
        added = true;
        return ref _entries[entryIndex].Item;
    }

    private ref TItem GetOrAdd<TTKey, TArg>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, Func<TTKey, TArg, TItem> itemFactory, TArg factoryArgument, out bool added) where TTKey : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(itemFactory);
        ref Entry entry = ref FindEntry<TTKey>(comparer, key, out uint hashCode, out int bucketIndex, out _, out _);
        if (entry.IsNotNull) { added = false; return ref entry.Item; }
        DualVersion capturedVersion = _version;
        TItem newItem = itemFactory(key, factoryArgument);
        AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(itemFactory));
        AddNewInternal(hashCode, newItem, bucketIndex, out int entryIndex);
        added = true;
        return ref _entries[entryIndex].Item;
    }

    private ref TItem AddOrReplace<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, Func<TTKey, TItem> addItemFactory,
        Func<TTKey, TItem, TItem> replaceItemFactory,
        out bool replaced, out TItem originalItem) where TTKey : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(addItemFactory);
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        ref Entry entry = ref FindEntry(comparer, key, out uint hashCode, out int bucketIndex, out int entryIndex, out _);
        DualVersion capturedVersion = _version;
        if (entry.IsNull)
        {
            originalItem = default;
            TItem newItem = addItemFactory(key);
            AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(addItemFactory));
            AddNewInternal(hashCode, newItem, bucketIndex, out entryIndex);
            replaced = false;
        }
        else
        {
            originalItem = entry.Item;
            TItem newItem = replaceItemFactory(key, entry.Item);
            AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(replaceItemFactory));
            entry.Item = newItem;
            replaced = true;
        }
        return ref _entries[entryIndex].Item;
    }

    private ref TItem AddOrReplace<TTKey, TArg>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, Func<TTKey, TArg, TItem> addItemFactory,
        Func<TTKey, TItem, TArg, TItem> replaceItemFactory,
        TArg factoryArgument, out bool replaced, out TItem originalItem) where TTKey : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(addItemFactory);
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        ref Entry entry = ref FindEntry(comparer, key, out uint hashCode, out int bucketIndex, out int entryIndex, out _);
        DualVersion capturedVersion = _version;
        if (entry.IsNull)
        {
            originalItem = default;
            TItem newItem = addItemFactory(key, factoryArgument);
            AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(addItemFactory));
            AddNewInternal(hashCode, newItem, bucketIndex, out entryIndex);
            replaced = false;
        }
        else
        {
            originalItem = entry.Item;
            TItem newItem = replaceItemFactory(key, entry.Item, factoryArgument);
            AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(replaceItemFactory));
            entry.Item = newItem;
            replaced = true;
        }
        return ref _entries[entryIndex].Item;
    }

    private ref TItem TryReplace<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, Func<TTKey, TItem, TItem> replaceItemFactory,
        out bool replaced, out TItem originalItem) where TTKey : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        ref Entry entry = ref FindEntry(comparer, key, out _, out int bucketIndex, out int entryIndex, out _);
        if (entry.IsNull)
        {
            replaced = false; originalItem = default; return ref Unsafe.NullRef<TItem>();
        }
        originalItem = entry.Item;
        DualVersion capturedVersion = _version;
        TItem newItem = replaceItemFactory(key, entry.Item);
        AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(replaceItemFactory));
        entry.Item = newItem;
        replaced = true;
        return ref _entries[entryIndex].Item;
    }

    private ref TItem TryReplace<TTKey, TArg>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, Func<TTKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument,
        out bool replaced, out TItem originalItem) where TTKey : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        ref Entry entry = ref FindEntry(comparer, key, out _, out int bucketIndex, out int entryIndex, out _);
        if (entry.IsNull)
        {
            replaced = false; originalItem = default; return ref Unsafe.NullRef<TItem>();
        }
        originalItem = entry.Item;
        DualVersion capturedVersion = _version;
        TItem newItem = replaceItemFactory(key, entry.Item, factoryArgument);
        AssertFactoryInvocationRules(comparer, capturedVersion, newItem, key, nameof(replaceItemFactory));
        entry.Item = newItem;
        replaced = true;
        return ref _entries[entryIndex].Item;
    }

    private bool TryRemove<TTKey>(IAlternateEqualityComparer<TTKey, TKey> comparer,
        TTKey key, [MaybeNullWhen(false)] out TItem removedItem) where TTKey : allows ref struct
    {
        ref Entry entry = ref FindEntry(comparer, key, out _, out int bucketIndex, out int entryIndex, out int previousEntryIndex);
        if (entry.IsNull) { removedItem = default; return false; }
        removedItem = entry.Item;
        RemoveExistingItem(ref entry, bucketIndex, entryIndex, previousEntryIndex);
        return true;
    }
}
