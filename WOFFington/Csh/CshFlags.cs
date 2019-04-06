using System;

namespace WOFFington.Csh
{
    /// <summary>
    ///     Flags used for Serializing
    /// </summary>
    [Flags]
    public enum CshFlags : uint
    {
        /// <summary>
        ///     Any value under this is a string
        /// </summary>
        StringBoundary = 0x0FFF,

        /// <summary>
        ///     Value is BE
        /// </summary>
        BigEndian = 0x1,

        /// <summary>
        ///     Value is LE
        /// </summary>
        LittleEndian = 0x2,

        /// <summary>
        ///     BE int32
        /// </summary>
        Int = 0x40000000,

        /// <summary>
        ///     LE int32?
        /// </summary>
        Float = 0x80000000
    }
}
