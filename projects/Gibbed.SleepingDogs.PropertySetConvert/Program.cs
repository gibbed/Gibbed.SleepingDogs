﻿/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.SleepingDogs.FileFormats;
using NDesk.Options;

namespace Gibbed.SleepingDogs.PropertySetConvert
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var mode = Mode.Unknown;
            bool showHelp = false;
            bool verbose = false;

            OptionSet options = new()
            {
                { "e|export", "convert from binary to XML", v => { if (v != null) { mode = Mode.Export; } } },
                { "i|import", "convert from XML to binary", v => { if (v != null) { mode = Mode.Import; } } },
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

            // detect!
            if (mode == Mode.Unknown && extras.Count >= 1)
            {
                if (Directory.Exists(extras[0]) == true)
                {
                    mode = Mode.Import;
                }
                else if (File.Exists(extras[0]) == true)
                {
                    if (Path.GetFileName(extras[0]) == "@resource.xml")
                    {
                        mode = Mode.Import;
                    }
                    else
                    {
                        mode = Mode.Export;
                    }
                }
            }

            if (mode == Mode.Unknown ||
                showHelp == true ||
                extras.Count < 1 || extras.Count > 2)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ [-e] input_bin [output_dir]", ProjectHelpers.GetExecutableName());
                Console.WriteLine("       {0} [OPTIONS]+ [-i] input_dir [output_bin]", ProjectHelpers.GetExecutableName());
                Console.WriteLine("Convert a property sets file between binary and XML format.");
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

            var project = ProjectData.Project.Load(projectPath);
            if (project == null)
            {
                Console.WriteLine("Failed to load project!");
                return;
            }

            if (mode == Mode.Export)
            {
                Exporter.Run(project, extras);
            }
            else if (mode == Mode.Import)
            {
                Importer.Run(project, extras);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
