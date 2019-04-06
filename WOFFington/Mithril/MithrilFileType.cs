namespace WOFFington.Mithril
{
    /// <summary>
    ///     File Types found in Mithril compressed files
    /// </summary>
    public enum MithrilFileType
    {
        /// <summary>
        ///     Default
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Hashed/Cell-based CSV binary file
        /// </summary>
        Csh = 0x0D
    }
}
