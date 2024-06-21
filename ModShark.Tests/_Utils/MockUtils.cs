using Microsoft.Extensions.Logging;
using Moq;

namespace ModShark.Tests._Utils;

internal static class MockUtils
{
    // Based on https://stackoverflow.com/a/58697253
    public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, LogLevel level, string message, Times times)
        => mockLogger.Verify(l
            => l.Log(
                level,
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((o, t) => o.ToString() == message), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
            , times
        );
    
    public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, LogLevel level, Times times)
        => mockLogger.Verify(l
                => l.Log(
                    level,
                    It.IsAny<EventId>(), 
                    It.IsAny<It.IsAnyType>(), 
                    It.IsAny<Exception>(), 
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            , times
        );
    
    public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, string message, Times times)
        => mockLogger.Verify(l
                => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((o, t) => o.ToString() == message), 
                    It.IsAny<Exception>(), 
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            , times
        );
    
    public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, Times times)
        => mockLogger.Verify(l
                => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(), 
                    It.IsAny<It.IsAnyType>(), 
                    It.IsAny<Exception>(), 
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            , times
        );
}