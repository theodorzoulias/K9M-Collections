![Logo](Logo.png)

# K9M-Collections

[![Nuget](https://img.shields.io/nuget/v/K9M-Collections)](https://www.nuget.org/packages/K9M-Collections)

This .NET 10 repository contains two collections,
the `K9M.KeyedCollection<TKey,TItem>` which is a
[dictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)
with the keys embedded in the items, and the `K9M.ValueList<T>`
which is a value-type
[`IList<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1).
Both collections are tailored for elements that are structs,
either immutable or mutable. They are equipped with the APIs needed for efficient
in-place mutations of the contained elements. To learn more about these collections,
their characteristics, features and philosophy, click on the links below:

- [`K9M.KeyedCollection<TKey,TItem>`](Docs/KeyedCollection.md)
- [`K9M.ValueList<T>`](Docs/ValueList.md)

## Support for .NET versions older than .NET 10?

This repository can't be compiled on older .NET versions or platforms, without losing big chunks of its functionality.
It uses many features, like
[extension members](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods),
that became available in C# 14.

## License

This repository is licensed with the [MIT License](LICENSE).
