﻿namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class MinutesDownload : Download
    {
        private static readonly ImmutableArray<Slice> AscendingSlices = Enum.GetValues<Slice>().OrderBy(x => TimeRange.FromSlice(x).Max).ToImmutableArray();
        private static readonly ImmutableArray<Slice> DescendingSlices = AscendingSlices.Reverse().ToImmutableArray();

        private readonly Downloader downloader;

        public MinutesDownload(string symbol, Slice? slice, Downloader downloader)
            : base(symbol)
        {
            this.downloader = downloader;
            this.Slice = slice;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DownloadCommand = new RelayCommand(_ => this.ExecuteAsync(), _ => this.State is { Status: DownloadStatus.Waiting or DownloadStatus.Error });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public Slice? Slice { get; }

        public ICommand DownloadCommand { get; }

        public TimeRange SliceRange => TimeRange.FromSlice(this.Slice ?? AlphaVantage.Slice.Year1Month1);

        public override string Info => $"Minutes from {this.SliceRange.Min:d} to {this.SliceRange.Max:d}";

        public static ImmutableArray<MinutesDownload> Create(string symbol, TimeRange existingDays, TimeRange existingMinutes, Downloader downloader, AlphaVantageSettings settings)
        {
            if (settings.SymbolsWithMissingMinutes.Contains(symbol))
            {
                return ImmutableArray<MinutesDownload>.Empty;
            }

            if (TradingDay.From(existingMinutes.Max) < TradingDay.LastComplete() &&
                TradingDay.From(existingMinutes.Max.AddMonths(1)) >= TradingDay.LastComplete())
            {
                return ImmutableArray.Create(new MinutesDownload(symbol, null, downloader));
            }

            var builder = ImmutableArray.CreateBuilder<MinutesDownload>();
            foreach (var slice in Slices())
            {
                if (ShouldDownload(slice))
                {
                    builder.Add(new MinutesDownload(symbol, slice, downloader));
                }
            }

            return builder.ToImmutable();

            IEnumerable<Slice> Slices()
            {
                var effective = existingMinutes == default
                    ? new TimeRange(DateTimeOffset.MaxValue, DateTimeOffset.MaxValue)
                    : existingMinutes;
                foreach (var slice in DescendingSlices)
                {
                    if (TimeRange.FromSlice(slice).Min < effective.Min)
                    {
                        yield return slice;
                    }
                }

                foreach (var slice in AscendingSlices)
                {
                    if (TimeRange.FromSlice(slice).Max > effective.Max)
                    {
                        yield return slice;
                    }
                }
            }

            bool ShouldDownload(Slice slice)
            {
                var sliceRange = TimeRange.FromSlice(slice);
                if (existingMinutes.Contains(sliceRange))
                {
                    return false;
                }

                if (settings.FirstMinutes.TryGetValue(symbol, out var firstMinute))
                {
                    if (sliceRange.Contains(firstMinute) &&
                        existingMinutes.Min.Date == firstMinute.Date)
                    {
                        return false;
                    }

                    if (sliceRange.Max < firstMinute)
                    {
                        return false;
                    }
                }

                if (settings.UnlistedSymbols.Contains(symbol))
                {
                    if (sliceRange.Min > existingDays.Max.Date)
                    {
                        return false;
                    }

                    if (existingMinutes.Overlaps(sliceRange) &&
                        existingDays.Max.Date == existingMinutes.Max.Date)
                    {
                        return false;
                    }
                }

                if (sliceRange.Max < existingDays.Min)
                {
                    return false;
                }

                return true;
            }
        }

        public async Task<int> ExecuteAsync()
        {
            this.downloader.Add(this);
            this.State.Start = DateTimeOffset.Now;
            this.State.Exception = null;

            try
            {
                var candles = await Task().ConfigureAwait(false);
                this.State.End = DateTimeOffset.Now;
                if (!candles.IsDefaultOrEmpty)
                {
                    Database.WriteMinutes(this.Symbol, candles);
                    this.downloader.NotifyDownloadedMinutes(this.Symbol);
                }

                if (candles.IsDefaultOrEmpty && this.Slice is { } &&
                    Database.FirstMinute(this.Symbol) is { } first)
                {
                    this.downloader.FirstMinute(this.Symbol, first);
                    throw new InvalidOperationException("Downloaded empty slice, maybe missing data on AlphaVantage. Adding symbol to FirstMinutes in %APPDATA%\\Sideways\\Sideways.cfg");
                }

                if (candles.IsDefaultOrEmpty &&
                    IsMostRecentSlice())
                {
                    this.downloader.MissingMinutes(this.Symbol);
                    throw new InvalidOperationException("Downloaded empty slice, maybe missing data on AlphaVantage. Adding symbol to SymbolsWithMissingMinutes in %APPDATA%\\Sideways\\Sideways.cfg");
                }

                return candles.Length;

                bool IsMostRecentSlice()
                {
                    return this.Slice switch
                    {
                        AlphaVantage.Slice.Year1Month1 => true,
                        { } slice => TimeRange.FromSlice(slice).Contains(Database.DayRange(this.Symbol).Max),
                        null => true,
                    };
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.State.Exception = e;
                this.State.End = DateTimeOffset.Now;
                return 0;
            }

            Task<ImmutableArray<Candle>> Task()
            {
                if (this.Slice is { } slice)
                {
                    return this.downloader.Client.IntradayExtendedAsync(this.Symbol, Interval.Minute, slice);
                }

                return this.downloader.Client.IntradayAsync(this.Symbol, Interval.Minute);
            }
        }
    }
}
