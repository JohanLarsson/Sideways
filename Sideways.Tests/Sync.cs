﻿namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    public static class Sync
    {
        private static readonly FileInfo FlashDrive = new("D:\\Database.sqlite3");

        [Explicit]
        [TestCaseSource(nameof(AllSymbols))]
        public static void OneWay(string symbol)
        {
            var source = Database.DbFile;
            var target = FlashDrive;
            if (File.Exists(source.FullName) &&
                File.Exists(target.FullName))
            {
                if (Sideways.Sync.CountDays(symbol, source) > Sideways.Sync.CountDays(symbol, target))
                {
                    Sideways.Sync.CopyDays(symbol, source, target);
                    Sideways.Sync.CopySplits(symbol, source, target);
                    Sideways.Sync.CopyDividends(symbol, source, target);
                }

                if (Sideways.Sync.CountMinutes(symbol, source) > Sideways.Sync.CountMinutes(symbol, target))
                {
                    Sideways.Sync.CopyMinutes(symbol, source, target);
                }
            }
        }

        [Explicit]
        [TestCaseSource(nameof(AllSymbols))]
        public static void TwoWay(string symbol)
        {
            var x = Database.DbFile;
            var y = FlashDrive;
            if (File.Exists(x.FullName) &&
                File.Exists(y.FullName))
            {
                switch (Comparer<long>.Default.Compare(Sideways.Sync.CountDays(symbol, x), Sideways.Sync.CountDays(symbol, y)))
                {
                    case < 0:
                        CopyDays(symbol, y, x);
                        break;
                    case > 0:
                        CopyDays(symbol, x, y);
                        break;
                    case 0:
                        break;
                }

                switch (Comparer<long>.Default.Compare(Sideways.Sync.CountMinutes(symbol, x), Sideways.Sync.CountMinutes(symbol, y)))
                {
                    case < 0:
                        Console.WriteLine($"Copy minutes from {y} to {x}.");
                        Sideways.Sync.CopyMinutes(symbol, y, x);
                        break;
                    case > 0:
                        Console.WriteLine($"Copy minutes from {x} to {y}.");
                        Sideways.Sync.CopyMinutes(symbol, x, y);
                        break;
                    case 0:
                        break;
                }

                static void CopyDays(string symbol, FileInfo source, FileInfo target)
                {
                    Console.WriteLine($"Copy days from {source} to {target}.");
                    Sideways.Sync.CopyDays(symbol, source, target);
                    Sideways.Sync.CopySplits(symbol, source, target);
                    Sideways.Sync.CopyDividends(symbol, source, target);
                }
            }
        }

        private static IEnumerable<string> AllSymbols()
        {
            return Read(Database.DbFile).Concat(Read(FlashDrive)).Distinct();

            static ImmutableArray<string> Read(FileInfo file)
            {
                if (File.Exists(file.FullName))
                {
                    return Database.ReadSymbols(file);
                }

                return ImmutableArray<string>.Empty;
            }
        }
    }
}
