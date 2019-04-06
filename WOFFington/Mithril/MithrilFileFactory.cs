using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace WOFFington.Mithril
{
    /// <summary>
    /// </summary>
    public static class MithrilFileFactory
    {
        /// <summary>
        ///     Dictionary of file parsers.
        /// </summary>
        public static Dictionary<MithrilFileType, Type> FileTypes { get; } = new Dictionary<MithrilFileType, Type>();

        /// <summary>
        ///     Parse the stream
        /// </summary>
        /// <param name="stream">stream to parse</param>
        /// <returns>instance of decoded mithril file</returns>
        /// <exception cref="InvalidEnumArgumentException">thrown when the file type is unrecognized</exception>
        public static IMithrilFile Parse([NotNull] MithrilCompressedFile stream)
        {
            if (FileTypes.Count == 0)
            {
                LoadInternal();
            }

            if (!FileTypes.TryGetValue(stream.FileType, out var parser))
            {
                throw new InvalidEnumArgumentException(nameof(stream.FileType));
            }

            var instance = Activator.CreateInstance(parser) as IMithrilFile;
            Contract.Assert(instance != null, nameof(instance) + " != null");
            instance.Load(stream);
            return instance;
        }

        /// <summary>
        ///     Loads internal parser files
        /// </summary>
        /// <exception cref="ArgumentException">when two internal parsers parse the same file type</exception>
        public static void LoadInternal()
        {
            var iMithrilFile = typeof(IMithrilFile);
            var internalTypes = iMithrilFile.Assembly.GetTypes().Select(x => x.FullName).ToHashSet();
            var parsers = iMithrilFile.Assembly.GetTypes().Where(x =>
                iMithrilFile.IsAssignableFrom(x) && x.GetCustomAttributes<MithrilFileAttribute>().Any());

            foreach (var parser in parsers)
            {
                var attributes = parser.GetCustomAttributes<MithrilFileAttribute>();
                foreach (var attribute in attributes)
                {
                    if (FileTypes.TryGetValue(attribute.FileType, out var loadedParser))
                    {
                        if (loadedParser.FullName != parser.FullName && internalTypes.Contains(loadedParser.FullName))
                        {
                            throw new ArgumentException(
                                $"File type {attribute.FileType} is already processed by {loadedParser.FullName} (attempting to add {parser.FullName})");
                        }
                    }
                    else
                    {
                        FileTypes.Add(attribute.FileType, parser);
                    }
                }
            }
        }
    }
}
