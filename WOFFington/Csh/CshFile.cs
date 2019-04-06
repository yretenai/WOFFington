using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using WOFFington.Extensions;

namespace WOFFington.Csh
{
    /// <summary>
    ///     Reads and decodes a Csh file
    /// </summary>
    public class CshFile
    {
        /// <summary>
        ///     Parse the given <paramref name="data" /> into row data
        /// </summary>
        /// <param name="data">stream to parse</param>
        /// <exception cref="NotSupportedException">When a cell is an unknown type</exception>
        public CshFile([NotNull] Stream data)
        {
            using (var reader = new BinaryReader(data, Encoding.UTF8, true))
            {
                Type = (CshType) reader.ReadInt32BE();
                Contract.Assert(Type == CshType.Normal, "Type == CshType.Normal");
                ColumnCount = reader.ReadInt32BE();
                RowCount = reader.ReadInt32BE();
                Meta = reader.ReadInt32Array(ColumnCount * RowCount * 2)
                    .Select((value, i) => (i, IPAddress.HostToNetworkOrder(value)))
                    .GroupBy(x => Math.Floor(x.Item1 / 2d), x => x.Item2)
                    .Select((x, i) => (i, new CshMeta(x.ElementAt(0), (CshFlags) x.ElementAt(1))))
                    .GroupBy(x => x.Item1 / ColumnCount, x => x.Item2).Select(x => x.ToArray()).ToArray();

                Rows = new object[RowCount][];

                for (var i = 0; i < RowCount; ++i)
                {
                    Rows[i] = new object[ColumnCount];
                    for (var j = 0; j < ColumnCount; ++j)
                    {
                        var meta = Meta[i][j];

                        if (meta.Offset == 0)
                        {
                            Rows[i][j] = (uint)meta.Flags;
                        }
                        else
                        {
                            using (new RememberStream(data))
                            {
                                data.Position = meta.Offset;
                                if (meta.Flags <= CshFlags.StringBoundary)
                                {
                                    var buffer = new List<(int, byte)>();

                                    byte b;
                                    while ((b = reader.ReadByte()) != 0x00)
                                    {
                                        buffer.Add((buffer.Count, b));
                                    }

                                    Rows[i][j] = Encoding.UTF8.GetString(buffer.OrderBy(x => x.Item1)
                                        .Select(x => x.Item2).ToArray());
                                }
                                else if (meta.Flags.HasFlag(CshFlags.Int) || meta.Flags.HasFlag(CshFlags.Float))
                                {
                                    Contract.Assert(
                                        meta.Flags.HasFlag(CshFlags.Float) || meta.Flags.HasFlag(CshFlags.Int),
                                        "meta.Flags.HasFlag(CshFlags.BigEndianInt) || meta.Flags.HasFlag(CshFlags.LittleEndianInt)");
                                    Contract.Assert(meta.Flags.HasFlag(CshFlags.BigEndian),
                                        "meta.Flags.HasFlag(CshFlags.BigEndian)");
                                    Contract.Assert((uint) meta.Flags == 0x40000001 || (uint) meta.Flags == 0x80000001,
                                        "(uint)meta.Flags == 0x40000001 || (uint)meta.Flags == 0x80000001");
                                    object value = reader.ReadInt32();
                                    if (meta.Flags.HasFlag(CshFlags.BigEndian))
                                    {
                                        value = IPAddress.HostToNetworkOrder((int) value);
                                    }

                                    if (meta.Flags.HasFlag(CshFlags.Float))
                                    {
                                        value = BitConverter.ToSingle(BitConverter.GetBytes((int) value), 0);
                                    }

                                    Rows[i][j] = value;
                                }
                                else
                                {
                                    throw new NotSupportedException(((uint) meta.Flags).ToString("X"));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     File type enum
        /// </summary>
        public CshType Type { get; }

        /// <summary>
        ///     Number of columns per row
        /// </summary>
        public int ColumnCount { get; }

        /// <summary>
        ///     Number of rows
        /// </summary>
        public int RowCount { get; }

        /// <summary>
        ///     Type info
        /// </summary>
        public CshMeta[][] Meta { get; }

        /// <summary>
        ///     Parsed rows
        /// </summary>
        public object[][] Rows { get; }

        /// <summary>
        ///     Output into CSV
        /// </summary>
        /// <returns>csv string</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var row in Rows)
            {
                builder.AppendLine(string.Join(", ", row.Select(x =>
                {
                    switch (x)
                    {
                        case null:
                            return string.Empty;
                        case string s:
                            return $"\"{s.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
                        default:
                            return x.ToString();
                    }
                })));
            }

            return builder.ToString();
        }
    }
}
