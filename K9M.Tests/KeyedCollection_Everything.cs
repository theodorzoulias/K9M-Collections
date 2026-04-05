using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BCL = System.Collections.Generic;

namespace K9M.Tests;

[TestClass]
public class KeyedCollection_Everything
{
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

    #region Utility Methods

    private void PrintTitle([CallerMemberName] string callerName = "") => UF.PrintTitle(MethodBase.GetCurrentMethod(), callerName);

    #endregion
}
