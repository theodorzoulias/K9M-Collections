using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BCL = System.Collections.Generic;

namespace K9M.Tests;

[TestClass]
public class KeyedCollection_Main
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

    [TestMethod]
    public void TestEverything()
    {
        PrintTitle();
        new KeyedCollectionEverythingTester(randomSeed: null).Run(iterations: 10_000);
    }

    public class KeyedCollectionEverythingTester : EverythingTester
    {
        private readonly K9M.KeyedCollection<int, EntityS> _collection = new(EntityS.KeySelector);
        private readonly BCL.Dictionary<int, EntityS> _dict = new();
        private const int SizeLimit = 150;
        private int _maxSize = 0;

        public KeyedCollectionEverythingTester(int? randomSeed) : base(randomSeed)
        {
            Randomizer.Add(ContainsKey, 1000);
            Randomizer.Add(TryGetItem, 1000);
            Randomizer.Add(Getter, 1000);
            Randomizer.Add(ToArray, 100);

            Randomizer.Add(TryAdd, 1200);
            Randomizer.Add(AddExistingKey, 100);
            Randomizer.Add(GetOrAdd, 500);
            Randomizer.Add(AddOrReplace, 500);
            Randomizer.Add(TryReplace, 300);
            Randomizer.Add(ReplaceAll, 100);
            Randomizer.Add(TryRemove, 400);
            Randomizer.Add(RemoveWhere, 30);
            Randomizer.Add(GetItemRef, 200);

            Randomizer.Add(Clear, 10);
            Randomizer.Add(TrimExcess, 10);
            Randomizer.Add(EnsureCapacity, 10);
        }

        public override void Run(int iterations)
        {
            base.Run(iterations);
            Console.WriteLine($"MaxSize: {_maxSize:#,0}");
        }

        protected override void OnOperationCompleted()
        {
            _maxSize = Math.Max(_maxSize, _dict.Count);
            if (_dict.Count >= SizeLimit) Clear();
        }

        private void ContainsKey()
        {
            int key = GetFiftyFiftyKey();
            Assert.AreEqual(_collection.ContainsKey(key), _dict.ContainsKey(key));
            OperationCompleted();
        }

        private void TryGetItem()
        {
            int key = GetFiftyFiftyKey();
            Assert.AreEqual(_collection.TryGetItem(key, out var v1), _dict.TryGetValue(key, out var v2));
            Assert.AreEqual(v1, v2);
            OperationCompleted();
        }

        private void Getter()
        {
            if (!TryGetExistingKey(out int key)) return;
            Assert.AreEqual(_collection[key], _dict[key]);
            OperationCompleted();
        }

        private void ToArray()
        {
            Assert.IsTrue(_collection.ToArray().ToHashSet().SetEquals(_dict.Values.ToArray()));
            OperationCompleted();
        }

        private void TryAdd()
        {
            int key = GetFiftyFiftyKey();
            var item = EntityS.Create(key, Random.Next());
            bool added1 = _collection.TryAdd(item);
            bool added2 = _dict.TryAdd(key, item);
            Assert.AreEqual(added1, added2);
            if (added1) { AssertIdentical(); }
            OperationCompleted();
        }

        private void AddExistingKey()
        {
            if (!TryGetExistingKey(out int key)) return;
            int value = Random.Next();
            Assert.Throws<ArgumentException>(() => _collection.Add(new(key, value)));
            Assert.Throws<ArgumentException>(() => _dict.Add(key, new(key, value)));
            OperationCompleted();
        }

        private void GetOrAdd()
        {
            int key = GetFiftyFiftyKey();
            EntityS newItem = new(key, Random.Next());
            EntityS result1; EntityS result2;
            switch (Random.Next(1, 4))
            {
                case 1:
                    result1 = _collection.GetOrAdd(newItem);
                    result2 = _dict.GetOrAdd(key, newItem);
                    break;
                case 2:
                    result1 = _collection.GetOrAdd(key, _ => newItem);
                    result2 = _dict.GetOrAdd(key, _ => newItem);
                    break;
                case 3:
                    result1 = _collection.GetOrAdd(key, (_, arg) => arg, newItem);
                    result2 = _dict.GetOrAdd(key, (_, arg) => arg, newItem);
                    break;
                default: throw new UnreachableException();
            }
            Assert.AreEqual(result1, result2);
            if (result2 != newItem) { AssertIdentical(); }
            OperationCompleted();
        }

        private void AddOrReplace()
        {
            int key = GetFiftyFiftyKey();
            EntityS newItem = new(key, Random.Next());
            switch (Random.Next(1, 3))
            {
                case 1:
                    _collection.AddOrReplace(key, _ => newItem, (_, e) => new(e.Key, e.Value + 1));
                    _dict.AddOrReplace(key, _ => newItem, (_, e) => new(e.Key, e.Value + 1));
                    break;
                case 2:
                    _collection.AddOrReplace(key, (_, arg) => arg, (_, _, arg) => arg, newItem);
                    _dict.AddOrReplace(key, (_, arg) => arg, (_, _, arg) => arg, newItem);
                    break;
                default: throw new UnreachableException();
            }
            AssertIdentical();
            OperationCompleted();
        }

        private void TryReplace()
        {
            int key = GetFiftyFiftyKey();
            EntityS newItem = new(key, Random.Next());
            bool result1; bool result2;
            switch (Random.Next(1, 4))
            {
                case 1:
                    result1 = _collection.TryReplace(newItem);
                    result2 = _dict.TryReplace(key, newItem);
                    break;
                case 2:
                    result1 = _collection.TryReplace(key, (_, _) => newItem);
                    result2 = _dict.TryReplace(key, (_, _) => newItem);
                    break;
                case 3:
                    result1 = _collection.TryReplace(key, (_, _, arg) => arg, newItem);
                    result2 = _dict.TryReplace(key, (_, _, arg) => arg, newItem);
                    break;
                default: throw new UnreachableException();
            }
            Assert.AreEqual(result1, result2);
            if (result1) AssertIdentical();
            OperationCompleted();
        }

        private void ReplaceAll()
        {
            _collection.ReplaceAll(x => new(x.Key, x.Value + 1));
            _dict.ReplaceAll((k, v) => new(k, v.Value + 1), _collection.KeySelector);
            AssertIdentical();
            OperationCompleted();
        }

        private void TryRemove()
        {
            int key = GetFiftyFiftyKey();
            int value = Random.Next();
            bool removed1 = _collection.TryRemove(key, out var value1);
            bool removed2 = _dict.Remove(key, out var value2);
            Assert.AreEqual(removed1, removed2);
            Assert.AreEqual(value1, value2);
            if (removed1) { AssertIdentical(); }
            OperationCompleted();
        }

        private void RemoveWhere()
        {
            double probabilityToRemove = Random.NextDouble();
            BCL.HashSet<int> toRemove = _dict.Keys
                .Where(_ => Random.NextDouble() < probabilityToRemove).ToHashSet();
            int removedCount1 = _collection.RemoveWhere(e => toRemove.Contains(e.Key));
            int removedCount2 = _dict.RemoveWhere((k, v) => toRemove.Contains(k));
            Assert.AreEqual(removedCount1, removedCount2);
            AssertIdentical();
            OperationCompleted();
        }

        private void GetItemRef()
        {
            int key = GetFiftyFiftyKey();
            ref EntityS valueRef1 = ref _collection.GetItemRef(key);
            if (!Unsafe.IsNullRef(ref valueRef1)) valueRef1.Value++;
            ref EntityS valueRef2 = ref _dict.GetValueRef(key);
            if (!Unsafe.IsNullRef(ref valueRef2)) valueRef2.Value++;
            Assert.AreEqual(Unsafe.IsNullRef(ref valueRef1), Unsafe.IsNullRef(ref valueRef2));
            if (!Unsafe.IsNullRef(ref valueRef1)) { AssertIdentical(); }
            OperationCompleted();
        }

        private void Clear()
        {
            _collection.Clear();
            _dict.Clear();
            AssertIdentical();
            OperationCompleted();
        }

        private void TrimExcess()
        {
            _collection.TrimExcess();
            _dict.TrimExcess();
            AssertIdentical();
            OperationCompleted();
        }

        private void EnsureCapacity()
        {
            int newCapacity = _collection.Count + Random.Next(0, 100);
            _collection.EnsureCapacity(newCapacity);
            _dict.EnsureCapacity(newCapacity);
            AssertIdentical();
            OperationCompleted();
        }

        private bool TryGetExistingKey(out int key)
        {
            if (_dict.Count == 0) { key = default; return false; }
            key = _dict.GetRandom(Random).Key; return true;
        }

        private int GetFiftyFiftyKey()
            => (Random.NextDouble() < 0.5 && TryGetExistingKey(out var key)) ? key : GetNonExistentKey();

        private int GetNonExistentKey()
        {
            while (true)
            {
                int randomKey = Random.Next();
                if (!_dict.ContainsKey(randomKey)) return randomKey;
            }
        }

        private IEnumerable<int> GetDistinctNonExistentKeys()
        {
            return Enumerable.Range(0, Int32.MaxValue).Select(_ => GetNonExistentKey()).Distinct();
        }

        private void AssertIdentical(string info = default, [CallerMemberName] string callerName = "")
        {
            Assert.AreEqual(_collection.Count, _dict.Count, callerName);
            Assert.AreEqual(_collection.IsEmpty, _dict.IsEmpty, callerName);
            Assert.IsTrue(_dict.SetEquals(_collection.Select(e => KeyValuePair.Create(e.Key, e))), callerName);
        }
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
