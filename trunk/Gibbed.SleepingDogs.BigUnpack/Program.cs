﻿/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.SleepingDogs.FileFormats;
using Gibbed.IO;
using NDesk.Options;

namespace Gibbed.SleepingDogs.BigUnpack
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
            bool extractUnknowns = true;
            string filterPattern = null;
            bool overwriteFiles = false;
            bool paranoia = false;
            bool verbose = false;

            var options = new OptionSet()
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
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_bix [output_dir]", GetExecutableName());
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
                filter = new Regex(filterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            var manager = ProjectData.Manager.Load();
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Warning: no active project loaded.");
            }

            var hashes = manager.LoadListsBigNames();

            var bix = new BigFileInventory();
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
                            var guess = new byte[64];
                            int read = 0;

                            var offset = (entry.Offset << 2) + (entry.Size.LoadOffset & 0xFFF);
                            if (entry.Size.CompressedSize == 0)
                            {
                                if (entry.Size.UncompressedSize > 0)
                                {
                                    input.Seek(offset, SeekOrigin.Begin);
                                    read = input.Read(guess, 0, (int)Math.Min(entry.Size.UncompressedSize, guess.Length));
                                }
                            }
                            else
                            {
                                input.Seek(offset, SeekOrigin.Begin);

                                // todo: don't uncompress everything
                                var uncompressedData = QuickCompression.Decompress(input);
                                read = Math.Min(guess.Length, uncompressedData.Length);
                                Array.Copy(uncompressedData, 0, guess, 0, read);
                            }

                            extension = FileDetection.Detect(guess, Math.Min(guess.Length, read));
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
                        if (entry.Size.CompressedSize == 0)
                        {
                            if (entry.Size.LoadOffset != 0 ||
                                entry.Size.CompressedExtra != 0)
                            {
                                throw new InvalidOperationException();
                            }

                            if (entry.Size.UncompressedSize > 0)
                            {
                                input.Seek(entry.Offset << 2, SeekOrigin.Begin);
                                output.WriteFromStream(input, entry.Size.UncompressedSize);
                            }
                        }
                        else
                        {
                            var uncompressedSize = entry.Size.CompressedSize +
                                                   entry.Size.LoadOffset -
                                                   entry.Size.CompressedExtra;
                            if (uncompressedSize != entry.Size.UncompressedSize)
                            {
                            }

                            if (entry.Size.UncompressedSize > 0)
                            {
                                input.Seek((entry.Offset << 2) + (entry.Size.LoadOffset & 0xFFF), SeekOrigin.Begin);
                                QuickCompression.Decompress(input, output);
                            }
                        }
                    }
                }
            }
        }
    }
}
