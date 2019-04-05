namespace WOFFington.Mithril
{
    /// <summary>
    ///     The different compression types found in mostly csh files.
    /// </summary>
    public enum MithrilCompressionType
    {
        /// <summary>
        ///     Not compressed.
        /// </summary>
        Flat = 0,

        /// <summary>
        ///     Zlib DEFLATE compressed
        /// </summary>
        Zlib = 7
    }
}
