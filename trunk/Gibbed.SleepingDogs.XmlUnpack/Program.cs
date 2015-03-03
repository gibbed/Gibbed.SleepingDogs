/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.SleepingDogs.FileFormats;
using NDesk.Options;

namespace Gibbed.SleepingDogs.XmlUnpack
{
    public class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool overwriteFiles = false;
            bool paranoia = false;
            bool verbose = false;

            var options = new OptionSet()
            {
                { "o|overwrite", "overwrite existing files", v => overwriteFiles = v != null },
                { "p|paranoid", "be paranoid (validate hash when uncompressing files)", v => paranoia = v != null },
                { "v|verbose", "be verbose", v => verbose = v != null },
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_bin [output_dir]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null) + "_unpack";

            var manager = ProjectData.Manager.Load();
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Warning: no active project loaded.");
            }

            var hashes = manager.LoadListsXmlNames();

            var inventory = new XmlFileInventory();
            using (var input = File.OpenRead(inputPath))
            {
                inventory.Deserialize(input, Endian.Little);
            }

            long current = 0;
            long total = inventory.Items.Count;

            foreach (var item in inventory.Items)
            {
                current++;

                string path = item.DebugName;

                if (hashes.Contains(item.Id) == false)
                {
                    if (path.HashFileName() != item.Id)
                    {
                        // todo: make this look up correct names from lists
                        Console.WriteLine(
                            "Hash of {0:X8} doesn't match hash of '{1}' -- name probably got truncated!",
                            item.Id,
                            item.DebugName);
                        path = string.Format(@"__TRUNCATED\{0}_{1:X8}", item.DebugName, item.Id);
                    }
                }
                else
                {
                    path = hashes[item.Id];
                }

                path = path.Replace("/", "\\");
                if (path.StartsWith("\\") == true)
                {
                    path = path.Substring(1);
                }

                var entryPath = Path.Combine(outputPath, path);
                var entryParentPath = Path.GetDirectoryName(entryPath);
                if (entryParentPath != null)
                {
                    Directory.CreateDirectory(entryParentPath);
                }

                if (overwriteFiles == false && File.Exists(entryPath) == true)
                {
                    continue;
                }

                if (verbose == true)
                {
                    Console.WriteLine("[{0}/{1}] {2}", current, total, path);
                }

                using (var output = File.Create(entryPath))
                {
                    output.WriteBytes(item.Data);
                }
            }
        }
    }
}
