using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BCL = System.Collections.Generic;

namespace K9M.Tests;

[TestClass]
public class KeyedCollection_ICollection
{
    [TestMethod]
    public void TestEverything()
    {
        PrintTitle();
        new KeyedCollectionICollectionEverythingTester(randomSeed: null).Run(iterations: 2_000);
    }

    public class KeyedCollectionICollectionEverythingTester : EverythingTester
    {
        private readonly ICollection<(int, int)> _collection1 = new K9M.KeyedCollection<int, (int, int)>(x => x.Item1);
        private readonly BCL.HashSet<(int, int)> _set2 = new();
        private const int SizeLimit = 150;
        private int _maxSize = 0;

        private ICollection<(int, int)> _collection2 => _set2;

        public KeyedCollectionICollectionEverythingTester(int? randomSeed) : base(randomSeed)
        {
            Randomizer.Add(Add, 1000);
            Randomizer.Add(Remove, 400);
            Randomizer.Add(Contains, 1000);
            Randomizer.Add(CopyTo, 50);
            Randomizer.Add(Clear, 10);
        }

        public override void Run(int iterations)
        {
            base.Run(iterations);
            Console.WriteLine($"MaxSize: {_maxSize:#,0}");
        }

        protected override void OnOperationCompleted()
        {
            _maxSize = Math.Max(_maxSize, _collection2.Count);
            if (_collection2.Count >= SizeLimit) Clear();
        }

        private void Add()
        {
            var item = GetNonExistentItem();
            _collection1.Add(item);
            _collection2.Add(item);
            AssertIdentical();
            OperationCompleted();
        }

        private void Remove()
        {
            var item = GetFiftyFiftyItem();
            bool result1 = _collection1.Remove(item);
            bool result2 = _collection2.Remove(item);
            Assert.AreEqual(result1, result2);
            if (result2) AssertIdentical();
            OperationCompleted();
        }

        private void Contains()
        {
            var item = GetFiftyFiftyItem();
            Assert.AreEqual(_collection1.Contains(item), _collection2.Contains(item));
            OperationCompleted();
        }

        private void CopyTo()
        {
            (int, int)[] array1 = new (int, int)[_collection1.Count];
            _collection1.CopyTo(array1, 0);
            (int, int)[] array2 = new (int, int)[_collection2.Count];
            _collection2.CopyTo(array2, 0);
            Assert.IsTrue(array1.ToHashSet().SetEquals(array2));
            OperationCompleted();
        }

        private void Clear()
        {
            _collection1.Clear();
            _collection2.Clear();
            AssertIdentical();
            OperationCompleted();
        }

        private bool TryGetExistingItem(out (int, int) item)
        {
            if (_collection2.Count == 0) { item = default; return false; }
            item = _collection2.GetRandom(Random); return true;
        }

        private (int, int) GetFiftyFiftyItem()
            => (Random.NextDouble() < 0.5 && TryGetExistingItem(out var item)) ? item : GetNonExistentItem();

        private (int, int) GetNonExistentItem()
        {
            while (true)
            {
                int randomKey = Random.Next();
                var randomItem = (randomKey, Random.Next());
                if (!_collection2.Contains(randomItem)) return randomItem;
            }
        }

        private void AssertIdentical(string info = default, [CallerMemberName] string callerName = "")
        {
            Assert.AreEqual(_collection1.Count, _collection2.Count, callerName);
            Assert.IsTrue(_set2.SetEquals(_collection1), callerName);
        }
    }

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
