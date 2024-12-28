namespace MinimalApi_Test.Services;

public static class LoggerService
{
    private static ILogger? _logger;

    public static void ConfigureLogger(ILogger logger)
    {
        _logger = logger;
    }

    public static void LogInformation(string message)
    {
        _logger?.LogInformation(message);
    }

    public static void LogWarning(string message)
    {
        _logger?.LogWarning(message);
    }

    public static void LogError(string message, Exception? exception = null)
    {
        _logger?.LogError(exception, message);
    }
}