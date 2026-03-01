![Logo](Logo.png)

# K9M-Collections

[![Nuget](https://img.shields.io/nuget/v/K9M-Collections)](https://www.nuget.org/packages/K9M-Collections)

This .NET 10 repository contains the `K9M.KeyedCollection<TKey,TItem>` class, which is
a dictionary with the keys embedded in the items.
This collection is tailored for items that are structs, either immutable or mutable.
It consumes the same memory per item as a .NET
[`HashSet<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1),
which is less than a .NET
[`Dictionary<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2),
because the keys are stored only once.
It provides the tools needed for efficient in-place mutations of the items, with minimal hashings of the keys.
It is implemented as a hashtable, similar to the .NET `Dictionary<K,V>` and `HashSet<T>`, with
similar performance characteristics. After inserting an item in the collection, its key should be immutable.
Every other property of the item is allowed to change, except the key.

## How it differs from the `System.Collections.ObjectModel.KeyedCollection<TKey,TItem>`?

The native [`ObjectModel.KeyedCollection<TKey,TItem>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.keyedcollection-2)
collection is an abstract class. You have to derive a new class from it, and override the
[`GetKeyForItem`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.keyedcollection-2.getkeyforitem)
abstract method. On the contrary the `K9M.KeyedCollection<TKey,TItem>` can be instantiated directly,
providing the `keySelector` function in the constructor:

```C#
K9M.KeyedCollection<string, Item> collection = new(x => x.Key);
```

So the `ObjectModel.KeyedCollection<K,I>` is intended as a base class for
building publicly exposed collections, while the `K9M.KeyedCollection<K,I>` is intended
as an internal component that should be encapsulated, and not exposed to clients directly.

## How it differs from the standard `Dictionary<TKey,TValue>`?

The [`Dictionary<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)
is a general purpose collection that emphasizes safety and speed.
It does allow limited direct access to its internal backing storage,
but this functionality is deliberately hidden in the
`System.Runtime.InteropServices` namespace (the
[`CollectionsMarshal`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.collectionsmarshal) class).
On the contrary the `K9M.KeyedCollection<K,I>` exposes references directly from most of its members.
It assumes that the developer knows what he is doing, and will be careful enough to not shoot himself in the foot. Here is
an example of mutating a `struct` stored inside a `Dictionary<K,V>`, and then inside a `KeyedCollection<K,I>`:

```C#
CollectionsMarshal.GetValueRefOrNullRef(dictionary, "Key").Counter += 1;
```

```C#
collection["Key"].Counter += 1;
```

The `K9M.KeyedCollection<K,I>` includes many APIs that allow efficient mutations of its
contents, with minimal hashing of the keys. For example `GetOrAdd`, `AddOrReplace`, `TryReplace`, `ReplaceAll` etc.
Some of these APIs can be implemented as extension methods on the `Dictionary<K,V>` using the
`CollectionsMarshal`, but not all. For example it's not possible to mutate all the values in a
`Dictionary<K,V>` without hashing all the keys (at least not in .NET 10). This is trivial with a `K9M.KeyedCollection<K,I>`,
because it has an `Enumerator` that yields direct references to the items inside the collection:

```C#
foreach (ref var item in collection) item.Counter += 1;
```

## How it differs from the standard `HashSet<T>`?

The [`HashSet<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1)
collection can be provided with a custom comparer in the constructor, that compares for equality
only the keys of the contained elements. So it can be used as a keyed collection,
that consumes less memory than the `Dictionary<K,V>` because the keys are stored in one place.
It is quite inconvenient though, because
searching for a key requires to construct a dummy element that has only the key initialized.
If the `T`s are classes, these dummy elements will have to be allocated, adding pressure to the
garbage collector. The `HashSet<T>` also lacks any `CollectionsMarshal` support, i.e. direct access
to its backing storage is unavailable. This means that mutating an element requires three
hashings of the key. One hashing for retrieving the element, another one for removing the existing element,
and a third one for inserting again the mutated element. The `K9M.KeyedCollection<K,I>` is
a dedicated storage for items with embedded keys, with the same memory footprint as a `HashSet<T>`,
and none of its limitations.

## What is the advantage of using structs over classes?

When the `TItem` is a `struct`, the data of all the items in the collection are stored
directly in a single compact array.
So for example if each `TItem` weighs 16 bytes and there are 1,000 of them,
the total memory used for the data is 16,000 bytes. When the `TItem` is a `class`, the objects
are stored one by one individually in the heap, and the collection contains pointers
to those objects. So the memory used increases to 24,000, because beyond the data there is
also a dead weight of 1,000 x 8 bytes of pointers.

## What is the advantage of using classes over structs?

Classes are far more convenient and safer than structs. When you want to mutate an instance of a
class, you don't have much to worry about. You just mutate it, and the mutation persists in memory.
When you want to mutate a struct, you have to think carefully about whether you have a copy of the struct,
or a reference to its underlying location. If you are working with a copy, any mutation will be local
to this variable, and won't affect the underlying location where the variable was copied from.
So working with structs is significantly more challenging, and error-prone.
This repository does not exist to promote liberal use of structs everywhere.
It exists to enable the use of structs in restricted isolated environments where it makes sense.
When and where it makes sense is up to the judgement of the developer.

## Usage examples

Let's say that we have a list of file paths, we want to associate each path with the information
if the file exists on the disk, and we want to make efficient lookups and mutations on this data.
We can create a `K9M.KeyedCollection<TKey,TItem>` where the `TKey` is a `string`, and the `TItem` is
a `(string, bool)` (a [value tuple with two members](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple-2)).
Here is the instantiation of the collection:

```C#
K9M.KeyedCollection<string, (string Path, bool Exists)> collection = new(x => x.Path,
    StringComparer.OrdinalIgnoreCase);
```

Adding an item:

```C#
collection.Add((@"C:\MyFile.txt", true));
```

Retrieving an item:

```C#
if (collection.TryGetItem(@"C:\MyFile.txt", out var item))
{
    Console.WriteLine(item.Exists);
};
```

Removing an item:

```C#
collection.TryRemove(@"C:\MyFile.txt");
```

The `Add` throws an exception if the path already exists. To avoid this we can use the `TryAdd`:

```C#
collection.TryAdd((@"C:\MyFile.txt", true));
```

The `TryAdd` won't affect the collection if the path already exists. If we want to overwrite
the existing information about the file, we can use the `AddOrReplace`.
This API is equivalent to removing the old item, and then adding the replacement in a single move:

```C#
collection.AddOrReplace((@"C:\MyFile.txt", true));
```

Another option is to use the `GetOrAdd`, that either gets the existing item or adds a new, and
then mutate the returned value:

```C#
collection.GetOrAdd((@"C:\MyFile.txt", true)).Exists = true;
```

This works because the `GetOrAdd` returns a reference to the item inside the backing storage
of the collection, not a copy of the item. This is more obvious if we split the above line in two:

```C#
ref var item = ref collection.GetOrAdd((@"C:\MyFile.txt", true));
item.Exists = true;
```

But what if the `TItem` was an immutable struct, and we couldn't mutate it after retrieving it?
In that case we could use the `AddOrReplace` overload with the
two delegates `addItemFactory` and `replaceItemFactory`,
one of which will be invoked depending on whether an item with the same key exists or not:

```C#
collection.AddOrReplace(@"C:\MyFile.txt", k => (k, true),
    (k, existing) => existing with { Exists = true });
```

There is also a similar `TryReplace` method that has a single `replaceItemFactory` delegate,
and does nothing if the key is not found in the collection.

```C#
collection.TryReplace(@"C:\MyFile.txt",
    (k, existing) => existing with { Exists = true });
```

Now we want to update the collection en masse, and change the `Exists` of all items to `true`:

```C#
foreach (ref var item in collection) item.Exists = true;
```

Notice the `ref` in the `foreach` syntax. If we omit the `ref`, the C# 14 compiler will complain:
*"Cannot modify members of 'item' because it is a 'foreach iteration variable'"*.

Another way to update all the items is by replacing them with the `ReplaceAll` method:

```C#
collection.ReplaceAll(x => x with { Exists = true });
```

The `replaceItemFactory` must return an item with the same key as the original, otherwise an
exception is thrown.

## What about the `GetAlternateLookup`?

The [`GetAlternateLookup`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.getalternatelookup)
API was added in .NET 9 to the `Dictionary<K,V>` and other generic collections,
and allows to perform operations on the collection using a different type of key than
the `TKey`. This API is typically used when the `TKey` is `string`, and the desirable
alternate key is [`ReadOnlySpan<char>`](https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1).
The purpose is to avoid the allocation of strings used solely for querying the collection.
The `K9M.KeyedCollection<K,I>` collection includes the same functionality, in the same shape. Example:

```C#
private readonly K9M.KeyedCollection<string, string> _fileNames = new(x => x);

private bool FileNameExists(string filePath)
{
    var lookup = _fileNames.GetAlternateLookup<ReadOnlySpan<char>>();
    ReadOnlySpan<char> fileNameSpan = Path.GetFileName(filePath.AsSpan());
    return lookup.ContainsKey(fileNameSpan);
}
```

Moreover there are extension methods for the `K9M.KeyedCollection<string,TItem>` type,
that allow using `ReadOnlySpan<char>` keys directly on the collection. These extensions
are enabled by importing the `K9M.Span` namespace in the code file. So the above example
can be written equivalently like this:

```C#
using K9M.Span;

private readonly K9M.KeyedCollection<string, string> _fileNames = new(x => x);

private bool FileNameExists(string filePath)
{
    ReadOnlySpan<char> fileNameSpan = Path.GetFileName(filePath.AsSpan());
    return _fileNames.ContainsKey(fileNameSpan);
}
```

One peculiarity is the `ReadOnlySpan<char>` extension member for the collection's indexer. Since extension
indexers [didn't make it into C# 14](https://github.com/dotnet/roslyn/issues/80312), this member is available as a method, the `GetItem` extension method.

## Any APIs missing?

The `K9M.KeyedCollection<TKey,TItem>` collection does not include the
[`Keys`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.keys) property
of the `Dictionary<TKey,TValue>`. This property could be useful, but since the
keys are embedded in the items I decided not to include it. It can be implemented quite
easily as an extension property if needed.

```C#
extension<TKey, TItem>(K9M.KeyedCollection<TKey, TItem> source)
{
    public IEnumerable<TKey> Keys => source.Select(x => source.KeySelector(x));
}
```

## Any behavioral difference?

No. The `K9M.KeyedCollection<TKey,TItem>` allows to remove items during the enumeration just
like the standard `Dictionary<TKey,TValue>`. So this code behaves as expected, without throwing
any errors:

```C#
foreach (var item in collection)
    if (SomeCondition(item))
        collection.TryRemove(item.Key);
```

There is an alternative way to remove multiple items though, the `RemoveWhere` method:

```C#
collection.RemoveWhere(x => SomeCondition(x));
```

This is also more efficient, because it doesn't involve hashing of the keys.

The `K9M.KeyedCollection<TKey,TItem>` also does not permit items with `null` keys.

## What about performance?

Most operations on the `K9M.KeyedCollection<TKey,TItem>` collection are about 30-40% slower than
on the `Dictionary<TKey,TValue>`. This is because the `Dictionary<K,V>` is heavily optimized
for speed, and the `K9M.KeyedCollection<K,I>` is not. This repository places a lot of emphasis on
features, code maintainability and correctness.
Contrary to the `Dictionary<K,V>`, all APIs use the
same internal function for finding the entry that holds the key. I.e. code repetition has been kept
deliberately to a minimum. So if maximizing speed is more important than reducing
the memory footprint, the standard `Dictionary<K,V>` is still a better choice.

## Support for .NET versions older than .NET 10?

This repository can't be compiled on older .NET versions, without losing big chunks of its functionality.
It uses many features, like
[extension members](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods),
that became available in C# 14.

## License

This repository is licensed with the MIT License.
