using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace K9M;

internal class ReadOnlyKeyedCollection<TKey, TItem>(
    K9M.KeyedCollection<TKey, TItem> source) : IReadOnlyDictionary<TKey, TItem>
    where TKey : notnull where TItem : notnull
{
    public int Count => source.Count;
    public TItem this[TKey key] => source[key];
    public bool ContainsKey(TKey key) => source.ContainsKey(key);
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TItem value) => source.TryGetItem(key, out value);

    public IEnumerator<KeyValuePair<TKey, TItem>> GetEnumerator() => source.EnumerateKeyValuePairs();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<TKey> Keys => source.EnumerateKeys();
    public IEnumerable<TItem> Values => source.AsEnumerable();
}
