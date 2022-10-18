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
    public struct ResourceHandle : IEquatable<ResourceHandle>
    {
        public readonly uint NameId;

        public ResourceHandle(uint nameId)
        {
            this.NameId = nameId;
        }

        public static ResourceHandle Read(Stream input, Endian endian)
        {
            var previousPointer = input.ReadValueU64(endian);
            var nextPointer = input.ReadValueU64(endian);
            var dataPointer = input.ReadValueU64(endian);
            var nameId = input.ReadValueU32(endian);
            input.Seek(4, SeekOrigin.Current);

            if (previousPointer != 0 || nextPointer != 0 || dataPointer != 0)
            {
                throw new FormatException();
            }

            return new ResourceHandle(nameId);
        }

        public static void Write(ResourceHandle instance, Stream output, Endian endian)
        {
            output.WriteValueU64(0, endian); // previous pointer
            output.WriteValueU64(0, endian); // next pointer
            output.WriteValueU64(0, endian); // data pointer
            output.WriteValueU32(instance.NameId, endian);
            output.Seek(4, SeekOrigin.Current); // tail padding
        }

        public void Write(Stream output, Endian endian)
        {
            Write(this, output, endian);
        }

        public bool Equals(ResourceHandle other)
        {
            return this.NameId == other.NameId;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is ResourceHandle handle && Equals(handle) == true;
        }

        public override int GetHashCode()
        {
            return (int)this.NameId;
        }

        public static bool operator ==(ResourceHandle a, ResourceHandle b)
        {
            return a.Equals(b) == true;
        }

        public static bool operator !=(ResourceHandle a, ResourceHandle b)
        {
            return a.Equals(b) == false;
        }
    }
}
