using K9M.NullRef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace K9M;

// Fields and core operations.
public partial class KeyedCollection<TKey, TItem>
{
    private readonly Func<TItem, TKey> _keySelector;
    private readonly IAlternateEqualityComparer<TKey, TKey> _keyComparer;

    private Entry[] _entries;

    /// <summary>
    /// Points to the first entry of each bucket. The indices are 1-based.
    /// For example the value 1 points to the _entries[0], the value 2 to the _entries[1] etc.
    /// The value 0 indicates that the specific bucket is empty.
    /// </summary>
    private int[] _buckets;

    /// <summary>The portion of the _entries array that is occupied, containing either data or free slots.</summary>
    private int _reservedLength;

    /// <summary>The number of slots populated with data (excluding the free slots).</summary>
    private int _dataCount;

    /// <summary>
    /// The head of the linked list that points to free slots.
    /// The tail of the linked list is marked with the EndOfChain value.
    /// When the linked list is empty, this field has the value EndOfChain.
    /// </summary>
    private int _freeList;

    /// <summary>
    /// A value that, when changed, invalidates enumerators and callback-based operations.
    /// Any operation that changes the Count is a version change, with the exception
    /// of TryRemove that invalidates only callback-based operations, not enumerators.
    /// Resizing the collection is also a version change.
    /// </summary>
    private DualVersion _version;

    /// <summary>This value denotes the tail of a linked list chain.</summary>
    private const int EndOfChain = Int32.MaxValue;

    private struct Entry
    {
        public uint HashCode;

        /// <summary>
        /// 1-based index of next entry in chain. Int32.MaxValue means end of chain.
        /// Also encodes whether this entry is part of the free linked list.
        /// The free list is negative 1-based. Int32.MinValue means end of the free list chain.
        /// Positive index means entry populated with data.
        /// Negative index means free slot.
        /// Zero index means that the entry is beyond the _reservedLength, and has never been used.
        /// </summary>
        public int Next;

        public TItem Item;
    }

    /// <summary>Sets empty arrays as backing storage.</summary>
    [MemberNotNull(nameof(_entries), nameof(_buckets))]
    private void InitializeToZeroCapacity()
    {
        _entries = [];
        _buckets = [];
        _reservedLength = 0;
        _dataCount = 0;
        _freeList = EndOfChain;
        unchecked { _version.Core++; }
        DebugAssertions();
    }

    /// <summary>
    /// Extracts the key.
    /// Also validates that the item is not null.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TKey ExtractKey(TItem item)
    {
        if (item == null) ThrowHelper.ArgumentNullException(nameof(item));
        return KeySelector(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucket(uint hashCode)
    {
        return ref _buckets[GetBucketIndex(hashCode)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBucketIndex(uint hashCode)
    {
        return (int)(hashCode % (uint)_buckets.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry FindEntry(TKey key,
        out uint hashCode, out int bucketIndex, out int entryIndex, out int previousEntryIndex)
    {
        return ref FindEntry<TKey>(_keyComparer, key,
            out hashCode, out bucketIndex, out entryIndex, out previousEntryIndex);
    }

    /// <summary>
    /// Finds the entry of the key.
    /// Also validates that the key is not null, and returns the hashcode of the key.
    /// </summary>
    private ref Entry FindEntry<TAlternateKey>(
        IAlternateEqualityComparer<TAlternateKey, TKey> comparer,
        TAlternateKey key,
        out uint hashCode, out int bucketIndex, out int entryIndex, out int previousEntryIndex) where TAlternateKey : allows ref struct
    {
        if (key == null) ThrowHelper.ArgumentNullException(nameof(key));
        hashCode = (uint)comparer.GetHashCode(key);
        if (Capacity == 0) { bucketIndex = entryIndex = previousEntryIndex = -1; return ref Unsafe.NullRef<Entry>(); }
        bucketIndex = GetBucketIndex(hashCode);
        previousEntryIndex = -1;
        entryIndex = _buckets[bucketIndex] - 1; // Value in _buckets is 1-based.
        int collisions = 0;
        while (entryIndex >= 0)
        {
            ref Entry entry = ref _entries[entryIndex];
            if (entry.HashCode == hashCode && comparer.Equals(key, KeySelector(entry.Item)))
                return ref entry;
            if (entry.Next == EndOfChain) break;
            previousEntryIndex = entryIndex;
            entryIndex = entry.Next - 1; // Value in Next is 1-based.
            ThrowHelper.InvalidOperationException_IfChainCorrupted(++collisions, Capacity);
        }
        return ref Unsafe.NullRef<Entry>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryAddInternal(TKey key, TItem item, out int entryIndex)
    {
        ref Entry entry = ref FindEntry(key, out uint hashCode, out int bucketIndex, out entryIndex, out _);
        if (entry.IsNotNull) return false;
        AddNewInternal(hashCode, item, bucketIndex, out entryIndex);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddInternal(TKey key, TItem item, out int entryIndex)
    {
        if (!TryAddInternal(key, item, out entryIndex))
            ThrowHelper.ArgumentException_TheKeyAlreadyExists(key);
    }

    private void AddNewInternal(uint hashCode, TItem item, int bucketIndex, out int entryIndex)
    {
        // For some reason isolating the reserving-space code in a separate method makes the Add faster.
        ReserveSpaceForEntry(hashCode, ref bucketIndex, out entryIndex);
        ref Entry entry = ref _entries[entryIndex];
        ref int bucket = ref _buckets[bucketIndex];
        entry.HashCode = hashCode;
        entry.Item = item;
        entry.Next = (bucket == 0) ? EndOfChain : bucket;
        bucket = entryIndex + 1; // Values in _buckets are 1-based.
        _dataCount++;
        DebugAssertions();
    }

    private void RemoveExistingItem(ref Entry entry, int bucketIndex, int entryIndex, int previousEntryIndex)
    {
        Debug.Assert(entry.IsNotNull);
        Debug.Assert(_dataCount > 0);
        if (previousEntryIndex < 0)
        {
            ref int bucket = ref _buckets[bucketIndex];
            Debug.Assert(bucket == entryIndex + 1);
            bucket = (entry.Next == EndOfChain) ? 0 : entry.Next;
        }
        else
            _entries[previousEntryIndex].Next = entry.Next;

        entry.HashCode = default;
        entry.Item = default!;
        entry.Next = -_freeList - 1; // Free list: value in Next is negative 1-based.
        _freeList = entryIndex;
        _dataCount--;
        unchecked { _version.Removals++; }
        DebugAssertions();
    }

    private void ReserveSpaceForEntry(uint hashCode, ref int bucketIndex, out int entryIndex)
    {
        Debug.Assert((bucketIndex == -1) == (Capacity == 0));
        if (_dataCount < _reservedLength)
        {
            Debug.Assert(_freeList != EndOfChain);
            entryIndex = _freeList;
            _freeList = -_entries[_freeList].Next - 1; // Free list: value in Next is negative 1-based.
        }
        else
        {
            if (_reservedLength == Capacity)
            {
                Grow(Capacity + 1, GrowStrategy.PowerOfTwo);
                bucketIndex = GetBucketIndex(hashCode);
            }
            entryIndex = _reservedLength;
            _reservedLength++;
        }
        unchecked { _version.Core++; }
    }

    /// <summary>
    /// Assert that a factory invocation follows the rules:
    /// 1. The collection should not be modified during the invocation.
    /// 2. The produced item should be non-null.
    /// 3. The produced item should have the original key.
    /// </summary>
    private void AssertFactoryInvocationRules<TAlternateKey>(
        IAlternateEqualityComparer<TAlternateKey, TKey> comparer,
        DualVersion capturedVersion, TItem newItem, TAlternateKey originalKey, string factoryDelegateName) where TAlternateKey : allows ref struct
    {
        ThrowHelper.InvalidOperationException_IfCollectionWasModified(_version, capturedVersion);
        ThrowHelper.InvalidOperationException_IfFactoryReturnedNullItem(newItem);
        TKey newKey = KeySelector(newItem);
        if (!comparer.Equals(originalKey, newKey))
            ThrowHelper.InvalidOperationException_FactoryReturnedIncompatibleKey(factoryDelegateName);
    }

    private const int DefaultCapacity = 4;
    private const double MinimumGrowth = 1.15;

    /// <summary>Expand the capacity to at least the given number.</summary>
    private void Grow(int minimumCapacity, GrowStrategy growStrategy)
    {
        Debug.Assert(minimumCapacity > 0);
        if (minimumCapacity <= Capacity) return;
        int newSize;
        if (growStrategy == GrowStrategy.ExactNumber)
        {
            newSize = CapacityHelper.GrowToExactNumber(minimumCapacity, Array.MaxLength);
        }
        else if (growStrategy == GrowStrategy.PowerOfTwo)
        {
            newSize = CapacityHelper.GrowToPowerOfTwo(Capacity,
                Math.Max(minimumCapacity, DefaultCapacity), Array.MaxLength, MinimumGrowth);
        }
        else throw new NotSupportedException();
        Resize(newSize);
    }

    /// <summary>Change the capacity to the exact given number.</summary>
    private void Resize(int newSize)
    {
        Debug.Assert(newSize >= Count);
        Debug.Assert(newSize != Capacity);
        if (newSize == 0)
        {
            InitializeToZeroCapacity(); return;
        }
        Entry[]? oldEntries = _entries;
        AllocateNewBucketsAndEntries(newSize);
        if (newSize >= oldEntries.Length)
        {
            // Grow
            Array.Copy(oldEntries, _entries, _reservedLength);
            oldEntries = null; // Let it get recycled.
            for (int i = 0; i < _reservedLength; i++)
            {
                ref Entry entry = ref _entries[i];
                if (entry.Next <= 0) continue;
                ref int bucket = ref GetBucket(entry.HashCode);
                entry.Next = (bucket == 0) ? EndOfChain : bucket;
                bucket = i + 1; // Values in _buckets are 1-based.
            }
        }
        else
        {
            // Shrink
            int newCount = 0;
            for (int i = 0; i < _reservedLength; i++)
            {
                ref Entry oldEntry = ref oldEntries[i];
                if (oldEntry.Next <= 0) continue;
                ref Entry entry = ref _entries[newCount];
                entry = oldEntry; // Copy the entry in place
                ref int bucket = ref GetBucket(entry.HashCode);
                entry.Next = (bucket == 0) ? EndOfChain : bucket;
                bucket = newCount + 1; // Values in _buckets are 1-based.
                newCount++;
            }
            Debug.Assert(newCount == _dataCount);
            _reservedLength = newCount;
            _dataCount = newCount;
            _freeList = EndOfChain;
        }
        DebugAssertions(scanAllEntries: true);
    }

    /// <summary>Replace both arrays with new arrays in unison.</summary>
    private void AllocateNewBucketsAndEntries(int newSize)
    {
        Debug.Assert(newSize > 0);
        int newBucketsSize = GetBucketsSize(newSize);
        Entry[] newEntries = new Entry[newSize];
        int[] newBuckets = new int[newBucketsSize];
        // Assign the fields after the allocation of both arrays,
        // to guard against corruption from OOM if second fails.
        _entries = newEntries;
        _buckets = newBuckets;
        unchecked { _version.Core++; }
    }

    private int GetBucketsSize(int entriesLength)
    {
        int size = entriesLength;
        if (!MathHelper.TryGetPrime(size, Array.MaxLength, out size))
            size = Array.MaxLength;
        return size;
    }

    /// <remarks>
    /// The index should be initialized to -1. The version to 0.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryMoveNext(ref int index, ref ushort version)
    {
        Debug.Assert(index >= -1);
        if (index == -1)
        {
            version = _version.Core;
        }
        else
        {
            ThrowHelper.InvalidOperationException_IfCollectionWasModified(version, _version.Core);
        }
        if (index < _reservedLength)
        {
            while (++index < _reservedLength)
            {
                if (_entries[index].Next > 0) return true;
            }
        }
        return false;
    }

    private IEnumerator<TItem> EnumerateItems()
    {
        int index = -1;
        ushort version = 0;
        while (TryMoveNext(ref index, ref version))
            yield return _entries[index].Item;
    }

    internal IEnumerable<TKey> EnumerateKeys()
    {
        int index = -1;
        ushort version = 0;
        while (TryMoveNext(ref index, ref version))
            yield return _keySelector(_entries[index].Item);
    }

    internal IEnumerator<KeyValuePair<TKey, TItem>> EnumerateKeyValuePairs()
    {
        int index = -1;
        ushort version = 0;
        while (TryMoveNext(ref index, ref version))
        {
            ref TItem item = ref _entries[index].Item;
            yield return KeyValuePair.Create(_keySelector(item), item);
        }
    }

    /// <summary>
    /// Grows the collection in advance, before an AddRange operation.
    /// When the behavior regarding existing keys is "ignore", the other collection should
    /// have distinct elements, otherwise the space reserved by growing might remain unused.
    /// </summary>
    private void GrowBeforeAddRange(int otherCount)
    {
        Debug.Assert(otherCount > 0);
        if (Count == 0)
        {
            // The dictionary is empty. Allocate just the space required for the collection.
            Grow(otherCount, GrowStrategy.ExactNumber);
        }
        else
        {
            // The dictionary is not empty. Expand by doubling the space recursively as with Add.
            Grow(checked(Count + otherCount), GrowStrategy.PowerOfTwo);
        }
    }

    private void AddRangeArray(TItem[] array, KeyExistsBehavior keyExistsBehavior,
        [CallerArgumentExpression(nameof(array))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(array, paramName);
        if (array.Length == 0) return; // Nothing to do
        if (keyExistsBehavior == KeyExistsBehavior.Throw)
            GrowBeforeAddRange(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            TItem item = array[i];
            if (item is null) ThrowHelper.ArgumentException_CollectionContainsNullItem(paramName);
            TKey key = KeySelector(item);
            bool added = TryAddInternal(key, item, out _);
            ThrowHelper.ArgumentException_WhenKeyExists(key, added, keyExistsBehavior);
        }
    }

    private void AddRangeEnumerable(
        IEnumerable<TItem> collection, KeyExistsBehavior keyExistsBehavior,
        [CallerArgumentExpression(nameof(collection))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(collection, paramName);
        if (collection is TItem[] array)
        {
            AddRangeArray(array, keyExistsBehavior, paramName);
            return;
        }
        else if (Enumerable.TryGetNonEnumeratedCount(collection, out int colCount))
        {
            if (colCount <= 0) return; // Nothing to do
            if (keyExistsBehavior == KeyExistsBehavior.Throw)
            {
                // We can grow the collection in advance, because the other collection is not
                // expected to have duplicates. In case it does, an exception will be thrown.
                // This is only used during the construction of the collection.
                GrowBeforeAddRange(colCount);
            }
        }
        foreach (TItem item in collection)
        {
            if (item is null) ThrowHelper.ArgumentException_CollectionContainsNullItem(paramName);
            TKey key = KeySelector(item);
            bool added = TryAddInternal(key, item, out _);
            ThrowHelper.ArgumentException_WhenKeyExists(key, added, keyExistsBehavior);
        }
    }

    [Conditional("DEBUG")]
    private void DebugAssertions(bool scanAllEntries = false)
    {
        Debug.Assert((_entries.Length == 0) == (_buckets.Length == 0));
        Debug.Assert(_reservedLength >= 0);
        Debug.Assert(_reservedLength <= Capacity);
        Debug.Assert(_dataCount >= 0);
        Debug.Assert(_dataCount <= _reservedLength);
        if (!scanAllEntries) return;
        int emptyEntriesCount = 0;
        int leafEntriesCount = 0;
        for (int i = 0; i < _entries.Length; i++)
        {
            ref Entry entry = ref _entries[i];
            if (i < _reservedLength)
            {
                Debug.Assert(entry.Next != 0);
                if (entry.Next < 0)
                {
                    emptyEntriesCount++;
                    Debug.Assert(entry.HashCode == 0);
                }
                else if (entry.Next == EndOfChain)
                {
                    leafEntriesCount++;
                }
            }
            else
            {
                Debug.Assert(entry.HashCode == default);
                Debug.Assert(entry.Next == default);
                Debug.Assert(EqualityComparer<TItem>.Default.Equals(entry.Item, default));
            }
        }
        Debug.Assert(_dataCount + emptyEntriesCount == _reservedLength);
        int bucketsUsed = _buckets.Count(x => x > 0);
        Debug.Assert(bucketsUsed == leafEntriesCount, $"{bucketsUsed}, {leafEntriesCount}");
        int countFromBuckets = GetCountFromBuckets();
        Debug.Assert(countFromBuckets == _dataCount, $"{countFromBuckets}, {_dataCount}");

        // Walks the chain of entries starting from each non-empty bucket, and counts the entries.
        int GetCountFromBuckets()
        {
            int count = 0;
            for (int bucketIndex = 0; bucketIndex < _buckets.Length; bucketIndex++)
            {
                Debug.Assert(_buckets[bucketIndex] >= 0);
                int entryIndex = _buckets[bucketIndex] - 1; // Value in _buckets is 1-based.
                if (entryIndex < 0) continue;
                Debug.Assert(entryIndex < _reservedLength);
                int collisions = 0;
                while (true)
                {
                    count++;
                    ref Entry entry = ref _entries[entryIndex];
                    Debug.Assert(entry.Next > 0);
                    if (entry.Next == EndOfChain) break;
                    entryIndex = entry.Next - 1; // Value in Next is 1-based.
                    ThrowHelper.InvalidOperationException_IfChainCorrupted(++collisions, Capacity);
                }
            }
            return count;
        }
    }

    private void Dump()
    {
        Console.WriteLine($"Dump:");
        Console.WriteLine($"  {nameof(_entries)}.Length: {_entries.Length:#,0}, {nameof(_buckets)}.Length: {_buckets.Length:#,0}");
        Console.WriteLine($"  {nameof(_reservedLength)}: {_reservedLength}, {nameof(_dataCount)}: {_dataCount}, {nameof(_freeList)}: {_freeList}, {nameof(_version)}: {_version}");
        Console.WriteLine($"  {nameof(_buckets)}: {String.Join(", ", _buckets)}");
        for (int i = 0; i < _entries.Length; i++)
            Console.WriteLine($"  Entry {{ HashCode = {_entries[i].HashCode}, Next = {_entries[i].Next}, Item = {_entries[i].Item} }}");
        DebugAssertions(scanAllEntries: true);
    }
}
