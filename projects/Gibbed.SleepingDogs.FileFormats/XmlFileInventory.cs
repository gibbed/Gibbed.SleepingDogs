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
using System.IO;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.FileFormats
{
    public class XmlFileInventory : ResourceInventory<DataFormats.XmlFileResource, XmlFileInventory.Item>
    {
        public const uint TypeId = 0x4FF578D5;
        public const uint ChunkId = 0x24D0C3A0;

        public XmlFileInventory()
            : base(TypeId, ChunkId)
        {
        }

        public struct Item
        {
            public uint Id;
            public string DebugName;
            public byte[] Data;
        }

        protected override void Import(
            Item item,
            Stream stream,
            Endian endian,
            DataFormats.XmlFileResource resource,
            long ownerOffset)
        {
            throw new NotImplementedException();
        }

        protected override Item Export(
            DataFormats.XmlFileResource resource,
            Stream stream,
            Endian endian)
        {
            return new Item()
            {
                Id = resource.Id,
                DebugName = resource.DebugName,
                Data = resource.CompressedSize != 0
                           ? QuickCompression.Decompress(stream)
                           : stream.ReadBytes(resource.UncompressedSize)
            };
        }
    }
}
