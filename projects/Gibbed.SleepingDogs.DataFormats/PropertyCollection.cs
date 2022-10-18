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
    public class PropertyCollection : ISerializable
    {
        #region Fields
        private PropertyCollectionFlags _Flags;
        private long _OwnerOffset;
        #endregion

        #region Properties
        public PropertyCollectionFlags Flags
        {
            get { return this._Flags; }
            set { this._Flags = value; }
        }

        public long OwnerOffset
        {
            get { return this._OwnerOffset; }
            set { this._OwnerOffset = value; }
        }

        public virtual int Size
        {
            get { return 16; }
        }
        #endregion

        public virtual void Serialize(Stream output, Endian endian)
        {
            output.WriteValueU32((uint)this._Flags, endian);
            output.Seek(4, SeekOrigin.Current);
            output.WriteOffset(this._OwnerOffset, endian);
        }

        public virtual void Deserialize(Stream input, Endian endian)
        {
            this._Flags = (PropertyCollectionFlags)input.ReadValueU32(endian);
            input.Seek(4, SeekOrigin.Current);
            this._OwnerOffset = input.ReadOffset(endian);
        }
    }
}
