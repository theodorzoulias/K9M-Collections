using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace K9M.Tests;

public class Randomizer<T> : IEnumerable<KeyValuePair<T, double>>
{
    private readonly record struct Entry(T Item, double Probability, double Accumulated);

    private readonly List<Entry> _list = new();
    private readonly Random _random;
    private double _totalProbability;

    private static IComparer<Entry> s_Comparer = Comparer<Entry>
        .Create((Comparison<Entry>)((x, y) => x.Accumulated.CompareTo(y.Accumulated)));

    public Randomizer(Random random = default)
    {
        _random = random ?? new Random();
    }

    public void Add(T item, double probability)
    {
        if (probability < 0) throw new ArgumentOutOfRangeException(nameof(probability));
        _totalProbability += probability;
        _list.Add(new(item, probability, _totalProbability));
    }

    public T GetRandom()
    {
        if (_list.Count == 0) throw new InvalidOperationException();
        int index;
        if (_totalProbability > 0)
        {
            double value = _random.NextDouble() * _totalProbability;
            index = _list.BinarySearch(new Entry(default, default, value), s_Comparer);
            if (index < 0) index = ~index;
            if (index < _list.Count) return _list[index].Item;
        }
        index = _random.Next(_list.Count);
        return _list[index].Item;
    }

    public IEnumerator<KeyValuePair<T, double>> GetEnumerator() => _list
        .Select(e => KeyValuePair.Create(e.Item, e.Probability)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
