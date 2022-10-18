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
using System.Linq;
using Gibbed.IO;
using Gibbed.SleepingDogs.FileFormats;
using NDesk.Options;

namespace RebuildFileLists
{
    internal class Program
    {
        private static string GetListPath(string installPath, string inputPath)
        {
            installPath = installPath.ToLowerInvariant();
            inputPath = inputPath.ToLowerInvariant();

            if (inputPath.StartsWith(installPath) == false)
            {
                return null;
            }

            var baseName = inputPath.Substring(installPath.Length + 1);

            string outputPath;
            outputPath = Path.Combine("files", baseName);
            outputPath = Path.ChangeExtension(outputPath, ".biglist");
            return outputPath;
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool verbose = false;

            OptionSet options = new()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
                { "v|verbose", "be verbose", v => verbose = v != null },
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

            if (extras.Count != 0 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+", ProjectHelpers.GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
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

            var project = Gibbed.ProjectData.Project.Load(projectPath);
            if (project == null)
            {
                Console.WriteLine("Failed to load project!");
                return;
            }

            var hashes = project.LoadListsBigNames();

            var installPath = project.InstallPath;
            var listsPath = project.ListsPath;

            if (installPath == null)
            {
                Console.WriteLine("Could not detect install path.");
                return;
            }
            else if (listsPath == null)
            {
                Console.WriteLine("Could not detect lists path.");
                return;
            }

            Console.WriteLine("Searching for archives...");
            List<string> inputPaths = new();
            inputPaths.AddRange(Directory.GetFiles(installPath, "*.bix", SearchOption.AllDirectories));

            List<string> outputPaths = new();

            Breakdown breakdown = new();

            Console.WriteLine("Processing...");
            foreach (var inputPath in inputPaths)
            {
                var outputPath = GetListPath(installPath, inputPath);
                if (outputPath == null)
                {
                    throw new InvalidOperationException();
                }

                Console.WriteLine(outputPath);
                outputPath = Path.Combine(listsPath, outputPath);

                if (outputPaths.Contains(outputPath) == true)
                {
                    throw new InvalidOperationException();
                }

                outputPaths.Add(outputPath);

                var bix = new BigFileInventory();

                if (File.Exists(inputPath + ".bak") == true)
                {
                    using var input = File.OpenRead(inputPath + ".bak");
                    bix.Deserialize(input, Endian.Little);
                }
                else
                {
                    using var input = File.OpenRead(inputPath);
                    bix.Deserialize(input, Endian.Little);
                }

                Breakdown localBreakdown = new();

                List<string> names = new();
                foreach (var nameHash in bix.Entries.Select(e => e.Id).Distinct())
                {
                    var name = hashes[nameHash];
                    if (name != null)
                    {
                        if (names.Contains(name) == false)
                        {
                            names.Add(name);
                            localBreakdown.Known++;
                        }
                    }

                    localBreakdown.Total++;
                }

                breakdown.Known += localBreakdown.Known;
                breakdown.Total += localBreakdown.Total;

                names.Sort();

                var outputParentPath = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrEmpty(outputParentPath) == true)
                {
                    throw new InvalidOperationException();
                }
                Directory.CreateDirectory(outputParentPath);

                using (StreamWriter output = new(outputPath))
                {
                    output.WriteLine($"; {localBreakdown}");
                    foreach (string name in names)
                    {
                        output.WriteLine(name);
                    }
                }
            }

            using (StreamWriter output = new(Path.Combine(listsPath, "files", "status.txt")))
            {
                output.WriteLine($"{breakdown}");
            }
        }
    }
}
