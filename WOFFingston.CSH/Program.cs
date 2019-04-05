using System.IO;
using WOFFington.Csh;
using WOFFington.Extensions;
using WOFFington.Mithril;

namespace WOFFingston.CSH
{
    internal static class Program
    {
        public static void Main()
        {
            // TODO: Testing for now.
            const string file = @"csv\character_list.csh";

            using (var stream = new MithrilCompressedFile(file))
            {
                using (new RememberStream(stream))
                {
                    stream.Position = 0;

                    using (var fs = File.OpenWrite(file + ".inflate"))
                    {
                        stream.CopyTo(fs);
                    }
                }

                var csh = new CshFile(stream);
                File.WriteAllText(Path.ChangeExtension(file, "csv"), csh.ToString());
            }
        }
    }
}
