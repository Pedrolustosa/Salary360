using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Salary360.Application.Settings;
using Salary360.Infrastructure.Inflation;

namespace Salary360.UnitTests.Infrastructure;

public sealed class IbgeInflationProviderTests : IDisposable
{
    private readonly IHttpClientFactory factory = Substitute.For<IHttpClientFactory>();
    private readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
    private readonly ILogger<IbgeInflationProvider> logger = Substitute.For<ILogger<IbgeInflationProvider>>();
    private readonly IOptions<IbgeSettings> options = Options.Create(new IbgeSettings
    {
        BaseUrl = "https://apisidra.ibge.gov.br",
        Table = "1737",
        Variable = "2266",
        CacheHours = 12,
        StaleCacheHours = 168,
        TimeoutSeconds = 10
    });

    [Fact]
    public async Task GetIndicesAsync_SkipsHeaderAndParsesStringValues()
    {
        var json = """
            [{"V":"Valor","D3C":"Mês (Código)"},{"V":"100.00","D3C":"202401"},{"V":"...","D3C":"202402"},{"V":"101.50","D3C":"202403"}]
            """;
        using var provider = CreateProvider(new FakeHandler(_ => JsonResponse(json)));

        var result = await provider.GetIndicesAsync();

        Assert.True(result.Available);
        Assert.False(result.IsStale);
        Assert.Equal(2, result.Indices.Count);
        Assert.Equal(100.00m, result.Indices[(2024, 1)]);
        Assert.Equal(101.50m, result.Indices[(2024, 3)]);
        Assert.False(result.Indices.ContainsKey((2024, 2)));
    }

    [Fact]
    public async Task GetIndicesAsync_ParsesNumericV()
    {
        var json = """
            [{"V":"Valor","D3C":"Mês (Código)"},{"V":100.25,"D3C":"202401"}]
            """;
        using var provider = CreateProvider(new FakeHandler(_ => JsonResponse(json)));

        var result = await provider.GetIndicesAsync();

        Assert.True(result.Available);
        Assert.Equal(100.25m, result.Indices[(2024, 1)]);
    }

    [Fact]
    public async Task GetIndicesAsync_IgnoresInvalidValues()
    {
        var json = """
            [
              {"V":"Valor","D3C":"Mês (Código)"},
              {"V":"bad","D3C":"202401"},
              {"V":"100","D3C":"202413"},
              {"V":"101","D3C":"2024"},
              {"V":"102","D3C":"202405"},
              {"D3C":"202406"},
              {"V":"103"}
            ]
            """;
        using var provider = CreateProvider(new FakeHandler(_ => JsonResponse(json)));

        var result = await provider.GetIndicesAsync();

        Assert.True(result.Available);
        Assert.Single(result.Indices);
        Assert.Equal(102m, result.Indices[(2024, 5)]);
    }

    [Fact]
    public async Task GetIndicesAsync_ServerError_UnavailableWithoutCache()
    {
        using var provider = CreateProvider(new FakeHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var result = await provider.GetIndicesAsync();

        Assert.False(result.Available);
        Assert.False(result.IsStale);
        Assert.Empty(result.Indices);
    }

    [Fact]
    public async Task GetIndicesAsync_InvalidJson_UnavailableWithoutCache()
    {
        using var provider = CreateProvider(new FakeHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{not-json", Encoding.UTF8, "application/json")
            }));

        var result = await provider.GetIndicesAsync();

        Assert.False(result.Available);
        Assert.Empty(result.Indices);
    }

    [Fact]
    public async Task GetIndicesAsync_Timeout_UnavailableWithoutCache()
    {
        using var provider = CreateProvider(new FakeHandler(_ => throw new TaskCanceledException("timeout")));

        var result = await provider.GetIndicesAsync();

        Assert.False(result.Available);
        Assert.Empty(result.Indices);
    }

    [Fact]
    public async Task GetIndicesAsync_CacheHit_OnSecondCall()
    {
        var calls = 0;
        var json = """
            [{"V":"Valor","D3C":"Mês (Código)"},{"V":"100.00","D3C":"202401"}]
            """;
        using var provider = CreateProvider(new FakeHandler(_ =>
        {
            Interlocked.Increment(ref calls);
            return JsonResponse(json);
        }));

        var first = await provider.GetIndicesAsync();
        var second = await provider.GetIndicesAsync();

        Assert.True(first.Available);
        Assert.True(second.Available);
        Assert.False(second.IsStale);
        Assert.Equal(1, calls);
        Assert.Equal(100m, second.Indices[(2024, 1)]);
    }

    [Fact]
    public async Task GetIndicesAsync_FetchFailure_ReturnsStaleCache()
    {
        var json = """
            [{"V":"Valor","D3C":"Mês (Código)"},{"V":"100.00","D3C":"202401"}]
            """;
        var succeed = true;
        using var provider = CreateProvider(new FakeHandler(_ =>
        {
            if (succeed)
                return JsonResponse(json);
            return new HttpResponseMessage(HttpStatusCode.BadGateway);
        }));

        var first = await provider.GetIndicesAsync();
        Assert.True(first.Available);
        Assert.False(first.IsStale);

        cache.Remove("ibge:ipca:series");
        succeed = false;

        var stale = await provider.GetIndicesAsync();

        Assert.True(stale.Available);
        Assert.True(stale.IsStale);
        Assert.Equal(100m, stale.Indices[(2024, 1)]);
    }

    [Fact]
    public async Task GetIndicesAsync_EmptyPayload_UnavailableWithoutCache()
    {
        var json = """[{"V":"Valor","D3C":"Mês (Código)"}]""";
        using var provider = CreateProvider(new FakeHandler(_ => JsonResponse(json)));

        var result = await provider.GetIndicesAsync();

        Assert.False(result.Available);
        Assert.Empty(result.Indices);
    }

    public void Dispose() => cache.Dispose();

    private IbgeInflationProvider CreateProvider(FakeHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://apisidra.ibge.gov.br/")
        };
        factory.CreateClient(IbgeInflationProvider.HttpClientName).Returns(client);
        return new IbgeInflationProvider(factory, cache, options, logger);
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }
}