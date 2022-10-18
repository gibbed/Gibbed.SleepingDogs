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
using Gibbed.IO;

namespace Gibbed.SleepingDogs.DataFormats
{
    public class PropertyList : PropertyCollection
    {
        #region Fields
        private long _DataOffset;
        private uint _TypeId;
        private int _ItemSize;
        private long _WeightsOffset;
        private int _ItemCount;
        private uint _TotalWeight;
        #endregion

        #region Properties
        public long DataOffset
        {
            get { return this._DataOffset; }
            set { this._DataOffset = value; }
        }

        public uint TypeId
        {
            get { return this._TypeId; }
            set { this._TypeId = value; }
        }

        public int ItemSize
        {
            get { return this._ItemSize; }
            set { this._ItemSize = value; }
        }

        public long WeightsOffset
        {
            get { return this._WeightsOffset; }
            set { this._WeightsOffset = value; }
        }

        public int ItemCount
        {
            get { return this._ItemCount; }
            set { this._ItemCount = value; }
        }

        public uint TotalWeight
        {
            get { return this._TotalWeight; }
            set { this._TotalWeight = value; }
        }

        public override int Size
        {
            get { return base.Size + 32; }
        }
        #endregion

        public override void Serialize(Stream output, Endian endian)
        {
            base.Serialize(output, endian);
            output.WriteOffset(this._DataOffset, endian);
            output.WriteValueU32(this._TypeId, endian);
            output.WriteValueS32(this._ItemSize, endian);
            output.WriteOffset(this._WeightsOffset, endian);
            output.WriteValueS32(this._ItemCount, endian);
            output.WriteValueU32(this._TotalWeight, endian);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            base.Deserialize(input, endian);
            this._DataOffset = input.ReadOffset(endian);
            this._TypeId = input.ReadValueU32(endian);
            this._ItemSize = input.ReadValueS32(endian);
            this._WeightsOffset = input.ReadOffset(endian);
            this._ItemCount = input.ReadValueS32(endian);
            this._TotalWeight = input.ReadValueU32(endian);
        }
    }
}
