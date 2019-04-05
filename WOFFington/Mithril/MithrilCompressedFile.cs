using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using WOFFington.Extensions;
using zlib;

namespace WOFFington.Mithril
{
    /// <inheritdoc />
    /// <summary>
    ///     Mithril file stream, usually just unwraps and decompresses a file.
    /// </summary>
    public class MithrilCompressedFile : Stream
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initialize a stream given the <paramref name="baseStream" />
        /// </summary>
        /// <param name="baseStream">The underlying stream to unwrap</param>
        public MithrilCompressedFile([NotNull] Stream baseStream)
        {
            InitializeLifetime(baseStream);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initialize a stream given the <paramref name="filepath" />
        /// </summary>
        /// <param name="filepath">File to unwrap.</param>
        /// <exception cref="T:System.IO.FileNotFoundException">Thrown when <paramref name="filepath" /> does not exist</exception>
        public MithrilCompressedFile([NotNull] string filepath)
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException(filepath);

            using (var stream = File.OpenRead(filepath))
            {
                InitializeLifetime(stream);
            }
        }

        /// <summary>
        ///     Unwraps and decompresses given the <paramref name="baseStream" />.
        ///     This is a shadow copy of <seealso cref="MithrilCompressedFile(System.IO.Stream)" />
        ///     Considerations:
        ///     There are 31 unknown integers in the header structure, in debug builds these will assert for known values.
        ///     If an assertion is trapped, please note the file!
        /// </summary>
        /// <param name="baseStream">Stream to work with</param>
        /// <exception cref="InvalidEnumArgumentException">
        ///     Thrown when the <seealso cref="CompressionType" /> is not
        ///     recognized
        /// </exception>
        private void InitializeLifetime([NotNull] Stream baseStream)
        {
            using (var Reader = new BinaryReader(baseStream, Encoding.UTF8, true))
            {
                Magic = Reader.ReadInt32();
                Contract.Assert(Magic == 0x63736800, nameof(Magic) + " == 0x63736800");

                Ints = Reader.ReadInt32Array(0x1F).Select(IPAddress.NetworkToHostOrder).ToArray();
#if DEBUG
                Contract.Assert(Ints[0] == 0x2, "Ints[0] == 0x2");
                Contract.Assert(Ints[1] == 0xD, "Ints[1] == 0xD");
                Contract.Assert(Ints[2] == 0x2, "Ints[2] == 0x2");
                Contract.Assert(Ints[9] == 0x1, "Ints[9] == 0x1");
                for (var i = 0; i < Ints.Length; ++i)
                {
                    if (i <= 2 || i == 9) continue;
                    Contract.Assert(Ints[i] == 0x0, "Ints[i] == 0x0");
                }
#endif

                ZlibMagic = Reader.ReadInt32();
                Contract.Assert(ZlibMagic == 0x5A4C4942, nameof(ZlibMagic) + " == 0x5A4C4942");
                UncompressedSize = Reader.ReadInt32BE();
                CompressedSize = Reader.ReadInt32BE();
                CompressionType = (MithrilCompressionType) Reader.ReadInt32BE();
            }

            BaseStream = new MemoryStream(UncompressedSize);

            switch (CompressionType)
            {
                case MithrilCompressionType.Flat:
                    baseStream.CopyTo(BaseStream);
                    break;
                case MithrilCompressionType.Zlib:
                    // I tried DeflateStream(CompressionMode.Decompress), but it failed.
                    using (var shadow = new MemoryStream(UncompressedSize))
                    using (var zlib = new ZOutputStream(shadow))
                    {
                        var counter = 0;
                        var buffer = new byte[0x1024];
                        while (counter < CompressedSize)
                        {
                            var sz = Math.Min(CompressedSize - counter, 0x1024);
                            baseStream.Read(buffer, 0, sz);
                            zlib.Write(buffer, 0, sz);
                            counter += sz;
                        }

                        zlib.Flush();
                        shadow.Position = 0;
                        shadow.CopyTo(BaseStream);
                    }

                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            BaseStream.Position = 0;
        }

        #region Vars

        /// <summary>
        ///     The underlying decompressed stream
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        ///     Magic number of this Mithril file
        /// </summary>
        public int Magic { get; private set; }

        /// <summary>
        ///     Unknown ints
        /// </summary>
        public int[] Ints { get; private set; }

        /// <summary>
        ///     Mithril Zlib magic header
        /// </summary>
        public int ZlibMagic { get; private set; }

        /// <summary>
        ///     Target size when decompressed
        /// </summary>
        public int UncompressedSize { get; private set; }

        /// <summary>
        ///     Target size when compressed
        /// </summary>
        public int CompressedSize { get; private set; }

        /// <summary>
        ///     The method this file got compressed
        /// </summary>
        public MithrilCompressionType CompressionType { get; private set; }

        #endregion

        #region Stream

        /// <inheritdoc />
        /// <summary>True</summary>
        public override bool CanRead { get; } = true;

        /// <inheritdoc />
        /// <summary>True</summary>
        public override bool CanSeek { get; } = true;

        /// <inheritdoc />
        /// <summary>False</summary>
        public override bool CanWrite { get; } = false;

        /// <inheritdoc />
        public override long Length => BaseStream.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        /// <inheritdoc />
        /// <exception cref="NotImplementedException"></exception>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        /// <inheritdoc />
        /// <exception cref="NotImplementedException"></exception>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing) BaseStream.Dispose();
        }

        #endregion
    }
}
