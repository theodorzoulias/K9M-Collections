using System;
using System.Diagnostics;

namespace K9M;

internal static class MathHelper
{
    /// <summary>
    /// Returns the next prime number that is equal or greater than the given minimum number,
    /// and also equal or less than the given maximum number, ignoring the prime number 2.
    /// Returns false if the next prime number is greater than the given maximum number.
    /// </summary>
    public static bool TryGetPrime(int minimumNumber, int maximumNumber, out int result)
    {
        Debug.Assert(minimumNumber >= 0);
        Debug.Assert(maximumNumber >= 0);

        // The i += 2 can't overflow because the 'i' is odd.
        // The Int32.MaxValue is also odd (2147483647), and it's also a prime.
        for (int i = (minimumNumber | 1); i <= maximumNumber; i += 2)
        {
            Debug.Assert(i > 0);
            if (IsPrime_OddNumber(i)) { result = i; return true; }
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Computes if the given number is prime. The given number should be odd and positive.
    /// </summary>
    private static bool IsPrime_OddNumber(int number)
    {
        Debug.Assert(number >= 0);
        Debug.Assert(number % 2 == 1);

        int limit = (int)Math.Sqrt(number);
        // The divisor += 2 can't overflow because the `limit` is far below the Int32.MaxValue.
        for (int divisor = 3; divisor <= limit; divisor += 2)
        {
            if (number % divisor == 0) return false;
        }
        // The body of the `for` loop is skipped for numbers smaller than 9.
        // All odd numbers between 3 and 7 are prime.
        return number >= 3;
    }
}
