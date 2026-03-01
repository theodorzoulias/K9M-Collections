using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace K9M.Tests;

public static class UF
{
    private static string FrameworkDescription => RuntimeInformation.FrameworkDescription;

    public static void PrintTitle(MethodBase methodBase, string callerName)
    {
        Console.WriteLine($"{methodBase.ReflectedType.Name} - {callerName} ({FrameworkDescription}, {Build}) {DateTime.Now:yyyy.MM.dd HH:mm:ss}");
    }

    public static string Build
#if DEBUG
        => "Debug";
#else
        => "Release";
#endif
}
