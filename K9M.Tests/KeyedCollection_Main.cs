using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BCL = System.Collections.Generic;

namespace K9M.Tests;

[TestClass]
public partial class KeyedCollection_Main
{
    [TestMethod]
    public void Add_TryGetValue_Contains()
    {
        PrintTitle();
        Random random = new(0);
        const int keysCount = 1000;
        const int nonExistentKeysCount = 1000;
        BCL.HashSet<int> keys = Enumerable.Range(1, Int32.MaxValue)
            .Select(_ => random.Next()).Distinct().Take(keysCount).ToHashSet();
        int[] nonExistentKeys = Enumerable.Range(1, Int32.MaxValue)
            .Select(_ => random.Next()).Where(x => !keys.Contains(x)).Distinct().Take(nonExistentKeysCount).ToArray();
        Console.WriteLine($"Keys: {keys.Count:#,0}, Non-existent keys: {nonExistentKeys.Length:#,0}");
        IEnumerable<EntityS> entities = keys
            .Select((k, i) => EntityS.Create(k, i + 1));

        K9M.KeyedCollection<int, EntityS> collection = new(EntityS.KeySelector);
        foreach (var entity in entities)
            collection.Add(entity);
        Assert.AreEqual(collection.Count, keys.Count);
        Assert.IsTrue(keys.SetEquals(collection.Select(e => e.Key)));
        foreach (var e in entities)
            Assert.IsTrue(collection.TryGetItem(e.Key, out var itemFound) && itemFound == e, $"key: {e.Key}, value: {e.Value}, valueFound: {itemFound}");
        foreach (var k in nonExistentKeys)
            Assert.IsFalse(collection.ContainsKey(k));
        Console.WriteLine($"OK, Capacity: {collection.Capacity:#,0}");
    }

    [TestMethod]
    public void TestInitialization()
    {
        PrintTitle();
        K9M.KeyedCollection<int, EntityS> collection;
        EntityS e1 = new(1, 1);
        collection = new(EntityS.KeySelector); collection.TryAdd(e1); Assert.AreEqual(collection.Count, 1);
        collection = new(EntityS.KeySelector); collection.Add(e1); Assert.AreEqual(collection.Count, 1);
        collection = new(EntityS.KeySelector); collection.TryRemove(1); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.TryRemove(1, out _); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.ContainsKey(1); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.TryGetItem(1, out _); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.GetItemRef(1); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.GetOrAdd(e1); Assert.AreEqual(collection.Count, 1);
        collection = new(EntityS.KeySelector); collection.GetOrAdd(1, _ => e1); Assert.AreEqual(collection.Count, 1);
        collection = new(EntityS.KeySelector); collection.GetOrAdd(1, (_, _) => e1, 0); Assert.AreEqual(collection.Count, 1);
        collection = new(EntityS.KeySelector); collection.AddOrReplace(1, _ => e1, (_, _) => e1); Assert.AreEqual(collection.Count, 1);
        collection = new(EntityS.KeySelector); collection.AddOrReplace(1, (_, _) => e1, (_, _, _) => e1, 0); Assert.AreEqual(collection.Count, 1);
        collection = new(EntityS.KeySelector); collection.TryReplace(e1); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.TryReplace(1, (_, _) => e1); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.TryReplace(1, (_, _, _) => e1, 0); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.Count(); Assert.AreEqual(collection.Count, 0); // Enumeration
        collection = new(EntityS.KeySelector); collection.Clear(); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.EnsureCapacity(0); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.TrimExcess(); Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); _ = collection.IsEmpty; Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); _ = collection.Count; Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); _ = collection.Capacity; Assert.AreEqual(collection.Count, 0);
        collection = new(EntityS.KeySelector); collection.Add(e1); collection.GetRandom(); Assert.AreEqual(collection.Count, 1);
    }

    [TestMethod]
    public void Constructors()
    {
        PrintTitle();
        K9M.KeyedCollection<string, (string Key, int Value)> collection;
        (string, int)[] items = Enumerable.Range(1, 10).Select(x => (x.ToString(), 0)).ToArray();

        collection = new(x => x.Key);
        Assertions(collection, 0, 0, default);

        collection = new(x => x.Key, comparer: null);
        Assertions(collection, 0, 0, default);
        collection = new(x => x.Key, StringComparer.OrdinalIgnoreCase);
        Assertions(collection, 0, 0, StringComparer.OrdinalIgnoreCase);

        collection = new(x => x.Key, 10);
        Assertions(collection, 0, 10, default);

        collection = new(x => x.Key, 10, comparer: null);
        Assertions(collection, 0, 10, default);
        collection = new(x => x.Key, 10, StringComparer.OrdinalIgnoreCase);
        Assertions(collection, 0, 10, StringComparer.OrdinalIgnoreCase);

        collection = new(x => x.Key, items);
        Assertions(collection, items.Length, items.Length, default);

        collection = new(x => x.Key, items, comparer: null);
        Assertions(collection, items.Length, items.Length, default);
        collection = new(x => x.Key, items, StringComparer.OrdinalIgnoreCase);
        Assertions(collection, items.Length, items.Length, StringComparer.OrdinalIgnoreCase);

        static void Assertions<TKey, TValue>(K9M.KeyedCollection<TKey, TValue> collection, int count, int capacity, IEqualityComparer<TKey> comparer)
        {
            Assert.AreEqual(collection.Count, count);
            if (capacity == 0)
                Assert.AreEqual(collection.Capacity, capacity);
            else
                Assert.IsTrue(collection.Capacity >= capacity, $"{collection.Capacity}, {capacity}");
            Assert.AreSame(collection.KeyComparer, comparer ?? EqualityComparer<TKey>.Default);
        }
    }

    [TestMethod]
    public void Constructor_WithDuplicates()
    {
        PrintTitle();
        K9M.KeyedCollection<int, EntityS> collection;
        var items = Enumerable.Repeat(EntityS.Create(1, 1), 2);
        Assert.Throws<ArgumentException>(() => collection = new(EntityS.KeySelector, items));
    }

    [TestMethod]
    public void Constructor_WithNullItems()
    {
        PrintTitle();
        K9M.KeyedCollection<int, EntityR> collection;
        IEnumerable<EntityR> items = new EntityR[] { null }.HideIdentity();
        Assert.Throws<ArgumentException>(() => collection = new(EntityR.KeySelector, items));
        Assert.Throws<ArgumentException>(() => collection = new(EntityR.KeySelector, items.ToArray()));
    }

    [TestMethod]
    public void ArgumentExceptions()
    {
        PrintTitle();
        K9M.KeyedCollection<int, EntityR> collection = new(EntityR.KeySelector);
        EntityR e1 = new(1, 1);
        EntityR e0 = new(0, 0);
        EntityR[] arrayWithNullItem = new EntityR[] { (EntityR)null };

        Assert.That.ThrowsArgumentNullException(() => collection = new(keySelector: null), "keySelector");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection = new(EntityR.KeySelector, -1), "capacity");
        Assert.That.ThrowsArgumentNullException(() => collection = new(EntityR.KeySelector, items: null), "items");
        Assert.That.ThrowsArgumentNullException(() => collection.Add(null), "item");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection.EnsureCapacity(-1), "capacity");
        Assert.Throws<OutOfMemoryException>(() => collection.EnsureCapacity(Int32.MaxValue));
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection.TrimExcess(-1), "capacity");
        Assert.Throws<KeyNotFoundException>(() => _ = collection[0]);

        Assert.That.ThrowsArgumentNullException(() => collection.GetOrAdd(0, itemFactory: null), "itemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.GetOrAdd(0, itemFactory: null, 1), "itemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.AddOrReplace(0, _ => e0, replaceItemFactory: null), "replaceItemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.AddOrReplace(0, (_, _) => e0, replaceItemFactory: null, 0), "replaceItemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.AddOrReplace(0, addItemFactory: null, (_, _) => e0), "addItemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.AddOrReplace(0, addItemFactory: null, (_, _, _) => e0, 0), "addItemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.TryReplace(0, replaceItemFactory: null), "replaceItemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.TryReplace(0, replaceItemFactory: null, 0), "replaceItemFactory");
        Assert.That.ThrowsArgumentNullException(() => collection.RemoveWhere(match: null), "match");
        Assert.That.ThrowsArgumentNullException(() => collection.RemoveWhere(match: null, 0), "match");

        collection.Add(e1); Assert.Throws<ArgumentException>(() => collection.Add(e1), "key");
    }

    [TestMethod]
    public void NullItem_ThrowsArgumentNullException()
    {
        PrintTitle();
        K9M.KeyedCollection<int, EntityR> collection = new(EntityR.KeySelector);
        EntityR e1 = new(1, 1);
        EntityR e0 = new(0, 0);
        EntityR[] arrayWithNullItem = new EntityR[] { (EntityR)null };
        collection.Add(e1);

        Assert.Throws<ArgumentNullException>(() => collection.Add(null));
        Assert.Throws<ArgumentNullException>(() => collection.GetOrAdd(null));
        Assert.Throws<InvalidOperationException>(() => collection.GetOrAdd(0, _ => null));
        Assert.Throws<InvalidOperationException>(() => collection.AddOrReplace(0, _ => null, (_, _) => e0));
        Assert.Throws<InvalidOperationException>(() => collection.TryReplace(1, (_, _) => null));
        Assert.Throws<InvalidOperationException>(() => collection.TryReplace(1, (_, _, _) => null, 0));
    }

    [TestMethod]
    public void TestKeyComparer()
    {
        PrintTitle();
        K9M.KeyedCollection<string, (string Key, int Value)> collection;
        collection = new(x => x.Key, StringComparer.Ordinal);
        collection.Add(("a", default));
        Assert.IsFalse(collection.ContainsKey("A"));
        Assert.AreSame(collection.KeyComparer, StringComparer.Ordinal);

        collection = new(x => x.Key, StringComparer.OrdinalIgnoreCase);
        collection.Add(("a", default));
        Assert.IsTrue(collection.ContainsKey("A"));
        Assert.AreSame(collection.KeyComparer, StringComparer.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void MaxHashCodeCollisions()
    {
        PrintTitle();
        // Use bad equality comparer that returns the same hashcode for all elements.
        IEqualityComparer<int> comparer = EqualityComparer<int>.Create((x, y) => x.Equals(y), _ => 0);
        const int from = 0, to = 100;
        for (int size = from; size < to; size++)
        {
            EntityS[] items = Enumerable.Range(1, size).Select(x => EntityS.Create(x, 0)).ToArray();
            K9M.KeyedCollection<int, EntityS> collection = new(EntityS.KeySelector, items, comparer);
            foreach (var item in items) Assert.IsTrue(collection.ContainsKey(item.Key));
            foreach (var item in items) Assert.IsTrue(collection.TryRemove(item.Key));
        }
        Console.WriteLine($"Tested from size {from:#,0} to {to:#,0}.");
    }

    [TestMethod]
    public void ModifyingCollectionInvalidatesCallbacks()
    {
        PrintTitle();
        K9M.KeyedCollection<int, EntityS> collection;
        EntityS e1 = new(1, 1);
        EntityS e2 = new(2, 2);

        collection = new(EntityS.KeySelector); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.GetOrAdd(1, _ => { collection.Add(e2); return e1; });
        });
        collection = new(EntityS.KeySelector); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.GetOrAdd(1, (_, _) => { collection.Add(e2); return e1; }, 1);
        });

        collection = new(EntityS.KeySelector); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.AddOrReplace(1, _ => { collection.Add(e2); return e1; }, (_, _) => e1);
        });
        collection = new(EntityS.KeySelector); collection.Add(e1); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.AddOrReplace(1, _ => e1, (_, _) => { collection.Add(e2); return e1; });
        });
        collection = new(EntityS.KeySelector); collection.Add(e1); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.AddOrReplace(1, (_, _) => e1, (_, _, _) => { collection.Add(e2); return e1; }, 1);
        });

        collection = new(EntityS.KeySelector); collection.Add(e1); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.TryReplace(1, (_, _) => { collection.Add(e2); return e1; });
        });
        collection = new(EntityS.KeySelector); collection.Add(e1); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.TryReplace(1, (_, _, _) => { collection.Add(e2); return e1; }, 1);
        });

        collection = new(EntityS.KeySelector); collection.Add(e1); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.RemoveWhere(e => { if (e.Key == 1) collection.Add(e2); return true; });
        });
        collection = new(EntityS.KeySelector); collection.Add(e1); Assert.Throws<InvalidOperationException>(() =>
        {
            collection.RemoveWhere((e, _) => { if (e.Key == 1) collection.Add(e2); return true; }, 1);
        });
    }

    [TestMethod]
    public void AddRange_Capacity()
    {
        PrintTitle();
        int[] sizes = [10, 100, 1000];
        foreach (var size in sizes)
        {
            Console.WriteLine();
            Console.WriteLine($"Size: {size:#,0}");
            EntityS[] array = Enumerable
                .Range(1, size).Select(x => EntityS.Create(x, 0)).ToArray();
            IEnumerable<EntityS> enumerable = array.HideIdentity();

            K9M.KeyedCollection<int, EntityS> collection = null;
            (string Title, bool ExpectedTheMinimum, Action Create)[] actions =
            [
                ("Using the constructor with array", true, () => collection = new(EntityS.KeySelector, array)),
                ("Using the constructor with enumerable", false, () => collection = new(EntityS.KeySelector, enumerable)),
                ("Adding one by one", false, () => { collection = new(EntityS.KeySelector); foreach (var item in array) collection.Add(item); }),
            ];
            foreach (var (title, expectedTheMinimum, createDictionary) in actions)
            {
                createDictionary();
                Console.WriteLine($"- {title.PadRight(37)} Count: {collection.Count:#,0}, Capacity: {collection.Capacity:#,0}");
                Assert.AreEqual(collection.Count, size, title);
                if (expectedTheMinimum)
                    Assert.AreEqual(collection.Capacity, collection.Count, title);
            }
        }
    }

    [TestMethod]
    public void Clear_TrimExcess_CapacityZero()
    {
        // Test that after calling Clear() and TrimExcess(), the Capacity is zero.
        PrintTitle();
        int[] sizes = [0, 1, 10, 100, 1000];
        foreach (var size in sizes)
        {
            IEnumerable<EntityS> items = Enumerable
                .Range(1, size).Select(x => EntityS.Create(x, 0));
            K9M.KeyedCollection<int, EntityS> collection = new(EntityS.KeySelector, items);
            int maxCount = collection.Count;
            int maxCapacity = collection.Capacity;
            collection.Clear();
            collection.TrimExcess();
            Console.WriteLine($"Size: {size:#,0}, Max count: {maxCount:#,0}, Max capacity: {maxCapacity:#,0}, Current capacity: {collection.Capacity:#,0}");
            Assert.AreEqual(collection.Capacity, 0);
        }
    }

    [TestMethod]
    public void RemoveItemsDuringEnumeration()
    {
        K9M.KeyedCollection<int, int> collection = new(x => x, Enumerable.Range(1, 10));
        Assert.AreEqual(collection.Count, 10);
        foreach (int x in collection) collection.TryRemove(x);
        Assert.AreEqual(collection.Count, 0);
    }

    private record struct EntityS
    {
        public int Key;
        public int Value;

        public EntityS(int key, int value) { Key = key; Value = value; }
        public KeyValuePair<int, int> ToKeyValuePair() => new(Key, Value);
        public override string ToString() => $"[{Key}, {Value}]";
        public static EntityS Create(int key, int value) => new(key, value);
        public static int KeySelector(EntityS entity) => entity.Key;
    }

    private record class EntityR
    {
        public int Key;
        public int Value;

        public EntityR(int key, int value) { Key = key; Value = value; }
        public KeyValuePair<int, int> ToKeyValuePair() => new(Key, Value);
        public override string ToString() => $"[{Key}, {Value}]";
        public static EntityR Create(int key, int value) => new(key, value);
        public static int KeySelector(EntityR entity) => entity.Key;
    }

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
