using System;
using System.IO;
using System.Linq;
using WOFFington.Extensions;
using WOFFington.Mithril;

namespace WOFFingston.CSH
{
    internal static class Program
    {
        public static void Main()
        {
            // TODO: Testing for now.
            const string fileDir1 = @"csv";
            const string fileDir2 = @"csv\message\us";
            const string filePattern = "*.csh";

            foreach (var file in Directory.GetFiles(fileDir1, filePattern)
                .Concat(Directory.GetFiles(fileDir2, filePattern)))
            {
                Console.WriteLine(file);
                using (var stream = new MithrilCompressedFile(file))
                {
                    using (new RememberStream(stream))
                    using (var fs = File.OpenWrite(Path.ChangeExtension(file, ".inflate")))
                    {
                        stream.CopyTo(fs);
                    }

                    var mithrilFile = stream.Deserialize();
                    File.WriteAllText(Path.ChangeExtension(file, ".csv"), mithrilFile.ToString());
                }
            }
        }
    }
}
