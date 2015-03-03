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
    public class PropertySetResource : ResourceData
    {
        #region Fields
        private PropertySetResourceFlags _Flags;
        private uint _SourceTextHash;
        private long _NameOffset;
        private readonly PropertySet _Root;
        #endregion

        public PropertySetResource()
        {
            this._Root = new PropertySet();
        }

        #region Properties
        public PropertySetResourceFlags Flags
        {
            get { return this._Flags; }
            set { this._Flags = value; }
        }

        public uint SourceTextHash
        {
            get { return this._SourceTextHash; }
            set { this._SourceTextHash = value; }
        }

        public long NameOffset
        {
            get { return this._NameOffset; }
            set { this._NameOffset = value; }
        }

        public PropertySet Root
        {
            get { return this._Root; }
        }

        public override int Size
        {
            get { return base.Size + 16 + this.Root.Size; }
        }
        #endregion

        public override void Serialize(Stream output, Endian endian)
        {
            base.Serialize(output, endian);
            output.WriteValueU32((uint)this._Flags, endian);
            output.WriteValueU32(this._SourceTextHash, endian);
            output.WriteOffset(this._NameOffset, endian);
            this._Root.Serialize(output, endian);
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            base.Deserialize(input, endian);
            this._Flags = (PropertySetResourceFlags)input.ReadValueU32(endian);
            this._SourceTextHash = input.ReadValueU32(endian);
            this._NameOffset = input.ReadOffset(endian);
            this._Root.Deserialize(input, endian);
        }
    }
}
