using System;
using System.IO;
using JetBrains.Annotations;

namespace WOFFington.Extensions
{
    /// <inheritdoc />
    /// <summary>
    ///     Helper class to remember the position of a stream after an operation has been performed
    /// </summary>
    public class RememberStream : IDisposable
    {
        private readonly Stream Stream;

        /// <summary>
        ///     Initialize with a stream to remember.
        /// </summary>
        /// <param name="stream">the stream</param>
        public RememberStream([NotNull] Stream stream)
        {
            Stream = stream;
            Position = stream.Position;
        }

        private long Position { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Restore stream position
        /// </summary>
        public void Dispose()
        {
            Stream.Position = Position;
        }

        /// <summary>
        ///     Resets the cached stream position.
        /// </summary>
        public void Forget()
        {
            Position = Stream.Position;
        }
    }
}
