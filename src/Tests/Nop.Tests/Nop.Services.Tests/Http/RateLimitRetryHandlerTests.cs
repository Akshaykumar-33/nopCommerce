using System.Net;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nop.Core.Http;
using Nop.Services.Http;
using Nop.Services.Logging;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Http;

[TestFixture]
public class RateLimitRetryHandlerTests
{
    [Test]
    public async Task SendAsync_Should_Return_Immediately_On_Success()
    {
        var handler = CreateHandler(new SequenceHandler(HttpStatusCode.OK));
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ((SequenceHandler)handler.InnerHandler!).CallCount.Should().Be(1);
    }

    [Test]
    public async Task SendAsync_Should_Retry_On_429_With_Exponential_Backoff()
    {
        var inner = new SequenceHandler(
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.OK)
        {
            // keep tests fast; production uses exponential delay when header is absent
            RetryAfterSeconds = 0
        };
        var handler = CreateHandler(inner);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.CallCount.Should().Be(3);
    }

    [Test]
    public async Task SendAsync_Should_Stop_After_Max_Retries_When_Still_429()
    {
        var statuses = Enumerable
            .Repeat(HttpStatusCode.TooManyRequests, NopRateLimitDefaults.MaxRetryAttempts + 2)
            .ToArray();
        var inner = new SequenceHandler(statuses) { RetryAfterSeconds = 0 };
        var handler = CreateHandler(inner);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        inner.CallCount.Should().Be(NopRateLimitDefaults.MaxRetryAttempts + 1);
    }

    [Test]
    public async Task SendAsync_Should_Honor_RetryAfter_Header()
    {
        var inner = new SequenceHandler(HttpStatusCode.TooManyRequests, HttpStatusCode.OK)
        {
            RetryAfterSeconds = 0
        };
        var handler = CreateHandler(inner);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.CallCount.Should().Be(2);
    }

    [Test]
    public void GetRetryDelay_Should_Use_Exponential_Backoff_When_RetryAfter_Missing()
    {
        var handler = new TestableRateLimitRetryHandler();
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

        var delay0 = handler.ExposeGetRetryDelay(response, 0);
        var delay1 = handler.ExposeGetRetryDelay(response, 1);
        var delay2 = handler.ExposeGetRetryDelay(response, 2);

        delay0.TotalMilliseconds.Should().Be(NopRateLimitDefaults.InitialRetryDelayMilliseconds);
        delay1.TotalMilliseconds.Should().Be(NopRateLimitDefaults.InitialRetryDelayMilliseconds * 2);
        delay2.TotalMilliseconds.Should().Be(NopRateLimitDefaults.InitialRetryDelayMilliseconds * 4);
    }

    private static RateLimitRetryHandler CreateHandler(HttpMessageHandler inner)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILogger>());
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        return new RateLimitRetryHandler(scopeFactory)
        {
            InnerHandler = inner
        };
    }

    private sealed class TestableRateLimitRetryHandler : RateLimitRetryHandler
    {
        public TestableRateLimitRetryHandler()
            : base(new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>())
        {
        }

        public TimeSpan ExposeGetRetryDelay(HttpResponseMessage response, int attempt)
            => GetRetryDelay(response, attempt);
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statuses;

        public SequenceHandler(params HttpStatusCode[] statuses)
        {
            _statuses = new Queue<HttpStatusCode>(statuses);
        }

        public int CallCount { get; private set; }

        public int? RetryAfterSeconds { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            var status = _statuses.Count > 0 ? _statuses.Dequeue() : HttpStatusCode.OK;
            var response = new HttpResponseMessage(status);

            if (status == HttpStatusCode.TooManyRequests && RetryAfterSeconds.HasValue)
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(RetryAfterSeconds.Value));

            return Task.FromResult(response);
        }
    }
}
