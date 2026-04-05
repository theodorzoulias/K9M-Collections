using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace K9M;

internal static class CapacityHelper
{
    /// <summary>
    /// Grows the capacity to a number equal or larger to both the given minimum,
    /// and to the given current * minimumGrowth.
    /// The resulting number is either a power of 2, or the given maximum.
    /// An exception is thrown if the minimum is larger than the maximum.
    /// </summary>
    /// <exception cref="OutOfMemoryException"></exception>
    public static int GrowToPowerOfTwo(int current, int minimum, int maximum, double minimumGrowth)
    {
        Debug.Assert(current >= 0);
        Debug.Assert(minimum > current);
        Debug.Assert(maximum >= 0);
        Debug.Assert(minimumGrowth >= 1.0);
        if (minimum > maximum) ThrowOutOfMemoryException();
        uint combinedMin = (uint)Math.Clamp(Math.Max(minimum, current * minimumGrowth), 0, maximum);
        uint result = BitOperations.RoundUpToPowerOf2(combinedMin);
        Debug.Assert(result >= (uint)minimum);
        return (int)Math.Clamp(result, (uint)minimum, (uint)maximum);
    }

    /// <summary>
    /// Grows the capacity to a number equal to the given minimum.
    /// An exception is thrown if the minimum is larger than the maximum.
    /// </summary>
    /// <exception cref="OutOfMemoryException"></exception>
    public static int GrowToExactNumber(int minimum, int maximum)
    {
        Debug.Assert(minimum > 0);
        if (minimum > maximum) ThrowOutOfMemoryException();
        return minimum;
    }

    [DoesNotReturn]
    public static void ThrowOutOfMemoryException()
    {
        throw new OutOfMemoryException("Capacity overflow.");
    }
}
