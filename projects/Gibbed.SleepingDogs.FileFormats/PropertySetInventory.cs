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

using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.FileFormats
{
    public class PropertySetInventory : ResourceInventory<DataFormats.PropertySetResource, PropertySetInventory.Item>
    {
        public const uint TypeId = 0x54606C31;
        public const uint ChunkId = 0x5B9BF81E;

        private readonly PropertySetFormats.PropertySetSchemaProvider _SchemaProvider;

        public PropertySetInventory()
            : base(TypeId, ChunkId)
        {
            this._SchemaProvider = new PropertySetFormats.PropertySetSchemaProvider();
        }

        public struct Item
        {
            public uint Id;
            public string DebugName;
            public DataFormats.PropertySetResourceFlags Flags;
            public uint SourceTextHash;
            public string Name;
            public PropertySetFormats.PropertySet Root;

            public override string ToString()
            {
                return this.Name;
            }

            public static string GetDebugName(string name)
            {
                if (name == null || name.Length <= 35)
                {
                    return name;
                }

                return name.Substring(0, 20) + "~" + name.Substring(name.Length - 14);
            }
        }

        protected override void Import(
            Item item,
            Stream data,
            Endian endian,
            DataFormats.PropertySetResource resource,
            long ownerOffset)
        {
            item.Root.Write(data, endian, resource.Root, ownerOffset + 104, this._SchemaProvider);

            var nameOffset = data.Position;
            data.WriteStringZ(item.Name, Encoding.UTF8);

            resource.Id = item.Id;
            resource.DebugName = item.DebugName;
            resource.Flags = item.Flags;
            resource.SourceTextHash = item.SourceTextHash;
            resource.NameOffset = nameOffset;
        }

        protected override Item Export(
            DataFormats.PropertySetResource resource,
            Stream data,
            Endian endian)
        {
            string name = null;
            if (resource.NameOffset != 0)
            {
                data.Position = resource.NameOffset;
                name = data.ReadStringZ(Encoding.UTF8);
            }

            return new Item
            {
                Id = resource.Id,
                DebugName = resource.DebugName,
                Flags = resource.Flags,
                SourceTextHash = resource.SourceTextHash,
                Name = name,
                Root = PropertySetFormats.PropertySet.Read(data, resource.Root, endian, this._SchemaProvider),
            };
        }
    }
}
