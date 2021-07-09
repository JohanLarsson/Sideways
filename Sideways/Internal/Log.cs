namespace Sideways
{
    using System;
    using System.Diagnostics;

    internal static class Log
    {
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
            }

            public void Dispose()
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;
                this.stopwatch.Stop();
                Debug.WriteLine(this.text + (this.stopwatch.ElapsedMilliseconds > 1 ? $" took: {this.stopwatch.ElapsedMilliseconds} ms" : $" took: {1_000_000 * this.stopwatch.ElapsedTicks / Stopwatch.Frequency} µs"));
            }
        }
    }
}
