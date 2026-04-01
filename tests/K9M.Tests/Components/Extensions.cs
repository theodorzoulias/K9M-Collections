using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BCL = System.Collections.Generic;

namespace K9M.Tests;

internal static class Extensions
{
    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<T> HideIdentity()
        {
            ArgumentNullException.ThrowIfNull(source);
            foreach (var item in source) yield return item;
        }
    }

    extension<T>(ICollection<T> source)
    {
        public T GetRandom(Random random = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (source.Count == 0) throw new InvalidOperationException("The collection is empty.");
            int randomIndex = (random ?? Random.Shared).Next(source.Count);
            return source.ElementAt(randomIndex);
        }
    }

    extension<T>(List<T> source)
    {
        public ref T GetValueRef(int index)
        {
            ArgumentNullException.ThrowIfNull(source);
            return ref CollectionsMarshal.AsSpan(source)[index];
        }

        public void SetCount(int count)
        {
            ArgumentNullException.ThrowIfNull(source);
            int oldCount = source.Count;
            CollectionsMarshal.SetCount(source, count);
            // Initialize the exposed data.
            if (count > oldCount)
                for (int i = oldCount; i < count; i++)
                    source[i] = default;
        }

        public void SetCount(int count, T emptyFiller)
        {
            ArgumentNullException.ThrowIfNull(source);
            int oldCount = source.Count;
            CollectionsMarshal.SetCount(source, count);
            if (count > oldCount)
                for (int i = oldCount; i < count; i++)
                    source[i] = emptyFiller;
        }
    }

    extension<T>(IList<T> source)
    {
        public T GetRandom(Random random = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (source.Count == 0) throw new InvalidOperationException("The collection is empty.");
            int randomIndex = (random ?? Random.Shared).Next(source.Count);
            return source[randomIndex];
        }
    }

    extension<TKey, TValue>(IDictionary<TKey, TValue> source)
    {
        /// <summary>
        /// To avoid switching between Remove and TryRemove.
        /// </summary>
        public bool TryRemove(TKey key)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.Remove(key);
        }
    }

    extension<TKey, TItem>(K9M.KeyedCollection<TKey, TItem> source)
    {
        public string Dump()
        {
            MethodInfo mi = typeof(K9M.KeyedCollection<TKey, TItem>).GetMethod("Dump", BindingFlags.NonPublic | BindingFlags.Instance);
            return (string)mi.Invoke(source, null);
        }
    }

    extension<T>(ValueList<T> source)
    {
        public bool IsDefault => source == default;
    }

    extension<TKey, TValue>(BCL.Dictionary<TKey, TValue> source) where TKey : notnull
    {
        public bool IsEmpty => source.Count == 0;

        public bool SetEquals(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(other);
            int count;
            if (other.TryGetNonEnumeratedCount(out count) && count != source.Count) return false;
            count = 0;
            BCL.HashSet<TKey> foundKeys = new(source.Comparer);
            foreach ((TKey key, TValue value) in other)
            {
                if (++count > source.Count) return false;
                if (!foundKeys.Add(key)) return false;
                if (!source.TryGetValue(key, out TValue v)) return false;
                if (!EqualityComparer<TValue>.Default.Equals(value, v)) return false;
            }
            if (count != source.Count) return false;
            return true;
        }

        public bool SetEquals(IEnumerable<TKey> other, IEqualityComparer<TKey> comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(other);
            int count;
            if (other.TryGetNonEnumeratedCount(out count) && count != source.Count) return false;
            count = 0;
            BCL.HashSet<TKey> foundKeys = new(comparer);
            foreach (TKey key in other)
            {
                if (++count > source.Count) return false;
                if (!foundKeys.Add(key)) return false;
                if (!source.ContainsKey(key)) return false;
            }
            if (count != source.Count) return false;
            return true;
        }

        public ref TValue GetValueRef(TKey key)
        {
            ArgumentNullException.ThrowIfNull(source);
            return ref CollectionsMarshal.GetValueRefOrNullRef(source, key);
        }

        public void UnionWith(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(collection);
            foreach (var (k, v) in collection) source.TryAdd(k, v);
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(source);
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(source, key, out bool exists);
            if (!exists) valueRef = value;
            return valueRef;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary by using the specified function
        /// if the key does not already exist. Returns the new value, or the
        /// existing value if the key exists.
        /// </summary>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(valueFactory);

            ref TValue value = ref CollectionsMarshal
                .GetValueRefOrAddDefault(source, key, out bool exists);
            if (!exists)
            {
                BCL.Dictionary<TKey, TValue>.Enumerator enumerator = source.GetEnumerator();
                try { value = valueFactory(key); enumerator.MoveNext(); }
                catch { source.Remove(key); throw; }
            }
            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary by using the specified function
        /// and an argument if the key does not already exist. Returns the new value,
        /// or the existing value if the key exists.
        /// </summary>
        public TValue GetOrAdd<TArg>(TKey key,
            Func<TKey, TArg, TValue> valueFactory,
            TArg factoryArgument)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(valueFactory);

            ref TValue value = ref CollectionsMarshal
                .GetValueRefOrAddDefault(source, key, out bool exists);
            if (!exists)
            {
                BCL.Dictionary<TKey, TValue>.Enumerator enumerator = source.GetEnumerator();
                try { value = valueFactory(key, factoryArgument); enumerator.MoveNext(); }
                catch { source.Remove(key); throw; }
            }
            return value;
        }

        public void AddOrReplace(TKey key, TValue addValue,
            Func<TKey, TValue, TValue> replaceValueFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(source, key, out bool exists);
            if (exists)
                valueRef = replaceValueFactory(key, valueRef);
            else
                valueRef = addValue;
        }

        public void AddOrReplace(TKey key,
            Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> replaceValueFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(source, key, out bool exists);
            if (exists)
                valueRef = replaceValueFactory(key, valueRef);
            else
                try { valueRef = addValueFactory(key); } catch { source.Remove(key); throw; }
        }

        public void AddOrReplace<TArg>(TKey key,
            Func<TKey, TArg, TValue> addValueFactory,
            Func<TKey, TValue, TArg, TValue> replaceValueFactory, TArg factoryArgument)
        {
            ArgumentNullException.ThrowIfNull(source);
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(source, key, out bool exists);
            if (exists)
                valueRef = replaceValueFactory(key, valueRef, factoryArgument);
            else
                try { valueRef = addValueFactory(key, factoryArgument); } catch { source.Remove(key); throw; }
        }

        public bool TryReplace(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(source);
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(source, key);
            if (Unsafe.IsNullRef(ref valueRef)) return false;
            valueRef = value;
            return true;
        }

        public bool TryReplace(TKey key,
            Func<TKey, TValue, TValue> replaceValueFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(source, key);
            if (Unsafe.IsNullRef(ref valueRef)) return false;
            valueRef = replaceValueFactory(key, valueRef);
            return true;
        }

        public bool TryReplace<TArg>(TKey key,
            Func<TKey, TValue, TArg, TValue> replaceValueFactory,
            TArg factoryArgument)
        {
            ArgumentNullException.ThrowIfNull(source);
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(source, key);
            if (Unsafe.IsNullRef(ref valueRef)) return false;
            valueRef = replaceValueFactory(key, valueRef, factoryArgument);
            return true;
        }

        public void ReplaceAll(Func<TKey, TValue, TValue> replaceValueFactory)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(replaceValueFactory);
            foreach (var (k, v) in source)
            {
                source[k] = replaceValueFactory(k, v);
            }
        }

        // Replaces both key and value.
        public void ReplaceAll(Func<TKey, TValue, TValue> replaceValueFactory, Func<TValue, TKey> keySelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(replaceValueFactory);
            ArgumentNullException.ThrowIfNull(keySelector);
            foreach (var (k, v) in source.ToArray())
            {
                var newValue = replaceValueFactory(k, v);
                var newKey = keySelector(newValue);
                source.Remove(k);
                try { source.Add(newKey, newValue); }
                catch { source.Add(k, v); throw; }
            }
        }

        public int RemoveWhere(Func<TKey, TValue, bool> match)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(match);
            int removedCount = 0;
            foreach (var (k, v) in source)
            {
                if (match(k, v))
                {
                    bool removed = source.Remove(k);
                    Debug.Assert(removed);
                    if (removed) removedCount++;
                }
            }
            return removedCount;
        }

        public KeyValuePair<TKey, TValue> GetRandomUnfair(Random random = null)
            => DictionaryRandomizer<TKey, TValue>.GetRandom(source, random);
    }
}

internal static class DictionaryRandomizer<TKey, TValue> where TKey : notnull
{
    private static readonly FieldInfo _countField;
    private static readonly FieldInfo _indexField;

    static DictionaryRandomizer()
    {
        const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;
        _countField = typeof(BCL.Dictionary<TKey, TValue>).GetField("_count", FLAGS);
        _indexField = typeof(BCL.Dictionary<TKey, TValue>.Enumerator).GetField("_index", FLAGS);
    }

    public static KeyValuePair<TKey, TValue> GetRandom(BCL.Dictionary<TKey, TValue> source, Random random = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (_countField is null)
            throw new NotSupportedException("FieldInfo _count not found.");
        if (_indexField is null)
            throw new NotSupportedException("FieldInfo _index not found.");
        if (source.Count > 0)
        {
            random ??= Random.Shared;
            int count = (int)_countField.GetValue(source);
            int randomIndex = random.Next(0, count);
            using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = source.GetEnumerator())
            {
                _indexField.SetValue(enumerator, randomIndex);
                if (enumerator.MoveNext()) return enumerator.Current;
            }
            // Not found. Get the first item.
            foreach (KeyValuePair<TKey, TValue> item in source) return item;
        }
        throw new InvalidOperationException("The dictionary is empty.");
    }
}
