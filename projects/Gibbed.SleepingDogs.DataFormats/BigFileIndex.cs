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
using System.Text;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.DataFormats
{
    public class BigFileIndex : ResourceData
    {
        #region Fields
        private ulong _SortKey;
        private uint _EntryCount;
        private long _EntriesOffset;
        private string _BigFileName;
        #endregion

        #region Properties
        public ulong SortKey
        {
            get { return this._SortKey; }
            set { this._SortKey = value; }
        }

        public uint EntryCount
        {
            get { return this._EntryCount; }
            set { this._EntryCount = value; }
        }

        public long EntriesOffset
        {
            get { return this._EntriesOffset; }
            set { this._EntriesOffset = value; }
        }

        public string BigFileName
        {
            get { return this._BigFileName; }
            set { this._BigFileName = value; }
        }

        public override int Size
        {
            get { return base.Size + 80; }
        }
        #endregion

        public override void Serialize(Stream output, Endian endian)
        {
            base.Serialize(output, endian);

            output.WriteValueU64(this._SortKey, endian);
            output.WriteValueU32(this._EntryCount, endian);
            output.Seek(4, SeekOrigin.Current);
            output.WriteOffset(this._EntriesOffset, endian);
            output.WriteValueU64(0, endian);
            output.WriteValueU16(0xFFFF, endian);
            output.WriteValueU16(0, endian);
            output.Seek(8, SeekOrigin.Current);
            output.WriteString(this._BigFileName, 32, Encoding.ASCII);
            output.Seek(4, SeekOrigin.Current);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            base.Deserialize(input, endian);

            this._SortKey = input.ReadValueU64(endian);
            this._EntryCount = input.ReadValueU32(endian);
            input.Seek(4, SeekOrigin.Current);
            this._EntriesOffset = input.ReadOffset(endian);
            var filePointer = input.ReadValueU64(endian);
            var state = input.ReadValueU16(endian);
            var mountIndex = input.ReadValueU16(endian);
            input.Seek(8, SeekOrigin.Current);
            this._BigFileName = input.ReadString(32, true, Encoding.ASCII);
            input.Seek(4, SeekOrigin.Current);

            if (filePointer != 0 || state != 0 || mountIndex != 0xFFFF)
            {
                throw new FormatException();
            }
        }

        public static IEnumerable<Entry> ReadEntries(Stream input, uint count, Endian endian)
        {
            var items = new Entry[count];
            for (uint i = 0; i < count; i++)
            {
                items[i] = Entry.Read(input, endian);
            }

            return items;
        }

        public static void WriteEntries(Stream output, IEnumerable<Entry> items, Endian endian)
        {
            foreach (var item in items)
            {
                item.Write(output, endian);
            }
        }

        public struct Entry
        {
            public uint Id;
            public uint Offset;
            public BigFileSize Size;

            internal static Entry Read(Stream input, Endian endian)
            {
                Entry instance;
                instance.Id = input.ReadValueU32(endian);
                instance.Offset = input.ReadValueU32(endian);
                instance.Size = BigFileSize.Read(input, endian);
                return instance;
            }

            public static void Write(Entry instance, Stream output, Endian endian)
            {
                output.WriteValueU32(instance.Id, endian);
                output.WriteValueU32(instance.Offset, endian);
                instance.Size.Write(output, endian);
            }

            public void Write(Stream output, Endian endian)
            {
                Write(this, output, endian);
            }
        }
    }
}
