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
    public struct BigFileSize
    {
        public uint LoadOffset;
        public uint CompressedSize;
        public uint CompressedExtra;
        public uint UncompressedSize;

        internal static BigFileSize Read(Stream input, Endian endian)
        {
            BigFileSize instance;
            instance.LoadOffset = input.ReadValueU32(endian);
            instance.CompressedSize = input.ReadValueU32(endian);
            instance.CompressedExtra = input.ReadValueU32(endian);
            instance.UncompressedSize = input.ReadValueU32(endian);
            return instance;
        }

        internal static void Write(BigFileSize instance, Stream output, Endian endian)
        {
            output.WriteValueU32(instance.LoadOffset, endian);
            output.WriteValueU32(instance.CompressedSize, endian);
            output.WriteValueU32(instance.CompressedExtra, endian);
            output.WriteValueU32(instance.UncompressedSize, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(this, output, endian);
        }
    }
}
