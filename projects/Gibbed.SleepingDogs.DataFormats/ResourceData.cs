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
using System.Text;
using Gibbed.IO;

namespace Gibbed.SleepingDogs.DataFormats
{
    public class ResourceData : ISerializable
    {
        #region Fields
        private uint _Id;
        private uint _TypeId;
        private string _DebugName;
        #endregion

        #region Properties
        public uint Id
        {
            get { return this._Id; }
            set { this._Id = value; }
        }

        public uint TypeId
        {
            get { return _TypeId; }
            set { _TypeId = value; }
        }

        public string DebugName
        {
            get { return _DebugName; }
            set { _DebugName = value; }
        }

        public virtual int Size
        {
            get { return 88; }
        }
        #endregion

        public virtual void Serialize(Stream output, Endian endian)
        {
            output.WriteValueU64(0, endian); // parent pointer
            output.WriteValueU64(0, endian); // child[0] pointer
            output.WriteValueU64(0, endian); // child[1] pointer
            output.WriteValueU32(this._Id, endian);
            output.Seek(4, SeekOrigin.Current); // padding
            output.WriteValueU64(0, endian); // resource handles previous pointer
            output.WriteValueU64(0, endian); // resource handles next pointer
            output.WriteValueU32(this._TypeId, endian);
            output.WriteString(this._DebugName, 36, Encoding.ASCII);
        }

        public virtual void Deserialize(Stream input, Endian endian)
        {
            var parentPointer = input.ReadValueU64(endian);
            var child0Pointer = input.ReadValueU64(endian);
            var child1Pointer = input.ReadValueU64(endian);
            this._Id = input.ReadValueU32(endian);
            input.Seek(4, SeekOrigin.Current); // padding
            var resourceHandlesPreviousPointer = input.ReadValueU64(endian);
            var resourceHandlesNextPointer = input.ReadValueU64(endian);
            this._TypeId = input.ReadValueU32(endian);
            this._DebugName = input.ReadString(36, true, Encoding.ASCII);

            if (parentPointer != 0 ||
                child0Pointer != 0 || child1Pointer != 0 ||
                resourceHandlesPreviousPointer != 0 || resourceHandlesNextPointer != 0)
            {
                throw new FormatException();
            }
        }
    }
}
