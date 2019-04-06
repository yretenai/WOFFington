namespace WOFFington.Csh
{
    /// <summary>
    ///     Row cell metadata
    /// </summary>
    public class CshMeta
    {
        /// <summary>
        ///     Initialize a meta block with params
        /// </summary>
        /// <param name="offset">Offset of value</param>
        /// <param name="flags">Value flags</param>
        public CshMeta(int offset, CshFlags flags)
        {
            Offset = offset;
            Flags = flags;
        }

        /// <summary>
        ///     Offset of the value
        /// </summary>
        public int Offset { get; }

        /// <summary>
        ///     What kind of value it is
        /// </summary>
        public CshFlags Flags { get; }
    }
}
