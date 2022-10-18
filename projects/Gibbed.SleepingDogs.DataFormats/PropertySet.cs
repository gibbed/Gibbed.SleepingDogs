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

using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.DataFormats
{
    public class PropertySet : PropertyCollection
    {
        #region Fields
        private long _ParentsOffset;
        private long _DataOffset;
        private long _DefaultBitsOffset;
        private long _PropertiesOffset;
        private Symbol _Name;
        private ushort _ReferenceCount;
        private ushort _ParentCount;
        private uint _ParentMask;
        private Symbol _SchemaName;
        private uint _PropertyMask;
        private ushort _DataSize;
        private ushort _PropertyCount;
        #endregion

        #region Properties
        public long ParentsOffset
        {
            get { return this._ParentsOffset; }
            set { this._ParentsOffset = value; }
        }

        public long DataOffset
        {
            get { return this._DataOffset; }
            set { this._DataOffset = value; }
        }

        public long DefaultBitsOffset
        {
            get { return this._DefaultBitsOffset; }
            set { this._DefaultBitsOffset = value; }
        }

        public long PropertiesOffset
        {
            get { return this._PropertiesOffset; }
            set { this._PropertiesOffset = value; }
        }

        public Symbol Name
        {
            get { return this._Name; }
            set { this._Name = value; }
        }

        public ushort ReferenceCount
        {
            get { return this._ReferenceCount; }
            set { this._ReferenceCount = value; }
        }

        public ushort ParentCount
        {
            get { return this._ParentCount; }
            set { this._ParentCount = value; }
        }

        public uint ParentMask
        {
            get { return this._ParentMask; }
            set { this._ParentMask = value; }
        }

        public Symbol SchemaName
        {
            get { return this._SchemaName; }
            set { this._SchemaName = value; }
        }

        public uint PropertyMask
        {
            get { return this._PropertyMask; }
            set { this._PropertyMask = value; }
        }

        public ushort DataSize
        {
            get { return this._DataSize; }
            set { this._DataSize = value; }
        }

        public ushort PropertyCount
        {
            get { return this._PropertyCount; }
            set { this._PropertyCount = value; }
        }

        public override int Size
        {
            get { return base.Size + 16 + 56; }
        }
        #endregion

        public override void Serialize(Stream output, Endian endian)
        {
            base.Serialize(output, endian);

            output.WriteValueU64(0, endian); // previousPointer
            output.WriteValueU64(0, endian); // nextPointer

            output.WriteOffset(this._ParentsOffset, endian);
            output.WriteOffset(this._DataOffset, endian);
            output.WriteOffset(this._DefaultBitsOffset, endian);
            output.WriteOffset(this._PropertiesOffset, endian);
            this._Name.Write(output, endian);
            output.WriteValueU16(this._ReferenceCount, endian);
            output.WriteValueU16(this._ParentCount, endian);
            output.WriteValueU32(this._ParentMask, endian);
            this._SchemaName.Write(output, endian);
            output.WriteValueU32(this._PropertyMask, endian);
            output.WriteValueU16(this._DataSize, endian);
            output.WriteValueU16(this._PropertyCount, endian);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            base.Deserialize(input, endian);

            var previousPointer = input.ReadValueU64(endian);
            var nextPointer = input.ReadValueU64(endian);

            this._ParentsOffset = input.ReadOffset(endian);
            this._DataOffset = input.ReadOffset(endian);
            this._DefaultBitsOffset = input.ReadOffset(endian);
            this._PropertiesOffset = input.ReadOffset(endian);
            this._Name = Symbol.Read(input, endian);
            this._ReferenceCount = input.ReadValueU16(endian);
            this._ParentCount = input.ReadValueU16(endian);
            this._ParentMask = input.ReadValueU32(endian);
            this._SchemaName = Symbol.Read(input, endian);
            this._PropertyMask = input.ReadValueU32(endian);
            this._DataSize = input.ReadValueU16(endian);
            this._PropertyCount = input.ReadValueU16(endian);

            if (previousPointer != 0 || nextPointer != 0)
            {
                throw new FormatException();
            }
        }
    }
}
