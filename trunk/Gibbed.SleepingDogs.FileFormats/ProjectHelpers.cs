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

namespace Gibbed.SleepingDogs.FileFormats
{
    public static class ProjectHelpers
    {
        public static ProjectData.HashList<uint> LoadListsBigNames(this ProjectData.Manager manager)
        {
            return manager.LoadLists("*.biglist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsBigNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.biglist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsXmlNames(this ProjectData.Manager manager)
        {
            return manager.LoadLists("*.xmllist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsXmlNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.xmllist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetNames(this ProjectData.Manager manager)
        {
            return manager.LoadLists("*.propsetlist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.propsetlist", s => s.HashFileName(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetPropertyNames(this ProjectData.Manager manager)
        {
            return manager.LoadLists("*.proplist", s => s.HashSymbol(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetPropertyNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.proplist", s => s.HashSymbol(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetSymbolNames(this ProjectData.Manager manager)
        {
            return manager.LoadLists("*.symbollist", s => s.HashSymbol(), s => s.Replace('/', '\\'));
        }

        public static ProjectData.HashList<uint> LoadListsPropertySetSymbolNames(this ProjectData.Project project)
        {
            return project.LoadLists("*.symbollist", s => s.HashSymbol(), s => s.Replace('/', '\\'));
        }
    }
}
