using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salary360.Application.Common;
using Salary360.Application.Common.Interfaces;
using Salary360.Application.Settings;

namespace Salary360.Infrastructure.Inflation;

public sealed class IbgeInflationProvider : IInflationProvider, IDisposable
{
    public const string HttpClientName = "Ibge";

    private const string CacheKey = "ibge:ipca:series";
    private const string StaleMetaKey = "ibge:ipca:stale-meta";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly IMemoryCache cache;
    private readonly IbgeSettings settings;
    private readonly ILogger<IbgeInflationProvider> logger;
    private readonly SemaphoreSlim gate = new(1, 1);

    public IbgeInflationProvider(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IOptions<IbgeSettings> options,
        ILogger<IbgeInflationProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.cache = cache;
        settings = options.Value;
        this.logger = logger;
    }

    public async Task<InflationSeriesResult> GetIndicesAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyDictionary<(int Year, int Month), decimal>? fresh)
            && fresh is not null)
            return new InflationSeriesResult(true, false, fresh);

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(CacheKey, out fresh) && fresh is not null)
                return new InflationSeriesResult(true, false, fresh);

            var fetched = await FetchAsync(cancellationToken);
            if (fetched is not null)
            {
                cache.Set(CacheKey, fetched, TimeSpan.FromHours(settings.CacheHours));
                cache.Set(
                    StaleMetaKey,
                    new StaleEntry(fetched, DateTimeOffset.UtcNow),
                    TimeSpan.FromHours(settings.StaleCacheHours));
                return new InflationSeriesResult(true, false, fetched);
            }

            if (cache.TryGetValue(StaleMetaKey, out StaleEntry? stale)
                && stale is not null
                && DateTimeOffset.UtcNow - stale.FetchedAtUtc <= TimeSpan.FromHours(settings.StaleCacheHours))
            {
                logger.LogWarning("Serving stale IBGE IPCA series after fetch failure.");
                return new InflationSeriesResult(true, true, stale.Indices);
            }

            logger.LogWarning("IBGE IPCA series unavailable and no usable cache.");
            return InflationSeriesResult.Unavailable;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<IReadOnlyDictionary<(int Year, int Month), decimal>?> FetchAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            var path = $"values/t/{settings.Table}/n1/all/v/{settings.Variable}/p/all?formato=json";
            using var response = await client.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IBGE IPCA request failed with status {StatusCode}.", (int)response.StatusCode);
                return null;
            }

            var rows = await response.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>(
                cancellationToken: cancellationToken);
            if (rows is null || rows.Count < 2)
            {
                logger.LogWarning("IBGE IPCA response was empty or missing data rows.");
                return null;
            }

            var indices = new Dictionary<(int Year, int Month), decimal>();
            foreach (var row in rows.Skip(1))
            {
                if (!row.TryGetValue("V", out var valueElement) || !row.TryGetValue("D3C", out var periodElement))
                    continue;

                var rawValue = valueElement.ValueKind == JsonValueKind.String
                    ? valueElement.GetString()
                    : valueElement.ToString();
                var period = periodElement.ValueKind == JsonValueKind.String
                    ? periodElement.GetString()
                    : periodElement.ToString();

                if (string.IsNullOrWhiteSpace(rawValue)
                    || string.IsNullOrWhiteSpace(period)
                    || period.Length != 6
                    || !decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var index)
                    || !int.TryParse(period[..4], NumberStyles.None, CultureInfo.InvariantCulture, out var year)
                    || !int.TryParse(period[4..], NumberStyles.None, CultureInfo.InvariantCulture, out var month)
                    || month is < 1 or > 12)
                    continue;

                indices[(year, month)] = index;
            }

            return indices.Count == 0 ? null : indices;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or NotSupportedException)
        {
            logger.LogWarning(ex, "IBGE IPCA fetch failed.");
            return null;
        }
    }

    public void Dispose() => gate.Dispose();

    private sealed record StaleEntry(
        IReadOnlyDictionary<(int Year, int Month), decimal> Indices,
        DateTimeOffset FetchedAtUtc);
}
