using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace K9M.Tests;

[TestClass]
public partial class ValueList_Main
{
    [TestMethod]
    public void Constructors()
    {
        PrintTitle();
        ValueList<int> collection;
        var items = Enumerable.Range(1, 10);

        collection = new();
        Assertions(collection, 0, 0);

        collection = new(10);
        Assertions(collection, 0, 10);

        collection = new(items);
        Assertions(collection, items.Count(), items.Count());

        collection = new(items.HideIdentity());
        Assertions(collection, items.Count(), items.Count());

        collection = new(items.ToArray());
        Assertions(collection, items.Count(), items.Count());

        static void Assertions<T>(ValueList<T> collection, int count, int capacity)
        {
            Assert.AreEqual(collection.Count, count);
            if (capacity == 0)
                Assert.AreEqual(collection.Capacity, capacity);
            else
                Assert.IsTrue(collection.Capacity >= capacity, $"{collection.Capacity}, {capacity}");
        }
    }

    [TestMethod]
    public void ArgumentExceptions()
    {
        PrintTitle();
        ValueList<int> collection = new() { 1, 2, 3 };

        Assert.That.ThrowsArgumentOutOfRangeException(() => collection = new(-1), "capacity");
        Assert.That.ThrowsArgumentNullException(() => collection = new(items: null), "items");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection.Capacity = -1, "value");
        Assert.Throws<OutOfMemoryException>(() => collection.Capacity = Int32.MaxValue);

        Assert.That.ThrowsArgumentOutOfRangeException(() => _ = collection[-1], "index");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection[-1] = -1, "index");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection.RemoveAt(-1), "index");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection.SetCount(-1), "count");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection.Insert(-1, -1), "index");
        Assert.That.ThrowsArgumentNullException(() => collection.AddRange(null), "items");
        Assert.That.ThrowsArgumentOutOfRangeException(() => collection.GetItemRef(-1), "index");
        Assert.That.ThrowsArgumentNullException(() => collection.CopyTo(null, 0), "array");
        Assert.That.ThrowsArgumentException(() => collection.CopyTo([], 1), "destinationArray");
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
            IEnumerable<int> items = Enumerable.Range(1, size);
            IEnumerable<int> enumerable = items.HideIdentity();
            int[] array = Enumerable.Range(1, size).ToArray();

            ValueList<int> collection = default;
            (string Title, bool ExpectedTheMinimum, Action Create)[] actions =
            [
                ("Using the constructor with ICollection", true, () => collection = new(items)),
                ("Using the constructor with array", true, () => collection = new(array)),
                ("Using the constructor with enumerable", false, () => collection = new(enumerable)),
                ("Adding one by one", false, () => { collection = new(); foreach (var item in items) collection.Add(item); }),
            ];
            foreach (var (title, expectedTheMinimum, createDictionary) in actions)
            {
                createDictionary();
                Console.WriteLine($"- {title.PadRight(38)} Count: {collection.Count:#,0}, Capacity: {collection.Capacity:#,0}");
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
            IEnumerable<int> items = Enumerable.Range(1, size);
            ValueList<int> collection = new(items);
            int maxCount = collection.Count;
            int maxCapacity = collection.Capacity;
            collection.Clear();
            collection.TrimExcess();
            Console.WriteLine($"Size: {size:#,0}, Max count: {maxCount:#,0}, Max capacity: {maxCapacity:#,0}, Current capacity: {collection.Capacity:#,0}");
            Assert.AreEqual(collection.Capacity, 0);
        }
    }

    [TestMethod]
    public void Box()
    {
        PrintTitle();
        ValueList<int> list = new() { 1, 2, 3 };
        ValueList<int>.Box box = list.Wrap();
        box.List.Insert(0, 13);
        box.List.Add(42);
        Assert.AreEqual(list.Count, 3);
        Assert.AreEqual(box.List.Count, 5);
        Assert.IsFalse(box.List.SequenceEqual(list));
        ValueList<int> unboxed = box.Unwrap();
        Assert.IsFalse(unboxed.Equals(list));
    }

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
