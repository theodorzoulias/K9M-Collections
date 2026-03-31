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
    public void Count_IsEmpty_IsDefault()
    {
        PrintTitle();
        {
            VL collection = CreateEmptyCollection();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.IsEmpty, true);
            Assert.AreEqual(collection.IsDefault, true);
        }
        {
            VL collection = CreateCollection3();
            Assert.AreEqual(collection.Count, 3);
            Assert.AreEqual(collection.IsEmpty, false);
            Assert.AreEqual(collection.IsDefault, false);
        }
        {
            VL collection = CreateEmptyCollection();
            collection.Clear();
            Assert.AreEqual(collection.Count, 0);
            Assert.AreEqual(collection.IsEmpty, true);
            Assert.AreEqual(collection.IsDefault, false);
        }
    }

    [TestMethod]
    public void Capacity()
    {
        PrintTitle();
        {
            VL collection = CreateEmptyCollection();
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
        VL collection = CreateCollection3();
        ref int item = ref collection.Add(3);
        Assert.AreEqual(collection.Count, 4);
        Assert.IsTrue(collection.AsSpan().Contains(3));
        Assert.AreEqual(item, 3);
        item = 10; Assert.AreEqual(collection[3], 10);
        Assert.IsTrue(collection.AsSpan().Contains(10));
    }

    [TestMethod]
    public void Insert()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        ref int item = ref collection.Insert(0, 3);
        Assert.AreEqual(collection.Count, 4);
        Assert.IsTrue(collection.AsSpan().Contains(3));
        Assert.AreEqual(item, 3);
        item = 10; Assert.AreEqual(collection[0], 10);
        Assert.IsTrue(collection.AsSpan().Contains(10));
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
            VL collection = CreateCollection3();
            collection.AddRange(facade);
            Assert.AreEqual(collection.Count, 6);
            Assert.IsTrue(collection.AsSpan().Contains(3));
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

    [TestMethod]
    public void Clear()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        collection.Clear();
        Assert.AreEqual(collection.Count, 0);
        Assert.AreEqual(collection.Count(), collection.Count);
    }

    [TestMethod]
    public void TrimExcess()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        collection.TrimExcess();
        Assert.AreEqual(collection.Capacity, 3);
        Assert.AreEqual(collection.Count, 3);
        Assert.AreEqual(collection.Count(), collection.Count);
    }

    [TestMethod]
    public void SetCount()
    {
        PrintTitle();
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
        VL collection = CreateCollection3();
        int[] array = new int[10]; Array.Fill(array, -1);
        collection.CopyTo(array, 5);
        Assert.IsTrue(array.SequenceEqual([-1, -1, -1, -1, -1, 0, 1, 2, -1, -1]));
    }

    [TestMethod]
    public void ToArray()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        int[] array = collection.ToArray();
        Assert.AreEqual(array.Length, 3);
        Assert.IsTrue(collection.SequenceEqual(array));
    }

    [TestMethod]
    public void AsSpan()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        Span<int> span = collection.AsSpan();
        Assert.AreEqual(span.Length, 3);
        Assert.IsTrue(collection.SequenceEqual(span.ToArray()));
    }

    [TestMethod]
    public void AsEnumerable()
    {
        PrintTitle();
        VL collection = CreateCollection3();
        ArraySegment<int> enumerable = collection.AsEnumerable();
        Assert.AreEqual(enumerable.Count, 3);
        Assert.IsTrue(enumerable.SequenceEqual(collection.AsSpan().ToArray()));
    }

    [TestMethod]
    public void Equals()
    {
        PrintTitle();
        VL collection0 = CreateEmptyCollection();
        VL collection1 = CreateCollection3();
        VL collection2 = CreateCollection3();
        Assert.IsTrue(collection0.Equals(collection0));
        Assert.IsTrue(collection1.Equals(collection1));
        Assert.IsFalse(collection0.Equals(collection1));
        Assert.IsFalse(collection1.Equals(collection2));
    }

    #region Private Members

    private static VL CreateEmptyCollection() => new();

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
