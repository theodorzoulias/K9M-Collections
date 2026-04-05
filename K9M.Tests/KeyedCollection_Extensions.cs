using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using K9M.NullRef;
using K9M.Span;
using StringKC = K9M.KeyedCollection<string, K9M.Tests.KeyedCollection_Extensions.Item>;

namespace K9M.Tests;

[TestClass]
public class KeyedCollection_Extensions
{
    [TestMethod]
    public void ContainsKey()
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        Assert.AreEqual(collection.ContainsKey(SpanA), true);
        Assert.AreEqual(collection.ContainsKey(SpanX), false);
    }

    [TestMethod]
    public void GetItem() // Corresponds to the indexer of the collection.
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        Assert.AreEqual(collection.GetItem(SpanA), ItemA);
        Assert.Throws<KeyNotFoundException>(() => collection.GetItem(SpanX));
    }

    [TestMethod]
    public void TryGetItem()
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        Item item;
        Assert.AreEqual(collection.TryGetItem(SpanA, out item), true);
        Assert.AreEqual(item, ItemA);
        Assert.AreEqual(collection.TryGetItem(SpanX, out item), false);
        Assert.AreEqual(item, default);
    }

    [TestMethod]
    public void GetItemRef()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            ref Item item = ref collection.GetItemRef(SpanA);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(item, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
            item = ref collection.GetItemRef(SpanX);
            Assert.IsTrue(item.IsNull);
        }
        {
            StringKC collection = CreateCollection3();
            bool exists;
            ref Item item = ref collection.GetItemRef(SpanA, out exists);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(exists, true);
            Assert.AreEqual(item, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
            item = ref collection.GetItemRef(SpanX, out exists);
            Assert.IsTrue(item.IsNull);
            Assert.AreEqual(exists, false);
        }
    }

    [TestMethod]
    public void GetOrAdd_Factory()
    {
        PrintTitle();
        {
            // The itemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.GetOrAdd(SpanD, k => { invokedCount++; collection.TryRemove(SpanA); return ItemD; });
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
            Assert.AreEqual(invokedCount, 1);
            Assert.AreEqual(collection.Count, 2);
        }
        {
            // Add incompatible item.
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                return collection.GetOrAdd(SpanE, k => { invokedCount++; return ItemA; });
            });
            Assert.AreEqual(exception.Message, "The itemFactory produced an item with different key than the original.");
            Assert.AreEqual(invokedCount, 1);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // Get
            StringKC collection = CreateCollection3();
            int invokedCount = 0; bool added;
            ref Item item = ref collection.GetOrAdd(SpanA, k => { invokedCount++; return ItemA; }, out added);
            Assert.AreEqual(invokedCount, 0);
            Assert.AreEqual(added, false);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        {
            // Add
            StringKC collection = CreateCollection3();
            int invokedCount = 0; string key = null; bool added;
            ref Item item = ref collection.GetOrAdd(SpanD, k => { invokedCount++; key = k.ToString(); return ItemD; }, out added);
            Assert.AreEqual(invokedCount, 1);
            Assert.AreEqual(added, true);
            Assert.AreEqual(key, "D");
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 4);
            Assert.AreEqual(item, ItemD);
            item.Value = 10; Assert.AreEqual(collection["D"].Value, 10);
        }
        // The other GetOrAdd factory overload shares the same implementation.
    }

    [TestMethod]
    public void GetOrAdd_Factory_Argument()
    {
        PrintTitle();
        {
            // The itemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.GetOrAdd(SpanD, (k, arg) => { collection.TryRemove(SpanA); return arg; }, ItemD);
            });
            Assert.AreEqual(ex.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
        }
        {
            // Add incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                return collection.GetOrAdd(SpanE, (k, arg) => arg, ItemA);
            });
            Assert.AreEqual(exception.Message, "The itemFactory produced an item with different key than the original.");
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // Get
            StringKC collection = CreateCollection3();
            int invokedCount = 0; bool added;
            ref Item item = ref collection.GetOrAdd(SpanA, (k, arg) => { invokedCount++; return arg; }, ItemA, out added);
            Assert.AreEqual(invokedCount, 0);
            Assert.AreEqual(added, false);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        {
            // Add
            StringKC collection = CreateCollection3();
            int invokedCount = 0; string key = null; bool added;
            ref Item item = ref collection.GetOrAdd(SpanD, (k, arg) => { invokedCount++; key = k.ToString(); return arg; }, ItemD, out added);
            Assert.AreEqual(invokedCount, 1);
            Assert.AreEqual(added, true);
            Assert.AreEqual(key, "D");
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 4);
            Assert.AreEqual(item, ItemD);
            item.Value = 10; Assert.AreEqual(collection["D"].Value, 10);
        }
        // The other GetOrAdd factory argument overload shares the same implementation.
    }

    [TestMethod]
    public void AddOrReplace_Factory()
    {
        PrintTitle();
        {
            // The addItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanD,
                    k => { collection.TryRemove(SpanC); return default; },
                    (k, e) => default);
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
        }
        {
            // The replaceItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanA, k => default,
                    (k, e) => { collection.TryRemove(SpanC); return default; });
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // Add incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanD, k => ItemA, (k, e) => default);
            });
            Assert.AreEqual(exception.Message, "The addItemFactory produced an item with different key than the original.");
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanA, k => default, (k, e) => ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
        }
        {
            // Add
            StringKC collection = CreateCollection3();
            int invokedCount1 = 0, invokedCount2 = 0;
            string key1 = null, key2 = null;
            Item existing = default;
            bool replaced; Item originalItem;
            ref Item item = ref collection.AddOrReplace(SpanD,
                    k => { invokedCount1++; key1 = k.ToString(); return ItemD; },
                    (k, e) => { invokedCount2++; key2 = k.ToString(); existing = e; return default; },
                    out replaced, out originalItem);
            Assert.AreEqual(replaced, false);
            Assert.AreEqual(originalItem, default);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(invokedCount1, 1);
            Assert.AreEqual(key1, "D");
            Assert.AreEqual(collection.Count, 4);
            Assert.AreEqual(invokedCount2, 0);
            Assert.AreEqual(item, ItemD);
            item.Value = 10; Assert.AreEqual(collection["D"].Value, 10);
        }
        {
            // Replace
            StringKC collection = CreateCollection3();
            int invokedCount1 = 0, invokedCount2 = 0;
            string key1 = null, key2 = null;
            Item existing = default;
            bool replaced; Item originalItem;
            var newItemA = ItemA with { Value = 13 };
            ref Item item = ref collection.AddOrReplace(SpanA,
                    k => { invokedCount1++; key1 = k.ToString(); return default; },
                    (k, e) => { invokedCount2++; key2 = k.ToString(); existing = e; return newItemA; },
                    out replaced, out originalItem);
            Assert.AreEqual(replaced, true);
            Assert.AreEqual(originalItem, ItemA);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(invokedCount1, 0);
            Assert.AreEqual(invokedCount2, 1);
            Assert.AreEqual(key2, "A");
            Assert.AreEqual(existing, ItemA);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, newItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        // The other two AddOrReplace factory overloads are trivial shortcuts of the same implementation.
    }

    [TestMethod]
    public void AddOrReplace_Factory_Argument()
    {
        PrintTitle();
        {
            // The addItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanD,
                    (k, arg) => { collection.TryRemove(SpanC); return arg; },
                    (k, e, arg) => arg, ItemA);
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
        }
        {
            // The replaceItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanA, (k, arg) => arg,
                    (k, e, arg) => { collection.TryRemove(SpanC); return arg; }, ItemD);
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // Add incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanD, (k, arg) => arg, (k, e, arg) => arg, ItemA);
            });
            Assert.AreEqual(exception.Message, "The addItemFactory produced an item with different key than the original.");
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace(SpanA, (k, arg) => arg, (k, e, arg) => arg, ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
        }
        {
            // Add
            StringKC collection = CreateCollection3();
            int invokedCount1 = 0, invokedCount2 = 0;
            string key1 = null, key2 = null;
            Item existing = default;
            bool replaced; Item originalItem;
            ref Item item = ref collection.AddOrReplace(SpanD,
                    (k, arg) => { invokedCount1++; key1 = k.ToString(); return arg; },
                    (k, e, arg) => { invokedCount2++; key2 = k.ToString(); existing = e; return arg; },
                    ItemD, out replaced, out originalItem);
            Assert.AreEqual(replaced, false);
            Assert.AreEqual(originalItem, default);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(invokedCount1, 1);
            Assert.AreEqual(key1, "D");
            Assert.AreEqual(collection.Count, 4);
            Assert.AreEqual(invokedCount2, 0);
            Assert.AreEqual(item, ItemD);
            item.Value = 10; Assert.AreEqual(collection["D"].Value, 10);
        }
        {
            // Replace
            StringKC collection = CreateCollection3();
            int invokedCount1 = 0, invokedCount2 = 0;
            string key1 = null, key2 = null;
            Item existing = default;
            bool replaced; Item originalItem;
            var newItemA = ItemA with { Value = 13 };
            ref Item item = ref collection.AddOrReplace(SpanA,
                    (k, arg) => { invokedCount1++; key1 = k.ToString(); return arg; },
                    (k, e, arg) => { invokedCount2++; key2 = k.ToString(); existing = e; return arg; },
                    newItemA, out replaced, out originalItem);
            Assert.AreEqual(replaced, true);
            Assert.AreEqual(originalItem, ItemA);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(invokedCount1, 0);
            Assert.AreEqual(invokedCount2, 1);
            Assert.AreEqual(key2, "A");
            Assert.AreEqual(existing, ItemA);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, newItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        // The other two AddOrReplace factory argument overloads are trivial shortcuts of the same implementation.
    }

    [TestMethod]
    public void TryReplace_Factory()
    {
        PrintTitle();
        {
            // The replaceItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.TryReplace(SpanA,
                    (k, e) => { collection.TryRemove(SpanC); return default; });
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.TryReplace(SpanA, (k, e) => ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // No-op
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            bool replaced; Item originalItem;
            ref Item item = ref collection.TryReplace(SpanD,
                    (k, e) => { invokedCount++; return default; },
                    out replaced, out originalItem);
            Assert.AreEqual(replaced, false);
            Assert.AreEqual(originalItem, default);
            Assert.IsTrue(item.IsNull);
            Assert.AreEqual(invokedCount, 0);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // Replace
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            string key = null;
            Item existing = default;
            bool replaced; Item originalItem;
            var newItemA = ItemA with { Value = 13 };
            ref Item item = ref collection.TryReplace(SpanA,
                    (k, e) => { invokedCount++; key = k.ToString(); existing = e; return newItemA; },
                    out replaced, out originalItem);
            Assert.AreEqual(replaced, true);
            Assert.AreEqual(originalItem, ItemA);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(invokedCount, 1);
            Assert.AreEqual(key, "A");
            Assert.AreEqual(existing, ItemA);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, newItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        // The other two TryReplace factory overloads are trivial shortcuts of the same implementation.
    }

    [TestMethod]
    public void TryReplace_Factory_Argument()
    {
        PrintTitle();
        {
            // The replaceItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.TryReplace(SpanA,
                    (k, e, arg) => { collection.TryRemove(SpanC); return arg; }, ItemA);
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.TryReplace(SpanA, (k, e, arg) => arg, ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // No-op
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            bool replaced; Item originalItem;
            ref Item item = ref collection.TryReplace(SpanD,
                    (k, e, arg) => { invokedCount++; return arg; }, ItemD,
                    out replaced, out originalItem);
            Assert.AreEqual(replaced, false);
            Assert.AreEqual(originalItem, default);
            Assert.IsTrue(item.IsNull);
            Assert.AreEqual(invokedCount, 0);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // Replace
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            string key = null;
            Item existing = default;
            bool replaced; Item originalItem;
            var newItemA = ItemA with { Value = 13 };
            ref Item item = ref collection.TryReplace(SpanA,
                    (k, e, arg) => { invokedCount++; key = k.ToString(); existing = e; return arg; }, newItemA,
                    out replaced, out originalItem);
            Assert.AreEqual(replaced, true);
            Assert.AreEqual(originalItem, ItemA);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(invokedCount, 1);
            Assert.AreEqual(key, "A");
            Assert.AreEqual(existing, ItemA);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, newItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        // The other two TryReplace factory argument overloads are trivial shortcuts of the same implementation.
    }

    [TestMethod]
    public void TryRemove()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            bool removed;
            Item removedItem;
            removed = collection.TryRemove(SpanA, out removedItem);
            Assert.AreEqual(removed, true);
            Assert.AreEqual(removedItem, ItemA);
            Assert.AreEqual(collection.Count, 2);
        }
        {
            StringKC collection = CreateCollection3();
            bool removed;
            Item removedItem;
            removed = collection.TryRemove(SpanD, out removedItem);
            Assert.AreEqual(removed, false);
            Assert.AreEqual(removedItem, default);
            Assert.AreEqual(collection.Count, 3);
        }
    }

    #region Private Members

    private static Item ItemA = new("A", 1);
    private static Item ItemB = new("B", 2);
    private static Item ItemC = new("C", 3);
    private static Item ItemD = new("D", 4);
    private static Item ItemE = new("E", 4);
    private static ref Item NullItem => ref Unsafe.NullRef<Item>();

    private static ReadOnlySpan<char> SpanA => "A".AsSpan();
    private static ReadOnlySpan<char> SpanB => "B".AsSpan();
    private static ReadOnlySpan<char> SpanC => "C".AsSpan();
    private static ReadOnlySpan<char> SpanD => "D".AsSpan();
    private static ReadOnlySpan<char> SpanE => "E".AsSpan();
    private static ReadOnlySpan<char> SpanX => "X".AsSpan();

    private static IEqualityComparer<string> CreatedStringComparer = EqualityComparer<string>.Create((x, y) => x == y, x => x.GetHashCode());
    private static IEqualityComparer<string> CustomStringComparer = new MyCustomStringComparer();

    private class MyCustomStringComparer : IEqualityComparer<string>, IAlternateEqualityComparer<string, string>
    {
        public bool Equals(string x, string y) => x == y;
        public int GetHashCode(string x) => x.GetHashCode();
        string IAlternateEqualityComparer<string, string>.Create(string alternate) => alternate;
    }

    private static StringKC CreateEmptyCollection(IEqualityComparer<string> comparer = default)
        => new(Item.KeySelector, comparer);

    /// <summary>
    /// Returns a collection with 3 items.
    /// </summary>
    private static StringKC CreateCollection3(IEqualityComparer<string> comparer = default)
    {
        StringKC collection = new(Item.KeySelector, comparer);
        collection.Add(ItemA);
        collection.Add(ItemB);
        collection.Add(ItemC);
        return collection;
    }

    private static StringKC CreateCollection10()
    {
        StringKC collection = new(Item.KeySelector);
        for (int i = 1; i <= 10; i++)
        {
            string key = ((char)(i + 'A' - 1)).ToString();
            collection.Add(new(key, i));
        }
        return collection;
    }

    public record struct Item(string Key, int Value)
    {
        public static string KeySelector(Item item) => item.Key;
    }

    #endregion

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
