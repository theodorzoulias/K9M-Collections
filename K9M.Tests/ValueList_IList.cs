using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace K9M.Tests;

[TestClass]
public class ValueList_IList
{
    [TestMethod]
    public void TestEverything()
    {
        PrintTitle();
        new KeyedCollectionEverythingTester(randomSeed: null).Run(iterations: 30_000);
    }

    public class KeyedCollectionEverythingTester : EverythingTester
    {
        private IList<int> _valueList = new ValueList<int>();
        private readonly IList<int> _standardList = new List<int>();
        private const int SizeLimit = 150;
        private int _maxSize = 0;

        public KeyedCollectionEverythingTester(int? randomSeed) : base(randomSeed)
        {
            Randomizer.Add(Contains, 500);
            Randomizer.Add(IndexOf, 500);
            Randomizer.Add(Getter, 500);
            Randomizer.Add(Setter, 500);
            Randomizer.Add(Add, 1000);
            Randomizer.Add(Insert, 200);
            Randomizer.Add(RemoveAt, 400);
            Randomizer.Add(Clear, 10);
        }

        public override void Run(int iterations)
        {
            base.Run(iterations);
            Console.WriteLine($"MaxSize: {_maxSize:#,0}");
        }

        protected override void OnOperationCompleted()
        {
            _maxSize = Math.Max(_maxSize, _standardList.Count);
            if (_standardList.Count >= SizeLimit) Clear();
        }

        private void Contains()
        {
            int key = GetFiftyFiftyItem();
            Assert.AreEqual(_valueList.Contains(key), _standardList.Contains(key));
            OperationCompleted();
        }

        private void IndexOf()
        {
            int key = GetFiftyFiftyItem();
            Assert.AreEqual(_valueList.IndexOf(key), _standardList.IndexOf(key));
            OperationCompleted();
        }

        private void Getter()
        {
            if (!TryGetExistingIndex(out int index)) return;
            Assert.AreEqual(_valueList[index], _standardList[index]);
            OperationCompleted();
        }

        private void Setter()
        {
            if (!TryGetExistingIndex(out int index)) return;
            int item = Random.Next();
            _valueList[index] = item;
            _standardList[index] = item;
            Assert.AreEqual(_valueList[index], _standardList[index]);
            OperationCompleted();
        }

        private void Add()
        {
            int item = Random.Next();
            _valueList.Add(item);
            _standardList.Add(item);
            AssertIdentical();
            OperationCompleted();
        }

        private void Insert()
        {
            int index = Random.Next(0, _standardList.Count + 1);
            int item = Random.Next();
            _valueList.Insert(index, item);
            _standardList.Insert(index, item);
            AssertIdentical();
            OperationCompleted();
        }

        private void RemoveAt()
        {
            if (!TryGetExistingIndex(out int index)) return;
            _valueList.RemoveAt(index);
            _standardList.RemoveAt(index);
            AssertIdentical();
            OperationCompleted();
        }

        private void Clear()
        {
            _valueList.Clear();
            _standardList.Clear();
            AssertIdentical();
            OperationCompleted();
        }

        private bool TryGetExistingIndex(out int item)
        {
            if (_standardList.Count == 0) { item = default; return false; }
            item = Random.Next(0, _standardList.Count); return true;
        }

        private bool TryGetExistingItem(out int item)
        {
            if (_standardList.Count == 0) { item = default; return false; }
            item = _standardList.GetRandom(Random); return true;
        }

        private int GetFiftyFiftyItem()
            => (Random.NextDouble() < 0.5 && TryGetExistingItem(out var key)) ? key : GetNonExistentItem();

        private int GetNonExistentItem()
        {
            while (true)
            {
                int randomItem = Random.Next();
                if (!_standardList.Contains(randomItem)) return randomItem;
            }
        }

        private void AssertIdentical(string info = default, [CallerMemberName] string callerName = "")
        {
            Assert.AreEqual(_valueList.Count, _standardList.Count, callerName);
            Assert.IsTrue(_standardList.SequenceEqual(_valueList), callerName);
        }
    }

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
