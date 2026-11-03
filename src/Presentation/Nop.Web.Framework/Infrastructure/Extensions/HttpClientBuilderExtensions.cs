using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Security;
using Nop.Services.Http;

namespace Nop.Web.Framework.Infrastructure.Extensions;

/// <summary>
/// Represents extensions of IHttpClientBuilder
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Configure proxy connection for HTTP client (if enabled)
    /// </summary>
    /// <param name="httpClientBuilder">A builder for configuring HttpClient</param>
    public static void WithProxy(this IHttpClientBuilder httpClientBuilder)
    {
        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(provider =>
        {
            var handler = new HttpClientHandler();

            //whether proxy is enabled
            var proxySettings = provider.GetService<ProxySettings>();
            if (!proxySettings?.Enabled ?? true)
                return handler;

            //configure proxy connection
            var webProxy = new WebProxy($"{proxySettings.Address}:{proxySettings.Port}", proxySettings.BypassOnLocal);
            if (!string.IsNullOrEmpty(proxySettings.Username) && !string.IsNullOrEmpty(proxySettings.Password))
            {
                webProxy.UseDefaultCredentials = false;
                webProxy.Credentials = new NetworkCredential
                {
                    UserName = proxySettings.Username,
                    Password = proxySettings.Password
                };
            }
            else
            {
                webProxy.UseDefaultCredentials = true;
                webProxy.Credentials = CredentialCache.DefaultCredentials;
            }

            //configure HTTP client handler
            handler.UseDefaultCredentials = webProxy.UseDefaultCredentials;
            handler.Proxy = webProxy;
            handler.PreAuthenticate = proxySettings.PreAuthenticate;

            return handler;
        });

        //retry outbound HTTP 429 responses with exponential backoff
        httpClientBuilder.WithRateLimitRetry();
    }

    /// <summary>
    /// Adds HTTP 429 handling with exponential backoff and admin system-log events
    /// </summary>
    /// <param name="httpClientBuilder">A builder for configuring HttpClient</param>
    /// <returns>A builder for configuring HttpClient</returns>
    public static IHttpClientBuilder WithRateLimitRetry(this IHttpClientBuilder httpClientBuilder)
    {
        ArgumentNullException.ThrowIfNull(httpClientBuilder);

        return httpClientBuilder.AddHttpMessageHandler<RateLimitRetryHandler>();
    }
}
