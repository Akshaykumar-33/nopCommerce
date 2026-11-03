using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Http;
using Nop.Data;
using Nop.Services.Logging;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Web.Framework.Mvc.Filters;

/// <summary>
/// Represents a filter attribute that handles HTTP 429 / rate-limit exceptions for all controllers,
/// returns a 429 response with Retry-After (for client exponential backoff), and logs the event
/// to the admin system log (error dashboard)
/// </summary>
public sealed class HandleRateLimitAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    public HandleRateLimitAttribute() : base(typeof(HandleRateLimitFilter))
    {
    }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that handles rate-limit failures raised by controller actions
    /// </summary>
    private class HandleRateLimitFilter : IAsyncExceptionFilter
    {
        #region Fields

        protected readonly ILogger _logger;

        #endregion

        #region Ctor

        public HandleRateLimitFilter(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Determines whether an exception represents an HTTP 429 rate-limit failure
        /// </summary>
        protected virtual bool IsRateLimitException(Exception exception)
        {
            if (exception == null)
                return false;

            if (exception is HttpRequestException httpRequestException
                && httpRequestException.StatusCode == HttpStatusCode.TooManyRequests)
                return true;

            if (exception.InnerException != null && IsRateLimitException(exception.InnerException))
                return true;

            var message = exception.Message ?? string.Empty;
            return message.Contains("429", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("Too Many Requests", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("Rate Limit", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("Quota/Rate Limit Exceeded", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Logs a rate-limit event to the admin system log (error dashboard)
        /// </summary>
        protected virtual async Task LogRateLimitAsync(ExceptionContext context)
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            try
            {
                var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                var method = context.HttpContext.Request.Method;
                var shortMessage = $"{NopRateLimitDefaults.LogMessagePrefix} (controller): {method} {path}";

                await _logger.ErrorAsync(shortMessage, context.Exception);
            }
            catch
            {
                // never break the request pipeline because of logging failures
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called after an action has thrown an exception
        /// </summary>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.ExceptionHandled || !IsRateLimitException(context.Exception))
                return;

            await LogRateLimitAsync(context);

            var retryAfterSeconds = NopRateLimitDefaults.DefaultRetryAfterSeconds;
            context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
            context.Result = new ObjectResult(new
            {
                error = NopRateLimitDefaults.LogMessagePrefix,
                status = NopRateLimitDefaults.RejectionStatusCode,
                retryAfterSeconds
            })
            {
                StatusCode = NopRateLimitDefaults.RejectionStatusCode
            };
            context.ExceptionHandled = true;
        }

        #endregion
    }

    #endregion
}
