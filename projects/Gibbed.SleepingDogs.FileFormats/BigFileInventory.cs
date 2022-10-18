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
    public class BigFileInventory
    {
        public const uint TypeId = 0x2AE784F9;
        public const uint ChunkId = 0x2C5C40A8;

        #region Fields
        private uint _Id;
        private string _DebugName;
        private ulong _SortKey;
        private readonly List<DataFormats.BigFileIndex.Entry> _Entries; 
        private string _BigFileName;
        #endregion

        public BigFileInventory()
        {
            this._Entries = new();
        }

        #region Properties
        public uint Id
        {
            get { return this._Id; }
            set { this._Id = value; }
        }

        public string DebugName
        {
            get { return this._DebugName; }
            set { this._DebugName = value; }
        }

        public ulong SortKey
        {
            get { return this._SortKey; }
            set { this._SortKey = value; }
        }

        public List<DataFormats.BigFileIndex.Entry> Entries => this._Entries;

        public string BigFileName
        {
            get { return this._BigFileName; }
            set { this._BigFileName = value; }
        }
        #endregion

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            var chunk = DataFormats.Chunk.Read(input, endian);
            if (chunk.Id != ChunkId || chunk.ChunkSize != chunk.DataSize)
            {
                throw new FormatException();
            }

            using (var data = input.ReadToMemoryStream(chunk.DataSize))
            {
                var basePosition = chunk.DataOffset;
                data.Position = basePosition;

                DataFormats.BigFileIndex index = new();
                index.Deserialize(data, endian);

                if (index.TypeId != TypeId)
                {
                    throw new FormatException();
                }

                data.Position = index.EntriesOffset;
                var entries = DataFormats.BigFileIndex.ReadEntries(data, index.EntryCount, endian);

                this._Id = index.TypeId;
                this._DebugName = index.DebugName;
                this._SortKey = index.SortKey;
                this._Entries.Clear();
                this._Entries.AddRange(entries);
                this._BigFileName = index.BigFileName;
            }
        }
    }
}
