using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using K9M.NullRef;
using VL = K9M.ValueList<int>;

namespace K9M.Tests;

[TestClass]
public class ValueList_Members
{
    [TestMethod]
    public void Constructors()
    {
        PrintTitle();
        ValueList<int> collection;
        var items = Enumerable.Range(1, 10);

        collection = default;
        Assertions(collection, 0, 0, true);

        collection = new();
        Assertions(collection, 0, 0, false);

        collection = new(10);
        Assertions(collection, 0, 10, false);

        collection = new(items);
        Assertions(collection, items.Count(), items.Count(), false);

        collection = new(items.HideIdentity());
        Assertions(collection, items.Count(), items.Count(), false);

        collection = new(items.ToArray());
        Assertions(collection, items.Count(), items.Count(), false);

        static void Assertions<T>(ValueList<T> collection, int count, int capacity, bool isDefault)
        {
            Assert.AreEqual(collection.Count, count);
            if (capacity == 0)
                Assert.AreEqual(collection.Capacity, capacity);
            else
                Assert.IsTrue(collection.Capacity >= capacity, $"{collection.Capacity}, {capacity}");
            Assert.AreEqual(collection == default, isDefault);
        }
    }

    [TestMethod]
    public void Count_IsEmpty()
    {
        PrintTitle();
        {
            VL collection = CreateCollectionX();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.IsEmpty, true);
            Assert.AreEqual(collection.IsDefault, true);
            collection.Clear();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.IsEmpty, true);
            Assert.AreEqual(collection.IsDefault, true);
        }
        {
            VL collection = CreateCollection0();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.IsEmpty, true);
            Assert.AreEqual(collection.IsDefault, false);
            collection.Clear();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.IsEmpty, true);
            Assert.AreEqual(collection.IsDefault, false);
        }
        {
            VL collection = CreateCollection3();
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(collection.IsEmpty, false);
            Assert.AreEqual(collection.IsDefault, false);
        }
    }

    [TestMethod]
    public void Capacity()
    {
        PrintTitle();
        {
            VL collection = CreateCollectionX();
            Assert.AreEqual(collection.Capacity, 0);
        }
        {
            VL collection = CreateCollection0();
            Assert.AreEqual(collection.Capacity, 0);
        }
        {
            VL collection = CreateCollection3();
            Assert.IsGreaterThanOrEqualTo(3, collection.Capacity);
        }
        {
            VL collection = CreateCollection10();
            Assert.AreEqual(10, collection.Capacity);
        }
        {
            VL collection = CreateCollection3();
            collection.Capacity = 10;
            Assert.AreEqual(collection.Capacity, 10);
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(collection.Count(), collection.Count);
        }
    }

    [TestMethod]
    public void Indexer()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        Assert.AreEqual(collection[0], 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => collection[3]);
        collection[0] = 10;
        Assert.AreEqual(collection[0], 10);
    }

    [TestMethod]
    public void GetItemRef()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        ref int item = ref collection.GetItemRef(0);
        Assert.IsTrue(item.IsNotNull);
        Assert.AreEqual(item, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => collection.GetItemRef(10));
    }

    [TestMethod]
    public void Add()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            ref int item = ref collection.Add(3);
            Assert.AreEqual(collection.Count, 4);
            Assert.IsTrue(collection.AsSpan().Contains(3));
            Assert.AreEqual(item, 3);
            item = 10; Assert.AreEqual(collection[3], 10);
            Assert.IsTrue(collection.AsSpan().Contains(10));
        }
        {
            VL collection = CreateCollectionX();
            ref int item = ref collection.Add(13);
            Assert.IsTrue(collection.SequenceEqual([13]));
        }
    }

    [TestMethod]
    public void Insert()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            ref int item = ref collection.Insert(0, 3);
            Assert.AreEqual(collection.Count, 4);
            Assert.IsTrue(collection.AsSpan().Contains(3));
            Assert.AreEqual(item, 3);
            item = 10; Assert.AreEqual(collection[0], 10);
            Assert.IsTrue(collection.AsSpan().Contains(10));
        }
        {
            VL collection = CreateCollectionX();
            ref int item = ref collection.Insert(0, 13);
            Assert.IsTrue(collection.SequenceEqual([13]));
        }
    }

    [TestMethod]
    public void AddRange()
    {
        PrintTitle();
        IEnumerable<int> newItems = Enumerable.Range(3, 3);
        List<IEnumerable<int>> facades = new()
        {
            newItems,
            newItems.ToArray(),
            newItems.HideIdentity()
        };
        foreach (IEnumerable<int> facade in facades)
        {
            Console.WriteLine($"AddRange with type {facade.GetType().Name}");
            {
                VL collection = CreateCollection3();
                collection.AddRange(facade);
                Assert.AreEqual(collection.Count, 6);
                Assert.IsTrue(collection.AsSpan().Contains(3));
            }
            {
                VL collection = CreateCollectionX();
                collection.AddRange(facade);
                Assert.IsTrue(collection.SequenceEqual(facade));
            }
        }
    }

    [TestMethod]
    public void RemoveAt()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        collection.RemoveAt(0);
        Assert.AreEqual(collection.Count, 2);
    }

    [TestMethod]
    public void RemoveWhere()
    {
        PrintTitle();
        {
            VL collection = CreateCollection10();
            int invokedCount = 0;
            int removed = collection.RemoveWhere(x =>
            {
                invokedCount++;
                return x % 2 == 0;
            });
            Assert.AreEqual(invokedCount, 10);
            Assert.AreEqual(removed, 5);
            Assert.AreEqual(collection.Count, 5);
            Assert.AreEqual(collection.Count(), collection.Count);
            Assert.IsTrue(collection.All(x => x % 2 != 0));
            Console.WriteLine(String.Join("\r\n", collection));
        }
        {
            VL collection = CreateCollectionX();
            int removed = collection.RemoveWhere(x => true);
            Assert.AreEqual(removed, 0);
        }
    }

    [TestMethod]
    public void Clear()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            collection.Clear();
            Assert.AreEqual(collection.Count, 0);
            Assert.IsFalse(collection.IsDefault);
        }
        {
            VL collection = CreateCollectionX();
            collection.Clear();
            Assert.AreEqual(collection.Count, 0);
            Assert.IsTrue(collection.IsDefault);
        }
    }

    [TestMethod]
    public void TrimExcess()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            collection.TrimExcess();
            Assert.AreEqual(collection.Capacity, 3);
            Assert.AreEqual(collection.Count, 3);
        }
        {
            VL collection = CreateCollectionX();
            collection.TrimExcess();
            Assert.AreEqual(collection.Capacity, 0);
            Assert.AreEqual(collection.Count, 0);
            Assert.IsTrue(collection.IsDefault);
        }
    }

    [TestMethod]
    public void SetCount()
    {
        PrintTitle();
        {
            VL collection = CreateCollectionX();
            collection.SetCount(0);
            Assert.AreEqual(collection.Capacity, 0);
            Assert.AreEqual(collection.Count, 0);
            Assert.IsTrue(collection.IsDefault);
        }
        {
            VL collection = CreateCollection3();
            collection.SetCount(6);
            Assert.AreEqual(collection.Capacity, 6);
            Assert.AreEqual(collection.Count, 6);
            Assert.IsTrue(collection.SequenceEqual([0, 1, 2, 0, 0, 0]));
        }
        {
            VL collection = CreateCollection10();
            collection.SetCount(5);
            Assert.AreEqual(collection.Capacity, 10);
            Assert.AreEqual(collection.Count, 5);
            Assert.IsTrue(collection.SequenceEqual([0, 1, 2, 3, 4]));
        }
        {
            VL collection = CreateCollection3();
            collection.SetCount(6, -1);
            Assert.AreEqual(collection.Capacity, 6);
            Assert.AreEqual(collection.Count, 6);
            Assert.IsTrue(collection.SequenceEqual([0, 1, 2, -1, -1, -1]));
        }
    }

    [TestMethod]
    public void CopyTo()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            int[] array = new int[10]; Array.Fill(array, -1);
            collection.CopyTo(array, 5);
            Assert.IsTrue(array.SequenceEqual([-1, -1, -1, -1, -1, 0, 1, 2, -1, -1]));
        }
        {
            VL collection = CreateCollectionX();
            int[] array = new int[10]; Array.Fill(array, -1);
            collection.CopyTo(array, 5);
            Assert.IsTrue(array.SequenceEqual([-1, -1, -1, -1, -1, -1, -1, -1, -1, -1]));
        }
    }

    [TestMethod]
    public void ToArray()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            int[] array = collection.ToArray();
            Assert.AreEqual(array.Length, 3);
            Assert.IsTrue(collection.SequenceEqual(array));
        }
        {
            VL collection = CreateCollectionX();
            int[] array = collection.ToArray();
            Assert.IsTrue(ReferenceEquals(array, Array.Empty<int>()));
        }
    }

    [TestMethod]
    public void AsSpan()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            Span<int> span = collection.AsSpan();
            Assert.AreEqual(span.Length, 3);
            Assert.IsTrue(collection.SequenceEqual(span.ToArray()));
        }
        {
            VL collection = CreateCollectionX();
            Span<int> span = collection.AsSpan();
            Assert.AreEqual(span.Length, 0);
        }
    }

    [TestMethod]
    public void AsEnumerable()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            ArraySegment<int> enumerable = collection.AsEnumerable();
            Assert.AreEqual(enumerable.Count, 3);
            Assert.IsTrue(enumerable.SequenceEqual(collection.AsSpan().ToArray()));
        }
        {
            VL collection = CreateCollectionX();
            ArraySegment<int> enumerable = collection.AsEnumerable();
            Assert.AreEqual(enumerable.Count, 0);
        }
    }

    [TestMethod]
    public void GetEnumerator()
    {
        PrintTitle();
        {
            VL collection = CreateCollection3();
            Span<int>.Enumerator enumerator = collection.GetEnumerator();
            Assert.IsTrue(Enumerate(enumerator).SequenceEqual(collection.ToArray()));
        }
        {
            VL collection = CreateCollectionX();
            Span<int>.Enumerator enumerator = collection.GetEnumerator();
            Assert.AreEqual(Enumerate(enumerator).Length, 0);
        }
        static int[] Enumerate(Span<int>.Enumerator enumerator)
        {
            List<int> items = new();
            using (enumerator)
                while (enumerator.MoveNext())
                    items.Add(enumerator.Current);
            return items.ToArray();
        }
    }

    [TestMethod]
    public void Equals()
    {
        PrintTitle();
        VL collectionX = CreateCollectionX();
        VL collection0 = CreateCollection0();
        VL collection3 = CreateCollection3();
        VL collection3b = CreateCollection3();
        Assert.IsTrue(collectionX.Equals(default));
        Assert.IsTrue(collectionX.Equals(collectionX));
        Assert.IsTrue(collection0.Equals(collection0));
        Assert.IsTrue(collection3.Equals(collection3));
        Assert.IsFalse(collection0.Equals(collectionX));
        Assert.IsFalse(collection0.Equals(default));
        Assert.IsFalse(collection0.Equals(collection3));
        Assert.IsFalse(collection3.Equals(collection3b));
    }

    #region Private Members

    /// <summary>
    /// Returns the default collection.
    /// </summary>
    private static VL CreateCollectionX() => default;

    /// <summary>
    /// Returns a collection with 0 items.
    /// </summary>
    private static VL CreateCollection0() => new();

    /// <summary>
    /// Returns a collection with 3 items.
    /// </summary>
    private static VL CreateCollection3() => new() { 0, 1, 2 };

    /// <summary>
    /// Returns a collection with 10 items.
    /// </summary>
    private static VL CreateCollection10() => new(Enumerable.Range(0, 10));

    #endregion

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
