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

using System.IO;

namespace Gibbed.SleepingDogs.FileFormats
{
    public static class ProjectHelpers
    {
        public static string GetExecutablePath()
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            var path = Path.GetFullPath(process.MainModule.FileName);
            return Path.GetFullPath(path);
        }

        public static string GetExecutableName()
        {
            return Path.GetFileName(GetExecutablePath());
        }

        public static string GetProjectPath()
        {
            // TODO(gibbed): support more games later?
            const string projectName = "Sleeping Dogs Definitive Edition";
            var executablePath = GetExecutablePath();
            var binPath = Path.GetDirectoryName(executablePath);
            return Path.Combine(binPath, "..", "configs", projectName, "project.json");
        }

        public static ProjectData.HashList<uint> LoadListsBigNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.biglist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsXmlNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.xmllist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.propsetlist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetPropertyNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.proplist", s => s.HashSymbol(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetSymbolNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.symbollist", s => s.HashSymbol(), s => s.Replace('/', '\\'));
        }
    }
}
