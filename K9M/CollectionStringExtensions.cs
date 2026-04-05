using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace K9M.Span;

/// <summary>
/// Provides extension methods for collections with string keys in this package.
/// </summary>
public static class CollectionStringExtensions
{
    /// <summary>
    /// Determines whether the collection contains an item with the specified alternate key.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key to locate in the collection.</param>
    /// <returns>true if the collection contains an item with the specified alternate key, otherwise false.</returns>
    public static bool ContainsKey<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetAlternateLookup<ReadOnlySpan<char>>().ContainsKey(key);
    }

    /// <summary>Gets the item with the specified alternate key.</summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to get.</param>
    /// <returns>A reference to the item with the specified alternate key. An exception is thrown if an item with the specified alternate key is not found.</returns>
    /// <exception cref="KeyNotFoundException">The given alternate key was not present in the collection.</exception>
    /// <remarks>
    /// This extension method corresponds to the indexer of the AlternateLookup type. Extension indexers <a href="https://github.com/dotnet/roslyn/issues/80312">didn't make it</a> for C# 14.
    /// </remarks>
    public static ref TItem GetItem<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>()[key];
    }

    /// <summary>Gets the item with the specified alternate key.</summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to get.</param>
    /// <param name="item">
    /// Returns the item with the specified alternate key, if the alternate key is found.
    /// Otherwise returns the default value for the type.
    /// </param>
    /// <returns>true if the collection contains an item with the specified alternate key, otherwise false.</returns>
    public static bool TryGetItem<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TItem item)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetAlternateLookup<ReadOnlySpan<char>>().TryGetItem(key, out item);
    }

    /// <summary>
    /// Gets a reference to the item with the specified alternate key, or a null managed pointer if
    /// the alternate key does not exist in the collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to get.</param>
    /// <returns>Either a reference to the item with the specified alternate key, or a null managed pointer if the alternate key is not found.</returns>
    public static ref TItem GetItemRef<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().GetItemRef(key);
    }

    /// <summary>
    /// Gets a reference to the item with the specified alternate key, or a null managed pointer if
    /// the alternate key does not exist in the collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to get.</param>
    /// <param name="exists">Returns true if an item with the specified alternate key was found in the collection, otherwise false.</param>
    /// <returns>Either a reference to the item with the specified alternate key, or a null managed pointer if the alternate key is not found.</returns>
    public static ref TItem GetItemRef<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, out bool exists)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().GetItemRef(key, out exists);
    }


    /// <summary>
    /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
    /// by using the specified factory delegate.
    /// Returns the new item, or the existing item if the alternate key exists.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem GetOrAdd<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem> itemFactory)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().GetOrAdd(key, itemFactory);
    }

    /// <summary>
    /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
    /// by using the specified factory delegate.
    /// Returns the new item, or the existing item if the alternate key exists.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <param name="added">Returns true if an item was created and added in the collection, otherwise false.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem GetOrAdd<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem> itemFactory, out bool added)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().GetOrAdd(key, itemFactory, out added);
    }

    /// <summary>
    /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
    /// by using the specified factory delegate and an argument.
    /// Returns the new item, or the existing item if the alternate key exists.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <param name="factoryArgument">The argument to pass into itemFactory.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem GetOrAdd<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TArg, TItem> itemFactory, TArg factoryArgument)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().GetOrAdd(key, itemFactory, factoryArgument);
    }

    /// <summary>
    /// Adds a new item to the collection, if an item with the same alternate key does not already exist,
    /// by using the specified factory delegate and an argument.
    /// Returns the new item, or the existing item if the alternate key exists.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="itemFactory">The factory delegate used to create a new item.</param>
    /// <param name="factoryArgument">The argument to pass into itemFactory.</param>
    /// <param name="added">Returns true if an item was created and added in the collection, otherwise false.</param>
    /// <returns>A reference to the item inside the collection, either the newly created or the already existing.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem GetOrAdd<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TArg, TItem> itemFactory, TArg factoryArgument, out bool added)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().GetOrAdd(key, itemFactory, factoryArgument, out added);
    }

    /// <summary>
    /// Adds an item to the collection, if an item with the same alternate key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem AddOrReplace<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem> addItemFactory, Func<ReadOnlySpan<char>, TItem, TItem> replaceItemFactory)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().AddOrReplace(key, addItemFactory, replaceItemFactory);
    }

    /// <summary>
    /// Adds an item to the collection, if an item with the same alternate key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the specified alternate key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem AddOrReplace<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem> addItemFactory, Func<ReadOnlySpan<char>, TItem, TItem> replaceItemFactory, out bool replaced)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().AddOrReplace(key, addItemFactory, replaceItemFactory, out replaced);
    }

    /// <summary>
    /// Adds an item to the collection, if an item with the same alternate key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the specified alternate key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">
    /// Returns the existing item that was replaced by the new item, if an existing item was found.
    /// Otherwise returns the default value for the type.
    /// </param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem AddOrReplace<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem> addItemFactory, Func<ReadOnlySpan<char>, TItem, TItem> replaceItemFactory, out bool replaced, [MaybeNull] out TItem originalItem)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().AddOrReplace(key, addItemFactory, replaceItemFactory, out replaced, out originalItem);
    }

    /// <summary>
    /// Adds an item to the collection, if an item with the same alternate key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates and an argument.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem AddOrReplace<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TArg, TItem> addItemFactory, Func<ReadOnlySpan<char>, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().AddOrReplace(key, addItemFactory, replaceItemFactory, factoryArgument);
    }

    /// <summary>
    /// Adds an item to the collection, if an item with the same alternate key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates and an argument.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to add.</param>
    /// <param name="addItemFactory">The factory delegate used to create a new item for an absent alternate key.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item for an existing alternate key, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into the addItemFactory and replaceItemFactory.</param>
    /// <param name="replaced">Returns true if an item with the specified alternate key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection.</returns>
    /// <exception cref="ArgumentNullException">Any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem AddOrReplace<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TArg, TItem> addItemFactory, Func<ReadOnlySpan<char>, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().AddOrReplace(key, addItemFactory, replaceItemFactory, factoryArgument, out replaced);
    }

    /// <summary>
    /// Adds an item to the collection, if an item with the same alternate key does not
    /// already exist, or replaces the existing item. The new item is created by using one
    /// of the two specified factory delegates and an argument.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegates.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
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
    /// <exception cref="ArgumentNullException">Any of the factory delegates is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of a factory delegate,
    /// or a factory delegate returned a null item,
    /// or a factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem AddOrReplace<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TArg, TItem> addItemFactory, Func<ReadOnlySpan<char>, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, [MaybeNull] out TItem originalItem)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().AddOrReplace(key, addItemFactory, replaceItemFactory, factoryArgument, out replaced, out originalItem);
    }

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified alternate key is found, by using the specified factory delegate.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <returns>true if an item with the same alternate key was found in the collection and replaced, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static bool TryReplace<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem, TItem> replaceItemFactory)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetAlternateLookup<ReadOnlySpan<char>>().TryReplace(key, replaceItemFactory);
    }

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified alternate key is found, by using the specified factory delegate.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem TryReplace<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem, TItem> replaceItemFactory, out bool replaced)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().TryReplace(key, replaceItemFactory, out replaced);
    }

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified alternate key is found, by using the specified factory delegate.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">Returns the existing item that was replaced by the new item, if an existing item was found. Otherwise returns the default value for the type.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem TryReplace<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem, TItem> replaceItemFactory, out bool replaced, [MaybeNull] out TItem originalItem)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().TryReplace(key, replaceItemFactory, out replaced, out originalItem);
    }

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified alternate key is found, by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
    /// <returns>true if an item with the same alternate key was found in the collection and replaced, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static bool TryReplace<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetAlternateLookup<ReadOnlySpan<char>>().TryReplace(key, replaceItemFactory, factoryArgument);
    }

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified alternate key is found, by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
    /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem TryReplace<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().TryReplace(key, replaceItemFactory, factoryArgument, out replaced);
    }

    /// <summary>
    /// Replaces an existing item with a new item, if an existing item
    /// with the specified alternate key is found, by using the specified factory delegate and an argument.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <typeparam name="TArg">The type of the argument to pass into the factory delegate.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to replace.</param>
    /// <param name="replaceItemFactory">The factory delegate used to create a new item, based on the existing item that will be replaced.</param>
    /// <param name="factoryArgument">The argument to pass into factory delegate.</param>
    /// <param name="replaced">Returns true if an item with the same alternate key was found in the collection and replaced, otherwise false.</param>
    /// <param name="originalItem">Returns the existing item that was replaced by the new item, if an existing item was found. Otherwise returns the default value for the type.</param>
    /// <returns>A reference to the new item inside the collection, or a null managed pointer if no existing item was found.</returns>
    /// <exception cref="ArgumentNullException">The factory delegate is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The collection was modified during the invocation of the factory delegate,
    /// or the factory delegate returned a null item,
    /// or the factory delegate produced an item with different alternate key than the original.
    /// </exception>
    public static ref TItem TryReplace<TItem, TArg>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, Func<ReadOnlySpan<char>, TItem, TArg, TItem> replaceItemFactory, TArg factoryArgument, out bool replaced, [MaybeNull] out TItem originalItem)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return ref source.GetAlternateLookup<ReadOnlySpan<char>>().TryReplace(key, replaceItemFactory, factoryArgument, out replaced, out originalItem);
    }

    /// <summary>
    /// Removes the item with the specified alternate key from the collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to remove.</param>
    /// <returns>true if the item is found and removed, otherwise false.</returns>
    public static bool TryRemove<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetAlternateLookup<ReadOnlySpan<char>>().TryRemove(key);
    }

    /// <summary>
    /// Removes the item with the specified alternate key from the collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    /// <param name="source">A collection with keys of type string.</param>
    /// <param name="key">The alternate key of the item to remove.</param>
    /// <param name="removedItem">Returns the removed item if found, otherwise returns the default value for the type.</param>
    /// <returns>true if the item is found and removed, otherwise false.</returns>
    public static bool TryRemove<TItem>(this KeyedCollection<string, TItem> source, ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TItem removedItem)
        where TItem : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetAlternateLookup<ReadOnlySpan<char>>().TryRemove(key, out removedItem);
    }
}
