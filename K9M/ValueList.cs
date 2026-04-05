using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace K9M;

public partial struct ValueList<T> : IList<T>, IReadOnlyList<T>, IEquatable<ValueList<T>>
{
    // All public constructors initialize the _items to the empty array singleton.
    public partial ValueList()
    {
        _items = [];
    }

    public partial ValueList(int capacity) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0) return;
        Grow(capacity, GrowStrategy.ExactNumber);
    }

    public partial ValueList(IEnumerable<T> items) : this()
    {
        AddRange(items);
    }

    public partial ValueList(T item, int count) : this()
    {
        SetCount(count, item);
    }

    // This internal constructor doesn't force the initialization of the _items.
    internal ValueList(T[]? array, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, array?.Length ?? 0);
        //// Check for null first, because of the [DisallowNull] attribute on the _items.
        //if (array is null) return;
        _items = array;
        _count = count;
    }

    public readonly partial bool IsDefault
    {
        get
        {
            DebugAssertions();
            return _items is null;
        }
    }

    public readonly partial bool IsEmpty => _count == 0;

    public readonly partial int Count => _count;

    public partial int Capacity
    {
        readonly get => _items?.Length ?? 0;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, _count);
            if (_items is null && value == 0) return;
            _items ??= [];
            if (value == _items.Length) return;
            if (value == 0)
            {
                _items = [];
                return;
            }

            T[] newItems = new T[value];
            if (_count > 0)
            {
                Array.Copy(_items, newItems, _count);
            }
            _items = newItems;
        }
    }

    public partial T this[int index]
    {
        readonly get
        {
            ValidateIndexWithinRange(index);
            return _items[index];
        }
        set
        {
            ValidateIndexWithinRange(index);
            _items[index] = value;
        }
    }

    public readonly partial ref T GetItemRef(int index)
    {
        ValidateIndexWithinRange(index);
        return ref _items[index];
    }

    public partial ref T Add(T item)
    {
        _items ??= [];
        EnsureSpaceForOne();
        ref T refItem = ref _items[_count];
        refItem = item;
        _count++;
        return ref refItem;
    }

    public partial ref T Insert(int index, T item)
    {
        // Insertions at the end are legal, so index equal to _count is within range.
        if (index < 0 || index > _count)
            ThrowHelper.ArgumentOutOfRangeException_IndexOutOfRange(nameof(index));

        _items ??= [];
        EnsureSpaceForOne();
        if (index < _count)
        {
            Array.Copy(_items, index, _items, index + 1, _count - index);
        }
        ref T refItem = ref _items[index];
        refItem = item;
        _count++;
        return ref refItem;
    }

    public partial void AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items is T[] array)
        {
            if (array.Length == 0) return;
            _items ??= [];
            EnsureSpaceForMany(array.Length);
            array.CopyTo(_items, _count);
            _count += array.Length;
        }
        else if (items is ICollection<T> collection)
        {
            int count = collection.Count;
            if (count == 0) return;
            _items ??= [];
            EnsureSpaceForMany(count);
            collection.CopyTo(_items, _count);
            _count += count;
        }
        else
        {
            System.Linq.Enumerable.TryGetNonEnumeratedCount(items, out int count);
            if (count > 1)
            {
                _items ??= [];
                EnsureSpaceForMany(count);
            }

            using IEnumerator<T> enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext()) return;
            _items ??= [];
            while (true)
            {
                EnsureSpaceForOne();
                _items[_count] = enumerator.Current;
                _count++;
                if (!enumerator.MoveNext()) return;
            }
        }
    }

    public partial void RemoveAt(int index)
    {
        ValidateIndexWithinRange(index);
        int newCount = _count - 1;
        if (index < newCount)
        {
            Array.Copy(_items, index + 1, _items, index, newCount - index);
        }
        _items[newCount] = default!;
        _count = newCount;
    }

    public partial int RemoveWhere(Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        return RemoveWhere(static (x, _, f) => f(x), match);
    }

    public partial int RemoveWhere(Func<T, int, bool> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        return RemoveWhere(static (x, i, f) => f(x, i), match);
    }

    public partial int RemoveWhere<TArg>(Func<T, int, TArg, bool> match, TArg argument)
    {
        ArgumentNullException.ThrowIfNull(match);
        if (_items is null) return 0;

        int i = 0, j = 0;
        try
        {
            for (; i < _count; i++)
            {
                if (match(_items[i], i, argument)) continue;
                if (j < i) _items[j] = _items[i];
                j++;
            }
        }
        finally
        {
            if (j < i)
            {
                for (; i < _count; i++, j++)
                    _items[j] = _items[i];
                Debug.Assert(j <= _count);
                SetCount(j);
            }
        }
        return i - j;
    }

    public partial void Reset()
    {
        InitializeToNull();
    }

    public partial void Clear()
    {
        if (_items is null) return;
        InitializeToZeroCapacity();
    }

    public partial void TrimExcess()
    {
        Capacity = _count;
    }

    public partial void SetCount(int count)
    {
        SetCount(count, false, default);
    }

    public partial void SetCount(int count, T? emptyFiller)
    {
        SetCount(count, true, emptyFiller);
    }

    /// <summary>
    /// Implements both public partial SetCount overloads.
    /// </summary>
    private void SetCount(int count, bool fillEmptySpace, T? emptyFiller)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (_items is null && count == 0) return;
        _items ??= [];
        if (count > _count)
        {
            if (count > _items.Length)
                Grow(count, GrowStrategy.ExactNumber);
            if (fillEmptySpace)
                Array.Fill(_items, emptyFiller, _count, count - _count);
        }
        else if (count < _count)
        {
            Array.Clear(_items, count, _count - count);
        }
        _count = count;
        DebugAssertions();
    }

    public readonly partial void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        Array.Copy(_items ?? [], 0, array, arrayIndex, _count);
    }

    public readonly partial T[] ToArray()
    {
        if (_items is null) return [];
        return _items[.._count];
    }

    public readonly partial Span<T> AsSpan()
    {
        if (_items is null) return default;
        return _items.AsSpan(0, _count);
    }

    public readonly partial Span<T>.Enumerator GetEnumerator()
    {
        return AsSpan().GetEnumerator();
    }

    public readonly partial ArraySegment<T> AsEnumerable()
    {
        if (_items is null) return new([]);
        return new ArraySegment<T>(_items, 0, _count);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => AsEnumerable().GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => AsEnumerable().GetEnumerator();

    readonly bool ICollection<T>.IsReadOnly => false;

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        int index = AsSpan().IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    bool ICollection<T>.Contains(T item)
    {
        return _items is not null && Array.IndexOf(_items, item, 0, _count) >= 0;
    }

    int IList<T>.IndexOf(T item)
    {
        return _items is not null ? Array.IndexOf(_items, item, 0, _count) : -1;
    }

    void IList<T>.Insert(int index, T item)
    {
        Insert(index, item);
    }

    public readonly partial bool Equals(ValueList<T> other)
    {
        return ReferenceEquals(_items, other._items) && _count == other._count;
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is ValueList<T> other && Equals(other);
    }

    public readonly override int GetHashCode()
    {
        HashCode hashcode = new();
        hashcode.Add(_items);
        hashcode.Add(_count);
        return hashcode.ToHashCode();
    }

    /// <summary>
    /// Determines whether the two specified collections are backed by the same array,
    /// and contain the same number of items.
    /// </summary>
    /// <param name="left">The first collection to compare.</param>
    /// <param name="right">The second collection to compare.</param>
    /// <returns>true if the two collections are equal, otherwise false.</returns>
    public static bool operator ==(ValueList<T> left, ValueList<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether the two specified collections are backed by different arrays,
    /// or contain different number of items.
    /// </summary>
    /// <param name="left">The first collection to compare.</param>
    /// <param name="right">The second collection to compare.</param>
    /// <returns>true if the two collections are different, otherwise false.</returns>
    public static bool operator !=(ValueList<T> left, ValueList<T> right)
    {
        return !(left == right);
    }
}
