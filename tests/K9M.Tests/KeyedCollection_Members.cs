using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using K9M.NullRef;
using StringKC = K9M.KeyedCollection<string, K9M.Tests.KeyedCollection_Members.Item>;

namespace K9M.Tests;

[TestClass]
public class KeyedCollection_Members
{
    [TestMethod]
    public void KeySelector()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3(null);
            Assert.AreSame(Item.KeySelector, collection.KeySelector);
        }
        {
            StringKC collection = CreateCollection3(StringComparer.OrdinalIgnoreCase);
            Assert.AreSame(Item.KeySelector, collection.KeySelector);
        }
        {
            StringKC collection = CreateCollection3(CreatedStringComparer);
            Assert.AreSame(Item.KeySelector, collection.KeySelector);
        }
        {
            StringKC collection = CreateCollection3(CustomStringComparer);
            Assert.AreSame(Item.KeySelector, collection.KeySelector);
        }
    }

    [TestMethod]
    public void KeyComparer()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3(null);
            Assert.AreSame(collection.KeyComparer, EqualityComparer<string>.Default);
        }
        {
            StringKC collection = CreateCollection3(StringComparer.OrdinalIgnoreCase);
            Assert.AreSame(collection.KeyComparer, StringComparer.OrdinalIgnoreCase);
        }
        {
            StringKC collection = CreateCollection3(CreatedStringComparer);
            Assert.AreSame(collection.KeyComparer, CreatedStringComparer);
        }
        {
            StringKC collection = CreateCollection3(CustomStringComparer);
            Assert.AreSame(collection.KeyComparer, CustomStringComparer);
        }
    }

    [TestMethod]
    public void Count_IsEmpty()
    {
        PrintTitle();
        {
            StringKC collection = CreateEmptyCollection();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.IsEmpty, true);
        }
        {
            StringKC collection = CreateCollection3();
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(collection.IsEmpty, false);
        }
    }

    [TestMethod]
    public void Capacity()
    {
        PrintTitle();
        {
            StringKC collection = CreateEmptyCollection();
            Assert.AreEqual(collection.Capacity, 0);
        }
        {
            StringKC collection = CreateCollection3();
            Assert.IsGreaterThanOrEqualTo(3, collection.Capacity);
        }
    }

    [TestMethod]
    public void ContainsKey()
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        Assert.AreEqual(collection.ContainsKey("A"), true);
        Assert.AreEqual(collection.ContainsKey("X"), false);
    }

    [TestMethod]
    public void ContainsItem()
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        Assert.AreEqual(collection.ContainsItem(new("A", 1)), true);
        Assert.AreEqual(collection.ContainsItem(new("A", 2)), false);
        Assert.AreEqual(collection.ContainsItem(new("X", 1)), false);
        IEqualityComparer<Item> comparer = EqualityComparer<Item>.Create((x, y) => false);
        Assert.AreEqual(collection.ContainsItem(new("A", 1), comparer), false);
    }

    [TestMethod]
    public void Indexer()
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        Assert.AreEqual(collection["A"], ItemA);
        Assert.Throws<KeyNotFoundException>(() => collection["X"]);
    }

    [TestMethod]
    public void TryGetItem()
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        Item item;
        Assert.AreEqual(collection.TryGetItem("A", out item), true);
        Assert.AreEqual(item, ItemA);
        Assert.AreEqual(collection.TryGetItem("X", out item), false);
        Assert.AreEqual(item, default);
    }

    [TestMethod]
    public void GetItemRef()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            ref Item item = ref collection.GetItemRef("A");
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(item, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
            item = ref collection.GetItemRef("X");
            Assert.IsTrue(item.IsNull);
        }
        {
            StringKC collection = CreateCollection3();
            bool exists;
            ref Item item = ref collection.GetItemRef("A", out exists);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(exists, true);
            Assert.AreEqual(item, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
            item = ref collection.GetItemRef("X", out exists);
            Assert.IsTrue(item.IsNull);
            Assert.AreEqual(exists, false);
        }
    }

    [TestMethod]
    public void Add()
    {
        PrintTitle();
        StringKC collection = CreateCollection3();
        ref Item item = ref collection.Add(ItemD);
        Assert.AreEqual(collection.Count, 4);
        Assert.IsTrue(collection.ContainsItem(ItemD));
        Assert.AreEqual(item, ItemD);
        item.Value = 10; Assert.AreEqual(collection["D"].Value, 10);
        ArgumentException exception = Assert.Throws<ArgumentException>(() => collection.Add(ItemA));
        Assert.StartsWith("An item with the same key already exists in the collection.", exception.Message);
        Assert.AreEqual(collection.Count, 4);
    }

    [TestMethod]
    public void TryAdd()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            bool added;
            added = collection.TryAdd(ItemA);
            Assert.AreEqual(added, false);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            StringKC collection = CreateCollection3();
            bool added;
            added = collection.TryAdd(ItemD);
            Assert.AreEqual(added, true);
            Assert.AreEqual(collection.Count, 4);
            Assert.IsTrue(collection.ContainsItem(ItemD));
        }
    }

    [TestMethod]
    public void GetOrAdd_Item()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            bool added;
            ref Item item = ref collection.GetOrAdd(ItemA, out added);
            Assert.AreEqual(added, false);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        {
            StringKC collection = CreateCollection3();
            bool added;
            ref Item item = ref collection.GetOrAdd(ItemD, out added);
            Assert.AreEqual(added, true);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 4);
            Assert.AreEqual(item, ItemD);
            item.Value = 10; Assert.AreEqual(collection["D"].Value, 10);
        }
        // The other simpler GetOrAdd item overload shares the same implementation.
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
                _ = collection.GetOrAdd("D", k => { invokedCount++; collection.TryRemove("A"); return ItemD; });
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
                return collection.GetOrAdd("E", k => { invokedCount++; return ItemA; });
            });
            Assert.AreEqual(exception.Message, "The itemFactory produced an item with different key than the original.");
            Assert.AreEqual(invokedCount, 1);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // Get
            StringKC collection = CreateCollection3();
            int invokedCount = 0; bool added;
            ref Item item = ref collection.GetOrAdd("A", k => { invokedCount++; return ItemA; }, out added);
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
            ref Item item = ref collection.GetOrAdd("D", k => { invokedCount++; key = k; return ItemD; }, out added);
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
                _ = collection.GetOrAdd("D", (k, arg) => { collection.TryRemove("A"); return arg; }, ItemD);
            });
            Assert.AreEqual(ex.Message, "The collection was modified.");
            Assert.AreEqual(collection.Count, 2);
        }
        {
            // Add incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                return collection.GetOrAdd("E", (k, arg) => arg, ItemA);
            });
            Assert.AreEqual(exception.Message, "The itemFactory produced an item with different key than the original.");
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // Get
            StringKC collection = CreateCollection3();
            int invokedCount = 0; bool added;
            ref Item item = ref collection.GetOrAdd("A", (k, arg) => { invokedCount++; return arg; }, ItemA, out added);
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
            ref Item item = ref collection.GetOrAdd("D", (k, arg) => { invokedCount++; key = k; return arg; }, ItemD, out added);
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
    public void AddOrReplace_Item()
    {
        PrintTitle();
        {
            // Add
            StringKC collection = CreateCollection3();
            bool replaced;
            Item originalItem;
            ref Item item = ref collection.AddOrReplace(ItemD, out replaced, out originalItem);
            Assert.AreEqual(replaced, false);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 4);
            Assert.AreEqual(item, ItemD);
            Assert.AreEqual(originalItem, default);
            item.Value = 10; Assert.AreEqual(collection["D"].Value, 10);
        }
        {
            // Replace
            StringKC collection = CreateCollection3();
            var newItemA = ItemA with { Value = 13 };
            bool replaced;
            Item originalItem;
            ref Item item = ref collection.AddOrReplace(newItemA, out replaced, out originalItem);
            Assert.AreEqual(replaced, true);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, newItemA);
            Assert.AreEqual(originalItem, ItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        // The other two AddOrReplace item overloads are trivial shortcuts of the same implementation.
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
                _ = collection.AddOrReplace("D",
                    k => { collection.TryRemove("C"); return default; },
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
                _ = collection.AddOrReplace("A", k => default,
                    (k, e) => { collection.TryRemove("C"); return default; });
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
                _ = collection.AddOrReplace("D", k => ItemA, (k, e) => default);
            });
            Assert.AreEqual(exception.Message, "The addItemFactory produced an item with different key than the original.");
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace("A", k => default, (k, e) => ItemD);
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
            ref Item item = ref collection.AddOrReplace("D",
                    k => { invokedCount1++; key1 = k; return ItemD; },
                    (k, e) => { invokedCount2++; key2 = k; existing = e; return default; },
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
            ref Item item = ref collection.AddOrReplace("A",
                    k => { invokedCount1++; key1 = k; return default; },
                    (k, e) => { invokedCount2++; key2 = k; existing = e; return newItemA; },
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
                _ = collection.AddOrReplace("D",
                    (k, arg) => { collection.TryRemove("C"); return arg; },
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
                _ = collection.AddOrReplace("A", (k, arg) => arg,
                    (k, e, arg) => { collection.TryRemove("C"); return arg; }, ItemD);
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
                _ = collection.AddOrReplace("D", (k, arg) => arg, (k, e, arg) => arg, ItemA);
            });
            Assert.AreEqual(exception.Message, "The addItemFactory produced an item with different key than the original.");
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = collection.AddOrReplace("A", (k, arg) => arg, (k, e, arg) => arg, ItemD);
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
            ref Item item = ref collection.AddOrReplace("D",
                    (k, arg) => { invokedCount1++; key1 = k; return arg; },
                    (k, e, arg) => { invokedCount2++; key2 = k; existing = e; return arg; },
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
            ref Item item = ref collection.AddOrReplace("A",
                    (k, arg) => { invokedCount1++; key1 = k; return arg; },
                    (k, e, arg) => { invokedCount2++; key2 = k; existing = e; return arg; },
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
    public void TryReplace_Item()
    {
        PrintTitle();
        {
            // No-op
            StringKC collection = CreateCollection3();
            bool replaced = collection.TryReplace(ItemD);
            Assert.AreEqual(replaced, false);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // No-op
            StringKC collection = CreateCollection3();
            bool replaced;
            Item originalItem;
            ref Item item = ref collection.TryReplace(ItemD, out replaced, out originalItem);
            Assert.AreEqual(replaced, false);
            Assert.AreEqual(originalItem, default);
            Assert.IsTrue(item.IsNull);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            // Replace
            StringKC collection = CreateCollection3();
            var newItemA = ItemA with { Value = 13 };
            bool replaced;
            Item originalItem;
            ref Item item = ref collection.TryReplace(newItemA, out replaced, out originalItem);
            Assert.AreEqual(replaced, true);
            Assert.AreEqual(originalItem, ItemA);
            Assert.IsTrue(item.IsNotNull);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(item, newItemA);
            item.Value = 10; Assert.AreEqual(collection["A"].Value, 10);
        }
        // The other TryReplace item overload is trivial shortcut of the same implementation.
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
                _ = collection.TryReplace("A",
                    (k, e) => { collection.TryRemove("C"); return default; });
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
                _ = collection.TryReplace("A", (k, e) => ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // No-op
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            bool replaced; Item originalItem;
            ref Item item = ref collection.TryReplace("D",
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
            ref Item item = ref collection.TryReplace("A",
                    (k, e) => { invokedCount++; key = k; existing = e; return newItemA; },
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
                _ = collection.TryReplace("A",
                    (k, e, arg) => { collection.TryRemove("C"); return arg; }, ItemA);
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
                _ = collection.TryReplace("A", (k, e, arg) => arg, ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
            Assert.AreEqual(collection["A"].Value, 1);
        }
        {
            // No-op
            StringKC collection = CreateCollection3();
            int invokedCount = 0;
            bool replaced; Item originalItem;
            ref Item item = ref collection.TryReplace("D",
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
            ref Item item = ref collection.TryReplace("A",
                    (k, e, arg) => { invokedCount++; key = k; existing = e; return arg; }, newItemA,
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
            removed = collection.TryRemove("A", out removedItem);
            Assert.AreEqual(removed, true);
            Assert.AreEqual(removedItem, ItemA);
            Assert.AreEqual(collection.Count, 2);
        }
        {
            StringKC collection = CreateCollection3();
            bool removed;
            Item removedItem;
            removed = collection.TryRemove("D", out removedItem);
            Assert.AreEqual(removed, false);
            Assert.AreEqual(removedItem, default);
            Assert.AreEqual(collection.Count, 3);
        }
    }

    [TestMethod]
    public void Clear()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            collection.Clear();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.Count(), collection.Count);
        }
    }

    [TestMethod]
    public void EnsureCapacity()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            collection.EnsureCapacity(10);
            Assert.AreEqual(collection.Capacity, 10);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(collection.Count(), collection.Count);
        }
    }

    [TestMethod]
    public void TrimExcess()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            collection.TrimExcess();
            Assert.AreEqual(collection.Capacity, 3);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(collection.Count(), collection.Count);
        }
        {
            StringKC collection = CreateCollection3();
            collection.EnsureCapacity(10);
            collection.TrimExcess(5);
            Assert.AreEqual(collection.Capacity, 5);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(collection.Count(), collection.Count);
        }
    }

    [TestMethod]
    public void ToArray()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection3();
            Item[] array = collection.ToArray();
            Assert.AreEqual(array.Length, 3);
            Assert.IsTrue(collection.SequenceEqual(array));
        }
    }

    [TestMethod]
    public void ReplaceAll()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection10();
            Dictionary<string, int> newValues = new();
            foreach (Item item in collection)
                newValues.Add(item.Key, item.Value + 100);
            int invokedCount = 0;
            collection.ReplaceAll(x =>
            {
                invokedCount++;
                return x with { Value = newValues[x.Key] };
            });
            Assert.AreEqual(invokedCount, 10);
            foreach (Item item in collection)
                Assert.AreEqual(item.Value, newValues[item.Key]);
            Assert.AreEqual(collection.Count, 10);
            Assert.AreEqual(collection.Count(), collection.Count);
            Console.WriteLine(String.Join("\r\n", collection));
        }
        {
            // The replaceItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                collection.ReplaceAll(x => { collection.TryRemove(x.Key); return x; });
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                collection.ReplaceAll(x => ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
        }
    }

    [TestMethod]
    public void ReplaceWhere()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection10();
            Dictionary<string, int> newValues = new();
            foreach (Item item in collection)
                newValues.Add(item.Key, item.Value + 100);
            int invokedCount1 = 0, invokedCount2 = 0;
            int replaced = collection.ReplaceWhere(x =>
            {
                invokedCount1++;
                return x.Value % 2 == 0;
            }, x =>
            {
                invokedCount2++;
                return x with { Value = newValues[x.Key] };
            });
            Assert.AreEqual(invokedCount1, 10);
            Assert.AreEqual(invokedCount2, 5);
            Assert.AreEqual(replaced, 5);
            foreach (Item item in collection)
            {
                if (item.Value % 2 == 0)
                    Assert.AreEqual(item.Value, newValues[item.Key]);
            }
            Assert.AreEqual(collection.Count, 10);
            Assert.AreEqual(collection.Count(), collection.Count);
            Console.WriteLine(String.Join("\r\n", collection));
        }
        {
            // The match modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                collection.ReplaceWhere(x => { collection.TryRemove(x.Key); return true; },
                    x => x);
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
        }
        {
            // The replaceItemFactory modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                collection.ReplaceWhere(x => true,
                    x => { collection.TryRemove(x.Key); return x; });
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
        }
        {
            // Replace with incompatible item.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                collection.ReplaceWhere(x => true, x => ItemD);
            });
            Assert.AreEqual(exception.Message, "The replaceItemFactory produced an item with different key than the original.");
        }
    }

    [TestMethod]
    public void RemoveWhere()
    {
        PrintTitle();
        {
            StringKC collection = CreateCollection10();
            int invokedCount = 0;
            int removed = collection.RemoveWhere(x =>
            {
                invokedCount++;
                return x.Value % 2 == 0;
            });
            Assert.AreEqual(invokedCount, 10);
            Assert.AreEqual(removed, 5);
            Assert.AreEqual(collection.Count, 5);
            Assert.AreEqual(collection.Count(), collection.Count);
            Assert.IsTrue(collection.All(x => x.Value % 2 != 0));
            Console.WriteLine(String.Join("\r\n", collection));
        }
        {
            // The match modifies the collection.
            StringKC collection = CreateCollection3();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                collection.RemoveWhere(x => { collection.TryRemove(x.Key); return true; });
            });
            Assert.AreEqual(exception.Message, "The collection was modified.");
        }
    }

    #region Private Members

    private static Item ItemA = new("A", 1);
    private static Item ItemB = new("B", 2);
    private static Item ItemC = new("C", 3);
    private static Item ItemD = new("D", 4);
    private static Item ItemE = new("E", 4);
    private static ref Item NullItem => ref Unsafe.NullRef<Item>();

    //private static ReadOnlySpan<char> SpanA => "A".AsSpan();
    //private static ReadOnlySpan<char> SpanB => "B".AsSpan();
    //private static ReadOnlySpan<char> SpanC => "C".AsSpan();
    //private static ReadOnlySpan<char> SpanD => "D".AsSpan();
    //private static ReadOnlySpan<char> SpanE => "E".AsSpan();
    //private static ReadOnlySpan<char> SpanX => "X".AsSpan();

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
