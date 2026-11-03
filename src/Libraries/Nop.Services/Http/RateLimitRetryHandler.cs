using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Http;
using Nop.Data;
using Nop.Services.Logging;

namespace Nop.Services.Http;

/// <summary>
/// Represents an HTTP message handler that retries requests after HTTP 429 responses using exponential backoff
/// and writes rate-limit events to the admin system log
/// </summary>
public partial class RateLimitRetryHandler : DelegatingHandler
{
    #region Fields

    protected readonly IServiceScopeFactory _serviceScopeFactory;

    #endregion

    #region Ctor

    public RateLimitRetryHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Calculates the delay before the next retry attempt
    /// </summary>
    /// <param name="response">HTTP response that returned 429</param>
    /// <param name="attempt">Zero-based retry attempt index</param>
    /// <returns>Delay before the next retry</returns>
    protected virtual TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta)
            return ClampDelay(delta < TimeSpan.Zero ? TimeSpan.Zero : delta);

        if (response.Headers.RetryAfter?.Date is { } date)
        {
            var untilDate = date - DateTimeOffset.UtcNow;
            return ClampDelay(untilDate < TimeSpan.Zero ? TimeSpan.Zero : untilDate);
        }

        var exponentialDelayMs = NopRateLimitDefaults.InitialRetryDelayMilliseconds * Math.Pow(2, attempt);
        return ClampDelay(TimeSpan.FromMilliseconds(exponentialDelayMs));
    }

    /// <summary>
    /// Clamps a delay to the configured maximum
    /// </summary>
    protected virtual TimeSpan ClampDelay(TimeSpan delay)
    {
        var maxDelay = TimeSpan.FromMilliseconds(NopRateLimitDefaults.MaxRetryDelayMilliseconds);
        return delay > maxDelay ? maxDelay : delay;
    }

    /// <summary>
    /// Determines whether the request can be safely retried
    /// </summary>
    protected virtual bool CanRetry(HttpRequestMessage request)
    {
        // Avoid replaying non-buffered request bodies
        if (request.Content == null)
            return true;

        if (request.Content is StreamContent)
            return request.Content.Headers.ContentLength.HasValue;

        return true;
    }

    /// <summary>
    /// Writes a rate-limit event to the admin system log (error dashboard)
    /// </summary>
    protected virtual async Task LogRateLimitEventAsync(HttpRequestMessage request, int attempt, TimeSpan retryAfter, bool exhausted)
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        try
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetService<ILogger>();
            if (logger == null)
                return;

            var uri = request.RequestUri?.ToString() ?? "unknown";
            var shortMessage = exhausted
                ? $"{NopRateLimitDefaults.LogMessagePrefix} (outbound API, retries exhausted): {request.Method} {uri}"
                : $"{NopRateLimitDefaults.LogMessagePrefix} (outbound API): {request.Method} {uri}";
            var fullMessage =
                $"HTTP 429 Too Many Requests received from external API.{Environment.NewLine}" +
                $"Attempt: {attempt + 1} of {NopRateLimitDefaults.MaxRetryAttempts + 1}{Environment.NewLine}" +
                $"Retry after: {retryAfter.TotalSeconds:0.###} seconds{Environment.NewLine}" +
                $"Retries exhausted: {exhausted}";

            if (exhausted)
                await logger.ErrorAsync(shortMessage, new Exception(fullMessage));
            else
                await logger.WarningAsync(shortMessage, new Exception(fullMessage));
        }
        catch
        {
            // never break the HTTP pipeline because of logging failures
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sends an HTTP request with retry on HTTP 429 using exponential backoff
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpResponseMessage response = null;

        for (var attempt = 0; ; attempt++)
        {
            response?.Dispose();
            response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                return response;

            var retryDelay = GetRetryDelay(response, attempt);
            var retriesExhausted = attempt >= NopRateLimitDefaults.MaxRetryAttempts || !CanRetry(request);

            await LogRateLimitEventAsync(request, attempt, retryDelay, retriesExhausted);

            if (retriesExhausted)
                return response;

            await Task.Delay(retryDelay, cancellationToken);
        }
    }

    #endregion
}
