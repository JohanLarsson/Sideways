namespace Sideways
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal static class Log
    {
        private static string indent = string.Empty;

        internal static IDisposable Time([CallerFilePath] string? path = null, [CallerMemberName] string? name = null)
        {
            return new Logger($"{Path.GetFileNameWithoutExtension(path)}.{name}");
        }

        internal static IDisposable Time(string text)
        {
            return new Logger(text);
        }

        private sealed class Logger : IDisposable
        {
            private readonly Stopwatch stopwatch = Stopwatch.StartNew();
            private readonly string text;

            private bool disposed;

            internal Logger(string text)
            {
                this.text = text;
                Debug.WriteLine(indent + text + " start");
                indent += "  ";
            }

            public void Dispose()
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;
                this.stopwatch.Stop();
                indent = indent.Substring(2);
                Debug.WriteLine(indent + this.text + (this.stopwatch.ElapsedMilliseconds > 0 ? $" took: {this.stopwatch.ElapsedMilliseconds} ms" : $" took: {1_000_000 * this.stopwatch.ElapsedTicks / Stopwatch.Frequency} µs"));
            }
        }
    }
}
