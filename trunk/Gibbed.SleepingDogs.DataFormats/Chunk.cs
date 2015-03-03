﻿/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
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
    public struct Chunk
    {
        public uint Id;
        public uint ChunkSize;
        public uint DataSize;
        public uint DataOffset;

        public static Chunk Read(Stream input, Endian endian)
        {
            var instance = new Chunk();
            instance.Id = input.ReadValueU32(endian);
            instance.ChunkSize = input.ReadValueU32(endian);
            instance.DataSize = input.ReadValueU32(endian);
            instance.DataOffset = input.ReadValueU32(endian);
            return instance;
        }

        public static void Write(Stream output, Chunk instance, Endian endian)
        {
            output.WriteValueU32(instance.Id, endian);
            output.WriteValueU32(instance.ChunkSize, endian);
            output.WriteValueU32(instance.DataSize, endian);
            output.WriteValueU32(instance.DataOffset, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
