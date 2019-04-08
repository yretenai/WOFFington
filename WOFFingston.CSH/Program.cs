using System;
using System.IO;
using WOFFington.Extensions;
using WOFFington.Mithril;

namespace WOFFingston.CSH
{
    internal static class Program
    {
        public static void Main()
        {
            // TODO: Testing for now.
            const string fileDir1 = @"F:\Steam\steamapps\common\WOFF\resource\finalizedCommon";
            const string fileDir2 = @"F:\Steam\steamapps\common\WOFF\resource\finalizedWin64";
            const string targetDir1 = @"extract\common";
            const string targetDir2 = @"extract\win64";

//            LoopDir(fileDir1, targetDir1);
            LoopDir(fileDir2, targetDir2);
        }

        private static void LoopDir(string path, string target)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                var ext = Path.GetExtension(file);
                if (ext == ".po" || ext == ".vo" || ext == ".go") continue;
                Console.WriteLine(file);
                using (var stream = new MithrilCompressedFile(file))
                {
                    CheckDir(target);
                    var targetFile = Path.Combine(target, Path.GetFileName(file));
                    using (new RememberStream(stream))
                    using (var fs = File.OpenWrite(targetFile))
                    {
                        stream.CopyTo(fs);
                    }

                    var mithrilFile = stream.Deserialize();
                    if (mithrilFile == null) continue;

                    targetFile = Path.ChangeExtension(targetFile, mithrilFile.Extension);
                    mithrilFile.Export(targetFile);
                }
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                var name = Path.GetFileName(dir);
                if (name == null) continue;
                LoopDir(Path.Combine(path, name), Path.Combine(target, name));
            }
        }

        private static void CheckDir(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
    }
}
