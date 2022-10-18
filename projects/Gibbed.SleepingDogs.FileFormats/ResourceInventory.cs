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

namespace Gibbed.SleepingDogs.FileFormats
{
    public abstract class ResourceInventory<TResource, TItem>
        where TResource : DataFormats.ResourceData, new()
    {
        private readonly uint _TypeId;
        private readonly uint _ChunkId;
        private readonly List<TItem> _Items;

        protected ResourceInventory(uint typeId, uint chunkId)
        {
            this._TypeId = typeId;
            this._ChunkId = chunkId;
            this._Items = new List<TItem>();
        }

        public List<TItem> Items
        {
            get { return this._Items; }
        }

        public void Serialize(Stream output, Endian endian)
        {
            foreach (var item in this._Items)
            {
                using (var data = new MemoryStream())
                {
                    var startPosition = data.Position;

                    var resource = new TResource();
                    resource.TypeId = this._TypeId;

                    data.Position = resource.Size;

                    this.Import(item, data, endian, resource, startPosition);
                    data.Flush();

                    data.Position = 0;
                    resource.Serialize(data, endian);

                    if (data.Position != resource.Size)
                    {
                        throw new InvalidOperationException();
                    }

                    data.SetLength(data.Length.Align(16));

                    var chunk = new DataFormats.Chunk();
                    chunk.Id = this._ChunkId;
                    chunk.ChunkSize = chunk.DataSize = (int)data.Length;
                    chunk.Write(output, endian);

                    data.Position = 0;
                    output.WriteFromStream(data, data.Length);
                }
            }
        }

        public void Deserialize(Stream input, Endian endian)
        {
            this._Items.Clear();

            while (input.Position < input.Length)
            {
                var chunk = DataFormats.Chunk.Read(input, endian);
                if (chunk.Id != this._ChunkId || chunk.ChunkSize != chunk.DataSize)
                {
                    throw new FormatException();
                }

                using (var data = input.ReadToMemoryStream(chunk.DataSize))
                {
                    data.Position = chunk.DataOffset;

                    var resource = new TResource();
                    resource.Deserialize(data, endian);

                    if (resource.TypeId != this._TypeId)
                    {
                        throw new FormatException();
                    }

                    var item = this.Export(resource, data, endian);
                    this._Items.Add(item);
                }
            }
        }

        protected abstract void Import(TItem item, Stream data, Endian endian, TResource resource, long ownerOffset);
        protected abstract TItem Export(TResource resource, Stream data, Endian endian);
    }
}
