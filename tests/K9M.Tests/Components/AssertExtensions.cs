using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BCL = System.Collections.Generic;

namespace K9M.Tests;

internal static class AssertExtensions
{
    public static ArgumentNullException ThrowsArgumentNullException(this Assert source, Action action, string expectedTextInsideMessage)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(expectedTextInsideMessage);
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(action);
        Assert.IsTrue(exception is not null);
        Assert.IsTrue(exception.Message.Contains(expectedTextInsideMessage), $"Message: {exception.Message}\r\nExpected text inside message: {expectedTextInsideMessage}");
        return exception;
    }

    public static ArgumentOutOfRangeException ThrowsArgumentOutOfRangeException(this Assert source, Action action, string expectedTextInsideMessage)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(expectedTextInsideMessage);
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(action);
        Assert.IsTrue(exception is not null);
        Assert.IsTrue(exception.Message.Contains(expectedTextInsideMessage), $"Message: {exception.Message}\r\nExpected text inside message: {expectedTextInsideMessage}");
        return exception;
    }
}
