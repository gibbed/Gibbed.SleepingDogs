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

namespace Gibbed.SleepingDogs.PropertySetFormats
{
    public struct PropertySchema
    {
        public readonly uint Id;
        public readonly byte Type;
        public readonly uint Offset;

        public PropertySchema(uint id, byte type, uint offset)
        {
            this.Id = id;
            this.Type = type;
            this.Offset = offset;
        }

        public static PropertySchema Read(Stream input, Endian endian)
        {
            var typeAndOffset = input.ReadValueU32(endian);
            var id = input.ReadValueU32(endian);
            return new(id, (byte)(typeAndOffset >> 24), typeAndOffset & 0x00FFFFFFu);
        }

        public static void Write(PropertySchema instance, Stream output, Endian endian)
        {
            var typeAndOffset = instance.Offset & 0x00FFFFFFu | (uint)instance.Type << 24;
            output.WriteValueU32(typeAndOffset, endian);
            output.WriteValueU32(instance.Id, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(this, output, endian);
        }
    }
}
