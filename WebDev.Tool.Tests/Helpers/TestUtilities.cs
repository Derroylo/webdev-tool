using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace WebDev.Tool.Tests.Helpers;

/// <summary>
/// Utility methods for devcontainer tests
/// </summary>
public static class TestUtilities
{
    private static IConfiguration? _configuration;
    private static ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Gets the test configuration
    /// </summary>
    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            return _configuration;
        }
    }

    /// <summary>
    /// Gets the logger factory for tests
    /// </summary>
    public static ILoggerFactory LoggerFactory
    {
        get
        {
            if (_loggerFactory == null)
            {
                _loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.AddConfiguration(Configuration.GetSection("Logging"));
                });
            }
            return _loggerFactory;
        }
    }

    /// <summary>
    /// Gets a test configuration value
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <param name="defaultValue">Default value if key is not found</param>
    /// <returns>The configuration value</returns>
    public static string GetTestSetting(string key, string defaultValue = "")
    {
        return Configuration[$"TestSettings:{key}"] ?? defaultValue;
    }

    /// <summary>
    /// Gets a test configuration value as an integer
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <param name="defaultValue">Default value if key is not found</param>
    /// <returns>The configuration value as integer</returns>
    public static int GetTestSettingInt(string key, int defaultValue = 0)
    {
        var value = GetTestSetting(key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets a test configuration value as a boolean
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <param name="defaultValue">Default value if key is not found</param>
    /// <returns>The configuration value as boolean</returns>
    public static bool GetTestSettingBool(string key, bool defaultValue = false)
    {
        var value = GetTestSetting(key);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Creates a logger for a specific type
    /// </summary>
    /// <typeparam name="T">The type to create a logger for</typeparam>
    /// <returns>A logger instance</returns>
    public static ILogger<T> CreateLogger<T>()
    {
        return LoggerFactory.CreateLogger<T>();
    }

    /// <summary>
    /// Waits for a condition to be true with timeout
    /// </summary>
    /// <param name="condition">The condition to wait for</param>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <param name="pollIntervalSeconds">Poll interval in seconds</param>
    /// <returns>True if condition was met, false if timed out</returns>
    public static async Task<bool> WaitForConditionAsync(Func<Task<bool>> condition, int timeoutSeconds = 30, int pollIntervalSeconds = 1)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var pollInterval = TimeSpan.FromSeconds(pollIntervalSeconds);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            if (await condition())
            {
                return true;
            }

            await Task.Delay(pollInterval);
        }

        return false;
    }

    /// <summary>
    /// Retries an operation with exponential backoff
    /// </summary>
    /// <typeparam name="T">The return type of the operation</typeparam>
    /// <param name="operation">The operation to retry</param>
    /// <param name="maxAttempts">Maximum number of attempts</param>
    /// <param name="baseDelaySeconds">Base delay in seconds</param>
    /// <returns>The result of the operation</returns>
    public static async Task<T> RetryWithBackoffAsync<T>(Func<Task<T>> operation, int maxAttempts = 3, int baseDelaySeconds = 1)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < maxAttempts - 1)
            {
                attempt++;
                var delay = TimeSpan.FromSeconds(baseDelaySeconds * Math.Pow(2, attempt - 1));
                await Task.Delay(delay);
            }
        }
    }

    /// <summary>
    /// Sanitizes a string for use in test names
    /// </summary>
    /// <param name="input">The input string</param>
    /// <returns>A sanitized string safe for test names</returns>
    public static string SanitizeTestName(string input)
    {
        return input
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace(".", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "_");
    }
}
