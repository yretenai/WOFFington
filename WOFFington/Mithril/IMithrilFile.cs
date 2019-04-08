using System.IO;
using JetBrains.Annotations;

namespace WOFFington.Mithril
{
    /// <summary>
    ///     Generic Mithril File
    /// </summary>
    public interface IMithrilFile
    {
        /// <summary>
        ///     Proper extension of this file type.
        /// </summary>
        [PublicAPI]
        string Extension { get; }

        /// <summary>
        ///     Parse the given <paramref name="data" />
        /// </summary>
        /// <param name="data">stream to parse</param>
        /// <param name="magicNumber"></param>
        void Load([NotNull] Stream data, int magicNumber);

        /// <summary>
        ///     Exports the data to the given path.
        /// </summary>
        /// <param name="path"></param>
        [PublicAPI]
        void Export([NotNull] string path);

        /// <summary>
        ///     Exports the data to the given stream.
        /// </summary>
        /// <param name="stream"></param>
        [PublicAPI]
        void Export([NotNull] Stream stream);
    }
}
