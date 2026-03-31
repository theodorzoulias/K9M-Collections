using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace K9M;

public partial struct ValueList<T>
{
    private const int DefaultCapacity = 4;
    private const double MinimumGrowth = 1.15;

    [DisallowNull] private T[]? _items;
    private int _count;

    [MemberNotNull(nameof(_items))]
    private void InitializeToZeroCapacity()
    {
        _items = [];
        _count = 0;
    }

    /// <summary>
    /// Ensure that the collection has space for one extra item.
    /// The field _items should be initialized before calling this method.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureSpaceForOne()
    {
        Debug.Assert(_items is not null);
        DebugAssertions();
        if (_count >= _items.Length)
            Grow(_count + 1, GrowStrategy.PowerOfTwo);
    }

    /// <summary>
    /// Ensure that the collection has space for many extra items.
    /// The field _items should be initialized before calling this method.
    /// </summary>
    private void EnsureSpaceForMany(int spaceNeeded)
    {
        Debug.Assert(spaceNeeded > 0);
        Debug.Assert(_items is not null);
        DebugAssertions();
        int emptySpace = _items.Length - _count;
        if (emptySpace >= spaceNeeded) return;
        // If the collection is empty, allocate just the space needed.
        // Otherwise expand by doubling.
        GrowStrategy growStrategy = _count == 0 ?
            GrowStrategy.ExactNumber : GrowStrategy.PowerOfTwo;
        Grow(checked(spaceNeeded + _count), growStrategy);
    }

    /// <summary>Expand the capacity to at least the given number.</summary>
    [MemberNotNull(nameof(_items))]
    private void Grow(int minimumCapacity, GrowStrategy growStrategy)
    {
        Debug.Assert(minimumCapacity > 0);
        DebugAssertions();
        _items ??= []; // Ensure not null.
        if (minimumCapacity <= _items.Length) return;
        int newSize;
        if (growStrategy == GrowStrategy.ExactNumber)
        {
            newSize = CapacityHelper.GrowToExactNumber(minimumCapacity, Array.MaxLength);
        }
        else if (growStrategy == GrowStrategy.PowerOfTwo)
        {
            newSize = CapacityHelper.GrowToPowerOfTwo(_items.Length,
                Math.Max(minimumCapacity, DefaultCapacity), Array.MaxLength, MinimumGrowth);
        }
        else throw new NotSupportedException();
        Resize(newSize);
    }

    /// <summary>
    /// Change the capacity to the exact given number.
    /// The field _items should be initialized before calling this method.
    /// </summary>
    private void Resize(int newSize)
    {
        Debug.Assert(_items is not null);
        Debug.Assert(newSize >= _count);
        Debug.Assert(newSize != _items.Length);
        DebugAssertions();
        if (newSize == 0)
        {
            InitializeToZeroCapacity();
            return;
        }
        T[] newItems = new T[newSize];
        if (_count > 0)
        {
            Array.Copy(_items, newItems, _count);
        }
        _items = newItems;
        DebugAssertions();
    }

    /// <summary>
    /// Validation of the 'index' argument, for properties and methods with index parameter.
    /// It's not used by the Insert method, because this method has special validation rules.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [MemberNotNull(nameof(_items))]
    private readonly void ValidateIndexWithinRange(int index)
    {
        if (index < 0 || index >= _count)
            ThrowHelper.ArgumentOutOfRangeException_IndexOutOfRange(nameof(index));

        // If the _items is null, _count is 0, and any index is out of range.
        // So the _items can't be null here.
        Debug.Assert(_items is not null);
        DebugAssertions();
    }

    [Conditional("DEBUG")]
    private readonly void DebugAssertions()
    {
        Debug.Assert(_count >= 0);
        if (_items is null)
        {
            Debug.Assert(_count == 0);
        }
        else
        {
            Debug.Assert(_count <= _items.Length);
        }
    }
}
