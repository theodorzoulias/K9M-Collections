using System;
using System.Collections.Generic;

namespace K9M;

/// <summary>
/// Represents a collection of items with embedded keys. Every item in the collection
/// must have a unique key, according to the collection's key equality comparer.
/// </summary>
/// <typeparam name="TKey">The type of the embedded keys.</typeparam>
/// <typeparam name="TItem">The type of the items.</typeparam>
/// <remarks>
/// This collection is tailored for items that are structs, either immutable or mutable.
/// It consumes the same memory per item as a .NET HashSet, which is less than a .NET Dictionary.
/// It provides the tools needed for efficient in-place mutations of the items, with minimal hashings of the keys.
/// It is implemented as a hashtable, similar to the .NET Dictionary and HashSet, with
/// similar performance characteristics. After inserting an item in the collection, its key should be immutable.
/// Every other property of the item is allowed to change, except the key.
/// </remarks>
public partial class KeyedCollection<TKey, TItem> : ICollection<TItem> where TKey : notnull where TItem : notnull
{
    /// <summary>
    /// Initializes a new instance of the collection that is empty, has the default initial
    /// capacity, and uses the default equality comparer for the key type.
    /// </summary>
    /// <param name="keySelector">The function that is used to extract the keys from the items.</param>
    /// <exception cref="ArgumentNullException">The keySelector is null.</exception>
    public partial KeyedCollection(Func<TItem, TKey> keySelector);

    /// <summary>
    /// Initializes a new instance of the collection that is empty, has the default initial
    /// capacity, and uses the specified equality comparer for the key type.
    /// </summary>
    /// <param name="keySelector">The function that is used to extract the keys from the items.</param>
    /// <param name="comparer">The equality comparer that is used to determine equality
    /// of keys for the collection, and to provide hash values for the keys.</param>
    /// <exception cref="ArgumentNullException">The keySelector is null.</exception>
    public partial KeyedCollection(Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer);

    /// <summary>
    /// Initializes a new instance of the collection that is empty, has the specified initial
    /// capacity, and uses the default equality comparer for the key type.
    /// </summary>
    /// <param name="keySelector">The function that is used to extract the keys from the items.</param>
    /// <param name="capacity">The initial capacity of the internal data structure.</param>
    /// <exception cref="ArgumentNullException">The keySelector is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The capacity is negative.</exception>
    public partial KeyedCollection(Func<TItem, TKey> keySelector, int capacity);

    /// <summary>
    /// Initializes a new instance of the collection that is empty, has the specified initial
    /// capacity, and uses the specified equality comparer for the key type.
    /// </summary>
    /// <param name="keySelector">The function that is used to extract the keys from the items.</param>
    /// <param name="capacity">The initial capacity of the internal data structure.</param>
    /// <param name="comparer">The equality comparer that is used to determine equality
    /// of keys for the collection, and to provide hash values for the keys.</param>
    /// <exception cref="ArgumentNullException">The keySelector is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The capacity is negative.</exception>
    public partial KeyedCollection(Func<TItem, TKey> keySelector, int capacity, IEqualityComparer<TKey> comparer);

    /// <summary>
    /// Initializes a new instance of the collection that contains items copied from
    /// the specified enumerable sequence, and uses the default equality comparer
    /// for the key type.
    /// </summary>
    /// <param name="keySelector">The function that is used to extract the keys from the items.</param>
    /// <param name="items">The enumerable sequence whose items are copied to the collection.</param>
    /// <exception cref="ArgumentNullException">The keySelector or the enumerable sequence is null.</exception>
    /// <exception cref="ArgumentException">The enumerable sequence contains one or more items with duplicated keys.</exception>
    public partial KeyedCollection(Func<TItem, TKey> keySelector, IEnumerable<TItem> items);

    /// <summary>
    /// Initializes a new instance of the collection that contains items copied from
    /// the specified enumerable sequence, and uses the specified equality comparer
    /// for the key type.
    /// </summary>
    /// <param name="keySelector">The function that is used to extract the keys from the items.</param>
    /// <param name="items">The enumerable sequence whose items are copied to the collection.</param>
    /// <param name="comparer">The equality comparer that is used to determine equality
    /// of keys for the collection, and to provide hash values for the keys.</param>
    /// <exception cref="ArgumentNullException">The keySelector or the enumerable sequence is null.</exception>
    /// <exception cref="ArgumentException">The enumerable sequence contains one or more items with duplicated keys.</exception>
    public partial KeyedCollection(Func<TItem, TKey> keySelector, IEnumerable<TItem> items, IEqualityComparer<TKey> comparer);

    /// <summary>Gets the number of items contained in the collection.</summary>
    /// <returns>The number of items in the collection.</returns>
    public partial int Count { get; }

    /// <summary>Gets a value that indicates whether the collection is empty.</summary>
    /// <returns>true if the collection contains zero items, otherwise false.</returns>
    public partial bool IsEmpty { get; }

    /// <summary>
    /// Gets the function that is used to extract the keys from the items.
    /// </summary>
    /// <returns>
    /// The function that was provided in the constructor, and is used to extract
    /// the keys from the items.
    /// </returns>
    public partial Func<TItem, TKey> KeySelector { get; }

    /// <summary>
    /// Gets the equality comparer that is used to determine equality of keys for the collection.
    /// </summary>
    /// <returns>
    /// The equality comparer that is used to determine equality of keys for the collection,
    /// and to provide hash values for the keys.
    /// </returns>
    public partial IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Gets the total numbers of items that can be stored in the collection without resizing.
    /// </summary>
    /// <returns>
    /// The total numbers of items the collection's backing storage can hold without resizing.
    /// </returns>
    public partial int Capacity { get; }

    /// <summary>
    /// Determines whether the collection contains an item with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the collection.</param>
    /// <returns>true if the collection contains an item with the specified key, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">The key is null.</exception>
    public partial bool ContainsKey(TKey key);

    /// <summary>
    /// Determines whether the collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to locate in the collection.</param>
    /// <returns>
    /// true if the collection contains an item with the same key, that is also equal
    /// according to the default equality comparer for the type. Otherwise false.
    /// </returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial bool ContainsItem(TItem item);

    /// <summary>
    /// Determines whether the collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to locate in the collection.</param>
    /// <param name="comparer">
    /// The equality comparer to use to determine if an item in the collection is equal
    /// with the specified item, after locating a candidate item in the collection based
    /// on the equality of the keys.
    /// If this argument is null, the default equality comparer for the type is used.
    /// </param>
    /// <returns>
    /// true if the collection contains an item with the same key,
    /// that is also equal according to the specified equality comparer. Otherwise false.
    /// </returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial bool ContainsItem(TItem item, IEqualityComparer<TItem> comparer);

    /// <summary>Gets the item with the specified key.</summary>
    /// <param name="key">The key of the item to get.</param>
    /// <returns>A reference to the item with the specified key. An exception is thrown if an item with the specified key is not found.</returns>
    /// <exception cref="ArgumentNullException">The key is null.</exception>
    /// <exception cref="KeyNotFoundException">The given key was not present in the collection.</exception>
    public partial ref TItem this[TKey key] { get; }

    /// <summary>Gets the item with the specified key.</summary>
    /// <param name="key">The key of the item to get.</param>
    /// <param name="item">
    /// Returns the item with the specified key, if the key is found.
    /// Otherwise returns the default value for the type.
    /// </param>
    /// <returns>true if the collection contains an item with the specified key, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">The key is null.</exception>
    public partial bool TryGetItem(TKey key, out TItem item);

    /// <summary>
    /// Gets a reference to the item with the specified key, or a null managed pointer if
    /// the key does not exist in the collection.
    /// </summary>
    /// <param name="key">The key of the item to get.</param>
    /// <returns>Either a reference to the item with the specified key, or a null managed pointer if the key is not found.</returns>
    public partial ref TItem GetItemRef(TKey key);

    /// <summary>
    /// Gets a reference to the item with the specified key, or a null managed pointer if
    /// the key does not exist in the collection.
    /// </summary>
    /// <param name="key">The key of the item to get.</param>
    /// <param name="exists">Returns true if an item with the specified key was found in the collection, otherwise false.</param>
    /// <returns>Either a reference to the item with the specified key, or a null managed pointer if the key is not found.</returns>
    public partial ref TItem GetItemRef(TKey key, out bool exists);

    /// <summary>Adds the specified item to the collection.</summary>
    /// <param name="item">The item to add.</param>
    /// <returns>A reference to the newly added item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    /// <exception cref="ArgumentException">An item with the same key already exists in the collection.</exception>
    public partial ref TItem Add(TItem item);

    /// <summary>
    /// Adds the specified item to the collection, if an item with the same key does not
    /// already exist.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>true if the item was added to the collection, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial bool TryAdd(TItem item);

    /// <summary>
    /// Adds the specified item to the collection, if an item with the same key does not
    /// already exist. Returns the new item, or the existing item if the key exists.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>A reference to the item inside the collection, either the newly added or the existing.</returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial ref TItem GetOrAdd(TItem item);

    /// <summary>
    /// Adds the specified item to the collection, if an item with the same key does not
    /// already exist. Returns the new item, or the existing item if the key exists.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="added">Returns true if the item was added to the collection, otherwise false.</param>
    /// <returns>A reference to the item inside the collection, either the newly added or the existing.</returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial ref TItem GetOrAdd(TItem item, out bool added);

    /// <summary>
    /// Adds a new item to the collection, if an item with the same key does not already exist,
    /// by using the specified factory delegate.
    /// Returns the new item, or the existing item if the key exists.
    /// </summary>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem GetOrAdd(TKey key, Func<TKey, TItem> itemFactory);

    /// <summary>
    /// Adds a new item to the collection, if an item with the same key does not already exist,
    /// by using the specified factory delegate.
    /// Returns the new item, or the existing item if the key exists.
    /// </summary>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <param name="added">Returns true if an item was created and added in the collection, otherwise false.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem GetOrAdd(TKey key, Func<TKey, TItem> itemFactory, out bool added);

    /// <summary>
    /// Adds a new item to the collection, if an item with the same key does not already exist,
    /// by using the specified factory delegate and an argument.
    /// Returns the new item, or the existing item if the key exists.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <param name="factoryArgument">The argument to pass into itemFactory.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TItem> itemFactory, TArg factoryArgument);

    /// <summary>
    /// Adds a new item to the collection, if an item with the same key does not already exist,
    /// by using the specified factory delegate and an argument.
    /// Returns the new item, or the existing item if the key exists.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <param name="factoryArgument">The argument to pass into itemFactory.</param>
    /// <param name="added">Returns true if an item was created and added in the collection, otherwise false.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TItem> itemFactory, TArg factoryArgument, out bool added);

    /// <summary>
    /// Adds the specified item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">The item is null.</exception>
    public partial ref TItem AddOrReplace(TItem item);

    /// <summary>
    /// Adds the specified item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="replaced">Returns true if an item with the specified key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">The item is null.</exception>
    public partial ref TItem AddOrReplace(TItem item, out bool replaced);

    /// <summary>
    /// Adds the specified item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="replaced">Returns true if an item with the specified key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">
    /// Returns the existing item that was replaced by the new item, if an existing item was found.
    /// Otherwise returns the default value for the type.
    /// </param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">The item is null.</exception>
    public partial ref TItem AddOrReplace(TItem item, out bool replaced, out TItem originalItem);

    /// <summary>
    /// Adds an item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates.
    /// </summary>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing key, based on the existing item that will be replaced.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Either the key or any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem AddOrReplace(TKey key, Func<TKey, TItem> addItemFactory, Func<TKey, TItem, TItem> replaceItemFactory);

    /// <summary>
    /// Adds an item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates.
    /// </summary>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing key, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the specified key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Either the key or any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem AddOrReplace(TKey key, Func<TKey, TItem> addItemFactory, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced);

    /// <summary>
    /// Adds an item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates.
    /// </summary>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing key, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the specified key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">
    /// Returns the existing item that was replaced by the new item, if an existing item was found.
    /// Otherwise returns the default value for the type.
    /// </param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Either the key or any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem AddOrReplace(TKey key, Func<TKey, TItem> addItemFactory, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced, out TItem originalItem);

    /// <summary>
    /// Adds an item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing key, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Either the key or any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem AddOrReplace<TArg>(TKey key, Func<TKey, TArg, TItem> addItemFactory, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument);

    /// <summary>
    /// Adds an item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing key, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
    /// <param name="replaced">Returns true if an item with the specified key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Either the key or any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem AddOrReplace<TArg>(TKey key, Func<TKey, TArg, TItem> addItemFactory, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced);

    /// <summary>
    /// Adds an item to the collection, if an item with the same key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing key, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
    /// <param name="replaced">Returns true if an item with the specified key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">
    /// Returns the existing item that was replaced by the new item, if an existing item was found.
    /// Otherwise returns the default value for the type.
    /// </param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Either the key or any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem AddOrReplace<TArg>(TKey key, Func<TKey, TArg, TItem> addItemFactory, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, out TItem originalItem);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the same key is found.
    /// </summary>
    /// <param name="item">The new item with which to replace the existing item.</param>
    /// <returns>true if an item with the same key was found in the collection and replaced, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial bool TryReplace(TItem item);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the same key is found.
    /// </summary>
    /// <param name="item">The new item with which to replace the existing item.</param>
    /// <param name="replaced">Returns true if an item with the same key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial ref TItem TryReplace(TItem item, out bool replaced);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the same key is found.
    /// </summary>
    /// <param name="item">The new item with which to replace the existing item.</param>
    /// <param name="replaced">Returns true if an item with the same key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">Returns the existing item that was replaced by the new item, if an existing item was found. Otherwise returns the default value for the type.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">The item is null, or has a null key.</exception>
    public partial ref TItem TryReplace(TItem item, out bool replaced, out TItem originalItem);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified key is found, by using the specified factory delegate.
    /// </summary>
    /// <param name="key">The key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <returns>true if an item with the same key was found in the collection and replaced, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial bool TryReplace(TKey key, Func<TKey, TItem, TItem> replaceItemFactory);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified key is found, by using the specified factory delegate.
    /// </summary>
    /// <param name="key">The key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the same key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem TryReplace(TKey key, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified key is found, by using the specified factory delegate.
    /// </summary>
    /// <param name="key">The key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the same key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">Returns the existing item that was replaced by the new item, if an existing item was found. Otherwise returns the default value for the type.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem TryReplace(TKey key, Func<TKey, TItem, TItem> replaceItemFactory, out bool replaced, out TItem originalItem);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified key is found, by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="key">The key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
    /// <returns>true if an item with the same key was found in the collection and replaced, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial bool TryReplace<TArg>(TKey key, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified key is found, by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="key">The key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
    /// <param name="replaced">Returns true if an item with the same key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem TryReplace<TArg>(TKey key, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced);

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified key is found, by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="key">The key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
    /// <param name="replaced">Returns true if an item with the same key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">Returns the existing item that was replaced by the new item, if an existing item was found. Otherwise returns the default value for the type.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">Either the key or the factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different key than the original.
    /// </exception>
    public partial ref TItem TryReplace<TArg>(TKey key, Func<TKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, out TItem originalItem);

    /// <summary>
    /// Removes the item with the specified key from the collection.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns>true if the item is found and removed, otherwise false.</returns>
    public partial bool TryRemove(TKey key);

    /// <summary>
    /// Removes the item with the specified key from the collection.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <param name="removedItem">Returns the removed item if found, otherwise returns the default value for the type.</param>
    /// <returns>true if the item is found and removed, otherwise false.</returns>
    public partial bool TryRemove(TKey key, out TItem removedItem);

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public partial void Clear();

    /// <summary>
    /// Ensures that the collection can hold up to a specified number of items,
    /// without any further expansion of its backing storage.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The current capacity of the collection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The capacity is negative.</exception>
    public partial int EnsureCapacity(int capacity);

    /// <summary>
    /// Sets the capacity of the collection to hold up exactly the current number of items.
    /// </summary>
    public partial void TrimExcess();

    /// <summary>
    /// Sets the capacity of the collection to hold up the specified number of items,
    /// without any further expansion of its backing storage.
    /// </summary>
    /// <param name="capacity">The new capacity.</param>
    /// <exception cref="ArgumentOutOfRangeException">The capacity is less than the number of items in the collection.</exception>
    public partial void TrimExcess(int capacity);

    /// <summary>
    /// Copies the items of the collection to a new array.
    /// </summary>
    /// <returns>An array containing copies of the items of the collection.</returns>
    public partial TItem[] ToArray();

    /// <summary>
    /// Returns an enumerator that iterates through the collection,
    /// yielding the references of the items inside the collection.
    /// </summary>
    /// <returns>An enumerator that iterates through the collection.</returns>
    public partial Enumerator GetEnumerator();

    /// <summary>
    /// Gets an instance of a type that can be used to perform operations on the
    /// current collection, using a TAlternateKey as a key instead of a TKey.
    /// </summary>
    /// <typeparam name="TAlternateKey">The alternate type of a key for performing lookups.</typeparam>
    /// <param name="lookup">
    /// The created lookup instance when the method returns true, or a default instance
    /// that should not be used if the method returns false.
    /// </param>
    /// <returns>true if a lookup could be created, otherwise false.</returns>
    public partial bool TryGetAlternateLookup<TAlternateKey>(out AlternateLookup<TAlternateKey> lookup) where TAlternateKey : allows ref struct;

    /// <summary>
    /// Gets an instance of a type that can be used to perform operations on the
    /// current collection, using a TAlternateKey as a key instead of a TKey.
    /// </summary>
    /// <typeparam name="TAlternateKey">The alternate type of a key for performing lookups.</typeparam>
    /// <returns>The created lookup instance.</returns>
    /// <exception cref="InvalidOperationException">The collections's comparer is not compatible with TAlternateKey.</exception>
    public partial AlternateLookup<TAlternateKey> GetAlternateLookup<TAlternateKey>() where TAlternateKey : allows ref struct;

    /// <summary>
    /// Replaces all the items in the collection,
    /// by using the specified factory delegate.
    /// </summary>
    /// <param name="replaceItemFactory">The factory delegate that creates a new item, based on the existing item that will be replaced.</param>
    /// <exception cref="InvalidOperationException">The collection was modified during an invocation of the delegate.</exception>
    public partial void ReplaceAll(Func<TItem, TItem> replaceItemFactory);

    /// <summary>
    /// Replaces all the items in the collection,
    /// by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="replaceItemFactory">The factory delegate that creates a new item, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into both delegates.</param>
    /// <exception cref="InvalidOperationException">The collection was modified during an invocation of the delegate.</exception>
    public partial void ReplaceAll<TArg>(Func<TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument);

    /// <summary>
    /// Replaces all the items that are matched by the specified match delegate,
    /// by using the specified factory delegate.
    /// </summary>
    /// <param name="match">The delegate that matches the items to be replaced.</param>
    /// <param name="replaceItemFactory">The factory delegate that creates a new item, based on the existing item that will be replaced.</param>
    /// <returns>The number of items replaced inside the collection.</returns>
    /// <exception cref="InvalidOperationException">The collection was modified during an invocation of a delegate.</exception>
    public partial int ReplaceWhere(Func<TItem, bool> match, Func<TItem, TItem> replaceItemFactory);

    /// <summary>
    /// Replaces all the items that are matched by the specified match delegate,
    /// by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into both delegates.</typeparam>
    /// <param name="match">The delegate that matches the items to be replaced.</param>
    /// <param name="replaceItemFactory">The factory delegate that creates a new item, based on the existing item that will be replaced.</param>
    /// <param name="argument">The argument to pass into both delegates.</param>
    /// <returns>The number of items replaced inside the collection.</returns>
    /// <exception cref="InvalidOperationException">The collection was modified during an invocation of a delegate.</exception>
    public partial int ReplaceWhere<TArg>(Func<TItem, TArg, bool> match, Func<TItem, TArg, TItem> replaceItemFactory, TArg argument);

    /// <summary>
    /// Removes all the items that are matched by the specified delegate.
    /// </summary>
    /// <param name="match">The delegate that matches the items for removal.</param>
    /// <returns>The number of items removed from the collection.</returns>
    /// <remarks>The delegate is invoked in the order of the internal buckets, not in the order of the enumerator.</remarks>
    /// <exception cref="InvalidOperationException">The collection was modified during an invocation of the match delegate.</exception>
    public partial int RemoveWhere(Predicate<TItem> match);

    /// <summary>
    /// Removes all the items that are matched by the specified delegate,
    /// passing the same argument to all invocations of the delegate.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument to pass into the delegate.</typeparam>
    /// <param name="match">The delegate that matches the items to remove.</param>
    /// <param name="argument">The argument to pass into the delegate.</param>
    /// <returns>The number of items removed from the collection.</returns>
    /// <remarks>The delegate is invoked in the order of the internal buckets, not in the order of the enumerator.</remarks>
    /// <exception cref="InvalidOperationException">The collection was modified during an invocation of the match delegate.</exception>
    public partial int RemoveWhere<TArg>(Func<TItem, TArg, bool> match, TArg argument);

    /// <summary>
    /// Enumerates the items of the collection, yielding the references of the items.
    /// </summary>
    public ref partial struct Enumerator
    {
        /// <summary>Advances the enumerator to the next item of the collection.</summary>
        /// <returns>true if the enumerator was advanced to the next item, or false if the enumerator has passed the end of the collection.</returns>
        public partial bool MoveNext();

        /// <summary>
        /// Gets a reference to the item in the collection at the current position of the enumerator.
        /// </summary>
        public partial ref TItem Current { get; }

        /// <summary>
        /// Releases all resources used by the enumerator.
        /// </summary>
        public partial void Dispose();
    }
}
