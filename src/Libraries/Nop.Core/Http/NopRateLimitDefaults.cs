namespace Nop.Core.Http;

/// <summary>
/// Represents default values related to HTTP rate limiting
/// </summary>
public static partial class NopRateLimitDefaults
{
    /// <summary>
    /// Gets the default HTTP status code returned when a request is rate limited
    /// </summary>
    public static int RejectionStatusCode => 429;

    /// <summary>
    /// Gets the maximum number of retry attempts after an HTTP 429 response
    /// </summary>
    public static int MaxRetryAttempts => 3;

    /// <summary>
    /// Gets the initial delay (in milliseconds) used for exponential backoff retries
    /// </summary>
    public static int InitialRetryDelayMilliseconds => 1000;

    /// <summary>
    /// Gets the maximum delay (in milliseconds) allowed between retry attempts
    /// </summary>
    public static int MaxRetryDelayMilliseconds => 30000;

    /// <summary>
    /// Gets the default Retry-After value (in seconds) when the rate limiter does not provide one
    /// </summary>
    public static int DefaultRetryAfterSeconds => 60;

    /// <summary>
    /// Gets the short message prefix written to the system log for rate-limit events
    /// </summary>
    public static string LogMessagePrefix => "Rate limit exceeded";
}
