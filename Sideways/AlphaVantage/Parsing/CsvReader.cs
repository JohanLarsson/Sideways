namespace Sideways.AlphaVantage
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class CsvReader : IDisposable
    {
        private readonly StreamReader reader;
        private bool disposed;

        internal CsvReader(Stream stream, Encoding encoding)
        {
            this.reader = new StreamReader(stream, encoding);
        }

        internal bool EndOfStream => this.reader.EndOfStream;

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.reader.Dispose();
        }

        internal Task<string?> ReadLineAsync() => this.reader.ReadLineAsync();

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(CsvReader));
            }
        }
    }
}
