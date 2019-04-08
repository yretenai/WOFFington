using System;
using System.Diagnostics.Contracts;
using System.IO;
using WOFFington.Mithril;

namespace WOFFington.Shader
{
    /// <inheritdoc />
    /// <summary>
    ///     Loads SPK shader bundles.
    /// </summary>
    // TODO
    [MithrilFile(MithrilFileType.ShaderPack)]
    public class ShaderPackFile : IMithrilFile
    {
        private const int SPK_MAGIC = 0x73706B00;

        /// <inheritdoc />
        public string Extension { get; } = ".fxo.pak";

        /// <inheritdoc />
        public void Load(Stream data, int magicNumber)
        {
            Contract.Assert(magicNumber == SPK_MAGIC, "magicNumber == SPK_MAGIC");
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Export(string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Export(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
