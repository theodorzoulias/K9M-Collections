using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BCL = System.Collections.Generic;
using K9M.NullRef;

namespace K9M.Tests;

[TestClass]
public class KeyedCollection_Enumerator
{
    [TestMethod]
    public void TestEnumeration()
    {
        PrintTitle();
        (int, int)[] items = [(1, 1), (2, 2), (3, 3)];
        K9M.KeyedCollection<int, (int, int)> collection = new(x => x.Item1, items);
        var enumerator = collection.GetEnumerator();
        int movedCount = 0;
        for (int i = -1; i <= items.Length; i++)
        {
            // Use the Current multiple times.
            for (int j = 0; j < 3; j++)
            {
                ref var item = ref enumerator.Current;
                if (i >= 0 && i < items.Length)
                {
                    Assert.IsTrue(item.IsNotNull);
                    Assert.IsTrue(item == items[i]);
                }
                else
                {
                    Assert.IsTrue(item.IsNull);
                }
            }
            bool moved = enumerator.MoveNext();
            if (moved) movedCount++;
        }
        Assert.AreEqual(movedCount, items.Length);
    }

    [TestMethod]
    public void ModifyingCollectionInvalidatesEnumerators()
    {
        PrintTitle();
        (int, int)[] items = [(1, 1), (2, 2), (3, 3)];
        K9M.KeyedCollection<int, (int, int)> collection = new(x => x.Item1, items);
        List<(string Name, Action Action, bool ShouldInvalidate)> actions = new();
        actions.Add(("ContainsKey", () => collection.ContainsKey(1), false));
        actions.Add(("TryReplace", () => collection.TryReplace((1, 11)), false));
        actions.Add(("AddOrReplace", () => collection.AddOrReplace((4, 4)), true));
        actions.Add(("Add", () => collection.Add((5, 5)), true));
        actions.Add(("TryRemove", () => collection.TryRemove(1), false));
        actions.Add(("GetOrAdd", () => collection.GetOrAdd((6, 6)), true));
        actions.Add(("EnsureCapacity", () => collection.EnsureCapacity(20), true));
        actions.Add(("TrimExcess", () => collection.TrimExcess(), true));
        foreach (var (name, action, shouldInvalidate) in actions)
        {
            string message = $"Action {name} should{(shouldInvalidate ? "" : " not")} invalidate enumerators";
            Console.WriteLine(message);
            var enumerator = collection.GetEnumerator();
            enumerator.MoveNext();
            action();
            Exception exception = null;
            try { enumerator.MoveNext(); } catch (Exception ex) { exception = ex; }
            if (shouldInvalidate)
            {
                Assert.IsNotNull(exception, message);
                Assert.IsInstanceOfType<InvalidOperationException>(exception, message);
            }
            else
                Assert.IsNull(exception, message);
        }
    }

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
