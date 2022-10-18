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
    public class XmlFileResource : ResourceData
    {
        #region Fields
        private int _UncompressedSize;
        private int _CompressedSize;
        #endregion

        #region Properties
        public int UncompressedSize
        {
            get { return this._UncompressedSize; }
            set { this._UncompressedSize = value; }
        }

        public int CompressedSize
        {
            get { return this._CompressedSize; }
            set { this._CompressedSize = value; }
        }

        public override int Size
        {
            get { return base.Size + 16; }
        }
        #endregion

        public override void Serialize(Stream output, Endian endian)
        {
            base.Serialize(output, endian);
            output.WriteValueS32(this._UncompressedSize, endian);
            output.WriteValueS32(this._CompressedSize, endian);
            output.Seek(8, SeekOrigin.Current); // 4, 4
        }

        public override void Deserialize(Stream input, Endian endian)
        {
            base.Deserialize(input, endian);
            this._UncompressedSize = input.ReadValueS32(endian);
            this._CompressedSize = input.ReadValueS32(endian);
            input.Seek(8, SeekOrigin.Current); // 4, 4
        }
    }
}
