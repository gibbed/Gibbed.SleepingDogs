/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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
using System.Text.RegularExpressions;
using Gibbed.IO;
using Gibbed.SleepingDogs.FileFormats;
using NDesk.Options;
using BigFileIndex = Gibbed.SleepingDogs.DataFormats.BigFileIndex;

namespace Gibbed.SleepingDogs.BigUnpack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool extractUnknowns = true;
            string filterPattern = null;
            bool overwriteFiles = false;
            bool paranoia = false;
            bool verbose = false;

            OptionSet options = new()
            {
                { "o|overwrite", "overwrite existing files", v => overwriteFiles = v != null },
                { "nu|no-unknowns", "don't extract unknown files", v => extractUnknowns = v == null },
                { "f|filter=", "only extract files using pattern", v => filterPattern = v },
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
                Console.Write("{0}: ", ProjectHelpers.GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", ProjectHelpers.GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_bix [output_dir]", ProjectHelpers.GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string bixPath = Path.GetFullPath(extras[0]);
            string outputPath = extras.Count > 1
                ? Path.GetFullPath(extras[1])
                : Path.ChangeExtension(bixPath, null) + "_unpack";

            Regex filter = null;
            if (string.IsNullOrEmpty(filterPattern) == false)
            {
                filter = new(filterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            var projectPath = ProjectHelpers.GetProjectPath();
            if (File.Exists(projectPath) == false)
            {
                Console.WriteLine($"Project file '{projectPath}' is missing!");
                return;
            }

            if (verbose == true)
            {
                Console.WriteLine("Loading project...");
            }

            var project = ProjectData.Project.Load(projectPath);
            if (project == null)
            {
                Console.WriteLine("Failed to load project!");
                return;
            }

            var hashes = project.LoadListsBigNames();

            BigFileInventory bix = new();
            using (var input = File.OpenRead(bixPath))
            {
                bix.Deserialize(input, Endian.Little);
            }

            var basePath = Path.GetDirectoryName(bixPath);
            if (string.IsNullOrEmpty(basePath) == true)
            {
                throw new InvalidOperationException();
            }

            var bigPath = Path.Combine(basePath, bix.BigFileName + ".big");

            using (var input = File.OpenRead(bigPath))
            {
                long current = 0;
                long total = bix.Entries.Count;

                foreach (var entry in bix.Entries)
                {
                    current++;

                    string name = hashes[entry.Id];
                    if (name == null)
                    {
                        if (extractUnknowns == false)
                        {
                            continue;
                        }

                        KeyValuePair<string, string> extension;

                        // detect type
                        {
                            var guessLength = (int)Math.Min(entry.Size.UncompressedSize, 64);
                            byte[] guess;
                            using (var temp = new MemoryStream())
                            {
                                LoadEntry(input, entry, temp, guessLength);
                                temp.Flush();
                                guess = temp.ToArray();
                            }
                            extension = FileDetection.Detect(guess, guess.Length);
                        }

                        name = entry.Id.ToString("X8");
                        name = Path.ChangeExtension(name, "." + extension.Value);
                        name = Path.Combine(extension.Key, name);
                        name = Path.Combine("__UNKNOWN", name);
                    }
                    else
                    {
                        name = name.Replace(@"/", @"\");
                        if (name.StartsWith(@"\") == true)
                        {
                            name = name.Substring(1);
                        }
                    }

                    if (filter != null && filter.IsMatch(name) == false)
                    {
                        continue;
                    }

                    var entryPath = Path.Combine(outputPath, name);
                    if (overwriteFiles == false && File.Exists(entryPath) == true)
                    {
                        continue;
                    }

                    var entryParentPath = Path.GetDirectoryName(entryPath);
                    if (string.IsNullOrEmpty(entryParentPath) == true)
                    {
                        throw new InvalidOperationException();
                    }
                    Directory.CreateDirectory(entryParentPath);

                    if (verbose == true)
                    {
                        Console.WriteLine("[{0}/{1}] {2}", current, total, name);
                    }

                    using (var output = File.Create(entryPath))
                    {
                        LoadEntry(input, entry, output);
                    }
                }
            }
        }

        private static void LoadEntry(Stream input, BigFileIndex.Entry entry, Stream output)
        {
            LoadEntry(input, entry, output, entry.Size.UncompressedSize);
        }

        private static void LoadEntry(Stream input, BigFileIndex.Entry entry, Stream output, long length)
        {
            if (length > entry.Size.UncompressedSize)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (entry.Size.CompressedSize == 0 ||
                entry.Size.CompressedSize == entry.Size.UncompressedSize)
            {
                if (entry.Size.LoadOffset != 0 || entry.Size.CompressedExtra != 0)
                {
                    throw new InvalidOperationException();
                }

                if (length > 0)
                {
                    input.Position = entry.Offset;
                    output.WriteFromStream(input, length);
                }
            }
            else
            {
                var uncompressedSize =
                    entry.Size.CompressedSize +
                    entry.Size.LoadOffset -
                    entry.Size.CompressedExtra;
                if (uncompressedSize != entry.Size.UncompressedSize)
                {
                    throw new InvalidOperationException();
                }

                if (length > 0)
                {
                    input.Position = entry.Offset + (entry.Size.LoadOffset & 0xFFF);
                    QuickCompression.Decompress(input, output, length);
                }
            }
        }
    }
}
