using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace K9M;

/// <summary>
/// Represents a strongly typed list of items that can be accessed by index.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>
/// This collection is an alternative to the standard .NET
/// System.Collections.Generic.List&lt;T&gt; collection.
/// It is value-type, and allocates zero memory until the first item is added
/// in the collection.
/// </remarks>
public partial struct ValueList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IEquatable<ValueList<T>>
{
    /// <summary>
    /// Initializes a new instance of the collection that is empty,
    /// and has the default initial capacity.
    /// </summary>
    public partial ValueList();

    /// <summary>
    /// Initializes a new instance of the collection that is empty,
    /// and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the internal data structure.</param>
    /// <exception cref="ArgumentOutOfRangeException">The capacity is negative.</exception>
    public partial ValueList(int capacity);

    /// <summary>
    /// Initializes a new instance of the collection that contains elements copied
    /// from the specified sequence, and has sufficient capacity to accommodate
    /// the number of elements copied.
    /// </summary>
    /// <param name="items">The enumerable sequence whose items are copied to the collection.</param>
    /// <exception cref="ArgumentNullException">The enumerable sequence is null.</exception>
    public partial ValueList(IEnumerable<T> items);

    /// <summary>
    /// Initializes a new instance of the collection that contains one repeated item.
    /// </summary>
    /// <param name="item">The item to be repeated.</param>
    /// <param name="count">The number of times to repeat the item in the collection.</param>
    /// <exception cref="ArgumentOutOfRangeException">The count is negative.</exception>
    public partial ValueList(T item, int count);

    /// <summary>Gets a value that indicates whether the collection is empty.</summary>
    /// <returns>true if the collection contains zero items, otherwise false.</returns>
    public readonly partial bool IsEmpty { get; }

    /// <summary>Gets the number of items contained in the collection.</summary>
    /// <returns>The number of items in the collection.</returns>
    public readonly partial int Count { get; }

    /// <summary>
    /// Gets the total numbers of items that can be stored in the collection without resizing.
    /// </summary>
    /// <returns>
    /// The total numbers of items the collection's backing storage can hold without resizing.
    /// </returns>
    public partial int Capacity { readonly get; set; }

    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is negative, or the index is equal to or greater than the number of items in the collection.</exception>
    public partial T this[int index]
    {
        readonly get;
        set;
    }

    /// <summary>
    /// Gets a reference to the item with the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>A reference to the item with the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is negative, or the index is equal to or greater than the number of items in the collection.</exception>
    public readonly partial ref T GetItemRef(int index);

    /// <summary>Adds the specified item to the collection.</summary>
    /// <param name="item">The item to add.</param>
    /// <returns>A reference to the newly added item inside the collection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is negative, or the index is equal to or greater than the number of items in the collection.</exception>
    public partial ref T Add(T item);

    /// <summary>
    /// Adds to the collection items copied from the specified sequence.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <exception cref="ArgumentNullException">The enumerable sequence is null.</exception>
    public partial void AddRange(IEnumerable<T> items);

    /// <summary>
    /// Inserts an item to the collection at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the item should be inserted.</param>
    /// <param name="item">The item to insert.</param>
    /// <returns>A reference to the newly added item inside the collection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is negative, or the index is greater than the number of items in the collection.</exception>
    public partial ref T Insert(int index, T item);

    /// <summary>
    /// Removes the item at the specified index from the collection.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is negative, or the index is equal to or greater than the number of items in the collection.</exception>
    public partial void RemoveAt(int index);

    /// <summary>
    /// Removes all the items that are matched by the specified delegate.
    /// </summary>
    /// <param name="match">The delegate that matches the items for removal.</param>
    /// <returns>The number of items removed from the collection.</returns>
    /// <exception cref="ArgumentNullException">The match is null.</exception>
    public partial int RemoveWhere(Predicate<T> match);

    /// <summary>
    /// Removes all the items that are matched by the specified delegate,
    /// passing the index of each item into the delegate.
    /// </summary>
    /// <param name="match">The delegate that matches the items for removal.</param>
    /// <returns>The number of items removed from the collection.</returns>
    /// <exception cref="ArgumentNullException">The match is null.</exception>
    public partial int RemoveWhere(Func<T, int, bool> match);

    /// <summary>
    /// Removes all the items that are matched by the specified delegate,
    /// passing the index of each item and the specified argument into the delegate.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the delegate.</typeparam>
    /// <param name="match">The delegate that matches the items to remove.</param>
    /// <param name="argument">The argument to pass into the delegate.</param>
    /// <returns>The number of items removed from the collection.</returns>
    /// <exception cref="ArgumentNullException">The match is null.</exception>
    public partial int RemoveWhere<TArg>(Func<T, int, TArg, bool> match, TArg argument);

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public partial void Clear();

    /// <summary>
    /// Sets the capacity of the collection to hold up exactly the current number of items.
    /// </summary>
    public partial void TrimExcess();

    /// <summary>
    /// Sets the Count of the collection to the specified value.
    /// </summary>
    /// <param name="count">The value to set the list's Count to.</param>
    /// <remarks>
    /// When increasing the Count, the revealed empty slots are filled
    /// with the default value of T.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The count is negative.</exception>
    public partial void SetCount(int count);

    /// <summary>
    /// Sets the Count of the collection to the specified value.
    /// </summary>
    /// <param name="count">The value to set the list's Count to.</param>
    /// <param name="emptyFiller">The value with which to fill the revealed empty slots.</param>
    /// <remarks>
    /// When increasing the Count, the revealed empty slots are filled
    /// with the specified T value.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The count is negative.</exception>
    public partial void SetCount(int count, T? emptyFiller);

    /// <summary>
    /// Copies the entire collection to a compatible one-dimensional array,
    /// starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from this collection.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">The target array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The arrayIndex is negative.</exception>
    /// <exception cref="ArgumentException">
    /// The number of elements in this collection is greater than the available space
    /// from arrayIndex to the end of the destination array.
    /// </exception>
    public readonly partial void CopyTo(T[] array, int arrayIndex);

    /// <summary>
    /// Copies the items of the collection to a new array.
    /// </summary>
    /// <returns>An array containing copies of the items of the collection.</returns>
    public readonly partial T[] ToArray();

    /// <summary>
    /// Gets a Span&lt;T&gt; view over the data in a collection. Items should not be added
    /// or removed from the collection while the Span&lt;T&gt; is in use.
    /// </summary>
    /// <returns>A Span&lt;T&gt; instance over the collection.</returns>
    public readonly partial Span<T> AsSpan();

    /// <summary>
    /// Returns a ref struct enumerator that iterates through the collection,
    /// yielding the references of the items inside the collection.
    /// </summary>
    /// <returns>An enumerator that iterates through the collection.</returns>
    /// <remarks>
    /// Items should not be added or removed from the collection while the enumerator is in use.
    /// </remarks>
    public readonly partial Span<T>.Enumerator GetEnumerator();

    /// <summary>
    /// Returns a value-type enumerable that can be used to iterate through the collection,
    /// that can cross await and yield boundaries.
    /// </summary>
    /// <returns>An enumerator that iterates through the collection.</returns>
    /// <remarks>
    /// Items should not be added or removed from the collection while the enumerator is in use.
    /// </remarks>
    public readonly partial ArraySegment<T> AsEnumerable();

    /// <summary>
    /// Determines whether this and the other collection are backed by the same array,
    /// and contain the same number of items.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <returns>true if this collection is equal to the other collection, otherwise false.</returns>
    public readonly partial bool Equals(ValueList<T> other);
}
