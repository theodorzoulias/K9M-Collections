using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace K9M.Tests;

public class EverythingTester
{
    private int _operationsCount = 0;
    private int _randomSeed;
    private Random _random;
    private Randomizer<Action> _randomizer;
    private Dictionary<string, int> _statistics = new();

    protected EverythingTester(int? randomSeed)
    {
        _statistics = new();
        _randomSeed = randomSeed ?? Random.Shared.Next(1000, 10000);
        _random = new(_randomSeed);
        _randomizer = new(_random);
    }

    protected Random Random => _random;
    protected Randomizer<Action> Randomizer => _randomizer;
    protected int OperationsCount => _operationsCount;

    public virtual void Run(int iterations)
    {
        Console.WriteLine($"Random seed: #{_randomSeed}");
        while (_operationsCount < iterations)
        {
            _randomizer.GetRandom().Invoke();
        }
        Console.WriteLine($"Statistics:\r\n  {String.Join("\r\n  ", _statistics.OrderBy(e => e.Key).Select(e => $"{e.Key}: {e.Value:#,0}"))}");
        Console.WriteLine($"Total: {_statistics.Select(e => e.Value).Sum():#,0}");
    }

    protected void OperationCompleted([CallerMemberName] string callerName = "")
    {
        _operationsCount++;
        _statistics.AddOrReplace(callerName, 1, (_, x) => x + 1);
        OnOperationCompleted();
    }

    protected virtual void OnOperationCompleted() { }
}
