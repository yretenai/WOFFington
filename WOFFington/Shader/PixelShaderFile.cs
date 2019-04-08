using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using WOFFington.Extensions;
using WOFFington.Mithril;

namespace WOFFington.Shader
{
    /// <inheritdoc />
    /// <summary>
    ///     Loads psb files
    /// </summary>
    [MithrilFile(MithrilFileType.PixelShader)]
    public class PixelShaderFile : IMithrilFile
    {
        private const int PSB_MAGIC = 0x70736200;

        /// <summary>
        ///     Load the given <paramref name="data" /> into shader block
        /// </summary>
        /// <param name="data">stream to parse</param>
        public PixelShaderFile([NotNull] Stream data)
        {
            Load(data, PSB_MAGIC);
        }

        /// <summary>
        ///     Dummy for initialization
        /// </summary>
        [UsedImplicitly]
        public PixelShaderFile()
        {
        }

        /// <summary>
        ///     Shader Byte Code
        /// </summary>
        public byte[] ShaderBytes { get; set; }

        /// <summary>
        ///     Miscellaneous integers
        /// </summary>
        public int[] Ints { get; set; }

        /// <summary>
        ///     EOS Data
        /// </summary>
        public byte[] ExtraData { get; set; }

        /// <inheritdoc />
        public string Extension { get; } = ".ps.fxo";

        /// <inheritdoc />
        /// <summary>
        ///     Load the given <paramref name="data" /> into shader block
        /// </summary>
        /// <param name="data">stream to parse</param>
        /// <param name="magicNumber">number to verify</param>
        public void Load(Stream data, int magicNumber)
        {
            Contract.Assert(magicNumber == PSB_MAGIC, "magicNumber == PSB_MAGIC");

            using (var reader = new BinaryReader(data, Encoding.UTF8, true))
            {
                var bytes = reader.ReadInt32();
                Ints = reader.ReadInt32Array(15);
                ShaderBytes = new byte[bytes];
                data.Read(ShaderBytes, 0, bytes);
                if (data.Length == data.Position)
                {
                    ExtraData = Array.Empty<byte>();
                }
                else
                {
                    ExtraData = new byte[data.Length - data.Position];
                    data.Read(ExtraData, 0, ExtraData.Length);
                }
            }
        }

        /// <inheritdoc />
        public void Export(string path)
        {
            using (Stream fp = File.OpenWrite(path))
            {
                Export(fp);
            }
        }

        /// <inheritdoc />
        public void Export(Stream stream)
        {
            stream.Write(ShaderBytes, 0, ShaderBytes.Length);
        }
    }
}
