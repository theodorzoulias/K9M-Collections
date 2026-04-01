using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace K9M.Tests;

[TestClass]
public class ValueList_Everything
{
    [TestMethod]
    public void TestEverything()
    {
        PrintTitle();
        new KeyedCollectionEverythingTester(randomSeed: null).Run(iterations: 30_000);
    }

    public class KeyedCollectionEverythingTester : EverythingTester
    {
        private ValueList<int> _valueList = new();
        private readonly List<int> _standardList = new();
        private const int SizeLimit = 150;
        private int _maxSize = 0;

        public KeyedCollectionEverythingTester(int? randomSeed) : base(randomSeed)
        {
            Randomizer.Add(Getter, 500);
            Randomizer.Add(Setter, 500);
            Randomizer.Add(GetItemRef, 200);

            Randomizer.Add(Add, 1000);
            Randomizer.Add(Insert, 200);
            Randomizer.Add(AddRange, 100);
            Randomizer.Add(RemoveAt, 400);
            Randomizer.Add(RemoveWhere, 50);

            Randomizer.Add(Clear, 10);
            Randomizer.Add(TrimExcess, 10);
            Randomizer.Add(SetCapacity, 10);
            Randomizer.Add(SetCount, 20);
            Randomizer.Add(ToArray, 100);

            Randomizer.Add(SetDefault, 20);
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

        private void GetItemRef()
        {
            if (!TryGetExistingIndex(out int index)) return;
            ref int valueRef1 = ref _valueList.GetItemRef(index);
            valueRef1++;
            ref int valueRef2 = ref _standardList.GetValueRef(index);
            valueRef2++;
            AssertIdentical();
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

        private void AddRange()
        {
            int[] newItemsArray = Enumerable.Range(0, Random.Next(0, 100))
                .Select(_ => Random.Next()).ToArray();
            IEnumerable<int> facade = Random.NextDouble() switch
            {
                < 0.33 => newItemsArray,
                < 0.66 => newItemsArray.AsReadOnly(),
                _ => newItemsArray.HideIdentity()
            };
            _valueList.AddRange(facade);
            _standardList.AddRange(facade);
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

        private void RemoveWhere()
        {
            _valueList.RemoveWhere(x => x % 5 == 0);
            _standardList.RemoveAll(x => x % 5 == 0);
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

        private void SetDefault()
        {
            _valueList = default;
            _standardList.Clear();
            AssertIdentical();
            OperationCompleted();
        }

        private void TrimExcess()
        {
            _valueList.TrimExcess();
            _standardList.TrimExcess();
            AssertIdentical();
            OperationCompleted();
        }

        private void SetCapacity()
        {
            int newCapacity = _valueList.Count + Random.Next(0, 100);
            _valueList.Capacity = newCapacity;
            _standardList.Capacity = newCapacity;
            AssertIdentical();
            OperationCompleted();
        }

        private void SetCount()
        {
            int newCount = Random.Next(0, _valueList.Count * 2);
            if (Random.NextDouble() < 0.5)
            {
                _valueList.SetCount(newCount);
                _standardList.SetCount(newCount);
            }
            else
            {
                int emptyFiller = Random.Next();
                _valueList.SetCount(newCount, emptyFiller);
                _standardList.SetCount(newCount, emptyFiller);
            }
            AssertIdentical();
            OperationCompleted();
        }

        private void ToArray()
        {
            Assert.IsTrue(_valueList.ToArray().ToHashSet().SetEquals(_standardList.ToArray()));
            OperationCompleted();
        }

        private bool TryGetExistingIndex(out int item)
        {
            if (_standardList.Count == 0) { item = default; return false; }
            item = Random.Next(0, _standardList.Count); return true;
        }

        private void AssertIdentical(string info = default, [CallerMemberName] string callerName = "")
        {
            Assert.AreEqual(_valueList.Count, _standardList.Count, callerName);
            Assert.AreEqual(_valueList.IsEmpty, _standardList.Count == 0, callerName);
            Assert.IsTrue(_standardList.SequenceEqual(_valueList), callerName);
        }
    }

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
