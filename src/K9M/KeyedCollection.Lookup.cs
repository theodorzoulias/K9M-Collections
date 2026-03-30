using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace K9M;

// AlternateLookup implementation.
public partial class KeyedCollection<TKey, TItem>
{
    /// <summary>
    /// Provides a type that can be used to perform operations on a collection
    /// using a TAlternateKey as a key instead of a TKey.
    /// </summary>
    /// <typeparam name="TAlternateKey">The alternate key type.</typeparam>
    public readonly struct AlternateLookup<TAlternateKey> where TAlternateKey : notnull, allows ref struct
    {
        private readonly KeyedCollection<TKey, TItem> _parent;
        private readonly IAlternateEqualityComparer<TAlternateKey, TKey> _keyComparer;

        internal AlternateLookup(KeyedCollection<TKey, TItem> parent, IAlternateEqualityComparer<TAlternateKey, TKey> keyComparer)
        {
            _parent = parent;
            _keyComparer = keyComparer;
        }

        /// <summary>
        /// Gets the parent collection of this instance.
        /// </summary>
        public KeyedCollection<TKey, TItem> Parent => _parent;

        /// <summary>
        /// Gets the alternate equality comparer that is used to determine equality of
        /// alternate keys for the collection.
        /// </summary>
        /// <returns>
        /// The alternate equality comparer that is used to determine equality of
        /// alternate keys for the collection, and to provide hash values for the alternate keys.
        /// </returns>
        public IAlternateEqualityComparer<TAlternateKey, TKey> KeyComparer => _keyComparer;

        /// <summary>
        /// Determines whether the collection contains an item with the specified alternate key.
        /// </summary>
        /// <param name="key">The alternate key to locate in the collection.</param>
        /// <returns>true if the collection contains an item with the specified alternate key, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The alternate key is null.</exception>
        public bool ContainsKey(TAlternateKey key) => _parent.ContainsKey(_keyComparer, key);

        /// <summary>Gets the item with the specified alternate key.</summary>
        /// <param name="key">The alternate key of the item to get.</param>
        /// <returns>A reference to the item with the specified alternate key. An exception is thrown if an item with the specified alternate key is not found.</returns>
        /// <exception cref="ArgumentNullException">The alternate key is null.</exception>
        /// <exception cref="KeyNotFoundException">The given alternate key was not present in the collection.</exception>
        public ref TItem this[TAlternateKey key]
            => ref _parent.GetItem(_keyComparer, key);

        /// <summary>Gets the item with the specified alternate key.</summary>
        /// <param name="key">The alternate key of the item to get.</param>
        /// <param name="item">
        /// Returns the item with the specified alternate key, if the alternate key is found.
        /// Otherwise returns the default value for the type.
        /// </param>
        /// <returns>true if the collection contains an item with the specified alternate key, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">The alternate key is null.</exception>
        public bool TryGetItem(TAlternateKey key, [MaybeNullWhen(false)] out TItem item)
            => _parent.TryGetItem<TAlternateKey>(_keyComparer, key, out item);

        /// <summary>
        /// Gets a reference to the item with the specified alternate key, or a null managed pointer if
        /// the alternate key does not exist in the collection.
        /// </summary>
        /// <param name="key">The alternate key of the item to get.</param>
        /// <returns>Either a reference to the item with the specified alternate key, or a null managed pointer if the alternate key is not found.</returns>
        public ref TItem GetItemRef(TAlternateKey key)
            => ref _parent.GetItemRef(_keyComparer, key, out _);

        /// <summary>
        /// Gets a reference to the item with the specified alternate key, or a null managed pointer if
        /// the alternate key does not exist in the collection.
        /// </summary>
        /// <param name="key">The alternate key of the item to get.</param>
        /// <param name="exists">Returns true if an item with the specified alternate key was found in the collection, otherwise false.</param>
        /// <returns>Either a reference to the item with the specified alternate key, or a null managed pointer if the alternate key is not found.</returns>
        public ref TItem GetItemRef(TAlternateKey key, out bool exists)
            => ref _parent.GetItemRef(_keyComparer, key, out exists);

        /// <summary>
        /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
        /// by using the specified factory delegate.
        /// Returns the new item, or the existing item if the alternate key exists.
        /// </summary>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="itemFactory">The factory delegate used to create a new item.</param>
        /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem GetOrAdd(TAlternateKey key, Func<TAlternateKey, TItem> itemFactory)
            => ref _parent.GetOrAdd(_keyComparer, key, itemFactory, out _);

        /// <summary>
        /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
        /// by using the specified factory delegate.
        /// Returns the new item, or the existing item if the alternate key exists.
        /// </summary>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="itemFactory">The factory delegate used to create a new item.</param>
        /// <param name="added">Returns true if an item was created and added in the collection, otherwise false.</param>
        /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem GetOrAdd(TAlternateKey key, Func<TAlternateKey, TItem> itemFactory, out bool added)
            => ref _parent.GetOrAdd(_keyComparer, key, itemFactory, out added);

        /// <summary>
        /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
        /// by using the specified factory delegate and an argument.
        /// Returns the new item, or the existing item if the alternate key exists.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="itemFactory">The factory delegate used to create a new item.</param>
        /// <param name="factoryArgument">The argument to pass into itemFactory.</param>
        /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem GetOrAdd<TArg>(TAlternateKey key, Func<TAlternateKey, TArg, TItem> itemFactory, TArg factoryArgument)
            => ref _parent.GetOrAdd(_keyComparer, key, itemFactory, factoryArgument, out _);

        /// <summary>
        /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
        /// by using the specified factory delegate and an argument.
        /// Returns the new item, or the existing item if the alternate key exists.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="itemFactory">The factory delegate used to create a new item.</param>
        /// <param name="factoryArgument">The argument to pass into itemFactory.</param>
        /// <param name="added">Returns true if an item was created and added in the collection, otherwise false.</param>
        /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem GetOrAdd<TArg>(TAlternateKey key, Func<TAlternateKey, TArg, TItem> itemFactory, TArg factoryArgument, out bool added)
            => ref _parent.GetOrAdd(_keyComparer, key, itemFactory, factoryArgument, out added);

        /// <summary>
        /// Adds an item to the collection, if an item with the same alternate key does not
        /// already exist, or replaces the existing item. The new item is created by using one
        /// of the two specified factory delegates.
        /// </summary>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
        /// <returns>A reference to the new item inside the collection.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or any of the factory delegates is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of a factory delegate,
        /// or a factory delegate returned a null item,
        /// or a factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem AddOrReplace(TAlternateKey key, Func<TAlternateKey, TItem> addItemFactory, Func<TAlternateKey, TItem, TItem> replaceItemFactory)
            => ref _parent.AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, out _, out _);

        /// <summary>
        /// Adds an item to the collection, if an item with the same alternate key does not
        /// already exist, or replaces the existing item. The new item is created by using one
        /// of the two specified factory delegates.
        /// </summary>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
        /// <param name="replaced">Returns true if an item with the specified alternate key was found in the collection and replaced, otherwise false.</param>
        /// <returns>A reference to the new item inside the collection.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or any of the factory delegates is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of a factory delegate,
        /// or a factory delegate returned a null item,
        /// or a factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem AddOrReplace(TAlternateKey key, Func<TAlternateKey, TItem> addItemFactory, Func<TAlternateKey, TItem, TItem> replaceItemFactory, out bool replaced)
            => ref _parent.AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, out replaced, out _);

        /// <summary>
        /// Adds an item to the collection, if an item with the same alternate key does not
        /// already exist, or replaces the existing item. The new item is created by using one
        /// of the two specified factory delegates.
        /// </summary>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
        /// <param name="replaced">Returns true if an item with the specified alternate key was found in the collection and replaced, otherwise false.</param>
        /// <param name="originalItem">
        /// Returns the existing item that was replaced by the new item, if an existing item was found.
        /// Otherwise returns the default value for the type.
        /// </param>
        /// <returns>A reference to the new item inside the collection.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or any of the factory delegates is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of a factory delegate,
        /// or a factory delegate returned a null item,
        /// or a factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem AddOrReplace(TAlternateKey key, Func<TAlternateKey, TItem> addItemFactory, Func<TAlternateKey, TItem, TItem> replaceItemFactory, out bool replaced, out TItem originalItem)
            => ref _parent.AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, out replaced, out originalItem);

        /// <summary>
        /// Adds an item to the collection, if an item with the same alternate key does not
        /// already exist, or replaces the existing item. The new item is created by using one
        /// of the two specified factory delegates and an argument.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
        /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
        /// <returns>A reference to the new item inside the collection.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or any of the factory delegates is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of a factory delegate,
        /// or a factory delegate returned a null item,
        /// or a factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem AddOrReplace<TArg>(TAlternateKey key, Func<TAlternateKey, TArg, TItem> addItemFactory, Func<TAlternateKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument)
            => ref _parent.AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, factoryArgument, out _, out _);

        /// <summary>
        /// Adds an item to the collection, if an item with the same alternate key does not
        /// already exist, or replaces the existing item. The new item is created by using one
        /// of the two specified factory delegates and an argument.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
        /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
        /// <param name="replaced">Returns true if an item with the specified alternate key was found in the collection and replaced, otherwise false.</param>
        /// <returns>A reference to the new item inside the collection.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or any of the factory delegates is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of a factory delegate,
        /// or a factory delegate returned a null item,
        /// or a factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem AddOrReplace<TArg>(TAlternateKey key, Func<TAlternateKey, TArg, TItem> addItemFactory, Func<TAlternateKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced)
            => ref _parent.AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, factoryArgument, out replaced, out _);

        /// <summary>
        /// Adds an item to the collection, if an item with the same alternate key does not
        /// already exist, or replaces the existing item. The new item is created by using one
        /// of the two specified factory delegates and an argument.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
        /// <param name="key">The alternate key of the item to add.</param>
        /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
        /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
        /// <param name="replaced">Returns true if an item with the specified alternate key was found in the collection and replaced, otherwise false.</param>
        /// <param name="originalItem">
        /// Returns the existing item that was replaced by the new item, if an existing item was found.
        /// Otherwise returns the default value for the type.
        /// </param>
        /// <returns>A reference to the new item inside the collection.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or any of the factory delegates is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of a factory delegate,
        /// or a factory delegate returned a null item,
        /// or a factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem AddOrReplace<TArg>(TAlternateKey key, Func<TAlternateKey, TArg, TItem> addItemFactory, Func<TAlternateKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, out TItem originalItem)
            => ref _parent.AddOrReplace(_keyComparer, key, addItemFactory, replaceItemFactory, factoryArgument, out replaced, out originalItem);

        /// <summary>
        /// Replaces an existing item with a new item, if an existing item
        /// with the specified alternate key is found, by using the specified factory delegate.
        /// </summary>
        /// <param name="key">The alternate key of the item to replace.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
        /// <returns>true if an item with the same alternate key was found in the collection and replaced, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public bool TryReplace(TAlternateKey key, Func<TAlternateKey, TItem, TItem> replaceItemFactory)
            => !Unsafe.IsNullRef(ref _parent.TryReplace(_keyComparer, key, replaceItemFactory, out bool replaced, out _));

        /// <summary>
        /// Replaces an existing item with a new item, if an existing item
        /// with the specified alternate key is found, by using the specified factory delegate.
        /// </summary>
        /// <param name="key">The alternate key of the item to replace.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
        /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
        /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem TryReplace(TAlternateKey key, Func<TAlternateKey, TItem, TItem> replaceItemFactory, out bool replaced)
            => ref _parent.TryReplace(_keyComparer, key, replaceItemFactory, out replaced, out _);

        /// <summary>
        /// Replaces an existing item with a new item, if an existing item
        /// with the specified alternate key is found, by using the specified factory delegate.
        /// </summary>
        /// <param name="key">The alternate key of the item to replace.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
        /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
        /// <param name="originalItem">Returns the existing item that was replaced by the new item, if an existing item was found. Otherwise returns the default value for the type.</param>
        /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem TryReplace(TAlternateKey key, Func<TAlternateKey, TItem, TItem> replaceItemFactory, out bool replaced, out TItem originalItem)
            => ref _parent.TryReplace(_keyComparer, key, replaceItemFactory, out replaced, out originalItem);

        /// <summary>
        /// Replaces an existing item with a new item, if an existing item
        /// with the specified alternate key is found, by using the specified factory delegate and an argument.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
        /// <param name="key">The alternate key of the item to replace.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
        /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
        /// <returns>true if an item with the same alternate key was found in the collection and replaced, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public bool TryReplace<TArg>(TAlternateKey key, Func<TAlternateKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument)
            => !Unsafe.IsNullRef(ref _parent.TryReplace(_keyComparer, key, replaceItemFactory, factoryArgument, out bool replaced, out _));

        /// <summary>
        /// Replaces an existing item with a new item, if an existing item
        /// with the specified alternate key is found, by using the specified factory delegate and an argument.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
        /// <param name="key">The alternate key of the item to replace.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
        /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
        /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
        /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem TryReplace<TArg>(TAlternateKey key, Func<TAlternateKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced)
            => ref _parent.TryReplace(_keyComparer, key, replaceItemFactory, factoryArgument, out replaced, out _);

        /// <summary>
        /// Replaces an existing item with a new item, if an existing item
        /// with the specified alternate key is found, by using the specified factory delegate and an argument.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
        /// <param name="key">The alternate key of the item to replace.</param>
        /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
        /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
        /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
        /// <param name="originalItem">Returns the existing item that was replaced by the new item, if an existing item was found. Otherwise returns the default value for the type.</param>
        /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
        /// <exception cref="ArgumentNullException">Either the alternate key or the factory delegate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified during the invocation of the factory delegate,
        /// or the factory delegate returned a null item,
        /// or the factory delegate produced an item with different alternate key than the original.
        /// </exception>
        public ref TItem TryReplace<TArg>(TAlternateKey key, Func<TAlternateKey, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, out TItem originalItem)
            => ref _parent.TryReplace(_keyComparer, key, replaceItemFactory, factoryArgument, out replaced, out originalItem);

        /// <summary>
        /// Removes the item with the specified alternate key from the collection.
        /// </summary>
        /// <param name="key">The alternate key of the item to remove.</param>
        /// <returns>true if the item is found and removed, otherwise false.</returns>
        public bool TryRemove(TAlternateKey key)
            => _parent.TryRemove(_keyComparer, key, out _);

        /// <summary>
        /// Removes the item with the specified alternate key from the collection.
        /// </summary>
        /// <param name="key">The alternate key of the item to remove.</param>
        /// <param name="removedItem">Returns the removed item if found, otherwise returns the default value for the type.</param>
        /// <returns>true if the item is found and removed, otherwise false.</returns>
        public bool TryRemove(TAlternateKey key, [MaybeNullWhen(false)] out TItem removedItem)
            => _parent.TryRemove(_keyComparer, key, out removedItem);
    }
}
