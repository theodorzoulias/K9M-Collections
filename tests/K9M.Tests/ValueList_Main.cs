using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace K9M.Tests;

[TestClass]
public class ValueList_Main
{
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
    public void DefaultInstanceLifecycle()
    {
        PrintTitle();
        ValueList<int> collection = default;
        Assert.IsTrue(collection == default);
        collection.AddRange([]);
        Assert.IsTrue(collection == default);
        collection.Clear();
        Assert.IsTrue(collection == default);
        collection.CopyTo([], 0);
        Assert.IsTrue(collection == default);
        collection.RemoveWhere(_ => true);
        Assert.IsTrue(collection == default);
        collection.SetCount(0);
        Assert.IsTrue(collection == default);
        collection.Capacity = 0;
        Assert.IsTrue(collection == default);
        collection.TrimExcess();
        Assert.IsTrue(collection == default);
        _ = collection.ToArray();
        Assert.IsTrue(collection == default);
        _ = collection.GetEnumerator();
        Assert.IsTrue(collection == default);
        collection.Add(13);
        Assert.IsTrue(collection != default);
    }

    [TestMethod]
    public void AddRange_Capacity()
    {
        PrintTitle();
        int[] sizes = [0, 10, 100];
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
                ("AddRange on new() with ICollection", true, () => { collection = new(); collection.AddRange(items); }),
                ("AddRange on new() with array", true, () => { collection = new(); collection.AddRange(array); }),
                ("AddRange on new() with enumerable", false, () => { collection = new(); collection.AddRange(enumerable); }),
                ("AddRange on default with ICollection", true, () => { collection = default; collection.AddRange(items); }),
                ("AddRange on default with array", true, () => { collection = default; collection.AddRange(array); }),
                ("AddRange on default with enumerable", false, () => { collection = default; collection.AddRange(enumerable); }),
                ("Adding one by one on new()", false, () => { collection = new(); foreach (var item in items) collection.Add(item); }),
                ("Adding one by one on default", false, () => { collection = default; foreach (var item in items) collection.Add(item); }),
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
        int?[] sizes = [null, 0, 1, 10, 100, 1000];
        foreach (var size in sizes)
        {
            IEnumerable<int> items = Enumerable.Range(1, size ?? 0);
            ValueList<int> collection = size is null ? default : new(items);
            int maxCount = collection.Count;
            int maxCapacity = collection.Capacity;
            collection.Clear();
            collection.TrimExcess();
            Console.WriteLine($"Size: {size?.ToString("#,0") ?? "(default)"}, Max count: {maxCount:#,0}, Max capacity: {maxCapacity:#,0}, Current capacity: {collection.Capacity:#,0}");
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
