using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication9
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) ExitWithUsage();
            if (!File.Exists(args[0])) ExitWithUsage("Can not find file " + args[0]);

            var filename = Path.GetFileNameWithoutExtension(args[0]);
            var extension = Path.GetExtension(args[0]);

            var newFilename = filename + ".stripped." + extension;

            var stream = File.OpenRead(args[0]);
			stream.Position = 0;
			var inStream = new GZipStream(stream, CompressionMode.Decompress);

            var rdr = new JsonTextReader(new StreamReader(inStream));
            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            });
            dynamic obj = jsonSerializer.Deserialize(rdr);
            obj.Identities = new JArray();

            using (var fileStream = File.Open(newFilename, FileMode.Create))
            {
                using (var wrt = new JsonTextWriter(new StreamWriter(new GZipStream(fileStream, CompressionMode.Compress))))
                {
                    jsonSerializer.Serialize(wrt, obj);
                }
            }
        }

        private static void ExitWithUsage(string s = "")
        {
            Console.Out.WriteLine(s);
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Usage: RavenDumpIdentityStripper <dumpfile>.dump");
            Console.Out.WriteLine("Create a new ravendb dump file with the name <dumpfile>.stripped.dump");
            Console.Out.WriteLine("The new dump file has the identities stripped");
            Console.Out.WriteLine("");
        }
    }
}
