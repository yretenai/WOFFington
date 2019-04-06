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
        ///     Parse the given <paramref name="data" />
        /// </summary>
        /// <param name="data">stream to parse</param>
        void Load([NotNull] Stream data);
    }
}
