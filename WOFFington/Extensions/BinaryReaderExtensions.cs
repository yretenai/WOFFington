using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace WOFFington.Extensions
{
    /// <summary>
    ///     Series of helper functions embedded into <seealso cref="BinaryReader" />
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        ///     Read an 32-bit integer but as Big Endian, because Mithril apparently does that.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>big-endian 32-bit integer</returns>
        public static int ReadInt32BE(this BinaryReader reader)
        {
            return IPAddress.NetworkToHostOrder(reader.ReadInt32());
        }

        /// <summary>
        ///     Read an 32-bit integer array given size <paramref name="count" />
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="count">elements to read</param>
        /// <returns>array of 32-bit host-endian integers</returns>
        public static int[] ReadInt32Array(this BinaryReader reader, int count)
        {
            if (count == 0) return Array.Empty<int>();
            var arr = new int[count];
            var bytes = reader.ReadBytes(count * 4);
            unsafe
            {
                fixed (byte* pin = bytes)
                {
                    Marshal.Copy((IntPtr) pin, arr, 0, count);
                }
            }

            return arr;
        }
    }
}
