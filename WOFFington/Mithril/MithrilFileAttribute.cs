using System;
using JetBrains.Annotations;

namespace WOFFington.Mithril
{
    /// <inheritdoc />
    /// <summary>
    ///     Contains Meta-info for Mithril file types
    /// </summary>
    [BaseTypeRequired(typeof(IMithrilFile))]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MithrilFileAttribute : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        ///     Create new instance with the specified type
        /// </summary>
        /// <param name="fileType"></param>
        public MithrilFileAttribute(MithrilFileType fileType)
        {
            FileType = fileType;
        }

        /// <summary>
        ///     File type this MithrilFile loads
        /// </summary>
        public MithrilFileType FileType { get; }

        /// <inheritdoc />
        public override object TypeId => $"WOFFington.Mithril.MithrilFileAttribute[{FileType:X}]";

        /// <inheritdoc />
        public override bool IsDefaultAttribute()
        {
            return FileType == 0;
        }

        /// <inheritdoc />
        public override bool Match(object obj)
        {
            switch (obj)
            {
                case MithrilFileAttribute attr:
                    return attr.FileType == FileType;
                default:
                    return base.Equals(obj);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case MithrilFileType ft:
                    return ft == FileType;
                case MithrilFileAttribute attr:
                    return attr.FileType == FileType;
                default:
                    return base.Equals(obj);
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) FileType;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(MithrilFileAttribute)}] {FileType}";
        }
    }
}
