namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Sideways.AlphaVantage;

    internal static class HttpClientExtensions
    {
        internal static async Task<ImmutableArray<Listing>> GetListingFromCsvAsync(this HttpClient client, Uri requestUri, CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var encoding = GetEncoding(response.Content.Headers.ContentType?.CharSet);
            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return await Csv.ParseListingsAsync(content, encoding).ConfigureAwait(false);
        }

        internal static async Task<ImmutableArray<Candle>> GetCandlesFromCsvAsync(this HttpClient client, Uri requestUri, CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var encoding = GetEncoding(response.Content.Headers.ContentType?.CharSet);
            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return await Csv.ParseCandlesAsync(content, encoding).ConfigureAwait(false);
        }

        internal static async Task<ImmutableArray<AdjustedCandle>> GetAdjustedCandleFromCsvAsync(this HttpClient client, Uri requestUri, CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var encoding = GetEncoding(response.Content.Headers.ContentType?.CharSet);
            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return await Csv.ParseAdjustedCandlesAsync(content, encoding).ConfigureAwait(false);
        }

        internal static Encoding GetEncoding(string? charset)
        {
            if (charset is null)
            {
                return Encoding.UTF8;
            }

            // Remove at most a single set of quotes.
            if (charset.Length > 2 && charset[0] == '\"' && charset[^1] == '\"')
            {
                return Encoding.GetEncoding(charset[1..^2]);
            }
            else
            {
                return Encoding.GetEncoding(charset);
            }
        }
    }
}
