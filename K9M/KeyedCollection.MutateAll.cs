using System;
using System.Diagnostics;

namespace K9M;

public partial class KeyedCollection<TKey, TItem>
{
    public partial void ReplaceAll(Func<TItem, TItem> replaceItemFactory)
    {
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        ReplaceAll(static (item, f) => f(item), replaceItemFactory);
    }

    public partial void ReplaceAll<TArg>(Func<TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument)
    {
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        for (int i = 0; i < _reservedLength; i++)
        {
            ref Entry entry = ref _entries[i];
            if (entry.Next <= 0) continue;
            TKey key = KeySelector(entry.Item);
            DualVersion capturedVersion = _version;
            TItem newItem = replaceItemFactory(entry.Item, factoryArgument);
            AssertFactoryInvocationRules(_keyComparer, capturedVersion, newItem, key, nameof(replaceItemFactory));
            entry.Item = newItem;
        }
    }

    public partial int ReplaceWhere(Func<TItem, bool> match, Func<TItem, TItem> replaceItemFactory)
    {
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        var argument = (match: match, replaceItemFactory: replaceItemFactory);
        return ReplaceWhere(static (item, arg) => arg.match(item), static (item, arg) => arg.replaceItemFactory(item), argument);
    }

    public partial int ReplaceWhere<TArg>(Func<TItem, TArg, bool> match, Func<TItem, TArg, TItem> replaceItemFactory, TArg argument)
    {
        ArgumentNullException.ThrowIfNull(replaceItemFactory);
        int replacedCount = 0;
        for (int i = 0; i < _reservedLength; i++)
        {
            ref Entry entry = ref _entries[i];
            if (entry.Next <= 0) continue;
            DualVersion capturedVersion = _version;
            bool matched = match(entry.Item, argument);
            ThrowHelper.InvalidOperationException_IfCollectionWasModified(capturedVersion, _version);
            if (!matched) continue;
            TKey key = KeySelector(entry.Item);
            capturedVersion = _version;
            TItem newItem = replaceItemFactory(entry.Item, argument);
            AssertFactoryInvocationRules(_keyComparer, capturedVersion, newItem, key, nameof(replaceItemFactory));
            entry.Item = newItem;
            replacedCount++;
        }
        return replacedCount;
    }

    public partial int RemoveWhere(Predicate<TItem> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        return RemoveWhere(static (item, f) => f(item), match);
    }

    // The match delegate is invoked in buckets order, not in enumeration order.
    public partial int RemoveWhere<TArg>(Func<TItem, TArg, bool> match, TArg argument)
    {
        ArgumentNullException.ThrowIfNull(match);
        int removedCount = 0;
        for (int bucketIndex = 0; bucketIndex < _buckets.Length; bucketIndex++)
        {
            if (_buckets[bucketIndex] == 0) continue;
            int entryIndex = _buckets[bucketIndex] - 1; // Value in _buckets is 1-based.
            int previousEntryIndex = -1;
            int collisions = 0;
            while (true)
            {
                ref Entry entry = ref _entries[entryIndex];
                Debug.Assert(entry.Next > 0);
                int entryNext = entry.Next; // Preserve the Next before calling RemoveExistingItem.
                DualVersion capturedVersion = _version;
                bool matched = match(entry.Item, argument);
                ThrowHelper.InvalidOperationException_IfCollectionWasModified(capturedVersion, _version);
                if (matched)
                {
                    RemoveExistingItem(ref entry, bucketIndex, entryIndex, previousEntryIndex);
                    removedCount++;
                }
                if (entryNext == EndOfChain) break;
                if (!matched) previousEntryIndex = entryIndex;
                entryIndex = entryNext - 1; // Value in Next is 1-based.
                ThrowHelper.InvalidOperationException_IfChainCorrupted(++collisions, Capacity);
            }
        }
        return removedCount;
    }
}
