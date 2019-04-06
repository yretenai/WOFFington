using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using JetBrains.Annotations;
using WOFFington.Extensions;

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
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException(filepath);
            }

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
                FileType = (MithrilFileType) Ints[1];
#if DEBUG
                Contract.Assert(Ints[0] == 0x2, "Ints[0] == 0x2");
                Contract.Assert(Ints[1].ToString("X") != FileType.ToString("X"),
                    "Ints[1].ToString('X') != FileType.ToString('X')");
                Contract.Assert(Ints[2] == 0x2, "Ints[2] == 0x2");
                Contract.Assert(Ints[9] == 0x1, "Ints[9] == 0x1");
                for (var i = 0; i < Ints.Length; ++i)
                {
                    if (i <= 2 || i == 9)
                    {
                        continue;
                    }

                    Contract.Assert(Ints[i] == 0x0, "Ints[i] == 0x0");
                }
#endif

                ZlibMagic = Reader.ReadInt32();
                Contract.Assert(ZlibMagic == 0x5A4C4942, nameof(ZlibMagic) + " == 0x5A4C4942");
                UncompressedSize = Reader.ReadInt32BE();
                CompressedSize = Reader.ReadInt32BE();
                CompressionType = (MithrilCompressionType) Reader.ReadInt32BE();
            }


            switch (CompressionType)
            {
                case MithrilCompressionType.Flat:
                    BaseStream = new MemoryStream(UncompressedSize);
                    baseStream.CopyTo(BaseStream);
                    break;
                case MithrilCompressionType.Zlib:
                    // I tried DeflateStream(CompressionMode.Decompress), but it failed.
                    var data = new byte[CompressedSize];
                    var buffer = new byte[UncompressedSize];
                    baseStream.Read(data, 0, CompressedSize);
                    var inflater = new Inflater();
                    inflater.SetInput(data, 0, CompressedSize);
                    inflater.Inflate(buffer, 0, UncompressedSize);
                    BaseStream = new MemoryStream(buffer);

                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            BaseStream.Position = 0;
        }

        /// <summary>
        ///     Deserializes this stream into a parsed file type
        /// </summary>
        /// <returns>IMithrilFile instance of the target filetype</returns>
        public IMithrilFile Deserialize()
        {
            return ParsedFile ?? (ParsedFile = MithrilFileFactory.Parse(this));
        }

        #region Vars

        /// <summary>
        ///     This stream, but parsed
        /// </summary>
        public IMithrilFile ParsedFile { get; private set; }

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
        ///     File type declared by the file.
        /// </summary>
        public MithrilFileType FileType { get; private set; }

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
            if (disposing)
            {
                BaseStream.Dispose();

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (ParsedFile != null && ParsedFile is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(MithrilCompressedFile)}] FileType = {FileType}, Compression Mode = {CompressionType}";
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return BaseStream.GetHashCode() ^ Ints.Sum(x => x.GetHashCode()) ^ CompressedSize ^ UncompressedSize ^
                   (int) CompressionType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is MithrilCompressedFile compressedFile)
            {
                return Ints.SequenceEqual(compressedFile.Ints) && compressedFile.FileType == FileType &&
                       compressedFile.CompressedSize == CompressedSize &&
                       compressedFile.UncompressedSize == CompressedSize &&
                       compressedFile.CompressionType == CompressionType && BaseStream.Equals(compressedFile);
            }

            return false;
        }

        #endregion
    }
}
