# The `ValueList<T>` collection

The `K9M.ValueList<T>` collection is an alternative to the standard
[`System.Collections.Generic.List<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1).
It comes with rich support
for struct `T` elements, and it's a struct itself. Similarly to the `List<T>` it is
implemented as an expandable array of `T` elements, with a private `_count` field that
marks the populated portion of the `T[]` array. The standard `List<T>` exposes its
internal array as a [`Span<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.span-1),
but this functionality is deliberately hidden in the
`System.Runtime.InteropServices` namespace. The
[`CollectionsMarshal.AsSpan()`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.collectionsmarshal.asspan)
static method returns a `Span<T>` representation of the list's backing array, and the
[`CollectionsMarshal.SetCount`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.collectionsmarshal.setcount)
static method allows to set the `Count` of the list to any positive value.
The `ValueList<T>` supports the same functionality, in the same shape.
Contrary to the `List<T>`, these methods are first class citizens.
For example to reverse the contents of a `ValueList<int>` I can do this:

```C#
ValueList<int> list = new() { 1, 2, 3 };
list.AsSpan().Reverse();
Console.WriteLine(String.Join(", ", list)); // Prints 3, 2, 1
```

...and to increase the size of a `ValueList<char>`, and fill the revealed space with asterisks,
I can do this:

```C#
ValueList<char> list = new() { 'a', 'b', 'c' };
list.SetCount(10, '*');
Console.WriteLine(String.Join(", ", list)); // Prints a, b, c, *, *, *, *, *, *, *
```

In addition to the `AsSpan` and `SetCount`, many APIs of the `ValueList<T>` collection return
direct references to slots inside the internal array. For example the `GetItemRef()` method
returns a reference to a `T` by index, allowing to manipulate directly the memory location of a
specific `T` element within the array. So to increment the first element of a `ValueList<int>`
I can do this:

```C#
ValueList<int> list = new() { 10, 10, 10 };
ref int item = ref list.GetItemRef(0);
item++;
Console.WriteLine(String.Join(", ", list)); // Prints 11, 10, 10
```

To achieve the same effect with a `List<T>`, I have to access the indexer twice:

```C#
list[0] = list[0] + 1;
```

...incurring twice the cost of the index validation and the array dereferencing.
I can avoid the double access by using the `CollectionsMarshal.AsSpan()` API, but it's a bit
verbose and inconvenient:

```C#
Span<int> span = CollectionsMarshal.AsSpan(list);
ref int item = ref span[0];
item++;
```

For the sake of clarity I've shown above the verbose increment syntax. The increment
can be coded more succinctly without the explicit `ref int item` variable:

```C#
// ValueList<int>
list.GetItemRef(0)++;

// List<int>
CollectionsMarshal.AsSpan(list)[0]++;
```

You might think that `list[0]++` is the same, but it's not. This succinct syntax
performs double access on the list. It is equivalent to `list[0] = list[0] + 1`.

## But why is it a value type?

The reason that the `ValueList<T>` has been implemented as a `struct` is to avoid
allocating an object until it's actually needed. If you delve into the  source code of the
.NET standard libraries, you'll see occasionally code like this:

```C#
if (someCondition)
{
    list ??= new();
    list.Add(someElement);
}
```

The `list` is not initialized, until there is something to add in the list. The `ValueList<T>`
is a `struct`, so it doesn't need explicit initialization. I can declare
a `ValueList<T>` variable or field as `default`, and then immediately add things in it.
The is no risk that my code will crash with a `NullReferenceException`:

```C#
ValueList<int> list = default;
list.Add(1); // Valid
```

The `list.Add(1)` line above is where the internal `T[]` array of the collection
is initialized. Until this point the internal array is `null`.

## How to tell if the backing array is `null` or empty?

You can check at any time whether the internal array is `null` by comparing the list
with the `default`:

```C#
ValueList<int> list = default;
Console.WriteLine(list == default); // Prints true
list.Add(1);
Console.WriteLine(list == default); // Prints false
Console.WriteLine(list.Count); // Prints 1
```

All public constructors return `ValueList<T>` instances backed by a non-`null` array, usually
the empty array singleton
[`Array.Empty<T>()`](https://learn.microsoft.com/en-us/dotnet/api/system.array.empty).
There are two ways to get a `ValueList<T>` with `null` backing array.
Assigning the `default`, or using the `static` factory method
`ValueList.FromArray()` with `null` argument:

- `ValueList<int> list = default;`
- `ValueList<int> list = ValueList.FromArray<int>(null);`

The backing array transitions from `null` to not-`null` when the first item is added to the
collection. Afterwards it never transitions back to `null`. The dinstinction between
`list == default` and `list != default` can be used reliably as state of a program.

## What if I want to store a `ValueList<T>` in the heap?

You can store a `ValueList<T>` in the heap by encapsulating it inside your own classes, as
a non-[`readonly`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/readonly)
field. The field **should not be readonly**, because the struct is mutable.
The `ValueList<T>` collection is intended as an internal component, not as something to expose to
clients directly. So normally you'll have already some class to box the `ValueList<T>`
inside. If not, the `ValueList<T>` comes with its own wrapper. You can wrap a `ValueList<T>`
inside a box like this:

```C#
ValueList<int>.Box boxed = new();
boxed.List.Add(13); // Adds an element in the boxed list.
Console.WriteLine(boxed.List.Count); // Prints 1
```

The `ValueList<T>.Box` is a reference type (a `class`), so it can be moved around freely just like a standard `List<T>`.

## Any APIs missing?

Yes, there are many APIs that the `List<T>` has and the `ValueList<T>` has not.
Most of these APIs have been omitted because they offer functionality already available in the `Span<T>` type.
As an example the `ValueList<T>` is not equipped with an `IndexOf()` method, but the index of a
`T `value can be found with the
[`Span<T>.IndexOf()`](https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions.indexof)
method. Example:

```C#
ValueList<int> list = new() { 1, 2, 3, 4, 5 };
Console.WriteLine(list.AsSpan().IndexOf(3)); // Prints 2
```

As a bonus the `Span<T>.IndexOf()` method has an overload with an optional
`IEqualityComparer<T>` parameter, that the `List<T>` lacks. And there are lots of other similar APIs
like the `IndexOfAny`, `IndexOfAnyInRange` etc.

## Any behavioral difference?

Yes, the `List<T>` is equipped with a
[versioning mechanism](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs#L27),
that reacts to incorrect
usages of the collection by throwing `InvalidOperationException`s. On the contrary the `ValueList<T>`
has no `_version` internal field, so no checking takes place. For example if you
add or remove elements from the collection during an enumeration, no exception is going to
be thrown, but the result of the enumeration might not be what you expect. In general
it is assumed that the `ValueList<T>` is going to be used by careful developers
who know what they are doing, and will avoid shooting in their own foot while using
this powerful component.

It should be noted that Microsoft is thinking about removing the versioning mechanism from all
standard .NET collections as well. Quoting from
[this relevant](https://github.com/dotnet/runtime/issues/81523 "Reconsider version checks in collections?") GitHub issue:

> I'm opening this issue to gauge folks' appetite for removing these versions. Do they still provide good enough value that they're worth keeping? Have they outlived their usefulness?

## How is the collection enumerated?

There are three ways to enumerate the collection, each with its own advantages and limitations.
The first way is to `foreach` the collection directly, which uses a
[`Span<T>.Enumerator`](https://learn.microsoft.com/en-us/dotnet/api/system.span-1.enumerator).
The advantage of this approach is that it allows to manipulate the elements of the collection
directly, because this enumerator is a
[`ref struct`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct):

```C#
ValueList<int> list = new() { 1, 2, 3 };
foreach (ref int item in list) item += 10;
Console.WriteLine(String.Join(", ", list)); // Prints 11, 12, 13
```

Notice the `ref` in the `foreach` syntax. The disadvantage is that it can't be used inside `async` methods or [iterators](https://learn.microsoft.com/en-us/dotnet/csharp/iterators), because
`ref struct`s can't be preserved across `await`/`yield` boundaries. For this reason
the `ValueList<T>` comes with an `AsEnumerable()` method, that returns an
[`ArraySegment<T>.Enumerator`](https://learn.microsoft.com/en-us/dotnet/api/system.arraysegment-1.enumerator),
which is a `struct` but not `ref`.
This one can be preserved across `await`/`yield` boundaries just fine:

```C#
ValueList<int> list = new() { 1, 2, 3 };
foreach (int item in list.AsEnumerable())
    yield return item; // Compiles and works as expected
```

Finally the collection implements implicitly the `IEnumerable<T>` interface, allowing to
be enumerated from LINQ operators like `Select`/`Where` etc. The downside is that each enumeration
causes the allocation of a new `IEnumerable<T>` object, so it's not allocation-free.

When the collection is mutated during an enumeration, all three above enumeration methods behave the same.
A `Span<T>` or `ArraySegment<T>` representation of the
internal array is captured when the enumerator is acquired with the `GetEnumerator()` or `AsEnumerable()` method.
Subsequently the elements in this representation are yielded
one by one. Any changes inside the captured portion of the backing array will be reflected on the enumerated values.
In case the `ValueList<T>` needs to replace its backing array because it grew or shrunk,
any active enumerators will continue enumerating the representation of the discarded array.

## What about performance?

I haven't measured it, but I expect it to be slightly slower than the standard `List<T>`, or
slightly faster because the versioning is missing, depending on the usage. The performance
is not critically important for the project that I plan to use this collection,
so I didn't bother running a benchmark.

## Any caveats?

It has been said by many authors that mutable structs are evil (usually quoting
[this](https://ericlippert.com/2008/05/14/mutating-readonly-structs/ "Mutating readonly structs")
blog post by Eric Lippert). One way to experience this evil is by
creating a nullable `ValueList<T>?`:

```
ValueList<int>? list = new();
list?.Add(13);
Console.WriteLine(list?.Count); // Prints 0!
```

The [`Nullable<T>.Value`](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1.value)
member is a property, not a field. Every time you read this property you get a copy of the
internal list, not the list itself. So the `list?.Add(13)` line above affected only the copy.
The list inside the `Nullable<T>` was not mutated. But it can get worse:

```
ValueList<int>? list = new() { 1, 2, 3 };
list?.Insert(0, 13);
Console.WriteLine(String.Join(", ", list.Value)); // Prints 13, 1, 2
```

The value `3` disappeared! What happened is that the copy of the list and the original list
share the same backing array. The single `Insert` call didn't cause the capacity to grow,
because there was space for an extra item in the existing array
(the default initial capacity is 4).
The two lists don't share the same `_count` though. So the `_count` of the copy increased to 4,
but the `_count` of the original remained 3.

The moral lesson is, don't create nullable `ValueList<T>?`s.
And if you have to expose a `ValueList<T>`, expose it as field, not property.
